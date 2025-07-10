using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	/// <summary>
	/// Class for all web Find boxes modules.
	/// </summary>
	public abstract class ZFilterGridModule : ZWebModule, IContainResources
	{
		public ZFilterGridModule(BusinessObjectFactory factory, ZPage page) : base(factory)
		{
			Page = page;
		}

		protected internal ZPage Page;

		public abstract Type GridCollectionType { get; }

		public void SetViewStateData(object data)
		{
			if (CacheCollection)
			{
				SessionCollectionIndexer = (string)data;
			}
		}

		public object GetViewStateData() => CacheCollection ? SessionCollectionIndexer : default;

		#region LoadCollection

		public IBusinessObjectCollection LoadExcelCollection(FilterBusinessObject filterBizO)
		{
			var alternativeFilter = filterBizO.Filter;
			alternativeFilter.MaximumRows = WebDataRegistry.Instance.MaxFilteredRecordsForExportToExcel.Value;

			return LoadExcelCollection(filterBizO, alternativeFilter, GetNewExcelCollection(new BusinessObjectFactory()));
		}

		protected virtual IBusinessObjectCollection LoadExcelCollection(FilterBusinessObject filterBizO, ZQuery query, IBusinessObjectCollection collection) => LoadCollectionCore(filterBizO, query, collection, ignoreCache: true);

		protected virtual IBusinessObjectCollection GetNewExcelCollection(BusinessObjectFactory factory) => GetNewCollection(new BusinessObjectFactory(), ignoreCache: true);

		public int LoadCollection(FilterBusinessObject filterBizO) => LoadCollection(filterBizO, filterBizO.Filter);

		public int LoadCollection(FilterBusinessObject filterBizO, ZQuery alternativeFilter)
		{
			alternativeFilter.MaximumRows = MaxRows;

			return LoadCollectionCore(filterBizO, alternativeFilter, GridCollection).Count;
		}

		protected virtual Type GetObjectsToFilterType() => null;

		protected virtual IBusinessObjectCollection LoadCollectionCore(FilterBusinessObject filterBizO, ZQuery query, IBusinessObjectCollection collection, bool ignoreCache = false)
		{
			if (ignoreCache || IsSearching || ReloadCachedCollection(collection))
			{
				if (Page is ZIFramePage page)
				{
					var moduleFilterProvider = page.ModuleFilterProvider;
					if (moduleFilterProvider != null)
					{
						var moduleFilter = moduleFilterProvider.FilterSubQuery(GetObjectsToFilterType());
						if (moduleFilter != null)
						{
							query.AddToFilter(moduleFilter);
						}
					}
				}

				var sortInfos = GetSortInfos(filterBizO) ?? Array.Empty<ColumnAndSortOrder>();
				var sorts = GetListSortDescriptionCollection(sortInfos);
				if (collection is BusinessObjectCollection legacyCollection)
				{
					if (legacyCollection is IModuleManualSortCollection manualSortLegacyColletion)
					{
						manualSortLegacyColletion.Load(query, sorts);
					}
					else
					{
						query.OrderBy = string.Join(",", sortInfos.Select(o => o.OrderByColumnName + (o.SortDirection == ListSortDirection.Ascending ? OrderByClause.Ascending : OrderByClause.Descending)));
						if (!CacheCollection)
						{
							query.OrderBy += string.Format(", {0}", GetBusinessObjectPKColumn(filterBizO).Name);
						}
						legacyCollection.Load(query);
					}
				}
				if (collection is IActiveBusinessObjectCollection flyweightCollection)
				{
					query.OrderBy = string.Empty;
					flyweightCollection.AdditionalFilter = query;
					var elementType = ListUtil.GetListElementType(collection);
					if (elementType != null)
					{
						flyweightCollection.ApplySort(sorts);
					}
				}

				if (!ignoreCache)
				{
					SaveSessionCollection(collection);
				}
			}

			return collection;
		}

		protected ListSortDescriptionCollection GetListSortDescriptionCollection(ColumnAndSortOrder[] sortInfos)
		{
			var list = new List<ListSortDescription>();
			foreach (var sortInfo in sortInfos)
			{
				list.Add(new ListSortDescription(((ITypedList)GridCollection).GetItemProperties(null)[sortInfo.OrderByColumnName], sortInfo.SortDirection));
			}

			return new ListSortDescriptionCollection(list.ToArray());
		}

		#endregion

		#region Grid columns

		public DataGridColumn[] GridColumnFields => gridColumnFields ?? (gridColumnFields = GetNewGridColumnFields());

		DataGridColumn[] gridColumnFields;

		protected abstract DataGridColumn[] GetNewGridColumnFields();

		public DataGridColumn[] DefaultGridColumnFields => defaultGridColumnFields ?? (defaultGridColumnFields = GetDefaultGridColumnFields());

		DataGridColumn[] defaultGridColumnFields;

		ZGroupColumn[] GroupColumnFields => groupColumnFields ?? (groupColumnFields = GridColumnFields.OfType<ZGroupColumn>().ToArray());

		ZGroupColumn[] groupColumnFields;

		public DataGridColumn[] GroupMemberColumnFields => groupMemberColumnFields ?? (groupMemberColumnFields = GroupColumnFields.SelectMany(g => g.GroupMembers).ToArray());

		DataGridColumn[] groupMemberColumnFields;

		protected virtual DataGridColumn[] GetDefaultGridColumnFields() => GridColumnFields.Where(c => !GroupMemberColumnFields.Contains(c)).ToArray();

		public DataGridColumn[] RequiredGridColumnFields => requiredGridColumnFields ?? (requiredGridColumnFields = GetRequiredGridColumnFields());

		DataGridColumn[] requiredGridColumnFields;

		protected virtual DataGridColumn[] GetRequiredGridColumnFields() => new DataGridColumn[] { SelectionColumn };

		public virtual ZString ResultsGridCssClass => ZString.Empty;

		public DataGridColumn SelectionColumn => GridColumnFields[SelectionColumnIndex];

		protected virtual int SelectionColumnIndex => 0;

		#endregion

		public abstract Type FilterBusinessObjectType { get; }

		protected bool IsUsedAsLookup => Page != null && !(Page is IRememberFilterCriteriaPage);

		#region Internal Properties

		protected internal bool IsUsedAsLookupInternal => IsUsedAsLookup;
		protected internal FilterBusinessObjectDefaults FilterBusinessObjectDefaultsInternal => FilterBusinessObjectDefaults;
		protected internal Type GetCancellableCollectionElementTypeInternal() => GetCancellableCollectionElementType();
		protected internal WebFilterBusinessObjectFactory FilterFactoryInternal => FilterFactory;

		#endregion

		public virtual int MaxRows
		{
			get
			{
				if (maxRows == 0)
				{
					maxRows = WebDataRegistry.Instance.MaxFilteredRecords.Value;
				}

				return maxRows;
			}
			set { maxRows = value; }
		}

		int maxRows;

		#region FilterControl Resource

		public ZWebResource FilterControlResource => filterControlResource ?? (filterControlResource = GetNewFilterControlResource());

		ZWebResource filterControlResource;

		public abstract Type FilterControlType { get; }

		protected abstract ZWebResource GetNewFilterControlResource();

		#endregion

		public void ResetFilterBusinessObject(FilterBusinessObject filterBizO)
		{
			filterBizO.ResetToDefaultValues();
			SetExternalDefaults(filterBizO);
			filterBizO.ClearAllNotifications();
		}

		#region FilterBusinessObjectDefaults

		protected void SetExternalDefaults(FilterBusinessObject filterBizO)
		{
			GetAdditionalFilterBusinessObjectDefaults(filterBizO, FilterBusinessObjectDefaults);
			filterBizO.SetExternalDefaults(FilterBusinessObjectDefaults);
		}

		FilterBusinessObjectDefaults FilterBusinessObjectDefaults => filterBusinessObjectDefaults ?? (filterBusinessObjectDefaults = GetNewFilterBusinessObjectDefaults());

		FilterBusinessObjectDefaults filterBusinessObjectDefaults;

		protected virtual void GetAdditionalFilterBusinessObjectDefaults(FilterBusinessObject filterBizO, FilterBusinessObjectDefaults filterBODetauls)
		{
			if (Page is ZIFramePage framePage)
			{
				var moduleFilterProvider = framePage.ModuleFilterProvider;
				if (moduleFilterProvider != null)
				{
					moduleFilterProvider.SetupDefaults(filterBizO, filterBODetauls);
				}
			}
		}

		protected abstract FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults();
		protected internal FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults_Internal() => GetNewFilterBusinessObjectDefaults();

		#endregion

		public abstract ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter);

		public FilterBusinessObject CreateNewFilterBusinessObject()
		{
			var result = CreateNewFilterBusinessObjectCore();
			var filterStripBusinessObject = result as FilterStripBusinessObject;
			if (filterStripBusinessObject != null)
			{
				filterStripBusinessObject.AddActiveStatusFilters(GetCancellableCollectionElementType());
			}

			return result;
		}

		protected virtual FilterBusinessObject CreateNewFilterBusinessObjectCore()
		{
			var fFilterBusinessObject = FilterFactory.New(FilterBusinessObjectType);
			SetExternalDefaults(fFilterBusinessObject);
			fFilterBusinessObject.Filter.ReLoadExistingRows = true;

			return fFilterBusinessObject;
		}

		protected virtual Type GetCancellableCollectionElementType() => GridCollection.TypeOfElements;

		#region Grid Collection

		public IBusinessObjectCollection GridCollection => gridCollection ?? (gridCollection = GetNewCollection(Factory));

		IBusinessObjectCollection gridCollection;

