namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class SessionInfoForKeyGenerationForTesting : SessionInfoForKeyGeneration
	{
		public SessionInfoForKeyGenerationForTesting(string dbServer, string databaseName, string sessionDisplay)
			: base(dbServer, databaseName, sessionDisplay)
		{
		}

		public string GetReleaseKey_Exposed(string sessionIdHash)
		{
			return this.GetReleaseKey(sessionIdHash);
		}
	}
}
