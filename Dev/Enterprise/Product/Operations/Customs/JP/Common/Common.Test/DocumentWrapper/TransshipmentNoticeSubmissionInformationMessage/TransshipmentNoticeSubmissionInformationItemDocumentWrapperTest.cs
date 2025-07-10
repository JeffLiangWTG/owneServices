using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(TransshipmentNoticeSubmissionInformationItemDocumentWrapper))]
sealed class TransshipmentNoticeSubmissionInformationItemDocumentWrapperTest
	: InboundMessageDocumentItemWrapperTest<TransshipmentNoticeSubmissionInformationItemDocumentWrapper, TransshipmentNoticeSubmissionInformationDocumentWrapper>
{
	public void TestItemProperties()
	{
		var wrapper = DocItemWrapper;

		CombineAssertions(() =>
		{
			AssertEquals("I_9_1", "12345678901", wrapper.I_9_1);
			AssertEquals("I_10_1", "10101010101", wrapper.I_10_1);
			AssertEquals("I_9_2", "12345678902", wrapper.I_9_2);
			AssertEquals("I_10_2", "10101010102", wrapper.I_10_2);
			AssertEquals("I_9_3", "12345678903", wrapper.I_9_3);
			AssertEquals("I_10_3", "10101010103", wrapper.I_10_3);
			AssertEquals("I_9_4", "12345678904", wrapper.I_9_4);
			AssertEquals("I_10_4", "10101010104", wrapper.I_10_4);
			AssertEquals("I_9_5", "12345678905", wrapper.I_9_5);
			AssertEquals("I_10_5", "10101010105", wrapper.I_10_5);
			AssertEquals("I_9_6", "12345678906", wrapper.I_9_6);
			AssertEquals("I_10_6", "10101010106", wrapper.I_10_6);
			AssertEquals("I_9_7", "12345678907", wrapper.I_9_7);
			AssertEquals("I_10_7", "10101010107", wrapper.I_10_7);
			AssertEquals("I_9_8", "12345678908", wrapper.I_9_8);
			AssertEquals("I_10_8", "10101010108", wrapper.I_10_8);
			AssertEquals("I_9_9", "12345678909", wrapper.I_9_9);
			AssertEquals("I_10_9", "10101010109", wrapper.I_10_9);
			AssertEquals("I_9_10", "12345678910", wrapper.I_9_10);
			AssertEquals("I_10_10", "10101010110", wrapper.I_10_10);
		});
	}

	protected override string GetDefaultMessageTestFile() => "TransshipmentNoticeSubmissionInformation.txt";
}
