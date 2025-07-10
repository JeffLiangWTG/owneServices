using CargoWise.Data;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check
{
	sealed class SlowInstantFileInitializationCheckerTest : TestCase
	{
		const int currentSpid = 42;
		const int otherSpid = 16;
		readonly TestServiceLogger logger = new TestServiceLogger();

		public void TestIsInstantFileInitializationEnabledGivenInsufficientLogRecordsReturnsFalse()
		{
			var strategy = CreateStrategy($@"
		  select '2017-12-14 17:00:29.550', 'spid{currentSpid}', 'DBCC TRACEON 3004, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.550', 'spid{currentSpid}', 'DBCC TRACEON 3605, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.570', 'spid{currentSpid}', 'Zeroing filepath\OdysseyDat_DbChecker.ldf from page 0 to 1024 (0x0 to 0x800000)'
union all select '2017-12-14 17:00:29.590', 'spid{currentSpid}', 'Zeroing completed on filepath\OdysseyDat_DbChecker.ldf (elapsed = 13 ms)'
union all select '2017-12-14 17:00:29.595', 'spid{currentSpid}', 'Setting database option DISABLE_BROKER to ON for database ''{Db.DatabaseName}_DbChecker''.'
");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals(false, result);
		}

		public void TestIsInstantFileInitializationEnabledIgnoresLogsFromOtherSpid()
		{
			var strategy = CreateStrategy($@"
		  select '2017-12-14 17:00:29.550', 'spid{otherSpid}', 'Zeroing filepath\OdysseyDat_DbChecker.ldf from page 0 to 1024 (0x0 to 0x800000)'
union all select '2017-12-14 17:00:29.550', 'spid{currentSpid}', 'DBCC TRACEON 3605, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.570', 'spid{otherSpid}', 'Zeroing completed on filepath\OdysseyDat_DbChecker.ldf (elapsed = 13 ms)'
union all select '2017-12-14 17:00:29.590', 'spid{otherSpid}', 'Starting up database ''{Db.DatabaseName}_DbChecker''.'
union all select '2017-12-14 17:00:29.630', 'spid{currentSpid}', 'Zeroing filepath\OdysseyDat_DbChecker.ldf from page 0 to 1024 (0x0 to 0x800000)'
union all select '2017-12-14 17:00:29.670', 'spid{currentSpid}', 'Zeroing completed on filepath\OdysseyDat_DbChecker.ldf (elapsed = 13 ms)'
union all select '2017-12-14 17:00:29.670', 'spid{otherSpid}', 'FixupLogTail(progress) zeroing filepath\OdysseyDat_DbChecker.ldf from 0x5000 to 0x6000.'
union all select '2017-12-14 17:00:29.670', 'spid{otherSpid}', 'Setting database option DISABLE_BROKER to ON for database ''{Db.DatabaseName}_DbChecker''.'
union all select '2017-12-14 17:00:29.800', 'spid{currentSpid}', 'Setting database option DISABLE_BROKER to ON for database ''{Db.DatabaseName}_DbChecker''.'
");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals(false, result);
		}

		public void TestIsInstantFileInitializationEnabledGivenSufficientLogRecordsReturnsTrue()
		{
			var strategy = CreateStrategy($@"
		  select '2017-12-14 17:00:29.570', 'spid{currentSpid}', 'Zeroing filepath\OdysseyDat_DbChecker.ldf from page 0 to 1024 (0x0 to 0x800000)'
union all select '2017-12-14 17:00:29.590', 'spid{currentSpid}', 'Zeroing completed on filepath\OdysseyDat_DbChecker.ldf (elapsed = 13 ms)'
union all select '2017-12-14 17:00:29.630', 'spid{currentSpid}', 'Starting up database ''{Db.DatabaseName}_DbChecker''.'
union all select '2017-12-14 17:00:29.670', 'spid{currentSpid}', 'FixupLogTail(progress) zeroing filepath\OdysseyDat_DbChecker.ldf from 0x5000 to 0x6000.'
union all select '2017-12-14 17:00:29.675', 'spid{currentSpid}', 'Setting database option DISABLE_BROKER to ON for database ''{Db.DatabaseName}_DbChecker''.'
");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals(true, result);
		}

		public void TestIsInstantFileInitializationEnabledGivenRepeatedLogRecordsReturnsTrue()
		{
			var strategy = CreateStrategy(
				$@"
		  select '2017-12-14 17:00:29.550', 'spid{currentSpid}', 'DBCC TRACEON 3004, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.550', 'spid{currentSpid}', 'DBCC TRACEON 3605, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.570', 'spid{currentSpid}', 'Zeroing filepath\OdysseyDat_DbChecker.ldf from page 0 to 1024 (0x0 to 0x800000)'
union all select '2017-12-14 17:00:29.590', 'spid{currentSpid}', 'Zeroing completed on filepath\OdysseyDat_DbChecker.ldf (elapsed = 13 ms)'
union all select '2017-12-14 17:00:29.630', 'spid{currentSpid}', 'Starting up database ''{Db.DatabaseName}_DbChecker''.'
union all select '2017-12-14 17:00:29.670', 'spid{currentSpid}', 'FixupLogTail(progress) zeroing filepath\OdysseyDat_DbChecker.ldf from 0x5000 to 0x6000.'
union all select '2017-12-14 17:00:29.670', 'spid{currentSpid}', 'Zeroing filepath\OdysseyDat_DbChecker.ldf from page 3 to 249 (0x6000 to 0x1f2000)'
union all select '2017-12-14 17:00:29.670', 'spid{currentSpid}', 'Zeroing completed on filepath\OdysseyDat_DbChecker.ldf (elapsed = 2 ms)'
union all select '2017-12-14 17:00:29.800', 'spid{currentSpid}', 'DBCC TRACEOFF 3004, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.800', 'spid{currentSpid}', 'DBCC TRACEOFF 3605, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.910', 'spid{currentSpid}', 'Starting up database ''{Db.DatabaseName}_DbChecker''.'
union all select '2017-12-14 17:00:29.910', 'spid{currentSpid}', 'Setting database option DISABLE_BROKER to ON for database ''{Db.DatabaseName}_DbChecker''.'

union all select '2017-12-14 17:00:29.550', 'spid{currentSpid}', 'DBCC TRACEON 3605, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.570', 'spid{currentSpid}', 'Zeroing filepath\OdysseyDat_DbChecker.ldf from page 0 to 1024 (0x0 to 0x800000)'
union all select '2017-12-14 17:00:29.800', 'spid{currentSpid}', 'DBCC TRACEOFF 3605, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.910', 'spid{currentSpid}', 'Starting up database ''{Db.DatabaseName}_DbChecker''.'
union all select '2017-12-14 17:00:29.550', 'spid{currentSpid}', 'DBCC TRACEON 3004, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.670', 'spid{currentSpid}', 'FixupLogTail(progress) zeroing filepath\OdysseyDat_DbChecker.ldf from 0x5000 to 0x6000.'
union all select '2017-12-14 17:00:29.670', 'spid{currentSpid}', 'Zeroing filepath\OdysseyDat_DbChecker.ldf from page 3 to 249 (0x6000 to 0x1f2000)'
union all select '2017-12-14 17:00:29.590', 'spid{currentSpid}', 'Zeroing completed on filepath\OdysseyDat_DbChecker.ldf (elapsed = 13 ms)'
union all select '2017-12-14 17:00:29.630', 'spid{currentSpid}', 'Starting up database ''{Db.DatabaseName}_DbChecker''.'
union all select '2017-12-14 17:00:29.670', 'spid{currentSpid}', 'Zeroing completed on filepath\OdysseyDat_DbChecker.ldf (elapsed = 2 ms)'
union all select '2017-12-14 17:00:29.800', 'spid{currentSpid}', 'DBCC TRACEOFF 3004, server process ID (SPID) 51. This is an informational message only; no user action is required.'
union all select '2017-12-14 17:00:29.910', 'spid{currentSpid}', 'Setting database option DISABLE_BROKER to ON for database ''{Db.DatabaseName}_DbChecker''.'
");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals(true, result);
		}

		IInstantFileInitializationStrategy CreateStrategy(string query)
		{
			return new InstantFileInitializationSlowQueryStrategy(query, _ => currentSpid);
		}
	}
}
