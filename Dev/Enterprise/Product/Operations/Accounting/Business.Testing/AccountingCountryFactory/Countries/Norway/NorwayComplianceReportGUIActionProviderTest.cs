using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class NorwayComplianceReportGUIActionProviderTest : BaseComplianceReportGUIActionProviderTest
	{
		public void TestGetSAFTFileCompressionInfo()
		{
			var provider = GetProvider();

			AssertNotNull(provider);
			var monthlySAFTFileCompressionInfo = provider.GetSAFTFileCompressionInfo();
			Assert(monthlySAFTFileCompressionInfo.IsNeedCompression);
			AssertEquals(10 * 1024 * 1024, monthlySAFTFileCompressionInfo.AllowedMaxSize);
		}

		[TestDate(2025, 05, 20)]
		public override void TestValidateBeforeGenerateSAFT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var (provider, report, debitGLAccount, creditGLAccount) = CreateReportBase();

				var result = provider.ValidateBeforeGenerateSAFT(new[] { report });

				AssertEquals(@"Please configure the 'Tax ID and Tax Message Combination Rules' registry before exporting the SAF-T file.
This registry cannot be empty.

Please create GL Mappings with Language = NB-NO and Country = NO for all active GL Accounts before exporting the SAF-T file.

The Parent GL Accounts are:
A100.00.00, A200.00.00.", result);

				CreateReportTaxMessageCombinationRules(report, debitGLAccount, creditGLAccount);
				CreateReportGLLanguageMappings(debitGLAccount, creditGLAccount);

				result = provider.ValidateBeforeGenerateSAFT(new[] { report });

				AssertEquals(ZString.Empty, result);
			}
		}

		[TestDate(2025, 05, 20)]
		public void TestValidateBeforeGenerateSAFTv1_30_EmptyAlternateChartAccounts()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var (provider, report, debitGLAccount, creditGLAccount) = CreateReportBase();
				CreateReportTaxMessageCombinationRules(report, debitGLAccount, creditGLAccount);

				var mockFeatureManager = new Mock<IFeatureControlManager>();
				var mockFeatureData = new Mock<IFeatureData>();
				mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
				using (ObjectFactory.Substitute(mockFeatureManager.Object))
				{
					var emptyChartMessage = @"To create a SAF-T report for a Norwegian company, it is mandatory to specify an Alternate Chart of Account in the registry under: Accounting -> Government Compliance Invoice Document -> Norway (NO) -> Alternate Chart of Accounts for SAF-T.";
					var missingMappingMessage = @"All accounts must be mapped and the alternate GL account numbers must be in accordance with the Norwegian tax authorities specifications.
The Parent GL Accounts are: A100.00.00, A200.00.00.";
					var expectedMessage = emptyChartMessage + "\r\n\r\n" + missingMappingMessage;

					var result = provider.ValidateBeforeGenerateSAFT(new[] { report });

					AssertEquals("Validation message must be returned when AlternateChartOfAccountsForSAFT registry item is empty", expectedMessage, result);
				}
			}
		}

		[TestDate(2025, 05, 20)]
		public void TestValidateBeforeGenerateSAFTv1_30_MissingAlternateGLAccounts()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var (provider, report, debitGLAccount, creditGLAccount) = CreateReportBase();
				CreateReportTaxMessageCombinationRules(report, debitGLAccount, creditGLAccount);

				var mockFeatureManager = new Mock<IFeatureControlManager>();
				var mockFeatureData = new Mock<IFeatureData>();
				mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
				using (ObjectFactory.Substitute(mockFeatureManager.Object))
				{
					// Create Alternate Chart of Account
					var testCreator = new TestObjectCreator(Factory);
					var chartCode = "SAFT";
					var chart = testCreator.CreateAlternateChart(chartCode, "SAF-T Alternate GL Accounts");
					testCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
					var glHeader1 = testCreator.CreateGLHeader("Test.aa");

					Factory.Save();

					// Create the first Alternate GL Account record
					testCreator.CreateAccAlternateGLAccountDissection(debitGLAccount, chart.PK, AlternateGLAccountAttributeCode.LFE, true);
					var alternateGLAccount1 = testCreator.CreateAccAlternateGlAccount(chart.PK, "ALT100.00.00", "BSH", "DR", 1, "OV", 1);
					testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount1, debitGLAccount.PK, 1, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC);

					Factory.Save();

					// Set Alternate Chart of Account to SAFT registry item
					var alternateChartRegistryItem = AccountingConfigurationRegistry.Instance.AlternateChartOfAccountsForSAFT;
					alternateChartRegistryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chart.PK.ToGuid());

					Factory.Save();

					AssertEquals("Registry item value must have been set correctly", chart.PK.ToGuid(), alternateChartRegistryItem.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

					var missingMappingMessage = $@"One or more GL accounts have not been mapped to the Alternate Chart of Accounts: {chartCode}.

All accounts must be mapped and the alternate GL account numbers must be in accordance with the Norwegian tax authorities specifications.
The Parent GL Accounts are: A200.00.00.";

					var result1 = provider.ValidateBeforeGenerateSAFT(new[] { report });

					AssertEquals("Validation message must be returned when GL Account Mapping is missing for any account", missingMappingMessage, result1);

					// Create the second Alternate GL Account record
					testCreator.CreateAccAlternateGLAccountDissection(creditGLAccount, chart.PK, AlternateGLAccountAttributeCode.LFE, true);
					var alternateGLAccount2 = testCreator.CreateAccAlternateGlAccount(chart.PK, "ALT200.00.00", "BSH", "DR", 1, "OV", 1);
					testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount2, creditGLAccount.PK, 1, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC);

					Factory.Save();

					var result2 = provider.ValidateBeforeGenerateSAFT(new[] { report });

					AssertEquals("Validation must be passed", string.Empty, result2);
				}
			}
		}

		[TestDate(2025, 05, 20)]
		public void TestValidateBeforeGenerateSAFTv1_30_DuplicateAlternateGLAccounts()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var (provider, report, debitGLAccount, creditGLAccount) = CreateReportBase();
				CreateReportTaxMessageCombinationRules(report, debitGLAccount, creditGLAccount);

				var mockFeatureManager = new Mock<IFeatureControlManager>();
				var mockFeatureData = new Mock<IFeatureData>();
				mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
				using (ObjectFactory.Substitute(mockFeatureManager.Object))
				{
					// Create Alternate Chart of Account
					var testCreator = new TestObjectCreator(Factory);
					var chartCode = "SAFT";
					var chart = testCreator.CreateAlternateChart(chartCode, "SAF-T Alternate GL Accounts");
					testCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
					var glHeader1 = testCreator.CreateGLHeader("Test.aa");

					Factory.Save();

					// Create the first Alternate GL Account record as duplicate
					testCreator.CreateAccAlternateGLAccountDissection(debitGLAccount, chart.PK, AlternateGLAccountAttributeCode.LFO, true);
					var alternateGLAccount1 = testCreator.CreateAccAlternateGlAccount(chart.PK, "LOC100.00.00", "BSH", "DR", 1, "OV", 1);
					var alternateGLAccount2 = testCreator.CreateAccAlternateGlAccount(chart.PK, "FOR100.00.00", "BSH", "DR", 1, "OV", 1);
					testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount1, debitGLAccount.PK, 1, AlternateGLAccountAttributeCode.LFO, LFOCodes.LOC);
					testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount2, debitGLAccount.PK, 2, AlternateGLAccountAttributeCode.LFO, LFOCodes.FOR);

					Factory.Save();

					// Create the second Alternate GL Account record
					testCreator.CreateAccAlternateGLAccountDissection(creditGLAccount, chart.PK, AlternateGLAccountAttributeCode.LFE, true);
					var alternateGLAccount4 = testCreator.CreateAccAlternateGlAccount(chart.PK, "ALT200.00.00", "BSH", "DR", 1, "OV", 1);
					testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount4, creditGLAccount.PK, 1, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC);

					Factory.Save();

					// Set Alternate Chart of Account to SAFT registry item
					var alternateChartRegistryItem = AccountingConfigurationRegistry.Instance.AlternateChartOfAccountsForSAFT;
					alternateChartRegistryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chart.PK.ToGuid());

					Factory.Save();

					AssertEquals("Registry item value must have been set correctly", chart.PK.ToGuid(), alternateChartRegistryItem.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

					var duplicateMappingMessage = $@"All Parent GL accounts must be mapped exactly once in the Alternate Chart of Accounts: {chartCode}.
The following parent GL Accounts are mapped more than once: A100.00.00.";

					var result = provider.ValidateBeforeGenerateSAFT(new[] { report });

					AssertEquals("Validation message must be returned when GL Account Mapping is duplicated for any account", duplicateMappingMessage, result);
				}
			}
		}

		[TestDate(2025, 05, 20)]
		public void TestValidateBeforeGenerateSAFTv1_30_ValidationPasses()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var (provider, report, debitGLAccount, creditGLAccount) = CreateReportBase();
				CreateReportTaxMessageCombinationRules(report, debitGLAccount, creditGLAccount);

				var mockFeatureManager = new Mock<IFeatureControlManager>();
				var mockFeatureData = new Mock<IFeatureData>();
				mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
				using (ObjectFactory.Substitute(mockFeatureManager.Object))
				{
					// Create Alternate Chart of Account
					var testCreator = new TestObjectCreator(Factory);
					var chartCode = "SAFT";
					var chart = testCreator.CreateAlternateChart(chartCode, "SAF-T Alternate GL Accounts");
					testCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
					var glHeader1 = testCreator.CreateGLHeader("Test.aa");

					Factory.Save();

					// Create the first Alternate GL Account record
					testCreator.CreateAccAlternateGLAccountDissection(debitGLAccount, chart.PK, AlternateGLAccountAttributeCode.LFE, true);
					var alternateGLAccount1 = testCreator.CreateAccAlternateGlAccount(chart.PK, "ALT100.00.00", "BSH", "DR", 1, "OV", 1);
					testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount1, debitGLAccount.PK, 1, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC);

					Factory.Save();

					// Set Alternate Chart of Account to SAFT registry item
					var alternateChartRegistryItem = AccountingConfigurationRegistry.Instance.AlternateChartOfAccountsForSAFT;
					alternateChartRegistryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chart.PK.ToGuid());

					Factory.Save();

					AssertEquals("Registry item value must have been set correctly", chart.PK.ToGuid(), alternateChartRegistryItem.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

					// Create the second Alternate GL Account record
					testCreator.CreateAccAlternateGLAccountDissection(creditGLAccount, chart.PK, AlternateGLAccountAttributeCode.LFE, true);
					var alternateGLAccount2 = testCreator.CreateAccAlternateGlAccount(chart.PK, "ALT200.00.00", "BSH", "DR", 1, "OV", 1);
					testCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount2, creditGLAccount.PK, 1, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC);

					Factory.Save();

					var result = provider.ValidateBeforeGenerateSAFT(new[] { report });

					AssertEquals("Validation must be passed", string.Empty, result);
				}
			}
		}

		public void TestValidationMessageWhenWithoutGLLanguageMapping()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var provider = GetProvider();
				AssertNotNull(provider);

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
				Factory.Save();

				var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxRate = taxRateCollection.AddNew();
				validTaxRate.AT_Code = "TGST";
				validTaxRate.AT_Type = "RAT";
				validTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxRate.SetRateNumerator_ForTestOnly(10);
				validTaxRate.AT_ExtraTaxRateType = "QST";
				validTaxRate.SetExtraRate_ForTestOnly(4, 2);

				var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxMessage = taxMessageCollection.AddNew();
				validTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxMessage.A9_TaxGroupCode = "N1";

				Factory.Save();

				var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection
				{
					{ "N1", (NoResString)"Description N1", true, "N1.0" },
				};
				AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

				Factory.Save();

				var config = Creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
					(TransactionLineTypes.Revenue, validTaxRate, validTaxMessage));
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, config);

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
				var previousPeriod = periodCalculator.GetPreviousPeriod(currentPeriod.AM_Period);
				report.ACR_Periodicity = "PER";
				report.AccountingPeriod = currentPeriod.AM_Period;
				report1.ACR_Periodicity = "PER";
				report1.AccountingPeriod = currentPeriod.AM_Period + 1;
				Creator.CreateConfigurationForComplianceReport(report, "**", "DBW");
				Creator.CreateConfigurationForComplianceReport(report1, "**", "DBW");
				Factory.Save();

				var debitGLAccount = Creator.GLHeader1;
				debitGLAccount.AG_Description = "debit GL account";
				debitGLAccount.AG_AccountNum = "A100.00.00";
				debitGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.debit;

				Factory.Save();

				var openingBalance1 = Factory.New<AccGLAggregate>();
				openingBalance1.AA_AG = debitGLAccount.PK;
				openingBalance1.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance1.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance1.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance1.AA_Period = previousPeriod;
				openingBalance1.AA_Amount = 123m;

				Factory.Save();

				var result1 = provider.ValidateBeforeGenerateSAFT(new[] { report });

				AssertEquals(@"Please create GL Mappings with Language = NB-NO and Country = NO for all active GL Accounts before exporting the SAF-T file.

The Parent GL Accounts are:
A100.00.00.", result1);

				var creditGLAccount = Creator.GLHeader2;
				creditGLAccount.AG_Description = "credit GL Account";
				creditGLAccount.AG_AccountNum = "A200.00.00";
				creditGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.credit;

				var openingBalance2 = Factory.New<AccGLAggregate>();
				openingBalance2.AA_AG = creditGLAccount.PK;
				openingBalance2.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance2.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance2.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance2.AA_Period = previousPeriod;
				openingBalance2.AA_Amount = -123m;

				Factory.Save();
				report.ClearReportLines_ForTestOnly();

				var result2 = provider.ValidateBeforeGenerateSAFT(new[] { report });

				AssertEquals(@"Please create GL Mappings with Language = NB-NO and Country = NO for all active GL Accounts before exporting the SAF-T file.

The Parent GL Accounts are:
A100.00.00, A200.00.00.", result2);

				var openingBalance3 = Factory.New<AccGLAggregate>();
				openingBalance3.AA_AG = creditGLAccount.PK;
				openingBalance3.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance3.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance3.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance3.AA_Period = currentPeriod.AM_Period;
				openingBalance3.AA_Amount = -10m;

				Factory.Save();
				report.ClearReportLines_ForTestOnly();

				var result3 = provider.ValidateBeforeGenerateSAFT(new[] { report, report1 });

				AssertEquals(@"Please create GL Mappings with Language = NB-NO and Country = NO for all active GL Accounts before exporting the SAF-T file.

The Parent GL Accounts are:
A100.00.00, A200.00.00.", result3);

				var debitDescriptor_Norway = Creator.CreateAccountDescriptor(Creator.GLHeader1, "71000.95", "COA", "P&L", Core.SharedConstants.Languages.Norwegian, "", "NO", Core.Constants.DebitCredit.Debit);
				debitDescriptor_Norway.AJ_AG = debitGLAccount.PK;

				var creditDescriptor_Norway = Creator.CreateAccountDescriptor(Creator.GLHeader2, "71000.96", "COA", "P&L", Core.SharedConstants.Languages.Norwegian, "", "NO", Core.Constants.DebitCredit.Credit);
				creditDescriptor_Norway.AJ_AG = creditGLAccount.PK;

				Creator.CreateGLDescriptorPivot(debitDescriptor_Norway, Creator.GLHeader1, "P&L", "D11");
				Creator.CreateGLDescriptorPivot(creditDescriptor_Norway, Creator.GLHeader2, "P&L", "D11");

				Factory.Save();

				var result4 = provider.ValidateBeforeGenerateSAFT(new[] { report });

				AssertEquals(ZString.Empty, result4);
			}
		}

		public void TestGLLanguageMappingValidation_WhenGLAccountTypeIsNTE()
		{
			var provider = GetProvider();
			AssertNotNull(provider);

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			var config = testObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration();
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
			report.ACR_Periodicity = "PER";
			report.AccountingPeriod = currentPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
			AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
			Creator.CreateConfigurationForComplianceReport(report, "**", "DBW");
			Factory.Save();

			var debitGLAccount = Creator.GLHeader1;
			debitGLAccount.AG_Description = "debit GL account";
			debitGLAccount.AG_AccountNum = "A100.00.00";
			debitGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.debit;

			Factory.Save();

			var previousPeriod = periodCalculator.GetPreviousPeriod(currentPeriod.AM_Period);

			var openingBalance1 = Factory.New<AccGLAggregate>();
			openingBalance1.AA_AG = debitGLAccount.PK;
			openingBalance1.AA_GE = GlbDepartment.CurrentDepartment.PK;
			openingBalance1.AA_GB = GlbBranch.CurrentBranch.PK;
			openingBalance1.AA_GC = GlbCompany.CurrentCompany.PK;
			openingBalance1.AA_Period = previousPeriod;
			openingBalance1.AA_Amount = 123m;

			Factory.Save();

			report.ClearReportLines_ForTestOnly();

			var result = provider.ValidateBeforeGenerateSAFT(new[] { report });
			var expectedMessage = "Please create GL Mappings with Language = NB-NO and Country = NO for all active GL Accounts before exporting the SAF-T file.";
			AssertContains(expectedMessage, result);

			debitGLAccount.AG_AccountType = Core.Constants.AccountType.Note;
			result = provider.ValidateBeforeGenerateSAFT(new[] { report });
			AssertNotContains(expectedMessage, result);
		}

		(IComplianceReportGUIActionProvider provider, AccComplianceReport report, AccGLHeader debitGLAccount, AccGLHeader creditGLAccount) CreateReportBase()
		{
			IComplianceReportGUIActionProvider provider;
			AccComplianceReport report;
			AccGLHeader debitGLAccount, creditGLAccount;

			provider = GetProvider();
			AssertNotNull(provider);

			report = Factory.NewWithValidTestData<AccComplianceReport>();
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			var config = testObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration();
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
			report.ACR_Periodicity = "PER";
			report.AccountingPeriod = currentPeriod.AM_Period;

			AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
			AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);

			Creator.CreateConfigurationForComplianceReport(report, "**", "DBW");
			Factory.Save();

			debitGLAccount = Creator.GLHeader1;
			debitGLAccount.AG_Description = "debit GL account";
			debitGLAccount.AG_AccountNum = "A100.00.00";
			debitGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.debit;
			creditGLAccount = Creator.GLHeader2;
			creditGLAccount.AG_Description = "credit GL Account";
			creditGLAccount.AG_AccountNum = "A200.00.00";
			creditGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.credit;

			Factory.Save();

			var previousPeriod = periodCalculator.GetPreviousPeriod(currentPeriod.AM_Period);

			var openingBalance1 = Factory.New<AccGLAggregate>();
			openingBalance1.AA_AG = debitGLAccount.PK;
			openingBalance1.AA_GE = GlbDepartment.CurrentDepartment.PK;
			openingBalance1.AA_GB = GlbBranch.CurrentBranch.PK;
			openingBalance1.AA_GC = GlbCompany.CurrentCompany.PK;
			openingBalance1.AA_Period = previousPeriod;
			openingBalance1.AA_Amount = 123m;

			var openingBalance2 = Factory.New<AccGLAggregate>();
			openingBalance2.AA_AG = creditGLAccount.PK;
			openingBalance2.AA_GE = GlbDepartment.CurrentDepartment.PK;
			openingBalance2.AA_GB = GlbBranch.CurrentBranch.PK;
			openingBalance2.AA_GC = GlbCompany.CurrentCompany.PK;
			openingBalance2.AA_Period = previousPeriod;
			openingBalance2.AA_Amount = -123m;

			Factory.Save();

			report.ClearReportLines_ForTestOnly();

			return (provider, report, debitGLAccount, creditGLAccount);
		}

		void CreateReportTaxMessageCombinationRules(AccComplianceReport report, AccGLHeader debitGLAccount, AccGLHeader creditGLAccount)
		{
			var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var validTaxRate = taxRateCollection.AddNew();
			validTaxRate.AT_Code = "TGST";
			validTaxRate.AT_Type = "RAT";
			validTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			validTaxRate.SetRateNumerator_ForTestOnly(10);
			validTaxRate.AT_ExtraTaxRateType = "QST";
			validTaxRate.SetExtraRate_ForTestOnly(4, 2);

			var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var validTaxMessage = taxMessageCollection.AddNew();
			validTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			validTaxMessage.A9_TaxGroupCode = "N1";

			Factory.Save();

			var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection
				{
					{ "N1", (NoResString)"Description N1", true, "N1.0" },
				};
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

			Factory.Save();

			var config = Creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
				(TransactionLineTypes.Revenue, validTaxRate, validTaxMessage));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, config);

			Factory.Save();
		}

		void CreateReportGLLanguageMappings(AccGLHeader debitGLAccount, AccGLHeader creditGLAccount)
		{
			var debitDescriptor_Norway = Creator.CreateAccountDescriptor(Creator.GLHeader1, "71000.95", "COA", "P&L", Core.SharedConstants.Languages.Norwegian, "", "NO", Core.Constants.DebitCredit.Debit);
			debitDescriptor_Norway.AJ_AG = debitGLAccount.PK;

			var creditDescriptor_Norway = Creator.CreateAccountDescriptor(Creator.GLHeader2, "71000.96", "COA", "P&L", Core.SharedConstants.Languages.Norwegian, "", "NO", Core.Constants.DebitCredit.Credit);
			creditDescriptor_Norway.AJ_AG = creditGLAccount.PK;

			Creator.CreateGLDescriptorPivot(debitDescriptor_Norway, Creator.GLHeader1, "P&L", "D11");
			Creator.CreateGLDescriptorPivot(creditDescriptor_Norway, Creator.GLHeader2, "P&L", "D11");

			Factory.Save();
		}

		TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator;

		protected override IComplianceReportGUIActionProvider GetProvider()
		{
			return new NorwayComplianceReportGUIActionProvider();
		}

		protected override bool ExpectedIsCountrySupportGenerateSAFT => true;
	}
}
