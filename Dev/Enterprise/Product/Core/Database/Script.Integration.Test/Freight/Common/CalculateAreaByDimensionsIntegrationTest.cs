using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Data;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	internal class CalculateAreaByLengthIntegrationTest : TransactionedTestCase
	{
		public void TestCheckForConversionMismatchs()
		{
			List<string> errors = new List<string>();

			foreach (FieldInfo info in typeof(Constants.Length).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (info.FieldType != typeof(string))
				{
					continue;
				}

				string code = (string)info.GetValue(null);

				decimal multiplier = Constants.Length.Convert(1, code, Constants.Length.Metres);
				multiplier *= multiplier;

				decimal length = 1;
				decimal width = 1;
				decimal expected = length * width * multiplier;

				while (expected < 1000)
				{
					length *= 10;
					width *= 10;
					expected = length * width * multiplier;
				}

				expected = Math.Round(expected, 4);
				decimal actual = Math.Round(CalculateAreaByDimensions(length, width, code), 4);

				if (actual == 0)
				{
					errors.Add(string.Format("{0} - {1} does not appear to be supported.", Constants.Length.GetDescription(code, Constants.PluralState.Plural), code));
				}
				else if (actual != expected)
				{
					errors.Add(string.Format("{0} - In code: {1} {2}^2 => {3} M2, but in sql: {1} {2}^2 => {4} M2.", Constants.Length.GetDescription(code, Constants.PluralState.Plural), length * width, code, expected, actual));
				}
			}

			Dictionary<string, IList<string>> results = new Dictionary<string, IList<string>>();
			if (errors.Count > 0)
			{
				results.Add("Conversion Mismatches", errors);
			}

			AssertGroupedErrorList(results);
		}

		#region Implementation

		decimal CalculateAreaByDimensions(decimal length, decimal width, string unitOfLength)
		{
			const string sql = "select value from dbo.CalculateAreaByDimensions(@length, @width, @unitOfLength)";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@length", SqlDbType.Decimal, length);
				command.AddParameter("@width", SqlDbType.Decimal, width);
				command.AddParameter("@unitOfLength", SqlDbType.VarChar, 2, unitOfLength);
				return (decimal)command.ExecuteScalar();
			}
		}
		#endregion
	}
}

