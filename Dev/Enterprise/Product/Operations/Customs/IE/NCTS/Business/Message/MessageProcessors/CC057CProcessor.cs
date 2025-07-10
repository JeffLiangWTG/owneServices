using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC057CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC057CProvider>
	{
		public CC057CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC057CProvider provider) => LogicalStatusList.Codes.Invalid;

		protected override string MessageFriendlyNameCore => Res.GetString("94819F2A-4F8D-4D28-8F6F-1C5D731822A9", "CC057C: REJECTION FROM OFFICE OF DESTINATION");

		protected override Type MessageInterpreterType => typeof(CC057CMessageInterpreter);
	}
}
