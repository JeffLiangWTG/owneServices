using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.Internal;
using Enterprise.ZArchitecture.Web.GUI.Utilities;
using Enterprise.ZArchitecture.Web.Modules;
using ZFilterGridModule = Enterprise.ZArchitecture.Web.Modules.ZFilterGridModule;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	///	Control to be used on all search pages
	///	Tested through ZFilterPage test classes
	///	</summary>
	public class SearchControl : BaseUserControl, ISearchControl
	{
		public event EventHandler CustomNewButtonClick;
		public event EventHandler CustomViewButtonClick;
		public event EventHandler CustomButton1Click;

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
									Text = Labels.Find,
									ToolTip = Labels.FindToolTip
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
						Text = Labels.Clear,
						ToolTip = Labels.ClearToolTip
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
						Text = Labels.New,
						ToolTip = Labels.New
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
						CssClass = "Button ViewButton",
						Text = Labels.ViewSelected,
						ToolTip = Labels.ViewSelected
					};
				}
				return viewButton;
			}
		}
		Button viewButton;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is a Css Class")]
		public Button ExportToExcelButton
		{
			get
			{
				if (exportToExcelButton == null)
				{
					exportToExcelButton = new Button
					{
						ID = "btnExcel",
						CssClass = "Button ExportToExcelButton",
						Text = Labels.ExportToExcel,
						ToolTip = Labels.ExportToExcel
					};
				}
				return exportToExcelButton;
			}
		}
		Button exportToExcelButton;

		public PlaceHolder ExportToExcelButtonsPlaceHolder
		{
			get
			{
				if (exportToExcelButtonsPlaceHolder == null)
				{
					exportToExcelButtonsPlaceHolder = new PlaceHolder();
					exportToExcelButtonsPlaceHolder.Controls.Add(ExportToExcelButton);
				}
				return exportToExcelButtonsPlaceHolder;
			}
		}
		PlaceHolder exportToExcelButtonsPlaceHolder;

		#endregion

		#region base Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			Buttons.Controls.Add(FindButton);
			Buttons.Controls.Add(ClearButton);
			Buttons.Controls.Add(NewButton);
			Buttons.Controls.Add(ViewButton);
			Buttons.Controls.Add(CustomButton1);
			Buttons.Controls.Add(ExportToExcelButtonsPlaceHolder);

			FindButton.Click += FindButton_Click;
			ClearButton.Click += ClearButton_Click;
			NewButton.Click += NewButton_Click;
			ViewButton.Click += ViewButton_Click;
			CustomButton1.Click += CustomButton1_Click;
			ExportToExcelButton.Click += ExportToExcelButton_Click;

			SetupFilterControl();

			// rara
			SetupGrid();
			if (Session != null)
			{
				SetupGridLayoutControl();
			}
		}

		public
#if DEBUG
		virtual
