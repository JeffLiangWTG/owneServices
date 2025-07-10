using System;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Data
{
	public class UniqueIndexInfo
	{
		public UniqueIndexInfo(string tableName, string indexName, string csvColumnList)
		{
			#if DEBUG
			CheckIndexCorrectness(tableName, indexName);
			#endif

			this.tableName = tableName;
			this.indexName = indexName;
			this.csvColumnList = csvColumnList;
		}

		public string TableName
		{
			get { return tableName; }
		}

		public string DropStatement
		{
			get { return String.Format("DROP INDEX {1} ON {0}", tableName, indexName); }
		}

		public virtual string CreateStatement
		{
			get
			{
				return String.Format("CREATE UNIQUE NONCLUSTERED INDEX {0} ON {1} ({2})", indexName, tableName, csvColumnList);
			}
		}

		#if DEBUG

		protected virtual void CheckIndexCorrectness(string tableName, string indexName)
		{
			CheckIndexExistAsUniqueAndNonclustered(tableName, indexName);
		}

		/// <summary>
		///  - Unique Index       => is_unique = 1
		///  - NonClustered Index => type = 2 
		/// </summary>
		void CheckIndexExistAsUniqueAndNonclustered(string tableName, string indexName)
		{
			string sqlText = String.Format(@"
				SELECT count(*)
				FROM sys.indexes ind
				INNER JOIN sys.objects obj ON obj.object_id = ind.object_id
				WHERE OBJ.name = '{0}'
				AND ind.name = '{1}'
				AND ind.type = 2
				AND ind.is_unique = 1",
				tableName, indexName);
			int rowCount = (int)Db.Connection.ExecuteScalar(sqlText);

			if (rowCount != 1)
			{
				throw new Exception(String.Format("Index [{0}.{1}] either don't exist or isn't UNIQUE and NONCLUSTERED.", tableName, indexName));
			}
		}

		#endif

		readonly protected string tableName;
		readonly protected string indexName;
		readonly protected string csvColumnList;
	}
}
