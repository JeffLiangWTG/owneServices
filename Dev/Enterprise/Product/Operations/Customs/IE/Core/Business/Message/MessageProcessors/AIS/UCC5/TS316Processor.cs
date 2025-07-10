using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS316Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, Messaging.UCC5.TS316Provider>
	{
		public TS316Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.TS316MessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS316Provider provider) => LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS316Provider provider) => AISEntryStatusList.Codes.Rejected;

		protected override Type MessageInterpreterType => typeof(TS316MessageInterpreter);

		protected override string GetEmailSubject(AISUCC5InboundEDIMessage message) => AISInterchangeTypeList.Descriptions.TS316;
	}
}
