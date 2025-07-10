using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(PCTProcessor))]
	sealed class PCTProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<PCTProcessor, PCTProvider>
	{
		protected override ZString MessageFriendlyName => "Period (monthly) details – combined taxes report for Payers";

		protected override PCTProcessor Processor => new PCTProcessor(logger, typeof(PCTMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.PCT;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePCTText();

		protected override ZString ExpectedMessageInterpretation => PCTMessageInterpreterTest.GetExpectedInterpretation();
	}
}
