using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	public abstract class ContraRowTest : TransactionHeaderTest
	{
		APInvoice TestInvoice1, TestInvoice2;
		ARInvoice TestInvoice3, TestInvoice4;

		protected virtual ContraRow TestContraRow
		{
			get { return Header as ContraRow; }
		}

		public virtual void TestSetAfterContraValues()
		{
		}

		protected virtual Type TypeOfOtherContraRow
		{
			get { return null; }
		}

		public override void TestTransactionNumberGenerator()
		{
			Assert("Transaction numbers on ContraRows are set by parent Contras", true);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesContraRow()
		{
			TestContraRow.AH_OH = ZGuid.NewZGuid();

			var localList = new List<string>
				{
					nameof(TestContraRow.AH_BeforeContra),
					nameof(TestContraRow.AH_AfterContra)
				};

			var tester = new DecimalPlacesAttributeTester(TestContraRow, TestContraRow.Company);
			tester.CheckLocalCurrency(localList, nameof(TestContraRow.LocalCurrencyDecimals));
		}

		public void TestInvoiceDateSetsDueDate()
		{
			TestContraRow.AH_InvoiceDate = ZDateTime.Empty;
			AssertEquals("due date should be empty", ZDateTime.Empty, TestContraRow.AH_InvoiceDate);
			TestContraRow.AH_InvoiceDate = ZDateTime.Today;
			AssertEquals("due date should be today's date", ZDateTime.Today, TestContraRow.AH_InvoiceDate);
		}

		public virtual void TestBeforeContra()
		{
			TestContraRow.AH_OH = ZGuid.NewZGuid();

			TestInvoice1 = Factory.New(typeof(APInvoice)) as APInvoice;
			TestInvoice1.AH_IsCancelled = false;
			TestInvoice1.AH_OH = TestContraRow.AH_OH;
			TestInvoice1.AH_OutstandingAmount = 10;

			TestInvoice2 = Factory.New(typeof(APInvoice)) as APInvoice;
			TestInvoice2.AH_IsCancelled = false;
			TestInvoice2.AH_OH = TestContraRow.AH_OH;
			TestInvoice2.AH_OutstandingAmount = 20;

			TestInvoice3 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			TestInvoice3.AH_IsCancelled = false;
			TestInvoice3.AH_OH = TestContraRow.AH_OH;
			TestInvoice3.AH_OutstandingAmount = 30;

			TestInvoice4 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			TestInvoice4.AH_IsCancelled = false;
			TestInvoice4.AH_OH = TestContraRow.AH_OH;
			TestInvoice4.AH_OutstandingAmount = 40;
		}

		public void TestIsMatched()
		{
			TestContraRow.AH_LocalExTaxAmount = 100m;
			TestContraRow.AH_LocalOutstandingAmount = 100.00m;
			Assert("Should not be matched", !((IMatching)TestContraRow).IsMatched);
			TestContraRow.AH_LocalOutstandingAmount = 50.00m;
			Assert("Should now be matched", ((IMatching)TestContraRow).IsMatched);
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

			TestContraRow.AH_OSExTaxAmount = 100m;
			TestContraRow.AH_OutstandingAmount = 100.00m;
			TestContraRow.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestContraRow.MakeOSOutstandingAmountApplicable(100m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestContraRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 100m, TestContraRow.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);
			((IMatching)TestContraRow).FullyPay(expectedFullyPaidDate);

			AssertEquals("Fully Paid date on TestContraRow", expectedFullyPaidDate, TestContraRow.AH_FullyPaidDate);
			AssertEquals("Outstanding Amount on TestContraRow", 0m, TestContraRow.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", 0m, TestContraRow.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestContraRow.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Private field should be set by calling method", 100.00m, TestContraRow.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
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

			TestContraRow.AH_LocalExTaxAmount = 100M;
			TestContraRow.AH_OSTotalAmount = 100M;
			TestContraRow.AH_LocalOutstandingAmount = 70M;
			TestContraRow.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 70m * TestContraRow.Multiplier_ForTestOnly;
				TestContraRow.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestContraRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 70m, TestContraRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 60M;

			IMatching thisIMatching = TestContraRow;
			thisIMatching.OSPartialPaymentAmount = AmountWithMultiplier;
			thisIMatching.PartiallyPay();

			AssertEquals("Local Outstanding Amount should be 10", 10M, TestContraRow.AH_LocalOutstandingAmount);
			AssertEquals("Should not be fully paid", ZDateTime.Empty, TestContraRow.AH_FullyPaidDate);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? 10m : 0m, TestContraRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestContraRow.AH_IsOSOutstandingAmountApplicable);

			thisIMatching.GenerateMatchLinks();
			TransactionMatchLinkCollection matchLinks = thisIMatching.CurrentMatchGroup;

			AssertEquals("There should be 1 matchlink", 1, matchLinks.Count);
			TransactionMatchLink matchLink = matchLinks[0];
			AssertEquals("Amount should be +60/-60 depending on Contra type", AmountWithMultiplier, matchLink.AP_Amount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), matchLink.AP_OSAmount);
		}

		public void TestGenerateMatchLinks()
		{
			AssertGenerateMatchlinks(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestGenerateMatchLinks_EnableNewOSOutstandingAmountFeature()
		{
			AssertGenerateMatchlinks(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertGenerateMatchlinks(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestContraRow.AH_OSExTaxAmount = 100m;
			TestContraRow.AH_OutstandingAmount = 100.00m;
			TestContraRow.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestContraRow.MakeOSOutstandingAmountApplicable(100m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestContraRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 100m, TestContraRow.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			((IMatching)TestContraRow).FullyPay(expectedFullyPaidDate);
			TestContraRow.GenerateMatchLinks();
			TransactionMatchLinkCollection matchLinks = ((IMatching)TestContraRow).CurrentMatchGroup;

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

			TestContraRow.AH_TransactionCreatedByMatching = false;
			TestContraRow.AH_LocalExTaxAmount = 96M;
			TestContraRow.AH_LocalOutstandingAmount = 0M;
			TestContraRow.AH_FullyPaidDate = ZDateTime.Today;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestContraRow.MakeOSOutstandingAmountApplicable(0m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestContraRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 0m, TestContraRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 96M;

			((IMatching)TestContraRow).Unmatch(AmountWithMultiplier, AmountWithMultiplier);

			AssertEquals("Outstanding amount should be +96/-96 depending on AR/AP", AmountWithMultiplier, TestContraRow.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), TestContraRow.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestContraRow.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("ContraRow is not fully paid any more", ZDateTime.Empty, TestContraRow.AH_FullyPaidDate);
		}

		public void TestUnmatchPartiallyPaid()
		{
			AssertUnmatchPartiallyPaid(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchPartiallyPaid_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchPartiallyPaid(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchPartiallyPaid(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestContraRow.AH_TransactionCreatedByMatching = false;
			TestContraRow.AH_LocalExTaxAmount = 20M;
			TestContraRow.AH_LocalOutstandingAmount = 4M;
			TestContraRow.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 4m * TestContraRow.Multiplier_ForTestOnly;
				TestContraRow.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestContraRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 4m, TestContraRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 17M;

			AssertEquals("This amount cannot be unmatched since it would make outstanding amount greater than Invoice amount", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, ((IMatching)TestContraRow).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = -1M;
			AssertEquals("This amount can't be unmatched since absolute value of outstanding amount would decrease", UnmatchingResult.DataErrorInvoiceAmountAndMatchLinkAmountHasOppositeSigns, ((IMatching)TestContraRow).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = 4M;
			((IMatching)TestContraRow).Unmatch(AmountWithMultiplier, AmountWithMultiplier);

			AmountWithMultiplier = 8M;

			AssertEquals("Outstanding amount should be +8/-8 depending on AR/AP", AmountWithMultiplier, TestContraRow.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), TestContraRow.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestContraRow.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("ContraRow not fully paid", ZDateTime.Empty, TestContraRow.AH_FullyPaidDate);
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

			TestContraRow.AH_TransactionCreatedByMatching = false;
			TestContraRow.AH_LocalExTaxAmount = 100M;
			TestContraRow.AH_LocalTaxAmount = 10M;
			TestContraRow.AH_LocalOutstandingAmount = 0M;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestContraRow.MakeOSOutstandingAmountApplicable(0m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, TestContraRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 0m, TestContraRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 111M;
			AssertEquals("This amount can't be unmatched since it is greater than tax amt + invoice amt", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, ((IMatching)TestContraRow).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = 109M;
			AssertEquals("This amount can be unmatched since it is less than tax amt + invoice amt", UnmatchingResult.Success, ((IMatching)TestContraRow).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = 110M;
			((IMatching)TestContraRow).Unmatch(AmountWithMultiplier, AmountWithMultiplier);

			AssertEquals("Outstanding amount should be 110", AmountWithMultiplier, TestContraRow.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), TestContraRow.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestContraRow.AH_IsOSOutstandingAmountApplicable);
		}

		public void TestWithInvoiceAmountSignDifferentfromTotalSign()
		{
			AmountWithMultiplier = 1;
			var multiplier = AmountWithMultiplier;
			TestContraRow.AH_InvoiceAmount = 402.9M * multiplier;
			TestContraRow.AH_GSTAmount = -491.25M * multiplier;
			AssertEquals("Invoice amount sign should be different than invoice total sign", false, Math.Sign(TestContraRow.AH_InvoiceAmount) == Math.Sign(TestContraRow.AH_InvoiceAmount + TestContraRow.AH_GSTAmount));
			var matchLinkAmount = -88.35M * multiplier;
			AssertEquals(UnmatchingResult.Success, ((IMatching)TestContraRow).CanUnmatch(matchLinkAmount));

			AmountWithMultiplier = -1;
			multiplier = AmountWithMultiplier;
			TestContraRow.AH_InvoiceAmount = 402.9M * multiplier;
			TestContraRow.AH_GSTAmount = -491.25M * multiplier;
			AssertEquals("Invoice amount sign should be different than invoice total sign", false, Math.Sign(TestContraRow.AH_InvoiceAmount) == Math.Sign(TestContraRow.AH_InvoiceAmount + TestContraRow.AH_GSTAmount));
			matchLinkAmount = -88.35M * multiplier;
			AssertEquals(UnmatchingResult.Success, ((IMatching)TestContraRow).CanUnmatch(matchLinkAmount));
		}

		public override void TestRelatedTransactions()
		{
			TestContraRow.AH_TransactionNum = "00003000";
			ContraRow otherContraRow = Factory.NewWithValidTestData(TypeOfOtherContraRow) as ContraRow;
			otherContraRow.AH_TransactionNum = "00003000";

			Factory.Save();

			AssertEquals("TestContraRow should have 1 related transaction", 1, TestContraRow.RelatedTransactions.Count);
			Assert("One of TestContraRow's related transactions should be the other row", TestContraRow.RelatedTransactions.Contains(otherContraRow));
		}

		public override void TestAreRelatedTransactionsCreatedByMatching()
		{
			TestContraRow.AH_TransactionNum = "00003000";
			TestContraRow.AH_TransactionCreatedByMatching = true;
			ContraRow otherContraRow = Factory.NewWithValidTestData(TypeOfOtherContraRow) as ContraRow;
			otherContraRow.AH_TransactionNum = "00003000";
			otherContraRow.AH_TransactionCreatedByMatching = true;

			Factory.Save();

			Assert("TestContraRow's Related Transactions are created by matching", TestContraRow.AreRelatedTransactionsCreatedByMatching);
		}

		public void TestLocalPartialPaymentAmount()
		{
			TestContraRow.AH_InvoiceAmount = 548.72M;
			TestContraRow.AH_OutstandingAmount = 548.72M;
			TestContraRow.AH_OSTotal = 377.13M;
			TestContraRow.AH_ExchangeRate = 0.6873M;

			((IMatching)TestContraRow).OSPartialPaymentAmount = 377.13M;
			AssertEquals("LocalPartialPayment amount should be 548.72 i.e. same as AH_OutstandingAmount",
				548.72M, ((IMatching)TestContraRow).LocalPartialPaymentAmount);

			((IMatching)TestContraRow).OSPartialPaymentAmount = 200M;
			AssertEquals("LocalPartialPaymentAmount should be 290.99",
				290.99M, ((IMatching)TestContraRow).LocalPartialPaymentAmount);
		}

		public override void TestTransactionNumberOnSave()
		{
			AssertNotNull(TestContraRow.AH_TransactionNum);
		}

		public void TestGeneratePaymentApprovalItems()
			=> AssertGeneratePaymentApprovalItems(isEnableNewOSOutstandingAmountFeature: false);

		public void TestGeneratePaymentApprovalItems_EnableNewOSOutstandingAmountFeature()
			=> AssertGeneratePaymentApprovalItems(isEnableNewOSOutstandingAmountFeature: true);

		void AssertGeneratePaymentApprovalItems(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestContraRow.AH_IsOSOutstandingAmountApplicable = isEnableNewOSOutstandingAmountFeature;
			// This will only be called for manually created contras
			PaymentApprovalBase newPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			IMatching contraRowAsIMatching = TestContraRow;
			contraRowAsIMatching.GeneratePaymentApprovalItems(newPaymentApproval);

			AssertEquals("Payment Approval Items", 1, contraRowAsIMatching.PaymentApprovalItems.Count);
			AssertEquals("Payment Approval Item Payment Amount", contraRowAsIMatching.LocalPartialPaymentAmount, contraRowAsIMatching.PaymentApprovalItems[0].A2_PaymentThisRun);
			AssertEquals("Payment Approval Item Payment OS Amount",
				isEnableNewOSOutstandingAmountFeature ? contraRowAsIMatching.OSPartialPaymentAmount : new ZDecimal(0m),
				contraRowAsIMatching.PaymentApprovalItems[0].A2_OSPaymentThisRun
			);
		}
	}
}
