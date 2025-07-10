using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"TPC",
	"Temporal Cache Cleaner",
	BMSServiceTaskBase.Category,
	typeof(TemporalCacheCleanerServiceTask),
	MinimumPeriod = "1hour",
	MaximumPeriod = "1month",
	CanRunInAnyBranch = true, 
	IsMandatory = true, 
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "2hours",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class TemporalCacheCleanerServiceTask : ServiceProviderImpl
	{
		protected virtual int BatchSize => 1000;

		protected virtual DbConnection GetConnection() => Db.Connection;

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var now = ZDateTimeOffset.UtcNow.ToDateTimeOffset();
			var totalItemsDeleted = 0;
			var deletedRowCount = 0;

			using (var cn = GetConnection())
			{
				do
				{
					if (youMustReactToThisToken.IsCancellationRequested)
					{
						return;
					}

					deletedRowCount = cn.ExecuteScalar<int>(
						$"DELETE TOP ({BatchSize}) TemporalCache FROM dbo.TemporalCache WITH (INDEX(NR_RX__TPC_ExpiresAtTime)) WHERE TPC_ExpiresAtTime <= @now SELECT @@ROWCOUNT",
						parameters => parameters.AddParameter((NoResString)"@now", System.Data.SqlDbType.DateTimeOffset, now));
					totalItemsDeleted += deletedRowCount;
				} while (deletedRowCount == BatchSize);
			}

			ServiceLogger.Log(Enterprise.Integration.LogType.Information, $"Cache items deleted: {totalItemsDeleted}");
		}
	}
}
