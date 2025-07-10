using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM457Processor : AISMessageProcessor<AISInboundEDIMessage, IM457Provider>
	{
		public IM457Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("34813D70-8241-46C9-B442-97913CF5A8CB", "IM457: Presentation Notification Registration");

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM457Provider provider) => AISEntryStatusList.Codes.Registered;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM457Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(IM457MessageInterpreter);
	}
}
