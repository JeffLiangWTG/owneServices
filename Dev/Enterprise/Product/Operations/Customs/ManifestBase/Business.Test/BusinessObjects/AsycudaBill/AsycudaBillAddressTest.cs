using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaBillAddress))]
	sealed class AsycudaBillAddressTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPostAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			var postalAddress = orgHeader.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = address.PK;
			var asycudaBillAddress = new AsycudaBillAddress(AsycudaBillAddress.AddressType.Consignee, bill.ABL_OA_ConsigneeInfo, bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_RN_NKConsigneeCountryInfo, bill.ABL_ConsigneePhoneInfo);
			AssertEquals(postalAddress, asycudaBillAddress.PostalAddress);
		}

		public void TestAsycudaBillAddressIntialization()
		{
			var asycudaBillAddress = (AsycudaBillAddress)GetNewBusinessObject();
			AssertEquals(((IManifestBillAddress)asycudaBillAddress).OA_AddressInfo.Value, bill.ABL_OA_Consignee);
			AssertEquals(AsycudaBillAddress.AddressType.Consignee, asycudaBillAddress.AsycudaBillAddressType);
			AssertEquals(asycudaBillAddress.CompanyName, "TEST");
			AssertEquals(asycudaBillAddress.Address1, "1");
			AssertEquals(asycudaBillAddress.Address2, "2");
			AssertEquals(asycudaBillAddress.City, "abc");
			AssertEquals(asycudaBillAddress.State, "xy");
			AssertEquals(asycudaBillAddress.Postcode, "2341");
			AssertEquals(asycudaBillAddress.RN_NKCountryCode, "GB");
			AssertEquals(asycudaBillAddress.Phone, "756");

			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.ABL_ConsigneeName = "BOB";
			bill.ABL_ConsigneeStreet1 = "STREET 1";
			bill.ABL_ConsigneeStreet2 = "STREET 2";
			bill.ABL_ConsigneeCity = "CT";
			bill.ABL_ConsigneeState = "ST";
			bill.ABL_ConsigneePostcode = "4343";
			bill.ABL_RN_NKConsigneeCountry = "NZ";
			bill.ABL_ConsigneePhone = "+7554554";
			AssertEquals(AsycudaBillAddress.AddressType.Consignee, asycudaBillAddress.AsycudaBillAddressType);
			AssertEquals(asycudaBillAddress.CompanyName, "BOB");
			AssertEquals(asycudaBillAddress.Address1, "STREET 1");
			AssertEquals(asycudaBillAddress.Address2, "STREET 2");
			AssertEquals(asycudaBillAddress.City, "CT");
			AssertEquals(asycudaBillAddress.State, "ST");
			AssertEquals(asycudaBillAddress.Postcode, "4343");
			AssertEquals(asycudaBillAddress.RN_NKCountryCode, "NZ");
			AssertEquals(asycudaBillAddress.Phone, "+7554554");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "TEST";
			address.OA_OH = orgHeader.PK;

			address.Address1 = "1";
			address.Address2 = "2";
			address.City = "abc";
			address.State = "xy";
			address.Postcode = "2341";
			address.OA_RN_NKCountryCode = "GB";
			address.OA_Phone = "756";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = address.PK;

			AsycudaBillAddress asycudaBillAddress = new AsycudaBillAddress(AsycudaBillAddress.AddressType.Consignee, bill.ABL_OA_ConsigneeInfo, bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_RN_NKConsigneeCountryInfo, bill.ABL_ConsigneePhoneInfo);

			return asycudaBillAddress;
		}

		AsycudaBill bill;
	}
}
