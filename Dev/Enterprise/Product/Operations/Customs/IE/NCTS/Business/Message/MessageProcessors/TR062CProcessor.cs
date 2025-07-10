using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR062CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR062CProvider>
	{
		public TR062CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B2015BB4-7241-4693-AD16-CBCE5888A6D8", "TR062C: REQUEST DECLARATION AMENDMENT");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR062CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR062CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;

		protected override Type MessageInterpreterType => typeof(TR062CMessageInterpreter);
	}
}
