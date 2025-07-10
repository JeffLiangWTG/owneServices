using System.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ShipmentShipmentDetailsPickup))]
	sealed class ShipmentShipmentDetailsPickupTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ShipmentShipmentDetailsPickup value = null;
			value = new Xsd.ShipmentShipmentDetailsPickup();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.ShipmentShipmentDetailsPickup pickup = new Xsd.ShipmentShipmentDetailsPickup();
			AssertEquals("Should not be specified by default", false, pickup.IsSpecified);

			pickup.GoodsPickup = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, pickup.IsSpecified);

			pickup.GoodsPickup = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, pickup.IsSpecified);

			pickup.CartageAdvised = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, pickup.IsSpecified);

			pickup.CartageAdvised = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, pickup.IsSpecified);

			pickup.PickupRequiredBy = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, pickup.IsSpecified);

			pickup.PickupRequiredBy = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, pickup.IsSpecified);

			pickup.PickupFrom = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, pickup.IsSpecified);

			pickup.PickupFrom = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, pickup.IsSpecified);

			pickup.DateOfReceipt = new ZDateTime(2005, 1, 1);
			AssertEquals("Should be specified", true, pickup.IsSpecified);

			pickup.DateOfReceipt = ZDateTime.Empty;
			AssertEquals("Should Not be specified", false, pickup.IsSpecified);

			pickup.IsSpecified = true;
			AssertEquals("Should Not be specified", false, pickup.IsSpecified);

			pickup.Address = new Xsd.OrgAddress();
			AssertEquals("Should Not be specified", false, pickup.IsSpecified);

			pickup.Address.AddressLine1 = "Address 1";
			AssertEquals("Should  be specified", true, pickup.IsSpecified);

			pickup.Address = new Xsd.OrgAddress();
			AssertEquals("Should Not be specified", false, pickup.IsSpecified);

			pickup.CFS.Address.Organisation.EDICode = "EDICODE";
			AssertEquals("Should  be specified", true, pickup.IsSpecified);
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
			  "If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
			  29, TypeDescriptor.GetProperties(typeof(Xsd.ShipmentShipmentDetailsPickup)).Count);
		}
	}
}
