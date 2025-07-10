using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[DebuggerDisplay("Count = {Count}")]
	public class CollectionOfIFilter : NonPersistentBusinessObject, IEnumerable<IFilter>, IEnumerable, IObsoleteValidation, IJsonSerializable
	{
		public CollectionOfIFilter()
		{
			this.filters = new List<IFilter>();
		}

		#region Constructor For IJsonSerializable

		internal CollectionOfIFilter(CollectionOfIFilterJsonData data)
		{
			filters = new List<IFilter>();
			foreach (var jsonData in data.Filters)
			{
				var converter = JsonConverterHelper.GetJsonConverters().First(c => c is IJsonConverter i && i.JsonDataType == jsonData.GetType());
				var filter = ((IJsonConverter)converter).GetObjectData(jsonData) as FilterField;
				filters.Add(filter);
			}
		}

		#endregion

		internal void ProcessRelationsBetweenFilters()
		{
			var lookupFilters = new CollectionOfIFilter();
			foreach (FilterField field in this)
			{
				if (field.HasReadOnlyIfFilter)
				{
					field.SetupReadOnlyIfRelation(this);
				}

				if (field.HasDependentFilter)
				{
					field.SetupDependentFilterRelation(this);
				}

				var lookupFilter = field as LookupFilterFieldBase;
				if (lookupFilter != null)
				{
					lookupFilters.Add(lookupFilter);
				}
			}

			foreach (LookupFilterFieldBase lookupFilter in lookupFilters)
			{
				lookupFilter.SetupAllMasterDetailRelations(lookupFilters);
			}
		}

		readonly List<IFilter> filters;

		public string WhereClause()
		{
			var childWhereClauses = new List<string>();

			foreach (IFilter filter in this)
			{
				var whereClause = filter.WhereClause();
				if (!string.IsNullOrEmpty(whereClause))
				{
					childWhereClauses.Add(string.Format("({0})", whereClause));
				}
			}

			return String.Join(" AND ", childWhereClauses.ToArray());
		}

		public SqlParameterList SqlParameters()
		{
			var result = new SqlParameterList();

			foreach (IFilter filter in filters)
			{
				result.AddRange(filter.SqlParameters());
			}

			return result;
		}

		public void Add(IFilter filter)
		{
			var businessObject = filter as IBusiness;
			if (businessObject != null)
			{
				RegisterEditableChildObject(businessObject);
			}

			var filterField = filter as FilterField;
			if (filterField != null
				&& this[filterField.DisplayName] != null)
			{
				throw new FilterFieldDuplicatedException(filterField.DisplayName);
			}

			filters.Add(filter);
		}

		public void AddRange(IEnumerable<IFilter> filters)
		{
			foreach (IFilter filter in filters)
			{
				Add(filter);
			}
		}

		public void Insert(int index, IFilter filter)
		{
			NonPersistentBusinessObject businessObject = filter as NonPersistentBusinessObject;
			if (businessObject != null)
			{
				RegisterEditableChildObject(businessObject);
			}

			filters.Insert(index, filter);
		}

		public void Remove(IFilter filter)
		{
			if (filters.Contains(filter))
			{
				BusinessObject businessObject = filter as BusinessObject;
				if (businessObject != null)
				{
					UnRegisterEditableChildObject(businessObject);
				}
				filters.Remove(filter);
			}
		}

		public IFilter this[int index]
		{
			get
			{
				return filters[index];
			}
			internal set
			{
				FilterField field = filters[index] as FilterField;
				if (field != null)
				{
					UnRegisterEditableChildObject(field);
				}
				filters[index] = value;
				FilterField newField = filters[index] as FilterField;
				if (newField != null)
				{
					RegisterEditableChildObject(newField);
				}
			}
		}

		public new FilterField this[string displayName]
		{
			get
			{
				return this.filters
						.FirstOrDefault(iFilter => iFilter is FilterField
							&& (iFilter as FilterField).DisplayName.Equals(displayName, StringComparison.InvariantCultureIgnoreCase)) as FilterField;
			}
		}

		public IEnumerator<IFilter> GetEnumerator() => filters.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count
		{
			get { return filters.Count; }
		}

		public void Clear()
		{
			foreach (IFilter filter in filters)
			{
				FilterField filterField = filter as FilterField;
				if (filterField != null)
				{
					UnRegisterEditableChildObject(filterField);
				}
			}
			filters.Clear();
		}

		public void ClearValues(LookupField linkedLookupFieldToIgnoreOnWeb = null)
		{
			foreach (IFilter filter in this)
			{
				var isLinkedFilter = linkedLookupFieldToIgnoreOnWeb != null && filter == linkedLookupFieldToIgnoreOnWeb;
				if (!(Globals.IsWeb && isLinkedFilter))
				{
					filter.ClearValues();
				}
			}
		}

		public CodeDescriptionPairList FilterGroups
		{
			get { return fFilterGroups; }
			set { fFilterGroups = value; }
		}
		CodeDescriptionPairList fFilterGroups = new CodeDescriptionPairList();

		public Dictionary<string, object> ToDictionary()
		{
			return this.filters
				.Where(iFilter => iFilter is FilterField)
				.Select(iFilter => iFilter as FilterField)
				.ToDictionary(filter => filter.DisplayName, filter => filter.ValueAsObject);
		}

		public IFilter[] ToArray()
		{
			return filters.ToArray();
		}

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new CollectionOfIFilterJsonData
			{
				Filters = this.Where(filter => filter is FilterField filterField && !filterField.IsEmpty && !filterField.ReadOnly)
					.OfType<IJsonSerializable>()
					.Select(i => (BaseFieldJsonData)i.GetJsonData())
					.ToList()
			};

		#endregion

		public bool Scheduled { get; set; }
	}
}
