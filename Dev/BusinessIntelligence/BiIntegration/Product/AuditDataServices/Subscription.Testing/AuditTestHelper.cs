namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using CargoWise.Application;
	using CargoWise.Bi.Common;
	using CargoWise.Bi.Common.Testing;
	using CargoWise.Data;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.ChangeDataCapture.Common;
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Core;
	using NUnit.Framework;

	public static class AuditTestHelper
	{
		public static void AssertLog(IEnumerable<string> actualLogs, params string[] expectedLogs)
		{
			var actualLogWithoutDiagnosticMessages = actualLogs.Where(l => !l.StartsWith(">"));
			string actualLogSingleString = string.Join("\r\n", actualLogWithoutDiagnosticMessages);

			var expectedLogWithoutDiagnosticMessages = expectedLogs.Where(l => !l.StartsWith(">"));
			string expectedLogSingleString = string.Join("\r\n", expectedLogWithoutDiagnosticMessages);

			AssertionWithHtml.CombineAssertions(
				"Actual Output Log:\r\n" + actualLogSingleString + "\r\nExpectedOutput Log:\r\n" + expectedLogSingleString + "\r\n",
				() =>
				{
					Assertion.AssertEquals("Number of log entries", expectedLogs.Length, actualLogWithoutDiagnosticMessages.Count());

					foreach (string expectedLog in expectedLogs)
					{
						Assertion.Assert("*" + expectedLog + "* not found in the output log.", actualLogs.Any(log => log.Contains(expectedLog, StringComparison.OrdinalIgnoreCase)));
					}
				});
		}
		public static void AssertLog(IEnumerable<BetterLogForTest> actualLog, params BetterLogForTest[] expectedLogs)
		{
			var actualLogWithoutDiagnosticMessages = actualLog.Where(l => !l.message.StartsWith(">"));
			string actualLogSingleString = string.Join("\r\n", actualLogWithoutDiagnosticMessages);

			AssertionWithHtml.CombineAssertions(
				"Actual Output Log:\r\n\r\n" + actualLogSingleString + "\r\n",
				() =>
				{
					Assertion.AssertEquals("Number of log entries", expectedLogs.Length, actualLogWithoutDiagnosticMessages.Count());

					foreach (var expectedLog in expectedLogs)
					{
						Assertion.Assert($"Message: {expectedLog.message}, Type: {expectedLog.type}, Exception: {expectedLog.ex} not found in the output log.", actualLog.Contains(expectedLog));
					}
				});
		}

		public static void CleanupAuditTestData(DbConnection auditConnection, params IAuditSubscriber[] testSubscribers)
		{
			var cmdBuilder = new StringBuilder();

			cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
				"TRUNCATE TABLE [{0}].LsnTimeMapping;",
				BiConstants.BiAdminSchemaName
			).AppendLine();

			cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
				"TRUNCATE TABLE [{0}].CdcHistorySummary;",
				BiConstants.BiAdminSchemaName
			).AppendLine();

			cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
				"TRUNCATE TABLE [{0}].SubscriberControl;",
				BiConstants.BiAdminSchemaName
			).AppendLine();

			cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
				"TRUNCATE TABLE [{0}].MasterState;",
				BiConstants.BiAdminSchemaName
			).AppendLine();

			foreach (var subscriber in testSubscribers)
			{
				if (typeof(IActualDataChangesAuditSubscriber).IsAssignableFrom(subscriber.GetType()))
				{
					var dataChangeSubscriber = (ActualDataChangesAuditSubscriber)subscriber;
					cmdBuilder.AppendLine(GetTruncateTableScript(dataChangeSubscriber.Table));
				}
				else if (typeof(IChangedTableListOnlyAuditSubscriber).IsAssignableFrom(subscriber.GetType()))
				{
					var changedTableListSubscriber = (ChangedTableListOnlyAuditSubscriber)subscriber;

					if (changedTableListSubscriber.SubscribedTables != null)
					{
						foreach (var table in changedTableListSubscriber.SubscribedTables)
						{
							cmdBuilder.AppendLine(GetTruncateTableScript(table));
						}
					}
				}
				else if (typeof(ITableValuePairSubscriber).IsAssignableFrom(subscriber.GetType()))
				{
					var changedTableListSubscriber = (TableValuePairSubscriber)subscriber;
					var tablesFromCdcConfig = CdcConfigurationInfo.GetEligibleCdcTables(Db.Connection);
					var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();

					foreach (var cdcConfigTable in tablesFromCdcConfig)
					{
						var tableSchema = schemaResolver.GetTableSchema(cdcConfigTable.TableName);
						if (tableSchema != null)
						{
							cmdBuilder.AppendLine(GetTruncateTableScript(tableSchema));
						}
					}
				}
				else
				{
					throw new Exception($"{subscriber.Code} subscriber is not a valid type of IAuditSubscriber");
				}

				cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
					"DELETE [{0}].SubscriberControl WHERE SubscriberCode = '{1}';",
					BiConstants.BiAdminSchemaName,
					subscriber.Code
				).AppendLine();
			}

			auditConnection.ExecuteNonQuery(cmdBuilder.ToString());
		}

		public static void ResetSubscriberControlWaterMark(DbConnection auditConnection, params IAuditSubscriber[] testSubscribers)
		{
			foreach (var subscriber in testSubscribers)
			{
				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
					MERGE [{0}].SubscriberControl AS tgt
						USING (SELECT '{1}') AS src (SubscriberCode) ON (tgt.SubscriberCode = src.SubscriberCode)
					WHEN MATCHED THEN
						UPDATE SET LsnHighWaterMark = 0x, PeriodHighWaterMark = 0
					WHEN NOT MATCHED THEN
						INSERT (SubscriberCode, LsnHighWaterMark, PeriodHighWaterMark) VALUES (src.SubscriberCode, 0x, 0);",
					BiConstants.BiAdminSchemaName,
					subscriber.Code
				);

				auditConnection.ExecuteNonQuery(sqlText);
			}
		}

		static string GetTruncateTableScript(ITableSchema table)
		{
			return (table == null)
				? ""
				: string.Format(CultureInfo.InvariantCulture,
					"IF (SELECT object_id('[{0}].[{1}]')) IS NOT NULL EXEC('TRUNCATE TABLE [{0}].[{1}]');",
					table.SqlSchemaName,
					table.TableName);
		}

		public static DbConnection GetAuditConnection()
		{
			return Db.NewAdminConnection(Db.ServerName, Db.AuditDatabaseName);
		}

		public static void RunNotificationCycleAndAssert(DbConnection auditConnection, ActualDataChangesAuditSubscriber[] testSubscribers, string temporaryMaxLsn, string temporaryMaxPeriod, BetterLogForTest expectedLog)
		{
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, temporaryMaxLsn))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, temporaryMaxPeriod))
			{
				var testLogger = new BetterLoggerForTest();
				foreach (var testSubscriber in testSubscribers)
				{
					var wrappedSub = testSubscriber.GetWrapper(auditConnection, testLogger);
					wrappedSub.ShouldRunSubscriber();
					wrappedSub.FetchDataAndProcessChanges();
					if (expectedLog is null)
					{
						AssertLog(testLogger.Logs, Array.Empty<BetterLogForTest>());
					}
					else
					{
						AssertLog(testLogger.Logs, expectedLog);
					}
				}
			}
		}
	}

	public class BetterLoggerForTest : ILogger
	{
		public void Log(LogType type, string message, Exception ex)
		{
			Logs.Add(new BetterLogForTest(type, message, ex));
		}

		public void Log(LogType type, string message)
		{
			Logs.Add(new BetterLogForTest(type, message, null));
		}

		public List<BetterLogForTest> Logs
		{
			get { return logs ?? (logs = new List<BetterLogForTest>()); }
		}

		List<BetterLogForTest> logs;
	}

	public class BetterLogForTest
	{
		public BetterLogForTest(LogType type, string message, Exception ex)
		{
			this.type = type;
			this.message = message;
			this.ex = ex;
		}

		public BetterLogForTest(LogType type, string message)
			: this(type, message, null)
		{
		}

		public override bool Equals(object obj)
		{
			BetterLogForTest rhs = obj as BetterLogForTest;
			return rhs != null && type == rhs.type && message == rhs.message;
		}

		public override int GetHashCode()
		{
			return type.GetHashCode() ^ message.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", type, message);
		}

		public readonly LogType type;
		public readonly string message;
		public readonly Exception ex;
	}
}
