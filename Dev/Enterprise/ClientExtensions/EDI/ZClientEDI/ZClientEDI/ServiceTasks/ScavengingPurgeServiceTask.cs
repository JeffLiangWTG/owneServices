using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ScavengingImportServiceTask;
using Enterprise.Client.EDI.ScavengingImportServiceTask.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"SPG",
	"Scavenging Purge Service Task",
	"ESV",
	typeof(Enterprise.Client.EDI.ServiceTasks.ScavengingPurgeServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.ServiceTasks
{
	public class ScavengingPurgeServiceTask : ServiceProviderImpl
	{
		public ScavengingPurgeServiceTask()
			: this(1000)
		{
		}

		internal ScavengingPurgeServiceTask(int recordsToPurge)
		{
			this.recordsToPurge = recordsToPurge;
		}

		public override void RunTask(CancellationToken token)
		{
			var waiter = new LowPriorityProcessPauser();
			var settings = EDIDataRegistry.Instance.ScavengingPurgeSettings.Value;

			Log("Starting purge.");
			foreach (var scavengingItem in settings.OfType<ScavengingPurgeItem>())
			{
				token.ThrowIfCancellationRequested();

				var purgeResult = new PurgeResult();
				do
				{
					token.ThrowIfCancellationRequested();
					waiter.Wait(ServiceLogger);
					purgeResult = Purge(scavengingItem);
					Log(string.Format(CultureInfo.InvariantCulture, "{0} {1} ({2}) items have been purged.", purgeResult.ItemsDeleted, scavengingItem.Code, scavengingItem.Description));
				} while (purgeResult.ItemsDeleted == recordsToPurge);
			}
			Log("Finished purge.");
		}

		#region Buffer

		void Log(string message)
		{
			Buffer.Notify(new InfoNotification(message));
		}

		NotificationBuffer Buffer
		{
			get { return buffer ?? (buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber())); }
		}

		#endregion

		#region Implementation

		class PurgeResult
		{
			public int ItemsDeleted { get; set; }
			public ZDateTime MaxDeletedItemCreateTime { get; set; }
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		PurgeResult Purge(ScavengingPurgeItem purgeItem)
		{
			var maxDateTime = ScavengingPurgeSettings.GetMaxPurgeDateTime(purgeItem);
			Func<ZDateTime, int, PurgeResult> purgeMethod;
			if (purgeMethods.TryGetValue(purgeItem.Code, out purgeMethod))
			{
				return purgeMethod(maxDateTime, recordsToPurge);
			}
			Log(string.Format("Unknown scavenging item code {0}", purgeItem.Code));
			return new PurgeResult();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static PurgeResult PurgeClientStatisticsArchive(ZDateTime maxDateTime, int recordsToPurge)
		{
			var query = string.Format(@"
DECLARE @MessagePKs TABLE (PK uniqueidentifier PRIMARY KEY, InsertUTC datetime);

INSERT INTO @MessagePKs
SELECT TOP({0}) IMA_PK, IMA_InsertUTC
FROM dbo.ClientStatisticsXMLArchive
WHERE IMA_InsertUTC < @maxInsertTime;

DELETE FROM dbo.ClientStatisticsXMLArchive
WHERE IMA_PK IN (SELECT PK FROM @MessagePKs);

SELECT COUNT(*) FROM @MessagePKs;
SELECT MAX(InsertUTC) FROM @MessagePKs;
", recordsToPurge);
			using (var cmd = Db.Connection.Command(query, CommandTimeout))
			{
				cmd.AddParameter("@maxInsertTime", SqlDbType.DateTime, maxDateTime.ToDateTime());
				using (var reader = cmd.ExecuteReader())
				{
					var result = new PurgeResult();
					if (reader.Read())
					{
						result.ItemsDeleted = reader.GetInt32(0);
					}
					if (reader.Read())
					{
						result.MaxDeletedItemCreateTime = reader.GetDateTime(0);
					}
					return result;
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static PurgeResult PurgeClientOrgImportHistory(ZDateTime maxDateTime, int recordsToPurge)
		{
			var result = new PurgeResult();
			if (ScavengingPartitioner.PartitionExists(maxDateTime))
			{
				var rowCount = ScavengingPartitioner.GetRowCountOfFileGroup(maxDateTime);
				ScavengingPartitioner.TruncatePartition(maxDateTime, AutoClientOrgImportHistory.Schema.TableName);
				result.ItemsDeleted = rowCount;
				result.MaxDeletedItemCreateTime = maxDateTime;
			}

			return result;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static PurgeResult PurgeClientOrgConsol(ZDateTime maxDateTime, int recordsToPurge)
		{
			var query = string.Format(@"
DECLARE @OrgConsolPKs TABLE (PK uniqueidentifier PRIMARY KEY, DateTimeUTC datetime);

INSERT INTO @OrgConsolPKs
SELECT TOP({0}) O7_PK, O7_ExportUTC
FROM dbo.ClientOrgConsol
WHERE O7_ExportUTC < @maxInsertTime;

DELETE FROM dbo.ClientOrgConsol
WHERE O7_PK IN (SELECT PK FROM @OrgConsolPKs);

SELECT COUNT(*) FROM @OrgConsolPKs;
SELECT MAX(DateTimeUTC) FROM @OrgConsolPKs;
", recordsToPurge);
			using (var cmd = Db.Connection.Command(query, CommandTimeout))
			{
				cmd.AddParameter("@maxInsertTime", SqlDbType.DateTime, maxDateTime.ToDateTime());
				using (var reader = cmd.ExecuteReader())
				{
					var result = new PurgeResult();
					if (reader.Read())
					{
						result.ItemsDeleted = reader.GetInt32(0);
					}
					if (reader.Read())
					{
						result.MaxDeletedItemCreateTime = reader.GetDateTime(0);
					}
					return result;
				}
			}
		}

		readonly int recordsToPurge;
		NotificationBuffer buffer;
		const int CommandTimeout = 600;

		readonly Dictionary<string, Func<ZDateTime, int, PurgeResult>> purgeMethods = new Dictionary<string, Func<ZDateTime, int, PurgeResult>>()
		{
			{ ScavengingPurgeSettings.ClientStatisticsArchiveCode, PurgeClientStatisticsArchive },
			{ ScavengingPurgeSettings.ClientOrgConsolCode, PurgeClientOrgConsol },
			{ ScavengingPurgeSettings.ClientOrgImportHistory, PurgeClientOrgImportHistory }
		};

		#endregion // Implementation
	}
}
