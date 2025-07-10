using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Utility.Testing
{
	public abstract class ScriptTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			if (ShouldCreateTestPeriodsForToday)
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			}
		}

		protected virtual bool ShouldCreateTestPeriodsForToday => true;
		protected const string MinDateTime = "1900-01-01 00:00:00";
		protected const string MaxDateTime = "2079-06-06 23:59:29";

		protected string GetMinDateTimeString(ZDateTime source)
		{
			return source.IsEmpty ? MinDateTime : source.ToISO8601String();
		}

		protected string GetMaxDateTimeString(ZDateTime source)
		{
			return source.IsEmpty ? MaxDateTime : source.ToISO8601String();
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator_cached ?? (testObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator_cached;

		protected AccountingPeriodCalculator PeriodCalculator
		{
			get { return periodCalculator_cached ?? (periodCalculator_cached = new AccountingPeriodCalculator(Factory, GlbCompany.CurrentCompany)); }
		}
		AccountingPeriodCalculator periodCalculator_cached;

		protected void AssertDataTableAllRows(string messagePrefix, DataTable table, string[] headers, object[][] lines)
		{
			AssertEquals(messagePrefix + ": Row count", lines.Length, table.Rows.Count);
			if (table.Rows.Count == 0 || lines.Length == 0)
			{
				return;
			}

			AssertDataTableSelectedRows(messagePrefix, table, headers, lines, headers, allLinesWithSameLineOrder: true);
		}

		protected void AssertDataTableAllRowsByKeyColumns(string messagePrefix, DataTable table, string[] headers, object[][] lines)
		{
			AssertDataTableAllRowsByKeyColumns(messagePrefix, table, headers, lines, new[] { headers[0] });
		}

		protected void AssertDataTableAllRowsByKeyColumns(string messagePrefix, DataTable table, string[] headers, object[][] lines, string[] keyColumnNames)
		{
			AssertEquals(messagePrefix + ": Row count", lines.Length, table.Rows.Count);
			if (table.Rows.Count == 0 || lines.Length == 0)
			{
				return;
			}

			AssertDataTableSelectedRows(messagePrefix, table, headers, lines, keyColumnNames, false);
		}

		protected void AssertDataTableSelectedRows(string messagePrefix, DataTable table, string[] headers, object[][] lines, bool withSameLineOrder = true)
		{
			AssertDataTableSelectedRows(messagePrefix, table, headers, lines, new[] { headers[0] }, withSameLineOrder);
		}

		protected void AssertDataTableSelectedRows(string messagePrefix, DataTable table, string[] headers, object[][] lines, string[] keyColumnNames, bool withSameLineOrder = true, bool allLinesWithSameLineOrder = false)
		{
			Assert(messagePrefix + ": lines count should be not more then table rows count.", lines.Length <= table.Rows.Count);

			if (table.Rows.Count == 0 || lines.Length == 0)
			{
				return;
			}

			int headerIndex = 0;
			var headerIndexes = headers.ToDictionary(key => key, value => headerIndex++);

			var linesNotMatched = new List<int>();

			var rowsNotMatched = new List<int>();
			for (int i = 0; i < table.Rows.Count; i++)
			{
				rowsNotMatched.Add(i);
			}

			for (int i = 0; i < lines.Length; i++)
			{
				bool lineMatched = false;
				var line = lines[i];
				int rowIndex = -1;

				foreach (int j in rowsNotMatched)
				{
					bool keyMatched = true;
					var row = table.Rows[j];

					if (!allLinesWithSameLineOrder)
					{
						foreach (string keyColumn in keyColumnNames)
						{
							if (!line[headerIndexes[keyColumn]].Equals(row[keyColumn]))
							{
								keyMatched = false;
								break;
							}
						}
					}

					if (keyMatched)
					{
						AssertDataRow($"{(string.IsNullOrEmpty(messagePrefix) ? "" : $"{messagePrefix}, ")}line {i + 1}, key {generateLineKey(line)}", row, headers, line);
						rowIndex = j;
						lineMatched = true;
						break;
					}
				}

				if (lineMatched)
				{
					var rowsToRemove = rowsNotMatched.Where(x => (withSameLineOrder && x < rowIndex) || x == rowIndex).ToArray();
					foreach (int ri in rowsToRemove)
					{
						rowsNotMatched.Remove(ri);
					}
				}
				else
				{
					linesNotMatched.Add(i);
				}
			}

			var lineKeysNotMatched = new ZStringBuilder();
			foreach (var i in linesNotMatched)
			{
				lineKeysNotMatched.Append(generateLineKey(lines[i]));
			}
			AssertEquals(string.Format(messagePrefix + ": not all lines were found in the table. Not found line keys: {1}{0}.",
				lineKeysNotMatched.ToStringWithNewLineBetweenAppends(), System.Environment.NewLine), 0, linesNotMatched.Count);

			string generateLineKey(object[] line)
			{
				ZStringBuilder lineKeyStringBuilder = new ZStringBuilder();
				foreach (var column in keyColumnNames)
				{
					object lineKey = line[headerIndexes[column]];
					lineKeyStringBuilder.Append(lineKey.ToString());
				}

				return lineKeyStringBuilder.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		protected void AssertDataRow(DataRow row, (string name, object value)[] expectedData, Action<string, object> actionForEachFieldDBValue = null)
		{
			AssertDataRow("", row, expectedData.Select(x => x.name).ToArray(), expectedData.Select(x => x.value).ToArray(), actionForEachFieldDBValue);
		}
		protected void AssertDataRow(DataRow row, string[] fieldNames, object[] fieldValues, Action<string, object> actionForEachFieldDBValue = null)
		{
			AssertDataRow("", row, fieldNames, fieldValues, actionForEachFieldDBValue);
		}

		protected void AssertDataRow(string messagePrefix, DataRow row, string[] fieldNames, object[] fieldValues, Action<string, object> actionForEachFieldDBValue = null)
		{
			AssertEquals(messagePrefix + ": fieldNames.Length should be equal fieldValues.Length.", fieldNames.Length, fieldValues.Length);

			CombineAssertions(() =>
				{
					for (int i = 0; i < fieldNames.Length; i++)
					{
						string fieldName = fieldNames[i];
						object fieldValue = fieldValues[i];
						object valueDB = row[fieldName];
						if (Convert.IsDBNull(row[fieldName]) && fieldValue != null)
						{
							try
							{
								decimal test = (decimal)fieldValue;
								valueDB = 0M;
							}
							catch
							{
							}
						}
						AssertEquals(messagePrefix + ": " + fieldName, fieldValue ?? Convert.DBNull, valueDB);

						actionForEachFieldDBValue?.Invoke(fieldName, valueDB);
					}
				}
			);
		}

		protected string GetScriptFromDb(string scriptName)
		{
			string sqlText = string.Format(
				"IF exists(SELECT null FROM sys.objects WHERE [name] = '{0}') EXEC sp_helptext {0}",
				scriptName);
			StringBuilder dbScriptBuilder = new StringBuilder();

			using (DbCommand cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					dbScriptBuilder.Append(reader.GetString(0));
				}
			}

			return dbScriptBuilder.ToString();
		}

		protected void AssertTableAsTextFromSQLServerManagenentStudio(string message, DataTable resultTable, string expectedResult, IEnumerable<string> fullDateTimeColumns, IEnumerable<string> dateOnlyColumns, IEnumerable<Tuple<ZGuid, string>> pkReplacements)
		{
			var expectedResultByLines = expectedResult.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var columnWidthTemplate = expectedResultByLines[1];
			var columnWidths = columnWidthTemplate.Split(' ');
			AssertEquals("Number of columns in expected and actual results is not equal", resultTable.Columns.Count, columnWidths.Length);

			var maxColumnWidth = new Dictionary<string, int>();
			int i = 0;
			foreach (DataColumn column in resultTable.Columns)
			{
				maxColumnWidth.Add(column.ColumnName, columnWidths[i++].Length);
			}

			var tableAsText = new List<List<string>>();
			foreach (DataRow row in resultTable.Rows)
			{
				var rowLine = new List<string>();
				foreach (DataColumn column in resultTable.Columns)
				{
					var rowColumn = row[column];
					var columnName = column.ColumnName;
					var value = DBValueToString(rowColumn, !fullDateTimeColumns.Contains(columnName), dateOnlyColumns.Contains(columnName));
					rowLine.Add(value.PadRight(maxColumnWidth[columnName]));
				}
				tableAsText.Add(rowLine);
			}

			var result = new ZStringBuilder();
			result.AppendLine();
			result.AppendLine(string.Join(" ", maxColumnWidth.Select(item => item.Key.PadRight(item.Value))).Trim());
			result.AppendLine(columnWidthTemplate);
			foreach (var row in tableAsText)
			{
				result.AppendLine(string.Join(" ", row).Trim());
			}

			var resultTableAsText = result.ToString();
			foreach (var pkReplacement in pkReplacements)
			{
				var oldValue = pkReplacement.Item1.ToString();
				var newValue = pkReplacement.Item2.PadRight(oldValue.Length);
				expectedResult = expectedResult.Replace(oldValue, newValue);
				resultTableAsText = resultTableAsText.Replace(oldValue, newValue);
			}

			//remove duplicate whitespace except newlines
			expectedResult = Regex.Replace(expectedResult, @"[^\S\n]+", " ");
			resultTableAsText = Regex.Replace(resultTableAsText, @"[^\S\n]+", " ");

			AssertMultilineASCIIEquals(message, expectedResult, resultTableAsText);
		}

		protected void AssertTableAsTextFromSQLServerManagenentStudio(string message, DataTable resultTable, string expectedResult, IEnumerable<string> fullDateTimeColumns, IEnumerable<Tuple<ZGuid, string>> pkReplacements)
		{
			AssertTableAsTextFromSQLServerManagenentStudio(message, resultTable, expectedResult, fullDateTimeColumns, new List<string>() { }, pkReplacements);
		}

		protected void AddTVPAndIsEmptyParameters<T>(DbCommand command, string paramName, string parameterTypeName, string isEmptyParamName, T[] values)
			where T : struct
		{
			var table = new DataTable();
			table.Columns.Add("Value", typeof(T));

			if (values != null)
			{
				foreach (T value in values)
				{
					table.Rows.Add(value);
				}
			}

			command.AddTableValuedParameter(paramName, parameterTypeName, table);
			command.AddParameter(isEmptyParamName, SqlDbType.Bit, table.Rows.Count == 0);
		}

		string DBValueToString(object rowColumn, bool isSmallDateTime, bool isDateOnly)
		{
			string value = "";
			if (rowColumn is DBNull)
			{
				value = "NULL";
			}
			else if (rowColumn is DateTime)
			{
				if (isDateOnly)
				{
					value = ((DateTime)rowColumn).ToString("yyyy-MM-dd");
				}
				else
				{
					var smallDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
					var dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
					value = ((DateTime)rowColumn).ToString(isSmallDateTime ? smallDateTimeFormat : dateTimeFormat);
				}
			}
			else if (rowColumn is decimal)
			{
				value = ((decimal)rowColumn).ToString("0.00");
			}
			else if (rowColumn is bool)
			{
				value = (bool)rowColumn ? "1" : "0";
			}
			else
			{
				value = rowColumn.ToString();
			}

			return value;
		}

		#region Test Data Setup
		protected void SetupDeclarationsShipmentsWithJobsRevRecognitionDates()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, ZDateTime.Today.AddMonths(-2));
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 3M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 2M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 4M, TestObjectCreator.ABIGAS);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			consol.Shipments.Add(shipment);
			consol = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C001002");
			consol.Shipments.Add(shipment);

			shipment = TestObjectCreator.CreateShipment("S001002");
			job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, ZDateTime.Today.AddMonths(-5));
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, ZDateTime.Today.AddMonths(-6));
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 10M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 30M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 20M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 40M, TestObjectCreator.ABIGAS);

			consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001003");
			consol.Shipments.Add(shipment);

			var declaration = TestObjectCreator.CreateDeclaration("B001001");
			job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ZDateTime.Today);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, ZDateTime.Today.AddMonths(-3));
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 300M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 200M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 400M, TestObjectCreator.ABIGAS);

			declaration = TestObjectCreator.CreateDeclaration("B001002");
			job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, AccountingConstants.RevenueRecognitionDateConstants.Immediate);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ZDateTime.Today.AddDays(-1));
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 1000M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 3000M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 2000M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 4000M, TestObjectCreator.ABIGAS);
		}

		#endregion
	}
}
