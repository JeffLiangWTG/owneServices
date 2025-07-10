using System.Linq;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(AddressWrapperCollection))]
	sealed class AddressWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<AddressWrapperCollection>
	{
		#region TestAdditionalCapabilitiesForTypedStringIndexer

		public void TestAdditionalCapabilitiesForTypedStringIndexer()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("BENEATSCATS", Factory);
			OrgAddress addressMain = organisation.MainAddress;
			OrgAddress addressPickup = organisation.Addresses.AddNew(OrgAddressType.Pickup, true);
			OrgAddress addressPickupAndDelivery = organisation.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			OrgAddress addressSales = organisation.Addresses.AddNew(OrgAddressType.Sales, true);

			AddressWrapperCollection collection = new AddressWrapperCollection(organisation, Factory);

			AddressWrapper wrapperMain = null;
			AddressWrapper wrapperPickup = null;
			AddressWrapper wrapperPickupAndDelivery = null;
			AddressWrapper wrapperSales = null;
			foreach (AddressWrapper wrapper in collection)
			{
				OrgAddress addressBO = (OrgAddress)wrapper.WrappedObject;
				if (addressBO == addressMain)
				{
					wrapperMain = wrapper;
				}
				else if (addressBO == addressPickup)
				{
					wrapperPickup = wrapper;
				}
				else if (addressBO == addressPickupAndDelivery)
				{
					wrapperPickupAndDelivery = wrapper;
				}
				else if (addressBO == addressSales)
				{
					wrapperSales = wrapper;
				}
			}
			AssertNotNull("Precondition: Able to find wrapperMain", wrapperMain);
			AssertNotNull("Precondition: Able to find wrapperPickup", wrapperPickup);
			AssertNotNull("Precondition: Able to find wrapperPickupAndDelivery", wrapperPickupAndDelivery);
			AssertNotNull("Precondition: Able to find wrapperSales", wrapperSales);

			AssertEquals("collection[AddressTypeList.Codes.Main]", wrapperMain, collection[AddressTypeList.Codes.Main]);
			AssertEquals("collection[AddressTypeList.Codes.Pickup]", wrapperPickup, collection[AddressTypeList.Codes.Pickup]);
			AssertEquals("collection[AddressTypeList.Codes.Transport]", wrapperPickupAndDelivery, collection[AddressTypeList.Codes.Transport]);
			AssertEquals("collection[AddressTypeList.Codes.Delivery]", wrapperPickupAndDelivery, collection[AddressTypeList.Codes.Delivery]);
			AssertEquals("collection[AddressTypeList.Codes.Payables]", wrapperMain, collection[AddressTypeList.Codes.Payables]);
			AssertEquals("collection[AddressTypeList.Codes.Postal]", wrapperMain, collection[AddressTypeList.Codes.Postal]);
			AssertEquals("collection[AddressTypeList.Codes.Receivables]", wrapperMain, collection[AddressTypeList.Codes.Receivables]);
			AssertEquals("collection[AddressTypeList.Codes.Sales]", wrapperSales, collection[AddressTypeList.Codes.Sales]);
		}

		#endregion

		#region TestGetAddressesWithWarehousing

		public void TestGetAddressesWithWarehousing()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("BENEATSCATS", Factory);
			var addressWithWarehousing = organisation.MainAddress;
			var addressWithConstraints = organisation.Addresses.AddNew(OrgAddressType.Pickup, true);
			var addressWithNone = organisation.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);

			addressWithWarehousing.OA_DockLeveler = true;
			addressWithConstraints.OA_LoadingUnloadingConstraints = "yo";

			var addressWrappers = new AddressWrapperCollection(organisation, Factory);
			AssertEquals("Should contain the 3 org Addresses", 3, addressWrappers.Count);

			addressWrappers.Add(new AddressWrapper(addressWithWarehousing, ContactType.Consignee, Factory));
			AssertEquals("Should contain 4 Addresses, one duplicate so we can check distinct", 4, addressWrappers.Count);
			AssertContainsExactElementsInAnyOrder(new[] { addressWithWarehousing, addressWithConstraints }, addressWrappers.GetAddressesWithWarehousing().Cast<AddressWrapper>().Select(a => a.WrappedObject));
		}

		#endregion

		#region Implementation
		protected override AddressWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new AddressWrapperCollection(Factory.New<OrgHeader>(), Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new AddressWrapper(null, ContactType.All, Factory);
		}
		#endregion
	}
}
