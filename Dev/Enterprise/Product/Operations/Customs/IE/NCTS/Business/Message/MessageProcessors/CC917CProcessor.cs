using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC917CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC917CProvider>
	{
		public CC917CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F5C21F4D-FC06-46EA-AD30-A84CC1C9666D", "CC917C: XML NACK");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC917CProvider provider) => LogicalStatusList.Codes.Error;

		protected override Type MessageInterpreterType => typeof(CC917CMessageInterpreter);
	}
}
