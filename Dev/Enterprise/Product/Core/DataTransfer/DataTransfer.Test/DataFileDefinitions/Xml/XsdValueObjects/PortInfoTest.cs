using System.ComponentModel;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.PortInfo))]
	sealed class PortInfoTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.PortInfo value = null;
			value = new Xsd.PortInfoCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.PortInfo port = new Xsd.PortInfo();
			AssertEquals("Should not be specified by default", false, port.IsSpecified);

			port.ContainerYard.Organisation.EDICode = "splaty";
			AssertEquals("Should be specified", true, port.IsSpecified);
			port.ContainerYard.IsSpecified = false;

			port.CTO.Organisation.EDICode = "splaty";
			AssertEquals("Should be specified", true, port.IsSpecified);
			port.CTO.IsSpecified = false;

			port.Depot.Organisation.EDICode = "splaty";
			AssertEquals("Should be specified", true, port.IsSpecified);

			port.IsSpecified = false;
			AssertEquals("Should not be specified when IsSpecified set to false explicitly", false, port.IsSpecified);
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
				"If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
				8, TypeDescriptor.GetProperties(typeof(Xsd.PortInfo)).Count);
		}
	}
}
