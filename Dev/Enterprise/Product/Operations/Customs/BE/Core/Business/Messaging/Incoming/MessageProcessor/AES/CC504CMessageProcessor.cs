using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC504C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC504CMessageProcessor : DeclarationMessageProcessor<ICC504CDataProvider>
{
	public CC504CMessageProcessor(LoggingInformation logger) : base(logger) { }
	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC504C;
	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC504C };
	protected override Type MessageInterpreterType => typeof(CC504CMessageInterpreter);
	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC504CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByLRNFallbackToMRN(message.Factory, messageDataProvider);
	protected internal override ICC504CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc504CType, CC504CDataProvider>();
	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC504CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		if (entryHeader.MovementReferenceNumber.IsEmpty)
		{
			return (StatusCodes.Amended, EDIMessageStatusList.Codes.ProcessedOK, Res.GetString("B59053E3-9B95-4A51-8A3E-487DB6ED479B", "The message with interchange {0} processed successfully.", message.Interchange?.EI_InterchangeNum));
		}
		return (StatusCodes.Amended, EDIMessageStatusList.Codes.ProcessedOK, Res.GetString("A013B473-0792-4F25-A7CD-5E68BD3B364A", "The message with interchange {0} processed successfully.", message.Interchange?.EI_InterchangeNum));
	}

	protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC504CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		base.PreProcessMessageWhenBOFoundCore(message, messageDataProvider);
		if (entryHeader.CH_EntryStatus != StatusCodes.AmendmentRequest)
		{
			DiscardMessage(message, Res.GetString("98CA4529-EDDB-4222-B7F1-9DD07EAC089A", "The message with interchange {0} is discarded, because the Status at Customs is not AMR.", message.Interchange?.EI_InterchangeNum));
		}
	}

	protected override ZString StatusForUnableToFindALinkedBusinessObject => EDIMessageStatusList.Codes.Failed;
}
