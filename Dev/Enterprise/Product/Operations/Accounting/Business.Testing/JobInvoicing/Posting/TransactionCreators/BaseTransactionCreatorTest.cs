using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public abstract class BaseTransactionCreatorTest : TransactionCreatorBaseTest
	{
		protected abstract BaseTransactionCreator GetTransactionCreator(Job job, IJobCostingPlugIn consol = null);

		protected virtual void PrepareCreator(Job job, TransactionCreatorHashtable transactions)
		{
		}

		public void TestCreateTransactions_IfJobWithhold()
		{
			var shipment = TestObjectCreator.CreateShipment("S001971");
			shipment.ConsignorPK = TestObjectCreator.Agent.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_GE = TestObjectCreator.FESDepartment.PK;
			job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			job.JH_Status = JobHeaderStatus.WorkOnHold.Code;

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			charge1.JR_APInvoiceNum = "TEST2222";
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			charge1.JR_ChequeNo = "111";
			charge1.JR_GE = TestObjectCreator.FESDepartment.PK;

			Factory.Save();

			Assert("Precondition: Job should contain no errors", !job.HasErrors);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			AssertEquals("Transaction list empty", 0, transactions.Count);

			BaseTransactionCreator transactionCreator = GetTransactionCreator(job);
			PrepareCreator(job, transactions);
			transactionCreator.CreateTransactions(transactions);
			AssertEquals("Transaction list empty", 0, transactions.Count);

			job.JH_Status = JobHeaderStatus.Working.Code;

			PrepareCreator(job, transactions);
			transactionCreator.CreateTransactions(transactions);
			Assert("Transaction list not empty", transactions.Count > 0);
		}

		public virtual void TestChargesForSubShipments()
		{
			Job job1 = CreateJob("S00001000", LocalClient, 5M, Agent, 10M);
			Job job2 = CreateJob("S00001001", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job2, USD, .67M);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			Factory.Save();

			Charge charge1 = CreateCharge(job1, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job2, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);
			charge1.JR_E6 = cost.PK;
			charge2.JR_E6 = cost.PK;
			job1.Charges.Add(charge2);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			charge1.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			charge1.JR_AB = TestObjectCreator.USDBankAccount.PK;
			charge1.JR_ChequeNo = "789";
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			charge2.JR_AB = TestObjectCreator.USDBankAccount.PK;
			charge2.JR_ChequeNo = "789";

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			BaseTransactionCreator transactionCreator = GetTransactionCreator(job1, consol);
			AssertEquals("Number of charges", 1, transactionCreator.Charges.Count);

			transactionCreator = GetTransactionCreator(job2, consol);
			AssertEquals("Number of charges", 1, transactionCreator.Charges.Count);
		}
	}
}