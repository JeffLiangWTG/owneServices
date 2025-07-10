using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC035CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC035CProvider>
	{
		public CC035CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("C2D40240-A00F-49D2-A598-6C310ABD2095", "CC035C: RECOVERY NOTIFICATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC035CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC035CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure;

		protected override Type MessageInterpreterType => typeof(CC035CMessageInterpreter);
	}
}
