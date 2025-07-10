using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace CargoWise.EntityFramework
{
	public struct FilterBusinessObjectDefault
	{
		#region Constructors

		FilterBusinessObjectDefault(ZString filterName, ZString propertyName, SearchType searchType = SearchType.Sql)
			: this()
		{
			FilterName = filterName;
			PropertyName = propertyName;
			Key = filterName + FilterPropertyDelimiter + propertyName;
			Category = FilterOrCategory.None;
			Instance = 0;

			value = null;
			ValueDelegate = null;
			IsRemovable = true;
			SearchType = searchType;
		}

		public FilterBusinessObjectDefault(ZString filterName, ZString propertyName, IZType value, SearchType searchType = SearchType.Sql)
			: this(filterName, propertyName, searchType)
		{
			this.value = value;
		}

		public FilterBusinessObjectDefault(ZString filterName, ZString propertyName, IZType value, bool isRemovable, SearchType searchType = SearchType.Sql)
			: this(filterName, propertyName, value, searchType)
		{
			IsRemovable = isRemovable;
		}

		public FilterBusinessObjectDefault(ZString filterName, ZString propertyName, IZType value, FilterOrCategory category, SearchType searchType = SearchType.Sql)
			: this(filterName, propertyName, value, searchType)
		{
			Category = category;
		}

		public FilterBusinessObjectDefault(ZString filterName, ZString propertyName, IZType value, FilterOrCategory category, ZInt instance, SearchType searchType = SearchType.Sql)
			: this(filterName, propertyName, value, category, searchType)
		{
			Instance = instance;
			ZString keyWithInstance = Key + FilterPropertyDelimiter + Instance.ToString();
			Key = !instance.IsEmpty ? keyWithInstance : Key;
		}

		public FilterBusinessObjectDefault(ZString filterName, ZString propertyName, IZType value, FilterOrCategory category, ZInt instance, bool isRemovable, SearchType searchType = SearchType.Sql)
			: this(filterName, propertyName, value, category, instance, searchType)
		{
			IsRemovable = isRemovable;
		}

		public FilterBusinessObjectDefault(ZString filterName, ZString propertyName, GetValueDelegate valueDelegate, SearchType searchType = SearchType.Sql)
			: this(filterName, propertyName, searchType)
		{
			ValueDelegate = valueDelegate;
		}

		/// <summary>
		/// For Old Filter Business Objects Only - ONLY used on the web
		/// </summary>
		public FilterBusinessObjectDefault(ZString propertyName, IZType value, SearchType searchType = SearchType.Sql)
			: this("", propertyName, value, searchType)
		{
		}

		#endregion

		#region Create

		public static FilterBusinessObjectDefault Create(
			ZString filterName,
			ZString propertyName,
			IZType value = null,
			FilterBusinessObjectDefaults childDefaults = null,
			FilterOrCategory category = FilterOrCategory.None,
			ZString comparisonOperator = default,
			ZInt instance = default,
			SearchType searchType = SearchType.Sql)
		{
			var result = new FilterBusinessObjectDefault(filterName, propertyName, value, category, instance, searchType);
			result.ChildDefaults = childDefaults;
			result.ComparisonOperator = comparisonOperator;

			return result;
		}

		#endregion

		public readonly ZString PropertyName;
		public readonly ZString FilterName;
		public readonly ZString Key;
		public readonly ZInt Instance;
		public readonly FilterOrCategory Category;

		public FilterBusinessObjectDefaults ChildDefaults { get; private set; }
		public ZString ComparisonOperator { get; private set; }

		public static readonly ZString FilterPropertyDelimiter = ":";

		public bool IsRemovable { get; }

		public SearchType SearchType { get; private set; } = SearchType.Sql;

		#region Value

		public IZType Value
		{
			get { return ValueDelegate == null ? value : ValueDelegate(); }
		}

		public delegate IZType GetValueDelegate();

		readonly IZType value;

		readonly GetValueDelegate ValueDelegate;

		#endregion
	}

	public enum SearchType { Sql, Index }

	[System.Diagnostics.DebuggerDisplay("Count = {List.Count}")]
	public class FilterBusinessObjectDefaults : IEnumerable
	{
		#region Add

		public void Add(FilterBusinessObjectDefault defaultToAdd)
		{
			var searchType = defaultToAdd.SearchType;
			if (defaultToAdd.FilterName.Length > 0) // Filter Strips
			{
				if (ContainsDefaultFor(defaultToAdd.Key, searchType))
				{
					Remove(defaultToAdd.Key, searchType);
				}
				List.Add((defaultToAdd.Key, searchType), defaultToAdd);
			}
			else // Old Style (Web Only)
			{
				if (ContainsDefaultFor(defaultToAdd.PropertyName, searchType))
				{
					Remove(defaultToAdd.PropertyName, searchType);
				}
				List.Add((defaultToAdd.PropertyName, searchType), defaultToAdd);
			}
		}

		#endregion

		#region SetDynamicDefaultFilters

		Action setDynamicDefaultFilters;

		public void SetDynamicDefaultFilters(Action dynamicDefaultFilters)
		{
			setDynamicDefaultFilters = dynamicDefaultFilters;
		}

		#endregion

		#region Retrieval

		public ZBool ContainsDefaultFor(ZString propertyName, SearchType searchType = SearchType.Sql)
		{
			return List.ContainsKey((propertyName, searchType));
		}

		public FilterBusinessObjectDefault this[ZString propertyName, SearchType searchType = SearchType.Sql]
		{
			get { return List[(propertyName, searchType)]; }
		}

		public FilterBusinessObjectDefaults GetDefaultsToUse(SearchType searchType)
		{
			var result = new FilterBusinessObjectDefaults();
			List.Where(x => x.Value.SearchType == searchType).ForEach(x => result.Add(x.Value));
			return result;
		}

		public bool HasDefaultsFor(SearchType searchType)
		{
			return List.Any(x => x.Key.SearchType == searchType);
		}

		#endregion

		#region Remove

		public void Remove(ZString propertyName, SearchType searchType = SearchType.Sql)
		{
			if (!ContainsDefaultFor(propertyName, searchType))
			{
				throw new Exception("No default for: " + propertyName + ", Type: " + searchType);
			}

			List.Remove((propertyName, searchType));
		}

		public void RemoveAll()
		{
			List.Clear();
		}

		#endregion

		#region List Management

		IEnumerator IEnumerable.GetEnumerator()
		{
			setDynamicDefaultFilters?.Invoke();

			return List.Values.GetEnumerator();
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
		readonly Dictionary<(string Key, SearchType SearchType), FilterBusinessObjectDefault> List = new();

		public int Count
		{
			get => List.Count;
		}

		#endregion
	}
}
