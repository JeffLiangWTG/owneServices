using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class AUAirCargoHouseDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.CusHAWB), new AUAirCargoHouseData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<CusHAWBCollectionNonDependent>("CollectionType", new AUAirCargoHouseData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.All, new AUAirCargoHouseData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Air Cargo House", new AUAirCargoHouseData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new AUAirCargoHouseData().IsAllowedForUnallocatedeDocs);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.Customs.AU.HouseAirCargo, new AUAirCargoHouseData().ModuleID);
		}
	}
}
