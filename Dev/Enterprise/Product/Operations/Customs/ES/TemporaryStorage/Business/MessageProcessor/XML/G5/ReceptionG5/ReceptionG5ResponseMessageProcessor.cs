using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5RecNotifV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.TemporaryStorage.Business
{
	public class ReceptionG5ResponseMessageProcessor : G5CommonResponseMessageProcessor<G5RecNotifV1Sal, ReceptionG5MessagePrettyFormatter>
	{
		public ReceptionG5ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"G5 Reception Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameG5RecNotifV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.G5v1Reception };

		protected override ReceptionG5MessagePrettyFormatter GetNewMessagePrettyFormatter(G5RecNotifV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader) => new ReceptionG5MessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(G5RecNotifV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader)
		{
			var correctResponseData = response.Accepted;
			SetCustomsStatus(temporaryStorageHeader, correctResponseData?.Channel);
			SetEntryNumbers(temporaryStorageHeader, correctResponseData?.Mrn, correctResponseData?.Channel.ToString(), response.EnvelopeG5?.PreparationDate, correctResponseData?.ReleaseCsv, correctResponseData?.TsAtDestination);

			return ZString.Empty;
		}

		const string XsdSchemaNameG5RecNotifV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.G5.Incoming.G5RecNotifV1Sal.xsd";
	}
}
