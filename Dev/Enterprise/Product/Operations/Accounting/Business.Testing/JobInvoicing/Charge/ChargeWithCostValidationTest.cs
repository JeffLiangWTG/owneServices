using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public abstract class ChargeWithCostValidationTest : BaseChargeValidationTest
	{
		#region Charge Group Validations

		public void TestSameChargeCodeIsUsedBySameRelatedJobNumber_AddingNewChargeInvalidatesCachedValidationResult()
		{
			var expectedError = @"There is more than one charge using this charge code for the same Related Job Number: S00001.
Please confirm that the selection is valid.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_Calc_RelatedJobNumber = "S00001";

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
			});

			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_Calc_RelatedJobNumber = "S00001";

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});
		}

		public void TestSameChargeCodeIsUsedBySameRelatedJobNumber_RemovingChargeInvalidatesCachedValidationResult()
		{
			var expectedError = @"There is more than one charge using this charge code for the same Related Job Number: S00001.
Please confirm that the selection is valid.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_Calc_RelatedJobNumber = "S00001";
			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_Calc_RelatedJobNumber = "S00001";

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});

			charge2.Delete();
			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
			});
		}

		public void TestSameChargeCodeIsUsedBySameRelatedJobNumber_ChangingChargeCodeInvalidatesCachedValidationResult()
		{
			var expectedError = @"There is more than one charge using this charge code for the same Related Job Number: S00001.
Please confirm that the selection is valid.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_Calc_RelatedJobNumber = "S00001";
			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_Calc_RelatedJobNumber = "S00001";

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});

			charge2.JR_AC = TestObjectCreator.CC2.PK;
			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
				AssertNoWarning(charge2.JR_ACInfo, expectedError);
			});
		}

		public void TestSameChargeCodeIsUsedBySameRelatedJobNumber_ChangingRelatedJobInvalidatesCachedValidationResult()
		{
			var expectedError = @"There is more than one charge using this charge code for the same Related Job Number: S00001.
Please confirm that the selection is valid.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_Calc_RelatedJobNumber = "S00001";
			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_Calc_RelatedJobNumber = "S00001";

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});

			charge2.JR_Calc_RelatedJobNumber = shipment2.JS_UniqueConsignRef;
			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
				AssertNoWarning(charge2.JR_ACInfo, expectedError);
			});
		}

		public void TestSameChargeCodeIsUsedBySameRelatedJobNumber_PostingChargeInvalidatesCachedValidationResult()
		{
			var expectedError = @"There is more than one charge using this charge code for the same Related Job Number: S00001.
Please confirm that the selection is valid.";

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S00001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", gatewayConsol);
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var charge1 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge1.JR_Calc_RelatedJobNumber = "S00001";
			var charge2 = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1);
			charge2.JR_Calc_RelatedJobNumber = "S00001";
			charge2.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasWarning(charge1.JR_ACInfo, expectedError);
				AssertHasWarning(charge2.JR_ACInfo, expectedError);
			});

			Factory.Save();

			//post charge2
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var line = TestObjectCreator.CreateARInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "test charge 2", 100m);
			charge2.ARLine.AL_JH = ZGuid.Empty;
			charge2.ARLine.AL_ReverseDate = ZDateTime.Today;
			charge2.JR_AL_ARLine = line.PK;
			Factory.Save();

			gatewayJob.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoWarning(charge1.JR_ACInfo, expectedError);
				AssertNoWarning(charge2.JR_ACInfo, expectedError);
			});
		}

		#endregion

		#region TaxDate

		public void TestCheckJR_CostTaxDate_HasNoError_WhenChargeIsPostedOrApportioned()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job);
			charge.JR_AT_CostGSTRate = CreateTaxRate().PK;
			const string errorMessage = "No rate found for selected date.";

			charge.JR_CostTaxDate = ZDate.Today;
			AssertNoError(charge.JR_CostTaxDateInfo, errorMessage);

			charge.JR_CostTaxDate = ZDate.Today.AddDays(5);
			AssertHasError(charge.JR_CostTaxDateInfo, errorMessage);

			charge.JR_CostTaxDate = ZDate.Today.AddDays(1);
			AssertNoError(charge.JR_CostTaxDateInfo, errorMessage);

			charge.JR_CostTaxDate = ZDate.Today.AddDays(5);
			charge.JR_AL_APLine = Factory.New<APInvoiceLine>().PK;
			Assert("Precondition: IsCostPosted", charge.IsCostPosted);
			charge.Validation.ValidateJR_CostTaxDate();
			AssertNoError(charge.JR_CostTaxDateInfo, errorMessage);

			charge.ClearCostLink();
			charge.JR_AL_APLine = Factory.New<Accrual>().PK;
			Assert("Precondition: IsCostPosted", !charge.IsCostPosted);
			charge.Validation.ValidateJR_CostTaxDate();
			AssertHasError(charge.JR_CostTaxDateInfo, errorMessage);

			charge.JR_E6 = Factory.New<JobConsolCost>().PK;
			Assert("Precondition: JR_IsApportioned", charge.JR_IsApportioned);
			charge.Validation.ValidateJR_CostTaxDate();
			AssertNoError(charge.JR_CostTaxDateInfo, errorMessage);

			charge.JR_E6 = ZGuid.Empty;
			Assert("Precondition: JR_IsApportioned", !charge.JR_IsApportioned);
			charge.Validation.ValidateJR_CostTaxDate();
			AssertHasError(charge.JR_CostTaxDateInfo, errorMessage);

			charge.JR_AT_CostGSTRate = ZGuid.Invalid;
			AssertNoError(charge.JR_CostTaxDateInfo, errorMessage);
		}

		public void TestCheckJR_SellTaxDate()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job);
			charge.JR_AT_SellGSTRate = CreateTaxRate().PK;
			const string errorMessage = "No rate found for selected date.";

			charge.JR_SellTaxDate = ZDate.Today;
			AssertNoError(charge.JR_SellTaxDateInfo, errorMessage);

			charge.JR_SellTaxDate = ZDate.Today.AddDays(5);
			AssertHasError(charge.JR_SellTaxDateInfo, errorMessage);

			charge.JR_SellTaxDate = ZDate.Today.AddDays(1);
			AssertNoError(charge.JR_SellTaxDateInfo, errorMessage);

			charge.JR_SellTaxDate = ZDate.Today.AddDays(5);
			charge.JR_AL_ARLine = Factory.New<ARInvoiceLine>().PK;
			Assert("Precondition: IsRevenuePosted", charge.IsRevenuePosted);
			charge.Validation.ValidateJR_SellTaxDate();
			AssertNoError(charge.JR_SellTaxDateInfo, errorMessage);

			charge.ClearRevenueLink();
			charge.JR_AL_ARLine = Factory.New<WIP>().PK;
			Assert("Precondition: IsRevenuePosted", !charge.IsRevenuePosted);
			charge.Validation.ValidateJR_SellTaxDate();
			AssertHasError(charge.JR_SellTaxDateInfo, errorMessage);

			charge.JR_AT_SellGSTRate = ZGuid.Invalid;
			AssertNoError(charge.JR_SellTaxDateInfo, errorMessage);
		}

		AccTaxRate CreateTaxRate()
		{
			var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			rate.SetRate_ForTestOnly(10, 1, ZDate.Today.AddDays(-1), ZDate.Today);
			rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
			return rate;
		}

		#endregion

		#region  Sell Account

		public void TestValidateJR_OH_SellAccountWhenDifferentToLocalClientOrAgent()
		{
			OrgHeader testAgent = OrgHeader.New(Factory);
			testAgent.OH_Code = "TA1";
			OrgHeader localClient = OrgHeader.New(Factory);
			localClient.OH_Code = "LC1";
			OrgHeader otherOrg = OrgHeader.New(Factory);
			otherOrg.OH_Code = "OO1";

			localClient.CompanyData.OB_ARCreditLimit = 100m;
			localClient.OH_IsDebtor = true;
			Factory.Save();

			var aRInv = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", TestObjectCreator.AUD, 1m, 90m, 0m, 90m, 0m, localClient, TestObjectCreator.CC3.PK);
			AccTransactionMatchLink matchlink1 = ((IMatching)aRInv).CurrentMatchGroup.AddNew();
			matchlink1.AP_Amount = 90m;
			matchlink1.AP_AH = aRInv.PK;
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_IsCancelled = false;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -90m;

			AccTransactionMatchLink linkToMatch = ((IMatching)aRInv).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -90m;
			TestObjectCreator.SetupMatchLinkMatchDate(aRInv);

			aRInv.AH_LocalOutstandingAmount = 0m;

			Factory.Save();

			CommonShipment ship = CommonShipment.New(Factory);

			using (Job job = Job.CreateWithMutex(Factory, ship))
			{
				job.PlugInData = ship;
				job.LocalChargesPK = localClient.PK;

				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				Charge testCharge = job.Charges.AddNew();

				testCharge.JR_AC = TestObjectCreator.CC1.PK;

				testCharge.JR_OH_SellAccount = ZGuid.Empty;
				AssertNoWarnings("No warnings if blank", testCharge.JR_OH_SellAccountInfo);

				//TestCharge = Job.Charges.AddNew();
				testCharge.JR_OH_SellAccount = localClient.PK;
				AssertNoWarnings("No warnings if local client", testCharge.JR_OH_SellAccountInfo);

				testCharge.JR_OH_SellAccount = otherOrg.PK;
				AssertHasWarnings("Warning as Debtor is different to Local Client or Agent", testCharge.JR_OH_SellAccountInfo);

				testCharge.JR_OH_SellAccount = testAgent.PK;
				AssertHasWarnings("Warning as Debtor is different to Local Client or Agent", testCharge.JR_OH_SellAccountInfo);

				aRInv.AH_OH = testAgent.PK;
				Factory.Save();

				job.AgentCollectPK = testAgent.PK;
				testCharge.Validation.ValidateJR_OH_SellAccount();
				AssertNoWarnings("Warning cleared as Agent is now specified and job agent is also specified", testCharge.JR_OH_SellAccountInfo);

				testCharge.JR_OH_SellAccount = ZGuid.NewZGuid();
				AssertHasErrors("Invalid Org", testCharge.JR_OH_SellAccountInfo);
			}

			//With Cross trade shipment
			ship.JS_RL_NKOrigin = "USLAX";
			ship.JS_RL_NKDestination = "NZAKL";

			using (Job job = Job.CreateWithMutex(Factory, ship))
			{
				job.PlugInData = ship;
				job.LocalChargesPK = localClient.PK;
				job.AgentCollectPK = testAgent.PK;

				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				Charge testCharge = job.Charges.AddNew();

				testCharge.JR_AC = TestObjectCreator.CC1.PK;
				testCharge.JR_OH_SellAccount = otherOrg.PK;
				AssertNoWarnings("With Cross Trade, No warning if Debtor is different to Local Client or Agent", testCharge.JR_OH_SellAccountInfo);
			}
		}

		public void TestValidateJR_OH_SellAccount_Gateway()
		{
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			using (var job = TestObjectCreator.CreateJob(consol))
			{
				Assert("Pre-condition, job is gateway billing", job.IsGatewayBillingJob());
				var charge = job.Charges.AddNew();
				var expectedErrorMessage = "You have selected a Debtor that is not an Organization Proxy of any sister company or branch. Generally, gateway agents perform services for other branches in their own network and not third party customers. Please confirm that the selection is valid.";

				charge.JR_OH_SellAccount = TestObjectCreator.CreateOrgHeader("newOrg", true, true).PK;
				charge.Validation.ValidateJR_OH_SellAccount();
				AssertHasWarning("Warning added when debtor isn't org proxy", charge.JR_OH_SellAccountInfo, expectedErrorMessage);

				charge.JR_OH_SellAccount = TestObjectCreator.CreateOrgHeader("newerOrg", true, true, false, false, false, false, isOrgProxyForAnotherCompany: true).PK;
				AssertNotEquals("Pre-condtion, sell account is not agent", job.AgentCollectPK, charge.JR_OH_SellAccount);
				AssertNotEquals("Pre-condtion, sell account is not agent", job.LocalChargesPK, charge.JR_OH_SellAccount);
				charge.Validation.ValidateJR_OH_SellAccount();
				AssertNoWarnings("no warning when debtor is org proxy", charge.JR_OH_SellAccountInfo);
			}
		}

		public void TestCreditCheckingForSellAccount()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.OH_IsDebtor = true;
			Factory.Save();

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = org.PK;
			aRInv.AH_InvoiceAmount = 90m;
			aRInv.AH_OutstandingAmount = 90m;

			ARInvoiceLine line = (ARInvoiceLine)aRInv.Lines.AddNew();
			AccChargeCode chargeCode = TestObjectCreator.CC1;
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = 90m;

			JobCharge jobCharge = job.Charges.AddNew();
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OSSellAmt = 90m;

			Factory.Save();

			Job testJob = new BusinessObjectFactory().NewJobForTesting<Job>();
			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org.PK;
			AssertNoWarning(testCharge.JR_OH_SellAccountInfo, "The Credit Limit for " + org.OH_Code.Trim() + " is set to ");

			ARInvoice aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv2.AH_OH = org.PK;
			aRInv2.AH_InvoiceAmount = 20m;
			aRInv2.AH_OutstandingAmount = 20m;

			line = (ARInvoiceLine)aRInv2.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = 20m;

			jobCharge = job.Charges.AddNew();
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OSSellAmt = 20m;

			Factory.Save();

			Job testJob2 = new BusinessObjectFactory().NewJobForTesting<Job>();
			Charge testCharge2 = testJob2.Charges.AddNew();
			testCharge2.JR_OH_SellAccount = org.PK;
			AssertHasWarningContaining(testCharge2.JR_OH_SellAccountInfo, "The Credit Limit for " + org.OH_Code.Trim() + " is set to ");
		}

		public void TestCreditCheckingForSellAccountWithOrgNotDebtor()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.OH_IsDebtor = true;
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_ARCreditLimit = 100m;
			org2.OH_IsDebtor = false;
			Factory.Save();

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = org.PK;
			aRInv.AH_InvoiceAmount = 200m;
			aRInv.AH_OutstandingAmount = 200m;

			ARInvoiceLine line = (ARInvoiceLine)aRInv.Lines.AddNew();
			AccChargeCode chargeCode = TestObjectCreator.CC1;
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = 200m;

			JobCharge jobCharge = job.Charges.AddNew();
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OSSellAmt = 200m;

			Factory.Save();

			Job testJob = new BusinessObjectFactory().NewJobForTesting<Job>();
			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org.PK;

			Factory.Save();

			testCharge.JR_OH_SellAccount = ZGuid.Empty;
			testCharge.JR_OH_SellAccount = org.PK;
			AssertHasWarningContaining(testCharge.JR_OH_SellAccountInfo, "over the credit limit");

			testCharge.JR_OH_SellAccount = ZGuid.Empty;
			testCharge.JR_OH_SellAccount = org2.PK;
			AssertEquals("Charges should have warnings", true, testCharge.JR_OH_SellAccountInfo.HasWarnings());
			AssertNoWarningContaining(testCharge.JR_OH_SellAccountInfo, "over the credit limit");
		}

		public void TestSellAccountNotValidatedWhenRevenuePosted()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = false;
			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = org.PK;
			InvoicingLineBase line = (InvoicingLineBase)aRInv.Lines.AddNew();
			line.AL_OSExTaxAmount = 200m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_IsCancelled = false;
			Factory.Save();

			Job testJob = new BusinessObjectFactory().NewJobForTesting<Job>();
			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_AL_ARLine = line.PK;
			testCharge.JR_OH_SellAccount = org.PK;
			testCharge.Validation.ValidateJR_OH_SellAccount();
			AssertNoErrors(testCharge.JR_OH_SellAccountInfo);

			testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org.PK;
			testCharge.Validation.ValidateJR_OH_SellAccount();
			AssertHasErrors(testCharge.JR_OH_SellAccountInfo);
		}

		#endregion

		#region  JR_OSSellAmt

		public void TestAllowNegativePostedRevenueChargeOnJob()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var job = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				job.JH_JobNum = TestObjectCreator.GetRandomString(10);
				TestObjectCreator.Factory.Save();

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(10), TestObjectCreator.USD, 1m, TestObjectCreator.Debtor);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.USD, 1m, "text", -500m);
				var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);
				TestObjectCreator.Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					charge.Validation.ValidateJR_OSSellAmt();
					AssertNoErrors(charge.JR_OSSellAmtInfo);
				}
			}
		}

		public void TestAllowNegativeRevenueChargesOnJob()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1m, TestObjectCreator.Agent, 1m);

			using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0m, -9m);
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.Validation.ValidateJR_OSSellAmt();
				AssertHasError(charge.JR_OSSellAmtInfo, "Negative revenue charges are not allowed. This is controlled by the registry setting at Accounting > Job Invoicing > Allow negative charges on a job.");
			}

			using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0m, 99m);
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.Validation.ValidateJR_OSSellAmt();
				AssertNoErrors(charge.JR_OSSellAmtInfo);
			}

			using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0m, -999m);
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.Validation.ValidateJR_OSSellAmt();
				AssertHasError(charge.JR_OSSellAmtInfo, "Negative revenue charges are not allowed. This is controlled by the registry setting at Accounting > Job Invoicing > Allow negative charges on a job.");
			}

			using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0m, 9999m);
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.Validation.ValidateJR_OSSellAmt();
				AssertNoErrors(charge.JR_OSSellAmtInfo);
			}
		}

		#endregion

		#region JR_AT_SellGSTRate

		public void TestCheckJR_AT_SellGSTRateWithTaxIsApplicable()
		{
			var org = TestObjectCreator.CreateOrgHeader("TSTORG", false, true);
			org.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var job = TestObjectCreator.CreateJobHeader();
			Charge charge = job.Charges.AddNew();
			charge.JR_OH_SellAccount = org.PK;

			charge.Validation.ValidateJR_AT_SellGSTRate();

			AssertHasError(charge.JR_AT_SellGSTRateInfo, "Tax IDs on unposted charges conflict with the debtor \"Tax is Applicable\" flag.\r\nOne possible way to resolve this is to go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.");

			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;

			charge.Validation.ValidateJR_AT_SellGSTRate();

			AssertNoErrors(charge.JR_AT_SellGSTRateInfo);
		}

		public void TestCheckJR_AT_SellGSTRateWithTaxIsNotApplicable()
		{
			var org = TestObjectCreator.CreateOrgHeader("TSTORG", false, true);
			org.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var job = TestObjectCreator.CreateJobHeader();
			Charge charge = job.Charges.AddNew();

			charge.Validation.ValidateJR_AT_SellGSTRate();
			AssertEquals("Pre-condition: the sell account should be empty.", false, charge.JR_OH_SellAccount.IsValid);
			AssertNoErrors("Should not add error as the sell account is empty.", charge.JR_AT_SellGSTRateInfo);

			charge.JR_OH_SellAccount = org.PK;
			charge.Validation.ValidateJR_AT_SellGSTRate();
			AssertNoErrors(charge.JR_AT_SellGSTRateInfo);

			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
			charge.Validation.ValidateJR_AT_SellGSTRate();

			AssertHasError(charge.JR_AT_SellGSTRateInfo, "Tax IDs on unposted charges conflict with the debtor \"Tax is Applicable\" flag.\r\nOne possible way to resolve this is to go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.");
		}

		public void TestCheckJR_AT_SellGSTRateWithTaxIsNotApplicableAndRevenuePosted()
		{
			var org = TestObjectCreator.CreateOrgHeader("TSTORG", false, true);
			org.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M, org);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 12M, Guid.Empty, true); //Tax not set on the invoice line

			var job = TestObjectCreator.CreateJobHeader();
			Charge charge = job.Charges.AddNew();

			charge.JR_OH_SellAccount = org.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
			charge.Validation.ValidateJR_AT_SellGSTRate();
			AssertHasError(charge.JR_AT_SellGSTRateInfo, "Tax IDs on unposted charges conflict with the debtor \"Tax is Applicable\" flag.\r\nOne possible way to resolve this is to go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.");

			charge.JR_AL_ARLine = line.PK;
			AssertEquals("Pre-condition: Revenue should be posted.", true, charge.IsRevenuePosted);
			charge.Validation.ValidateJR_AT_SellGSTRate();
			AssertNoErrors("Should not add error as Revenue posted.", charge.JR_AT_SellGSTRateInfo);
		}

		#endregion

		#region JR_AT_CostGSTRate

		public void TestJR_AT_CostGSTRateWithTaxIsApplicable()
		{
			var org = TestObjectCreator.CreateOrgHeader("TSTORG", true, false);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var job = TestObjectCreator.CreateJobHeader();
			Charge charge = job.Charges.AddNew();
			charge.JR_OH_CostAccount = org.PK;

			charge.Validation.ValidateJR_AT_CostGSTRate();

			AssertHasError(charge.JR_AT_CostGSTRateInfo, "Tax IDs on unposted charges conflict with the creditor \"Tax is Applicable\" flag.\r\nOne possible way to resolve this is to go into the \"Job Invoicing\" menu and click \"Reset Unposted lines Tax Default\" option.");

			charge.JR_AT_CostGSTRate = TestObjectCreator.GSTFREE1.PK;

			charge.Validation.ValidateJR_AT_CostGSTRate();

			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);
		}

		public void TestJR_AT_CostGSTRateWithTaxIsNotApplicable()
		{
			var org = TestObjectCreator.CreateOrgHeader("TSTORG", true, false);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var job = TestObjectCreator.CreateJobHeader();
			Charge charge = job.Charges.AddNew();

			charge.Validation.ValidateJR_AT_CostGSTRate();
			AssertEquals("Pre-condition: the cost account should be empty.", false, charge.JR_OH_CostAccount.IsValid);
			AssertNoErrors("Should not add error as the cost account is empty.", charge.JR_AT_CostGSTRateInfo);

			charge.JR_OH_CostAccount = org.PK;
			charge.Validation.ValidateJR_AT_CostGSTRate();
			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);

			charge.JR_AT_CostGSTRate = TestObjectCreator.GSTFREE1.PK;
			charge.Validation.ValidateJR_AT_CostGSTRate();

			AssertHasError(charge.JR_AT_CostGSTRateInfo, "Tax IDs on unposted charges conflict with the creditor \"Tax is Applicable\" flag.\r\nOne possible way to resolve this is to go into the \"Job Invoicing\" menu and click \"Reset Unposted lines Tax Default\" option.");
		}

		public void TestCheckJR_AT_CostGSTRateWithTaxIsNotApplicableAndCostPosted()
		{
			var org = TestObjectCreator.CreateOrgHeader("TSTORG", true, false);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M, org);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 12M, Guid.Empty, true); //Tax not set on the invoice line

			var job = TestObjectCreator.CreateJobHeader();
			Charge charge = job.Charges.AddNew();

			charge.JR_OH_CostAccount = org.PK;
			charge.JR_AT_CostGSTRate = TestObjectCreator.GSTFREE1.PK;
			charge.Validation.ValidateJR_AT_SellGSTRate();
			AssertHasError(charge.JR_AT_CostGSTRateInfo, "Tax IDs on unposted charges conflict with the creditor \"Tax is Applicable\" flag.\r\nOne possible way to resolve this is to go into the \"Job Invoicing\" menu and click \"Reset Unposted lines Tax Default\" option.");

			charge.JR_AL_APLine = line.PK;
			AssertEquals("Pre-condition: Cost should be posted.", true, charge.IsCostPosted);
			charge.Validation.ValidateJR_AT_SellGSTRate();
			AssertNoErrors("Should not add error as Cost posted.", charge.JR_AT_SellGSTRateInfo);
		}

		#endregion

		#region JR_GB_SellTaxBranch

		public void TestCheckJR_GB_SellTaxBranchWithTaxIsApplicable()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = TestObjectCreator.CreateOrgHeader("TSTORG", false, true);
			org.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var job = TestObjectCreator.CreateJobHeader();
			var charge = job.Charges.AddNew();
			charge.JR_OH_SellAccount = org.PK;
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;

			charge.Validation.ValidateJR_GB_SellTaxBranch();

			AssertHasError(charge.JR_GB_SellTaxBranchInfo, TaxBranchConflictErrorMessage);

			charge.JR_GB_SellTaxBranch = TestObjectCreator.NonCurrentBranch.PK;

			charge.Validation.ValidateJR_GB_SellTaxBranch();

			AssertNoErrors(charge.JR_GB_SellTaxBranchInfo);
		}

		public void TestCheckJR_GB_SellTaxBranchWithTaxIsNotApplicable()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = TestObjectCreator.CreateOrgHeader("TSTORG", false, true);
			org.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var job = TestObjectCreator.CreateJobHeader();
			var charge = job.Charges.AddNew();

			charge.Validation.ValidateJR_GB_SellTaxBranch();
			AssertEquals("Pre-condition: the sell account should be empty.", false, charge.JR_OH_SellAccount.IsValid);
			AssertNoErrors("Should not add error as the sell account is empty.", charge.JR_GB_SellTaxBranchInfo);

			charge.JR_OH_SellAccount = org.PK;
			charge.Validation.ValidateJR_GB_SellTaxBranch();
			AssertNoErrors(charge.JR_GB_SellTaxBranchInfo);

			charge.JR_GB_SellTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge.Validation.ValidateJR_GB_SellTaxBranch();

			AssertHasError(charge.JR_GB_SellTaxBranchInfo, TaxBranchConflictErrorMessage);
		}

		public void TestCheckJR_GB_SellTaxBranchWithTaxIsNotApplicableAndRevenuePosted()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = TestObjectCreator.CreateOrgHeader("TSTORG", false, true);
			org.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M, org);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 12M, Guid.Empty, true); //Tax not set on the invoice line

			var job = TestObjectCreator.CreateJobHeader();
			var charge = job.Charges.AddNew();

			charge.JR_OH_SellAccount = org.PK;
			charge.JR_GB_SellTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge.Validation.ValidateJR_GB_SellTaxBranch();
			AssertHasError(charge.JR_GB_SellTaxBranchInfo, TaxBranchConflictErrorMessage);

			charge.JR_AL_ARLine = line.PK;
			AssertEquals("Pre-condition: Revenue should be posted.", true, charge.IsRevenuePosted);
			charge.Validation.ValidateJR_GB_SellTaxBranch();
			AssertNoErrors("Should not add error as Revenue posted.", charge.JR_GB_SellTaxBranchInfo);
		}

		string TaxBranchConflictErrorMessage => @"Tax branch values on unposted charges in the billing tab conflict with at least one of the following settings:
