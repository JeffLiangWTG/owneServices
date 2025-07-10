using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.Internal;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZSearchControl : BaseUserControl, ISearchControl
	{
		public event EventHandler CustomNewButtonClick;
		public event EventHandler CustomViewButtonClick;
		public event EventHandler CustomButton1Click;

		HtmlGenericControl resultsGridDiv;
		HtmlGenericControl filterControl;
		ZGrid searchResultsDataGrid;

		#region Controls and Page Layout

		#region Grid

		public ZGrid SearchResultsDataGrid
		{
			get
			{
				if (searchResultsDataGrid == null)
				{
					searchResultsDataGrid = GetNewDataGrid();
					searchResultsDataGrid.ID = "SearchResultsDataGrid";
					searchResultsDataGrid.HideButtonsToMenu = false;
					searchResultsDataGrid.AllowCollapseOfAlwaysVisibleHolder = true;
					searchResultsDataGrid.ShowCollapsedMessage = ShowCollapsedMessage;
				}
				return searchResultsDataGrid;
			}
		}

		protected virtual ZGrid GetNewDataGrid() => new ZGrid();

		#region Setup Grid

		protected void SetupGrid(ZGrid grid)
		{
			string scrollStyle = ZString.Empty;
			switch (ScrollingStyle)
			{
				case ScrollingStyle.Full:
					scrollStyle = CssConstants.ScrollableFull;
					break;
				case ScrollingStyle.Horizontal:
					scrollStyle = CssConstants.Scrollable;
					break;
			}
			ResultsGridDiv.Attributes["class"] = ZCssHelper.Join(CssConstants.ContentSection, scrollStyle);
			grid.CssClass = ZCssHelper.Join(grid.CssClass, TableCss);
			grid.HeaderStyle.CssClass = ZCssHelper.Join(grid.HeaderStyle.CssClass, HeaderCss);
			grid.ItemStyle.CssClass = ZCssHelper.Join(grid.ItemStyle.CssClass, ItemCss);
			grid.AlternatingItemStyle.CssClass = ZCssHelper.Join(grid.AlternatingItemStyle.CssClass, AlternatingCss);
			grid.SelectedItemStyle.CssClass = ZCssHelper.Join(grid.SelectedItemStyle.CssClass, SelectedCss);

			grid.PagerStyle.Mode = PagerMode.NumericPages;
			grid.PagerStyle.CssClass = ZCssHelper.Join(grid.CssClass, PagerCss);
			grid.AllowPaging = true;
			grid.PageSize = PageSize;

			grid.SortCommand += new DataGridSortCommandEventHandler(SearchResultsDataGrid_SortCommand);
			grid.AllowSorting = Module.AllowSort;
			grid.PageIndexChanged += new DataGridPageChangedEventHandler(SearchResultsDataGrid_PageIndexChanged);

			grid.GetCollectionToExportOverride = () =>
			{
				var collectionToExport = Module.LoadExcelCollection(FilterBusinessObject);
				SortResults(collectionToExport, Module);

				return collectionToExport;
			};
		}

		protected void SetupGridColumns(ZGrid grid)
		{
			if (FilterStripBizO.LastUsedLayout != null)
			{
				grid.LayoutNameToUse = FilterStripBizO.LastUsedLayout.S9_FilterNameMultilingual.GetUnresolvedString();
			}
			var colProvider = new GridColumnProvider();
			if (Module is ZFilterStripGridModule)
			{
				colProvider = ((ZFilterStripGridModule)Module).ColumnProvider;
				if (SupportEDocsBulkDownload)
				{
					ZGridModuleEDocsHelper.AddEDocsColumnsToProvider(this.Factory, grid, (ISupportEDocsBulkDownload)Module, colProvider);
				}
			}
			grid.ColumnProvider = colProvider;
		}

		bool SupportEDocsBulkDownload
		{
			get { return Module is ISupportEDocsBulkDownload && (Module as ISupportEDocsBulkDownload).GetRegistryWebEDocsBulkDownload() != null; }
		}

#endregion

#endregion

#region Filter Control

		protected HtmlGenericControl FilterControl
		{
			get { return filterControl ?? (filterControl = NewHtmlGenericControl("FilterControl")); }
		}

#endregion

#region ResultsGridDiv

		protected HtmlGenericControl ResultsGridDiv
		{
			get { return resultsGridDiv ?? (resultsGridDiv = NewHtmlGenericControl("ResultsGridDiv")); }
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html contant")]
		protected HtmlGenericControl NewHtmlGenericControl(string id)
		{
			var result = new HtmlGenericControl("div");
			result.ID = id;
			result.Attributes.Add("class", CssConstants.ContentSection);
			return result;
		}

		#endregion

		#region Buttons

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is a Css Class")]
		protected Button CustomButton1
		{
			get
			{
				if (customButton1 == null)
				{
					customButton1 = new Button
					{
						ID = "btnCustomerButton1SearchControl",
						CssClass = "Button",
					};
				}
				return customButton1;
			}
		}
		Button customButton1;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is a Css Class")]
		protected Button FindButton
		{
			get
			{
				if (findButton == null)
				{
					findButton = new Button
					{
						CssClass = "Button FindButton",
						Text = SearchControl.Labels.Find,
						ToolTip = SearchControl.Labels.FindToolTip
					};
				}
				return findButton;
			}
		}
		Button findButton;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is a Css Class")]
		protected Button ClearButton
		{
			get
			{
				if (clearButton == null)
				{
					clearButton = new Button
					{
						CssClass = "Button ClearButton",
						Text = SearchControl.Labels.Clear,
						ToolTip = SearchControl.Labels.ClearToolTip
					};
				}
				return clearButton;
			}
		}
		Button clearButton;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is a Css Class")]
		protected Button NewButton
		{
			get
			{
				if (newButton == null)
				{
					newButton = new Button
					{
						ID = "btnNewButtonSearchControl",
						CssClass = "Button NewButton",
						Text = SearchControl.Labels.New,
						ToolTip = SearchControl.Labels.New
					};
				}
				return newButton;
			}
		}
		Button newButton;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is a Css Class")]
		protected Button ViewButton
		{
			get
			{
				if (viewButton == null)
				{
					viewButton = new Button
					{
						ID = "btnViewButtonSearchControl",
						CssClass = "Button ViewButton",
						Text = SearchControl.Labels.ViewSelected,
						ToolTip = SearchControl.Labels.ViewSelected
					};
				}
				return viewButton;
			}
		}
		Button viewButton;

#endregion

#region Custom Control

		public Control CustomControl { get; set; }

		#endregion

		#region base Overrides

#if DEBUG
		public void InitResultsGridForTesting()
		{
			InitResultsGrid();
			SearchResultsDataGrid.InitGridControllerForTesting();
			SetupGridColumns(SearchResultsDataGrid);
		}
#endif
		void InitResultsGrid()
		{
			ResultsGridDiv.Controls.Add(SearchResultsDataGrid);

			this.Controls.Add(FilterControl);
			this.Controls.Add(ResultsGridDiv);
		}

		protected override void OnInit(EventArgs e)
		{
			ShowCollapsedMessage = true;
			base.OnInit(e);
			InitResultsGrid();

			SearchResultsDataGrid.Header.Controls.Add(NewButton);

			if (CustomControl != null)
			{
				SearchResultsDataGrid.Header.Controls.AddAt(0, CustomControl);
			}

			SearchResultsDataGrid.Menu.Controls.AddAt(0, ViewButton);
			SearchResultsDataGrid.RegisterPostBackControl(ViewButton);
			SearchResultsDataGrid.Menu.Controls.AddAt(0, CustomButton1);
			SearchResultsDataGrid.RegisterPostBackControl(CustomButton1);

			NewButton.Click += NewButton_Click;
			ViewButton.Click += ViewButton_Click;
			CustomButton1.Click += CustomButton1_Click;

			NewButton.Visible = IsNewButtonVisible;
			NewButton.OnClientClick = NewButtonClientClickScript;

			ViewButton.OnClientClick = ViewButtonClientClickScript;
			ViewButton.Visible = IsViewButtonVisible;

			CustomButton1.Text = CustomButton1Text;
			CustomButton1.ToolTip = CustomButton1ToolTip;
			CustomButton1.OnClientClick = CustomButton1ClientClickScript;
			CustomButton1.Visible = IsCustomButton1Visible;

			SearchResultsDataGrid.ShowExportToExcelButton = IsExportToExcelButtonVisible;
			SearchResultsDataGrid.ShowCustomizeColumnsButton = IsCustomiseColumnsButtonVisible;
			SearchResultsDataGrid.ShowDownloadEDocsButton = IsDownloadEDocsButtonVisible;

			SetupFilterControl();

			SetupGrid(SearchResultsDataGrid);
		}

		public
#if DEBUG
 virtual
#endif
 void RePopulateGrid()
		{
			SearchResultsDataGrid.AllowCustomPaging = false;
			if (!SearchResultsDataGrid.Collapsed && !FindCommandHasBeenExecuted) // don't run 'Find' on first entering a module
			{
				//the LoadCollectionFromCachedPKs() is very slow for large number of rows, it should be fixed or excluded from the code as LoadAndPopulateGrid() is much faster! 
				if (LoadCollectionFromCachedPKs())
				{
					PopulateGrid();
				}
				else
				{
					LoadAndPopulateGrid();
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			SetupGridColumns(SearchResultsDataGrid);
			if (FilterStripBizO != null)
			{
				FilterStripBizO.LayoutLoaded += new EventHandler(FilterStripBizO_LayoutLoaded);
			}
			base.OnLoad(e);
			if (IsResultsRelatedOperation)
			{
				RePopulateGrid();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			SearchResultsDataGrid.Collapsed = !IsResultsRelatedOperation || !SearchResultsDataGrid.IsBound;
			SearchResultsDataGrid.CollapsedMessageLabel.Text = Res.GetString("d8521f1a-ba46-4167-9730-e96ba10553b1", "Click 'Find' to refresh search results.");
			SearchResultsDataGrid.ShowCollapsedMessage = Page.IsPostBack && ShowCollapsedMessage;
			base.OnPreRender(e);
		}

		public bool IsResultsRelatedOperation
		{
			get
			{
				if (isResultsRelatedOperation == null)
				{
					isResultsRelatedOperation = true;
					if (Page.IsPostBack)
					{
						var ajaxPage = Page as ZAjaxPage;
						if (ajaxPage != null && ajaxPage.IsAsyncPostBack)
						{
							isResultsRelatedOperation = ajaxPage.AsyncPostBackSourceElementID.StartsWith(SearchResultsDataGrid.ControllerUniqueID) || ajaxPage.AsyncPostBackSourceElementID.EndsWith("TextBox", StringComparison.InvariantCultureIgnoreCase);
						}
					}
					RaiseOnIsResultsRelatedOperationChanged();
				}
				return isResultsRelatedOperation.Value;
			}
			set
			{
				isResultsRelatedOperation = value;
				RaiseOnIsResultsRelatedOperationChanged();
			}
		}

		bool? isResultsRelatedOperation;

		public event EventHandler OnIsResultsRelatedOperationChanged;

		protected void RaiseOnIsResultsRelatedOperationChanged()
		{
			if (OnIsResultsRelatedOperationChanged != null)
			{
				OnIsResultsRelatedOperationChanged(this, new EventArgs());
			}
		}

		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);
			if (FilterStripBizO != null)
			{
				FilterStripBizO.LayoutLoaded -= new EventHandler(FilterStripBizO_LayoutLoaded);
			}
		}

		void FilterStripBizO_LayoutLoaded(object sender, EventArgs e)
		{
			IsResultsRelatedOperation = false;
		}

		public override void Dispose()
		{
			if (module != null)
			{
				module.Dispose();
				module = null;
			}
			base.Dispose();
		}

#endregion

#region Properties

		[DefaultValue(true)]
		public bool ShowCollapsedMessage { get; set; }

#region CSS

		public ZString CssClassPrefix
		{
			get { return cssClassPrefix; }
			set { cssClassPrefix = value.Trim(); }
		}
		ZString cssClassPrefix;

		protected string TableCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsTable); }
		}

		protected string ItemCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsCell); }
		}

		protected string AlternatingCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsAlternatingCell); }
		}

		protected string SelectedCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsSelectedCell); }
		}

		protected string PagerCss
		{
			get { return GetBasePageModuleClass(CssConstants.ResultsTablePager); }
		}

		protected string HeaderCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsHeader); }
		}

