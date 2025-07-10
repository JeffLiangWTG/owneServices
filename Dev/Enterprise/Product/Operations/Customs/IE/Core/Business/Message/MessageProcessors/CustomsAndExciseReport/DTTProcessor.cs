using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class DTTProcessor : CustomsAndExciseReportInboundMessageProcessor<DTTProvider>
	{
		public DTTProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.DTTMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(DTTMessageInterpreter);
	}
}
