using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Semaphores.Common
{
	public class Heartbeat : IHeartbeat
	{
		protected internal Heartbeat(IHeartbeatInfoFactory sessionInfoFactory, TimeSpan heartbeatDuration, TimeSpan upgradeCheckDuration, IHeartBeatRemoteLogoff logoffHandler, ISemaphoreDbManager dbManager, IUpgradeChecker upgradeChecker, IWindowsTimer windowsTimer = null)
		{
			Argument.NotNull(sessionInfoFactory, nameof(sessionInfoFactory));
			Argument.NotNull(dbManager, nameof(dbManager));
			Argument.NotNull(upgradeChecker, nameof(upgradeChecker));
			Argument.NotNull(logoffHandler, nameof(logoffHandler));

			this.sessionInfo = sessionInfoFactory.New();
			this.heartbeatDuration = heartbeatDuration;
			this.upgradeCheckDuration = upgradeCheckDuration;
			this.logoffHandler = logoffHandler;
			this.windowsTimer = windowsTimer;

			this.dbManager = dbManager;
			this.upgradeChecker = upgradeChecker;
			this.isRefreshingHeartbeatMutexSyncObject = new object();
		}

		public static IHeartbeat New(IHeartbeatInfoFactory sessionInfo, TimeSpan heartbeatDuration, IHeartBeatRemoteLogoff logoffHandler, IWindowsTimer windowsTimer = null)
		{
			Argument.NotNull(sessionInfo, nameof(sessionInfo));

			CheckNotRunningDbUpgrade();

			var newHeartbeat = new Heartbeat(sessionInfo, heartbeatDuration, TimeSpan.FromMinutes(5), logoffHandler, new SemaphoreDbManager(), new UpgradeChecker(), windowsTimer);

			newHeartbeat.Register();
			return newHeartbeat;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Issue message for development team.")]
		static void CheckNotRunningDbUpgrade()
		{
			if (IsRunningDatabaseUpgrade)
			{
				const string message = "Heartbeat must not be initialised during the database upgrade as the keep-alive thread will interfere with it.";
				throw new InvalidOperationException(message);
			}
		}

		static bool IsRunningDatabaseUpgrade
		{
			get { return ReferenceEquals(Db.Connection, Db.AdminConnection); }
		}

		#region IHeartbeat Members

		IHeartbeatInfo IHeartbeat.Info
		{
			get { return sessionInfo; }
		}

		IHeartbeatInfo sessionInfo;

		public Guid HeartbeatId
		{
			get { return heartbeatUniqueId; }
		}

		void IHeartbeat.UpdateUserContext(IHeartbeatInfoFactory heartbeatInfoFactory)
		{
			sessionInfo = heartbeatInfoFactory.New();
			dbManager.UpdateUserContext(heartbeatUniqueId, sessionInfo.HostName, sessionInfo.UserPk, GlbStaffSchema.Constants.Prefix, sessionInfo.HeartbeatType);
		}

		bool IHeartbeat.RegisterIfUserPkNotAlreadyInDatabase()
		{
			if (!dbManager.UserPkInDatabase(sessionInfo.UserPk))
			{
				RegisterCore();
				return true;
			}
			return false;
		}

		public IDisposable TemporarilyDisableTimers()
		{
			StopTimers();
			return new DisposableAction(() =>
			{
				KeepHeartbeatAlive(true);
				StartKeepAliveTimers();
			});
		}

		protected readonly Guid heartbeatUniqueId = Guid.NewGuid();

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);

			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue 
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		~Heartbeat()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					StopHeartbeat();
				}
				else
				{
					StopTimers();
				}
			}
			disposed = true;
		}

		bool disposed;

#if DEBUG
		protected virtual
#endif
		void StopHeartbeat()
		{
			StopTimers();
			dbManager.DeleteHeartbeatFromDatabase(heartbeatUniqueId);
		}

		protected void StopTimers()
		{
			if (threadTimer != null)
			{
				threadTimer.Dispose();
				threadTimer = null;
			}

			if (windowsTimer != null)
			{
				windowsTimer.Stop();
				windowsTimer.Tick -= WindowsTimerCallback;
				windowsTimer.Dispose();
			}
		}

		#endregion

		#region Register Heartbeat

		protected internal void Register()
		{
			if (threadTimer != null)
			{
				throw new InvalidOperationException("Cannot register a Heartbeat more than once");
			}

			RegisterCore();
			StartKeepAliveTimers();
		}

#if DEBUG
		protected virtual
#endif
		void RegisterCore()
		{
			dbManager.CreateHeartbeatInDatabase(heartbeatUniqueId, sessionInfo.HostName, sessionInfo.ProcessId, sessionInfo.UserPk, GlbStaffSchema.Constants.Prefix, (int)heartbeatDuration.TotalSeconds, sessionInfo.HeartbeatType, sessionInfo.ClientIdentifier);
		}

		#endregion

		#region Keep Heartbeat Alive

#if DEBUG
		protected virtual
