using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.DataPurge
{
	class RepositoryManager
	{
		public IEnumerable<string> GetSytemLevelPurgeScripts(DbConnection connection, bool includeRating, bool includeProduct, bool includeOrganisation, bool includeChargeCode, bool includeTariff, bool includeQuotations)
		{
			var repositories = new List<ScriptRepository>();

			repositories.Add(new OperationalScriptRepository());

			if (includeRating)
			{
				repositories.Add(new RatingScriptRepository());
			}

			if (includeQuotations)
			{
				repositories.Add(new QuotationsScriptRepsitory());
			}

			if (includeProduct)
			{
				repositories.Add(new ProductsScriptRepository());
			}

			if (includeOrganisation)
			{
				repositories.Add(new NonProxyOrganizationsScriptRepository(connection));
			}

			if (includeChargeCode)
			{
				repositories.Add(new NonSystemChargeCodesScriptRepository());
			}

			if (includeTariff)
			{
				repositories.Add(new TariffScriptRepository());
			}

			repositories.Add(new FinalSystemScriptRepository());

			foreach (var repository in repositories)
			{
				foreach (var script in repository.GetPurgeScripts())
				{
					yield return script;
				}
			}
		}

		/// <summary>
		/// To get the actual script this method is used to replace {0} by the Company PK.
		/// Compose the company specific purge script inserting the selected Company PK variable declaration and initialisation.
		/// </summary>
		public string GetPurgeCompanySpecificScript(ZGuid companySpecificPk)
		{
			var result = string.Empty;

			if (companySpecificPk.IsValid)
			{
				var companyScriptRepository = new CompanySpecificScriptRepository();
				var builder = new StringBuilder();

				foreach (string script in companyScriptRepository.GetPurgeScripts())
				{
					builder.AppendLine(script);
				}

				string rawSqlText = "DECLARE @CompanyPk uniqueidentifier; SET {0} = '{1}'\r\n" + builder.ToString();
				result = String.Format(rawSqlText, "@CompanyPk", companySpecificPk.ToString());
			}

			return result;
		}
	}
}
