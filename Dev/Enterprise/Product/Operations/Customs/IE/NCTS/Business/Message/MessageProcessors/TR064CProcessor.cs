using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR064CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR064CProvider>
	{
		public TR064CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("8B87682C-7016-44D2-9302-521CE97CB99D", "TR064C: REQUEST DECLARATION INVALIDATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR064CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR064CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.CancellationRequestedByCustoms;

		protected override Type MessageInterpreterType => typeof(TR064CMessageInterpreter);
	}
}
