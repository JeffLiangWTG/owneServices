using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(TransshipmentNoticeSubmissionInformationDocumentWrapper))]
sealed class TransshipmentNoticeSubmissionInformationDocumentWrapperTest : InboundMessageDocumentWrapperTest<TransshipmentNoticeSubmissionInformationDocumentWrapper, ITransshipmentNoticeSubmissionInformation>
{
	public void TestHeaderProperties()
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
		});
	}

	public void TestItems()
	{
		AssertEquals(1, MessageDocumentWrapper.Items.Count);
	}

	protected override string GetDefaultMessageTestFile() => "TransshipmentNoticeSubmissionInformation.txt";
}
