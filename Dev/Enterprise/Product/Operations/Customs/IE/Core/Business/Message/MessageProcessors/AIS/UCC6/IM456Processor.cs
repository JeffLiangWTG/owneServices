using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM456Processor : AISMessageProcessor<AISInboundEDIMessage, IM456Provider>
	{
		public IM456Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("EF403629-08A2-4511-A60E-3BC4A99CC1C0", "IM456: Rejection from SCI");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM456Provider provider) => LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM456Provider provider) => AISEntryStatusList.Codes.Rejected;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM456Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				aisMessageAttachee.SetCustomsRegistrationNumber(provider.CustomsRegistrationNumber);
			}
		}

		protected override Type MessageInterpreterType => typeof(IM456MessageInterpreter);
	}
}
