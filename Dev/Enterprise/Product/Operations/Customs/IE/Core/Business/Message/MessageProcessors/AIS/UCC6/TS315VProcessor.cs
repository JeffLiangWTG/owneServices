using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS315VProcessor : AISMessageProcessor<AISInboundEDIMessage, TS315VProvider>
	{
		public TS315VProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.TS315VMessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS315VProvider provider) => LogicalStatusList.Codes.Acknowledged;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS315VProvider provider) => AISEntryStatusList.Codes.Registered;

		protected override Type MessageInterpreterType => typeof(TS315VMessageInterpreter);
	}
}
