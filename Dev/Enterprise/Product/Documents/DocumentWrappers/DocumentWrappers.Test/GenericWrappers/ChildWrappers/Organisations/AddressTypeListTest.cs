using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class AddressTypeListTest : TestCaseWithFactory
	{
		public void TestGetAddressUsingFallbackIfTypeCodeRecognised()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("MROWRRR", Factory);
			OrgAddress addressMain = organisation.MainAddress;
			AddressTypeList list = new AddressTypeList();
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Main));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Delivery));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Transport));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Pickup));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Receivables));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Payables));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Postal));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Sales));
			AssertEquals(null, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, "IDon'tKnowWhySometimesIKeptTry'en"));
			AssertEquals(null, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, ""));

			OrgAddress addressPickup = organisation.Addresses.AddNew(OrgAddressType.Pickup, true);
			OrgAddress addressPickupAndDelivery = organisation.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			OrgAddress addressSales = organisation.Addresses.AddNew(OrgAddressType.Sales, true);

			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Main));
			AssertEquals(addressPickupAndDelivery, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Delivery));
			AssertEquals(addressPickupAndDelivery, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Transport));
			AssertEquals(addressPickup, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Pickup));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Receivables));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Payables));
			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Postal));
			AssertEquals(addressSales, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Sales));
			AssertEquals(null, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, "IDon'tKnowWhySometimesIKeptTry'en"));
			AssertEquals(null, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, ""));

			OrgAddress addressDelivery = organisation.Addresses.AddNew(OrgAddressType.Delivery, true);
			OrgAddress addressReceivables = organisation.Addresses.AddNew(OrgAddressType.Receivables, true);
			OrgAddress addressPayables = organisation.Addresses.AddNew(OrgAddressType.Payables, true);
			OrgAddress addressPostal = organisation.Addresses.AddNew(OrgAddressType.Postal, true);

			AssertEquals(addressMain, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Main));
			AssertEquals(addressDelivery, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Delivery));
			AssertEquals(addressPickupAndDelivery, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Transport));
			AssertEquals(addressPickup, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Pickup));
			AssertEquals(addressReceivables, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Receivables));
			AssertEquals(addressPayables, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Payables));
			AssertEquals(addressPostal, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Postal));
			AssertEquals(addressSales, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, AddressTypeList.Codes.Sales));
			AssertEquals(null, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, "IDon'tKnowWhySometimesIKeptTry'en"));
			AssertEquals(null, list.GetAddressUsingFallbackIfTypeCodeRecognised(organisation, ""));
		}
	}
}
