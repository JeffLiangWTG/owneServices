using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business;

public class TR060CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR060CProvider>
{
	public TR060CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
	{
	}

	public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR060CProvider provider) => LogicalStatusList.Codes.Accepted;

	public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR060CProvider provider) => NCTS5ArrivalCustomsStatusList.Codes.UnderControlAtDestination;

	protected override string MessageFriendlyNameCore => Res.GetString("EEE191DF-0919-4CE7-81C8-4633CDA73D1E", "TR060C: CONTROL DECISION NOTIFICATION AT DESTINATION");

	protected override Type MessageInterpreterType => typeof(TR060CMessageInterpreter);
}
