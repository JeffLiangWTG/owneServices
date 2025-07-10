using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	internal class RowRangeList
	{
		internal RowRangeList()
		{
			list = new List<RowRange>();
			dictionaryEnd = new Dictionary<int, RowRange>();
		}

		readonly Dictionary<int, RowRange> dictionaryEnd;
		readonly List<RowRange> list;

		public int Count
		{
			get { return list.Count; }
		}

		public void Clear()
		{
			list.Clear();
		}

		public List<RowRange>.Enumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}

		public RowRange this[int index]
		{
			get { return list[index]; }
		}

		public void AddRange(RowRangeList ranges)
		{
			foreach (RowRange range in ranges)
			{
				Add(range);
			}
		}

		public void Add(RowRange newRowRange)
		{
			int startMinus1 = newRowRange.Start - 1;
			RowRange rowRange;
			if (dictionaryEnd.TryGetValue(startMinus1, out rowRange))
			{
				dictionaryEnd.Remove(startMinus1);
				rowRange.End = newRowRange.End;
			}
			else
			{
				list.Add(newRowRange);
				rowRange = newRowRange;
			}
			dictionaryEnd.Add(rowRange.End, rowRange);
		}
	}
}
