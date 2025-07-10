using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM464Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM464Provider>
	{
		public IM464Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("3406264B-BEF1-4432-96D4-63782340D5D8", "IM464: Request Declaration Invalidation");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM464Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM464Provider provider) => AISEntryStatusList.Codes.CancellationRequested;

		protected override Type MessageInterpreterType => typeof(IM464MessageInterpreter);
	}
}
