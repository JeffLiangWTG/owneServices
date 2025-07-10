using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common.Async.Testing;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class DbConnectionCleanerTaskTest : AbstractApplicationStartupTaskTest<DbConnectionCleanerTask>
	{
		public void TestTimeout()
		{
			var oldValue = EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod;
			try
			{
				EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod = 1;
				new DbConnectionCleanerTask().Execute(new ApplicationArguments(Array.Empty<string>()));

				using (var connectionA = Db.NewExtraConnectionToMainDb())
				{
					using (Db.DisposableActionForDbConnection())
					{
						AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
						AssertEquals(System.Data.ConnectionState.Open, connectionA.State);
					}
					for (var i = 0; i < 10; ++i)
					{
						Thread.Sleep(1000);
						DbConnectionCleaner.CloseIdleDbConnections(); //Unfortunately Application_Idle is not called during unit tests, even if you call Application.DoEvents().
						if (connectionA.State == System.Data.ConnectionState.Closed)
						{ break; }
					}
					AssertEquals(System.Data.ConnectionState.Closed, connectionA.State);
					AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
				}
			}
			finally
			{
				ResetDbConnectionCleanerTask(oldValue);
			}
		}

		[GuiTest]
		public void TestTimeout_Multithreaded()
		{
			var oldValue = EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod;
			try
			{
				EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod = 1;
				new DbConnectionCleanerTask().Execute(new ApplicationArguments(Array.Empty<string>()));

				DbConnection connectionA = null;
				DbConnection connectionB = null;

				List<Tuple<SendOrPostCallback, object>> postedCalls = new List<Tuple<SendOrPostCallback, object>>();

				ThreadSwapper
					.SetupOtherThread(Db.DisposableActionForDbConnection)
					.ThenOnMain(() =>
					{
						connectionA = Db.NewExtraConnectionToMainDb();
						using (Db.DisposableActionForDbConnection())
						{
							AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
							AssertEquals(System.Data.ConnectionState.Open, connectionA.State);
						}
					})
					.ThenOnOther(() =>
					{
						Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA);
						SynchronizationContext.SetSynchronizationContext(new DummySynchronizationContext(postedCalls));
						connectionB = Db.NewExtraConnectionToMainDb();
						using (Db.DisposableActionForDbConnection())
						{
							AssertEquals(2, (int)connectionB.ExecuteScalar("Select 1 + 1;"));
							AssertEquals(System.Data.ConnectionState.Open, connectionB.State);
						}
					})
					.ThenOnMain(() =>
					{
						for (var i = 0; i < 10; ++i)
						{
							Thread.Sleep(1000);
							DbConnectionCleaner.CloseIdleDbConnections(); //Unfortunately Application_Idle is not called during unit tests.
							if (connectionA.State == System.Data.ConnectionState.Closed)
							{ break; }
						}
						AssertEquals(System.Data.ConnectionState.Closed, connectionA.State);
						AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
						connectionA?.Dispose();
					})
					.ThenOnOther(() =>
					{
						//Directly call functions of DummySynchronizationContext to simulate DispatcherSynchronizationContext/Application.DoEvents loop happening.
						AssertEquals(System.Data.ConnectionState.Open, connectionB.State);
						AssertEquals(1, postedCalls.Count);
						postedCalls[0].Item1(postedCalls[0].Item2);
						AssertEquals(System.Data.ConnectionState.Closed, connectionB.State);
						AssertEquals(2, (int)connectionB.ExecuteScalar("Select 1 + 1;"));
						connectionB?.Dispose();
					})
					.Go();
			}
			finally
			{
				ResetDbConnectionCleanerTask(oldValue);
			}
		}

		public class DummySynchronizationContext : SynchronizationContext
		{
			public DummySynchronizationContext(List<Tuple<SendOrPostCallback, object>> postedCalls)
			{
				this.postedCalls = postedCalls;
			}

			readonly List<Tuple<SendOrPostCallback, object>> postedCalls;

			public override void Post(SendOrPostCallback d, object state)
			{
				postedCalls.Add(new Tuple<SendOrPostCallback, object>(d, state));
			}
		}

		public void TestTimeout_ConnectionIsGarbageCollected()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var oldValue = EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod;
				try
				{
					DbConnectionCleaner.StopTimer();//Need to make sure that DbConnectionCleaner are not running
					DbConnection.InitializeTimeout();
					var count = DbConnection.TimeoutableConnections.Count();
					var weakref = WeakReferenceToActiveConnection();
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();

					Assert(!weakref.TryGetTarget(out var dummy));

					EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod = 1;
					new DbConnectionCleanerTask().Execute(new ApplicationArguments(Array.Empty<string>()));
					Thread.Sleep(1000);
					DbConnectionCleaner.CloseIdleDbConnections();

					AssertEquals(0, DbConnection.TimeoutableConnections.Count());
				}
				finally
				{
					ResetDbConnectionCleanerTask(oldValue);
				}
			}
		}

		public void TestTimeout_FreshConnectionIsKeptAlive()
		{
			var oldCount = DbConnection.TimeoutableConnections.Count();
			int spid = -1;

			new DbConnectionCleanerTask().Execute(new ApplicationArguments(Array.Empty<string>()));

			using (var connectionA = Db.NewExtraConnectionToMainDb())
			{
				using (Db.DisposableActionForDbConnection())
				{
					AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
					spid = connectionA.SPID;
					AssertEquals(System.Data.ConnectionState.Open, connectionA.State);
				}

				DbConnectionCleaner.CloseIdleDbConnections(); //Unfortunately Application_Idle is not called during unit tests, even if you call Application.DoEvents().

				AssertEquals(System.Data.ConnectionState.Open, connectionA.State);
				AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
				AssertEquals(spid, connectionA.SPID);
			}
		}

		WeakReference<DbConnection> WeakReferenceToActiveConnection()
		{
			var oldCount = DbConnection.TimeoutableConnections.Count();
			var connectionA = Db.NewExtraConnectionToMainDb();
			using (Db.DisposableActionForDbConnection())
			{
				AssertEquals(2, (int)connectionA.ExecuteScalar("Select 1 + 1;"));
				AssertEquals(System.Data.ConnectionState.Open, connectionA.State);
				AssertEquals(oldCount + 1, DbConnection.TimeoutableConnections.Count());
			}
			connectionA.Dispose();
			return new WeakReference<DbConnection>(connectionA);
		}

		void ResetDbConnectionCleanerTask(int timeoutPeriod)
		{
			while (Db.Connection.AppTransactionCount > 0)
			{
				Db.Connection.RollbackTransaction();
			}
			Db.Connection.BeginTransaction();
			EnvProxy.Instance.Registry.DbConnectionTimeoutPeriod = timeoutPeriod;
			new DbConnectionCleanerTask().Execute(new ApplicationArguments(Array.Empty<string>()));
		}

		public override int DefaultErrorExitCode => ExitCodes.DbConnectionCleanerTaskError;
	}
}
