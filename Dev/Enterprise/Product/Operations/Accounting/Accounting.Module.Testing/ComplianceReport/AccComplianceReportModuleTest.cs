using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Module
{
	using System;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Modules.Testing;
	using NUnit.Framework;
	using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

	[TestedType(typeof(AccComplianceReportModule))]
	public class AccComplianceReportModuleTest : ZModuleBasherTest
	{
		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Accountant, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ComplianceReports, Module.SecurityCheckpoint);
		}

		public void TestActionMenu()
		{
			AssertNotNull(Module.FormActionMenu);
			AssertNotNull(Module.FormActionMenu.FindByText("&Actions"));
			AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Re-Queue"));
			AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Generate"));
			AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Finalize"));
			AssertNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Generate &Monthly Transaction XML"));
			AssertNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Generate &Annual SAFT XML"));
		}

		public void TestActionMenus_SupportGenerateSAFT()
		{
			var supportGenerateSAFTContries = new[] { Constants.CountryCodes.Portugal, Constants.CountryCodes.Norway };

			foreach (var country in supportGenerateSAFTContries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					AssertCommonActionMenu();
					AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Generate &Monthly Transaction XML"));
					AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Generate &Annual SAFT XML"));
				}
			}
		}

		public void TestPopupPerformAggregationFormBeforeGenerateSAFT()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			Factory.Save();

			AssertPopupPerformAggregationForm(Constants.CountryCodes.Portugal, false);
			AssertPopupPerformAggregationForm(Constants.CountryCodes.Norway, true);
		}

		void AssertPopupPerformAggregationForm(string countryCode, bool hasPopup)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				using (var module = (AccComplianceReportModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					using (var form = new ZForm())
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						form.Controls.Add(module.EmbeddedControl);
						form.Show();
						var report = Factory.NewWithValidTestData<AccComplianceReport>();
						Factory.Save();

						module.FilterBusinessObject.ResetToDefaultValues();
						((IFilterModuleInternalsForTesting)module).PerformSearch();
						module.DisplayGrid.SelectAllElements();

						var actions = module.FormActionMenu.FindByText("&Actions");
						var monthlyMenuItem = actions.MenuItems.FindByText("Generate &Monthly Transaction XML");
						monthlyMenuItem.PerformClick();

						var expectedMesg = "If you wish to report on current trading / sub-ledger movements then answer 'Yes'.";
						AssertEquals("Popup the Perform Aggregation form", hasPopup, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMesg));

						UnitTestUserNotification.Instance.ClearMessages();
						Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMesg));

						var yearlMenuItem = actions.MenuItems.FindByText("Generate &Annual SAFT XML");
						yearlMenuItem.PerformClick();
						AssertEquals("Popup the Perform Aggregation form", hasPopup, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMesg));
					}
				}
			}
		}

		public void TestActionMenuIT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertCommonActionMenu();
				AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Generate &Esterometro XML"));
			}
		}

		void AssertCommonActionMenu()
		{
			AssertNotNull(Module.FormActionMenu);
			AssertNotNull(Module.FormActionMenu.FindByText("&Actions"));
			AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Re-Queue"));
			AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Generate"));
			AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Finalize"));
		}

		public void TestExportVATFileActionMenu()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AssertNotNull(Module.FormActionMenu);
				AssertNotNull(Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Export Compliance Report"));
			}
		}

		public void TestSupportsWorkflow()
		{
			Assert(Module.SupportsWorkflow);
		}

		public void TestReQueueWithoutSelection()
		{
			AssertMessagePopupWithoutSelection("&Re-Queue", "Please select a Report to re-queue.");
		}

		public void TestGenerateWithoutSelection()
		{
			AssertMessagePopupWithoutSelection("&Generate", "Please select a Report to generate.");
		}

		public void TestFinalizeWithoutSelection()
		{
			AssertMessagePopupWithoutSelection("&Finalize", "Please select a Report to finalize.");
		}

		public void TestGenerateSAFTXMLWithoutSelection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				AssertMessagePopupWithoutSelection("Generate &Monthly Transaction XML", "Please select a Report to generate Monthly Transaction XML.");
				AssertMessagePopupWithoutSelection("Generate &Annual SAFT XML", "Please select a Report to generate Annual SAFT XML.");
			}
		}

		public void TestGenerateEsterometroXMLWithoutSelection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertMessagePopupWithoutSelection("Generate &Esterometro XML", "Please select a Report to generate Esterometro XML.");
			}
		}

		public void TestExportOpenFormatFilesWithoutSelection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Israel))
			{
				AssertMessagePopupWithoutSelection("Export Open Format Files", "Please select a report to export open format files.");
			}
		}

		public void TestExportVATFileWithoutSelection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AssertMessagePopupWithoutSelection("&Export Compliance Report", "Please select a Report to export.");
			}
		}

		public void TestNewStandardMenu()
		{
			using (AddConfiguration("LIB"))
			{
				AssertNotNull(Module.FormActionMenu);
				AssertNotNull(Module.NewMenuItem);
				AssertEquals("Incorrect number of submenu", 1, Module.NewMenuItem.MenuItems.Count);
				AssertNotNull(Module.NewMenuItem.MenuItems.FindByText("&New LIB Report"));
			}
		}

		public void TestNewStandardMenu_MultipleConfig()
		{
			using (AddConfiguration("LIB", "PTR", "PTA"))
			{
				AssertNotNull(Module.FormActionMenu);
				AssertNotNull(Module.NewMenuItem);
				AssertEquals("Incorrect number of submenu", 3, Module.NewMenuItem.MenuItems.Count);
				AssertNotNull(Module.NewMenuItem.MenuItems.FindByText("&New LIB Report"));
				AssertNotNull(Module.NewMenuItem.MenuItems.FindByText("&New PTR Report"));
				AssertNotNull(Module.NewMenuItem.MenuItems.FindByText("&New PTA Report"));
			}
		}

		public void TestNewStandardMenu_NoConfiguration()
		{
			using (AddConfiguration())
			{
				AssertNotNull(Module.FormActionMenu);
				AssertNotNull(Module.NewMenuItem);
				AssertEquals("No submenus within New Action", 0, Module.NewMenuItem.MenuItems.Count);
			}
		}

		public void TestHasError_WhenComplianceReportFromGLDConfiguration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var allTransactionsReportConfig = complianceConfig.AddNew();
			allTransactionsReportConfig.ReportCode = "TST";
			allTransactionsReportConfig.ReportTitle = "Test Tax Report";
			allTransactionsReportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			allTransactionsReportConfig.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
			allTransactionsReportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			allTransactionsReportConfig.TaxRegistrationType = "ABN";
			allTransactionsReportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;

			var gldReportConfig = complianceConfig.AddNew();
			gldReportConfig.ReportCode = "TSG";
			gldReportConfig.ReportTitle = "Test Tax Report";
			gldReportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.GeneralLedgerData;
			gldReportConfig.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
			gldReportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gldReportConfig.TaxRegistrationType = "ABN";
			gldReportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig);

			var gldComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			gldComplianceReport.ACR_ReportType = gldReportConfig.ReportCode;
			gldComplianceReport.ACR_DateFrom = ZDate.Today.AddDays(-2);
			gldComplianceReport.ACR_DateTo = ZDate.Today.AddDays(2);
			gldComplianceReport.ACR_Status = AccComplianceReport.Status.ReportCreated;

			var allTransactionsComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			allTransactionsComplianceReport.ACR_ReportType = allTransactionsReportConfig.ReportCode;
			allTransactionsComplianceReport.ACR_DateFrom = ZDate.Today.AddDays(-2);
			allTransactionsComplianceReport.ACR_DateTo = ZDate.Today.AddDays(2);
			allTransactionsComplianceReport.ACR_Status = AccComplianceReport.Status.ReportCreated;

			Factory.Save();

			using (var module = (AccComplianceReportModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.FilterBusinessObject.ResetToDefaultValues();
				((IFilterModuleInternalsForTesting)module).PerformSearch();
				module.DisplayGrid.SelectSingleElementByPK(gldComplianceReport.PK);

				var actions = module.FormActionMenu.FindByText("&Actions");
				var monthlyMenuItem = actions.MenuItems.FindByText("&Re-Queue");
				monthlyMenuItem.PerformClick();

				var expectedMesg = "The compliance report is generated using accounting journals data source. The queue and re-queue actions are not applicable.";
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMesg));

				UnitTestUserNotification.Instance.ClearMessages();
				module.DisplayGrid.SelectSingleElementByPK(allTransactionsComplianceReport.PK);

				actions = module.FormActionMenu.FindByText("&Actions");
				monthlyMenuItem = actions.MenuItems.FindByText("&Re-Queue");
				monthlyMenuItem.PerformClick();

				AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMesg));
			}
		}

		IDisposable AddConfiguration(params ZString[] reportCodes)
		{
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			foreach (ZString reportCode in reportCodes)
			{
				var reportConfig = reportConfigurations.AddNew();
				reportConfig.Country = Env.CurrentCompany.Country.Code;
				reportConfig.ReportCode = reportCode;
				reportConfig.ReportTitle = ZString.Format("{0} Report", reportCode);
				reportConfig.ReportBaseTablePrefix = Lookups.ReportBaseTablePrefixListCodes.AllTransactions;
				reportConfig.ReportLineGrouping = Lookups.ReportLineGroupingListCodes.DayBook;
				reportConfig.ReportPeriodicity = Lookups.ReportPeriodicityCodes.DateRange;
				reportConfig.TaxRegistrationType = reportConfig.Lookups.TaxRegistrationTypeList[0].Code;
			}

			return AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
		}

		void AssertMessagePopupWithoutSelection(string action, string expectedMessage)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText(action).PerformClick();
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccComplianceReport;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			GetBusinessObjectsToGetControllersFor();
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			return new BusinessObject[] { Factory.NewWithValidTestData<AccComplianceReport>(), Factory.NewWithValidTestData<AccComplianceReport>(), Factory.NewWithValidTestData<AccComplianceReport>() };
		}

		AccComplianceReportModule Module;

		protected override void SetUp()
		{
			base.SetUp();
			Module = (AccComplianceReportModule)ZModuleFactory.Instance.Create(GetModuleID());
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		#endregion
	}
}

