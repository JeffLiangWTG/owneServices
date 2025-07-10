using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(HBLCancellationInformationDocumentWrapper))]
	sealed class HBLCancellationInformationDocumentWrapperTest
		: InboundMessageDocumentWrapperTest<HBLCancellationInformationDocumentWrapper, IHBLCancellationInformation>
	{
		public void TestHeaderProperties()
		{
			var wrapper = MessageDocumentWrapper;
			CombineAssertions(() =>
			{
				AssertEquals("H_2", "AAAAAAAAA1AAAAAAAAA2AAAAAAAAA3AAAAA", wrapper.H_2);
				AssertEquals("H_3", "AAAA3", wrapper.H_3);
				AssertEquals("H_4", "AAAA4", wrapper.H_4);
			});
		}

		protected override string GetDefaultMessageTestFile() => "SAS0731TestMessage.txt";
	}
}
