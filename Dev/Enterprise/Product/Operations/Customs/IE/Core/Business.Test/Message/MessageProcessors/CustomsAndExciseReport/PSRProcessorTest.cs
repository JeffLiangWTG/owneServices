using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(PSRProcessor))]
	sealed class PSRProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<PSRProcessor, PSRProvider>
	{
		protected override ZString MessageFriendlyName => "Period (monthly) details – summary report for Payers";

		protected override PSRProcessor Processor => new PSRProcessor(logger, typeof(PSRMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.PSR;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePSRText();

		protected override ZString ExpectedMessageInterpretation => PSRMessageInterpreterTest.GetExpectedInterpretation();

		protected override (
			string FileName,
			(int row, (int column, string expectedValue)[] values)[] Cells
		) XlsAttachmentTestCases =>
		("Period (Monthly) details - summary report for Payers", [
			(1, [
				(1, "EORI"),
				(2, "Period"),
				(3, "Tax Total")
			]),
		]);
	}
}
