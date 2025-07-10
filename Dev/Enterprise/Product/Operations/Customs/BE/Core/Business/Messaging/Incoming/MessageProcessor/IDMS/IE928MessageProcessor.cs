using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.IDMS.NonIDMSType.IE928;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class IE928MessageProcessor : DeclarationMessageProcessor<IIE928DataProvider>
{
	public IE928MessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.IE928;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.IE928 };

	protected override Type MessageInterpreterType => typeof(IE928MessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, IIE928DataProvider messageDataProvider) => MessageHelper.LocateHeaderByEdiInterchange(message.Interchange);

	protected internal override IIE928DataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Ie928, IE928DataProvider>();

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, IIE928DataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		var cidEntryNum = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, entryHeader.CountryCode);
		cidEntryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		cidEntryNum.CE_EntryNum = messageDataProvider.CorrelationId;
		cidEntryNum.CE_EntryLineReference = messageDataProvider.CorrelationId;

		var jobStatus = (entryHeader.CH_EntryStatus.IsEmpty || entryHeader.CH_EntryStatus == LogicalStatusList.Codes.Error) ? StatusCodes.DeclarationAcknowledged : string.Empty;

		return (jobStatus, EDIMessageStatusList.Codes.ProcessedOK, ZString.Empty);
	}
}
