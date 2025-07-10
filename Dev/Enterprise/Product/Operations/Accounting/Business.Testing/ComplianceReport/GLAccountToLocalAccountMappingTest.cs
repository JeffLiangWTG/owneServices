using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport
{
	public class GLAccountToLocalAccountMappingTest : TestCaseWithFactory
	{
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		public void TestGetLocalAccount()
		{
			var previousSuppressReportingOfErrors = ErrorReporter.SuppressReportingOfErrors;
			ErrorReporter.SuppressReportingOfErrors = true;

			var frenchAccount = Creator.CreateGLAccountAndLocalAccountMapping("5436.11.00", Constants.Languages.French, Constants.CountryCodes.France, "BSH");
			var spanishAccount = Creator.CreateGLAccountAndLocalAccountMapping("9988.11.00", Constants.Languages.Spanish, Constants.CountryCodes.Spain, "BSH");
			var accountWithMultipleMappings = Factory.NewWithValidTestData<AccGLHeader>();
			accountWithMultipleMappings.AG_AccountNum = "1234.56.78";
			Factory.Save();
			Creator.CreateLocalAccountMappingForGLHeader(accountWithMultipleMappings, Constants.Languages.French, Constants.CountryCodes.France);
			Creator.CreateLocalAccountMappingForGLHeader(accountWithMultipleMappings, Constants.Languages.German, Constants.CountryCodes.Germany);
			Creator.CreateLocalAccountMappingForGLHeader(accountWithMultipleMappings, Constants.Languages.English, Constants.CountryCodes.France);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = CreateDayBookReportConfigForSpecificCountry(Constants.CountryCodes.France, "TVA");
				var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, Constants.Languages.French);

				AssertEquals("Test for France: 5436.11.00 - French account mapped", ("FR-5436.11.00", "FR-5436.11.00 - Description"), accountMappingHelper.GetLocalAccount(frenchAccount.PK, ""));
				AssertEquals("Test for France: 9988.11.00 - Spanish account not mapped", ("", ""), accountMappingHelper.GetLocalAccount(spanishAccount.PK, ""));
				AssertEquals("Test for France: 1234.56.78 - Multi account mapped", ("FR-1234.56.78", "FR-1234.56.78 - Description"), accountMappingHelper.GetLocalAccount(accountWithMultipleMappings.PK, ""));
				AssertEquals("Test for France: Mapping errors reported via ErrorReporter", 1, accountMappingHelper.UnmappedLocalAccounts.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				ErrorReporter.Clear();
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = CreateDayBookReportConfigForSpecificCountry(Constants.CountryCodes.Spain, "NIF");
				var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, Core.SharedConstants.Languages.Spanish);

				AssertEquals("Test for Spain: 5436.11.00 - French account not mapped", ("", ""), accountMappingHelper.GetLocalAccount(frenchAccount.PK, ""));
				AssertEquals("Test for Spain: 9988.11.00 - Spanish account mapped", ("ES-9988.11.00", "ES-9988.11.00 - Description"), accountMappingHelper.GetLocalAccount(spanishAccount.PK, ""));
				AssertEquals("Test for Spain: 1234.56.78 - Multi account not mapped", ("", ""), accountMappingHelper.GetLocalAccount(accountWithMultipleMappings.PK, ""));
				AssertEquals("Test for Spain: Mapping errors reported via ErrorReporter", 2, accountMappingHelper.UnmappedLocalAccounts.Count);
			}

			ErrorReporter.SuppressReportingOfErrors = previousSuppressReportingOfErrors;
		}

		public void TestCheckMappingIsValidAndHandleErrors()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				Creator.CreateTestPeriods(new ZDateTime(2021, 1, 1));
				Creator.CreateTestPeriods(new ZDateTime(2022, 1, 1));
				Creator.CreateTestPeriods(new ZDateTime(2023, 1, 1));

				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = CreateDayBookReportConfigForSpecificCountry(Constants.CountryCodes.France, "TVA");
				complianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2022, 12, 3);

				var frenchAccount1 = Creator.CreateGLAccountAndLocalAccountMapping("5436.11.00", Constants.Languages.French, Constants.CountryCodes.France, "BSH");
				var frenchAccount2 = Creator.CreateGLAccountAndLocalAccountMapping("5812.11.00", Constants.Languages.French, Constants.CountryCodes.France, "BSH");
				var frenchAccount3 = Creator.CreateGLAccountAndLocalAccountMapping("5900.11.00", Constants.Languages.English, Constants.CountryCodes.France, "BSH");  // we can't use this mapping because of incorrect language
				var spanishAccount = Creator.CreateGLAccountAndLocalAccountMapping("9988.11.00", Constants.Languages.French, Constants.CountryCodes.Spain, "BSH");  // we can't use this mapping because of incorrect country
																																									// This account has a posting before and after the report date range but not in the report date range and is irrelevant therefore.
				var frenchAccount4 = Creator.CreateGLHeader("5436.11.10", "P&L");
				// This account has a posting in the report date range and is relevant therefore.
				var frenchAccount5 = Creator.CreateGLHeader("5436.11.20", "P&L");
				// This account is a balance sheet account with a posting before the report date range and is relevant.
				var frenchAccount6 = Creator.CreateGLHeader("5812.11.10", "BSH");
				// This account is a balance sheet account with a posting after the report date range and is irrelevant.
				var frenchAccount7 = Creator.CreateGLHeader("8812.11.10", "BSH");
				// The mapping for retained earnings account is always required.

				var companyPK = complianceReport.Company.PK;
				Creator.CreateAccGLAggregate(100, 202201, frenchAccount1.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(200, 202202, frenchAccount1.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(300, 202203, frenchAccount1.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(400, 202301, frenchAccount2.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");  // after our report end date and therefore not relevant
				Creator.CreateAccGLAggregate(500, 202101, frenchAccount3.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");  // before our report start date but everything before is relevant too
				Creator.CreateAccGLAggregate(600, 202102, frenchAccount3.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(700, 202112, spanishAccount.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(800, 202201, spanishAccount.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(100, 202104, frenchAccount4.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(100, 202304, frenchAccount4.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(200, 202205, frenchAccount5.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(300, 202106, frenchAccount6.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(300, 202307, frenchAccount7.PK, GlbBranch.CurrentBranch.PK, companyPK, GlbDepartment.CurrentDepartment.PK, "");
				Factory.Save();

				var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, Constants.Languages.French);
				var mappingExistsForAllRelevantAccounts = accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus();
				AssertEquals("Mapping exists for all relevant accounts", false, mappingExistsForAllRelevantAccounts);
				var notes = complianceReport.GetNotes().GetAllNotes();
				AssertEquals("Number of notes attached to report", 1, notes.Count);
				var firstNote = notes.Cast<StmNote>().First();
				AssertEquals("Note text", "This report requires a complete mapping of GL Accounts to local accounts but some mappings are not defined in MAINTAIN > ACCOUNT > GL MULTI-LANGUAGE MAPPING.\r\n\r\n4900.00.00 - RETAINED EARNINGS FROM PREVIOUS YR\r\n5436.11.20 - \r\n5812.11.10 - \r\n5900.11.00 - \r\n9988.11.00 - ", firstNote.ST_NoteDataAsText);
				AssertEquals("Account mapping is incomplete. Please check the Notes tab for details.", complianceReport.ACR_StatusMessage);

				// fix the mapping and test again
				complianceReport.ACR_Status = AccComplianceReport.Status.ReportCreated;
				complianceReport.GetNotes().GetAllNotes().DeleteAll();
				Creator.CreateLocalAccountMappingForGLHeader(frenchAccount3, Constants.Languages.French, Constants.CountryCodes.France);
				Creator.CreateLocalAccountMappingForGLHeader(spanishAccount, Constants.Languages.French, Constants.CountryCodes.France);
				Creator.CreateLocalAccountMappingForGLHeader(frenchAccount5, Constants.Languages.French, Constants.CountryCodes.France);
				Creator.CreateLocalAccountMappingForGLHeader(frenchAccount6, Constants.Languages.French, Constants.CountryCodes.France);
				var retainedEarningsAccount = Creator.CreateRetainedEarningsAccount();
				Creator.CreateLocalAccountMappingForGLHeader(retainedEarningsAccount, Constants.Languages.French, Constants.CountryCodes.France);
				Factory.Save();
				accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, Constants.Languages.French);
				mappingExistsForAllRelevantAccounts = accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus();
				AssertEquals("Mapping exists for all relevant accounts", true, mappingExistsForAllRelevantAccounts);
				AssertEquals("Number of notes attached to report", 0, complianceReport.GetNotes().GetAllNotes().Count);
				AssertEquals("Report is waiting to be queued. Please wait.", complianceReport.ACR_StatusMessage);
			}
		}

		public void TestCheckMappingWithMissingMapping()
		{
			bool? previousSuppressReportingOfErrors = null;
			using (new DisposableAction(() =>
					{
						previousSuppressReportingOfErrors = ErrorReporter.SuppressReportingOfErrors;
						ErrorReporter.SuppressReportingOfErrors = true;
					},
					() =>
					{
						ErrorReporter.SuppressReportingOfErrors = previousSuppressReportingOfErrors ?? ErrorReporter.SuppressReportingOfErrors;
					}))
			{
				var accountWithMissingMapping = Factory.NewWithValidTestData<AccGLHeader>();
				accountWithMissingMapping.AG_AccountNum = "1234.56.78";
				accountWithMissingMapping.AG_Description = "Test GL Header";
				Factory.Save();
				Creator.CreateLocalAccountMappingForGLHeader(accountWithMissingMapping, Constants.Languages.French, Constants.CountryCodes.France);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
				{
					var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
					complianceReport.ACR_ReportType = CreateDayBookReportConfigForSpecificCountry(Constants.CountryCodes.France, "TVA");
					var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, Constants.Languages.English);

					AssertEquals("Test for France: 1234.56.78 - Multi account mapped", ("", ""), accountMappingHelper.GetLocalAccount(accountWithMissingMapping.PK, ""));
					AssertEquals("Test for France: Mapping errors reported via ErrorReporter", 1, accountMappingHelper.UnmappedLocalAccounts.Count);

					Assert(!accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus());

					var notes = complianceReport.GetNotes().GetAllNotes();
					AssertEquals("Number of notes attached to report", 1, notes.Count);
					var firstNote = notes.Cast<StmNote>().First();
					AssertEquals("Note text", "This report requires a complete mapping of GL Accounts to local accounts but some mappings are not defined in MAINTAIN > ACCOUNT > GL MULTI-LANGUAGE MAPPING.\r\n\r\n1234.56.78 - Test GL Header\r\n4900.00.00 - RETAINED EARNINGS FROM PREVIOUS YR", firstNote.ST_NoteDataAsText);
					AssertEquals(AccComplianceReport.Status.ReportError, complianceReport.ACR_Status);
					AssertEquals("StatusMessage value", "Account mapping is incomplete. Please check the Notes tab for details.",
						complianceReport.ACR_StatusMessage);
				}
			}
		}

		public void TestFunctionalityforSwitchingBetweenLocalAccountsAndStandardChartofAccountsWhenIgnoreLocalMappingRegistryIsFalse()
		{
			var standardAccount1 = Creator.CreateGLHeader("TEST.10.10");
			standardAccount1.AG_Description = "Test Standard Account 1";
			var retainedEarningsAccount = Creator.CreateGLHeader("RRRR.10.10");
			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, retainedEarningsAccount.PK.ToGuid());
			var standardAccount2 = Creator.CreateGLHeader("TEST.10.20");
			standardAccount2.AG_Description = "Test Standard Account 2";
			Creator.CreateLocalAccountMappingForGLHeader(standardAccount1, Constants.Languages.French, Constants.CountryCodes.France);
			Creator.CreateLocalAccountMappingForGLHeader(retainedEarningsAccount, Constants.Languages.French, Constants.CountryCodes.France);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			using (AccountingMasterFilesRegistry.Instance.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = CreateDayBookReportConfigForSpecificCountry(Constants.CountryCodes.France, "TVA");
				var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, Constants.Languages.French);
				AssertEquals("Test for France: PLAppropriationAccount Account", ("FR-RRRR.10.10", "FR-RRRR.10.10 - Description"), accountMappingHelper.GetLocalAccount(retainedEarningsAccount.PK, ""));
				AssertEquals("Test for France: TEST.10.10 - French account mapped", ("FR-TEST.10.10", "FR-TEST.10.10 - Description"), accountMappingHelper.GetLocalAccount(standardAccount1.PK, ""));
				AssertEquals("Unmapped Local Account count should be 0", 0, accountMappingHelper.UnmappedLocalAccounts.Count);
				AssertEquals("No errors should be present", true, accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus());
				AssertEquals("Test for France: TEST.10.20 - French account not mapped", ("", ""), accountMappingHelper.GetLocalAccount(standardAccount2.PK, ""));
				AssertEquals("Unmapped Local Account count should be 1", 1, accountMappingHelper.UnmappedLocalAccounts.Count);
				AssertEquals("Unmapped Local account should lead to errors", false, accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus());
			}
		}

		public void TestFunctionalityforSwitchingBetweenLocalAccountsAndStandardChartofAccountsWhenIgnoreLocalMappingRegistryIsTrue()
		{
			var standardAccount1 = Creator.CreateGLHeader("TEST.10.10");
			standardAccount1.AG_Description = "Test Standard Account 1";
			var standardAccount2 = Creator.CreateGLHeader("TEST.10.20");
			standardAccount2.AG_Description = "Test Standard Account 2";
			Creator.CreateLocalAccountMappingForGLHeader(standardAccount1, Constants.Languages.French, Constants.CountryCodes.France);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			using (AccountingMasterFilesRegistry.Instance.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = CreateDayBookReportConfigForSpecificCountry(Constants.CountryCodes.France, "TVA");
				var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, Constants.Languages.French);
				AssertEquals("Test for France: TEST.10.10 - standard account mapped", ("TEST.10.10", "Test Standard Account 1"), accountMappingHelper.GetLocalAccount(standardAccount1.PK, ""));
				AssertEquals("Test for France: TEST.10.20 - standard account mapped", ("TEST.10.20", "Test Standard Account 2"), accountMappingHelper.GetLocalAccount(standardAccount2.PK, ""));
				AssertEquals("Unmapped Local Account count should be 0", 0, accountMappingHelper.UnmappedLocalAccounts.Count);
				AssertEquals("No errors should be present", true, accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus());
			}
		}

		#region Implementation

		ZString CreateDayBookReportConfigForSpecificCountry(ZString countryCode, ZString taxRegistrationType)
		{
			var reportCode = $"X{countryCode}";
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = reportCode;
			reportConfig.ReportTitle = "Test Compliance Report";
			reportConfig.Country = countryCode;
			reportConfig.TaxRegistrationType = taxRegistrationType;
			reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			return reportCode;
		}

		#endregion
	}
}
