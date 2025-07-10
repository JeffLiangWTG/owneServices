using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Intrastat.Business.Testing;

namespace Enterprise.Customs.DE.Intrastat.Business.Testing
{
	sealed class CusIntrastatLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRegions_Import()
		{
			transactionLine.Header.CIH_CountryOfSupply = Core.Constants.CountryCodes.France;
			AssertType<FederalStateList>(lookups.Regions);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 99", ((FederalStateList)lookups.Regions).CodesAsString);
		}

		public void TestRegions_Export()
		{
			transactionLine.Header.CIH_CountryOfSupply = Core.Constants.CountryCodes.Germany;
			AssertType<FederalStateList>(lookups.Regions);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 25", ((FederalStateList)lookups.Regions).CodesAsString);
		}

		protected override void SetUp()
		{
			var helper = IntrastatTestDataHelper.New(Factory);
			var header = helper.NewCusIntrastatHeaderWithValidData<CusIntrastatHeader>();
			transactionLine = (CusIntrastatLine)IntrastatTestDataHelper.New(Factory).NewCusIntrastatLineWithValidData(header);
			lookups = transactionLine.Lookups;
		}

		CusIntrastatLineLookups lookups;
		CusIntrastatLine transactionLine;
	}
}