#endregion

#region New Button

		[Category(ZGUIConstants.DesignerCategory)]
		public bool IsNewButtonVisible
		{
			get { return isNewButtonVisible; }
			set { isNewButtonVisible = value; }
		}
		bool isNewButtonVisible;

		[Category(ZGUIConstants.DesignerCategory)]
		public string NewButtonText
		{
			get { return newButtonText; }
			set { newButtonText = value; }
		}
		string newButtonText = SearchControl.Labels.New;

		[Category(ZGUIConstants.DesignerCategory)]
		public string NewButtonUrl
		{
			get { return newButtonUrl; }
			set { newButtonUrl = value; }
		}
		string newButtonUrl;

		[Category(ZGUIConstants.DesignerCategory)]
		public string NewButtonToolTip
		{
			get
			{
				return !string.IsNullOrEmpty(newButtonToolTip) ? newButtonToolTip : NewButtonText;
			}
			set { newButtonToolTip = value; }
		}
		string newButtonToolTip;

		[Category(ZGUIConstants.DesignerCategory)]
		public string NewButtonClientClickScript
		{
			get { return newButtonClientClickScript; }
			set { newButtonClientClickScript = value; }
		}
		string newButtonClientClickScript;

#endregion New Button

#region Custom Button1

		[Category(ZGUIConstants.DesignerCategory)]
		public bool IsCustomButton1Visible
		{
			get { return isCustomButton1Visible; }
			set { isCustomButton1Visible = value && (!string.IsNullOrEmpty(CustomButton1Text)); }
		}
		bool isCustomButton1Visible;

		[Category(ZGUIConstants.DesignerCategory)]
		public string CustomButton1Text
		{
			get { return customButton1Text; }
			set { customButton1Text = value; }
		}
		string customButton1Text = "";

		[Category(ZGUIConstants.DesignerCategory)]
		public string CustomButton1Url
		{
			get { return customButton1Url; }
			set { customButton1Url = value; }
		}
		string customButton1Url;

		[Category(ZGUIConstants.DesignerCategory)]
		public string CustomButton1ToolTip
		{
			get
			{
				return !string.IsNullOrEmpty(customButton1ToolTip) ? customButton1ToolTip : CustomButton1Text;
			}
			set { customButton1ToolTip = value; }
		}
		string customButton1ToolTip;

		[Category(ZGUIConstants.DesignerCategory)]
		public string CustomButton1ClientClickScript
		{
			get { return customButton1ClientClickScript; }
			set { customButton1ClientClickScript = value; }
		}
		string customButton1ClientClickScript;

