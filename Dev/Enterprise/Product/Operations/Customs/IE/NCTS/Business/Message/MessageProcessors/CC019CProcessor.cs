using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC019CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC019CProvider>
	{
		public CC019CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("FAA68181-DAEC-4B87-B8C3-B060AC878A0E", "CC019C: DISCREPANCIES");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC019CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC019CProvider provider) => EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList.Codes.DiscrepanciesAtDestination;

		protected override Type MessageInterpreterType => typeof(CC019CMessageInterpreter);
	}
}
