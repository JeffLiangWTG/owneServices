using System;
using System.Data;
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
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class ReceiptReversingTest : PayablesAndReceivablesReversingTest
	{
		public override void TestCantReverseMatchedIPayablesAndReceivables()
		{
			TestIPayablesAndReceivables.SetIsMatched(true);
			TestIPayablesAndReceivables.SetIsReversed(false);
			Assert("Should be allowed to reverse matched receipt as it gets unmatched automatically", ReceiptReversing.CanReverseTransaction);
			AssertEquals("Error message should be emtpy", ZString.Empty, ReceiptReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		#region TestCanTransactionBeReversed

		public void TestCanTransactionBeReversed()
		{
			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_DateClearedInCashbook = ZDateTime.Empty;
			aRRec.AH_IsCancelled = false;
			//ARRec.AH_LocalExTaxAmount = 0m;
			//ARRec.AH_LocalTaxAmount = 0m;
			//ARRec.AH_LocalOutstandingAmount = 0m;
			Factory.Save();

			ReceiptReversing recReversing = new ReceiptReversing(aRRec);
			Assert("Receipt should be reversable", recReversing.CanReverseTransaction);

			aRRec.AH_DateClearedInCashbook = ZDateTime.Now;
			Assert("Receipt should not be reversable since it is cleared in cashbook", !recReversing.CanReverseTransaction);

			aRRec.AH_DateClearedInCashbook = ZDateTime.Empty;
			Assert("Receipt should be reversable", recReversing.CanReverseTransaction);

			aRRec.AH_IsCancelled = true;
			Assert("Receipt should not be reversable since it is already reversed", !recReversing.CanReverseTransaction);

			aRRec.AH_IsCancelled = false;
			Assert("Receipt should be reversable", recReversing.CanReverseTransaction);

			//ARRec.AH_LocalOutstandingAmount = 10m;
			//Assert("Receipt should not be reversable since it is matched", !RecReversing.CanReverseTransaction);
		}

		#endregion

		#region TestReverseDirectCreditReceipt

		public void TestReverseDirectCreditReceipt()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DepositBatch testDepositBatch = factory.NewWithValidTestData<DepositBatch>();
			testDepositBatch.AH_TransactionNum = "00001580";
			testDepositBatch.AH_ReceiptBatchNo = "00001580";
			factory.Save();

			ARReceipt testReceipt = factory.NewWithValidTestData<ARReceipt>();
			testReceipt.AH_InvoiceAmount = -30M;
			testReceipt.AH_OSExTaxAmount = 30M;
			testReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			testReceipt.AH_ReceiptBatchNo = "00001580";
			factory.Save();

			testReceipt.Reload();

			ZQuery depBatFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testReceipt.AH_ReceiptBatchNo);
			depBatFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			depBatFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, testReceipt.AH_GC);
			DepositBatch testDepBat = factory.LoadTop1<DepositBatch>(depBatFilter);

			ReceiptReversing testRecRev = new ReceiptReversing(testReceipt);
			testRecRev.Reverse();
			factory.Save();

			// Check the reversing Receipt
			ZQuery revRecFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Receipt);
			revRecFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testReceipt.PK);
			revRecFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, testReceipt.AH_GC);

			TransactionHeaderCollection receipts = new TransactionHeaderCollection(factory, revRecFilter);
			receipts.Load();
			AssertEquals("There should be one reversing receipt in the DB other than the original receipt", 1, receipts.Count);
			Receipt revRec = (Receipt)receipts[0];
			AssertEquals("Reversing Receipt should have local amount = 30", 30M, revRec.AH_InvoiceAmount);
			AssertEquals("Reversing Receipt should have os amount = 30", 30M, revRec.AH_OSTotal);

			// Check the reversing deposit batch
			ZQuery revDepBatFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			revDepBatFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testDepBat.PK);
			revDepBatFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, testDepBat.AH_GC);

			TransactionHeaderCollection depBats = new TransactionHeaderCollection(factory, revDepBatFilter);
			depBats.Load();
			AssertEquals("There should be one reversing deposit batch in the DB other than the original deposit batch", 1, depBats.Count);
			DepositBatch revDepBat = (DepositBatch)depBats[0];
			AssertEquals("Reversing DepositBatch should have local amount = -30", -30M, revDepBat.AH_InvoiceAmount);
			AssertEquals("Reversing DepositBatch should have os amount = -30", -30M, revDepBat.AH_OSTotal);
		}

		#endregion

		#region TestReverseOnceMatchedNonDirectCreditReceipt

		public void TestReverseOnceMatchedNonDirectCreditReceipt()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader testOrg = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			ARReceipt aRRec = factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OH = testOrg.PK;
			aRRec.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aRRec.AH_OSExTaxAmount = 10M;
			aRRec.AH_LocalExTaxAmount = 10M;
			aRRec.AH_LocalOutstandingAmount = 0M;
			aRRec.AH_FullyPaidDate = ZDateTime.Now;

			ARInvoice aRInv = factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(aRInv, aRInv.TransactionCurrency, aRInv.AH_ExchangeRate, 10m, 0m, 0m, 10m, 0m, 0m);
			aRInv.AH_LocalOutstandingAmount = 0M;
			aRInv.AH_FullyPaidDate = ZDateTime.Now;

			TransactionMatchLink aRRecMatch = ((IMatching)aRInv).CurrentMatchGroup.AddNew();
			aRRecMatch.AP_AH = aRRec.PK;
			aRRecMatch.AP_MatchGroupNum = "M00001303";
			aRRecMatch.AP_Amount = -10M;

			TransactionMatchLink aRInvMatch = ((IMatching)aRInv).CurrentMatchGroup.AddNew();
			aRInvMatch.AP_AH = aRInv.PK;
			aRInvMatch.AP_MatchGroupNum = "M00001303";
			aRInvMatch.AP_Amount = 10M;
			TestObjectCreator.SetupMatchLinkMatchDate(aRInv);

			factory.Save();

			ReceiptReversing recRev = new ReceiptReversing(aRRec);
			recRev.Reverse();

			factory.Save();

			// Check reversing Receipt
			ZQuery receiptFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Receipt);
			receiptFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, aRRec.PK);
			receiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, aRRec.AH_GC);
			TransactionHeaderCollection receipts = new TransactionHeaderCollection(factory, receiptFilter);
			receipts.Load();
			AssertEquals("There should only be 1 receipt in the DB other than ARRec", 1, receipts.Count);
			Receipt reversingRec = (Receipt)receipts[0];
			Assert("ReversingReceipt should be cancelled", reversingRec.AH_IsCancelled);
			AssertEquals("ReversingReceipt local amount should be 10", 10M, reversingRec.AH_InvoiceAmount);
			AssertEquals("ReversingReceipt os amount should be 10", 10M, reversingRec.AH_OSTotal);
			AssertEquals("ReversingReceipt outstandingamount should be 0", 0M, reversingRec.AH_OutstandingAmount);
			Assert("ReversingReceipt fully paid date should be non-null", !reversingRec.AH_FullyPaidDate.IsEmpty);

			// Check original Receipt
			aRRec.Reload();
			Assert("OriginalReceipt should be cancelled", aRRec.AH_IsCancelled);
			AssertEquals("OriginalReceipt local amount should be -10", -10M, aRRec.AH_InvoiceAmount);
			AssertEquals("OriginalReceipt outstanding amount should be 0", 0M, aRRec.AH_OutstandingAmount);
			Assert("OriginalReceipt fully paid date should be non-null", !reversingRec.AH_FullyPaidDate.IsEmpty);

			// Check Invoice
			aRInv.Reload();
			Assert("ARInv should mot be fully paid", aRInv.AH_FullyPaidDate.IsEmpty);
			AssertEquals("ARInv outstanding amount = 10", 10M, aRInv.AH_OutstandingAmount);
			AssertEquals("ARInv local amount = 10", 10M, aRInv.AH_InvoiceAmount);

			// Check MatchLinks
			Assert("Matchlink for ARRec should be deleted", aRRecMatch.IsDeleted);
			Assert("Matchlink for ARInv should be deleted", aRInvMatch.IsDeleted);

			ZQuery origRecMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, aRRec.PK);
			TransactionMatchLink origRecMatch = factory.LoadTop1<TransactionMatchLink>(origRecMatchFilter);
			AssertNotNull("Matchlink should be created for the original receipt", origRecMatch);
			AssertEquals("MatchAmount should be -10", -10M, origRecMatch.AP_Amount);
			ZString matchGroupNum = origRecMatch.AP_MatchGroupNum;

			ZQuery revRecMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, reversingRec.PK);
			TransactionMatchLink revRecMatch = factory.LoadTop1<TransactionMatchLink>(revRecMatchFilter);
			AssertNotNull("Matchlink should be created for the reversing receipt", revRecMatch);
			AssertEquals("MatchAmount should be 10", 10M, revRecMatch.AP_Amount);
			AssertEquals("MatchLinks should be in the same match group", matchGroupNum, revRecMatch.AP_MatchGroupNum);
		}

		#endregion

		#region TestReverseTwiceMatchedNonDirectCreditReceipt

		public void TestReverseTwiceMatchedNonDirectCreditReceipt()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader testOrg = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			APReceipt aPRec = factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_OH = testOrg.PK;
			aPRec.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			aPRec.AH_LocalExTaxAmount = 62M;
			aPRec.AH_OSExTaxAmount = 62M;
			aPRec.AH_LocalOutstandingAmount = 2M;

			ARInvoice aRInv = factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(aRInv, aRInv.TransactionCurrency, aRInv.AH_ExchangeRate, 40m, 0m, 0m, 40m, 0m, 0m);
			aRInv.AH_LocalOutstandingAmount = 10M;

			APCreditNote aPCrd = factory.NewWithValidTestData<APCreditNote>();
			aPCrd.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(aPCrd, aPCrd.TransactionCurrency, aPCrd.AH_ExchangeRate, 32m, 0m, 0m, 32m, 0m, 0m);
			aPCrd.AH_LocalOutstandingAmount = 2M;

			TransactionMatchLink aPRecMatch1 = ((IMatching)aRInv).CurrentMatchGroup.AddNew();
			aPRecMatch1.AP_AH = aPRec.PK;
			aPRecMatch1.AP_MatchGroupNum = "M00001456";
			aPRecMatch1.AP_Amount = -30M;

			TransactionMatchLink aRInvMatch = ((IMatching)aRInv).CurrentMatchGroup.AddNew();
			aRInvMatch.AP_AH = aRInv.PK;
			aRInvMatch.AP_MatchGroupNum = "M00001456";
			aRInvMatch.AP_Amount = 30M;
			TestObjectCreator.SetupMatchLinkMatchDate(aRInv);

			TransactionMatchLink aPRecMatch2 = ((IMatching)aPCrd).CurrentMatchGroup.AddNew();
			aPRecMatch2.AP_AH = aPRec.PK;
			aPRecMatch2.AP_MatchGroupNum = "M00001509";
			aPRecMatch2.AP_Amount = -30M;

			TransactionMatchLink aPCrdMatch = ((IMatching)aPCrd).CurrentMatchGroup.AddNew();
			aPCrdMatch.AP_AH = aPCrd.PK;
			aPCrdMatch.AP_MatchGroupNum = "M00001509";
			aPCrdMatch.AP_Amount = 30M;
			TestObjectCreator.SetupMatchLinkMatchDate(aPCrd);

			factory.Save();

			ReceiptReversing recReversing = new ReceiptReversing(aPRec);
			recReversing.Reverse();
			factory.Save();

			// Check the original receipt
			aPRec.Reload();
			Assert("Original receipt should be cancelled", aPRec.AH_IsCancelled);
			AssertEquals("Original receipt should have outstanding amount = 0", 0M, aPRec.AH_LocalOutstandingAmount);
			Assert("Original receipt should be fully paid", !aPRec.AH_FullyPaidDate.IsEmpty);

			// Check the reversing receipt
			ZQuery revReceiptFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Receipt);
			revReceiptFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, aPRec.PK);
			revReceiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, aPRec.AH_GC);
			TransactionHeaderCollection receipts = new TransactionHeaderCollection(factory, revReceiptFilter);
			receipts.Load();
			AssertEquals("There should only be 1 receipt in the DB other than original receipt", 1, receipts.Count);
			Receipt reversingRec = (Receipt)receipts[0];
			Assert("Reversing Receipt should be cancelled", reversingRec.AH_IsCancelled);
			AssertEquals("Reversing Receipt outstanding amount = 0", 0M, reversingRec.AH_OutstandingAmount);
			AssertEquals("Reversing Receipt local amount = 62", 62M, reversingRec.AH_InvoiceAmount);
			AssertEquals("Reversing Receipt os amount = 62", 62M, reversingRec.AH_OSTotal);
			Assert("Reversing Receipt should be fully paid", !reversingRec.AH_FullyPaidDate.IsEmpty);

			// Check the AR Invoice
			aRInv.Reload();
			Assert("ARInvoice should not be fully paid", aRInv.AH_FullyPaidDate.IsEmpty);
			AssertEquals("ARInvoice outstanding amount = 40", 40M, aRInv.AH_LocalOutstandingAmount);
			AssertEquals("ARInvoice local amount = 40", 40M, aRInv.AH_InvoiceAmount);

			// Check the APCreditNote
			aPCrd.Reload();
			Assert("APCreditNote should not be fully paid", aPCrd.AH_FullyPaidDate.IsEmpty);
			AssertEquals("APCreditNote outstanding amount = 32", 32M, aPCrd.AH_OutstandingAmount);
			AssertEquals("APCreditNote local amount = 32", 32M, aPCrd.AH_InvoiceAmount);
			AssertEquals("APCreditNote os amount = 32", 32M, aPCrd.AH_OSTotal);

			// Check the MatchLinks
			Assert("ARInvMatch should be deleted", aRInvMatch.IsDeleted);
			Assert("APCrdMatch should be deleted", aPCrdMatch.IsDeleted);
			Assert("APRecMatch1 should be deleted", aPRecMatch1.IsDeleted);
			Assert("APRecMatch2 should be deleted", aPRecMatch2.IsDeleted);

			TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(factory);
			matchlinks.Load();
			AssertEquals("There should be 2 matchlinks in the DB", 2, matchlinks.Count);

			ZQuery origRecMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, aPRec.PK);
			TransactionMatchLink origRecMatch = factory.LoadTop1<TransactionMatchLink>(origRecMatchFilter);
			AssertNotNull("Original receipt should have matchlink created", origRecMatch);
			AssertEquals("Original receipt should have match amount = -62", -62M, origRecMatch.AP_Amount);
			ZString matchGroupNum = origRecMatch.AP_MatchGroupNum;

			ZQuery revRecMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, reversingRec.PK);
			TransactionMatchLink revRecMatch = factory.LoadTop1<TransactionMatchLink>(revRecMatchFilter);
			AssertNotNull("Reversing receipt should have matchlink created", revRecMatch);
			AssertEquals("Reversing receipt should have match amount = 62", 62M, revRecMatch.AP_Amount);
			AssertEquals("Matchlinks should be in the same matchgroup", matchGroupNum, revRecMatch.AP_MatchGroupNum);
		}

		#endregion

		#region TestReverseOnceMatchedDirectCreditReceipt

		public void TestReverseOnceMatchedDirectCreditReceipt()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader testOrg = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			DepositBatch testDepositBatch = factory.NewWithValidTestData<DepositBatch>();
			testDepositBatch.AH_TransactionNum = "00001580";
			testDepositBatch.AH_ReceiptBatchNo = "00001580";
			factory.Save();

			ARReceipt aRRec = factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OH = testOrg.PK;
			aRRec.AH_OSExTaxAmount = 66M;
			aRRec.AH_LocalExTaxAmount = 66M;
			aRRec.AH_LocalOutstandingAmount = 33M;
			aRRec.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			aRRec.AH_ReceiptBatchNo = "00001580";

			APCreditNote aPCrd = factory.NewWithValidTestData<APCreditNote>();
			aPCrd.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(aPCrd, aPCrd.TransactionCurrency, aPCrd.AH_ExchangeRate, 66m, 0m, 0m, 66m, 0m, 0m);
			aPCrd.AH_LocalOutstandingAmount = 33M;

			TransactionMatchLink aRRecMatch = ((IMatching)aPCrd).CurrentMatchGroup.AddNew();
			aRRecMatch.AP_AH = aRRec.PK;
			aRRecMatch.AP_Amount = -33M;
			aRRecMatch.AP_MatchGroupNum = "M00001144";
			aRRecMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			TransactionMatchLink aPCrdMatch = ((IMatching)aPCrd).CurrentMatchGroup.AddNew();
			aPCrdMatch.AP_AH = aPCrd.PK;
			aPCrdMatch.AP_Amount = 33M;
			aPCrdMatch.AP_MatchGroupNum = "M00001144";
			aPCrdMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			factory.Save();

			ReceiptReversing recReversing = new ReceiptReversing(aRRec);
			recReversing.Reverse();

			AssertEquals("MinUnmatchDate should be initialized.", ZDateTime.BrettsBirthday, ((IUnmatchOnReversing)aRRec.ReverseTransaction).UnmatchingData.MinUnmatchDate);
			aRRec.ReverseTransaction.UnmatchDate = ZDateTime.Today.AddDays(-100);

			// Check that reverse does not save to the DB - reload the original transactions in a new factory
			Assert("Original Receipt Matchlink should not be deleted", aRRecMatch.IsInDatabase);
			Assert("Original CreditNote MAtchlink should not be deleted", aPCrdMatch.IsInDatabase);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ARReceipt aRRecReload = newFactory.Load<ARReceipt>(aRRec.PK);
			APCreditNote aPCrdReload = newFactory.Load<APCreditNote>(aPCrd.PK);
			AssertEquals("Outstanding amount of Original Receipt should be unchanged", -33M, aRRecReload.AH_OutstandingAmount);
			AssertEquals("Outstanding amount of Original CreditNote should be unchanged", 33M, aPCrdReload.AH_OutstandingAmount);

			factory.Save();

			aRRec.Reload();
			aPCrd.Reload();

			// Check the Original ARReceipt
			AssertEquals("ARRec outstandingamount = 0", 0M, aRRec.AH_OutstandingAmount);
			Assert("ARRec should be cancelled", aRRec.AH_IsCancelled);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", ZDateTime.Today, aRRec.AH_FullyPaidDate.Date);

			// Check the APCreditNote
			AssertEquals("APCrd outstanding amount = 66", 66M, aPCrd.AH_OutstandingAmount);
			Assert("APCrd should not be cancelled", !aPCrd.AH_IsCancelled);
			Assert("APCrd should not be fully paid", aPCrd.AH_FullyPaidDate.IsEmpty);

			// Check the Reversing ARReceipt
			ZQuery revRecFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Receipt);
			revRecFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, aRRec.PK);
			revRecFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, aRRec.AH_GC);
			TransactionHeaderCollection receipts = new TransactionHeaderCollection(factory, revRecFilter);
			receipts.Load();
			AssertEquals("There should only be 1 receipt in the DB other than the original receipt", 1, receipts.Count);
			Receipt reversingRec = (Receipt)receipts[0];

			Assert("Reversing Receipt should be cancelled", reversingRec.AH_IsCancelled);
			AssertEquals("Reversing Receipt outstanding amount = 0", 0M, reversingRec.AH_OutstandingAmount);
			AssertEquals("Reversing Receipt local amount = 66", 66M, reversingRec.AH_InvoiceAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", ZDateTime.Today, reversingRec.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should not be changed on unmatch date", ZDateTime.Today, reversingRec.AH_PostDate.Date);

			// Check the reversing deposit batch
			ZQuery revDepBatFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			revDepBatFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, reversingRec.AH_ReceiptBatchNo);
			revDepBatFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, reversingRec.AH_GC);
			TransactionHeaderCollection depBats = new TransactionHeaderCollection(factory, revDepBatFilter);
			depBats.Load();
			AssertEquals("There should be 1 deposit batch with TransactionNum = " + reversingRec.AH_ReceiptBatchNo, 1, depBats.Count);
			DepositBatch reversingDepBat = (DepositBatch)depBats[0];

			Assert("Should be empty", reversingDepBat.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Reversing DepositBatch should have outstandingamount = 0", 0M, reversingDepBat.AH_OutstandingAmount);
			AssertEquals("Reversing DepositBatch should have local amount = -66", -66M, reversingDepBat.AH_InvoiceAmount);
			AssertEquals("Reversing DepositBatch should have os amount = -66", -66M, reversingDepBat.AH_OSTotal);

			// Check the Matchlinks 
			Assert("Original Receipt Matchlink should be deleted", aRRecMatch.IsDeleted);
			Assert("Original Payment Matchlink should be deleted", aPCrdMatch.IsDeleted);
			TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(factory);
			matchlinks.Load();
			AssertEquals("There should be 2 matchlinks in the DB", 2, matchlinks.Count);
			AssertEquals("Match date of new reversing matchinkg should not be changed on unmatch date", ZDateTime.Today, matchlinks[0].AP_MatchDate.Date);
			AssertEquals("Match date of new reversing matchinkg should not be changed on unmatch date", ZDateTime.Today, matchlinks[1].AP_MatchDate.Date);
		}

		#endregion

		public void TestChangeUnmatchDateOnReversing()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var receipt = Factory.NewWithValidTestData<APReceipt>();
			receipt.AH_OH = testOrg.PK;
			receipt.AH_OSExTaxAmount = 70M;
			receipt.AH_LocalOutstandingAmount = 40M;

			var discount = Factory.NewWithValidTestData<APDiscount>();
			discount.AH_OH = testOrg.PK;
			discount.AH_OSExTaxAmount = 20M;
			discount.AH_LocalOutstandingAmount = 0M;

			var clearingJournal = Factory.NewWithValidTestData<APJournal>();
			clearingJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
			clearingJournal.AH_OH = testOrg.PK;
			clearingJournal.DebitCreditSign = DebitCreditDataEntry.CR;
			clearingJournal.AH_OSExTaxAmount = 10M;
			clearingJournal.AH_LocalOutstandingAmount = 0M;
			clearingJournal.AH_TransactionCreatedByMatching = true;

			string matchGroupNumber = "M00001840";
			var receiptMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			receiptMatch.AP_AH = receipt.PK;
			receiptMatch.AP_Amount = -discount.AH_LocalExTaxAmount - clearingJournal.AH_LocalExTaxAmount;
			receiptMatch.AP_MatchGroupNum = matchGroupNumber;
			receiptMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			var discountMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			discountMatch.AP_AH = discount.PK;
			discountMatch.AP_Amount = discount.AH_LocalExTaxAmount;
			discountMatch.AP_MatchGroupNum = matchGroupNumber;
			discountMatch.AP_MatchDate = receiptMatch.AP_MatchDate;

			var clearingJournalMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			clearingJournalMatch.AP_AH = clearingJournal.PK;
			clearingJournalMatch.AP_Amount = clearingJournal.AH_LocalExTaxAmount;
			clearingJournalMatch.AP_MatchGroupNum = matchGroupNumber;
			clearingJournalMatch.AP_MatchDate = receiptMatch.AP_MatchDate;

			discount.AH_FullyPaidDate = receiptMatch.AP_MatchDate;
			clearingJournal.AH_FullyPaidDate = receiptMatch.AP_MatchDate;

			Factory.Save();

			((IMatching)receipt).CurrentMatchGroup.RemoveAll();
			var overpayment = Factory.NewWithValidTestData<APOverpayment>();
			overpayment.AH_OH = testOrg.PK;
			overpayment.AH_OSExTaxAmount = 40M;
			overpayment.AH_LocalOutstandingAmount = 0M;

			matchGroupNumber = "M00001841";
			var receiptMatch2 = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			receiptMatch2.AP_AH = receipt.PK;
			receiptMatch2.AP_Amount = -overpayment.AH_LocalExTaxAmount;
			receiptMatch2.AP_MatchGroupNum = matchGroupNumber;
			receiptMatch2.AP_MatchDate = ZDateTime.BrettsBirthday.AddDays(50);

			var overpaymentMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			overpaymentMatch.AP_AH = overpayment.PK;
			overpaymentMatch.AP_Amount = overpayment.AH_LocalExTaxAmount;
			overpaymentMatch.AP_MatchGroupNum = matchGroupNumber;
			overpaymentMatch.AP_MatchDate = receiptMatch2.AP_MatchDate;

			receipt.AH_LocalOutstandingAmount = 0M;
			receipt.AH_FullyPaidDate = receiptMatch2.AP_MatchDate;
			overpayment.AH_FullyPaidDate = receiptMatch2.AP_MatchDate;

			Factory.Save();

			var payReversing = new ReceiptReversing(receipt);
			payReversing.Reverse();

			AssertEquals("MinUnmatchDate should be initialized.", ZDateTime.BrettsBirthday.AddDays(50), ((IUnmatchOnReversing)receipt.ReverseTransaction).UnmatchingData.MinUnmatchDate);
			var expectedUnmatchDate = ZDateTime.Today.AddDays(-100);
			receipt.ReverseTransaction.UnmatchDate = ZDateTime.Today.AddDays(-100);

			Factory.Save();

			receipt.Reload();
			AssertEquals("Original receipt Outstanding amount = 0", 0M, receipt.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", ZDateTime.Today, receipt.AH_FullyPaidDate.Date);
			Assert("Original receipt should be cancelled", receipt.AH_IsCancelled);

			discount.Reload();
			AssertEquals("Original discount outstanding amount = 0", 0M, discount.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", expectedUnmatchDate, discount.AH_FullyPaidDate.Date);
			Assert("Original receipt should be cancelled", discount.AH_IsCancelled);

			overpayment.Reload();
			AssertEquals("Original overpayment outstanding amount = 0", 0M, overpayment.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", expectedUnmatchDate, overpayment.AH_FullyPaidDate.Date);
			Assert("Original receipt should be cancelled", overpayment.AH_IsCancelled);

			clearingJournal.Reload();
			AssertEquals("Original clearingJournal outstanding amount = 0", 0M, clearingJournal.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", expectedUnmatchDate, clearingJournal.AH_FullyPaidDate.Date);
			Assert("Original clearingJournal should be cancelled", clearingJournal.AH_IsCancelled);

			var revRec = Factory.Load<TransactionHeader>(receipt.ReverseTransaction.PK);
			Assert("revRec should be a receipt", revRec is Receipt);

			AssertEquals("Reversing receipt local amount", 70M, revRec.AH_InvoiceAmount);
			AssertEquals("Reversing receipt outstanding amount = 0", 0M, revRec.AH_OutstandingAmount);
			Assert("Reversing receipt should be cancelled", revRec.AH_IsCancelled);
			AssertEquals("FullyPaidDate should not be changed on unmatch date", ZDateTime.Today, revRec.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should not be changed on unmatch date", ZDateTime.Today, revRec.AH_PostDate.Date);

			var revDiscountFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Discount);
			revDiscountFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, discount.PK);
			revDiscountFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, discount.AH_GC);
			var discounts = Factory.Load<TransactionHeader>(revDiscountFilter);
			AssertEquals("discounts.Length", 1, discounts.Length);
			var revDiscount = discounts[0];
			Assert("revDiscount should be a Discount", revDiscount is Discount);

			AssertEquals("Reversing discount local amount", -20M, revDiscount.AH_InvoiceAmount);
			AssertEquals("Reversing discount outstanding amount = 0", 0M, revDiscount.AH_OutstandingAmount);
			Assert("Reversing discount should be cancelled", revDiscount.AH_IsCancelled);
			AssertEquals("FullyPaidDate should be changed on unmatch date", expectedUnmatchDate, revDiscount.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should be changed on unmatch date", expectedUnmatchDate, revDiscount.AH_PostDate.Date);

			var revOverpaymentFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Overpayment);
			revOverpaymentFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, overpayment.PK);
			revOverpaymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, overpayment.AH_GC);
			var overpayments = Factory.Load<TransactionHeader>(revOverpaymentFilter);
			AssertEquals("overpayments.Length", 1, overpayments.Length);
			var revOverpayment = overpayments[0];
			Assert("revOverpayment should be a Overpayment", revOverpayment is Overpayment);

			AssertEquals("Reversing overpayment local amount", -40M, revOverpayment.AH_InvoiceAmount);
			AssertEquals("Reversing overpayment outstanding amount = 0", 0M, revOverpayment.AH_OutstandingAmount);
			Assert("Reversing overpayment should be cancelled", revOverpayment.AH_IsCancelled);
			AssertEquals("FullyPaidDate should be changed on unmatch date", expectedUnmatchDate, revOverpayment.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should be changed on unmatch date", expectedUnmatchDate, revOverpayment.AH_PostDate.Date);

			var revClearingJournalFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			revClearingJournalFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, clearingJournal.PK);
			revClearingJournalFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, clearingJournal.AH_GC);
			var journals = Factory.Load<TransactionHeader>(revClearingJournalFilter);
			AssertEquals("journals.Length", 1, journals.Length);
			var revClearingJournal = journals[0];
			Assert("revClearingJournal should be a Journal", revClearingJournal is Journal);

			AssertEquals("Reversing ClearingJournal local amount", -10M, revClearingJournal.AH_InvoiceAmount);
			AssertEquals("Reversing ClearingJournal outstanding amount = 0", 0M, revClearingJournal.AH_OutstandingAmount);
			Assert("Reversing ClearingJournal should be cancelled", revClearingJournal.AH_IsCancelled);
			AssertEquals("FullyPaidDate should be changed on unmatch date", expectedUnmatchDate, revClearingJournal.AH_FullyPaidDate.Date);
			AssertEquals("PostDate should be changed on unmatch date", expectedUnmatchDate, revClearingJournal.AH_PostDate.Date);

			Assert("receiptMatch should be deleted", receiptMatch.IsDeleted);
			Assert("discountMatch should be deleted", discountMatch.IsDeleted);
			Assert("receiptMatch2 should be deleted", receiptMatch2.IsDeleted);
			Assert("overpaymentMatch should be deleted", overpaymentMatch.IsDeleted);
			Assert("clearingJournalMatch should be deleted", clearingJournalMatch.IsDeleted);

			var origRecMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receipt.PK);
			var origRecMatch = Factory.LoadTop1<TransactionMatchLink>(origRecMatchFilter);
			AssertEquals("Match date of new reversing matching should not be changed on unmatch date", ZDateTime.Today, origRecMatch.AP_MatchDate.Date);
			AssertEquals("origRecMatch amount", -70M, origRecMatch.AP_Amount);

			var revRecMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revRec.PK);
			var revRecMatch = Factory.LoadTop1<TransactionMatchLink>(revRecMatchFilter);
			AssertEquals("revRecMatch Match amount", 70M, revRecMatch.AP_Amount);
			AssertEquals("Match date of new reversing matching should not be changed on unmatch date", ZDateTime.Today, revRecMatch.AP_MatchDate.Date);

			var origDiscountMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, discount.PK);
			var origDiscountMatch = Factory.LoadTop1<TransactionMatchLink>(origDiscountMatchFilter);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, origDiscountMatch.AP_MatchDate.Date);
			AssertEquals("origDiscountMatch amount", 20M, origDiscountMatch.AP_Amount);

			var revDiscountMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revDiscount.PK);
			var revDiscountMatch = Factory.LoadTop1<TransactionMatchLink>(revDiscountMatchFilter);
			AssertEquals("revDiscountMatch amount", -20M, revDiscountMatch.AP_Amount);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, revDiscountMatch.AP_MatchDate.Date);

			var origOverpaymentMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, overpayment.PK);
			var origOverpaymentMatch = Factory.LoadTop1<TransactionMatchLink>(origOverpaymentMatchFilter);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, origOverpaymentMatch.AP_MatchDate.Date);
			AssertEquals("origOverpaymentMatch amount", 40M, origOverpaymentMatch.AP_Amount);

			var revOverpaymentMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revOverpayment.PK);
			var revOverpaymentMatch = Factory.LoadTop1<TransactionMatchLink>(revOverpaymentMatchFilter);
			AssertEquals("revOverpaymentMatch amount", -40M, revOverpaymentMatch.AP_Amount);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, revOverpaymentMatch.AP_MatchDate.Date);

			var origClearingJournalMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, clearingJournal.PK);
			var origClearingJournalMatch = Factory.LoadTop1<TransactionMatchLink>(origClearingJournalMatchFilter);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, origClearingJournalMatch.AP_MatchDate.Date);
			AssertEquals("origClearingJournalMatch amount", 10M, origClearingJournalMatch.AP_Amount);

			var revClearingJournalMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revClearingJournal.PK);
			var revClearingJournalMatch = Factory.LoadTop1<TransactionMatchLink>(revClearingJournalMatchFilter);
			AssertEquals("revClearingJournalMatch amount", -10M, revClearingJournalMatch.AP_Amount);
			AssertEquals("Match date of new reversing matching should be changed on unmatch date", expectedUnmatchDate, revClearingJournalMatch.AP_MatchDate.Date);
		}

		#region TestReverseBatchedReceiptCreatesNewBatchCorrectly

		public void TestReverseBatchedReceiptCreatesNewBatchCorrectly()
		{
			// create 2 receipts
			TestObjectCreator creator = new TestObjectCreator(Factory);
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DepositBatch testDepositBatch = Factory.NewWithValidTestData<DepositBatch>();
			testDepositBatch.AH_TransactionNum = "00001580";
			testDepositBatch.AH_ReceiptBatchNo = "00001580";
			Factory.Save();

			ARReceipt aRRec1 = Factory.NewWithValidTestData<ARReceipt>();
			aRRec1.AH_OH = testOrg.PK;
			aRRec1.AH_AB = creator.AUDBankAccount.PK;
			aRRec1.AH_OSExTaxAmount = 100M;
			aRRec1.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			aRRec1.AH_ReceiptBatchNo = "00001580";

			ARReceipt aRRec2 = Factory.NewWithValidTestData<ARReceipt>();
			aRRec2.AH_OH = testOrg.PK;
			aRRec2.AH_AB = creator.AUDBankAccount.PK;
			aRRec2.AH_OSExTaxAmount = 200M;
			aRRec2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;

			DepositBatch originalBatch = Factory.New<DepositBatch>();
			originalBatch.AH_InvoiceAmount = 300m;
			originalBatch.AH_AB = creator.AUDBankAccount.PK;

			Factory.Save();

			aRRec1.AH_ReceiptBatchNo = originalBatch.AH_TransactionNum;
			aRRec2.AH_ReceiptBatchNo = originalBatch.AH_TransactionNum;

			AssertNotNull("Related Batch for receipt 1", aRRec1.RelatedDepositBatch);
			AssertEquals("Related Batch for receipt 1", originalBatch.PK, aRRec1.RelatedDepositBatch.PK);

			AssertNotNull("Related Batch for receipt 2", aRRec2.RelatedDepositBatch);
			AssertEquals("Related Batch for receipt 2", originalBatch.PK, aRRec2.RelatedDepositBatch.PK);

			ReceiptReversing reversing = new ReceiptReversing(aRRec1);
			reversing.Reverse();

			Factory.Save();

			ARReceipt aRRec1Reversal = (ARReceipt)reversing.ReverseTransaction;
			AssertNotNull("Reversing Receipt should have a batch", aRRec1Reversal.RelatedDepositBatch);

			AssertEquals("Reversing batch should be for correct amount", -100m, aRRec1Reversal.RelatedDepositBatch.AH_InvoiceAmount);
		}

		#endregion

		public void TestReverseBatchedReceiptCreatesNewBatchCorrectlyWithForeignCurrency()
		{
			var testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			testReceipt.AH_RX_NKTransactionCurrency = "USD";
			testReceipt.AH_ExchangeRate = 7;
			testReceipt.AH_OSTotal = 100M;
			testReceipt.AH_OutstandingAmount = 700M;
			testReceipt.AH_InvoiceAmount = 700M;

			Factory.Save();

			var testDepositBatchParent = new DepositBatchParent(Factory);
			var testBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			var reversing = new ReceiptReversing(testReceipt);
			reversing.Reverse();

			Factory.Save();

			var testReceiptReversal = (ARReceipt)reversing.ReverseTransaction;

			AssertNotNull("Reversing Receipt should have a batch", testReceiptReversal.RelatedDepositBatch);
			AssertEquals("Reversing batch should be for correct amount", 700m, testReceiptReversal.RelatedDepositBatch.AH_OSTotal);
		}

		public void TestReversedTransactionUnmatchDateReadOnly_APOnlyMatching()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var receipt = Factory.NewWithValidTestData<APReceipt>();
			receipt.AH_OH = testOrg.PK;
			receipt.AH_OSExTaxAmount = 40M;
			receipt.AH_LocalOutstandingAmount = 0M;

			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			creditNote.AH_OH = testOrg.PK;

			var line = (APCreditNoteLine)creditNote.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = 40m;
			creditNote.AH_LocalOutstandingAmount = 0M;

			string matchGroupNumber = "M00001";
			var paymentMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			paymentMatch.AP_AH = receipt.PK;
			paymentMatch.AP_Amount = -receipt.AH_LocalExTaxAmount;
			paymentMatch.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			var invoiceMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			invoiceMatch.AP_AH = creditNote.PK;
			invoiceMatch.AP_Amount = creditNote.AH_LocalExTaxAmount;
			invoiceMatch.AP_MatchGroupNum = matchGroupNumber;
			invoiceMatch.AP_MatchDate = paymentMatch.AP_MatchDate;

			receipt.AH_FullyPaidDate = paymentMatch.AP_MatchDate;
			creditNote.AH_FullyPaidDate = paymentMatch.AP_MatchDate;

			Factory.Save();

			Action<bool, bool, bool, bool> assertUnmatchDateReadonly = (bool apPermition, bool arPermition, bool backPostingRegistry, bool unmatchDateReadonly) =>
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = apPermition;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = arPermition;
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backPostingRegistry);
				var newFactory = new BusinessObjectFactory();
				var paymentForReversing = newFactory.Load<APPayment>(receipt.PK);
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

			var receipt = Factory.NewWithValidTestData<ARReceipt>();
			receipt.AH_OH = testOrg.PK;
			receipt.AH_OSExTaxAmount = 40M;
			receipt.AH_LocalOutstandingAmount = 0M;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = testOrg.PK;

			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = 40m;
			invoice.AH_LocalOutstandingAmount = 0M;

			string matchGroupNumber = "M00001";
			var paymentMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			paymentMatch.AP_AH = receipt.PK;
			paymentMatch.AP_Amount = -receipt.AH_LocalExTaxAmount;
			paymentMatch.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			var invoiceMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			invoiceMatch.AP_AH = invoice.PK;
			invoiceMatch.AP_Amount = invoice.AH_LocalExTaxAmount;
			invoiceMatch.AP_MatchGroupNum = matchGroupNumber;
			invoiceMatch.AP_MatchDate = paymentMatch.AP_MatchDate;

			receipt.AH_FullyPaidDate = paymentMatch.AP_MatchDate;
			invoice.AH_FullyPaidDate = paymentMatch.AP_MatchDate;

			Factory.Save();

			Action<bool, bool, bool, bool> assertUnmatchDateReadonly = (bool apPermition, bool arPermition, bool backPostingRegistry, bool unmatchDateReadonly) =>
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = apPermition;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = arPermition;
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backPostingRegistry);
				var newFactory = new BusinessObjectFactory();
				var paymentForReversing = newFactory.Load<APPayment>(receipt.PK);
				var payReversing = new PaymentReversing(paymentForReversing);
				payReversing.Reverse();
				AssertEquals(unmatchDateReadonly, paymentForReversing.ReverseTransaction.UnmatchDateInfo.ReadOnly);
			};

			assertUnmatchDateReadonly(false, true, true, false);
			assertUnmatchDateReadonly(true, false, true, true);
			assertUnmatchDateReadonly(true, true, false, true);
			assertUnmatchDateReadonly(true, true, true, false);
		}

		public void TestReversedTransactionUnmatchDateReadOnly_MixedMatching()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var receipt = Factory.NewWithValidTestData<ARReceipt>();
			receipt.AH_OH = testOrg.PK;
			receipt.AH_OSExTaxAmount = 40M;
			receipt.AH_LocalOutstandingAmount = 0M;

			var invoice = Factory.NewWithValidTestData<APCreditNote>();
			invoice.AH_OH = testOrg.PK;

			var line = (APCreditNoteLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = 40m;
			invoice.AH_LocalOutstandingAmount = 0M;

			string matchGroupNumber = "M00001";
			var paymentMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			paymentMatch.AP_AH = receipt.PK;
			paymentMatch.AP_Amount = -receipt.AH_LocalExTaxAmount;
			paymentMatch.AP_MatchGroupNum = matchGroupNumber;
			paymentMatch.AP_MatchDate = ZDateTime.BrettsBirthday;

			var invoiceMatch = ((IMatching)receipt).CurrentMatchGroup.AddNew();
			invoiceMatch.AP_AH = invoice.PK;
			invoiceMatch.AP_Amount = invoice.AH_LocalExTaxAmount;
			invoiceMatch.AP_MatchGroupNum = matchGroupNumber;
			invoiceMatch.AP_MatchDate = paymentMatch.AP_MatchDate;

			receipt.AH_FullyPaidDate = paymentMatch.AP_MatchDate;
			invoice.AH_FullyPaidDate = paymentMatch.AP_MatchDate;

			Factory.Save();

			Action<bool, bool, bool, bool> assertUnmatchDateReadonly = (bool apPermition, bool arPermition, bool backPostingRegistry, bool unmatchDateReadonly) =>
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = apPermition;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = arPermition;
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backPostingRegistry);
				var newFactory = new BusinessObjectFactory();
				var paymentForReversing = newFactory.Load<APPayment>(receipt.PK);
				var payReversing = new PaymentReversing(paymentForReversing);
				payReversing.Reverse();
				AssertEquals(unmatchDateReadonly, paymentForReversing.ReverseTransaction.UnmatchDateInfo.ReadOnly);
			};

			assertUnmatchDateReadonly(false, true, true, true);
			assertUnmatchDateReadonly(true, false, true, true);
			assertUnmatchDateReadonly(true, true, false, true);
			assertUnmatchDateReadonly(true, true, true, false);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestCheckpointsToReverse_OverpaymentInMatchGroup()
		{
			var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR1234", TestObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			arInvoice.AH_OH = TestObjectCreator.Debtor.PK;
			arInvoice.AH_LocalOutstandingAmount = 0M;

			var arReceipt = TestObjectCreator.CreateARReceipt(0m, 150m, arInvoice.AH_PostDate, arInvoice.AH_PostDate, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			arReceipt.AH_InvoiceAmount = arReceipt.AH_OSTotalAmount;
			arReceipt.AH_LocalOutstandingAmount = 0M;

			var overpayment = TestObjectCreator.CreateOverpayment<AROverpayment>(50m, arInvoice.AH_PostDate, TestObjectCreator.Debtor.PK);

			var invoiceMatchGroup = ((IMatching)arInvoice).CurrentMatchGroup.AddNew();
			invoiceMatchGroup.AP_AH = arInvoice.PK;
			invoiceMatchGroup.AP_Amount = -200m;
			invoiceMatchGroup.AP_MatchGroupNum = "M00001234";
			invoiceMatchGroup.AP_MatchDate = arInvoice.AH_PostDate;

			var receiptMatchGroup = ((IMatching)arReceipt).CurrentMatchGroup.AddNew();
			receiptMatchGroup.AP_AH = arReceipt.PK;
			receiptMatchGroup.AP_Amount = 150m;
			receiptMatchGroup.AP_MatchGroupNum = "M00001234";
			receiptMatchGroup.AP_MatchDate = arInvoice.AH_PostDate;

			var overpaymentMatchGroup = ((IMatching)overpayment).CurrentMatchGroup.AddNew();
			overpaymentMatchGroup.AP_AH = overpayment.PK;
			overpaymentMatchGroup.AP_Amount = 50m;
			overpaymentMatchGroup.AP_MatchGroupNum = "M00001234";
			overpaymentMatchGroup.AP_MatchDate = arInvoice.AH_PostDate;

			Factory.Save();

			var reverser = ReversingFactory.NewReversing(arReceipt);
			AssertSequencesEqual("When match group with overpayment, there should be a(n additional) Checkpoint required to reverse", new Security.SecurityCheckpoint[] { Env.Security.ReceivablesUnMatchTransactionOverpaymentType }, reverser.CheckpointsToReverse);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestCheckpointsToReverse_NoMiscTransactionsInMatchGroup()
		{
			var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR1234", TestObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			arInvoice.AH_OH = TestObjectCreator.Debtor.PK;
			arInvoice.AH_LocalOutstandingAmount = 0M;

			var arReceipt = TestObjectCreator.CreateARReceipt(0m, 150m, arInvoice.AH_PostDate, arInvoice.AH_PostDate, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			arReceipt.AH_InvoiceAmount = arReceipt.AH_OSTotalAmount;
			arReceipt.AH_LocalOutstandingAmount = 0M;

			var invoiceMatchGroup = ((IMatching)arInvoice).CurrentMatchGroup.AddNew();
			invoiceMatchGroup.AP_AH = arInvoice.PK;
			invoiceMatchGroup.AP_Amount = -200m;
			invoiceMatchGroup.AP_MatchGroupNum = "M00001234";
			invoiceMatchGroup.AP_MatchDate = arInvoice.AH_PostDate;

			var receiptMatchGroup = ((IMatching)arReceipt).CurrentMatchGroup.AddNew();
			receiptMatchGroup.AP_AH = arReceipt.PK;
			receiptMatchGroup.AP_Amount = -200m;
			receiptMatchGroup.AP_MatchGroupNum = "M00001234";
			receiptMatchGroup.AP_MatchDate = arInvoice.AH_PostDate;

			Factory.Save();

			var reverser = ReversingFactory.NewReversing(arReceipt);
			AssertSequencesEqual("When match group with without misc transactions, there should be no (additional) Checkpoints required to reverse", Array.Empty<Security.SecurityCheckpoint>(), reverser.CheckpointsToReverse);
		}

		protected ReceiptReversing ReceiptReversing
		{
			get { return (ReceiptReversing)PayablesAndReceivablesReversing; }
		}

		protected override Type GetTestingClassType()
		{
			return typeof(ReceiptReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = Factory.NewWithValidTestData<ReceiptForReversingTest>();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = Factory.NewWithValidTestData<ReceiptForReversingTest>();
		}

		class ReceiptForReversingTest : ARReceipt, IPayablesAndReceivablesForTests
		{
			public ReceiptForReversingTest(BusinessObjectFactory factory, DataRow row)
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
	}
}
