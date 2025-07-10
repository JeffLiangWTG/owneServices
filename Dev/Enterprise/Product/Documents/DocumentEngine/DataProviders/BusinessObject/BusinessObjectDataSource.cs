using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.DataProviders
{
	class BusinessObjectDataSource : IDataRowSource, IDataRowSourceXXXX
	{
		public BusinessObjectDataSource(string name, IBusinessObjectCollection collection)
			: this(name, new List<BusinessObject>(new TypedEnumerable<BusinessObject>(collection)))
		{
		}

		public BusinessObjectDataSource(string name, List<BusinessObject> groupedOrFilteredCollection)
		{
			Name = name;
			GroupedOrFilteredCollection = groupedOrFilteredCollection;
		}

		public BusinessObjectDataSource(string name, List<BusinessObject> groupedOrFilteredCollection, bool isApplyingCustomSorting)
			: this(name, groupedOrFilteredCollection)
		{
			IsApplyingCustomSorting = isApplyingCustomSorting;
		}

		public BusinessObjectDataSource(string name, IBusinessObjectCollection collection, bool isApplyingCustomSorting)
			: this(name, collection)
		{
			IsApplyingCustomSorting = isApplyingCustomSorting;
		}

		internal readonly string Name;
		internal readonly List<BusinessObject> GroupedOrFilteredCollection;

		internal bool IsApplyingCustomSorting { get; set; }

		public int RowCount
		{
			get { return GroupedOrFilteredCollection.Count; }
		}

		public IDataRowSource[] GroupBy(string[] columns)
		{
			if (columns.Length == 0 || RowCount == 0)
			{
				return new IDataRowSource[] { this };
			}
			else
			{
				var groups = new Dictionary<IEnumerable<IComparable>, IDataRowSource>(new GroupEqualityComparer());

				foreach (var row in GroupedOrFilteredCollection)
				{
					var rowValues = new List<IComparable>();
					var provider = new BusinessObjectDataProvider(new DataProviderList(BODocDataProvider.Get(row)), null);

					foreach (var column in columns)
					{
						var value = provider.GetColumnValue(null, 0, RemoveCollectionNameFromGroupByColumnName(column));

						if (value is IZTypeInternals)
						{
							value = ((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
						}

						if (value == null || value == DBNull.Value)
						{
							value = string.Empty;
						}

						var comparable = value as IComparable;
						if (comparable != null)
						{
							rowValues.Add(comparable);
						}
						else
						{
							rowValues.Add(value.ToString());
						}
					}

					IDataRowSource datasource;

					if (!groups.TryGetValue(rowValues, out datasource))
					{
						datasource = GetEmptyDataSource();
						groups.Add(rowValues, datasource);
					}

					((BusinessObjectDataSource)datasource).GroupedOrFilteredCollection.Add(row);
				}

				var helper = new GroupOrderByHelper();
				return IsApplyingCustomSorting ? helper.GetNonOrderedGroupBy(groups.Select(keyValuePair => new GroupBySource(keyValuePair.Key, keyValuePair.Value))) :
					helper.GetOrderedGroupBy(groups.Select(keyValuePair => new GroupBySource(keyValuePair.Key, keyValuePair.Value)));
			}
		}

		public IDataRowSource Split(int rowsToKeep)
		{
			var result = GetEmptyDataSource();
			for (int i = rowsToKeep; i < RowCount;)
			{
				var bizO = this.GroupedOrFilteredCollection[i];
				result.GroupedOrFilteredCollection.Add(bizO);
				this.GroupedOrFilteredCollection.Remove(bizO);
			}
			return result;
		}

		public IDataRowSource GetFirstNRows(int n)
		{
			var result = GetEmptyDataSource();
			for (int i = 0; i < n; i++)
			{
				if (i < GroupedOrFilteredCollection.Count)
				{
					result.GroupedOrFilteredCollection.Add(GroupedOrFilteredCollection[i]);
				}
				else
				{
					result.GroupedOrFilteredCollection.Add(null);
				}
			}
			return result;
		}

		public string RemoveCollectionNameFromGroupByColumnName(string groupByColumnName)
		{
			if (groupByColumnName.ToLower().StartsWith(Name.ToLower()))
			{
				groupByColumnName = groupByColumnName.Remove(0, Name.Length);
			}
			return groupByColumnName;
		}

		protected BusinessObjectDataSource GetEmptyDataSource()
		{
			return new BusinessObjectDataSource(Name, new List<BusinessObject>());
		}

		public IDataRowSource Filter(string expressions)
		{
			var l = GroupedOrFilteredCollection.FindAll(obj => new BusinessObjectFilterDataSource(obj, expressions).Evaluate());
			return new BusinessObjectDataSource(Name, l);
		}

		public int GroupCount(string[] columnNames)
		{
			return GroupBy(columnNames).Length;
		}

		public IDataRowSource GetRowsFromIndexes(int[] indexes)
		{
			var businessObjects = DataRowSourceHelper.GetRowsFromIndexes(indexes, GroupedOrFilteredCollection.ToArray());

			return new BusinessObjectDataSource(Name, businessObjects.ToList()) { IsApplyingCustomSorting = this.IsApplyingCustomSorting };
		}
	}
}
