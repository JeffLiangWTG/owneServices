using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(UDRProcessor))]
	sealed class UDRProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<UDRProcessor, UDRProvider>
	{
		protected override ZString MessageFriendlyName => "Unpaid declarations report for Payers";

		protected override UDRProcessor Processor => new UDRProcessor(logger, typeof(UDRMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.UDR;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreateUDRText();

		protected override ZString ExpectedMessageInterpretation => UDRMessageInterpreterTest.GetExpectedInterpretation();
	}
}
