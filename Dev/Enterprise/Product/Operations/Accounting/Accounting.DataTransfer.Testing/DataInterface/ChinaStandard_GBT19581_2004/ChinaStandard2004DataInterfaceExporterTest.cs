using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.DataInterface.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004.Testing
{
	sealed class ChinaStandard2004DataInterfaceExporterTest : ChinaStandardExporterTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportData()
		{
			using (Factory.AddDisposableService())
			{
				var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Australia));
				var helper = new RefCurrencyTestHelper(Factory);
				helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "澳元");
				Factory.Save();
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
				GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
				GLLocalNumberFormat gNF = list.AddNew();
				gNF.NumberFormat = "4-2-2";
				gNF.CountryCode = Constants.CountryCodes.China;
				gNF.Language = Constants.Languages.ChineseSimplified;
				gNF.IsFixedLength = true;
				AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
				Factory.Save();
				string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GBT19581Standard会计核算软件数据20060329153942.xml");

				try
				{
					AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
					var bizObj = new ChinaStandard2004DataInterfaceWrapper(Factory)
					{
						Branch = GlbBranch.CurrentBranch.PK,
						Period = 200603,
						ExportXML = true,
						AccountBook = true,
						SupplementaryAccounts = false,
						AccountingVouchers = false,
						TrialBalance = false,
						BalanceSheet = false,
						ProfitAndLoss = false,
						ChartOfAccounts = false,
						VATDetailed = false,
						AssetProvision = false,
						PNLAppropriation = false,
						EquityMovement = false,
						ExportDirectory = EnvProxy.Instance.TempPath,
						DeliveryTo = "test@test.com"
					};
					bizObj.ProcessFiles = bizObj.ExportFiles;
					ChinaStandard2004DataInterfaceExporter exporter = new ChinaStandard2004DataInterfaceExporter(bizObj, new NotificationBuffer());

					Assert(exporter.ExportData(new ChinaStandard2004DataAdapter()));
					AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));

					string expectedExportData = File.ReadAllText(BaseTestFilePath + @"DataInterface\ChinaStandard_GBT19581_2004\DataAdapter\TestXML\ChinaStandardExporter2004.xml", Encoding.GetEncoding("GB18030"));

					string actualExportData = File.ReadAllText(exportFileName);

					this.AssertXMLEqualsByDiff("Exporter is not exporting what is expected", expectedExportData, actualExportData);
				}
				finally
				{
					DeleteIfExists(exportFileName);
				}

				exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "DZZB.xml");
				try
				{
					AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
					ChinaStandard2004DataInterfaceWrapper bizObj = new ChinaStandard2004DataInterfaceWrapper(Factory)
					{
						ExportTXT = true,
						AccountBook = true,
						ExportDirectory = EnvProxy.Instance.TempPath,
						DeliveryTo = "test@test.com",
					};
					bizObj.ProcessFiles = bizObj.ExportFiles;
					ChinaStandard2004DataInterfaceExporter exporter = new ChinaStandard2004DataInterfaceExporter(bizObj, new NotificationBuffer());

					Assert(exporter.ExportData(new ChinaStandard2004DataAdapter()));
				}
				finally
				{
					DeleteIfExists(exportFileName);
				}

				exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "KJKM.xml");
				try
				{
					AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
					ChinaStandard2004DataInterfaceWrapper bizObj = new ChinaStandard2004DataInterfaceWrapper(Factory)
					{
						DeliveryTo = "test@test.com",
						ExportTXT = true,
						ChartOfAccounts = true,
						ExportDirectory = EnvProxy.Instance.TempPath
					};
					bizObj.ProcessFiles = bizObj.ExportFiles;
					ChinaStandard2004DataInterfaceExporter exporter = new ChinaStandard2004DataInterfaceExporter(bizObj, new NotificationBuffer());
					Assert(exporter.ExportData(new ChinaStandard2004DataAdapter()));
					Factory.Save();
				}
				finally
				{
					DeleteIfExists(exportFileName);
				}
			}
		}
	}
}
