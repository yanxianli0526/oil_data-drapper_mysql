using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using Dapper;
using System.ComponentModel;

namespace oil_data
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                List<oil> results = null;

                //一筆約6ms
                using (MySqlConnection conn = new MySqlConnection(db_connectString))
                {
                    conn.Open();
                    string SQL = "SELECT * from `oil_status`;";
                    results = conn.Query<oil>(SQL).ToList();

                    //var qqqqqq = conn.Query<oil>(SQL); // 也可以這樣子取得select的結果

                    //where條件寫法
                    //string sql2 = "select * from `oil_status` where oil_date = @date_str;";
                    //var test_ob = new
                    //{
                    //    date_str = "2020-06-01"
                    //};
                    //var qqqqqq2 = conn.query<oil>(sql2, test_ob); // 也可以這樣子取得select的結果

                }

                foreach (oil a in results)
                {
                    int count = results.IndexOf(a);
                    //第一筆不用新增
                    if (count > 0)
                    {
                        //計算目前這筆跟前一筆差幾天
                        int date_count = Convert.ToInt32(((TimeSpan)Convert.ToDateTime(results[count].oil_date).Subtract(Convert.ToDateTime(results[count - 1].oil_date))).TotalDays);
                        //相差不等於1 代表要補資料
                        if (date_count != 1)
                        {
                            using (MySqlConnection conn = new MySqlConnection(db_connectString))
                            {
                                conn.Open();
                                string SQL = "INSERT INTO `oil_status_date`(oil_date,oil_ninety_two,oil_ninety_two_up_down" +
                            ",oil_ninety_five,oil_ninety_five_up_down,oil_ninety_eight,oil_ninety_eight_up_down,oil_super,oil_super_up_down) " +
                                                              "VALUES (@oil_date,@oil_ninety_two,@oil_ninety_two_up_down," +
                            "@oil_ninety_five,@oil_ninety_five_up_down,@oil_ninety_eight,@oil_ninety_eight_up_down,@oil_super,@oil_super_up_down);";
                                var wea = new List<oil>();
                                //相差幾天就要新增幾筆 (前一筆也要新增)
                                for (int i = 0; i < date_count; i++)
                                {
                                    wea.Add(new oil()
                                    {
                                        //日期加上i補中間的每一天(第一次是前一筆的日期)
                                        oil_date = Convert.ToDateTime(results[count - 1].oil_date).AddDays(i).ToString("yyyy/MM/dd"),
                                        oil_ninety_two = results[count - 1].oil_ninety_two,
                                        oil_ninety_two_up_down = results[count - 1].oil_ninety_two_up_down,
                                        oil_ninety_five = results[count - 1].oil_ninety_five,
                                        oil_ninety_five_up_down = results[count - 1].oil_ninety_five_up_down,
                                        oil_ninety_eight = results[count - 1].oil_ninety_eight,
                                        oil_ninety_eight_up_down = results[count - 1].oil_ninety_eight_up_down,
                                        oil_super = results[count - 1].oil_super,
                                        oil_super_up_down = results[count].oil_super_up_down,
                                    });
                                }
                                conn.Execute(SQL, wea);
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
            }
        }



        #region

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyname)
        {
            if (PropertyChanged != null) //當屬性值變更時發生
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname)); //propertyname為變更的值
        }
        #endregion

        String db_connectString { get { return "server=127.0.0.1;uid=root;pwd=yang108!;database=starbucks;Port=3306;"; } }

        public class oil
        {
            public string oil_date { get; set; }       //日期
            public string oil_ninety_two { get; set; }      //九二汽油價格
            public string oil_ninety_two_up_down { get; set; }      //九二汽油價格
            public string oil_ninety_five { get; set; }     //九五汽油價格
            public string oil_ninety_five_up_down { get; set; }     //九五汽油價格
            public string oil_ninety_eight { get; set; }    //九八汽油價格
            public string oil_ninety_eight_up_down { get; set; }    //九八汽油價格
            public string oil_super { get; set; }           //超級柴油價格
            public string oil_super_up_down { get; set; }           //超級柴油價格
        }

    }
}
