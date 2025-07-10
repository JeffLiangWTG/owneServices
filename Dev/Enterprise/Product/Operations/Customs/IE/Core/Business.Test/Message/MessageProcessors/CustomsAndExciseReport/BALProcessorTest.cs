using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(BALProcessor))]
	sealed class BALProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<BALProcessor, BALProvider>
	{
		protected override ZString MessageFriendlyName => "Balance";

		protected override BALProcessor Processor => new BALProcessor(logger, typeof(BALMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.BAL;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreateBALText();

		protected override ZString ExpectedMessageInterpretation => BALMessageInterpreterTest.GetExpectedInterpretation();
	}
}
