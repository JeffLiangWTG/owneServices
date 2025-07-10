using System;
using System.Data;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CargoWise.Bi.Product.ServiceTask.Maintenance.AuditMaintenanceTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.Maintenance.AuditMaintenanceTask.ServiceTaskName,
	"BI",
	typeof(CargoWise.Bi.Product.ServiceTask.Maintenance.AuditMaintenanceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1month",
	MaximumPeriod = "1month",
	IsReadOnlyForWiseCloudClient = true,
	DefaultScheduleRunEvery = "1month",
	DefaultScheduleDayOfMonth = 1,
	ActiveByDefault = true
	)
]

namespace CargoWise.Bi.Product.ServiceTask.Maintenance
{
	public class AuditMaintenanceTask : ServiceProviderImpl
	{
		#region SuppressResourceStringsCheckRegion

		public const string ServiceTaskCode = "ADM";
		public const string ServiceTaskName = "Audit Database Maintenance"; // Service task name

		[HostedServiceRequirement]
		public static string CheckCanLoadAuditServer() => BiServiceTaskHelpers.CheckAuditEnabledForHostedServiceRequirements();

		string DatabaseServerName
		{
			get
			{
				return auditServer ??
					(auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string auditServer;

		protected virtual string AuditDatabaseName => Db.AuditDatabaseName;

		public override void RunTask(CancellationToken token)
		{
			if (!string.IsNullOrEmpty(DatabaseServerName))
			{
				using (var biConnection = Db.NewAdminConnection(DatabaseServerName, AuditDatabaseName))
				{
					ShrinkAuditDatabase(biConnection, token);
				}
			}
		}

		void ShrinkAuditDatabase(AdminConnection biConnection, CancellationToken token)
		{
			try
			{
				string fileName;
				double totalSpaceMb;
				double spaceUsedMb;
				GetAuditFileProperties(biConnection, out fileName, out totalSpaceMb, out spaceUsedMb);

				if (totalSpaceMb > MinimumDatabaseSizeForShrinkingMb)
				{
					var shrinkDatabaseDone = false;
					if (!token.IsCancellationRequested)
					{
						var targetFileSize = GetTargetFileSize(totalSpaceMb, spaceUsedMb);
						ShrinkDatabase(biConnection, fileName, targetFileSize);
						shrinkDatabaseDone = true;
						var originalTotalSpaceMb = totalSpaceMb;
						GetAuditFileProperties(biConnection, out fileName, out totalSpaceMb, out spaceUsedMb);
						if (totalSpaceMb >= originalTotalSpaceMb)
						{
							ServiceLogger.Log(LogType.Debug, "Shrinking Audit database file failed - no change in space used");
							shrinkDatabaseDone = false;
						}
					}
					if (shrinkDatabaseDone)
					{
						ServiceLogger.Information("Shrinking Audit database file completed");
					}
				}
				else
				{
					ServiceLogger.Debug($"Did not shrink audit DB file '{fileName}' as total space in Mb {totalSpaceMb} <= {MinimumDatabaseSizeForShrinkingMb}");
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.TimeoutExpired)
			{
				ServiceLogger.Warning($"Timeout expired while shrinking Audit database file.\r\n{ex.Message}\r\n{ex.StackTrace}");
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CouldNotAdjustTheSpaceAllocationForFile)
			{
				ServiceLogger.Warning($"DBCC Failure to adjust size. Task will be rescheduled in five minutes.\r\n{ex.Message}\r\n{ex.StackTrace}");
				ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskNextRuntime(ServiceTaskCode, ZDateTimeOffset.UtcNow.AddMinutes(5).ToDateTimeOffsetSafe());
			}
		}

		void GetAuditFileProperties(AdminConnection biConnection, out string fileName, out double totalSpaceMb, out double spaceUsedMb)
		{
			fileName = null;
			totalSpaceMb = 0;
			spaceUsedMb = 0;

			var sqlText = @"
DECLARE @FileName NVARCHAR(128), @TotalSpaceMB FLOAT
SELECT TOP 1 @FileName = [name], @TotalSpaceMB = size * 8.0 / 1024.0 FROM sys.database_files WHERE type = 0
DECLARE @SpaceUsed FLOAT = (select FILEPROPERTY(@FileName, 'SpaceUsed') * 8.0 / 1024.0 As SpaceUsedMB)
SELECT @FileName, @TotalSpaceMB, @SpaceUsed";
			using (var cmd = biConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					fileName = reader.GetString(0);
					totalSpaceMb = reader.GetDouble(1);
					spaceUsedMb = reader.GetDouble(2);

					ServiceLogger.Log(LogType.Debug, $"Audit data file property\r\nUsed Space: {spaceUsedMb} MB, Total Space: {totalSpaceMb} MB, Usage Percentage: {100 * (spaceUsedMb / totalSpaceMb)}%");
				}
			}
		}

		long GetTargetFileSize(double totalSpaceMb, double spaceUsed)
		{
			var targetFileSize = spaceUsed / 0.9;

			if (totalSpaceMb - IncrementalShrinkingSize > targetFileSize)
			{
				targetFileSize = totalSpaceMb - IncrementalShrinkingSize;
			}

			return Convert.ToInt64(targetFileSize, CultureInfo.InvariantCulture);
		}

		protected virtual void ShrinkDatabase(AdminConnection biConnection, string fileName, long targetFileSize)
		{
			ServiceLogger.Log(LogType.Debug, "Shrinking Audit database file");
			var sqlText = "DBCC SHRINKFILE (@FileName , @TargetDatafileSize)";
			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.CommandTimeout = ShrinkDatabaseCommandTimeout;

				cmd.AddParameter("@FileName", SqlDbType.VarChar, 128, fileName);
				cmd.AddParameter("@TargetDatafileSize", SqlDbType.Int, targetFileSize);

				cmd.ExecuteNonQuery();
			}
		}

		protected virtual float MinimumDatabaseSizeForShrinkingMb => 10 * 1024; // Shrinking database will only be executed for databases bigger than 10 GB
		protected virtual float IncrementalShrinkingSize => 5 * 1024; // 5 GB increment for shrinking database size
		protected virtual int ShrinkDatabaseCommandTimeout => 30 * 60; // 30 minute command timeout

		#endregion
	}
}
