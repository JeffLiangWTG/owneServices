using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobInvoicingPaymentApprovalMatcherTest : BaseTransactionCreatorTest
	{
		protected override BaseTransactionCreator GetTransactionCreator(Job job, IJobCostingPlugIn consol)
		{
			return new JobInvoicingPaymentApprovalMatcher(job, ZDateTime.Now);
		}

		protected override void PrepareCreator(Job job, TransactionCreatorHashtable transactions)
		{
			APInvoiceCreator invoiceCreator = new APInvoiceCreator(job);
			invoiceCreator.CreateTransactions(transactions);

			APPaymentApprovalCreator paymentCreator = new APPaymentApprovalCreator(job);
			paymentCreator.CreateTransactions(transactions);
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

			AssertEquals("AP Invoice and Payment Count", 7, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);

			APInvoice invoice = transactions.RetrieveAPInvoice(Creditor1, "1");
			APPayment payment = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);

			TransactionMatchLinkGroup group1 = new TransactionMatchLinkGroup(payment.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);

			#region Group 1 Matching

			AssertEquals("Group 1 Match Count", 2, group1.Count);
			TransactionMatchLink invoiceMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(invoice, -110M);
			TransactionMatchLink paymentMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(payment, 110M);

			AssertNotNull("Invoice Match Link Found", invoiceMatchLink);
			AssertNotNull("Payment Match Link Found", paymentMatchLink);
			AssertInvoiceAndPaymentAreInSameGroup(group1, invoiceMatchLink, paymentMatchLink);

			AssertMatchLinkDefaults(invoiceMatchLink);
			AssertMatchLinkDefaults(paymentMatchLink);
			AssertAPInvoiceShowsAsPaid(invoice);

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

			AssertEquals("AP Invoice and Payment Count", 7, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);

			APPayment payment = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);
			APInvoice invoice1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			APInvoice invoice2 = transactions.RetrieveAPInvoice(Creditor1, "3");

			TransactionMatchLinkGroup group1 = new TransactionMatchLinkGroup(payment.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);

			#region Group 1 Matching

			AssertEquals("Group 1 Match Count", 3, group1.Count);
			TransactionMatchLink invoice1MatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(invoice1, -110M);
			TransactionMatchLink invoice2MatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(invoice2, -200M);
			TransactionMatchLink paymentMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(payment, 310M);

			AssertNotNull("Invoice1 Match Link Found", invoice1MatchLink);
			AssertNotNull("Invoice2 Match Link Found", invoice2MatchLink);
			AssertNotNull("Payment Match Link Found", paymentMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice1MatchLink, paymentMatchLink);
			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice2MatchLink, paymentMatchLink);

			AssertMatchLinkDefaults(paymentMatchLink);
			AssertMatchLinkDefaults(invoice1MatchLink);
			AssertMatchLinkDefaults(invoice2MatchLink);

			AssertAPInvoiceShowsAsPaid(invoice1);
			AssertAPInvoiceShowsAsPaid(invoice2);

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
			Factory.Save();

			AssertEquals("AP Payment Count", 8, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);

			APPayment payment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);
			APInvoice invoice1a = transactions.RetrieveAPInvoice(Creditor1, "1");
			APInvoice invoice1b = transactions.RetrieveAPInvoice(Creditor1, "3");

			APPayment payment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job.JH_JobNum);
			APInvoice invoice2a = transactions.RetrieveAPInvoice(Creditor2, "2");

			TransactionMatchLinkGroup group1 = new TransactionMatchLinkGroup(payment1.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group2 = new TransactionMatchLinkGroup(payment2.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);

			#region Group 1 Matching

			AssertEquals("Group 1 Match Count", 3, group1.Count);
			TransactionMatchLink payment1MatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(payment1, 310M);
			TransactionMatchLink invoice1aMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(invoice1a, -110M);
			TransactionMatchLink invoice1bMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(invoice1b, -200M);

			AssertNotNull("Payment1 Match Link Found", payment1MatchLink);
			AssertNotNull("Invoice1a Match Link Found", invoice1aMatchLink);
			AssertNotNull("Invoice1b Match Link Found", invoice1bMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice1aMatchLink, payment1MatchLink);
			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice1bMatchLink, payment1MatchLink);

			AssertMatchLinkDefaults(payment1MatchLink);
			AssertMatchLinkDefaults(invoice1aMatchLink);
			AssertMatchLinkDefaults(invoice1bMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice1a);
			AssertAPInvoiceShowsAsPaid(invoice1b);

			#endregion

			#region Group 2 Matching

			AssertEquals("Group 2 Match Count", 2, group2.Count);
			TransactionMatchLink payment2MatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(payment2, 314.28M);
			TransactionMatchLink invoice2aMatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(invoice2a, -314.28M);

			AssertNotNull("Payment 2 Match Link Found", payment2MatchLink);
			AssertNotNull("Invoice 2a Match Link Found", invoice2aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group2, invoice2aMatchLink, payment2MatchLink);

			AssertMatchLinkDefaults(payment2MatchLink);
			AssertMatchLinkDefaults(invoice2aMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice2a);

			#endregion
		}

		#endregion

		#region TEST: Create One Payment Against One Invoice On Two Charges With Same Payment Info

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateOnePaymentAgainstOneInvoicesOnTwoChargesWithSamePaymentInfo()
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
			SetAPInvoiceInfo(charge7, "1", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge7, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("AP Invoice and Payment Count", 6, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);

			APPayment payment = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);
			APInvoice invoice1 = transactions.RetrieveAPInvoice(Creditor1, "1");

			TransactionMatchLinkGroup group1 = new TransactionMatchLinkGroup(payment.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);

			#region Group 1 Matching

			AssertEquals("Group 1 Match Count", 2, group1.Count);
			TransactionMatchLink paymentMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(payment, 310M);
			TransactionMatchLink invoice1MatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(invoice1, -310M);

			AssertNotNull("Payment Match Link Found", paymentMatchLink);
			AssertNotNull("Invoice1 Match Link Found", invoice1MatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice1MatchLink, paymentMatchLink);

			AssertMatchLinkDefaults(paymentMatchLink);
			AssertMatchLinkDefaults(invoice1MatchLink);

			AssertAPInvoiceShowsAsPaid(invoice1);

			#endregion
		}

		#endregion

		#region TEST: (Auto Allocation )Create Two Payments Against Three Invoices Two With Same Payment Info

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestAutoAllocation_CreateThreePaymentsAgainstFourInvoicesTwoWithSamePaymentInfo()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			AccChequeBook autoAllocateChequeBook = GetAutoPrintChequeBook(AUDBankAccount, Factory, 1, 4, 2, "Chk1");
			AccChequeBook autoAllocateChequeBook2 = GetAutoPrintChequeBook(USDBankAccount, Factory, 1, 4, 4, "Chk2");

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
			Charge charge8 = CreateCharge(job, CC8, "Charge Code 8", USD, 200M, Creditor2, USD, 275M, Agent);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge8, "5", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.Cheque, AUDBankAccount, "");
			SetAPPaymentInfo(charge6, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge7, ReceiptTypes.Cheque, AUDBankAccount, "");
			SetAPPaymentInfo(charge8, ReceiptTypes.Cheque, USDBankAccount, "");

			charge1.JR_AK = autoAllocateChequeBook.PK;
			charge7.JR_AK = autoAllocateChequeBook.PK;
			charge8.JR_AK = autoAllocateChequeBook2.PK;
			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("AP Payment Count", 10, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);

			APPayment payment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CHQ", "Chk1", job.JH_JobNum);
			APInvoice invoice1a = transactions.RetrieveAPInvoice(Creditor1, "1");
			APInvoice invoice1b = transactions.RetrieveAPInvoice(Creditor1, "3");

			APPayment payment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job.JH_JobNum);
			APInvoice invoice2a = transactions.RetrieveAPInvoice(Creditor2, "2");

			APPayment payment3 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CHQ", "Chk2", job.JH_JobNum);
			APInvoice invoice3a = transactions.RetrieveAPInvoice(Creditor2, "5");

			TransactionMatchLinkGroup group1 = new TransactionMatchLinkGroup(payment1.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group2 = new TransactionMatchLinkGroup(payment2.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group3 = new TransactionMatchLinkGroup(payment3.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);

			#region Group 1 Matching

			AssertEquals("Group 1 Match Count", 3, group1.Count);
			TransactionMatchLink payment1MatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(payment1, 310M);
			TransactionMatchLink invoice1aMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(invoice1a, -110M);
			TransactionMatchLink invoice1bMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(invoice1b, -200M);

			AssertNotNull("Payment1 Match Link Found", payment1MatchLink);
			AssertNotNull("Invoice1a Match Link Found", invoice1aMatchLink);
			AssertNotNull("Invoice1b Match Link Found", invoice1bMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice1aMatchLink, payment1MatchLink);
			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice1bMatchLink, payment1MatchLink);

			AssertMatchLinkDefaults(payment1MatchLink);
			AssertMatchLinkDefaults(invoice1aMatchLink);
			AssertMatchLinkDefaults(invoice1bMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice1a);
			AssertAPInvoiceShowsAsPaid(invoice1b);

			#endregion

			#region Group 2 Matching

			AssertEquals("Group 2 Match Count", 2, group2.Count);
			TransactionMatchLink payment2MatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(payment2, 314.28M);
			TransactionMatchLink invoice2aMatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(invoice2a, -314.28M);

			AssertNotNull("Payment 2 Match Link Found", payment2MatchLink);
			AssertNotNull("Invoice 2a Match Link Found", invoice2aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group2, invoice2aMatchLink, payment2MatchLink);

			AssertMatchLinkDefaults(payment2MatchLink);
			AssertMatchLinkDefaults(invoice2aMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice2a);

			#endregion

			#region Group 2 Matching

			AssertEquals("Group 3 Match Count", 2, group3.Count);
			TransactionMatchLink payment3MatchLink = group3.FindMatchLinkByTransactionHeaderAndAmount(payment3, 314.28M);
			TransactionMatchLink invoice3aMatchLink = group3.FindMatchLinkByTransactionHeaderAndAmount(invoice3a, -314.28M);

			AssertNotNull("Payment 2 Match Link Found", payment3MatchLink);
			AssertNotNull("Invoice 2a Match Link Found", invoice3aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group3, invoice3aMatchLink, payment3MatchLink);

			AssertMatchLinkDefaults(payment3MatchLink);
			AssertMatchLinkDefaults(invoice3aMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice3a);

			#endregion

			//The numbers will be actually allocated on Factory.Saving
			((IChequeNumberAutoAllocation)payment1).AssignChequeNumber("2");
			((IChequeNumberAutoAllocation)payment3).AssignChequeNumber("4");
			Factory.Save();
			AssertEquals("Payment should have its Cheque number set", "2", payment1.AH_ChequeOrReference);
			AssertEquals("Payment should have its Cheque number set", "CC", payment2.AH_ChequeOrReference);
			AssertEquals("Payment should have its Cheque number set", "4", payment3.AH_ChequeOrReference);
		}

		#endregion

		public override void TestChargesForSubShipments()
		{
			//This test has been overriden because JobInvoicingPaymentApprovalMatcher does not use base.IsChargeApplicable(charge)
			Assert(true);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestExchangeDifferenceCreation()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			APExchangeDifference[] exchangeDifference = Factory.Load<APExchangeDifference>(new ZQuery());

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", USD, 100M, Creditor1, AUD, 150M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, ReceiptTypes.eNettDirectDebit, USDBankAccount, "CASH");
			Factory.Save();

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			APInvoiceCreator invoiceCreator = new APInvoiceCreator(job);
			invoiceCreator.CreateTransactions(transactions);
			APPaymentApprovalCreator paymentCreator = new APPaymentApprovalCreator(job);
			paymentCreator.CreateTransactions(transactions);
			JobInvoicingPaymentApprovalMatcher approvalMatcher = new JobInvoicingPaymentApprovalMatcher(job, Now);
			approvalMatcher.CreateTransactions(transactions);
			Factory.Save();

			AssertEquals("AP Invoice and Payment Count", 2, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);
		}

		#region Implementation

		AccChequeBook GetAutoPrintChequeBook(AccBankAccount bank, BusinessObjectFactory newFactory, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO, ZString aK_Code)
		{
			BusinessObjectFactory testFactory = newFactory ?? Factory;
			AccBankAccount bankAccount = bank;
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook chequeBook = testFactory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			chequeBook.AK_Code = aK_Code;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			testFactory.Save();
			return chequeBook;
		}

		#endregion
	}
}
