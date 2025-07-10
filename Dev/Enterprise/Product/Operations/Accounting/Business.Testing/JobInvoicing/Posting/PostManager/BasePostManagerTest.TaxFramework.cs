using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	partial class BasePostManagerTest
	{
		public void TestCalculateOtherTaxesWithError()
		{
			AssertCalculateOtherTaxesWithError(true);
		}

		public void TestCalculateOtherTaxesWithoutError()
		{
			AssertCalculateOtherTaxesWithError(false);
		}

		void AssertCalculateOtherTaxesWithError(bool hasError)
		{
			var lastErrorReported = "";
			var postManager = SetupPostManagerForOtherTaxes(true);
			postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;

			var taxProcessorMock = new Mock<ITaxProcessor>(MockBehavior.Strict);
			var expectedError = hasError ? "Tax Transactions Error message" : "";
			taxProcessorMock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns(expectedError);
			ObjectFactory.Substitute(taxProcessorMock.Object);

			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals("CancelPosting", hasError, postManager.CancelPosting);
			AssertEquals(nameof(lastErrorReported), expectedError, lastErrorReported);
			taxProcessorMock.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Exactly(hasError ? 1 : GetProcessTaxesOnPosting_CallCount(true)));
		}

		public void TestCalculatesOtherTaxes_WhenActiveTaxConfigurationsExist()
		{
			var postManager = SetupPostManagerForOtherTaxes(isTaxConfigActive: true);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Assert("CancelPosting", !postManager.CancelPosting);
			taxProcessorMock.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Exactly(GetProcessTaxesOnPosting_CallCount(true)));
		}

		public void TestDoesNotCalculateOtherTaxes_WhenNoTaxConfigurationsExist()
		{
			var postManager = SetupPostManagerForOtherTaxes(hasTaxConfigs: false);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Assert("CancelPosting", !postManager.CancelPosting);
			taxProcessorMock.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Exactly(GetProcessTaxesOnPosting_CallCount(false)));
		}

		public void TestCalculatesOtherTaxes_WhenTaxConfigurationsAreInactive()
		{
			var postManager = SetupPostManagerForOtherTaxes(isTaxConfigActive: false);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Assert("CancelPosting", !postManager.CancelPosting);
			taxProcessorMock.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), Times.Exactly(GetProcessTaxesOnPosting_CallCount(true)));
		}

		public void Test_ISSTaxes_SingleChargeCodeCreatesMultipleISSTaxes_Case1()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				var organization = TestObjectCreator.TestOrganisation;
				var chargeCode = TestObjectCreator.CC1;
				SetupMultipleTaxesForSingleChargeCode(organization, chargeCode);

				SetupDebtor(organization);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CreateCharge(job, chargeCode, debtor: organization);

				Factory.Save();

				var lastErrorReported = "";
				var postManager = GetPostManager(new[] { job });
				postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;
				var expectedErrorMessage = @"Posting is prevented because Tax defaulting rules for Charge Code 'ZZCC1' would create more than one Tax record for the 'ISS' Tax System.
Please review the Tax Override rules configured for this charge code.";

				AssertNoExceptionThrown(expectedErrorMessage, () => postManager.CreateTransactions(GetRevenuePostingOption()));

				Assert("Posting is cancelled", postManager.CancelPosting);
				AssertEquals(expectedErrorMessage, lastErrorReported);
			}
		}

		public void Test_ISSTaxes_SingleChargeCodeCreatesMultipleISSTaxes_Case2()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				var organization = TestObjectCreator.TestOrganisation;
				var chargeCode1 = TestObjectCreator.CC1;
				var chargeCode2 = TestObjectCreator.CC2;
				SetupMultipleTaxesForSingleChargeCode(organization, chargeCode1);
				SetupMultipleTaxesForSingleChargeCode(organization, chargeCode2, "TA3", "TS4");

				SetupDebtor(organization);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CreateCharge(job, chargeCode1, debtor: organization);
				TestObjectCreator.CreateCharge(job, chargeCode2, debtor: organization);

				Factory.Save();

				var lastErrorReported = "";
				var postManager = GetPostManager(new[] { job });
				postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;
				var expectedErrorMessage = @"Posting is prevented because Tax defaulting rules for Charge Code 'ZZCC1' would create more than one Tax record for the 'ISS' Tax System.
