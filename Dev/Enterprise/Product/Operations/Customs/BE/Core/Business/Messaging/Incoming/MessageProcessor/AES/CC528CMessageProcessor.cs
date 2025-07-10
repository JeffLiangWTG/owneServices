using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC528C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC528CMessageProcessor : DeclarationMessageProcessor<ICC528CDataProvider>
{
	public CC528CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC528C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC528C };

	protected override Type MessageInterpreterType => typeof(CC528CMessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC528CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByLRN(message.Factory, messageDataProvider);

	protected internal override ICC528CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc528CType, CC528CDataProvider>();

	protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC528CDataProvider messageDataProvider)
	{
		var validStatuses = new List<string> { StatusCodes.Presented, StatusCodes.Rejected, StatusCodes.ACK, string.Empty };
		var entryStatus = ((CusEntryHeader)message.EM_LinkedObject).CH_EntryStatus;
		if (!validStatuses.Contains(entryStatus))
		{
			DiscardMessage(message, Res.GetString("28627121-9177-4E8E-887F-0A3F550CE37B", "The message is discarded because the Entry Status was not {0}, {1}, {2} or empty. (Interchange Number:{3}, Number:{4}, Type:{5}); message status set to DISCARDED.", StatusCodes.Presented, StatusCodes.Rejected, StatusCodes.ACK, message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
		}
	}

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC528CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		if (!messageDataProvider.MRN.IsNullOrEmpty())
		{
			var entryNumber = Extensions.CreateMovementReferenceNumber(entryHeader, messageDataProvider.MRN);
			entryNumber.CE_IssueDate = messageDataProvider.DeclarationAcceptanceDate;
		}

		entryHeader.CH_Status = StatusCodes.DeclarationAcknowledged;

		return (StatusCodes.MRNAllocated, EDIMessageStatusList.Codes.ProcessedOK, null);
	}
}
