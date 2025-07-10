using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	class CACusStatementHeaderDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.BaseCusStatementHeader), new CACusStatementHeaderData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertEquals("CollectionType", null, new CACusStatementHeaderData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.SupplyChainLogistics, new CACusStatementHeaderData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "DN/SOA Statements", new CACusStatementHeaderData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new CACusStatementHeaderData().IsAllowedForUnallocatedeDocs);
		}
	}
}
