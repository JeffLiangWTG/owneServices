using System;

using CargoWise.Data;

namespace Enterprise.DbUpgrader.Shared
{
	public static class UpgCommandRunner
	{
		public static void RunCommandOnGivenDb(DbConnection conn, string dbName, string sqlText)
		{
			conn.ExecuteNonQuery(GetScripToRunOnGivenDb(sqlText, dbName));
		}

		public static object RunScalarCommandOnGivenDb(DbConnection conn, string dbName, string sqlText)
		{
			return conn.ExecuteScalar(GetScripToRunOnGivenDb(sqlText, dbName));
		}

		static string GetScripToRunOnGivenDb(string sqlText, string dbName)
		{
			string sqlTextWithEscapedSingleQuotes = DataUtils.EscapeSingleQuotes(sqlText);
			string result = String.Format("EXEC [{0}]..sp_executesql N'{1}'", dbName, sqlTextWithEscapedSingleQuotes);
			return result;
		}
	}
}
