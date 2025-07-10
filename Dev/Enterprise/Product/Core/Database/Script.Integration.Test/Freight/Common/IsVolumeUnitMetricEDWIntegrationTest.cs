using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using CargoWise.Data;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	class IsVolumeUnitMetricEDWIntegrationTest : TransactionedTestCase
	{
		public void TestIsVolumeUnitMetric()
		{
			List<string> errors = new List<string>();

			foreach (FieldInfo info in typeof(Constants.Volume).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (info.FieldType != typeof(string))
				{
					continue;
				}

				string code = (string)info.GetValue(null);

				string expected = Constants.Volume.IsImperial(code) ? "Imperial" : "Metric";
				string actualEDW = IsVolumeUnitMetricEDW(code);

				if (actualEDW != expected)
				{
					errors.Add(string.Format("{0} ({1}) - In code: {2}, but in edw sql: {3}", code, Constants.Volume.GetDescription(code, Constants.PluralState.Plural), expected, actualEDW));
				}
			}

			Dictionary<string, IList<string>> results = new Dictionary<string, IList<string>>();
			if (errors.Count > 0)
			{
				results.Add("Missmatches", errors);
			}

			AssertGroupedErrorList(results);
		}

		#region Implementation

		string IsVolumeUnitMetricEDW(string unitOfVolume)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"select value from [{0}].dbo.IsVolumeUnitMetric(@unitOfVolume)", Db.EdwDatabaseName);
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@unitOfVolume", SqlDbType.VarChar, unitOfVolume);
				return (int)command.ExecuteScalar() == 0 ? "Imperial" : "Metric";
			}
		}

		#endregion
	}
}

