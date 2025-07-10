using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.PDI400V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Declaration.CodeDescriptionList;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business;

public sealed class IncompleteImportH1ResponseMessageProcessor : ImportH1CommonResponseMessageProcessor<Pdi400V1Sal>
{
	public IncompleteImportH1ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Import Incomplete Pre-Declaration (H1) Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNamePdi400V1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1 };

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Pdi400V1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new IncompleteImportH1MessagePrettyFormatter(response, entryHeader);

	protected override ZString ProcessAcceptedDeclaration(Pdi400V1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		var ackResponseData = response.Cc415R?.Ack;
		if (ackResponseData == null) { return ZString.Empty; }

		entryHeader.ZG_ExportMRN = ackResponseData.ImportOperation?.ExportMrn;
		SetEntryStatus(ackResponseData.OperationRegistered, entryHeader);

		return ZString.Empty;
	}

	void SetEntryStatus(ZString operationRegisteredCode, CusEntryHeader entryHeader)
	{
		if (operationRegisteredCode != ImportH1OperationRegisteredCodeList.Codes.AcceptedPdi) { return; }
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
	}

	const string XsdSchemaNamePdi400V1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.H1.Incoming.PDI400V1Sal.xsd";
}
