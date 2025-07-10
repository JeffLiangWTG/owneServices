using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC509C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC509CMessageProcessor : DeclarationMessageProcessor<ICC509CDataProvider>
{
	public CC509CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC509C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC509C };

	protected override Type MessageInterpreterType => typeof(CC509CMessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC509CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByLRNFallbackToMRN(message.Factory, messageDataProvider);

	protected internal override ICC509CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc509CType, CC509CDataProvider>();

	protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC509CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		if (entryHeader.CH_EntryStatus != StatusCodes.InvalidationRequest)
		{
			entryHeader.CH_EntryStatus = StatusCodes.DeclarationCancelled;
			DiscardMessage(message, Res.GetString("5A9C1B01-FC7B-4FCC-A2D6-5FA03FD22DCF", "The message with interchange {0} is discarded, because the Status at Customs is not INR.", message.Interchange?.EI_InterchangeNum));
		}
	}

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC509CDataProvider messageDataProvider)
		=> (StatusCodes.Cancelled, EDIMessageStatusList.Codes.ProcessedOK, Res.GetString("035AF7B4-218B-4C40-83FF-58502E643400", "The message with interchange {0} processed successfully.", message.Interchange?.EI_InterchangeNum));

	protected override ZString StatusForUnableToFindALinkedBusinessObject => EDIMessageStatusList.Codes.Failed;
}
