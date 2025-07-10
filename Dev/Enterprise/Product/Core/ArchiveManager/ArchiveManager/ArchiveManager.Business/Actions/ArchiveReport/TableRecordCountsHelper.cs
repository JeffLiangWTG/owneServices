using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport
{
	public static class TableRecordCountsHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Need to call stored proc directly to minimise memory usage and roundtrips")]
		public static Dictionary<string, TableInfo> GetTableRecordCounts()
		{
			var tableRecordCount = new Dictionary<string, TableInfo>();

			using (var command = Db.Connection.Command("EXEC [dbo].[ep_spaceused]"))
			{
				using var reader = command.ExecuteReader();
				while (reader.Read())
				{
					var tableName = reader.GetString(reader.GetOrdinal("TableName"));
					var rowCount = Convert.ToInt64(reader.GetString(reader.GetOrdinal((NoResString)"Rows")));
					var dataKB = Convert.ToInt64(reader.GetString(reader.GetOrdinal((NoResString)"Data")).Split(' ')[0]);
					var indexSizeKB = Convert.ToInt64(reader.GetString(reader.GetOrdinal("IndexSize")).Split(' ')[0]);

					tableRecordCount.Add(tableName, new TableInfo(rowCount, dataKB, indexSizeKB));
				}
			}

			return tableRecordCount;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "SQL expression")]
		public static TableInfo GetSDTableRecordCounts()
		{
			long rowCount = 0;
			long dataKB = 0;
			long indexSizeKB = 0;

			var dbList = Db.Connection.GetDatabases(DatabaseType.SD);

			foreach (var db in dbList)
			{
				var query = string.Format(@"DROP TABLE IF EXISTS {0}
					CREATE TABLE {0}
					(
						TableName varchar(128),
						Rows      varchar(50),
						Reserved  varchar(50),
						Data      varchar(50),
						IndexSize varchar(50),
						Unused    varchar(50)
					)

					INSERT INTO {0} EXEC {1}.dbo.sp_spaceused StorageDocs

					SELECT 
					TableName,
					convert(bigint, Rows) Rows,
					convert(bigint, left(Reserved, charindex('KB', Reserved) - 1)) Reserved,
					convert(bigint, left(Data, charindex('KB', Data) - 1)) Data,
					convert(bigint, left(IndexSize, charindex('KB', IndexSize) - 1)) IndexSize,
					convert(bigint, left(Unused, charindex('KB', Unused) - 1)) Unused
					FROM {0}

					DROP TABLE {0}", (NoResString)"#Result", db);

				using var command = Db.Connection.Command(query);
				using var reader = command.ExecuteReader();
				while (reader.Read())
				{
					rowCount += reader.GetInt64(reader.GetOrdinal((NoResString)"Rows"));
					dataKB += reader.GetInt64(reader.GetOrdinal((NoResString)"Data"));
					indexSizeKB += reader.GetInt64(reader.GetOrdinal("IndexSize"));
				}
			}

			return new TableInfo(rowCount, dataKB, indexSizeKB);
		}
	}
}
