using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC599C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC599CMessageProcessor : DeclarationMessageProcessor<ICC599CDataProvider>
{
	public CC599CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC599C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC599C };

	protected override Type MessageInterpreterType => typeof(CC599CMessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC599CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByLRNFallbackToMRN(message.Factory, messageDataProvider);

	protected internal override ICC599CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc599CType, CC599CDataProvider>();

	protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC599CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		if (entryHeader.CH_EntryStatus != StatusCodes.ReleasedForExport)
		{
			DiscardMessage(message, Res.GetString("C29DC595-4561-4AA7-97FE-97AD448361EC", "The message is discarded because the present entry status is not REL. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
		}
	}

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC599CDataProvider messageDataProvider) => (StatusCodes.GoodsExitedEU, EDIMessageStatusList.Codes.ProcessedOK, null);
}
