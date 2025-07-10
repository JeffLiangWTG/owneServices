
using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class EUTaxBusinessRegistrationCodesTest : ScriptTest
	{
		public void TestEUTaxBusinessRegistrationCodes()
		{
			RefCountryCollection countryCollection = new RefCountryCollection(Factory);

			DataTable result = RunScript();
			result.PrimaryKey = new DataColumn[] { result.Columns[0] };

			foreach (RefCountry country in countryCollection)
			{
				if (country.RN_EconomicGrouping == "EUN")
				{
					string orgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(country.RN_Code);

					DataRow row = result.Rows.Find(country.RN_Code);

					AssertEquals(row["CountryCode"], country.RN_Code);
					AssertEquals(row["TaxBusinessRegistrationCode"], orgCusCode);
				}
			}
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM EUTaxBusinessRegistrationCodes ()  
"
			));
		}
	}
}

