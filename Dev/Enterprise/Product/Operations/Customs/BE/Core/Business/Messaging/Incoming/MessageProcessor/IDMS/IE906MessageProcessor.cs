using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.IDMS.NonIDMSType.IE906;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class IE906MessageProcessor : BaseMessageProcessor<IIE906DataProvider>
{
	public IE906MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}
	protected override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => DeclarationMessageProcessorHelper.GetBranchPkFromJobBO(linkedObject);

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.IE906;

	protected override Type MessageInterpreterType => typeof(IE906MessageInterpreter);

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.IE906 };

	protected override BusinessObject FindParentOfMessage(BEMessage message, IIE906DataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByLrnOrMRNFallbackToInterchange(message.Factory, messageDataProvider, message.Interchange);

	protected override void ProcessMessageCore(BEMessage message, IIE906DataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
		var entryStatus = entryHeader.CH_EntryStatus;
		if (entryStatus.IsEmpty)
		{
			entryHeader.CH_EntryStatus = StatusCodes.Rejected;
		}
		message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
	}

	protected internal override IIE906DataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Ie906, IE906DataProvider>();
}
