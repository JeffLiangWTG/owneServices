using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public abstract class ColumnValueRankerBase
	{
		#region Get Best Match from a Collection

		public abstract IEnumerable<T> GetBestMatch<T>(IEnumerable<T> collection) where T : BusinessObject;

		protected IEnumerable<T> GetBestMatchFromCollection<T>(IEnumerable<T> collection, Stack<ColumnValuesPairBase> stack) where T : BusinessObject
		{
			var resultCollection = collection;
			if (collection != null && stack.Any())
			{
				var rankBy = stack.Pop();

				var itemsWithRank = collection.Select(x => new { item = x, Rank = GetRank(rankBy.Values, x[rankBy.ColumnName]) }).Where(x => x.Rank != -1);
				if (itemsWithRank.Any())
				{
					var ranks = itemsWithRank.Select(x => x.Rank).Distinct().OrderBy(x => x);
					foreach (int rank in ranks)
					{
						var items = itemsWithRank.Where(x => x.Rank == rank).Select(x => x.item);
						resultCollection = GetBestMatchFromCollection(items, stack);

						if (!stack.Any())
						{
							break;
						}
					}
				}
				else
				{
					resultCollection = null;
				}

				if (resultCollection == null)
				{
					stack.Push(rankBy);
				}
			}
			return resultCollection;
		}

		int GetRank(object[] values, object value)
		{
			int index = -1;
			var castedValue = value as IZType;

			if (castedValue != null)
			{
				index = Array.IndexOf(values.OfType<IZType>().ToArray(), castedValue);
			}
			return index;
		}

		#endregion

		public abstract class ColumnValuesPairBase
		{
			protected ColumnValuesPairBase(object[] values)
			{
				Values = values;
			}

			public ZString ColumnName { get; protected set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
			public readonly object[] Values;
		}
	}
}
