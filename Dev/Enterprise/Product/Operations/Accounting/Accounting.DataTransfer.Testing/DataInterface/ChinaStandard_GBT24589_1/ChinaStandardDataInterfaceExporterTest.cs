using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.Accounting.DataTransfer.DataInterface.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	sealed class ChinaStandardDataInterfaceExporterTest : ChinaStandardExporterTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportData()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				AddTranslations();
				PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));

				string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GBT245891ChinaStandard公共档案20060329153942.xml");

				try
				{
					AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
					ChinaStandard2010DataInterfaceWrapper bizObj = new ChinaStandard2010DataInterfaceWrapper(Factory) { Period = 200603, ExportDirectory = EnvProxy.Instance.TempPath };
					ChinaStandard2010DataInterfaceExporter exporter = new ChinaStandard2010DataInterfaceExporter(bizObj, new NotificationBuffer());
					Assert(exporter.ExportData(new ReferenceFilesDataAdapter()));
					AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));

					string expectedExportData = File.ReadAllText(BaseTestFilePath + @"DataInterface\ChinaStandard_GBT24589_1\TestXMLReferenceFiles.xml", Encoding.GetEncoding("GB18030"));

					string actualExportData = File.ReadAllText(exportFileName);

					this.AssertXMLEqualsByDiff("Exporter is not exporting what is expected", expectedExportData, actualExportData);
				}
				finally
				{
					DeleteIfExists(exportFileName);
				}
			}
		}

		void AddTranslations()
		{
			var currency = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_IsActive, true));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "COP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "哥伦比亚比索");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KMF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "科摩罗法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CDF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "刚果民主共和国刚果法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CRC").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "哥斯达黎加科朗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KYD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "开曼元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KHR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "柬埔寨瑞尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CAD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "加元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CVE").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "佛得角埃斯库多");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CLP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "智利比索");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CNY").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "中国人民币元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "HRK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "克罗地亚库纳");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CZK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "捷克克朗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CUP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "古巴比索");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BWP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "博茨瓦纳普拉");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BOB").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "玻利维亚玻利维亚诺");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BAM").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "波斯尼亚和黑塞哥维那可兑换马克");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BHD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "巴林第纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BSD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "巴哈马元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BDT").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "孟加拉塔卡");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BBD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "巴巴多斯元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BTN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "不丹努扎姆");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BZD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "伯利兹元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BYN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "白俄罗斯卢布");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BMD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "百慕大元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BND").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "文莱元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BRL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "巴西雷亚尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BGN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "保加利亚列弗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BIF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "布隆迪法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "AOA").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "安哥拉宽扎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ALL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "阿尔巴尼亚列克");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "DZD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "阿尔及利亚第纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "AFN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "阿富汗阿富汗尼");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "AWG").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "阿鲁巴弗罗林");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ARS").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "阿根廷比索");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "AMD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "亚美尼亚德拉姆");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "AZN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "阿塞拜疆马纳特");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "AUD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "澳元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "GMD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "冈比亚达拉西");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "GIP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "直布罗陀镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "GEL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "格鲁吉亚拉里");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "GYD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "圭亚那元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "GTQ").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "危地马拉格查尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "GNF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "几内亚法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "DOP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "多米尼加比索");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "DKK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "丹麦克朗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "DJF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "吉布提法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "FKP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "福克兰镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "FJD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "斐济元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "EGP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "埃及镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "XCD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "东加勒比元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ERN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "厄立特里亚纳克法");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ETB").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "埃塞俄比亚比尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "EUR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "欧元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KZT").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "哈萨克坦吉");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KES").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "肯尼亚先令");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KGS").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "吉尔吉斯斯坦索姆");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KWD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "科威特第纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "HNL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "洪都拉斯伦皮拉");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "HKD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "港币");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "HTG").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "海地古德");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "HUF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "匈牙利 福林");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "JOD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "约旦第纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "JMD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "牙买加元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "JPY").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "日元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "INR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "印度卢比");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "IDR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "印度尼西亚盾");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ISK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "冰岛克朗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "IRR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "伊朗里亚尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "IQD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "伊拉克第纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ILS").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "以色列新谢克尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "OMR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "阿曼里亚尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "LAK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "老挝基普");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "LYD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "利比亚第纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "LRD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "利比里亚元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "LBP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "黎巴嫩镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "LSL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "莱索托洛蒂");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "NOK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "挪威克朗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KPW").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "朝鲜朝鲜圆");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "NAD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "纳米比亚元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "NIO").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "尼加拉瓜科多巴");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "NGN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "尼日利亚奈拉");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ANG").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "荷属安的列斯盾");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "NZD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "新西兰元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "TWD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "新台币");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "NPR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "尼泊尔卢比");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MZN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "莫桑比克梅蒂卡尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MDL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "摩尔多瓦列伊");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MNT").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "蒙古图格里克");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MAD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "摩洛哥迪拉姆");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MUR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "毛里求斯卢比");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MRO").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "毛里塔尼亚乌吉亚");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MOP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "澳门币");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MKD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "前南马其顿代纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MGA").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "马达加斯加阿里亚里");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MWK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "马拉维克瓦查");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MYR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "马来西亚令吉");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MVR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "马尔代夫拉菲亚");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MXN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "墨西哥比索");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MMK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "缅元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ZAR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "南非兰特");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "KRW").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "韩国韩圆");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SSP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "南苏丹南苏丹镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SBD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "所罗门群岛元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SOS").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "索马里先令");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SAR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "沙特里亚尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SHP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "圣赫勒拿镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SVC").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "萨尔瓦多科朗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "WST").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "萨摩亚塔拉");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SLL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "塞拉利昂利昂");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SGD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "新加坡元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SCR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "塞舌尔卢比");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SZL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "斯威士兰里兰吉尼");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SEK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "瑞典克朗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "CHF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "瑞士法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "LKR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "斯里兰卡卢比");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SYP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "叙利亚镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "STD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "圣多美和普林西比多布拉");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "GBP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "英镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "PLN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "波兰兹罗提");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "PKR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "巴基斯坦卢比");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "PAB").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "巴拿马巴波亚");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "PGK").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "巴布亚新几内亚基那");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "PYG").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "巴拉圭瓜拉尼");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "RWF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "卢旺达法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "RUB").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "俄罗斯卢布");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "QAR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "卡塔尔里亚尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "TOP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "汤加潘加");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "TZS").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "坦桑尼亚先令");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "THB").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "泰铢");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "TTD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "特立尼达和多巴哥元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "TND").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "突尼斯第纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "TRY").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "新土耳其里拉");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "VUV").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "瓦努阿图瓦图");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "VND").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "越南盾");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "VEF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "委内瑞拉大玻利瓦尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "AED").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "阿拉伯联合酋长国迪尔汗");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "USD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "美元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "UGX").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "乌干达先令");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "UAH").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "乌克兰赫里夫尼亚");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "UYU").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "乌拉圭比索");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "UZS").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "乌兹别克斯坦苏姆");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ZMW").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "赞比亚克瓦查");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "ZWL").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "津巴布韦元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "YER").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "也门里亚尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "GHS").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "加纳塞地");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "XDR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "IMF 特别提款权");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "TJS").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "索莫尼");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "RSD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "塞尔维亚第纳尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SDG").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "苏丹镑");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "SRD").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "苏利南元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "RON").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "罗马尼亚列伊");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "TMT").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "土库曼新马纳特");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "PEN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "秘鲁索尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "XOF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "非洲金融共同体法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "XAF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "中非金融合作法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "XPF").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "太平洋法郎");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "PHP").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "菲律宾比索");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "VES").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "委内瑞拉新货币主权玻利瓦尔");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "MRU").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "毛里塔尼亚乌吉亚");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "BYR").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "白俄罗斯卢布");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.FirstOrDefault(x => x.RX_Code == "STN").PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "圣多美和普林西比多布拉");

			Factory.Save();
		}
	}
}
