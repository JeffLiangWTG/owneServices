using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Data;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	internal class ConvertWeightIntegrationTest : TransactionedTestCase
	{
		public void TestCheckForConversionMissmatchs()
		{
			List<string> errors = new List<string>();

			foreach (FieldInfo infoFrom in typeof(Constants.Weight).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (infoFrom.FieldType != typeof(string))
				{
					continue;
				}

				foreach (FieldInfo infoTo in typeof(Constants.Weight).GetFields(BindingFlags.Public | BindingFlags.Static))
				{
					if (infoTo.FieldType != typeof(string))
					{
						continue;
					}

					string codeFrom = (string)infoFrom.GetValue(null);
					string codeTo = (string)infoTo.GetValue(null);

					decimal value = 1;
					decimal expected = Constants.Weight.Convert(value, codeFrom, codeTo);

					while (expected < 1000)
					{
						value *= 10;
						expected = Constants.Weight.Convert(value, codeFrom, codeTo);
					}

					if (value <= 10000000000000)
					{
						expected = Math.Round(expected, 6, MidpointRounding.AwayFromZero);
						decimal actual = Math.Round(ConvertWeight(value, codeFrom, codeTo), 6);

						if (actual == 0)
						{
							errors.Add(string.Format("{0} - {1} to {2} - {3} does not appear to be supported.", Constants.Weight.GetDescription(codeFrom, Constants.PluralState.Plural), codeFrom, Constants.Weight.GetDescription(codeTo, Constants.PluralState.Plural), codeTo));
						}
						else if (actual != expected)
						{
							errors.Add(string.Format("{0} - {1} - In code: {2} {3} => {4} {5}, but in sql: {2} {3} => {6} {5}.",
								Constants.Weight.GetDescription(codeFrom, Constants.PluralState.Plural), Constants.Weight.GetDescription(codeTo, Constants.PluralState.Plural), value, codeFrom, expected, codeTo, actual));
						}
					}
				}
			}

			Dictionary<string, IList<string>> results = new Dictionary<string, IList<string>>();
			if (errors.Count > 0)
			{
				results.Add("Conversion Missmatches", errors);
			}

			AssertGroupedErrorList(results);
		}

		#region Implementation

		decimal ConvertWeight(decimal weight, string fromUnit, string toUnit)
		{
			const string sql = "SELECT Value FROM dbo.ConvertWeight(@Weight, @FromUnit, @ToUnit)";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@Weight", SqlDbType.Decimal, weight);
				command.AddParameter("@FromUnit", SqlDbType.VarChar, fromUnit);
				command.AddParameter("@ToUnit", SqlDbType.VarChar, toUnit);
				return (decimal)command.ExecuteScalar();
			}
		}
		#endregion
	}
}

