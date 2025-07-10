using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(CancellationOfTransshipmentReportDocumentWrapper))]
sealed class CancellationOfTransshipmentReportMessageDocumentWrapperTest : InboundMessageDocumentWrapperTest<CancellationOfTransshipmentReportDocumentWrapper, ICancellationOfTransshipReport>
{
	protected override string GetDefaultMessageTestFile() => "CancellationOfTransshipmentReport.txt";

	public void TestHeaderAndItemFields()
	{
		var wrapper = MessageDocumentWrapper;

		CombineAssertions(() =>
		{
			AssertEquals("H_2", "2222", wrapper.H_2);
			AssertEquals("H_3", "333333333", wrapper.H_3);
			AssertEquals("H_4", "44444444444444444444444444444444444", wrapper.H_4);
			AssertEquals("H_5", "55555", wrapper.H_5);
			AssertEquals("H_6", "6", wrapper.H_6);
			AssertEquals("H_7", "77777", wrapper.H_7);
			AssertEquals("H_8", "88888888888888888888888888888888888", wrapper.H_8);

			AssertEquals("I_9_1", "12345678901", wrapper.I_9_1);
			AssertEquals("I_9_2", "12345678902", wrapper.I_9_2);
			AssertEquals("I_9_3", "12345678903", wrapper.I_9_3);
			AssertEquals("I_9_4", "12345678904", wrapper.I_9_4);
			AssertEquals("I_9_5", "12345678905", wrapper.I_9_5);
			AssertEquals("I_9_6", "12345678906", wrapper.I_9_6);
			AssertEquals("I_9_7", "12345678907", wrapper.I_9_7);
			AssertEquals("I_9_8", "12345678908", wrapper.I_9_8);
			AssertEquals("I_9_9", "12345678909", wrapper.I_9_9);
			AssertEquals("I_9_10", "12345678910", wrapper.I_9_10);
			AssertEquals("I_9_11", "12345678911", wrapper.I_9_11);
			AssertEquals("I_9_12", "12345678912", wrapper.I_9_12);
			AssertEquals("I_9_13", "12345678913", wrapper.I_9_13);
			AssertEquals("I_9_14", "12345678914", wrapper.I_9_14);
			AssertEquals("I_9_15", "12345678915", wrapper.I_9_15);
			AssertEquals("I_9_16", "12345678916", wrapper.I_9_16);
			AssertEquals("I_9_17", "12345678917", wrapper.I_9_17);
			AssertEquals("I_9_18", "12345678918", wrapper.I_9_18);
			AssertEquals("I_9_19", "12345678919", wrapper.I_9_19);
			AssertEquals("I_9_20", "12345678920", wrapper.I_9_20);
		});
	}
}
