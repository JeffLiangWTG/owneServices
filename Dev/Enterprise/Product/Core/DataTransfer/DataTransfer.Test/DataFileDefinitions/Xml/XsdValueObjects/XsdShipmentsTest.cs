using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(Xsd.Shipments))]
	sealed class XsdShipmentsTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ShipmentCollection value = null;
			value = new Xsd.Shipments().Shipment;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
