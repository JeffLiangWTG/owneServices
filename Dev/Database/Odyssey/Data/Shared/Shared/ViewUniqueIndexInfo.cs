using System;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Data
{
	public class ViewUniqueIndexInfo : UniqueIndexInfo
	{
		public ViewUniqueIndexInfo(string viewName, string csvColumnList) : base(viewName, "NR_UC__" + viewName, csvColumnList)
		{
		}

		public ViewUniqueIndexInfo(string tableName, string indexName, string csvColumnList) : base(tableName, indexName, csvColumnList)
		{
		}

		public override string CreateStatement
		{
			get
			{
				return String.Format("CREATE UNIQUE CLUSTERED INDEX {0} ON {1} ({2})", indexName, tableName, csvColumnList);
			}
		}

#if DEBUG

		protected override void CheckIndexCorrectness(string tableName, string indexName)
		{
			CheckIndexExistAsUniqueAndClustered(tableName, indexName);
		}

		/// <summary>
		///  - Unique Index    => is_unique = 1
		///  - Clustered Index => type = 1 
		/// </summary>
		void CheckIndexExistAsUniqueAndClustered(string tableName, string indexName)
		{
			string sqlText = String.Format(@"
				SELECT count(*)
				FROM sys.indexes ind
				INNER JOIN sys.objects obj ON obj.object_id = ind.object_id
				WHERE OBJ.name = '{0}'
				AND iND.name = '{1}'
				AND ind.type = 1
				AND ind.is_unique = 1",
				tableName, indexName);
			int rowCount = (int)Db.Connection.ExecuteScalar(sqlText);

			if (rowCount != 1)
			{
				throw new Exception(String.Format("Index [{0}.{1}] either don't exist or isn't UNIQUE and CLUSTERED.", tableName, indexName));
			}
		}

#endif

	}
}
