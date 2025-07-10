using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC917C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC917CMessageProcessor : DeclarationMessageProcessor<ICC917CDataProvider>
{
	public CC917CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC917C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC917C };

	protected override Type MessageInterpreterType => typeof(CC917CMessageInterpreter);

	protected internal override ICC917CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc917CType, CC917CDataProvider>();

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC917CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByLrnOrMRNFallbackToInterchange(message.Factory, messageDataProvider, message.Interchange);

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC917CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		ZString jobStatus = StatusCodes.Rejected;

		switch (entryHeader.CH_EntryStatus)
		{
			case StatusCodes.InvalidationRequest:
				jobStatus = StatusCodes.RejectedInvalidation;
				break;
			case StatusCodes.AmendmentRequest:
				jobStatus = StatusCodes.RejectedAmendment;
				break;
		}

		entryHeader.CH_Status = EDIMessageStatusList.Codes.Error;

		return (jobStatus, EDIMessageStatusList.Codes.ProcessedOK, null);
	}

	protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC917CDataProvider messageDataProvider)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader && entryHeader.CH_Status != EDIMessageStatusList.Codes.Sent)
		{
			DiscardMessage(message, Res.GetString("62E0983A-5D66-4F19-AF77-66EBC10DF690", "The message is discarded when the present message status is {0} (Interchange: {1}, Number: {2}, Type {3})", entryHeader.CH_Status, message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
		}
	}
}
