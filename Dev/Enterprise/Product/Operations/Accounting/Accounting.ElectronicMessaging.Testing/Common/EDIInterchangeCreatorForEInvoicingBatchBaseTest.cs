using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class EDIInterchangeCreatorForEInvoicingBatchBaseTest : TestCaseWithFactory
	{
		public abstract void TestIsBillingSupported();

		public abstract void TestGetEInvoicingServicePoint();

		public abstract void TestGetCommunicationMode();

		public abstract void TestProcessSingleBatch();

		public abstract void TestAllowSendingEInvoicingBatchWithError_RegistryOff();

		public abstract void TestAllowSendingEInvoicingBatchWithError_RegistryOn();

		public abstract void TestProcessMultipleBatches();

		public abstract void TestProcessSingleBatchFailed();

		public abstract void TestProcessPartOfBatchesFailed();

		public abstract void TestProcessPartOfBatchesThrowException();

		public abstract void TestCreateBillingTransaction();

		public abstract void TestSavesWithExceptionHandling();

		public abstract EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company);

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();
		}

		protected EInvoicingTestHelper Helper
		{
			get { return helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator)); }
		}
		EInvoicingTestHelper helper;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}