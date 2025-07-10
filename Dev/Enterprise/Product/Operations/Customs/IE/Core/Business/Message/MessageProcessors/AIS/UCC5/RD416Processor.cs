using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using RD416Provider = Enterprise.Customs.IE.Messaging.UCC5.RD416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	class RD416Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, RD416Provider>
	{
		public RD416Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, RD416Provider provider) => LogicalStatusList.Codes.Invalid;

		protected override string MessageFriendlyNameCore => CommonResStrings.RD416MessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(RD416MessageInterpreter);
	}
}
