using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class PaymentReversingTest : PayablesAndReceivablesReversingTest
	{
		public override void TestCantReverseMatchedIPayablesAndReceivables()
		{
			TestIPayablesAndReceivables.SetIsMatched(true);
			TestIPayablesAndReceivables.SetIsReversed(false);
			Assert("Should be allowed to reverse matched payment as it gets unmatched automatically", PaymentReversing.CanReverseTransaction);
			AssertEquals("Error message should be emtpy", ZString.Empty, PaymentReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIPayment();
		}

		protected PaymentReversing PaymentReversing
		{
			get { return (PaymentReversing)PayablesAndReceivablesReversing; }
		}

		#region TestReverseOnceMatchedPayment

		public void TestReverseOnceMatchedPayment()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OH = testOrg.PK;
			aPPay.AH_LocalExTaxAmount = 61M;
			aPPay.AH_OSExTaxAmount = 61M;
			aPPay.AH_LocalOutstandingAmount = 0M;

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(aPInv, aPInv.TransactionCurrency, aPInv.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			aPInv.AH_LocalOutstandingAmount = 39M;

			TransactionMatchLink aPPayMatch = ((IMatching)aPInv).CurrentMatchGroup.AddNew();
			aPPayMatch.AP_AH = aPPay.PK;
			aPPayMatch.AP_Amount = 61M;
			aPPayMatch.AP_MatchGroupNum = "M00001840";
			aPPayMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			TransactionMatchLink aPInvMatch = ((IMatching)aPInv).CurrentMatchGroup.AddNew();
			aPInvMatch.AP_AH = aPInv.PK;
			aPInvMatch.AP_Amount = -61M;
			aPInvMatch.AP_MatchGroupNum = "M00001840";
			aPInvMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			aPPay.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();

			PaymentReversing payReversing = new PaymentReversing(aPPay);
			payReversing.Reverse();

			AssertEquals("MinUnmatchDate should be initialized.", ZDateTime.BrettsBirthday, ((IUnmatchOnReversing)aPPay.ReverseTransaction).UnmatchingData.MinUnmatchDate);
			aPPay.ReverseTransaction.UnmatchDate = ZDateTime.Today.AddDays(-100);

			Factory.Save();

			// Check the OriginalPayment
			aPPay.Reload();
			AssertEquals("Original Payment Outstanding amount = 0", 0M, aPPay.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", ZDateTime.Today, aPPay.AH_FullyPaidDate.Date);
			Assert("Original Payment should be cancelled", aPPay.AH_IsCancelled);

			// Check the Original Invoice
			aPInv.Reload();
			AssertEquals("Original Invoice outstanding amount = -100", -100M, aPInv.AH_OutstandingAmount);

			// Check the Reversing Payment
			ZQuery revPayFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Payment);
			revPayFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, aPPay.PK);
			TransactionHeader revPay = Factory.LoadTop1<TransactionHeader>(revPayFilter);
			Assert("RevPay should be a Payment", revPay is Payment);

			AssertEquals("Reversing Payment local amount = -61", -61M, revPay.AH_InvoiceAmount);
			AssertEquals("Reversing Payment outstanding amount = 0", 0M, revPay.AH_OutstandingAmount);
			Assert("Reversing Payment should be cancelled", revPay.AH_IsCancelled);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", ZDateTime.Today, revPay.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should not be changed on unmatch date", ZDateTime.Today, revPay.AH_PostDate.Date);

			// Check Matchlinks
			Assert("APPayMatch should be deleted", aPPayMatch.IsDeleted);
			Assert("APInvMatch should be deleted", aPInvMatch.IsDeleted);

			ZQuery origPayMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, aPPay.PK);
			TransactionMatchLink origPayMatch = Factory.LoadTop1<TransactionMatchLink>(origPayMatchFilter);
			AssertEquals("Match date of new reversing matchinkg should not be changed on unmatch date", ZDateTime.Today, origPayMatch.AP_MatchDate.Date);
			AssertEquals("OrigPay Match amount = 61", 61M, origPayMatch.AP_Amount);

			ZQuery revPayMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revPay.PK);
			TransactionMatchLink revPayMatch = Factory.LoadTop1<TransactionMatchLink>(revPayMatchFilter);
			AssertEquals("RevPay Match amount = -61", -61M, revPayMatch.AP_Amount);
			AssertEquals("Match date of new reversing matchinkg should not be changed on unmatch date", ZDateTime.Today, revPayMatch.AP_MatchDate.Date);
		}

		#endregion

		public void TestReversedTransactionUnmatchDateReadOnly_MixedMatching()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_OH = testOrg.PK;
			payment.AH_OSExTaxAmount = 40M;
			payment.AH_LocalOutstandingAmount = 0M;

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OH = testOrg.PK;

			var line = (ARCreditNoteLine)creditNote.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = 40m;
			creditNote.AH_LocalOutstandingAmount = 0M;

			string matchGroupNumber = "M00001";
			var paymentMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			paymentMatch.AP_AH = payment.PK;
			paymentMatch.AP_Amount = payment.AH_LocalExTaxAmount;
			paymentMatch.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			var invoiceMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			invoiceMatch.AP_AH = creditNote.PK;
			invoiceMatch.AP_Amount = -creditNote.AH_LocalExTaxAmount;
			invoiceMatch.AP_MatchGroupNum = matchGroupNumber;
			invoiceMatch.AP_MatchDate = paymentMatch.AP_MatchDate;

			payment.AH_FullyPaidDate = paymentMatch.AP_MatchDate;
			creditNote.AH_FullyPaidDate = paymentMatch.AP_MatchDate;

			Factory.Save();

			Action<bool, bool, bool, bool> assertUnmatchDateReadonly = (bool apPermition, bool arPermition, bool backPostingRegistry, bool unmatchDateReadonly) =>
				{
					Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = apPermition;
					Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = arPermition;
					AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backPostingRegistry);
					var newFactory = new BusinessObjectFactory();
					var paymentForReversing = newFactory.Load<APPayment>(payment.PK);
					var payReversing = new PaymentReversing(paymentForReversing);
					payReversing.Reverse();
					AssertEquals(unmatchDateReadonly, paymentForReversing.ReverseTransaction.UnmatchDateInfo.ReadOnly);
				};

			assertUnmatchDateReadonly(false, true, true, true);
			assertUnmatchDateReadonly(true, false, true, true);
			assertUnmatchDateReadonly(true, true, false, true);
			assertUnmatchDateReadonly(true, true, true, false);
		}

		public void TestReversedTransactionUnmatchDateReadOnly_APOnlyMatching()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_OH = testOrg.PK;
			payment.AH_OSExTaxAmount = 40M;
			payment.AH_LocalOutstandingAmount = 0M;

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("00001001", TestObjectCreator.AUD, 1m, 40m, 0m, 0m, 40m, 0m, 0m);
			invoice.AH_OH = testOrg.PK;
			invoice.AH_LocalOutstandingAmount = 0M;

			string matchGroupNumber = "M00001";
			var paymentMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			paymentMatch.AP_AH = payment.PK;
			paymentMatch.AP_Amount = payment.AH_LocalExTaxAmount;
			paymentMatch.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			var invoiceMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			invoiceMatch.AP_AH = invoice.PK;
			invoiceMatch.AP_Amount = -invoice.AH_LocalExTaxAmount;
			invoiceMatch.AP_MatchGroupNum = matchGroupNumber;
			invoiceMatch.AP_MatchDate = paymentMatch.AP_MatchDate;

			payment.AH_FullyPaidDate = paymentMatch.AP_MatchDate;
			invoice.AH_FullyPaidDate = paymentMatch.AP_MatchDate;

			Factory.Save();

			Action<bool, bool, bool, bool> assertUnmatchDateReadonly = (bool apPermition, bool arPermition, bool backPostingRegistry, bool unmatchDateReadonly) =>
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = apPermition;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = arPermition;
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backPostingRegistry);
				var newFactory = new BusinessObjectFactory();
				var paymentForReversing = newFactory.Load<APPayment>(payment.PK);
				var payReversing = new PaymentReversing(paymentForReversing);
				payReversing.Reverse();
				AssertEquals(unmatchDateReadonly, paymentForReversing.ReverseTransaction.UnmatchDateInfo.ReadOnly);
			};

			assertUnmatchDateReadonly(false, true, true, true);
			assertUnmatchDateReadonly(true, false, true, false);
			assertUnmatchDateReadonly(true, true, false, true);
			assertUnmatchDateReadonly(true, true, true, false);
		}

		public void TestReversedTransactionUnmatchDateReadOnly_AROnlyMatching()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var payment = Factory.NewWithValidTestData<ARPayment>();
			payment.AH_OH = testOrg.PK;
			payment.AH_OSExTaxAmount = 40M;
			payment.AH_LocalOutstandingAmount = 0M;

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OH = testOrg.PK;

			var line = (ARCreditNoteLine)creditNote.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = 40m;
			creditNote.AH_LocalOutstandingAmount = 0M;

			string matchGroupNumber = "M00001";
			var paymentMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			paymentMatch.AP_AH = payment.PK;
			paymentMatch.AP_Amount = payment.AH_LocalExTaxAmount;
			paymentMatch.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			var invoiceMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			invoiceMatch.AP_AH = creditNote.PK;
			invoiceMatch.AP_Amount = -creditNote.AH_LocalExTaxAmount;
			invoiceMatch.AP_MatchGroupNum = matchGroupNumber;
			invoiceMatch.AP_MatchDate = paymentMatch.AP_MatchDate;

			payment.AH_FullyPaidDate = paymentMatch.AP_MatchDate;
			creditNote.AH_FullyPaidDate = paymentMatch.AP_MatchDate;

			Factory.Save();

			Action<bool, bool, bool, bool> assertUnmatchDateReadonly = (bool apPermition, bool arPermition, bool backPostingRegistry, bool unmatchDateReadonly) =>
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = apPermition;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = arPermition;
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backPostingRegistry);
				var newFactory = new BusinessObjectFactory();
				var paymentForReversing = newFactory.Load<APPayment>(payment.PK);
				var payReversing = new PaymentReversing(paymentForReversing);
				payReversing.Reverse();
				AssertEquals(unmatchDateReadonly, paymentForReversing.ReverseTransaction.UnmatchDateInfo.ReadOnly);
			};

			assertUnmatchDateReadonly(false, true, true, false);
			assertUnmatchDateReadonly(true, false, true, true);
			assertUnmatchDateReadonly(true, true, false, true);
			assertUnmatchDateReadonly(true, true, true, false);
		}

		public void TestChangeUnmatchDateOnReversing()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_OH = testOrg.PK;
			payment.AH_OSExTaxAmount = 70M;
			payment.AH_LocalOutstandingAmount = 50M;

			var discount = Factory.NewWithValidTestData<APDiscount>();
			discount.AH_OH = testOrg.PK;
			discount.AH_OSExTaxAmount = -20M;
			discount.AH_LocalOutstandingAmount = 0M;

			string matchGroupNumber = "M00001840";
			var paymentMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			paymentMatch.AP_AH = payment.PK;
			paymentMatch.AP_Amount = -discount.AH_LocalExTaxAmount;
			paymentMatch.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			var discountMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			discountMatch.AP_AH = discount.PK;
			discountMatch.AP_Amount = discount.AH_LocalExTaxAmount;
			discountMatch.AP_MatchGroupNum = matchGroupNumber;
			discountMatch.AP_MatchDate = paymentMatch.AP_MatchDate;

			discount.AH_FullyPaidDate = paymentMatch.AP_MatchDate;

			Factory.Save();

			((IMatching)payment).CurrentMatchGroup.RemoveAll();
			var overpayment = Factory.NewWithValidTestData<APOverpayment>();
			overpayment.AH_OH = testOrg.PK;
			overpayment.AH_OSExTaxAmount = -40M;
			overpayment.AH_LocalOutstandingAmount = 0M;

			matchGroupNumber = "M00001841";
			var paymentMatch2 = ((IMatching)payment).CurrentMatchGroup.AddNew();
			paymentMatch2.AP_AH = payment.PK;
			paymentMatch2.AP_Amount = -overpayment.AH_LocalExTaxAmount;
			paymentMatch2.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch2.AP_MatchDate = ZDateTime.BrettsBirthday.AddDays(50);

			var overpaymentMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			overpaymentMatch.AP_AH = overpayment.PK;
			overpaymentMatch.AP_Amount = overpayment.AH_LocalExTaxAmount;
			overpaymentMatch.AP_MatchGroupNum = matchGroupNumber;
			overpaymentMatch.AP_MatchDate = paymentMatch2.AP_MatchDate;

			payment.AH_LocalOutstandingAmount = 10M;
			overpayment.AH_FullyPaidDate = paymentMatch2.AP_MatchDate;

			Factory.Save();

			((IMatching)payment).CurrentMatchGroup.RemoveAll();
			var clearingJournal = Factory.NewWithValidTestData<APJournal>();
			clearingJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
			clearingJournal.AH_OH = testOrg.PK;
			clearingJournal.AH_OSExTaxAmount = 10M;
			clearingJournal.AH_LocalOutstandingAmount = 0M;
			clearingJournal.AH_TransactionCreatedByMatching = true;

			matchGroupNumber = "M00001842";
			var paymentMatch3 = ((IMatching)payment).CurrentMatchGroup.AddNew();
			paymentMatch3.AP_AH = payment.PK;
			paymentMatch3.AP_Amount = clearingJournal.AH_LocalExTaxAmount;
			paymentMatch3.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch3.AP_MatchDate = ZDateTime.BrettsBirthday.AddDays(30);

			var clearingJournalMatch = ((IMatching)payment).CurrentMatchGroup.AddNew();
			clearingJournalMatch.AP_AH = clearingJournal.PK;
			clearingJournalMatch.AP_Amount = -clearingJournal.AH_LocalExTaxAmount;
			clearingJournalMatch.AP_MatchGroupNum = matchGroupNumber;
			clearingJournalMatch.AP_MatchDate = paymentMatch3.AP_MatchDate;

			payment.AH_LocalOutstandingAmount = 0M;
			payment.AH_FullyPaidDate = paymentMatch3.AP_MatchDate;
			clearingJournal.AH_FullyPaidDate = paymentMatch3.AP_MatchDate;

			Factory.Save();

			var payReversing = new PaymentReversing(payment);
			payReversing.Reverse();

			AssertEquals("MinUnmatchDate should be initialized.", ZDateTime.BrettsBirthday.AddDays(50), ((IUnmatchOnReversing)payment.ReverseTransaction).UnmatchingData.MinUnmatchDate);
			var expectedUnmatchDate = ZDateTime.Today.AddDays(-100);
			payment.ReverseTransaction.UnmatchDate = ZDateTime.Today.AddDays(-100);

			Factory.Save();

			payment.Reload();
			AssertEquals("Original Payment Outstanding amount = 0", 0M, payment.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", ZDateTime.Today, payment.AH_FullyPaidDate.Date);
			Assert("Original Payment should be cancelled", payment.AH_IsCancelled);

			discount.Reload();
			AssertEquals("Original discount outstanding amount = 0", 0M, discount.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", expectedUnmatchDate, discount.AH_FullyPaidDate.Date);
			Assert("Original discount should be cancelled", discount.AH_IsCancelled);

			overpayment.Reload();
			AssertEquals("Original overpayment outstanding amount = 0", 0M, overpayment.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", expectedUnmatchDate, overpayment.AH_FullyPaidDate.Date);
			Assert("Original overpayment should be cancelled", overpayment.AH_IsCancelled);

			clearingJournal.Reload();
			AssertEquals("Original clearingJournal outstanding amount = 0", 0M, clearingJournal.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", expectedUnmatchDate, clearingJournal.AH_FullyPaidDate.Date);
			Assert("Original clearingJournal should be cancelled", clearingJournal.AH_IsCancelled);

			var revPayFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
			revPayFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, payment.PK);
			revPayFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, payment.AH_GC);
			var payments = Factory.Load<TransactionHeader>(revPayFilter);
			AssertEquals("payments.Length", 1, payments.Length);
			var revPay = payments[0];
			Assert("RevPay should be a Payment", revPay is Payment);

			AssertEquals("Reversing Payment local amount", -70M, revPay.AH_InvoiceAmount);
			AssertEquals("Reversing Payment outstanding amount = 0", 0M, revPay.AH_OutstandingAmount);
			Assert("Reversing Payment should be cancelled", revPay.AH_IsCancelled);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", ZDateTime.Today, revPay.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should not be changed on unmatch date", ZDateTime.Today, revPay.AH_PostDate.Date);

			var revDiscountFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Discount);
			revDiscountFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, discount.PK);
			revDiscountFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, discount.AH_GC);
			var discounts = Factory.Load<TransactionHeader>(revDiscountFilter);
			AssertEquals("discounts.Length", 1, discounts.Length);
			var revDiscount = discounts[0];
			Assert("revDiscount should be a Discount", revDiscount is Discount);

			AssertEquals("Reversing Discount local amount", 20M, revDiscount.AH_InvoiceAmount);
			AssertEquals("Reversing Discount outstanding amount = 0", 0M, revDiscount.AH_OutstandingAmount);
			Assert("Reversing Discount should be cancelled", revDiscount.AH_IsCancelled);
			AssertEquals("FullyPaidDate should be changed on unmatch date", expectedUnmatchDate, revDiscount.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should be changed on unmatch date", expectedUnmatchDate, revDiscount.AH_PostDate.Date);

			var revOverpaymentFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Overpayment);
			revOverpaymentFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, overpayment.PK);
			revOverpaymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, overpayment.AH_GC);
			var overpayments = Factory.Load<TransactionHeader>(revOverpaymentFilter);
			AssertEquals("overpayments.Length", 1, overpayments.Length);
			var revOverpayment = overpayments[0];
			Assert("revOverpayment should be a Overpayment", revOverpayment is Overpayment);

			AssertEquals("Reversing Overpayment local amount", 40M, revOverpayment.AH_InvoiceAmount);
			AssertEquals("Reversing Overpayment outstanding amount = 0", 0M, revOverpayment.AH_OutstandingAmount);
			Assert("Reversing Overpayment should be cancelled", revOverpayment.AH_IsCancelled);
			AssertEquals("FullyPaidDate should be changed on unmatch date", expectedUnmatchDate, revOverpayment.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should be changed on unmatch date", expectedUnmatchDate, revOverpayment.AH_PostDate.Date);

			var revClearingJournalFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			revClearingJournalFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, clearingJournal.PK);
			revClearingJournalFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, clearingJournal.AH_GC);
			var journals = Factory.Load<TransactionHeader>(revClearingJournalFilter);
			AssertEquals("journals.Length", 1, journals.Length);
			var revClearingJournal = journals[0];
			Assert("revClearingJournal should be a Journal", revClearingJournal is Journal);

			AssertEquals("Reversing ClearingJournal local amount", 10M, revClearingJournal.AH_InvoiceAmount);
			AssertEquals("Reversing ClearingJournal outstanding amount = 0", 0M, revClearingJournal.AH_OutstandingAmount);
			Assert("Reversing ClearingJournal should be cancelled", revClearingJournal.AH_IsCancelled);
			AssertEquals("FullyPaidDate should be changed on unmatch date", expectedUnmatchDate, revClearingJournal.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should be changed on unmatch date", expectedUnmatchDate, revClearingJournal.AH_PostDate.Date);

			Assert("paymentMatch should be deleted", paymentMatch.IsDeleted);
			Assert("discountMatch should be deleted", discountMatch.IsDeleted);
			Assert("paymentMatch2 should be deleted", paymentMatch2.IsDeleted);
			Assert("overpaymentMatch should be deleted", overpaymentMatch.IsDeleted);
			Assert("clearingJournalMatch should be deleted", clearingJournalMatch.IsDeleted);

			var origPayMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment.PK);
			var origPayMatch = Factory.LoadTop1<TransactionMatchLink>(origPayMatchFilter);
			AssertEquals("Match date of new reversing matching should not be changed on unmatch date", ZDateTime.Today, origPayMatch.AP_MatchDate.Date);
			AssertEquals("OrigPay Match amount", 70M, origPayMatch.AP_Amount);

			var revPayMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revPay.PK);
			var revPayMatch = Factory.LoadTop1<TransactionMatchLink>(revPayMatchFilter);
			AssertEquals("RevPay Match amount", -70M, revPayMatch.AP_Amount);
			AssertEquals("Match date of new reversing matching should not be changed on unmatch date", ZDateTime.Today, revPayMatch.AP_MatchDate.Date);

			var origDiscountMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, discount.PK);
			var origDiscountMatch = Factory.LoadTop1<TransactionMatchLink>(origDiscountMatchFilter);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, origDiscountMatch.AP_MatchDate.Date);
			AssertEquals("origDiscountMatch amount", -20M, origDiscountMatch.AP_Amount);

			var revDiscountMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revDiscount.PK);
			var revDiscountMatch = Factory.LoadTop1<TransactionMatchLink>(revDiscountMatchFilter);
			AssertEquals("revDiscountMatch amount", 20M, revDiscountMatch.AP_Amount);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, revDiscountMatch.AP_MatchDate.Date);

			var origOverpaymentMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, overpayment.PK);
			var origOverpaymentMatch = Factory.LoadTop1<TransactionMatchLink>(origOverpaymentMatchFilter);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, origOverpaymentMatch.AP_MatchDate.Date);
			AssertEquals("origOverpaymentMatch amount", -40M, origOverpaymentMatch.AP_Amount);

			var revOverpaymentMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revOverpayment.PK);
			var revOverpaymentMatch = Factory.LoadTop1<TransactionMatchLink>(revOverpaymentMatchFilter);
			AssertEquals("revOverpaymentMatch amount", 40M, revOverpaymentMatch.AP_Amount);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, revOverpaymentMatch.AP_MatchDate.Date);

			var origClearingJournalMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, clearingJournal.PK);
			var origClearingJournalMatch = Factory.LoadTop1<TransactionMatchLink>(origClearingJournalMatchFilter);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, origClearingJournalMatch.AP_MatchDate.Date);
			AssertEquals("origClearingJournalMatch amount", -10M, origClearingJournalMatch.AP_Amount);

			var revClearingJournalMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revClearingJournal.PK);
			var revClearingJournalMatch = Factory.LoadTop1<TransactionMatchLink>(revClearingJournalMatchFilter);
			AssertEquals("revClearingJournalMatch amount", 10M, revClearingJournalMatch.AP_Amount);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, revClearingJournalMatch.AP_MatchDate.Date);
		}

		#region TestCanTransactionBeReversed

		public void TestCanTransactionBeReversed()
		{
			TestIReversingInstance.SetIsClearedInCashbook(false);

			TestIReversingInstance.SetIsReversed(false);

			Factory.Save();
			PaymentReversing payReversing = new PaymentReversing((IPayment)TestIReversingInstance);
			Assert("Transaction should be reversable since it has not been reversed or cleared in cashbook", payReversing.CanReverseTransaction);
			AssertEquals("Error message should be empty", ZString.Empty, payReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			TestIReversingInstance.SetIsReversed(true);
			Assert("Transaction should not be reversable since it has already been reversed", !payReversing.CanReverseTransaction);
			AssertEquals("Error message should be already reversed", payReversing.AlreadyReversedErrorMessage_ForTestOnly, payReversing.CantReverseErrorMessage);

			TestIReversingInstance.SetIsReversed(false);
			TestIReversingInstance.SetIsClearedInCashbook(true);
			Assert("Transaction should not be reversable since it has been cleared in cashbook", !payReversing.CanReverseTransaction);
			AssertEquals("Error message should be cleared in cashbook", payReversing.ClearedInCashBookErrorMessage_ForTestOnly, payReversing.ClearedInCashBookErrorMessage_ForTestOnly);
		}

		#endregion

		public void TestReversedPaymentGenerateDDRBatch_ForDDL()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;

			var testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_OSExTaxAmount = 120m;
			testPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			Factory.Save();

			var dDRBatch = Factory.NewWithValidTestData((typeof(DirectDebitBatchHeader))) as DirectDebitBatchHeader;
			dDRBatch.AH_TransactionNum = ZString.Empty;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert(testPayment.IncludeInTheBatch);

			Factory.Save();

			AssertEquals(ReceiptTypes.DirectDebitLine, testPayment.AH_ReceiptType);
			AssertEquals(120m, testPayment.AH_OSTotal);
			AssertEquals(120m, dDRBatch.AH_OSTotal);

			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = true;

			var payReversing = new PaymentReversing(testPayment);
			payReversing.Reverse();
			Factory.Save();

			AssertReversedTransactionValues(testPayment, dDRBatch, ReceiptTypes.DirectDebitLine, ReceiptTypes.DirectDebit);
		}

		public void TestReversedPaymentGenerateDDRBatch_ForDDR()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = true;

			var testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_OSExTaxAmount = 120m;
			testPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			Factory.Save();

			var dDRBatch = Factory.NewWithValidTestData((typeof(DirectDebitBatchHeader))) as DirectDebitBatchHeader;
			dDRBatch.AH_TransactionNum = ZString.Empty;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert(testPayment.IncludeInTheBatch);

			Factory.Save();

			AssertEquals(ReceiptTypes.DirectDebit, testPayment.AH_ReceiptType);
			AssertEquals(120m, testPayment.AH_OSTotal);
			AssertEquals(120m, dDRBatch.AH_OSTotal);

			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;

			var payReversing = new PaymentReversing(testPayment);
			payReversing.Reverse();
			Factory.Save();

			AssertReversedTransactionValues(testPayment, dDRBatch, ReceiptTypes.DirectDebit, ReceiptTypes.NonRolledUpBatch);
		}

		public void TestReversedPaymentDoesNotIncludeOtherPayments()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = true;

			var testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_OSExTaxAmount = 120m;
			testPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			var nonInclusionPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_OSExTaxAmount = 120m;
			testPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			Factory.Save();

			var dDRBatch = Factory.NewWithValidTestData((typeof(DirectDebitBatchHeader))) as DirectDebitBatchHeader;
			dDRBatch.AH_TransactionNum = ZString.Empty;
			dDRBatch.AH_ReceiptType = ReceiptTypes.NonRolledUpBatch;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert(testPayment.IncludeInTheBatch);
			nonInclusionPayment.IncludeInTheBatch = false;

			Factory.Save();

			AssertEquals(120m, testPayment.AH_OSTotal);
			AssertEquals(120m, dDRBatch.AH_OSTotal);

			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;

			var payReversing = new PaymentReversing(testPayment);
			payReversing.Reverse();
			Factory.Save();

			AssertReversedTransactionValues(testPayment, dDRBatch, ReceiptTypes.DirectDebit, ReceiptTypes.NonRolledUpBatch);
			Assert(nonInclusionPayment.AH_ReceiptBatchNo.IsEmpty);
		}

		void AssertReversedTransactionValues(APPayment testPayment, DirectDebitBatchHeader dDRBatch, string originalReceiptType, string rolledUpBatchType)
		{
			// Check the Original Pay
			testPayment.Reload();
			Assert(testPayment.AH_IsCancelled);
			AssertEquals(originalReceiptType, testPayment.AH_ReceiptType);
			AssertEquals(dDRBatch.AH_TransactionNum, testPayment.AH_ReceiptBatchNo);

			// Check the Original DDR Batch
			dDRBatch.Reload();
			Assert(!dDRBatch.AH_IsCancelled);
			AssertEquals(120.0m, dDRBatch.AH_OSTotal);

			// Check the Reversed DDR Batch
			ZQuery newReversedDDRFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch);
			newReversedDDRFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, dDRBatch.PK);
			newReversedDDRFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, dDRBatch.AH_GC);

			DirectDebitBatchHeader[] createdDDR = Factory.Load(typeof(DirectDebitBatchHeader), newReversedDDRFilter) as DirectDebitBatchHeader[];
			AssertEquals(1, createdDDR.Length);
			Assert(createdDDR[0].PK != dDRBatch.PK);
			AssertEquals(TransactionTypes.DDRBatch, createdDDR[0].AH_TransactionType);
			AssertEquals(-dDRBatch.AH_OSTotal, createdDDR[0].AH_OSTotal);
			AssertEquals(rolledUpBatchType, createdDDR[0].AH_ReceiptType);

			// Check the Reversed Pay
			ZQuery newReversedPayFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
			newReversedPayFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testPayment.PK);
			newReversedPayFilter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, createdDDR[0].AH_TransactionNum);
			newReversedPayFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, createdDDR[0].AH_GC);

			Payment[] createdPayment = Factory.Load(typeof(APPayment), newReversedPayFilter) as Payment[];
			AssertEquals(1, createdPayment.Length);
			Assert(createdPayment[0].PK != testPayment.PK);
			AssertEquals(TransactionTypes.Payment, createdPayment[0].AH_TransactionType);
			AssertEquals(createdDDR[0].AH_TransactionNum, createdPayment[0].AH_ReceiptBatchNo);
			AssertEquals(testPayment.AH_ReceiptType, createdPayment[0].AH_ReceiptType);
			AssertEquals(-testPayment.AH_OSTotal, createdPayment[0].AH_OSTotal);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestCheckpointsToReverse_OverpaymentInMatchGroup()
		{
			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP1234", TestObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			apInvoice.AH_OH = TestObjectCreator.Debtor.PK;
			apInvoice.AH_LocalOutstandingAmount = 0M;

			var apReceipt = TestObjectCreator.CreateARReceipt(0m, 150m, apInvoice.AH_PostDate, apInvoice.AH_PostDate, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			apReceipt.AH_InvoiceAmount = apReceipt.AH_OSTotalAmount;
			apReceipt.AH_LocalOutstandingAmount = 0M;

			var overpayment = TestObjectCreator.CreateOverpayment<APOverpayment>(50m, apReceipt.AH_PostDate, TestObjectCreator.Debtor.PK);

			var invoiceMatchGroup = ((IMatching)apInvoice).CurrentMatchGroup.AddNew();
			invoiceMatchGroup.AP_AH = apInvoice.PK;
			invoiceMatchGroup.AP_Amount = 200m;
			invoiceMatchGroup.AP_MatchGroupNum = "M00001234";
			invoiceMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;

			var receiptMatchGroup = ((IMatching)apReceipt).CurrentMatchGroup.AddNew();
			receiptMatchGroup.AP_AH = apReceipt.PK;
			receiptMatchGroup.AP_Amount = -150m;
			receiptMatchGroup.AP_MatchGroupNum = "M00001234";
			receiptMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;

			var overpaymentMatchGroup = ((IMatching)overpayment).CurrentMatchGroup.AddNew();
			overpaymentMatchGroup.AP_AH = overpayment.PK;
			overpaymentMatchGroup.AP_Amount = -50m;
			overpaymentMatchGroup.AP_MatchGroupNum = "M00001234";
			overpaymentMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;

			Factory.Save();

			var reverser = ReversingFactory.NewReversing(apReceipt);
			AssertSequencesEqual("When match group with overpayment, there should be a(n additional) Checkpoint required to reverse", new Security.SecurityCheckpoint[] { Env.Security.PayablesUnMatchTransactionsOverpaymentType }, reverser.CheckpointsToReverse);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestCheckpointsToReverse_NoMiscTransactionsInMatchGroup()
		{
			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP1234", TestObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			apInvoice.AH_OH = TestObjectCreator.Debtor.PK;
			apInvoice.AH_LocalOutstandingAmount = 0M;

			var apReceipt = TestObjectCreator.CreateARReceipt(0m, 150m, apInvoice.AH_PostDate, apInvoice.AH_PostDate, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			apReceipt.AH_InvoiceAmount = apReceipt.AH_OSTotalAmount;
			apReceipt.AH_LocalOutstandingAmount = 0M;

			var invoiceMatchGroup = ((IMatching)apInvoice).CurrentMatchGroup.AddNew();
			invoiceMatchGroup.AP_AH = apInvoice.PK;
			invoiceMatchGroup.AP_Amount = -200m;
			invoiceMatchGroup.AP_MatchGroupNum = "M00001234";
			invoiceMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;

			var receiptMatchGroup = ((IMatching)apReceipt).CurrentMatchGroup.AddNew();
			receiptMatchGroup.AP_AH = apReceipt.PK;
			receiptMatchGroup.AP_Amount = -200m;
			receiptMatchGroup.AP_MatchGroupNum = "M00001234";
			receiptMatchGroup.AP_MatchDate = apReceipt.AH_PostDate;

			Factory.Save();

			var reverser = ReversingFactory.NewReversing(apReceipt);
			AssertSequencesEqual("When match group with without misc transactions, there should be no (additional) Checkpoints required to reverse", Array.Empty<Security.SecurityCheckpoint>(), reverser.CheckpointsToReverse);
		}

		protected override Type GetTestingClassType()
		{
			return typeof(PaymentReversing);
		}
	}
}
