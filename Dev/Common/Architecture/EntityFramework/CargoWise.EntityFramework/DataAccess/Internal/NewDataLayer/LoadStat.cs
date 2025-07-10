using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	internal class LoadStat
	{
		public LoadStat(int databaseLoadCount, IEnumerable<TableHitCount> hitCounts)
		{
			DatabaseLoadCount = databaseLoadCount;
			HitCounts = hitCounts;
		}

		internal int DatabaseLoadCount { get; private set; }
		internal IEnumerable<TableHitCount> HitCounts { get; private set; }

		public static LoadStat operator -(LoadStat newValue, LoadStat original)
		{
			Argument.NotNull(newValue, nameof(newValue));
			Argument.NotNull(original, nameof(original));

			var query =
					from newCount in newValue.HitCounts
					join originalCount in original.HitCounts on newCount.TableName equals originalCount.TableName into resultSet
					from resultSetItem in resultSet.DefaultIfEmpty()
#if DEBUG
					select new TableHitCount(newCount.TableName, newCount.Value - resultSetItem.Value, newCount.Queries?.Except(resultSetItem.Queries ?? Enumerable.Empty<TableHitQuery>()));
#else
					select new TableHitCount(newCount.TableName, newCount.Value - resultSetItem.Value);
#endif

			var diff = newValue.DatabaseLoadCount - original.DatabaseLoadCount;
			return new LoadStat(diff, diff != 0 ? query.ToArray() : Enumerable.Empty<TableHitCount>());
		}
	}
}
