using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region Delegates / EventArgs

	public delegate void CommandErrorHandler(string operation, Exception ex);

	public delegate void DataGridDeleteHandler(DataGridDeleteEventArgs e);

	public class DataGridDeleteEventArgs : EventArgs
	{
		public DataGridDeleteEventArgs(object source, DataGridItem item)
		{
			this.source = source;
			this.Item = item;
			fCanDelete = true;
		}
		public readonly object source;
		public readonly DataGridItem Item;

		public bool CanDelete
		{
			get { return fCanDelete; }
			set { fCanDelete = value; }
		}
		bool fCanDelete;
	}
	#endregion

	[DefaultProperty("Text"),
	ToolboxData("<{0}:ZDataGrid runat=server></{0}:ZDataGrid>")]
	public class ZDataGrid : DataGrid, ISelfBindingWebControl, IExternallyFiredPostBackHandler, INotificationProvider
	{
		#region Constructor

		public ZDataGrid()
		{
			AutoSizeColumns = false;
			this.PagerStyle.Mode = PagerMode.NumericPages;
			this.PagerStyle.PageButtonCount = 20;

			this.AutoGenerateColumns = false;
			this.EnableViewState = true;
			this.ButtonType = ButtonColumnType.LinkButton;
		}

		#endregion

		#region Binding

		#region Collection

		protected internal IBusinessObjectCollection Collection
		{
			get { return DataSource as IBusinessObjectCollection; }
		}

		#endregion

		#region Bind

		void AddInitialRows()
		{
			if (Collection != null)
			{
				if (Collection.Count == 0)
				{
					for (int i = 0; i < InitialRowsToDisplay; i++)
					{
						AddNewRow();
					}
				}
			}
		}

		public void SetCurrentPageIndex(int pageIndex)
		{
			CurrentPageIndex = pageIndex;
			CachedCurrentPage = CurrentPageIndex + 1;
		}

		/// <summary>
		/// Bind the object as a data source
		/// </summary>
		/// <param name="dataSource">The object to bind</param>
		public void Bind(object dataSource)
		{
			BusinessEntity = dataSource;
			DataKeyField = "PK";
			if (IsBindable(dataSource))
			{
				if (!string.IsNullOrEmpty(BindTo))
				{
					DataSource = ZPropertyAccessor.Get(dataSource, BindTo);
				}
				else if (dataSource is ICollection)
				{
					DataSource = dataSource;
				}
				CurrentPageIndex = CachedCurrentPage - 1;
				if ((CurrentPageIndex > 0) && ((CurrentPageIndex * PageSize) >= TotalRecordCount))
				{
					SetCurrentPageIndex(0);
				}

				ApplySort(DataSource);
				AddFetchHints(DataSource);

				BindCore();
			}
			else
			{
				DataSource = null;
			}
		}

		public bool IsBound;

		public void BindCore()
		{
			try
			{
				DataBind();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				try
				{
					SetCurrentPageIndex(0);
					DataBind();
				}
				catch (Exception ex) when (!ex.IsCriticalException()) { }
			}
		}

		object BusinessEntity;

		#endregion

		#region OnDataBinding

		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			if (IsBindable(DataSource))
			{
				AddInitialRows();
				if (DisplayAdditionalNewRow)
				{
					bool addAdditionalRow = false;
					if (Collection.Count > 0)
					{
						addAdditionalRow = true;
						BusinessObject bO = (BusinessObject)Collection[Collection.Count - 1];
						if (BOAnalyzer.IsNewAndDefault(bO))
						{
							addAdditionalRow = false;
						}
					}
					if (addAdditionalRow)
					{
						AddNewRow();
					}
				}
				IsBound = true;
				if (AllowPaging && (Collection != null))
				{
					if (TotalRecordCount == 0)
					{
						StatusLabel.Text = Res.GetString("8f76304f-2687-4ecd-943f-d046a5eac49b", "No records found.");
					}
					else
					{
						if (MaxRows > -1 && TotalRecordCount == MaxRows)
						{
							StatusLabel.Text = Res.GetString("4da6042b-d40b-42d5-a93d-6c4b2ffa3346", "Search returned {0} or more records. Please review your search criteria.", MaxRows);
						}
						else
						{
							StatusLabel.Text = StatusLabel.Text = Res.GetString("8eff11fc-e2f4-455f-9076-7990ca0dc24d", "Found {0} record(s).", TotalRecordCount);
						}
					}
				}
				if (fNotifications == null)
				{
					fNotifications = GetNotifications(Collection);
				}
			}
		}

		public WebBusinessObjectAnalyzer BOAnalyzer
		{
			get
			{
				if (bOAnalyzer == null)
				{
					bOAnalyzer = GetNewBOAnalyzer();
				}
				return bOAnalyzer;
			}
			set
			{
				bOAnalyzer = value;
			}
		}

		protected virtual WebBusinessObjectAnalyzer GetNewBOAnalyzer()
		{
			return new WebBusinessObjectAnalyzer();
		}

		WebBusinessObjectAnalyzer bOAnalyzer;

		static IEnumerable<INotification> GetNotifications(IBusinessObjectCollection collection)
		{
			foreach (BusinessObject child in collection)
			{
				foreach (INotification notification in new ZNotificationCollector(child, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName))
				{
					yield return notification;
				}
			}
		}

		#endregion

		#region MaxRows

		public int MaxRows
		{
			get
			{
				object storedValue = ViewState["MaxRows"];
				return (storedValue == null) ? -1 : (int)storedValue;
			}
			set { ViewState["MaxRows"] = value; }
		}

		#endregion

		#region TotalRecordCount

		protected virtual int TotalRecordCount
		{
			get { return Collection.Count; }
		}
		#endregion

		#region BindTo

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindToList")]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		#endregion

		#region PrepareControlHierarchy

		protected override void PrepareControlHierarchy()
		{
			if (IsBound && Controls.Count == 0)
			{
				StatusLabel.Text = Res.GetString("54a8dfcb-0122-4e1a-ba64-b24e3c047c63", "No Data Found");
				StatusPanel.CssClass = ZCssHelper.Join(StatusPanel.CssClass, CssConstants.DetailsTable);
				Controls.Add(StatusPanel);
				this.Caption = "";
			}
			else
			{
				base.PrepareControlHierarchy();
			}
		}
		#endregion

		#endregion Binding

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return (dataSource is IBusinessObjectCollection) || (!string.IsNullOrEmpty(BindTo) && dataSource != null);
		}

		public void UnBind()
		{
			this.DataSource = null;
			this.DataBind();
		}

		#endregion

		#region Control Overrides

		#region CreateChildControls

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			ShowFooter = AllowAdd;
			RaiseAfterCreateChildControls();
		}

		public event EventHandler AfterCreateChildControls;

		protected virtual void RaiseAfterCreateChildControls()
		{
			if (AfterCreateChildControls != null)
			{
				AfterCreateChildControls(this, new EventArgs());
			}
		}

		#endregion

		#region CreateColumnSet

		public override string Caption
		{
			get
			{
				return base.Caption;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(this.OriginalCaption))
				{
					this.OriginalCaption = value;
				}
				base.Caption = value;
			}
		}

		public override string CssClass
		{
			get
			{
				return base.CssClass;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(this.OriginalCssClass))
				{
					this.OriginalCssClass = value;
				}
				base.CssClass = value;
			}
		}

		protected string CssClassForEmptyGrid
		{
			get
			{
				return (this.OriginalCssClass != null ? this.OriginalCssClass + "_Empty" : this.CssClass);
			}
		}
		protected int ColumnIndexToInsert;
		protected override ArrayList CreateColumnSet(PagedDataSource dataSource, bool useDataSource)
		{
			ArrayList result = new ArrayList();
			ColumnIndexToInsert = 0;

			this.ShowHeader = true;
			this.Caption = this.OriginalCaption;
			this.CssClass = this.OriginalCssClass;
			if (!ReadOnly && AllowAdd && Collection.Count == 0 && AllowAdd && AllowEdit)
			{
				this.ShowHeader = false;
				this.BorderWidth = 0;
				this.GridLines = GridLines.None;
				result.Clear();
				result.Insert(ColumnIndexToInsert++, new ZEnterDetailsColumn(ButtonColumnType.LinkButton, EnterDetailsCommandName, this.Caption));
				this.Caption = "";
				this.CssClass = CssClassForEmptyGrid;
			}
			else
			{
				result = CreateColumnsCore(dataSource, useDataSource);
			}

			return result;
		}

		protected ArrayList CreateColumnsCore(PagedDataSource dataSource, bool useDataSource)
		{
			ArrayList result = new ArrayList();
			Columns.Remove(AddDeleteColumn);
			Columns.Remove(PanelEditColumn);
			Columns.Remove(SelectionColumn);

			if (!ReadOnly && (AllowDelete || AllowAdd))
			{
				Columns.AddAt(ColumnIndexToInsert++, AddDeleteColumn);
			}
			else if (ExtendedEditMode)
			{
				PanelEditColumn.Text = !ReadOnly ? EditLabelText : ViewLabelText;
				Columns.AddAt(ColumnIndexToInsert++, PanelEditColumn);
			}
			if (AllowMultiLineSelection)
			{
				Columns.AddAt(ColumnIndexToInsert, SelectionColumn);
			}
			return result = base.CreateColumnSet(dataSource, useDataSource);
		}

		string OriginalCaption;
		string OriginalCssClass;

		#endregion

		#region DataGrid Initialization

		#region InitializePager

		protected override void InitializePager(DataGridItem item, int columnSpan, PagedDataSource pagedDataSource)
		{
			base.InitializePager(item, columnSpan, pagedDataSource);
			if (item.Cells[0].FindControl(PagesLabel.ID) == null)
			{
				item.Cells[0].Controls.AddAt(0, PagesLabel);
			}
			if (item.Cells[0].FindControl(StatusPanel.ID) == null)
			{
				item.Cells[0].Controls.Add(StatusPanel);
			}
			if (this.FindControl(CachedCurrentPageControl.ID) == null)
			{
				this.Controls.Add(CachedCurrentPageControl);
			}
		}

		#endregion

		#region InitializeItem

		protected override void InitializeItem(DataGridItem item, DataGridColumn[] columns)
		{
			base.InitializeItem(item, columns);
			if ((item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem) &&
				AllowMultiLineSelection && IsRowSelected(DataKeys[item.ItemIndex]))
			{
				item.ApplyStyle(SelectedItemStyle);

				ZSelectionCheckBox checkBox = item.Cells[0].Controls[0] as ZSelectionCheckBox;
				if (checkBox != null)
				{
					checkBox.Checked = true;
				}
			}
			else if (item.ItemType == ListItemType.Footer)
			{
				if (!AllowAdd && !AllowPaging)
				{
					if (item.Cells[0].FindControl(StatusPanel.ID) == null)
					{
						if (Collection == null || TotalRecordCount == 0)
						{
							StatusLabel.Text = Res.GetString("cdfb4c05-6da5-4019-9ff0-5c4798740980", "No records found");
							item.Cells.Clear();
							TableCell cell = new TableCell();
							cell.Controls.Add(StatusPanel);
							cell.ColumnSpan = columns.Length;
							item.Cells.Add(cell);
							this.ShowFooter = true;
						}
					}
				}
			}
		}
		#endregion

		#region CreateItem

		protected override DataGridItem CreateItem(int itemIndex, int dataSourceIndex, ListItemType itemType)
		{
			return new ZDataGridItem(itemIndex, dataSourceIndex, itemType);
		}
		#endregion

		#endregion

		#endregion Control Overrides

		#region Properties

		public bool IncludeItemDataRefKey;

		#region AutoSizeColumns

		[Category("Appearance"), DefaultValue(false)]
		public bool AutoSizeColumns { get; set; }

		#endregion

		#region StatusLabel

		protected internal ZTextLabel StatusLabel
		{
			get
			{
				if (fStatusLabel == null)
				{
					fStatusLabel = new ZTextLabel();
				}
				return fStatusLabel;
			}
		}
		ZTextLabel fStatusLabel;
		#endregion

		#region StatusPanel

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html element id should not be translated")]
		protected internal Panel StatusPanel
		{
			get
			{
				if (fStatusPanel == null)
				{
					fStatusPanel = new Panel();
					fStatusPanel.ID = "Status";
					fStatusPanel.Controls.Add(StatusLabel);
				}
				return fStatusPanel;
			}
		}
		Panel fStatusPanel;
		#endregion

		#region CurrentPage

		protected internal int CachedCurrentPage
		{
			get
			{
				var result = 1;
				if (CachedCurrentPageControl == null || string.IsNullOrEmpty(CachedCurrentPageControl.Value.Trim()))
				{
					if (cachedParamsCurrentPage == null)
					{
						HttpRequest request = null;
						try
						{
							request = Page.Request;
						}
						catch (Exception e) when (!e.IsCriticalException()) { }
						if (request != null && request.Params != null)
						{
							if (!int.TryParse(request.Params[string.Format("{0}${1}", UniqueID, CachedCurrentPageControl.ID)], out result))
							{
								if (!int.TryParse(request.Params[CachedCurrentPageControl.UniqueID], out result))
								{
									result = 1;
								}
							}
						}
						cachedParamsCurrentPage = result;
					}
					else
					{
						result = cachedParamsCurrentPage.Value;
					}
				}
				else
				{
					if (!int.TryParse(CachedCurrentPageControl.Value, out result))
					{
						result = 1;
					}
				}
				return result;
			}
			set
			{
				CachedCurrentPageControl.Value = value.ToString();
			}
		}

		int? cachedParamsCurrentPage;

		protected HiddenField CachedCurrentPageControl
		{
			get
			{
				if (cachedCurrentPageControl == null)
				{
					cachedCurrentPageControl = new HiddenField();
					cachedCurrentPageControl.ID = "CurrentPage";
				}
				return cachedCurrentPageControl;
			}
		}

		HiddenField cachedCurrentPageControl;

		#endregion

		#region PagesLabel

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html element id should not be translated")]
		protected ZTextLabel PagesLabel
		{
			get
			{
				if (fPagesLabel == null)
				{
					fPagesLabel = new ZTextLabel();
					fPagesLabel.ID = "Pages";
					fPagesLabel.Text = " " + Res.GetString("b9cf7191-1d2d-4f65-a918-21c4e69bdd62", "Pages:") + " ";
				}
				return fPagesLabel;
			}
		}
		protected ZTextLabel fPagesLabel;
		#endregion

		#region PanelEditColumn

		protected ButtonColumn PanelEditColumn
		{
			get
			{
				if (fPanelEditColumn == null)
				{
					fPanelEditColumn = new ButtonColumn();
					fPanelEditColumn.ButtonType = ButtonType;
					fPanelEditColumn.CommandName = EditPanelCommand;
				}
				return fPanelEditColumn;
			}
		}
		ButtonColumn fPanelEditColumn;
		#endregion

		#region SelectionColumn

		protected ZSelectionColumn SelectionColumn
		{
			get
			{
				if (fSelectionColumn == null)
				{
					fSelectionColumn = new ZSelectionColumn(this);
				}
				return fSelectionColumn;
			}
		}
		ZSelectionColumn fSelectionColumn;
		#endregion

		#region AddDeleteColumn

		protected ZAddDeleteColumn AddDeleteColumn
		{
			get
			{
				if (fAddDeleteColumn == null)
				{
					fAddDeleteColumn = new ZAddDeleteColumn(ButtonType);
				}
				return fAddDeleteColumn;
			}
		}
		ZAddDeleteColumn fAddDeleteColumn;
		#endregion

		#region ExtendedEditMode

		public bool ExtendedEditMode
		{
			get
			{
				object result = ViewState["ExtendedEditMode"];
				return (result != null) && (bool)result;
			}
			set
			{
				ViewState["ExtendedEditMode"] = value;
			}
		}
		#endregion

		#region EditLabelText

		public string EditLabelText
		{
			get
			{
				object result = ViewState["EditLabelText"];
				return (result != null) ? (string)result : Res.GetString("b3856315-6c95-43bd-a866-cad26876e714", "Edit");
			}
			set
			{
				ViewState["EditLabelText"] = value;
			}
		}
		#endregion

		#region ViewLabelText

		public string ViewLabelText
		{
			get
			{
				object result = ViewState["ViewLabelText"];
				return (result != null) ? (string)result : Res.GetString("2b99a041-98a7-49d0-8019-6484d3985004", "View");
			}
			set
			{
				ViewState["ViewLabelText"] = value;
			}
		}
		#endregion

		#region AllowEdit

		[DefaultValue(true), Category("Appearance"), Description("")]
		public bool AllowEdit
		{
			get { return fAllowEdit; }
			set { fAllowEdit = value; }
		}
		bool fAllowEdit;
		#endregion

		#region AllowDelete

		[DefaultValue(true), Category("Appearance"), Description("Show the Delete button")]
		public bool AllowDelete
		{
			get { return fAllowDelete; }
			set { fAllowDelete = value; }
		}
		bool fAllowDelete;
		#endregion

		#region ReadOnly

		[DefaultValue(false), Category("Appearance"), Description("DataGrid is ReadOnly")]
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set { fReadOnly = value; }
		}
		bool fReadOnly;
		#endregion

		#region DisplayAdditionalNewRow

		[DefaultValue(false), Category("Appearance"), Description("")]
		public bool DisplayAdditionalNewRow
		{
			get { return fDisplayAdditionalNewRow; }
			set { fDisplayAdditionalNewRow = value; }
		}
		bool fDisplayAdditionalNewRow;

		#endregion

		#region InitialRowsToDisplay

		[DefaultValue(0), Category("Appearance"), Description("Show number of empty rows to display for empty grid in editing mode")]
		public int InitialRowsToDisplay
		{
			get { return fInitialRowsToDisplay; }
			set { fInitialRowsToDisplay = value; }
		}
		int fInitialRowsToDisplay;

		#endregion

		#region AllowAdd

		[DefaultValue(true), Category("Appearance"), Description("Show the Add button")]
		public bool AllowAdd
		{
			get { return fAllowAdd; }
			set { fAllowAdd = value; }
		}
		bool fAllowAdd;
		#endregion

		#region EditItemIndex

		public override int EditItemIndex
		{
			get
			{
				return base.EditItemIndex;
			}
			set
			{
				if (base.EditItemIndex != value && value > -1 && value < Items.Count)
				{
					BusinessObject bizOToEdit = ((IList)Collection)[GetCollectionIndex(Items[value])] as BusinessObject;
					if (bizOToEdit != null && bizOToEdit.SupportsClone())
					{
						var nonPersistentBo = bizOToEdit as NonPersistentBusinessObject;
						if (nonPersistentBo != null)
						{
							BusinessObjectForRollback = nonPersistentBo.Clone();
						}
						else
						{
							BusinessObjectForRollback = new BusinessObjectFactory().New(bizOToEdit.GetType());
							BusinessObjectForRollback.CopyPersistentValuesFrom(bizOToEdit);
						}
					}
				}

				base.EditItemIndex = value;

				if (base.EditItemIndex == -1)
				{
					BusinessObjectForRollback = null;
				}
				ShowFooter = AllowAdd && value == -1;
			}
		}

		#endregion

		#region ButtonType

		public ButtonColumnType ButtonType
		{
			get { return fButtonType; }
			set { fButtonType = value; }
		}
		ButtonColumnType fButtonType;
		#endregion

		public const string EditPanelCommand = "EditPanel";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "command name should not be translated")]
		public const string InsertCommandName = "Insert";
		public const string EnterDetailsCommandName = "EnterDetails";

		#endregion Properties

		#region Commands handling

		#region AddNewRow

		public void AddNewRow()
		{
			DataGridCommandEventArgs e = new DataGridCommandEventArgs(null, this, new CommandEventArgs(InsertCommandName, this));
			this.OnItemCommand(e);
		}

		#endregion

		#region SaveDiscardChanges

		public void SaveChanges(object sender)
		{
			SaveDiscardChanges(true, sender);
		}

		public void DiscardChanges(object sender)
		{
			SaveDiscardChanges(false, sender);
		}

		protected void SaveDiscardChanges(bool save, object sender)
		{
			if (EditItemIndex > -1 && EditItemIndex < Items.Count)
			{
				if (save)
				{
					OnUpdateCommand(new DataGridCommandEventArgs(Items[EditItemIndex], sender, new CommandEventArgs("SaveChanges", null)));
				}
				else
				{
					OnCancelCommand(new DataGridCommandEventArgs(Items[EditItemIndex], sender, new CommandEventArgs("DiscardChanges", null)));
				}
			}
		}

		#region Rollback Changes

		protected internal BusinessObject BusinessObjectForRollback
		{
			get
			{
				if (fBusinessObjectForRollback == null && !BusinessObjectForRollbackIndexer.IsEmpty)
				{
					fBusinessObjectForRollback = LoadDataObject(BusinessObjectForRollbackIndexer) as BusinessObject;
				}
				return fBusinessObjectForRollback;
			}
			set
			{
				if (value == null)
				{
					if (BusinessObjectForRollback != null)
					{
						fBusinessObjectForRollback.Delete();
						ClearDataObject(BusinessObjectForRollbackIndexer);
						BusinessObjectForRollbackIndexer = ZGuid.Empty;
					}
				}
				fBusinessObjectForRollback = value;
				if (value != null)
				{
					BusinessObjectForRollbackIndexer = ZGuid.NewZGuid();
					SaveDataObject(BusinessObjectForRollbackIndexer, value);
				}
			}
		}
		BusinessObject fBusinessObjectForRollback;

		internal ZGuid BusinessObjectForRollbackIndexer
		{
			get
			{
				string guidString = ViewState["RollbackIndexer"] as string;
				return !string.IsNullOrEmpty(guidString) ? new ZGuid(guidString) : ZGuid.Empty;
			}
			set
			{
				ViewState["RollbackIndexer"] = value.IsValid ? value.ToString() : "";
			}
		}
		#endregion

		#region Apply Control Changes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "operation name should not be translated")]
		public void BindControlChangesToDataSource(ISelfBindingWebControl editedControl)
		{
			if (editedControl != null && (RowAdded || BusinessObjectForRollback != null))
			{
				BusinessObject boundObject = GetEditedBusinessObject();

				if (boundObject != null)
				{
					try
					{
						editedControl.Bind(boundObject);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (CommandError != null)
						{
							CommandError("Update", ex);
						}
						else
						{
							throw;
						}
					}

					BindCore();
				}
			}
		}

		BusinessObject GetEditedBusinessObject()
		{
			BusinessObject result = null;

			if (Collection != null && EditItemIndex > -1 && EditItemIndex < Collection.Count)
			{
				int indexInCollection = GetCollectionIndex(Items[EditItemIndex]);
				result = ((IList)Collection)[indexInCollection] as BusinessObject;
			}

			return result;
		}

		#endregion

		#region MultiLineSelection

		#region AllowMultiLineSelection

		[DefaultValue(false), Category("Appearance"), Description("DataGrid allows MultiLine selection")]
		public bool AllowMultiLineSelection
		{
			get { return fAllowMultiLineSelection; }
			set { fAllowMultiLineSelection = value; }
		}
		bool fAllowMultiLineSelection;

		#endregion

		public void ClearMultiLineSelection()
		{
			if (AllowMultiLineSelection)
			{
				this.SelectedKeys = "";
			}
		}

		public void SelectRow(object key, bool select)
		{
			if (AllowMultiLineSelection && key != null)
			{
				if (select && !IsRowSelected(key))
				{
					if (SelectedKeys.Length > 0)
					{
						SelectedKeys += ",";
					}
					SelectedKeys += key.ToString();
				}
				else if (!select && IsRowSelected(key))
				{
					string[] selectedKeys = SelectedKeys.Split(',');
					List<string> list = new List<string>(selectedKeys);
					list.Remove(key.ToString());
					SelectedKeys = string.Join(",", list.ToArray());
				}
			}
		}

		public ZGuid[] GetSelectedPKs()
		{
			List<ZGuid> res = new List<ZGuid>();
			string[] selectedKeys = SelectedKeys.Split(',');
			foreach (string key in selectedKeys)
			{
				if (!string.IsNullOrEmpty(key))
				{
					res.Add(new ZGuid(key));
				}
			}
			return res.ToArray();
		}

		public bool IsRowSelected(object key)
		{
			string[] selectedKeys = SelectedKeys.Split(',');
			return ((IList)selectedKeys).Contains(key.ToString());
		}

		internal string SelectedKeys
		{
			get
			{
				object result = ViewState["SelectedKeys"];
				return (result != null) ? (string)result : string.Empty;
			}
			set
			{
				ViewState["SelectedKeys"] = value;
			}
		}

