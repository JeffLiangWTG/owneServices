using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.ServiceManager.Tasks.SystemServices;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("ATC",
	"Access Token Cleanup",
	"SYS",
	typeof(AccessTokenCleanupTask),
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MinimumPeriod = "1day",
	MaximumPeriod = "1week",
	DefaultScheduleRunEvery = "2days"
	)]

namespace Enterprise.ServiceManager.Tasks.SystemServices
{
	public sealed class AccessTokenCleanupTask : ServiceProviderImpl
	{
		public int BatchSize { get; set; } = 1000;

		public override void RunTask(CancellationToken cancellationToken)
		{
			int numRowsAffected;
			do
			{
				cancellationToken.ThrowIfCancellationRequested();

				numRowsAffected = Db.Connection.ExecuteNonQuery(
					FormattableString.Invariant(
						$@"
DELETE TOP({BatchSize}) FROM {StmAccessTokenSchema.Constants.SqlSchemaName}.{StmAccessTokenSchema.Constants.TableName} WITH (READPAST, READCOMMITTEDLOCK)
WHERE
	{StmAccessTokenSchema.Constants.SAT_IsPermanentToken} = 0
AND
(

	{StmAccessTokenSchema.Constants.SAT_RemainingUseCount} <= 0
	OR
	{StmAccessTokenSchema.Constants.SAT_ExpiresAt} <= SYSUTCDATETIME()
)
AND
	{StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc} <= DATEADD(DAY, -7, SYSUTCDATETIME())
"));
			} while (numRowsAffected > 0);
		}
	}
}
