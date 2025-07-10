using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.DB.Exception;
using Enterprise.DataTransfer.Native.DB.Keys;

namespace Enterprise.DataTransfer.Native.DB
{
	public class ColumnCollection : IEnumerable<ColumnDef>, IKeyFinder
	{
		/// <summary>
		/// Mainly, it just contain Finder Method(Named Scope in ROR)
		/// TODO: Could create a dynamic method in .Net 4.0
		/// </summary>
		/// <param name="grouping"></param>
		public ColumnCollection(string tableName, ColumnDef[] columns)
		{
			this.tableName = tableName;
			PrimaryKey = columns.Single(column => column.Type == ColumnType.PrimaryKey);
			TableCodes = columns.Where(column => column.Type == ColumnType.TableCode || column.Type == ColumnType.TableName).ToArray();
			ForeignKeys = columns.Where(c => c is ForeignKey).OfType<ForeignKey>().ToArray();
			AddInfoColumn = columns.Where(c => c.Name.Contains("_AddInfo")).FirstOrDefault();
			this.columns = columns.ToDictionary(c => c.Name);
		}

		readonly string tableName;
		readonly Dictionary<string, ColumnDef> columns;

		public ColumnDef this[string name]
		{
			get
			{
				var column = Find(name) ?? throw new NoColumnFoundException(tableName, name);
				return column;
			}
		}

		#region IKeyFinder Members

		public ColumnDef PrimaryKey { get; }

		public IEnumerable<ColumnDef> TableCodes { get; }

		public IEnumerable<ForeignKey> ForeignKeys { get; }

		public ColumnDef AddInfoColumn { get; }

		public ColumnDef Find(string name)
		{
			ColumnDef result = null;
			columns.TryGetValue(name, out result);
			return result;
		}

		public IEnumerator<ColumnDef> GetEnumerator()
		{
			return columns.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

	}
}
