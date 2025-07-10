using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class DCTProcessor : CustomsAndExciseReportInboundMessageProcessor<DCTProvider>
	{
		public DCTProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.DCTMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(DCTMessageInterpreter);
	}
}
