using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public sealed class RF409Processor : AISMessageProcessor<AISInboundEDIMessage, RF409Provider>
	{
		public RF409Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.RF409MessageFriendlyName;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, RF409Provider provider)
			=> provider.RefundApplicationAccepted ? AISEntryStatusList.Codes.RefundApplicationAccepted : AISEntryStatusList.Codes.RefundApplicationRejected;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, RF409Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(RF409MessageInterpreter);
	}
}
