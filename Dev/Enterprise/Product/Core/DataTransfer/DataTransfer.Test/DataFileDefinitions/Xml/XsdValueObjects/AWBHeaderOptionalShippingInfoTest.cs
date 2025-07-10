using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(AWBHeaderOptionalShippingInfo))]
	sealed class AWBHeaderOptionalShippingInfoTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				10, typeof(AWBHeaderOptionalShippingInfo).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			Xsd.AWBHeaderOptionalShippingInfo optionalShippingInfo = new Xsd.AWBHeaderOptionalShippingInfo();
			AssertEquals("Should not be specified by default", false, optionalShippingInfo.IsSpecified);

			optionalShippingInfo.Text1 = "ABC";
			AssertEquals("Should be specified if text1 is not empty", true, optionalShippingInfo.IsSpecified);
		}
	}
}
