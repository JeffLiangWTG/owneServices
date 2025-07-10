using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentScanning.Business
{
	public class DbMergeInfoCollection
	{
		public static DbMergeInfoCollection New(DbConnection connection, int maxDbSize)
		{
			DbMergeInfoCollection result = new DbMergeInfoCollection();
			result.InitialiseInternalAttributes(connection, maxDbSize);
			return result;
		}

		public string MainDbName
		{
			get;
#if !DEBUG
			private protected
#endif
			set;
		}

		public DbMergeInfo this[int dbNumber]
		{
			get
			{
				return internalDictionary[dbNumber];
			}
		}

		public IEnumerator<DbMergeInfo> GetEnumerator()
		{
			return internalDictionary.Values.GetEnumerator();
		}

		public int Count
		{
			get { return internalDictionary.Count; }
		}

		public int CountWritableWithFreeSpace
		{
			get { return writableAndWithFreeSpaceDbList.Count; }
		}

		public DbMergeInfo FirstWritableDbWithFreeSpace
		{
			get { return writableAndWithFreeSpaceDbList.Count > 0 ? internalDictionary[writableAndWithFreeSpaceDbList[0]] : null; }
		}

		public DbMergeInfo LastWritableDatabase
		{
			get { return lastWritableDatabase; }
		}

		public int GetMaximumUsedSize()
		{
			return internalDictionary.Values.Max(info => info.Size);
		}

		public DbMergeInfo GetLastWritableDatabaseBeforeDbNumber(int beforeDbNumber)
		{
			int maxDbNumberBeforeSpecified =
				internalDictionary.Values
					.Where(dbInfo => !dbInfo.IsReadOnly && dbInfo.Number < beforeDbNumber)
					.Select(dbInfo => dbInfo.Number)
					.Concat(new[] { -1 })
					.Max();

			return maxDbNumberBeforeSpecified > 0 ? internalDictionary[maxDbNumberBeforeSpecified] : null;
		}

		void InitialiseInternalAttributes(DbConnection connection, int maxDbSize)
		{
			maxDbSizeMb = maxDbSize;
			MainDbName = connection.CurrentDatabase;
			lastWritableDatabase = null;
			internalDictionary = new Dictionary<int, DbMergeInfo>();
			writableAndWithFreeSpaceDbList = new List<int>();

			var shouldMergeAllDatabase = SystemDataRegistry.Instance.EDocsStorageProvider.Value == Core.Constants.EDocsStorageProviders.Code.S3;
			var sqlText = "EXEC ep_StorageDocsMerge_AllDbSizes";
			using (DbCommand cmd = connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var dbInfo = new DbMergeInfo(
						(int)reader[StorageDbSizesSchema.DbNumber],
						reader[StorageDbSizesSchema.DbName].ToString(),
						(int)reader[StorageDbSizesSchema.DbSize],
						(bool)reader[StorageDbSizesSchema.DbReadOnly]);

					internalDictionary.Add(dbInfo.Number, dbInfo);

					if (!dbInfo.IsReadOnly)
					{
						lastWritableDatabase = dbInfo;

						if (shouldMergeAllDatabase || dbInfo.Size < maxDbSizeMb)
						{
							writableAndWithFreeSpaceDbList.Add(dbInfo.Number);
						}
					}
				}
			}
		}

		internal bool UpdateInternalAttributesSuccessfully(DbMergeInfo sourceDb, DbMergeInfo destinationDb, int maxDbSize)
		{
			var helper = new DocManagerDBHelper();
			var destinationDbSize = helper.GetDatabaseSizeMB(destinationDb.Number);
			if (destinationDbSize >= maxDbSize)
			{
				return false;
			}

			destinationDb.Size = destinationDbSize;
			sourceDb.Size = helper.GetDatabaseSizeMB(sourceDb.Number);
			return true;
		}

		int maxDbSizeMb;
		DbMergeInfo lastWritableDatabase;
#if DEBUG
		public
#else
		private protected
#endif
		Dictionary<int, DbMergeInfo> internalDictionary;
		List<int> writableAndWithFreeSpaceDbList;
	}

	static class StorageDbSizesSchema
	{
		public const string DbNumber = "DbNumber";
		public const string DbName = "DbName";
		public const string DbSize = "DbSizeMb";
		public const string DbReadOnly = "DbReadOnly";
	}
}
