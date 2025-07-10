using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM451Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM451Provider>
	{
		public IM451Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4D08673C-6C80-4243-85F8-AF48F5BE423C", "IM451: Release Rejection");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, Messaging.IMessageAttachee messageAttachee, IM451Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, Messaging.IMessageAttachee messageAttachee, IM451Provider provider) => AISEntryStatusList.Codes.NotReleased;

		protected override Type MessageInterpreterType => typeof(IM451MessageInterpreter);
	}
}
