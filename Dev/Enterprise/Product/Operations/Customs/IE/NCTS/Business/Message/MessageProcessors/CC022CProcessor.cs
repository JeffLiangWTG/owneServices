using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC022CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC022CProvider>
	{
		public CC022CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("61989749-9716-4012-BDD3-F2C66E40F31F", "CC022C: NOTIFICATION TO AMEND DECLARATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC022CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC022CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;

		protected override Type MessageInterpreterType => typeof(CC022CMessageInterpreter);
	}
}
