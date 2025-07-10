using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public sealed class RD416Processor : AISMessageProcessor<AISInboundEDIMessage, RD416Provider>
	{
		public RD416Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.RD416MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, RD416Provider provider) => LogicalStatusList.Codes.Invalid;

		protected override Type MessageInterpreterType => typeof(RD416MessageInterpreter);
	}
}
