using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ContainerType))]
	sealed class ContainerTypeTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ContainerType value = null;
			value = new Xsd.ContainerTypeCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.ContainerType containerType = new Xsd.ContainerType();
			AssertEquals("ContainerType should not be specified by default", false, containerType.IsSpecified);

			containerType.ISOCode = "splaty";
			containerType.ContainerCode = null;
			AssertEquals("ContainerType should be specified when ISOCode populated", true, containerType.IsSpecified);

			containerType.ISOCode = null;
			containerType.ContainerCode = "splaty";
			AssertEquals("ContainerType should be specified when ContainerCode populated", true, containerType.IsSpecified);

			containerType.IsSpecified = false;
			AssertEquals("IsSpecified=false when explicitly set to false", false, containerType.IsSpecified);
		}
	}
}
