using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	abstract class AccountingReceiptPaymentBatchDataWriterTest : TestCaseWithFactory
	{
		[TestDate(2017, 04, 13, 1, 0, 0)]
		public void TestWrite_ARReceipt()
		{
			var matchingBase = (MatchingBase)new ARMatchingBase(TestFactory);
			matchingBase.PrimaryOrganization = TestOrg1.PK;

			SetMiscellaneousTransaction(matchingBase);

			matchingBase.AddIMatching(aRReceipt);
			matchingBase.AddIMatching(aRReceipt2);
			matchingBase.AddIMatching(aRPayment2);
			matchingBase.AddIMatching(aRCreditNote);
			matchingBase.AddIMatching(aRInvoice);
			matchingBase.AddIMatching(aRAdjustmentNote);
			matchingBase.AddIMatching(aRContra);
			matchingBase.AddIMatching(aRJournal);

			matchingBase.MatchedTransactions.SetPartialPaidAmount();
			Assert("ARReceipt should allow to be matched since balance is 0", (matchingBase.Match_ForTestOnly()));

			TestFactory.Save();

			var writer = GetBatchDataWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, aRReceipt)));
			var batchDataObject = writer.GetDataObject(aRReceipt);
			AssertNotNull(batchDataObject);
			AssertBatchWasWrittenCorrectly(aRReceipt, batchDataObject, RecipientRoleType.ORP);
		}

		[TestDate(2017, 04, 13, 1, 0, 0)]
		public void TestWrite_ARPayment()
		{
			var matchingBase = (MatchingBase)new ARMatchingBase(TestFactory);
			matchingBase.PrimaryOrganization = TestOrg1.PK;

			SetMiscellaneousTransaction(matchingBase);

			matchingBase.AddIMatching(aRPayment);
			matchingBase.AddIMatching(aRReceipt2);
			matchingBase.AddIMatching(aRPayment2);
			matchingBase.AddIMatching(aRCreditNote);
			matchingBase.AddIMatching(aRInvoice);
			matchingBase.AddIMatching(aRAdjustmentNote);
			matchingBase.AddIMatching(aRContra);
			matchingBase.AddIMatching(aRJournal);

			matchingBase.MatchedTransactions.SetPartialPaidAmount();
			Assert("ARPayment should allow to be matched since balance is 0", (matchingBase.Match_ForTestOnly()));

			TestFactory.Save();

			var writer = GetBatchDataWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, aRPayment)));
			var batchDataObject = writer.GetDataObject(aRPayment);
			AssertNotNull(batchDataObject);
			AssertBatchWasWrittenCorrectly(aRPayment, batchDataObject, RecipientRoleType.ORP);
		}

		[TestDate(2017, 04, 13, 1, 0, 0)]
		public void TestWrite_APReceipt()
		{
			var matchingBase = (MatchingBase)new APMatchingBase(TestFactory);
			matchingBase.PrimaryOrganization = TestOrg1.PK;

			SetMiscellaneousTransaction(matchingBase);

			matchingBase.AddIMatching(aPReceipt);
			matchingBase.AddIMatching(aPReceipt2);
			matchingBase.AddIMatching(aPPayment2);
			matchingBase.AddIMatching(aPCreditNote);
			matchingBase.AddIMatching(aPInvoice);
			matchingBase.AddIMatching(aPAdjustmentNote);
			matchingBase.AddIMatching(aPContra);
			matchingBase.AddIMatching(aPJournal);

			matchingBase.MatchedTransactions.SetPartialPaidAmount();
			Assert("APReceipt should allow to be matched since balance is 0", (matchingBase.Match_ForTestOnly()));

			TestFactory.Save();

			var writer = GetBatchDataWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, aPReceipt)));
			var batchDataObject = writer.GetDataObject(aPReceipt);
			AssertNotNull(batchDataObject);
			AssertBatchWasWrittenCorrectly(aPReceipt, batchDataObject, RecipientRoleType.ORP);
		}

		[TestDate(2017, 04, 13, 1, 0, 0)]
		public void TestWrite_APPayment()
		{
			var matchingBase = (MatchingBase)new ARMatchingBase(TestFactory);
			matchingBase.PrimaryOrganization = TestOrg1.PK;

			SetMiscellaneousTransaction(matchingBase);

			matchingBase.AddIMatching(aPPayment);
			matchingBase.AddIMatching(aPReceipt2);
			matchingBase.AddIMatching(aPPayment2);
			matchingBase.AddIMatching(aPCreditNote);
			matchingBase.AddIMatching(aPInvoice);
			matchingBase.AddIMatching(aPAdjustmentNote);
			matchingBase.AddIMatching(aPContra);
			matchingBase.AddIMatching(aPJournal);

			matchingBase.MatchedTransactions.SetPartialPaidAmount();
			Assert("APPayment should allow to be matched since balance is 0", (matchingBase.Match_ForTestOnly()));

			TestFactory.Save();

			var writer = GetBatchDataWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, aPPayment)));
			var batchDataObject = writer.GetDataObject(aPPayment);
			AssertNotNull(batchDataObject);
			AssertBatchWasWrittenCorrectly(aPPayment, batchDataObject, RecipientRoleType.ORP);
		}

		[TestDate(2017, 04, 13, 1, 0, 0)]
		public void TestWrite_ReverseReceipt()
		{
			var matchingBase = (MatchingBase)new ARMatchingBase(TestFactory);
			matchingBase.PrimaryOrganization = TestOrg1.PK;

			SetMiscellaneousTransaction(matchingBase);

			matchingBase.AddIMatching(aRReceipt);
			matchingBase.AddIMatching(aRReceipt2);
			matchingBase.AddIMatching(aRPayment2);
			matchingBase.AddIMatching(aRCreditNote);
			matchingBase.AddIMatching(aRInvoice);
			matchingBase.AddIMatching(aRAdjustmentNote);
			matchingBase.AddIMatching(aRContra);
			matchingBase.AddIMatching(aRJournal);

			matchingBase.MatchedTransactions.SetPartialPaidAmount();
			Assert("ARReceipt should allow to be matched since balance is 0", (matchingBase.Match_ForTestOnly()));

			var receiptReversing = new ReceiptReversing(aRReceipt);
			receiptReversing.Reverse();
			aRReceipt.AH_IsCancelled = true;
			Assert("Receipt is cancelled", aRReceipt.IsCancelled);

			TestFactory.Save();

			var writer = GetBatchDataWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, aRReceipt)));
			var batchDataObject = writer.GetDataObject(aRReceipt);
			AssertNotNull(batchDataObject);
			AssertBatchWasWrittenCorrectly(aRReceipt, batchDataObject, RecipientRoleType.ORP);
		}

		[TestDate(2017, 04, 13, 1, 0, 0)]
		public void TestWrite_ReversePayment()
		{
			var matchingBase = (MatchingBase)new ARMatchingBase(TestFactory);
			matchingBase.PrimaryOrganization = TestOrg1.PK;

			SetMiscellaneousTransaction(matchingBase);

			matchingBase.AddIMatching(aRPayment);
			matchingBase.AddIMatching(aRReceipt2);
			matchingBase.AddIMatching(aRPayment2);
			matchingBase.AddIMatching(aRCreditNote);
			matchingBase.AddIMatching(aRInvoice);
			matchingBase.AddIMatching(aRAdjustmentNote);
			matchingBase.AddIMatching(aRContra);
			matchingBase.AddIMatching(aRJournal);

			matchingBase.MatchedTransactions.SetPartialPaidAmount();
			Assert("ARPayment should allow to be matched since balance is 0", (matchingBase.Match_ForTestOnly()));

			var paymentReversing = new PaymentReversing(aRPayment);
			paymentReversing.Reverse();
			aRPayment.AH_IsCancelled = true;
			Assert("Payment is cancelled", aRPayment.IsCancelled);

			TestFactory.Save();

			var writer = GetBatchDataWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, aRPayment)));
			var batchDataObject = writer.GetDataObject(aRPayment);
			AssertNotNull(batchDataObject);
			AssertBatchWasWrittenCorrectly(aRPayment, batchDataObject, RecipientRoleType.ORP);
		}

		void AssertBatchWasWrittenCorrectly(ReceiptPaymentBase receiptPaymentBase, TransactionBatch batchDataObject, RecipientRoleType recipientRoleType)
		{
			var matchForTransaction = TestFactory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receiptPaymentBase.PK));
			var matchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchForTransaction.Select(x => x.AP_MatchGroupNum));
			var matchLinks = receiptPaymentBase.Factory.Load<TransactionMatchLink>(matchLinkQuery).Where(x => x.AP_AH != receiptPaymentBase.PK);

			if (recipientRoleType != RecipientRoleType.ORP)
			{
				matchLinks = matchLinks.Where(x => !x.TransactionHeader.AH_TransactionCreatedByMatching);
			}

			AssertEquals(matchLinks.Count() + 1, batchDataObject.TransactionCollection.Count);

			var matchTransactions = matchLinks.Select(x => x.TransactionHeader);

			foreach (var transaction in batchDataObject.TransactionCollection)
			{
				var transactionNumber = transaction.Number.GetValueOrDefault();
				var ledger = transaction.Ledger.GetValueOrDefault();
				var transationType = transaction.TransactionType.GetValueOrDefault().ToString();
				var expectTransaction = (AccTransactionHeader)null;
				var isMainTransaction = true;

				if (receiptPaymentBase.AH_TransactionNum == transactionNumber && receiptPaymentBase.AH_Ledger == ledger && receiptPaymentBase.AH_TransactionType == transationType)
				{
					expectTransaction = receiptPaymentBase;
				}
				else
				{
					expectTransaction = matchTransactions.FirstOrDefault(x => x.AH_TransactionNum == transactionNumber && x.AH_Ledger == ledger && x.AH_TransactionType == transationType);
					isMainTransaction = false;
				}

				var transactionDataSource = transaction.DataContext.DataSourceCollection.First();

				AssertEquals("Transaction DataSource Type", GetTransactionType(expectTransaction.AH_TransactionType), transactionDataSource.Type);
				AssertEquals("Transaction DataSource Key", expectTransaction.AH_Ledger + " " + expectTransaction.AH_TransactionType + " " + expectTransaction.AH_TransactionNum, transactionDataSource.Key);
				AssertEquals("Number", expectTransaction.AH_TransactionNum, transaction.Number);
				if (isMainTransaction)
				{
					if (receiptPaymentBase is Payment payment && payment.AH_ReceiptType == ReceiptTypes.Cheque)
					{
						payment.ChequeBooks.Load();
						AssertEquals("CheckBookCode", payment.ChequeBookBizO.AK_Code, transaction.CheckBookCode);
					}

					AssertEquals("Match Line Count", matchLinks.Count(), transaction.MatchLineCollection.Count);
					foreach (var matchLink in matchLinks)
					{
						var matchTransaction = matchLink.TransactionHeader;
						var key = matchTransaction.AH_Ledger + " " + matchTransaction.AH_TransactionType + " " + matchTransaction.AH_TransactionNum;
						var matchLine = transaction.MatchLineCollection.FirstOrDefault(x => (x.LinkedTransactionIDCollection.FirstOrDefault().Key ?? string.Empty) == key);
						var linkTransactionID = matchLine.LinkedTransactionIDCollection[0];

						AssertEquals("LinkedTransactionID Type", GetTransactionType(matchTransaction.AH_TransactionType), linkTransactionID.Type);
						AssertEquals("LinkedTransactionID Key", key, linkTransactionID.Key);
						AssertEquals("Match Number", matchLink.AP_MatchGroupNum, matchLine.MatchGroupNumber);
						AssertEquals("Match Date", matchLink.AP_MatchDate.Date, matchLine.MatchDate);
						AssertEquals("OSPaidAmount", matchLink.OSAmount, matchLine.OSPaidAmount);
					}
				}

				AssertNotNull("PostingJournalCollection is not null", transaction.PostingJournalCollection);
				AssertNotNull("PostingJournalCollection is not empty", transaction.PostingJournalCollection.FirstOrDefault());

				AssertTransactionDataObject(expectTransaction, transaction);
			}
		}

		void SetMiscellaneousTransaction(MatchingBase matchingBase)
		{
			var overpayment = (Overpayment)matchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			overpayment.AH_OSExTaxAmount = 100M;
			overpayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			overpayment.AH_ExchangeRate = 2M;
			matchingBase.AddIMatching(overpayment);

			var discount = (Discount)matchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			discount.AH_OSExTaxAmount = 100M;
			discount.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			discount.AH_ExchangeRate = 2M;
			matchingBase.AddIMatching(discount);

			var exchangeDiff = (ExchangeDifference)matchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			exchangeDiff.AH_OSExTaxAmount = 100M;
			exchangeDiff.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exchangeDiff.AH_ExchangeRate = 2M;
			matchingBase.AddIMatching(exchangeDiff);

			var bankFeeJournal = (Journal)matchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Journal);
			bankFeeJournal.AH_OSExTaxAmount = 100M;
			bankFeeJournal.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bankFeeJournal.AH_ExchangeRate = 2M;
			matchingBase.AddIMatching(bankFeeJournal);
		}

		string GetTransactionType(ZString transactionType)
		{
			var result = ZString.Empty;

			switch (transactionType)
			{
				case TransactionTypes.Payment:
					result = "AccountingPayment";
					break;
				case TransactionTypes.Receipt:
					result = "AccountingReceipt";
					break;
				case TransactionTypes.Journal:
					result = "AccountingJournal";
					break;
				case TransactionTypes.Invoice:
					result = "AccountingInvoice";
					break;
				case TransactionTypes.CreditNote:
					result = "AccountingCreditNote";
					break;
				case TransactionTypes.AdjustmentNote:
					result = "AccountingAdjustmentNote";
					break;
				case TransactionTypes.Contra:
					result = "AccountingContra";
					break;
				case TransactionTypes.Transfer:
					result = "AccountingTransfer";
					break;
				case TransactionTypes.Overpayment:
					result = "AccountingOverpayment";
					break;
				case TransactionTypes.Discount:
					result = "AccountingDiscount";
					break;
				case TransactionTypes.ExchangeDifference:
					result = "AccountingExchangeDifferences";
					break;
				default:
					break;
			}

			return result;
		}

		void AssertTransactionDataObject(AccTransactionHeader source, UniversalTransaction target, bool isCashBasisVAT = false, AccCashBasisVAT cashVAT = null, bool hasTax = false)
		{
			var postingJournal = target.PostingJournalCollection.First();
			CombineAssertions(delegate
			{
				AssertNotNull("DataContext", target.DataContext);
				AssertNotNull("DataContext.DataSourceCollection", target.DataContext.DataSourceCollection);
				AssertEquals("DataContext.DataSourceCollection has one element", 1, target.DataContext.DataSourceCollection.Count());
				AssertEquals("DataContext.DataSourceCollection.First().Type", GetTransactionType(source.AH_TransactionType), target.DataContext.DataSourceCollection.First().Type);
				AssertEquals("DataContext.DataSourceCollection.First().Key", source.AH_Ledger + " " + source.AH_TransactionType + " " + source.AH_TransactionNum, target.DataContext.DataSourceCollection.First().Key);

				AssertEquals("Ledger", source.AH_Ledger, target.Ledger);
				AssertEquals("TransactionType", (TransactionType)Enum.Parse(typeof(TransactionType), source.AH_TransactionType, true), target.TransactionType);
				AssertEquals("PostDate", source.AH_PostDate, target.PostDate);
				AssertEquals("Number", source.AH_TransactionNum, target.Number);

				AssertNotNull("Branch", target.Branch);
				AssertEquals("Branch.Code", source.Branch.GB_Code, target.Branch.Code);
				AssertNotNull("Department", target.Department);
				AssertEquals("Department.Code", source.Department.GE_Code, target.Department.Code);

				AssertNotNull("OSCurrency", target.OSCurrency);
				AssertEquals("OSCurrency.Code", source.AH_RX_NKTransactionCurrency, target.OSCurrency.Code);
				AssertEquals("OSTotal", source.AH_OSTotal, target.OSTotal);

				AssertNotNull("LocalCurrency", target.LocalCurrency);
				AssertEquals("LocalCurrency.Code", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, target.LocalCurrency.Code);

				AssertNotNull("PostingJournalCollection", target.PostingJournalCollection);

				var postingJournalCollectionCount = (source.AH_TransactionType == TransactionTypes.Contra || source.AH_TransactionType == TransactionTypes.Transfer) ? 2 : 1;
				AssertEquals("PostingJournalCollection has one element", postingJournalCollectionCount, target.PostingJournalCollection.Count);

				if (hasTax)
				{
					AssertNotNull("PostingJournalCollection.First().TaxRate", postingJournal.VATTaxID);
					AssertNotNull("PostingJournalCollection.First().TaxMessageID", postingJournal.TaxMessageID);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestFactory = new BusinessObjectFactory();
			TestObjectCreator = new TestObjectCreator(TestFactory);
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2021, 03, 01));
			TestOrg1 = TestFactory.NewWithValidTestData<OrgHeader>();
			var bank1 = TestFactory.NewWithValidTestData<AccBankAccount>();
			var cheques1 = TestFactory.NewWithValidTestData<AccChequeBook>();
			cheques1.AK_Code = "CH1";
			cheques1.AK_AB = bank1.PK;
			cheques1.AK_GB = GlbBranch.CurrentBranch.PK;
			TestFactory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateJobRevenueJournalControlAccount().PK.ToGuid());

			aRReceipt = TestFactory.NewWithValidTestData<ARReceipt>();
			aRReceipt.AH_LocalExTaxAmount = 100;
			aRReceipt.AH_OSTotalAmount = 100;

			aRPayment = TestFactory.NewWithValidTestData<ARPayment>();
			aRPayment.AH_LocalExTaxAmount = -100;
			aRPayment.AH_OSTotalAmount = -100;
			aRPayment.AH_AB = bank1.PK;
			aRPayment.ChequeBook = cheques1.PK;
			((IChequeNumberAutoAllocation)aRPayment).AssignChequeNumber("001");

			aRReceipt2 = TestFactory.NewWithValidTestData<ARReceipt>();
			aRReceipt2.AH_LocalExTaxAmount = 100;
			aRReceipt2.AH_OSTotalAmount = 100;

			aRPayment2 = TestFactory.NewWithValidTestData<ARPayment>();
			aRPayment2.AH_LocalExTaxAmount = 100;
			aRPayment2.AH_OSTotalAmount = 100;

			aRCreditNote = TestFactory.NewWithValidTestData<ARCreditNote>();
			TestObjectCreator.CreateInvoiceLine(aRCreditNote, aRCreditNote.TransactionCurrency, aRCreditNote.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

			aRInvoice = TestFactory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(aRInvoice, aRInvoice.TransactionCurrency, aRInvoice.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

			aRAdjustmentNote = TestFactory.NewWithValidTestData<ARAdjustmentNote>();
			TestObjectCreator.CreateInvoiceLine(aRAdjustmentNote, aRAdjustmentNote.TransactionCurrency, aRAdjustmentNote.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

			var transactionBelongsToGroupForContra = ZGuid.NewZGuid();
			aRContra = TestFactory.NewWithValidTestData<ARContraRow>();
			aRContra.AH_TransactionNum = "Contra";
			aRContra.AH_LocalExTaxAmount = 100;
			aRContra.AH_OSTotalAmount = 100;
			aRContra.AH_TransactionBelongsToGroup = transactionBelongsToGroupForContra;

			aRJournal = TestFactory.NewWithValidTestData<ARJournal>();
			aRJournal.AH_LocalExTaxAmount = 100;
			aRJournal.AH_OSTotalAmount = 100;

			aPReceipt = TestFactory.NewWithValidTestData<APReceipt>();
			aPReceipt.AH_LocalExTaxAmount = -100;
			aPReceipt.AH_OSTotalAmount = -100;

			aPReceipt2 = TestFactory.NewWithValidTestData<APReceipt>();
			aPReceipt2.AH_LocalExTaxAmount = 100;
			aPReceipt2.AH_OSTotalAmount = 100;

			aPPayment = TestFactory.NewWithValidTestData<APPayment>();
			aPPayment.AH_LocalExTaxAmount = 100;
			aPPayment.AH_OSTotalAmount = 100;
			aPPayment.AH_AB = bank1.PK;
			aPPayment.ChequeBook = cheques1.PK;
			((IChequeNumberAutoAllocation)aPPayment).AssignChequeNumber("002");

			aPPayment2 = TestFactory.NewWithValidTestData<APPayment>();
			aPPayment2.AH_LocalExTaxAmount = 100;
			aPPayment2.AH_OSTotalAmount = 100;

			aPCreditNote = TestFactory.NewWithValidTestData<APCreditNote>();
			TestObjectCreator.CreateInvoiceLine(aPCreditNote, aPCreditNote.TransactionCurrency, aPCreditNote.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

			aPInvoice = TestFactory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(aPInvoice, aPInvoice.TransactionCurrency, aPInvoice.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

			aPAdjustmentNote = TestFactory.NewWithValidTestData<APAdjustmentNote>();
			TestObjectCreator.CreateInvoiceLine(aPAdjustmentNote, aPAdjustmentNote.TransactionCurrency, aPAdjustmentNote.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

			aPContra = TestFactory.NewWithValidTestData<APContraRow>();
			aPContra.AH_TransactionNum = "Contra";
			aPContra.AH_LocalExTaxAmount = 100;
			aPContra.AH_OSTotalAmount = 100;
			aPContra.AH_TransactionBelongsToGroup = transactionBelongsToGroupForContra;

			aPJournal = TestFactory.NewWithValidTestData<APJournal>();
			aPJournal.AH_LocalExTaxAmount = 100;
			aPJournal.AH_OSTotalAmount = 100;

			TestFactory.Save();
		}

		protected abstract AccountingReceiptPaymentBatchDataWriter GetBatchDataWriter(IDataWritingManager manager);

		TestObjectCreator TestObjectCreator;
		BusinessObjectFactory TestFactory;
		OrgHeader TestOrg1;
		ARReceipt aRReceipt;
		ARReceipt aRReceipt2;
		ARPayment aRPayment;
		ARPayment aRPayment2;
		ARCreditNote aRCreditNote;
		ARInvoice aRInvoice;
		ARAdjustmentNote aRAdjustmentNote;
		ARContraRow aRContra;
		ARJournal aRJournal;
		APReceipt aPReceipt;
		APReceipt aPReceipt2;
		APPayment aPPayment;
		APPayment aPPayment2;
		APCreditNote aPCreditNote;
		APInvoice aPInvoice;
		APAdjustmentNote aPAdjustmentNote;
		APContraRow aPContra;
		APJournal aPJournal;
	}
}
