using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS305Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, Messaging.UCC5.TS305Provider>
	{
		public TS305Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.TS305MessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS305Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(TS305MessageInterpreter);
	}
}
