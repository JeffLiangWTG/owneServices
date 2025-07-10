using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	public class DbIndexReader
	{
		public DbIndexReader(string tableOrSynonymName)
		{
			GetActualDatabaseAndTableNames(tableOrSynonymName, out this.databaseName, out this.tableName);
		}

		public DbIndex[] Indexes
		{
			get { return fIndexes ?? (fIndexes = GetIndexes(Db.Connection).ToArray()); }
		}

		DbIndex[] fIndexes;

		public bool IsIndexed(SchemaColumn column)
		{
			return Indexes.Any(index => ((IList)index.ColumnNames).Contains(column.Name));
		}

		public bool IsIndexed(string[] columnNames)
		{
			return Indexes.Any(index => ArrayEquals(index.ColumnNames, columnNames));
		}

		#region Implementation

		/// <summary>
		/// Get database and table names catering for fully qualified tables and reference database sysnonyms.
		/// </summary>
		void GetActualDatabaseAndTableNames(string tableOrSynonymName, out string baseDbName, out string baseTableName)
		{
			int firstDotIndex = tableOrSynonymName.IndexOf(".");

			if (firstDotIndex >= 0)
			{
				int lastDotIndex = tableOrSynonymName.LastIndexOf(".");
				baseDbName = tableOrSynonymName.Substring(0, firstDotIndex);
				baseTableName = tableOrSynonymName.Substring(lastDotIndex + 1);
			}
			else
			{
				if (!RefDbTableNameResolverTestHelper.GetDbAndBaseTableFromSynonym(tableOrSynonymName, out baseDbName, out baseTableName))
				{
					baseDbName = Db.DatabaseName;
					baseTableName = tableOrSynonymName;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<DbIndex> GetIndexes(DbConnection connection)
		{
			using (var command = connection.Command(String.Format("EXEC [{0}]..sp_helpindex [{1}]", databaseName, tableName)))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var indexColumns = ParseIndexColumnNames((string)reader["index_keys"]);
					yield return new DbIndex(indexColumns);
				}
			}
		}

		static bool ArrayEquals(string[] lHS, string[] rHS)
		{
			var result = lHS.Length == rHS.Length;
			if (result)
			{
				for (var i = 0; i < lHS.Length; i++)
				{
					if (lHS[i] != rHS[i])
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		static string[] ParseIndexColumnNames(string valueFromDb)
		{
			var result = valueFromDb.Split(',');
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = result[i].Trim();
			}
			return result;
		}

		readonly string databaseName;
		readonly string tableName;

		#endregion
	}
}
