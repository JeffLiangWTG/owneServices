using PaymentType = Enterprise.DataTransfer.Xml.XsdVersion1.PaymentType;

namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	sealed class OceanBillLineTest : BaseLineTest
	{
		public void TestProperties()
		{
			AssertEquals("OceanBillNumber", "8PGUADL400700", OceanBill.OceanBillNumber);
			AssertEquals("MethodOfPayment", PaymentType.PPD, OceanBill.MethodOfPayment);
			AssertEquals("PortOfOrigin", "MYPGU", OceanBill.PortOfOrigin);
			AssertEquals("PortOfLoading", "MYPKG", OceanBill.PortOfLoading);
			AssertNotNull(OceanBill.VesselVoyage);
		}

		public void TestPlaceInfo()
		{
			AssertNull("Precondition: no placeinfo", OceanBill.PlaceInfo);
			var newPlaceInfo = OceanBill.AddNewPlaceInfo(PlaceInfoRow);
			AssertEquals(OceanBill.PlaceInfo, newPlaceInfo);
			AssertNotNull(newPlaceInfo);
			AssertEquals(PlaceInfo.PortOfDestination, newPlaceInfo.PortOfDestination);
			Assert("Lazy-loading PlaceInfo replaces our placeinfo.", newPlaceInfo != OceanBill.PlaceInfo);
		}

		public void TestShipperDetails()
		{
			AssertNull("Precondition: no shipper details", OceanBill.ShipperDetails);
			var newShipperDetails = OceanBill.AddNewPartyDetails(ShipperDetailsRow);
			AssertNotNull(newShipperDetails);
			AssertEquals(newShipperDetails, OceanBill.ShipperDetails);
			AssertEquals(ShipperDetails.Address1, newShipperDetails.Address1);
			Assert("Lazy-loading ShipperDetails replaces our shipperdetails.", newShipperDetails != OceanBill.ShipperDetails);
		}

		public void TestConsigneeDetails()
		{
			AssertNull("Precondition: no consignee details", OceanBill.ConsigneeDetails);
			var newConsigneeDetails = OceanBill.AddNewPartyDetails(ConsigneeDetailsRow);
			AssertNotNull(newConsigneeDetails);
			AssertEquals(newConsigneeDetails, OceanBill.ConsigneeDetails);
			AssertEquals(ConsigneeDetails.Address1, newConsigneeDetails.Address1);
			Assert("Lazy-loading ConsigneeDetails replaces our consigneedetails", newConsigneeDetails != OceanBill.ConsigneeDetails);
		}

		public void TestPartyInfoConsigneeAndShipperAreSeparate()
		{
			AssertNull("Precondition: no shipper details", OceanBill.ShipperDetails);
			AssertNull("Precondition: no consignee details", OceanBill.ConsigneeDetails);
			var newConsigneeDetails = OceanBill.AddNewPartyDetails(ConsigneeDetailsRow);
			var newShipperDetails = OceanBill.AddNewPartyDetails(ShipperDetailsRow);
			AssertNotNull(newConsigneeDetails);
			AssertNotNull(newShipperDetails);
			AssertEquals(newConsigneeDetails, OceanBill.ConsigneeDetails);
			AssertEquals(newShipperDetails, OceanBill.ShipperDetails);
		}

		public void TestCargoFields()
		{
			AssertEquals("Precondition: No cargo fields", 0, OceanBill.CargoFields.Count);
			var newCargoField = OceanBill.AddNewCargoField(CargoFieldRow);
			AssertNotNull(newCargoField);
			AssertEquals(1, OceanBill.CargoFields.Count);
			AssertEquals(newCargoField, OceanBill.CargoFields[0]);
		}

		public void TestContainerFields()
		{
			AssertEquals("Precondition: No container fields", 0, OceanBill.ContainerFields.Count);
			var newContainerField = OceanBill.AddNewContainerField(ContainerFieldRow);
			AssertNotNull(newContainerField);
			AssertEquals(1, OceanBill.ContainerFields.Count);
			AssertEquals(newContainerField, OceanBill.ContainerFields[0]);
		}

		public void TestHouseBills()
		{
			AssertEquals("Precondition: No housebills", 0, OceanBill.HouseBills.Count);
			var newHouseBill = OceanBill.AddNewHouseBill(HouseBillRow);
			AssertNotNull(newHouseBill);
			AssertEquals(1, OceanBill.HouseBills.Count);
			AssertEquals(newHouseBill, OceanBill.HouseBills[0]);
		}

		public void TestVesselVoyage()
		{
			AssertEquals(VesselVoyage, OceanBill.VesselVoyage);
		}
	}
}
