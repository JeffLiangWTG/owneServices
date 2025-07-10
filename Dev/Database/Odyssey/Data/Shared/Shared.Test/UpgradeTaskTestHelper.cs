using System.Data;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public static class UpgradeTaskTestHelper
	{
		public static DataTable GetFkReferences(DbConnection connection, string parentTable, string tablesToExcludeList)
		{
			var sqlText = string.Format(@"SELECT * FROM dbo.vw_FkReferences WHERE PkTable = '{0}' AND FkTable NOT IN ({1})", parentTable, tablesToExcludeList);
			var fkReferences = DataUtils.GetDataTableFromQuery(connection, sqlText);
			return fkReferences;
		}
	}
}
