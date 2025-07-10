using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM431Processor : AISMessageProcessor<AISInboundEDIMessage, IM431Provider>
	{
		public IM431Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("0325AA8C-D544-4C84-AB80-4B6B50DDA909", "IM431: Expiration of Timer for Supplementary Declaration");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM431Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM431Provider provider)
		{
			(messageAttachee as IAISMessageAttachee)?.MovementReferenceNumberSetter(provider.MovementReferenceNumber, expiryDate: provider.LodgementOfSupplementaryDeclarationExpiryDate);
		}

		protected override Type MessageInterpreterType => typeof(IM431MessageInterpreter);
	}
}
