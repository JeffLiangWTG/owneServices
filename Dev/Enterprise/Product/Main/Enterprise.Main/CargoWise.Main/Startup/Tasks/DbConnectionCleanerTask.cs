using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup
{
	public class DbConnectionCleanerTask : AbstractApplicationStartupTask
	{
		public override string TaskDescription => string.Empty;

		public override int FailureExitCode => ExitCodes.DbConnectionCleanerTaskError;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			if (!Globals.IsWeb)
			{
				try
				{
					DbConnectionCleaner.SetupTimer();
				}
				catch (System.Data.Common.DbException ex) when (ex.IsTimeoutExpired())
				{
					return false;
				}
			}

			return true;
		}

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;
	}

	public static class DbConnectionCleaner
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It's initialized once")]
		internal static Timer timer;

		internal static void SetupTimer()
		{
			if (EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod > 0)
			{
				Db.Connection.ShouldStopDbConnectionCleaner = false;
				if (timer == null)
				{
					DbConnection.InitializeTimeout();
					timer = new Timer();
					timer.Tick += timer_Tick;
				}
				timer.Interval = EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod * 1000;
				timer.Start();
			}
		}

		public static void StopTimer()
		{
			if (timer != null)
			{
				timer.Stop();
				timer = null;
			}
		}

		static void timer_Tick(object o, EventArgs e)
		{
			Application.Idle -= new EventHandler(Application_Idle);
			Application.Idle += new EventHandler(Application_Idle);
		}

		static void Application_Idle(object sender, EventArgs e)
		{
			Application.Idle -= new EventHandler(Application_Idle);
			CloseIdleDbConnections();
		}

		static bool IsRunningDatabaseUpgrade
		{
			get { return ReferenceEquals(Db.Connection, Db.AdminConnection); }
		}

		internal static void CloseIdleDbConnections()
		{
			if (Db.Connection.DatabaseUpgradedExceptionHasBeenThrown || Db.Connection.ShouldStopDbConnectionCleaner || IsRunningDatabaseUpgrade)
			{
				StopTimer();
				return;
			}
			try
			{
				foreach (DbConnection connection in DbConnection.TimeoutableConnections)
				{
					CloseAndRemoveIfTimedOut(connection);
				}
			}
			catch (DatabaseUpgradedException ex)
			{
				StopTimer();
				DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException(ex);
			}
		}

		static void CloseAndRemoveIfTimedOut(DbConnection target)
		{
			if (target.TimeSinceLastActive > TimeSpan.FromMilliseconds(timer.Interval) && !target.IsInTransactionOtherThanTransactionedTestCase && !target.HasSqlLocks)
			{
				target.CloseAndRemoveDueToTimeOut();
			}
		}
	}
}
