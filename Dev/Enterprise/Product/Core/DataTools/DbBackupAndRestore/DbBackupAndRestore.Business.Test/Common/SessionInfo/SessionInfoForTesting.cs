namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class SessionInfoForTesting : SessionInfo
	{
		public SessionInfoForTesting(string dbServer, string databaseName)
			: base(dbServer, databaseName)
		{
		}

		public string GetSessionDisplayByMergingExtraCharactersToIdHash_Exposed(string sessionIdHash, string extraChars)
		{
			return this.GetSessionDisplayByMergingExtraCharactersToIdHash(sessionIdHash, extraChars);
		}

		public string GetSessionIdHashByUsingOnlyEvenCharactersFromDisplay_Exposed()
		{
			return this.GetSessionIdHashByUsingOnlyEvenCharactersFromDisplay();
		}

		public void SetSessionDisplay(string newValue)
		{
			sessionDisplay = newValue;
		}
	}
}
