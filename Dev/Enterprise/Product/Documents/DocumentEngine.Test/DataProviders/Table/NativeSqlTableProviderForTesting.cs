using System.Data;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	class NativeSqlTableProviderForTesting : NativeSqlTableProvider
	{
		public bool ShouldProduceSnapshotIsolationError;

		public override DataTable GetDataTable(string tableName, string dataSourceString, Report reportObject, bool needsToAddWhereClause)
		{
			return GetDataTable(tableName, dataSourceString, reportObject, needsToAddWhereClause, -1);
		}

		public override DataTable GetDataTable(string tableName, string dataSourceString, Report reportObject, bool needsToAddWhereClause, int maximumNumberOfRows)
		{
			if (ShouldProduceSnapshotIsolationError)
			{
				reportObject.RunningConnection.ExecuteNonQuery("SET TRANSACTION ISOLATION LEVEL SNAPSHOT; BEGIN TRANSACTION");
			}
			return base.GetDataTable(tableName, dataSourceString, reportObject, needsToAddWhereClause, maximumNumberOfRows);
		}

		internal string RemoveRecompileFromHintsForTest(string hints)
		{
			return RemoveRecompileFromHints(hints);
		}

		internal bool ExposedDoesSqlHaveWhereClause(string sql)
		{
			return DoesSqlHaveWhereClause(sql);
		}
	}
}