#endif
		void RePopulateGrid()
		{
			if (IsPostBack) // don't run 'Find' on first entering a module
			{
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
			if (FilterStripBizO != null)
			{
				FilterStripBizO.LayoutLoaded += new EventHandler(FilterStripBizO_LayoutLoaded);
			}

			base.OnLoad(e);
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
			GridLayoutControl.Text = ModuleGridLayoutHelper.CurrentGridLayout;
			if (SearchResultsDataGrid != null && SearchResultsDataGrid.Items.Count > 0)
			{
				SetupGrid();
				RePopulateGrid();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			NewButton.Visible = NewButtonVisible;
			NewButton.OnClientClick = NewButtonClientClickScript;

			bool canExportToExcel = SearchResultsDataGrid.Visible && new DataGridExcelExportHelper(SearchResultsDataGrid).CanContinueWithExport;

			ViewButton.OnClientClick = ViewButtonClientClickScript;
			ViewButton.Visible = ViewButtonVisible && canExportToExcel;

			CustomButton1.Text = CustomButton1Text;
			CustomButton1.ToolTip = CustomButton1ToolTip;
			CustomButton1.OnClientClick = CustomButton1ClientClickScript;
			CustomButton1.Visible = CustomButton1Visible && canExportToExcel;

			ExportToExcelButton.Visible = ExportToExcelButtonVisible && canExportToExcel;
			if (ExportToExcelButtonVisibilityPublisher != null)
			{
				ExportToExcelButtonVisibilityPublisher(ExportToExcelButton.Visible);
			}
			GridLayoutControl.Visible = CustomiseColumnsButtonVisible && canExportToExcel;
		}

		public override void Dispose()
		{
			GridLayoutControl.UnhookTextChangedEvent(new EventHandler(OnGridLayoutChanged));

			if (fModule != null)
			{
				fModule.Dispose();
				fModule = null;
			}

			base.Dispose();
		}

		#endregion

		#region Setup routines 

		protected void SetupFilterControl()
		{
			LoadFilterControl(Module);

			NewButton.Visible = NewButtonVisible;
			if (NewButton.Visible)
			{
				NewButton.Text = NewButtonText;
				NewButton.ToolTip = NewButtonToolTip;
			}

			ViewButton.Visible = ViewButtonVisible;
			if (ViewButton.Visible)
			{
				ViewButton.Text = ViewButtonText;
				ViewButton.ToolTip = ViewButtonToolTip;
			}

			ExportToExcelButton.Visible = ExportToExcelButtonVisible;
			GridLayoutControl.Visible = CustomiseColumnsButtonVisible;
		}

		public void HideFilterHeader()
		{
			FilterControl.Visible = false;
			NewButton.Visible = false;
			ViewButton.Visible = false;
			ExportToExcelButton.Visible = false;
			GridLayoutControl.Visible = false;
			FindButton.Visible = false;
			ClearButton.Visible = false;
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
				Control control = Page.LoadControl(module.FilterControlResource.FileName);
				FilterControl.Controls.AddAt(0, control);

				InitialiseFilterStripControl(control as ZFilterStripControl, module as ZFilterStripGridModule);
			}
		}

		void InitialiseFilterStripControl(ZFilterStripControl filterStripControl, ZFilterStripGridModule module)
		{
			if (filterStripControl != null) // remove these checks once old filter controls are gone
			{
				filterStripControl.OnFind += new EventHandler(FilterStripControl_OnFind);
				FindButton.Visible = false;  //
				ClearButton.Visible = false; // these buttons will be deleted

				SetupAjaxControls();

				if (module != null)
				{
					filterStripControl.DefaultLayoutName = module.DefaultLayoutName;
				}
			}
		}

		internal void InitialiseFilterStripControlInternal(ZFilterStripControl filterStripControl, ZFilterStripGridModule module) => InitialiseFilterStripControl(filterStripControl, module);

		void SetupAjaxControls()
		{
			//UpdatePanel gridUpdatePanel = new UpdatePanel();
			//gridUpdatePanel.ID = "GridUpdatePanel";
			//gridUpdatePanel.ContentTemplateContainer.Controls.Add(SearchResultsDataGrid);
			//Controls.Add(gridUpdatePanel);
		}

		public ZFilterStripControl FilterStripControl
		{
			get
			{
				return FilterControl != null && FilterControl.Controls.Count > 0 ? FilterControl.Controls[0] as ZFilterStripControl : null;
			}
		}

		void FilterStripControl_OnFind(object sender, EventArgs e)
		{
			Find();
		}

		#region Setup Grid

		protected ModuleGridLayoutHelper ModuleGridLayoutHelper
		{
			get
			{
				if (moduleGridLayoutHelper == null)
				{
					moduleGridLayoutHelper = new ModuleGridLayoutHelper(this);
				}
				return moduleGridLayoutHelper;
			}
		}
		ModuleGridLayoutHelper moduleGridLayoutHelper;

		protected void StoreGridLayout()
		{
			ModuleGridLayoutHelper.CurrentGridLayout = GridLayoutControl.Text;
		}

		protected internal void SetupGrid()
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
			SearchResultsDataGrid.CssClass = ZCssHelper.Join(SearchResultsDataGrid.CssClass, TableCss);
			SearchResultsDataGrid.HeaderStyle.CssClass = ZCssHelper.Join(SearchResultsDataGrid.HeaderStyle.CssClass, HeaderCss);
			SearchResultsDataGrid.ItemStyle.CssClass = ZCssHelper.Join(SearchResultsDataGrid.ItemStyle.CssClass, ItemCss);
			SearchResultsDataGrid.AlternatingItemStyle.CssClass = ZCssHelper.Join(SearchResultsDataGrid.AlternatingItemStyle.CssClass, AlternatingCss);
			SearchResultsDataGrid.SelectedItemStyle.CssClass = ZCssHelper.Join(SearchResultsDataGrid.SelectedItemStyle.CssClass, SelectedCss);

			SetupGridColumns();

			SearchResultsDataGrid.PagerStyle.Mode = PagerMode.NumericPages;
			SearchResultsDataGrid.PagerStyle.CssClass = ZCssHelper.Join(SearchResultsDataGrid.CssClass, PagerCss);
			SearchResultsDataGrid.AllowPaging = true;
			SearchResultsDataGrid.PageSize = PageSize;

			SearchResultsDataGrid.SortCommand += new DataGridSortCommandEventHandler(SearchResultsDataGrid_SortCommand);
			SearchResultsDataGrid.AllowSorting = Module.AllowSort;
			SearchResultsDataGrid.PageIndexChanged += new DataGridPageChangedEventHandler(SearchResultsDataGrid_PageIndexChanged);
		}

		protected internal void SetupGridColumns()
		{
			List<DataGridColumn> columnsToAdd = ModuleGridLayoutHelper.GetGridColumns();

			if (SearchResultsDataGrid.Columns.Count > 0)
			{
				SearchResultsDataGrid.Columns.Clear();
			}

			List<DataGridColumn> newRowColumns = new List<DataGridColumn>();
			foreach (DataGridColumn columnToAdd in columnsToAdd)
			{
				if (columnToAdd is ZNewRowColumn)
				{
					newRowColumns.Add(columnToAdd);
				}
				else
				{
					columnToAdd.HeaderStyle.CssClass = ZCssHelper.Join(columnToAdd.HeaderStyle.CssClass, HeaderCss);
					SearchResultsDataGrid.Columns.Add(columnToAdd);
				}
			}
			foreach (DataGridColumn newRowColumn in newRowColumns)
			{
				SearchResultsDataGrid.Columns.Add(newRowColumn);
			}
		}

		#endregion

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

		void SetupGridLayoutControl()
		{
			GridLayoutControl.ModuleID = ModuleID;
			GridLayoutControl.Text = ModuleGridLayoutHelper.CurrentGridLayout;
			GridLayoutControl.HookTextChangedEvent(new EventHandler(OnGridLayoutChanged));
		}

		protected internal string TableCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsTable); }
		}

		protected internal string ItemCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsCell); }
		}

		protected internal string AlternatingCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsAlternatingCell); }
		}

		protected string SelectedCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsSelectedCell); }
		}

		protected internal string PagerCss
		{
			get { return GetBasePageModuleClass(CssConstants.ResultsTablePager); }
		}

		protected internal string HeaderCss
		{
			get { return GetBasePageModuleClass(CssConstants.DetailsHeader); }
		}

		public ScrollingStyle ScrollingStyle
		{
			get { return fScrollingStyle; }
			set { fScrollingStyle = value; }
		}
		ScrollingStyle fScrollingStyle = ScrollingStyle.None;

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

		#region ClientIDs of controls 

		public string ResultsGridDivClientID
		{
			get { return ResultsGridDiv.ClientID; }
		}

		#endregion

		#region CssClass Properties

		public ZString CssClassPrefix
		{
			get { return fCssClassPrefix; }
			set { fCssClassPrefix = value.Trim(); }
		}
		ZString fCssClassPrefix;

		#endregion

		#region ModuleID

		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(WebModuleIDEditor), typeof(UITypeEditor))]
		public virtual WebModuleID ModuleID
		{
			get { return fModuleID; }
			set { fModuleID = value; }
		}
		protected WebModuleID fModuleID = WebModuleIDs.NotAssigned;

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
				if (fPageSize == 0)
				{
					fPageSize = WebDataRegistry.Instance.PageSize.Value;
				}

				return fPageSize;
			}
			set { fPageSize = value; }
		}
		protected int fPageSize;

		#endregion

		#region MaxRows

		[Category(ZGUIConstants.DesignerCategory)]
		public int MaxRows
		{
			get { return Module.MaxRows; }
			set { Module.MaxRows = value; }
		}

		#endregion

		#region New Button

		[Category(ZGUIConstants.DesignerCategory)]
		public bool NewButtonVisible
		{
			get { return fNewButtonVisible; }
			set { fNewButtonVisible = value; }
		}
		bool fNewButtonVisible;

		[Category(ZGUIConstants.DesignerCategory)]
		public string NewButtonText
		{
			get { return fNewButtonText; }
			set { fNewButtonText = value; }
		}
		string fNewButtonText = Labels.New;

		[Category(ZGUIConstants.DesignerCategory)]
		public string NewButtonUrl
		{
			get { return fNewButtonUrl; }
			set { fNewButtonUrl = value; }
		}
		string fNewButtonUrl;

		[Category(ZGUIConstants.DesignerCategory)]
		public string NewButtonToolTip
		{
			get
			{
				return !string.IsNullOrEmpty(fNewButtonToolTip) ? fNewButtonText : NewButtonText;
			}
			set { fNewButtonToolTip = value; }
		}
		string fNewButtonToolTip;

		[Category(ZGUIConstants.DesignerCategory)]
		public string NewButtonClientClickScript
		{
			get { return fNewButtonClientClickScript; }
			set { fNewButtonClientClickScript = value; }
		}
		string fNewButtonClientClickScript;

		#endregion New Button

		#region Custom Button1

		[Category(ZGUIConstants.DesignerCategory)]
		public bool CustomButton1Visible
		{
			get { return fCustomButton1Visible; }
			set { fCustomButton1Visible = value && (!string.IsNullOrEmpty(CustomButton1Text)); }
		}
		bool fCustomButton1Visible;

		[Category(ZGUIConstants.DesignerCategory)]
		public string CustomButton1Text
		{
			get { return fCustomButton1Text; }
			set { fCustomButton1Text = value; }
		}
		string fCustomButton1Text = "";

		[Category(ZGUIConstants.DesignerCategory)]
		public string CustomButton1Url
		{
			get { return fCustomButton1Url; }
			set { fCustomButton1Url = value; }
		}
		string fCustomButton1Url;

		[Category(ZGUIConstants.DesignerCategory)]
		public string CustomButton1ToolTip
		{
			get
			{
				return !string.IsNullOrEmpty(fCustomButton1ToolTip) ? fCustomButton1ToolTip : CustomButton1Text;
			}
			set { fCustomButton1ToolTip = value; }
		}
		string fCustomButton1ToolTip;

		[Category(ZGUIConstants.DesignerCategory)]
		public string CustomButton1ClientClickScript
		{
			get { return fCustomButton1ClientClickScript; }
			set { fCustomButton1ClientClickScript = value; }
		}
		string fCustomButton1ClientClickScript;

		#endregion View Button

		#region View Button

		[Category(ZGUIConstants.DesignerCategory)]
		public bool ViewButtonVisible
		{
			get { return fViewButtonVisible; }
			set { fViewButtonVisible = value; }
		}
		bool fViewButtonVisible;

		[Category(ZGUIConstants.DesignerCategory)]
		public string ViewButtonText
		{
			get { return fViewButtonText; }
			set { fViewButtonText = value; }
		}
		string fViewButtonText = Labels.View;

		[Category(ZGUIConstants.DesignerCategory)]
		public string ViewButtonUrl
		{
			get { return fViewButtonUrl; }
			set { fViewButtonUrl = value; }
		}
		string fViewButtonUrl;

		[Category(ZGUIConstants.DesignerCategory)]
		public string ViewButtonToolTip
		{
			get
			{
				return !string.IsNullOrEmpty(fViewButtonToolTip) ? fViewButtonText : ViewButtonText;
			}
			set { fViewButtonToolTip = value; }
		}
		string fViewButtonToolTip;

		[Category(ZGUIConstants.DesignerCategory)]
		public string ViewButtonClientClickScript
		{
			get { return fViewButtonClientClickScript; }
			set { fViewButtonClientClickScript = value; }
		}
		string fViewButtonClientClickScript;

		#endregion View Button

		#region ExportToExcel button

		[Category(ZGUIConstants.DesignerCategory)]
		public bool ExportToExcelButtonVisible
		{
			get { return (bool)(ViewState["ExportButtonVisible"] ?? true); }
			set { ViewState["ExportButtonVisible"] = value; }
		}

		public delegate void VisibilityDelegate(bool visible);

		public event VisibilityDelegate ExportToExcelButtonVisibilityPublisher;

		#endregion

		#region CustomiseColumns button

		[Category(ZGUIConstants.DesignerCategory)]
		public bool CustomiseColumnsButtonVisible
		{
			get
			{
				object result = ViewState["CustomiseColumnsButtonVisible"];
				return (result == null) ? Session != null && WebEnv.CurrentUser != null : (bool)result;
			}
			set { ViewState["CustomiseColumnsButtonVisible"] = value; }
		}

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
				if (fModule == null)
				{
					try
					{
						fModule = ZWebModuleFactory.Create(ModuleID, Factory, Page);
					}
					catch (ModuleIDIsNullException ex)
					{
						throw new ZException(ex.Message);
					}

					if (fModule == null)
					{
						throw new ZException("Module ID : " + ModuleID + " resolves to null. Web FindBox Modules must be ZGenericFilterGridModule.");
					}
				}
				return fModule;
			}
		}
