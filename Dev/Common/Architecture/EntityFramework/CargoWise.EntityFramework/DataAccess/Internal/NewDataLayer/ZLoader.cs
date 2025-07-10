using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class NonPersistentLoaderResponse
	{
		public ZDataTable DataTable { get; internal set; }
		public DataRow[] Rows { get; internal set; }
	}

	// TODO: This should probably take in ZConnectionInfo and not have this on the ZDataQuery 
	public abstract class ZLoader : ZPersistentOperationPerformer
	{
		protected ZLoader(DataSet data) : base(data)
		{
		}

		protected abstract ZDataTable AddTableToDataSet(string tableName);
		public abstract int GetCount(ZCountDataQuery query);

		public LoaderResponse LoadPersistentRowsIntoDataSet(ZDataQuery query)
		{
			return LoadPersistentRowsIntoDataSet(new ZDataQuery[] { query });
		}

		public abstract LoaderResponse LoadPersistentRowsIntoDataSet(IList<ZDataQuery> queries);
		public abstract NonPersistentLoaderResponse LoadNonPersistentRows(ZNonPersistentDataQuery filter);
		public abstract void LoadBlobField(DataRow row, SchemaColumn schemaColumn);
		public abstract void LoadBlobFieldsForTable(string tableName, ZDataRowDictionary rows, IEnumerable<SchemaColumn> columns);
		public abstract Stream GetBinaryFieldStream(DataRow row, string tableName, string columnName);
		public abstract TextReader GetTextFieldReader(DataRow row, string tableName, string columnName, bool closeReaderBetweenReads);
		public abstract Stream GetStream(DataRow row, string tableName, string columnName);

		public ZDataTable GetTable(string tableName, bool createIfNotExisting = true)
		{
			if (tableName == null)
			{
				throw new ArgumentNullException(nameof(tableName));
			}

			ZDataTable result = GetTableCore(tableName, createIfNotExisting);
			if (result == null && createIfNotExisting)
			{
				throw new Exception("Could not generate a table for tableName = " + tableName);
			}
			return result;
		}

		ZDataTable GetTableCore(string tableName, bool createIfNotExisting)
		{
			ZDataTable result;

			lock (Data.Tables.SyncRoot)
			{
				var tableLookup = (Dictionary<string, ZDataTable>)(Data.ExtendedProperties[tableAccessor] ?? (Data.ExtendedProperties[tableAccessor] = new Dictionary<string, ZDataTable>(StringComparer.OrdinalIgnoreCase)));
				if (!(tableLookup.TryGetValue(tableName, out result) && result.DataSet != null))
				{
					result = (ZDataTable)Data.Tables[tableName];
					if (result == null && createIfNotExisting)
					{
						result = AddTableToDataSet(tableName);
						ZRowRelationshipManager.RemoveNotNullConstraintsOnFKsAndDates(result);
					}
					if (result != null)
					{
						tableLookup[tableName] = result;
					}
				}
			}

			return result;
		}

		static readonly object tableAccessor = new object();
	}
}
