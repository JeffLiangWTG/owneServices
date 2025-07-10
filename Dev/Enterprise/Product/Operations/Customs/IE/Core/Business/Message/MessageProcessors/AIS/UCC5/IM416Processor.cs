using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM416Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM416Provider>
	{
		public IM416Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("A141F031-8DC6-46F0-BC05-8E2A734CA4AC", "IM416: Customs Declaration Rejection");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, Messaging.IMessageAttachee messageAttachee, IM416Provider provider) => LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, Messaging.IMessageAttachee messageAttachee, IM416Provider provider) => AISEntryStatusList.Codes.Rejected;

		protected override Type MessageInterpreterType => typeof(IM416MessageInterpreter);
	}
}
