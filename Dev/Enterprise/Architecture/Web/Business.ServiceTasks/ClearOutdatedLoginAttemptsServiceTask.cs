using System;
using System.Data;
using System.Text;
using System.Threading;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ClearOutdatedLoginAttemptsServiceTask.Code,
	ClearOutdatedLoginAttemptsServiceTask.Description,
	"SYS",
	typeof(ClearOutdatedLoginAttemptsServiceTask),
	AllowsMultipleInstances = false,
	MinimumPeriod = "1week",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	ActiveByDefault = true)]

namespace Enterprise.ZArchitecture.Web.Business.ServiceTasks
{
	public class ClearOutdatedLoginAttemptsServiceTask : ServiceProviderImpl
	{
		public const string Code = "CLA";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Clear Outdated Login Attempts";

		[HostedServiceRequirement]
		public static string CheckShouldRun() =>
			WebDataRegistry.Instance.WebLoginLockoutMinutes.Value > 0 || Env.Registry.LoginLockoutMinutes > 0 ?
				string.Empty :
				FormattableString.Invariant($"The registry WebLoginLockoutMinutes or LoginLockoutMinutes need to have a value greater than 0"); // information for logging only.

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			try
			{
				DeleteLogs(WebDataRegistry.Instance.WebLoginLockoutMinutes.Value, OrgContactSchema.Constants.Prefix);
				DeleteLogs(Env.Registry.LoginLockoutMinutes, GlbStaffSchema.Constants.Prefix);
			}
			catch (Exception ex)
			{
				ServiceLogger.Log(LogType.Error, "Error running service task", ex);
			}
		}

#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
		void DeleteLogs(int minutes, string tableCode)
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration
		{
			ServiceLogger.Log(LogType.Information, $"Deleting records for '{tableCode}'");
			ServiceLogger.Log(LogType.Information, $"Login lockout minutes '{minutes}'");

			#region SuppressResourceStringsCheckRegion

			var deleteQuery = new StringBuilder();
			deleteQuery.AppendLine("DECLARE @lockoutTime datetime = DATEADD(MINUTE, -@LockoutMinutes, GETUTCDATE());");
			deleteQuery.AppendLine("SET @lockoutTime = DATEADD(DAY, -7, @lockoutTime); -- Leave the record an extra week in case they are needed for debugging, diagnostic, or audit");
			deleteQuery.AppendLine("DELETE dbo.StmLoginFailureLog");
			deleteQuery.AppendLine("WHERE SFL_TableCode = @TableCode");
			deleteQuery.AppendLine("	  AND @LockoutMinutes > 0");
			deleteQuery.AppendLine("	  AND SFL_SystemCreateTimeUtc < @lockoutTime");

			#endregion

#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
			using (var command = Db.Connection.Command(deleteQuery.ToString()))
			{
				command.AddParameter("@TableCode", SqlDbType.VarChar, tableCode);
				command.AddParameter("@LockoutMinutes", SqlDbType.Int, minutes);
				var affectedRows = command.ExecuteNonQuery();

				ServiceLogger.Log(LogType.Information, $"Deleted {affectedRows} attempt logs");
			}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
		}
	}
}
