using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS316Processor : AISMessageProcessor<AISInboundEDIMessage, TS316Provider>
	{
		public TS316Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.TS316MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS316Provider provider) => LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS316Provider provider) => AISEntryStatusList.Codes.Rejected;

		protected override Type MessageInterpreterType => typeof(TS316MessageInterpreter);

		protected override string GetEmailSubject(AISInboundEDIMessage message) => AISInterchangeTypeList.Descriptions.TS316;
	}
}
