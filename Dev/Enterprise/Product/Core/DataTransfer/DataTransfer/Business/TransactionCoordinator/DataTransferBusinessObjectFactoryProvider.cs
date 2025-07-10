using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.DataTransfer.Business
{
	public class DataTransferBusinessObjectFactoryProvider : BusinessObjectFactoryProvider
	{
		public DataTransferBusinessObjectFactoryProvider() { }

		public DataTransferBusinessObjectFactoryProvider(BusinessObjectFactory initialFactory) : base(initialFactory) { }

		public DataTransferBusinessObjectFactoryProvider(DbConnection connection) : base(connection) { }

		protected override void SaveCurrent()
		{
			SaveCurrentWithAdditionalParticipantsWithoutCreateNew(null);
		}

		public void SaveCurrentWithAdditionalParticipantsAndCreateNew(params ITransactionParticipant[] participants)
		{
			SaveTogether(true, participants);
		}

		public void SaveCurrentWithAdditionalParticipantsWithoutCreateNew(params ITransactionParticipant[] participants)
		{
			SaveTogether(false, participants);
		}

		void SaveTogether(bool createNew, params ITransactionParticipant[] participants)
		{
			if (HasCurrentFactory)
			{
				List<ITransactionParticipant> allParticipants = new List<ITransactionParticipant>();
				allParticipants.Add(Current);
				if (participants != null)
				{
					allParticipants.AddRange(participants);
				}

				UpdateRecordCounts();
				RowFactory.SaveTogether(new DataTransferTransactionCoordinator(allParticipants.ToArray()));
				if (createNew)
				{
					CreateNewWithoutSave();
				}
			}
		}
	}

	public class DataTransferSingleBusinessObjectFactoryProvider : DataTransferBusinessObjectFactoryProvider
	{
		public DataTransferSingleBusinessObjectFactoryProvider(BusinessObjectFactory factory) : base(factory) { }

		protected override BusinessObjectFactory CreateNew(bool reclaimMemory)
		{
			return Current;
		}
	}
}
