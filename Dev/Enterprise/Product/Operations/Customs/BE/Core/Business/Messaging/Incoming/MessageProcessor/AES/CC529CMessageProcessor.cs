using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC529C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC529CMessageProcessor : DeclarationMessageProcessor<ICC529CDataProvider>
{
	public CC529CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC529C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC529C };

	protected override Type MessageInterpreterType => typeof(CC529CMessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC529CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByLRNFallbackToMRN(message.Factory, messageDataProvider);

	protected internal override ICC529CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc529CType, CC529CDataProvider>();

	protected override bool CheckMessageSequenceIsValidCore(BEMessage message)
	{
		var validStatuses = new List<string> { StatusCodes.MRNAllocated, StatusCodes.Control };
		var entryStatus = ((CusEntryHeader)message.EM_LinkedObject).CH_EntryStatus;
		return validStatuses.Contains(entryStatus);
	}

	protected override string MessageSequenceInvalidMessage => Res.GetString("91484BBB-CD6B-41F6-BF96-476C56AC96D7", "Entry Status was not {0} or {1}", StatusCodes.MRNAllocated, StatusCodes.Control);

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC529CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
		entryHeader.CH_EntryReleaseDate = messageDataProvider.ReleaseDate;

		return (StatusCodes.ReleasedForExport, EDIMessageStatusList.Codes.ProcessedOK, null);
	}
}