#endregion View Button

#region View Button

		[Category(ZGUIConstants.DesignerCategory)]
		public bool IsViewButtonVisible
		{
			get { return isViewButtonVisible; }
			set { isViewButtonVisible = value; }
		}
		bool isViewButtonVisible;

		[Category(ZGUIConstants.DesignerCategory)]
		public string ViewButtonText
		{
			get { return viewButtonText; }
			set { viewButtonText = value; }
		}
		string viewButtonText = SearchControl.Labels.View;

		[Category(ZGUIConstants.DesignerCategory)]
		public string ViewButtonUrl
		{
			get { return viewButtonUrl; }
			set { viewButtonUrl = value; }
		}
		string viewButtonUrl;

		[Category(ZGUIConstants.DesignerCategory)]
		public string ViewButtonToolTip
		{
			get
			{
				return !string.IsNullOrEmpty(viewButtonToolTip) ? viewButtonToolTip : ViewButtonText;
			}
			set { viewButtonToolTip = value; }
		}
		string viewButtonToolTip;

		[Category(ZGUIConstants.DesignerCategory)]
		public string ViewButtonClientClickScript
		{
			get { return viewButtonClientClickScript; }
			set { viewButtonClientClickScript = value; }
		}
		string viewButtonClientClickScript;

#endregion View Button

