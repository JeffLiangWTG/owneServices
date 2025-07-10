using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public abstract class ZFilterStripGridModule : ZFilterGridModule, ISupportEDocsBulkDownload
	{
		public ZFilterStripGridModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
			if (page != null)
			{
				page.Load += new EventHandler(page_Load);
			}
		}

		void page_Load(object sender, EventArgs e)
		{
			SetupLayoutHelperForWeb(FilterStripBizO);
		}

		void SetupLayoutHelperForWeb(FilterStripBusinessObject filterStripBizO)
		{
			if (filterStripBizO != null)
			{
				var webLayoutsHelper = new FilterStripLayoutsHelperForWeb(filterStripBizO, Page.SiteUser.LoggedInUser, DefaultLayoutRegistryItem);
				((IFilterStripBusinessObjectInternals)filterStripBizO).LayoutContext = FilterStripLayoutContext;
				filterStripBizO.LayoutsHelper = webLayoutsHelper; // LayoutsHelper must be set before filterStripBizO is used, otherwise a wrong helper will be used
				webLayoutsHelper.DefaultLayout = GetDefaultLayout(filterStripBizO);
			}
		}

		public ZString DefaultLayoutName
		{
			get { return DefaultLayoutRegistryItem != null ? DefaultLayoutRegistryItem.Value : string.Empty; }
		}

		public StmModuleFilter GetDefaultLayout(FilterStripBusinessObject filterStripBizO)
		{
			return filterStripBizO != null ? filterStripBizO.FindLayout(DefaultLayoutName, true) : null;
		}

		protected virtual FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return null; }
		}

		protected virtual ZString FilterStripLayoutContext
		{
			get { return ID.Name; }
		}

#if DEBUG
		public ZString DefaultLayoutNameForTest
		{
			get { return DefaultLayoutName; }
		}

		public FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItemForTest
		{
			get { return DefaultLayoutRegistryItem; }
		}

		public ZString FilterStripLayoutContextForTest
		{
			get { return FilterStripLayoutContext; }
		}
