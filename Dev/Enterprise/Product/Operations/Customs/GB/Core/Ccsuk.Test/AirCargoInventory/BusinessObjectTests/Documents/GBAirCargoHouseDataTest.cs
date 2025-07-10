using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class GBAirCargoHouseDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.CusHAWB), new GBAirCargoHouseData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<CusHAWBCollectionNonDependent>("CollectionType", new GBAirCargoHouseData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.SupplyChainLogistics, new GBAirCargoHouseData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "CCSUK House Air Waybill", new GBAirCargoHouseData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new GBAirCargoHouseData().IsAllowedForUnallocatedeDocs);
		}

		public void TestAllowLookupOfBizOFromPk()
		{
			Assert("AllowLookupOfBizOFromPk", new GBAirCargoHouseData().AllowLookupOfBizOFromPk);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse, new GBAirCargoHouseData().ModuleID);
		}
	}
}
