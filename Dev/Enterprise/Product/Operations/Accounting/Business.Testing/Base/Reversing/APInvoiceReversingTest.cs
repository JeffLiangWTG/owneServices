using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class APInvoiceReversingTest : InvoicingBaseReversingTest
	{
		#region Implementation

		protected APInvoiceReversing APInvReversing
		{
			get { return Reversing as APInvoiceReversing; }
		}

		void SetupMatchedPaymentAndInvoiceData()
		{
			ZGuid groupingGuid = ZGuid.NewZGuid();
			APInv_Orig = Factory.NewWithValidTestData<APInvoice>();
			APInv_Orig.AH_TransactionBelongsToGroup = groupingGuid;
			APInv_Orig.AH_TransactionCount = 1;

			APPay_Orig = Factory.NewWithValidTestData<APPayment>();
			APPay_Orig.AH_TransactionBelongsToGroup = groupingGuid;
			APPay_Orig.AH_TransactionCount = 2;
		}

		APInvoice APInv_Orig;
		APPayment APPay_Orig;

		protected override Type GetTestingClassType()
		{
			return typeof(APInvoiceReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = Factory.NewWithValidTestData<APInvoiceForReversingTest>();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = Factory.NewWithValidTestData<APInvoiceForReversingTest>();
		}

		class APInvoiceForReversingTest : APInvoice, IPayablesAndReceivablesForTests
		{
			public APInvoiceForReversingTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IReversingForTests Members

			public void SetIsClearedInCashbook(bool value)
			{
				AH_DateClearedInCashbook = value ? ZDateTime.Now : ZDateTime.Empty;
			}

			public void SetIsReversed(bool value)
			{
				AH_IsCancelled = value;
			}

			public void SetReverseTransactionToBeGenerated(IReversing reverseTransaction)
			{
				fReverseTransaction = (TransactionHeader)reverseTransaction;
			}

			#endregion

			#region ITransactionForTests Members

			public void SetFactory(BusinessObjectFactory factory)
			{
			}

			public ZDateTime FullyPaidDate
			{
				get
				{
					return AH_FullyPaidDate;
				}
				set
				{
					AH_FullyPaidDate = value;
				}
			}

			#endregion

			#region IPayablesAndReceivablesForTests Members

			public void SetIsMatched(bool value)
			{
				if (((IMatching)this).IsMatched != value)
				{
					if (value)
					{
						AH_LocalOutstandingAmount = AH_LocalExTaxAmount + AH_LocalTaxAmount + 1M;
					}
					else
					{
						AH_LocalOutstandingAmount = AH_LocalExTaxAmount + AH_LocalTaxAmount;
					}
				}
			}

			public void SetMatchLinksToBeGenerated(TransactionMatchLinkGroup matchLinksToSet)
			{
				fCurrentMatchGroup = matchLinksToSet;
			}

			#endregion
		}

		#endregion

		public void TestCantReverseAPInvoiceWhenNotAllowedCreation()
		{
			AssertCreationOfReversalTransactionPayable(false);
			AssertCreationOfReversalTransactionPayable(true);
		}

		void AssertCreationOfReversalTransactionPayable(bool reverseIsPrevented)
		{
			using (AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reverseIsPrevented))
			{
				APInvoice originalInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, 1m, 1m, 1m, 1m, 1m, 1m);
				var reversing = new APInvoiceReversing(originalInvoice);
				AssertEquals("CanTransactionBeReversed", !reverseIsPrevented, reversing.CanTransactionBeReversed_ForTestOnly());
				if (reverseIsPrevented)
				{
					AssertEquals("GenerateCantReverseErrorMessage()", AccountingMasterFilesUtils.APInvoiceReversalDisallowedMessage, reversing.GenerateCantReverseErrorMessage_ForTestOnly());
				}
				else
				{
					AssertNotEquals("GenerateCantReverseErrorMessage()", AccountingMasterFilesUtils.APInvoiceReversalDisallowedMessage, reversing.GenerateCantReverseErrorMessage_ForTestOnly());
				}
			}
		}

		public void TestCantReverseJobConsolInvoice()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			cost.E6_AH_APInvoice = invoice.PK;
			cost.E6_AH_ARInvoice = ZGuid.NewZGuid();
			APInvoiceReversing reversing = new APInvoiceReversing(invoice);
			Assert(!reversing.CanReverseTransaction);
			cost.E6_AH_APInvoice = Guid.Empty;
			Assert(reversing.CanReverseTransaction);
		}

		public void TestDontReReversePayment()
		{
			ZGuid groupingGuid = ZGuid.NewZGuid();
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_TransactionBelongsToGroup = groupingGuid;
			aPInv.AH_TransactionCount = 1;
			aPInv.AH_FullyPaidDate = ZDateTime.Today.AddDays(-1);
			TransactionMatchLink aPInv_Link = ((IMatching)aPInv).CurrentMatchGroup.AddNew();
			aPInv_Link.AP_AH = aPInv.PK;
			aPInv_Link.AP_MatchGroupNum = "M00002045";

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_TransactionBelongsToGroup = groupingGuid;
			aPPay.AH_TransactionCount = 2;
			aPPay.AH_FullyPaidDate = ZDateTime.Today.AddDays(-1);

			TransactionMatchLink aPPay_Link = ((IMatching)aPInv).CurrentMatchGroup.AddNew();
			aPPay_Link.AP_AH = aPPay.PK;
			aPPay_Link.AP_MatchGroupNum = "M00002045";
			TestObjectCreator.SetupMatchLinkMatchDate(aPInv);

			Factory.Save();

			PaymentReversing paymentReverser = new PaymentReversing(aPPay);
			paymentReverser.Reverse();

			Assert("Payment should be reversed", aPPay.AH_IsCancelled);

			var paymentReverseTransaction = paymentReverser.ReverseTransaction as AccTransactionHeader;  
			paymentReverseTransaction.AH_TransactionNum = "TEST0003";
			Factory.Save();

			int dBCountBeforeReversingInvoice = Factory.GetDatabaseCount(typeof(TransactionHeader));

			APInvoiceReversing invoiceReverser = new APInvoiceReversing(aPInv);
			AssertNull("Shouldn't find a payment that is already reversed", invoiceReverser.PaymentToReverse);
			invoiceReverser.Reverse();

			var invoiceReverseTransaction = invoiceReverser.ReverseTransaction as AccTransactionHeader;
			invoiceReverseTransaction.AH_TransactionNum = "TEST0004";
			Factory.Save();

			int dBCountAfterReversingInvoice = Factory.GetDatabaseCount(typeof(TransactionHeader));

			AssertEquals("Should have only created one new transaction", 1, dBCountAfterReversingInvoice - dBCountBeforeReversingInvoice);
		}

		#region CanReverseTransaction Test

		public void TestCanReverseTransaction()
		{
			ZGuid groupGuid = ZGuid.NewZGuid();
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_TransactionBelongsToGroup = groupGuid;
			aPInv.AH_TransactionCount = 1;
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(aPInv);

			Assert("Should be able to reverse transaction", Reversing.CanReverseTransaction);

			aPInv.AH_OSExTaxAmount = 10m;
			aPInv.AH_LocalExTaxAmount = 10m;
			aPInv.AH_LocalOutstandingAmount = 0m;

			Reversing = ReversingFactory.NewReversing(aPInv);
			Assert("Should not be able to reverse transaction because it is already matched", !Reversing.CanReverseTransaction);

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_TransactionCount = 2;
			aPPay.AH_TransactionBelongsToGroup = groupGuid;
			aPPay.AH_IsCancelled = false;

			Reversing = ReversingFactory.NewReversing(aPInv);
			Assert("Should NOT be able to reverse transaction because it is matched to a payment", !Reversing.CanReverseTransaction);

			//Below code is testing logic that is not use anymore after WI00018491 removed ability to auto reverse linked payments. 
			//Ideally, we should remove all payment reversing code and below message from production code. Until that to keep that message tested, we do hack and restore outstanding about to overcome IsMatched restriction.
			aPInv.AH_LocalOutstandingAmount = 10m;
			aPPay.AH_DateClearedInCashbook = ZDateTime.BrettsBirthday;

			Reversing = ReversingFactory.NewReversing(aPInv);
			Assert("Should not be able to reverse transaction because it is cleared in Cashbook", !Reversing.CanReverseTransaction);

			string errorMessage = "This transaction cannot be reversed because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing.";
			AssertEquals("Error message must be as etalon", errorMessage, Reversing.CantReverseErrorMessage);
		}

		#endregion

		#region PaymentToReverse Test

		public void TestPaymentToReverse()
		{
			SetupMatchedPaymentAndInvoiceData();
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			AssertNotNull("Match Group is 1-to-1", APInvReversing.PaymentToReverse);
			AssertEquals("APPay_Orig should be loaded", APPay_Orig.PK, APInvReversing.PaymentToReverse.PK);

			APInvoice aPInv2 = Factory.NewWithValidTestData<APInvoice>();
			aPInv2.AH_TransactionBelongsToGroup = ZGuid.Empty;

			APPayment aPPay2 = Factory.NewWithValidTestData<APPayment>();
			aPPay2.AH_TransactionBelongsToGroup = ZGuid.Empty;

			Factory.Save();

			Reversing = ReversingFactory.NewReversing(aPInv2);
			AssertNull("Match Group is not 1-to-1", APInvReversing.PaymentToReverse);
		}

		#endregion

		#region ReversePaymentMatchedToInvoice Test

		public void TestReversePaymentMatchedToInvoice()
		{
			ZGuid groupingGuid = ZGuid.NewZGuid();
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_TransactionBelongsToGroup = groupingGuid;
			aPInv.AH_TransactionCount = 1;
			aPInv.AH_FullyPaidDate = ZDateTime.Today.AddDays(-1);

			TransactionMatchLink aPInv_Link = ((IMatching)aPInv).CurrentMatchGroup.AddNew();
			aPInv_Link.AP_AH = aPInv.PK;
			aPInv_Link.AP_MatchGroupNum = "M00002045";

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_TransactionBelongsToGroup = groupingGuid;
			aPPay.AH_TransactionCount = 2;
			aPPay.AH_FullyPaidDate = ZDateTime.Today.AddDays(-1);

			TransactionMatchLink aPPay_Link = ((IMatching)aPInv).CurrentMatchGroup.AddNew();
			aPPay_Link.AP_AH = aPPay.PK;
			aPPay_Link.AP_MatchGroupNum = "M00002045";
			TestObjectCreator.SetupMatchLinkMatchDate(aPInv);

			Factory.Save();

			APInvoiceReversing invoiceReverser = new APInvoiceReversing(aPInv);
			invoiceReverser.Reverse();
			invoiceReverser.ReverseTransaction.TransactionNumber = "00001000";
			Factory.Save();

			aPInv = invoiceReverser.OriginalTransaction as APInvoice;
			Assert("Invoice should be cancelled", aPInv.AH_IsCancelled);
			Assert("Invoice should be fullypaid", !aPInv.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Invoice should have the same grouping guid", groupingGuid, aPInv.AH_TransactionBelongsToGroup);

			APCreditNote reversingAPCrd = invoiceReverser.ReverseTransaction as APCreditNote;
			Assert("Invoice should be cancelled", reversingAPCrd.AH_IsCancelled);
			Assert("Invoice should be fullypaid", !reversingAPCrd.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Invoice should have the same grouping guid", groupingGuid, reversingAPCrd.AH_TransactionBelongsToGroup);
			AssertEquals("Transaction count on reversing creditnote should be 3", (byte)3, reversingAPCrd.AH_TransactionCount);

			ZQuery matchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, reversingAPCrd.PK);
			TransactionMatchLink revAPCrdLink = invoiceReverser.OriginalTransaction.Factory.LoadTop1<TransactionMatchLink>(matchLinkFilter);
			AssertNotNull("There should be a matchlink for the reversing creditnote", revAPCrdLink);
			AssertEquals("The matchlink should be in M00002045", "M00002045", revAPCrdLink.AP_MatchGroupNum);

			APPayment originalAPPay = invoiceReverser.PaymentToReverse as APPayment;
			Assert("Payment matched to invoice should be cancelled", originalAPPay.AH_IsCancelled);
			Assert("Payment matched to invoice should be fullypaid", !originalAPPay.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Original payment should have the same grouping guid", groupingGuid, originalAPPay.AH_TransactionBelongsToGroup);

			APPayment reversingAPPay = invoiceReverser.ReversingPayment as APPayment;
			AssertNotNull("There should be a reversing payment", reversingAPPay);
			Assert("Reversing payment should be cancelled", reversingAPPay.AH_IsCancelled);
			Assert("Reversing payment should be fullypaid", !reversingAPPay.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Reversing payment should have the same grouping guid", groupingGuid, reversingAPPay.AH_TransactionBelongsToGroup);
			AssertEquals("Transaction count on reversing payment should be 4", (byte)4, reversingAPPay.AH_TransactionCount);

			matchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, reversingAPPay.PK);
			TransactionMatchLink revAPPayLink = invoiceReverser.OriginalTransaction.Factory.LoadTop1<TransactionMatchLink>(matchLinkFilter);
			AssertNotNull("There should be a matchlink for the reversing payment", revAPPayLink);
			AssertEquals("The matchlink should be in M00002045", "M00002045", revAPPayLink.AP_MatchGroupNum);
		}

		#endregion

		#region GenerateReverseTransactions Test

		public void TestGenerateReverseTransactions()
		{
			SetupMatchedPaymentAndInvoiceData();
			APPay_Orig.AH_OSExTaxAmount = 10m;
			APPay_Orig.AH_LocalExTaxAmount = 10m;
			APPay_Orig.AH_LocalOutstandingAmount = 0m;

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			AssertNotNull("PaymentToReverse should not be null", APInvReversing.PaymentToReverse);
			APPayment reversingAPPay = APInvReversing.ReversingPayment as APPayment;
			AssertNotNull("ReversingPayment should not be null", reversingAPPay);
			AssertEquals("Amount on Reversing Payment should be -10", -10m, reversingAPPay.AH_InvoiceAmount);

			APInv_Orig = Factory.NewWithValidTestData<APInvoice>();
			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
		}

		#endregion

		#region SetCancellationFlagOnTransactionsToReverse Test

		public void TestSetCancellationFlagOnTransactionsToReverse()
		{
			SetupMatchedPaymentAndInvoiceData();
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();

			APPay_Orig.AH_IsCancelled = false;
			APInvReversing.ReversingPayment.SetCancellationFlag(false);

			APInvReversing.SetCancellationFlagOnTransactionsToReverse_ForTestOnly();
			Assert("Original Payment should be cancelled", APPay_Orig.AH_IsCancelled);
			Assert("Original Invoice should be cancelled", APInv_Orig.AH_IsCancelled);
			Assert("Reversing Payment should be cancelled", (APInvReversing.ReversingPayment as APPayment).AH_IsCancelled);
			Assert("Reversing CreditNote should be cancelled", (APInvReversing.ReverseTransaction as APCreditNote).AH_IsCancelled);

			APInv_Orig = Factory.NewWithValidTestData<APInvoice>();
			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.SetCancellationFlagOnTransactionsToReverse_ForTestOnly();
			Assert("Original invoice should be cancelled", APInvReversing.InvoiceToReverse_ForTestOnly.AH_IsCancelled);
			Assert("Reversing credit note should be cancelled", APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_IsCancelled);
		}

		#endregion

		#region SetReversingDescriptionOnTransactions Test

		public void TestSetReversingDescriptionOnTransactions()
		{
			SetupMatchedPaymentAndInvoiceData();
			APPay_Orig.IsManuallySetTransactionNumber_ForTestOnly = true;
			APPay_Orig.AH_TransactionNum = "00004999";
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.SetReversingDescriptionOnTransactions_ForTestOnly();
			AssertEquals("Reversing Description on Reversing Payment", "REVERSAL RELATED TO 00004999",
				APInvReversing.ReversingAPPayment_ForTestOnly.AH_Desc);

			APInv_Orig = Factory.NewWithValidTestData<APInvoice>();
			APInv_Orig.AH_TransactionNum = "00002859";
			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.SetReversingDescriptionOnTransactions_ForTestOnly();
			AssertEquals("Description should be set on reversing creditnote", "REVERSAL RELATED TO 00002859",
				APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_Desc);
		}

		#endregion

		#region SetReversingPaymentPostDate Test

		public void TestSetReversingPaymentPostDate()
		{
			ZDateTime expectedPostDate = ZDateTime.Now.AddDays(-2);

			SetupMatchedPaymentAndInvoiceData();

			APPay_Orig.AH_TransactionNum = "00000999";
			APInv_Orig.AH_PostDate = expectedPostDate.AddDays(-11);
			APPay_Orig.AH_PostDate = APInv_Orig.AH_PostDate;

			TransactionMatchLink aPInv_Link = ((IMatching)APInv_Orig).CurrentMatchGroup.AddNew();
			aPInv_Link.AP_AH = APInv_Orig.PK;
			aPInv_Link.AP_MatchGroupNum = "M00002232";
			TransactionMatchLink aPPay_Link = ((IMatching)APInv_Orig).CurrentMatchGroup.AddNew();
			aPPay_Link.AP_AH = APPay_Orig.PK;
			aPPay_Link.AP_MatchGroupNum = "M00002232";
			TestObjectCreator.SetupMatchLinkMatchDate(APInv_Orig);
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.GetMatchLinksFromTransactions_ForTestOnly();

			APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_PostDate = expectedPostDate;
			APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_TransactionNum = "00001000";

			APInvReversing.SetMatchGroupNumberAndMatchDateOnSaving_ForTestOnly(Factory);

			Factory.Save();

			AssertEquals("Reversing Payment should have the same post date as reversing CreditNote", APInvReversing.ReversingAPPayment_ForTestOnly.AH_PostDate, expectedPostDate);

			AssertEquals("Expected transaction count", 2, APInvReversing.ReverseTransactionMatchLinks_ForTestOnly.Count);
			AssertEquals("Expected transaction count", 2, APInvReversing.OriginalTransactionMatchLinks_ForTestOnly.Count);
			foreach (TransactionMatchLink link in APInvReversing.ReverseTransactionMatchLinks_ForTestOnly)
			{
				AssertEquals("AP_MatchDate", expectedPostDate, link.AP_MatchDate);
			}
			foreach (TransactionMatchLink link in APInvReversing.OriginalTransactionMatchLinks_ForTestOnly)
			{
				AssertEquals("AP_MatchDate", expectedPostDate, link.AP_MatchDate);
			}
		}

		#endregion

		public void TestAlreadyReversedTAPTransactionWillNotThrowNullExceptionWhenReverseAgain()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "12341234", TestObjectCreator.AUD, 1.0m, 1000.00m, 100.00m, 1000.00m, 100.00m);
			invoice.AH_TransactionCategory = "TAP";
			invoice.AH_IsCancelled = true;

			Reversing = ReversingFactory.NewReversing(invoice);

			Assert("CanReverseTransaction when rights for transaction with related paid invoice.", !APInvReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage should not throw a null ref exception.", @"This TAP/TNF Journal cannot be reversed.
TAP/TNF journals are created in a pair. Reversing one part will result in an imbalance in the Clearing GL Account.", APInvReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		#region SetTransactionBelongsToGroup Test

		public void TestSetTransactionBelongsToGroup()
		{
			SetupMatchedPaymentAndInvoiceData();
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.ReversingAPPayment_ForTestOnly.AH_TransactionBelongsToGroup = ZGuid.Empty;
			APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_TransactionBelongsToGroup = ZGuid.Empty;

			APInvReversing.SetTransactionBelongsToGroupOnTransactionsToReverse_ForTestOnly();

			ZGuid groupingGuid = APInv_Orig.AH_TransactionBelongsToGroup;
			AssertEquals("Grouping Guid should be set on APPay_Orig", groupingGuid, APPay_Orig.AH_TransactionBelongsToGroup);
			AssertEquals("Grouping Guid should be set on reversing payment", groupingGuid, APInvReversing.ReversingAPPayment_ForTestOnly.AH_TransactionBelongsToGroup);
			AssertEquals("Grouping Guid should be set on reversing creditnote", groupingGuid, APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_TransactionBelongsToGroup);

			APInv_Orig = Factory.NewWithValidTestData<APInvoice>();
			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.SetTransactionBelongsToGroupOnTransactionsToReverse_ForTestOnly();

			AssertEquals("GroupingGuid on reversing creditnote should be PK of original invoice", APInv_Orig.PK, APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_TransactionBelongsToGroup);
		}

		#endregion

		#region FullyPayBothTransactions Test

		public void TestFullyPayBothTransactions()
		{
			SetupMatchedPaymentAndInvoiceData();

			TransactionLine aPInv_OrigLine = APInv_Orig.Lines.AddNew();
			aPInv_OrigLine.AL_OSExTaxAmount = 10m;
			aPInv_OrigLine.AL_LocalExTaxAmount = 10m;
			APInv_Orig.AH_LocalOutstandingAmount = 0m;
			APInv_Orig.AH_PostDate = ZDateTime.Now.AddHours(-5);

			APPay_Orig.AH_OSExTaxAmount = 10m;
			APPay_Orig.AH_LocalExTaxAmount = 10m;
			APPay_Orig.AH_LocalOutstandingAmount = 0m;

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.APPaymentToReverse_ForTestOnly.AH_FullyPaidDate = APInv_Orig.AH_PostDate;
			APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_PostDate = APInv_Orig.AH_PostDate.AddHours(2);
			APInvReversing.ReversingAPPayment_ForTestOnly.AH_PostDate = APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_PostDate;
			APInvReversing.FullyPayBothTransactions_ForTestOnly();

			Assert("Fullypaid date should be set on reversing payment", !APInvReversing.ReversingAPPayment_ForTestOnly.AH_FullyPaidDate.IsEmpty);
			Assert("Fullypaid date should be set on reversing creditnote", !APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Fullypaid date should be as post date", APInvReversing.ReversingAPPayment_ForTestOnly.AH_PostDate, APInvReversing.ReversingAPPayment_ForTestOnly.AH_FullyPaidDate);
			AssertEquals("Fullypaid date should be as post date", APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_PostDate, APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_FullyPaidDate);
			Assert("Fullypaid date should be set on reversing payment", !APInvReversing.APPaymentToReverse_ForTestOnly.AH_FullyPaidDate.IsEmpty);
			Assert("Fullypaid date should be set on reversing creditnote", !APInvReversing.InvoiceToReverse_ForTestOnly.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Fullypaid date should be as post date", APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_PostDate, APInvReversing.APPaymentToReverse_ForTestOnly.AH_FullyPaidDate);
			AssertEquals("Fullypaid date should be as post date", APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_PostDate, APInvReversing.InvoiceToReverse_ForTestOnly.AH_FullyPaidDate);

			APInv_Orig = Factory.NewWithValidTestData<APInvoice>();
			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInv_Orig.AH_PostDate = ZDateTime.Now.AddHours(-5);
			APInvReversing.FullyPayBothTransactions_ForTestOnly();
			Assert("Fullypaid date should be set on original invoice", !APInvReversing.InvoiceToReverse_ForTestOnly.AH_FullyPaidDate.IsEmpty);
			Assert("Fullypaid date should be set on reversing creditnote", !APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Fullypaid date should be as post date", APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_PostDate, APInvReversing.InvoiceToReverse_ForTestOnly.AH_FullyPaidDate);
			AssertEquals("Fullypaid date should be as post date", APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_PostDate, APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_FullyPaidDate);
		}

		#endregion

		#region GetMatchLinksFromTransactions Test

		public void TestGetMatchLinksFromTransactions()
		{
			SetupMatchedPaymentAndInvoiceData();
			TransactionMatchLink aPInv_Link = ((IMatching)APInv_Orig).CurrentMatchGroup.AddNew();
			aPInv_Link.AP_AH = APInv_Orig.PK;
			aPInv_Link.AP_MatchGroupNum = "M00002232";
			TransactionMatchLink aPPay_Link = ((IMatching)APInv_Orig).CurrentMatchGroup.AddNew();
			aPPay_Link.AP_AH = APPay_Orig.PK;
			aPPay_Link.AP_MatchGroupNum = "M00002232";
			TestObjectCreator.SetupMatchLinkMatchDate(APInv_Orig);
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.GetMatchLinksFromTransactions_ForTestOnly();

			AssertEquals("There should be 2 matchlinks in OriginalTransactionMatchlinks", 2, APInvReversing.OriginalTransactionMatchLinks_ForTestOnly.Count);
			AssertEquals("There should be 2 matchlinks in ReverseTransactionMatchlinks", 2, APInvReversing.ReverseTransactionMatchLinks_ForTestOnly.Count);

			APInvReversing.ReversePayablesAndReceivables_ForTestOnly.Matchlinks.Load();
			APInvReversing.ReversingPayment.Matchlinks.Load();
			AssertEquals("Reversing creditnote should have a matchlink", 1, APInvReversing.ReversePayablesAndReceivables_ForTestOnly.Matchlinks.Count);
			AssertEquals("Reversing payment should have a matchlink", 1, APInvReversing.ReversingPayment.Matchlinks.Count);

			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, APInvReversing.ReversingAPCreditNote_ForTestOnly.PK);
			AssertEquals("Reversing matchlinks should contain matchlink for reversing creditnote", 1,
				APInvReversing.ReverseTransactionMatchLinks_ForTestOnly.Find(filter).Length);
			filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, APInvReversing.ReversingAPPayment_ForTestOnly.PK);
			AssertEquals("Reversing matchlinks should contain matchlink for reversing payment", 1,
				APInvReversing.ReverseTransactionMatchLinks_ForTestOnly.Find(filter).Length);

			APInv_Orig = Factory.NewWithValidTestData<APInvoice>();
			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.GetMatchLinksFromTransactions_ForTestOnly();
			AssertEquals("OriginalMatchlinks should contain 1 matchlink", 1, APInvReversing.OriginalTransactionMatchLinks_ForTestOnly.Count);
			AssertEquals("ReversingMatchlinks should contain 1 matchlink", 1, APInvReversing.OriginalTransactionMatchLinks_ForTestOnly.Count);
		}

		#endregion

		#region SetTransactionCount Test

		public void TestSetTransactionCount()
		{
			SetupMatchedPaymentAndInvoiceData();
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.SetTransactionCount_ForTestOnly();
			AssertEquals("Transaction count on reversing creditnote should be 3", (byte)3, APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_TransactionCount);
			AssertEquals("Transaction count on reversing payment should be 4", (byte)4, APInvReversing.ReversingAPPayment_ForTestOnly.AH_TransactionCount);

			AssertEquals("Transaction count on original invoice should be 1", (byte)1, APInvReversing.InvoiceToReverse_ForTestOnly.AH_TransactionCount);
		}

		#endregion

		#region SetMatchGroupNumberAndDateOnSaving

		public void TestSetMatchGroupNumberAndDateOnSaving()
		{
			SetupMatchedPaymentAndInvoiceData();
			TransactionMatchLink aPInv_Link = ((IMatching)APInv_Orig).CurrentMatchGroup.AddNew();
			aPInv_Link.AP_AH = APInv_Orig.PK;
			aPInv_Link.AP_MatchGroupNum = "M00003950";
			TransactionMatchLink aPPay_Link = ((IMatching)APInv_Orig).CurrentMatchGroup.AddNew();
			aPPay_Link.AP_AH = APPay_Orig.PK;
			aPPay_Link.AP_MatchGroupNum = "M00003950";
			TestObjectCreator.SetupMatchLinkMatchDate(APInv_Orig);
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(APInv_Orig);
			APInvReversing.GenerateReverseTransactions_ForTestOnly();
			APInvReversing.GetMatchLinksFromTransactions_ForTestOnly();
			APInvReversing.SetMatchGroupNumberAndMatchDateOnSaving_ForTestOnly(Factory);
			foreach (TransactionMatchLink link in APInvReversing.ReverseTransactionMatchLinks_ForTestOnly)
			{
				AssertEquals("MatchGroup should be M00003950", "M00003950", link.AP_MatchGroupNum);
				AssertZDatesWithin5Minutes("AP_MatchDate", ZDateTime.Now, link.AP_MatchDate);
			}
			foreach (TransactionMatchLink link in APInvReversing.OriginalTransactionMatchLinks_ForTestOnly)
			{
				AssertEquals("MatchGroup should be M00003950", "M00003950", link.AP_MatchGroupNum);
				AssertZDatesWithin5Minutes("AP_MatchDate", ZDateTime.Now, link.AP_MatchDate);
			}
		}

		#endregion

		#region Reversing Payment along with Invoice

		public void TestReversingPayment()
		{
			SetupMatchedPaymentAndInvoiceData();

			APInv_Orig.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			APPay_Orig.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			APPay_Orig.AH_ReceiptType = ReceiptTypes.DirectDebit;

			DirectDebitBatchHeader dDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			Factory.Save();

			APPay_Orig.AH_ReceiptBatchNo = dDRBatch.AH_TransactionNum;
			Factory.Save();

			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();
			DirectDebitBatchHeader retrievedDDRBatch = readOnlyFactory.Load(typeof(DirectDebitBatchHeader), dDRBatch.PK) as DirectDebitBatchHeader;

			AssertEquals(1, retrievedDDRBatch.Lines.Count);
			AssertEquals(APPay_Orig.PK, ((BusinessObject)retrievedDDRBatch.Lines[0]).PK);

			string reverseBatchNo = AccountingNumberFountainWrapperFactory.Instance.DDRBatchNo.PeekPreliminary(Factory);

			Db.Connection.BeginTransaction();
			try
			{
				APInvoiceReversing invoiceReverser = new APInvoiceReversing(APInv_Orig);
				invoiceReverser.GenerateReverseTransactions_ForTestOnly();
				invoiceReverser.SetOtherNumberFountainFields_ForTestOnly(Factory);

				AssertNotNull(invoiceReverser.FReversingPayment_ForTestOnly);
				AssertNotNull(invoiceReverser.FDirectDebitBatchHeaderToReverse_ForTestOnly);

				AssertEquals(reverseBatchNo, ((Payment)invoiceReverser.FReversingPayment_ForTestOnly).AH_ReceiptBatchNo);
				AssertEquals(reverseBatchNo, invoiceReverser.FDirectDebitBatchHeaderToReverse_ForTestOnly.AH_TransactionNum);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestReversingPaymentDoNotGenerateDDRIfNotBatchedUp()
		{
			SetupMatchedPaymentAndInvoiceData();

			APInv_Orig.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			APPay_Orig.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			APPay_Orig.AH_ReceiptType = ReceiptTypes.DirectDebit;

			Factory.Save();

			Db.Connection.BeginTransaction();
			try
			{
				APInvoiceReversing invoiceReverser = new APInvoiceReversing(APInv_Orig);
				invoiceReverser.GenerateReverseTransactions_ForTestOnly();
				invoiceReverser.SetOtherNumberFountainFields_ForTestOnly(Factory);

				AssertNotNull(invoiceReverser.FReversingPayment_ForTestOnly);
				AssertNull(invoiceReverser.FDirectDebitBatchHeaderToReverse_ForTestOnly);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		#endregion

		#region GetMatchGroupNumber Test

		public void TestGetMatchGroupNumber()
		{
			APInvoice testAPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvoice.AH_OSExTaxAmount = 140m;

			Reversing = ReversingFactory.NewReversing(testAPInvoice);

			try
			{
				Db.Connection.BeginTransaction();
				APInvReversing.GenerateReverseTransactions_ForTestOnly();
				APInvReversing.GetMatchLinksFromTransactions_ForTestOnly();
				APInvReversing.SetMatchGroupNumberAndMatchDateOnSaving_ForTestOnly(Factory);

				((IMatching)APInvReversing.OriginalTransaction).Matchlinks.Load();
				((IMatching)APInvReversing.ReverseTransaction).Matchlinks.Load();
				AssertNotNull("Original Transaction Has Matchlinks", APInvReversing.OriginalTransaction.LatestMatchLink);
				AssertNotNull("Reverse Transaction Has Matchlinks", ((TransactionHeader)APInvReversing.ReverseTransaction).LatestMatchLink);
				Assert(APInvReversing.OriginalTransaction.LatestMatchLink.AP_MatchGroupNum ==
					((TransactionHeader)APInvReversing.ReverseTransaction).LatestMatchLink.AP_MatchGroupNum);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestReversingWithMatchLink()
		{
			APInvoice testAPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvoice.AH_OSExTaxAmount = 140m;

			Factory.Save();

			Reversing = ReversingFactory.NewReversing(testAPInvoice);

			Reversing.Reverse();
			APInvReversing.ReversingAPCreditNote_ForTestOnly.AH_TransactionNum = "00001000";

			Factory.Save();

			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();
			ZQuery matchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testAPInvoice.PK);

			TransactionMatchLink[] result = readOnlyFactory.Load(typeof(TransactionMatchLink), matchLinkFilter) as TransactionMatchLink[];
			AssertEquals(1, result.Length);
			Assert(!result[0].AP_MatchGroupNum.IsEmpty);

			matchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, result[0].AP_MatchGroupNum);
			result = readOnlyFactory.Load(typeof(TransactionMatchLink), matchLinkFilter) as TransactionMatchLink[];
			AssertEquals(2, result.Length);

			Assert(result[0].AP_MatchGroupNum == result[1].AP_MatchGroupNum);
		}

		#endregion

		public void TestReverseInvoiceLinkedToPurchaseOrder()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_InvoiceNumber = "111";
			order.APH_InvoiceDate = ZDate.Today;
			var invoice = order.PopulateInvoiceFromOrder();
			invoice.Factory.Save();

			var invoiceInCurrentFactory = Factory.Load<APInvoice>(invoice.PK);
			var reversing = new APInvoiceReversing(invoiceInCurrentFactory);
			reversing.Reverse();

			AssertEquals(true, invoiceInCurrentFactory.IsReversed);

			var reloadedOrder = Factory.Load<AccPayableOrderHeader>(order.PK);
			var sIVEventLogs = reloadedOrder.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code));
			foreach (var log in sIVEventLogs)
			{
				AssertEquals(true, log.IsCancelled);
			}
			AssertEquals(ZGuid.Empty, reloadedOrder.APH_AH);
			AssertEquals("ITP", reloadedOrder.APH_Disposition);
			AssertEquals(ZString.Empty, reloadedOrder.APH_InvoiceNumber);
		}

		public void TestGetReverseConfirmationMessage()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.OrderLines.AddNew();
			order.APH_InvoiceNumber = "111";
			order.APH_InvoiceDate = ZDate.Today;
			Factory.Save();
			var invoice = order.PopulateInvoiceFromOrder();
			invoice.Factory.Save();

			var invoiceInCurrentFactory = Factory.Load<APInvoice>(invoice.PK);
			var reversing = new APInvoiceReversing(invoiceInCurrentFactory);
			var warningMessageText = string.Format("You are about to reverse an invoice generated from a purchase order. Do you want to proceed?\r\n\r\nRelated Purchase Order Details:\r\n\r\nPurchase Order Number : {0}\r\nStage : Receive\r\nDisposition : Pending Goods Received Audit\r\nCreated Time : {1}\r\nCreated By : ", order.APH_OrderNumber, order.APH_SystemCreateTimeUtc);
			AssertEquals(warningMessageText, reversing.GetReverseConfirmationMessage());
		}

		public void TestShouldShowReverseConfirmationMessage()
		{
			var invoiceNotGeneratedFromPurchaseOrder = Factory.NewWithValidTestData<APInvoice>();
			var reversing1 = new APInvoiceReversing(invoiceNotGeneratedFromPurchaseOrder);
			AssertEquals("Reverse confirmation Message should not be shown to invoices that are not generated from purchase order", false, reversing1.ShouldShowReverseConfirmationMessage());

			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_InvoiceNumber = "111";
			order.APH_InvoiceDate = ZDate.Today;
			var invoice = order.PopulateInvoiceFromOrder();
			invoice.Factory.Save();

			var invoiceInCurrentFactory = Factory.Load<APInvoice>(invoice.PK);
			var reversing2 = new APInvoiceReversing(invoiceInCurrentFactory);
			AssertEquals("Reverse confirmation Message should be shown to invoices that are generated from purchase order", true, reversing2.ShouldShowReverseConfirmationMessage());
		}
	}
}
