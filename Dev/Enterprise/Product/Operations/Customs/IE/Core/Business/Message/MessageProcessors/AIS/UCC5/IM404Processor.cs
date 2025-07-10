using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using IM404Provider = Enterprise.Customs.IE.Messaging.UCC5.IM404Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM404Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM404Provider>
	{
		public IM404Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM404Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM404Provider provider) => AISEntryStatusList.Codes.AmendmentRequestRegistration;

		protected override string MessageFriendlyNameCore => Res.GetString("BB29499A-CFE7-4277-A5F3-87038D0E364D", "IM404: Amendment Acceptance");

		protected override Type MessageInterpreterType => typeof(IM404MessageInterpreter);

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM404Provider provider)
		{
			(messageAttachee as IAISMessageAttachee)?.PopulateConfirmedDutiesAndTaxes(provider.GoodsShipment.Items);
		}
	}
}
