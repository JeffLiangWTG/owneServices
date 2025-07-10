using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ConsolAPPaymentApprovalCreatorTest : TransactionCreatorBaseTest
	{
		[ExpectNoExceptions]
		public void TestPostCostsFromConsolCostWithSubShipments()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var consol = TestObjectCreator.CreateConsol("KRSEL", "AUSYD", "C00001");
			var masterShipment = TestObjectCreator.CreateShipment("S0001", "KRSEL", "AUSYD", consol);
			masterShipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_PackingMode = "BCN";
			var subShipment = TestObjectCreator.CreateShipmentWithCoLoadMaster("S0002", consol, masterShipment);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 1000m, true, "SHP");
			consolCost.E6_InvoiceNum = "1";
			consolCost.E6_InvoiceDate = Now.AddDays(10);
			consolCost.E6_PaymentDate = Now.AddDays(20);
			consolCost.E6_PaymentType = ReceiptTypes.Cash;
			consolCost.E6_AB_BankAccount = AUDBankAccount.PK;
			consolCost.E6_ChequeOrReference = "test";

			Factory.Save();

			masterShipment.Job.LocalChargesPK = subShipment.Job.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var masterShipmentInNewFactory = newFactory.Load<ForwardingShipment>(masterShipment.PK);
			var subShipmentInNewFactory = newFactory.Load<ForwardingShipment>(subShipment.PK);
			newFactory.Load<JobConsolCost>(consolCost.PK);
			var consolCosts = new JobConsolCostCollection(newFactory, consolInNewFactory);

			var jobs = new[] { (Job)masterShipmentInNewFactory.Job, (Job)subShipmentInNewFactory.Job };
			var apInvoiceCreator = new ConsolAPInvoiceCreator(newFactory, jobs, false, consolInNewFactory, consolCosts);
			var creator = new ConsolAPPaymentApprovalCreator(newFactory, jobs, consolInNewFactory, false);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			apInvoiceCreator.CreateTransactions(transactions);
			creator.CreateTransactions(transactions);

			AssertEquals("AP Transactions Count", 1, transactions.APTransactionsCount);
			AssertEquals("Payment Count", 1, transactions.GetAllAPPaymentApprovals().Length);
		}

		#region TEST: Create No Payment Posted When No Invoice Posted

		public void TestCreateNoAPPaymentPostedWhenNoAPInovicePosted()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ZGuid consolID = consol.PK;
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			JobConsolCostCollection consolCosts = new JobConsolCostCollection(Factory, consol);

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);

			AccChequeBook accChequeBook = GetAutoPrintChequeBook(AUDBankAccount, Factory, 0, 99, 1, "Chk1");

			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cheque, AUDBankAccount, "Check", accChequeBook);
			SetAPPaymentInfo(charge1_2, ReceiptTypes.Cheque, AUDBankAccount, "Check", accChequeBook);

			Factory.Save();

			var jobs = new[] { job1 };

			ConsolAPInvoiceCreator aPInvoiceCreator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol, consolCosts);
			ConsolAPPaymentApprovalCreator creator = new ConsolAPPaymentApprovalCreator(Factory, jobs, consol, false);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			aPInvoiceCreator.CreateTransactions(transactions);
			creator.CreateTransactions(transactions);
			Factory.Save();

			AssertEquals("Payment Count", 0, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AP Transactions Count", 0, transactions.APTransactionsCount);

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job2, USD, .7M);
			CreateExchangeRate(job2, GBP, .4M);
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consolID = consol.PK;
			cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			consolCosts = new JobConsolCostCollection(Factory, consol);

			charge1_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			charge1_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));

			accChequeBook = GetAutoPrintChequeBook(AUDBankAccount, Factory, 0, 99, 1, "Chk1");

			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cheque, AUDBankAccount, "Check", accChequeBook);
			SetAPPaymentInfo(charge1_2, ReceiptTypes.Cheque, AUDBankAccount, "Check", accChequeBook);

			Factory.Save();

			jobs = new[] { job2 };
			var postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
			transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("Payment Count", 1, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AP Transactions Count", 1, transactions.APTransactionsCount);
		}

		#endregion

		#region TEST: Create Two Payment Against Three Invoices On Different Jobs

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateOnePaymentAgsinstThreeInvoicesOnTwoDifferentJobs()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ZGuid consolID = consol.PK;
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			JobConsolCostCollection consolCosts = new JobConsolCostCollection(Factory, consol);

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

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
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

			AssertEquals("Payment Count", 2, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Job1 AUD Payment

			APPayment aUDPayment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job1.JH_JobNum);
			AssertTransactionHeaderValues(aUDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
				310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment1);

			#endregion

			#region Job2 AUD Payment

			APPayment aUDPayment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job2.JH_JobNum);
			AssertTransactionHeaderValues(aUDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
				165.00M, 0M, 0M, 165M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment2);

			#endregion
		}

		#endregion

		#region TEST: Create Four Payments Against Four Invoices On Different Jobs

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateTwoPaymentsAgainstFiveInvoicesOnDifferentJobs()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ZGuid consolID = consol.PK;
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			JobConsolCostCollection consolCosts = new JobConsolCostCollection(Factory, consol);

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

			AssertEquals("Payment Count", 4, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Job1 AUD Payment

			PaymentApprovalBase paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CSH", "CASH", job1.JH_JobNum);
			APPayment aUDPayment1 = (APPayment)paymentApproval.NewPayment;
			AssertTransactionHeaderValues(aUDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
				310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment1);

			#endregion

			#region Job2 AUD Payment

			paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CSH", "CASH", job2.JH_JobNum);
			APPayment aUDPayment2 = (APPayment)paymentApproval.NewPayment;
			AssertTransactionHeaderValues(aUDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
				165.00M, 0M, 0M, 165M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment2);

			#endregion

			#region Job1 USD Payment

			paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor2, USDBankAccount, "CCD", "CC", job1.JH_JobNum);
			APPayment uSDPayment1 = (APPayment)paymentApproval.NewPayment;
			AssertTransactionHeaderValues(uSDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
				314.28M, 0M, 0M, 220M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
				USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment1);

			#endregion

			#region Job2 USD Payment

			paymentApproval = transactions.RetrieveAPPaymentApproval(Creditor2, USDBankAccount, "CCD", "CC", job2.JH_JobNum);
			APPayment uSDPayment2 = (APPayment)paymentApproval.NewPayment;
			AssertTransactionHeaderValues(uSDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
				392.85M, 0M, 0M, 275M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
				USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment2);

			#endregion
		}

		#endregion

		#region TEST: Create Same Payment for Apportioned Charge

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateSamePaymentForApportionedCharge()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC8.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "4";
			cost.E6_InvoiceDate = Now.AddDays(15);
			cost.E6_PaymentDate = Now.AddDays(25);
			ZGuid costSplitGroup = cost.PK;
			JobConsolCostCollection consolCosts = new JobConsolCostCollection(Factory, consol);
			Factory.Save();

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Charge Code 8", AUD, 150M, Creditor3, AUD, 200M, LocalClient);
			charge1_8.JR_E6 = costSplitGroup;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "4", Now.AddDays(15), Now.AddDays(25));

			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge1_6, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge1_7, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge1_8, ReceiptTypes.Cash, AUDBankAccount, "CASH2");

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CreateExchangeRate(job2, USD, .7M);
			CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, Agent);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", USD, 250M, Creditor2, USD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Charge Code 3", AUD, 200M, Creditor3, AUD, 250M, Agent);
			Charge charge2_4 = CreateCharge(job2, CC8, "Charge Code 8", AUD, 150M, Creditor3, AUD, 200M, LocalClient);
			charge2_4.JR_E6 = costSplitGroup;

			SetAPInvoiceInfo(charge2_1, "4", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_2, "5", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_3, "6", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "4", Now.AddDays(15), Now.AddDays(25));

			SetAPPaymentInfo(charge2_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge2_2, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge2_4, ReceiptTypes.Cash, AUDBankAccount, "CASH2");
			Factory.Save();

			// Fix Consol Cost data to pass Critical Validation
			cost.E6_OSCostAmount = 300m;
			cost.E6_AH_APInvoice = charge2_4.APLine.AL_AH;

			var jobs = new[] { job1, job2 };
			var postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();

			AssertEquals("Payment Count", 5, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Job1 AUD Payment

			APPayment aUDPayment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job1.JH_JobNum);
			AssertTransactionHeaderValues(aUDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
				310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment1);

			#endregion

			#region Job2 AUD Payment

			APPayment aUDPayment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job2.JH_JobNum);
			AssertTransactionHeaderValues(aUDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
				165.00M, 0M, 0M, 165M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment2);

			#endregion

			#region Job1 USD Payment

			APPayment uSDPayment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job1.JH_JobNum);
			AssertTransactionHeaderValues(uSDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
				314.28M, 0M, 0M, 220M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
				USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment1);

			#endregion

			#region Job2 USD Payment

			APPayment uSDPayment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job2.JH_JobNum);
			AssertTransactionHeaderValues(uSDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
				392.85M, 0M, 0M, 275M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
				USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment2);

			#endregion

			#region Apportionment Payment

			APPayment apportionmentPayment = transactions.RetrieveAPPayment_ForTestOnly(Creditor3, AUDBankAccount, "CSH", "CASH2", ZString.Empty);
			AssertTransactionHeaderValues(apportionmentPayment, "AP", "PAY", null, "AP Payment " + consol[JobConsolSchema.Constants.JK_UniqueConsignRef],
			Now, ZDateTime.Empty, 330M, 0M, 0M, 330M, AUD, 1, Now, ZBool.False, Creditor3, null, ZString.Empty, 0, "CASH2", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(apportionmentPayment);

			#endregion
		}

		#endregion

		#region TEST: Create One Payment Against One Mixed Currency AP Invoice

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateOnePaymentAgainstOneMixedCurrencyAPInvoice()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = TestObjectCreator.CreateShipment("Z00001000", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_GE = TestObjectCreator.FESDepartment.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "USD";
			rate.JF_BaseRate = 6.10m;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = CC1.PK;
			cost1.E6_OH_Creditor = Creditor1.PK;
			cost1.E6_OSCostAmount = 100M;
			cost1.E6_InvoiceNum = "1";
			cost1.E6_InvoiceDate = Now.AddDays(15);
			cost1.E6_PaymentDate = Now.AddDays(25);
			cost1.E6_PaymentType = ReceiptTypes.Cash;
			cost1.E6_AB_BankAccount = AUDBankAccount.PK;
			cost1.E6_ChequeOrReference = "CASH";
			cost1.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;

			JobConsolCost cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = CC2.PK;
			cost2.E6_OH_Creditor = Creditor1.PK;
			cost2.E6_RX_NKCurrency = USD.RX_Code;
			cost2.E6_ExchangeRate = .7M;
			cost2.E6_OSCostAmount = 200M;
			cost2.E6_InvoiceNum = "1";
			cost2.E6_InvoiceDate = Now.AddDays(15);
			cost2.E6_PaymentDate = Now.AddDays(25);
			cost2.E6_PaymentType = ReceiptTypes.Cash;
			cost2.E6_AB_BankAccount = AUDBankAccount.PK;
			cost2.E6_ChequeOrReference = "CASH";
			cost2.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;

			cost1.RunPreSaveValidation();
			cost2.RunPreSaveValidation();

			AssertNoErrors("Consol cost 1", cost1);
			AssertNoErrors("Consol cost 2", cost2);
			AssertHasWarning(cost1.E6_RX_NKCurrencyInfo, @"This cost will be posted on a local currency Payables Invoice.
A mix of Cost Currencies have recorded for this Creditor and Invoice Number. Because of this, they will be posted as a local currency payables transaction.");
			AssertHasWarning(cost2.E6_RX_NKCurrencyInfo, @"This cost will be posted on a local currency Payables Invoice.
A mix of Cost Currencies have recorded for this Creditor and Invoice Number. Because of this, they will be posted as a local currency payables transaction.");

			cost2.E6_AB_BankAccount = TestObjectCreator.GBPBankAccount.PK;
			AssertHasError(cost2.E6_AB_BankAccountInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the bank account does not match.");
			AssertHasError(cost2.E6_AB_BankAccountInfo, "Bank Account currency is incorrect. Choose AUD currency Bank Account.");

			cost1.E6_AB_BankAccount = TestObjectCreator.GBPBankAccount.PK;
			AssertHasError(cost2.E6_AB_BankAccountInfo, "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the bank account does not match.");

			cost1.E6_AB_BankAccount = AUDBankAccount.PK;
			cost2.E6_AB_BankAccount = AUDBankAccount.PK;

			cost1.RunPreSaveValidation();
			cost2.RunPreSaveValidation();
			AssertNoErrors("Consol cost 1", cost1);
			AssertNoErrors("Consol cost 2", cost2);

			job.RunPreSaveValidation();
			AssertNoErrors("Job", job);

			Factory.Save();

			var jobs = new[] { job };
			var postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			// Set Invoice on Consol Costs to pass Critical Validation
			var apInvoices = transactions.GetAllAPTransactions();
			AssertEquals("One AP Invoice", 1, apInvoices.Length);
			cost1.E6_AH_APInvoice = apInvoices[0].PK;
			cost2.E6_AH_APInvoice = apInvoices[0].PK;
			Factory.Save();

			AssertEquals("Payment Count", 1, transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Length);

			APPayment payment = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", ZString.Empty);
			AssertTransactionHeaderValues(payment, "AP", "PAY", null, "AP Payment " + consol[JobConsolSchema.Constants.JK_UniqueConsignRef],
			Now, ZDateTime.Empty, 424.28M, 0M, 0M, 424.28M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(payment);
		}

		#endregion

		#region Implementation

		AccChequeBook GetAutoPrintChequeBook(AccBankAccount bank, BusinessObjectFactory newFactory, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO, ZString aK_Code)
		{
			BusinessObjectFactory testFactory = newFactory ?? Factory;
			AccBankAccount bankAccount = bank;
			bankAccount.AB_ChequeNumDigits = 1;
			StmTemplate chequeTemplate = testFactory.NewWithValidTestData<StmTemplate>();
			bankAccount.AB_SO_ChequeTemplate = chequeTemplate.PK;
			BusinessObject printQueue = testFactory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
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
