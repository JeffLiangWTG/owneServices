using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.GUI.Support;
using Enterprise.ZArchitecture.Modules.Internal;
using Res = Enterprise.ZArchitecture.GUI.Res;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.ZArchitecture.Modules
{
	public abstract class ZFilterModule : ZEmbeddedModule, IFilterModuleInternalsForTesting, IShowNewForm, IBusinessObjectReaderProvider, IFilterModuleForFactory, IZFilterModule, IZFilterModuleHelper, IGlowReportingModuleDataQueryProvider
	{
		protected ZFilterModule()
		{
			LoadActiveCollection = true;
		}

		/// <summary>
		/// When a form is shown, it will be shown modal to ParentForm.
		/// </summary>
		public void SetFormsModalTo(Form parentModalForm)
		{
			this.ParentModalForm = parentModalForm;
		}

		/// <summary>
		/// Does this module support actions that rely on a controller.
		/// Eg, New, Edit, Delete, View, etc.
		/// </summary>
		public virtual ZBool HasActions
		{
			get { return true; }
		}

		internal void SetInitialCodeForSearch(ZString code)
		{
			FilterBusinessObject.SetInitialCodeForSearch(code, GridCollection.TypeOfElements);
		}

		internal void SetInitialCodeForSearch(ZString code, string propertyName)
		{
			FilterBusinessObject.SetInitialCodeForSearch(code, propertyName);
		}

		internal void ActivateNonEmptyFiltersInitializedFromCode(ZString code)
		{
			var prefixLen = code.IndexOf(':');
			if (prefixLen > 0)
			{
				string prefix = code.Left(prefixLen);
				foreach (var moduleFilter in FilterBusinessObject)
				{
					var textBaseFilter = moduleFilter as ModuleTextBaseFilter;
					if (textBaseFilter != null && textBaseFilter.Prefix == prefix && !textBaseFilter.IsEmpty && !textBaseFilter.IsActive)
					{
						textBaseFilter.IsActive = true;
					}
				}
			}
		}

		internal void AdditionalSetupForSearch(ZString code)
		{
			FilterBusinessObject.SetAdditionalFilterDefaults(code, GridCollection);
		}

		internal ZString InitialTabPageNameToSelectWhenAFormIsShown { get; set; }

		protected Form ParentModalForm { get; private set; }

		protected virtual bool CanBeReversed()
		{
			return typeof(ITemplateReversible).IsAssignableFrom(this.GridCollection.TypeOfElements);
		}

		protected virtual bool CanBeCopied()
		{
			return typeof(ITemplateCopyable).IsAssignableFrom(this.GridCollection.TypeOfElements);
		}

		public ZController GetNewController()
		{
			return GetNewController(null);
		}

		protected internal abstract ZController GetNewController(BusinessObject selectedBusinessObject);
		protected abstract IBusinessObjectCollection GetNewGridCollection();

		public IBusinessObjectCollection GetNewBusinessObjectCollection()
		{
			return GetNewGridCollection();
		}

		public Type TypeOfTopLevelBusinessObject => TypeOfTopLevelBusinessObjectCore;

		protected virtual Type TypeOfTopLevelBusinessObjectCore
			=> GetNewController(null)?.TypeOfTopLevelBusinessObject;

		protected abstract IFilterControl GetNewFilterControl();
		protected abstract FilterBusinessObject GetNewFilterBusinessObject();
		protected abstract SortInfo DefaultSortOrder { get; }
		protected abstract BusinessObject CurrentBusinessObjectInGrid { get; }

		public IFilterControl GetNewFilterControlForGrid()
		{
			return GetNewFilterControl();
		}

		protected virtual BusinessObject[] SelectedBusinessObjects
		{
			get { return new[] { CurrentBusinessObjectInGrid }; }
		}

		public IEnumerable<BusinessObject> GetBizObjsToEditOrView(ZFilterModule module, ZString codeToFind, Func<IEnumerable<BusinessObject>> getBizoActionDefault) => GetBizObjsToEditOrViewCore(module, codeToFind, getBizoActionDefault);

		protected virtual IEnumerable<BusinessObject> GetBizObjsToEditOrViewCore(ZFilterModule module, ZString codeToFind, Func<IEnumerable<BusinessObject>> getBizoActionDefault) => getBizoActionDefault();

		#region Getting an Instance by ModuleID

		public static ZFilterModule GetZFilterModule(ModuleIdentifier id, string countryOverride = null)
		{
			ZFilterModule module = null;

			if (id != null && id != ModuleIDs.NotAssigned)
			{
				var tempModule = ZModule.GetZModule(id, countryOverride)
					?? throw new ZException("ZModuleFactory did not return a module for ID : " + id.ToString());

				module = tempModule as ZFilterModule;
				if (module == null)
				{
					tempModule.Dispose();
					throw new ZException("Module ID : " + id.ToString() + " resolves to null. FindBox Modules must be a ZFilterModule.");
				}
			}
			return module;
		}

		IZFilterModuleHelper IZFilterModuleHelper.GetZFilterModule(ModuleIdentifier id, string countryOverride) => ZFilterModule.GetZFilterModule(id, countryOverride);

		#endregion

		#region Module Decision Provider

		public IModuleDecisionProvider ModuleDecisionProvider
		{
			get { return fModuleDecisionProvider ?? (fModuleDecisionProvider = CreateDefaultModuleDecisionProvider()); }
		}
		IModuleDecisionProvider fModuleDecisionProvider;

		public void OverrideModuleDecisionProvider(IModuleDecisionProvider moduleDecisionProvider)
		{
			// BETTER TO NOT USE THIS METHOD BECAUSE YOU WILL JUST GET OVERRIDDEN BY SOMETHING ELSE
			fModuleDecisionProvider = moduleDecisionProvider;
		}

		void IZFilterModuleHelper.OverrideModuleDecisionProvider(ISQLFilterOnlyModuleDecisionProvider moduleDecisionProvider) => OverrideModuleDecisionProvider((IModuleDecisionProvider)moduleDecisionProvider);

		public IModuleDecisionProvider GetModuleDecisionProviderForFindBox(IFindBox findbox)
		{
			return GetModuleDecisionProviderForFindBoxCore(findbox);
		}

		public IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopup(IFindBox findbox)
		{
			return GetModuleDecisionProviderForFindBoxPopupCore(findbox);
		}

		protected virtual IModuleDecisionProvider GetModuleDecisionProviderForFindBoxCore(IFindBox findbox)
		{
			return new DirectToFormModuleDecisionProvider(findbox);
		}

		protected virtual IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopupCore(IFindBox findbox)
		{
			return findbox is IFindBoxWithMultipleSelect multipleSelectSupporter && multipleSelectSupporter.AllowModuleMultiSelect ? new PopupModuleDecisionProviderWithMultipleSelect(findbox) : new PopupModuleDecisionProvider(findbox);
		}

		protected virtual IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new DefaultModuleDecisionProvider(this);
		}

		#endregion

		#region DefaultModuleDecisionProvider class

		public class DefaultModuleDecisionProvider : IModuleDecisionProvider
		{
			public DefaultModuleDecisionProvider(ZFilterModule module)
			{
				this.Module = module;
			}

			protected readonly ZFilterModule Module;

			#region IModuleDecisionProvider Members

			public bool EnablePreviousNextSupport
			{
				get { return true; }
			}

			public bool ShouldLoadFilterBizObj
			{
				get { return true; }
			}

			public bool ShouldSaveFilterBizObj
			{
				get { return true; }
			}

			public virtual bool ShouldDisplayNotifications
			{
				get { return false; }
			}

			public bool ShouldIgnoreAdditionalFilter
			{
				get { return false; }
			}

			public virtual bool AllowExcelExport
			{
				get { return true; }
			}

			public virtual IBusinessObjectCollection List
			{
				get { return null; }
			}

			public virtual void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				HandleFindBoxOKButton(selectedBusinessObjects);
			}

			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
			{
				if (selectedBusinessObjects != null &&
					selectedBusinessObjects.Length > 0)
				{
					if (Module.AllowEdit)
					{
						Module.ShowEditForm(selectedBusinessObjects);
					}
					else if (Module.AllowView)
					{
						Module.ShowViewForm(selectedBusinessObjects);
					}
				}
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
			}

			public void InitialiseFindBoxControllerLink(ZController controller)
			{
			}

			public void SetFindBoxCodeDescription(BusinessObject bizo)
			{
			}

			#endregion
		}

		#endregion

		#region Exporting Results

		protected internal SecurityCheckpoint ExportSecurityCheckpoint
		{
			get
			{
				var result = SecurityCheckpoint;
				var security = EnvProxy.Instance.Security;
				if (result != security.None)
				{
					result = security.FindOrCreateExportCheckPoint(SecurityCheckpoint) as SecurityCheckpoint;
				}
				return result;
			}
		}

		protected internal SecurityCheckpoint ImportSecurityCheckpoint
		{
			get
			{
				var result = SecurityCheckpoint;
				var security = EnvProxy.Instance.Security;
				if (result != security.None)
				{
					result = security.FindOrCreateImportCheckPoint(SecurityCheckpoint) as SecurityCheckpoint;
				}
				return result;
			}
		}

		protected abstract BusinessObjectReader CollectionForExport { get; }

		protected internal BusinessObjectReader BusinessObjectReaderWithQuery => SearchManager.CreateReader(ExportQuery);

		BusinessObjectReader IBusinessObjectReaderProvider.BusinessObjectReaderWithFilter => BusinessObjectReaderWithQuery;

		/// <summary>
		/// Nested FilterBusinessObject.Filter with MaxNumberOfRecordsToShow implemented
		/// Query can be added to without damaging FilterBusinessObject.Filter
		/// </summary>
		ZQuery FilterWithMaximumRows
		{
			get
			{
				var result = FilterBusinessObject.Filter;
#if DEBUG
				if (ReturnSingleRow)
				{
					result.MaximumRows = 1;
				}
#endif
				if (!result.IsTopNQuery)
				{
					result.MaximumRows = MaxRowsToLoad + 1;
				}
				return result;
			}
		}

		// TODO : req'd until all modules have moved across to FilterStrips.
		FilterStripBusinessObject FilterStripBusinessObject
		{
			get { return FilterBusinessObject; }
		}

		protected internal virtual ZQuery ExportQuery
		{
			get
			{
				var query = FilterWithMaximumRows;
				GridCollection.CompleteFilter.IsNoResultQuery = false;
				query.AddToFilter(GridCollection.CompleteFilter);
				var activeCollection = GridCollection as IActiveBusinessObjectCollection;
				if (activeCollection != null)
				{
					query.AddToFilter(activeCollection.RelationshipFilter);
				}
				return query;
			}
		}

		#endregion

		#region Grid and Context Menu

		public abstract ZFilterGrid DisplayGrid
		{
			get;
		}

		#endregion

		#region SecurityCheckerForImportingData

		protected virtual ImportSecurityChecker SecurityCheckerForImportingData(EventHandler handler)
		{
			return new ImportSecurityChecker(handler, ImportSecurityCheckpoint);
		}

		ImportSecurityChecker LegacySecurityCheckerForImportingData(EventHandler handler)
		{
			return new ImportSecurityChecker(handler, ImportSecurityCheckpoint);
		}

		#endregion

		#region Export Menu Items

		protected void AddExportDataMenuItem(string caption, EventHandler handler)
		{
			ExportMenuItems.Add(ResString.GetMultilingualString("28bb59f4-aea8-4992-8092-62bf8be9464c", "Export {0}", caption), handler);
		}

		protected virtual FilterModuleMenuItemDescriptorCollection ExportMenuItems
		{
			get
			{
				if (fExportMenuItems == null)
				{
					fExportMenuItems = new FilterModuleMenuItemDescriptorCollection();
					if (ModuleDecisionProvider.AllowExcelExport)
					{
						fExportMenuItems.Add(ResString.GetMultilingualString("7d11d550-24fc-4eca-9d4f-4c346428442a", "Export All Columns To Excel"), new EventHandler(DefaultImportExportAction));
						fExportMenuItems.Add(ResString.GetMultilingualString("138C3CC1-A662-4ADD-B8E1-72F308FDE481", "Export Visible Columns To Excel"), new EventHandler(DefaultVisibleExportAction));
					}
				}
				return fExportMenuItems;
			}
		}
		protected FilterModuleMenuItemDescriptorCollection fExportMenuItems;

		public bool HasExportMenuItems
		{
			get { return ExportMenuItems != null && ExportMenuItems.Count > 0; }
		}

		#endregion

		#region Import Menu Items

		protected void AddImportDataMenuItem(string caption, EventHandler handler, bool includeAccessAllowedCheck = true)
		{
			if (includeAccessAllowedCheck)
			{
				handler = SecurityCheckerForImportingData(handler).OnClick;
			}

			ImportMenuItems.Add(GetImportMenuItemText(caption), handler);
		}

		protected void AddInterfaceConnectorImportMenuItem(string caption, EventHandler handler, bool temporarilyAllowed = false)
		{
			if (!temporarilyAllowed)
			{
				if (HasInterfaceConnector)
				{
					handler = LegacySecurityCheckerForImportingData(handler).OnClick;
					ImportMenuItems.Add(GetImportMenuItemText(caption), handler);
				}
			}
			else
			{
				ImportMenuItems.Add(GetImportMenuItemText(caption), handler);
			}
		}

		protected void AddInterfaceConnectorExportMenuItem(string caption, EventHandler handler, bool temporarilyAllowed = false)
		{
			if (!temporarilyAllowed)
			{
				if (HasInterfaceConnector)
				{
					ExportMenuItems.Add(ResString.GetMultilingualString("28bb59f4-aea8-4992-8092-62bf8be9464c", "Export {0}", caption), handler);
				}
			}
			else
			{
				ExportMenuItems.Add(ResString.GetMultilingualString("28bb59f4-aea8-4992-8092-62bf8be9464c", "Export {0}", caption), handler);
			}
		}

		ResourceString GetImportMenuItemText(string caption)
		{
			return ResString.GetMultilingualString("cc066bfe-14b8-4e53-b049-77cbd46f267e", "Import {0}", caption);
		}

		bool HasInterfaceConnector
		{
			get { return Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasInterfaceConnector; }
		}

		protected virtual EventHandler DefaultImportExportAction
		{
			get { return ModuleDecisionProvider.AllowExcelExport ? new EventHandler(HandleExportClick) : delegate { }; }
		}

		protected virtual EventHandler DefaultVisibleExportAction
		{
			get { return ModuleDecisionProvider.AllowExcelExport ? new EventHandler(HandleExportVisibleClick) : delegate { }; }
		}

		protected FilterModuleMenuItemDescriptorCollection ImportMenuItems
		{
			get { return fImportMenuItems ?? (fImportMenuItems = InitImportMenuItems()); }
		}
		FilterModuleMenuItemDescriptorCollection fImportMenuItems;

		FilterModuleMenuItemDescriptorCollection InitImportMenuItems()
		{
			var menuItems = new FilterModuleMenuItemDescriptorCollection();
			PopulateImportMenuItems(menuItems);
			return menuItems;
		}

		protected virtual void PopulateImportMenuItems(FilterModuleMenuItemDescriptorCollection menuItems)
		{
		}

		public bool HasImportMenuItems => (ImportMenuItems?.Count ?? 0) > 0;

		public delegate Form CreateFormHandler();

		protected void AddImportFromCSVDataMenuItem(CreateFormHandler createFormHandler, bool includeAccessAllowedCheck = true)
		{
			AddImportDataMenuItem(Res.GetString("f2c1775e-b42a-4ebb-b7ce-fc50895baf15", "From CSV"), delegate
			{ ZFormModaliser.Show(createFormHandler(), EmbeddedControl.FindForm()); }, includeAccessAllowedCheck);
		}

		protected void AddInterfaceConnectorCSVImportMenuItem(string caption, EventHandler handler)
		{
			// BG: This will come in later on when we set a date and publish when this will be deprecated.
			//if (HasInterfaceConnector)
			//{
			handler = SecurityCheckerForImportingData(handler).OnClick;
			ImportMenuItems.Add(GetImportMenuItemText(caption), handler);
			//}
		}

		#endregion

		#region Factory

		internal BusinessObjectFactory GetNewFactoryForInternal()
			=> GetNewFactory();

		protected internal virtual BusinessObjectFactory GetNewFactory()
			=> new BusinessObjectFactory { NameForDebugging = "Module : " + ID.Name };

		BusinessObjectFactory IFilterModuleForFactory.GetNewFactoryForFilterModule()
			=> GetNewFactory();

		#endregion

		#region FilterBusinessObject

		public FilterStripBusinessObject FilterBusinessObject
		{
			get
			{
				if (filterBusinessObject == null)
				{
					filterBusinessObject = (FilterStripBusinessObject)GetNewFilterBusinessObject();

					if (filterBusinessObject != null)
					{
						filterBusinessObject.SearchTypeChanged += SearchTypeChanged;
						var typeOfElements = GridCollection.TypeOfElements;
						filterBusinessObject.QueryObjectType = typeOfElements;
						filterBusinessObject.ParentType = typeOfElements;
						DefaultLayoutContext(filterBusinessObject);
						filterBusinessObject.AddActiveStatusFilters(typeOfElements);
						filterBusinessObject.ModuleFiltersCreated += new ModuleFiltersCreatedHandler(filterBusinessObject_ModuleFiltersCreated);
						filterBusinessObject.IsGlowIndexSearchAllowed = ShouldLoadFilterBizOIndexSearchFilter;
						filterBusinessObject.ParentModule = this;
						filterBusinessObject.ShouldLoadFilterBusinessObjectDefaults = ShouldLoadFilterBusinessObjectDefaults;
						filterBusinessObject.OnGlowIndexQueryErrorAction = OnGlowIndexQueryErrorAction;
						filterBusinessObject.SetGlowFiltersIfAllowed();
					}

					filterBusinessObject?.SetExternalDefaults();
				}

				return filterBusinessObject;
			}
		}

		public bool ShouldLoadFilterBizOIndexSearchFilter { get; set; } = true;

		protected virtual void SearchTypeChanged(SearchType searchType)
		{
			// Overridden in ZFilterGridModule
		}

		protected virtual void OnGlowIndexQueryErrorAction(string errorMessage)
		{
			// Overridden in ZFilterGridModule
		}

		FilterStripBusinessObject filterBusinessObject;

		protected virtual void DefaultLayoutContext(FilterStripBusinessObject filterStripBusinessObject)
		{
			((IFilterStripBusinessObjectInternals)filterStripBusinessObject).LayoutContext = ID.Name;
		}

		protected virtual bool ShouldLoadFilterBusinessObjectDefaults
		{
			get { return !ModuleDecisionProvider.ShouldLoadFilterBizObj; }
		}

		void filterBusinessObject_ModuleFiltersCreated(IFilterStripBusinessObject filterStripBizO)
		{
			var typeOfElements = GridCollection.TypeOfElements;
			if (!typeOfElements.IsInterface)
			{
				foreach (var strategy in FilterModuleStrategies)
				{
					strategy.RunOnModuleFiltersCreated(filterBusinessObject.ModuleFilters, typeOfElements, Factory);
				}
			}
		}

		#endregion

		#region GridCollection / RowValidationFilter

		ZQuery RowValidationFilter
		{
			get
			{
				if (rowValidationFilter == null)
				{
					InitializeGridCollectionAndRowValidationFilter();
				}
				return rowValidationFilter;
			}
		}
		ZQuery rowValidationFilter;

		void InitializeGridCollectionAndRowValidationFilter()
		{
			var list = ModuleDecisionProvider.List;

			if (list == null || (list.Factory != null && !list.Factory.ThreadSentry.IsOwner))
			{
				gridCollection = GetNewGridCollection();

				gridCollection.Factory?.ThreadSentry.EnsureCurrentThreadIsOwner(() => $"GetNewGridCollection, Module ID: {ID.Name}"); // Developer diagnostic info

				gridCollection.SuspendValidation();
			}
			else
			{
				gridCollection = list;

				gridCollection.Factory?.ThreadSentry.EnsureCurrentThreadIsOwner(() => $"List from ModuleDecisionProvider of type {ModuleDecisionProvider.GetType()}{GetModuleDecisionProviderListProviderInfo()}, Module ID: {ID.Name}"); // Developer diagnostic info
			}

			if (gridCollection is IActiveBusinessObjectCollection activeCollection)
			{
				gridCollection = activeCollection.Clone();
				rowValidationFilter = activeCollection.AdditionalFilter;

				gridCollection.Factory?.ThreadSentry.EnsureCurrentThreadIsOwner(() => $"Cloned ActiveBusinessCollection, Module ID: {ID.Name}"); // Developer diagnostic info
			}
			else if (gridCollection is BusinessObjectCollection legacyCollection)
			{
				rowValidationFilter = ((ILegacyBusinessObjectCollectionInternals)legacyCollection).AdditionalFilter;
			}
			rowValidationFilter.ModificationsEnabled = false;
			gridCollection.IncrementReadOnlyIncludingChildren();
		}

		string GetModuleDecisionProviderListProviderInfo()
		{
			if (!(ModuleDecisionProvider is ModuleDecisionProvider provider))
			{
				return string.Empty;
			}
			return $" provided by FindBox.ListProvider of type {provider.FindBoxListProviderType}"; // Developer diagnostic info
		}

		#endregion

		bool isRecentItemsSelection;
		internal IDisposable SetRecentItemsSelection()
		{
			isRecentItemsSelection = true;
			return new DisposableAction(() => isRecentItemsSelection = false);
		}

		#region CheckValidSelectionForFindBox

		internal bool CheckValidSelectionForFindBox(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject == null)
			{
				return false;
			}

			var result = true;

			var notificationType = GetNotificationTypeWhenAdditionalFilterNotMet();
			if (notificationType == CargoWise.ComponentModel.NotificationType.Error)
			{
				var reloadFilter = RowValidationFilter.DeepClone();
				reloadFilter.AddToFilter(GetPKFilter(selectedBusinessObject));
				
				if (isRecentItemsSelection)
				{
					reloadFilter.AddToFilter(GetQueryForRecentItems());
				}
				else
				{
					if (GridCollection is IActiveBusinessObjectCollection activeCollection)
					{
						reloadFilter.AddToFilter(activeCollection.RelationshipFilter);
					}
				}

				var canReloadWithFilter = CanReloadWithFilter(newFactory, selectedBusinessObject, reloadFilter);
				if (!canReloadWithFilter)
				{
					Globals.Message.ShowError(GetAllNotificationsWhenAdditionalFilterNotMet(selectedBusinessObject));
					result = false;
				}
			}

			if (result && GridCollection is IFilterModuleExtraNotificationProvider extraNotificationProvider)
			{
				if (selectedBusinessObject.HasRowErrors)
				{
					Globals.Message.ShowError(selectedBusinessObject.RowErrors.ToMessageListString());
					result = false;
				}

				var isBizoFromDifferentFactory = selectedBusinessObject.Factory._Instance != Factory._Instance;
				if (result && (ShouldAlwaysCheckExtraNotification(GridCollection) || isBizoFromDifferentFactory))
				{
					var extraNotification = extraNotificationProvider.GetExtraNotification(selectedBusinessObject);
					if (extraNotification != null && extraNotification.Type == CargoWise.ComponentModel.NotificationType.Error)
					{
						Globals.Message.ShowError(extraNotification.Message);
						result = false;
					}
				}
			}

			return result;
		}

		bool ShouldAlwaysCheckExtraNotification(IBusinessObjectCollection bizOCollection)
		{
			return Attribute.IsDefined(bizOCollection.GetType(), typeof(ZFilterModuleAlwaysCheckExtraNotificationAttribute));
		}

		/// <summary>
		/// check a.default RelationshipFilter(override ZQuery CreateRelationshipFilter()), b.default AdditionalFilter(override ZQuery CreateAdditionalFilter()), c.always visible and readonly filters
		/// for recent items
		/// </summary>
		ZQuery GetQueryForRecentItems()
		{
			var query = new ZQuery();

			FilterBusinessObject.ActiveModuleFilters.ForEach(moduleFilter =>
			{
				if (moduleFilter != null && moduleFilter.ReadOnly && moduleFilter.Visibility == FilterVisibility.AlwaysVisible)
				{
					query.AddToFilter(moduleFilter.Query);
				}
			});

			if (GridCollection is ILegacyBusinessObjectCollectionInternals businessObjectCollection)
			{
				query.AddToFilter(businessObjectCollection.AdditionalFilter);
				query.AddToFilter(businessObjectCollection.RelationshipFilter);
			}
			else if (GridCollection is IActiveBusinessObjectCollection activeBusinessObjectCollection)
			{
				var createRelationshipFilterMethod = GridCollection.GetType().GetMethod(
					"CreateRelationshipFilter",
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					Type.EmptyTypes,  // No argument
					null
				);

				if (createRelationshipFilterMethod?.Invoke(GridCollection, null) is ZQuery relationshipFilter)
				{
					query.AddToFilter(relationshipFilter);
				}
			}

			return query;
		}

		internal bool CheckValidSelectionForFindBox(IEnumerable<BusinessObject> selectedBusinessObjects)
		{
			var newFactory = new BusinessObjectFactory();
			foreach (var selectedBusinessObject in selectedBusinessObjects)
			{
				if (selectedBusinessObject != null)
				{
					newFactory.AddFetchHint(selectedBusinessObject.PKSchemaColumn, selectedBusinessObject.PK);
				}
			}

			return selectedBusinessObjects.All((selectedBusinessObject) => CheckValidSelectionForFindBox(newFactory, selectedBusinessObject));
		}

		ZQuery GetPKFilter(BusinessObject selectedBusinessObject)
		{
			return new ZQuery(selectedBusinessObject.PKSchemaColumn, selectedBusinessObject.PK);
		}

		protected virtual bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery query)
		{
			return newFactory.LoadTop1(selectedBusinessObject.GetType(), query) != null;
		}

		#endregion

		#region Handling New/Edit/View/etc

		protected void HandleShowingFormSafely(Action showFormAction)
		{
			if (ShowFormsFromMainThread)
			{
				MainThreadRunner.RunOnMainThread(showFormAction);
			}
			else
			{
				showFormAction();
			}
		}

		/// <summary>
		/// If True, all forms opened from this module will be forced onto the Main Thread.
		/// If False, all forms opened from this module will be opened on the Current Thread.
		/// Set this to True if you're opening the module in a thread-uncertain way.
		/// Which you shouldn't be doing in the first place, but eh.
		/// </summary>
		public bool ShowFormsFromMainThread { get; set; }

		protected void HandleKeyDown(Keys keyData)
		{
			HandleShowingFormSafely(() =>
			{
				if (keyData == Keys.Enter)
				{
					HandleEnterOrDoubleClick();
				}
				else if ((keyData & Keys.KeyCode) == Keys.Delete)
				{
					HandleDeleteClick(null, null);
				}
			});
		}

		protected void HandleViewClick(object sender, EventArgs e)
		{
			HandleShowingFormSafely(() => HandleViewClickCore(sender, e));
		}

		protected virtual void HandleViewClickCore(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				ShowViewForm(SelectedBusinessObjects);
			}
			else if (CurrentBusinessObjectInGrid != null)
			{
				ShowViewForm(CurrentBusinessObjectInGrid);
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected void HandleNewClick(object sender, EventArgs e)
		{
			var newMenuItem = FormActionMenu.Find(NewMenuItemName)
				?? sender as MenuItem;

			if (newMenuItem != null)
			{
				foreach (MenuItem item in newMenuItem.MenuItems)
				{
					if (item.DefaultItem)
					{
						item.PerformClick();

#if DEBUG
						NewMenuClickPerformDefaultSubMenu = true;
#endif

						return;
					}
				}
			}
#if DEBUG
			NewMenuClickPerformDefaultSubMenu = false;
#endif
			HandleShowingFormSafely(HandleNewClickCore);
		}

#if DEBUG
		internal bool NewMenuClickPerformDefaultSubMenu;
#endif

		protected virtual void HandleNewClickCore()
		{
			ShowNewForm();
		}

		protected void HandleTemplateCopyClick(object sender, EventArgs e)
		{
			HandleShowingFormSafely(() =>
			{
				if (CurrentBusinessObjectInGrid != null)
				{
					if (ZFilterGridModule.IsTemplateRecord(CurrentBusinessObjectInGrid))
					{
						Globals.Message.ShowError(TemplateRecordCopyErrorMessage);
					}
					else
					{
						ShowTemplateCopyForm(GetOnCurrentThread(CurrentBusinessObjectInGrid));
					}
				}
				else
				{
					ShowNoSelectedMessage();
				}
			});
		}

		protected void HandleCopyAndReverseClick(object sender, EventArgs e)
		{
			HandleShowingFormSafely(() =>
			{
				if (CurrentBusinessObjectInGrid != null)
				{
					if (ZFilterGridModule.IsTemplateRecord(CurrentBusinessObjectInGrid))
					{
						Globals.Message.ShowError(TemplateRecordCopyErrorMessage);
					}
					else
					{
						ShowTemplateCopyAndReverseForm(CurrentBusinessObjectInGrid);
					}
				}
				else
				{
					ShowNoSelectedMessage();
				}
			});
		}

		string TemplateRecordCopyErrorMessage => Res.GetString("ecd8bec7-7e62-49e5-a66d-1d2606901c33", "The Copy function cannot be used to copy template records. Instead, use the Universal Copy function to create new records from templates.");

		protected void HandleEditClick(object sender, EventArgs e)
		{
			HandleShowingFormSafely(() => HandleEditClickCore(sender, e));
		}

		protected virtual void HandleEditClickCore(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				ShowEditForm(SelectedBusinessObjects);
			}
			else if (CurrentBusinessObjectInGrid != null)
			{
				ShowEditForm(CurrentBusinessObjectInGrid);
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected void HandleEnterOrDoubleClick()
		{
			BusinessObject[] objects = null;
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				objects = SelectedBusinessObjects;
			}
			else if (CurrentBusinessObjectInGrid != null)
			{
				objects = new BusinessObject[] { CurrentBusinessObjectInGrid };
			}

			if (objects?.Length > 0)
			{
				using (new ZWaitCursorChanger())
				{
					ModuleDecisionProvider.HandleDefaultAction(objects);
				}
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected void HandleDeleteClick(object sender, EventArgs e)
		{
			HandleShowingFormSafely(() => HandleDeleteClickCore(sender, e));
		}

		protected virtual void HandleDeleteClickCore(object sender, EventArgs e)
		{
			if (!(HasActions && AllowDelete))
			{
				return;
			}

			if (SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
			}
			else if (SelectedBusinessObjects.Length > 1 && AllowMultiDelete)
			{
				if (AllowMultiDeleteWithoutListing)
				{
					DeleteMultipleWithoutListing(SelectedBusinessObjects);
				}
				else
				{
					DeleteMultiple(SelectedBusinessObjects);
				}
			}
			else
			{
				var cancellableBusinessObjectInGrid = CancellableHelper.GetICancellable(CurrentBusinessObjectInGrid);

				if (cancellableBusinessObjectInGrid != null)
				{
					var cancellableBusinessObjectType = cancellableBusinessObjectInGrid.GetType();

					if (PreventDeleteAttribute.IsTrue(cancellableBusinessObjectType))
					{
						var factory = new BusinessObjectFactory();
						var cancellable = factory.Load(cancellableBusinessObjectType, (cancellableBusinessObjectInGrid as BusinessObject).PK) as ICancellable;

						if (!cancellable.IsCancelled && string.IsNullOrEmpty(cancellable.CanCancel()) ||
							cancellable.IsCancelled && string.IsNullOrEmpty(cancellable.CanReactivate()))
						{
							ShowDeleteForm(cancellable as BusinessObject);
						}
						else
						{
							var message = !cancellable.IsCancelled ? cancellable.CanCancel() : cancellable.CanReactivate();
							Globals.Message.ShowInformation(message);
						}
					}
					else
					{
						ShowDeleteForm(CurrentBusinessObjectInGrid);
					}
				}
				else
				{
					ShowDeleteForm(CurrentBusinessObjectInGrid);
				}
			}
		}

		protected bool AllowMultiDelete
		{
			get { return true; }
		}

		protected virtual bool AllowMultiDeleteWithoutListing
		{
			get { return false; }
		}

		protected void HandleExportVisibleClick(object sender, EventArgs e)
		{
			if (ValidateBeforeExport())
			{
				ExportVisibleIntoAndOpenExcel();
			}
		}

		protected void HandleExportClick(object sender, EventArgs e)
		{
			if (ValidateBeforeExport())
			{
				ExportIntoAndOpenExcel();
			}
		}

		bool ValidateBeforeExport()
		{
			if (filter != null)
			{
				filter.CommitAllFilters();
			}
			FilterBusinessObject?.RunPreSaveValidation();
			if (FilterBusinessObject != null && FilterBusinessObject.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("d78ab605-51cd-4b69-aef1-0c8e1130adb6", "There are errors. Please correct these before exporting to excel."), Res.GetString("40be89ab-f3e5-4812-8c1d-355d8d17b830", "Errors..."));
				return false;
			}
			return true;
		}

		protected void ExportReferenceXML(object sender, EventArgs e)
		{
			using (var dialog = new ZSaveFileDialog { AddExtension = true, DefaultExt = ".xml" })
			{
				var dialogResult = dialog.ShowDialog(ParentModalForm);
				if (dialogResult != DialogResult.OK)
				{
					return;
				}
				var referenceXMLExport = GridCollection.TypeOfElements.GetCustomAttribute<Customs.Shared.EnableRefererenceXMLExporterAttribute>();
				var dataSetPKs = SelectedBusinessObjects.Cast<IBusinessObjectInternals>().Select(x => (Guid)x.Row[referenceXMLExport.ColumnName]);
				var exporter = ObjectFactory.Get<Customs.Shared.IReferenceXMLExporter>();
				exporter.Export(dataSetPKs, referenceXMLExport.TableName, dialog.UnmappedFileName);
				Globals.Message.Show(Res.GetString("9E8F2422-F5BC-4A07-803B-E7C3C05B1648", "{0} was created.", dialog.UnmappedFileName));
			}
		}

		protected void UploadReferenceXML(object sender, EventArgs e)
		{
			var confirmationEmail = InputBox.Show(Res.GetString("BF658CCF-6B00-45B2-B345-540EC7729DA8", "Notification emails: (use ; to separate)"), Res.GetString("A7C2056D-570F-4F71-BCDD-57B0041664D1", "Email confirmation"), false);
			if (!string.IsNullOrEmpty(confirmationEmail))
			{
				UploadXml(confirmationEmail);
			}
		}

		public void UploadXml(string emails)
		{
			var referenceXMLExport = GridCollection.TypeOfElements.GetCustomAttribute<Customs.Shared.EnableRefererenceXMLExporterAttribute>();
			var datasetPKs = SelectedBusinessObjects.Cast<IBusinessObjectInternals>().Select(x => (Guid)x.Row[referenceXMLExport.ColumnName]);
			var exporter = ObjectFactory.Get<Customs.Shared.IReferenceXMLExporter>();

			var result = exporter.UploadFile(datasetPKs, referenceXMLExport.TableName, emails);

			if (result)
			{
				Globals.Message.Show(Res.GetString("9FAFE2F7-E75B-4F0E-B155-BC9042B4F70A", "File uploaded with success"));
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("C8464866-0A59-41CF-BB13-6D6186710A80", "Error trying to upload the file, check your email format"));
			}
		}

		protected void ShowNoSelectedMessage()
		{
			Enterprise.ZArchitecture.Environment.Globals.Message.Show(Res.GetString("FilterModule|PleaseSelectARecordInTheGrid", "Please select a record in the grid."),
				Res.GetString("FilterModule|NoRecordSelected", "No record selected..."), MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
		}

		protected void ShowTooManyRecordsSelectedMessage(int recordsLeft)
		{
			Enterprise.ZArchitecture.Environment.Globals.Message.Show(Res.GetString("FilterModule|TooManyRecordsSelectedInTheGrid",
				"Too many records have been selected to show simultaneously. (You can display about {0} more right now.) Close some other windows or select fewer records.", recordsLeft),
				Res.GetString("FilterModule|TooManyRecords", "Too many records..."), MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
		}

		protected abstract void ExportVisibleIntoAndOpenExcel();

		protected abstract void ExportIntoAndOpenExcel();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu item key")]
		public const string NewMenuItemName = "New";

		#endregion

		#region Showing Forms

		protected internal virtual IZForm ShowNewForm()
		{
			return ShowNewFormCore(null);
		}

		protected IZForm ShowNewFormCore(Func<BusinessObject> newBusinessObjectGetter)
		{
			BusinessObject newBusinessObject = null;
			if (newBusinessObjectGetter != null)
			{
				try
				{
					newBusinessObject = newBusinessObjectGetter();
				}
				catch (InvalidOperationException)
				{
					return null;
				}
			}

			var controller = GetNewControllerInternal(null, true);

			var form = newBusinessObject != null ? controller.ShowFormForNewEntity(newBusinessObject) : controller.ShowNewForm();

			ModuleDecisionProvider.InitialiseFindBoxControllerLink(controller);
			return form;
		}

		protected internal virtual IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;

			if (HasTypeErrorForSelectedBusinessObjects(selectedBusinessObject))
			{
				ShowIncorrectTypeErrorMessage();
			}
			else
			{
				result = TryShowFormSafelyOnMainThread(selectedBusinessObject, (controller, safeBusinessObject) => controller.ShowEditForm(safeBusinessObject), ModuleDecisionProvider.InitialiseFindBoxControllerLink);
			}

			return result;
		}

		protected internal virtual IZForm[] ShowEditForm(BusinessObject[] selectedBusinessObjects)
		{
			return ShowForms(selectedBusinessObjects, false);
		}

		protected internal virtual IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;

			if (HasTypeErrorForSelectedBusinessObjects(selectedBusinessObject))
			{
				ShowIncorrectTypeErrorMessage();
			}
			else
			{
				result = TryShowFormSafelyOnMainThread(selectedBusinessObject, (controller, safeBusinessObject) => controller.ShowViewForm(safeBusinessObject));
			}

			return result;
		}
		protected void RunOnMainThread<T>(Func<T> getParametersOnCurrentThreadFunc, Action<T> showFormFunc)
		{
			T parameters = getParametersOnCurrentThreadFunc.Invoke();
			MainThreadRunner.RunOnMainThread(() =>
			{
				showFormFunc.Invoke(parameters);
			});
		}

		protected IZForm TryShowFormSafelyOnMainThread(BusinessObject businessObject, Func<ZController, BusinessObject, IZForm> showFormFunc, Action<ZController> initialiseControllerAction = null, Func<BusinessObject, bool> shouldShowFormFunc = null, Func<ZController> createControllerAction = null)
		{
			return MainThreadRunner.RunOnMainThreadSync(() =>
			{
				var safeBusinessObject = GetOnCurrentThread(businessObject);

				if (safeBusinessObject == null)
				{
					Globals.Message.ShowError(Res.GetString("a05b65c0-619e-43b5-bf06-b0177099d23a", "Unable to display the selected record. It may have been deleted."));
					return null;
				}

				if (shouldShowFormFunc != null && !shouldShowFormFunc(safeBusinessObject))
				{
					return null;
				}

				var controller = createControllerAction == null ? GetNewControllerInternal(safeBusinessObject, false) : createControllerAction();
				var form = showFormFunc(controller, safeBusinessObject);
				initialiseControllerAction?.Invoke(controller);

				return form;
			});
		}

		protected internal virtual IZForm[] ShowViewForm(BusinessObject[] selectedBusinessObjects)
		{
			return ShowForms(selectedBusinessObjects, true);
		}

		protected internal virtual IZForm[] ShowForms(BusinessObject[] selectedBusinessObjects, bool view)
		{
			selectedBusinessObjects = selectedBusinessObjects.Where(x => x != null).ToArray();
			var result = new List<IZForm>();
			var initialHandles = -1;
			var afterHandles = -1;
			var handlesPerForm = -1;
			var i = 0;

			foreach (var bizO in selectedBusinessObjects)
			{
				result.Add(view ? ShowViewForm(bizO) : ShowEditForm(bizO));

				if (initialHandles <= -1)
				{
					initialHandles = NativeMethods.GetWindowHandlesForCurrentProcess();
				}
				else
				{
					++i;
					afterHandles = NativeMethods.GetWindowHandlesForCurrentProcess();
					handlesPerForm = (afterHandles - initialHandles) / i; //estimate gets better the more forms are opened
				}

				//prevent division by zero by checking if handlesPerForm > 0
				//improve accuracy by letting the first form open, and then averaging the next 2+ forms
				if (i >= 2 && handlesPerForm > 0 && initialHandles + (handlesPerForm * selectedBusinessObjects.Length) > NativeMethods.GuiResourcesThreshold)
				{
					var recordsLeft = (NativeMethods.GuiResourcesThreshold - afterHandles) / handlesPerForm;
					ShowTooManyRecordsSelectedMessage(recordsLeft);
					return result.ToArray();
				}
			}

			return result.ToArray();
		}

#if DEBUG
		public IZForm[] ShowForms_Exposed(BusinessObject[] selectedBusinessObjects, bool view)
		{
			return ShowForms(selectedBusinessObjects, view);
		}
#endif

		protected virtual bool HasTypeErrorForSelectedBusinessObjects(BusinessObject selectedBusinessObject)
		{
			return false;  //Module which needs this check can override it
		}

		protected void ShowIncorrectTypeErrorMessage()
		{
			Enterprise.ZArchitecture.Environment.Globals.Message.Show(IncorrectTypeErrorMessage,
				Res.GetString("d4a9c65d-cab0-42cc-a78e-c86f930cb963", "Action cannot be completed"), MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
		}

		protected virtual ZString IncorrectTypeErrorMessage
		{
			get { return Res.GetString("4cb44aae-49a3-4d9f-b212-57f35e5d8892", "The selected object is no longer valid. Please refresh the grid and try again."); }
		}

		protected internal virtual IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;

			if (HasTypeErrorForSelectedBusinessObjects(selectedBusinessObject))
			{
				ShowIncorrectTypeErrorMessage();
			}
			else
			{
				result = TryShowFormSafelyOnMainThread(selectedBusinessObject, (controller, safeBusinessObject) => controller.ShowDeleteForm(safeBusinessObject));
			}

			return result;
		}

		protected internal virtual void DeleteMultiple(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Any(HasTypeErrorForSelectedBusinessObjects))
			{
				ShowIncorrectTypeErrorMessage();
			}
			else
			{
				GetNewControllerInternal(selectedBusinessObjects[0], false).DeleteMultiple(selectedBusinessObjects);
			}
		}

		protected internal virtual void DeleteMultipleWithoutListing(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Any(HasTypeErrorForSelectedBusinessObjects))
			{
				ShowIncorrectTypeErrorMessage();
			}
			else
			{
				GetNewControllerInternal(selectedBusinessObjects[0], false).DeleteMultipleWithoutListing(selectedBusinessObjects);
			}
		}

		protected virtual IZForm ShowTemplateCopyAndReverseForm(BusinessObject selectedBusinessObject)
		{
			var safeBizo = GetOnCurrentThread(selectedBusinessObject);
			return GetNewControllerInternal(safeBizo, true).ShowCopyAndReverseForm(safeBizo);
		}

		protected virtual IZForm ShowTemplateCopyForm(BusinessObject selectedBusinessObject)
		{
			var safeBizo = GetOnCurrentThread(selectedBusinessObject);
			return GetNewControllerInternal(safeBizo, true).ShowTemplateCopyForm(safeBizo);
		}

		BusinessObject GetOnCurrentThread(BusinessObject maybeUnsafeBizo)
		{
			return currentThreadBusinessObjectLoader.GetOnCurrentThread(maybeUnsafeBizo);
		}

		CurrentThreadBusinessObjectLoader currentThreadBusinessObjectLoader = new CurrentThreadBusinessObjectLoader();

		protected internal ZPKCollection ModuleResultsPKCollection
		{
			get;
			set;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debug message")]
		ZController GetNewControllerInternal(BusinessObject selectedBusinessObject, bool setCollectionFormDefaultsAndValidation)
		{
			var controller = GetNewController(selectedBusinessObject);

			if (controller == null)
			{
				var exceptionText = "GetNewController() returns null : this.GetType() == " + GetType().FullName + ",";
				if (selectedBusinessObject != null)
				{
					exceptionText += "SelectedBusinessObject.GetType() == " + selectedBusinessObject.GetType().FullName;
				}
				else
				{
					exceptionText += "SelectedBusinessObject == null";
				}
				throw new ModuleCannotFindControllerException(exceptionText);
			}

			controller.ParentModule = this;
			controller.SetLicenceCheckpointsOverrides(LicenceCheckPointOverride as Licensing.LicenceCheckpoint);
			if (ModuleResultsPKCollection != null)
			{
				controller.ModuleResultsPKCollection = ModuleResultsPKCollection;
			}

			if (setCollectionFormDefaultsAndValidation)
			{
				AdditionalSetupForCollectionDefaults(ParentModalForm);
				controller.SetCollectionForDefaultsAndValidation(GridCollection);
			}

			if (GridCollection.Factory != null)
			{
				controller.AddAdditionalDomainValidationGroups(GridCollection.Factory.Validation);
			}

			if (ParentModalForm != null && !ParentModalForm.InvokeRequired)
			{
				controller.SetFormsModalTo(ParentModalForm);
			}

			controller.EnablePreviousNextSupport = ModuleDecisionProvider.EnablePreviousNextSupport;
			controller.InitialTabPageNameToSelectWhenAFormIsShown = InitialTabPageNameToSelectWhenAFormIsShown;

#if DEBUG
			// This is a testing only feature because it is a MEMORY LEAK. If you think the code is ugly, refactor it so it's not leaky.
			if (Globals.IsTest)
			{
				LastController = controller;
			}
#endif
			return controller;
		}

		protected virtual void AdditionalSetupForCollectionDefaults(Form parentModalForm)
		{
		}

		public ZString DefaultMessageWhenCreatingANewBizObjFromFindBox(IFindBox findBox)
		{
			return DefaultMessageWhenCreatingANewBizObjFromFindBoxCore(findBox);
		}

		protected virtual ZString DefaultMessageWhenCreatingANewBizObjFromFindBoxCore(IFindBox findBox)
		{
			return Res.GetString("a9de96bb-1045-4954-bb27-9690ec95c804", "The code '{0}' does not exist. Would you like to create a new {1}?", findBox.Code, Description);
		}

		public virtual ZString DefaultMessageOverridingSecurityRightMessage
		{ get; set; }

		#endregion

		#region Show as Popup Form
		public override IZForm GetForm()
		{
			return new EmbeddedModulePopupWithNoButtonPanelAndNoModality(this);
		}

		public override IZForm ShowPopup()
		{
			var popup = GetForm();
			popup.Show();
			return popup;
		}

#if DEBUG
		internal
#endif
		class EmbeddedModulePopupWithNoButtonPanelAndNoModality : EmbeddedModulePopup
		{
			public EmbeddedModulePopupWithNoButtonPanelAndNoModality(ZFilterModule module)
				: base(module)
			{
				ButtonPanel.Visible = false;
				module.SetFormsModalTo(null);
				OpenedFormCache.GetInstance().AdditionalFormsCount += 1;
			}

			bool loweredAdditionalFormsCount;

			protected override void Dispose(bool isDisposing)
			{
				if (!loweredAdditionalFormsCount)
				{
					OpenedFormCache.GetInstance().AdditionalFormsCount -= 1;
					loweredAdditionalFormsCount = true;
				}

				base.Dispose(isDisposing);
			}
		}

		#endregion

		#region IShowNewForm

		IZForm IShowNewForm.ShowNewForm()
		{
			return ShowNewForm();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				try
				{
					searchManager?.Dispose();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("DisposeGridCollection threw", e);
				}

				try
				{
					DisposeFilterBusinessObject();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("DisposeFilterBusinessObject threw", e);
				}
			}

			base.Dispose(isDisposing);
		}

		protected virtual void DisposeFilterBusinessObject()
		{
			if (filterBusinessObject != null && !DoNotCheckOrSaveChanges && ModuleDecisionProvider.ShouldSaveFilterBizObj)
			{
				filterBusinessObject.RunPreSaveValidation();
				if (!filterBusinessObject.HasErrors)
				{
					if (FilterStripBusinessObject != null)
					{
						try
						{
							FilterStripBusinessObject.SaveLastUsedLayout(); // the active filter name
						}
						catch (Exception e) when (e is ZSaveConcurrencyException || e is ZCannotSaveException)
						{
						}
					}
				}
			}

			if (filterBusinessObject != null)
			{
				filterBusinessObject.ModuleFiltersCreated -= new ModuleFiltersCreatedHandler(filterBusinessObject_ModuleFiltersCreated);
				filterBusinessObject.SearchTypeChanged -= SearchTypeChanged;
				filterBusinessObject.OnGlowIndexQueryErrorAction -= OnGlowIndexQueryErrorAction;

				if (filterBusinessObject.NullOutParentModuleOnDispose)
				{
					filterBusinessObject.ParentModule = null;
					filterBusinessObject = null;
				}
			}
		}

		public bool DoNotCheckOrSaveChanges { get; set; }

		#endregion

		#region Implementation

		FilteredGridLoader searchManager;
		protected FilteredGridLoader SearchManager
		{
			get
			{
				if (searchManager == null)
				{
					searchManager = CreateSearchManager();
					searchManager.ParentForm = ParentModalForm;
					searchManager.MaxRowsToLoad = MaxRowsToLoad;
				}

				return searchManager;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected void SwapInNewFactory()
			=> searchManager?.SwapInNewFactory(GridCollection);

		protected internal virtual int MaxRowsToLoad => maxRowsToLoad ?? (maxRowsToLoad = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value).Value;
		int? maxRowsToLoad;

		protected int MaxRecommendedRowsToLoad => maxRecommendedRowsToLoad ?? (maxRecommendedRowsToLoad = EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids).Value;
		int? maxRecommendedRowsToLoad;

		protected ZBool IsPerformingSearch;
		protected ZBool HasSearched;
		public IFilterControl filter;
		protected ZBool HasMaxRowsLoaded;

		public void UpdateModuleResultsCache()
			=> SearchManager.UpdateModuleResultsCache(GridCollection);

		#region Embedded Control

		protected abstract FilteredGridLoader CreateSearchManager();

		protected override Control GetNewEmbeddedControl()
		{
			var filterControl = GetNewFilterControl();
			filterControl.ParentModuleID = ID;

			if (filterControl != null)
			{
				if (filterControl.FilteredGrid != null)
				{
					filterControl.FilteredGrid.LimitedColumns = LimitedColumns as ZLimitedColumnsProvider;
					filterControl.FilteredGrid.SetReadOnlyIncludingColumnStyles(true);
				}

				if (ShouldPerformSearchAsync && IsModuleAllowAsync)
				{
					filterControl.PerformSearchAsync += FilterControl_PerformSearchAsync;
				}
				else
				{
					filterControl.PerformSearch += FilterControl_PerformSearch;
				}
			}

			foreach (var strategy in FilterModuleStrategies)
			{
				strategy.RunOnFilterControlInitialisation(filterControl, GridCollection);
			}

			filter = filterControl;

			return (Control)filterControl;
		}

		public bool ShouldPerformSearchAsync { get; set; }

		protected virtual bool IsModuleAllowAsync => true;

		public void CommitAllFilters()
		{
			filter?.CommitAllFilters();
		}

		#endregion

		IEnumerable<FilterModuleStrategy> FilterModuleStrategies
		{
			get
			{
				foreach (FilterModuleStrategy strategy in (IEnumerable)ObjectFactory.Get("FilterModuleStrategies"))
				{
					yield return strategy;
				}
			}
		}

		void FilterControl_PerformSearch(object sender, PerformSearchEventArgs e)
		{
			PerformSearch(e);
		}

		void FilterControl_PerformSearchAsync(object sender, PerformSearchAsyncEventArgs e)
		{
			PerformSearchAsync(e);
		}

		protected internal abstract void PerformSearch(PerformSearchEventArgs performSearchArgs = null);

		protected internal virtual void PerformSearchAsync(PerformSearchAsyncEventArgs performSearchArgs = null)
		{
		}

#if DEBUG
		public void PerformSearch_ForTest()
		{
			PerformSearch();
		}
#endif

#if DEBUG
		public bool sqlExceptionOccured;
		public void ThrowSqlExceptionForTest()
		{
			var sqlError = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlError(258, byte.MaxValue, byte.MinValue, "dbserver", "TCP Provider: Timeout error ", "@@lols", 0);
			var errors = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);
			var exception = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(errors);
			throw exception;
		}
#endif

		protected abstract void SuspendGridLayout();
		protected abstract void ResumeGridLayout();
		protected abstract void ForceGridPreFetch();

		protected void SetNotificationsOnRowsAsAppropriate()
		{
			if (ModuleDecisionProvider.ShouldIgnoreAdditionalFilter)
			{
				var gridCollectionExtraNotificationProvider = GridCollection as IFilterModuleExtraNotificationProvider;
				var moduleDecisionProviderExtraNotificationProvider = ModuleDecisionProvider as IFilterModuleExtraNotificationProvider;
				foreach (BusinessObject bizObj in GridCollection)
				{
					if (!bizObj.MatchesFilter(RowValidationFilter))
					{
						using (bizObj.ResumeValidationTemporarily())
						{
							SetNotificationsOnRow(bizObj);
						}
					}
					else
					{
						CheckExtraNotification(gridCollectionExtraNotificationProvider, bizObj);
						CheckExtraNotification(moduleDecisionProviderExtraNotificationProvider, bizObj);
					}
				}
			}
		}

		void CheckExtraNotification(IFilterModuleExtraNotificationProvider extraNotificationProvider, BusinessObject bizObj)
		{
			if (extraNotificationProvider != null)
			{
				var extraNotification = extraNotificationProvider.GetExtraNotification(bizObj);
				if (extraNotification != null)
				{
					using (bizObj.ResumeValidationTemporarily())
					{
						bizObj.AddRowNotification(extraNotification);
					}
				}
			}
		}

		void SetNotificationsOnRow(BusinessObject bizObj)
		{
			bizObj.AddRowNotification(new Notification(GetNotificationTypeWhenAdditionalFilterNotMet(), GetAllNotificationsWhenAdditionalFilterNotMet(bizObj)));
		}

		INotificationType GetNotificationTypeWhenAdditionalFilterNotMet()
		{
			var result = CargoWise.ComponentModel.NotificationType.Error;

			if (GridCollection is BusinessObjectCollection legacyGridCollection)
			{
				result = legacyGridCollection.GetNotificationTyoeWhenAdditionalFilterNotMet();
			}

			if (GridCollection is IActiveBusinessObjectCollection activeCollection)
			{
				result = activeCollection.GetNotificationTyoeWhenAdditionalFilterNotMet();
			}

			return result;
		}

		ZString GetAllNotificationsWhenAdditionalFilterNotMet(BusinessObject selectedBusinessObject)
		{
			var result = ZString.Empty;

			if (DisallowedPKList != null && ((IList)DisallowedPKList).Contains(selectedBusinessObject.PK))
			{
				result = Res.GetString("FilterModule|ThisRecordHasAlreadyBeenSelected", "This record has already been selected. Please ensure you select only records that have not already been used.");
			}
			else
			{
				if (GridCollection is BusinessObjectCollection legacyGridCollection)
				{
					result = legacyGridCollection.GetAllNotificationsWhenAdditionalFilterNotMet(selectedBusinessObject);
				}

				if (GridCollection is IActiveBusinessObjectCollection activeCollection)
				{
					result = activeCollection.GetAllNotificationsWhenAdditionalFilterNotMet(selectedBusinessObject);
				}
			}

			return result;
		}

		internal ZGuid[] DisallowedPKList { get; set; }

		ZQuery IGlowReportingModuleDataQueryProvider.BuildQuery() => GetDisplayResultsQuery();

		protected virtual ZQuery GetDisplayResultsQuery()
		{
			var collection = GridCollection;
			return GetQueryForCollection(collection);
		}

		protected virtual ZQuery GetQueryForCollection(IBusinessObjectCollection collection)
		{
			var result = new ZQuery();
			if (collection is BusinessObjectCollection legacyCollection)
			{
				result = GetDisplayResultsQuery_ForLegacyGridCollection(legacyCollection);
			}
			if (LoadActiveCollection)
			{
				if (collection is IActiveBusinessObjectCollection activeCollection)
				{
					result = GetDisplayResultsQuery_ForActiveCollection(activeCollection);
				}
			}
			AddAdditionalDisplayFilter?.Invoke(result);

			return result;
		}

		internal bool LoadActiveCollection { get; set; }

		public Action<ZQuery> AddAdditionalDisplayFilter { get; set; }

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (gridCollection == null)
				{
					return originalFactory ?? (originalFactory = GetNewFactory());
				}

				return GridCollection.Factory;
			}
		}
		BusinessObjectFactory originalFactory;

		public IBusinessObjectCollection GridCollection
		{
			get
			{
				if (gridCollection == null)
				{
					InitializeGridCollectionAndRowValidationFilter();
				}
				return gridCollection;
			}
		}
		IBusinessObjectCollection gridCollection;

		ZQuery GetDisplayResultsQuery_ForLegacyGridCollection(BusinessObjectCollection collection)
		{
			var query = FilterWithMaximumRows;
			query.AddToFilter(((ILegacyBusinessObjectCollectionInternals)collection).RelationshipFilter);
			query.AddToFilter(((ILegacyBusinessObjectCollectionInternals)collection).AdditionalRelationshipFilter);
			if (!ModuleDecisionProvider.ShouldIgnoreAdditionalFilter)
			{
				query.AddToFilter(RowValidationFilter);
			}
			return query;
		}

		ZQuery relationshipFilter;

		protected void SetRelationshipFilter(IActiveBusinessObjectCollection collection)
		{
			relationshipFilter = (relationshipFilter ?? collection?.RelationshipFilter); // Is important that we cache this and don't change it, since we change the relationship later.
		}

		ZQuery GetDisplayResultsQuery_ForActiveCollection(IActiveBusinessObjectCollection collection)
		{
			var query = FilterWithMaximumRows;
			SetRelationshipFilter(collection);
			query.AddToFilter(relationshipFilter);
			if (!ModuleDecisionProvider.ShouldIgnoreAdditionalFilter)
			{
				query.AddToFilter(RowValidationFilter);
			}
			return query;
		}

		protected ResultCountMessage ResultCountMessage
		{
			get { return resultCountMessage ?? (resultCountMessage = GetNewResultCountMessage()); }
		}
		ResultCountMessage resultCountMessage;

		protected virtual ResultCountMessage GetNewResultCountMessage()
		{
			return new ResultCountMessage((IFilterControl)EmbeddedControl, MaxRowsToLoad, MaxRecommendedRowsToLoad);
		}

		#endregion

		#region Testing Only
