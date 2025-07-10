using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC551C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC551CMessageProcessor : DeclarationMessageProcessor<ICC551CDataProvider>
{
	public CC551CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override ZString StatusForUnableToFindALinkedBusinessObject => EDIMessage.Status.Failed;

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC551C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC551C };

	protected override Type MessageInterpreterType => typeof(CC551CMessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC551CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByMRN(message.Factory, messageDataProvider);

	protected internal override ICC551CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc551CType, CC551CDataProvider>();

	protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC551CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		if (!new ZString[] { StatusCodes.MRNAllocated, StatusCodes.Control, StatusCodes.ACK }.Contains(entryHeader.CH_EntryStatus))
		{
			DiscardMessage(message, Res.GetString("611C6A58-C13F-4617-98ED-41C9E9B86EC3", "The message is discarded because the present status is not MRN, CTL or ACK. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
		}
	}

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC551CDataProvider messageDataProvider) => (StatusCodes.NotReleasedForExport, EDIMessageStatusList.Codes.ProcessedOK, null);
}
