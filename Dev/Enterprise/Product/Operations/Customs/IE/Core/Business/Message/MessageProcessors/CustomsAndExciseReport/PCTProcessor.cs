using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class PCTProcessor : CustomsAndExciseReportInboundMessageProcessor<PCTProvider>
	{
		public PCTProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.PCTMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(PCTMessageInterpreter);
	}
}
