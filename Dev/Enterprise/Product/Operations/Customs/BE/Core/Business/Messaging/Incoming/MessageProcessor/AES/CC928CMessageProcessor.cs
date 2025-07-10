using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC928C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC928CMessageProcessor : DeclarationMessageProcessor<ICC928CDataProvider>
{
	public CC928CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC928C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC928C };

	protected override Type MessageInterpreterType => typeof(CC928CMessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC928CDataProvider messageDataProvider) => MessageHelper.LocateHeaderByEdiInterchange(message.Interchange);

	protected internal override ICC928CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc928CType, CC928CDataProvider>();

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC928CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		var cidEntryNum = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, entryHeader.CountryCode);
		cidEntryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		cidEntryNum.CE_EntryNum = messageDataProvider.CorrelationId;
		cidEntryNum.CE_EntryLineReference = messageDataProvider.CorrelationId;

		entryHeader.CH_Status = StatusCodes.DeclarationAcknowledged;
		if (entryHeader.CH_EntryStatus.IsEmpty && !(entryHeader.EntryInstruction == null))
		{
			entryHeader.CH_EntryStatus = entryHeader.EntryInstruction.CEI_SubStyle.Equals(EntrySubStyleList.Codes.PreliminaryStandardDeclarationUnderCodeA) ? StatusCodes.PreLodged : EDIMessageStatusList.Codes.Acknowledged;
		}

		return (entryHeader.CH_EntryStatus, EDIMessageStatusList.Codes.ProcessedOK, ZString.Empty);
	}
}
