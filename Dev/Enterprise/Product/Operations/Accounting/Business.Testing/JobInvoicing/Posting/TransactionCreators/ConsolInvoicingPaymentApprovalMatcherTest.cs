using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ConsolInvoicingPaymentApprovalMatcherTest : TransactionCreatorBaseTest
	{
		#region TEST: Create One Payment Against Three Invoices On Different Jobs

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateOnePaymentAgainstThreeInvoicesOnDifferentJobs()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job1 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);
			var consol = (ForwardingConsol)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
			var cost = Factory.New<JobConsolCost>();
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge1_7, ReceiptTypes.Cash, AUDBankAccount, "CASH");

			Job job2 = CreateJob("Z00001002", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job2, USD, .7M);
			CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, Agent);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 250M, Creditor2, AUD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Charge Code 3", USD, 200M, Creditor3, USD, 250M, Agent);

			SetAPInvoiceInfo(charge2_1, "4", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_2, "5", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_3, "6", Now.AddDays(15), Now.AddDays(25));

			SetAPPaymentInfo(charge2_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");

			Factory.Save();

			var jobs = new[] { job1, job2 };
			var postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("AP Invoice and Payment Count", 11, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			APPayment payment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job1.JH_JobNum);
			APInvoice invoice1a = transactions.RetrieveAPInvoice(Creditor1, "1");
			APInvoice invoice1b = transactions.RetrieveAPInvoice(Creditor1, "3");

			APPayment payment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job2.JH_JobNum);
			APInvoice invoice2a = transactions.RetrieveAPInvoice(Creditor1, "4");

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
			TransactionMatchLink payment2MatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(payment2, 165M);
			TransactionMatchLink invoice2aMatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(invoice2a, -165M);

			AssertNotNull("Payment2 Match Link Found", payment2MatchLink);
			AssertNotNull("Invoice2a Match Link Found", invoice2aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group2, invoice2aMatchLink, payment2MatchLink);

			AssertMatchLinkDefaults(payment2MatchLink);
			AssertMatchLinkDefaults(invoice2aMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice2a);

			#endregion
		}

		#endregion

		#region TEST: Create Two Payments Against Five Invoices On Different Jobs

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateTwoPaymentsAgainstFiveInvoicesOnDifferentJobs()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job1 = CreateJob("Z00001002", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);
			var consol = (ForwardingConsol)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
			var cost = Factory.New<JobConsolCost>();
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge1_6, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge1_7, ReceiptTypes.Cash, AUDBankAccount, "CASH");

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CreateExchangeRate(job2, USD, .7M);
			CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, Agent);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", USD, 250M, Creditor2, USD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Charge Code 3", AUD, 200M, Creditor3, AUD, 250M, Agent);

			SetAPInvoiceInfo(charge2_1, "4", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_2, "5", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_3, "6", Now.AddDays(15), Now.AddDays(25));

			SetAPPaymentInfo(charge2_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge2_2, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			Factory.Save();

			var jobs = new[] { job1, job2 };
			var postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("AP Payment Count", 13, transactions.APTransactionsCount + transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			APPayment payment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job1.JH_JobNum);
			APInvoice invoice1a = transactions.RetrieveAPInvoice(Creditor1, "1");
			APInvoice invoice1b = transactions.RetrieveAPInvoice(Creditor1, "3");

			APPayment payment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job1.JH_JobNum);
			APInvoice invoice2a = transactions.RetrieveAPInvoice(Creditor2, "2");

			APPayment payment3 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job2.JH_JobNum);
			APInvoice invoice3a = transactions.RetrieveAPInvoice(Creditor1, "4");

			APPayment payment4 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job2.JH_JobNum);
			APInvoice invoice4a = transactions.RetrieveAPInvoice(Creditor2, "5");

			TransactionMatchLinkGroup group1 = new TransactionMatchLinkGroup(payment1.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group2 = new TransactionMatchLinkGroup(payment2.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group3 = new TransactionMatchLinkGroup(payment3.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group4 = new TransactionMatchLinkGroup(payment4.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);

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

			#region Group 3 Matching

			AssertEquals("Group 3 Match Count", 2, group3.Count);
			TransactionMatchLink payment3MatchLink = group3.FindMatchLinkByTransactionHeaderAndAmount(payment3, 165M);
			TransactionMatchLink invoice3aMatchLink = group3.FindMatchLinkByTransactionHeaderAndAmount(invoice3a, -165M);

			AssertNotNull("Payment3 Match Link Found", payment3MatchLink);
			AssertNotNull("Invoice3a Match Link Found", invoice3aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group3, invoice3aMatchLink, payment3MatchLink);

			AssertMatchLinkDefaults(payment3MatchLink);
			AssertMatchLinkDefaults(invoice3aMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice3a);

			#endregion

			#region Group 4 Matching

			AssertEquals("Group 4 Match Count", 2, group4.Count);
			TransactionMatchLink payment4MatchLink = group4.FindMatchLinkByTransactionHeaderAndAmount(payment4, 392.85M);
			TransactionMatchLink invoice4aMatchLink = group4.FindMatchLinkByTransactionHeaderAndAmount(invoice4a, -392.85M);

			AssertNotNull("Payment4 Match Link Found", payment4MatchLink);
			AssertNotNull("Invoice4a Match Link Found", invoice4aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group4, invoice4aMatchLink, payment4MatchLink);

			AssertMatchLinkDefaults(payment4MatchLink);
			AssertMatchLinkDefaults(invoice4aMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice4a);

			#endregion
		}

		#endregion

		#region TEST:(Auto Allocation) Create One Payment Against Three Invoices On Different Jobs

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestAutoAllocation_CreateOnePaymentAgainstThreeInvoicesOnDifferentJobs()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			AccChequeBook autoAllocateChequeBook = GetAutoPrintChequeBook(AUDBankAccount, Factory, 1, 4, 2, "Chk1");
			AccChequeBook autoAllocateChequeBook2 = GetAutoPrintChequeBook(USDBankAccount, Factory, 1, 4, 4, "Chk2");

			Job job1 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);
			var consol = (ForwardingConsol)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
			var cost = Factory.New<JobConsolCost>();
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cheque, AUDBankAccount, "");
			SetAPPaymentInfo(charge1_7, ReceiptTypes.Cheque, AUDBankAccount, "");

			charge1_1.JR_AK = autoAllocateChequeBook.PK;
			charge1_7.JR_AK = autoAllocateChequeBook.PK;

			Job job2 = CreateJob("Z00001002", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job2, USD, .7M);
			CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, Agent);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 250M, Creditor2, AUD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Charge Code 3", USD, 200M, Creditor3, USD, 250M, Agent);

			SetAPInvoiceInfo(charge2_1, "4", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_2, "5", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_3, "6", Now.AddDays(15), Now.AddDays(25));

			SetAPPaymentInfo(charge2_1, ReceiptTypes.Cheque, USDBankAccount, "");
			charge2_1.JR_AK = autoAllocateChequeBook2.PK;
			Factory.Save();

			var jobs = new[] { job1, job2 };
			var postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("AP Invoice and Payment Count", 11, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			APPayment payment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CHQ", "Chk1", job1.JH_JobNum);
			APInvoice invoice1a = transactions.RetrieveAPInvoice(Creditor1, "1");
			APInvoice invoice1b = transactions.RetrieveAPInvoice(Creditor1, "3");

			APPayment payment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, USDBankAccount, "CHQ", "Chk2", job2.JH_JobNum);
			APInvoice invoice2a = transactions.RetrieveAPInvoice(Creditor1, "4");

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
			TransactionMatchLink payment2MatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(payment2, 165M);
			TransactionMatchLink invoice2aMatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(invoice2a, -165M);

			AssertNotNull("Payment2 Match Link Found", payment2MatchLink);
			AssertNotNull("Invoice2a Match Link Found", invoice2aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group2, invoice2aMatchLink, payment2MatchLink);

			AssertMatchLinkDefaults(payment2MatchLink);
			AssertMatchLinkDefaults(invoice2aMatchLink);

			AssertAPInvoiceShowsAsPaid(invoice2a);

			#endregion

			//The numbers will be actually allocated on Factory.Saving
			((IChequeNumberAutoAllocation)payment1).AssignChequeNumber("2");
			((IChequeNumberAutoAllocation)payment2).AssignChequeNumber("4");
			Factory.Save();
			AssertEquals("Payment should have its Cheque number set", "2", payment1.AH_ChequeOrReference);
			AssertEquals("Payment should have its Cheque number set", "4", payment2.AH_ChequeOrReference);
		}

		#endregion

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
