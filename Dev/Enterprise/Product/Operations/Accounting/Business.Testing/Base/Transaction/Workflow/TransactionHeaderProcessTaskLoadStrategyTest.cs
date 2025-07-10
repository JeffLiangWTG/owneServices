using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	class TransactionHeaderProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestTypeDeciderWorksForARAPInvoice()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001001", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m);
			var arTask = MasterFilesTestHelper.CreateTask(arInvoice);
			var apTask = MasterFilesTestHelper.CreateTask(apInvoice);
			arTask.P9_Description = "AR Invoice";
			apTask.P9_Description = "AP Invoice";
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.Load<ARInvoice>(arInvoice.PK);
			newFactory.Load<ARInvoice>(apInvoice.PK);

			var typeDecider = ProcessTaskTypeDecider.GetInstance();
			var arResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new ARInvoicingWorkflowDescriptor()));
			var apResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new APInvoicingWorkflowDescriptor()));

			AssertEquals(1, arResults.Length);
			AssertEquals(1, apResults.Length);
			AssertEquals("AR Invoice", arResults[0].P9_Description);
			AssertEquals("AP Invoice", apResults[0].P9_Description);
		}

		public void TestTypeDeciderWorksForARAPPayment()
		{
			var apPayment = TestObjectCreator.CreateAPPayment(1m, 100m, ZDateTime.Now, ZDateTime.Now.AddDays(10), TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
			var arPayment = TestObjectCreator.CreateARPayment(1m, 200m, ZDateTime.Now, ZDateTime.Now.AddDays(20), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AUDBankAccount2.PK);
			var apTask = MasterFilesTestHelper.CreateTask(apPayment);
			var arTask = MasterFilesTestHelper.CreateTask(arPayment);
			apTask.P9_Description = "AP Payment";
			arTask.P9_Description = "AR Payment";
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.Load<APPayment>(apPayment.PK);
			newFactory.Load<ARPayment>(arPayment.PK);

			var typeDecider = ProcessTaskTypeDecider.GetInstance();
			var apResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new APPaymentWorkflowDescriptor()));
			var arResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new ARPaymentWorkflowDescriptor()));

			AssertEquals(1, apResults.Length);
			AssertEquals(1, arResults.Length);
			AssertEquals("AP Payment", apResults[0].P9_Description);
			AssertEquals("AR Payment", arResults[0].P9_Description);
		}

		public void TestTypeDeciderWorksForARAPReceipt()
		{
			var arReceipt = TestObjectCreator.CreateARReceipt(1, 100, ZDateTime.Now, ZDateTime.Now.AddDays(1), TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
			var apReceipt = TestObjectCreator.CreateAPReceipt(1, 200, ZDateTime.Now, ZDateTime.Now.AddDays(2), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AUDBankAccount.PK);
			var arTask = MasterFilesTestHelper.CreateTask(arReceipt);
			var apTask = MasterFilesTestHelper.CreateTask(apReceipt);
			apTask.P9_Description = "AP Receipt";
			arTask.P9_Description = "AR Receipt";
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.Load<ARReceipt>(arReceipt.PK);
			newFactory.Load<APReceipt>(apReceipt.PK);

			var typeDecider = ProcessTaskTypeDecider.GetInstance();
			var apResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new APReceiptWorkflowDescriptor()));
			var arResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new ARReceiptWorkflowDescriptor()));

			AssertEquals(1, apResults.Length);
			AssertEquals(1, arResults.Length);
			AssertEquals("AP Receipt", apResults[0].P9_Description);
			AssertEquals("AR Receipt", arResults[0].P9_Description);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
