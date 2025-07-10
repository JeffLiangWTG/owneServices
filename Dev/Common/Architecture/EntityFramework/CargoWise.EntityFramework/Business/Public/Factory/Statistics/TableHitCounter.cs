using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public class TableHitCounter
	{
		public TableHitCounter()
		{
			tableHitCounts = new Dictionary<string, TableHitCount>();
		}

#if DEBUG
		public int IncreaseAccessCount(string tableName, IEnumerable<TableHitQuery> queries = null, int quantity = 1)
		{
			if (tableHitCounts.TryGetValue(tableName, out var hitCounter))
			{
				hitCounter = new TableHitCount(tableName, hitCounter.Value + quantity, GetCombinedQueries(hitCounter.Queries, queries));
			}
			else
			{
				hitCounter = new TableHitCount(tableName, quantity, queries);
			}
			if (IsReportingHitsOnTable(tableName))
			{
				ErrorReporter.ReportOnce("DBHit:" + tableName, "The table " + tableName + " has been hit from GUI thread");
			}
#else
		public int IncreaseAccessCount(string tableName, int quantity = 1)
		{
			if (tableHitCounts.TryGetValue(tableName, out var hitCounter))
			{
				hitCounter = new TableHitCount(tableName, hitCounter.Value + quantity);
			}
			else
			{
				hitCounter = new TableHitCount(tableName, quantity);
			}
#endif

			tableHitCounts[tableName] = hitCounter;
			return hitCounter.Value;
		}

		public ICollection<TableHitCount> Values
		{
			get { return tableHitCounts.Values; }
		}

		public void Clear()
		{
			tableHitCounts.Clear();
		}

		readonly Dictionary<string, TableHitCount> tableHitCounts;

		#region For Test Purposes
#if DEBUG

		IEnumerable<TableHitQuery> GetCombinedQueries(IEnumerable<TableHitQuery> existingQueries, IEnumerable<TableHitQuery> newQueries)
		{
			if (existingQueries != null)
			{
				foreach (var query in existingQueries)
				{
					yield return query;
				}
			}
			if (newQueries != null)
			{
				foreach (var query in newQueries)
				{
					yield return query;
				}
			}
		}

		public TableHitCount GetTableHitCount(string tableName)
		{
			if (!tableHitCounts.TryGetValue(tableName, out var result))
			{
				result = new TableHitCount(tableName, 0);
			}
			return result;
		}

		public bool ShouldReportHits
		{
			get { return shouldReportHits; }
		}

		public static void ReportHitsOnAllTablesExceptSpecifiedTables(ICollection<string> allowedTables)
		{
			shouldReportHits = true;
			TableHitCounter.allowedTables = allowedTables;
		}

		public static void StopReportingHitsOnAllTables()
		{
			shouldReportHits = false;
			allowedTables = null;
		}

		static bool IsReportingHitsOnTable(string tableName)
		{
			return shouldReportHits
				&& allowedTables != null
				&& !allowedTables.Contains(tableName);
		}

		[ThreadStatic]
		static bool shouldReportHits;

		[ThreadStatic]
		static ICollection<string> allowedTables;

#endif
		#endregion
	}
}
