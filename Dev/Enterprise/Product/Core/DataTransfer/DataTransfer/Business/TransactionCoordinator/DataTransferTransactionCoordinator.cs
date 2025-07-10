using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.DataTransfer.Business
{
	public class DataTransferTransactionCoordinator : TransactionCoordinator
	{
		public DataTransferTransactionCoordinator(ITransactionParticipant[] participants) : base(participants) { }

		protected override bool AllowMultipleParticipantsInTransaction
		{
			get { return true; }
		}

		public static void SaveFactory(BusinessObjectFactory factory)
		{
			RowFactory.SaveTogether(new DataTransferTransactionCoordinator(new[] { factory }));
		}

		public static void SaveTogether(ITransactionParticipant[] participants)
		{
			RowFactory.SaveTogether(new DataTransferTransactionCoordinator(participants));
		}
	}
}
