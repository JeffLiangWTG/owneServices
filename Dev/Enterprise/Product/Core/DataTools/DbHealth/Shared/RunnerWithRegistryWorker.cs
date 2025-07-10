using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DbHealth.Shared
{
	public abstract class RunnerWithRegistryWorker
	{
		#region Properties

		protected IDbRegistryWorker worker;
		protected virtual IDbRegistryWorker RegistryWorker
		{
			get
			{
				return worker ?? (worker = GetNewWorker());
			}
		}

		public abstract IDbRegistryWorker GetNewWorker();

		#endregion

		#region Constructor

		protected RunnerWithRegistryWorker()
		{
		}

		#endregion

		#region ICheckerBase Members

		public void Run(DbConnection connection, IEnumerable<string> dbNames)
		{
			Argument.NotNull(dbNames, nameof(dbNames));
			timeoutwatch = Stopwatch.StartNew();

			RegistryWorker.LoadStartPoint();
			ExecuteCommandsForEachDatabase(connection, dbNames);
			RegistryWorker.SaveCurrentRunningStep(timeoutwatch.Elapsed);

			if (RegistryWorker.IsStuckAtInitialPoint)
			{
				HandleRegistryWorkerIsStuck();
			}
		}

		protected abstract void HandleRegistryWorkerIsStuck();

		#endregion

		#region Timeouts and registry stuff

		protected abstract int InitialTimeoutMs { get; }

		#endregion

		#region Checks

		void ExecuteCommandsForEachDatabase(DbConnection connection, IEnumerable<string> dbNames)
		{
			Argument.NotNull(dbNames, nameof(dbNames));
			var databasesFromStartDb = RegistryWorker.GetDbListFromStartDb(dbNames);

			try
			{
				foreach (string dbName in databasesFromStartDb)
				{
					RegistryWorker.CurrentDatabase = dbName;
					RunOnGivenDB(connection, dbName);
				}

				RegistryWorker.ResetCurrentValues();
			}
			catch (CustomTimeoutException)
			{
				// DO NOTHING: Current Running Step will be saved to registry
			}
		}

		protected abstract void RunOnGivenDB(DbConnection connection, string dbName);

		#endregion

		#region Execute DBCC commands controling total elapsed time

		protected virtual void ExecuteCommand(DbCommand cmd)
		{
			Argument.NotNull(cmd, nameof(cmd));
			cmd.CommandTimeout = CheckAndReturnRemainingTimeoutInSeconds();

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (SqlException ex)
			{
				DbErrorMatch errorMatch = new DbErrorMatch(ex);

				if (errorMatch.ExceptionType == DbErrorType.TimeoutExpired)
				{
					throw new CustomTimeoutException(CustomExceptionMessage);
				}
				else
				{
					throw;
				}
			}
		}

		protected virtual int CheckAndReturnRemainingTimeoutInSeconds()
		{
			long elapsedMilliseconds = timeoutwatch.ElapsedMilliseconds;

			if (elapsedMilliseconds >= InitialTimeoutMs)
			{
				throw new CustomTimeoutException(CustomExceptionMessage);
			}

			return (InitialTimeoutMs - (int)elapsedMilliseconds) / 1000;
		}

		protected abstract string CustomExceptionMessage { get; }

		Stopwatch timeoutwatch = new Stopwatch();

		#endregion

		#region SqlApplicationLock

		protected void RunWithAppLock(string dbName, Action action)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			RunWithAppLock(dbName, null, action);
		}

		protected void RunWithAppLock(string dbName, string key, Action action)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (string.IsNullOrWhiteSpace(key))
			{
				key = "ISU-" + dbName;
			}

			using (var appLockConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, dbName))
			using (appLockConnection.BeginTransactionWithManager())
			{
				var lockGranted = false;
				using (var cmd = appLockConnection.Command("sp_getapplock"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, key);
					cmd.AddParameter("@LockMode", SqlDbType.VarChar, 32, "Exclusive");
					cmd.AddParameter("@LockOwner", SqlDbType.VarChar, 32, "Transaction");
					cmd.AddParameter("@LockTimeout", SqlDbType.Int, 0);

					lockGranted = cmd.ExecuteProcedureWithReturnValue() >= 0;
				}

				if (lockGranted)
				{
					action?.Invoke();
				}
			}
		}

		#endregion // SqlApplicationLock

		#region Custom timeout exception

		[Serializable]
		public class CustomTimeoutException : Exception
		{
			public CustomTimeoutException(string message)
			{
			}

#if NETFRAMEWORK
			protected CustomTimeoutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			string CustomMessage { get; set; }

			public override string Message
			{
				get
				{
					return string.IsNullOrEmpty(CustomMessage) ? "Command timed out" : CustomMessage;
				}
			}
		}

		#endregion
	}
}
