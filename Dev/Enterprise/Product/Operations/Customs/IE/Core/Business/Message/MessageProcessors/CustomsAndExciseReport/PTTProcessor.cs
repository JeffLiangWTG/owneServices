using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class PTTProcessor : CustomsAndExciseReportInboundMessageProcessor<PTTProvider>
	{
		public PTTProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.PTTMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(PTTMessageInterpreter);
	}
}
