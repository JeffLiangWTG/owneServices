using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using CargoWise.Common;
using CargoWise.Data;
using Microsoft.Win32;

namespace Enterprise.ZArchitecture.Environment
{
	#region IUtcAndServerTimeZoneWrapper

	interface ISystemTimeZoneWrapper : IDisposable
	{
		DateTime GetCurrentUtc();
		bool IsExpired { get; }
	}

	#endregion

	class UtcDateTimeCache : ISystemTimeZoneWrapper, IDisposable
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public UtcDateTimeCache()
		{
			(dbServerUtc, clientUtcTime, clientPcToDbUtcTimeDelta) = TryReallyHardToGetValues();
			HookTimeChangedSystemEventSafe();
		}

		public bool IsExpired
		{
			get
			{
				var clientPcUtcTime = GetUtcNowFromClientPc();
				return forceExpired
					|| clientUtcTime == DateTime.MinValue
					|| clientPcUtcTime.Subtract(clientUtcTime) > cacheLifeTime
					|| clientPcUtcTime < clientUtcTime
					|| clientPcUtcTime.Date != clientUtcTime.Date;
			}
		}
		bool forceExpired;

#if DEBUG
		public
#endif
		(DateTime dbServerUtc, DateTime clientUtcTime, TimeSpan clientPcToDbUtcTimeDelta) TryReallyHardToGetValues(int count = 0)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					return CreateDateTimeCacheWithExceptionHandling();
				}
			}
			catch (Exception ex)
			{
				if ((ex is SqlTypeException || ex is OverflowException) && (count++ < 3))
				{
					return TryReallyHardToGetValues(count);
				}

				throw GetClientServerDateTimeDeltaTooLargeException(ex);
			}
		}

		/// <summary>
		/// Wraps exception within another with more diagnostic information.
		/// </summary>
		/// <typeparam name="T">Exception type: SqlTypeException, OverflowException</typeparam>
		/// <param name="ex">Original exception</param>
		/// <returns></returns>
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Low level class. Must avoid DB hits.")]
		Exception GetClientServerDateTimeDeltaTooLargeException<T>(T ex) where T : Exception
		{
			try
			{
				var clientPcToDbUtcTimeDeltaReport = string.Format(
					"Client-Server time difference:{0}",
					clientPcToDbUtcTimeDelta.TotalDays.ToString("N"));

				var dateTimeUtcNowReport = string.Format(
					"Client PC time (UTC):{0}\r\nDatabase Server time (UTC):{1}",
					SqlFormatInfo.ToSqlDateTimeString(clientUtcTime),
					SqlFormatInfo.ToSqlDateTimeString(dbServerUtc));

				var message = string.Format(
					"The difference between database time and the workstation time is too large.\r\n{0}\r\n{1}",
					clientPcToDbUtcTimeDeltaReport,
					dateTimeUtcNowReport);

				return (T)Activator.CreateInstance(ex.GetType(), message, ex);
			}
			catch
			{
				return new Exception("The different between database time and the workstation time is too large.", ex);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
#if DEBUG
		protected virtual
#endif
		(DateTime dbServerUtcOnLastCacheRefresh, DateTime cacheLastRefreshClientUtcTime, TimeSpan clientPcToDbUtcTimeDelta) CreateDateTimeCacheWithExceptionHandling()
		{
			try
			{
				return CreateDateTimeCacheCateringForClientServerLatency(Db.Connection);
			}
			catch (CommunicationException)
			{
				try
				{
					// if original exception is (There is already an open DataReader associated with this Command)
					// => retries with an alternative connection
					using (var connection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.SqlMasterDb))
					{
						return CreateDateTimeCacheCateringForClientServerLatency(connection);
					}
				}
				catch
				{
				}
			}
			catch (DatabaseUpgradeException)
			{
				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
			}
			catch (Exception ex)
			{
				//Issue 01255316 -- The added or subtracted value results in an un-representable DateTime.Parameter name: value
				// check in "Previous section thrown" to see what cause the issue
				// prolly need to be removed after fix 
				ErrorReporter.PopulateExceptionsBuffer(ex);
			}

			//deliberate garbage values to make UtcDateTimeCache IsExpired true
			return (DateTime.MinValue, DateTime.MinValue, TimeSpan.MaxValue);
		}

#if DEBUG
		protected internal virtual
#endif
		(DateTime dbServerUtc, DateTime cacheClientUtcTime, TimeSpan clientPcToDbUtcTimeDelta) CreateDateTimeCacheCateringForClientServerLatency(DbConnection connection)
		{
			var newClientUtcAfter = DateTime.MinValue;
			var roundTripLatency = TimeSpan.MaxValue;

			var newDbServerUtc = DateTime.MinValue;
			for (int i = 0; i < 2; i++)
			{
				var clientUtcBefore = GetUtcNowFromClientPc();
				newDbServerUtc = GetUtcNowFromDatabaseServer(connection);
				newClientUtcAfter = GetUtcNowFromClientPc();
				roundTripLatency = newClientUtcAfter.Subtract(clientUtcBefore);

				if (roundTripLatency.TotalSeconds < 60)
				{
					break;
				}
			}

			var oneWayLatency = new TimeSpan(roundTripLatency.Ticks / 2);
			var newClientPcToDbUtcTimeDelta = newDbServerUtc.Subtract(newClientUtcAfter).Add(oneWayLatency);
			return (newDbServerUtc, newClientUtcAfter, newClientPcToDbUtcTimeDelta);
		}

		void SystemEvents_TimeChanged(object sender, EventArgs e)
			=> forceExpired = true;

		void HookTimeChangedSystemEventSafe()
		{
			try
			{
				SystemEvents.TimeChanged += SystemEvents_TimeChanged;
			}
			catch
			{
				// System.Runtime.InteropServices.ExternalException
				// Failed to create system events window thread.
				// * May happen in an unattended session.
				// * => Ignore it and rely on cache expiration
			}
		}

		/// <summary>
		/// Rounds to SqlDateTime precision before return
		/// </summary>
		DateTime GetUtcDateTimeFromCache()
		{
			DateTime clientPcUtcTime, timeFromCache;
			clientPcUtcTime = GetUtcNowFromClientPc();

			try
			{
				timeFromCache = clientPcUtcTime.Add(clientPcToDbUtcTimeDelta);
			}
			catch (ArgumentOutOfRangeException)
			{
				//happens in case where we call GetUtcDateTimeFromCache on an IsExpired UtcDateTimeCache - in this case we're likely desperate
				//(see TimeZoneCollection.cs CurrentUtc() and Issue 01265245)
				//and connecting to the DB is impossible anyway
				//so current UTC is OK.
				timeFromCache = clientPcUtcTime;
			}
			SqlDateTime sqlPrecisionResult = timeFromCache;
			return DateTime.SpecifyKind(sqlPrecisionResult.Value, DateTimeKind.Utc);
		}

		protected virtual DateTime GetUtcNowFromClientPc()
		{
			return DateTime.UtcNow;
		}

		protected virtual DateTime GetUtcNowFromDatabaseServer(DbConnection connection)
		{
			const string currentUtcSqlText = "SELECT getutcdate() UtcDateTime;";
			return (DateTime)connection.ExecuteScalar(currentUtcSqlText);
		}

		static readonly TimeSpan cacheLifeTime = TimeSpan.FromMinutes(30);
		readonly protected DateTime clientUtcTime = DateTime.MinValue;
		readonly protected TimeSpan clientPcToDbUtcTimeDelta = TimeSpan.Zero;
		readonly DateTime dbServerUtc = DateTime.MinValue;

		#region System Time Zones (UTC)

		public DateTime GetCurrentUtc()
			=> GetUtcDateTimeFromCache();

		#endregion

		#region For Test

		public DateTime ClientUtcTime => clientUtcTime;
		public TimeSpan ClientPcToDbTimeDelta => clientPcToDbUtcTimeDelta;

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

		~UtcDateTimeCache()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					SystemEvents.TimeChanged -= SystemEvents_TimeChanged;
				}
			}
			disposed = true;
		}
		bool disposed;

		#endregion
	}
}
