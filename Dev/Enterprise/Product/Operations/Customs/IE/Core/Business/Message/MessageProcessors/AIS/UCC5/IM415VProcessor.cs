using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using IM415VProvider = Enterprise.Customs.IE.Messaging.UCC5.IM415VProvider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM415VProcessor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM415VProvider>
	{
		public IM415VProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("8DED169A-99BE-4A4D-BF5D-D8D53A3B004E", "IM415V: Customs Declaration Acknowledgment");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM415VProvider provider)
		{
			(messageAttachee as IAISMessageAttachee)?.MovementReferenceNumberSetter(provider.MovementReferenceNumber, provider.DeclarationAcknowledgementDate);
		}

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM415VProvider provider) => LogicalStatusList.Codes.Acknowledged;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM415VProvider provider) => AISEntryStatusList.Codes.Prelodged;

		protected override Type MessageInterpreterType => typeof(IM415VMessageInterpreter);
	}
}