#endif
		void StartKeepAliveTimers()
		{
			var keepAliveDelegate = new TimerCallback(ThreadTimerCallback);
			if (this.windowsTimer != null)
			{
				threadTimer = new Timer(keepAliveDelegate, null, (uint)(KeepAliveIntervalMs * 1.5), (uint)KeepAliveIntervalMs);

				windowsTimer.Interval = KeepAliveIntervalMs;
				windowsTimer.Tick += WindowsTimerCallback;
				windowsTimer.Start();
			}
			else
			{
				threadTimer = new Timer(keepAliveDelegate, null, (uint)(KeepAliveIntervalMs), (uint)KeepAliveIntervalMs);
			}
		}

		void ThreadTimerCallback(object state)
		{
			using (Db.DisposableActionForDbConnection())
			{
				KeepHeartbeatAlive(isForegroundThread: false);
			}
		}

		void WindowsTimerCallback(object sender, EventArgs e)
		{
			Argument.NotNull(windowsTimer, nameof(windowsTimer));

			windowsTimer.Stop();

			try
			{
				if (!Db.Connection.IsInTransaction)
				{
					KeepHeartbeatAlive(isForegroundThread: true);
				}
			}
			finally
			{
				windowsTimer.Start();
			}
		}

		readonly object isRefreshingHeartbeatMutexSyncObject;
#if DEBUG
		protected virtual
#endif
		void KeepHeartbeatAlive(bool isForegroundThread)
		{
			try
			{
				if (!DbEnv.Instance.IsTimerDisabledDuringDbUpgrade &&
					Monitor.TryEnter(isRefreshingHeartbeatMutexSyncObject, TimeSpan.Zero))
				{
					try
					{
						if (Db.DatabaseUpgradedExceptionHasBeenThrownInConnection)
						{
							return;
						}

						if (!HeartbeatRefresh(!isForegroundThread) || CheckExpiredTimeIsExpired())
						{
							StopTimers();
							logoffHandler.OnRemoteLogoff();
							return;
						}

						var upgradeDateTimeUtc = upgradeChecker.CheckForUpgrade(isForegroundThread, upgradeCheckDuration);
						if (upgradeDateTimeUtc != DateTime.MinValue)
						{
							logoffHandler.OnRemoteUpgradeLogoff(upgradeDateTimeUtc, () => GetSystemUpgradeDateTime() != DateTime.MinValue);
						}
#if !DEBUG
						// Developers use their own debug build of binaries which will not match the version of the binaries in the database.
						// We want to support this scenario, so skip checking for version upgrades on heartbeats in DEBUG builds.
						DbEnv.Instance.ConnectionGuiPlugin.CheckVersionUpgraded();
#endif
					}
					catch (DatabaseUpgradeException ex)
					{
						DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException(ex);
					}
					finally
					{
						Monitor.Exit(isRefreshingHeartbeatMutexSyncObject);
					}
				}
			}
			catch (Exception ex) // when run in a thread timer this is a top level exception handler, unhandled exception will cause the process to crash
			{
				if (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce(null, ex);
				}
			}
		}

		public static DateTime GetSystemUpgradeDateTime()
		{
			var forthcomingUpgPtyValue = DataUtils.LoadDbExtendedPropertyWithNoLock(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);

			if (string.IsNullOrWhiteSpace(forthcomingUpgPtyValue))
			{
				return DateTime.MinValue;
			}

			DateTime dateTimeUpgradeUtc = DateTime.MinValue;
			DateTime dateTimeUpgradeRefreshedUtc = DateTime.MinValue;

			var values = forthcomingUpgPtyValue.Split('|'); // 'upgradeUtc|refreshedUtc'

			if (values.Length != 2 || string.IsNullOrEmpty(values[0]) || string.IsNullOrEmpty(values[1]))
			{
				return DateTime.MinValue;
			}

			if (
				!SqlFormatInfo.TryParseFromSqlDateTime(values[0], out dateTimeUpgradeUtc)
				|| !SqlFormatInfo.TryParseFromSqlDateTime(values[1], out dateTimeUpgradeRefreshedUtc))
			{
				return DateTime.MinValue;
			}

			DateTime utcNow = ZDateTime.UtcNow.ToDateTime();

			if (dateTimeUpgradeUtc > utcNow && Math.Abs((utcNow - dateTimeUpgradeRefreshedUtc).TotalMinutes) <= 3)
			{
				return DateTime.SpecifyKind(dateTimeUpgradeUtc, DateTimeKind.Utc);
			}

			return DateTime.MinValue;
		}

		bool HeartbeatRefresh(bool isNewConnection)
		{
			try
			{
				var refreshed = dbManager.RefreshHeartbeatInDatabase(heartbeatUniqueId, heartbeatDuration, isNewConnection);
				exceptionCountInSequence = 0;
				return refreshed;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Allows to skip one heartbeat only to avoid throwing transient errors in the timer thread.
				// This can be done because the keep alive cycle is less than half of the heartbeat expiring time.
				if (++exceptionCountInSequence > 1)
				{
					throw new HeartbeatIsNotAliveException(ex);
				}
			}
			return true;
		}

		bool CheckExpiredTimeIsExpired()
		{
			return dbManager.CheckHeartbeatExpiredTimeIsExpired(heartbeatUniqueId);
		}

#if DEBUG
		void IHeartbeat.PumpIfNeeded()
		{
			HeartbeatRefresh(isNewConnection: false);
		}
#endif

		int exceptionCountInSequence;
		internal const double DURATION_GUARANTEE_PERCENTAGE = 0.4;

		readonly TimeSpan heartbeatDuration;
		readonly TimeSpan upgradeCheckDuration;
		protected Timer threadTimer;
		readonly IWindowsTimer windowsTimer;
		readonly IHeartBeatRemoteLogoff logoffHandler;

		public int KeepAliveIntervalMs
		{
			get { return (int)(Math.Min(heartbeatDuration.TotalMilliseconds, upgradeCheckDuration.TotalMilliseconds) * DURATION_GUARANTEE_PERCENTAGE); }
		}

#endregion

		readonly ISemaphoreDbManager dbManager;
		readonly IUpgradeChecker upgradeChecker;
	}
}
