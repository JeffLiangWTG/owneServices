using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Testing
{
	class SchemaHelper
	{
		readonly DbConnection connection;
		readonly string targetDbName;

		public SchemaHelper(DbConnection connection, string targetDbName)
		{
			this.connection = connection;
			this.targetDbName = targetDbName;
		}

		public void CreateTable(string tableName)
		{
			connection.ExecuteNonQuery($"SELECT * INTO [{targetDbName}].dbo.{tableName} FROM [{Db.DatabaseName}].dbo.{tableName} WHERE 1 = 0");
		}
	}
}