#region ExportToExcel

		[Category(ZGUIConstants.DesignerCategory)]
		public bool IsExportToExcelButtonVisible
		{
			get { return (bool)(ViewState["IsExportButtonVisible"] ?? true); }
			set { ViewState["IsExportButtonVisible"] = value; }
		}

#endregion

#region CustomiseColumns

		[Category(ZGUIConstants.DesignerCategory)]
		public bool IsCustomiseColumnsButtonVisible
		{
			get
			{
				object result = ViewState["IsCustomiseColumnsButtonVisible"];
				return (result == null) ? Session != null && WebEnv.CurrentUser != null : (bool)result;
			}
			set { ViewState["IsCustomiseColumnsButtonVisible"] = value; }
		}

#endregion

#region DownloadEDocs

		[Category(ZGUIConstants.DesignerCategory)]
		public bool IsDownloadEDocsButtonVisible
		{
			get { return SupportEDocsBulkDownload; }
		}

#endregion

#region ModuleID

		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(WebModuleIDEditor), typeof(UITypeEditor))]
		public WebModuleID ModuleID
		{
			get { return moduleID; }
			set { moduleID = value; }
		}
		WebModuleID moduleID = WebModuleIDs.NotAssigned;

		// Do *not* remove this method!
		// .NET will use this method to determine whether or not to serialize the ModuleID property value.
		bool ShouldSerializeModuleID()
		{
			return (ModuleID != WebModuleIDs.NotAssigned);
		}

