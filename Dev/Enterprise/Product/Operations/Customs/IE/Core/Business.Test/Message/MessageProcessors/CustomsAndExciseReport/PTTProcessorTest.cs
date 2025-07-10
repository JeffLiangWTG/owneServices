using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(PTTProcessor))]
	sealed class PTTProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<PTTProcessor, PTTProvider>
	{
		protected override ZString MessageFriendlyName => "Period (monthly) details – tax type report for Payers";

		protected override PTTProcessor Processor => new PTTProcessor(logger, typeof(PTTMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.PTT;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePTTText();

		protected override ZString ExpectedMessageInterpretation => PTTMessageInterpreterTest.GetExpectedInterpretation();
	}
}
