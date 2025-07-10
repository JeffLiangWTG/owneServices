using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Common.Data;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DataProviders
{
	class NativeSqlTableProvider : TableProvider
	{
		public override string GetValidSelectStatement(string section, string dataSourceString, Report reportObject, int maximumNumberOfRows = -1)
		{
			dataSourceString = dataSourceString.Replace('\r', ' ');
			dataSourceString = dataSourceString.Replace('\n', ' ');

			if (!dataSourceString.TrimStart().StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase) && !dataSourceString.TrimStart().StartsWith("EXEC ", StringComparison.OrdinalIgnoreCase))
			{
				if (maximumNumberOfRows == -1)
				{
					dataSourceString = string.Format(CultureInfo.InvariantCulture, (NoResString)"SELECT <{0}.SelectList> FROM {1}", section, dataSourceString);
				}
				else
				{
					dataSourceString = string.Format(CultureInfo.InvariantCulture, "SELECT TOP {0} <{1}.SelectList> FROM {2}", maximumNumberOfRows, section, dataSourceString);
				}
			}
			else if (dataSourceString.TrimStart().StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase) && maximumNumberOfRows != -1)
			{
				if (dataSourceString.TrimStart().StartsWith("SELECT TOP", StringComparison.OrdinalIgnoreCase))
				{
					reportObject.ErrorManager.Add(new ReportProcessingError(Res.GetString("8bc09ba8-0b4a-4c46-ada2-c957505c6010", "You cannot use '{0}' macro and a data source string including 'SELECT TOP' at the same time. Please check the data source string({1}) and the section body area", "MaximumNumberOfRowsToShow", dataSourceString), ReportProcessingErrorSeverity.Error));
				}
				else if (dataSourceString.TrimStart().StartsWith("SELECT DISTINCT ", StringComparison.OrdinalIgnoreCase))
				{
					dataSourceString = dataSourceString.Insert(dataSourceString.IndexOf("SELECT DISTINCT ", StringComparison.OrdinalIgnoreCase) + 16, string.Format(CultureInfo.InvariantCulture, (NoResString)"TOP {0} ", maximumNumberOfRows));
				}
				else
				{
					dataSourceString = dataSourceString.Insert(dataSourceString.IndexOf("SELECT ", StringComparison.OrdinalIgnoreCase) + 7, string.Format(CultureInfo.InvariantCulture, (NoResString)"TOP {0} ", maximumNumberOfRows));
				}
			}

			MatchCollection matches = RegexProvider.SelectListMacroRegex.Matches(dataSourceString);
			if (matches.Count > 0 && reportObject == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "You must specify a report object for replacing the <SelectList> macro within the DataSource string({0}).", dataSourceString));
			}
			foreach (Match match in matches)
			{
				if (match.Success)
				{
					ValueProvider provider = reportObject.MacroTranslator.GetValueProvider(Passes.FirstPass, match.Value);
					if (provider != null)
					{
						dataSourceString = dataSourceString.Replace(match.Value, (string)provider.GetReplacement(match.Value, reportObject));
					}
				}
			}

			DataSourceString = dataSourceString;
			return dataSourceString;
		}

		protected string AddWhereAndOrderByClause(string sourceString, Report report)
		{
			if (!sourceString.TrimStart().StartsWith("EXEC ", StringComparison.OrdinalIgnoreCase))
			{
				var whereClause = report.FilterCollection.WhereClause();
				if (!string.IsNullOrWhiteSpace(whereClause))
				{
					var hasWhereClause = DoesSqlHaveWhereClause(sourceString);
					sourceString += (hasWhereClause ? " AND " : " WHERE ") + whereClause;
				}

				if (report.SortOrderCollection.SelectedOrder.FieldList != "NULL")
				{
					//TODO: check for existing order by 
					sourceString += (NoResString)" ORDER BY " + report.SortOrderCollection.SelectedOrder.FieldList;
				}
			}
			return sourceString;
		}

		protected bool DoesSqlHaveWhereClause(string sourceString)
		{
			sourceString = DataUtils.StripCommentsFromSql(sourceString);
			return RemoveSubQueries(sourceString).IndexOf(" WHERE ", StringComparison.OrdinalIgnoreCase) > 0;
		}

		string RemoveSubQueries(string query)
		{
			while (RemoveQuotesRegex.IsMatch(query))
			{
				query = RemoveQuotesRegex.Replace(query, "");
			}

			while (RemoveSubQueriesRegex.IsMatch(query))
			{
				query = RemoveSubQueriesRegex.Replace(query, "");
			}
			return query;
		}

		public override DataTable GetDataTable(string tableName, string dataSourceString, Report reportObject, bool needsToAddWhereClause)
		{
			return GetDataTable(tableName, dataSourceString, reportObject, needsToAddWhereClause, -1);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		public override DataTable GetDataTable(string tableName, string dataSourceString, Report reportObject, bool needsToAddWhereClause, int maximumNumberOfRows)
		{
			// really only here for tests. I don't think we need to support it
			dataSourceString = GetValidSelectStatement(tableName, dataSourceString, reportObject, maximumNumberOfRows);

			// Filtering and Sorting for first query only
			if (needsToAddWhereClause)
			{
				dataSourceString = AddWhereAndOrderByClause(dataSourceString, reportObject);
			}

			var macroReplacer = new MacroRegexReplacer(reportObject);
			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			{
				dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList(dataSourceString);
				var queryhints = GetSQLQueryHints(reportObject, Regex.IsMatch(dataSourceString, @"^\s*EXEC(UTE)?\s+", RegexOptions.IgnoreCase));
				dataSourceString += macroReplacer.ReplaceParametersForSQLQueryHints(queryhints);
			}
			return FillDataTable(tableName, reportObject.RunningConnection, dataSourceString, reportObject, needsToAddWhereClause, macroReplacer.UdfParameters);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		DataTable FillDataTable(string tableName, DbConnection connection, string dataSourceString, Report report, bool needsToAddWhereClause, SqlParameter[] udfParameters)
		{
			bool statisticsOn = false;
			bool useStatistics = DataRegistry.Instance.ReportStatisticsLogExecutionPlan;
			if (connection == Db.Connection)
			{
				dataSourceString += "\r\n\r\n" + DbCommand.ExecuteAsReaderFlagComments;
			}
			var timeOutValue = report.RemainingTimeForTimeout;
			var comments = GetReportComments(report, timeOutValue, udfParameters);

			if (timeOutValue == 0)
			{
				throw new ReportSQLTimeoutException();
			}
#pragma warning disable CW1107 // connection.Command() - Use the BusinessObjectFactory rather than hitting the DB directly.
			var command = connection.Command(comments + dataSourceString, (int)Math.Ceiling(timeOutValue));
#pragma warning restore CW1107 // connection.Command() - Use the BusinessObjectFactory rather than hitting the DB directly.

			var udfParaStr = new StringBuilder();
			var whereClauseParaStr = new StringBuilder();

			foreach (var parameter in udfParameters)
			{
				if (parameter.SqlDbType == SqlDbType.Structured)
				{
					command.AddTableValuedParameter(parameter.ParameterName, parameter.TypeName, (DataTable)parameter.Value);
				}
				else
				{
					command.AddParameter(parameter.ParameterName, parameter.SqlDbType, parameter.Size, parameter.Precision, parameter.Scale, parameter.Value);
				}
				udfParaStr.AppendFormat("{0},{1},{2},{3},{4},{5};", parameter.ParameterName, parameter.SqlDbType, parameter.Size, parameter.Precision, parameter.Scale, parameter.Value);
			}

			if (needsToAddWhereClause)
			{
				foreach (var parameter in report.FilterCollection.SqlParameters())
				{
					command.AddParameter(parameter.ParameterName, parameter.SqlDbType, parameter.Size, parameter.Precision, parameter.Scale, parameter.Value);
					whereClauseParaStr.AppendFormat("{0},{1},{2},{3},{4},{5};", parameter.ParameterName, parameter.SqlDbType, parameter.Size, parameter.Precision, parameter.Scale, parameter.Value);
				}
			}

			DataTable result = null;
			using (var adapter = command.NewDataAdapter((int)Math.Ceiling(timeOutValue)))
			using (var dataSet = new DataSet())
			{
				var sw = Stopwatch.StartNew();
				var serverName = connection.ServerName.ToUpper();
				var commandTextWithPara = string.Format((NoResString)"\r\n--Udf Parameters: {0}\r\n\r\n--Server: {1}\r\n\r\n--WhereClause Parameters: {2}\r\n{3}", udfParaStr.ToString(), serverName, whereClauseParaStr.ToString(), command.CommandText.Replace(DbCommand.ExecuteAsReaderFlagComments, ""));
				try
				{
					if (report.stmReportRun != null)
					{
						report.stmReportRun.RRI_QueryText += commandTextWithPara;

						if (report.stmReportRun.RRI_SQLServer.Length < report.stmReportRun.RRI_SQLServerInfo.MaxLength && report.stmReportRun.RRI_SQLServer.IndexOf(serverName) < 0)
						{
#pragma warning disable EDI003 // The Local variable "newValue" has a length of 511, which exceeds RRI_SQLServer's maximum length of 510
							var newValue = report.stmReportRun.RRI_SQLServer.IsEmpty ? serverName : report.stmReportRun.RRI_SQLServer + "," + serverName;
							report.stmReportRun.RRI_SQLServer = newValue.Length > report.stmReportRun.RRI_SQLServerInfo.MaxLength ? newValue.Substring(0, report.stmReportRun.RRI_SQLServerInfo.MaxLength) : newValue;
#pragma warning restore EDI003
						}

						if (useStatistics)
						{
							connection.ExecuteNonQuery((NoResString)"SET STATISTICS XML ON");
							statisticsOn = true;
						}
					}

					try
					{
						adapter.Fill(dataSet);
					}
					catch (SqlException sqlEx) when (statisticsOn && new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.PermissionDeniedInDatabase)
					{
						connection.ExecuteNonQuery((NoResString)"SET STATISTICS XML OFF");
						statisticsOn = false;
						adapter.Fill(dataSet);
						if (report.stmReportRun != null)
						{
							var stmNote = report.stmReportRun.Notes.AddNew();
							stmNote.ST_Description = (NoResString)"Permission denied in database";
							stmNote.ST_NoteDataAsText = sqlEx.ToString();
							stmNote.ST_NoteType = nameof(StmNoteVisibility.PUB);
						}
					}

					if (dataSet.Tables.Count == 0 || statisticsOn && dataSet.Tables.Count == 1 && dataSet.Tables[0].Columns.Contains(DatabaseConstants.ColumnNameForStatisticsXmlOn))
					{
						result = new DataTable();
					}
					else
					{
						if (statisticsOn)
						{
							result = dataSet.Tables.OfType<DataTable>().FirstOrDefault(t =>
								t.Columns.Count > 1 || t.Columns.Count == 1 && t.Columns[0].ColumnName != DatabaseConstants.ColumnNameForStatisticsXmlOn);
						}
						else
						{
							result = dataSet.Tables[0];
						}

						if (result != null)
						{
							dataSet.Tables.Remove(result);
						}
					}
					if (report.stmReportRun != null)
					{
						report.stmReportRun.RRI_RowsReturned += result.Rows.Count;

						if (statisticsOn)
						{
							connection.ExecuteNonQuery((NoResString)"SET STATISTICS XML OFF");
							statisticsOn = false;
							try
							{
								// SQLs without FROM/WHERE/RECOMPILE may not have the execution plan included in the query result.
								// Enable Registry > Documents > Use recompile SQL Query Hint or add where 1=1 may solve the problem.
								report.stmReportRun.RRI_ExecutionPlanText += System.Environment.NewLine + (string)dataSet.Tables[dataSet.Tables.Count - 1].Rows[0][0];
							}
							catch (Exception e) when (!e.IsCriticalException()) { }
						}
					}
				}
				catch (SqlException sqlEx)
				{
					var errorHandler = new DbErrorHandler(sqlEx, connection);

					if (errorHandler.ExceptionType == DbErrorType.TimeoutExpired)
					{
						throw new ReportSQLTimeoutException();
					}
					else if (errorHandler.ExceptionType == DbErrorType.MetadataHasBeenChanged)
					{
						throw new MetadataHasChangedException();
					}
					else if (errorHandler.ExceptionType == DbErrorType.GhostRecordsBeingDeleted)
					{
						throw new GhostRecordsBeingDeletedException();
					}
					else
					{
						throw new SQLExecutionException(tableName, commandTextWithPara, sqlEx);
					}
				}
				catch (TimeoutException)
				{
					throw new ReportSQLTimeoutException();
				}
				catch (Exception exception)
				{
					if (exception.IsCriticalException())
					{
						throw;
					}

					throw new SQLExecutionException(tableName, command.CommandText, exception);
				}
				finally
				{
					sw.Stop();
					report.SetTableProviderTimeConsumed(this, sw.Elapsed.TotalSeconds);
					if (report.stmReportRun != null)
					{ report.stmReportRun.RRI_QueryText += string.Format(CultureInfo.CurrentCulture, (NoResString)"\r\n\r\n--Time Consumed: {0}\r\n\r\n", sw.Elapsed.TotalSeconds); }
					if (statisticsOn)
					{ connection.ExecuteNonQuery((NoResString)"SET STATISTICS XML OFF"); }
				}
			}

			return result;
		}

		string GetReportComments(Report report, double timeOutValue, SqlParameter[] udfParameters)
		{
			var comments = string.Format((NoResString)"\r\n--Report Name: {0}\r\n\r\n--Staff Name: {1}\r\n\r\n--Time Out: {2}\r\n\r\n", report.Name, GlbStaff.CurrentUser.GS_FullName, timeOutValue);

			if (ShouldAddParameterComments(report))
			{
				comments += GetParameterComments(udfParameters);
			}

			return comments;
		}

		string GetParameterComments(SqlParameter[] udfParameters)
		{
			var parameters = new StringBuilder();

			if (udfParameters.Length > 0)
			{
				parameters.Append("--");
				foreach (var parameter in udfParameters)
				{
					parameters.Append($"{parameter.ParameterName}, {parameter.SqlDbType}, \"{parameter.Value}\"; ");
				}
				parameters.AppendLine();
				parameters.AppendLine();
			}

			return parameters.ToString();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System Report name defined in Documents.xml")]
		bool ShouldAddParameterComments(Report report)
		{
#if DEBUG
			if (IncludeParameterOverrideForTest)
			{
				return true;
			}
#endif
			var reportsToIncludeParameterComments = new[]
			{
				"Shipment Profile Report",
				"Job Profit - Forwarding"
			};

			return reportsToIncludeParameterComments.Contains(report.Name.ToString());
		}

		string GetSQLQueryHints(Report report, bool isStoredProcedure)
		{
			var analyser = report.Analyser;

			if (analyser != null)
			{
				var configSection = analyser.Config;
				if (configSection != null)
				{
					string hints = configSection.SqlQueryHints;

					if (hints.IndexOf(RecompileOption, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						if (configSection.SqlQueryHintsIgnoreRecompile)
						{
							hints = RemoveRecompileFromHints(hints);
						}
					}
					else if (!configSection.SqlQueryHintsIgnoreRecompile && DocumentsDataRegistry.Instance.UseRecompileQueryHint.Value)
					{
						if (hints.Length > 0)
						{
							hints += ", ";
						}

						hints += RecompileOption;
					}

					if (hints.Length > 0)
					{
						if (isStoredProcedure)
						{
							return string.Format((NoResString)" with {0}", hints);
						}

						return ShouldAddMaxDopOption(report) ? string.Format((NoResString)" option ({0}, MAXDOP {1})", hints, GetMaxDopValue(report)) : string.Format((NoResString)" option ({0})", hints);
					}
				}
			}

			if (!isStoredProcedure && ShouldAddMaxDopOption(report))
			{
				return string.Format((NoResString)" option (MAXDOP {0})", GetMaxDopValue(report));
			}

			return "";
		}

		internal bool ShouldAddMaxDopOption(Report report)
		{
			if (report.MaxDop > 0)
			{
				return true;
			}

			if (report.MaxDop == 0)
			{
				return DocumentsDataRegistry.Instance.ReportDBCommandMaximumDegreeOfParallelism.Value > 0;
			}

			return false;
		}

		internal ZInt GetMaxDopValue(Report report)
		{
			return report.MaxDop > 0 ? report.MaxDop : new ZInt(DocumentsDataRegistry.Instance.ReportDBCommandMaximumDegreeOfParallelism.Value);
		}

		protected string RemoveRecompileFromHints(string hints)
		{
			var result = RemoveRecompileRegex.Replace(hints, "");
			return result;
		}

#if DEBUG
		internal bool IncludeParameterOverrideForTest { get; set; }
#endif

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Language Independent SQL Keyword")]
		const string RecompileOption = "recompile";
		static readonly Regex RemoveRecompileRegex = new Regex(
			String.Format(@"(^\s*{0}\b[\s,]*)|([\s,]+{0}\b[\s]*)", RecompileOption),
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded regex")]
		const string OutermostSubQueriesRegexString =
			@"\( # Opening (
   (?>
        [^\(\)]+ # Any characters other than (, or )
      |
        \( (?<DEPTH>) # Opening ( adds one to DEPTH counter
      |
        \) (?<-DEPTH>) # Closing ) subtracts one from DEPTH counter
   )*

   (?(DEPTH)(?!)) # DEPTH must be zero (i.e. brackets must match)
\) # Closing )
";
		static readonly Regex RemoveSubQueriesRegex = new Regex(OutermostSubQueriesRegexString, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

		static readonly Regex RemoveQuotesRegex = new Regex("'[^']*'", RegexOptions.Compiled);
	}
}
