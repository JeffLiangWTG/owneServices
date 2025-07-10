using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class SessionInfoUserKeyEntryTest : TransactionedTestCase
	{
		public void TestInitializeSessionDisplay()
		{
			DateTime serverDateTime = new DateTime(2006, 10, 17, 16, 7, 54, 23);
			SessionInfoUserKeyEntry sessionInfo = new SessionInfoUserKeyEntry("server", "database", "latestlog", serverDateTime);

			AssertEquals("Session Display - Char 1", '0', sessionInfo.SessionDisplay[1]);
			AssertEquals("Session Display - Char 3", '2', sessionInfo.SessionDisplay[3]);
			AssertEquals("Session Display - Char 5", '3', sessionInfo.SessionDisplay[5]);
			AssertEquals("Session Display - Char 7", '5', sessionInfo.SessionDisplay[7]);
			AssertEquals("Session Display - Char 9", '4', sessionInfo.SessionDisplay[9]);
			AssertEquals("Session Display - Char 11", '0', sessionInfo.SessionDisplay[11]);
			AssertEquals("Session Display - Char 13", '7', sessionInfo.SessionDisplay[13]);
			AssertEquals("Session Display - Char 15", '1', sessionInfo.SessionDisplay[15]);
			AssertEquals("Session Display - Char 17", '6', sessionInfo.SessionDisplay[17]);
		}

		public void TestShouldRelease()
		{
			DateTime serverDateTime = new DateTime(2006, 10, 17, 16, 40, 0);
			SessionInfoUserKeyEntryForTesting sessionInfo =
				new SessionInfoUserKeyEntryForTesting("server", "database", "latestlog", serverDateTime);

			AssertEquals("Should Release Restore (blank entered key)", false, sessionInfo.ShouldRelease());

			DateTime testDate = new DateTime(2006, 10, 18);
			string testSessionId = sessionInfo.GetSessionIdHashForDate_Exposed(testDate);
			sessionInfo.UserEnteredReleaseKey = sessionInfo.GetReleaseKey_Exposed(testSessionId);
			AssertEquals("Should Release Restore (entered date > server date)", false, sessionInfo.ShouldRelease());

			testDate = new DateTime(2006, 10, 17);
			testSessionId = sessionInfo.GetSessionIdHashForDate_Exposed(testDate);
			sessionInfo.UserEnteredReleaseKey = sessionInfo.GetReleaseKey_Exposed(testSessionId);
			AssertEquals("Should Release Restore (entered date = server date)", true, sessionInfo.ShouldRelease());

			testDate = new DateTime(2006, 10, 16);
			testSessionId = sessionInfo.GetSessionIdHashForDate_Exposed(testDate);
			sessionInfo.UserEnteredReleaseKey = sessionInfo.GetReleaseKey_Exposed(testSessionId);
			AssertEquals("Should Release Restore (entered date = server date - 1)", true, sessionInfo.ShouldRelease());

			testDate = new DateTime(2006, 10, 15);
			testSessionId = sessionInfo.GetSessionIdHashForDate_Exposed(testDate);
			sessionInfo.UserEnteredReleaseKey = sessionInfo.GetReleaseKey_Exposed(testSessionId);
			AssertEquals("Should Release Restore (entered date = server date - 2)", true, sessionInfo.ShouldRelease());

			testDate = new DateTime(2006, 10, 14);
			testSessionId = sessionInfo.GetSessionIdHashForDate_Exposed(testDate);
			sessionInfo.UserEnteredReleaseKey = sessionInfo.GetReleaseKey_Exposed(testSessionId);
			AssertEquals("Should Release Restore (entered date = server date - 3)", true, sessionInfo.ShouldRelease());

			testDate = new DateTime(2006, 10, 13);
			testSessionId = sessionInfo.GetSessionIdHashForDate_Exposed(testDate);
			sessionInfo.UserEnteredReleaseKey = sessionInfo.GetReleaseKey_Exposed(testSessionId);
			AssertEquals("Should Release Restore (entered date = server date - 4)", false, sessionInfo.ShouldRelease());
		}

		public void TestGetLatestLogKey()
		{
			string sqlTextRaw = "INSERT dbo.StmALog (SL_PK, SL_PostedTimeUtc, SL_Table, SL_Parent, SL_EventTime) VALUES ('{0}', '{1}', 'StmData', newid(), getdate())";

			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE StmALog");
			string latestKey = SessionInfoUserKeyEntryForTesting.GetLatestLogKey_Exposed(Db.Connection, Db.ServerName, Db.DatabaseName);
			AssertEquals("Latest Log (no log default)", "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789A", latestKey);

			string key1 = "A30FEC4D-86FD-45A9-86E3-FF436A2DAC8A";
			string date1 = "2006-10-12 01:00";
			Db.Connection.ExecuteNonQuery(String.Format(sqlTextRaw, key1, date1));
			latestKey = SessionInfoUserKeyEntryForTesting.GetLatestLogKey_Exposed(Db.Connection, Db.ServerName, Db.DatabaseName);
			AssertEquals("Latest Log = Key 1", "A30FEC4D-86FD-45A9-86E3-FF436A2DAC8A2006-10-12T01:00:00", latestKey);

			string key2 = "13006A26-F01E-47DA-A723-F3CC05D19EBF";
			string date2 = "2006-10-12 02:07:0.54";
			Db.Connection.ExecuteNonQuery(String.Format(sqlTextRaw, key2, date2));
			latestKey = SessionInfoUserKeyEntryForTesting.GetLatestLogKey_Exposed(Db.Connection, Db.ServerName, Db.DatabaseName);
			AssertEquals("Latest Log = Key 2", "13006A26-F01E-47DA-A723-F3CC05D19EBF2006-10-12T02:07:00.540", latestKey);

			string key3 = "F03A6761-AD87-4F38-941B-5622938E2D62";
			string date3 = "2006-10-11 03:00";
			Db.Connection.ExecuteNonQuery(String.Format(sqlTextRaw, key3, date3));
			latestKey = SessionInfoUserKeyEntryForTesting.GetLatestLogKey_Exposed(Db.Connection, Db.ServerName, Db.DatabaseName);
			AssertEquals("Latest Log = Key 2 (still)", "13006A26-F01E-47DA-A723-F3CC05D19EBF2006-10-12T02:07:00.540", latestKey);
		}
	}
}
