using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR015VProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR015VProvider>
	{
		public TR015VProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F9ECCEFF-231E-4007-BB7E-44275112415V", "TR015V: TRANSIT PRE-LODGED DECLARATION ACKNOWLEDGMENT");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR015VProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR015VProvider provider) => NCTS5DepartureCustomsStatusList.Codes.PreLodged;

		protected override Type MessageInterpreterType => typeof(TR015MessageInterpreter);
	}
}
