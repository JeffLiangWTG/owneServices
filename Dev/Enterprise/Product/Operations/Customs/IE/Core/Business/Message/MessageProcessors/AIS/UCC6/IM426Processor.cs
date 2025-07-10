using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM426Processor : AISMessageProcessor<AISInboundEDIMessage, IM426Provider>
	{
		public IM426Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM426Provider provider)
		{
			if (!provider.CustomsRegistrationNumber.IsEmpty && messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				aisMessageAttachee.SetCustomsRegistrationNumber(provider.CustomsRegistrationNumber);
			}
		}

		protected override string MessageFriendlyNameCore => Res.GetString("EB84AB50-894A-4DD1-A0FD-921604DC0439", "IM426 – Registration Notification");

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM426Provider provider) => CustomsWareEntryStatusList.Codes.Accepted;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM426Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(IM426MessageInterpreter);
	}
}
