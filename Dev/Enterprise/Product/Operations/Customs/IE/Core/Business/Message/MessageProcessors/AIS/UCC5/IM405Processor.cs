using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM405Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM405Provider>
	{
		public IM405Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("12B94AB1-7E2C-4546-93B0-F5DFA9D78797", "IM405: Amendment Request Rejection");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, Messaging.IMessageAttachee messageAttachee, IM405Provider provider) => LogicalStatusList.Codes.Invalid;

		protected override Type MessageInterpreterType => typeof(IM405MessageInterpreter);

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;
	}
}
