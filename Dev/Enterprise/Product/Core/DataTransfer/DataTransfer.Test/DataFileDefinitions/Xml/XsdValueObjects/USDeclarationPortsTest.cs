using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationPorts))]
	sealed class USDeclarationPortsTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationPorts uSDeclarationPorts = new Xsd.USDeclarationPorts();
			AssertEquals(false, uSDeclarationPorts.IsSpecified);

			uSDeclarationPorts.DesignatedExamPort = "1233";
			AssertEquals(true, uSDeclarationPorts.IsSpecified);

			uSDeclarationPorts.DesignatedExamPort = "";
			uSDeclarationPorts.Discharge = "1233";
			AssertEquals(true, uSDeclarationPorts.IsSpecified);

			uSDeclarationPorts.Discharge = "";
			uSDeclarationPorts.Loading = "1233";
			AssertEquals(true, uSDeclarationPorts.IsSpecified);

			uSDeclarationPorts.Loading = "";
			uSDeclarationPorts.PortOfEntry = "1233";
			AssertEquals(true, uSDeclarationPorts.IsSpecified);

			uSDeclarationPorts.PortOfEntry = "";
			uSDeclarationPorts.PreparerDistrictPort = "1233";
			AssertEquals(true, uSDeclarationPorts.IsSpecified);

			uSDeclarationPorts.PreparerDistrictPort = "";
			AssertEquals(false, uSDeclarationPorts.IsSpecified);
		}
	}
}
