using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DTTProcessor))]
	sealed class DTTProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<DTTProcessor, DTTProvider>
	{
		protected override ZString MessageFriendlyName => "Daily details – tax type report for Payers";

		protected override DTTProcessor Processor => new DTTProcessor(logger, typeof(DTTMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.DTT;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreateDTTText();

		protected override ZString ExpectedMessageInterpretation => DTTMessageInterpreterTest.GetExpectedInterpretation();
	}
}
