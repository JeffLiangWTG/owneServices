using System.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ShipmentShipmentDetailsDeliver))]
	sealed class ShipmentShipmentDetailsDeliverTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ShipmentShipmentDetailsDeliver value = null;
			value = new Xsd.ShipmentShipmentDetailsDeliver();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.ShipmentShipmentDetailsDeliver deliver = new Xsd.ShipmentShipmentDetailsDeliver();
			AssertEquals("Should not be specified by default", false, deliver.IsSpecified);

			deliver.GoodsDelivered = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, deliver.IsSpecified);

			deliver.GoodsDelivered = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, deliver.IsSpecified);

			deliver.CartageAdvised = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, deliver.IsSpecified);

			deliver.CartageAdvised = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, deliver.IsSpecified);

			deliver.DeliveryRequiredBy = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, deliver.IsSpecified);

			deliver.DeliveryRequiredBy = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, deliver.IsSpecified);

			deliver.DeliveryFrom = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, deliver.IsSpecified);

			deliver.DeliveryFrom = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, deliver.IsSpecified);

			deliver.DeliveryAgent.EDICode = "EDICODE";
			AssertEquals("Should be specified", true, deliver.IsSpecified);

			deliver.DeliveryAgent = new Organisation();
			AssertEquals("Should Not be specified", false, deliver.IsSpecified);

			deliver.IsSpecified = true;
			AssertEquals("Should Not be specified", false, deliver.IsSpecified);

			deliver.Address = new Xsd.OrgAddress();
			AssertEquals("Should Not be specified", false, deliver.IsSpecified);

			deliver.Address.AddressLine1 = "Address 1";
			AssertEquals("Should be specified", true, deliver.IsSpecified);

			deliver.DeliveryLegs = new ContainerLegCollection();
			AssertEquals("Should be specified", false, deliver.DeliveryLegs.IsSpecified);
			ContainerLeg leg = deliver.DeliveryLegs.AddNew();
			AssertEquals("Should be specified", true, deliver.DeliveryLegs.IsSpecified);
			AssertEquals("Should be specified", true, leg.IsSpecified);
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
			  "If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
			  28, TypeDescriptor.GetProperties(typeof(Xsd.ShipmentShipmentDetailsDeliver)).Count);
		}
	}
}
