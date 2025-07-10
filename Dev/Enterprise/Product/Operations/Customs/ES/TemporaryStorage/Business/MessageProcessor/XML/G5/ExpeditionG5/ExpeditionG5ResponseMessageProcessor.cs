using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpNotifV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.TemporaryStorage.Business
{
	public class ExpeditionG5ResponseMessageProcessor : G5CommonResponseMessageProcessor<G5ExpNotifV1Sal, ExpeditionG5MessagePrettyFormatter>
	{
		public ExpeditionG5ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"G5 Expedition Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameG5ExpNotifV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.G5v1Expedition };

		protected override ExpeditionG5MessagePrettyFormatter GetNewMessagePrettyFormatter(G5ExpNotifV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader) => new ExpeditionG5MessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(G5ExpNotifV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader)
		{
			var correctResponseData = response.Accepted;
			SetEntryNumbers(temporaryStorageHeader, correctResponseData?.Mrn, correctResponseData?.Channel.ToString(), response.EnvelopeG5?.PreparationDate, correctResponseData?.ReleaseCsv, correctResponseData?.TsAtDestination);
			SetCustomsStatus(temporaryStorageHeader, correctResponseData?.Channel);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Expedition();

			return ZString.Empty;
		}

		protected override void ProcessRejectedDeclaration(G5ExpNotifV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader)
		{
			base.ProcessRejectedDeclaration(response, message, temporaryStorageHeader);

			if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(temporaryStorageHeader.CountryCode))
			{
				EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, temporaryStorageHeader.TemporaryStorageTransactionInternalReferenceNumber, temporaryStorageHeader.TemporaryStorageTransactionInternalReferenceType);
			}
		}

		const string XsdSchemaNameG5ExpNotifV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.G5.Incoming.G5ExpNotifV1Sal.xsd";
	}
}
