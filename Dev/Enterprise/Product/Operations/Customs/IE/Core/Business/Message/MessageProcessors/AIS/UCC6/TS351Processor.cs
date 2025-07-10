using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	class TS351Processor : AISMessageProcessor<AISInboundEDIMessage, TS351Provider>
	{
		public TS351Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.TS351MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS351Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS351Provider provider) => AISEntryStatusList.Codes.NotReleased;

		protected override Type MessageInterpreterType => typeof(TS351MessageInterpreter);
	}
}
