using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Schema;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	static class FkToParentIdAndTableCodeTestDataHelper
	{
		internal static Guid InsertTestRows(DbConnection connection, ITransformFkToParentIdAndTableCodeInfo detail, Guid fk, params KeyValuePair<SchemaColumn, string>[] constraints)
		{
			var transformationTableSchema = detail.NewParentIDColumn.TableSchema;
			var tableInserts = new Dictionary<ITableSchema, IList<KeyValuePair<string, string>>>();

			foreach (var constraint in constraints)
			{
				FkToNkTestDataHelper.AddTableInsert(tableInserts, constraint.Key.TableSchema, constraint.Key.Name, constraint.Value);
			}

			foreach (var tableSchema in tableInserts.Keys.Where(k => k != transformationTableSchema))
			{
				FkToNkTestDataHelper.BuildInsertsSqlAndExecute(connection, tableSchema, tableInserts[tableSchema]);
			}
			FkToNkTestDataHelper.AddTableInsert(tableInserts, detail.NewParentIDColumn.TableSchema, detail.OldFkColumnName, fk.ToString());
			return FkToNkTestDataHelper.BuildInsertsSqlAndExecute(connection, transformationTableSchema, tableInserts[transformationTableSchema]);
		}

		internal static IDataReader GetRowDataReader(ITransformFkToParentIdAndTableCodeInfo info, Guid pk, DbConnection connection)
		{
			var sqlText = String.Format(@"SELECT {0}, {1} FROM {2} WHERE {3} = '{4}'",
										info.NewParentIDColumn.Name,
										info.NewParentTableCodeColumn.Name,
										info.NewParentTableCodeColumn.TableName,
										info.NewParentIDColumn.TableSchema.PK.Name,
										pk);
			return connection.Command(sqlText).ExecuteReader(CommandBehavior.SingleResult);
		}
	}
}
