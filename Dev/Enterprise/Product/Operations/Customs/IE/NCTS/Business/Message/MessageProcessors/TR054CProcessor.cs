using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR054CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR054CProvider>
	{
		public TR054CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("8A10449F-EF21-429A-8B93-BA3022E31807", "TR054C: REQUEST FOR ADVICE");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR054CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR054CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice;

		protected override Type MessageInterpreterType => typeof(TR054CMessageInterpreter);
	}
}