#if DEBUG

		public void ResetGridCollection()
		{
			gridCollection = null;
		}

#endif

		public IBusinessObjectCollection GetNewCollection(BusinessObjectFactory factory, bool ignoreCache = false)
		{
			if (!ignoreCache && CacheCollection)
			{
				var cachedCollection = LoadSessionCollection();
				if (cachedCollection != null)
				{
					return cachedCollection;
				}
			}
			if (typeof(IBusinessObjectCollection).IsAssignableFrom(GridCollectionType))
			{
				return (IBusinessObjectCollection)Activator.CreateInstance(GridCollectionType, new object[] { factory });
			}

			return null;
		}

		#endregion

		#region FilterFactory

		WebFilterBusinessObjectFactory FilterFactory => filterFactory ?? (filterFactory = new WebFilterBusinessObjectFactory(Factory));

		WebFilterBusinessObjectFactory filterFactory;

		#endregion

		#region Query-related stuff

		public virtual string GetBusinessObjectTableName(FilterBusinessObject filter) => businessObjectTableName ?? (businessObjectTableName = BusinessObjectFactory.GetTableNameFromType(GridCollection.TypeOfElements));

		string businessObjectTableName;

		public virtual SchemaColumn GetBusinessObjectPKColumn(FilterBusinessObject filter) => ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumnSafe(GetBusinessObjectTableName(filter));

		public virtual ZGuid[] GetBusinessObjectPK(BusinessObject bizO) => new ZGuid[] { bizO.PK };

		public virtual bool CacheCollectionPKs => !CacheCollection;

		#endregion

		#region Caching

		IBusinessObjectCollection LoadSessionCollection() => (IBusinessObjectCollection)Page?.GetSessionTimeSensitiveData(SessionCollectionIndexer);

		void SaveSessionCollection(IBusinessObjectCollection collection)
		{
			if (CacheCollection && Page != null && !string.IsNullOrEmpty(SessionCollectionIndexer))
			{
				Page.SetSessionTimeSensitiveData(SessionCollectionIndexer, collection);
			}
		}

		protected virtual bool CacheCollection => false;

		bool IsSearching => true.Equals(Page?.Session[SearchControl.SearchControlIsSearchingIndexer]);

		bool ReloadCachedCollection(IBusinessObjectCollection collection) => !CacheCollection || !collection.IsLoaded;

		protected internal string SessionCollectionIndexer
		{
			get
			{
				if (string.IsNullOrEmpty(sessionCollectionIndexerKey))
				{
					sessionCollectionIndexerKey = Guid.NewGuid().ToString();
				}

				return sessionCollectionIndexerKey;
			}
			set
			{
				sessionCollectionIndexerKey = value;
			}
		}

		string sessionCollectionIndexerKey;

		#endregion

		#region Sorting

		public virtual bool AllowSort => true;

		public virtual ListSortDirection DefaultSortOrder => ListSortDirection.Ascending;

		public IComparer GetNewCollectionSorter(string sortProperty, ListSortDirection sortDirection)
		{
			IComparer returnObject = null;
			if (sortProperty.StartsWith(CustomSorterSupportConst.IsCustomSorter))
			{
				foreach (DataGridColumn col in GridColumnFields)
				{
					if (sortProperty == col.SortExpression)
					{
						var customSorterColumn = col as ISupportCustomSorter;
						if (customSorterColumn != null)
						{
							returnObject = customSorterColumn.GetCustomSorter(sortDirection);
						}
						else
						{
							throw new ApplicationException("Illegal using SortExpression of ZTileLineColumn with ISupportCostomSort interface");
						}
					}
				}

				if (returnObject == null)
				{
					ErrorReporter.ReportOnce(Description.GetUnresolvedString(), string.Format("Cannot find column for Custom sorting Expression {0} , Module {1}", sortProperty, Description.GetUnresolvedString()));
				}
			}
			else
			{
				returnObject = GetNewCollectionSorterCore(sortProperty, sortDirection);
			}

			return returnObject;
		}

		public virtual IComparer GetNewCollectionSorterCore(string sortProperty, ListSortDirection sortDirection) => new WebCollectionSorter(sortProperty, sortDirection);

		#endregion

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				var resources = new ZWebResourceCollection();
				resources.Add(FilterControlResource);

				return resources;
			}
		}

		#endregion

		#region Application path and formatting of Urls

		protected string UrlFormatWithAppRoot(string page) => AppPath + page;

		protected string AppPath
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return "/";
				}
				else
#endif
				{
					var result = System.Web.HttpContext.Current.Request.ApplicationPath;
					if (result != "/")
					{
						result += "/";
					}

					return result;
				}
			}
		}

		#endregion
	}
}
