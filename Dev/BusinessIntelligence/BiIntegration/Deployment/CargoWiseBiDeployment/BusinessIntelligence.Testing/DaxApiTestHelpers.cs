using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Bi.Deployment.AnalysisServices;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.Bi.BusinessIntelligence.Testing
{
	public static class DaxApiTestHelpers
	{
		#region Assertions

		public static void AssertDaxQueryReturnsSameResultAsCsv(string ssasDatabaseName, string daxQuery, string expectedCsvPath)
		{
			var queryResult = DaxApi.GetDaxQueryResult(ssasDatabaseName, daxQuery);
			var expectedCsv = ReadCsv(expectedCsvPath);

			Assertion.AssertEquals("Tables are the same", CompareDataTables(expectedCsv, queryResult));
		}

		public static void AssertDaxQueryReturnsSameResultAsJsonFile(string ssasDatabaseName, string daxQuery, string expectedJsonPath)
		{
			var queryResult = DaxApi.GetDaxQueryResult(ssasDatabaseName, daxQuery);
			var expectedJson = File.ReadAllText(expectedJsonPath);
			var truncatedResult = Regex.Replace(expectedJson, @"[\d-]", string.Empty).Replace("\"", string.Empty).Replace(".", string.Empty);
			var truncatedExpected = Regex.Replace(expectedJson, @"[\d-]", string.Empty).Replace("\"", string.Empty).Replace(".", string.Empty);
			Assertion.AssertContains("Files are the same",truncatedExpected, truncatedResult);
		}

		public static void AssertDaxQueryReturnsSameResultAsJsonString(string ssasDatabaseName, string daxQuery, string expectedJsonString)
		{
			var queryResult = DaxApi.GetDaxQueryResult(ssasDatabaseName, daxQuery);
			var expectedJson = JsonStringToDataTable(expectedJsonString);

			Assertion.AssertEquals("Tables are the same", CompareDataTables(expectedJson, queryResult));
		}

		#region SuppressResourceStringsCheckRegion

		public static string CompareDataTables(DataTable dtExpected, DataTable dtActual)
		{
			if (dtExpected == null)
			{
				if (dtActual == null)
				{
					return "Both data tables are null";
				}

				return "Data table 1 is null but 2 is not";
			}

			if (dtActual == null)
			{
				return "Data table 2 is null but 1 is not";
			}

			if (dtExpected.Rows.Count != dtActual.Rows.Count)
			{
				return "Data tables have different number of rows";
			}

			if (dtExpected.Columns.Count != dtActual.Columns.Count)
			{
				return "Data tables have different number of columns";
			}

			for (int i = 0; i < dtExpected.Rows.Count; i++)
			{
				for (int j = 0; j < dtExpected.Columns.Count; j++)
				{
					if (NoMatch(dtExpected.Rows[i][j], dtActual.Rows[i][j]))
					{
						return $@"Values are not equal in column {dtExpected.Columns[j].ColumnName} row #{i}
Query = {dtExpected.Rows[i][j]} ({dtExpected.Columns[j].DataType})
Flat file = {dtActual.Rows[i][j]} ({dtActual.Columns[j].DataType})";
					}
				}
			}

			return "Tables are the same";
		}

		static bool NoMatch(object expected, object actual)
		{
			var expectedStr = expected.ToString();
			var actualStr = actual.ToString();
			if (expected is double || expected is decimal)
			{
				expectedStr = string.Format("{0:0.00}", expected);
			}
			else if (expected is string)
			{
				if (double.TryParse(expectedStr, out var converted))
				{
					expectedStr = string.Format("{0:0.00}", converted);
				}
			}

			if (actual is double || actual is decimal)
			{
				actualStr = string.Format("{0:0.00}", actual);
			}
			else if (actual is string)
			{
				if (double.TryParse(actualStr, out var converted))
				{
					actualStr = string.Format("{0:0.00}", converted);
				}
			}

			return actualStr != expectedStr;
		}

		#endregion

		#endregion

		#region File readers

		static DataTable ReadCsv(string path)
		{
			DataTable dt = new DataTable();
			using (StreamReader sr = new StreamReader(path))
			{
				string[] headers = sr.ReadLine().Split(',');
				foreach (string header in headers)
				{
					dt.Columns.Add(header);
				}
				while (!sr.EndOfStream)
				{
					string[] rows = sr.ReadLine().Split(',');
					DataRow dr = dt.NewRow();
					for (int i = 0; i < headers.Length; i++)
					{
						dr[i] = rows[i];
					}
					dt.Rows.Add(dr);
				}
			}
			return dt;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		static DataTable ReadJsonFromTextFile(string path)
		{
			var text = File.ReadAllText(path);
			var dt = JsonStringToDataTable(text);
			return dt;
		}

		static DataTable JsonStringToDataTable(string jsonString)
		{
			var dt = (DataTable)JsonConvert.DeserializeObject(jsonString, (typeof(DataTable)));
			return dt;
		}

		#endregion
	}
}
