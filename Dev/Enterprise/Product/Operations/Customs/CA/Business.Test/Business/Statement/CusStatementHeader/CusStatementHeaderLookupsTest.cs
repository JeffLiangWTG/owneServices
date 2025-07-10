using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusStatementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatementTypes()
		{
			var cachedList = Factory.GetCachedValue<CusStatementHeaderTypes>();

			var header = Factory.New<CusStatementHeader>();
			var list = header.Lookups.StatementTypes;

			AssertSame(cachedList, list);
		}

		public void TestCARMSOAConvertedStatementTypes()
		{
			var cachedList = Factory.GetCachedValue<CARMStatementOfAccountStatementTypeList>();

			var header = Factory.New<CusStatementHeader>();
			var list = header.Lookups.CARMSOAConvertedStatementTypes;

			AssertSame(cachedList, list);
		}
	}
}
