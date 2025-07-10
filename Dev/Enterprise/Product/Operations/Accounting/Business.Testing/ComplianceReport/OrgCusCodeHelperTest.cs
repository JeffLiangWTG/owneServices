using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport
{
	public class OrgCusCodeHelperTest : TestCaseWithFactory
	{
		public void TestGetConsumptionTaxRegistrationCodeForAllCountries()
		{
			var orgCusCodeHelper = new OrgCusCodeHelper();
			var countryCollection = new RefCountryCollection(Factory);
			AssertEquals("Wrong number of CusCodes for VAT ID", countryCollection.Count, orgCusCodeHelper.ConsumptionTaxRegistrationCodeForAllCountriesDataTable.Rows.Count);
		}

		public void TestGetConsumptionTaxRegistrationCodeForEUCountriesExcludingOne()
		{
			var orgCusCodeHelper = new OrgCusCodeHelper();
			var countryCollection = new RefCountryCollection(Factory);
			var countryCollectionEUexcludingDe = countryCollection.Where(v => v.IsPartOfEuropeanUnion && v.Code != Constants.CountryCodes.Germany);
			var consumptiontaxRegistrationEUexcludingDe = orgCusCodeHelper.GetConsumptionTaxRegistrationCodeForEUCountriesExcludingOne(Constants.CountryCodes.Germany);
			AssertEquals("Wrong number of CusCodes for EU VAT IDs", countryCollectionEUexcludingDe.Count(), consumptiontaxRegistrationEUexcludingDe.Rows.Count);

			var expected = new List<(string countryCode, string taxRegCode)>();
			countryCollectionEUexcludingDe.ForEach(z => expected.Add((z.Code, z.ConsumptionTaxRegistrationCode)));
			var actual = new List<(string countryCode, string taxRegCode)>();
			foreach (DataRow row in consumptiontaxRegistrationEUexcludingDe.Rows)
			{
				actual.Add((row["Country"].ToString(), row["BusinessRegType"].ToString()));
			}
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}
	}
}
