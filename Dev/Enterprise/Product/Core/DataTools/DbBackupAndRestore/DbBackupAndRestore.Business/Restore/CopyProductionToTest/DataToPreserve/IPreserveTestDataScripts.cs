namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	interface IPreserveTestValueScripts
	{
		/// <summary>
		/// Save main test database info in a temporary database.
		/// </summary>
		/// <param name="auxDbName">Temporary database name</param>
		/// <param name="targetDbName">Main database name</param>
		/// <returns>Returns sql script to save test database info in a temporary database.</returns>
		string GetPopulateTemporaryDataScript(string auxDbName, string targetDbName);

		/// <summary>
		/// Clear production specific info to be ovveritten by test data.
		/// </summary>
		/// <param name="auxDbName">Temporary database name</param>
		/// <param name="targetDbName">Main database name</param>
		/// <returns>Returns sql script to clear production specific info.</returns>
		string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName);

		/// <summary>
		/// Copy previosly saved test data from a test table.
		/// </summary>
		/// <param name="auxDbName">Temporary database name</param>
		/// <param name="targetDbName">Main database name</param>
		/// <returns>Returns sql script to copy previosly saved test data.</returns>
		string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName);
	}
}
