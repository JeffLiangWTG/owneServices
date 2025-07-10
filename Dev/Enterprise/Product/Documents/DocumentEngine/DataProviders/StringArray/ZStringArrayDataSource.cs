using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.DataProviders
{
	internal class ZStringArrayDataSource : IDataRowSource
	{
		public ZStringArrayDataSource(string name, ZString[] collection)
		{
			this.Collection = (ZString[])collection.Clone();
			this.Name = name;
		}

		public int RowCount => Collection.Length;

		public IDataRowSource[] GroupBy(string[] groupByColumnName)
		{
			throw new DocumentEngineException("you cannot group by on a string list!");
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		public IDataRowSource[] GroupBy(string groupByColumnName)
		{
			throw new DocumentEngineException("you cannot group by on a string list!");
		}

		public int GroupCount(string[] columnNames)
		{
			throw new DocumentEngineException("you cannot group by on a string list!");
		}

		public IDataRowSource Filter(string expressions)
		{
			throw new DocumentEngineException("you cannot filter a string list!");
		}

		public IDataRowSource Split(int rowsToKeep)
		{
			if (RowCount <= rowsToKeep)
			{
				return new ZStringArrayDataSource(Name, Array.Empty<ZString>());
			}

			var firstPart = new ZString[rowsToKeep];
			var secondPart = new ZString[RowCount - rowsToKeep];
			for (var i = 0; i < rowsToKeep; i++)
			{
				firstPart[i] = Collection[i];
			}

			for (var i = rowsToKeep; i < RowCount; i++)
			{
				secondPart[i - rowsToKeep] = Collection[i];
			}

			Collection = firstPart;
			return new ZStringArrayDataSource(Name, secondPart);
		}

		public IDataRowSource GetFirstNRows(int n)
		{
			var firstPart = new ZString[n];

			for (var i = 0; i < n; i++)
			{
				if (i < Collection.Length)
				{
					firstPart[i] = Collection[i];
				}
				else
				{
					firstPart[i] = "";
				}
			}
			return new ZStringArrayDataSource(Name, firstPart);
		}

		public string RemoveCollectionNameFromGroupByColumnName(string groupByColumnName)
		{
			if (groupByColumnName.ToLower().StartsWith(Name.ToLower()))
			{
				groupByColumnName = groupByColumnName.Remove(0, Name.Length);
			}
			return groupByColumnName;
		}

		public IDataRowSource GetRowsFromIndexes(int[] indexes)
		{
			var newSource = DataRowSourceHelper.GetRowsFromIndexes(indexes, Collection);

			return new ZStringArrayDataSource(Name, newSource.ToArray());
		}

		internal ZString[] Collection;
		internal string Name;
	}
}
