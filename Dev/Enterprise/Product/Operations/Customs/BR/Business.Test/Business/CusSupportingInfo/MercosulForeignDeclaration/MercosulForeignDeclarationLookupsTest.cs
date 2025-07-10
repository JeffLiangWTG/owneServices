using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class MercosulForeignDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMercosulCountriesList()
		{
			var parent = Factory.New<MercosulForeignDeclaration>();
			var mercosulCountriesList = parent.Lookups.MercosulCountriesList;
			AssertEquals(4, mercosulCountriesList.Count);
			AssertEquals("AR, BR, PY, UY", mercosulCountriesList.CodesAsString);
		}
	}
}