- 'Enabled Tax Branch Reporting' registry value
- Debtor / Creditor 'Tax is Applicable' flag
- Current Login Company 'VAT Registered' flag
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click the ""Reset Unposted lines Tax Default"" option.";

		#endregion

		#region JR_GB_CostTaxBranch

		public void TestCheckJR_GB_CostTaxBranchWithTaxIsApplicable()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = TestObjectCreator.CreateOrgHeader("TSTORG", true, false);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var job = TestObjectCreator.CreateJobHeader();
			var charge = job.Charges.AddNew();
			charge.JR_OH_CostAccount = org.PK;
			charge.JR_GB_CostTaxBranch = ZGuid.Empty;

			charge.Validation.ValidateJR_GB_CostTaxBranch();

			AssertHasError(charge.JR_GB_CostTaxBranchInfo, TaxBranchConflictErrorMessage);

			charge.JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;

			charge.Validation.ValidateJR_GB_CostTaxBranch();

			AssertNoErrors(charge.JR_GB_CostTaxBranchInfo);
		}

		public void TestCheckJR_GB_CostTaxBranchWithTaxIsNotApplicable()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = TestObjectCreator.CreateOrgHeader("TSTORG", true, false);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var job = TestObjectCreator.CreateJobHeader();
			var charge = job.Charges.AddNew();

			charge.Validation.ValidateJR_GB_CostTaxBranch();
			AssertEquals("Pre-condition: the cost account should be empty.", false, charge.JR_OH_CostAccount.IsValid);
			AssertNoErrors("Should not add error as the cost account is empty.", charge.JR_GB_CostTaxBranchInfo);

			charge.JR_OH_CostAccount = org.PK;
			charge.Validation.ValidateJR_GB_CostTaxBranch();
			AssertNoErrors(charge.JR_GB_CostTaxBranchInfo);

			charge.JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge.Validation.ValidateJR_GB_CostTaxBranch();

			AssertHasError(charge.JR_GB_CostTaxBranchInfo, TaxBranchConflictErrorMessage);
		}

		public void TestCheckJR_GB_CostTaxBranchWithTaxIsNotApplicableAndCostPosted()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = TestObjectCreator.CreateOrgHeader("TSTORG", true, false);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M, org);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 12M, Guid.Empty, true); //Tax not set on the invoice line

			var job = TestObjectCreator.CreateJobHeader();
			var charge = job.Charges.AddNew();

			charge.JR_OH_CostAccount = org.PK;
			charge.JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge.Validation.ValidateJR_GB_CostTaxBranch();
			AssertHasError(charge.JR_GB_CostTaxBranchInfo, TaxBranchConflictErrorMessage);

			charge.JR_AL_APLine = line.PK;
			AssertEquals("Pre-condition: Cost should be posted.", true, charge.IsCostPosted);
			charge.Validation.ValidateJR_GB_CostTaxBranch();
			AssertNoErrors("Should not add error as Cost posted.", charge.JR_GB_CostTaxBranchInfo);
		}

		public void TestCheckJR_GB_CostTaxBranchWithTaxIsApplicable_WithApportionedCharge()
		{
			var org = TestObjectCreator.CreateOrgHeader("TSTORG", true, false);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S015", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, creditor: org);
			consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			consolCost.E6_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var charge = Factory.Load<Charge>(consolCost.ApportionmentCharges[0].PK);
			charge.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;

			AssertEquals("Pre-condition", true, charge.JR_IsApportioned);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Pre-condition", false, charge.IsCostTaxBranchActual);
			AssertNotEquals(ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);

			charge.Validation.ValidateJR_GB_CostTaxBranch();
			AssertNoErrors("Should not fill tax branch", charge.JR_GB_CostTaxBranchInfo);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			consolCost.E6_GB_CostTaxBranch = ZGuid.Empty;
			AssertEquals("Pre-condition", false, charge.IsCostTaxBranchActual);
			AssertEquals(ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);

			charge.Validation.ValidateJR_GB_CostTaxBranch();
			AssertNoErrors("Should fill tax branch", charge.JR_GB_CostTaxBranchInfo);

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", TestObjectCreator.AUD, 1m, 100m, 100m, 100m, 100m);
			charge.JR_AL_APLine = invoice.Lines[0].PK;
			AssertEquals("Pre-condition", true, charge.JR_IsCostPosted);

			charge.Validation.ValidateJR_GB_CostTaxBranch();
			AssertNoErrors("Cost Charge is posted", charge.JR_GB_CostTaxBranchInfo);
		}

		#endregion

		#region Properties

		public void TestJR_CostPlaceOfSupplyWhenEnforcePostingFPOSForAP()
		{
			JobCharge charge1 = Factory.New<JobCharge>();
			JobCharge charge2 = Factory.New<JobCharge>();
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

			void AssertCostPlaceOfSupply(OrgHeader charge1CostAccount, string charge1ApInvoiceNum, string charge1CostFPOS,
				OrgHeader charge2CostAccount, string charge2ApInvoiceNum, string charge2CostFPOS, bool expectError)
			{
				charge1.JR_CostPlaceOfSupply = "";
				charge2.JR_CostPlaceOfSupply = "";

				charge1.JR_OH_CostAccount = charge1CostAccount?.PK ?? ZGuid.Empty;
				charge2.JR_OH_CostAccount = charge2CostAccount?.PK ?? ZGuid.Empty;
				charge1.JR_APInvoiceNum = charge1ApInvoiceNum;
				charge2.JR_APInvoiceNum = charge2ApInvoiceNum;
				charge1.JR_CostPlaceOfSupply = charge1CostFPOS;
				charge2.JR_CostPlaceOfSupply = charge2CostFPOS;

				charge1.Validation.ValidateJR_CostPlaceOfSupply();
				charge2.Validation.ValidateJR_CostPlaceOfSupply();

				if (expectError)
				{
					var expectedError = "This Charge has a Creditor and an AP Invoice number same as another Charge, but the Cost Fixed Place of Supply does not match.";
					AssertHasError("charge1.JR_CostPlaceOfSupplyInfo.HasError", charge1.JR_CostPlaceOfSupplyInfo, expectedError);
					AssertHasError("charge2.JR_CostPlaceOfSupplyInfo.HasError", charge2.JR_CostPlaceOfSupplyInfo, expectedError);
				}
				else
				{
					AssertNoErrors(charge1.JR_CostPlaceOfSupplyInfo);
					AssertNoErrors(charge2.JR_CostPlaceOfSupplyInfo);
				}
			}
		}

		public void TestValidateJR_LocalSellAmtAndJR_OSSellAmtShouldHaveSameSign_ForceOS()
		{
			var job = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			job.FillWithValidTestData();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000m, TestObjectCreator.ABIGAS);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKSellCurrency = "USD";
			var exchangeRateUSD = job.FirstOrDefault(x => x.CurrencyCode == "USD");
			AssertNotNull(exchangeRateUSD);
			exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			charge.JR_LocalSellAmt = 50;
			charge.JR_OSSellExRate = 1m;
			AssertNoError("Precondition", charge.JR_LocalSellAmtInfo, GetErrorMessageForSameSign(true));
			AssertNoError("Precondition", charge.JR_OSSellAmtInfo, GetErrorMessageForSameSign(true));

			decimal osAmount = -50m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_OSSellAmt] = osAmount;

			charge.Validation.ValidateJR_LocalSellAmt();
			AssertHasError(charge.JR_LocalSellAmtInfo, GetErrorMessageForSameSign(true));

			charge.Validation.ValidateJR_OSSellAmt();
			AssertHasError(charge.JR_OSSellAmtInfo, GetErrorMessageForSameSign(true));
		}

		public void TestValidateJR_LocalSellAmtAndJR_OSSellAmtShouldHaveSameSign_ForceLocal()
		{
			var job = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			job.FillWithValidTestData();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000m, TestObjectCreator.ABIGAS);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKSellCurrency = "USD";
			var exchangeRateUSD = job.FirstOrDefault(x => x.CurrencyCode == "USD");
			AssertNotNull(exchangeRateUSD);
			exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			charge.JR_OSSellAmt = 50;
			charge.JR_OSSellExRate = 1m;
			AssertNoError("Precondition", charge.JR_LocalSellAmtInfo, GetErrorMessageForSameSign(true));
			AssertNoError("Precondition", charge.JR_OSSellAmtInfo, GetErrorMessageForSameSign(true));

			var localAmount = -50m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_LocalSellAmt] = localAmount;

			charge.Validation.ValidateJR_LocalSellAmt();
			AssertHasError(charge.JR_LocalSellAmtInfo, GetErrorMessageForSameSign(true));

			charge.Validation.ValidateJR_OSSellAmt();
			AssertHasError(charge.JR_OSSellAmtInfo, GetErrorMessageForSameSign(true));
		}

		public void TestValidateJR_LocalCostAmtAndJR_OSCostAmtShouldHaveSameSign_ForceOS()
		{
			var job = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			job.FillWithValidTestData();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000m, TestObjectCreator.ABIGAS);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKCostCurrency = "USD";
			var exchangeRateUSD = job.FirstOrDefault(x => x.CurrencyCode == "USD");
			AssertNotNull(exchangeRateUSD);
			exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			charge.JR_LocalCostAmt = 50;
			charge.JR_OSCostExRate = 1m;
			AssertNoError("Precondition", charge.JR_LocalCostAmtInfo, GetErrorMessageForSameSign(false));
			AssertNoError("Precondition", charge.JR_OSCostAmtInfo, GetErrorMessageForSameSign(false));

			decimal osAmount = -50m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_OSCostAmt] = osAmount;

			charge.Validation.ValidateJR_LocalCostAmt();
			AssertHasError(charge.JR_LocalCostAmtInfo, GetErrorMessageForSameSign(false));

			charge.Validation.ValidateJR_OSCostAmt();
			AssertHasError(charge.JR_OSCostAmtInfo, GetErrorMessageForSameSign(false));
		}

		public void TestValidateJR_LocalCostAmtAndJR_OSCostAmtShouldHaveSameSign_ForceLocal()
		{
			var job = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			job.FillWithValidTestData();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000m, TestObjectCreator.ABIGAS);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKCostCurrency = "USD";
			var exchangeRateUSD = job.FirstOrDefault(x => x.CurrencyCode == "USD");
			AssertNotNull(exchangeRateUSD);
			exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			charge.JR_OSCostAmt = 50;
			charge.JR_OSCostExRate = 1m;
			AssertNoError("Precondition", charge.JR_LocalCostAmtInfo, GetErrorMessageForSameSign(false));
			AssertNoError("Precondition", charge.JR_OSCostAmtInfo, GetErrorMessageForSameSign(false));

			var localAmount = -50m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_LocalCostAmt] = localAmount;

			charge.Validation.ValidateJR_LocalCostAmt();
			AssertHasError(charge.JR_LocalCostAmtInfo, GetErrorMessageForSameSign(false));

			charge.Validation.ValidateJR_OSCostAmt();
			AssertHasError(charge.JR_OSCostAmtInfo, GetErrorMessageForSameSign(false));
		}

		string GetErrorMessageForSameSign(bool isSell)
		{
			return string.Format("OS {0} Amount and Local {0} Amount should have the same sign.", isSell ? "Sell" : "Cost");
		}

		public void TestValidateJR_Desc()
		{
			var charge = Factory.New<ChargeWithCostForTest>();
			charge.JR_Desc = "Blah";
			AssertNoErrors(charge.JR_DescInfo);

			charge.JR_Desc = RatingConstants.RateNotePrefix + "blah";
			AssertHasErrors(charge.JR_DescInfo);

			charge.JR_Desc = "Zubin";
			AssertNoErrors(charge.JR_DescInfo);

			charge.JR_Desc = RatingConstants.RateNotePrefix + "blah";
			AssertHasErrors(charge.JR_DescInfo);

			charge.JR_Desc = "Zubin";
			AssertNoErrors(charge.JR_DescInfo);

			charge.ReadOnly = true;
			charge.JR_Desc = RatingConstants.RateNotePrefix + "blah";
			AssertNoErrors(charge.JR_DescInfo);
			charge.ReadOnly = false;

			var costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			charge.JR_AL_APLine = costLine.PK;

			charge.JR_Desc = "";
			AssertHasErrors(charge.JR_DescInfo);

			charge.JR_Desc = "Zubin";
			AssertNoErrors(charge.JR_DescInfo);

			charge.JR_Desc = RatingConstants.RateNotePrefix + "blah";
			AssertHasErrors(charge.JR_DescInfo);

			var sellLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			charge.JR_AL_ARLine = sellLine.PK;

			charge.JR_Desc = "";
			AssertNoErrors(charge.JR_DescInfo);

			charge.JR_Desc = "Zubin";
			AssertNoErrors(charge.JR_DescInfo);

			charge.JR_Desc = RatingConstants.RateNotePrefix + "blah";
			AssertNoErrors(charge.JR_DescInfo);
		}

		public void TestValidateJR_DescWithEnableLocalChargeCodeDescriptionDefaultRegsitry()
		{
			var expectedWarningMessage = "Charge description was changed from default. This description will appear on AR Invoice without translation.";
			var chargeCode = TestObjectCreator.CreateChargeCode("ABC");
			chargeCode.AC_Desc = "My Test Charge Code";
			var charge = Factory.New<ChargeWithCostForTest>();
			charge.JR_AC = chargeCode.PK;

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(chargeCode.AC_Desc, charge.JR_Desc);
				Assert(charge.JR_Desc.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(charge.JR_DescInfo, expectedWarningMessage);

				charge.JR_Desc = "My Test Charge Code and some appended text";
				Assert(charge.JR_Desc.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(charge.JR_DescInfo, expectedWarningMessage);

				charge.JR_Desc = "This is a very different charge code description";
				Assert(!charge.JR_Desc.StartsWith(chargeCode.AC_Desc));
				AssertHasWarning(charge.JR_DescInfo, expectedWarningMessage);
			}

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				charge.JR_Desc = "My Test Charge Code";
				AssertEquals(chargeCode.AC_Desc, charge.JR_Desc);
				Assert(charge.JR_Desc.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(charge.JR_DescInfo, expectedWarningMessage);

				charge.JR_Desc = "My Test Charge Code and some appended text";
				Assert(charge.JR_Desc.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(charge.JR_DescInfo, expectedWarningMessage);

				charge.JR_Desc = "This is a very different charge code description";
				Assert(!charge.JR_Desc.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(charge.JR_DescInfo, expectedWarningMessage);
			}
		}

		public void TestValidateJR_Desc_MinimumLength_InIndiaCompanyOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var charge = Factory.New<ChargeWithCostForTest>();
				charge.JR_Desc = "1";
				AssertHasError(charge.JR_DescInfo, "Charge description has less than 3 characters.");
				charge.JR_Desc = "123";
				AssertNoError(charge.JR_DescInfo, "Charge description has less than 3 characters.");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes._TemplateCountryName_))
			{
				var charge = Factory.New<ChargeWithCostForTest>();
				charge.JR_Desc = "1";
				AssertNoError(charge.JR_DescInfo, "Charge description has less than 3 characters.");
				charge.JR_Desc = "123";
				AssertNoError(charge.JR_DescInfo, "Charge description has less than 3 characters.");
			}
		}

		[TestDate(2005, 1, 11)]
		public virtual void TestValidateJR_PaymentDate()
		{
			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();

			Assert("JR_PaymentDateInfo.ReadOnly must be initially true for this test", testCharge.JR_PaymentDateInfo.ReadOnly);
			testCharge.JR_PaymentDate = ZDateTime.Empty;
			AssertNoErrors("Payment date should not be compulsory when readonly", testCharge.JR_PaymentDateInfo);

			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_APInvoiceNum = "231";
			testCharge.JR_PaymentDate = ZDateTime.Empty;

			AssertHasErrors("Invoice number entered, Creditor entered, payment date should be mandatory", testCharge.JR_PaymentDateInfo);
			testCharge.JR_PaymentDate = new ZDateTime(2004, 5, 5, 5, 5, 5);
			AssertNoErrors("Payment date, Invoice number and creditor entered", testCharge.JR_PaymentDateInfo);

			testCharge.JR_OH_CostAccount = ZGuid.Empty;
			testCharge.JR_PaymentDate = new ZDateTime(2005, 2, 2, 3, 3, 3);
			AssertHasErrors("No creditor entered, payment date should have error when entered", testCharge.JR_PaymentDateInfo);
			testCharge.JR_PaymentDate = ZDateTime.Empty;
			AssertNoErrors("Creditor not entered, Payment date should not be mandatory", testCharge.JR_PaymentDateInfo);

			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_APInvoiceNum = ZString.Empty;
			testCharge.JR_PaymentDate = new ZDateTime(2005, 2, 2, 2, 2, 2);
			AssertHasErrors("No invoice number entered, payment date should have error when entered", testCharge.JR_PaymentDateInfo);
			testCharge.JR_PaymentDate = ZDateTime.Empty;
			AssertNoErrors("invoice number not entered, Payment date should not be mandatory", testCharge.JR_PaymentDateInfo);

			testCharge.JR_APInvoiceDate = ZDateTime.Now;
			testCharge.JR_PaymentDate = ZDateTime.Now.AddDays(-1);

			Assert(testCharge.JR_PaymentDateInfo.HasError("Due date should be after or equal to Invoice Date"));

			testCharge.JR_PaymentDate = ZDateTime.Now.AddDays(3);

			Assert(!testCharge.JR_PaymentDateInfo.HasError("Due date should be after or equal to Invoice Date"));

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			testCharge.JR_AL_APLine = costLine.PK;
			Assert("ReadOnly", testCharge.JR_PaymentDateInfo.ReadOnly);
			testCharge.JR_PaymentDate = new ZDateTime(2005, 2, 2, 3, 3, 3);
			AssertNoErrors("No errors because Cost posted", testCharge.JR_PaymentDateInfo);
		}

		[TestDate(2005, 1, 11)]
		public virtual void TestValidateJR_APInvoiceDate()
		{
			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();

			Assert("JR_APInvoiceDateInfo.ReadOnly must be initially true for this test", testCharge.JR_APInvoiceDateInfo.ReadOnly);
			testCharge.JR_APInvoiceDate = ZDateTime.Empty;
			AssertNoErrors("Invoice date should not be compulsory when readonly", testCharge.JR_APInvoiceDateInfo);

			testCharge.JR_APInvoiceNum = ZString.Empty;
			testCharge.JR_APInvoiceDateInitialReadOnlyExposed = false;
			testCharge.JR_APInvoiceDate = new ZDateTime(2005, 2, 2, 2, 2, 2);
			testCharge.JR_OH_SellAccount = fCreditor.PK;

			AssertHasErrors("Creditor, Invoice date entered but invoice number empty, should have errors", testCharge.JR_APInvoiceDateInfo);
			testCharge.JR_APInvoiceDate = ZDateTime.Empty;
			AssertNoErrors("Invoice date not entered, should not have errors", testCharge.JR_APInvoiceDateInfo);

			testCharge.JR_APInvoiceDate = new ZDateTime(2005, 1, 1, 2, 1, 1);
			AssertHasErrors("Creditor, Invoice date entered but invoice number empty, should have errors", testCharge.JR_APInvoiceDateInfo);

			testCharge.JR_APInvoiceNum = "2323";
			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_APInvoiceDate = new ZDateTime(2005, 1, 1, 1, 1, 1);
			AssertNoErrors("Invoice date, number and creditor entered", testCharge.JR_APInvoiceDateInfo);

			testCharge.JR_APInvoiceNum = "2323";
			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_APInvoiceDate = new ZDateTime(2005, 2, 2, 2, 2, 2);
			AssertHasError(testCharge.JR_APInvoiceDateInfo, "Invoice date cannot be in the future.\r\nThis is determined by registry: Accounting -> Payable Defaults -> Default Settings -> Allow Forward Dating of AP Invoice Date");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testCharge.JR_APInvoiceDate = new ZDateTime(2005, 3, 3, 3, 3, 3);
				AssertNoErrors(testCharge.JR_APInvoiceDateInfo);
				AssertHasWarnings("Invoice Date is in the future. Please check the Invoice Date.", testCharge.JR_APInvoiceDateInfo);
			}

			testCharge.JR_OH_CostAccount = ZGuid.Empty;
			testCharge.JR_APInvoiceDate = new ZDateTime(2005, 1, 1, 1, 1, 1);
			AssertHasErrors("Invoice date, number but no creditor entered", testCharge.JR_APInvoiceDateInfo);

			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_APInvoiceNum = ZString.Empty;
			testCharge.JR_APInvoiceDate = new ZDateTime(2005, 1, 1, 2, 1, 1);
			AssertHasErrors("Invoice date and creditor entered but no invoice number", testCharge.JR_APInvoiceDateInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			testCharge.JR_AL_APLine = costLine.PK;
			Assert("ReadOnly", testCharge.JR_APInvoiceDateInfo.ReadOnly);
			testCharge.JR_APInvoiceDate = new ZDateTime(2005, 1, 1, 1, 1, 1);
			AssertNoErrors("No errors because Cost posted", testCharge.JR_APInvoiceDateInfo);
		}

		[TestDate(2005, 1, 11)]
		public void TestVaildateJR_APDocumentReceivedDate()
		{
			var charge = Factory.New<ChargeWithCostForTest>();

			Assert("JR_APDocumentReceivedDateInfo.ReadOnly must be initially true for this test", charge.JR_APDocumentReceivedDateInfo.ReadOnly);
			charge.JR_APDocumentReceivedDate = ZDateTime.Empty;
			AssertNoErrors("Document Received date should not be compulsory when readonly", charge.JR_APDocumentReceivedDateInfo);

			charge.JR_APDocumentReceivedDateInitialReadOnlyExposed = false;
			charge.JR_APInvoiceNum = ZString.Empty;
			charge.JR_OH_CostAccount = fCreditor.PK;
			charge.JR_APDocumentReceivedDate = new ZDateTime(2005, 2, 2, 2, 2, 2);

			AssertHasError(charge.JR_APDocumentReceivedDateInfo, "An AP Invoice Number must be entered before a Document Received date is entered");
			charge.JR_APDocumentReceivedDate = ZDateTime.Empty;
			AssertNoErrors("Document Received date not entered, should not have errors", charge.JR_APDocumentReceivedDateInfo);

			charge.JR_APInvoiceNum = "1234";
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_APDocumentReceivedDate = new ZDateTime(2005, 1, 1, 1, 1, 1);
			AssertHasError(charge.JR_APDocumentReceivedDateInfo, "A valid Creditor must be entered before a Document Received date is entered");

			charge.JR_OH_CostAccount = fCreditor.PK;

			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Assert(!charge.JR_APInvoiceNum.IsEmpty);

				charge.JR_APDocumentReceivedDate = ZDateTime.Empty;
				charge.Validation.ValidateJR_APDocumentReceivedDate();

				AssertHasError(charge.JR_APDocumentReceivedDateInfo, "Please enter a value.");
			}
		}

		public void TestNotMandatoryToHaveLocalCostAndLocalSellAmountsEqualOnDisbursementCharge()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.AUD.Code;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = creator.AUD.Code;
			Job job = creator.Job1;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var exRate = creator.SetExchangeRate(job, creator.USD, 2m, creator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);
			exRate.JF_CFXPercent = 2m;
			Charge charge = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m);
			AssertEquals("Should have calculated local value", 100.00m, charge.JR_LocalCostAmt);
			AssertEquals("Should have calculated correct sell currency", creator.USD.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals("Should have calculated correct OS sell amount", 200.00m, charge.JR_OSSellAmt);
			AssertEquals("Should have calculated correct local sell amount", 102.04m, charge.JR_LocalSellAmt);
			AssertEquals("Should have calculated correct CFX", 2.04m, charge.JR_CFXAmt);

			Assert("Cost and Sell amounts should not be equal", charge.JR_LocalCostAmt != charge.JR_LocalSellAmt);
			AssertNoWarnings("Should not have any warning due to unequal Cost and Sell amounts", charge.JR_LocalCostAmtInfo);
			AssertNoWarnings("Should not have any warning due to unequal Cost and Sell amounts", charge.JR_LocalSellAmtInfo);
		}

		public void TestAPDatesAreNotMandatoryForCustomsDisbursementCharges()
		{
			using (RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetTemporaryValue(
						GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
						TestObjectCreator.CC1.PK.ToGuid()))
			{
				Factory.ClearCachedValue<ZGuid[]>("827eea6b-9769-4217-bc44-843e7b7b736d" + GlbBranch.CurrentBranch.PK);
				ChargeWithCostForTest charge = Factory.New<ChargeWithCostForTest>();
				charge.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_APInvoiceNum = "98744789";

				charge.JR_APInvoiceDate = ZDateTime.Today;
				charge.JR_PaymentDate = ZDateTime.Today;

				charge.JR_APInvoiceDate = ZDateTime.Empty;
				Assert("For Customs Disbursement Charges, AP invoice date should not be mandatory", !charge.JR_APInvoiceDateInfo.HasErrors());
				charge.JR_PaymentDate = ZDateTime.Empty;
				Assert("For Customs Disbursement Charges, Payment Date should not be mandatory", !charge.JR_PaymentDateInfo.HasErrors());

				charge.JR_APInvoiceDate = ZDateTime.Today;
				charge.JR_PaymentDate = ZDateTime.Today;

				charge.JR_AC = TestObjectCreator.CC2.PK;
				charge.JR_APInvoiceDate = ZDateTime.Empty;
				charge.JR_PaymentDate = ZDateTime.Empty;
				Assert("For non-Customs Disbursement Charges, AP invoice date should be mandatory", charge.JR_APInvoiceDateInfo.HasErrors());
				Assert("For non-Customs Disbursement Charges, Payment Date should be mandatory", charge.JR_PaymentDateInfo.HasErrors());
			}
		}

		public virtual void TestValidateJR_APInvoiceNum()
		{
			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();

			Assert("JR_APInvoiceNumInfo.ReadOnly must be initially true for this test", testCharge.JR_APInvoiceNumInfo.ReadOnly);
			testCharge.JR_APInvoiceNum = ZString.Empty;
			AssertNoErrors("Invoice number should not be compulsory when readonly", testCharge.JR_APInvoiceNumInfo);

			testCharge.JR_OH_CostAccount = ZGuid.Invalid;
			testCharge.JR_APInvoiceNum = "222";
			AssertHasErrors("Invoice num entered but Invalid creditor entered, should have errors", testCharge.JR_OH_CostAccountInfo);

			testCharge.JR_APInvoiceNum = ZString.Empty;
			AssertNoErrors("No invoice number entered, should not have errors", testCharge.JR_APInvoiceNumInfo);

			testCharge.JR_APInvoiceNum = "222";
			AssertHasErrors("Invoice num entered but Invalid creditor entered, should have errors", testCharge.JR_OH_CostAccountInfo);
			testCharge.JR_APInvoiceNum = ZString.Empty;
			AssertNoErrors("No invoice number entered, should not have errors", testCharge.JR_APInvoiceNumInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			testCharge.JR_AL_APLine = costLine.PK;
			testCharge.JR_APInvoiceNum = "222";
			AssertNoErrors("No errors because Cost posted", testCharge.JR_APInvoiceNumInfo);
		}

		public void TestValidateJR_APInvoiceNum_ValidationFor_BranchLevelPostingEnabled()
		{
			var expectedError = @"Please review the charge lines entered and ensure all charges for each Payables invoice have been assigned branch from the same Posting Group. All charges posted in one transaction must be within the same Branch Posting Group.
Posting is prevented because charges for the same Invoice number have been entered using a mix of branch Posting Groups.";

			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			foreach (var registryValue in new bool[] { true, false })
			{
				var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = registryValue };
				AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

				var testCharge1 = Factory.NewWithValidTestData<Charge>();
				var testCharge2 = Factory.NewWithValidTestData<Charge>();

				testCharge1.JR_OSCostAmt = 50M;
				testCharge2.JR_OSCostAmt = 50M;

				//both charge have same job
				testCharge1.JR_JH = TestObjectCreator.Job1.PK;
				testCharge2.JR_JH = TestObjectCreator.Job1.PK;

				//both charges have same creditor
				testCharge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				testCharge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

				//charges have different branches
				testCharge1.JR_GB = branch1.PK;
				testCharge2.JR_GB = branch2.PK;

				//both charges have same
				testCharge1.JR_APInvoiceNum = "111";
				testCharge2.JR_APInvoiceNum = "111";

				if (registryValue)
				{
					AssertHasError("testCharge2 will have error in JR_APInvoiceNum as both charges have same invoice number when they have different branches", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}
				else
				{
					AssertNoError("testCharge2 will not have error as registry is turned off", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}

				//both charges now have same branch
				testCharge2.JR_GB = branch1.PK;

				if (registryValue)
				{
					AssertNoError("testCharge2 no longer will have error as both the charges have same branch", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}
				else
				{
					AssertNoError("testCharge2 will not have error as registry is turned off", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}

				testCharge2.JR_GB = branch2.PK;

				if (registryValue)
				{
					AssertHasError("testCharge2 will have error in JR_APInvoiceNum as both charges have same invoice number when they have different branches", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}
				else
				{
					AssertNoError("testCharge2 will not have error as registry is turned off", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}

				testCharge2.JR_APInvoiceNum = "222";

				if (registryValue)
				{
					AssertNoError("testCharge2 no longer will have error as both the charges have different invoice number", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}
				else
				{
					AssertNoError("testCharge2 will not have error as registry is turned off", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}

				testCharge2.JR_APInvoiceNum = "111";

				if (registryValue)
				{
					AssertHasError("testCharge2 will have error in JR_APInvoiceNum as both charges have same invoice number when they have different branches", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}
				else
				{
					AssertNoError("testCharge2 will not have error as registry is turned off", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}

				branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
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

				testCharge2.RunPreSaveValidation();

				if (registryValue)
				{
					AssertNoError("testCharge2 no longer will have error as both the charges belong to branches in the same posting group", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}
				else
				{
					AssertNoError("testCharge2 will not have error as registry is turned off", testCharge2.JR_APInvoiceNumInfo, expectedError);
				}
			}
		}

		public void TestValidateJR_APInvoiceNum_OtherFieldsRevalidation()
		{
			fCharge1.JR_APInvoiceDate = ZDateTime.Now;
			fCharge1.JR_PaymentDate = ZDateTime.Now;
			fCharge1.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			fCharge1.JR_OSCostAmt = 100M;
			fCharge1.JR_AB = Factory.New<AccBankAccount>().PK;

			AssertNoErrors("Precondition: ", fCharge1.JR_APInvoiceDateInfo);
			AssertNoErrors("Precondition: ", fCharge1.JR_PaymentDateInfo);
			AssertNoErrors("Precondition: ", fCharge1.JR_ABInfo);
			AssertNoErrors("Precondition: ", fCharge1.JR_PaymentTypeInfo);

			fCharge1.JR_APInvoiceNum = "";

			AssertNoErrors(fCharge1.JR_APInvoiceDateInfo);
			AssertNoErrors(fCharge1.JR_PaymentDateInfo);
			AssertNoErrors(fCharge1.JR_ABInfo);
			AssertNoErrors(fCharge1.JR_PaymentTypeInfo);

			fCharge1.Validation.ValidateJR_APInvoiceDate();
			fCharge1.Validation.ValidateJR_PaymentDate();
			fCharge1.Validation.ValidateJR_AB();
			fCharge1.Validation.ValidateJR_PaymentType();

			AssertHasErrors(fCharge1.JR_APInvoiceDateInfo);
			AssertHasErrors(fCharge1.JR_PaymentDateInfo);
			AssertHasErrors(fCharge1.JR_ABInfo);
			AssertHasErrors(fCharge1.JR_PaymentTypeInfo);

			fCharge1.JR_APInvoiceNum = "INV NUMER 1";
			AssertNoErrors(fCharge1.JR_APInvoiceDateInfo);
			AssertNoErrors(fCharge1.JR_PaymentDateInfo);
			AssertNoErrors(fCharge1.JR_ABInfo);
			AssertNoErrors(fCharge1.JR_PaymentTypeInfo);
		}

		public void TestValidateJR_APInvoiceNum_InvoiceAmountSecurityWhenNoConfigurationExists()
		{
			Env.Security.APUnapprovedInvoices.IsAllowed = false;
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_JH = job.PK;
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OSCostAmt = 100;
			charge1.JR_APInvoiceNum = "INV";
			AssertNoWarnings("Invoice amount is less than Approval registry level", charge1.JR_APInvoiceNumInfo);
		}

		public void TestValidateJR_OSCostAmt()
		{
			Job job = Factory.NewJobForTesting<Job>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = true;
			org.CompanyData.OB_APCostsSelfBilled = false;
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_OH_CostAccount = org.PK;
			charge1.JR_OSCostAmt = -100m;
			AssertHasErrors(charge1.JR_OSCostAmtInfo);

			charge1.JR_APInvoiceDate = ZDateTime.Now;
			AssertHasErrors(charge1.JR_OSCostAmtInfo);

			charge1.JR_APInvoiceNum = "123";
			AssertNoErrors(charge1.JR_OSCostAmtInfo);

			charge1.JR_APInvoiceNum = "";
			AssertHasErrors(charge1.JR_OSCostAmtInfo);

			org.CompanyData.OB_APCostsSelfBilled = true;
			charge1.JR_OH_CostAccount = ZGuid.Empty;
			charge1.JR_OH_CostAccount = org.PK;
			AssertNoErrors(charge1.JR_OSCostAmtInfo);

			AccountingConfigurationRegistry.Instance.NegativeCostValidationEnforced.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			charge1.JR_OSCostAmt = -200m;
			AssertNoErrors(charge1.JR_OSCostAmtInfo);

			AccountingConfigurationRegistry.Instance.NegativeCostValidationEnforced.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			charge1.JR_OH_CostAccount = ZGuid.Empty;
			charge1.JR_OSCostAmt = -101m;
			AssertHasErrors(charge1.JR_OSCostAmtInfo);
			var costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			charge1.JR_AL_APLine = costLine.PK;
			charge1.JR_OSCostAmt = -102m;
			AssertNoErrors("No errors because Cost posted", charge1.JR_OSCostAmtInfo);
		}

		public void TestValidateJR_APInvoiceNum_InvoiceAmountSecurity()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			SetUpRegistryForTest();
			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
			try
			{
				Job job = Factory.NewJobForTesting<Job>();
				Charge charge1 = job.Charges.AddNew();
				charge1.JR_JH = job.PK;
				charge1.JR_AC = TestObjectCreator.CC1.PK;
				charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				charge1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
				charge1.JR_OSCostAmt = 100;
				charge1.JR_APInvoiceNum = "INV";
				AssertNoWarnings("Invoice amount is less than Approval registry level", charge1.JR_APInvoiceNumInfo);

				Charge charge2 = job.Charges.AddNew();
				charge2.JR_JH = job.PK;
				charge2.JR_AC = TestObjectCreator.CC2.PK;
				charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				charge2.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
				charge2.JR_OSCostAmt = 100;
				charge2.JR_APInvoiceNum = "INV";
				AssertNoWarnings("Invoice amount is less than Approval registry level", charge2.JR_APInvoiceNumInfo);

				Charge charge3 = job.Charges.AddNew();
				charge3.JR_JH = job.PK;
				charge3.JR_AC = TestObjectCreator.CC3.PK;
				charge3.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				charge3.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
				charge3.JR_OSCostAmt = 100;
				charge3.JR_APInvoiceNum = "INV";
				AssertHasWarnings("Invoice amount is more than Approval registry level", charge3.JR_APInvoiceNumInfo);

				charge1.Validation.ValidateAll();
				AssertHasWarnings("Invoice amount is more than Approval registry level", charge1.JR_APInvoiceNumInfo);
				charge2.Validation.ValidateAll();
				AssertHasWarnings("Invoice amount is more than Approval registry level", charge3.JR_APInvoiceNumInfo);

				charge1.JR_OSCostAmt = 20;
				charge1.Validation.ValidateAll();
				AssertNoWarnings("Invoice amount is less than Approval registry level", charge1.JR_APInvoiceNumInfo);
				charge2.Validation.ValidateAll();
				AssertNoWarnings("Invoice amount is less than Approval registry level", charge2.JR_APInvoiceNumInfo);
				charge3.Validation.ValidateAll();
				AssertNoWarnings("Invoice amount is less than Approval registry level", charge3.JR_APInvoiceNumInfo);
			}
			finally
			{
				ResetRegistryForTest();
			}
		}

		public void TestValidateJR_APInvoiceNum_UnapprovedInvoiceWithPaymentError()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			SetUpRegistryForTest();
			try
			{
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				Job job = Factory.NewJobForTesting<Job>();
				Charge charge = job.Charges.AddNew();
				charge.JR_JH = job.PK;
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				charge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
				charge.JR_OSCostAmt = 300;
				charge.JR_APInvoiceNum = "INV";
				AssertHasWarningContaining(charge.JR_APInvoiceNumInfo, "You do not have security rights");

				string expectedPartOfWarning = "Some of the charge lines that you are posting also contain Payment information.";
				charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.EFT;
				AssertHasWarningContaining(charge.JR_APInvoiceNumInfo, expectedPartOfWarning);

				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = true;
				charge.Validation.ValidateJR_APInvoiceNum();
				AssertNoWarningContaining(charge.JR_APInvoiceNumInfo, expectedPartOfWarning);

				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				charge.Validation.ValidateJR_APInvoiceNum();
				AssertHasWarningContaining(charge.JR_APInvoiceNumInfo, expectedPartOfWarning);
				charge.JR_OSCostAmt = 227;
				Assert("Precondition: charge total amount must be not bigger than approved level", charge.JR_OSCostAmtWithGSTAmt <= 250);
				charge.Validation.ValidateJR_APInvoiceNum();
				AssertNoWarningContaining(charge.JR_APInvoiceNumInfo, expectedPartOfWarning);
			}
			finally
			{
				ResetRegistryForTest();
			}
		}

		[TestDate(2017, 12, 1)]
		public virtual void TestRunAPInvoiceNumberExists_Standard()
		{
			AssertRunAPInvoiceNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths),
				"The transaction number is already in use. Last posted transaction’s invoice date is 01-Dec-17 which is at least 12 months apart. You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used.",
				"The transaction number is already in use. Last posted transaction’s invoice date is 01-Dec-17 which is at least 12 months apart. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.");
		}

		[TestDate(2017, 12, 1)]
		public virtual void TestRunAPInvoiceNumberExists_Calendar()
		{
			AssertRunAPInvoiceNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				new ZDateTime(ZDateTime.Today.Year + 1, 5, 6),
				"The transaction number is already in use. Last posted transaction’s invoice date is 01-Dec-17 which is in another calendar year. You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used.",
				"The transaction number is already in use. Last posted transaction’s invoice date is 01-Dec-17 which is in another calendar year. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.");
		}

		void AssertRunAPInvoiceNumberExists(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2, string expErrorMessageWithPermission, string expErrorMessageNoPermission)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string existingAPInvoiceNum = "TESTAP5787";
				InvoicingBase existingInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as InvoicingBase;
				existingInvoice.AH_TransactionNum = existingAPInvoiceNum;
				existingInvoice.AH_OH = fCreditor.PK;
				existingInvoice.AH_InvoiceDate = invoiceDate;

				Factory.Save();

				ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();
				testCharge.JR_OH_CostAccount = fCreditor.PK;
				testCharge.JR_APInvoiceNum = existingAPInvoiceNum;
				AssertHasError(testCharge.JR_APInvoiceNumInfo, "The transaction number is already in use. Cannot check if the duplicate transaction number is allowed as the Invoice Date is empty.");

				testCharge.JR_APInvoiceDate = invoiceDate2;
				testCharge.Validation.ValidateJR_APInvoiceNum();
				AssertHasWarning(testCharge.JR_APInvoiceNumInfo, expErrorMessageWithPermission);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				testCharge.Validation.ValidateJR_APInvoiceNum();
				AssertHasError(testCharge.JR_APInvoiceNumInfo, expErrorMessageNoPermission);
			}
		}

		[TestDate(2017, 12, 1)]
		public virtual void TestRunUAInvoiceNumberExists_Standard()
		{
			AssertRunUAInvoiceNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths),
				"The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 01-Dec-17 which is at least 12 months apart. You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used.",
				"The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 01-Dec-17 which is at least 12 months apart. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.");
		}

		[TestDate(2017, 12, 1)]
		public virtual void TestRunUAInvoiceNumberExists_Calendar()
		{
			AssertRunUAInvoiceNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				new ZDateTime(ZDateTime.Today.Year + 1, 5, 6),
				"The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 01-Dec-17 which is in another calendar year. You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used.",
				"The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 01-Dec-17 which is in another calendar year. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.");
		}

		void AssertRunUAInvoiceNumberExists(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, string expInvoiceMessageWithPermission, string expErrorMessageNoPermission)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string existingAPInvoiceNum = "TESTAP5787";
				InvoicingBase existingInvoice = Factory.NewWithValidTestData(typeof(UAInvoice)) as InvoicingBase;
				existingInvoice.AH_TransactionNum = existingAPInvoiceNum;
				existingInvoice.AH_OH = fCreditor.PK;

				Factory.Save();

				ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();
				testCharge.JR_OH_CostAccount = fCreditor.PK;
				testCharge.JR_APInvoiceNum = existingAPInvoiceNum;
				AssertHasError(testCharge.JR_APInvoiceNumInfo, "The transaction number is already in use by Unapproved Invoice. Cannot check if the duplicate transaction number is allowed as the Invoice Date is empty.");

				testCharge.JR_APInvoiceDate = invoiceDate;
				testCharge.Validation.ValidateJR_APInvoiceNum();
				AssertHasWarning(testCharge.JR_APInvoiceNumInfo, expInvoiceMessageWithPermission);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				testCharge.Validation.ValidateJR_APInvoiceNum();
				AssertHasError(testCharge.JR_APInvoiceNumInfo, expErrorMessageNoPermission);
			}
		}

		public abstract void TestIsInvoiceNumberApplicable();

		public override void TestValidateJR_AB()
		{
			base.TestValidateJR_AB();

			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();

			Assert("JR_ABInfo.ReadOnly must be initially true for this test", testCharge.JR_ABInfo.ReadOnly);
			testCharge.JR_AB = ZGuid.Empty;
			AssertNoErrors("Bank Account should not be compulsory when readonly", testCharge.JR_ABInfo);

			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_APInvoiceNum = "333";
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			testCharge.Validation.ValidateJR_AB();

			AssertHasErrors("Creditor, Inv. Num, Payment type entered: Bank account should be mandatory", testCharge.JR_ABInfo);
			testCharge.JR_AB = bankAccount.PK;
			AssertNoErrors("Bank account should be entered", testCharge.JR_ABInfo);

			testCharge.JR_PaymentType = ZString.Empty;
			testCharge.JR_AB = ZGuid.Empty;
			AssertNoErrors("Not all details entered, Bank account should NOT be mandatory", testCharge.JR_ABInfo);
			testCharge.JR_AB = bankAccount.PK;
			AssertHasErrors("Payment type not entered, bank account should have an error when entered", testCharge.JR_ABInfo);

			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			testCharge.JR_APInvoiceNum = ZString.Empty;
			testCharge.JR_AB = ZGuid.Empty;
			AssertNoErrors("Not all details entered, Bank account should NOT be mandatory", testCharge.JR_ABInfo);
			testCharge.JR_AB = bankAccount.PK;
			AssertHasErrors("Invoice number not entered, bank account should have an error when entered", testCharge.JR_ABInfo);
			testCharge.CostAccount.CompanyData.OB_APCostsSelfBilled = true;
			testCharge.JR_AB = ZGuid.Empty;
			testCharge.JR_AB = bankAccount.PK;
			AssertNoErrors("Invoice Num not entered, but Cost Account flagged as Self Billing and bank account should not have an error when entered", testCharge.JR_ABInfo);
			testCharge.CostAccount.CompanyData.OB_APCostsSelfBilled = false;

			testCharge.JR_OH_CostAccount = ZGuid.Empty;
			testCharge.JR_APInvoiceNum = "333";
			testCharge.JR_AB = ZGuid.Empty;
			AssertNoErrors("Not all details entered, Bank account should NOT be mandatory", testCharge.JR_ABInfo);
			testCharge.JR_AB = bankAccount.PK;
			AssertHasErrors("Creditor not entered, bank account should have an error when entered", testCharge.JR_ABInfo);

			testCharge.JR_AB = ZGuid.Empty;
			AssertNoErrors("Not all details entered, Bank account should NOT be mandatory", testCharge.JR_ABInfo);
			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			testCharge.JR_AL_APLine = costLine.PK;
			testCharge.JR_AB = ZGuid.NewZGuid();
			AssertNoErrors("No errors because Cost posted", testCharge.JR_ABInfo);
		}

		public void TestValidateJR_ABForCorrectCurrency()
		{
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.AALSHI, 0M, TestObjectCreator.ABIGAS, 0M);

			Charge charge = job.Charges.AddNew();
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_APInvoiceNum = "333";
			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;

			charge = job.Charges.AddNew();
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_APInvoiceNum = "333";
			charge.JR_RX_NKCostCurrency = TestObjectCreator.GBP.RX_Code;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			charge.JR_AB = TestObjectCreator.GBPBankAccount.PK;
			AssertHasError(charge.JR_ABInfo, "Bank Account currency is incorrect. Choose AUD currency Bank Account.");

			charge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertNoError(charge.JR_ABInfo, "Bank Account currency is incorrect. Choose AUD currency Bank Account.");
		}

		public override void TestValidateJR_AK()
		{
			base.TestValidateJR_AK();

			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_Code = "PPP";
			chequeBook.AK_AB = bankAccount.PK;

			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();

			Assert("JR_AKInfo.ReadOnly must be initially true for this test", testCharge.JR_AKInfo.ReadOnly);
			testCharge.JR_AK = ZGuid.Empty;
			AssertNoErrors("Cheque book should not be compulsory when readonly", testCharge.JR_AKInfo);

			testCharge.JR_AB = bankAccount.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testCharge.JR_AK = ZGuid.Empty;
			testCharge.ChequeBooks.Add(chequeBook);
			AssertHasErrors("Payment type is cheque, charge code is entered, cheque book should be mandatory", testCharge.JR_AKInfo);

			testCharge.JR_AK = chequeBook.PK;
			AssertNoErrors("Cheque book entered, should not have errors", testCharge.JR_AKInfo);

			testCharge.JR_AC = ZGuid.Empty;
			testCharge.JR_AK = ZGuid.Empty;
			testCharge.ChequeBooks.Add(chequeBook);
			AssertNoErrors("Charge code not entered, cheque book should not be mandatory", testCharge.JR_AKInfo);
			testCharge.JR_AK = chequeBook.PK;
			testCharge.ChequeBooks.Add(chequeBook);
			AssertHasErrors("Charge code not entered, cheque book should have an error when entered", testCharge.JR_AKInfo);

			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			testCharge.JR_AK = ZGuid.Empty;
			testCharge.ChequeBooks.Add(chequeBook);
			AssertNoErrors("Payment type not cheque, cheque book should not be mandatory", testCharge.JR_AKInfo);
			testCharge.JR_AK = chequeBook.PK;
			testCharge.ChequeBooks.Add(chequeBook);
			AssertNoErrors("Payment type not cheque, cheque book should have an error when entered", testCharge.JR_AKInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			testCharge.JR_AL_APLine = costLine.PK;
			testCharge.JR_AC = ZGuid.Empty;
			testCharge.JR_AK = ZGuid.Empty;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testCharge.JR_AK = chequeBook.PK;
			testCharge.ChequeBooks.Add(chequeBook);
			AssertNoErrors("No error because Cost posted", testCharge.JR_AKInfo);
		}

		public virtual void TestValidateJR_AKWithPostedCharge()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_Code = "PPP";
			chequeBook.AK_AB = bankAccount.PK;
			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();
			testCharge.JR_AB = bankAccount.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testCharge.JR_AK = ZGuid.Empty;
			testCharge.ChequeBooks.Add(chequeBook);
			testCharge.JR_AK = chequeBook.PK;

			chequeBook.AK_IsActive = false;
			testCharge.Validation.ValidateJR_AK();
			AssertHasErrors("Cheque book entered, should not have errors", testCharge.JR_AKInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			testCharge.JR_AL_APLine = costLine.PK;
			testCharge.Validation.ValidateJR_AK();
			AssertNoErrors("Cheque book entered, should not have errors", testCharge.JR_AKInfo);
		}

		public virtual void TestValidateJR_PaymentType()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();

			Assert("JR_PaymentTypeInfo.ReadOnly must be initially true for this test", testCharge.JR_PaymentTypeInfo.ReadOnly);
			testCharge.JR_PaymentType = ZString.Empty;
			AssertNoErrors("PaymentType should not be compulsory when readonly", testCharge.JR_PaymentTypeInfo);

			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_APInvoiceNum = "555";
			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertNoErrors("Charge code, Invoice num and creditor entered, payment type may be entered", testCharge.JR_PaymentTypeInfo);

			testCharge.JR_PaymentType = "XYZ";
			AssertHasErrors("Payment type should not be valid", testCharge.JR_PaymentTypeInfo);

			testCharge.JR_AC = ZGuid.Empty;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertHasErrors("Charge code not entered, payment type should have errors when entered", testCharge.JR_PaymentTypeInfo);

			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertNoErrors("Charge code, Invoice num and creditor entered, payment type may be entered", testCharge.JR_PaymentTypeInfo);

			testCharge.JR_APInvoiceNum = ZString.Empty;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertHasErrors("Invoice Num not entered, payment type should have errors when entered", testCharge.JR_PaymentTypeInfo);

			testCharge.CostAccount.CompanyData.OB_APCostsSelfBilled = true;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.EFT;
			AssertNoErrors("Invoice Num not entered, but Cost Account flagged as Self Billing and payment type should not have errors when entered", testCharge.JR_PaymentTypeInfo);
			fCreditor.CompanyData.OB_APCostsSelfBilled = false;

			testCharge.JR_APInvoiceNum = "555";
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertNoErrors("Charge code, Invoice num and creditor entered, payment type may be entered", testCharge.JR_PaymentTypeInfo);

			testCharge.JR_APInvoiceNum = "555";
			testCharge.JR_OH_CostAccount = ZGuid.Empty;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertHasErrors("Creditor not entered, payment type should have errors when entered", testCharge.JR_PaymentTypeInfo);

			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertHasErrors("Creditor not entered, payment type should have errors when entered", testCharge.JR_PaymentTypeInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			testCharge.JR_AL_APLine = costLine.PK;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertNoErrors("No errors because Cost posted", testCharge.JR_PaymentTypeInfo);
		}

		public void TestCheckJR_PaymentTypeSecurity()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_APInvoiceNum = "555";
			testCharge.JR_OH_CostAccount = fCreditor.PK;

			Env.Security.APPaymentProcessingNewCheque.IsAllowed = Env.Security.APPaymentProcessingNewCash.IsAllowed = Env.Security.APPaymentProcessingNewCreditCard.IsAllowed = Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed = Env.Security.APPaymentProcessingNewEFT.IsAllowed = Env.Security.APPaymentProcessingNewSFT.IsAllowed = Env.Security.APPaymentProcessingNewCRQ.IsAllowed = false;

			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			testCharge.JR_PaymentType = ReceiptTypes.Cheque;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			testCharge.JR_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.Cheque;
			AssertHasError(testCharge.JR_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCash.IsAllowed = true;
			testCharge.JR_PaymentType = ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			testCharge.JR_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCash.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.Cash;
			AssertHasError(testCharge.JR_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = true;
			testCharge.JR_PaymentType = ReceiptTypes.CreditCard;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			testCharge.JR_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.CreditCard;
			AssertHasError(testCharge.JR_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = true;
			testCharge.JR_PaymentType = ReceiptTypes.DirectDebit;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			testCharge.JR_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.DirectDebit;
			AssertHasError(testCharge.JR_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentEFT.IsAllowed = true;
			testCharge.JR_PaymentType = ReceiptTypes.EFT;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			testCharge.JR_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentEFT.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.EFT;
			AssertHasError(testCharge.JR_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentSFT.IsAllowed = true;
			testCharge.JR_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			testCharge.JR_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentSFT.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.ScheduledEFT;
			AssertHasError(testCharge.JR_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCRQ.IsAllowed = true;
			testCharge.JR_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			testCharge.JR_PaymentType = ZString.Empty;
			Assert("Receipt Type should have no errors", !testCharge.JR_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCRQ.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.CollectionRequest;
			AssertHasError(testCharge.JR_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");
		}

		public virtual void TestValidateJR_ChequeNo()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			ChargeWithCostForTest testCharge = Factory.New<ChargeWithCostForTest>();
			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			testCharge.JR_AC = chargeCode.PK;

			testCharge.JR_ChequeNo = ZString.Empty;
			AssertHasErrors("Charge code entered, Creditor entered, payment type entered, Cheque No is mandatory", testCharge.JR_ChequeNoInfo);

			testCharge.JR_ChequeNo = "222.5";
			AssertHasErrors("Cheque no must be an integer", testCharge.JR_ChequeNoInfo);

			testCharge.JR_ChequeNo = "222";
			AssertNoErrors("Cheque no is entered, should not have errors", testCharge.JR_ChequeNoInfo);

			testCharge.JR_PaymentType = ReceiptTypes.Cheque;
			testCharge.JR_ChequeNo = "111111111111111111111111111111";
			AssertHasErrors("Cheque no should have an error for being too large instead of throwing an exception", testCharge.JR_ChequeNoInfo);

			testCharge.JR_OH_CostAccount = ZGuid.Empty;
			testCharge.JR_ChequeNo = ZString.Empty;
			AssertNoErrors("Creditor not entered, Cheque No should not be mandatory", testCharge.JR_ChequeNoInfo);
			testCharge.JR_ChequeNo = "111";
			AssertHasErrors("Creditor not entered, Cheque No should have an error when entered", testCharge.JR_ChequeNoInfo);

			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_PaymentType = ZString.Empty;
			testCharge.JR_ChequeNo = ZString.Empty;
			AssertNoErrors("Payment type not entered, Cheque No should not be mandatory", testCharge.JR_ChequeNoInfo);
			testCharge.JR_ChequeNo = "111";
			AssertHasErrors("Payment type not entered, Cheque No should have an error when entered", testCharge.JR_ChequeNoInfo);

			testCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			testCharge.JR_AC = ZGuid.Empty;
			testCharge.JR_ChequeNo = ZString.Empty;
			AssertNoErrors("Charge code not entered, Cheque No should not be mandatory", testCharge.JR_ChequeNoInfo);
			testCharge.JR_ChequeNo = "111";
			AssertHasErrors("Charge code not entered, Cheque No should have an error when entered", testCharge.JR_ChequeNoInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			testCharge.JR_AL_APLine = costLine.PK;
			testCharge.JR_ChequeNo = "123";
			AssertNoErrors("No errors because Cost posted", testCharge.JR_ChequeNoInfo);
		}

		public void TestValidateJR_AC()
		{
			var testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var chargeCode = Factory.New<AccChargeCode>();

			var normalMessage = @"There is more than one charge using this charge code for the same Sell Reference Number: {0}.
You are advised to review these charges before posting.";
			var gatewayMessage = @"There is more than one charge using this charge code for the same Related Job Number: {0}.
Please confirm that the selection is valid.";

			var newCharge = testJob.Charges.AddNew();
			var aCharge = testJob.Charges.AddNew();
			aCharge.JR_AC = chargeCode.PK;
			newCharge.JR_AC = chargeCode.PK;

			aCharge.Validation.ValidateJR_AC();
			newCharge.Validation.ValidateJR_AC();

			AssertHasWarning(newCharge.JR_ACInfo, string.Format(normalMessage, "<empty>"));
			AssertHasWarning(aCharge.JR_ACInfo, string.Format(normalMessage, "<empty>"));

			aCharge.JR_SellReference = "CON1010";
			aCharge.Validation.ValidateJR_AC();
			newCharge.Validation.ValidateJR_AC();
			AssertNoWarnings("Only show warning when Sell Reference Numbers are equal", newCharge.JR_ACInfo);
			AssertNoWarnings("Only show warning when Sell Reference Numbers are equal", aCharge.JR_ACInfo);

			newCharge.JR_SellReference = "CON1010";
			aCharge.Validation.ValidateJR_AC();
			newCharge.Validation.ValidateJR_AC();
			AssertHasWarning(newCharge.JR_ACInfo, string.Format(normalMessage, "CON1010"));
			AssertHasWarning(aCharge.JR_ACInfo, string.Format(normalMessage, "CON1010"));

			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJob = TestObjectCreator.CreateJob(consol, false);
			consolJob.Charges.Add(aCharge);
			consolJob.Charges.Add(newCharge);
			aCharge.JR_JH = consolJob.PK;
			newCharge.JR_JH = consolJob.PK;

			AssertEquals("Precondition: charges still have same charge code", chargeCode.PK, newCharge.JR_AC);
			AssertEquals("Precondition: charges still have same charge code", chargeCode.PK, aCharge.JR_AC);

			aCharge.Validation.ValidateJR_AC();
			newCharge.Validation.ValidateJR_AC();
			AssertHasWarning(newCharge.JR_ACInfo, string.Format(gatewayMessage, "<empty>"));
			AssertHasWarning(aCharge.JR_ACInfo, string.Format(gatewayMessage, "<empty>"));

			newCharge.JR_Calc_RelatedJobNumber = "123456";
			aCharge.Validation.ValidateJR_AC();
			newCharge.Validation.ValidateJR_AC();
			AssertNoWarnings("Only show warning when related jobs are equal", newCharge.JR_ACInfo);
			AssertNoWarnings("Only show warning when related jobs are equal", aCharge.JR_ACInfo);

			aCharge.JR_Calc_RelatedJobNumber = "123456";
			aCharge.Validation.ValidateJR_AC();
			newCharge.Validation.ValidateJR_AC();
			AssertHasWarning(newCharge.JR_ACInfo, string.Format(gatewayMessage, "123456"));
			AssertHasWarning(aCharge.JR_ACInfo, string.Format(gatewayMessage, "123456"));
		}

		public void TestChequeNumberInChequeBookRange()
		{
			fCharge1.JR_ChequeNo = "99";
			AssertEquals("Cheque No. not in Cheque Book Range", true, fCharge1.JR_ChequeNoInfo.HasErrors());
			fCharge1.JR_ChequeNo = "201";
			AssertEquals("Cheque No. not in Cheque Book Range", true, fCharge1.JR_ChequeNoInfo.HasErrors());
			fCharge1.JR_ChequeNo = "100";
			AssertEquals("Cheque No. in Cheque Book Range", false, fCharge1.JR_ChequeNoInfo.HasErrors());
		}

		public void TestHasChequeBeenAlreadyUsedAPPayment()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APPayment testPayment = newFactory.NewWithValidTestData<APPayment>();
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AH_AB = fBank.PK;
			testPayment.ChequeBook = fChequeBook.PK;
			testPayment.AH_ChequeOrReference = "101";
			newFactory.Save();

			fCharge1.JR_ChequeNo = "101";
			AssertEquals("Has Cheque been already used", true, fCharge1.JR_ChequeNoInfo.HasErrors());
		}

		public void TestCancelledCheque()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APPayment cancelledPayment = newFactory.NewWithValidTestData<APPayment>();
			cancelledPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cancelledPayment.AH_AB = fBank.PK;
			cancelledPayment.ChequeBook = fChequeBook.PK;
			cancelledPayment.AH_ChequeOrReference = "102";
			cancelledPayment.AH_IsCancelled = true;
			((IMatching)cancelledPayment).CurrentMatchGroup.AddNew().AP_AH = cancelledPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(cancelledPayment);
			newFactory.Save();

			fCharge1.JR_ChequeNo = "102";
			AssertEquals("Cancelled Cheque", false, fCharge1.JR_ChequeNoInfo.HasErrors());
		}

		public void TestHasChequeBeenAlreadyUsed()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_RX_NKCurrency = fCurrency.RX_Code;
			cost.E6_OSCostAmount = 100m;
			cost.E6_LocalCostAmount = 100m;
			cost.E6_InvoiceNum = "INV2";
			cost.E6_InvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			cost.E6_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			cost.E6_OH_Creditor = fCreditor2.PK;

			Charge charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_E6 = cost.PK;
			charge2.JR_JH = job.PK;
			charge2.JR_AC = fChargeCode.PK;
			charge2.JR_OH_CostAccount = fCreditor2.PK;
			charge2.JR_RX_NKCostCurrency = fCurrency.RX_Code;
			charge2.JR_OSCostAmt = 100M;
			charge2.JR_LocalCostAmt = 100m;
			charge2.JR_APInvoiceNum = "INV2";
			charge2.JR_APInvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge2.JR_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_AB = fBank.PK;
			charge2.JR_AK = fChequeBook.PK;
			charge2.JR_ChequeNo = "102";
			Factory.Save();

			fCharge1.JR_ChequeNo = "102";
			AssertHasError(fCharge1.JR_ChequeNoInfo, "This check number is already used on Consol " + consol.JK_UniqueConsignRef + ".");

			charge2.JR_E6 = ZGuid.Empty;
			Factory.Save();

			fCharge1.JR_ChequeNo = "";
			fCharge1.JR_ChequeNo = "102";
			AssertHasError(fCharge1.JR_ChequeNoInfo, "This check number is already used on Job " + job.JH_JobNum + ".");
		}

		public void TestValidateChequeNumberDigits()
		{
			fCharge1.JR_ChequeNo = "123456";
			AssertEquals("Invalid Cheque Number digits", true, fCharge1.JR_ChequeNoInfo.HasErrors());

			fCharge1.JR_ChequeNo = "A999";
			AssertHasError(fCharge1.JR_ChequeNoInfo, "Only numbers are allowed in this field.");

			fCharge1.JR_ChequeNo = ((decimal)int.MaxValue + 1).ToString();
			AssertHasError(fCharge1.JR_ChequeNoInfo, @"Check number 2147483648 is not contained in the selected check book.
The check number must be between 000100 and 000200.");
		}

		public void TestValidateMiddleChequeNumber()
		{
			fCharge1.JR_ChequeNo = "105";
			AssertEquals("Valid Cheque Number", false, fCharge1.JR_ChequeNoInfo.HasErrors());

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			Charge charge2 = newFactory.NewWithValidTestData<Charge>();
			charge2.JR_AC = fChargeCode.PK;
			charge2.JR_OH_CostAccount = fCreditor2.PK;
			charge2.JR_RX_NKCostCurrency = fCurrency.RX_Code;
			charge2.JR_OSCostAmt = 100M;
			charge2.JR_APInvoiceNum = "INV2";
			charge2.JR_APInvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge2.JR_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_AB = fBank.PK;
			charge2.JR_AK = fChequeBook.PK;
			charge2.JR_ChequeNo = "107";
			AssertEquals("Valid cheque number", false, charge2.JR_ChequeNoInfo.HasErrors());

			Charge charge3 = newFactory.NewWithValidTestData<Charge>();
			charge3.JR_AC = fChargeCode.PK;
			charge3.JR_OH_CostAccount = fCreditor2.PK;
			charge3.JR_RX_NKCostCurrency = fCurrency.RX_Code;
			charge3.JR_OSCostAmt = 100M;
			charge3.JR_APInvoiceNum = "INV3";
			charge3.JR_APInvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge3.JR_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge3.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge3.JR_AB = fBank.PK;
			charge3.JR_AK = fChequeBook.PK;
			charge3.JR_ChequeNo = "106";
			AssertEquals("Valid cheque number", false, charge3.JR_ChequeNoInfo.HasErrors());
			newFactory.Save();

			Charge charge4 = newFactory.NewWithValidTestData<Charge>();
			charge4.JR_AC = fChargeCode.PK;
			charge4.JR_OH_CostAccount = fCreditor2.PK;
			charge4.JR_RX_NKCostCurrency = fCurrency.RX_Code;
			charge4.JR_OSCostAmt = 100M;
			charge4.JR_APInvoiceNum = "INV4";
			charge4.JR_APInvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge4.JR_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge4.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge4.JR_AB = fBank.PK;
			charge4.JR_AK = fChequeBook.PK;
			charge4.JR_ChequeNo = "107";
			AssertEquals("Invalid cheque number", true, charge4.JR_ChequeNoInfo.HasErrors());
		}

		public void TestRunCostCurrencyValidation()
		{
			OrgHeader costAccount = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			APInvoiceLine costLine = (APInvoiceLine)Factory.NewWithValidTestData<APInvoice>().Lines.AddNew();
			costLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			Charge postedCharge = fJob.Charges.AddNew();
			postedCharge.JR_OH_CostAccount = costAccount.PK;
			postedCharge.JR_APInvoiceNum = "120";
			postedCharge.JR_RX_NKCostCurrency = currency.RX_Code;
			postedCharge.JR_AL_APLine = costLine.PK;

			Charge unpostedCharge = fJob.Charges.AddNew();
			unpostedCharge.JR_OH_CostAccount = costAccount.PK;
			unpostedCharge.JR_APInvoiceNum = "120";
			unpostedCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertNoErrors(postedCharge.JR_RX_NKCostCurrencyInfo);
			postedCharge.Validation.ValidateJR_RX_NKCostCurrency();
			AssertNoErrors(postedCharge.JR_RX_NKCostCurrencyInfo);

			AssertNoErrors(unpostedCharge.JR_RX_NKCostCurrencyInfo);
			unpostedCharge.Validation.ValidateJR_RX_NKCostCurrency();
			AssertNoErrors(unpostedCharge.JR_RX_NKCostCurrencyInfo);

			postedCharge.JR_RX_NKCostCurrency = "$$$";
			AssertNoErrors(unpostedCharge.JR_RX_NKCostCurrencyInfo);
			unpostedCharge.Validation.ValidateJR_RX_NKCostCurrency();
			AssertNoErrors(unpostedCharge.JR_RX_NKCostCurrencyInfo);

			Charge unpostedCharge2 = fJob.Charges.AddNew();
			unpostedCharge2.JR_OH_CostAccount = costAccount.PK;
			unpostedCharge2.JR_APInvoiceNum = "120";
			unpostedCharge2.JR_RX_NKCostCurrency = currency.RX_Code;

			AssertNoErrors(unpostedCharge2.JR_RX_NKCostCurrencyInfo);
			AssertHasWarning(unpostedCharge2.JR_RX_NKCostCurrencyInfo, @"This cost will be posted on a local currency Payables Invoice.
A mix of Cost Currencies have recorded for this Creditor and Invoice Number. Because of this, they will be posted as a local currency payables transaction.");
		}

		public void TestRunCostExchangeRateValidation()
		{
			OrgHeader costAccount = Factory.NewWithValidTestData<OrgHeader>();
			APInvoiceLine costLine = (APInvoiceLine)Factory.NewWithValidTestData<APInvoice>().Lines.AddNew();
			costLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			Charge postedCharge = fJob.Charges.AddNew();
			postedCharge.JR_OH_CostAccount = costAccount.PK;
			postedCharge.JR_RX_NKCostCurrency = "USD";
			postedCharge.JR_APInvoiceNum = "333";
			postedCharge.JR_OSCostExRate = 0.389m;
			postedCharge.JR_AL_APLine = costLine.PK;

			Charge unpostedCharge = fJob.Charges.AddNew();
			unpostedCharge.JR_OH_CostAccount = costAccount.PK;
			unpostedCharge.JR_RX_NKCostCurrency = "USD";
			unpostedCharge.JR_APInvoiceNum = "333";
			unpostedCharge.JR_OSCostExRate = 0.390m;

			AssertNoErrors(postedCharge.JR_OSCostExRateInfo);
			postedCharge.Validation.ValidateJR_OSCostExRate();
			AssertNoErrors(postedCharge.JR_OSCostExRateInfo);

			AssertNoErrors(unpostedCharge.JR_OSCostExRateInfo);
			unpostedCharge.Validation.ValidateJR_OSCostExRate();
			AssertNoErrors(unpostedCharge.JR_OSCostExRateInfo);

			Charge unpostedCharge2 = fJob.Charges.AddNew();
			unpostedCharge2.JR_OH_CostAccount = costAccount.PK;
			unpostedCharge2.JR_RX_NKCostCurrency = "USD";
			unpostedCharge2.JR_APInvoiceNum = "333";
			unpostedCharge2.JR_OSCostExRate = 0.389m;

			AssertNoErrors(unpostedCharge2.JR_OSCostExRateInfo);
		}

		public void TestJR_ChequeNoValidationIsDisabledForAutoAllocationMode()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook(testBank);

			Charge charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_AC = fChargeCode.PK;
			charge2.JR_OH_CostAccount = fCreditor2.PK;
			charge2.JR_RX_NKCostCurrency = fCurrency.RX_Code;
			charge2.JR_OSCostAmt = 100M;
			charge2.JR_APInvoiceNum = "INV2";
			charge2.JR_APInvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge2.JR_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_AB = testChequeBook.AK_AB;
			charge2.JR_AK = testChequeBook.PK;
			charge2.JR_ChequeNo = "BLAH!";
			AssertHasErrors("Cheque Number should have an error", charge2.JR_ChequeNoInfo);

			charge2.JR_AB = autoPrintChequeBook.AK_AB;
			charge2.JR_AK = autoPrintChequeBook.PK;
			charge2.JR_ChequeNo = "BLAH!";
			AssertNoErrors("Should not have any errors as now in autoallocation mode", charge2.JR_ChequeNoInfo);
		}

		public void TestJR_AKHasAutoAllocationValidation()
		{
			AccChequeBook newChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			newChequeBook.AK_AB = testBank.PK;
			Charge charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_AB = testBank.PK;
			charge2.JR_AK = newChequeBook.PK;

			Assert("Should be no errors so far", !charge2.JR_AKInfo.HasErrors());

			newChequeBook.AK_IsActive = ZBool.False;
			charge2.JR_AK = ZGuid.Empty;
			charge2.JR_AK = newChequeBook.PK;
			AssertHasError(charge2.JR_AKInfo, AccChequeBookAutoAllocationValidation.ChequeBookIsInActiveMessage);
		}

		public void TestTwoChargeWithDifferentCurrencyAndSameInvoiceNum()
		{
			Job job = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);
			Factory.Save();
			Charge charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc1", TestObjectCreator.AUD, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000m, TestObjectCreator.ABIGAS);
			Charge charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc1", TestObjectCreator.AUD, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000m, TestObjectCreator.ABIGAS);
			charge1.JR_APInvoiceNum = charge2.JR_APInvoiceNum = "987654321";
			charge1.JR_APInvoiceDate = charge2.JR_APInvoiceDate = ZDate.Today;
			Factory.Save();

			AssertNoErrors(charge1.JR_RX_NKCostCurrencyInfo);

			charge1.JR_APInvoiceNum = "98765432";
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			AssertNoErrors(charge1.JR_RX_NKCostCurrencyInfo);

			charge1.JR_APInvoiceNum = "987654321";
			AssertNoErrors(charge1.JR_RX_NKCostCurrencyInfo);
			AssertHasWarnings(charge1.JR_RX_NKCostCurrencyInfo);
		}

		public void TestAPCreditNoteWarning()
		{
			Job job = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			Charge charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Charge Code 1", TestObjectCreator.AUD, -100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, -100M, TestObjectCreator.ABIGAS);

			charge1.JR_APInvoiceNum = "1";
			charge1.JR_APInvoiceDate = ZDateTime.Today;
			charge1.JR_PaymentType = "CSH";
			charge1.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			charge1.JR_ChequeNo = "CASH";

			job.RunPreSaveValidation();
			Assert("JR_OSCostAmt should have warning", charge1.JR_OSCostAmtInfo.HasWarning("A negative invoice with bank details may create an AP Credit note, If Posting this charge creates an AP Credit Note, Bank details will be ignored"));
			Assert("JR_PaymentType should have warning", charge1.JR_PaymentTypeInfo.HasWarning("A negative invoice with bank details may create an AP Credit note, If Posting this charge creates an AP Credit Note, Bank details will be ignored"));
			Factory.Save();
		}

		public void TestInactiveBranchesNotPermittedInNewCharges()
		{
			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Job job = TestObjectCreator.InsertJobHeader(testBranch.PK, Env.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);
			Factory.Save();

			Charge charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc1", TestObjectCreator.AUD, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000m, TestObjectCreator.ABIGAS);
			charge1.JR_GB = testBranch.PK;
			Factory.Save();
			AssertNoErrors("New charge, branch active", charge1.JR_GBInfo);

			Factory.Save();
			charge1.Reload();
			AssertNoErrors("Saved charge, branch active", charge1.JR_GBInfo);

			testBranch.GB_IsActive = false;
			Factory.Save();
			charge1.Reload();
			AssertNoErrors("Saved charge, branch inactive", charge1.JR_GBInfo);

			Charge charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc1", TestObjectCreator.AUD, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000m, TestObjectCreator.ABIGAS);
			charge2.JR_GB = testBranch.PK;
			AssertHasError("New charge, branch inactive", charge2.JR_GBInfo, string.Format("This {0} is inactive - it may not be used.", charge2.JR_GBInfo.Description));
		}

		public override void TestValidateJR_OH_CostAccount()
		{
			base.TestValidateJR_OH_CostAccount();

			fCharge1.JR_APInvoiceDate = ZDateTime.Now;
			fCharge1.JR_PaymentDate = ZDateTime.Now;
			fCharge1.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			fCharge1.JR_OSCostAmt = 100M;
			fCharge1.JR_AB = Factory.New<AccBankAccount>().PK;

			AssertNoErrors("Precondition: ", fCharge1.JR_APInvoiceNumInfo);
			AssertNoErrors("Precondition: ", fCharge1.JR_APInvoiceDateInfo);
			AssertNoErrors("Precondition: ", fCharge1.JR_PaymentDateInfo);
			AssertNoErrors("Precondition: ", fCharge1.JR_ABInfo);
			AssertNoErrors("Precondition: ", fCharge1.JR_PaymentTypeInfo);
			AssertNoErrors("Precondition: ", fCharge1.JR_ChequeNoInfo);

			fCharge1.JR_OH_CostAccount = ZGuid.Empty;

			AssertNoErrors(fCharge1.JR_APInvoiceNumInfo);
			AssertNoErrors(fCharge1.JR_APInvoiceDateInfo);
			AssertNoErrors(fCharge1.JR_PaymentDateInfo);
			AssertNoErrors(fCharge1.JR_ABInfo);
			AssertNoErrors(fCharge1.JR_PaymentTypeInfo);
			AssertNoErrors(fCharge1.JR_ChequeNoInfo);

			fCharge1.Validation.ValidateJR_APInvoiceNum();
			fCharge1.Validation.ValidateJR_APInvoiceDate();
			fCharge1.Validation.ValidateJR_PaymentDate();
			fCharge1.Validation.ValidateJR_AB();
			fCharge1.Validation.ValidateJR_PaymentType();
			fCharge1.Validation.ValidateJR_ChequeNo();

			AssertHasErrors(fCharge1.JR_APInvoiceNumInfo);
			AssertHasErrors(fCharge1.JR_APInvoiceDateInfo);
			AssertHasErrors(fCharge1.JR_PaymentDateInfo);
			AssertHasErrors(fCharge1.JR_ABInfo);
			AssertHasErrors(fCharge1.JR_PaymentTypeInfo);
			AssertHasErrors(fCharge1.JR_ChequeNoInfo);

			fCharge1.JR_OH_CostAccount = fCreditor.PK;
			AssertNoErrors(fCharge1.JR_APInvoiceNumInfo);
			AssertNoErrors(fCharge1.JR_APInvoiceDateInfo);
			AssertNoErrors(fCharge1.JR_PaymentDateInfo);
			AssertNoErrors(fCharge1.JR_ABInfo);
			AssertNoErrors(fCharge1.JR_PaymentTypeInfo);
			AssertNoErrors(fCharge1.JR_ChequeNoInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			fCharge1.JR_AL_APLine = costLine.PK;
			fCharge1.JR_OH_CostAccount = ZGuid.Empty;
			AssertNoErrors("No errors because Cost posted", fCharge1.JR_OH_CostAccountInfo);

			fCharge1.Validation.ValidateJR_APInvoiceNum();
			fCharge1.Validation.ValidateJR_APInvoiceDate();
			fCharge1.Validation.ValidateJR_PaymentDate();
			fCharge1.Validation.ValidateJR_AB();
			fCharge1.Validation.ValidateJR_PaymentType();
			fCharge1.Validation.ValidateJR_ChequeNo();

			AssertNoErrors(fCharge1.JR_APInvoiceNumInfo);
			AssertNoErrors(fCharge1.JR_APInvoiceDateInfo);
			AssertNoErrors(fCharge1.JR_PaymentDateInfo);
			AssertNoErrors(fCharge1.JR_ABInfo);
			AssertNoErrors(fCharge1.JR_PaymentTypeInfo);
			AssertNoErrors(fCharge1.JR_ChequeNoInfo);
		}

		public void TestValidateAPInvoiceNumberDoesntUseQueryCache()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 10, TestObjectCreator.Creditor1, null, 10, null);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 10, TestObjectCreator.Creditor1);
			Factory.Save();

			var charge2 = consolCost.ApportionmentCharges[0];
			var jobToTest = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);
			var chargeToTest = TestObjectCreator.CreateCharge(jobToTest, TestObjectCreator.CC1, "", null, 10, TestObjectCreator.Creditor1, null, 10, null);

			chargeToTest.JR_APInvoiceNum = "INV1";
			AssertNoErrors(chargeToTest.JR_APInvoiceNumInfo);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_APInvoiceNum = 'INV1',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", charge1.PK));
			chargeToTest.JR_APInvoiceNum = "";
			chargeToTest.JR_APInvoiceNum = "INV1";
			var expectedError = "The Invoice/Credit Note is already used on another job or apportionment.";
			AssertHasError(chargeToTest.JR_APInvoiceNumInfo, expectedError);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_APInvoiceNum = 'INV2',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
where
	JR_PK = '{0}'", charge1.PK));
			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_APInvoiceNum = 'INV1',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
where
	JR_PK = '{0}'", charge2.PK));
			chargeToTest.JR_APInvoiceNum = "";
			chargeToTest.JR_APInvoiceNum = "INV1";
			AssertHasError(chargeToTest.JR_APInvoiceNumInfo, expectedError);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_APInvoiceNum = 'INV2',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
where
	JR_PK = '{0}'", charge2.PK));
			chargeToTest.JR_APInvoiceNum = "";
			chargeToTest.JR_APInvoiceNum = "INV1";
			AssertNoErrors(chargeToTest.JR_APInvoiceNumInfo);
		}

		public void TestValidateChequeNumberDoesntUseQueryCache()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 10, TestObjectCreator.Creditor1, null, 10, null);
			charge1.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			charge1.JR_PaymentType = ReceiptTypes.Cheque;
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 10, TestObjectCreator.Creditor1);
			consolCost.E6_AK_ChequeBook = TestObjectCreator.AUDChequeBook.PK;
			consolCost.E6_PaymentType = ReceiptTypes.Cheque;
			Factory.Save();

			var charge2 = consolCost.ApportionmentCharges[0];
			var jobToTest = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);
			var chargeToTest = TestObjectCreator.CreateCharge(jobToTest, TestObjectCreator.CC1, "", null, 10, TestObjectCreator.Creditor1, null, 10, null);
			chargeToTest.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			chargeToTest.JR_PaymentType = ReceiptTypes.Cheque;
			chargeToTest.JR_APInvoiceNum = "INV1";

			chargeToTest.JR_ChequeNo = "0001";
			AssertNoErrors(chargeToTest.JR_ChequeNoInfo);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_ChequeNo = '0001',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'

