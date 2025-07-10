using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	class CusStatementHeaderAssemblyDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.BaseCusStatementHeader), new CusStatementHeaderAssemblyData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<CusStatementHeaderCollection>("CollectionType", new CusStatementHeaderAssemblyData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.SupplyChainLogistics, new CusStatementHeaderAssemblyData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Statement Header", new CusStatementHeaderAssemblyData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new CusStatementHeaderAssemblyData().IsAllowedForUnallocatedeDocs);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.Customs.EU.FR.CustomsStatement, new CusStatementHeaderAssemblyData().ModuleID);
		}
	}
}
