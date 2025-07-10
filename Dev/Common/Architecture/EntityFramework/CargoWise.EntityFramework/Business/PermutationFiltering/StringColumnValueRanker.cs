using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Types;
using static System.FormattableString;

namespace CargoWise.EntityFramework
{
	public class StringColumnValueRanker : ColumnValueRankerBase
	{
		public StringColumnValueRanker()
		{
			list = new List<StringColumnValuesPair>();
		}

		public void Add(ZString stringColumn, params object[] values)
		{
			list.Add(new StringColumnValuesPair(stringColumn, values));
		}

		readonly List<StringColumnValuesPair> list;

		public IEnumerable<StringColumnValuesPair> ColumnValues => list;

		#region Get Best Match from a Collection

		public override IEnumerable<T> GetBestMatch<T>(IEnumerable<T> collection)
		{
			var ranks = ColumnValues;
			if (ranks == null || !ranks.Any())
			{
				return null;
			}

			var stack = new Stack<ColumnValuesPairBase>(ranks.Reverse());
			return stack.Any() ? GetBestMatchFromCollection(collection, stack) : null;
		}

		#endregion

		[DebuggerDisplay("{DebuggerView}")]
		public class StringColumnValuesPair : ColumnValuesPairBase
		{
			public StringColumnValuesPair(ZString columnName, object[] values)
				: base(values)
			{
				ColumnName = columnName;
			}

			string DebuggerView
			{
				get
				{
					return Invariant($"{ColumnName}({string.Join(", ", Values)})");
				}
			}
		}
	}
}
