using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(APPaymentBatchApprovalMatching))]
	public class APPaymentBatchApprovalMatchingBaseTest : APPaymentApprovalMatchingBaseTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetTestMatchingBase();
		}

		protected override MatchingBase GetTestMatchingBase()
		{
			PaymentApprovalBase aPApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			aPApproval.AV_OH = TestOrgHeader.PK;
			return new APPaymentBatchApprovalMatching(Factory, aPApproval);
		}

		protected override Type InvoiceType
		{
			get { return typeof(APInvoice); }
		}

		public void TestBalanceCore()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg.CompanyData.OB_IsCreditor = true;
			testOrg.APSettlementGroupPK = testOrg.PK;

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg.PK;
			testAPInv.AH_Desc = "For Payment Approval 1";
			TestObjectCreator.CreateInvoiceLine(testAPInv, GlbCompany.CurrentCompany.LocalCurrency, 1m, 94m, 0m, 0m, 94m, 0m, 0m);
			((IMatching)testAPInv).OSPartialPaymentAmount = ((IMatching)testAPInv).OSOutstandingAmount;

			Factory.Save();

			PaymentApprovalBase aPApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			aPApproval.AV_OH = testOrg.PK;
			aPApproval.AV_Amount = -testAPInv.AH_OutstandingAmount;
			APPaymentBatchApprovalMatching matchingObject = new APPaymentBatchApprovalMatching(Factory, aPApproval);
			matchingObject.MatchedTransactions.RemoveAllFromAllCollections();
			matchingObject.UnmatchedTransactions.Add(testAPInv);
			matchingObject.MoveFromUnmatchToMatch(new BusinessObject[] { testAPInv });

			Assert("Should return zero balance", matchingObject.Balance.IsEmpty);
			matchingObject.MatchedTransactions.AddTransactionThatMustBeMatched(aPApproval);
			Assert("The balance should remain zero", matchingObject.Balance.IsEmpty);
		}

		public void TestMatchingFilterBizO()
		{
			AssertEquals("MatchingFilterBizO should be of valid type", typeof(APMatchingFilterBusinessObject), PaymentBatchTestMatchingBase.MatchingFilterBizO.GetType());
			Assert("MatchingFilterBizO should have it's FilterForDBReload set to no result query.", PaymentBatchTestMatchingBase.MatchingFilterBizO.FilterForDBReload.IsNoResultQuery);
		}

		public void TestValidateBalanceWhenPaymentApprovalDetailIsCancelled()
		{
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval.InitializeForPaymentBatch(() => false);
			approval.AV_Status = PaymentApprovalStatus.Cancelled;
			approval.AV_Amount = 100m;
			Factory.Save();

			AssertType<APPaymentBatchApprovalMatching>(approval.MatchingBaseObject);
			AssertEquals(100m, approval.MatchingBaseObject.Balance);
			approval.MatchingBaseObject.RunPreSaveValidation();
			AssertEquals("Balance not 0, should not have error", false, TestMatchingBase.BalanceInfo.HasErrors());
		}

		public override void TestDeleteCachedMiscTransactions()
		{
			TestOrg1 = GetNewTestOrg();
			Factory.Save();

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_LocalExTaxAmount = 320M;
			Factory.Save();

			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			PaymentTestMatchingBase.UnmatchedTransactions.Add(TestAPInvoice1);
			PaymentTestMatchingBase.MoveAllFromUnmatchToMatch();

			AssertEquals("There should be 2 transactions selected for matching", 2, PaymentTestMatchingBase.MatchedTransactions.Count);

			PaymentBatchTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_Discount = 10000M;
			PaymentBatchTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_ExchangeDifference = -10000M;

			PaymentTestMatchingBase.CreateTemporaryTransactions();

			AssertNull("OverpaymentBizO_ForTestOnly should be null", PaymentBatchTestMatchingBase.OverpaymentBizO_ForTestOnly);

			AssertNotNull("DiscountBizO_ForTestOnly should not be null", PaymentBatchTestMatchingBase.DiscountBizO_ForTestOnly);
			AssertEquals("Discount Amount", 10000M, PaymentTestMatchingBase.DiscountAmount);

			AssertNotNull("ExchangeDiffTmp should not be null", PaymentTestMatchingBase.ExchangeDiffTmp);
			AssertEquals("Exchange Difference Amount", -10000M, PaymentTestMatchingBase.ExchangeDifferenceAmount);

			PaymentTestMatchingBase.DeleteTemporaryTransactions();

			AssertNull("OverpaymentBizO_ForTestOnly should be null", PaymentBatchTestMatchingBase.OverpaymentBizO_ForTestOnly);
			AssertNull("DiscountBizO_ForTestOnly should be null", PaymentBatchTestMatchingBase.DiscountBizO_ForTestOnly);
			AssertNull("ExchangeDiffTmp should be null", PaymentBatchTestMatchingBase.ExchangeDifferenceBizO_ForTestOnly);
		}

		public override void TestFilteringWhenFilterDoesNotMatchOutstandingTransactions()
		{
			Assert("Cannot test filtering as APMatchingFilterBusinessObject returns no result query", true);
		}

		public override void TestInitialTransactionLoadingOccurs()
		{
			Assert("Cannot test InitialTransactions loading as APMatchingFilterBusinessObject returns no result query", true);
		}

		public override void TestLoadTransactionsFromCollection()
		{
			Assert("Cannot test LoadTransactionsFromCollection as APMatchingFilterBusinessObject returns no result query", true);
		}

		public override void TestMainCurrencyFilter()
		{
			Assert("Cannot test MainCurrencyFilter as APMatchingFilterBusinessObject returns no result query", true);
		}

		public override void TestReloadSettlementOrgTransactions()
		{
			Assert("Cannot test ReloadSettlementOrgTransactions as APMatchingFilterBusinessObject returns no result query", true);
		}

		public override void TestExcludeTransactionsFromUnmatchedList()
		{
			Assert("Cannot test Excluding as APPaymentBatchApprovalMatching doesn't load transactions from DB", true);
		}

		public override void TestSetPrimaryOrganization_UsesExpectedIndex()
		{
			Assert("Cannot test SetPrimaryOrganization_UsesExpectedIndex as APPaymentBatchApprovalMatching doesn't load transactions from DB", true);
		}

		public override void TestMatchingWithErrorOnSelectedTransactions()
		{
			TestOrg1 = GetNewTestOrg();

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_OH = TestOrg1.PK;
			aPInv.AH_LocalOutstandingAmount = 1000M;
			TestObjectCreator.CreateInvoiceLine(aPInv, GlbCompany.CurrentCompany.LocalCurrency, 1m, 1000m, 0m, 0m, 1000m, 0m, 0m);

			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_OH = TestOrg1.PK;
			aPRec.AH_LocalExTaxAmount = 90M;
			aPRec.AH_OSExTaxAmount = 90M;
			aPRec.AH_LocalOutstandingAmount = 90M;

			Factory.Save();

			PaymentBatchTestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			PaymentBatchTestMatchingBase.UnmatchedTransactions.Add(aPInv);
			PaymentBatchTestMatchingBase.UnmatchedTransactions.Add(aPRec);
			PaymentBatchTestMatchingBase.MoveAllFromUnmatchToMatch();

			((IMatching)aPInv).OSPartialPaymentAmount = 400M;   // set an invalid OSPartialPaymentAmt
			((IBusinessObjectInternals)aPInv).Validate(((IMatching)aPInv).OSPartialPaymentAmountInfo);

			TransactionHeader miscTrans = PaymentBatchTestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			PaymentBatchTestMatchingBase.AddMiscellaneousTransaction(miscTrans);

			Assert("The row for APInvoice should have errors", ((IMatching)aPInv).OSPartialPaymentAmountInfo.HasErrors());
			PaymentBatchTestMatchingBase.RunPreSaveValidation();
			Assert("Should not be able to match these because the OSPartialPayment amount on APInvoice is positive",
					PaymentBatchTestMatchingBase.HasErrors);
		}

		public override void TestMoveFromUnmatchToMatchMakesChequeNumReadonly()
		{
			TestOrg1 = GetNewTestOrg();
			Factory.Save();

			ARInvoice aRInv = GetNewTestARInvoice(378M, TestOrg1);
			PaymentBatchTestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			PaymentBatchTestMatchingBase.UnmatchedTransactions.Add(aRInv);
			Assert("Precondition: AH_ChequeOrReference should be readonly", aRInv.AH_ChequeOrReferenceInfo.ReadOnly);
			Assert("Precondition: ChequeOrReference should be readonly", ((IMatching)aRInv).ChequeOrReferenceInfo.ReadOnly);

			PaymentBatchTestMatchingBase.MoveAllFromUnmatchToMatch();

			Assert("ChequeOrReference should be readonly", ((IMatching)aRInv).ChequeOrReferenceInfo.ReadOnly);
		}

		protected APPaymentBatchApprovalMatching PaymentBatchTestMatchingBase
		{
			get { return (APPaymentBatchApprovalMatching)TestMatchingBase; }
		}

		public override void TestLoadTransactionsFromCollection_WhenManyTransactionsAreBeingMatched()
		{
			Assert("Cannot test as APPaymentBatchApprovalMatching doesn't load transactions from DB", true);
		}
	}
}
