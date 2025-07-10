using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS309Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, Messaging.UCC5.TS309Provider>
	{
		public TS309Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.TS309MessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS309Provider provider) => provider.InvalidationDecision ? LogicalStatusList.Codes.Accepted : LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS309Provider provider) => provider.InvalidationDecision ? AISEntryStatusList.Codes.Invalid : null;

		protected override Type MessageInterpreterType => typeof(TS309MessageInterpreter);
	}
}
