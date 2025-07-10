using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM433Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM433Provider>
	{
		public IM433Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("C716EC70-B692-4E0C-9E35-35160453D3E9", "IM433: Presentation Notification Rejection");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM433Provider provider) => LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM433Provider provider) => AISEntryStatusList.Codes.Rejected;

		protected override Type MessageInterpreterType => typeof(IM433MessageInterpreter);
	}
}
