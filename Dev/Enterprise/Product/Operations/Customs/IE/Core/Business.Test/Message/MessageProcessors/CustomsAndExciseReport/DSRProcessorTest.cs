using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DSRProcessor))]
	sealed class DSRProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<DSRProcessor, DSRProvider>
	{
		protected override ZString MessageFriendlyName => "Daily details – summary report for Payers";

		protected override DSRProcessor Processor => new DSRProcessor(logger, typeof(DSRMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.DSR;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreateDSRText();

		protected override ZString ExpectedMessageInterpretation => DSRMessageInterpreterTest.GetExpectedInterpretation();
	}
}
