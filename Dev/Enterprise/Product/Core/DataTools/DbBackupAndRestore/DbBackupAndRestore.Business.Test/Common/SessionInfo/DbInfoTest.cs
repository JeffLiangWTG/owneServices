using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DbInfoTest : TestCase
	{
		public void TestBasicProperties()
		{
			SessionInfoForTesting sessionInfo = new SessionInfoForTesting("servername", "databasename");

			AssertEquals("Server", "servername", sessionInfo.DbServer);
			AssertEquals("Database", "databasename", sessionInfo.DatabaseName);
			AssertNull("Session Display", sessionInfo.SessionDisplay);
		}

		public void TestSessionIdHashAndSessionDisplay()
		{
			SessionInfoForTesting sessionInfo = new SessionInfoForTesting("servername", "databasename");

			sessionInfo.SetSessionDisplay("");
			AssertEquals("Session Display (case 1)", "", sessionInfo.SessionDisplay);
			AssertEquals("Session ID (case 1)", "", sessionInfo.GetSessionIdHashByUsingOnlyEvenCharactersFromDisplay_Exposed());

			sessionInfo.SetSessionDisplay("S*O*M*E*T*H*I*N*G");
			AssertEquals("Session Display (case 2)", "S*O*M*E*T*H*I*N*G", sessionInfo.SessionDisplay);
			AssertEquals("Session ID (case 2)", "SOMETHING", sessionInfo.GetSessionIdHashByUsingOnlyEvenCharactersFromDisplay_Exposed());

			sessionInfo.SetSessionDisplay(
				sessionInfo.GetSessionDisplayByMergingExtraCharactersToIdHash_Exposed("sessionhash", "ABC"));
			AssertEquals("Session Display (case 3)", "sAeBsCsAiBoCnAhBaCsAhB", sessionInfo.SessionDisplay);
			AssertEquals("Session ID (case 3)", "sessionhash", sessionInfo.GetSessionIdHashByUsingOnlyEvenCharactersFromDisplay_Exposed());
		}
	}
}
