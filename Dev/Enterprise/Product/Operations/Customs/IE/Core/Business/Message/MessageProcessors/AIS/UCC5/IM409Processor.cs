using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using IM409Provider = Enterprise.Customs.IE.Messaging.UCC5.IM409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM409Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM409Provider>
	{
		public IM409Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM409Provider provider)
		{
			return provider.InvalidationDecision ? LogicalStatusList.Codes.Accepted : LogicalStatusList.Codes.Invalid;
		}

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM409Provider provider)
		{
			return provider.InvalidationDecision ? AISEntryStatusList.Codes.Cancelled : null;
		}

		protected override string MessageFriendlyNameCore => Res.GetString("DB4ADDEE-9DF4-498C-BD9E-04D59C874EE7", "IM409: Invalidation Request Decision");

		protected override Type MessageInterpreterType => typeof(IM409MessageInterpreter);

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;
	}
}
