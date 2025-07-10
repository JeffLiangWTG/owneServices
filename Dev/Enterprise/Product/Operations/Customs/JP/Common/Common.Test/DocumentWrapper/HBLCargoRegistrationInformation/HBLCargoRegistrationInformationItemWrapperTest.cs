using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(HBLCargoRegistrationInformationItemWrapper))]
sealed class HBLCargoRegistrationInformationItemWrapperTest : InboundMessageDocumentItemWrapperTest<HBLCargoRegistrationInformationItemWrapper, HBLCargoRegistrationInformationWrapper>
{
	public void TestItemProperties()
	{
		var wrapper = DocItemWrapper;

		CombineAssertions(() =>
		{
			AssertEquals("ItemNo", "01", wrapper.ItemNo);

			AssertEquals("I_5", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA5", wrapper.I_5);
			AssertEquals("I_6", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA7AAAAAAAAA8AAAAAAAAA9AAAAAAAAA:AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA7AAAAAAAAA8AAAAAAAAA9AAAAAAAAA:AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA7AAAAAAAAA8AAAAAAAAA9AAAAAAAAA:AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA6", wrapper.I_6);
			AssertEquals("I_7", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA7AAAAAAAAA8AAAAAAAAA9AAAAAAAAA:AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA7", wrapper.I_7);
			AssertEquals("I_8", "AAAAAAAAA1AAAAAA8", wrapper.I_8);
			AssertEquals("I_9", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAAAAAA4AAAAAAAAA5AAAAAAAAA6AAAAAAAAA9", wrapper.I_9);
			AssertEquals("I_10", "20,250,611", wrapper.I_10);
			AssertEquals("I_11", "A11", wrapper.I_11);
			AssertEquals("I_12", "9,999,999,312", wrapper.I_12);
			AssertEquals("I_13", "A13", wrapper.I_13);
			AssertEquals("I_14", "9,999,999,914", wrapper.I_14);
			AssertEquals("I_15", "A15", wrapper.I_15);
			AssertEquals("I_16", "A14", wrapper.I_16);
			AssertEquals("I_17", "A17", wrapper.I_17);
			AssertEquals("I_18", "2025/06/18", wrapper.I_18);
			AssertEquals("I_19", "2025/06/19", wrapper.I_19);
			AssertEquals("I_20_1", "21", wrapper.I_20_1);
			AssertEquals("I_20_2", "22", wrapper.I_20_2);
			AssertEquals("I_20_3", "23", wrapper.I_20_3);
			AssertEquals("I_20_4", "24", wrapper.I_20_4);
			AssertEquals("I_20_5", "25", wrapper.I_20_5);
		});
	}

	protected override string GetDefaultMessageTestFile() => "SAS0711Message.txt";
}
