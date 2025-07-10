using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	internal class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCurrencyList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var lookups = entryHeader.Lookups;
			AssertType<RefCurrencyCollection>(lookups.CurrencyList);
		}
	}
}