#endregion ModuleID

#region Page Size

		[Category(ZGUIConstants.DesignerCategory)]
		public int PageSize
		{
			get
			{
				if (pageSize == 0)
				{
					pageSize = WebDataRegistry.Instance.PageSize.Value;
				}

				return pageSize;
			}
			set { pageSize = value; }
		}
		protected int pageSize;

#endregion

#region MaxRows

		[Category(ZGUIConstants.DesignerCategory)]
		public int MaxRows
		{
			get { return Module.MaxRows; }
			set { Module.MaxRows = value; }
		}

#endregion

#endregion

#region Module

		public
#if DEBUG
		virtual
#endif
	ZFilterGridModule Module
		{
			get
			{
				if (module == null)
				{
					module = ZWebModuleFactory.Create(ModuleID, Factory, Page);
					if (module == null)
					{
						throw new ZException("Module ID : " + ModuleID + " resolves to null. Web FindBox Modules must be ZGenericFilterGridModule.");
					}
				}
				return module;
			}
		}
#if DEBUG
		protected
#endif
		ZFilterGridModule module;

#endregion Module

#region Setup routines

		protected void SetupFilterControl()
		{
			LoadFilterControl(Module);

			NewButton.Visible = IsNewButtonVisible;
			if (NewButton.Visible)
			{
				NewButton.Text = NewButtonText;
				NewButton.ToolTip = NewButtonToolTip;
			}

			ViewButton.Visible = IsViewButtonVisible;
			if (ViewButton.Visible)
			{
				ViewButton.Text = ViewButtonText;
				ViewButton.ToolTip = ViewButtonToolTip;
			}

			SearchResultsDataGrid.ShowExportToExcelButton = IsExportToExcelButtonVisible;
			SearchResultsDataGrid.ShowCustomizeColumnsButton = IsCustomiseColumnsButtonVisible;
			SearchResultsDataGrid.ShowDownloadEDocsButton = IsDownloadEDocsButtonVisible;
		}

		public void HideFilterHeader()
		{
			FilterControl.Visible = false;
			NewButton.Visible = false;
			ViewButton.Visible = false;
			SearchResultsDataGrid.ShowExportToExcelButton = false;
			SearchResultsDataGrid.ShowCustomizeColumnsButton = false;
			SearchResultsDataGrid.ShowDownloadEDocsButton = false;
		}

		protected
