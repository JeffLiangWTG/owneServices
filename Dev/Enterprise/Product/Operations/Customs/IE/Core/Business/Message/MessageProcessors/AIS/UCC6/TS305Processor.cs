using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS305Processor : AISMessageProcessor<AISInboundEDIMessage, TS305Provider>
	{
		public TS305Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.TS305MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS305Provider provider) => LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS305Provider provider) => AISEntryStatusList.Codes.Registered;

		protected override Type MessageInterpreterType => typeof(TS305MessageInterpreter);
	}
}
