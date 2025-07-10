using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(Extension))]
public sealed class ExtensionTest : TestCaseWithFactory
{
	public void TestBillGetConsigneeAddress()
	{
		var bill = Factory.New<AsycudaBill>();
		bill.ABL_ConsigneeName = "Consignee Name";
		bill.ABL_ConsigneeStreet1 = "Consignee Street 1";
		bill.ABL_ConsigneeStreet2 = "Street 2 ";
		bill.ABL_RN_NKConsigneeCountry = "CN";
		bill.ABL_ConsigneePostcode = "510000";
		bill.ABL_ConsigneeCity = "ConsigneeGuangzhou";

		var consigneeAddress = bill.GetConsigneeAddress();
		CombineAssertions(() =>
		{
			AssertType<AddressWrapper>(consigneeAddress);
			AssertEquals("Consignee Name", consigneeAddress.Name);
			AssertEquals("Consignee Street 1 Street 2", consigneeAddress.StreetAndNumber);
			AssertEquals("CN", consigneeAddress.Country);
			AssertEquals("510000", consigneeAddress.ZipCode);
			AssertEquals("ConsigneeGuangzhou", consigneeAddress.City);
		});
	}

	public void TestBillGetShipperAddress()
	{
		var bill = Factory.New<AsycudaBill>();
		bill.ABL_ShipperName = "Shipper Name";
		bill.ABL_ShipperStreet1 = "Shipper Street 1";
		bill.ABL_ShipperStreet2 = "Street 2 ";
		bill.ABL_RN_NKShipperCountry = "AU";
		bill.ABL_ShipperPostcode = "2000";
		bill.ABL_ShipperCity = "ShipperSydney";

		var shipperAddress = bill.GetShipperAddress();
		CombineAssertions(() =>
		{
			AssertType<AddressWrapper>(shipperAddress);
			AssertEquals("Shipper Name", shipperAddress.Name);
			AssertEquals("Shipper Street 1 Street 2", shipperAddress.StreetAndNumber);
			AssertEquals("AU", shipperAddress.Country);
			AssertEquals("2000", shipperAddress.ZipCode);
			AssertEquals("ShipperSydney", shipperAddress.City);
		});
	}
}
