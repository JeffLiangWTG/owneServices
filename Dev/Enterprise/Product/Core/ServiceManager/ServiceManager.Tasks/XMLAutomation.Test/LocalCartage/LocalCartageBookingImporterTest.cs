using System.IO;
using CargoWise.Integration;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class LocalCartageBookingImporterTest : AdditionalActionsXmlDataImporterTest<CommonCartage, CommonCartageBookingValueObjectDataAdapter>
	{
		protected override string PathToXmlFile => resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.JobCartageBookingSample.xml");

		protected override AdditionalActionsXmlDataImporter GetImporter()
		{
			return new LocalCartageBookingImporter();
		}

		protected override AdditionalActionsXmlDataImporter GetImporterWithError()
		{
			return new LocalCartageBookingImporterWithError();
		}

		class LocalCartageBookingImporterWithError : LocalCartageBookingImporter
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
