using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class APPaymentApprovalCreatorTest : BaseTransactionCreatorTest
	{
		protected override BaseTransactionCreator GetTransactionCreator(Job job, IJobCostingPlugIn consol)
		{
			return new APPaymentApprovalCreator(job, consol, false);
		}

		protected override void PrepareCreator(Job job, TransactionCreatorHashtable transactions)
		{
			APInvoiceCreator invoiceCreator = new APInvoiceCreator(job);
			invoiceCreator.CreateTransactions(transactions);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateTransactionsDoesNotLoadTransactionsFromDB()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			Factory.Save();

			APPaymentApprovalCreator creator = new APPaymentApprovalCreator(job);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			int beforeHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
			creator.CreateTransactions(transactions);
			Factory.Save();
			int afterHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals("AccTransactionHeader table should not be loaded", beforeHit, afterHit);
		}

		#region TEST: Create One Payment Against One AP Invoice With Payment Info

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateOnePaymentAgainstOneInvoiceWithPaymentInfo()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("Payment Count", 1, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region AUD Payment

			PaymentApprovalBase paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);
			APPayment aUDPayment = (APPayment)paymentApproval.NewPayment;
			AssertTransactionHeaderValues(aUDPayment, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
				110.00M, 0M, 0M, 110M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment);

			#endregion
		}

		#endregion

		#region TEST: Create One Payment Against Two Invoices With Same Payment Info

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateOnePaymentAgainstTwoInvoicesWithSamePaymentInfo()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge7, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("Payment Count", 1, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region AUD Payment

			PaymentApprovalBase paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);
			APPayment aUDPayment = (APPayment)paymentApproval.NewPayment;
			AssertTransactionHeaderValues(aUDPayment, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
				310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment);

			#endregion
		}

		#endregion

		#region TEST: Create Two Payments Against Three Invoices Two With Same Payment Info

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateTwoPaymentsAgainstThreeInvoicesTwoWithSamePaymentInfo()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			Job job = CreateJob("Z00001002", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge6, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge7, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			AssertEquals("Payment Count", 2, transactions.GetAllAPPaymentApprovals().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region AUD Payment

			PaymentApprovalBase audPaymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);
			JobInvoicingPaymentApprovalMatcher approvalMatcher = new JobInvoicingPaymentApprovalMatcher(job, Now);
			approvalMatcher.CreateTransactions(transactions);
			audPaymentApproval.CreateNewPayment();

			APPayment aUDPayment = (APPayment)audPaymentApproval.NewPayment;
			AssertTransactionHeaderValues(aUDPayment, "AP", "PAY", null, "AP Payment Z00001002", Now, ZDateTime.Empty,
				310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment);

			#endregion

			#region USD Payment

			PaymentApprovalBase usdPaymentApproval = transactions.RetrieveAPPaymentApproval(Creditor2, USDBankAccount, "CCD", "CC", job.JH_JobNum);
			usdPaymentApproval.CreateNewPayment();

			APPayment uSDPayment = (APPayment)usdPaymentApproval.NewPayment;
			AssertTransactionHeaderValues(uSDPayment, "AP", "PAY", null, "AP Payment Z00001002", Now, ZDateTime.Empty,
				314.28M, 0M, 0M, 220M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
				USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment);

			#endregion
		}

		#endregion

		#region TEST: Don't Create Payment If Payment Amount Totals Zero

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestDoNotCreatePaymentIfPaymentAmountTotalsZero()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, -110M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge7, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			Factory.Save();

			APPaymentApprovalCreator creator = new APPaymentApprovalCreator(job);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			AssertEquals("Payment Count", 0, transactions.APTransactionsCount);
			AssertEquals("No Transactions", 0, transactions.Count);
		}

		#endregion

		#region TEST: (Auto Allocation) Create Two Payments Against Three Invoices Two With Same Payment Info

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestAutoAllocation_CreateTwoPaymentsAgainstThreeInvoicesTwoWithSamePaymentInfo()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			AccChequeBook autoAllocateChequeBook = GetAutoPrintChequeBook(AUDBankAccount);
			autoAllocateChequeBook.AK_StartNo = 1;
			autoAllocateChequeBook.AK_LastNo = 4;
			autoAllocateChequeBook.AK_CurrentNo = 2;
			autoAllocateChequeBook.AK_Code = "Chk1";
			AccChequeBook autoAllocateChequeBook2 = GetAutoPrintChequeBook(AUDBankAccount);
			autoAllocateChequeBook2.AK_StartNo = 1;
			autoAllocateChequeBook2.AK_LastNo = 4;
			autoAllocateChequeBook2.AK_CurrentNo = 3;
			autoAllocateChequeBook2.AK_Code = "Chk2";

			Job job = CreateJob("Z00001002", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor1, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "4", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cheque, AUDBankAccount, "");
			SetAPPaymentInfo(charge2, ReceiptTypes.Cheque, AUDBankAccount, "");
			SetAPPaymentInfo(charge6, ReceiptTypes.Cash, USDBankAccount, "CC");
			SetAPPaymentInfo(charge7, ReceiptTypes.Cheque, AUDBankAccount, "");

			charge1.JR_AK = autoAllocateChequeBook.PK;
			charge2.JR_AK = autoAllocateChequeBook2.PK;
			charge7.JR_AK = autoAllocateChequeBook.PK;

			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("Payment Count", 3, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region AUD Payment 1

			PaymentApprovalBase paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CHQ", autoAllocateChequeBook.AK_Code, job.JH_JobNum);
			APPayment aUDPayment1 = (APPayment)paymentApproval.NewPayment;
			((IChequeNumberAutoAllocation)aUDPayment1).AssignChequeNumber("2");

			paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CHQ", autoAllocateChequeBook2.AK_Code, job.JH_JobNum);
			APPayment aUDPayment2 = (APPayment)paymentApproval.NewPayment;
			((IChequeNumberAutoAllocation)aUDPayment2).AssignChequeNumber("3");

			paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor2, USDBankAccount, "CSH", "CC", job.JH_JobNum);
			APPayment uSDPayment = (APPayment)paymentApproval.NewPayment;

			Factory.Save();
			AssertTransactionHeaderValues(aUDPayment1, "AP", "PAY", null, "AP Payment Z00001002", Now, ZDateTime.Empty,
					310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "2", "CHQ",
					AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment1);

			charge1.Reload();
			charge7.Reload();
			AssertEquals("ChequeNumber should have been set on Charge1", "2", charge1.JR_ChequeNo);
			AssertEquals("ChequeNumber should have been set on Charge7", "2", charge7.JR_ChequeNo);

			#endregion

			#region AUD Payment 2

			AssertTransactionHeaderValues(aUDPayment2, "AP", "PAY", null, "AP Payment Z00001002", Now, ZDateTime.Empty,
					220.00M, 0M, 0M, 220M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "3", "CHQ",
					AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment2);

			charge2.Reload();
			AssertEquals("ChequeNumber should have been set on Charge2", "3", charge2.JR_ChequeNo);

			#endregion

			#region USD Payment

			AssertTransactionHeaderValues(uSDPayment, "AP", "PAY", null, "AP Payment Z00001002", Now, ZDateTime.Empty,
					314.28M, 0M, 0M, 220M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CSH",
					USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment);

			AssertEquals("ChequeNumber should remain same on Charge6", "CC", charge6.JR_ChequeNo);

			#endregion
		}

		#endregion

		#region TEST: Create One Payment Against One Mixed Currency AP Invoice

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateOnePaymentAgainstOneMixedCurrencyAPInvoice()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job, USD, .7M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", USD, 200M, Creditor1, USD, 200M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge2, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("Payment Count", 1, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);

			PaymentApprovalBase paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);
			APPayment payment = (APPayment)paymentApproval.NewPayment;
			AssertTransactionHeaderValues(payment, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
				424.28M, 0M, 0M, 424.28M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(payment);
		}

		#endregion

		public void TestCreatePaymentOnlyIfAutoPostPaymentsOnceFullyApprovedWhenPostingCostsRegistrySet()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			PaymentAuthorisationSettingsCollection collection = new PaymentAuthorisationSettingsCollection();
			PaymentAuthorisationSettings upToPaymentAuthorisationSettings = collection.AddNew();
			upToPaymentAuthorisationSettings.Range = PaymentAuthorisationSettings.RangeCodes.UpTo;
			upToPaymentAuthorisationSettings.Amount = 1000M;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = PaymentAuthorisationSettings.AuthorisationRequirementCodes.NoApprovalRequired;
			PaymentAuthorisationSettings abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = PaymentAuthorisationSettings.RangeCodes.Above;
			abovePaymentAuthorisationSettings.Amount = 1000M;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = PaymentAuthorisationSettings.AuthorisationRequirementCodes.NoApprovalRequired;
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			Charge charge1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPPaymentInfo(charge1, ReceiptTypes.Cash, AUDBankAccount, "CASH");

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			Charge charge2 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 200M, Creditor1, AUD, 250M, LocalClient);
			SetAPInvoiceInfo(charge2, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPPaymentInfo(charge2, ReceiptTypes.Cash, AUDBankAccount, "CASH");

			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutoPostPaymentsOnceFullyApprovedWhenPostingCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var postManager = new InvoicingPostManager(job1);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();
			AssertEquals("Payment Count when the registry is set to false.", 0, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);

			AccountingConfigurationRegistry.Instance.AutoPostPaymentsOnceFullyApprovedWhenPostingCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			postManager = new InvoicingPostManager(job2);
			transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();
			AssertEquals("Payment Count when the registry is set to true.", 1, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
		}

		#region Implementation

		AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		#endregion
	}
}