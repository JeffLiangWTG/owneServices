using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class UDRProcessor : CustomsAndExciseReportInboundMessageProcessor<UDRProvider>
	{
		public UDRProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.UDRMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(UDRMessageInterpreter);
	}
}
