using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(PCIProcessor))]
	sealed class PCIProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<PCIProcessor, PCIProvider>
	{
		protected override ZString MessageFriendlyName => "Period (monthly) details – combined taxes report for Importers";

		protected override PCIProcessor Processor => new PCIProcessor(logger, typeof(PCIMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.PCI;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePCIText();

		protected override ZString ExpectedMessageInterpretation => PCIMessageInterpreterTest.GetExpectedInterpretation();
	}
}
