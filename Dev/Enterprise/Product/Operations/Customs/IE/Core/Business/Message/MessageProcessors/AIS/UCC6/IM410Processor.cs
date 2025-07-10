using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM410Processor : AISMessageProcessor<AISInboundEDIMessage, IM410Provider>
	{
		public IM410Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("83C1D5B6-730E-4FA4-8A16-33C8037437AB", "IM410: Invalidation of Customs Declaration");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM410Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM410Provider provider) => AISEntryStatusList.Codes.Cancelled;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM410Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				aisMessageAttachee.SetCustomsRegistrationNumber(provider.CustomsRegistrationNumber);
			}
		}

		protected override Type MessageInterpreterType => typeof(IM410MessageInterpreter);
	}
}
