using System.IO;
using CargoWise.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class ShipmentDataImporterTest : AdditionalActionsXmlDataImporterTest<ForwardingShipment, BatchForwardingShipmentValueObjectDataAdapter>
	{
		protected override AdditionalActionsXmlDataImporter GetImporter()
		{
			return new ShipmentDataImporter();
		}

		protected override string PathToXmlFile => resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.ShipmentWithOrder.xml");

		protected override AdditionalActionsXmlDataImporter GetImporterWithError()
		{
			return new ShipmentDataImporterWithError();
		}

		class ShipmentDataImporterWithError : ShipmentDataImporter
		{
			protected override bool GetImportDataResult(TextReader dataReader, string attachmentFileName, NotificationBuffer buffer, out ITransactionParticipant[] newAdditionalTransactionActions)
			{
				bool result = base.GetImportDataResult(dataReader, attachmentFileName, buffer, out newAdditionalTransactionActions);
				buffer.Notify(new ErrorNotification(ErrorType.ImportingDataError, "Testing error"));
				return result;
			}
		}
	}
}
