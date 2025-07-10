using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class DSRProcessor : CustomsAndExciseReportInboundMessageProcessor<DSRProvider>
	{
		public DSRProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.DSRMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(DSRMessageInterpreter);
	}
}