#if DEBUG
		protected
#endif
			ZFilterGridModule fModule;

		#endregion Module

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
				if (Module is ZFilterStripGridModule)
				{
					((ZFilterStripGridModule)Module).OverrideFilterStripBizO(value);
				}
				if (FilterStripControl != null)
				{
					FilterStripControl.FilterStripBizO = value;
				}
			}
		}
		FilterStripBusinessObject filterStripBizO;

		protected string SortExpression = "";
		protected ListSortDirection SortDirection = ListSortDirection.Ascending;

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

		internal void LoadResults(FilterBusinessObject filterBizO)
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
				object obj = ViewState[HasLoadedResultsKey];
				if (obj != null)
				{
					result = (bool)obj;
				}
				return result;
			}
			set { ViewState[HasLoadedResultsKey] = value; }
		}
		const string HasLoadedResultsKey = "SearchControlHasLoadedResults";

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
			ZGuid[] pKs = LoadCachedPKs();
			if (pKs != null && pKs.Length > 0)
			{
				SchemaColumn pKSchemaColumn = Module.GetBusinessObjectPKColumn(FilterBusinessObject);
				ZQuery pKFilter = new ZQuery(pKSchemaColumn, pKs);
				pKFilter.OrderBy = string.Join(", ", Module.GetSortInfos(FilterBusinessObject).Select(s => s.OrderByColumnName));
				Module.LoadCollection(FilterBusinessObject, pKFilter);
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
			SearchResultsDataGrid.Bind(Module.GridCollection);
		}

		#endregion

		#region GetSelectionPK

		public ZGuid GetSelectionPK()
		{
			return GetSelectionPKCore();
		}

		protected virtual ZGuid GetSelectionPKCore()
		{
			ZGuid result = ZGuid.Empty;
			if (SearchResultsDataGrid != null && SearchResultsDataGrid.SelectedIndex != -1 && Module.GridCollection != null)
			{
				if (!Module.GridCollection.IsLoaded)
				{
					LoadResults(FilterBusinessObject);
					SortResults(Module.GridCollection);
				}

				int bizOIndex = SearchResultsDataGrid.SelectedIndex + (SearchResultsDataGrid.PageSize * SearchResultsDataGrid.CurrentPageIndex);
				if (bizOIndex >= 0 && bizOIndex < Module.GridCollection.Count)
				{
					BusinessObject bizO = ((IList)Module.GridCollection)[bizOIndex] as BusinessObject;
					result = bizO != null ? bizO.PK : ZGuid.Empty;
				}
			}
			return result;
		}

		#endregion

		#region Automatically generated

		protected PlaceHolder Buttons;
		protected System.Web.UI.HtmlControls.HtmlGenericControl ResultsGridDiv;
		protected System.Web.UI.HtmlControls.HtmlGenericControl FilterControl;
		protected internal ZGridLayoutControl GridLayoutControl;
		public ZDataGrid SearchResultsDataGrid;

		#endregion

		#region Event handlers 

		public void FindButton_Click(object sender, EventArgs e)
		{
			Find();
		}

		public const string SearchControlIsSearchingIndexer = "searchControlIsSearchingIndexer";

		protected virtual void Find()
		{
			Session[SearchControlIsSearchingIndexer] = true;

			FilterBusinessObject.Filter.ReLoadExistingRows = true;
			SearchResultsDataGrid.ClearMultiLineSelection();
			SearchResultsDataGrid.SetCurrentPageIndex(0);
			LoadAndPopulateGrid();
			StoreFilterCriteriaIfApplicable();

			if (OnSearch != null)
			{
				OnSearch(this, EventArgs.Empty);
			}

			Session.Remove(SearchControlIsSearchingIndexer);
		}

		public event EventHandler OnSearch;

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

		internal void ClearButton_Click(object sender, EventArgs e)
		{
			Module.ResetFilterBusinessObject(FilterBusinessObject);
			ClearCollection(Module.GridCollection);
			SearchResultsDataGrid.ClearMultiLineSelection();
			ClearCachedPKs();
			Page.Bind();
			if (HasLoadedResults)
			{
				BindResultsGrid();
			}
			HasLoadedResults = false;
			if (OnClear != null)
			{
				OnClear(this, e);
			}
		}
		public event EventHandler OnClear;

		void SearchResultsDataGrid_SortCommand(object source, DataGridSortCommandEventArgs e)
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
			RePopulateGrid();
		}

		protected void SearchResultsDataGrid_PageIndexChanged(object source, DataGridPageChangedEventArgs e)
		{
			RePopulateGrid();
			ZDataGrid dataGrid = source as ZDataGrid;
			if (dataGrid != null)
			{
				dataGrid.SetCurrentPageIndex(e.NewPageIndex);
				PopulateGrid();
			}
		}

		internal void NewButton_Click(object sender, EventArgs e)
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

		internal void ViewButton_Click(object sender, EventArgs e)
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

		internal void ExportToExcelButton_Click(object sender, EventArgs e)
		{
			if (SearchResultsDataGrid != null)
			{
				IBusinessObjectCollection collectionToExport = Module.LoadExcelCollection(FilterBusinessObject);
				SortResults(collectionToExport);
				SearchResultsDataGrid.ExportIntoExcel(collectionToExport);
			}
		}

		internal void OnGridLayoutChanged(object sender, EventArgs e)
		{
			StoreGridLayout();
			if (SearchResultsDataGrid != null)
			{
				SetupGrid();
				RePopulateGrid();
			}
		}

		#endregion

		#region ViewState

		protected override void LoadViewState(object savedState)
		{
			object[] viewState = (object[])savedState;
			base.LoadViewState(viewState[0]);
			SortExpression = (string)viewState[1];
			SortDirection = (ListSortDirection)viewState[2];
		}

		protected override object SaveViewState()
		{
			object[] viewState = new object[5];
			viewState[0] = base.SaveViewState();
			viewState[1] = SortExpression;
			viewState[2] = SortDirection;
			return (viewState);
		}

		#region CachedPKs

		protected virtual void SaveCachedPKs(ZGuid[] array)
		{
			ViewState[CollectionPKIndexer] = array;
		}

		protected virtual ZGuid[] LoadCachedPKs()
		{
			return ViewState[CollectionPKIndexer] as ZGuid[];
		}

		void ClearCachedPKs()
		{
			ViewState[CollectionPKIndexer] = null;
		}
		const string CollectionPKIndexer = "ModuleGridCollectionCachedPKs";
		#endregion

		#endregion

		#region Test Properties

		public void FindForTesting() => Find();

		#endregion

		public static class Labels
		{
			public static string View { get { return Res.GetString("12e6a2f8-9b7a-41d2-ae01-29052461a315", "View"); } }
			public static string Clear { get { return Res.GetString("152ad809-be54-4845-87d8-4ad1eed1c291", "Clear"); } }
			public static string ClearToolTip { get { return Res.GetString("c12cf4ac-30a6-4212-96e1-37bd01b9a4d6", "Clear search criteria"); } }
			public static string Find { get { return Res.GetString("fc352263-808e-4ce2-ab6b-d4c86498c482", "Find"); } }
			public static string FindToolTip { get { return Res.GetString("3940d106-7677-44f1-b9ea-49016517d89e", "Find records matching search criteria"); } }
			public static string New { get { return Res.GetString("2ea28c7b-818e-45a4-a555-9860a2df0769", "New"); } }
			public static string ViewSelected { get { return Res.GetString("71713975-c403-4b6f-a9b9-a9d60a0273cf", "View Selected"); } }
			public static string ExportToExcel { get { return Res.GetString("ce915337-bb64-45f9-af8d-ba5e32c50139", "Export to Excel"); } }
		}
	}
}
