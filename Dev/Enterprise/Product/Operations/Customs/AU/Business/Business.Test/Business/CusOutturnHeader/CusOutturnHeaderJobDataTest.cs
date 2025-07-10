using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnHeaderJobDataTest : AssemblyDataTest
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(CusOutturnHeader), new CusOutturnHeaderJobData().BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(typeof(CusOutturnHeaderCollection), new CusOutturnHeaderJobData().GetBusinessObjectCollection(Factory).GetType());
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.AU.SeaCargoDepot, new CusOutturnHeaderJobData().ModuleID);
		}

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, new CusOutturnHeaderJobData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Sea Cargo Outturn", new CusOutturnHeaderJobData().HumanReadableName.ToString());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, new CusOutturnHeaderJobData().IsAllowedForUnallocatedeDocs);
		}
	}
}
