using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USManufacturer))]
	sealed class USManufacturerTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USManufacturer uSManufacturer = new Xsd.USManufacturer();
			AssertEquals(false, uSManufacturer.IsSpecified);

			uSManufacturer.Item = "TEST";
			AssertEquals(true, uSManufacturer.IsSpecified);

			uSManufacturer.Item = "";
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "TST";
			uSManufacturer.Item = organisation;
			AssertEquals(true, uSManufacturer.IsSpecified);

			organisation.EDICode = "";
			AssertEquals(false, uSManufacturer.IsSpecified);
		}
	}
}
