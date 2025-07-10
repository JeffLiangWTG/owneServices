using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public class DotNetRecorderTaskScheduler : AbstractApplicationStartupTask
	{
		internal DotNetRecorderTaskScheduler() { }

		public override string TaskDescription => Res.GetString("CB0CF010-AAF6-42B2-9B86-24746D517146", "Setting .Net version recorder");

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			var dbInternals = (IDbConnectionInternals)Db.Connection;
			var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
			var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
			new DotNetRecorder(dbHandler).RecordDotNetVersionAndRuntimes();
			return true;
		}

		public static DotNetRecorderTaskScheduler Instance { get { return instance; } }

		public override int FailureExitCode => ExitCodes.DotNetRecorderTaskSchedulerError;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "It's only accessed by one thread during the startup of the program")]
		readonly static DotNetRecorderTaskScheduler instance = new DotNetRecorderTaskScheduler();
	}
}
