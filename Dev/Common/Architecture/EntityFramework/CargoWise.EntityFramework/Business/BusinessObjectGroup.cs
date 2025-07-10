using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectGroup<T>
		where T : BusinessObject
	{
		#region Construction

		BusinessObjectGroup(object groupKey)
		{
			GroupKey = groupKey;
		}

		#endregion

		#region Properties

		public object GroupKey { get; private set; }

		public IEnumerable<T> Elements
		{
			get { return elementsInternal; }
		}

		#endregion

		#region Implementation

		readonly List<T> elementsInternal = new List<T>();

		#region GroupByColumn

		public static IEnumerable<BusinessObjectGroup<T>> GroupByColumn(SchemaColumn column, IEnumerable<T> elements)
		{
			var groups = new Dictionary<object, BusinessObjectGroup<T>>();
			foreach (var item in elements)
			{
				var key = item[column];
				BusinessObjectGroup<T> group;
				if (!groups.TryGetValue(key, out group))
				{
					group = new BusinessObjectGroup<T>(key);
					groups.Add(key, group);
				}
				group.elementsInternal.Add(item);
			}
			return groups.Values;
		}

		#endregion

		#region GroupByColumnAndJoinAllValues

		public static IEnumerable<BusinessObjectGroup<T>> GroupByColumnAndJoinAllValues(SchemaColumn column, IEnumerable<SchemaColumn> compareColumns, IEnumerable<T> elements)
		{
			return JoinGroupsByKey(GroupAllColumnValues(column, compareColumns, elements));
		}

		static IEnumerable<BusinessObjectGroup<T>> GroupAllColumnValues(SchemaColumn column, IEnumerable<SchemaColumn> compareColumns, IEnumerable<T> elements)
		{
			var groups = new Dictionary<ComparableList<object>, BusinessObjectGroup<T>>();
			foreach (var item in elements)
			{
				var key = ComparableList<object>.FromBusinessObjectColumns(compareColumns, item);
				BusinessObjectGroup<T> group;
				if (!groups.TryGetValue(key, out group))
				{
					group = new BusinessObjectGroup<T>(new ComparableList<object> { MatchOrder = false });
					group.elementsInternal.Add(item);
					groups.Add(key, group);
				}

				var groupColumnValues = (ComparableList<object>)group.GroupKey;
				var columnValue = item[column];
				if (!groupColumnValues.Contains(columnValue))
				{
					groupColumnValues.Add(columnValue);
				}
				groupColumnValues.ResetHashCode();
			}
			return groups.Values;
		}

		static IEnumerable<BusinessObjectGroup<T>> JoinGroupsByKey(IEnumerable<BusinessObjectGroup<T>> elements)
		{
			var groups = new Dictionary<object, BusinessObjectGroup<T>>();
			foreach (var item in elements)
			{
				var key = item.GroupKey;
				BusinessObjectGroup<T> group;
				if (!groups.TryGetValue(key, out group))
				{
					group = new BusinessObjectGroup<T>(key);
					groups.Add(key, group);
				}
				group.elementsInternal.AddRange(item.Elements);
			}
			return groups.Values;
		}

		#region ComparableList

		class ComparableList<TItem> : List<TItem>
		{
			public static ComparableList<TItem> FromBusinessObjectColumns(IEnumerable<SchemaColumn> compareColumns, BusinessObject element)
			{
				var list = new ComparableList<TItem> { MatchOrder = true };
				list.AddRange(compareColumns.Select(column => (TItem)element[column]));
				list.ResetHashCode();
				return list;
			}

			public bool MatchOrder { get; set; }

			public void ResetHashCode()
			{
				hashCode = null;
			}

			int? hashCode;

			public override int GetHashCode()
			{
				return hashCode ?? (hashCode = this.Aggregate(0, (current, item) => current ^ item.GetHashCode())).Value;
			}

			public override bool Equals(object obj)
			{
				var otherList = obj as ComparableList<TItem>;

				return
					otherList != null && otherList.Count == Count &&
					(MatchOrder ? Enumerable.Range(0, Count).All(i => this[i].Equals(otherList[i])) : this.All(otherList.Contains));
			}
		}

		#endregion

		#endregion

		#endregion
	}
}
