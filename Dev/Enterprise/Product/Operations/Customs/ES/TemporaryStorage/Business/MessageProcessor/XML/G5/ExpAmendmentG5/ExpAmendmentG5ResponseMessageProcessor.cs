using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.SAL;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public class ExpAmendmentG5ResponseMessageProcessor : G5CommonResponseMessageProcessor<G5ExpAmendV1Sal, ExpAmendmentG5MessagePrettyFormatter>
{
	public ExpAmendmentG5ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"G5 Expedition Amendment Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameG5ExpAmendV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment };

	protected override ExpAmendmentG5MessagePrettyFormatter GetNewMessagePrettyFormatter(G5ExpAmendV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader) => new ExpAmendmentG5MessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(G5ExpAmendV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader)
	{
		var correctResponseData = response.Accepted;
		SetCustomsStatus(temporaryStorageHeader, correctResponseData?.Channel);
		SetEntryNumbers(temporaryStorageHeader, correctResponseData?.Mrn, correctResponseData?.Channel.ToString(), response.EnvelopeG5?.PreparationDate, correctResponseData?.ReleaseCsv, correctResponseData?.TsAtDestination);
		temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();

		return ZString.Empty;
	}

	const string XsdSchemaNameG5ExpAmendV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.G5.Incoming.G5ExpAmendV1Sal.xsd";
}
