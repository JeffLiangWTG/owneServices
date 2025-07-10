using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class AUAirCargoMasterDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.CusMAWB), new AUAirCargoMasterData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<CusMAWBCollection>("CollectionType", new AUAirCargoMasterData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.DocManagerCodes.AirCargoMaster, new AUAirCargoMasterData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Air Cargo Master", new AUAirCargoMasterData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new AUAirCargoMasterData().IsAllowedForUnallocatedeDocs);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.Customs.AU.AirCargo, new AUAirCargoMasterData().ModuleID);
		}
	}
}