WHERE
	JR_PK = '{0}'", charge1.PK));
			chargeToTest.JR_ChequeNo = "";
			chargeToTest.JR_ChequeNo = "0001";
			var expectedError = "The Check Number is already used on another job or apportionment.";
			AssertHasError(chargeToTest.JR_ChequeNoInfo, expectedError);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_ChequeNo = '0002',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", charge1.PK));
			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_ChequeNo = '0001',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", charge2.PK));
			chargeToTest.JR_ChequeNo = "";
			chargeToTest.JR_ChequeNo = "0001";
			AssertHasError(chargeToTest.JR_ChequeNoInfo, expectedError);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_ChequeNo = '0002',
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", charge2.PK));
			chargeToTest.JR_ChequeNo = "";
			chargeToTest.JR_ChequeNo = "0001";
			AssertNoErrors(chargeToTest.JR_ChequeNoInfo);
		}

		public virtual void TestValidateJR_APInvoiceDate_WithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var testCharge = Factory.New<ChargeWithCostForTest>();
			testCharge.JR_APInvoiceNum = "INV001";
			testCharge.JR_APInvoiceDate = ZDateTime.Now;
			testCharge.JR_OH_SellAccount = fCreditor.PK;
			testCharge.JR_OH_CostAccount = fCreditor.PK;
			testCharge.JR_RX_NKCostCurrency = "AUD";

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");

			testCharge.Validation.ValidateJR_APInvoiceDate();
			AssertNoErrors(testCharge.JR_APInvoiceDateInfo);

			testCharge.JR_RX_NKCostCurrency = "USD";
			testCharge.Validation.ValidateJR_APInvoiceDate();
			AssertNoErrors(testCharge.JR_APInvoiceDateInfo);

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			testCharge.Validation.ValidateJR_APInvoiceDate();
			AssertHasWarning(testCharge.JR_APInvoiceDateInfo, @"This estimated cost amount has been calculated based on current job exchange rate.
This will be converted based on ""AP Invoice Posting Exchange Rate Option"" registry value ""Today Exchange Rate"" on posting as actual cost.");

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			testCharge.Validation.ValidateJR_APInvoiceDate();
			AssertHasWarning(testCharge.JR_APInvoiceDateInfo, @"This estimated cost amount has been calculated based on current job exchange rate.
This will be converted based on ""AP Invoice Posting Exchange Rate Option"" registry value ""Exchange Rate based on Invoice Date"" on posting as actual cost.");

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			testCharge.Validation.ValidateJR_APInvoiceDate();
			AssertHasWarning(testCharge.JR_APInvoiceDateInfo, @"This estimated cost amount has been calculated based on current job exchange rate.
This will be converted based on ""AP Invoice Posting Exchange Rate Option"" registry value ""Exchange Rate based on Post Date"" on posting as actual cost.");
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestRunAPInvoiceNumberExistsValidationShouldNotCheckEmptyNum()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = fCreditor.PK;
			Factory.Save();

			var charge = Factory.New<ChargeWithCostForTest>();
			charge.JR_OH_CostAccount = fCreditor.PK;
			charge.JR_APInvoiceNum = "";
			charge.RunPreSaveValidation();
			AssertNoErrors(charge.JR_APInvoiceNumInfo); //the previous logic checks empty num.
		}

		public void TestValidateJR_RX_NKSellInvoiceCurrency()
		{
			var charge = (ChargeWithCost)GetNewParentBusinessObject();
			Assert("Precondition: Empty", charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
			AssertNoErrors("Precondition: No Errors", charge.JR_RX_NKSellInvoiceCurrencyInfo);

			charge.JR_RX_NKSellInvoiceCurrency = Env.CurrentCompany.LocalCurrency.Code;
			AssertNoErrors("Local Currency: No Errors", charge.JR_RX_NKSellInvoiceCurrencyInfo);

			charge.JR_RX_NKSellInvoiceCurrency = "???";
			AssertHasError(charge.JR_RX_NKSellInvoiceCurrencyInfo, "Enter a valid selection.");

			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			AssertNoErrors("USD: No Errors", charge.JR_RX_NKSellInvoiceCurrencyInfo);
		}

		#endregion

		#region  Implementation

		protected AccBankAccount fBank;
		protected AccChequeBook fChequeBook;
		protected AccChargeCode fChargeCode;
		protected RefCurrency fCurrency;
		protected OrgHeader fCreditor;
		protected OrgHeader fCreditor2;
		protected Job fJob;
		Charge fCharge1;

		protected override void SetUp()
		{
			base.SetUp();

			fBank = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			fBank.AB_ChequeNumDigits = 6;
			fChequeBook = TestObjectCreator.InsertChequeBook(100, 100, 200, fBank.PK);
			fChargeCode = TestObjectCreator.InsertMarginChargeCode(100);
			fCurrency = TestObjectCreator.GetCurrency("USD");

			Factory.Save();

			fCreditor = TestObjectCreator.CreateOrgHeader("AAA", true, false);
			fCreditor2 = TestObjectCreator.CreateOrgHeader("BBB", true, false);

			fJob = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			fJob.JH_JobNum = "S001";

			fCharge1 = fJob.Charges.AddNew();
			fCharge1.JR_JH = fJob.PK;
			fCharge1.JR_AC = fChargeCode.PK;
			fCharge1.JR_OH_CostAccount = fCreditor.PK;
			fCharge1.JR_RX_NKCostCurrency = fCurrency.RX_Code;
			fCharge1.JR_OSCostAmt = 100M;
			fCharge1.JR_APInvoiceNum = "INV1";
			fCharge1.JR_APInvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			fCharge1.JR_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			fCharge1.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			fCharge1.JR_AB = fBank.PK;
			fCharge1.JR_AK = fChequeBook.PK;
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

		public abstract void TestCheckFieldsThatMustBeEqualOverChargesForSameTransaction();

		protected BusinessObjectFactory TestFactory
		{
			get
			{
				if (fTestFactory == null)
				{
					fTestFactory = new BusinessObjectFactory();
				}
				return fTestFactory;
			}
		}
		BusinessObjectFactory fTestFactory;

		#region Registry Setup

		PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection,
		ZString range, ZInt amount, ZString requirement)
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

		#endregion
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		#endregion
	}
}
