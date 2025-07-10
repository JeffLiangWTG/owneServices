using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	class ForwardingConsolCostingValidationTest : CommonConsolCostValidationTest
	{
		protected override JobConsolCost GetConsolCost()
		{
			Consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			Apps = new ApportionmentListing(Factory, Consol);
			return Apps.CostsCollection.TryAddNew();
		}

		ForwardingConsol Consol;
		JobConsolCost Cost1;
		JobConsolCost Cost2;
		ApportionmentListing Apps;
		OrgHeader fTestOrg;
		TestObjectCreator TestObjectCreator;
		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected override void TearDown()
		{
			if (Apps != null)
			{
				Apps.ReleaseMutexes();
			}

			base.TearDown();
		}

		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		protected OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = TestObjectCreator.CreateOrgHeader("TSTREGORG", true, true, true, false, true, false);
				}

				return fTestOrg;
			}
		}

		PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection, ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;
			return newSetting;
		}

		protected void SetUpRegistryForTest()
		{
			OriginalRegistryValueBeforeTest = AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.Value;
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 250, AuthorisationCodes.NoApprovalRequired);
			var above = GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 250, AuthorisationCodes.FirstApprovalRequiredOnly);
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		void ResetRegistryForTest()
		{
			if (OriginalRegistryValueBeforeTest != null)
			{
				AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValueBeforeTest);
			}
		}

		PaymentTwelveLevelAuthorisationSettingsCollection OriginalRegistryValueBeforeTest;
		public void TestCheckE6_AT_TaxRate()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			JobConsolCost cost = GetConsolCost();
			cost.ParentAPInvoice = invoice;
			cost.E6_AH_APInvoice = invoice.PK;
			cost.E6_OH_Creditor = TestOrg.PK;
			cost.Validation.ValidateE6_AT_TaxRate();
			Assert(cost.E6_AT_TaxRateInfo.HasError("Please enter a " + cost.E6_AT_TaxRateInfo.Description + "."));
		}

		public void TestListValidationOnCreditor()
		{
			JobConsolCost cost = GetConsolCost();
			cost.Validation.ValidateE6_OH_Creditor();
			AssertNoErrors(cost.E6_OH_CreditorInfo);
			OrgHeader testOrgNotFlaggedAsCreditor = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			testOrgNotFlaggedAsCreditor.OH_Code = "TESTCODE";
			testOrgNotFlaggedAsCreditor.OH_IsCreditor = false;
			testOrgNotFlaggedAsCreditor.Factory.Save();
			cost.E6_OH_Creditor = testOrgNotFlaggedAsCreditor.PK;
			cost.Validation.ValidateE6_OH_Creditor();
			AssertHasErrors("Shouldn't be able to set a creditor in the consol cost unless it's flagged as AP", cost.E6_OH_CreditorInfo);
		}

		public void TestCheckE6_PlaceOfSupply()
		{
			JobConsolCost cost1 = GetConsolCost();
			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				foreach (var enableRegistry in new[] { true, false })
				{
					AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableRegistry);

					AssertCostPlaceOfSupply(org1, "ABC001", "DL", org1, "ABC001", "JH", enableRegistry);
					AssertCostPlaceOfSupply(org1, "ABC001", "DL", org1, "ABC001", "DL", false);
					AssertCostPlaceOfSupply(org1, "ABC001", "DL", org2, "ABC001", "JH", false);
					AssertCostPlaceOfSupply(org1, "ABC001", "DL", org2, "ABC002", "JH", false);
					AssertCostPlaceOfSupply(org1, "ABC001", "DL", org1, "ABC002", "JH", false);
					AssertCostPlaceOfSupply(org1, "ABC001", "", org1, "ABC001", "JH", enableRegistry);
					AssertCostPlaceOfSupply(org1, "ABC001", "DL", org1, "ABC001", "", enableRegistry);
					AssertCostPlaceOfSupply(null, "ABC001", "DL", org1, "ABC001", "JH", false);
					AssertCostPlaceOfSupply(org1, "ABC001", "DL", null, "ABC001", "JH", false);
					AssertCostPlaceOfSupply(org1, "ABC001", "DL", org1, "", "JH", false);
					AssertCostPlaceOfSupply(org1, "", "DL", org1, "ABC001", "JH", false);
				}
			}

			void AssertCostPlaceOfSupply(OrgHeader cost1Account, string cost1ApInvoiceNum, string cost1FPOS,
				OrgHeader cost2Account, string cost2ApInvoiceNum, string cost2FPOS, bool expectError)
			{
				cost1.E6_PlaceOfSupply = "";
				cost2.E6_PlaceOfSupply = "";

				cost1.E6_OH_Creditor = cost1Account?.PK ?? ZGuid.Empty;
				cost2.E6_OH_Creditor = cost2Account?.PK ?? ZGuid.Empty;
				cost1.E6_InvoiceNum = cost1ApInvoiceNum;
				cost2.E6_InvoiceNum = cost2ApInvoiceNum;
				cost1.E6_PlaceOfSupply = cost1FPOS;
				cost2.E6_PlaceOfSupply = cost2FPOS;

				cost1.Validation.ValidateE6_PlaceOfSupply();
				cost2.Validation.ValidateE6_PlaceOfSupply();

				if (expectError)
				{
					var expectedError = "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the Cost Fixed Place of Supply does not match.";
					AssertHasError("cost1.E6_PlaceOfSupplyInfo.HasError", cost1.E6_PlaceOfSupplyInfo, expectedError);
					AssertHasError("cost2.E6_PlaceOfSupplyInfo.HasError", cost2.E6_PlaceOfSupplyInfo, expectedError);
				}
				else
				{
					AssertNoErrors(cost1.E6_PlaceOfSupplyInfo);
					AssertNoErrors(cost2.E6_PlaceOfSupplyInfo);
				}
			}
		}

		public void TestCreditorOnAccrualIsMandatory()
		{
			JobConsolCost cost = GetConsolCost();
			cost.Validation.ValidateE6_OH_Creditor();
			AssertNoErrors(cost.E6_OH_CreditorInfo);
			cost.E6_LocalCostAmount = 100m;
			cost.Validation.ValidateE6_OH_Creditor();
			AssertNoErrors(cost.E6_OH_CreditorInfo);
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			cost.Validation.ValidateE6_OH_Creditor();
			AssertHasErrors(cost.E6_OH_CreditorInfo);
			var expectedError = "Please enter a Creditor before the job is autorated or saved. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.\r\n\r\n" + "The registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code";
			AssertHasError(cost.E6_OH_CreditorInfo, expectedError);
			cost.E6_LocalCostAmount = 0m;
			cost.Validation.ValidateE6_OH_Creditor();
			AssertNoErrors(cost.E6_OH_CreditorInfo);
		}

		public void TestCreditor_VATConfig()
		{
			var cost = GetConsolCost();
			cost.E6_OH_Creditor = TestOrg.PK;

			cost.Creditor.OH_IsCreditor = false;
			cost.Creditor.OH_IsDebtor = true;
			cost.E6_IsForCollectInvoice = false;
			cost.Creditor.CompanyData.OB_ARVATConfig = "DEF";
			cost.Creditor.CompanyData.OB_APVATConfig = "DEF";
			cost.Validation.ValidateE6_OH_Creditor();
			AssertEquals("EnableTaxBranchReporting is false.", false, AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value);
			AssertEquals("E6_IsForCollectInvoice is false.", false, cost.E6_IsForCollectInvoice);
			AssertNoErrors("No error for Creditor VAT Config because E6_IsForCollectInvoice is false and EnableTaxBranchReporting is false.", cost.E6_OH_CreditorInfo);

			cost.E6_IsForCollectInvoice = true;
			cost.Validation.ValidateE6_OH_Creditor();
			AssertEquals("EnableTaxBranchReporting is false.", false, AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value);
			AssertEquals("E6_IsForCollectInvoice is true.", true, cost.E6_IsForCollectInvoice);
			AssertNoErrors("No error for Creditor VAT Config because EnableTaxBranchReporting is false.", cost.E6_OH_CreditorInfo);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("EnableTaxBranchReporting is true.", true, AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value);
			AssertEquals("E6_IsForCollectInvoice is true.", true, cost.E6_IsForCollectInvoice);
			AssertEquals("OH_IsCreditor is false.", false, cost.Creditor.OH_IsCreditor);
			AssertEquals("OH_IsDebtor is true.", true, cost.Creditor.OH_IsDebtor);
			AssertEquals("IsAPTaxApplicable is equal to IsARTaxApplicable.", cost.Creditor.CompanyData.IsAPTaxApplicable, cost.Creditor.CompanyData.IsARTaxApplicable);

			var expectedError = "The organization ZTSTREGORG used for posting Collect Consol Cost has an invalid configuration. It should be both Receivables and Payables. Make sure that it has the same AR and AP Tax Recognition Rule.";

			cost.Validation.ValidateE6_OH_Creditor();
			AssertHasError("Has error for Creditor VAT Config when EnableTaxBranchReporting is true and E6_IsForCollectInvoice is true, becuase OH_IsCreditor is false.", cost.E6_OH_CreditorInfo, expectedError);

			cost.Creditor.OH_IsCreditor = true;
			cost.Creditor.OH_IsDebtor = false;

			AssertEquals("OH_IsCreditor is true.", true, cost.Creditor.OH_IsCreditor);
			AssertEquals("OH_IsDebtor is false.", false, cost.Creditor.OH_IsDebtor);
			AssertEquals("IsAPTaxApplicable is equal to IsARTaxApplicable.", cost.Creditor.CompanyData.IsAPTaxApplicable, cost.Creditor.CompanyData.IsARTaxApplicable);

			cost.Validation.ValidateE6_OH_Creditor();
			AssertHasError("Has error for Creditor VAT Config when EnableTaxBranchReporting is true and E6_IsForCollectInvoice is true, becuase OH_IsDebtor is true.", cost.E6_OH_CreditorInfo, expectedError);

			cost.Creditor.OH_IsDebtor = true;
			cost.Creditor.CompanyData.OB_APVATConfig = "NON";

			AssertEquals("OH_IsCreditor is true.", true, cost.Creditor.OH_IsCreditor);
			AssertEquals("OH_IsDebtor is true.", true, cost.Creditor.OH_IsDebtor);
			AssertNotEquals("IsAPTaxApplicable is not equal to IsARTaxApplicable.", cost.Creditor.CompanyData.IsAPTaxApplicable, cost.Creditor.CompanyData.IsARTaxApplicable);

			cost.Validation.ValidateE6_OH_Creditor();
			AssertHasError("Has error for Creditor VAT Config when EnableTaxBranchReporting is true and E6_IsForCollectInvoice is true, becuase OB_APVATConfig is not equal to OB_ARVATConfig.", cost.E6_OH_CreditorInfo, expectedError);

			cost.Creditor.CompanyData.OB_APVATConfig = "CSH";

			AssertEquals("OH_IsCreditor is true.", true, cost.Creditor.OH_IsCreditor);
			AssertEquals("OH_IsDebtor is true.", true, cost.Creditor.OH_IsDebtor);
			AssertEquals("IsAPTaxApplicable is equal to IsARTaxApplicable.", cost.Creditor.CompanyData.IsAPTaxApplicable, cost.Creditor.CompanyData.IsARTaxApplicable);

			cost.Validation.ValidateE6_OH_Creditor();
			AssertNoErrors("No error for Creditor VAT Config", cost.E6_OH_CreditorInfo);
		}

		[TestDate(2017, 12, 5)]
		public virtual void TestCheckTransactionNumForAPInvNumAlreadyExist_Standard()
		{
			AssertCheckTransactionNumForAPInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				"The transaction number is already in use. Last posted transaction’s invoice date is 05-Dec-17 which is less than 12 months apart. This transaction number cannot be used. Please enter another one.");
		}

		[TestDate(2017, 12, 5)]
		public virtual void TestCheckTransactionNumForAPInvNumAlreadyExist_Calendar()
		{
			AssertCheckTransactionNumForAPInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				"The transaction number is already in use. Last posted transaction’s invoice date is 05-Dec-17 which is in the same calendar year. This transaction number cannot be used. Please enter another one.");
		}

		void AssertCheckTransactionNumForAPInvNumAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, string expErrorMessage)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string existingAPInvoiceNum = "TESTAP5787";
				InvoicingBase existingInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as InvoicingBase;
				existingInvoice.AH_TransactionNum = existingAPInvoiceNum;
				existingInvoice.AH_OH = TestOrg.PK;
				Factory.Save();
				JobConsolCost cost = GetConsolCost();
				cost.E6_InvoiceNum = existingAPInvoiceNum;
				cost.E6_OH_Creditor = TestOrg.PK;
				cost.E6_InvoiceDate = invoiceDate;

				cost.Validation.ValidateE6_InvoiceNum();
				AssertHasError(cost.E6_InvoiceNumInfo, expErrorMessage);
			}
		}

		[TestDate(2017, 12, 5)]
		public virtual void TestCheckTransactionNumForUAInvNumAlreadyExist_Standard()
		{
			AssertCheckTransactionNumForUAInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				"The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 05-Dec-17 which is less than 12 months apart. This transaction number cannot be used. Please enter another one.");
		}

		[TestDate(2017, 12, 5)]
		public virtual void TestCheckTransactionNumForUAInvNumAlreadyExist_Calendar()
		{
			AssertCheckTransactionNumForUAInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				"The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 05-Dec-17 which is in the same calendar year. This transaction number cannot be used. Please enter another one.");
		}

		void AssertCheckTransactionNumForUAInvNumAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, string expErrorMessage)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string existingUAInvoiceNum = "TESTUA5787";
				InvoicingBase existingInvoice = Factory.NewWithValidTestData(typeof(UAInvoice)) as InvoicingBase;
				existingInvoice.AH_TransactionNum = existingUAInvoiceNum;
				existingInvoice.AH_OH = TestOrg.PK;
				Factory.Save();
				JobConsolCost cost = GetConsolCost();
				cost.E6_InvoiceNum = existingUAInvoiceNum;
				cost.E6_OH_Creditor = TestOrg.PK;
				cost.E6_InvoiceDate = invoiceDate;

				cost.Validation.ValidateE6_InvoiceNum();
				AssertHasError(cost.E6_InvoiceNumInfo, expErrorMessage);
			}
		}

		public void TestMinimumInformationCostValidation()
		{
			Consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			Consol.Shipments.AddNew();
			Apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost cost1 = Apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = ObjectCreator.CC12.PK;
			cost1.E6_OSCostAmount = 100;
			cost1.E6_LocalCostAmount = 100;
			cost1.Validation.ValidateAll();
			AssertNoErrors("There should be no errors", cost1);
		}

		public void TestDocumentReceivedDateValidation()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_DocumentReceivedDate = ZDateTime.Today;
			AssertHasError(cost1.E6_DocumentReceivedDateInfo, "A valid Creditor must be entered when Document Received Date is entered");
			AssertHasError(cost1.E6_DocumentReceivedDateInfo, "An AP Invoice Number must be entered when Document Received Date is entered");

			cost1.E6_DocumentReceivedDate = ZDateTime.Empty;
			AssertNoErrors(cost1.E6_DocumentReceivedDateInfo);

			cost1.E6_InvoiceNum = "123";
			AssertNoErrors(cost1.E6_DocumentReceivedDateInfo);

			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				cost1.E6_DocumentReceivedDate = ZDateTime.Empty;
				AssertHasError(cost1.E6_DocumentReceivedDateInfo, "Document Received Date must be entered when AP Invoice Number is entered.");
			}

			cost1.E6_DocumentReceivedDate = ZDate.Today.AddDays(1);
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_InvoiceNum = "123";
			cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost2.E6_InvoiceDate = ZDate.Today;
			cost2.E6_DocumentReceivedDate = ZDate.Today;
			AssertHasError(cost2.E6_DocumentReceivedDateInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the document received date does not match.");
		}

		public void TestInvoiceDateValidation()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_InvoiceDate = ZDate.Today;
			AssertHasError(cost1.E6_InvoiceDateInfo, "An AP Invoice Number must be entered when Invoice Date is entered");
			cost1.E6_InvoiceNum = "1234";
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_InvoiceDate = ZDate.Empty;
			AssertHasError(cost1.E6_InvoiceDateInfo, "Invoice date must be entered when AP invoice number is entered");
			cost1.E6_InvoiceDate = ZDate.Today;
			cost1.Validation.ValidateE6_InvoiceDate();
			AssertNoErrors(cost1.E6_InvoiceDateInfo);
			cost1.E6_InvoiceDate = ZDate.Today.AddDays(1);
			cost1.Validation.ValidateE6_InvoiceDate();
			AssertHasError(cost1.E6_InvoiceDateInfo, "Invoice date cannot be in the future.\r\nThis is determined by registry: Accounting -> Payable Defaults -> Default Settings -> Allow Forward Dating of AP Invoice Date");
			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_InvoiceNum = "1234";
			cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost2.E6_InvoiceDate = ZDate.Today;
			AssertHasError(cost2.E6_InvoiceDateInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the invoice date does not match.");
		}

		public void TestPaymentDateValidation()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_PaymentDate = ZDate.Today;
			AssertHasError(cost1.E6_PaymentDateInfo, "An AP Invoice Number must be entered when Payment Date is entered");
			cost1.E6_InvoiceNum = "1234";
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_PaymentDate = ZDate.Empty;
			AssertPaymentDateIsEnetered(cost1.E6_PaymentDateInfo);
			cost1.E6_PaymentDate = ZDate.Today;
			cost1.Validation.ValidateE6_PaymentDate();
			AssertNoErrors(cost1.E6_PaymentDateInfo);
			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_InvoiceNum = "1234";
			cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost2.E6_PaymentDate = ZDate.Today.AddDays(1);
			AssertHasError(cost2.E6_PaymentDateInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the invoice due date does not match.");
			cost1.E6_InvoiceDate = ZDateTime.Now;
			cost1.E6_PaymentDate = ZDateTime.Now.AddDays(-1);
			Assert(cost1.E6_PaymentDateInfo.HasError("Due date should be after or equal to Invoice Date"));
			cost1.E6_PaymentDate = ZDateTime.Now.AddDays(3);
			Assert(!cost1.E6_PaymentDateInfo.HasError("Due date should be after or equal to Invoice Date"));
		}

		public void TestSupplierCostReferenceValidation()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_InvoiceNum = "1234";
			cost1.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_InvoiceNum = "1234";
			cost2.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
			cost2.E6_CostReference = "ABC";
			AssertHasError(cost2.E6_CostReferenceInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the Supplier Cost Reference does not match.");
			cost2.E6_InvoiceNum = "4321";
			AssertNoError("Supplier Reference should no longer have an error because the invoice number is different.", cost2.E6_CostReferenceInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the Supplier Cost Reference does not match.");
			cost2.E6_InvoiceNum = "1234";
			AssertHasError(cost2.E6_CostReferenceInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the Supplier Cost Reference does not match.");
			cost2.E6_OH_Creditor = ObjectCreator.Creditor2.PK;
			AssertNoError("Supplier Reference should no longer have an error because the creditor is different.", cost2.E6_CostReferenceInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the Supplier Cost Reference does not match.");
			cost2.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
			AssertHasError(cost2.E6_CostReferenceInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the Supplier Cost Reference does not match.");
		}

		public override void TestValidateE6_IsForCollectInvoice()
		{
			AssertValidateE6_IsForCollectInvoice_ConsolAgentAndAPNettingGroup(delegate
			{
				Consol.JK_RL_NKLoadPort = "USLAX";
				Consol.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
				Assert("Should be import consol", Consol.IsImport());
			},
			value => Consol.SetDefaultSendingForwarderAddress(value),
			value => Consol.SetDefaultReceivingForwarderAddress(value),
			"Sending");

			AssertValidateE6_IsForCollectInvoice_ConsolAgentAndAPNettingGroup(delegate
			{
				Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
				Consol.JK_RL_NKDischargePort = "USLAX";
				Assert("Should be export consol", Consol.IsExport());
			},
			value => Consol.SetDefaultReceivingForwarderAddress(value),
			value => Consol.SetDefaultSendingForwarderAddress(value),
			"Receiving");

			AssertValidateE6_IsForCollectInvoice_ConsolAgentAndAPNettingGroup(delegate
			{
				var countryCN = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China));
				FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new Guid[] { countryCN.PK.ToGuid() });
				Consol.JK_RL_NKLoadPort = "CNAAT";
				Consol.JK_RL_NKDischargePort = "KRSEL";
				Assert("Should be export consol", Consol.IsExport());
			},
			value => Consol.SetDefaultReceivingForwarderAddress(value),
			value => Consol.SetDefaultSendingForwarderAddress(value),
			"Receiving");

			var cost1 = GetConsolCost();
			Consol.SetDefaultSendingForwarderAddress(ObjectCreator.AALSHI);
			cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			var cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = ObjectCreator.CC2.PK;

			SetCostsWithSameValues(cost1, cost2);
			cost1.E6_IsForCollectInvoice = true;
			cost2.E6_IsForCollectInvoice = false;

			cost2.Validation.ValidateE6_IsForCollectInvoice();
			AssertHasError(cost2.E6_IsForCollectInvoiceInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the 'Include on Collect Invoice' does not match.");
			cost2.E6_IsForCollectInvoice = true;
			AssertNoErrors(cost2.E6_IsForCollectInvoiceInfo);

			void SetCostsWithSameValues(params JobConsolCost[] costs)
			{
				foreach (var cost in costs)
				{
					cost.E6_OSCostAmount = 100m;
					cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
					cost.E6_InvoiceNum = "ABC123";
					cost.E6_InvoiceDate = ZDateTime.Now;
				}
			}
		}

		void AssertValidateE6_IsForCollectInvoice_ConsolAgentAndAPNettingGroup(AnonymousMethod setDirection, SetProperty setForwarder, SetProperty setOppositeForwarder, string agentKind)
		{
			var cost = GetConsolCost();
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			setDirection();
			string expectedError = string.Format(@"You can only mark this cost as 'Include on Collect Invoice' when the creditor is the '{0} Agent' for this consol.
If the {0} Agent has an AR or AP Netting Group, you must enter the AR or AP Netting group as the creditor.", agentKind);
			setForwarder(ZGuid.Empty);
			setOppositeForwarder(ZGuid.Empty);
			cost.E6_IsForCollectInvoice = true;
			AssertHasError(cost.E6_IsForCollectInvoiceInfo, expectedError);
			cost.E6_IsForCollectInvoice = false;
			AssertNoErrors(cost.E6_IsForCollectInvoiceInfo);
			setOppositeForwarder(ObjectCreator.AALSHI.PK);
			cost.E6_IsForCollectInvoice = true;
			AssertHasError(cost.E6_IsForCollectInvoiceInfo, expectedError);
			cost.E6_IsForCollectInvoice = false;
			AssertNoErrors(cost.E6_IsForCollectInvoiceInfo);
			setForwarder(ObjectCreator.AALSHI.PK);
			cost.E6_IsForCollectInvoice = true;
			AssertNoErrors(cost.E6_IsForCollectInvoiceInfo);
			cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
			cost.E6_IsForCollectInvoice = true;
			AssertHasError(cost.E6_IsForCollectInvoiceInfo, expectedError);
			ObjectCreator.AALSHI.SetRelatedParty(ObjectCreator.ZECTRA, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			ObjectCreator.AALSHI.SetRelatedParty(ObjectCreator.ABIGAS, RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_IsForCollectInvoice = true;
			AssertHasError(cost.E6_IsForCollectInvoiceInfo, string.Format(@"You can only mark this cost as 'Include on Collect Invoice' when the creditor is the '{0} Agent' for this consol.
If the {0} Agent has an AR or AP Netting Group, you must enter the AR or AP Netting group as the creditor.
AALSHI has a related AP Netting Group of ZECTRA.
AALSHI has a related AR Netting Group of ABIGAS.", agentKind));
			cost.E6_OH_Creditor = ObjectCreator.ZECTRA.PK;
			cost.E6_IsForCollectInvoice = true;
			AssertNoErrors(cost.E6_IsForCollectInvoiceInfo);
			cost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
			cost.E6_IsForCollectInvoice = true;
			AssertNoErrors(cost.E6_IsForCollectInvoiceInfo);
			cost.E6_IsForCollectInvoice = false;
			AssertNoErrors(cost.E6_IsForCollectInvoiceInfo);
			ObjectCreator.AALSHI.SetRelatedParty(ObjectCreator.AALSHI, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			ObjectCreator.AALSHI.SetRelatedParty(ObjectCreator.AALSHI, RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
		}

		public void TestValidateE6_IsForCollectInvoice_SellApportionment()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gatewayShipment1 = TestObjectCreator.CreateShipment("S001", gatewayConsol);
			var gatewayShipment2 = TestObjectCreator.CreateShipment("S002", gatewayConsol);
			using (var job = TestObjectCreator.CreateJob(gatewayConsol))
			using (TestObjectCreator.CreateJob(gatewayShipment1))
			using (TestObjectCreator.CreateJob(gatewayShipment2))
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_GE = TestObjectCreator.GEADepartment.PK;
				charge.JR_LocalSellAmt = 100;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

				GatewaySellToCostSynchroniser.Synchronise(job);
				Assert("Precondition: charge was synchronised successfully", charge.JR_E6_GatewaySellHeader.IsValid);
				var cost = Factory.Load<JobConsolCost>(charge.JR_E6_GatewaySellHeader);
				cost.E6_IsForCollectInvoice = true;
				Assert("Precondition: Local port is local to make Receiving Agent Invalid for IsForCollectInvoice", gatewayConsol.IsLoadPortLocal());
				AssertNotEquals("", gatewayConsol.SendingForwarderPK, cost.E6_OH_Creditor);
				cost.Validation.ValidateE6_IsForCollectInvoice();
				AssertNoErrors("IsForCollectInvoice should not run validation for a gateway apportionment", cost.E6_IsForCollectInvoiceInfo);
			}
		}

		delegate void SetProperty(ZGuid value);
		public void TestPaymentTypeValidation()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_InvoiceNum = "1234";
			cost1.E6_InvoiceDate = ZDate.Today;
			cost1.E6_PaymentDate = ZDate.Today;
			cost1.E6_OH_Creditor = ZGuid.Empty;
			cost1.E6_PaymentType = "TST";
			AssertHasError(cost1.E6_PaymentTypeInfo, "Enter a valid " + cost1.E6_PaymentTypeInfo.Description + ".");

			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "KRSEL";
			Consol.SetDefaultReceivingForwarderAddress(ObjectCreator.AALSHI);
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertHasError(cost1.E6_PaymentTypeInfo, "This is an agent related charge and cannot have payment details");

			Consol.JK_RL_NKLoadPort = ZString.Empty;
			Consol.SetDefaultReceivingForwarderAddress(ZGuid.Empty);
			cost1.Validation.ValidateE6_PaymentType();
			AssertNoErrors(cost1.E6_PaymentTypeInfo);

			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_InvoiceNum = "1234";
			cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertHasError(cost2.E6_PaymentTypeInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the payment type does not match.");
			cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost2.Validation.ValidateE6_PaymentType();
			AssertNoErrors(cost2.E6_PaymentTypeInfo);
		}

		public void TestPaymentTypeSecurity()
		{
			JobConsolCost cost = GetConsolCost();
			cost.E6_InvoiceNum = "1234";
			cost.E6_InvoiceDate = ZDate.Today;
			cost.E6_PaymentDate = ZDate.Today;
			cost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;

			Env.Security.APPaymentProcessingNewCheque.IsAllowed = Env.Security.APPaymentProcessingNewCash.IsAllowed = Env.Security.APPaymentProcessingNewCreditCard.IsAllowed = Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed = Env.Security.APPaymentProcessingNewEFT.IsAllowed = false;
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = Env.Security.APPaymentProcessingNewCash.IsAllowed = Env.Security.APPaymentProcessingNewCreditCard.IsAllowed = Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed = Env.Security.APPaymentProcessingNewEFT.IsAllowed = Env.Security.APPaymentProcessingNewSFT.IsAllowed = Env.Security.APPaymentProcessingNewCRQ.IsAllowed = false;

			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			cost.E6_PaymentType = ReceiptTypes.Cheque;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			cost.E6_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			cost.E6_PaymentType = ReceiptTypes.Cheque;
			AssertHasError(cost.E6_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCash.IsAllowed = true;
			cost.E6_PaymentType = ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			cost.E6_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCash.IsAllowed = false;
			cost.E6_PaymentType = ReceiptTypes.Cash;
			AssertHasError(cost.E6_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = true;
			cost.E6_PaymentType = ReceiptTypes.CreditCard;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			cost.E6_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = false;
			cost.E6_PaymentType = ReceiptTypes.CreditCard;
			AssertHasError(cost.E6_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = true;
			cost.E6_PaymentType = ReceiptTypes.DirectDebit;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			cost.E6_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = false;
			cost.E6_PaymentType = ReceiptTypes.DirectDebit;
			AssertHasError(cost.E6_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentEFT.IsAllowed = true;
			cost.E6_PaymentType = ReceiptTypes.EFT;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			cost.E6_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentEFT.IsAllowed = false;
			cost.E6_PaymentType = ReceiptTypes.EFT;
			AssertHasError(cost.E6_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentSFT.IsAllowed = true;
			cost.E6_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			cost.E6_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentSFT.IsAllowed = false;
			cost.E6_PaymentType = ReceiptTypes.ScheduledEFT;
			AssertHasError(cost.E6_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCRQ.IsAllowed = true;
			cost.E6_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			cost.E6_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !cost.E6_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCRQ.IsAllowed = false;
			cost.E6_PaymentType = ReceiptTypes.CollectionRequest;
			AssertHasError(cost.E6_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");
		}

		public void TestPaymentTypeRequiresInvoiceNumberOrSelfBillingCreditor()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost1.E6_OSCostAmount = 100m;
			cost1.E6_PaymentType = ReceiptTypes.Cheque;
			cost1.Validation.ValidateE6_PaymentType();
			AssertHasErrors(cost1.E6_PaymentTypeInfo);
			cost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			cost1.Validation.ValidateE6_PaymentType();
			AssertHasErrors(cost1.E6_PaymentTypeInfo);
			cost1.Creditor.CompanyData.OB_APCostsSelfBilled = true;
			cost1.Validation.ValidateE6_PaymentType();
			AssertNoErrors(cost1.E6_PaymentTypeInfo);
			cost1.Creditor.CompanyData.OB_APCostsSelfBilled = false;
			cost1.Validation.ValidateE6_PaymentType();
			AssertHasErrors(cost1.E6_PaymentTypeInfo);
			cost1.E6_InvoiceNum = "ABC123";
			cost1.Validation.ValidateE6_PaymentType();
			AssertNoErrors(cost1.E6_PaymentTypeInfo);
		}

		public void TestBankAccountIsValidZGuid()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_AB_BankAccount = ZGuid.NewZGuid();
			cost1.E6_InvoiceNum = "1234";
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_InvoiceDate = ZDate.Today;
			cost1.E6_PaymentDate = ZDate.Today;
			cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertHasError(cost1.E6_AB_BankAccountInfo, E6_AB_BankAccountInvalidCompanyError);
			cost1.E6_AB_BankAccount = ZGuid.Invalid;
			AssertHasError(cost1.E6_AB_BankAccountInfo, "Enter a valid " + cost1.E6_AB_BankAccountInfo.Description + ".");
			cost1.E6_AB_BankAccount = ZGuid.Missing;
			AssertHasError(cost1.E6_AB_BankAccountInfo, E6_AB_BankAccountInvalidCompanyError);
			cost1.E6_AB_BankAccount = ObjectCreator.AUDBankAccount.PK;
			AssertNoErrors(cost1.E6_AB_BankAccountInfo);
		}

		const string E6_AB_BankAccountInvalidCompanyError = "Please select a bank account belonging to the current company.";
		const string E6_AB_BankAccountWrongCurrencyError = "Bank Account currency is incorrect. Choose AUD currency Bank Account.";
		public void TestBankAccountValidation()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_InvoiceNum = "1234";
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_InvoiceDate = ZDate.Today;
			cost1.E6_PaymentDate = ZDate.Today;
			cost1.E6_AB_BankAccount = ObjectCreator.AUDBankAccount.PK;
			AssertPaymentTypeIsEnetered(cost1.E6_AB_BankAccountInfo);
			cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost1.E6_AB_BankAccount = ZGuid.Empty;
			AssertHasError(cost1.E6_AB_BankAccountInfo, "A valid bank account is required for selected payment type");
			cost1.E6_OH_Creditor = ZGuid.Empty;
			cost1.E6_AB_BankAccount = ObjectCreator.USDBankAccount.PK;
			AssertHasError(cost1.E6_AB_BankAccountInfo, E6_AB_BankAccountWrongCurrencyError);
			cost1.E6_AB_BankAccount = ObjectCreator.AUDBankAccount.PK;
			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "KRSEL";
			Consol.SetDefaultReceivingForwarderAddress(ObjectCreator.AALSHI);
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_InvoiceNum = "1234";
			cost1.Validation.ValidateE6_AB_BankAccount();
			AssertHasError(cost1.E6_AB_BankAccountInfo, "This is an agent related charge and cannot have payment details");
			Consol.JK_RL_NKLoadPort = ZString.Empty;
			Consol.SetDefaultReceivingForwarderAddress(ZGuid.Empty);
			cost1.Validation.ValidateE6_AB_BankAccount();
			AssertNoErrors(cost1.E6_AB_BankAccountInfo);
			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_InvoiceNum = "1234";
			cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost2.E6_AB_BankAccount = ObjectCreator.AUDBankAccount2.PK;
			Assert(cost2.E6_AB_BankAccount.IsValid);
			AssertHasError(cost2.E6_AB_BankAccountInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the bank account does not match.");
			cost2.E6_RX_NKCurrency = ObjectCreator.USDBankAccount.AB_RX_NKAccountCurrency;
			cost2.E6_AB_BankAccount = ObjectCreator.USDBankAccount.PK;
			AssertHasError(cost2.E6_AB_BankAccountInfo, E6_AB_BankAccountWrongCurrencyError);
			cost2.E6_AB_BankAccount = ObjectCreator.AUDChequeBook.AK_AB;
			AssertNoErrors(cost2.E6_AB_BankAccountInfo);
		}

		public void TestChequeBookValidation()
		{
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_InvoiceNum = "1234";
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_InvoiceDate = ZDate.Today;
			cost1.E6_PaymentDate = ZDate.Today;
			cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertHasError(cost1.E6_AK_ChequeBookInfo, "A valid checkbook is required for selected payment type");
			cost1.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
			AssertHasError(cost1.E6_AK_ChequeBookInfo, "A valid bank account is required when entering a check book");
			cost1.E6_AB_BankAccount = ObjectCreator.AUDBankAccount.PK;
			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "KRSEL";
			Consol.SetDefaultReceivingForwarderAddress(ObjectCreator.AALSHI);
			cost1.E6_OH_Creditor = ZGuid.Empty;
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_InvoiceNum = "1234";
			cost1.Validation.ValidateE6_AK_ChequeBook();
			AssertHasError(cost1.E6_AK_ChequeBookInfo, "This is an agent related charge and cannot have payment details");
			Consol.JK_RL_NKLoadPort = ZString.Empty;
			Consol.SetDefaultReceivingForwarderAddress(ZGuid.Empty);
			cost1.Validation.ValidateE6_AK_ChequeBook();
			AssertNoErrors(cost1.E6_AK_ChequeBookInfo);
			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_InvoiceNum = "1234";
			cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost2.E6_InvoiceDate = ZDate.Today;
			cost2.E6_AK_ChequeBook = ObjectCreator.USDChequeBook.PK;
			AssertHasError(cost2.E6_AK_ChequeBookInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the check book does not match.");
			cost2.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
			AssertNoErrors(cost2.E6_PaymentTypeInfo);
		}

		public void TestChequeOrReferenceValidation()
		{
			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testChequeBook.AK_StartNo = 1000;
			testChequeBook.AK_LastNo = 2000;
			JobConsolCost cost1 = GetConsolCost();
			cost1.E6_AC_ChargeCode = testChargeCode.PK;
			cost1.E6_ChequeOrReference = "1001";
			AssertPaymentTypeIsEnetered(cost1.E6_ChequeOrReferenceInfo);
			cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			cost1.E6_InvoiceNum = "1234";
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_InvoiceDate = ZDate.Today;
			cost1.E6_PaymentDate = ZDate.Today;
			AssertHasError(cost1.E6_ChequeOrReferenceInfo, "A valid check or reference number is required for selected payment type");
			cost1.E6_ChequeOrReference = "1001";
			AssertHasError(cost1.E6_ChequeOrReferenceInfo, "A valid bank account is required when entering a check or reference number");
			cost1.E6_AB_BankAccount = testChequeBook.AK_AB;
			cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertHasError(cost1.E6_ChequeOrReferenceInfo, "A valid check book is required when entering a check or reference number");
			cost1.E6_AK_ChequeBook = testChequeBook.PK;
			cost1.E6_ChequeOrReference = "A999";
			AssertHasError(cost1.E6_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");
			cost1.E6_ChequeOrReference = ((decimal)int.MaxValue + 1).ToString();
			AssertHasError(cost1.E6_ChequeOrReferenceInfo, GetExpectedErrorMessage(cost1.E6_ChequeOrReference));
			cost1.E6_ChequeOrReference = "999";
			AssertHasError(cost1.E6_ChequeOrReferenceInfo, GetExpectedErrorMessage(cost1.E6_ChequeOrReference));
			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "KRSEL";
			Consol.SetDefaultReceivingForwarderAddress(ObjectCreator.AALSHI);
			cost1.E6_OH_Creditor = ZGuid.Empty;
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_ChequeOrReference = "1001";
			AssertHasError(cost1.E6_ChequeOrReferenceInfo, "This is an agent related charge and cannot have payment details");
			Consol.JK_RL_NKLoadPort = ZString.Empty;
			Consol.SetDefaultReceivingForwarderAddress(ZGuid.Empty);
			cost1.Validation.ValidateE6_ChequeOrReference();
			AssertNoErrors(cost1.E6_ChequeOrReferenceInfo);
			cost1.E6_ChequeOrReference = "00001001";
			AssertHasError(cost1.E6_ChequeOrReferenceInfo, "The check number exceeds the number of digits (6 digits) set for the current bank account");
			cost1.E6_ChequeOrReference = "1001";
			JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
			cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost2.E6_AC_ChargeCode = testChargeCode.PK;
			cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost2.E6_InvoiceDate = ZDate.Today;
			cost2.E6_AB_BankAccount = testChequeBook.AK_AB;
			cost2.E6_AK_ChequeBook = testChequeBook.PK;
			cost2.E6_InvoiceNum = "4321";
			cost2.E6_ChequeOrReference = "1001";
			AssertHasError(cost2.E6_ChequeOrReferenceInfo, "This check number is used in another Cost, Charge, Payment or Hot Check");
			cost2.E6_InvoiceNum = "1234";
			cost2.E6_ChequeOrReference = "1002";
			AssertHasError(cost2.E6_ChequeOrReferenceInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the payment reference does not match.");
			cost2.E6_ChequeOrReference = "1001";
			AssertNoErrors(cost2.E6_ChequeOrReferenceInfo);
			Factory.Save();
			JobConsolCost cost3 = GetConsolCost();
			cost3.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost3.E6_AC_ChargeCode = testChargeCode.PK;
			cost3.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost3.E6_InvoiceDate = ZDate.Today;
			cost3.E6_AB_BankAccount = testChequeBook.AK_AB;
			cost3.E6_AK_ChequeBook = testChequeBook.PK;
			cost3.E6_InvoiceNum = "1234";
			cost3.E6_ChequeOrReference = "1001";
			AssertHasError(cost3.E6_ChequeOrReferenceInfo, "This check number is used in another Cost, Charge, Payment or Hot Check");
			cost3.E6_ChequeOrReference = "1003";
			AssertNoErrors(cost3.E6_ChequeOrReferenceInfo);
		}

		string GetExpectedErrorMessage(string chequeNum)
		{
			return string.Format(@"Check number {0} is not contained in the selected check book.
The check number must be between 001000 and 002000.", chequeNum);
		}

		void AssertPaymentDateIsEnetered(ZPropertyInfo info)
		{
			AssertHasError(info, "Invoice due date must be entered when AP invoice number is entered");
		}

		void AssertPaymentTypeIsEnetered(ZPropertyInfo info)
		{
			AssertHasError(info, "A valid Payment Type must be entered");
		}

		public void TestCheckE6_IsTaxAmountOverridden()
		{
			var errorMsg = @"You do not have appropriate security rights to tick this checkbox.
Please contact your system administrator for the following security right: Operate > Forwarding > Consolidations > Costing/Invoicing > Allow Tick Override Tax Amount Checkbox";

			var consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0001");
			var shipment1 = ObjectCreator.CreateShipment("S0001", consol);
			var job = ObjectCreator.CreateJob(shipment1, false);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.AALSHI);

			Env.Security.MaintainConsolJobInvoicingAllowTickOverrideTaxAmountCheckbox.IsAllowed = true;
			consolCost.E6_IsTaxAmountOverridden = true;
			Factory.Save();

			AssertNoError(consolCost.E6_IsTaxAmountOverriddenInfo, errorMsg);

			Env.Security.MaintainConsolJobInvoicingAllowTickOverrideTaxAmountCheckbox.IsAllowed = false;
			var consolCost1 = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.AALSHI);
			consolCost1.E6_IsTaxAmountOverridden = true;
			Factory.Save();

			AssertHasError(consolCost1.E6_IsTaxAmountOverriddenInfo, errorMsg);

			var validation = consolCost.Validation;
			validation.ValidateE6_IsTaxAmountOverridden();
			AssertNoError(consolCost.E6_IsTaxAmountOverriddenInfo, errorMsg);
		}

		public void TestCheckE6_GS_NKConsolCostOwner()
		{
			var errorMsg = @"You do not have appropriate security rights to modify this field.
Please contact your system administrator for the following security right: Operate > Forwarding > Consolidations > Costing/Invoicing > Allow Override of Consol Cost Owner";

			var consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0001");
			var shipment1 = ObjectCreator.CreateShipment("S0001", consol);
			var job1 = ObjectCreator.CreateJob(shipment1, false);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.AALSHI);

			consolCost.E6_GS_NKConsolCostOwner = GlbStaff.CurrentUser.GS_Code;
			AssertNoError(consolCost.E6_GS_NKConsolCostOwnerInfo, errorMsg);

			consolCost.E6_GS_NKConsolCostOwner = "AAA";
			AssertNoError(consolCost.E6_GS_NKConsolCostOwnerInfo, errorMsg);

			Env.Security.MaintainConsolJobInvoicingAllowOverrideConsolCostOwner.IsAllowed = false;
			consolCost.E6_GS_NKConsolCostOwner = "BBB";
			AssertHasError(consolCost.E6_GS_NKConsolCostOwnerInfo, errorMsg);

			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_OSCostAmount = 100M;
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consolCost.E6_InvoiceDate = ZDateTime.Empty;
			consolCost.E6_GS_NKConsolCostOwner = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			consolCost.E6_GS_NKConsolCostOwner = GlbStaff.CurrentUser.GS_Code;
			AssertNoError(consolCost.E6_GS_NKConsolCostOwnerInfo, errorMsg);

			consolCost.E6_GS_NKConsolCostOwner = "CCC";
			AssertHasError(consolCost.E6_GS_NKConsolCostOwnerInfo, errorMsg);
		}

		public void TestE6_ChequeOrReferenceInfoValidationIsDisabledForAutoAllocationMode()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook(testBank);
			JobConsolCost cost = GetConsolCost();
			cost.E6_InvoiceNum = "1234";
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceDate = ZDate.Today;
			cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost.E6_AB_BankAccount = testChequeBook.AK_AB;
			cost.E6_AK_ChequeBook = testChequeBook.PK;
			cost.E6_ChequeOrReference = ZString.Empty;
			AssertHasErrors("Cheque Number should have an error", cost.E6_ChequeOrReferenceInfo);
			cost.E6_AB_BankAccount = autoPrintChequeBook.AK_AB;
			cost.E6_AK_ChequeBook = autoPrintChequeBook.PK;
			cost.E6_ChequeOrReference = ZString.Empty;
			AssertNoErrors("Should not have any errors as now in autoallocation mode", cost.E6_ChequeOrReferenceInfo);
		}

		public void TestE6_AK_ChequeBookHasAutoAllocationValidation()
		{
			AccChequeBook newChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			newChequeBook.AK_AB = testBank.PK;
			JobConsolCost cost = GetConsolCost();
			cost.E6_InvoiceNum = "1234";
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceDate = ZDate.Today;
			cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost.E6_AB_BankAccount = testBank.PK;
			cost.E6_AK_ChequeBook = newChequeBook.PK;
			Assert("Should be no errors so far", !cost.E6_AK_ChequeBookInfo.HasErrors());
			newChequeBook.AK_IsActive = ZBool.False;
			cost.E6_AK_ChequeBook = ZGuid.Empty;
			cost.E6_AK_ChequeBook = newChequeBook.PK;
			AssertHasError(cost.E6_AK_ChequeBookInfo, "This Check Book is inactive - it may not be used.");
		}

		public void TestValidateE6_InvoiceNum_InvoiceAmountSecurityWhenNoConfigurationExists()
		{
			Env.Security.APUnapprovedInvoices.IsAllowed = false;
			Consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			Consol.Shipments.AddNew();
			Consol.Shipments.AddNew();
			Apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost cost = Apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 251m;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_ApportionmentMethod = "MAN";
			cost.ApportionmentCharges[0].JR_OSCostAmt = 100.00m;
			cost.ApportionmentCharges[1].JR_OSCostAmt = 100.00m;
			cost.E6_InvoiceNum = "ABC123";
			AssertNoWarnings("Invoice amount is less than Approval registry level", cost.E6_InvoiceNumInfo);
		}

		public void TestValidateE6_InvoiceNum_InvoiceAmountSecurity()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SetUpRegistryForTest();
			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
			try
			{
				Consol = Factory.New<ForwardingConsol>();
				Factory.Save();
				Consol.Shipments.AddNew();
				Consol.Shipments.AddNew();
				Apps = new ApportionmentListing(Factory, Consol);
				JobConsolCost cost = Apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 251m;
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost.E6_ApportionmentMethod = "MAN";
				cost.ApportionmentCharges[0].JR_OSCostAmt = 100.00m;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 100.00m;
				cost.E6_InvoiceNum = "ABC123";
				AssertNoWarnings("Invoice amount is less than Approval registry level", cost.E6_InvoiceNumInfo);
				cost.E6_ApportionmentMethod = "SHP";
				cost.Validation.ValidateAll();
				AssertHasWarnings("Invoice amount is more than Approval registry level", cost.E6_InvoiceNumInfo);
			}
			finally
			{
				ResetRegistryForTest();
			}
		}

		public void TestValidateE6_InvoiceNum_IncludeOtherConsolCosts()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SetUpRegistryForTest();
			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
			try
			{
				Consol = Factory.New<ForwardingConsol>();
				Factory.Save();
				Consol.Shipments.AddNew();
				Apps = new ApportionmentListing(Factory, Consol);
				JobConsolCost cost1 = Apps.CostsCollection.TryAddNew();
				cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				cost1.E6_OSCostAmount = 200m;
				cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost1.E6_ApportionmentMethod = "CHG";
				cost1.E6_InvoiceNum = "ABC123";
				AssertNoWarnings("Invoice amount is less than Approval registry level", cost1.E6_InvoiceNumInfo);
				JobConsolCost cost2 = Apps.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				cost2.E6_OSCostAmount = 51m;
				cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost2.E6_ApportionmentMethod = "CHG";
				cost2.E6_InvoiceNum = "XYZ123";
				AssertNoWarnings("Invoice amount is less than Approval registry level", cost2.E6_InvoiceNumInfo);
				cost2.E6_InvoiceNum = "ABC123";
				cost2.Validation.ValidateAll();
				AssertHasWarnings("Invoice amount is more than Approval registry level", cost2.E6_InvoiceNumInfo);
			}
			finally
			{
				ResetRegistryForTest();
			}
		}

		public void TestValidateE6_InvoiceNum_UnapprovedInvoiceWithPaymentError()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SetUpRegistryForTest();
			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
			try
			{
				Consol = Factory.New<ForwardingConsol>();
				Factory.Save();
				Consol.Shipments.AddNew();
				Apps = new ApportionmentListing(Factory, Consol);
				JobConsolCost cost = Apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 251m;
				cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost.E6_ApportionmentMethod = "SHP";
				cost.E6_InvoiceNum = "ABC123";
				AssertHasWarningContaining(cost.E6_InvoiceNumInfo, "You do not have security rights");
				string expectedPartOfWarning = "Some of the consol costs that you are posting also contain Payment information.";
				cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.EFT;
				AssertHasWarningContaining(cost.E6_InvoiceNumInfo, expectedPartOfWarning);
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = true;
				cost.Validation.ValidateE6_InvoiceNum();
				AssertNoWarningContaining(cost.E6_InvoiceNumInfo, expectedPartOfWarning);
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				cost.Validation.ValidateE6_InvoiceNum();
				AssertHasWarningContaining(cost.E6_InvoiceNumInfo, expectedPartOfWarning);
				cost.E6_OSCostAmount = 227;
				Assert("Precondition: cost total amount must be not bigger than approved level", cost.E6_OSCostAmount + cost.E6_OSGSTAmount_Calc <= 250);
				cost.Validation.ValidateE6_InvoiceNum();
				AssertNoWarningContaining(cost.E6_InvoiceNumInfo, expectedPartOfWarning);
			}
			finally
			{
				ResetRegistryForTest();
			}
		}

		public void TestInvoiceNumberIsUsedOnAnotherConsolOrAnotherShipmentCharge()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol1.Shipments.AddNew();
			ApportionmentListing apps1 = new ApportionmentListing(Factory, consol1);
			JobConsolCost cost1 = apps1.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost1.E6_OSCostAmount = 1m;
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_ApportionmentMethod = "MAN";
			cost1.ApportionmentCharges[0].JR_OSCostAmt = 1m;
			cost1.E6_InvoiceNum = "ABC123";
			JobConsolCost cost2 = apps1.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = ObjectCreator.CC2.PK;
			cost2.E6_OSCostAmount = 1m;
			cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost2.E6_ApportionmentMethod = "MAN";
			cost2.ApportionmentCharges[0].JR_OSCostAmt = 1m;
			cost2.E6_InvoiceNum = "ABC123";
			AssertNoErrors("Shouldn't have any errors as both these costs belong to the same consol", cost1.E6_InvoiceNumInfo);
			AssertNoErrors("Shouldn't have any errors as both these costs belong to the same consol", cost2.E6_InvoiceNumInfo);
			Factory.Save();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol2.Shipments.AddNew();
			ApportionmentListing apps2 = new ApportionmentListing(Factory, consol2);
			try
			{
				JobConsolCost cost3 = apps2.CostsCollection.TryAddNew();
				cost3.E6_AC_ChargeCode = ObjectCreator.CC2.PK;
				cost3.E6_OSCostAmount = 1m;
				cost3.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost3.E6_ApportionmentMethod = "MAN";
				cost3.ApportionmentCharges[0].JR_OSCostAmt = 1m;
				cost3.E6_InvoiceNum = "ABC123";
				AssertHasError("Should have error as the invoice number used on costs belonging to two different consols", cost3.E6_InvoiceNumInfo, "This invoice number is used on another consol or another shipment charge");
				cost3.E6_InvoiceNum = "ABC124";
				AssertNoErrors("Shouldn't have any errors", cost3.E6_InvoiceNumInfo);
			}
			finally
			{
				apps2.ReleaseMutexes();
			}
		}

		public void TestInvoiceNumberIsUsedOnAnotherConsolOrAnotherShipmentCharge_DoesNotCacheResults()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol1.Shipments.AddNew();
			ApportionmentListing apps1 = new ApportionmentListing(Factory, consol1);
			JobConsolCost cost1 = apps1.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost1.E6_OSCostAmount = 1m;
			cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost1.E6_ApportionmentMethod = "MAN";
			cost1.ApportionmentCharges[0].JR_OSCostAmt = 1m;
			cost1.E6_InvoiceNum = "ABC123";
			Factory.Save();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol2.Shipments.AddNew();
			ApportionmentListing apps2 = new ApportionmentListing(Factory, consol2);
			try
			{
				JobConsolCost cost2 = apps2.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = ObjectCreator.CC2.PK;
				cost2.E6_OSCostAmount = 1m;
				cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				cost2.E6_ApportionmentMethod = "MAN";
				cost2.ApportionmentCharges[0].JR_OSCostAmt = 1m;
				cost2.E6_InvoiceNum = "ABC123";
				AssertHasError("Should have error as the invoice number used on costs belonging to two different consols", cost2.E6_InvoiceNumInfo, "This invoice number is used on another consol or another shipment charge");
				TestConnection.ExecuteNonQuery(String.Format(@"
UPDATE dbo.JobCharge
SET
	JR_APInvoiceNum = 'ABC124',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_APInvoiceNum = 'ABC123'"));
				cost2.E6_InvoiceNum = "ABC123";
				AssertNoErrors("Shouldn't have any errors as ABC123 is no longer in use", cost2.E6_InvoiceNumInfo);
			}
			finally
			{
				apps2.ReleaseMutexes();
			}
		}

		public void TestValidateE6_OH_CreditorWithRevalidation()
		{
			JobConsolCost cost = GetConsolCost();
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			AssertNoErrors("Precondition: ", cost.E6_InvoiceDateInfo);
			AssertNoErrors("Precondition: ", cost.E6_PaymentDateInfo);
			cost.E6_OH_Creditor = ZGuid.Empty;
			AssertNoErrors(cost.E6_InvoiceDateInfo);
			AssertNoErrors(cost.E6_PaymentDateInfo);
			cost.Validation.ValidateE6_InvoiceDate();
			cost.Validation.ValidateE6_PaymentDate();
			AssertHasErrors(cost.E6_InvoiceDateInfo);
			AssertHasErrors(cost.E6_PaymentDateInfo);
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			AssertNoErrors(cost.E6_InvoiceDateInfo);
			AssertNoErrors(cost.E6_PaymentDateInfo);
		}

		public void TestValidateE6_InvoiceNumWithRevalidation()
		{
			JobConsolCost cost = GetConsolCost();
			cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			AssertNoErrors("Precondition: ", cost.E6_InvoiceDateInfo);
			AssertNoErrors("Precondition: ", cost.E6_PaymentDateInfo);
			cost.E6_InvoiceNum = "";
			AssertHasErrors(cost.E6_InvoiceDateInfo);
			AssertHasErrors(cost.E6_PaymentDateInfo);
			cost.E6_InvoiceNum = "ABC123";
			AssertNoErrors(cost.E6_InvoiceDateInfo);
			AssertNoErrors(cost.E6_PaymentDateInfo);
		}

		public void TestCheck_Validation_E6_ExchangeRate_CurrencyAndRates()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_RX_NKCurrency = "AUD";
			Cost1.E6_ExchangeRate = 1.0m;
			Cost2.E6_RX_NKCurrency = "USD";
			Cost2.E6_ExchangeRate = 1.2m;
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			Cost2.E6_RX_NKCurrency = "USD";
			Cost2.E6_ExchangeRate = 1.0m;
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			Cost2.E6_RX_NKCurrency = "AUD";
			Cost2.E6_ExchangeRate = 1.2m;
			AssertHasErrors(Cost2.E6_ExchangeRateInfo);
			AssertHasError(Cost2.E6_ExchangeRateInfo, "This consol cost has an AP Invoice number and Currency the same as another cost but the exchange rate does not match.");
			ErrorReporter.Clear();
			Cost2.E6_RX_NKCurrency = "AUD";
			Cost2.E6_ExchangeRate = 1.0m;
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
		}

		public void TestCheck_Validation_E6_ExchangeRate_PaymentDetailsAreSame()
		{
			Consol = CreateSampleForwardingConsol();
			//Everything Same
			Cost1.E6_InvoiceNum = "TEST_INV #1";
			Cost2.E6_InvoiceNum = "TEST_INV #2";
			Cost2.E6_OH_Creditor = Cost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			Cost2.E6_RX_NKCurrency = Cost1.E6_RX_NKCurrency = "USD";
			Cost2.E6_PaymentType = Cost1.E6_PaymentType = ReceiptTypes.Cash;
			Cost2.E6_AB_BankAccount = Cost1.E6_AB_BankAccount = TestObjectCreator.USDBankAccount.PK;
			Cost2.E6_ChequeOrReference = Cost1.E6_ChequeOrReference = "PAYMNTREF";
			//Exchange Rate
			Cost1.E6_ExchangeRate = 0.85m;
			Cost2.E6_ExchangeRate = 0.95m;
			AssertHasErrors(Cost2.E6_ExchangeRateInfo);
			AssertHasError(Cost2.E6_ExchangeRateInfo, "This consol cost has same payment details as another cost but the exchange rate does not match.");
			Cost2.E6_ExchangeRate = 0.85m;
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			//Creditor Change
			Cost2.E6_OH_Creditor = TestObjectCreator.Creditor2.PK;
			Cost2.E6_ExchangeRate = 1.85m;
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			Cost2.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			AssertHasErrors(Cost2.E6_ExchangeRateInfo);
			AssertHasError(Cost2.E6_ExchangeRateInfo, "This consol cost has same payment details as another cost but the exchange rate does not match.");
			//Currency Change
			Cost2.E6_RX_NKCurrency = "INR";
			Cost2.E6_ExchangeRate = 10m;
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			Cost2.E6_RX_NKCurrency = "USD";
			Cost2.E6_ExchangeRate = 1.85m;
			AssertHasErrors(Cost2.E6_ExchangeRateInfo);
			AssertHasError(Cost2.E6_ExchangeRateInfo, "This consol cost has same payment details as another cost but the exchange rate does not match.");
			//Payment Type Change
			Cost2.E6_PaymentType = ReceiptTypes.CreditCard;
			Cost2.E6_ExchangeRate = 10m;
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			Cost2.E6_PaymentType = ReceiptTypes.Cash;
			Cost2.E6_ChequeOrReference = "PAYMNTREF";
			AssertHasErrors(Cost2.E6_ExchangeRateInfo);
			AssertHasError(Cost2.E6_ExchangeRateInfo, "This consol cost has same payment details as another cost but the exchange rate does not match.");
			//Bank Account Change
			Cost2.E6_AB_BankAccount = TestObjectCreator.USDBankAccount2.PK;
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			Cost2.E6_AB_BankAccount = TestObjectCreator.USDBankAccount.PK;
			AssertHasErrors(Cost2.E6_ExchangeRateInfo);
			AssertHasError(Cost2.E6_ExchangeRateInfo, "This consol cost has same payment details as another cost but the exchange rate does not match.");
			//ChequeOrReference Change
			Cost2.E6_ChequeOrReference = "999";
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			Cost2.E6_ChequeOrReference = "PAYMNTREF";
			AssertHasErrors(Cost2.E6_ExchangeRateInfo);
			AssertHasError(Cost2.E6_ExchangeRateInfo, "This consol cost has same payment details as another cost but the exchange rate does not match.");
		}

		public void TestCheck_Validation_E6_AC_ChargeCode()
		{
			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Consol = CreateSampleForwardingConsol();
			AssertEquals(TestObjectCreator.AALSHI.PK, Cost1.E6_OH_Creditor);
			AssertEquals(TestObjectCreator.AALSHI.PK, Cost2.E6_OH_Creditor);

			ObjectCreator.CC1.AC_IsGroupageCharge = true;
			ObjectCreator.CC2.AC_IsGroupageCharge = true;

			Cost1.E6_AC_ChargeCode = ObjectCreator.CC2.PK;
			Cost2.E6_AC_ChargeCode = ObjectCreator.CC2.PK;
			AssertNoWarnings(Cost2.E6_AC_ChargeCodeInfo);
			AssertHasErrors(Cost2.E6_AC_ChargeCodeInfo);
			AssertHasError(Cost2.E6_AC_ChargeCodeInfo, @"There is an unposted consol cost with the same charge code.
The same charge code on unposted lines are allowed only when
- The registry 'Accrual Reversal Behavior When Allocated To Creditor' is set to YES; and

Please ensure that the registry is configured accordingly or use another charge code / creditor.");

			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			Cost2.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			AssertHasWarnings(Cost2.E6_AC_ChargeCodeInfo);
			AssertHasWarning(Cost2.E6_AC_ChargeCodeInfo, "There are more than one unposted charges with the same charge code, you are advised to review them before posting.");
			AssertNoErrors(Cost2.E6_AC_ChargeCodeInfo);

			Cost2.E6_OH_Creditor = ZGuid.Empty;
			Cost1.E6_AC_ChargeCode = ObjectCreator.CC2.PK;
			Cost2.E6_AC_ChargeCode = ObjectCreator.CC2.PK;
			AssertHasWarnings(Cost2.E6_AC_ChargeCodeInfo);
			AssertHasWarning(Cost2.E6_AC_ChargeCodeInfo, "There are more than one unposted charges with the same charge code, you are advised to review them before posting.");
			AssertNoErrors(Cost2.E6_AC_ChargeCodeInfo);

			Cost1.E6_OH_Creditor = TestObjectCreator.ABIGAS.PK;
			Cost2.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			Cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			Cost2.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			AssertNoErrors(Cost2.E6_AC_ChargeCodeInfo);
			AssertHasWarnings(Cost2.E6_AC_ChargeCodeInfo);
			AssertHasWarning(Cost2.E6_AC_ChargeCodeInfo, "There are more than one unposted charges with the same charge code, you are advised to review them before posting.");

			var cost3 = AddMoreCostToSampleConsol(ObjectCreator.CC1, TestObjectCreator.ABIGAS);
			AssertNoErrors(cost3.E6_AC_ChargeCodeInfo);
			AssertHasWarnings(cost3.E6_AC_ChargeCodeInfo);
			AssertHasWarning(cost3.E6_AC_ChargeCodeInfo, "There are more than one unposted charges with the same charge code, you are advised to review them before posting.");

			var cost4 = AddMoreCostToSampleConsol(ObjectCreator.CC1, null);
			AssertNoErrors(cost4.E6_AC_ChargeCodeInfo);
			AssertHasWarnings(cost4.E6_AC_ChargeCodeInfo);
			AssertHasWarning(cost4.E6_AC_ChargeCodeInfo, "There are more than one unposted charges with the same charge code, you are advised to review them before posting.");
		}

		public void TestCheck_Validation_E6_RX_NKCurrency_CurrencyAndRates()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_RX_NKCurrency = "USD";
			Cost1.E6_ExchangeRate = 1.2m;
			Cost2.E6_ExchangeRate = 1.0m;
			Cost2.E6_RX_NKCurrency = "AUD";
			AssertNoErrors(Cost2.E6_RX_NKCurrencyInfo);
			Cost2.E6_ExchangeRate = 1.0m;
			Cost2.E6_RX_NKCurrency = "AUD";
			AssertNoErrors(Cost2.E6_RX_NKCurrencyInfo);
			const string mixedCurrencyWarning = @"This cost will be posted on a local currency Payables Invoice.
A mix of Cost Currencies have recorded for this Creditor and Invoice Number. Because of this, they will be posted as a local currency payables transaction.";
			Cost1.Validation.ValidateE6_RX_NKCurrency();
			Cost2.Validation.ValidateE6_RX_NKCurrency();
			AssertHasWarning(Cost1.E6_RX_NKCurrencyInfo, mixedCurrencyWarning);
			AssertHasWarning(Cost2.E6_RX_NKCurrencyInfo, mixedCurrencyWarning);
			Cost2.E6_RX_NKCurrency = "USD";
			Cost2.E6_ExchangeRate = 1.1m;
			AssertHasError(Cost2.E6_RX_NKCurrencyInfo, "This consol cost has an AP Invoice number and Currency the same as another cost but the exchange rate does not match.");
			Cost1.Validation.ValidateE6_RX_NKCurrency();
			Cost2.Validation.ValidateE6_RX_NKCurrency();
			AssertNoWarning(Cost1.E6_RX_NKCurrencyInfo, mixedCurrencyWarning);
			AssertNoWarning(Cost2.E6_RX_NKCurrencyInfo, mixedCurrencyWarning);
			Cost1.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Cost2.E6_ExchangeRate = 1.1m;
			Cost2.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertNoErrors(Cost2.E6_RX_NKCurrencyInfo);
		}

		public void TestCheckE6_InvoiceNum_And_RelationWith_E6_OH_CreditorValidation()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			Cost2.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
			AssertNoErrors(Cost1.E6_OH_CreditorInfo);
			AssertNoErrors(Cost2.E6_OH_CreditorInfo);
			Cost2.E6_InvoiceNum = "InvoiceTest1";
			Cost2.E6_OH_Creditor = ObjectCreator.Creditor2.PK;
			Cost2.E6_InvoiceNum = "InvoiceTest";
			AssertNoErrors("It is possible to have different creditors if the invoice numbers are the same", Cost2.E6_OH_CreditorInfo);
		}

		public void TestCheckE6_InvoiceNum_And_RelationWith_E6_PaymentTypeValidation()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Cost1.Validation.ValidateE6_PaymentType();
			AssertNoErrors(Cost1.E6_PaymentTypeInfo);
			AssertNoErrors(Cost2.E6_PaymentTypeInfo);
			Cost2.E6_InvoiceNum = "InvoiceTest1";
			Cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Cost2.E6_InvoiceNum = "InvoiceTest";
			AssertHasErrors(Cost2.E6_PaymentTypeInfo);
		}

		public void TestCheckE6_InvoiceNum_And_RelationWith_E6_AB_BankAccountValidation()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Cost1.E6_AB_BankAccount = ObjectCreator.USDBankAccount.PK;
			Cost2.E6_AB_BankAccount = ObjectCreator.USDBankAccount.PK;
			AssertNoErrors(Cost1.E6_AB_BankAccountInfo);
			AssertNoErrors(Cost2.E6_AB_BankAccountInfo);
			Cost2.E6_InvoiceNum = "InvoiceTest1";
			Cost2.E6_AB_BankAccount = ObjectCreator.USDBankAccount2.PK;
			Cost2.E6_InvoiceNum = "InvoiceTest";
			AssertHasErrors(Cost2.E6_AB_BankAccountInfo);
		}

		public void TestCheckE6_InvoiceNum_And_RelationWith_E6_AK_ChequeBookValidation()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Cost1.E6_AB_BankAccount = ObjectCreator.USDBankAccount.PK;
			Cost2.E6_AB_BankAccount = ObjectCreator.USDBankAccount.PK;
			Cost1.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
			Cost2.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
			AssertNoErrors(Cost1.E6_AK_ChequeBookInfo);
			AssertNoErrors(Cost2.E6_AK_ChequeBookInfo);
			Cost2.E6_InvoiceNum = "InvoiceTest1";
			Cost2.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook2.PK;
			Cost2.E6_InvoiceNum = "InvoiceTest";
			AssertHasErrors(Cost2.E6_AK_ChequeBookInfo);
		}

		public void TestCheckE6_InvoiceNum_And_RelationWith_E6_ChequeOrReferenceValidation()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Cost2.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Cost1.E6_AB_BankAccount = ObjectCreator.USDBankAccount.PK;
			Cost2.E6_AB_BankAccount = ObjectCreator.USDBankAccount.PK;
			Cost1.E6_ChequeOrReference = "123";
			Cost2.E6_ChequeOrReference = "123";
			Cost1.Validation.ValidateE6_ChequeOrReference();
			AssertNoErrors(Cost1.E6_ChequeOrReferenceInfo);
			AssertNoErrors(Cost2.E6_ChequeOrReferenceInfo);
			Cost2.E6_InvoiceNum = "InvoiceTest1";
			Cost2.E6_ChequeOrReference = "231";
			Cost2.E6_InvoiceNum = "InvoiceTest";
			AssertHasErrors(Cost2.E6_ChequeOrReferenceInfo);
		}

		public void TestCheckE6_InvoiceNum_And_RelationWith_E6_IsForCollectInvoiceValidation()
		{
			Consol = CreateSampleForwardingConsol();
			Consol.SetDefaultSendingForwarderAddress(ObjectCreator.AALSHI);
			Cost1.E6_IsForCollectInvoice = true;
			Cost2.E6_IsForCollectInvoice = true;
			Cost1.Validation.ValidateE6_IsForCollectInvoice();
			AssertNoErrors(Cost1.E6_IsForCollectInvoiceInfo);
			AssertNoErrors(Cost2.E6_IsForCollectInvoiceInfo);
			Cost2.E6_InvoiceNum = "InvoiceTest1";
			Cost2.E6_IsForCollectInvoice = false;
			Cost2.E6_InvoiceNum = "InvoiceTest";
			AssertHasErrors(Cost2.E6_IsForCollectInvoiceInfo);
		}

		public void TestCheckE6_InvoiceNum_And_RelationWith_E6_RX_NKCurrencyValidation()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_RX_NKCurrency = "USD";
			Cost1.E6_ExchangeRate = 1.2;
			Cost2.E6_RX_NKCurrency = "USD";
			Cost2.E6_ExchangeRate = 1.2;
			Cost1.Validation.ValidateE6_RX_NKCurrency();
			Cost2.Validation.ValidateE6_RX_NKCurrency();
			AssertNoErrors(Cost1.E6_RX_NKCurrencyInfo);
			AssertNoErrors(Cost2.E6_RX_NKCurrencyInfo);
			Cost2.E6_InvoiceNum = "InvoiceTest1";
			Cost2.E6_RX_NKCurrency = "USD";
			Cost2.E6_ExchangeRate = 1.0;
			Cost2.E6_InvoiceNum = "InvoiceTest";
			AssertHasErrors(Cost2.E6_RX_NKCurrencyInfo);
		}

		public void TestCheckE6_InvoiceNum_And_RelationWith_E6_ExchangeRateValidation()
		{
			Consol = CreateSampleForwardingConsol();
			Cost1.E6_ExchangeRate = 1.2;
			Cost2.E6_ExchangeRate = 1.2;
			Cost1.Validation.ValidateE6_ExchangeRate();
			AssertNoErrors(Cost1.E6_ExchangeRateInfo);
			AssertNoErrors(Cost2.E6_ExchangeRateInfo);
			Cost2.E6_InvoiceNum = "InvoiceTest1";
			Cost2.E6_ExchangeRate = 1.3;
			Cost2.E6_InvoiceNum = "InvoiceTest";
			AssertHasErrors(Cost2.E6_ExchangeRateInfo);
		}

		public void TestCheckE6_InvoiceNum_ValidationFor_BranchLevelPostingEnabled()
		{
			var expectedError = @"Please review the Consol Cost allocations and ensure all charges for each Payables invoice have been assigned branch from the same Posting Group. All charges posted in one transaction must be within the same Branch Posting Group.
Saving is prevented because charges for the same Invoice number have been entered using a mix of branch Posting Groups.";
			ForwardingConsol consol = TestObjectCreator.CreateConsol();
			Factory.Save();
			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);
			var shipment1 = consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				var cost1 = apps.CostsCollection.TryAddNew();
				cost1.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
				cost1.E6_OSCostAmount = 100M;
				var cost2 = apps.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = TestObjectCreator.DSBChargeCode.PK;
				cost2.E6_OSCostAmount = 200M;
				//both apportionments have same creditor
				cost1.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cost2.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				var cost1SplitCharge = cost1.ApportionmentCharges[0];
				var cost2SplitCharge = cost2.ApportionmentCharges[0];
				//both apportionments have different branches
				cost1SplitCharge.JR_GB = branch1.PK;
				cost2SplitCharge.JR_GB = branch2.PK;
				foreach (var registryValue in new bool[] { true, false })
				{
					var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration()
					{ EnableBranchLevelPosting = registryValue };
					AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
					cost1.E6_InvoiceNum = "111";
					cost2.E6_InvoiceNum = "111";
					if (registryValue)
					{
						AssertHasError("splitCharge2 should have error as the two apportionemnts from different consol costs have different branches but have same invoice number when the registry is turned on", cost2.E6_InvoiceNumInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", cost2.E6_InvoiceNumInfo, expectedError);
					}

					//both apportionment now have same branch
					cost2SplitCharge.JR_GB = branch1.PK;
					cost2.RunPreSaveValidation();
					if (registryValue)
					{
						AssertNoError("splitCharge2 should no longer have error as the two apportionemnts from different consol costs with same invoice number now have same branch when the registry is turned on", cost2.E6_InvoiceNumInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", cost2.E6_InvoiceNumInfo, expectedError);
					}

					cost2SplitCharge.JR_GB = branch2.PK;
					cost2.RunPreSaveValidation();
					if (registryValue)
					{
						AssertHasError("splitCharge2 should have error as the two apportionemnts from different consol costs have different branches but have same invoice number when the registry is turned on", cost2.E6_InvoiceNumInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", cost2.E6_InvoiceNumInfo, expectedError);
					}

					cost2.E6_InvoiceNum = "222";
					if (registryValue)
					{
						AssertNoError("splitCharge2 should no longer have error as the two apportionemnts from different consol costs now have different invoice number when the registry is turned on", cost2.E6_InvoiceNumInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", cost2.E6_InvoiceNumInfo, expectedError);
					}

					cost2.E6_InvoiceNum = "111";
					if (registryValue)
					{
						AssertHasError("splitCharge2 should have error as the two apportionemnts from different consol costs have different branches but have same invoice number when the registry is turned on", cost2.E6_InvoiceNumInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", cost2.E6_InvoiceNumInfo, expectedError);
					}

					branchLevelPostingConfiguration = new BranchLevelPostingConfiguration()
					{ EnableBranchLevelPosting = true };
					var settings1 = new BranchGroupSettings();
					settings1.BranchPK = branch1.PK;
					settings1.GroupNumber = 1;
					settings1.IsParentBranch = false;
					var settings2 = new BranchGroupSettings();
					settings2.BranchPK = branch2.PK;
					settings2.GroupNumber = 1;
					settings2.IsParentBranch = true;
					branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
					branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);
					AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
					cost2.RunPreSaveValidation();
					if (registryValue)
					{
						AssertNoError("splitCharge2 should no longer have error as the two apportionemnts from different consol costs belong to branch which is in the same posting group", cost2.E6_InvoiceNumInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", cost2.E6_InvoiceNumInfo, expectedError);
					}
				}
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCheckE6_InvoiceDate_WithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			Consol = CreateSampleForwardingConsol();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			Cost1.E6_RX_NKCurrency = "AUD";
			Cost1.E6_InvoiceDate = ZDateTime.Now;
			Cost1.Validation.ValidateE6_InvoiceDate();
			AssertNoErrors(Cost1.E6_InvoiceDateInfo);
			Cost1.E6_RX_NKCurrency = "USD";
			Cost1.Validation.ValidateE6_InvoiceDate();
			AssertNoErrors(Cost1.E6_InvoiceDateInfo);
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			Cost1.Validation.ValidateE6_InvoiceDate();
			AssertHasWarning(Cost1.E6_InvoiceDateInfo, @"This estimated cost amount has been calculated based on current job exchange rate.
This will be converted based on ""AP Invoice Posting Exchange Rate Option"" registry value ""Today Exchange Rate"" on posting as actual cost.");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			Cost1.Validation.ValidateE6_InvoiceDate();
			AssertHasWarning(Cost1.E6_InvoiceDateInfo, @"This estimated cost amount has been calculated based on current job exchange rate.
This will be converted based on ""AP Invoice Posting Exchange Rate Option"" registry value ""Exchange Rate based on Invoice Date"" on posting as actual cost.");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			Cost1.Validation.ValidateE6_InvoiceDate();
			AssertHasWarning(Cost1.E6_InvoiceDateInfo, @"This estimated cost amount has been calculated based on current job exchange rate.
This will be converted based on ""AP Invoice Posting Exchange Rate Option"" registry value ""Exchange Rate based on Post Date"" on posting as actual cost.");
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		ForwardingConsol CreateSampleForwardingConsol()
		{
			Consol = Factory.New<ForwardingConsol>();
			var shipment = TestObjectCreator.CreateShipment("S0000199");
			Consol.Shipments.Add(shipment);
			Job job = TestObjectCreator.CreateJob(shipment);
			Charge charge = TestObjectCreator.CreateCharge(job, ObjectCreator.CC1, "", TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1, TestObjectCreator.USD, 0M, null);
			charge.JR_GB = ObjectCreator.AUDBankAccount.Branch.PK;
			Factory.Save();
			Consol.Shipments.AddNew();
			Apps = new ApportionmentListing(Factory, Consol);
			Cost1 = Apps.CostsCollection.TryAddNew();
			Cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			Cost1.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			Cost1.E6_OSCostAmount = 100;
			Cost1.E6_RX_NKCurrency = "USD";
			Cost1.E6_ExchangeRate = 1.2m;
			Cost1.E6_InvoiceNum = "InvoiceTest";
			Cost2 = Apps.CostsCollection.TryAddNew();
			Cost2.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			Cost2.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			Cost2.E6_OSCostAmount = 100;
			Cost2.E6_RX_NKCurrency = "USD";
			Cost2.E6_ExchangeRate = 1.1m;
			Cost2.E6_InvoiceNum = "InvoiceTest";
			return Consol;
		}

		JobConsolCost AddMoreCostToSampleConsol(AccChargeCode chargeCode, OrgHeader creditor)
		{
			var cost = Apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode.PK;
			cost.E6_OH_Creditor = creditor != null ? creditor.PK : ZGuid.Empty;
			cost.E6_OSCostAmount = 100;
			cost.E6_RX_NKCurrency = "USD";
			cost.E6_ExchangeRate = 1.1m;
			cost.E6_InvoiceNum = "InvoiceTest";
			return cost;
		}
	}
}