Please review the Tax Override rules configured for this charge code.";

				AssertNoExceptionThrown(expectedErrorMessage, () => postManager.CreateTransactions(GetRevenuePostingOption()));

				Assert("Posting is cancelled", postManager.CancelPosting);
				AssertEquals(expectedErrorMessage, lastErrorReported);
			}
		}

		public void Test_ISSTaxes_HeaderBranchAndLineBranchBothPopulated()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				var periodHelper = new AccountingPeriodTestHelper(Factory);
				periodHelper.SetupPeriods();

				var taxTestHelper = new AccountingTestObjectCreator(Factory);
				var taxAuthority1 = taxTestHelper.CreateTaxAuthority("TA1");
				var taxAuthority2 = taxTestHelper.CreateTaxAuthority("TA2");

				var taxSystem = taxTestHelper.CreateTaxSystem("TS", null, false, TaxSystemRegistrationLevels.Branch.Code, TaxSuperTypeList.TurnoverTax.Code);
				taxSystem.Code = "ISS";
				taxSystem.Country = Constants.CountryCodes.Brazil;

				var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);

				var taxConfiguration1 = taxTestHelper.CreateTaxConfiguration(branch1, taxAuthority1, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				taxConfiguration1.ETC_AG_TaxExpenseAccount = TestObjectCreator.GLHeader1.PK;
				taxConfiguration1.ETC_AG_TaxControlAccount = TestObjectCreator.GLHeader2.PK;
				taxConfiguration1.ETC_AG_LedgerControlAccount = ZGuid.Empty;

				var taxConfiguration2 = taxTestHelper.CreateTaxConfiguration(branch1, taxAuthority2, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				taxConfiguration2.ETC_AG_TaxExpenseAccount = TestObjectCreator.GLHeader1.PK;
				taxConfiguration2.ETC_AG_TaxControlAccount = TestObjectCreator.GLHeader2.PK;
				taxConfiguration2.ETC_AG_LedgerControlAccount = ZGuid.Empty;

				var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
				taxSystemsConfigCollection.Add(taxSystem);
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
				Factory.Save();

				var organization = TestObjectCreator.TestOrganisation;
				var company = GlbCompany.CurrentCompany;
				var companyData = organization.GetCompanyDataForGlbCompany(company);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration1, companyData);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration2, companyData);

				var chargeCode1 = TestObjectCreator.CC1;
				var chargeCode2 = TestObjectCreator.CC2;

				var taxID1 = taxTestHelper.CreateTaxRate_RateSource("TID", "TX1");
				taxID1.SetRate_ForTestOnly(10, 1);

				var taxID2 = taxTestHelper.CreateTaxRate_RateSource("TID", "TX2");
				taxID2.SetRate_ForTestOnly(5, 1);

				var taxFrameTaxOverrideGroup1 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration1);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode1);

				var taxFrameTaxOverrideGroup2 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration2);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode2);

				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);
				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK);

				Factory.Save();

				SetupDebtor(organization);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var jobCharge1 = TestObjectCreator.CreateCharge(job, chargeCode1, debtor: organization);
				jobCharge1.JR_GB = branch1.PK;
				jobCharge1.JR_GB_SellTaxBranch = branch1.PK;

				var jobCharge2 = TestObjectCreator.CreateCharge(job, chargeCode2, debtor: organization);
				jobCharge2.JR_GB = branch1.PK;
				jobCharge2.JR_GB_SellTaxBranch = branch1.PK;

				Factory.Save();

				var postManager = GetPostManager(new[] { job }, branch1);

				var transactions = postManager.CreateTransactions(GetRevenuePostingOption());

				AssertEquals("Posting should not be cancelled", false, postManager.CancelPosting);
				AssertEquals("There should be two invoices", 2, transactions.ARTransactionsCount);

				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		public void Test_ISSTaxes_SplittingUsesTaxBranchConfiguration()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				var periodHelper = new AccountingPeriodTestHelper(Factory);
				periodHelper.SetupPeriods();

				var taxTestHelper = new AccountingTestObjectCreator(Factory);
				var taxAuthority1 = taxTestHelper.CreateTaxAuthority("TA1");
				var taxAuthority2 = taxTestHelper.CreateTaxAuthority("TA2");

				var taxSystem = taxTestHelper.CreateTaxSystem("TS", null, false, TaxSystemRegistrationLevels.Branch.Code, TaxSuperTypeList.TurnoverTax.Code);
				taxSystem.Code = "ISS";
				taxSystem.Country = Constants.CountryCodes.Brazil;

				var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
				var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

				var taxConfiguration1 = taxTestHelper.CreateTaxConfiguration(branch1, taxAuthority1, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				taxConfiguration1.ETC_AG_TaxExpenseAccount = TestObjectCreator.GLHeader1.PK;
				taxConfiguration1.ETC_AG_TaxControlAccount = TestObjectCreator.GLHeader2.PK;
				taxConfiguration1.ETC_AG_LedgerControlAccount = ZGuid.Empty;

				var taxConfiguration2 = taxTestHelper.CreateTaxConfiguration(branch1, taxAuthority2, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				taxConfiguration2.ETC_AG_TaxExpenseAccount = TestObjectCreator.GLHeader1.PK;
				taxConfiguration2.ETC_AG_TaxControlAccount = TestObjectCreator.GLHeader2.PK;
				taxConfiguration2.ETC_AG_LedgerControlAccount = ZGuid.Empty;

				var taxConfiguration3 = taxTestHelper.CreateTaxConfiguration(branch2, taxAuthority1, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				taxConfiguration3.ETC_AG_TaxExpenseAccount = TestObjectCreator.GLHeader1.PK;
				taxConfiguration3.ETC_AG_TaxControlAccount = TestObjectCreator.GLHeader2.PK;
				taxConfiguration3.ETC_AG_LedgerControlAccount = ZGuid.Empty;

				var taxConfiguration4 = taxTestHelper.CreateTaxConfiguration(branch2, taxAuthority2, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				taxConfiguration4.ETC_AG_TaxExpenseAccount = TestObjectCreator.GLHeader1.PK;
				taxConfiguration4.ETC_AG_TaxControlAccount = TestObjectCreator.GLHeader2.PK;
				taxConfiguration4.ETC_AG_LedgerControlAccount = ZGuid.Empty;

				var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
				taxSystemsConfigCollection.Add(taxSystem);
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
				Factory.Save();

				var organization = TestObjectCreator.TestOrganisation;
				var company = GlbCompany.CurrentCompany;
				var companyData = organization.GetCompanyDataForGlbCompany(company);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration1, companyData);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration2, companyData);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration3, companyData);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration4, companyData);

				var chargeCode1 = TestObjectCreator.CC1;
				var chargeCode2 = TestObjectCreator.CC2;

				var taxID1 = taxTestHelper.CreateTaxRate_RateSource("TID", "TX1");
				taxID1.SetRate_ForTestOnly(10, 1);

				var taxID2 = taxTestHelper.CreateTaxRate_RateSource("TID", "TX2");
				taxID2.SetRate_ForTestOnly(5, 1);

				var taxFrameTaxOverrideGroup1 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration1);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode1);

				var taxFrameTaxOverrideGroup2 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration2);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode2);

				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);
				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK);

				var taxFrameTaxOverrideGroup3 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration3);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup3, chargeCode1);

				var taxFrameTaxOverrideGroup4 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration4);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup4, chargeCode2);

				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup3, taxID1.PK);
				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup4, taxID2.PK);

				Factory.Save();

				SetupDebtor(organization);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var jobCharge1 = TestObjectCreator.CreateCharge(job, chargeCode1, debtor: organization);
				jobCharge1.JR_GB = branch1.PK;
				jobCharge1.JR_GB_SellTaxBranch = branch2.PK;

				var jobCharge2 = TestObjectCreator.CreateCharge(job, chargeCode2, debtor: organization);
				jobCharge2.JR_GB = branch1.PK;
				jobCharge2.JR_GB_SellTaxBranch = branch2.PK;

				Factory.Save();

				var postManager = GetPostManager(new[] { job }, branch2);

				var transactions = postManager.CreateTransactions(GetRevenuePostingOption());

				AssertEquals("Posting should not be cancelled", false, postManager.CancelPosting);
				AssertEquals("There should be two invoices", 2, transactions.ARTransactionsCount);

				AssertNoExceptionThrown(() => Factory.Save());

				var taxTransactions = Factory.Load<AccTaxTransaction>(new ZQuery());
				AssertEquals(2, taxTransactions.Length);
				Assert("Both the tax trasactions are linked to branch2", taxTransactions.All(x => x.ATT_GB == branch2.PK));
			}
		}

		public void Test_ISSTaxes_MultipleChargeCodesCreateMultipleISSTaxes()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				var taxTestHelper = new AccountingTestObjectCreator(Factory);

				var taxAuthority1 = taxTestHelper.CreateTaxAuthority("TA1");
				var taxAuthority2 = taxTestHelper.CreateTaxAuthority("TA2");
				var taxSystem = taxTestHelper.CreateTaxSystem("TS", null, false, TaxSystemRegistrationLevels.Branch.Code, TaxSuperTypeList.TurnoverTax.Code);
				taxSystem.Code = "ISS";
				taxSystem.Country = Constants.CountryCodes.Brazil;

				var taxConfiguration1 = taxTestHelper.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority1, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				var taxConfiguration2 = taxTestHelper.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority2, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

				var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
				taxSystemsConfigCollection.Add(taxSystem);
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
				Factory.Save();

				var organization = TestObjectCreator.TestOrganisation;
				var company = GlbCompany.CurrentCompany;
				var companyData = organization.GetCompanyDataForGlbCompany(company);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration1, companyData);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration2, companyData);

				var chargeCode1 = TestObjectCreator.CC1;
				var chargeCode2 = TestObjectCreator.CC2;
				var chargeCode3 = TestObjectCreator.CC3;
				var taxFrameTaxOverrideGroup1 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration1);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode1);

				var taxFrameTaxOverrideGroup2 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration2);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode2);

				var taxID1 = taxTestHelper.CreateTaxRate_RateSource("TID", "TX1");
				taxID1.SetRate_ForTestOnly(10, 1);

				var taxID2 = taxTestHelper.CreateTaxRate_RateSource("TID", "TX2");
				taxID2.SetRate_ForTestOnly(5, 1);

				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);
				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK);

				Factory.Save();

				SetupDebtor(organization);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CreateCharge(job, chargeCode1, debtor: organization);
				TestObjectCreator.CreateCharge(job, chargeCode2, debtor: organization);
				TestObjectCreator.CreateCharge(job, chargeCode3, debtor: organization);

				Factory.Save();

				var postManager = GetPostManager(new[] { job });

				var transactions = postManager.CreateTransactions(GetRevenuePostingOption());

				AssertEquals("Posting should not be cancelled", false, postManager.CancelPosting);
				AssertEquals("There should be two invoices", 2, transactions.ARTransactionsCount);
			}
		}

		public void Test_ISSTaxes_MultipleChargeCodesCreateSingleISSTax()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				var taxTestHelper = new AccountingTestObjectCreator(Factory);

				var taxAuthority = taxTestHelper.CreateTaxAuthority("TA");
				var taxSystem = taxTestHelper.CreateTaxSystem("TS", null, false, TaxSystemRegistrationLevels.Branch.Code, TaxSuperTypeList.TurnoverTax.Code);
				taxSystem.Code = "ISS";
				taxSystem.Country = Constants.CountryCodes.Brazil;

				var taxConfiguration = taxTestHelper.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

				var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
				taxSystemsConfigCollection.Add(taxSystem);
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
				Factory.Save();

				var organization = TestObjectCreator.TestOrganisation;
				var company = GlbCompany.CurrentCompany;
				var companyData = organization.GetCompanyDataForGlbCompany(company);
				taxTestHelper.CreateOrgTaxConfiguration(taxConfiguration, companyData);

				var chargeCode1 = TestObjectCreator.CC1;
				var chargeCode2 = TestObjectCreator.CC2;
				var chargeCode3 = TestObjectCreator.CC3;
				var taxFrameTaxOverrideGroup1 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode1);

				var taxFrameTaxOverrideGroup2 = taxTestHelper.CreateTaxOverrideGroup(company, taxConfiguration);
				taxTestHelper.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode2);

				var taxID = taxTestHelper.CreateTaxRate_RateSource("TID", "TX");
				taxID.SetRate_ForTestOnly(10, 1);

				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID.PK);
				taxTestHelper.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID.PK);

				Factory.Save();

				SetupDebtor(organization);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CreateCharge(job, chargeCode1, debtor: organization);
				TestObjectCreator.CreateCharge(job, chargeCode2, debtor: organization);
				TestObjectCreator.CreateCharge(job, chargeCode3, debtor: organization);

				Factory.Save();

				var postManager = GetPostManager(new[] { job });

				var transactions = postManager.CreateTransactions(GetRevenuePostingOption());

				AssertEquals("Posting should not be cancelled", false, postManager.CancelPosting);
				AssertEquals("There should be one invoice", 1, transactions.ARTransactionsCount);
			}
		}

		public void TestCalculateOtherTaxes()
		{
			var transactions = SetupDataToCalculateOtherTaxes();

			AssertEquals("PostCondition: Posted Transactions Count", GetProcessTaxesOnPosting_CallCount(true), transactions.Count);
			AssertEquals("PostCondition: Posted AR Transactions Count", 1, transactions.ARTransactionsCount);
			if (transactions.Count == 2)
			{
				AssertEquals("PostCondition: Posted AP Transactions Count", 1, transactions.APTransactionsCount);
			}

			CombineAssertions(() =>
			{
				foreach (InvoicingBase transaction in transactions.Values)
				{
					int multiplier = transaction.AH_Ledger == LedgerTypes.AccountsPayable ? -1 : 1;
					AssertEquals($"{transaction.AH_Desc}: AH_OSTaxAmountOtherTaxes", 10m * multiplier, transaction.AH_OSTaxAmountOtherTaxes);
					AssertEquals($"{transaction.AH_Desc}: AH_LocalTaxAmountOtherTaxes", 10m * multiplier, transaction.AH_LocalTaxAmountOtherTaxes);
					AssertEquals($"{transaction.AH_Desc}: TaxTransactionCollection.Count", 1,
						TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(transaction).TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection.Count);
				}
			});
		}

		public void TestCalculateOtherTaxes_NoTaxCalcualtionsWhenForCancelledPosting()
		{
			var transactions = SetupDataToCalculateOtherTaxes(true);

			AssertEquals("PostCondition: Posted Transactions Count", GetProcessTaxesOnPosting_CallCount(true), transactions.Count);
			AssertEquals("PostCondition: Posted AR Transactions Count", 1, transactions.ARTransactionsCount);
			if (transactions.Count == 2)
			{
				AssertEquals("PostCondition: Posted AP Transactions Count", 1, transactions.APTransactionsCount);
			}

			CombineAssertions(() =>
			{
				foreach (InvoicingBase transaction in transactions.Values)
				{
					AssertEquals($"{transaction.AH_Desc}: AH_OSTaxAmountOtherTaxes", 0m, transaction.AH_OSTaxAmountOtherTaxes);
					AssertEquals($"{transaction.AH_Desc}: AH_LocalTaxAmountOtherTaxes", 0m, transaction.AH_LocalTaxAmountOtherTaxes);
					AssertEquals($"{transaction.AH_Desc}: TaxTransactionCollection.Count", 0,
						TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(transaction).TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection.Count);
				}
			});
		}

		public void TestCalculateOtherTaxes_RaiseErrorOnInvoice_WhenHasErrorsOnTaxTransaction()
		{
			var chargeCode = TestObjectCreator.CC1;
			var org = TestObjectCreator.TestOrganisation;
			TaxTestObjectCreator.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, org, chargeCode, taxSource: "ORG", taxCode: "TXRATECD", rateNumerator: 0);
			SetupDebtor(org);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"), false);
			TestObjectCreator.CreateCharge(job, chargeCode, debtor: org);

			var lastErrorReported = "";
			var expectedTaxTransactionErrorMessage = "[Tax record] Rate: No valid tax rate found for tax ID 'TXRATECD'. Please check the Rate Source of the tax ID and make sure there are valid tax rate for the Rate Source.";

			var postManager = GetPostManager(new[] { job });
			postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;

			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals(nameof(lastErrorReported), expectedTaxTransactionErrorMessage, lastErrorReported);
			Assert(postManager.CancelPosting);
		}

		AccountingTestObjectCreator TaxTestObjectCreator => taxTestObjectCreator ?? (taxTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator taxTestObjectCreator;

		BasePostManager SetupPostManagerForOtherTaxes(bool isTaxConfigActive = true, bool hasTaxConfigs = true)
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
			var chargeCode = TestObjectCreator.CC1;
			var org = TestObjectCreator.TestOrganisation;
			if (hasTaxConfigs)
			{
				taxTestHelper.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, org, chargeCode, LedgerTypes.AccountsReceivable, isTaxConfigActive: isTaxConfigActive);
				taxTestHelper.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, org, chargeCode, LedgerTypes.AccountsPayable, isTaxConfigActive: isTaxConfigActive);
			}
			SetupDebtor(org);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.CreateCharge(job, chargeCode, creditor: org, invoiceNum: "INV1", debtor: org);
			var postManager = GetPostManager(new[] { job });
			return postManager;
		}

		void SetupMultipleTaxesForSingleChargeCode(OrgHeader organization, AccChargeCode chargeCode, string taxAuthorityCode1 = "TA1", string taxAuthorityCode2 = "TA2")
		{
			var taxAuthority1 = TaxTestObjectCreator.CreateTaxAuthority(taxAuthorityCode1);
			var taxAuthority2 = TaxTestObjectCreator.CreateTaxAuthority(taxAuthorityCode2);
			var taxSystem = TaxTestObjectCreator.CreateTaxSystem("TS", null, false, TaxSystemRegistrationLevels.Branch.Code, TaxSuperTypeList.TurnoverTax.Code);
			taxSystem.Code = "ISS";
			taxSystem.Country = Constants.CountryCodes.Brazil;

			var taxConfiguration1 = TaxTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority1, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfiguration2 = TaxTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority2, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			var company = GlbCompany.CurrentCompany;
			var companyData = organization.GetCompanyDataForGlbCompany(company);
			TaxTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, companyData);
			TaxTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration2, companyData);

			var taxFrameTaxOverrideGroup1 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration1);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode);

			var taxFrameTaxOverrideGroup2 = TaxTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration2);
			TaxTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup2, chargeCode);

			var taxID1 = TaxTestObjectCreator.CreateTaxRate_RateSource("TID", "TX1");
			taxID1.SetRate_ForTestOnly(10, 1);

			var taxID2 = TaxTestObjectCreator.CreateTaxRate_RateSource("TID", "TX2");
			taxID2.SetRate_ForTestOnly(5, 1);

			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);
			TaxTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK);

			Factory.Save();
		}

		protected TransactionCreatorHashtable SetupDataToCalculateOtherTaxes(bool isPostingCancelled = false)
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
			var chargeCode = TestObjectCreator.CC1;
			var org = TestObjectCreator.TestOrganisation;
			taxTestHelper.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, org, chargeCode, LedgerTypes.AccountsReceivable);
			taxTestHelper.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, org, chargeCode, LedgerTypes.AccountsPayable);
			SetupDebtor(org);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.CreateCharge(job, chargeCode, creditor: org, invoiceNum: "INV1", debtor: org);
			var postManager = GetPostManager(new[] { job });
			if (isPostingCancelled)
			{
				postManager.CheckForCriticalErrors_ForTestOnly = _ => true;
			}
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);

			AssertEquals("PostCondition: CancelPosting", isPostingCancelled, postManager.CancelPosting);

			return transactions;
		}

		protected virtual int GetProcessTaxesOnPosting_CallCount(bool isTaxSystemActivated) => isTaxSystemActivated ? 2 : 0;
	}
}
