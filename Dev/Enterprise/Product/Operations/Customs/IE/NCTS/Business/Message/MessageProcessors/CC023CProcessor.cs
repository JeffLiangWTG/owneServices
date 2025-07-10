using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC023CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC023CProvider>
	{
		public CC023CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("2E8FADD5-3961-43C0-8FEC-33021F974103", "CC023C: GUARANTOR NOTIFICATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC023CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(CC023CMessageInterpreter);
	}
}