#if DEBUG
		virtual
#endif
		void LoadFilterControl(ZFilterGridModule module)
		{
			if (module.FilterControlResource != null)
			{
				module.FilterControlResource.Extract();
				var control = Page.LoadControl(module.FilterControlResource.FileName);

				var filterControlUpdatePanel = new UpdatePanel();
				filterControlUpdatePanel.ID = "FilterControlUpdatePanel"; // May be an identifier or GUID.
				filterControlUpdatePanel.ContentTemplateContainer.Controls.Add(control);
				FilterControl.Controls.AddAt(0, filterControlUpdatePanel);

				var filterStripControl = control as ZFilterStripControl;
				if (filterStripControl != null)
				{
					filterStripControl.BeforeSaveLayout += OnBeforeSaveLayout;

					if (!ShouldFindCauseAsyncPostBack)
					{
						ScriptManager.GetCurrent(Page)?.RegisterPostBackControl(filterStripControl.FindButton);
					}

					InitialiseFilterStripControl(filterStripControl, module as ZFilterStripGridModule);
				}
			}
		}

		public bool ShouldFindCauseAsyncPostBack { get; set; }

		protected void OnBeforeSaveLayout(object sender, LayoutEventArgs e)
		{
			var layout = e.Layout;
			SearchResultsDataGrid.SaveAsLayoutColumns(layout.S9_FilterNameMultilingual.GetUnresolvedString(), layout.S9_IsPublished, layout.IsPublishedForCompanyInWeb);
		}

		public ZFilterStripControl FilterStripControl
		{
			get
			{
				if (FilterControl != null && FilterControl.Controls.Count > 0)
				{
					var updatePanel = FilterControl.Controls[0] as UpdatePanel;
					if (updatePanel != null && updatePanel.ContentTemplateContainer.Controls.Count > 0)
					{
						return updatePanel.ContentTemplateContainer.Controls[0] as ZFilterStripControl;
					}
				}

				return null;
			}
		}

		void InitialiseFilterStripControl(ZFilterStripControl filterStripControl, ZFilterStripGridModule module)
		{
			filterStripControl.OnFind += new EventHandler(FilterStripControl_OnFind);
			if (module != null)
			{
				filterStripControl.DefaultLayoutName = module.DefaultLayoutName;
			}
		}

		void FilterStripControl_OnFind(object sender, EventArgs e)
		{
			Find();
		}

		public int SelectionColumnIndex
		{
			get
			{
				int result = 0;
				DataGridColumn selectionColumn = Module.SelectionColumn;
				if (SearchResultsDataGrid != null && SearchResultsDataGrid.Columns.Count > 0)
				{
					result = SearchResultsDataGrid.Columns.IndexOf(selectionColumn);
				}
				return result;
			}
		}

		public ScrollingStyle ScrollingStyle
		{
			get { return scrollingStyle; }
			set { scrollingStyle = value; }
		}
		ScrollingStyle scrollingStyle = ScrollingStyle.None;

		string GetBasePageModuleClass(string baseClass)
		{
			List<string> classes = new List<string>();
			classes.Add(baseClass);
			if (!CssClassPrefix.IsEmpty)
			{
				classes.Add(CssClassPrefix + baseClass);
			}

			if (!Module.ResultsGridCssClass.IsEmpty)
			{
				classes.Add(Module.ResultsGridCssClass + baseClass);
			}

			return ZCssHelper.Join(classes.ToArray());
		}

#endregion

#region Filter and Results

		public FilterBusinessObject FilterBusinessObject
		{
			get { return filterStripBizO ?? Page.DataSource as FilterBusinessObject; }
		}

		public FilterStripBusinessObject FilterStripBizO
		{
			get { return filterStripBizO ?? Page.DataSource as FilterStripBusinessObject; }
			set
			{
				filterStripBizO = value;
				filterStripBizO.AddActiveStatusFilters(Module.GridCollection.TypeOfElements);
				if (Module is ZFilterStripGridModule)
				{
					((ZFilterStripGridModule)Module).OverrideFilterStripBizO(filterStripBizO);
				}
				if (FilterStripControl != null)
				{
					FilterStripControl.FilterStripBizO = filterStripBizO;
				}
			}
		}
		FilterStripBusinessObject filterStripBizO;

		protected string SortExpression = "";
		protected ListSortDirection SortDirection = ListSortDirection.Ascending;

