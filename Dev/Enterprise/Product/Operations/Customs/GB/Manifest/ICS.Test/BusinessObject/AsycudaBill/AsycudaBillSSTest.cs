using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	[TestedType(typeof(AsycudaBillSS))]
	public class AsycudaBillSSTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestLookups()
		{
			var bill = Factory.New<AsycudaBillSS>();
			AssertEquals(typeof(AsycudaBillSSLookups), bill.Lookups.GetType());
		}

		public void TestPackType()
		{
			var bill = Factory.New<AsycudaBillSS>();
			AssertEquals(typeof(AsycudaPackSS), bill.GetPackType());
		}

		public void TestPartyOrgAddressValueClearWhenNewValueIsEmpty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var bill = header.Bills.AddNew();
			var org = GetTestOrgAddress();

			bill.ABL_OA_Shipper = org.PK;
			bill.ABL_OA_Consignee = org.PK;
			bill.ABL_OA_NotifyParty = org.PK;

			ValidatePartyOrgAddress(bill.ABL_ShipperNameInfo, bill.ABL_ShipperStreet1Info, bill.ABL_ShipperStreet2Info, bill.ABL_ShipperCityInfo, bill.ABL_ShipperStateInfo, bill.ABL_ShipperPostcodeInfo, bill.ABL_ShipperPhoneInfo, bill.ABL_RN_NKShipperCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");
			ValidatePartyOrgAddress(bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_ConsigneePhoneInfo, bill.ABL_RN_NKConsigneeCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");
			ValidatePartyOrgAddress(bill.ABL_NotifyPartyNameInfo, bill.ABL_NotifyPartyStreet1Info, bill.ABL_NotifyPartyStreet2Info, bill.ABL_NotifyPartyCityInfo, bill.ABL_NotifyPartyStateInfo, bill.ABL_NotifyPartyPostcodeInfo, bill.ABL_NotifyPartyPhoneInfo, bill.ABL_RN_NKNotifyPartyCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");

			bill.ABL_OA_Shipper = ZGuid.Empty;
			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.ABL_OA_NotifyParty = ZGuid.Empty;

			ValidatePartyOrgAddress(bill.ABL_ShipperNameInfo, bill.ABL_ShipperStreet1Info, bill.ABL_ShipperStreet2Info, bill.ABL_ShipperCityInfo, bill.ABL_ShipperStateInfo, bill.ABL_ShipperPostcodeInfo, bill.ABL_ShipperPhoneInfo, bill.ABL_RN_NKShipperCountryInfo,
								"", "", "", "", "", "", "", "");
			ValidatePartyOrgAddress(bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_ConsigneePhoneInfo, bill.ABL_RN_NKConsigneeCountryInfo,
								"", "", "", "", "", "", "", "");
			ValidatePartyOrgAddress(bill.ABL_NotifyPartyNameInfo, bill.ABL_NotifyPartyStreet1Info, bill.ABL_NotifyPartyStreet2Info, bill.ABL_NotifyPartyCityInfo, bill.ABL_NotifyPartyStateInfo, bill.ABL_NotifyPartyPostcodeInfo, bill.ABL_NotifyPartyPhoneInfo, bill.ABL_RN_NKNotifyPartyCountryInfo,
								"", "", "", "", "", "", "", "");
		}

		public void TestUpdateOnShipperPropagateToMasterBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var bill = (AsycudaBillSS)header.MasterBill;
			var org = GetTestOrgAddress();

			bill.ABL_OA_Shipper = org.PK;

			ValidatePartyOrgAddress(bill.ABL_ShipperNameInfo, bill.ABL_ShipperStreet1Info, bill.ABL_ShipperStreet2Info, bill.ABL_ShipperCityInfo, bill.ABL_ShipperStateInfo, bill.ABL_ShipperPostcodeInfo, bill.ABL_ShipperPhoneInfo, bill.ABL_RN_NKShipperCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");

			org.CompanyName = "FullName2";
			org.OA_Address1 = "Address12";
			org.OA_Address2 = "Address22";
			org.OA_City = "SIN2";
			org.OA_State = "STATE2";
			org.OA_PostCode = "00012";
			org.OA_Phone_Formatted = "+00123456789";
			org.OA_RN_NKCountryCode = "IT";

			AssertNotNull("Shipper", bill.Shipper);

			ValidatePartyOrgAddress(bill.ABL_ShipperNameInfo, bill.ABL_ShipperStreet1Info, bill.ABL_ShipperStreet2Info, bill.ABL_ShipperCityInfo, bill.ABL_ShipperStateInfo, bill.ABL_ShipperPostcodeInfo, bill.ABL_ShipperPhoneInfo, bill.ABL_RN_NKShipperCountryInfo,
								"FullName2", "Address12", "Address22", "SIN2", "STATE2", "00012", "+00123456789", "IT");
		}

		public void TestUpdateOnShipperPropagateToBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var bill = (AsycudaBillSS)header.Bills.AddNew();

			var org = GetTestOrgAddress();

			bill.ABL_OA_Shipper = org.PK;

			ValidatePartyOrgAddress(bill.ABL_ShipperNameInfo, bill.ABL_ShipperStreet1Info, bill.ABL_ShipperStreet2Info, bill.ABL_ShipperCityInfo, bill.ABL_ShipperStateInfo, bill.ABL_ShipperPostcodeInfo, bill.ABL_ShipperPhoneInfo, bill.ABL_RN_NKShipperCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");

			org.CompanyName = "FullName2";
			org.OA_Address1 = "Address12";
			org.OA_Address2 = "Address22";
			org.OA_City = "SIN2";
			org.OA_State = "STATE2";
			org.OA_PostCode = "00012";
			org.OA_Phone_Formatted = "+00123456789";
			org.OA_RN_NKCountryCode = "IT";

			AssertNotNull("Shipper", bill.Shipper);

			ValidatePartyOrgAddress(bill.ABL_ShipperNameInfo, bill.ABL_ShipperStreet1Info, bill.ABL_ShipperStreet2Info, bill.ABL_ShipperCityInfo, bill.ABL_ShipperStateInfo, bill.ABL_ShipperPostcodeInfo, bill.ABL_ShipperPhoneInfo, bill.ABL_RN_NKShipperCountryInfo,
								"FullName2", "Address12", "Address22", "SIN2", "STATE2", "00012", "+00123456789", "IT");
		}

		public void TestUpdateOnConsigneePropagateToMasterBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var bill = (AsycudaBillSS)header.MasterBill;
			var org = GetTestOrgAddress();

			bill.ABL_OA_Consignee = org.PK;

			ValidatePartyOrgAddress(bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_ConsigneePhoneInfo, bill.ABL_RN_NKConsigneeCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");

			org.CompanyName = "FullName2";
			org.OA_Address1 = "Address12";
			org.OA_Address2 = "Address22";
			org.OA_City = "SIN2";
			org.OA_State = "STATE2";
			org.OA_PostCode = "00012";
			org.OA_Phone_Formatted = "+00123456789";
			org.OA_RN_NKCountryCode = "IT";

			AssertNotNull("Consignee", bill.Consignee);

			ValidatePartyOrgAddress(bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_ConsigneePhoneInfo, bill.ABL_RN_NKConsigneeCountryInfo,
								"FullName2", "Address12", "Address22", "SIN2", "STATE2", "00012", "+00123456789", "IT");
		}

		public void TestUpdateOnConsigneePropagateToBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var bill = (AsycudaBillSS)header.Bills.AddNew();
			var org = GetTestOrgAddress();

			bill.ABL_OA_Consignee = org.PK;

			ValidatePartyOrgAddress(bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_ConsigneePhoneInfo, bill.ABL_RN_NKConsigneeCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");

			org.CompanyName = "FullName2";
			org.OA_Address1 = "Address12";
			org.OA_Address2 = "Address22";
			org.OA_City = "SIN2";
			org.OA_State = "STATE2";
			org.OA_PostCode = "00012";
			org.OA_Phone_Formatted = "+00123456789";
			org.OA_RN_NKCountryCode = "IT";

			AssertNotNull("Consignee", bill.Consignee);

			ValidatePartyOrgAddress(bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_ConsigneePhoneInfo, bill.ABL_RN_NKConsigneeCountryInfo,
								"FullName2", "Address12", "Address22", "SIN2", "STATE2", "00012", "+00123456789", "IT");
		}

		public void TestUpdateOnNotifyPartyPropagateToMasterBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var bill = (AsycudaBillSS)header.MasterBill;
			var org = GetTestOrgAddress();

			bill.ABL_OA_NotifyParty = org.PK;

			ValidatePartyOrgAddress(bill.ABL_NotifyPartyNameInfo, bill.ABL_NotifyPartyStreet1Info, bill.ABL_NotifyPartyStreet2Info, bill.ABL_NotifyPartyCityInfo, bill.ABL_NotifyPartyStateInfo, bill.ABL_NotifyPartyPostcodeInfo, bill.ABL_NotifyPartyPhoneInfo, bill.ABL_RN_NKNotifyPartyCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");

			org.CompanyName = "FullName2";
			org.OA_Address1 = "Address12";
			org.OA_Address2 = "Address22";
			org.OA_City = "SIN2";
			org.OA_State = "STATE2";
			org.OA_PostCode = "00012";
			org.OA_Phone_Formatted = "+00123456789";
			org.OA_RN_NKCountryCode = "IT";

			AssertNotNull("NotifyParty", bill.NotifyParty);

			ValidatePartyOrgAddress(bill.ABL_NotifyPartyNameInfo, bill.ABL_NotifyPartyStreet1Info, bill.ABL_NotifyPartyStreet2Info, bill.ABL_NotifyPartyCityInfo, bill.ABL_NotifyPartyStateInfo, bill.ABL_NotifyPartyPostcodeInfo, bill.ABL_NotifyPartyPhoneInfo, bill.ABL_RN_NKNotifyPartyCountryInfo,
								"FullName2", "Address12", "Address22", "SIN2", "STATE2", "00012", "+00123456789", "IT");
		}

		public void TestUpdateOnNotifyPartyPropagateToBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var bill = (AsycudaBillSS)header.Bills.AddNew();
			var org = GetTestOrgAddress();

			bill.ABL_OA_NotifyParty = org.PK;

			ValidatePartyOrgAddress(bill.ABL_NotifyPartyNameInfo, bill.ABL_NotifyPartyStreet1Info, bill.ABL_NotifyPartyStreet2Info, bill.ABL_NotifyPartyCityInfo, bill.ABL_NotifyPartyStateInfo, bill.ABL_NotifyPartyPostcodeInfo, bill.ABL_NotifyPartyPhoneInfo, bill.ABL_RN_NKNotifyPartyCountryInfo,
								"FullName", "Address1", "Address2", "SIN", "STATE", "0001", "+00000000000", "GB");

			org.CompanyName = "FullName2";
			org.OA_Address1 = "Address12";
			org.OA_Address2 = "Address22";
			org.OA_City = "SIN2";
			org.OA_State = "STATE2";
			org.OA_PostCode = "00012";
			org.OA_Phone_Formatted = "+00123456789";
			org.OA_RN_NKCountryCode = "IT";

			AssertNotNull("NotifyParty", bill.NotifyParty);

			ValidatePartyOrgAddress(bill.ABL_NotifyPartyNameInfo, bill.ABL_NotifyPartyStreet1Info, bill.ABL_NotifyPartyStreet2Info, bill.ABL_NotifyPartyCityInfo, bill.ABL_NotifyPartyStateInfo, bill.ABL_NotifyPartyPostcodeInfo, bill.ABL_NotifyPartyPhoneInfo, bill.ABL_RN_NKNotifyPartyCountryInfo,
								"FullName2", "Address12", "Address22", "SIN2", "STATE2", "00012", "+00123456789", "IT");
		}

		public void TestPopulateSpecialMentions()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			var bill = (AsycudaBillSS)header.Bills.AddNew();
			AssertEquals("SpecialMentions empty by default", ZString.Empty, bill.SpecialMentions);

			var address = GetTestOrgAddress();
			bill.ABL_OA_NotifyParty = address.PK;
			AssertEquals("SpecialMentions is 10600", "10600", bill.SpecialMentions);

			var bill2 = (AsycudaBillSS)header.Bills.AddNew();
			bill2.ABL_NotifyPartyName = "Name";
			AssertEquals("SpecialMentions empty with name only", ZString.Empty, bill2.SpecialMentions);
			bill2.ABL_NotifyPartyStreet1 = "Address1";
			AssertEquals("SpecialMentions empty with name and address1", ZString.Empty, bill2.SpecialMentions);
			bill2.ABL_NotifyPartyStreet2 = "Address2";
			AssertEquals("SpecialMentions empty with name, address1 and address2", ZString.Empty, bill2.SpecialMentions);
			bill2.ABL_NotifyPartyCity = "CI";
			AssertEquals("SpecialMentions empty with name, address1, address2 and city", ZString.Empty, bill2.SpecialMentions);
			bill2.ABL_RN_NKNotifyPartyCountry = "GB";
			AssertEquals("SpecialMentions empty with name, address1, address2, city and country", ZString.Empty, bill2.SpecialMentions);
			bill2.ABL_NotifyPartyPostcode = "AB1CD2";
			AssertEquals("SpecialMentions is 10600", "10600", bill.SpecialMentions);
		}

		void ValidatePartyOrgAddress(ZPropertyInfo nameInfo, ZPropertyInfo address1Info, ZPropertyInfo address2Info, ZPropertyInfo cityInfo, ZPropertyInfo stateInfo, ZPropertyInfo postCodeInfo, ZPropertyInfo phoneInfo, ZPropertyInfo countryInfo,
			ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString phone, ZString country)
		{
			AssertEquals($"{nameInfo.Name}, expected: {name}", name, nameInfo.Value);
			AssertEquals($"{address1Info.Name}, expected: {address1}", address1, address1Info.Value);
			AssertEquals($"{address2Info.Name}, expected: {address2}", address2, address2Info.Value);
			AssertEquals($"{cityInfo.Name}, expected: {city}", city, cityInfo.Value);
			AssertEquals($"{stateInfo.Name}, expected: {state}", state, stateInfo.Value);
			AssertEquals($"{postCodeInfo.Name}, expected: {postCode}", postCode, postCodeInfo.Value);
			AssertEquals($"{phoneInfo.Name}, expected: {phone}", phone, phoneInfo.Value);
			AssertEquals($"{countryInfo.Name}, expected: {country}", country, countryInfo.Value);
		}

		OrgAddress GetTestOrgAddress()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.CompanyName = "FullName";
			org.OA_Address1 = "Address1";
			org.OA_Address2 = "Address2";
			org.OA_City = "SIN";
			org.OA_State = "STATE";
			org.OA_PostCode = "0001";
			org.OA_Phone_Formatted = "+00000000000";
			org.OA_RN_NKCountryCode = "GB";
			return org;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeaderSS>();
			header.SuspendCheckBusinessObjectType();
			var bill = header.Bills.AddNew();
			return bill;
		}
	}
}
