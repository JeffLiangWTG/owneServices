using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public sealed class OrgCusCodeHelper
	{
		public OrgCusCodeHelper()
		{ }

		public DataTable ConsumptionTaxRegistrationCodeForAllCountriesDataTable => consumptionTaxRegistrationCodeForAllCountriesDataTable ??= GetConsumptionTaxRegistrationCodeForAllCountrieseAsDataTable();
		DataTable consumptionTaxRegistrationCodeForAllCountriesDataTable;
		IEnumerable<(string CountryCode, (string ConsumptionTaxRegistrationCode, bool IsPartOfEuropeanUnion))> ConsumptionTaxRegistrationCodeForAllCountries => consumptionTaxRegistrationCodeForAllCountries ??= GetConsumptionTaxRegistrationCodeForAllCountries();
		IEnumerable<(string CountryCode, (string ConsumptionTaxRegistrationCode, bool IsPartOfEuropeanUnion))> consumptionTaxRegistrationCodeForAllCountries;

		public DataTable GetConsumptionTaxRegistrationCodeForEUCountriesExcludingOne(string excludeCountry)
		{
			var taxRegistrationEUCountries = new List<(string CountryCode, string ConsumptionTaxRegistrationCode)>();
			ConsumptionTaxRegistrationCodeForAllCountries.Where(x => x.Item2.IsPartOfEuropeanUnion && x.CountryCode != excludeCountry).ForEach(y => taxRegistrationEUCountries.Add((y.CountryCode, y.Item2.ConsumptionTaxRegistrationCode)));
			var dataTable = CountryAndBusinessRegistrationTypeToTable(taxRegistrationEUCountries);

			return dataTable;
		}

		IEnumerable<(string CountryCode, (string ConsumptionTaxRegistrationCode, bool IsPartOfEuropeanUnion))> GetConsumptionTaxRegistrationCodeForAllCountries()
		{
			var factory = new BusinessObjectFactory();
			var allCountries = new RefCountryCollection(factory);
			var taxRegistrationAllCountries = new List<(string CountryCode, (string ConsumptionTaxRegistrationCode, bool IsPartOfEuropeanUnion))>();
			allCountries.ForEach(y => taxRegistrationAllCountries.Add((y.Code, (y.ConsumptionTaxRegistrationCode, y.IsPartOfEuropeanUnion))));

			return taxRegistrationAllCountries;
		}

		DataTable GetConsumptionTaxRegistrationCodeForAllCountrieseAsDataTable()
		{
			var taxRegistrationAllCountries = new List<(string CountryCode, string ConsumptionTaxRegistrationCode)>();
			ConsumptionTaxRegistrationCodeForAllCountries.ForEach(x => taxRegistrationAllCountries.Add((x.CountryCode, x.Item2.ConsumptionTaxRegistrationCode)));
			var dataTable = CountryAndBusinessRegistrationTypeToTable(taxRegistrationAllCountries);

			return dataTable;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Column name")]
		DataTable CountryAndBusinessRegistrationTypeToTable(IEnumerable<(string CountryCode, string ConsumptionTaxRegistrationCode)> enumerable)
		{
			var result = new DataTable();
			result.Locale = CultureInfo.InvariantCulture;
			result.Columns.Add("Country", typeof(string));
			result.Columns.Add("BusinessRegType", typeof(string));

			foreach (var entry in enumerable)
			{
				var row = result.NewRow();
				row["Country"] = entry.CountryCode;
				row["BusinessRegType"] = entry.ConsumptionTaxRegistrationCode;
				result.Rows.Add(row);
			}

			return result;
		}
	}
}
