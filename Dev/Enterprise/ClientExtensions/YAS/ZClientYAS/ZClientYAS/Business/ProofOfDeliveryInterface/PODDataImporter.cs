using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface
{
	public class PODDataImporter : DataImporter
	{
		public PODDataImporter() : base() { }

		public PODDataImporter(BusinessObjectFactory factory) : base(factory) { }

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string fileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			additionalTransactionActions = System.Array.Empty<ITransactionParticipant>();
			buffer = new NotificationBuffer(notifications);

			PODImportConverter dataConverter = new PODImportConverter(buffer, FactoryProvider.Current);
			dataConverter.ImportFlatFile(dataReader);
			FactoryProvider.SaveCurrentAndUpdateRecordCounts();

			return !buffer.HasErrors;
		}

		internal NotificationBuffer buffer;
	}
}