#if DEBUG
		public void SetSelectedKeyForTesting(string keys) => SelectedKeys = keys;
#endif

		public ZGuid GetPKByRowIndex(int rowIndex)
		{
			return (ZGuid)DataKeys[rowIndex];
		}

		#endregion

		#region Selectable Columns

		public void SelectCellValue(object gridKey, string cellValue, bool select)
		{
			SelectCellValue(gridKey, new List<ZGuid> { new ZGuid(gridKey) }, string.Empty, cellValue, select);
		}

		public void SelectCellValue(object gridKey, List<ZGuid> relevantKeys, string humanReadableName, string cellValue, bool select)
		{
			if (gridKey != null)
			{
				var keyString = gridKey.ToString();
				var relevantKeyGuids = relevantKeys ?? new List<ZGuid>();
				if (select)
				{
					AddCellValue(keyString, relevantKeyGuids, humanReadableName, cellValue);
				}
				else
				{
					RemoveCellValue(keyString, cellValue);
				}
			}
		}

		void AddCellValue(string key, List<ZGuid> relevantKey, string humanReadableName, string cellValue)
		{
			if (!SelectedCellsValues.ContainsKey(key))
			{
				SelectedCellsValues.Add(key, new CellsValues(relevantKey, humanReadableName, new List<string> { cellValue }));
			}
			else if (!SelectedCellsValues[key].Values.Contains(cellValue))
			{
				SelectedCellsValues[key].Values.Add(cellValue);
			}
		}

		void RemoveCellValue(string key, string cellValue)
		{
			if (SelectedCellsValues.ContainsKey(key))
			{
				SelectedCellsValues[key].Values.Remove(cellValue);
				if (SelectedCellsValues[key].Values.Count == 0)
				{
					SelectedCellsValues.Remove(key);
				}
			}
		}

		public Dictionary<string, CellsValues> SelectedCellsValues = new Dictionary<string, CellsValues>();

		#endregion

		#endregion

		#region OnPageIndexChanged

		protected override void OnPageIndexChanged(DataGridPageChangedEventArgs e)
		{
			IsBound = false;
			base.OnPageIndexChanged(e);
			SetCurrentPageIndex(e.NewPageIndex);
			OnPageIndexChangedInternal();
		}

		protected virtual void OnPageIndexChangedInternal()
		{
			if (!IsBound)
			{
				BindCore();
			}
		}

		#endregion

		#region OnUpdateCommand

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "operation name should not be translated")]
		protected override void OnUpdateCommand(DataGridCommandEventArgs e)
		{
			base.OnUpdateCommand(e);

			try
			{
				if (Collection != null)
				{
					int indexInCollection = GetCollectionIndex(e);
					BusinessObject boundObject = ((IList)Collection)[indexInCollection] as BusinessObject;

					if (boundObject != null)
					{
						for (int i = 0; i < e.Item.Cells.Count; i++)
						{
							if (e.Item.Cells[i].Controls.Count > 0)
							{
								ISelfBindingPostbackWebControl ctrl = e.Item.Cells[i].Controls[0] as ISelfBindingPostbackWebControl;
								if (ctrl != null && ctrl.HasChanges)
								{
									ctrl.Bind(boundObject);
								}
							}
						}
						EditItemIndex = -1;
						RowAdded = false;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (CommandError != null)
				{
					CommandError("Update", ex);
				}
				else
				{
					throw;
				}
			}

			BindCore();
			if (AfterUpdateCommand != null)
			{
				AfterUpdateCommand(this, e);
			}
		}

		internal int GetCollectionIndex(DataGridCommandEventArgs e)
		{
			return GetCollectionIndex(e.Item);
		}

		int GetCollectionIndex(DataGridItem item)
		{
			return item.ItemIndex + (PageSize * CurrentPageIndex);
		}
		#endregion

		#region OnItemCommand

		protected override void OnItemCommand(DataGridCommandEventArgs e)
		{
			if (BeforeItemCommand != null)
			{
				BeforeItemCommand(this, e);
			}

			base.OnItemCommand(e);
			if (Collection != null)
			{
				if (e.CommandName == InsertCommandName)
				{
					Collection.AddNew();
					if (!ExtendedEditMode)
					{
						EditItemIndex = TotalRecordCount - 1;
					}
					RowAdded = true;
					BindCore();
					if (!ExtendedEditMode && AllowPaging)
					{
						if (CurrentPageIndex < PageCount - 1)
						{
							SetCurrentPageIndex(PageCount - 1);
						}
						EditItemIndex = 0;
					}
				}
				if (e.CommandName == EnterDetailsCommandName)
				{
					AddNewRow();
				}
			}
			SaveChanges(this);
			if (AfterItemCommand != null)
			{
				AfterItemCommand(this, e);
			}
		}

		#endregion

		#region OnEditCommand

		protected override void OnEditCommand(DataGridCommandEventArgs e)
		{
			base.OnEditCommand(e);
			if (EditItemIndex > -1 && EditItemIndex != e.Item.ItemIndex)
			{
				SaveDiscardChanges(true, this);
			}
			EditItemIndex = e.Item.ItemIndex;
			RowAdded = false;
			BindCore();
			if (AfterEditCommand != null)
			{
				AfterEditCommand(this, e);
			}
		}
		#endregion

		#region OnCancelCommand

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "operation name should not be translated")]
		protected override void OnCancelCommand(DataGridCommandEventArgs e)
		{
			base.OnCancelCommand(e);
			try
			{
				int index = GetCollectionIndex(e);
				RemoveRowIfAdded(index);
				RollbackEditedBusinessObject(index);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (CommandError != null)
				{
					CommandError("Cancel", ex);
				}
				else
				{
					throw;
				}
			}
			EditItemIndex = -1;
			BindCore();
			if (AfterCancelCommand != null)
			{
				AfterCancelCommand(this, e);
			}
		}
		#endregion

		#region OnDeleteCommand

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "operation name should not be translated")]
		protected override void OnDeleteCommand(DataGridCommandEventArgs e)
		{
			if (EditItemIndex > -1)
			{
				SaveDiscardChanges(true, this);
			}
			DataGridDeleteEventArgs deleteArgs = new DataGridDeleteEventArgs(this, e.Item);
			if (CanDeleteCommand != null)
			{
				CanDeleteCommand(deleteArgs);
			}

			base.OnDeleteCommand(e);
			try
			{
				if (deleteArgs.CanDelete)
				{
					Collection.Delete(Collection.ToArray()[GetCollectionIndex(e)]);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (CommandError != null)
				{
					CommandError("Delete", ex);
				}
				else
				{
					throw;
				}
			}
			EditItemIndex = -1;
			RowAdded = false;
			BindCore();
			if (AfterDeleteCommand != null)
			{
				AfterDeleteCommand(this, e);
			}
		}
		#endregion

		#region Helper Methods

		#region RemoveRowIfAdded

		public bool RemoveRowIfAdded(int index)
		{
			bool result = false;
			if (RowAdded && index > -1 && index < TotalRecordCount)
			{
				object addedObject = ((IList)Collection)[index];
				BusinessObject addedBizO = addedObject as BusinessObject;
				if (addedBizO != null)
				{
					Collection.Delete(addedBizO);
				}
				else
				{
					Collection.RemoveAt(index);
					if (addedObject != null)
					{
						ErrorReporter.ReportOnce("ZDataGridDeleteNonBizOInBizOCollection", String.Format("An object of type {0} was found in an IBusinessObjectCollection.", addedObject.GetType().FullName));
					}
				}
				result = true;
			}
			RowAdded = false;
			return result;
		}
		#endregion

		#region RollbackEditedBusinessObject

		void RollbackEditedBusinessObject(int index)
		{
			if (index > -1 && index < TotalRecordCount)
			{
				object editedObject = ((IList)Collection)[index];
				BusinessObject editedBizO = editedObject as BusinessObject;
				if (editedBizO != null && BusinessObjectForRollback != null)
				{
					editedBizO.CopyPersistentValuesFrom(BusinessObjectForRollback);
				}
			}
			BusinessObjectForRollback = null;
		}
		#endregion

		#region GetBusinessObjectForItemCommand

		public BusinessObject GetBusinessObjectForItemCommand(DataGridCommandEventArgs e)
		{
			BusinessObject boundObject = null;
			if (Collection != null)
			{
				int indexInCollection = GetCollectionIndex(e);
				boundObject = ((IList)Collection)[indexInCollection] as BusinessObject;
			}
			return boundObject;
		}
		#endregion

		#region RowAdded

		public bool RowAdded
		{
			get
			{
				object savedValue = ViewState["RowAdded"];
				return (savedValue is bool) && (bool)savedValue;
			}
			set { ViewState["RowAdded"] = value; }
		}
		#endregion

		#endregion

		#region Event Members

		public event DataGridDeleteHandler CanDeleteCommand;
		public event DataGridCommandEventHandler BeforeItemCommand;
		public event DataGridCommandEventHandler AfterItemCommand;
		public event DataGridCommandEventHandler AfterDeleteCommand;
		public event DataGridCommandEventHandler AfterEditCommand;
		public event DataGridCommandEventHandler AfterCancelCommand;
		public event DataGridCommandEventHandler AfterUpdateCommand;
		public event CommandErrorHandler CommandError;

		#endregion

		#region Utility Methods

		protected ZGuid SaveDataObject(ZGuid indexer, object dataObject)
		{
			if (indexer == ZGuid.Empty)
			{
				indexer = ZGuid.NewZGuid();
			}
			Page.Session[indexer.ToString()] = dataObject;

			return indexer;
		}

		protected object LoadDataObject(ZGuid indexer)
		{
			return (indexer != ZGuid.Empty) ? Page.Session[indexer.ToString()] : null;
		}

		protected void ClearDataObject(ZGuid indexer)
		{
			Page.Session.Remove(indexer.ToString());
		}
		#endregion

		#endregion

		#region GetEDocs

		public IEnumerable<IeDocBase> GetEDocs(int rowIndex, ISupportEDocsBulkDownload module)
		{
			var itemPK = GetPKByRowIndex(rowIndex);

			if (itemsEDocsCache == null)
			{
				itemsEDocsCache = new Dictionary<ZGuid, IEnumerable<IeDocBase>>();
			}

			if (!itemsEDocsCache.TryGetValue(itemPK, out IEnumerable<IeDocBase> eDocs))
			{
				var helper = ObjectFactory.Get<IEDocsWebHelper>();
				var parentPks = module.GetEDocsBulkDownloadRelevantAndRelatedPKs(this, rowIndex);
				eDocs = helper.GetEDocs(parentPks);
				itemsEDocsCache.Add(itemPK, eDocs);
			}

			return eDocs;
		}

		Dictionary<ZGuid, IEnumerable<IeDocBase>> itemsEDocsCache;

		#endregion

		#region Apply Sort

		SortInfo sortInfo;
		public string OrderBy
		{
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					string[] orderBySegments = value.Split(' ');
					var sortColumnName = orderBySegments[0];
					var sortDirection = (orderBySegments.Length > 1 && orderBySegments[1].Trim().ToUpper() == "DESC") ? ListSortDirection.Descending : ListSortDirection.Ascending;
					sortInfo = new SortInfo(sortColumnName, sortDirection);
				}
			}
		}

		protected void ApplySort(object dataSource)
		{
			var bizOCollection = dataSource as IBusinessObjectCollection;
			if (bizOCollection != null && bizOCollection.Count > 0 && sortInfo != null)
			{
				bizOCollection.ApplySort(sortInfo);
			}
		}

		#endregion

		#region Hint fetching

		void AddFetchHints(object dataSource, int startIndex, int endIndex)
		{
			if (dataSource is IBusinessObjectCollection bizOCollection && bizOCollection?.Count > 0)
			{
				var businessObjects = new List<BusinessObject>();
				if (bizOCollection.Count < endIndex)
				{
					endIndex = bizOCollection.Count;
				}

				for (int i = startIndex; i < endIndex; i++)
				{
					businessObjects.Add((BusinessObject)(bizOCollection[i]));
				}

				if (businessObjects.Count > 0)
				{
					var columns = new DataGridColumn[Columns.Count];
					Columns.CopyTo(columns, 0);

					var tableColumns = new TableColumnCalculator().GetTableColumnsOnThisObject(businessObjects[0], columns);

					bizOCollection.FetchStrategy.FetchForView(businessObjects.ToArray(), tableColumns);

					foreach (var bizO in businessObjects)
					{
						if (bizO?.FetchStrategy != null)
						{
							bizO.FetchStrategy.FetchForView(tableColumns);
						}
					}

					foreach (var col in this.Columns)
					{
						if (col is ZTemplateColumn column && column != null)
						{
							if (column.GetItemTemplate() is ZItemTemplate itemTemplate && itemTemplate != null)
							{
								if (itemTemplate.GetControl() is IFetchHintGenerator hintGeneratorControl && hintGeneratorControl != null)
								{
									foreach (var bizO in businessObjects)
									{
										((IBindTo)hintGeneratorControl).BindTo = column.BindTo;
										hintGeneratorControl.AddFetchHint(bizO, "");
									}
								}
							}
						}
					}
				}
			}
		}

		protected void AddFetchHints(object dataSource)
		{
			if (dataSource is IBusinessObjectCollection bizOCollection && bizOCollection.Count > 0)
			{
				var endIndex = bizOCollection.Count;
				var startIndex = 0;
				if (AllowPaging)
				{
					startIndex = CurrentPageIndex * PageSize;
					var endOfCurrentPage = startIndex + PageSize;

					if (endOfCurrentPage < bizOCollection.Count)
					{
						endIndex = endOfCurrentPage;
					}
				}

				AddFetchHints(dataSource, startIndex, endIndex);
			}
		}

		#endregion

		#region Export to Excel

		public Func<IBusinessObjectCollection> GetCollectionToExportOverride;

		public void ExportIntoExcel()
		{
			var collectionToExport = GetCollectionToExportOverride?.Invoke() ?? Collection;
			ExportIntoExcel(collectionToExport);
		}

		public void ExportIntoExcel(IBusinessObjectCollection collectionToExport)
		{
			DataGridExcelExportHelper helper = new DataGridExcelExportHelper(collectionToExport, Columns);
			List<ExcelExportColumnBase> columns = helper.GetExcelExportColumns();
			if (helper.CanContinueWithExport)
			{
				ExportIntoExcel(collectionToExport, columns);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "http header should not be translated")]
		public void ExportIntoExcel(IBusinessObjectCollection collectionToExport, List<ExcelExportColumnBase> columns)
		{
			if (columns.Count > 0)
			{
				AddFetchHints(collectionToExport, 0, collectionToExport.Count);
				MemoryStream excelStream = new MemoryStream();
				ExcelExporter excelExporter = new ExcelExporter(collectionToExport, columns, null) { AllowMissingColumns = true };
				excelExporter.SaveToStream(excelStream);
				byte[] img = excelStream.ToArray();

				HttpContext.Current.Response.ContentType = DataContentTypes.Excel;
				HttpContext.Current.Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=\"SearchResults.xls\""));
				HttpContext.Current.Response.OutputStream.Write(img, 0, img.Length);
			}
		}

		#endregion

		#region IExternallyFiredPostBackHandler Members

		public void OnPostDataChanged(object sender, EventArgs e)
		{
			RaisePostDataChangedEvent();
		}

		public event EventHandler PostDataChanged;

		public void RaisePostDataChangedEvent()
		{
			Rebind();
			if (PostDataChanged != null)
			{
				PostDataChanged(this, new EventArgs());
			}
		}

		public void Rebind()
		{
			if (IsBindable(BusinessEntity))
			{
				Bind(BusinessEntity);
			}
		}

		public void HandleExternallyFiredPostBack(Control target, EventArgs e)
		{
			if (OnExternallyFiredPostback != null)
			{
				OnExternallyFiredPostback(target, e);
			}
		}
		public event EventHandler OnExternallyFiredPostback;

		#endregion

		#region INotificationProvider Members

		public IEnumerable<INotification> Notifications
		{
			get { return fNotifications ?? NotificationCollection.Empty; }
		}
		IEnumerable<INotification> fNotifications;

		bool INotificationProvider.HasNotifications()
		{
			return Notifications.HasNotifications();
		}

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return Notifications.HasNotifications(type);
		}

		INotificationType INotificationProvider.GetHighestSeverityNotificationType()
		{
			return Notifications.GetHighestSeverityNotificationType();
		}

		#endregion

		#region Internal Properties

		internal void OnPageIndexChangedInternal(DataGridPageChangedEventArgs e) => OnPageIndexChanged(e);
		internal void OnDeleteCommandInternal(DataGridCommandEventArgs e) => OnDeleteCommand(e);
		internal void OnEditCommandInternal(DataGridCommandEventArgs e) => OnEditCommand(e);
		internal void PrepareControlHierarchyInternal() => PrepareControlHierarchy();
		internal void InitializeItemInternal(DataGridItem item, DataGridColumn[] columns) => InitializeItem(item, columns);
		internal void OnCancelCommandInternal(DataGridCommandEventArgs e) => OnCancelCommand(e);

		#endregion
	}
}
