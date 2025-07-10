using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class BALProcessor : CustomsAndExciseReportInboundMessageProcessor<BALProvider>
	{
		public BALProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.BALMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(BALMessageInterpreter);
	}
}
