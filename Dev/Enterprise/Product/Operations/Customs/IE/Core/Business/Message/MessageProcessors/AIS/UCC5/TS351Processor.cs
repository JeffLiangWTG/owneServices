using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS351Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, Messaging.UCC5.TS351Provider>
	{
		public TS351Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.TS351MessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS351Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS351Provider provider) => AISEntryStatusList.Codes.NotReleased;

		protected override Type MessageInterpreterType => typeof(TS351MessageInterpreter);
	}
}