#if DEBUG

		internal void OverrideCurrentThreadBusinessObjectLoader_ForTest(CurrentThreadBusinessObjectLoader loader)
		{
			currentThreadBusinessObjectLoader = loader;
		}

		// This is a testing only feature because it is a MEMORY LEAK
		internal ZController LastController;
		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		public static bool ReturnSingleRow;

		#region IFilterModuleInternalsForTesting Members

		ZController IFilterModuleInternalsForTesting.LastController
		{
			get { return LastController; }
			set
			{
				if (Globals.IsTest)
				{
					LastController = value;
				}
			}
		}

		void IFilterModuleInternalsForTesting.PerformSearch()
		{
			PerformSearch();
		}

		IZForm IFilterModuleInternalsForTesting.ShowNewForm()
		{
			return ShowNewForm();
		}

		IZForm IFilterModuleInternalsForTesting.ShowViewForm(BusinessObject selectedBusinessObject)
		{
			return ShowViewForm(selectedBusinessObject);
		}

		IZForm IFilterModuleInternalsForTesting.ShowEditForm(BusinessObject selectedBusinessObject)
		{
			return ShowEditForm(selectedBusinessObject);
		}

		FilterBusinessObject IFilterModuleInternalsForTesting.FilterBusinessObject
		{
			get { return FilterBusinessObject; }
		}

		IBusinessObjectCollection IFilterModuleInternalsForTesting.GridCollection
		{
			get { return GridCollection; }
		}

		FilterModuleMenuItemDescriptorCollection IFilterModuleInternalsForTesting.ImportMenuItems
		{
			get { return ImportMenuItems; }
		}

		#endregion

#endif
		#endregion
	}
}
