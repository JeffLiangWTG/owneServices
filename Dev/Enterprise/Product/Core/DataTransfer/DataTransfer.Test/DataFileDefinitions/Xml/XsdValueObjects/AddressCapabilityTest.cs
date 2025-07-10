using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.AddressCapability))]
	sealed class AddressCapabilityTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.AddressCapability value = null;
			value = new Xsd.AddressCapabilityCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
