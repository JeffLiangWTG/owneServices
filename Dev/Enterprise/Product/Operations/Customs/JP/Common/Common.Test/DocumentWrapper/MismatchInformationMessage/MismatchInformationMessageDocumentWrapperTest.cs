using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(MismatchInformationMessageDocumentWrapper))]
sealed class MismatchInformationMessageDocumentWrapperTest : InboundMessageDocumentWrapperTest<MismatchInformationMessageDocumentWrapper, IMismatchInformation>
{
	public void TestHeaderProperties()
	{
		var wrapper = MessageDocumentWrapper;
		CombineAssertions(() =>
		{
			AssertEquals("H_2", "222", wrapper.H_2);
			AssertEquals("H_3", "33333", wrapper.H_3);
			AssertEquals("H_4", "44444", wrapper.H_4);
			AssertEquals("H_5", "55555", wrapper.H_5);
			AssertEquals("H_6", "666666", wrapper.H_6);
			AssertEquals("H_7", "01JAN", wrapper.H_7);
			AssertEquals("H_8", "888", wrapper.H_8);
			AssertEquals("H_9", "99999999999999999999", wrapper.H_9);
			AssertEquals("H_10", "2025/01/02", wrapper.H_10);
		});
	}

	protected override string GetDefaultMessageTestFile() => "MismatchInformationMessage.txt";
}
