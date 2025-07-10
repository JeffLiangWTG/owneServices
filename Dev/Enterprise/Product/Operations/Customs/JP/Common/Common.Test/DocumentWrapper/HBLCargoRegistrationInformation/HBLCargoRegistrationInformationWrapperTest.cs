using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(HBLCargoRegistrationInformationWrapper))]
sealed class HBLCargoRegistrationInformationWrapperTest : InboundMessageDocumentWrapperTest<HBLCargoRegistrationInformationWrapper, IHBLRegistrationInformationProvider>
{
	public void TestHeaderProperties()
	{
		var wrapper = MessageDocumentWrapper;
		CombineAssertions(() =>
		{
			AssertEquals("H_2", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAA2", wrapper.H_2);
			AssertEquals("H_3", "AAAA3", wrapper.H_3);
			AssertEquals("H_4", "AAAA4", wrapper.H_4);
		});
	}

	protected override string GetDefaultMessageTestFile() => "SAS0711Message.txt";
}
