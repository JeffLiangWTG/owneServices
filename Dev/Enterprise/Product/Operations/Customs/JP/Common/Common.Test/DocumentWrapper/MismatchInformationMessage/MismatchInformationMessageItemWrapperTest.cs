using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(MismatchInformationMessageItemWrapper))]
sealed class MismatchInformationMessageItemWrapperTest : InboundMessageDocumentItemWrapperTest<MismatchInformationMessageItemWrapper, MismatchInformationMessageDocumentWrapper>
{
	public void TestItemProperties()
	{
		var wrapper = DocItemWrapper;

		CombineAssertions(() =>
		{
			AssertEquals("I_11", "11111111111111111111", wrapper.I_11);
			AssertEquals("I_12", "2", wrapper.I_12);
			AssertEquals("I_13", "333,333", wrapper.I_13);
			AssertEquals("I_14", "444,444", wrapper.I_14);
			AssertEquals("I_15", "5", wrapper.I_15);
			AssertEquals("I_16", "666,666", wrapper.I_16);
			AssertEquals("I_17", "7", wrapper.I_17);
			AssertEquals("I_18", "88,888,888", wrapper.I_18);
			AssertEquals("I_19", "9", wrapper.I_19);
			AssertEquals("I_20", "10,000,000", wrapper.I_20);
			AssertEquals("I_21", "1", wrapper.I_21);
			AssertEquals("I_22", "22222", wrapper.I_22);
			AssertEquals("I_23", "333", wrapper.I_23);
		});
	}

	protected override string GetDefaultMessageTestFile() => "MismatchInformationMessage.txt";
}