#if DEBUG
		public void SetSortExpressionForTesting(string sortExpression) => SortExpression = sortExpression;
#endif

#if DEBUG
		virtual
#endif
		protected void LoadAndPopulateGrid()
		{
			LoadResults(FilterBusinessObject);
			PopulateGrid();
		}

		protected void PopulateGrid()
		{
			SortResults(Module.GridCollection);
			BindResultsGrid();
		}

		void LoadResults(FilterBusinessObject filterBizO)
		{
			Module.LoadCollection(filterBizO);
			SearchResultsDataGrid.MaxRows = MaxRows;
			if (Module.CacheCollectionPKs)
			{
				StoreCollectionPKsForPostback(Module.GridCollection);
			}
			HasLoadedResults = true;
		}

		bool HasLoadedResults
		{
			get
			{
				bool result = false;
				object obj = ViewState[hasLoadedResultsKey];
				if (obj != null)
				{
					result = (bool)obj;
				}
				return result;
			}
			set { ViewState[hasLoadedResultsKey] = value; }
		}
		const string hasLoadedResultsKey = "SearchControlHasLoadedResults";

		void StoreCollectionPKsForPostback(IBusinessObjectCollection collection)
		{
			List<ZGuid> pKs = new List<ZGuid>();
			foreach (BusinessObject bizo in collection)
			{
				pKs.AddRange(Module.GetBusinessObjectPK(bizo));
			}
			SaveCachedPKs(pKs.ToArray());
		}

		void StoreFilterCriteriaIfApplicable()
		{
			if (Page is IRememberFilterCriteriaPage && FilterBusinessObject != null && FilterStripBizO == null)
			{
				FilterBusinessObject.RunPreSaveValidation();
				if (!FilterBusinessObject.HasErrors)
				{
					new WebFilterBusinessObjectFactory(Factory).Save(FilterBusinessObject);
				}
			}
		}

		bool LoadCollectionFromCachedPKs()
		{
			bool result = false;
			ZGuid[] listOfPKs = LoadCachedPKs();
			if (listOfPKs != null && listOfPKs.Length > 0)
			{
				SchemaColumn pkSchemaColumn = Module.GetBusinessObjectPKColumn(FilterBusinessObject);
				ZQuery pkFilter = new ZQuery(pkSchemaColumn, listOfPKs);
				Module.LoadCollection(FilterBusinessObject, pkFilter);
				result = true;
			}
			return result;
		}

		void SortResults(IBusinessObjectCollection collectionToSort)
		{
			SortResults(collectionToSort, Module);
		}

		public void SortResults(IBusinessObjectCollection collectionToSort, ZFilterGridModule module)
		{
			if (!string.IsNullOrEmpty(SortExpression))
			{
				collectionToSort.ApplySort(module.GetNewCollectionSorter(SortExpression, SortDirection));
			}
		}

		void BindResultsGrid()
		{
			SearchResultsDataGrid.Bind(Module.GridCollection, true);
		}

#endregion

