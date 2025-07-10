using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContactWrapperCollection))]
	sealed class ContactWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<ContactWrapperCollection>
	{
		public void TestAdditionalCapabilitiesForTypedStringIndexer()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("MAIN1", Factory);
			OrgContact contactMain = OrgTestHelper.AddNewContact(organisation, ContactType.All, "MAIN");
			OrgContact contactPickup = OrgTestHelper.AddNewContact(organisation, ContactType.Consignor, "PICKUP");
			OrgContact contactDelivery = OrgTestHelper.AddNewContact(organisation, ContactType.Consignee, "DELIVERY");
			OrgContact contactSales = OrgTestHelper.AddNewContact(organisation, ContactType.Sales, "SALES");

			ContactWrapperCollection collection = new ContactWrapperCollection(organisation, Factory);

			ContactWrapper wrapperMain = null;
			ContactWrapper wrapperPickup = null;
			ContactWrapper wrapperDelivery = null;
			ContactWrapper wrapperSales = null;
			foreach (ContactWrapper wrapper in collection)
			{
				OrgContact contactBO = (OrgContact)wrapper.WrappedObject;
				if (contactBO == contactMain)
				{
					wrapperMain = wrapper;
				}
				else if (contactBO == contactPickup)
				{
					wrapperPickup = wrapper;
				}
				else if (contactBO == contactDelivery)
				{
					wrapperDelivery = wrapper;
				}
				else if (contactBO == contactSales)
				{
					wrapperSales = wrapper;
				}
			}
			AssertNotNull("Precondition: Able to find wrapperMain", wrapperMain);
			AssertNotNull("Precondition: Able to find wrapperPickup", wrapperPickup);
			AssertNotNull("Precondition: Able to find wrapperDelivery", wrapperDelivery);
			AssertNotNull("Precondition: Able to find wrapperSales", wrapperSales);

			AssertEquals(wrapperMain, collection[ContactTypeList.Codes.All]);
			AssertEquals(wrapperPickup, collection[ContactTypeList.Codes.Consignor]);
			AssertEquals(wrapperDelivery, collection[ContactTypeList.Codes.Consignee]);
			AssertEquals(wrapperSales, collection[ContactTypeList.Codes.Sales]);
			AssertEquals(null, collection["SomeoneWeDon'tKnow"]);
			AssertEquals(wrapperMain, collection[ContactTypeList.Codes.Warehouse]);
		}

		public void TestTypedStringIndexerIsCachedForContactsThatDontExist()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("EATME", Factory);
			ContactWrapperCollection collection = new ContactWrapperCollection(organisation, Factory);
			ContactWrapper warehouseWrapper = collection[ContactTypeList.Codes.Warehouse];
			AssertNotNull("warehouseWrapper, which doesn't actually exist in the collection", warehouseWrapper);
			AssertEquals("warehouseWrapper loaded a second time should get the same object", warehouseWrapper, collection[ContactTypeList.Codes.Warehouse]);
		}

		#region Implementation
		protected override ContactWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ContactWrapperCollection(Factory.New<OrgHeader>(), Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new ContactWrapper((OrgContact)null, Factory);
		}
		#endregion
	}
}
