using System;
using System.Globalization;

namespace CargoWise.Data.SqlServer
{
	public class LogFullnessProvider : IBacklogInfoProvider
	{
		public const string PerformanceCounterPercentLogUsed = "Percent Log Used"; // Development constant, should not use Res.GetString

		public LogFullnessProvider()
			: this(string.Empty)
		{
		}

		public LogFullnessProvider(string databaseName)
		{
			this.databaseName = string.IsNullOrWhiteSpace(databaseName) ? Db.DatabaseName : databaseName;
		}

		readonly string databaseName;

		public virtual AttemptInGettingBacklog GetCurrentBacklog()
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				if (DbRecoveryModelManager.GetActual(connection, databaseName) != DbRecoveryModel.Full)
				{
					return new AttemptInGettingBacklog(true, string.Empty, new BacklogResult());
				}

				var sql = @"
					Declare @result int = (Select CAST(cntr_value as int) Value
					From sys.dm_os_performance_counters
					Where counter_name = @CounterName
						AND instance_name = @DbName)
					Select ISNULL(@result, 0)";

				using (var command = connection.Command(sql))
				{
					command.AddParameter("@DbName", System.Data.SqlDbType.NVarChar, 128, databaseName);
					command.AddParameter("@CounterName", System.Data.SqlDbType.NVarChar, 128, PerformanceCounterPercentLogUsed);

					var currentBacklogResult = new BacklogResult();
					currentBacklogResult.BacklogSize = Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
					currentBacklogResult.BacklogDescription = string.Format(CultureInfo.InvariantCulture,
						"transaction log fullness (current: {0}%, acceptable: {1}%)" // this is a log information
						, currentBacklogResult.BacklogSize // 0
						, AcceptableBacklog         // 1
						);

					return new AttemptInGettingBacklog(true, string.Empty, currentBacklogResult);
				}
			}
		}

		public long AcceptableBacklog => 80;

		public TimeSpan[] Timespans { get; } =
			{
				TimeSpan.FromSeconds(10),
				TimeSpan.FromSeconds(20),
				TimeSpan.FromSeconds(30),
				TimeSpan.FromSeconds(60),
				TimeSpan.FromSeconds(120)
			};
	}
}
