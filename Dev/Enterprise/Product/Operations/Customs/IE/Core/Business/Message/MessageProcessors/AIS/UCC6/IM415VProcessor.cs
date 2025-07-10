using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM415VProcessor : AISMessageProcessor<AISInboundEDIMessage, IM415VProvider>
	{
		public IM415VProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("F6496ACC-5DCC-44DF-B7AA-9DFB0520D815", "IM415V: Customs Declaration Acknowledgment");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM415VProvider provider)
		{
			(messageAttachee as IAISMessageAttachee)?.MovementReferenceNumberSetter(provider.MovementReferenceNumber, provider.DeclarationAcknowledgementDate);
		}

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM415VProvider provider) => LogicalStatusList.Codes.Acknowledged;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM415VProvider provider) => AISEntryStatusList.Codes.Prelodged;

		protected override Type MessageInterpreterType => typeof(IM415VMessageInterpreter);
	}
}