#endif

		protected internal FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				return filterStripBusinessObjectToOverride ?? (Page != null ? Page.DataSource as FilterStripBusinessObject : null);
			}
		}

		#region Filter override

		public void OverrideFilterStripBizO(FilterStripBusinessObject filterBO)
		{
			filterStripBusinessObjectToOverride = filterBO;
		}

		FilterStripBusinessObject filterStripBusinessObjectToOverride;

		#endregion

		public sealed override Type FilterControlType
		{
			get { return typeof(ZFilterStripControl); }
		}

		protected sealed override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFilterStripGridModule), "ZFilterStripControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips", typeof(ZFilterStripControl).Assembly);
		}

		public sealed override Type FilterBusinessObjectType
		{
			get { return null; }
		}

		protected sealed override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			return new FilterBusinessObjectDefaults();
		}

		protected override IBusinessObjectCollection LoadCollectionCore(FilterBusinessObject filterBizO, ZQuery query, IBusinessObjectCollection collection, bool ignoreCache = false)
		{
			ZQuery loggedInUserFilter = GetCurrentLoggedInUserFilter(filterBizO);
			if (Page != null && Page is ZIFramePage)
			{
				IModuleFilterProvider moduleFilterProvider = ((ZIFramePage)Page).ModuleFilterProvider;
				if (moduleFilterProvider != null)
				{
					moduleFilterProvider.ApplyAdditionalLoggedInUserFilter(GetObjectsToFilterType(), loggedInUserFilter);
				}
			}
			query.AddToFilter(loggedInUserFilter);

			return base.LoadCollectionCore(filterBizO, query, collection, ignoreCache);
		}

		protected abstract ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO);

		protected abstract FilterStripBusinessObject GetNewFilterStripBusinessObject();

		protected sealed override FilterBusinessObject CreateNewFilterBusinessObjectCore()
		{
			var filterstripbizobj = GetNewFilterStripBusinessObject();
			filterstripbizobj.AddModuleFiltersCreatedHook((x) => OnModuleFiltersCreated(x));
			SetExternalDefaults(filterstripbizobj);
			SetupLayoutHelperForWeb(filterstripbizobj);
			return filterstripbizobj;
		}

		void OnModuleFiltersCreated(FilterStripBusinessObject filterStripBizO)
		{
			Type typeOfElements = GridCollection.TypeOfElements;
			string tableName = BusinessObjectFactory.GetTableNameFromType(typeOfElements, false);
			if (!string.IsNullOrEmpty(tableName))
			{
				ModuleFilterCollection auditFilters = new ModuleFilterCollection();
				ModuleAuditFilterProvider.AddAuditFilters(auditFilters, EnterpriseSchema.GetTableSchema(tableName), Factory, typeOfElements);
				foreach (ModuleFilter auditFilter in auditFilters)
				{
					try
					{
						filterStripBizO.ModuleFilters.AddFilter(auditFilter);
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
				}
			}
		}

		#region GridColumns

		public GridColumnCollection AllColumns
		{
			get
			{
				if (allColumns == null)
				{
					if (!ColumnProvider.IsEmpty)
					{
						allColumns = new GridColumnCollection();
						allColumns.AddRange(GetNewGridColumnFields());
					}
				}
				return allColumns;
			}
		}
		GridColumnCollection allColumns;

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			return ColumnProvider.GridColumnFields;
		}

		protected override DataGridColumn[] GetDefaultGridColumnFields()
		{
			return ColumnProvider != null ? ColumnProvider.DefaultGridColumnFields : base.GetDefaultGridColumnFields();
		}

		protected override DataGridColumn[] GetRequiredGridColumnFields()
		{
			return ColumnProvider != null ? ColumnProvider.RequiredGridColumnFields : base.GetRequiredGridColumnFields();
		}

		public GridColumnProvider ColumnProvider
		{
			get
			{
				if (columnProvider == null)
				{
					try
					{
						columnProvider = GetColumnProvider();
						columnProvider.CustomizeDictionary();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						//#if DEBUG
						//Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperException(ex);
						//#endif
					}
				}
				return columnProvider;
			}
		}
		GridColumnProvider columnProvider;

		protected virtual GridColumnProvider GetColumnProvider()
		{
			return null;
		}

		#endregion

		#region ISupportEDocsBulkDownload

		WebEDocsDownloadModulesList WebEDocsModulesPairList
		{
			get
			{
				if (webEDocsModulesPairList == null)
				{
					webEDocsModulesPairList = new WebEDocsDownloadModulesList();
				}
				return webEDocsModulesPairList;
			}
		}
		WebEDocsDownloadModulesList webEDocsModulesPairList;

		public WebEDocsDownloadEntry GetRegistryWebEDocsBulkDownload()
		{
			if (WebEDocsModulesPairList.ContainsCode(ID.Name))
			{
				return WebDataRegistry.Instance.WebEDocsBulkDownload.GetModule(ID.Name);
			}
			return null;
		}

		public virtual ZGuid GetEDocsBulkDownloadRelevantPK(ZDataGrid grid, int itemIndex)
		{
			var collection = grid.DataSource as IBusinessObjectCollection;
			var bizoPK = grid.GetPKByRowIndex(itemIndex);
			var bizo = collection.FindByPK(bizoPK);
			return new ZGuid(bizo[RelevantPersistantPKColumn]);
		}

		public List<ZGuid> GetEDocsBulkDownloadRelatedPKs(ZDataGrid grid, ZGuid gridItemPK)
		{
			var collection = grid.DataSource as IBusinessObjectCollection;
			var bizo = collection.FindByPK(gridItemPK);

			var webBizO = bizo as IWebDocumentsSupportBase;
			if (webBizO != null)
			{
				return webBizO.DocRelatedPKs;
			}
			else
			{
				return new List<ZGuid>();
			}
		}

		public List<ZGuid> GetEDocsBulkDownloadRelevantAndRelatedPKs(ZDataGrid grid, int itemIndex)
		{
			List<ZGuid> result = new List<ZGuid>();
			//relevantPK
			result.Add(GetEDocsBulkDownloadRelevantPK(grid, itemIndex));
			//RelatedPKs
			result.AddRange(GetEDocsBulkDownloadRelatedPKs(grid, grid.GetPKByRowIndex(itemIndex)));
			return result.Distinct().ToList();
		}

		public virtual ZString GetPersistantBizoHumanReadableName(ZDataGrid grid, ZGuid gridItemPK)
		{
			return grid.Collection.FindByPK(gridItemPK).HumanReadableName;
		}

		protected abstract SchemaPKColumn RelevantPersistantPKColumn
		{
			get;
		}

		#endregion
	}
}
