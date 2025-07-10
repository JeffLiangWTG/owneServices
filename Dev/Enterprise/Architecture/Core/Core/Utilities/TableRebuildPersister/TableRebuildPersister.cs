using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Database.ExtendedProperties;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Core
{
	public class TableRebuildPersister : ITableRebuildPersister
	{
		public TableRebuildPersister(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));
			this.connection = connection;
		}

		readonly DbConnection connection;
		const string TABLE_PROPERTY_NAME = "RequiresRebuild"; // not seen by user

		string TablePropertyValue
		{
			get
			{
				return DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			}
		}

		// pumps outs any pending data
		public void Clear()
		{
			foreach (var table in GetTablesToRebuild())
			{
				MarkTableAsNotRequiringRebuild(table);
			}
		}

		public bool MarkTableAsRequiringRebuild(DbSchemaTable table)
		{
			// avoid setting properties in XXX_Audit and XXX_EDW databases
			if (DataUtils.ObjectExists(connection, ExtProperty.TableName))
			{
				ExtProperty.Table.Update(connection, table.DatabaseName, table.SchemaName, table.TableName, TABLE_PROPERTY_NAME, TablePropertyValue);
				return true;
			}

			return false;
		}

		public void MarkTableAsNotRequiringRebuild(DbSchemaTable table)
		{
			ExtProperty.Table.Delete(connection, table.DatabaseName, table.SchemaName, table.TableName, TABLE_PROPERTY_NAME);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public IEnumerable<DbSchemaTable> GetTablesToRebuild()
		{
			var tablesToRebuild = new List<DbSchemaTable>();
			var sql = @"
SELECT
	SEP_DatabaseNameSuffix,
	SEP_SchemaName        ,
	SEP_MajorObjectName   
FROM
	dbo.StmExtendedProperty
WHERE 1=1
	AND SEP_Class              = @SEP_Class
	AND SEP_Name               = @SEP_Name
ORDER BY
	SEP_DatabaseNameSuffix,
	SEP_SchemaName        ,
	SEP_MajorObjectName   
";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@SEP_Class", SqlDbType.VarChar, 60, nameof(ExtProperty.Class.Table));
				cmd.AddParameterBasedOnDbColumn("@SEP_Name", TABLE_PROPERTY_NAME, StmExtendedPropertySchema.SEP_Name);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var dbName = connection.DbNameBySuffix((string)reader[StmExtendedPropertySchema.Constants.SEP_DatabaseNameSuffix]);
						var schemaName = (string)reader[StmExtendedPropertySchema.Constants.SEP_SchemaName];
						var tableName = (string)reader[StmExtendedPropertySchema.Constants.SEP_MajorObjectName];

						tablesToRebuild.Add(new DbSchemaTable(dbName, schemaName, tableName));
					}
				}
			}

			foreach (var table in tablesToRebuild.ToArray())
			{
				if (!DataUtils.ObjectExists(connection, table.ToString()))
				{
					MarkTableAsNotRequiringRebuild(table);
					tablesToRebuild.Remove(table);
				}
			}

			return tablesToRebuild;
		}
	}
}
