using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static NUnit.Framework.Assertion;
using ReceiptTypes = Enterprise.ZArchitecture.Core.ReceiptTypes;

namespace Enterprise.Accounting.Utility.Testing
{
	public class PaymentBatchPostingTestHelper
	{
		public PaymentBatchPostingTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public void BatchPoster_OnPaymentDeletedFromBatch(object sender, EventArgs e)
		{
			OnPaymentDeletedFromBatch_EventFiredDuringTest = ZBool.True;
		}

		public AccChequeBook GetAutoPrintChequeBook()
		{
			return GetAutoPrintChequeBook(null, 0, 0, 0);
		}

		public AccChequeBook GetAutoPrintChequeBook(BusinessObjectFactory newFactory, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			var testFactory = newFactory ?? Factory;
			var bankAccount = testFactory.NewWithValidTestData<AccBankAccount>();

			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			var printQueue = testFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			var chequeBook = testFactory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			testFactory.Save();
			return chequeBook;
		}

		public void PrepareForBaseTest(bool shouldCreateSingleTransactionBatch = true)
		{
			USDSellRate = CreateExchangeRate(TestObjectCreator.USD, 0.75M, Core.Constants.ExchangeRateTypes.Code.SellRate);
			USDBuyRate = CreateExchangeRate(TestObjectCreator.USD, 0.75M, Core.Constants.ExchangeRateTypes.Code.BuyRate);

			TestOrg = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("667482e0-78cd-4a9f-9e34-8e805264ad3b"));
			TestOrg.FillWithValidTestData();
			TestOrg.OH_Code = "A_" + Accounting.Business.TestObjectCreator.GetRandomString(8);
			TestOrg.CompanyData.OB_IsCreditor = true;
			TestOrg.CompanyData.SetAPTaxApplicable(false);
			TestOrg.APSettlementGroupPK = TestOrg.PK;

			PeriodHelper = new AccountingPeriodTestHelper(Factory);
			PeriodHelper.SetupPeriods();

			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestBank.AB_ChequeNumDigits = 6;

			TestCheques = Factory.NewWithValidTestData<AccChequeBook>();
			TestCheques.AK_StartNo = 1;
			TestCheques.AK_LastNo = 100;
			TestCheques.AK_CurrentNo = 1;
			TestCheques.AK_AB = TestBank.PK;

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = TestObjectCreator.CreateJob(shipment, false);

			TestAPInv = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv.AH_OH = TestOrg.PK;
			ZGuid genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(TestAPInv, TestAPInv.TransactionCurrency, TestAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestAPInv.TransactionCurrency);
			TestAPInv.AH_Desc = "For Payment Approval 1";
			TestAPInv.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			if (shouldCreateSingleTransactionBatch)
			{
				CreateBatchPosterForBaseTest(GetSingleTransactionForPosting(), false);
			}
		}

		public APPaymentBatchPoster CreateBatchPosterForBaseTest(TransactionHeaderCollection transactions, bool shouldUseCheques = true)
		{
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			BatchPoster.APB_AB = TestBank.PK;
			if (shouldUseCheques)
			{
				BatchPoster.APB_AK = TestCheques.PK;
			}

			return BatchPoster;
		}

		protected RefExchangeRate CreateExchangeRate(RefCurrency currency, ZDecimal rate, ZString type)
		{
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			exchangeRate.RE_ExRateType = type;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			return exchangeRate;
		}

		public void SetupDataForPostingTest(bool saveTransaction = true)
		{
			CreateBatchPosterForBaseTest(GetTransactionsForPosting(saveTransaction));
			PaymentApproval1 = BatchPoster.PaymentApprovalCollection[0];
			PaymentApproval2 = BatchPoster.PaymentApprovalCollection[1];
			PaymentApproval3 = BatchPoster.PaymentApprovalCollection[2];
		}

