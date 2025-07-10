using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DCTProcessor))]
	sealed class DCTProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<DCTProcessor, DCTProvider>
	{
		protected override ZString MessageFriendlyName => "Daily details – combined taxes report for Payers";

		protected override DCTProcessor Processor => new DCTProcessor(logger, typeof(DCTMessage));

		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.DCT;

		protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreateDCTText();

		protected override ZString ExpectedMessageInterpretation => DCTMessageInterpreterTest.GetExpectedInterpretation();

		protected override (
			string FileName,
			(int row, (int column, string expectedValue)[] values)[] Cells
		) XlsAttachmentTestCases =>
		("Daily details - combined taxes for Payers", [
			(6, [
				(1, "Importer Name"),
				(2, "Period"),
				(3, "MRN"),
				(6, "Declaration Message Type"),
				(20, "Commercial Transport Document"),
			]),
			(7, [
				(1, "MR Test ONeill"),
				(20, "N703124242"),
			]),
		]);
	}
}
