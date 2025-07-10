using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	HelpErrorStackLineCountImportServiceTask.Code,
	"Help Error Stack Line Count Import",
	"CSP",
	typeof(HelpErrorStackLineCountImportServiceTask),
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "30Minutes",
	DefaultScheduleRunEvery = "1hour"
	)]

namespace Enterprise.Client.EDI.ServiceTask
{
	class HelpErrorStackLineCountImportServiceTask : ServiceProviderImpl
	{
		public const string Code = "HLS";
		readonly BusinessObjectFactory factory = new BusinessObjectFactory();
		readonly HelpErrorLogStackLineExtractor stackLineExtractor = new HelpErrorLogStackLineExtractor();

		public override void RunTask(CancellationToken token)
		{
			var importTime = ZDateTime.UtcNow;
			var lastSyncTime = EDIDataRegistry.Instance.StackLineCountLastImport;

			if (lastSyncTime > importTime) // This is here because of a bug where importTime used local time instead of UTC
			{
				lastSyncTime = importTime.AddMonths(-1).ToDateTime();
			}

			var stackLineCounts = new Dictionary<StackLine, HelpErrorStackLineCount>(new StackLineEqualityComparer());
			ZDateTime rangeStart, rangeEnd;
			rangeStart = lastSyncTime;

			int processedLogsCount = EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs;
			do
			{
				token.ThrowIfCancellationRequested();
				rangeEnd = new ZDateTime(rangeStart.Year, rangeStart.Month, 1).AddMonths(1);
				if (rangeEnd > importTime)
				{
					rangeEnd = importTime;
				}

				ServiceLogger.Log(Integration.LogType.Information, FormattableString.Invariant($"Getting issues from {rangeStart} to {rangeEnd}"));
				var logData = GetLogs(rangeStart, rangeEnd);
				ServiceLogger.Log(Integration.LogType.Information, FormattableString.Invariant($"Processing {logData.Count} issues"));
				processedLogsCount += UpdateStackLineCount(stackLineCounts, logData);

				factory.Save();

				EDIDataRegistry.Instance.StackLineCountLastImport = rangeEnd.ToDateTime();
				EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = processedLogsCount;

				rangeStart = rangeEnd;
			} while (rangeEnd < importTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static List<LogItem> GetLogs(ZDateTime start, ZDateTime end)
		{
			const string LogsQuery = @"
SELECT O.LogXML
FROM dbo.HelpErrorLog AS L
OUTER APPLY (
	SELECT TOP 1 HO_PK
		,dbo.CLRUncompressAsString(HO_CompressedXmlData) AS LogXML
	FROM dbo.HelpErrorLogOccurrence
	WHERE HO_HE = L.HE_PK
	ORDER BY HO_EXEDateTime DESC
	) AS O
WHERE O.HO_PK IS NOT NULL
	AND HE_FirstProcessed >= @start
	AND HE_FirstProcessed < @end
";
			var logs = new List<LogItem>();
			using (var command = Db.Connection.Command(LogsQuery))
			{
				command.AddParameter("@start", System.Data.SqlDbType.DateTime, start.ToDateTime());
				command.AddParameter("@end", System.Data.SqlDbType.DateTime, end.ToDateTime());

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						logs.Add(new LogItem(reader["LogXML"] as string));
					}
				}
			}

			return logs;
		}

		HelpErrorStackLineCount CreateHelpErrorStackLineCount(StackLine call)
		{
			var stackLine = factory.New<HelpErrorStackLineCount>();
			stackLine.HSL_Assembly = call.Assembly;
			stackLine.HSL_Type = call.Type;
			stackLine.HSL_Method = call.Method;
			stackLine.HSL_Parameters = call.Parameters;
			stackLine.HSL_StackLine = call.FullStackLine;
			stackLine.HSL_Count = 1;

			return stackLine;
		}

		IEnumerable<HelpErrorStackLineCount> LoadHelpErrorStackLineCounts(IEnumerable<StackLine> calls)
		{
			var query = new ZQuery(HelpErrorStackLineCountSchema.HSL_StackLine, calls.Select(call => call.FullStackLine).ToArray());
			return factory.Load<HelpErrorStackLineCount>(query);
		}

		int UpdateStackLineCount(Dictionary<StackLine, HelpErrorStackLineCount> stackLineCounts, IEnumerable<LogItem> logData)
		{
			var comparer = new StackLineEqualityComparer();
			var processedLogsCount = 0;

			foreach (var log in logData)
			{
				var calls = GetStackLinesFromLog(log);

				var dbCounts = LoadHelpErrorStackLineCounts(calls.Distinct(comparer).Where(call => !stackLineCounts.TryGetValue(call, out _))).ToDictionary(call => (string)call.HSL_StackLine, StringComparer.OrdinalIgnoreCase);

				var addedHelpErrorStackLineCounts = new List<HelpErrorStackLineCount>();
				foreach (var call in calls)
				{
					if (stackLineCounts.TryGetValue(call, out HelpErrorStackLineCount stackLine))
					{
						CopyStackLineDetailsToHelpErrorStackLineCount(call, stackLine);
						stackLine.HSL_Count++;
					}
					else
					{
						HelpErrorStackLineCount existingCount;
						if (dbCounts.TryGetValue(call.FullStackLine, out existingCount))
						{
							CopyStackLineDetailsToHelpErrorStackLineCount(call, existingCount);
							existingCount.HSL_Count++;
							stackLineCounts[call] = existingCount;
							addedHelpErrorStackLineCounts.Add(existingCount);
						}
						else
						{
							var newCount = CreateHelpErrorStackLineCount(call);
							stackLineCounts[call] = newCount;
							addedHelpErrorStackLineCounts.Add(newCount);
						}
					}
				}
				StackLineAssemblyLookup.UpdateMissingAssemblyInformation(addedHelpErrorStackLineCounts);
				processedLogsCount++;
			}

			return processedLogsCount;
		}

		static void CopyStackLineDetailsToHelpErrorStackLineCount(StackLine call, HelpErrorStackLineCount stackLine)
		{
			if (!string.IsNullOrEmpty(call.Assembly))
			{
				stackLine.HSL_Assembly = call.Assembly;
			}
			if (!string.IsNullOrEmpty(call.Type))
			{
				stackLine.HSL_Type = call.Type;
			}
			if (!string.IsNullOrEmpty(call.Method))
			{
				stackLine.HSL_Method = call.Method;
			}
			if (!string.IsNullOrEmpty(call.Parameters))
			{
				stackLine.HSL_Parameters = call.Parameters;
			}
		}

		IEnumerable<StackLine> GetStackLinesFromLog(LogItem log)
		{
			try
			{
				var calls = stackLineExtractor.ReadStackLines(log.XmlData);
				return calls;
			}
			catch (XmlException)
			{
				// invalid XML
				return Enumerable.Empty<StackLine>();
			}
		}

		class LogItem
		{
			public LogItem(string xmlData)
			{
				XmlData = xmlData;
			}
			public string XmlData { get; }
		}
	}
}
