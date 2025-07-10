using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using RD409Provider = Enterprise.Customs.IE.Messaging.UCC5.RD409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public sealed class RD409Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, RD409Provider>
	{
		public RD409Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.RD409MessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, RD409Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, RD409Provider provider) => provider.ReasonNotApproved.IsEmpty ? AISEntryStatusList.Codes.RefundApplicationAccepted : AISEntryStatusList.Codes.RefundApplicationRejected;

		protected override Type MessageInterpreterType => typeof(RD409MessageInterpreter);
	}
}