#region Event handlers

		public void FindButton_Click(object sender, EventArgs e)
		{
			Find();
		}

		public const string SearchControlIsSearchingIndexer = "searchControlIsSearchingIndexer";

		protected virtual void Find()
		{
			IsResultsRelatedOperation = true;
			Session[SearchControlIsSearchingIndexer] = true;

			FilterBusinessObject.Filter.ReLoadExistingRows = true;
			SearchResultsDataGrid.DisableCollapsing = true;
			SearchResultsDataGrid.ClearMultiLineSelection();
			SearchResultsDataGrid.SetCurrentPageIndex(0);
			LoadAndPopulateGrid();
			StoreFilterCriteriaIfApplicable();

			if (OnSearch != null)
			{
				OnSearch(this, EventArgs.Empty);
			}

			Session.Remove(SearchControlIsSearchingIndexer);

			FindCommandHasBeenExecuted = true;
		}

		public event EventHandler OnSearch;

		[DefaultValue(false)]
		bool FindCommandHasBeenExecuted { get; set; }

		void ClearCollection(IBusinessObjectCollection collection)
		{
			BusinessObjectCollection legacyCollection = collection as BusinessObjectCollection;
			if (legacyCollection != null)
			{
				legacyCollection.RemoveAll();
			}
			IActiveBusinessObjectCollection flyweightCollection = collection as IActiveBusinessObjectCollection;
			if (flyweightCollection != null)
			{
				flyweightCollection.AdditionalFilter = ZQuery.NoResultQuery;
			}
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			Clear();
			if (OnClear != null)
			{
				OnClear(this, e);
			}
		}

		protected void Clear()
		{
			Module.ResetFilterBusinessObject(FilterBusinessObject);
			ClearCollection(Module.GridCollection);
			SearchResultsDataGrid.Collapsed = false;
			SearchResultsDataGrid.ClearMultiLineSelection();
			ClearCachedPKs();
			Page.Bind();
			if (HasLoadedResults)
			{
				BindResultsGrid();
			}
			HasLoadedResults = false;
		}

		public event EventHandler OnClear;

		protected void SearchResultsDataGrid_SortCommand(object source, DataGridSortCommandEventArgs e)
		{
			if (string.IsNullOrEmpty(SortExpression))
			{
				SortExpression = e.SortExpression;
			}
			else
			{
				if (SortExpression == e.SortExpression)
				{
					SortDirection = (SortDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
				}
				else
				{
					SortExpression = e.SortExpression;
					SortDirection = ListSortDirection.Ascending;
				}
			}
			PopulateGrid();
		}

		protected void SearchResultsDataGrid_PageIndexChanged(object source, DataGridPageChangedEventArgs e)
		{
			if (source is ZDataGrid dataGrid)
			{
				dataGrid.SetCurrentPageIndex(e.NewPageIndex);
				PopulateGrid();
			}
		}

		void NewButton_Click(object sender, EventArgs e)
		{
			if (CustomNewButtonClick != null)
			{
				CustomNewButtonClick(this, e);
			}
			if (!string.IsNullOrEmpty(NewButtonUrl))
			{
				Response.Redirect(NewButtonUrl);
			}
		}

		void CustomButton1_Click(object sender, EventArgs e)
		{
			if (CustomButton1Click != null)
			{
				CustomButton1Click(this, e);
			}
			if (!string.IsNullOrEmpty(CustomButton1Url))
			{
				Response.Redirect(CustomButton1Url);
			}
		}

		void ViewButton_Click(object sender, EventArgs e)
		{
			if (CustomViewButtonClick != null)
			{
				CustomViewButtonClick(this, e);
			}
			if (!string.IsNullOrEmpty(ViewButtonUrl))
			{
				Response.Redirect(ViewButtonUrl);
			}
		}

#endregion

#region ViewState

		protected override void LoadViewState(object savedState)
		{
			var viewState = (object[])savedState;
			base.LoadViewState(viewState[0]);
			SortExpression = (string)viewState[1];
			SortDirection = (ListSortDirection)viewState[2];
			Module.SetViewStateData(viewState[3]);
		}

		protected override object SaveViewState()
		{
			var viewState = new object[5];
			viewState[0] = base.SaveViewState();
			viewState[1] = SortExpression;
			viewState[2] = SortDirection;
			viewState[3] = Module.GetViewStateData();
			return (viewState);
		}

#region CachedPKs

		protected virtual void SaveCachedPKs(ZGuid[] array)
		{
			ViewState[collectionPKIndexer] = array;
		}

		protected virtual ZGuid[] LoadCachedPKs()
		{
			return ViewState[collectionPKIndexer] as ZGuid[];
		}

		void ClearCachedPKs()
		{
			ViewState[collectionPKIndexer] = null;
		}
		const string collectionPKIndexer = "ModuleGridCollectionCachedPKs";

#endregion

#endregion
	}
}
