namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class SessionInfoForKeyGeneration : SessionInfo
	{
		public SessionInfoForKeyGeneration(string dbServer, string databaseName, string sessionDisplay)
			: base(dbServer, databaseName)
		{
			this.sessionDisplay = sessionDisplay;
		}

		public string CalculateReleaseKey()
		{
			string sessionIdHash = GetSessionIdHashByUsingOnlyEvenCharactersFromDisplay();
			string result = GetReleaseKey(sessionIdHash);

			return result;
		}
	}
}
