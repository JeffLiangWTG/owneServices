using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CargoWise.EntityFramework
{
	[DebuggerDisplay("Name = {tableName}, {value}")]
	public struct TableHitCount
	{
#if DEBUG
		public TableHitCount(string tableName, int value, IEnumerable<TableHitQuery> queries = null)
#else
		public TableHitCount(string tableName, int value)
#endif
		{
			this.tableName = tableName;
			this.value = value;
#if DEBUG
			this.queries = queries == null ? null : queries.ToArray();
#endif
		}

		public string TableName
		{
			get { return tableName; }
		}

		public int Value
		{
			get { return value; }
		}

		readonly int value;
		readonly string tableName;

#if DEBUG
		public IEnumerable<TableHitQuery> Queries => queries;
		readonly TableHitQuery[] queries;
#endif
	}

#if DEBUG
	public struct TableHitQuery
	{
		public TableHitQuery(string query, string stackTrace)
		{
			this.query = query;
			this.stackTrace = stackTrace;
		}

		public string Query
		{
			get { return query; }
		}

		public string StackTrace
		{
			get { return stackTrace; }
		}

		public static bool operator !=(TableHitQuery a, object b)
		{
			return !(a == b);
		}

		public static bool operator ==(TableHitQuery a, object b)
		{
			var result = false;
			if (b is TableHitQuery)
			{
				var bTableHitQuery = (TableHitQuery)b;
				result = a.Query == bTableHitQuery.Query && a.StackTrace == bTableHitQuery.StackTrace;
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			return this == obj;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		readonly string query;
		readonly string stackTrace;
	}
#endif
}
