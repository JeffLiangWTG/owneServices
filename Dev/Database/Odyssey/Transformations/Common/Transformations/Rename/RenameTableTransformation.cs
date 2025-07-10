using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using static Enterprise.DbUpgrader.Shared.DbObjectCreator;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class RenameTableTransformation : DataTransformation
	{
		protected sealed override void OfflinePreUpgradeTransform()
		{
			foreach (var tableInfo in RenameTableInfoList)
			{
				if (ShouldRenameTable(Db.Connection, Db.DatabaseName, tableInfo.SchemaName, tableInfo.OldTableName, tableInfo.NewTableName))
				{
					RenameTable(Db.Connection, Db.DatabaseName, tableInfo.SchemaName, tableInfo.OldTableName, tableInfo.NewTableName);
				}
			}
			PostRenameTransformation();
		}

		protected internal abstract IEnumerable<IRenameTableTransformationInfo> RenameTableInfoList { get; }

		bool ShouldRenameTable(DbConnection connection, string dbName, string schemaName, string oldTableName, string newTableName)
		{
			return TableExists(connection, dbName, oldTableName, schemaName) && !TableExists(connection, dbName, newTableName, schemaName);
		}

		void RenameTable(DbConnection connection, string dbName, string schemaName, string oldTableName, string newTableName)
		{
			var oldTableColumns = DbObjectCreator.GetTableColumns(connection, oldTableName);
			foreach (var oldColumn in oldTableColumns)
			{
				new DbColumnDependencyRemover(schemaName, oldTableName, oldColumn).DropRelateObjects(connection);
			}
			DbObjectCreator.RenameTable(connection, dbName, schemaName, oldTableName, newTableName);
		}

		protected virtual void PostRenameTransformation()
		{
		}
	}
}
