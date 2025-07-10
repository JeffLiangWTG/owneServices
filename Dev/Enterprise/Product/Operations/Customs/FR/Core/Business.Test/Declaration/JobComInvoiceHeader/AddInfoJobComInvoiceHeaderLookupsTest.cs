using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public sealed class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestValuationMethodList()
		{
			var valuationMethodList = lookups.ValuationMethodList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "1, 2, 3, 4, 5, 6", valuationMethodList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<ValuationMethodList>(), valuationMethodList);
			});
		}
		public void TestIncotermCountriesList()
		{
			var list = lookups.IncotermCountries;
			AssertType<RefCountryCollection>(list);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var addInfoJobComInvoiceHeader = new AddInfoJobComInvoiceHeader(Factory.New<JobComInvoiceHeader>().JZ_AddInfoInfo);
			lookups = new AddInfoJobComInvoiceHeaderLookups(addInfoJobComInvoiceHeader);
		}

		AddInfoJobComInvoiceHeaderLookups lookups;
	}
}
