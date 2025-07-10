using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public sealed class PCIProcessor : CustomsAndExciseReportInboundMessageProcessor<PCIProvider>
	{
		public PCIProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.PCIMessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(PCIMessageInterpreter);
	}
}
