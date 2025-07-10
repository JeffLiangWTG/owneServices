using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class RestoreSessionInfoForKeyGenerationTest : TestCase
	{
		public void TestBasicProperties()
		{
			SessionInfoForKeyGeneration sessionInfo = new SessionInfoForKeyGeneration("servername", "databasename", "sessiondisplay");

			AssertEquals("Server", "servername", sessionInfo.DbServer);
			AssertEquals("Database", "databasename", sessionInfo.DatabaseName);
			AssertEquals("Session Display", "sessiondisplay", sessionInfo.SessionDisplay);
		}

		public void TestCalculateReleaseKey()
		{
			SessionInfoForKeyGenerationForTesting sessionInfo =
				new SessionInfoForKeyGenerationForTesting("servername", "databasename", "s1e3s5s7i9o1n3d5i9s1p3l5a7y9");

			string expectedKey = sessionInfo.GetReleaseKey_Exposed("sessiondisplay");
			string actualKey = sessionInfo.CalculateReleaseKey();
			AssertEquals("Release Key", expectedKey, actualKey);
		}
	}
}
