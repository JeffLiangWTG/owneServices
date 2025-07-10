using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class TransactionAllocationAndPosterCreatorTest : TestCaseWithFactory
	{
		public void TestCreateTransactionAllocationAndPoster()
		{
			AssertNotNull("", TransactionAllocationAndPosterCreator.CreateTransactionAllocationAndPoster(null));
			AssertNotNull("", TransactionAllocationAndPosterCreator.CreateTransactionAllocationAndPoster(new DummyIWorkflowProvider()));
			AssertNotNull("", TransactionAllocationAndPosterCreator.CreateTransactionAllocationAndPoster(Factory.New<ForwardingShipment>()));
			AssertNotNull("", TransactionAllocationAndPosterCreator.CreateTransactionAllocationAndPoster(Factory.New<TransactionPendingAllocation>()));
		}

		#region Implementation

		TransactionAllocationAndPosterCreator TransactionAllocationAndPosterCreator
		{
			get { return new TransactionAllocationAndPosterCreator(); }
		}

		#endregion
	}
}
