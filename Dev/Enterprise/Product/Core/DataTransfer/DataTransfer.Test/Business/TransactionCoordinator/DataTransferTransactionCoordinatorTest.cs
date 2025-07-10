using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing.DataAccess;
using CargoWise.Integration;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class DataTransferTransactionCoordinatorTest : TransactionCoordinatorTestCase
	{
		public void TestAllowMultipleParticipantsInTransaction()
		{
			Assert("DataTransfer transaction coordinator should allow multiple participants in pre-existing transaction.",
				new DataTransferTransactionCoordinatorForTest(new[] { new BusinessObjectFactory() }).AllowMultipleParticipantsInTransactionExposed);
		}

		class DataTransferTransactionCoordinatorForTest : DataTransferTransactionCoordinator
		{
			public DataTransferTransactionCoordinatorForTest(ITransactionParticipant[] participants) : base(participants) { }

			public bool AllowMultipleParticipantsInTransactionExposed
			{
				get { return AllowMultipleParticipantsInTransaction; }
			}
		}
	}
}
