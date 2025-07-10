using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHItemCollection))]
	sealed class CusCAeMHItemCollectionTest : ActiveBusinessObjectCollectionTestCase<CusCAeMHItemCollection>
	{
		public void TestAllocateLineNumber()
		{
			var house = Factory.New<CusCAeMHMaster>().HouseBills.AddNew();
			var item = house.Items.AddNew();
			AssertEquals((short)1, item.BX_LineNumber);
			item = house.Items.AddNew();
			AssertEquals((short)2, item.BX_LineNumber);
		}

		public void TestIOverrideDefaultValuesCollectionMembers()
		{
			var house = Factory.New<CusCAeMHMaster>().HouseBills.AddNew();
			house.BW_OverrideFreightDefaults = true;
			IOverrideDefaultValuesCollection collection = house.Items;
			AssertEquals(false, collection.IsOverrideDefaultValuesEnabled);
			var shipment = Factory.New<ForwardingShipment>();
			house.BW_ParentID = shipment.PK;
			AssertEquals(true, collection.IsOverrideDefaultValuesEnabled);

			var info = collection.OverrideDefaultValuesInfo;
			AssertEquals("BW_OverrideFreightDefaults", info.Name);
			AssertEquals(true, info.Value);

			house.BW_OverrideFreightDefaults = false;
			AssertEquals(false, info.Value);
		}

		protected override CusCAeMHItemCollection GetCollectionToTest()
		{
			return new CusCAeMHItemCollection(House);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return House.Items.AddNew();
		}

		CusCAeMHHouse House
		{
			get { return fHouse ?? (fHouse = Factory.New<CusCAeMHMaster>().HouseBills.AddNew()); }
		}
		CusCAeMHHouse fHouse;
	}
}
