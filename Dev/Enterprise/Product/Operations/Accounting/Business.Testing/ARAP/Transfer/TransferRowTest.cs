using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	public abstract class TransferRowTest : TransactionHeaderTest
	{
		protected APInvoice TestInvoice1;
		protected APInvoice TestInvoice2;
		protected ARInvoice TestInvoice3;
		protected ARInvoice TestInvoice4;

		protected virtual TransferRow TestTransferRow
		{
			get { return Header as TransferRow; }
		}

		protected abstract Type TypeOfOtherRowInPair { get; }

		protected abstract ZByte TransactionCountForThisRow { get; }
		protected abstract ZByte TransactionCountForOtherRow { get; }

		public override void TestTransactionNumberOnSave()
		{
			SetupForSave();

			var transferRow = GetNewBusinessObject() as TransactionHeader;
			AssertEquals("Should not set transaction number on individual rows, controlled by container Transfer non-persistent",
				ZString.Empty, transferRow.AH_TransactionNum);
			transferRow.AH_TransactionNum = "TESTNUM";
			Factory.Save();
			AssertEquals("Should not get transaction number from number fountain", "TESTNUM", transferRow.AH_TransactionNum);
		}

		public virtual void SetupTestInvoices()
		{
			//TestTransferRow = TestTransfer;
			TestTransferRow.AH_OH = ZGuid.NewZGuid();

			TestInvoice1 = Factory.New(typeof(APInvoice)) as APInvoice;
			TestInvoice1.AH_IsCancelled = false;
			TestInvoice1.AH_OH = TestTransferRow.AH_OH;
			TestInvoice1.AH_OutstandingAmount = 10;

			TestInvoice2 = Factory.New(typeof(APInvoice)) as APInvoice;
			TestInvoice2.AH_IsCancelled = false;
			TestInvoice2.AH_OH = TestTransferRow.AH_OH;
			TestInvoice2.AH_OutstandingAmount = 20;

			TestInvoice3 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			TestInvoice3.AH_IsCancelled = false;
			TestInvoice3.AH_OH = TestTransferRow.AH_OH;
			TestInvoice3.AH_OutstandingAmount = 30;

			TestInvoice4 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			TestInvoice4.AH_IsCancelled = false;
			TestInvoice4.AH_OH = TestTransferRow.AH_OH;
			TestInvoice4.AH_OutstandingAmount = 40;
		}

		public override void TestTransactionNumberGenerator()
		{
			Assert("Transaction numbers on TransferRows are set by parent Transfer", true);
		}

		public void TestAH_OutstandingAmount()
		{
			TestTransferRow.AH_LocalExTaxAmount = 90;
			AssertEquals("Outstanding amount should be 90", TestTransferRow.AH_LocalOutstandingAmount, new ZDecimal(90));
		}

		public void TestIsMatched()
		{
			TestTransferRow.AH_LocalExTaxAmount = 100m;
			TestTransferRow.AH_LocalOutstandingAmount = 100.00m;
			Assert("Should not be matched", !((IMatching)TestTransferRow).IsMatched);
			TestTransferRow.AH_LocalOutstandingAmount = 50.00m;
			Assert("Should now be matched", ((IMatching)TestTransferRow).IsMatched);
		}

		public void TestFullyPay()
		{
			AssertFullyPay(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestFullyPay_EnableNewOSOutstandingAmountFeature()
		{
			AssertFullyPay(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertFullyPay(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestTransferRow.AH_OSExTaxAmount = 100m;
			TestTransferRow.AH_OutstandingAmount = 100.00m;
			TestTransferRow.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestTransferRow.MakeOSOutstandingAmountApplicable(100m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestTransferRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 100m, TestTransferRow.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			((IMatching)TestTransferRow).FullyPay(expectedFullyPaidDate);

			AssertEquals("Fully Paid date on TestTransferRow", expectedFullyPaidDate, TestTransferRow.AH_FullyPaidDate);
			AssertEquals("Outstanding Amount on TestTransferRow", 0m, TestTransferRow.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", 0m, TestTransferRow.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestTransferRow.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Private field should be set by calling method", 100.00m, TestTransferRow.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
		}

		public void TestPartiallyPay()
		{
			AssertPartiallyPay(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestPartiallyPay_EnableNewOSOutstandingAmountFeature()
		{
			AssertPartiallyPay(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertPartiallyPay(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestTransferRow.AH_LocalExTaxAmount = 100M;
			TestTransferRow.AH_OSTotalAmount = 100M;
			TestTransferRow.AH_LocalOutstandingAmount = 70M;
			TestTransferRow.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 70m * TestTransferRow.Multiplier_ForTestOnly;
				TestTransferRow.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestTransferRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 70m, TestTransferRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 60M;

			IMatching thisIMatching = TestTransferRow;
			thisIMatching.OSPartialPaymentAmount = AmountWithMultiplier;
			thisIMatching.PartiallyPay();

			AssertEquals("Local Outstanding Amount should be 10", 10M, TestTransferRow.AH_LocalOutstandingAmount);
			AssertEquals("Should not be fully paid", ZDateTime.Empty, TestTransferRow.AH_FullyPaidDate);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? 10m : 0m, TestTransferRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestTransferRow.AH_IsOSOutstandingAmountApplicable);

			thisIMatching.GenerateMatchLinks();
			TransactionMatchLinkCollection matchLinks = thisIMatching.CurrentMatchGroup;

			AssertEquals("There should be 1 matchlink", 1, matchLinks.Count);
			TransactionMatchLink matchLink = matchLinks[0];
			AssertEquals("Amount should be +60/-60 depending on TransferRow type", AmountWithMultiplier, matchLink.AP_Amount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), matchLink.AP_OSAmount);
		}

		public void TestGenerateMatchLinks()
		{
			AssertGenerateMatchLinks(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestGenerateMatchLinks_EnableNewOSOutstandingAmountFeature()
		{
			AssertGenerateMatchLinks(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertGenerateMatchLinks(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestTransferRow.AH_OSExTaxAmount = 100m;
			TestTransferRow.AH_OutstandingAmount = 100.00m;
			TestTransferRow.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestTransferRow.MakeOSOutstandingAmountApplicable(100m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestTransferRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 100m, TestTransferRow.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			((IMatching)TestTransferRow).FullyPay(expectedFullyPaidDate);
			TestTransferRow.GenerateMatchLinks();
			TransactionMatchLinkCollection matchLinks = ((IMatching)TestTransferRow).CurrentMatchGroup;

			AssertEquals("Should have one matchlink record generated", 1, matchLinks.Count);

			TransactionMatchLink singleLink = matchLinks[0];

			AssertEquals("Match Amount should be same as outstanding amount after fully paying",
				100.00m, singleLink.AP_Amount);
			AssertEquals("AP_OSAmount",
				isEnableNewOSOutstandingAmountFeature ? 100.00m : 0m, singleLink.AP_OSAmount);
			AssertEquals("Match FK should be this header", Header.PK, singleLink.AP_AH);
		}

		public void TestUnmatchFullyPaid()
		{
			AssertUnmatchFullyPaid(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchFullyPaid_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchFullyPaid(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchFullyPaid(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestTransferRow.AH_LocalExTaxAmount = 12M;
			TestTransferRow.AH_LocalOutstandingAmount = 0M;
			TestTransferRow.AH_FullyPaidDate = ZDateTime.Today;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestTransferRow.MakeOSOutstandingAmountApplicable(0M);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestTransferRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 0m, TestTransferRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 13M;

			AssertEquals("Cannot unmatch 13", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, ((IMatching)TestTransferRow).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = -1M;
			AssertEquals("Cannot unmatch -1", UnmatchingResult.DataErrorInvoiceAmountAndMatchLinkAmountHasOppositeSigns, ((IMatching)TestTransferRow).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = 6M;
			AssertEquals("Can match 6", UnmatchingResult.Success, ((IMatching)TestTransferRow).CanUnmatch(AmountWithMultiplier));
			((IMatching)TestTransferRow).Unmatch(AmountWithMultiplier, AmountWithMultiplier);

			AssertEquals("Outstanding amount should be 6", AmountWithMultiplier, TestTransferRow.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), TestTransferRow.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestTransferRow.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Fullypaid date should be null", ZDateTime.Empty, TestTransferRow.AH_FullyPaidDate);
		}

		public void TestUnmatchWithTax()
		{
			AssertUnmatchWithTax(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchWithTax_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchWithTax(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchWithTax(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestTransferRow.AH_LocalExTaxAmount = 90M;
			TestTransferRow.AH_LocalOutstandingAmount = 40M;
			TestTransferRow.AH_LocalTaxAmount = 10M;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 40m * TestTransferRow.Multiplier_ForTestOnly;
				TestTransferRow.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestTransferRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 40m, TestTransferRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 61M;

			AssertEquals("Cannot unmatch 61 since greater than tax amt + invoice amt - outstanding amt", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount,
				((IMatching)TestTransferRow).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = 60M;

			AssertEquals("Can unmatch 60 since equals tax amt + invoice amt - outstanding amt", UnmatchingResult.Success,
				((IMatching)TestTransferRow).CanUnmatch(AmountWithMultiplier));

			((IMatching)TestTransferRow).Unmatch(AmountWithMultiplier, AmountWithMultiplier);

			AmountWithMultiplier = 100M;
			AssertEquals("Outstanding amt should be 100", AmountWithMultiplier, TestTransferRow.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), TestTransferRow.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestTransferRow.AH_IsOSOutstandingAmountApplicable);
		}

		public void TestWithInvoiceAmountSignDifferentfromTotalSign()
		{
			AmountWithMultiplier = 1;
			var multiplier = AmountWithMultiplier;
			TestTransferRow.AH_InvoiceAmount = 402.9M * multiplier;
			TestTransferRow.AH_GSTAmount = -491.25M * multiplier;
			AssertEquals("Invoice amount sign should be different than invoice total sign", false, Math.Sign(TestTransferRow.AH_InvoiceAmount) == Math.Sign(TestTransferRow.AH_InvoiceAmount + TestTransferRow.AH_GSTAmount));
			var matchLinkAmount = -88.35M * multiplier;
			AssertEquals(UnmatchingResult.Success, ((IMatching)TestTransferRow).CanUnmatch(matchLinkAmount));

			AmountWithMultiplier = -1;
			multiplier = AmountWithMultiplier;
			TestTransferRow.AH_InvoiceAmount = 402.9M * multiplier;
			TestTransferRow.AH_GSTAmount = -491.25M * multiplier;
			AssertEquals("Invoice amount sign should be different than invoice total sign", false, Math.Sign(TestTransferRow.AH_InvoiceAmount) == Math.Sign(TestTransferRow.AH_InvoiceAmount + TestTransferRow.AH_GSTAmount));
			matchLinkAmount = -88.35M * multiplier;
			AssertEquals(UnmatchingResult.Success, ((IMatching)TestTransferRow).CanUnmatch(matchLinkAmount));
		}

		public override void TestRelatedTransactions()
		{
			TestTransferRow.AH_TransactionNum = "00009999";
			TestTransferRow.AH_TransactionCount = TransactionCountForThisRow;
			TransferRow otherRow = Factory.NewWithValidTestData(TypeOfOtherRowInPair) as TransferRow;
			otherRow.AH_TransactionNum = "00009999";
			otherRow.AH_TransactionCount = TransactionCountForOtherRow;

			Factory.Save();

			AssertEquals("There should be 1 related Transactions", 1, TestTransferRow.RelatedTransactions.Count);
			Assert("Related transactions should contain other row in the pair", TestTransferRow.RelatedTransactions.Contains(otherRow));
		}

		public override void TestAreRelatedTransactionsCreatedByMatching()
		{
			TestTransferRow.AH_TransactionNum = "00007777";
			TestTransferRow.AH_TransactionCreatedByMatching = true;
			TestTransferRow.AH_TransactionCount = TransactionCountForThisRow;
			TransferRow otherRow = Factory.NewWithValidTestData(TypeOfOtherRowInPair) as TransferRow;
			otherRow.AH_TransactionNum = "00007777";
			otherRow.AH_TransactionCreatedByMatching = true;
			otherRow.AH_TransactionCount = TransactionCountForOtherRow;

			Factory.Save();

			Assert("Both related Transactions are created by matching", TestTransferRow.AreRelatedTransactionsCreatedByMatching);
		}

		public void TestLocalPartialPaymentAmount()
		{
			TestTransferRow.AH_InvoiceAmount = 548.72M;
			TestTransferRow.AH_OutstandingAmount = 548.72M;
			TestTransferRow.AH_OSTotal = 377.13M;
			TestTransferRow.AH_ExchangeRate = 0.6873M;

			((IMatching)TestTransferRow).OSPartialPaymentAmount = 377.13M;
			AssertEquals("LocalPartialPayment amount should be 548.72 i.e. same as AH_OutstandingAmount",
				548.72M, ((IMatching)TestTransferRow).LocalPartialPaymentAmount);
			((IMatching)TestTransferRow).OSPartialPaymentAmount = 130M;
			AssertEquals("LocalPartialPayment amount should be 189.15",
				189.15M, ((IMatching)TestTransferRow).LocalPartialPaymentAmount);
		}

		public void TestGeneratePaymentApprovalItems()
			=> AssertGeneratePaymentApprovalItems(isEnableNewOSOutstandingAmountFeature: false);

		public void TestGeneratePaymentApprovalItems_EnableNewOSOutstandingAmountFeature()
			=> AssertGeneratePaymentApprovalItems(isEnableNewOSOutstandingAmountFeature: true);

		void AssertGeneratePaymentApprovalItems(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestTransferRow.AH_IsOSOutstandingAmountApplicable = isEnableNewOSOutstandingAmountFeature;
			// This will only be called for manually created transfers
			PaymentApprovalBase newPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			IMatching transferRowAsIMatching = TestTransferRow;
			transferRowAsIMatching.GeneratePaymentApprovalItems(newPaymentApproval);

			AssertEquals("Payment Approval Items", 1, transferRowAsIMatching.PaymentApprovalItems.Count);
			AssertEquals("Payment Approval Item Amount", transferRowAsIMatching.LocalPartialPaymentAmount, transferRowAsIMatching.PaymentApprovalItems[0].A2_PaymentThisRun);
			AssertEquals("Payment Approval Item Payment OS Amount",
				isEnableNewOSOutstandingAmountFeature ? transferRowAsIMatching.OSPartialPaymentAmount : new ZDecimal(0m),
				transferRowAsIMatching.PaymentApprovalItems[0].A2_OSPaymentThisRun
			);
		}

		[SuspendCriticalValidation]
		public void TestPostToGLIsAlwaysYesAfterSaving()
		{
			AssertEquals("Precondition before saving", "N", TestTransferRow.AH_PostToGL);
			Factory.Save();
			AssertEquals("After saving", "Y", TestTransferRow.AH_PostToGL);
		}
	}
}
