using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS309Processor : AISMessageProcessor<AISInboundEDIMessage, TS309Provider>
	{
		public TS309Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.TS309MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS309Provider provider) => provider.InvalidationDecision ? LogicalStatusList.Codes.Accepted : LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS309Provider provider) => provider.InvalidationDecision ? AISEntryStatusList.Codes.Cancelled : AISEntryStatusList.Codes.Registered;

		protected override Type MessageInterpreterType => typeof(TS309MessageInterpreter);
	}
}
