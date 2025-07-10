using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public sealed class RD409Processor : AISMessageProcessor<AISInboundEDIMessage, RD409Provider>
	{
		public RD409Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.RD409MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, RD409Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, RD409Provider provider) => provider.ReasonNotApproved.IsEmpty ? AISEntryStatusList.Codes.RefundApplicationAccepted : AISEntryStatusList.Codes.RefundApplicationRejected;

		protected override Type MessageInterpreterType => typeof(RD409MessageInterpreter);
	}
}
