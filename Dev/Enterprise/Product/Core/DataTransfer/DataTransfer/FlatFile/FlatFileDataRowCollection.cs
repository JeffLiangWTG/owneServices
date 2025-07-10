using System.Collections;

namespace Enterprise.DataTransfer.Business
{
	public class FlatFileDataRowCollection : CollectionBase
	{
		public virtual void Add(FlatFileDataRow row)
		{
			if (row != null && !row.IsEmpty)
			{
				List.Add(row);
			}
		}

		public virtual void Add(FlatFileDataRowCollection collection)
		{
			if (collection != null && collection.Count != 0)
			{
				foreach (FlatFileDataRow row in collection)
				{
					List.Add(row);
				}
			}
		}

		public void Insert(int position, FlatFileDataRow row)
		{
			if (row != null && !row.IsEmpty)
			{
				List.Insert(position, row);
			}
		}

		public FlatFileDataRow this[int index]
		{
			get { return (FlatFileDataRow)List[index]; }
			set { List[index] = value; }
		}

		public void InsertBlankLine()
		{
			List.Add(new FlatFileDataRow(0));
		}

		public FlatFileDataRow[] ToArray()
		{
			FlatFileDataRow[] result = new FlatFileDataRow[List.Count];

			for (int i = 0; i < List.Count; i++)
			{
				result[i] = (FlatFileDataRow)List[i];
			}

			return result;
		}

		public void Sort(IComparer comparer)
		{
			InnerList.Sort(comparer);
		}
	}
}
