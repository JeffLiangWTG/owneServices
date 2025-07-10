using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Data;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	class IsVolumeUnitMetricIntegrationTest : TransactionedTestCase
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
				string actual = IsVolumeUnitMetric(code);

				if (actual != expected)
				{
					errors.Add(string.Format("{0} ({1}) - In code: {2}, but in sql: {3}", code, Constants.Volume.GetDescription(code, Constants.PluralState.Plural), expected, actual));
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

		string IsVolumeUnitMetric(string unitOfVolume)
		{
			const string sql = "select value from dbo.IsVolumeUnitMetric(@unitOfVolume)";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@unitOfVolume", SqlDbType.VarChar, unitOfVolume);
				return (int)command.ExecuteScalar() == 0 ? "Imperial" : "Metric";
			}
		}
		#endregion
	}
}

