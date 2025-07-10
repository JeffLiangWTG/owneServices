using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public sealed class RF416Processor : AISMessageProcessor<AISInboundEDIMessage, RF416Provider>
	{
		public RF416Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.RF416MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, RF416Provider provider) => LogicalStatusList.Codes.Invalid;

		protected override Type MessageInterpreterType => typeof(RF416MessageInterpreter);
	}
}
