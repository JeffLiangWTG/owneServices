using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class PSRProcessor : CustomsAndExciseReportInboundMessageProcessor<PSRProvider>
	{
		public PSRProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.PSRMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(PSRMessageInterpreter);
	}
}
