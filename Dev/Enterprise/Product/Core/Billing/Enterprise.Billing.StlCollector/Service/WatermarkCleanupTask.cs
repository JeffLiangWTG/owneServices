using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Billing.StlCollector.Service;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	WatermarkCleanupTask.ServiceTaskCode,
	WatermarkCleanupTask.ServiceTaskName,
	"SYS",
	typeof(WatermarkCleanupTask),
	IsMandatory = true,
	MinimumPeriod = "1week",
	MaximumPeriod = "1month",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1month",
	DefaultScheduleDayOfMonth = 1,
	ActiveByDefault = true
	)]

namespace Enterprise.Billing.StlCollector.Service
{
	public class WatermarkCleanupTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "WCS";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "literal String is safe to use in this Context")]
		public const string ServiceTaskName = "Watermark Cleanup Service";

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "literal String is safe to use in this context")]
		public override void RunTask(CancellationToken token)
		{
			var deleteQuery = getQuery();
			using (var cmd = Db.Connection.Command(deleteQuery))
			{
				ServiceLogger.Log(LogType.Information, "[WCS Watermark Cleanup] - Start");
				ServiceLogger.Log(LogType.Information, "Removing all Collector Watermarks over 1 month old, and is not in the active collector list.");
				var logsDeleted = cmd.ExecuteNonQuery();
				ServiceLogger.Log(LogType.Information, logsDeleted == 0 ? "No Watermarks removed" : $"Removed {logsDeleted} Watermarks");
				ServiceLogger.Log(LogType.Information, "[WCS Watermark Cleanup] - End");
			}
		}

		string getQuery()
		{
			var activeStlCollectors = ObjectFactory.Get<IScriptFactory>().CreateScripts(new BusinessObjectFactory() { RefreshEnabled = false }).ToDictionary(f => f.Code.ToUpper());
			var activeCollectorsQueryString = string.Empty;

			foreach (var stl in activeStlCollectors)
			{
				if (!string.IsNullOrEmpty(activeCollectorsQueryString))
				{
					activeCollectorsQueryString += ",";
				}
				activeCollectorsQueryString += $"'WaterBillDate{stl.Key}'";
			}

			var sql = $@"
WITH FilteredWaterBillData AS (
    SELECT SD_Name, 
           SD_BinaryValue, 
           CONVERT(NVARCHAR(MAX), SD_BinaryValue) AS ConvertedValue
    FROM dbo.StmData
    WHERE SD_Name LIKE 'WaterBillDate%' 
    AND SD_Name <> 'WaterBillDate'
)
DELETE 
FROM FilteredWaterBillData
WHERE 
    ((ISDATE(ConvertedValue) = 1
     AND CAST(ConvertedValue AS datetime) < DATEADD(month, -1, GETDATE()))
    OR ISDATE(ConvertedValue) = 0)";

			if (!string.IsNullOrEmpty(activeCollectorsQueryString))
			{
				sql += $@"
	AND SD_Name NOT IN ({activeCollectorsQueryString})";
			}

			return sql;
		}
	}
}