		public APPaymentBatchPoster CreateBatchPosterWithMultipleTransactionsWithForeignCurrency()
		{
			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.USD, 1m, 1000m, 0m, 1000m, 0m);
			invoice1.AH_OH = TestObjectCreator.Creditor1.PK;
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV002", TestObjectCreator.CNY, 2m, 2000m, 0m, 1000m, 0m);
			invoice2.AH_OH = TestObjectCreator.Creditor2.PK;
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV003", TestObjectCreator.TWD, 3m, 3000m, 0m, 1000m, 0m);
			invoice3.AH_OH = TestObjectCreator.Creditor3.PK;
			Factory.Save();

			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice1);
			transactions.Add(invoice2);
			transactions.Add(invoice3);

			var today = ZDateTime.Now;
			CreateBatchPosterForBaseTest(transactions);
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;

			using (Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Draft;
				BatchPoster.PaymentApprovalCollection[1].AV_Status = PaymentApprovalStatus.Draft;
				BatchPoster.PaymentApprovalCollection[2].AV_Status = PaymentApprovalStatus.Draft;

				BatchPoster.MatchTransactions();
				Factory.Save();
			}

			var paymentApprovalS = Factory.Load<AccPaymentApproval>(new ZQuery(AccPaymentApprovalSchema.AV_APB_PaymentBatch, BatchPoster.PK));
			AssertEquals(3, paymentApprovalS.Length);
			AssertEquals(true, paymentApprovalS.All(x => x.AV_Status == PaymentApprovalStatus.Draft));

			return BatchPoster;
		}

		public TransactionHeaderCollection GetTransactionsForPosting(bool saveTransactions = true)
		{
			if (TestOrg2 == null)
			{
				TestOrg2 = CreateCreditorTestOrg(new Guid("ff715a34-30c0-4646-9c32-1de8ff4925d0"));
			}

			if (TestOrg3 == null)
			{
				TestOrg3 = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("5776671f-570c-42ce-bb65-e447e4aec4a3"));
				TestOrg3.FillWithValidTestData();
				TestOrg3.OH_Code = "C_" + TestObjectCreator.GetRandomString(8);
				TestOrg3.CompanyData.OB_IsCreditor = true;
				TestOrg3.APSettlementGroupPK = ZGuid.Empty;
			}

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = TestObjectCreator.CreateJob(shipment, false);

			TestAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv2.AH_OH = TestOrg2.PK;
			TestAPInv2.AH_Desc = "For Payment Approval 2";
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(TestAPInv2, TestAPInv2.TransactionCurrency, TestAPInv2.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;

			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestAPInv2.TransactionCurrency);

			TestAPRec = Factory.NewWithValidTestData<APReceipt>();
			TestAPRec.AH_OH = TestOrg3.PK;
			TestAPRec.AH_LocalExTaxAmount = 94M;
			TestAPRec.AH_OSExTaxAmount = 94M;
			TestAPRec.AH_Desc = "For Payment Approval 3";

			if (saveTransactions)
			{
				Factory.Save();
			}

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(TestAPInv);
			transactions.Add(TestAPInv2);
			transactions.Add(TestAPRec);
			return transactions;
		}

		public OrgHeader CreateCreditorTestOrg(Guid? specifiedGuid)
		{
			var testOrg = specifiedGuid.HasValue ? Factory.NewWithPrimaryKey<OrgHeader>(specifiedGuid.Value) : Factory.NewWithValidTestData<OrgHeader>();
			testOrg.FillWithValidTestData();
			testOrg.OH_Code = "B_" + TestObjectCreator.GetRandomString(8);
			testOrg.CompanyData.OB_IsCreditor = true;
			testOrg.APSettlementGroupPK = testOrg.PK;
			return testOrg;
		}

		public TransactionHeaderCollection GetTransactionsForSplittingTest()
		{
			TestObjectCreator.GLHeader1.AG_Description = "Test";
			TransactionHeaderCollection transactions = GetTransactionsForPosting();

			OrgHeader testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg4.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg4.CompanyData.OB_IsCreditor = true;
			testOrg4.APSettlementGroupPK = TestOrg3.PK;

			OrgHeader testOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg5.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg5.CompanyData.OB_IsCreditor = true;
			testOrg5.APSettlementGroupPK = TestOrg2.PK;

			TestOrg6 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg6.OH_Code = TestObjectCreator.GetRandomString(10);
			TestOrg6.CompanyData.OB_IsCreditor = true;
			TestOrg6.APSettlementGroupPK = ZGuid.Empty;

			TestOrg7 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg7.OH_Code = TestObjectCreator.GetRandomString(10);
			TestOrg7.CompanyData.OB_IsCreditor = true;
			TestOrg7.APSettlementGroupPK = TestOrg7.PK;

			OrgHeader testOrg8 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg8.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg8.CompanyData.OB_IsCreditor = true;
			testOrg8.APSettlementGroupPK = TestOrg7.PK;

			TestAPRec2 = Factory.NewWithValidTestData<APReceipt>();
			TestAPRec2.AH_OH = testOrg4.PK;
			TestAPRec2.AH_LocalExTaxAmount = 94M;
			TestAPRec2.AH_OSExTaxAmount = 94M;
			TestAPRec2.AH_Desc = "For Payment Approval 3. TestAPRec2";

			TestAPInv3 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv3.AH_OH = testOrg5.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv3, TestAPInv3.TransactionCurrency, TestAPInv3.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.GLHeader1.PK);
			TestAPInv3.AH_Desc = "For Payment Approval 2";

			TestAPInv4 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv4.AH_OH = TestOrg3.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv4, TestAPInv4.TransactionCurrency, TestAPInv4.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.GLHeader1.PK);
			TestAPInv4.AH_Desc = "For Payment Approval 3";

			TestAPInv5 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv5.AH_OH = TestOrg6.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv5, TestAPInv5.TransactionCurrency, TestAPInv5.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.GLHeader1.PK);
			TestAPInv5.AH_Desc = "For Payment Approval 4";

			TestAPInv6 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv6.AH_OH = testOrg8.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv6, TestAPInv6.TransactionCurrency, TestAPInv6.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.GLHeader1.PK);
			TestAPInv6.AH_Desc = "For Payment Approval 5";

			transactions.Add(TestAPRec2);
			transactions.Add(TestAPInv3);
			transactions.Add(TestAPInv4);
			transactions.Add(TestAPInv5);
			transactions.Add(TestAPInv6);

			Factory.Save();

			return transactions;
		}

		public TransactionHeaderCollection GetSingleTransactionForPosting()
		{
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(TestAPInv);
			return transactions;
		}

		public void PrepareForTestChequeNumberInUse()
		{
			Job testJob = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 1m, TestObjectCreator.Agent);
			charge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_OSCostExRate = 1m;
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_AB = TestBank.PK;
			charge.BankAccount.AB_ChequeNumDigits = 6;
			charge.JR_AK = TestCheques.PK;
			charge.JR_ChequeNo = "000001";
			Assert("Cheque number is not used yet", !charge.JR_ChequeNoInfo.HasErrors());

			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = TestObjectCreator.ABIGAS.PK;
			paymentApproval.AV_AB = TestBank.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval.AV_AK = TestCheques.PK;
			paymentApproval.AV_ChequeOrReference = "000002";
			Assert("Cheque number is not used yet", !paymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_AK = TestCheques.PK;
			hotCheque.AQ_ChequeNumber = "000003";
			Assert("Cheque number is not used yet", !hotCheque.AQ_ChequeNumberInfo.HasErrors());

			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment.AH_AB = TestBank.PK;
			payment.ChequeBook = TestCheques.PK;
			payment.AH_ChequeOrReference = "000004";
			Assert("Cheque number is not used yet", !payment.AH_ChequeOrReferenceInfo.HasErrors());

			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			directPayment.AH_AB = TestBank.PK;
			directPayment.ChequeBookPK = TestCheques.PK;
			directPayment.AH_ChequeOrReference = "000005";
			Assert("Cheque number is not used yet", !directPayment.AH_ChequeOrReferenceInfo.HasErrors());

			APPayment reversedPayment = Factory.NewWithValidTestData<APPayment>();
			reversedPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			reversedPayment.AH_AB = TestBank.PK;
			reversedPayment.ChequeBook = TestCheques.PK;
			reversedPayment.AH_ChequeOrReference = "000006";
			reversedPayment.AH_IsCancelled = true;
			((IMatching)reversedPayment).CurrentMatchGroup.AddNew().AP_AH = reversedPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(reversedPayment);
			Factory.Save();
		}

		public void AssertBatchPosterWillMatchAndPost(APPaymentBatchPoster batchPoster, ZInt countOfApprovalItemsShouldBeCreated, ZInt countOfPaymentsShouldBeCreated, ZInt countOfMatchedTransactions)
		{
			AssertBatchPosterWillMatchAndPost(batchPoster, countOfApprovalItemsShouldBeCreated, countOfPaymentsShouldBeCreated, countOfMatchedTransactions, ZBool.True);
		}

		public void AssertBatchPosterWillMatchAndPost(APPaymentBatchPoster batchPoster, ZInt countOfApprovalItemsShouldBeCreated, ZInt countOfPaymentsShouldBeCreated, ZInt countOfMatchedTransactions, ZBool needToSave)
		{
			if (needToSave)
			{
				batchPoster.MatchTransactions();
				AssertEquals("Should successfully saved.", true, SimulateSave(batchPoster));
			}

			BusinessObjectFactory emptyFactory = new BusinessObjectFactory();
			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(emptyFactory);
			approvalItems.Load();
			TransactionHeaderCollection paymentBatch = new TransactionHeaderCollection(emptyFactory);
			ZQuery paymentFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Payment);
			paymentBatch.Load(paymentFilter);

			APPaymentApprovalMatching matchingBizO = batchPoster.PaymentApprovalCollection[0].MatchingBaseObject as APPaymentApprovalMatching;
			AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);
			if (countOfMatchedTransactions != -1)
			{
				AssertEquals("Should be " + countOfMatchedTransactions.ToString() + " transactions in MatchedTransactions", countOfMatchedTransactions, matchingBizO.MatchedTransactions.Count);
			}

			if (countOfApprovalItemsShouldBeCreated != -1)
			{
				AssertEquals(countOfApprovalItemsShouldBeCreated.ToString() + " PaymentApprovalItem should be posted to DB", countOfApprovalItemsShouldBeCreated, approvalItems.Count);
			}
			if (countOfPaymentsShouldBeCreated != -1)
			{
				AssertEquals("There should " + countOfPaymentsShouldBeCreated.ToString() + " payments created", countOfPaymentsShouldBeCreated, paymentBatch.Count);
			}

			Assert("PaymentApproval should be in database", batchPoster.PaymentApprovalCollection[0].IsInDatabase);
			Assert("PaymentApproval should be readonly", batchPoster.PaymentApprovalCollection[0].ReadOnly);
		}

		public void AssertBatchPosterWillNotMatchAndPost(APPaymentBatchPoster batchPoster)
		{
			AssertBatchPosterWillNotMatchAndPost(batchPoster, ZBool.False);
		}

		public void AssertBatchPosterWillNotMatchAndPost(APPaymentBatchPoster batchPoster, ZBool needToSave)
		{
			if (needToSave)
			{
				batchPoster.MatchTransactions();
				AssertEquals("Should successfully saved.", true, SimulateSave(batchPoster));
			}

			BusinessObjectFactory emptyFactory = new BusinessObjectFactory();

			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(emptyFactory);
			approvalItems.Load();

			TransactionHeaderCollection paymentBatch = new TransactionHeaderCollection(emptyFactory);
			ZQuery paymentFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Payment);
			paymentBatch.Load(paymentFilter);

			AssertEquals("There should be no payments created", 0, paymentBatch.Count);
			AssertEquals("No Approval Items should be posted", 0, approvalItems.Count);
		}

		public ZBool CollectionIsSorted(TransactionHeaderCollection transactions)
		{
			TransactionHeader previousTransaction = null;
			foreach (TransactionHeader transaction in transactions)
			{
				if (previousTransaction != null)
				{
					if (previousTransaction.OH_APSettlementGroup != ZGuid.Empty && transaction.OH_APSettlementGroup != ZGuid.Empty && previousTransaction.OH_APSettlementGroup != transaction.OH_APSettlementGroup)
					{
						if (previousTransaction.OH_APSettlementGroup > transaction.OH_APSettlementGroup)
						{
							return ZBool.False;
						}
					}
					else if (previousTransaction.OH_APSettlementGroup == ZGuid.Empty && transaction.OH_APSettlementGroup == ZGuid.Empty && previousTransaction.AH_OH != transaction.AH_OH)
					{
						if (previousTransaction.AH_OH > transaction.AH_OH)
						{
							return ZBool.False;
						}
					}
				}
				previousTransaction = transaction;
			}
			return ZBool.True;
		}

		bool SimulateSave(APPaymentBatchPoster batchPoster)
		{
			batchPoster.RunPreSaveValidation();
			if (!batchPoster.HasErrors)
			{
				batchPoster.Factory.Save();
				return true;
			}

			return false;
		}

		public TransactionHeaderCollection MakeCollectionToBeUnsorted(TransactionHeaderCollection transactions)
		{
			if (CollectionIsSorted(transactions))
			{
				TransactionHeaderCollection newCollection = new TransactionHeaderCollection(Factory);
				for (int i = transactions.Count - 1; i > -1; i--)
				{
					newCollection.Add(transactions[i]);
				}
				return newCollection;
			}
			return transactions;
		}

		public PaymentApprovalBase FindPaymentApprovalThatContainSpecificTransaction(PaymentApprovalBaseCollection paymentBatch, TransactionHeader transaction)
		{
			foreach (PaymentApprovalBase payment in paymentBatch)
			{
				if (payment.MatchingBaseObject.MatchedTransactions.Contains(transaction))
				{
					return payment;
				}
			}
			return null;
		}

		public void TestSaveReloadAndPostPaymentApprovalItems_Core(
			ZDecimal invoice1OSAmount, ZDecimal invoice1ExchangeRate, ZDecimal invoice1LocalAmount, RefCurrency invoice1Currency,
			ZDecimal invoice2OSAmount, ZDecimal invoice2ExchangeRate, ZDecimal invoice2LocalAmount, RefCurrency invoice2Currency,
			ZDecimal approvalOSAmount, ZDecimal approvalExchangeRate, ZDecimal approvalLocalAmount, RefCurrency approvalCurrency, ZDateTime paymentDate, ZDateTime postDate,
			bool postAsPaymentApprovals = false)
		{
			BusinessObjectFactory invoiceCreationFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreatorInInvoiceCreationFactory = new TestObjectCreator(invoiceCreationFactory);

			APInvoice invoice1 = invoiceCreationFactory.New<APInvoice>();
			invoice1.AH_OH = TestOrg.PK;
			invoice1.AH_TransactionNum = "00001100";
			invoice1.AH_RX_NKTransactionCurrency = invoice1Currency.RX_Code;
			invoice1.AH_ExchangeRate = invoice1ExchangeRate;
			testObjectCreatorInInvoiceCreationFactory.CreateInvoiceLine(invoice1, invoice1Currency, invoice1ExchangeRate, invoice1OSAmount, 0m, 0m);
			invoiceCreationFactory.Save();

			invoice1 = Factory.Load<APInvoice>(invoice1.PK);
			AssertEquals("Precondition: OS Invoice Amount", invoice1OSAmount, invoice1.AH_OSExTaxAmount);
			AssertEquals("Precondition: Invoice Exchange Rate", invoice1ExchangeRate, invoice1.AH_ExchangeRate);
			AssertEquals("Precondition: Local Invoice Amount", invoice1LocalAmount, invoice1.AH_LocalExTaxAmount);

			APInvoice invoice2 = invoiceCreationFactory.New<APInvoice>();
			invoice2.AH_OH = TestOrg.PK;
			invoice2.AH_TransactionNum = "00001101";
			invoice2.AH_RX_NKTransactionCurrency = invoice2Currency.RX_Code;
			invoice2.AH_ExchangeRate = invoice2ExchangeRate;
			testObjectCreatorInInvoiceCreationFactory.CreateInvoiceLine(invoice2, invoice2Currency, invoice2ExchangeRate, invoice2OSAmount, 0m, 0m);
			invoiceCreationFactory.Save();

			invoice2 = Factory.Load<APInvoice>(invoice2.PK);
			AssertEquals("Precondition: OS Invoice Amount", invoice2OSAmount, invoice2.AH_OSExTaxAmount);
			AssertEquals("Precondition: Invoice Exchange Rate", invoice2ExchangeRate, invoice2.AH_ExchangeRate);
			AssertEquals("Precondition: Local Invoice Amount", invoice2LocalAmount, invoice2.AH_LocalExTaxAmount);
			invoiceCreationFactory.Save();

			AccBankAccount bankAccount = approvalExchangeRate == 1m ? TestObjectCreator.AUDBankAccount : TestObjectCreator.USDBankAccount;
			bankAccount.AB_RX_NKAccountCurrency = approvalCurrency.RX_Code;
			AccChequeBook chequeBook = approvalExchangeRate == 1m ? TestObjectCreator.AUDChequeBook : TestObjectCreator.USDChequeBook;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice1);
			transactions.Add(invoice2);

			CreateBatchPosterForBaseTest(transactions);
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.APB_AK = chequeBook.PK;
			AssertEquals("There should be 1 payment in the collection", 1, BatchPoster.PaymentApprovalCollection.Count);
			PaymentApprovalBase testPaymentApproval = BatchPoster.PaymentApprovalCollection[0];

			if (paymentDate != ZDateTime.Empty)
			{
				BatchPoster.APB_PaymentDate = paymentDate;
			}
			if (postDate != ZDateTime.Empty)
			{
				BatchPoster.APB_PostDate = postDate;
			}

			testPaymentApproval.AV_Amount = approvalOSAmount;
			BatchPoster.APB_ChequeOrReference = chequeBook.AK_CurrentNo.ToString();
			AssertEquals("Precondition: Payment Approval OS Amount", approvalOSAmount, testPaymentApproval.AV_Amount);
			AssertEquals("Precondition: Payment Approval OS Amount", approvalLocalAmount, testPaymentApproval.AV_Calc_LocalAmount);

			AssertEquals("Matched Transactions Count", 2, testPaymentApproval.MatchingBaseObject.MatchedTransactions.Count);

			Assert("Matched Transactions should contain Invoice1", testPaymentApproval.MatchingBaseObject.MatchedTransactions.Contains(invoice1));
			AssertEquals("Invoice1 OsPartialPaymentAmount", -invoice1OSAmount, ((IMatching)invoice1).OSPartialPaymentAmount);
			AssertEquals("Invoice1 LocalPartialPaymentAmount", -invoice1LocalAmount, ((IMatching)invoice1).LocalPartialPaymentAmount);

			Assert("Matched Transactions should contain Invoice2", testPaymentApproval.MatchingBaseObject.MatchedTransactions.Contains(invoice2));
			AssertEquals("Invoice2 OsPartialPaymentAmount", -invoice2OSAmount, ((IMatching)invoice2).OSPartialPaymentAmount);
			AssertEquals("Invoice2 LocalPartialPaymentAmount", -invoice2LocalAmount, ((IMatching)invoice2).LocalPartialPaymentAmount);

			BatchPoster.MatchTransactions();

			Assert("Matched Transactions should contain PaymentApprovalBase", testPaymentApproval.MatchingBaseObject.MatchedTransactions.Contains(testPaymentApproval));
			AssertEquals("Payment Approval OsPartialPaymentAmount", approvalOSAmount, ((IMatching)testPaymentApproval).OSPartialPaymentAmount);
			AssertEquals("Payment Approval LocalPartialPaymentAmount", approvalLocalAmount, ((IMatching)testPaymentApproval).LocalPartialPaymentAmount);

			BatchPoster.PostPaymentsAsPaymentApprovals = postAsPaymentApprovals;

			AssertEquals("Should successfully saved.", true, SimulateSave(BatchPoster));

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(testPaymentApproval);
			approvalItems.Load();
			AssertEquals("Approval Items Created", 2, approvalItems.Count);

			ZQuery invoice1PaymentItemQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AV, testPaymentApproval.PK);
			invoice1PaymentItemQuery.AddToFilter(AccPaymentApprovalItemSchema.A2_AH, invoice1.PK);
			PaymentApprovalItem invoice1ApprovalItem = Factory.LoadTop1<PaymentApprovalItem>(invoice1PaymentItemQuery);
			AssertNotNull("Invoice1ApprovalItem", invoice1ApprovalItem);
			AssertEquals("Invoice1ApprovalItem PaymentApprovalBase", invoice1ApprovalItem.PaymentApproval.PK, testPaymentApproval.PK);
			AssertEquals("Invoice1ApprovalItem TransactionHeader", invoice1ApprovalItem.TransactionHeader.PK, invoice1.PK);
			AssertEquals("Invoice1ApprovalItem PaymentThisRun", -invoice1LocalAmount, invoice1ApprovalItem.A2_PaymentThisRun);

			ZQuery invoice2PaymentItemQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AV, testPaymentApproval.PK);
			invoice2PaymentItemQuery.AddToFilter(AccPaymentApprovalItemSchema.A2_AH, invoice2.PK);
			PaymentApprovalItem invoice2ApprovalItem = Factory.LoadTop1<PaymentApprovalItem>(invoice2PaymentItemQuery);
			AssertNotNull("Invoice2ApprovalItem", invoice2ApprovalItem);
			AssertEquals("Invoice2ApprovalItem PaymentApprovalBase", invoice2ApprovalItem.PaymentApproval.PK, testPaymentApproval.PK);
			AssertEquals("Invoice2ApprovalItem TransactionHeader", invoice2ApprovalItem.TransactionHeader.PK, invoice2.PK);
			AssertEquals("Invoice2ApprovalItem PaymentThisRun", -invoice2LocalAmount, invoice2ApprovalItem.A2_PaymentThisRun);

			PaymentApprovalBase reloadedApproval = newFactory.Load<PaymentApprovalBase>(testPaymentApproval.PK);
			AssertEquals("Approval Currency", approvalCurrency.RX_Code, reloadedApproval.AV_RX_NKPaymentCurrency);
			AssertEquals("Approval OS Amount", approvalOSAmount, reloadedApproval.AV_Amount);
			AssertEquals("Approval Local Amount", approvalLocalAmount, reloadedApproval.AV_Calc_LocalAmount);

			AssertEquals("Payment Approval OS Amount", approvalOSAmount, testPaymentApproval.AV_Amount);
			AssertEquals("Payment Approval Exchange Rate", approvalExchangeRate, testPaymentApproval.AV_PayExRate);
			AssertEquals("Payment Approval OS Amount", approvalLocalAmount, testPaymentApproval.AV_Calc_LocalAmount);

			AssertEquals("Reloaded Approval Matched Transactions Count", 3, reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions Contains BatchPoster", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(reloadedApproval.PK));
			Assert("Matched Transactions Contains Invoice1", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice1.PK));
			Assert("Matched Transactions Contains Invoice2", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice2.PK));

			APInvoice invoice1Reloaded = newFactory.Load<APInvoice>(invoice1.PK);
			AssertNotNull(invoice1Reloaded);
			AssertEquals("Invoice1 OsPartialPaymentAmount", -invoice1OSAmount, ((IMatching)invoice1).OSPartialPaymentAmount);
			AssertEquals("Invoice1 LocalPartialPaymentAmount", -invoice1LocalAmount, ((IMatching)invoice1).LocalPartialPaymentAmount);

			APInvoice invoice2Reloaded = newFactory.Load<APInvoice>(invoice2.PK);
			AssertNotNull(invoice2Reloaded);
			AssertEquals("Invoice2 OsPartialPaymentAmount", -invoice2OSAmount, ((IMatching)invoice2Reloaded).OSPartialPaymentAmount);
			AssertEquals("Invoice2 LocalPartialPaymentAmount", -invoice2LocalAmount, ((IMatching)invoice2Reloaded).LocalPartialPaymentAmount);

			Assert("PaymentCreationErrorMessages should be empty. PaymentCreationErrorMessages: " +
				testPaymentApproval.PaymentCreationErrorMessages, !testPaymentApproval.PaymentCreationErrorMessages.HasErrors());

			if (postAsPaymentApprovals)
			{
				AssertEquals(PaymentApprovalStatus.FullyApproved, reloadedApproval.AV_Status);
				AssertEquals("00001000", reloadedApproval.PaymentBatchNumber);
			}
			else
			{
				Assert("New Payment should be created", !testPaymentApproval.AV_AH.IsEmpty);

				AssertNotNull("New Payment", testPaymentApproval.TransactionHeader);
				TransactionHeader newPayment = Factory.Load<TransactionHeader>(testPaymentApproval.AV_AH);

				AssertEquals("New Payment AH_OSExTaxAmount", approvalOSAmount, newPayment.AH_OSExTaxAmount);
				AssertEquals("New Payment AH_LocalExTaxAmount", approvalLocalAmount, newPayment.AH_LocalExTaxAmount);

				ZQuery newPaymentMatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newPayment.PK);
				TransactionMatchLinkCollection newPaymentMatchLinks = new TransactionMatchLinkCollection(newFactory, newPaymentMatchLinkQuery);
				newPaymentMatchLinks.Load();
				AssertEquals("NewPaymentMatchLinks Count", 1, newPaymentMatchLinks.Count);
				AssertEquals("NewPayment MatchLink Amount", approvalLocalAmount, newPaymentMatchLinks[0].AP_Amount);
				Assert("NewPaymentMatchLink Match Group Number should not be empty", !newPaymentMatchLinks[0].AP_MatchGroupNum.IsEmpty);
				ZString matchGroupNum = newPaymentMatchLinks[0].AP_MatchGroupNum;

				ZQuery matchLinksQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(newFactory, matchLinksQuery);
				matchLinks.Load();
				AssertEquals("MatchLinks Count", 3, matchLinks.Count);

				ZQuery invoice1MatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK);
				invoice1MatchLinkQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
				TransactionMatchLink invoice1Link = Factory.LoadTop1<TransactionMatchLink>(invoice1MatchLinkQuery);
				AssertNotNull("Invoice1Link", invoice1Link);
				AssertEquals("Invoice 1 MatchLink Amount", -invoice1LocalAmount, invoice1Link.AP_Amount);

				ZQuery invoice2MatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK);
				invoice2MatchLinkQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
				TransactionMatchLink invoice2Link = Factory.LoadTop1<TransactionMatchLink>(invoice2MatchLinkQuery);
				AssertNotNull("Invoice2Link", invoice2Link);
				AssertEquals("Invoice 2 MatchLink Amount", -invoice2LocalAmount, invoice2Link.AP_Amount);

				AssertEquals("Payment Date", testPaymentApproval.AV_PaymentDate, newPayment.AH_InvoiceDate);
				AssertEquals("Post Date", testPaymentApproval.AV_PostDate, newPayment.AH_PostDate);
				AssertEquals("Payment Branch", testPaymentApproval.AV_GB, newPayment.AH_GB);
				AssertEquals("Payment Department", GlbDepartment.CurrentDepartment.PK, newPayment.AH_GE);
				AssertEquals("Payment Creditor", testPaymentApproval.AV_OH, newPayment.AH_OH);
				AssertEquals("Payment Description", testPaymentApproval.AV_PaymentComment, newPayment.AH_Desc);
				AssertEquals("Payment ReceiptType", testPaymentApproval.AV_PaymentType, newPayment.AH_ReceiptType);
				AssertEquals("Payment BankAccount", testPaymentApproval.AV_AB, newPayment.AH_AB);
				AssertEquals("Payment Currency", testPaymentApproval.AV_RX_NKPaymentCurrency, newPayment.AH_RX_NKTransactionCurrency);
				AssertEquals("Payment ChequeOrReference", testPaymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
				AssertEquals("Payment ExchangeRate", testPaymentApproval.AV_PayExRate, newPayment.AH_ExchangeRate);
				AssertEquals("Payment OSExTaxAmount", testPaymentApproval.AV_Amount, newPayment.AH_OSExTaxAmount);
				AssertEquals("Payment LocalExTaxAmount", testPaymentApproval.AV_Calc_LocalAmount, newPayment.AH_LocalExTaxAmount);
			}
		}

		public TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		public OrgHeader TestOrg;
		public OrgHeader TestOrg2;
		public OrgHeader TestOrg3;
		public OrgHeader TestOrg6;
		public OrgHeader TestOrg7;

		public APInvoice TestAPInv;
		public APInvoice TestAPInv2;
		public APInvoice TestAPInv3;
		public APInvoice TestAPInv4;
		public APInvoice TestAPInv5;
		public APInvoice TestAPInv6;
		public APReceipt TestAPRec;
		public APReceipt TestAPRec2;
		public APPaymentBatchPoster BatchPoster;

		public RefExchangeRate USDSellRate;
		public RefExchangeRate USDBuyRate;

		public AccBankAccount TestBank;
		public AccBankAccount AnotherBank;
		public AccChequeBook TestCheques;

		public PaymentApprovalBase PaymentApproval1;
		public PaymentApprovalBase PaymentApproval2;
		public PaymentApprovalBase PaymentApproval3;

		public ZBool OnPaymentDeletedFromBatch_EventFiredDuringTest;

		readonly BusinessObjectFactory Factory;
		AccountingPeriodTestHelper PeriodHelper;
	}
}
