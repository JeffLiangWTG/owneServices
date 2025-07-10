using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Common;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.Bi.ConfigLoader.Testing
{
	abstract class ExpressionsTest : TransactionedTestCase
	{
		protected virtual string SchemaName { get; }

		public void TestEvaluateBaseTableExpressions()
		{
			var baseTables = ConfigData.EdwTableConfig.Where(t => t.Schema == SchemaName && !t.HasErrors);
			if (baseTables.Any())
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
				{
					CombineAssertions("Expression evaluation found errors. Please contact the Business Intelligence team and assign an assistance task for BI capability.", () =>
					{
						foreach (var baseTable in baseTables)
						{
							EvaluateExpression(baseTable);
						}
					});
				}
				Assert("All expressions passed.", true);
			}
			else
			{
				Assert("No tables to test.", true);
			}
		}

		public void TestEvaluateAggregateTableExpressions()
		{
			var aggTables = ConfigData.EdwDenormalizedTableConfig.Where(t => t.Schema == SchemaName && !t.HasErrors);
			if (aggTables.Any())
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
				{
					CombineAssertions("Expression evaluation found errors.", () =>
					{
						foreach (var aggTable in aggTables)
						{
							EvaluateExpression(aggTable);
						}
					});
				}
				Assert("All expressions passed.", true);
			}
			else
			{
				Assert("No tables to test.", true);
			}
		}

		#region Implementation

		readonly IEnumerable<string> EdwColumnsToSkipEvaluation = new List<string>
		{
			"[Customs].[BAS__Account].[GLAccountKey]",
			"[Finance].[AGG__GLTransactionINVCRDADJ3B].[ReportSubCode]",
			"[Finance].[AGG__GLTransactionDRCDPY2].[GLAmountCredit]",
			"[Finance].[AGG__GLTransactionDRCDPY2].[GLAmountDebit]",
			"[Finance].[AGG__GLTransactionDRCDPY3].[GLAmountCredit]",
			"[Finance].[AGG__GLTransactionDRCDPY3].[GLAmountDebit]",
			"[Finance].[AGG__GLTransactionINVCRDADJ3].[GeneralLedgerAmount]",
			"[Finance].[AGG__GLTransactionINVCRDADJ3].[ReportSubCode]",
			"[Finance].[AGG__GLTransactionINVCRDADJ5].[GeneralLedgerAmount]",
			"[Finance].[BAS__GeneralLedgerData].[LocalBalance]",
			"[Finance].[BAS__GeneralLedgerData].[OSBalance]",
			"[Finance].[BAS__GLAccountLog].[PreviousValue]",
			"[Finance].[BAS__GLAccountLog].[NewValue]"
		};

		void EvaluateExpression(BiAutomationConfigDataSet.EdwTableConfigRow baseTable)
		{
			try
			{
				var cdcTable = ConfigData.CdcTableConfig.FirstOrDefault(t => t.SourceSchema == baseTable.SourceSchema && t.SourceTable == baseTable.StagingTable);
				if (cdcTable == null)
				{
					Fail(String.Format(CultureInfo.InvariantCulture, "No staging table found for [{0}].[{1}].", baseTable.Schema, baseTable.Name));
				}
				else
				{
					var cdcColumns = cdcTable.GetCdcColumnConfigRows().Where(c => c.ColumnInEdw);

					var sqlText = String.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{0}].[{1}] ({2}ID, {2}Key, @columnName)
VALUES(newid(), 1, @columnValue)",
						baseTable.Schema,
						baseTable.Name,
						baseTable.BaseName);

					var cdcColumnNameList = cdcColumns.Select(c => c.SourceColumn);
					foreach (var edwColumn in baseTable.GetEdwColumnConfigRows().Where(c => !EdwColumnsToSkipEvaluation.Contains($"[{baseTable.Schema}].[{baseTable.Name}].[{c.Name}]") && !c.UsesFunction && !string.IsNullOrWhiteSpace(c.Expression) && !cdcColumnNameList.Contains(c.Expression)))
					{
						foreach (var expression in GetExpressionsToTest(edwColumn.Expression))
						{
							var expressionValues = new List<string>();
							var cdcColumnsInExpression = cdcColumns.Where(c => new Regex(String.Format(CultureInfo.InvariantCulture, @"\[?\b{0}\b\]?(?!')", c.SourceColumn), RegexOptions.IgnoreCase).IsMatch(expression)).ToList();
							CreateExpression(expressionValues, cdcColumnsInExpression, expression);

							foreach (var expressionValue in expressionValues)
							{
								var query = sqlText.Replace("@columnName", edwColumn.Name).Replace("@columnValue", expressionValue);
								try
								{
									TestConnection.ExecuteNonQuery(query);
								}
								catch (SqlException ex)
								{
									Fail(String.Format(CultureInfo.InvariantCulture, "Column: [{0}].[{1}].[{2}]\r\nExpression: {3}\r\nEvaluated as: {4}\r\nException: {5}", baseTable.Schema, baseTable.Name, edwColumn.Name, edwColumn.Expression, expressionValue, ex.Message));
								}
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Fail(String.Format(CultureInfo.InvariantCulture, "Table: [{0}]\r\nException: {1}\r\n{2}", baseTable.Name, ex.Message, ex.StackTrace));
			}
		}

		void EvaluateExpression(BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow aggTable)
		{
			try
			{
				var sqlText = String.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{0}].[{1}] ({2}Key, @columnName)
VALUES(1, @columnValue)",
					aggTable.Schema,
					aggTable.Name,
					aggTable.BaseName);

				var sourceTables = BiAutomationConfigDataSet.ParseSourceTables(aggTable.Expression);
				var simpleRegex = new Regex(@"^\[*.+?\]*\.\[*.+?\]*$", RegexOptions.IgnoreCase);
				var baseColumns = new List<BiAutomationConfigDataSet.EdwColumnConfigRow>();
				foreach (var sourceTable in sourceTables)
				{
					var baseTable = ConfigData.EdwTableConfig.FirstOrDefault(t => t.Schema == sourceTable.Schema && t.Name == sourceTable.Table);
					baseColumns.AddRange(baseTable.GetEdwColumnConfigRows());
				}
				foreach (var aggColumn in aggTable.GetEdwDenormalizedColumnConfigRows().Where(c => !EdwColumnsToSkipEvaluation.Contains($"[{aggTable.Schema}].[{aggTable.Name}].[{c.Name}]") && !string.IsNullOrEmpty(c.Expression) && !simpleRegex.IsMatch(c.Expression)))
				{
					foreach (var expression in GetExpressionsToTest(aggColumn.Expression))
					{
						var columnExpression = expression;
						foreach (var sourceTable in sourceTables)
						{
							var tableNameRegex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\b{0}\b", sourceTable.Alias));
							columnExpression = tableNameRegex.Replace(columnExpression, sourceTable.Table);
						}

						var expressionValues = new List<string>();
						var baseColumnsInExpression = baseColumns.Where(c => new Regex(String.Format(CultureInfo.InvariantCulture, @"\[*\b{0}\b\]*\.\[*\b{1}\b\]*", c.TableName, c.Name), RegexOptions.IgnoreCase).IsMatch(columnExpression)).ToList();
						CreateExpression(expressionValues, baseColumnsInExpression, columnExpression);

						foreach (var expressionValue in expressionValues)
						{
							var query = sqlText.Replace("@columnName", aggColumn.Name).Replace("@columnValue", expressionValue);
							try
							{
								TestConnection.ExecuteNonQuery(query);
							}
							catch (SqlException ex)
							{
								Fail(String.Format(CultureInfo.InvariantCulture, "Column: [{0}].[{1}]\r\nExpression: {2}\r\nEvaluated as: {3}\r\nException: {4}", aggTable.Name, aggColumn.Name, aggColumn.Expression, expressionValue, ex.Message));
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Fail(String.Format(CultureInfo.InvariantCulture, "Table: [{0}]\r\nException: {1}\r\n{2}", aggTable.Name, ex.Message, ex.StackTrace));
			}
		}

		List<string> GetExpressionsToTest(string expression)
		{
			var expressionsToTest = new List<string>();
			var caseRegex = new Regex(@"\bCASE\b(.|\s)*?\bEND\b", RegexOptions.IgnoreCase);
			if (caseRegex.IsMatch(expression))
			{
				ReplaceCaseWithPossibleValues(expressionsToTest, caseRegex.Matches(expression), expression);
			}
			else
			{
				expressionsToTest.Add(expression);
			}

			return expressionsToTest;
		}

		void ReplaceCaseWithPossibleValues(List<string> expressionsToTest, MatchCollection matches, string edwExpression, int counter = 0)
		{
			if (counter < matches.Count)
			{
				var caseGroup = matches[counter].Value;
				var regex = new Regex(@"\b(THEN|ELSE)\b\s+(?<Value>(.|\s)*?)\s+\b(?=WHEN|ELSE|END)\b", RegexOptions.IgnoreCase);
				foreach (Match match in regex.Matches(edwExpression))
				{
					var value = match.Groups["Value"].Value;
					var expression = edwExpression.Replace(caseGroup, value);
					if (counter == matches.Count - 1)
					{
						expressionsToTest.Add(expression);
					}
					else
					{
						var newCounter = counter + 1;
						ReplaceCaseWithPossibleValues(expressionsToTest, matches, expression, newCounter);
					}
				}
			}
		}

		void CreateExpression(List<string> expressionValues, List<BiAutomationConfigDataSet.CdcColumnConfigRow> cdcColumns, string edwExpression, int counter = 0)
		{
			if (counter < cdcColumns.Count)
			{
				var cdcColumn = cdcColumns[counter];
				var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[?\b{0}\b\]?(?!')", cdcColumn.SourceColumn), RegexOptions.IgnoreCase);
				foreach (var value in GetPossibleValues(cdcColumn.DataType, cdcColumn.MaxLength, cdcColumn.Precision, cdcColumn.Scale, cdcColumn.Nullable))
				{
					var expression = regex.Replace(edwExpression, value);
					if (counter == cdcColumns.Count - 1)
					{
						expressionValues.Add(expression);
					}
					else
					{
						var newCounter = counter + 1;
						CreateExpression(expressionValues, cdcColumns, expression, newCounter);
					}
				}
			}
			else if (!cdcColumns.Any())
			{
				expressionValues.Add(edwExpression);
			}
		}

		void CreateExpression(List<string> expressionValues, List<BiAutomationConfigDataSet.EdwColumnConfigRow> baseColumns, string edwExpression, int counter = 0)
		{
			if (counter < baseColumns.Count)
			{
				var baseColumn = baseColumns[counter];
				var regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"\[*\b{0}\b\]*\.\[*\b{1}\b\]*", baseColumn.TableName, baseColumn.Name), RegexOptions.IgnoreCase);
				foreach (var value in GetPossibleValues(baseColumn.DataType, baseColumn.MaxLength, baseColumn.Precision, baseColumn.Scale, true))
				{
					var expression = regex.Replace(edwExpression, value);
					if (counter == baseColumns.Count - 1)
					{
						expressionValues.Add(expression);
					}
					else
					{
						var newCounter = counter + 1;
						CreateExpression(expressionValues, baseColumns, expression, newCounter);
					}
				}
			}
			else if (!baseColumns.Any())
			{
				expressionValues.Add(edwExpression);
			}
		}

		List<string> GetPossibleValues(string dataType, int maxLength, int precision, int scale, bool nullable)
		{
			var results = new List<string>();

			if (nullable)
			{
				results.Add("NULL");
			}
			switch (dataType)
			{
				case "bigint":
					results.AddRange(new List<string> { "0", Int64.MinValue.ToString(), Int64.MaxValue.ToString() });
					break;
				case "bit":
					results.AddRange(new List<string> { "0", "1" });
					break;
				case "char":
					results.AddRange(new List<string> { "'a'", "'z'", "'0'", "'1'", "' '" });
					break;
				case "date":
					results.AddRange(new List<string> { "'1753-01-01'", "'9999-12-31'", "'" + DateTime.Now.ToString("yyyy-MM-dd") + "'" });
					break;
				case "datetime":
					results.AddRange(new List<string> { "'1753-01-01 00:00:00'", "'9999-12-31 23:59:59.997'", "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" });
					break;
				case "decimal":
					results.AddRange(GetDecimalValues(precision, scale));
					break;
				case "int":
					results.AddRange(new List<string> { "0", int.MinValue.ToString(), int.MaxValue.ToString() });
					break;
				case "money":
					results.AddRange(GetDecimalValues(precision, scale));
					break;
				case "nvarchar":
					results.AddRange(GetStringValues(maxLength).Select(x => "N" + x));
					break;
				case "smalldatetime":
					results.AddRange(new List<string> { "'1900-01-01 00:00:00'", "'2079-06-06 23:59:00'", "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" });
					break;
				case "smallint":
					results.AddRange(new List<string> { "0", Int16.MinValue.ToString(), Int16.MaxValue.ToString() });
					break;
				case "tinyint":
					results.AddRange(new List<string> { "0", "255" });
					break;
				case "uniqueidentifier":
					results.AddRange(new List<string> { "'" + Guid.NewGuid() + "'" });
					break;
				case "varchar":
					results.AddRange(GetStringValues(maxLength));
					break;
				case "binary":
					break;
				case "datetimeoffset":
					break;
				case "image":
					break;
				case "varbinary":
					break;
				case "xml":
					break;
				default:
					Fail(String.Format(CultureInfo.InvariantCulture, "Data type '{0}' not supported.", dataType));
					break;
			}

			return results;
		}

		List<string> GetStringValues(int maxLength)
		{
			return new List<string> { "''", "'" + new string('a', maxLength) + "'" };
		}

		List<string> GetDecimalValues(int precision, int scale)
		{
			var value = new string('9', precision - scale) + "." + new string('9', scale);
			return new List<string> { "0", value, "-" + value };
		}

		BiConfigurationData ConfigData
		{
			get
			{
				return configData ?? (configData = BiAutomationConfigLoader.Instance.ConfigData);
			}
		}
		BiConfigurationData configData;

		#endregion
	}
}
