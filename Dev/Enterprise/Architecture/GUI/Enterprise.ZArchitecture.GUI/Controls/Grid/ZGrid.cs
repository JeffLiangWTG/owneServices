using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.Enumeration;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Data;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Core.Forms.Internal;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.GUI.Design;
using Enterprise.ZArchitecture.GUI.Grid;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
#if DEBUG
using CargoWise.EntityFramework.Testing;
#endif
#if !WINZOR
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
#else
using Graphics = System.Drawing.BGraphics;
#endif
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using Res = Enterprise.ZArchitecture.GUI.Res;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.ZArchitecture
{
	#region Interfaces

	public interface IMouseUpMouseMove
	{
		event MouseEventHandler MouseUp;
		event MouseEventHandler MouseMove;
	}

	public interface IDataGridToolTipSuspender
	{
		void SuspendToolTip();
		void ResumeToolTip();
	}

	public interface ICustomKeyHandlingGridColumn
	{
		bool ShouldProcessCmdKey(ref Message m, Keys keyData);
		bool ProcessCmdKey(ref Message m, Keys keyData);
	}

	public interface INavigatingGridColumn
	{
		bool ShouldColumnHandleKey(Keys keyData);
	}

	#endregion

	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(ZGridDesigner))]
	[ToolboxItem(true)]
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class ZGrid :
		DataGrid,
		IDynamicToolTip,
		IDataGridToolTipSuspender,
		IMouseUpMouseMove,
		IPastableControl,
		IIsVisibleForBindingControl,
		IListEditableControl,
		IExtendedControl,
		IDataBoundControl,
		IBindingMemberForCompileTimeCheckProvider,
		ISupportInitialize,
		ICaptionedComponents,
		IBindTo,
		IShouldntBeReadOnly,
		IAdditionalHotkeyProviders,
		IMenuParent
	{
		#region Constructors

		// TODO: These constructors will be re-factored later as part of the ediInterfaceConnector Data Import Wizard changes.
		public ZGrid() : this(true) { }

		public ZGrid(bool importLicenceCheckArg, string[] columnsToResizeWidth = null, string[] columnsToResizeHeight = null)
		{
			ColorContextKey = "";
			fColumns = CreateNewColumnCollection();

			importLicenceCheck = importLicenceCheckArg; // must be called before SetupContextMenu()
			SetupContextMenu();

			AllowReadOnlyToModifyTabStop = false;
			AllowReadOnlyRowsToBeDeleted = false;
			movingCurrentCellWithoutFocus = false;
			shouldSetErrorsOnTabPage = true;
			AllowBeginDrag = true;
			AllowDragDropWithChanges = true;
			IsWholeRowSelectedOnClick = false;
			IsAllowedToStartEditing = true;
			CaptionVisible = false;
			AllowNavigation = false;

			InitializeControl();
			InitializeEventHandlers();
			InitializeExtensions();

			RegisterHotkeys();

			this.FlatMode = true;
			this.BorderStyle = BorderStyle.None;
			if (!DesignModeFinder.IsDesigning)
			{
				var theme = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme;
				BackgroundColor = theme.GridBackgroundColor;
				HeaderBackColor = theme.FilterBackgroundColor;
			}

			translationFeedbackManager = new TranslationFeedbackManager(this, TranslationFeedbackManager.ClickMode.None);
			devInfoPopupManager = new DevInfoPopupManager(this, TranslationFeedbackManager.ClickMode.None);

			DisposableLeakListener.Instance.RegisterDisposable(this);

			if (columnsToResizeWidth != null)
			{
				_ = new ColumnResizer(this, columnsToResizeWidth);
			}

			if (columnsToResizeHeight != null)
			{
				_ = new RowResizer(this, columnsToResizeHeight);
			}
		}

		#region Initialization

		void InitializeControl()
		{
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			UpdateStyles();
		}

		void InitializeEventHandlers()
		{
			removeAction = RemoveAction.RemoveAndDelete;
			WrapCurrentChangedEventHandler();
			WrapPositionChangedEventHandler();
		}

		void InitializeExtensions()
		{
			Extensions = NewExtensionCollection();
		}

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new ControlExtensionCollection(this) { new ZGridColumnLabelRenderer() };
		}

		#endregion

		#endregion

		#region Bare

		[WTG.StaticAnalysis.Annotation.CodeAlive("There are possible future usages.")]
		[ToolboxItem(false)]
		public class Bare : ZGrid
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Fields

		readonly bool importLicenceCheck = true;

#if DEBUG
		public bool IsListManagerNotNull = true;
#endif

		#endregion

		#region Properties

		#region Name

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string Name
		{
			get { return base.Name; }
			set
			{
				base.Name = value;
				Columns.TableName = GetTableFromControlName(Name);
			}
		}

		#endregion

		#region Columns

		/// <summary>
		/// Gets the collection of System.Windows.Forms.DataGridColumnStyle objects for the grid.
		/// </summary>
		public virtual ZGridColumns Columns
		{
			get { return fColumns; }
		}
		protected ZGridColumns fColumns;

		[DefaultValue(null)]
		public ZLimitedColumnsProvider LimitedColumns { get; set; }

#if DEBUG
		public
#else
		protected
#endif
		ZGridColumns DefaultColumns
		{
			get { return defaultColumns; }
			private set
			{
				if (defaultColumns != null && value != null)
				{
					if (oldDefaultColumns == null)
					{
						oldDefaultColumns = defaultColumns;
					}
					else
					{
						var defaultColumnsList = defaultColumns.ToList();
						foreach (var column in defaultColumnsList)
						{
							if (!oldDefaultColumns.Contains(column.ColumnName))
							{
								oldDefaultColumns.AddColumn(column);
							}
						}
					}
				}
				defaultColumns = value;

				GridLayoutManager.EnsureVisibleColumnsFirst(defaultColumns);
			}
		}
		ZGridColumns defaultColumns;
		ZGridColumns oldDefaultColumns;

		protected virtual ZGridColumns CreateNewColumnCollection()
		{
			return new ZGridColumns(this);
		}

		public ZTextBoxColumnStyle LastFocusedColumn
		{
			get { return lastFocusedColumn; }
			private set
			{
				if (lastFocusedColumn != value)
				{
					if (lastFocusedColumn != null)
					{
						lastFocusedColumn.TextBox.KeyDown -= new KeyEventHandler(TextBox_KeyDown);
					}

					lastFocusedColumn = value;

					if (lastFocusedColumn != null)
					{
						lastFocusedColumn.TextBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);
					}
				}
			}
		}

		void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (ParentForm != null)
			{
				var scanner = ParentForm.Scanner;
				if (scanner != null)
				{
					scanner.HandleKey(ParentForm, e);
				}
			}
		}

		ZTextBoxColumnStyle lastFocusedColumn;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsColumnStartingEdit
		{
			get; private set;
		}

		#endregion

		#region GridId

		[Browsable(false)]
		public string GridId
		{
			get
			{
#if DEBUG
				if (string.IsNullOrEmpty(gridId) && ComponentExtensions.IsDesignMode(this))
				{
					this.gridId = Guid.NewGuid().ToString();
				}
#endif
				return this.gridId;
			}
			set
			{
				this.gridId = value;
			}
		}
		string gridId;

		#endregion

		#region Readonly

		[DefaultValue(false)]
		public virtual new bool ReadOnly  // Enterprise needs a virtual ReadOnly
		{
			get { return base.ReadOnly || IsListReadOnly; }
			set
			{
				guiReadOnly = value;
				if (FindForm() != null || DesignModeFinder.IsDesigning)
				{
					UpdateReadOnly(); // base.ReadOnly creates the BindingContext at the wrong time.
				}
			}
		}

		public void SetReadOnlyIncludingColumnStyles(bool value)
		{
			this.ReadOnly = value;

			foreach (var columnStyle in ColumnStyles.OfType<ZGridColumnInfo>())
			{
				columnStyle.IsReadOnly = value;
			}

			foreach (var column in Columns)
			{
				column.ColumnStyle.ReadOnly = value;
				if (column.ColumnStyle is ZGridColumnStyle)
				{
					((ZGridColumnStyle)column.ColumnStyle).TextBox.ReadOnly = value;
				}
			}

			if (FindForm() != null || DesignModeFinder.IsDesigning)
			{
				UpdateReadOnly(); // base.ReadOnly creates the BindingContext at the wrong time.
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			UpdateReadOnly();
		}

		void UpdateReadOnly()
		{
			base.ReadOnly = (guiReadOnly || IsEmptyParentList);
		}

		/// <summary>
		/// Gets or sets a value indicating if ReadOnlyChanged will modify the TabStop of the Grid. 
		/// When the Grid, or all Grid Columns are ReadOnly, this property will be used to determine whether or not to set TabStop to false.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(false)]
		public bool AllowReadOnlyToModifyTabStop { get; set; }

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(false)]
		public bool AllowReadOnlyRowsToBeDeleted { get; set; }

		internal virtual bool CanEdit
		{
			get { return !IsScrolling && !IsRowHeaderClicked && !IsWholeRowSelectedOnClick && !movingSelection && !movingCurrentCellWithoutFocus; }
		}

		protected override void OnReadOnlyChanged(EventArgs e)
		{
			base.OnReadOnlyChanged(e);
			SetTabStop();
		}

		protected bool AllColumnsReadOnly
		{
			get
			{
				if (IsDisposing || this.IsDisposedOrHasDisposedParent())
				{
					return true;
				}

				if ((List != null && !List.AllowNew && List.Count == 0) || ReadOnly)
				{
					return true;
				}

				var result = true;
				var i = 0;
				var visibleColumnStylesCount = TableStyles.Count > 0 ? TableStyles[0].GridColumnStyles.Count : Columns.Count;

				while ((i < visibleColumnStylesCount) && result)
				{
					result &= TableStyles.Count > 0 ? IsCellReadOnly(CurrentCell.RowNumber, i) : Columns[i].ColumnStyle.ReadOnly;
					i++;
				}

				return result;
			}
		}

		bool IsCellReadOnly(int rowNum, int columnIndex)
		{
			return TableStyles[0].GridColumnStyles[columnIndex] is ZTextBoxColumnStyle column ? column.IsCellReadOnly(ListManager, rowNum) : TableStyles[0].GridColumnStyles[columnIndex].ReadOnly;
		}

		bool IsRowReadOnly(int rowNum)
		{
			if (List == null || rowNum >= List.Count || rowNum < 0)
			{
				return true;
			}
			var currentBusinessObject = List[rowNum] as BusinessObject;
			if (currentBusinessObject != null && currentBusinessObject.ReadOnly)
			{
				return true;
			}
			for (var i = 0; i < ColumnStyles.Count; ++i)
			{
				var column = ColumnStyles[i] as ZGridColumnInfo;
				if (column.IsReadOnly)
				{
					continue;
				}

				if (currentBusinessObject != null)
				{
					if (!((IAccessBusinessObject)currentBusinessObject).IsPropertyReadOnly(column.ColumnName))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		protected internal bool IsCellEdited { get; protected set; }

		bool IsListReadOnly
		{
			get
			{
				var listReadOnly = false;
				if (List != null && List is IBusinessObjectCollection)
				{
					listReadOnly = ((IBusinessObjectCollection)List).ReadOnly;
				}

				return listReadOnly;
			}
		}

		#endregion

		#region IsIndexInRange

		public bool IsIndexInRange(int index)
		{
			return DataGridRowsLength > 0 && (index >= 0 && index < DataGridRowsLength);
		}

		#endregion

		#region Binding Properties

		[Browsable(false)]
		public new CurrencyManager ListManager
		{
			get
			{
#if DEBUG
				if (!IsListManagerNotNull)
				{
					return null;
				}
#endif
				return base.ListManager;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string DataMember
		{
			get { return base.DataMember; }
			set { base.DataMember = value; }
		}

		/// <summary>
		/// Returns the underlying IBindingList that the Grid is bound to.
		/// </summary>
		[Browsable(false)]
		public IBindingList List
		{
			get { return (ListManager != null) ? ListManager.List as IBindingList : null; }
		}

		protected virtual bool IsDataSourceChanged
		{
			get { return ((IBusinessObjectState)List).HasChanges; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Sort
		{
			get
			{
				var view = ListManager != null ? ListManager.List as DataView : null;
				return (view != null) ? view.Sort : "";
			}
			set
			{
				var view = ListManager != null ? ListManager.List as DataView : null;
				if (view != null)
				{
					view.Sort = value;
				}
			}
		}

		#endregion

		#region Notification

		const int ErrorState = 0x400000;
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsErrored
		{
			get
			{
				var methodInfo = typeof(Control).GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic);
				return (bool)methodInfo.Invoke(this, new object[] { ErrorState });
			}
			set
			{
				if (IsErrored != value)
				{
					var methodInfo = typeof(Control).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic);
					methodInfo.Invoke(this, new object[] { ErrorState, value });
					Invalidate();
				}
			}
		}

		#region Errors On Tab Pages

		protected internal virtual bool ShouldShowNotifications
		{
			get { return true; }
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(true)]
		public bool ShouldSetErrorsOnTabPage
		{
			get { return shouldSetErrorsOnTabPage && ShouldShowNotifications; }
			set { shouldSetErrorsOnTabPage = value; }
		}
		bool shouldSetErrorsOnTabPage;

		#endregion

		#endregion

		#region DragDrop

		/// <summary>
		/// Gets or sets the value that determines if this Grid will begin a DragDrop.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(true)]
		public bool AllowBeginDrag { get; set; }

		/// <summary>
		/// Gets or sets the value that, if false, will prevent Drag Drop if the DataSource has changes.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(true)]
		public bool AllowDragDropWithChanges { get; set; }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDragging { get; set; }

		#endregion

		#region Selecting

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(false)]
		public bool IsWholeRowSelectedOnClick { get; set; }

		internal bool AllowFocus
		{
			get { return GetState(GridState.CanFocus); }
			set { SetState(GridState.CanFocus, value); }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsMouseOnAValidRow
		{
			get
			{
				var rowIndex = RowUnderMouse();
				return ListManager != null && ListManager.Count > 0 && rowIndex >= 0 && rowIndex < ListManager.Count;
			}
		}

		#endregion

		#region Custom Design

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Customise Columns menu item is visible on the context menu.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(true)]
		public bool IsCustomiseMenuVisible
		{
			get { return isCustomiseMenuVisible; }
			set
			{
				isCustomiseMenuVisible = value;
				if (Created)
				{
					CustomiseMenuItem.Visible = value;
				}
			}
		}
		bool isCustomiseMenuVisible = true;

		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			if (CustomiseMenuItem != null)
			{
				CustomiseMenuItem.Visible = isCustomiseMenuVisible;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(true)]
		public new bool FlatMode
		{
			get { return base.FlatMode; }
			set
			{
#if DEBUG
				if (!value)
				{
					throw new InvalidOperationException("Do not set FlatMode = false");
				}
#endif
				base.FlatMode = value;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(BorderStyle.None)]
		public new BorderStyle BorderStyle
		{
			get { return base.BorderStyle; }
			set
			{
#if DEBUG
				if (value != BorderStyle.None)
				{
					throw new InvalidOperationException("Do not set ZGrid border style");
				}
#endif
				base.BorderStyle = value;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(BorderStyle.None)]
		public new Color HeaderBackColor
		{
			get { return base.HeaderBackColor; }
			set
			{
#if DEBUG
				if (value != ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.FilterBackgroundColor)
				{
					throw new InvalidOperationException("Do not set ZGrid HeaderBackColor");
				}
#endif
				base.HeaderBackColor = value;
			}
		}

		#endregion

		#region ScrollBars

		/// <summary>
		/// Gets a value indicating whether the vertical scrollbar is currently visible.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsVerticalScrollBarVisible
		{
			get { return VertScrollBar.Visible; }
		}

		/// <summary>
		/// Gets a value indicating whether the horizontal scrollbar is currently visible.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsHorizontalScrollBarVisible
		{
			get { return HorizScrollBar.Visible; }
		}

		#endregion

		#region IsVisibleForBinding

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsVisibleForBinding
		{
			get { return Visible; }
			set
			{
				if (!fInVisibleForBinding)
				{
					fInVisibleForBinding = true;
					try
					{
						if (Visible != value)
						{
							Visible = value;
							OnIsVisibleForBindingChanged();
						}
					}
					finally
					{
						fInVisibleForBinding = false;
					}
				}
			}
		}

		bool fInVisibleForBinding;

		public event EventHandler IsVisibleForBindingChanged;

		protected virtual void OnIsVisibleForBindingChanged()
		{
			IsVisibleForBindingChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		#endregion

		#region Parents

		ZForm ParentForm
		{
			get { return fParentForm ?? (fParentForm = FindForm() as ZForm); }
		}
		ZForm fParentForm;

		bool IsEmptyParentList
		{
			get { return emptyParentList; }
			set
			{
				emptyParentList = value;
				base.ReadOnly = (guiReadOnly || emptyParentList);
			}
		}
		#endregion

		#region CopyColumnCaptionsToBoundFields

		[DefaultValue(true)]
		[Description("Specifies whether UI column captions should be copied to bound property infos on business layer.")]
		public virtual bool CopyColumnCaptionsToBoundFields
		{
			get { return copyColumnCaptionsToBoundFields; }
			set { copyColumnCaptionsToBoundFields = value; }
		}
		bool copyColumnCaptionsToBoundFields = true;

		#endregion

		internal bool SaveLastSelectedLayout { get; set; }

		#endregion

		#region Binding

		[DefaultValue(false)]
		[Description("Specifies whether after binding to an ActiveBusinessObjectCollection/BusinessObjectCollection for the first time, the rows should be reloaded from database or not.")]
		public bool CheckDatabaseAfterFirstBinding { get; set; }

		public event EventHandler OnRowsChangedInDatabase;

		protected virtual void OnAfterDataBound()
		{
			var collection = ListManager != null ? ListManager.List as IBusinessObjectCollection : null;
			var collectionInternals = ListManager != null ? ListManager.List as IBusinessObjectCollectionInternals : null;
			if (collection != null)
			{
				GenerateComparersForCollection();

				if (TableStyles.Count == 0 || !CheckValidColumnStyles(false))
				{
					RefreshTableStyles();
				}
			}

			if (CheckDatabaseAfterFirstBinding && collectionInternals != null)
			{
				var listChanged = collectionInternals.HasChangesFromDatabase();
				CheckDatabaseAfterFirstBinding = false;
				if (listChanged)
				{
					OnRowsChangedInDatabase?.Invoke(this, EventArgs.Empty);
				}
			}

			if (!gridExtensionsAreInitialized)
			{
				gridColourSchemeManager = GetNewGridColourSchemeManager();
#if DEBUG
				if (Globals.IsTest)
				{
					GridColourSchemeManagerForTest = gridColourSchemeManager;
				}
#endif

				var copyManager = ObjectFactory.New<IGridUniversalCopyManager>(this);
				copyManager.AddMenuItems();

				AddOperationalActions(collectionInternals);

				gridExtensionsAreInitialized = true;
			}
		}
		GridColourSchemeManager gridColourSchemeManager;
		bool gridExtensionsAreInitialized;

		void AddOperationalActions(IBusinessObjectCollectionInternals collectionInternals)
		{
			ObjectFactory.Get<Enterprise.Integration.IModuleOperationalActionsHelper>().AddOperationalActionsIntoActionsMenu(this);
		}

		protected void OnAfterBind()
		{
			AfterBind?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler AfterBind;

		public void RunAfterBind(EventHandler handler)
		{
			if (DataSource != null)
			{
				handler(this, EventArgs.Empty);
			}
			else
			{
				var holder = new EventHandlerHolder();
				holder.Value = delegate
				{
					AfterBind -= holder.Value;
					handler(this, EventArgs.Empty);
				};
				AfterBind += holder.Value;
			}
		}

		class EventHandlerHolder
		{
			public EventHandler Value;
		}

		void ReOrderColumsBaseOnColumnStyleOrder()
		{
			var newColumns = CreateNewColumnCollection();
			foreach (ZGridColumnInfo columnStyle in ColumnStyles)
			{
				var column = Columns[columnStyle.ColumnName];
				if (column != null)
				{
					newColumns.AddColumn(column);
				}
			}
			fColumns = newColumns;
			fColumns.HasLayoutChanged = true;
			RefreshTableStyles();
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			HookListChanged();

			if (ListManager != null && lastListManagerList != ListManager.List)
			{
				var oldLastListManagerList = lastListManagerList;
				lastListManagerList = ListManager.List;
				ApplySortOnListSwapped(oldLastListManagerList as IBindingListView);
				ListManagerListChanged?.Invoke(sender, new ListManagerListChangedEventArgs(oldLastListManagerList, lastListManagerList));
				ListManagerListSwapped();
			}
		}
		IList lastListManagerList;

		void ListManagerListSwapped()
		{
			HookNotificationsChanged();
			GenerateComparersForCollection();
		}

		public event EventHandler<ListManagerListChangedEventArgs> ListManagerListChanged;

		public class ListManagerListChangedEventArgs : EventArgs
		{
			internal ListManagerListChangedEventArgs(IList oldList, IList newList)
			{
				Old = oldList;
				New = newList;
			}

			[SuppressWeaklyTypedCollectionMessage]
			public IList Old { get; }
			[SuppressWeaklyTypedCollectionMessage]
			public IList New { get; }
		}

		void ApplySortOnListSwapped(IBindingListView oldLastListManagerList)
		{
			var listView = ListManager != null ? ListManager.List as IBindingListView : null;
			if (listView != null && listView.SupportsAdvancedSorting && oldLastListManagerList != null
				&& !SortDescriptionsAreEqual(listView.SortDescriptions, oldLastListManagerList.SortDescriptions))
			{
				listView.ApplySort((oldLastListManagerList).SortDescriptions);
			}
		}

		//without doing this check, TestTransferRules() in BMComponentsUserControlTest.cs fails on the last line, claiming 0 controls instead of 10.
		//No clue why ApplySort with the sort it was already using causes such a bug. Maybe it's not as much of a NOP as I like to think it is?
		bool SortDescriptionsAreEqual(ListSortDescriptionCollection a, ListSortDescriptionCollection b)
		{
			if (a.Count != b.Count)
			{
				return false;
			}

			if (a.Count == 0)
			{
				return true;
			}

			for (var i = 0; i < a.Count; ++i)
			{
				if (a[i].PropertyDescriptor.Equals(b[i].PropertyDescriptor) && a[i].SortDirection.Equals(b[i].SortDirection))
				{
					continue;
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		void HookNotificationsChanged()
		{
			if (ListForNotifications != null)
			{
				UnhookNotificationsChanged();
			}

			ListForNotifications = ListManager != null ? ListManager.List as IBusinessObjectCollection : null;

			if (ListForNotifications != null)
			{
				ListForNotifications.NotificationsChanged += List_NotificationsChanged;
				UpdateNonCellNotifications(null);
			}
		}

		void UnhookNotificationsChanged()
		{
			if (ListForNotifications != null)
			{
				ListForNotifications.NotificationsChanged -= List_NotificationsChanged;
				ListForNotifications = null;
			}
		}

		delegate void UpdateNonCellNotificationsDelegate(BusinessObject b);

		void List_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
#if DEBUG
			if (Globals.IsTest && (IsTestingUpdateNonCellNotifications || GridListTracked != null) || !IsHandleCreated)
#else
			if (!IsHandleCreated)
#endif
			{
				UpdateNonCellNotifications(sender as BusinessObject);
			}
			else
			{
				var source = e.SourceOfNotificationChange as BusinessObject;
				if (!IsUpdateNonCellNotificationsQueued)
				{
					Delegate d = new UpdateNonCellNotificationsDelegate(UpdateNonCellNotifications);
					BeginInvoke(d, new object[] { source });
					IsUpdateNonCellNotificationsQueued = true;
				}
				else if (!ForceDrawRowHeaderNotificationsOnNextPaint)
				{
					ForceDrawRowHeaderNotificationsOnNextPaint = source == null || IsBusinessObjectVisible(source);
				}
			}
		}

		IBusinessObjectCollection ListForNotifications;
		bool IsUpdateNonCellNotificationsQueued;

		void GridColumnStyles_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			CheckValidColumnStyles(true, e);
			GenerateComparersForCollection();
		}

		void TableStyles_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			CheckValidColumnStyles(true, e);
		}

		void GenerateComparersForCollection()
		{
			if (ListManager != null)
			{
				if (ListManager.List is IBusinessObjectCollection collection)
				{
					foreach (var o in ColumnStyles)
					{
						if (o is IMultiTypeColumnStyleInfo info)
						{
							string listMember;
							if (string.IsNullOrEmpty(info.BindToList) &&
								BindingContext != null &&
								ListManager != null &&
								ListManager.List != null)
							{
								var descriptor = ListManager.GetItemProperties()[info.ColumnName];

								if (descriptor == null)
								{
									if (o is IOverridablePropertyDescriptor overridablePropertyDescriptor)
									{
										descriptor = overridablePropertyDescriptor.PropertyDescriptor;
									}
								}

								listMember = descriptor != null ? MetadataAccessor.GetListMember(info.BindToList, descriptor, info.ColumnName) : null;
								if (listMember != null)
								{
									listMember = listMember.Replace('.', '+');
								}
							}
							else
							{
								listMember = info.BindToList.Replace('.', '+');
							}

							if (listMember != null)
							{
								collection.AddGuidListMapping(info.ColumnName, listMember);
							}
						}
						else if (o is ZTimeEditExColumnStyleInfo info2)
						{
							collection.AddIsTime(info2.ColumnName);
						}
					}
				}
			}
		}

		#region Show Error for Deprecated OGridColumnStyles!

		void ShowDeprecatedColumnErrors()
		{
			if (deprecatedColumnErrors.Count > 0)
			{
				var gridName = (Parent != null && Parent is IButtonGrid) ? Parent.Name : Name;
				var message = new StringBuilder();

				message.Append(Res.GetString("f99bd99d-1524-4d40-ae61-9f25f4d4b749", "The following objects used in grid '{0}' are deprecated:", gridName) + "  \n");

				foreach (var columnError in deprecatedColumnErrors)
				{
					message.Append(columnError);
				}

				message.Append("\n\n" + Res.GetString("26450d57-33b7-4000-a265-1d8d23f6fa50", "Please use the Grid Designer in design mode to replace these controls!") + "       ");

				Globals.Message.ShowInformation(message.ToString(), Res.GetString("3501d5da-a0de-436e-8a4a-7a11ec4ba9de", "Deprecated Controls.."));
			}
		}

		readonly List<string> deprecatedColumnErrors = new List<string>();

		#endregion

		#endregion

		#region GridColourSchemeManager

		GridColourSchemeManager GetNewGridColourSchemeManager()
		{
			if (GridColourSchemeManagerDeciding != null)
			{
				var args = new GridColourSchemeManagerDecidingEventArgs();
				GridColourSchemeManagerDeciding(this, args);
				if (args.GridColourSchemeManager != null)
				{
					return args.GridColourSchemeManager;
				}
			}

			return new GridColourSchemeManager(this);
		}

		/// <summary>
		/// Subscribe to this event to provide a custom Grid Colour Scheme Manager.
		/// </summary>
		public event EventHandler<GridColourSchemeManagerDecidingEventArgs> GridColourSchemeManagerDeciding;

#if DEBUG
		internal GridColourSchemeManager GridColourSchemeManagerForTest;
#endif

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(false)]
		public bool IsColourGridFactoryStandAlone { get; set; }

		#endregion

		#region CurrentListChanged
#if DEBUG
		internal
#endif
		IBindingList lastList;
#if DEBUG
		internal
#endif
		void HookListChanged()
		{
			if (lastList != List)
			{
				if (lastList != null)
				{
					lastList.ListChanged -= OGrid_ListChanged;
				}

				if (List != null)
				{
					List.ListChanged += OGrid_ListChanged;
					lastList = List;

					HandleParentPositionChangedCausesNewList();
				}
			}
		}
#if DEBUG
		internal
#endif
		void OGrid_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.Reset ||
				e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted ||
				e.ListChangedType == ListChangedType.ItemMoved)
			{
				if (e.ListChangedType == ListChangedType.Reset)
				{
					ColorByPKIsExpired = true;
				}
				CurrentLoadColorsVersion = Guid.NewGuid();
			}
			if (e.ListChangedType == ListChangedType.ItemDeleted)
			{
				if (LastFocusedColumn != null && !inOnLeave && !inProcessCmdKey && isBound)
				{
					LastFocusedColumn.HideEditControl();
					/*if (DataGridRowsLength > 1)
					{
						try
						{
							BeginEdit(LastFocusedColumn, CurrentCell.RowNumber); //WI00079815 - fix a bug where if you begin editing a cell on a grid (yellow highlight), save and a concurrency error causes the row you're on to be deleted, the edit remains open on the deleted row
						}
						catch (IndexOutOfRangeException) { }
						catch (ArgumentException) { } //Issue 00947614. In the case of an ArgumentException being thrown, no state has been modified, so it's OK to ignore and keep going. Making inProcessCmdKey static should also solve this, but I am paranoid now.
					}*/
					//Sorry, WI00079815, but after yet ANOTHER issue is due to this BeginEdit call, I'm reluctant to keep it in. Who knows what other bugs it's introducing I haven't gotten to yet? Maybe in the future a better solution can be found.
				}
			}
			if (e.ListChangedType == ListChangedType.ItemChanged && RowColorsAreDataViewOptimisable && suspendResetingRowColorOnItemChanged == 0 &&
				List != null &&  e.NewIndex >= 0 && e.NewIndex < List.Count)
			{
				var bizo = List[e.NewIndex] as BusinessObject;
				if (bizo != null)
				{
					if (ColorByPK.ContainsKey(bizo.PK))
					{
						ColorByPK.Remove(bizo.PK);
					}
				}
			}

			if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted || e.ListChangedType == ListChangedType.Reset)
			{
				lastListChangedStackTrace = new StackTrace().ToString();
			}

			OnListChanged(e);
		}

		internal string lastListChangedStackTrace;

		int suspendResetingRowColorOnItemChanged;
#if DEBUG
		internal
#endif
		IDisposable SuspendResetingRowColorOnItemChanged()
		{
			suspendResetingRowColorOnItemChanged++;
			return new DisposableAction(() => suspendResetingRowColorOnItemChanged--);
		}

		protected virtual void OnListChanged(ListChangedEventArgs e)
		{
			if (IsMaxRowsSet)
			{
				if (e.ListChangedType == ListChangedType.ItemAdded && e.NewIndex >= (MaximumRows - 1))
				{
					IsAtMaximumRowCount = true;
					DisableNewRows();
				}
				else if (e.ListChangedType == ListChangedType.ItemDeleted)
				{
					IsAtMaximumRowCount = false;
					EnableNewRows();
				}
			}

			if (e.ListChangedType == ListChangedType.ItemDeleted)
			{
				resetRowsOnNextPositionChanged = true;
			}
		}

		protected virtual void HandleParentPositionChangedCausesNewList()
		{
			if (IsMaxRowsSet)
			{
				if (List.Count >= MaximumRows)
				{
					IsAtMaximumRowCount = true;
					DisableNewRows();
				}
				else
				{
					IsAtMaximumRowCount = false;
				}
			}
		}

		#endregion

		#region Selected Rows

		public BusinessObject GetFirstSelectedRow()
		{
			BusinessObject selectedBizO = null;

			for (var index = 0; index < DataGridRowsLength; index++)
			{
				if (IsSelected(index) && index < ListManager.Count)
				{
					selectedBizO = ListManager.List[index] as BusinessObject;
					break;
				}
			}

			return selectedBizO;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public BusinessObject[] GetSelectedRows()
		{
			var result = new List<BusinessObject>();
			try
			{
				for (var index = 0; index < DataGridRowsLength; index++)
				{
					if (IsSelected(index) && (ListManager == null || index < ListManager.Count))
					{
						var obj = ListManager.List[index] as BusinessObject;
						if (obj != null)
						{
							result.Add(obj);
						}
					}
				}
			}
			catch (IndexOutOfRangeException ex)
			{
				Globals.Message.ShowDeveloperException(ex);
			}

			return result.ToArray();
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectedRowCount
		{
			get
			{
				var count = 0;

				try
				{
					for (var index = 0; index < DataGridRowsLength; index++)
					{
						if (IsSelected(index))
						{
							count++;
						}
					}
				}
				catch (IndexOutOfRangeException ex)
				{
					Globals.Message.ShowDeveloperException(ex);
				}

				return count;
			}
		}

		protected internal int DataGridRowsLength
		{
			get
			{
				if (dataGridRowsPropertyInfo == null)
				{
					dataGridRowsPropertyInfo = typeof(DataGrid).GetProperty("DataGridRowsLength", BindingFlags.Instance | BindingFlags.NonPublic);
				}

				return (int)dataGridRowsPropertyInfo.GetValue(this, null);
			}
		}
#if DEBUG
		internal
#endif
		PropertyInfo dataGridRowsPropertyInfo;
#if DEBUG
		internal int DataGridRowsLengthOverride { get; set; }
#endif
		#endregion

		#region Tool Tips

		public event QueryToolTipEventHandler QueryToolTip;

		protected virtual void GetToolTip(ToolTipInfo info)
		{
			QueryToolTip?.Invoke(this, info);
		}

		void IDynamicToolTip.GetToolTip(ToolTipInfo info)
		{
			GetToolTip(info);
		}

		/* this has been getting serialised all throughout enterprise. It needs to be obsoleted and removed */
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableToolTips
		{
			get { return false; }
			set { }
		}

		void IDataGridToolTipSuspender.SuspendToolTip()
		{
			var oldToolTip = ToolTipField.GetValue(this);
			if (oldToolTip != null)
			{
				ToolTip = oldToolTip;
			}
			ToolTipField.SetValue(this, null);
		}

		void IDataGridToolTipSuspender.ResumeToolTip()
		{
			ToolTipField.SetValue(this, ToolTip);
		}

		FieldInfo ToolTipField
		{
			get
			{
				if (fToolTipField == null)
				{
					fToolTipField = typeof(DataGrid).GetField("toolTipProvider", BindingFlags.Instance | BindingFlags.NonPublic);
				}

				return fToolTipField;
			}
		}

		object ToolTip;
		FieldInfo fToolTipField;

		#endregion

		#region Maximum Rows

		/// <summary>
		/// Restricts the number of Rows a User can enter into a Grid. No restriction when 0 or less.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(0), DpiState(DpiState.Unscaled)]
		public int MaximumRows { get; set; }
		bool IsAtMaximumRowCount;

		protected virtual void DisableNewRows()
		{
			if ((List is DataView view) && view.AllowNew)
			{
				view.AllowNew = false;
			}
		}

		void SetCurrentRowNotNew()
		{
			var gridStateField = typeof(DataGrid).GetField("gridState", BindingFlags.Instance | BindingFlags.NonPublic);
			var gridState = (BitVector32)gridStateField.GetValue(this);
			gridState[1048576] = false;
			gridStateField.SetValue(this, gridState);
		}

		protected virtual void EnableNewRows()
		{
			if ((List is DataView view) && !view.AllowNew)
			{
				try
				{
					ListManager.SuspendBinding();
					view.AllowNew = true;
				}
				finally
				{
					ListManager.ResumeBinding();
				}
			}
		}

		bool IsMaxRowsSet
		{
			get { return MaximumRows > 0; }
		}

		#endregion

		#region BindingContext refresh on font change issue fix

		// test in ZGridTest.TestBindingContext

		bool inOnFontChanged;

		protected override void OnFontChanged(EventArgs e)
		{
			inOnFontChanged = true;
			try
			{
				base.OnFontChanged(e);
			}
			finally
			{
				inOnFontChanged = false;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool BindingContextInitialized
		{
			get
			{
				return bindingContext != null;
			}
		}

		public override BindingContext BindingContext
		{
			get
			{
				if (!(inOnFontChanged && DataSource == null) && bindingContext == null)
				{
					bindingContext = base.BindingContext;
				}

				return bindingContext;
			}
			set
			{
				base.BindingContext = value;

				if (bindingContext != value)
				{
					bindingContext = null;
				}
			}
		}
#if DEBUG
		internal
#endif
		BindingContext bindingContext;

		#endregion

		#region Implementation

		bool movingCurrentCellWithoutFocus;
		public MenuItem DeleteMenuItem;
		public MenuItem ExportAllColumnsToExcelMenuItem;
		public MenuItem ExportVisibleColumnsToExcelMenuItem;
		public MenuItem DeleteMenuItemModule;
		public bool ShowExcelMenuItems = true;
		bool guiReadOnly;
		bool emptyParentList;
		bool isTaskMenuItemLoaded;
		const string menuItemTasksName = "ModuleGridTaskStatusChanger_MenuItemTask";
		const string ImportWizardContextMenuName = "ImportWizardContextMenu";
		public const string AvailableLayoutsContextMenuName = "AvailableLayoutsContextMenuName";
		ZMenuItem TasksMenuItem;
		ZMenuItem UserTasksMenuItem;

		protected static bool IsValidSource(BindingManagerBase source)
		{
			return (source != null && source.Count > 0 && source.Position > -1);
		}

		void TryToEndCurrentEdit()
		{
			try
			{
				if (List is IActiveBusinessObjectCollection collection && !(collection is IDoNotHaveFactory) && collection.Factory != null)
				{
					using (ActiveBusinessObjectCollection.DelayListChangedEvents(collection.Factory))
					{
						ListManager.EndCurrentEdit();
					}
				}
				else
				{
					ListManager.EndCurrentEdit();
				}
			}
			catch (IndexOutOfRangeException)
			{
				// nothing can be done, this may be caused by the BindingContext changing due to changing of tab visibility or similar
			}
			catch (NoNullAllowedException)
			{
				if (!(ListManager.GetCurrent() is DataRowView currentDataRowView) || currentDataRowView.Row.RowState != DataRowState.Detached)
				{
					throw;
				}
			}
			catch (ArgumentNullException)
			{
				if (!(ListManager.GetCurrent() is DataRowView currentDataRowView) || currentDataRowView.Row.RowState != DataRowState.Detached)
				{
					throw;
				}
			}
		}

		#region Context Menu
#if DEBUG
		internal
#endif
		void ContextMenu_Popup(object sender, EventArgs e)
		{
			contextPopupPoint = MousePosition;
#if DEBUG
			if (Globals.IsTest)
			{
				contextPopupPoint = MousePositionForTesting;
			}
#endif
			SetTickUntickAllMenuItemsVisible(ShouldShowTickUntickAllMenuItems());

			FinishAnyUncommittedRow(preferCommit: true);

			var rowUnderMouse = RowUnderMouse();

			ExportAllColumnsToExcelMenuItem.Visible = ShowExcelMenuItems && ShowExportToExcelMenuItem;
			ExportVisibleColumnsToExcelMenuItem.Visible = ShowExcelMenuItems && ShowExportToExcelMenuItem;
			FindMenuItem.Visible = List is IBusinessObjectCollection;
			FindNextMenuItem.Visible = List is IBusinessObjectCollection;
			FindPreviousMenuItem.Visible = List is IBusinessObjectCollection;
			FindMenuItemSeparator.Visible = List is IBusinessObjectCollection;
			importDataMenuItem.Visible = List is IBusinessObjectCollection;
			viewNotificationsMenuItem.Visible = List is IBusinessObjectCollectionNotificationsViewerProvider;
			viewNotificationsMenuItemSeparaor.Visible = viewNotificationsMenuItem.Visible;

			DeleteMenuItem.Visible = (List != null) && IsDeleteMenuItemVisible;
			DeleteMenuItem.Enabled = CanDeleteCurrentRow(rowUnderMouse);

			if (CheckGridRowsOutOfSyncWithListManager())
			{
				ForceRefreshManager();
			}

			ZGridContextMenuHelper.Instance.Synchronise(this);
			ShowOrRemoveRedCross();
			SetCopyMenuItemVisibleAndEnabled(rowUnderMouse);
			SetImportDataMenuItemVisibleAndEnabled();
			SetMassUpdateMenuItemVisibleAndEnabled();
			SetDataVersionLogsMenuItemVisibleAndEnabled();
			AddFastAccessLayoutSubMenuItems();
			SetDuplicateMenuItemEnabled();
		}

		void SetDuplicateMenuItemEnabled()
		{
			if (DuplicateMenuItem != null)
			{
				DuplicateMenuItem.Enabled = AllowNew;
			}
		}

		void ContextMenu_Collapse(object sender, EventArgs e)
		{
			isTaskMenuItemLoaded = false;
		}

		public void SetupTaskStatusMenuItem(string currentUserFullName, Action<ZMenuItem, ZMenuItem> populateMenuItem)
		{
			if (!isTaskMenuItemLoaded)
			{
				isTaskMenuItemLoaded = true;
				RemoveExistingTasksMenuItem();

				TasksMenuItem = new ZMenuItem(ResString.GetMultilingualString("2CA95DC4-1F00-4FEF-969F-A6EED6AE28E5", "Tasks"))
				{
					Name = menuItemTasksName
				};
				UserTasksMenuItem = new ZMenuItem(ResString.GetMultilingualString("2D8FA8CC-B1AA-4B98-B988-110ED2854575", "{0}'s Tasks", currentUserFullName));

				ContextMenu.MenuItems.Add(new ZMenuItem("-"));
				ContextMenu.MenuItems.Add(TasksMenuItem);
				ContextMenu.MenuItems.Add(UserTasksMenuItem);

				populateMenuItem(TasksMenuItem, UserTasksMenuItem);
			}
		}

		public void RemoveExistingTasksMenuItem()
		{
			for (var i = 0; i < ContextMenu.MenuItems.Count; i++)
			{
				if (ContextMenu.MenuItems[i].Name == menuItemTasksName)
				{
					ContextMenu.MenuItems.RemoveAt(i + 1);
					ContextMenu.MenuItems.RemoveAt(i);
					ContextMenu.MenuItems.RemoveAt(i - 1);
					break;
				}
			}
		}

		protected virtual bool CanDeleteCurrentRow(int currentRowIndex)
		{
			return currentRowIndex != -1 && !ReadOnly && (AllowReadOnlyRowsToBeDeleted || !IsRowReadOnly(currentRowIndex));
		}

		protected virtual int RowUnderMouse()
		{
			var p = MousePosition;
#if DEBUG
			if (Globals.IsTest)
			{
				p = MousePositionForTesting;
			}

#endif
			var hit = HitTest(PointToClient(p));
			return hit.Row;
		}

#if DEBUG
		MenuItem GridColumnDesigningMenuItem;

		void ExportGridColumnDetailsClick(object sender, EventArgs e)
		{
			var dialog = new ZSaveFileDialog
			{
				Filter = "All files (*.*)|*.*|Text Files (*.txt)|*.txt"
			};

			var dr = dialog.ShowDialog(this);
			if (dr == DialogResult.OK)
			{
				using (var sw = new StreamWriter(dialog.OpenFile()))
				{
					foreach (var column in Columns)
					{
						sw.WriteLine(column.ColumnStyle.MappingName + ".Width = " + column.ColumnStyle.Width.ToString() + ";");
					}
				}
			}
		}
#endif

		protected MenuItem CustomiseMenuItem;

		class ZContextMenu : ContextMenu
		{
			MenuItem FindShortcutMenuItem(MenuItemCollection root, Shortcut key, Control control)
			{
				for (var i = 0; i < root.Count; i++)
				{
					var menuItem = root[i];
					if (menuItem.Shortcut == key)
					{
						return menuItem;
					}

					if (ShouldCheckNestedMenuItemsForShortcut(menuItem))
					{
						try
						{
							FireOnPopup(menuItem);
						}
						catch (Exception ex) when (ex is NullReferenceException || ex.InnerException is NullReferenceException)
						{
							ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Error on FireOnPopup for menueitem: Text:{0}, Visibility:{1} on grid named {2}.", menuItem.Text, menuItem.Visible, control.Name), ex);
						}

						var nested = FindShortcutMenuItem(menuItem.MenuItems, key, control);
						if (nested != null)
						{
							return nested;
						}
					}
				}

				return null;
			}

			bool ShouldCheckNestedMenuItemsForShortcut(MenuItem menuItem)
			{
				return menuItem.MenuItems.Count > 0 && !Attribute.IsDefined(menuItem.GetType(), typeof(DoNotPopUpToCheckShortcutsAttribute));
			}

			void FireOnPopup(MenuItem m)
				=> typeof(MenuItem).GetMethod("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(m, new object[] { new ShortcutPopupEventArgs() });

			protected override bool ProcessCmdKey(ref Message msg, Keys keyData, Control control)
			{
				if ((keyData & Keys.Modifiers) == (Keys.Control | Keys.Shift))
				{
					OnPopup(EventArgs.Empty);

					var selectedMenu = FindShortcutMenuItem(MenuItems, (Shortcut)keyData, control);
					if (selectedMenu != null)
					{
						selectedMenu.PerformClick();
						return true;
					}
				}

				return base.ProcessCmdKey(ref msg, keyData, control);
			}
		}

		internal class ShortcutPopupEventArgs : EventArgs
		{
		}

		ResourceString GetDeleteMenuItemText(RemoveAction removeAction)
		{
			return removeAction == RemoveAction.Remove ? ResString.GetMultilingualString("20ed5985-7b9a-4a2d-b3c3-fdaf9b107d99", "&Remove") : ResString.GetMultilingualString("cd1ac264-c378-4a7f-8cb8-216a4049d645", "&Delete");
		}
		/// <summary>
		/// Builds the context menu.
		/// </summary>
		protected virtual void SetupContextMenu()
		{
			if (ContextMenu != null)
			{
				UnHookContextMenu();
				DisposeMenu();
			}

			ContextMenu = new ZContextMenu();
			DeleteMenuItem = new ZMenuItem(GetDeleteMenuItemText(RemoveAction), DeleteMenuItem_Click);
			CustomiseMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.Customize", "&Customize Columns"), CustomiseMenuItem_Click);

			HookContextMenu();
			if (!DesignMode)
			{
				AddContextMenuItems();
			}

			AddCopyToNewRowMenuItem();
			AddExportToExcelMenuItem();
			AddImportDataMenuItem();
			AddMassUpdateMenuItem();
			AddTickUntickAllMenuItems();
			AddSearchTextMenuItems();
			AddDataVersionLogsMenuItem();
			AddFastAccessLayoutMenuItem();
			AddViewNotificationsMenuItem();

#if DEBUG
			if (!DesignModeFinder.IsDesigning && EnvProxy.Instance.Registry.ExportGridLayoutDetails)
			{
				GridColumnDesigningMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.ExportGridColumnDetails", "Export &Grid's Column Details"), ExportGridColumnDetailsClick, Shortcut.CtrlShiftE);
				ContextMenu.MenuItems.Add(GridColumnDesigningMenuItem);
			}
#endif
		}

		void DisposeMenu()
		{
			if (ContextMenu != null)
			{
				ContextMenu.Dispose();
				ContextMenu = null;
			}

			if (DeleteMenuItem != null)
			{
				DeleteMenuItem.Dispose();
				DeleteMenuItem = null;
			}

			if (CustomiseMenuItem != null)
			{
				CustomiseMenuItem.Dispose();
				CustomiseMenuItem = null;
			}

			if (TasksMenuItem != null)
			{
				TasksMenuItem.Dispose();
				TasksMenuItem = null;
			}

			if (UserTasksMenuItem != null)
			{
				UserTasksMenuItem.Dispose();
				UserTasksMenuItem = null;
			}

			if (viewNotificationsMenuItem != null)
			{
				viewNotificationsMenuItem.Dispose();
				viewNotificationsMenuItem = null;
				viewNotificationsMenuItemSeparaor?.Dispose();
				viewNotificationsMenuItemSeparaor = null;
			}
		}

		protected virtual void AddContextMenuItems()
		{
			ContextMenu.MenuItems.Add(0, CustomiseMenuItem);
			ContextMenu.MenuItems.Add(1, DeleteMenuItem);
		}

		internal void DisableCustomiseMenuItem()
		{
			CustomiseMenuItem.Visible = false;
		}

		/// <summary>
		/// Customise menu item click event.
		/// </summary>
		protected void CustomiseMenuItem_Click(object sender, EventArgs e)
		{
			CustomiseColumns();
		}

		protected virtual void HookContextMenu()
		{
			ContextMenu.Popup += ContextMenu_Popup;
			ContextMenu.Collapse += ContextMenu_Collapse;
		}

		protected virtual void UnHookContextMenu()
		{
			ContextMenu.Popup -= ContextMenu_Popup;
			ContextMenu.Collapse -= ContextMenu_Collapse;
		}

		protected virtual bool IsDeleteMenuItemVisible
		{
			get
			{
				var result = RemoveAction != RemoveAction.NoRemovePossible && List.AllowRemove;
				if (GetDeleteMenuVisibleMethod != null)
				{
					result &= GetDeleteMenuVisibleMethod();
				}
				return result;
			}
		}

		public delegate bool GetDeleteMenuVisibleDelegate();

		public GetDeleteMenuVisibleDelegate GetDeleteMenuVisibleMethod;

		protected void DeleteMenuItem_Click(object sender, EventArgs e)
		{
			EndEdit();
			var rowsDeleted = HandleDelete(MouseUpInfo != null ? MouseUpInfo.Row : CurrentCell.RowNumber);
			if (rowsDeleted > 0 && LastFocusedColumn != null)
			{
				LastFocusedColumn.HideEditControl();
				if (DataGridRowsLength > 1)
				{
					BeginEdit(LastFocusedColumn, CurrentCell.RowNumber);
				}
			}
		}

		void DuplicateSelectedRowsMenuItemClick(object sender, EventArgs e)
		{
			if (AllowNew)
			{
				var selectedElements = SelectedElements.ToArray();
				if (ListManager != null)
				{
					ListManager.EndCurrentEdit();
				}

				foreach (var bizo in selectedElements)
				{
					if (!bizo.IsDeleted)
					{
						var clonedObject = bizo.Clone();
						List.Add(clonedObject);
					}
				}
			}
		}

		public void AddDuplicateSelectedRowsMenuItem()
		{
			if (DuplicateMenuItem == null)
			{
				DuplicateMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.Duplicate", "Du&plicate"), DuplicateSelectedRowsMenuItemClick);
				ContextMenu.MenuItems.Add(DuplicateMenuItem);
				DuplicateMenuItem.Name = "Duplicate";
			}
		}
		MenuItem DuplicateMenuItem;

		bool AllowNew => List?.AllowNew ?? false;

		void ShowOrRemoveRedCross()
		{
			if (IsErrored)
			{
				if (removeRedCrossMenuItem == null)
				{
					removeRedCrossMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.RemoveRedCross", "Remove Red Cross"), (s, e) => IsErrored = false);
				}
				if (!ContextMenu.MenuItems.Contains(removeRedCrossMenuItem))
				{
					ContextMenu.MenuItems.Add(removeRedCrossMenuItem);
				}
			}
			else
			{
				if (removeRedCrossMenuItem != null)
				{
					if (ContextMenu.MenuItems.Contains(removeRedCrossMenuItem))
					{
						ContextMenu.MenuItems.Remove(removeRedCrossMenuItem);
					}
					removeRedCrossMenuItem.Dispose();
					removeRedCrossMenuItem = null;
				}
			}
		}
		MenuItem removeRedCrossMenuItem;

		#region Search Text Menu Item

		MenuItem FindMenuItem;
		MenuItem FindNextMenuItem;
		MenuItem FindPreviousMenuItem;
		MenuItem FindMenuItemSeparator;

		void AddSearchTextMenuItems()
		{
			FindMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.Find", "&Find"), SearchTextClick, Shortcut.CtrlF);
			ContextMenu.MenuItems.Add(0, FindMenuItem);

			FindNextMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.FindNext", "Find &Next"), SearchNextTextClick, Shortcut.CtrlN);
			ContextMenu.MenuItems.Add(1, FindNextMenuItem);

			FindPreviousMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.FindPrevious", "Find &Previous"), SearchPreviousTextClick, Shortcut.CtrlP);
			ContextMenu.MenuItems.Add(2, FindPreviousMenuItem);

			FindMenuItemSeparator = new ZMenuItem("-");
			ContextMenu.MenuItems.Add(3, FindMenuItemSeparator);
		}

		void SearchPrevNext(object sender, EventArgs args, bool dir)
		{
			//If we're not set up with an existing search, show the Find dialog to set up a new one. Else do prev/next on the existing search.
			if (rowFinderBusinessObject == null || (rowFinderBusinessObject.isDirty && (gridRowFinderForm == null || !gridRowFinderForm.Visible || gridRowFinderForm.IsDisposed)))
			{
				SearchTextClick(sender, args);
			}
			else
			{
				rowFinderBusinessObject.DoSearch(dir, true);
			}
		}

		void SearchNextTextClick(object sender, EventArgs args)
		{
			SearchPrevNext(sender, args, true);
		}

		void SearchPreviousTextClick(object sender, EventArgs args)
		{
			SearchPrevNext(sender, args, false);
		}

		void SearchTextClick(object sender, EventArgs args)
		{
			if (rowFinderBusinessObject == null)
			{
				rowFinderBusinessObject = new GridRowFinderBusinessObject(this);
			}

			rowFinderBusinessObject.PossiblyRefreshColumnsToSearch();

			if (gridRowFinderForm == null || gridRowFinderForm.IsDisposed)
			{
				gridRowFinderForm = new GridRowFinderForm(rowFinderBusinessObject);
				gridRowFinderForm.Owner = this.FindForm();
				gridRowFinderForm.Show();
			}
			else
			{
				gridRowFinderForm.BringToFront();
				gridRowFinderForm.Focus();
			}
		}

		GridRowFinderBusinessObject rowFinderBusinessObject;
		GridRowFinderForm gridRowFinderForm;

		#endregion

		#region Tick/Untick All Menu Item
#if DEBUG
		internal
#endif
		MenuItem TickAllMenuItem;
#if DEBUG
		internal
#endif
		MenuItem UntickAllMenuItem;
#if DEBUG
		internal
#endif
		MenuItem TickUntickAllMenuItemSpearator;
		Point contextPopupPoint;
		int selectedColumn;

		void AddTickUntickAllMenuItems()
		{
			TickAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.CheckBoxColumn.TickAll", "&Tick All in Current Column"), TickAllClick, Shortcut.CtrlT);
			ContextMenu.MenuItems.Add(0, TickAllMenuItem);

			UntickAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.CheckBoxColumn.UntickAll", "&Untick All in Current Column"), UntickAllClick, Shortcut.CtrlU);
			ContextMenu.MenuItems.Add(1, UntickAllMenuItem);

			TickUntickAllMenuItemSpearator = new ZMenuItem("-");
			ContextMenu.MenuItems.Add(2, TickUntickAllMenuItemSpearator);
		}
#if DEBUG
		internal
#endif
		void TickAllClick(object sender, EventArgs args)
		{
			TickUntickAll(true);
		}
#if DEBUG
		internal
#endif
		void UntickAllClick(object sender, EventArgs args)
		{
			TickUntickAll(false);
		}
#if DEBUG
		internal
#endif
		void TickUntickAll(bool tick)
		{
			ZCheckBoxColumnStyle style;

			if (inProcessCmdKey)
			{
				style = Columns[CurrentCell.ColumnNumber].ColumnStyle as ZCheckBoxColumnStyle;
			}
			else
			{
				style = Columns[selectedColumn].ColumnStyle as ZCheckBoxColumnStyle;
			}

			if (style != null && ListManager.List.Count > 0 && !ReadOnly)
			{
				EndEdit();

				ListManager.List.OfType<BusinessObject>().ToList().ForEach(b =>
				{
					var propertyInfo = b.ZPropertyInfoHash.GetPropertySafe(style.MappingName);
					if (propertyInfo == null && b is ICustomFieldProvider customFieldProvider)
					{
						propertyInfo = customFieldProvider.GetCustomBusinessObject().ZPropertyInfoHash.GetPropertySafe(style.MappingName);
					}

					if (propertyInfo?.PropertyType == typeof(ZBool) && !propertyInfo.ReadOnly)
					{
						propertyInfo.Value = new ZBool(tick);
					}
				});

				if (LastFocusedColumn != null)
				{
					BeginEdit(LastFocusedColumn, CurrentCell.RowNumber);
				}
			}
		}

#if DEBUG
		internal void TickUntickAllForTest(int columnToTick, bool tick)
		{
			this.selectedColumn = columnToTick;
			TickUntickAll(tick);
		}
#endif

		void SetTickUntickAllMenuItemsVisible(bool visible)
		{
			TickAllMenuItem.Visible = visible;
			UntickAllMenuItem.Visible = visible;
			TickUntickAllMenuItemSpearator.Visible = visible;
		}

		bool ShouldShowTickUntickAllMenuItems()
		{
			var hit = HitTest(PointToClient(contextPopupPoint));
			selectedColumn = hit.Column;

			return selectedColumn > -1 && Columns[selectedColumn].ColumnStyle is ZCheckBoxColumnStyle && ListManager.List.Count > 0 && !ReadOnly;
		}

		#endregion

		#region AddLayoutFastAccessMenuItems

		MenuItem availableLayouts;
		void AddFastAccessLayoutMenuItem()
		{
			availableLayouts = new ZMenuItem(ResString.GetMultilingualString("ZGrid|AvailableLayouts", "Available Layouts"));
			availableLayouts.Name = AvailableLayoutsContextMenuName;
			ContextMenu.MenuItems.Add(availableLayouts);
			availableLayouts.Visible = IsGridLayoutConfigurable;
			availableLayouts.Enabled = IsGridLayoutConfigurable;
		}

		void AddFastAccessLayoutSubMenuItems()
		{
			if (isBound && availableLayouts.Visible)
			{
				availableLayouts.MenuItems.Clear();

				new FastAccessLayoutSubMenuItemsCreator(this, availableLayouts).AddSubMenus(GridLayoutManager.GetSortedAllLayouts(this));
			}
		}

		#endregion

		#region CopyMenuItem

		/// <summary>
		/// Will show the Copy menu item if the BusinessObject Type bound to implements ITemplateCopyable.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(true)]
		[Description("Will show the Copy menu item if the BusinessObject Type bound to implements ITemplateCopyable.")]
		public bool AllowCopyToNewRowMenuItem { get; set; } = true;

		void AddCopyToNewRowMenuItem()
		{
			CopyToNewRowMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.CopyToNewRow", "&Copy to New Row"), CopyMenuItemClick) { Visible = false, Enabled = false };

			var posOfDeleteItem = ContextMenu.MenuItems.IndexOf(DeleteMenuItem);
			ContextMenu.MenuItems.Add((posOfDeleteItem == -1 ? 0 : posOfDeleteItem), CopyToNewRowMenuItem);
		}
#if DEBUG
		internal
#endif
		MenuItem CopyToNewRowMenuItem;

		internal MenuItem CopyMenuItem
		{
			get { return copyMenuItem ?? CopyToNewRowMenuItem; }
			set { copyMenuItem = value; }
		}
		MenuItem copyMenuItem;

		void SetCopyMenuItemVisibleAndEnabled(int currentRowIndex)
		{
			CopyToNewRowMenuItem.Visible = false;
			CopyToNewRowMenuItem.Enabled = false;

			if (AllowCopyToNewRowMenuItem && ListManager != null && ListManager.GetCurrent() is ITemplateCopyable && AllowNew)
			{
				CopyToNewRowMenuItem.Visible = true;
				CopyToNewRowMenuItem.Enabled = currentRowIndex != -1 && !ReadOnly;
			}
		}
#if DEBUG
		internal
#endif
		void CopyMenuItemClick(object sender, EventArgs e)
		{
			if (ListManager != null && ListManager.Count > 0)
			{
				if (ListManager.GetCurrent() is ITemplateCopyable source)
				{
					if (List is IBusinessObjectCollection collection)
					{
						using (collection.SuspendListChanged())
						{
							var copied = source.TemplateCopy() as BusinessObject;

							if (copied != null)
							{
								collection.Add(copied);
								if (!(collection is IActiveBusinessObjectCollection))
								{
									collection.HasChanges = true;
								}
								ListManager.Position = ListManager.Count - 1;
							}
						}
						SetCurrentCellToFirstNonReadOnlyColumn(true);
					}
				}
			}
		}

		#endregion

		#region ViewDataVersionLogsMenuItem

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(true)]
		public bool IsDataVersionLogsMenuItemVisible { get; set; } = true;

		void AddDataVersionLogsMenuItem()
		{
			viewDataVersionLogsMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.ViewDataVersionLogs", "&View Data Version Logs..."), DataVersionLogsMenuItem_Click);
			ContextMenu.MenuItems.Add(viewDataVersionLogsMenuItem);
		}

		void SetDataVersionLogsMenuItemVisibleAndEnabled()
		{
			if (viewDataVersionLogsMenuItem != null)
			{
				currentLogTarget = null;

				var isMenuItemVisible = false;
				if (IsDataVersionLogsMenuItemVisible)
				{
					if (ListManager != null)
					{
#if DEBUG
						if (Globals.IsTest)
						{
							cellLocation = MousePositionForTesting;
						}
#endif
						var hit = HitTest(PointToClient(cellLocation));
						var currentRow = hit.Row;
						if (currentRow >= 0 && currentRow < DataGridRowsLength && currentRow < ListManager.Count)
						{
							currentLogTarget = ListManager.List[currentRow] as IDataVersionLoggingSupported;
						}
					}

					isMenuItemVisible = currentLogTarget != null && currentLogTarget.IsDataVersionsAutoLogged;
				}
				viewDataVersionLogsMenuItem.Visible = isMenuItemVisible;
				viewDataVersionLogsMenuItem.Enabled = isMenuItemVisible;
			}
		}
		Point cellLocation;

		void DataVersionLogsMenuItem_Click(object sender, EventArgs e)
		{
			if (currentLogTarget != null)
			{
				var auditLogsForm = ObjectFactory.Get<IZAuditLogsForm>("IZAuditLogsForm", currentLogTarget);
				if (auditLogsForm.IsAuditServerValid)
				{
					ZFormModaliser.Show((ZChildForm)auditLogsForm, FindForm());
				}
				else
				{
					auditLogsForm.Dispose();
					Globals.Message.Show(Res.GetString("29945a93-e6fd-485e-aba3-574b3b91589f", "Audit server is not valid."));
				}
			}
		}

#if DEBUG
		internal
#endif
		MenuItem viewDataVersionLogsMenuItem;
		IDataVersionLoggingSupported currentLogTarget;

		#endregion

		#region AddViewNotificationsMenuItem

		MenuItem viewNotificationsMenuItem;
		MenuItem viewNotificationsMenuItemSeparaor;

		void AddViewNotificationsMenuItem()
		{
			if (viewNotificationsMenuItem == null)
			{
				viewNotificationsMenuItem = new ZMenuItem(ResString.GetMultilingualString("ZGird|ViewNotificationsMenuItem", "View Grid Notifications"), ViewNotificationsMenuItem_Click);

				ContextMenu.MenuItems.Add(viewNotificationsMenuItemSeparaor ?? (viewNotificationsMenuItemSeparaor = new ZMenuItem("-")));
				ContextMenu.MenuItems.Add(viewNotificationsMenuItem);
				viewNotificationsMenuItem.Visible = false;
				viewNotificationsMenuItemSeparaor.Visible = false;
			}
		}

		void ViewNotificationsMenuItem_Click(object sender, EventArgs e)
		{
			if (DataSource != null && List is IBusinessObjectCollectionNotificationsViewerProvider provider)
			{
				var notificationsViewerProvider = new BusinessObjectNotificationsViewerSupporter(provider);
				if (notificationsViewerForm != null)
				{
					notificationsViewerForm.Close();
				}

				notificationsViewerForm = new BusinessObjectNotificationsViewerForm(notificationsViewerProvider);
				notificationsViewerForm.OnCurrentSelectedItemChangedAction = this.SelectSingleElementByPK;
				notificationsViewerForm.Show();
			}
			else
			{
				Globals.Message.Show(Res.GetString("22f4c717-d268-4755-b227-4e70982df847", "View Grid Notifications function has not yet been supported in this grid."));
			}
		}
		BusinessObjectNotificationsViewerForm notificationsViewerForm;

		#endregion

		#endregion

		#region Columns and Rows

		public void ExposeAllColumns()
		{
			foreach (var column in Columns)
			{
				column.IsVisible = true;
			}
			RefreshTableStyles();
		}

		internal virtual void AssignCurrentColumn(ZTextBoxColumnStyle currentColumn)
		{
			LastFocusedColumn = currentColumn;
		}

		protected
#if DEBUG
		internal
#endif
		int GetFirstVisibleRow()
		{
			return (int)typeof(DataGrid).GetField("firstVisibleRow", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This message is for developers only.")]
		bool CheckValidColumnStyles(bool reportError, CollectionChangeEventArgs changeEventArgs = null)
		{
#if DEBUG
			if (DataSource is AutoDummyBizo)
			{
				return true;
			}
#endif

			if (!IsDisposed && DataSource != null && List is IBusinessObjectCollection && TableStyles.Count > 0)
			{
				StringBuilder errorMessageBuilder = null;
				if (reportError)
				{
					errorMessageBuilder = new StringBuilder();
					errorMessageBuilder
						.AppendFormat("ZGrid {0}{1} bound to {2} has the following non-Z columns:", Name, GetParentControlNames(false), List != null ? List.GetType().FullName : string.Empty)
						.AppendLine();
				}

				var nonZColumnsCount = 0;
				var nonZColumnStyles = new List<DataGridColumnStyle>(TableStyles[0].GridColumnStyles.Count);

				foreach (DataGridColumnStyle columnStyle in TableStyles[0].GridColumnStyles)
				{
					if (columnStyle.PropertyDescriptor != null && !(columnStyle is ZTextBoxColumnStyle))
					{
						nonZColumnsCount++;

						if (reportError)
						{
							nonZColumnStyles.Add(columnStyle);
							errorMessageBuilder
								.AppendFormat("'{0}' of type {1} mapped to '{2}'.", columnStyle.HeaderText, columnStyle.GetType().FullName, columnStyle.MappingName)
								.AppendLine();
						}
					}
				}

				if (nonZColumnsCount > 0)
				{
					if (reportError)
					{
						errorMessageBuilder.AppendFormat("({0} non-Z columns of {1} total)", nonZColumnsCount, TableStyles[0].GridColumnStyles.Count).AppendLine();

						var allColumnStyles =
							TableStyles[0].GridColumnStyles.Cast<DataGridColumnStyle>()
								.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}: {1}: {2}", c.HeaderText, c.MappingName, c.GetType().FullName));

						var allColumnStylesFromColumns = Columns.Select(column => string.Format(CultureInfo.InvariantCulture, "{0}: {1}: {2}", column.ColumnStyle.HeaderText, column.ColumnStyle.MappingName, column.ColumnStyle.GetType().FullName));

						errorMessageBuilder.AppendFormat("All columns from TableStyles:		{0}", string.Join("| ", allColumnStyles.ToArray()));
						errorMessageBuilder.AppendLine();

						errorMessageBuilder.AppendFormat("All columns from Column.ColumnStyles:	{0}", string.Join("| ", allColumnStylesFromColumns.ToArray()));
						errorMessageBuilder.AppendLine();

						if (changeEventArgs != null)
						{
							var element = changeEventArgs.Element is DataGridColumnStyle triggerringStyle ? triggerringStyle.MappingName : changeEventArgs.Element != null ? changeEventArgs.Element.ToString() : "null";

							errorMessageBuilder.AppendFormat("CollectionChangeEvent: Action = {0}, Element = {1}", changeEventArgs.Action, element);
						}

						ErrorReporter.ReportOnce("ZGrid_CheckValidColumnStyles", errorMessageBuilder.ToString());
					}

					return false;
				}
			}

			return true;
		}

		public ZTextBoxColumnStyle GetValidColumnStyle(int columnNumber)
		{
			CheckValidColumnStyles(true);

			var index = 0;
			if (TableStyles.Count > 0)
			{
				foreach (DataGridColumnStyle colStyle in TableStyles[0].GridColumnStyles)
				{
					if (colStyle.PropertyDescriptor != null)
					{
						if (index == columnNumber)
						{
							return colStyle as ZTextBoxColumnStyle;
						}

						index++;
					}
				}
			}
			return null;
		}

		#region Set Column Properties

		public void SetColumnModuleID(string columnName, ModuleIdentifier moduleID)
		{
			if (GetColumnStyle(columnName) is IZColumnStyleInfoWithModuleID info)
			{
				info.ModuleID = moduleID;
				RefreshTableStyles();
			}
			else
			{
				ErrorReporter.ReportOnce("SetColumnModuleID" + columnName, columnName + " is not in the ColumnStyles for the Grid: " + Name);
			}
		}

		#region Group Name

		public void SetColumnGroupName(string columnName, ResourceStringData groupName)
		{
			if (DefaultColumns != null)
			{
				SetColumnGroupName(DefaultColumns, columnName, groupName);
			}
			var column = SetColumnGroupName(Columns, columnName, groupName);
			var info = GetColumnStyle(columnName);
			if (info != null)
			{
				info.GroupName = groupName;
			}

			if (column == null && info == null)
			{
				ErrorReporter.ReportOnce("SetColumnGroupName" + columnName, columnName + " is not in the ColumnStyles for the Grid: " + Name);
			}
			else
			{
				RefreshTableStyles();
			}
		}

		static ZGridColumn SetColumnGroupName(ZGridColumns columns, string columnName, ResourceStringData groupName)
		{
			var column = columns[columnName];
			if (column != null)
			{
				column.GroupName = groupName;
			}
			return column;
		}

		#endregion

		#region Setting Column Width

		public void SetColumnWidth(string columnName, int width)
		{
			if (DefaultColumns != null)
			{
				SetColumnWidth(DefaultColumns, columnName, width);
			}
			var column = SetColumnWidth(Columns, columnName, width);
			var info = GetColumnStyle(columnName);
			if (info != null)
			{
				ControlDpiScalingHelper.SetWidth(ref info, width, true);
			}

			if (column == null && info == null)
			{
				ErrorReporter.ReportOnce("SetColumnWidth" + columnName, columnName + " is not in the ColumnStyles for the Grid: " + Name);
			}
			else
			{
				RefreshTableStyles();
			}
		}

		static ZGridColumn SetColumnWidth(ZGridColumns columns, string columnName, int width)
		{
			var column = columns[columnName];
			if (column != null)
			{
				ControlDpiScalingHelper.SetWidth(column.ColumnStyle, width, true);
			}
			return column;
		}

		#endregion

		#region Setting Column Caption

		public void SetColumnCaption(string columnName, string caption)
		{
			if (DefaultColumns != null)
			{
				SetColumnCaption(DefaultColumns, columnName, caption);
			}
			var column = SetColumnCaption(Columns, columnName, caption);
			var info = GetColumnStyle(columnName);
			if (info != null)
			{
				info.Caption = caption;
			}

			if (column == null && info == null)
			{
				ErrorReporter.ReportOnce("SetColumnCaption" + columnName, columnName + " is not in the ColumnStyles for the Grid: " + Name);
			}
			else
			{
				RefreshTableStyles();
			}
		}

		static ZGridColumn SetColumnCaption(ZGridColumns columns, string columnName, string caption)
		{
			var column = columns[columnName];
			if (column != null)
			{
				column.ColumnStyle.HeaderText = caption;
			}
			return column;
		}

		#endregion

		#endregion

		#region Columns Customisation

		/// <summary>
		/// Displays the Column Customisation dialog.
		/// </summary>
		public void CustomiseColumns()
		{
			if (DataSource == null)
			{
				Globals.Message.ShowError(Res.GetString("9c71f563-27dd-49d4-89c4-0e5d8582b8a2", "Grid is not bound to data."));
				return;
			}
			if (DefaultColumns == null)
			{
				DefaultColumns = Columns.Clone();
			}

			EndCurrentEdit();
			LastFocusedColumn?.HideEditControl();

			var customiseForm = CreateNewGridCustomise();
			customiseForm.Closed += CustomiseForm_Closed;
			customiseForm.StartPosition = FormStartPosition.CenterScreen;
			Disposed += customiseForm.ParentGridDisposed;

			ZFormModaliser.Show(customiseForm, FindForm());
		}

		protected virtual ZGridCustomise CreateNewGridCustomise()
		{
			var helper = new GridLayoutContextKeyProviderHelper();
			customiseBizObj = new ZGridCustomiseBizObj(helper.GetAllGridIDsForStmModuleFilter(this), helper.GetAllGridIDsForStmData(this), CurrentColumnLayout, LayoutCategoryPK);
			return new ZGridCustomise(Columns, DefaultColumns, IsGridLayoutConfigurable, customiseBizObj);
		}
		protected ZGridCustomiseBizObj customiseBizObj;

		protected virtual void EndCurrentEdit()
		{
			if (ListManager != null)
			{
				ListManager.EndCurrentEdit();
			}
		}

		void CustomiseForm_Closed(object sender, EventArgs e)
		{
			var customiseForm = (ZGridCustomise)sender;
			Disposed -= customiseForm.ParentGridDisposed;

			if (customiseForm.DialogResult == DialogResult.OK)
			{
				fColumns = customiseForm.Result;
				fColumns.HasLayoutChanged = true;

				if (IsGridLayoutConfigurable && customiseBizObj != null)
				{
					if (CurrentColumnLayout?.ColumnLayoutName != customiseBizObj.CurrentLayout?.ColumnLayoutName)
					{
						SaveLastSelectedLayout = true;
					}
					CurrentColumnLayout = customiseBizObj.CurrentLayout;
				}

				BeginInvoke(new MethodInvoker(RefreshTableStyles));
			}

			customiseBizObj = null;
		}

		/// <summary>
		/// Resets the column view to the default state.
		/// </summary>
		public void ResetColumns()
		{
			if (Columns.HasLayoutChanged)
			{
				fColumns = DefaultColumns.Clone();
				fColumns.HasLayoutChanged = true;
			}

			RefreshTableStyles();
		}

		public void AddColumn(ZGridColumnInfo column)
		{
			if (column != null)
			{
				Columns.Add(column);
				DefaultColumns?.AddColumn(Columns[column.ColumnName].Clone());
			}
		}

		public void RemoveAndDisposeColumn(string columnName)
		{
			var columnToRemove = Columns[columnName];
			if (columnToRemove != null)
			{
				Columns.Remove(columnName);
				DefaultColumns?.Remove(columnName);
				columnToRemove.ColumnStyle.Dispose();
			}
		}

		#endregion

		#region Columns and Rows Indexes and Places

		bool IsNextColumnValid(bool forward, int nextColumn)
		{
			return forward ? (nextColumn < TotalColumnsInColumnStyleCollection) : (nextColumn >= 0);
		}

		protected bool IsNextRowValid(bool forward, int nextRow)
		{
			if (List == null)
			{
				return false;
			}

			if (forward && List.AllowNew)
			{
				return (ListManager.Position > -1) && DataGridRowsLength != ListManager.Count;
			}

			return forward ? (nextRow < ListManager.Count) : (nextRow >= 0);
		}

		protected int GetFirstEditableColumnIndex(int currentRow)
		{
			return GetEditableColumnIndex(0, 1, currentRow);
		}

		int GetLastEditableColumnIndex(int currentRow)
		{
			return GetEditableColumnIndex(TotalColumnsInColumnStyleCollection - 1, -1, currentRow);
		}

		int GetEditableColumnIndex(int startIndex, int increment, int currentRow)
		{
			var currentColumn = startIndex;
			var forward = increment > 0;

			while (IsNextColumnValid(forward, currentColumn))
			{
				if (ColumnHasTabStop(currentRow, currentColumn))
				{
					break;
				}
				currentColumn += increment;
			}

			return currentColumn;
		}

		protected void SetCurrentCellToEndOfGrid()
		{
			if (!ReadOnly && ListManager != null)
			{
				var lastEditableRowIndex = List.AllowNew ? ListManager.Count : ListManager.Count - 1;
				var lastEditableColumnIndex = AllColumnsReadOnly ? TotalColumnsInColumnStyleCollection : GetLastEditableColumnIndex(lastEditableRowIndex);
				Focus();
				CurrentCell = new DataGridCell(lastEditableRowIndex, lastEditableColumnIndex);
			}
		}

		protected void SetCurrentCellToFirstNonReadOnlyColumn(bool allowBeginEdit)
		{
			if (!ReadOnly && !IsWholeRowSelectedOnClick)
			{
				try
				{
					if (ListManager.Count > 0 || AllowNew)
					{
						movingCurrentCellWithoutFocus = !allowBeginEdit;
						var firstEditableColumnIndex = AllColumnsReadOnly ? 0 : GetFirstEditableColumnIndex(CurrentCell.RowNumber);

						if (CurrentCell.RowNumber >= ListManager.Count)
						{
							var row = List.AllowNew ? ListManager.Count : ListManager.Count - 1;
							CurrentCell = new DataGridCell(row, CurrentCell.ColumnNumber); // don't change column when changing row when rows greater than listmanager as it causes an invalidate row on an invalid row index.
							CurrentCell = new DataGridCell(row, firstEditableColumnIndex); // now change columns when on a safe row.
						}
						else
						{
							try
							{
								CurrentCell = new DataGridCell(CurrentCell.RowNumber, firstEditableColumnIndex);
							}
							catch (IndexOutOfRangeException)
							{
								try
								{
									FixCreateScrollableRegionException();
									CurrentCell = new DataGridCell(CurrentCell.RowNumber, firstEditableColumnIndex);
								}
								catch (IndexOutOfRangeException ex)
								{
									IndexOutOfRangeExceptionInfo(ex, CurrentCell.RowNumber, CurrentCell.ColumnNumber, CurrentCell.RowNumber, firstEditableColumnIndex);
								}
							}
						}
					}
				}
				catch (IndexOutOfRangeException ex)
				{
					Globals.Message.ShowDeveloperException("IndexOutOfRange exception in Grid <" + Name + ">, Cell <" + CurrentCell.ToString() + ">", ex); // Developer exception
				}
				catch (ArgumentException)
				{
					// sometimes it fails to set CurrentCell and we get ArgumentException with message:"CurrentCell cannot be set at this time. Moving your code to the Form.Load event should solve this problem." 
					// in this case it's better just do not set current cell and don't have Runtime Exception thrown to user. CS00123856.
				}
				finally
				{
					movingCurrentCellWithoutFocus = false;
				}
			}
		}

		[DpiState(DpiState.Unscaled)]
		protected int TotalColumnsInColumnStyleCollection
		{
			get { return (TableStyles.Count > 0) ? TableStyles[0].GridColumnStyles.Count : Columns.Count; }
		}

		#endregion

		#region ReOrderColumns

		bool columnsNeedReordering;

		public void ReOrderColumns(IReadOnlyList<string> columnNamesInSortOrder)
		{
			using (SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ReOrderGridColumnInfos(columnNamesInSortOrder);
				Columns.ReOrderColumns(columnNamesInSortOrder);
				Columns.HasLayoutChanged = true;
				if (DefaultColumns != null)
				{
					DefaultColumns.ReOrderColumns(columnNamesInSortOrder);
				}
				columnsNeedReordering = true;
			}
		}

		void ReOrderGridColumnInfos(IEnumerable<string> columnNamesInSortOrder)
		{
			var newColumnInfoOrder = GetNewColumnInfoOrderForGrid(columnNamesInSortOrder);

			if (newColumnInfoOrder.Length > 0)
			{
				ColumnStyles.Clear();
				ColumnStyles.AddRange(newColumnInfoOrder);
			}
		}

		ZGridColumnInfo[] GetNewColumnInfoOrderForGrid(IEnumerable<string> columnNamesInSortOrder)
		{
			var newColumnOrderList = new List<ZGridColumnInfo>();

			foreach (var columnName in columnNamesInSortOrder)
			{
				var column = GetColumnStyle(columnName);
				if (column != null)
				{
					newColumnOrderList.Add(column);
				}
			}

			foreach (ZGridColumnInfo columnInfo in ColumnStyles)
			{
				if (!newColumnOrderList.Contains(columnInfo))
				{
					newColumnOrderList.Add(columnInfo);
				}
			}
			return newColumnOrderList.ToArray();
		}

		#endregion

		#region Get Column

		public string GetColumnCaption(string columnName)
		{
			var result = "";
			var column = Columns[columnName];
			if (column != null)
			{
				result = column.ColumnStyle.HeaderText;
			}
			else
			{
				var columnInfo = GetColumnStyle(columnName);
				if (columnInfo != null)
				{
					result = columnInfo.Caption;
				}
			}
			return result;
		}

		public int GetColumnWidth(string columnName)
		{
			var result = 0;
			var column = Columns[columnName];
			if (column != null)
			{
				result = column.ColumnStyle.Width;
			}
			else
			{
				var columnInfo = GetColumnStyle(columnName);
				if (columnInfo != null)
				{
					result = columnInfo.Width;
				}
			}
			return result;
		}

		public ZGridColumnInfo GetColumnStyle(string columnName)
		{
			foreach (ZGridColumnInfo info in ColumnStyles)
			{
				if (info.ColumnName == columnName)
				{
					return info;
				}
			}
			return null;
		}

		#endregion

		#region Setting Column Visible

		public void SetAllColumnsVisible(bool isVisible)
		{
			using (SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (DefaultColumns != null)
				{
					SetAllColumnsVisible(DefaultColumns, isVisible);
				}
				SetAllColumnsVisible(Columns, isVisible);
				SetAllColumnStylesVisible(isVisible);
			}
		}

		static void SetAllColumnsVisible(ZGridColumns columns, bool isVisible)
		{
			foreach (var column in columns)
			{
				column.IsVisible = isVisible;
			}
		}

		void SetAllColumnStylesVisible(bool isVisible)
		{
			foreach (ZGridColumnInfo columnInfo in ColumnStyles)
			{
				columnInfo.IsVisible = isVisible;
			}
		}

		public void SetColumnVisible(bool isVisible, IReadOnlyList<string> columnNames)
		{
			using (SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				foreach (var columnName in columnNames)
				{
					SetColumnVisible(isVisible, columnName);
				}
			}
		}

		public void SetColumnVisible(bool isVisible, string columnName)
		{
			if (DefaultColumns != null)
			{
				SetColumnVisible(DefaultColumns, columnName, isVisible);
			}
			var column = SetColumnVisible(Columns, columnName, isVisible);
			var info = GetColumnStyle(columnName);
			if (info != null)
			{
				info.IsVisible = isVisible;
			}

			if (column == null && info == null)
			{
				ErrorReporter.ReportOnce("SetColumnVisible" + columnName, columnName + " is not in the ColumnStyles for the Grid: " + Name);
			}
			else
			{
				RefreshTableStyles();
			}
		}

		static ZGridColumn SetColumnVisible(ZGridColumns columns, string columnName, bool isVisible)
		{
			var column = columns[columnName];
			if (column != null)
			{
				column.IsVisible = isVisible;
			}
			return column;
		}

		#endregion

		#region Setting Column Mandatory

		public void SetColumnMandatory(string columnName, bool isMandatory)
		{
			if (DefaultColumns != null)
			{
				SetColumnMandatory(DefaultColumns, columnName, isMandatory);
			}
			var column = SetColumnMandatory(Columns, columnName, isMandatory);
			var info = GetColumnStyle(columnName);
			if (info != null)
			{
				info.IsMandatory = isMandatory;
				if (isMandatory)
				{
					info.IsVisible = true;
				}
			}

			if (column == null && info == null)
			{
				ErrorReporter.ReportOnce("SetColumnMandatory" + columnName, columnName + " is not in the ColumnStyles for the Grid: " + Name);
			}
			else
			{
				RefreshTableStyles();
			}
		}

		static ZGridColumn SetColumnMandatory(ZGridColumns columns, string columnName, bool isMandatory)
		{
			var column = columns[columnName];
			if (column != null)
			{
				column.IsMandatory = isMandatory;
			}
			return column;
		}

		#endregion

		#region Adding/Removing columns from available columns list

		protected Dictionary<string, ZGridColumn> CachedColums
		{
			get { return fCachedColums ?? (fCachedColums = new Dictionary<string, ZGridColumn>()); }
		}
		Dictionary<string, ZGridColumn> fCachedColums;

		/// <summary>
		/// Add a removed column(s) at run-time which added at design time. Doesnt not add a column that hasn't been added at design time
		/// </summary>
		/// <param name="columnNames"></param>
		public void AddToAvailableColumns(params string[] columnNames)
		{
			using (SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				foreach (var columnName in columnNames)
				{
					AddColumnToAvailableColumns(columnName);
				}
			}
		}

		void AddColumnToAvailableColumns(string columnName)
		{
			var cachedColumn = CachedColums.ContainsKey(columnName) ? CachedColums[columnName] : null;
			var needRefreshing = false;
			if (cachedColumn != null && !Columns.Contains(columnName))
			{
				Columns.AddColumn(cachedColumn);
				CachedColums.Remove(columnName);
				needRefreshing = true;
			}

			var column = Columns[columnName];
			if (column != null && column.IsUnavailable)
			{
				column.IsUnavailable = false;
			}

			var columnInfo = GetColumnStyle(columnName);
			if (columnInfo != null)
			{
				if (columnInfo.IsUnavailable)
				{
					needRefreshing = true;
					columnInfo.IsUnavailable = false;
				}

				if (DefaultColumns != null)
				{
					var defaultColumn = DefaultColumns[columnName];
					if (defaultColumn == null)
					{
						DefaultColumns.Add(columnInfo);
					}
				}
			}
			if (needRefreshing)
			{
				RefreshTableStyles();
			}
		}

		/// <summary>
		/// Remove a column or columns that has been added at design time
		/// </summary>
		/// <param name="columnNames"></param>
		public void RemoveFromAvailableColumns(params string[] columnNames)
		{
			using (SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				foreach (var columnName in columnNames)
				{
					RemoveColumnFromAvailableColumns(columnName);
				}
			}
		}

		void RemoveColumnFromAvailableColumns(string columnName)
		{
			var theColumn = Columns[columnName];
			var needRefreshing = false;
			if (theColumn != null)
			{
				CachedColums[columnName] = theColumn;
				Columns.Remove(columnName);
				theColumn.IsUnavailable = true;
				needRefreshing = true;
			}
			var columnInfo = GetColumnStyle(columnName);
			if (columnInfo != null)
			{
				if (!columnInfo.IsUnavailable)
				{
					needRefreshing = true;
					columnInfo.IsUnavailable = true;
				}

				if (DefaultColumns != null)
				{
					DefaultColumns.Remove(columnName);
				}
			}
			if (needRefreshing)
			{
				RefreshTableStyles();
			}
		}

		/// <summary>
		/// Adds or removes a column to the available columns list
		/// </summary>
		/// <param name="columnName"></param>
		/// <param name="available"></param>
		public void SetAvailability(bool available, string columnName)
		{
			if (available)
			{
				AddToAvailableColumns(columnName);
			}
			else
			{
				RemoveFromAvailableColumns(columnName);
			}
		}

		public void SetAvailability(bool available, IReadOnlyList<string> columnNames)
		{
			using (SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				foreach (var columnName in columnNames)
				{
					SetAvailability(available, columnName);
				}
			}
		}

		public void SetAllAvailability(bool available)
		{
			using (SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				foreach (ZGridColumnInfo column in ColumnStyles)
				{
					SetAvailability(available, column.ColumnName);
				}
			}
		}

		#endregion

		#region Sorting

#if DEBUG

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(false)]
		public bool ShiftBypass { get; set; } = false;

		protected virtual
#endif

		ListSortDescriptionCollection NewSorts(MouseEventArgs e)
		{
			if (!IsDisposed)
			{
				var hit = HitTest(e.Location);
				if (hit.Type == HitTestType.ColumnHeader &&
#if DEBUG
					(ModifierKeys == Keys.Shift || ShiftBypass))
#else
					ModifierKeys == Keys.Shift)
#endif
				{
					var listView = ListManager != null ? ListManager.List as IBindingListView : null;
					if (listView != null && listView.SupportsAdvancedSorting)
					{
						var column = (ZGridColumnStyle)TableStyles[0].GridColumnStyles[hit.Column];
						return AddSort(listView, column.PropertyDescriptor);
					}
				}
			}
			return null;
		}

		internal static ListSortDescriptionCollection AddSort(IBindingListView listView, PropertyDescriptor propertyDescriptor)
		{
			var sortDirection = ListSortDirection.Ascending;
			var sorts = new ArrayList(listView.SortDescriptions);
			for (var i = sorts.Count - 1; i >= 0; --i)
			{
				var sortDescription = (ListSortDescription)sorts[i];
				if (sortDescription.PropertyDescriptor.Name == propertyDescriptor.Name)
				{
					sortDirection = sortDescription.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
					sorts.RemoveAt(i);
					break;
				}
			}

			if (listView is IActiveBusinessObjectCollection)
			{
				sorts.Add(new ListSortDescription(propertyDescriptor, sortDirection));
			}
			else
			{
				sorts.Insert(0, new ListSortDescription(propertyDescriptor, sortDirection));
			}

			return new ListSortDescriptionCollection((ListSortDescription[])sorts.ToArray(typeof(ListSortDescription)));
		}

		#endregion

		#region MouseEvents

		public int GetRow(MouseEventArgs e)
		{
			return HitTest(e.Location).Row;
		}

		public int GetColumn(MouseEventArgs e)
		{
			return HitTest(e.Location).Column;
		}

		#endregion

		#endregion

		#region Table Style

		/// <summary>
		/// Clears the grid table styles and adds all ZGridColumns as table styles again. Use this after changing grid columns.
		/// </summary>
		public void RefreshTableStyles()
		{
			if (!IsRefreshTableStylesSuspended)
			{
				needsRefreshTableStyles = false;
				RefreshTableStylesCore();
			}
			else
			{
				needsRefreshTableStyles = true;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		protected virtual void RefreshTableStylesCore()
		{
			if (TableName != null)
			{
				if (string.IsNullOrWhiteSpace(TableName) && List is IBusinessObjectCollection)
				{
					ErrorReporter.ReportOnce("ZGrid_RefreshTableStyles_EmptyTableName",
						string.Format(CultureInfo.InvariantCulture, "ZGrid {0}{1} bound to {2} has empty TableName.",
							Name, GetParentControlNames(false), List != null ? List.GetType().FullName : string.Empty));
				}

				try
				{
					SuspendLayout();

					var tableStylesChangedHandler = (CollectionChangeEventHandler)typeof(DataGrid).GetField("dataGridTableStylesCollectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
					TableStyles.CollectionChanged -= tableStylesChangedHandler;
					TableStyles.CollectionChanged -= TableStyles_CollectionChanged;

					var oldTableStyle = TableStyles.Count == 0 ? null : TableStyles[0];
					if (oldTableStyle != null)
					{
						oldTableStyle.GridColumnStyles.CollectionChanged -= GridColumnStyles_CollectionChanged;
						oldTableStyle.GridColumnStyles.Clear();
						try
						{
							oldTableStyle.MappingName = "";
						}
						catch (ArgumentException)
						{
							// Ignore as we are dropping OldTableStyle.
						}
					}

					var tableStyle = new DataGridTableStyle
					{
						AllowSorting = AllowSorting,
						HeaderBackColor = HeaderBackColor,
						RowHeaderWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(35)
					};

					var colIndex = 0;
					foreach (var col in Columns)
					{
						if (col.IsVisible)
						{
							var columnStyle = col.ColumnStyle;
							if (columnStyle.Width <= 0)
							{
								// Reset to default width 100
								ControlDpiScalingHelper.SetWidth(ref columnStyle, 100, true);
							}
							tableStyle.GridColumnStyles.Add(columnStyle);
							if (columnStyle is ZCheckBoxColumnStyle buttonColumn)
							{
								buttonColumn.HookMouseHandler(this);
								buttonColumn.ColumnNumber = colIndex;
							}

							colIndex++;
						}
						else
						{
							((ZGridColumnStyle)col.ColumnStyle).SetParentGrid(this);
						}
					}

					TableStyles.Clear();
					TableStyles.Add(tableStyle);
					TableStyles.CollectionChanged += tableStylesChangedHandler;
					TableStyles.CollectionChanged += TableStyles_CollectionChanged;
					tableStyle.GridColumnStyles.CollectionChanged += GridColumnStyles_CollectionChanged;

					var oldMappingName = tableStyle.MappingName;

					try
					{
						tableStyle.MappingName = TableName;
					}
					catch (ArgumentException ex) when (ex.Message?.Contains("Data grid column styles collection already contains a column style with the same mapping name.", StringComparison.Ordinal) ?? false)
					{
#if !WINZOR
						var userEvents = UserEventTracker.UserEventList;

						if (!userEvents.Any(e => e.EventName == "MouseDown" && e.EventData == "Left" && e.ControlName == "DialogDefaultForm.ExitConfirmationControl.ExitButton"
						&& e.EventReference == "Caption = \"Exit or Home\""))
						{
							var message = string.Format($"A table style for grid {0}{1} bound to {2} threw an exception when attempting to change tableStyle.MappingName from {3} to {4}"
								, Name, GetParentControlNames(false), List != null ? List.GetType().FullName : String.Empty, oldMappingName, TableName);
							ErrorReporter.ReportOnce("ZGrid.RefreshTableStyles_DuplicateMappingName", message, ex);
						}
#else
						throw;
#endif
					}
				}
				finally
				{
					ResumeLayout();
				}
			}

			TableColumns = null;
		}

		protected static string GetMappingName(object src)
		{
			IList list;
			Type type;

			if (src is Array)
			{
				type = src.GetType();
				list = src as IList;
			}
			else
			{
				if (src is IListSource)
				{
					src = (src as IListSource).GetList();
				}

				if (src is IList)
				{
					type = src.GetType();
					list = src as IList;
				}
				else
				{
					throw new ArgumentException("Invalid Data Source");
				}
			}

			return (list is ITypedList) ? ((ITypedList)list).GetListName(null) : type.Name;
		}

		protected virtual void ClearTableStyle()
		{
			try
			{
				SuspendLayout();
				TableStyles.Clear();

				foreach (var col in Columns)
				{
					if (col.ColumnStyle is ZCheckBoxColumnStyle buttonColumn)
					{
						buttonColumn.UnHookMouseHandler(this);
						buttonColumn.ColumnNumber = -1;
					}
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		#endregion

		#region Table Name

		/// <summary>
		/// Extracts the table name from a bound grid control.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Table name")]
		protected static string GetTableFromControlName(string controlName) //TODO: move this functionality to a Utilities class once the new architecture is in place.
		{
			var controlTableName = GetTableNameFromSubstring("Bound", controlName);
			if (string.IsNullOrEmpty(controlTableName))
			{
				controlTableName = GetTableNameFromSubstring("Grid", controlName);
			}

			return controlTableName;
		}

		static string GetTableNameFromSubstring(string key, string controlName)
		{
			var start = controlName.IndexOf(key);
			return (start != -1) ? controlName.Substring(0, start) : "";
		}

		#endregion

		#region Saving / Loading Layout

		protected override void OnLayout(LayoutEventArgs e)
		{
			var initialCanFocus = AllowFocus;
			((IDataGridToolTipSuspender)this).SuspendToolTip();

			try
			{
				AllowFocus = false;
				base.OnLayout(e);
			}
			finally
			{
				((IDataGridToolTipSuspender)this).ResumeToolTip();
				AllowFocus = initialCanFocus;
			}
		}

		public virtual void SaveUserLayoutSettings()
		{
			if (Columns.HasLayoutChanged && (SaveLastSelectedLayout || CurrentColumnLayout == null || CurrentColumnLayout.IsDeleted || StmDataGridLayoutStorage.IsDefaultLayout(CurrentColumnLayout.ColumnLayoutName)))
			{
				SaveLastSelectedLayout = false;
				SaveLayoutEvenIfColumnsAreUnchanged();
			}
		}

		internal void SaveLayoutEvenIfColumnsAreUnchanged()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("SaveGridLayout", Name))
			{
				if (isBound)
				{
					GridLayoutManager.SaveDefaultLayout(this);

					Columns.HasLayoutChanged = false;
				}
#if DEBUG
				else
				{
					throw new Exception("Can't save layout as grid has never been bound.");
				}
#endif
			}
		}

		public void LoadUserLayoutSettings()
		{
			if (DataSource != null)
			{
				GridLayoutManager.LoadUserLayoutSettings(this);
				UpdateFrozenSort();
			}
		}

		#region ParentControls

		internal string GetParentControlNames(bool includeLayoutControls)
		{
			if (!parentControlNames.TryGetValue(includeLayoutControls, out var names))
			{
				var result = new StringBuilder();

				var parentContainer = Parent;
				while (parentContainer != null)
				{
					if (includeLayoutControls || !layoutControlsToIgnore.Contains(parentContainer.GetType()))
					{
						result.Append(".");
						result.Append(parentContainer.Name);
					}

					parentContainer = parentContainer.Parent;
				}

				names = result.ToString();
				parentControlNames[includeLayoutControls] = names;
			}

			return names;
		}

		readonly Dictionary<bool, string> parentControlNames = new Dictionary<bool, string>();
		static readonly Type[] layoutControlsToIgnore = { typeof(TableLayoutPanel) };

		#endregion

		static MethodInfo UnWireDataSourceMethodInfo
		{
			get
			{
				// UnWireDataSource is called directly to cause the grid to unhook events from the ListManager.
				// Calling SetDataBindings(null, null) instead causes TableStyles to be recreated, which is a performance problem.
				if (unWireDataSourceMethodInfo == null)
				{
					unWireDataSourceMethodInfo = typeof(DataGrid).GetMethod("UnWireDataSource", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return unWireDataSourceMethodInfo;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static MethodInfo unWireDataSourceMethodInfo;

		CurrencyManager ListManagerBase
		{
			get
			{
				CurrencyManager result = null;

				if (ListManagerBaseInfo != null)
				{
					result = ListManagerBaseInfo.GetValue(this) as CurrencyManager;
				}
				return result;
			}
		}

		FieldInfo ListManagerBaseInfo
		{
			get
			{
				if (listManagerBaseInfo == null)
				{
					listManagerBaseInfo = typeof(DataGrid).GetField("listManager", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return listManagerBaseInfo;
			}
		}
		FieldInfo listManagerBaseInfo;

		#endregion

		#region Row Delete

		public event EventHandler<RowsDeletingEventArgs> RowsDeleting;

		protected virtual void OnRowsDeleting(RowsDeletingEventArgs e)
		{
			RowsDeleting?.Invoke(this, e);
		}

		public event EventHandler<RowsDeletingEventArgs> RowsDeleted;

		protected virtual void AfterRowsDeleted(RowsDeletingEventArgs e)
		{
			RowsDeleted?.Invoke(this, e);
		}

		/// <summary>
		/// Grid method to handle a delete driven by the UI.
		/// </summary>
		/// <param name="clickedRow">The current row when the Delete was raised.</param>
		/// <returns>How many rows were deleted.</returns>
		protected virtual int HandleDelete(int clickedRow)
		{
			var deletedCount = 0;
			var indices = GetSelectedRowsIndices(clickedRow);
			var objects = GetBusinessObjects(indices);
			var args = new RowsDeletingEventArgs(objects);
			var cannotDeleteReasons = new List<Tuple<MultilingualString, string>>();
			OnRowsDeleting(args);
			var warningForDelete = new List<string>();
			objects.ForEach(o =>
			{
				if (o.CanDelete)
				{
					var warning = o.GetWarningBeforeBeingDeleted().Trim();
					if (!string.IsNullOrEmpty(warning) && !warningForDelete.Contains(warning))
					{
						warningForDelete.Add(warning);
					}
				}
			});

			if (warningForDelete.Count > 0 && Globals.Message.Show(new ZStringBuilder(warningForDelete).ToStringWithNewLineBetweenAppends() + System.Environment.NewLine + Res.GetString("0d6a1a36-1a55-4e5a-8483-dda9a5414f25", "Do you wish to continue?"), Res.GetString("f0f5c3fb-d581-4465-b37b-946b9d1bcd8a", "Warning"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
			{
				args.Cancel = true;
			}

			if (!args.Cancel)
			{
				if (List is IBusinessObjectCollection collection)
				{
					if (collection.AllowRemove)
					{
						using (collection.SuspendListChanged())
						{
							try
							{
								foreach (var businessObject in objects)
								{
									if (RemoveObjectFromCollection(collection, businessObject, cannotDeleteReasons))
									{
										++deletedCount;
									}
								}
							}
							catch (CannotDeleteException ex)
							{
								cannotDeleteReasons.Add(Tuple.Create((MultilingualString)((NoResString)ex.Message), Res.GetString("c5382f89-1016-436a-9fd1-cf01b80bf7ed", "Cannot delete this item")));
							}

							if (collection is IActiveBusinessObjectCollection activeCollection && indices.Count > 1)
							{
								// Several row deleted event could lead to accessing deleted objects till all events are processed.
								// Replace them with 1 reset event.
								activeCollection.FireListResetEvent();
							}
						}
					}
				}
				else
				{
					foreach (var index in indices)
					{
						ListManager?.RemoveAt(index);
						++deletedCount;
					}
				}
			}
			AfterRowsDeleted(args);
			foreach (var error in cannotDeleteReasons)
			{
				Globals.Message.ShowError(error.Item1, error.Item2);
			}
			return deletedCount;
		}

		//return true if a deletion was made
		bool RemoveObjectFromCollection(IBusinessObjectCollection collection, BusinessObject businessObject, List<Tuple<MultilingualString, string>> cannotDeleteReasons)
		{
			if (!businessObject.IsDeleted)
			{
				switch (RemoveAction)
				{
					case RemoveAction.Remove:
						if (OnRemovingBizOFromList == null ||
							OnRemovingBizOFromList(businessObject) == ContinueWithRemove.Remove)
						{
							OnDeleted(new GridRowOnDeletedEventArgs(new[] { businessObject }));
							collection.RemoveFromRelationship(businessObject);
							return true;
						}
						break;
					case RemoveAction.RemoveAndDelete:
						var deletableTarget = (ICanDelete)businessObject;
						if (!CanDeleteCurrentRow(collection.IndexOf(businessObject)))
						{
							return AddCannotDeleteReasons(cannotDeleteReasons, deletableTarget, ResString.GetMultilingualString("68E5837A-D1CC-49D1-A008-3E695D2C2329", "Cannot delete read-only rows."));
						}
						else if (deletableTarget.CanDelete)
						{
							if (OnRemovingBizOFromList == null ||
								OnRemovingBizOFromList(businessObject) == ContinueWithRemove.Remove)
							{
								OnDeleted(new GridRowOnDeletedEventArgs(new[] { businessObject }));
								collection.Delete(businessObject);
								return true;
							}
						}
						else
						{
							return AddCannotDeleteReasons(cannotDeleteReasons, deletableTarget, deletableTarget.ReasonForNotAbleToDelete);
						}
						break;
				}
			}
			return false;
		}

		static bool AddCannotDeleteReasons(List<Tuple<MultilingualString, string>> cannotDeleteReasons, ICanDelete deletableTarget, MultilingualString reason)
		{
			var reasonForNotAbleToDelete = Tuple.Create(reason, Res.GetString("de7a398d-1748-4f1c-ac91-9f09a1cee09c", "Cannot Delete Record"));
			if (!cannotDeleteReasons.Contains(reasonForNotAbleToDelete))
			{
				cannotDeleteReasons.Add(reasonForNotAbleToDelete);
			}
			deletableTarget.OnCannotDelete();
			return false;
		}

		public event EventHandler<GridRowOnDeletedEventArgs> Deleted;
		void OnDeleted(GridRowOnDeletedEventArgs eventArgs)
		{
			Deleted?.Invoke(this, eventArgs);
		}

		IEnumerable<BusinessObject> GetBusinessObjects(List<int> indices)
		{
			List<BusinessObject> result = null;
			if (List is IBusinessObjectCollection)
			{
				result = new List<BusinessObject>(indices.Count);
				foreach (var index in indices)
				{
					result.Add((BusinessObject)ListManager.List[index]);
				}
			}
			return result ?? Enumerable.Empty<BusinessObject>();
		}

		List<int> GetSelectedRowsIndices(int currentRow)
		{
			var result = new List<int>();
			for (var index = DataGridRowsLength - 1; index >= 0; index--)
			{
				if (IsSelected(index) && index < ListManager.Count)
				{
					result.Add(index);
				}
			}
			if (result.Count == 0 &&
				currentRow >= 0 &&
				currentRow < DataGridRowsLength &&
				currentRow < ListManager.Count)
			{
				result.Add(currentRow);
			}
			return result;
		}

		public enum ContinueWithRemove
		{
			Remove,
			CancelRemoval
		}

		/// <summary>
		/// Determines the action taken by the ZGrid in the BusinessObjectCollection for a GUI driven delete.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(RemoveAction.RemoveAndDelete)]
		[Description("Determines the action taken by the ZGrid in the BusinessObjectCollection for a GUI driven delete.")]
		public RemoveAction RemoveAction
		{
			get { return removeAction; }
			set
			{
				if (removeAction != value && DeleteMenuItem != null)
				{
					((ZMenuItem)DeleteMenuItem).Caption = GetDeleteMenuItemText(value);
				}

				removeAction = value;
				OnRemoveActionChanged();
			}
		}
		RemoveAction removeAction;

		/// <summary>
		/// Gets or sets the value of the AllowDelete property in the current bound List.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Please set RemoveAction instead. This property has never been implemented for ZGrids.")]
		public virtual bool AllowDelete
		{
			get { return removeAction != RemoveAction.NoRemovePossible; }
		}

		protected virtual void OnRemoveActionChanged()
		{
			RemoveActionChanged?.Invoke(this, EventArgs.Empty);
		}
		public event EventHandler RemoveActionChanged;

		public delegate ContinueWithRemove RemoveBizOFromListHandler(BusinessObject bizOToRemoveOrDelete);
		public RemoveBizOFromListHandler OnRemovingBizOFromList;

		#endregion

		#region Notifications

		protected bool DoesGridHaveTopLeftNotificationIconShowing;

		protected
#if DEBUG
		internal
#endif
		virtual void NavigateToFirstCellWithHighestPriorityNotification()
		{
			var row = GetFirstBusinessObjectRowWithINotificationType(notificationType);

			if (row != -1)
			{
				HideBalloon();
				NotificationRectForBalloon = TopLeftCornerNotificationRectangle; // so we don't show the balloon again until we move mouse off and on again

				var current = ListForNotifications[row] as BusinessObject;

				var isErrorInGrid = false;
				var columnInError = AllColumnsReadOnly ? 0 : GetFirstEditableColumnIndex(row);
				if (columnInError >= TableStyles[0].GridColumnStyles.Count)
				{
					columnInError = 0;
				}

				foreach (ZGridColumnStyle columnStyle in TableStyles[0].GridColumnStyles)
				{
					var info = ZPropertyInfoRetriever.GetZPropertyInfo(columnStyle, current);
					if (info != null && info.GetHighestSeverityNotificationType() == notificationType)
					{
						columnInError = TableStyles[0].GridColumnStyles.IndexOf(columnStyle);
						isErrorInGrid = true;
						break;
					}
				}

				if (!isErrorInGrid)
				{
					var insertIndex = 0;

					for (var index = 0; index < Columns.Count; ++index)
					{
						var column = Columns[index];

						if (column.IsVisible)
						{
							insertIndex = index + 1;
						}

						var info = ZPropertyInfoRetriever.GetZPropertyInfo(column.ColumnStyle, current);
						if (info != null && info.GetHighestSeverityNotificationType() == notificationType)
						{
							column.IsVisible = true;
							Columns.Move(column, insertIndex);
							RefreshTableStyles();

							columnInError = TableStyles[0].GridColumnStyles.IndexOf(column.ColumnStyle);
							isErrorInGrid = true;

							break;
						}
					}
				}

				if (isErrorInGrid)
				{
					NavigateToCell(row, columnInError);
				}
				else
				{
					if (row >= 0 && row <= ListManager.List.Count)
					{
						ListManager.Position = row;
					}
					Globals.Message.ShowInformation(Res.GetString("34eb16f3-8546-4ac6-a11e-5a61cd82bc95", "The {0} is not in a cell in this grid. Have a look at other fields or grids on the form which display further information about the currently selected row in this grid.", notificationType.NotificationTypeName(CargoWise.ResourceStrings.Grammar.PluralState.NonPlural, CargoWise.ResourceStrings.Grammar.Capitalisation.LowerCase)));
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1040", Justification = "Usually Scrollbar.Value is unscaled, but in this case it seems that it has been set to be pixel values")]
		void NavigateToCell(int row, int col)
		{
			CurrentCell = new DataGridCell(row, col);

#if !WINZOR
			var columnEnd = (int)typeof(DataGrid).GetMethod("GetColEnd", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { col });
			if (columnEnd > Width)
			{
				GridHScrolled(this, new ScrollEventArgs(ScrollEventType.LargeIncrement, HorizScrollBar.Value + columnEnd - Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(2)));
				GridHScrolled(this, new ScrollEventArgs(ScrollEventType.EndScroll, HorizScrollBar.Value));
			}
#endif

			BeginEdit(LastFocusedColumn, row);
		}

		protected void ValidateIfNotNewRow()
		{
			if (ListManager != null && ListManager.Position != -1)
			{
				if (IsCellEdited)
				{
					OnValidated(new EventArgs());
				}
			}
		}

		protected void UpdateNonCellNotifications(BusinessObject businessObjectWithChange)
		{
			var graphics = IsHandleCreated & !IsDisposed ? CreateGraphics() : null;
			try
			{
				if (suspendCellNotificationCount == 0)
				{
					UpdateGridNotificationType(graphics);
				}

#if WINZOR
				UpdateGridNotificationIcon(notificationType);
#endif
				UpdateRowHeaderNotifications(businessObjectWithChange, graphics);
			}
			finally
			{
				if (graphics != null)
				{
					graphics.Dispose();
				}
			}
			IsUpdateNonCellNotificationsQueued = false;

#if DEBUG
			UpdateNonCellNotificationsCountForTesting++;
#endif
		}
#if DEBUG
		internal
#endif
		INotificationType notificationType;

#if DEBUG
		internal
#endif
		int suspendCellNotificationCount;

		internal void SuspendCellNotifications()
		{
			suspendCellNotificationCount++;
		}

		internal void ResumeCellNotifications()
		{
			suspendCellNotificationCount--;
#if DEBUG
			if (suspendCellNotificationCount < 0)
			{
				ErrorReporter.ReportOnce("suspendCellNotificationCount", "suspendCellNotificationCount become < 0");
			}
#endif
			if (suspendCellNotificationCount == 0)  // this will happen every time resume occurs (previously just on change)
			{
				var graphics = IsHandleCreated ? CreateGraphics() : null;
				try
				{
					UpdateGridNotificationType(graphics);
				}
				finally
				{
					if (graphics != null)
					{
						graphics.Dispose();
					}
				}
			}
		}

#if DEBUG

		public static IDisposable TrackUpdateGridNotificationType()
		{
			GridListTracked = new List<int>();

			return new DisposableAction(() => GridListTracked = null);
		}

		[ThreadStatic]
		static List<int> GridListTracked;

#endif

		protected virtual void UpdateGridNotificationType(Graphics graphics)
		{
			var newState = GetNotificationTypeForList();
			if (notificationType != newState)
			{
				notificationType = newState;
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(newState, this);
				if (graphics != null)
				{
					DrawGridNotification(graphics);
				}
			}
#if DEBUG
			UpdateGridNotificationTypeCountForTesting++;

			if (GridListTracked != null)
			{
				var hashCode = GetHashCode();
				if (GridListTracked.Contains(hashCode))
				{
					throw new InvalidOperationException($"{NameForDebugging} Should not do UpdateGridNotificationType again.");
				}
				else
				{
					GridListTracked.Add(hashCode);
				}
			}
#endif
		}

		void UpdateRowHeaderNotifications(BusinessObject businessObjectWithChange, Graphics graphics)
		{
			if (ForceDrawRowHeaderNotificationsOnNextPaint || businessObjectWithChange == null || IsBusinessObjectVisible(businessObjectWithChange))
			{
				if (graphics != null)
				{
					DrawRowHeaderNotification(graphics, true);
				}
				ForceDrawRowHeaderNotificationsOnNextPaint = false;
			}
		}
		bool ForceDrawRowHeaderNotificationsOnNextPaint;

#if DEBUG
		internal
#endif
		bool IsBusinessObjectVisible(BusinessObject bo)
		{
			var result = false;
			if (ListForNotifications != null)
			{
				var firstVisibleRow = GetFirstVisibleRow();
				if (firstVisibleRow >= 0 && firstVisibleRow + VisibleRowCount <= ListForNotifications.Count)
				{
					result = ListForNotifications.IndexOf(bo, firstVisibleRow, VisibleRowCount) != -1;
				}
			}
			return result;
		}

		INotificationType GetNotificationTypeForList()
		{
			INotificationType result = null;

			if (ListManager != null && ListManager.List != null)
			{
				foreach (BusinessObject bizO in ListManager.List)
				{
					var current = bizO.GetHighestSeverityNotificationType();
					if (current != null && (result == null || current.Severity < result.Severity))
					{
						result = current;
					}
					if (result != null && result.IsFatal)
					{
						break;
					}
				}
			}

			return result;
		}

		void HideBalloon(MouseEventArgs e = null)
		{
			if ((e == null || !NotificationRectForBalloon.Contains(ControlDpiScalingHelper.NewScaledPoint(e.X, e.Y, false))) && NotificationRectForBalloon != Rectangle.Empty)
			{
				Balloon.Instance.Hide();
				NotificationRectForBalloon = Rectangle.Empty;
			}
		}

		IEnumerable<INotification> GetNotifications(IBusiness businessEntity)
		{
			return new ZNotificationCollector(businessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		void ShowGridCornerBalloon()
		{
			if (ShouldShowNotifications && NotificationRectForBalloon.IsEmpty && notificationType != null)
			{
				NotificationRectForBalloon = TopLeftCornerNotificationRectangle;
				var descriptor = new BalloonDescriptor(
#if WINZOR
					NotificationIcon,
#else
					this,
#endif
					TopLeftCornerNotificationRectangle, Res.GetString("96eb11ed-fa89-45c4-a513-4e3add3bccad", "There are {0} in the grid.", notificationType.NotificationTypeName(CargoWise.ResourceStrings.Grammar.PluralState.Plural, CargoWise.ResourceStrings.Grammar.Capitalisation.LowerCase)), Res.GetString("88733a89-7b27-4be9-be9d-906d8d82e352", "Click on the icon to navigate to the first {0}.", notificationType.NotificationTypeName(CargoWise.ResourceStrings.Grammar.PluralState.NonPlural, CargoWise.ResourceStrings.Grammar.Capitalisation.LowerCase)), GetNotifications((IBusiness)List).GetUniqueNotifications());
				Balloon.Instance.Show(descriptor);
			}
		}

		void ShowGridColumnBalloon(IBusiness bO, [DpiState(DpiState.ScaledVariant)] Rectangle notificationRect, Control anchor)
		{
			if (NotificationRectForBalloon.IsEmpty)
			{
				NotificationRectForBalloon = notificationRect;
				var notifications = GetNotifications(bO);
				var notificationType = notifications.GetHighestSeverityNotificationType();
				if (notificationType != null)
				{
					var descriptor = new BalloonDescriptor(anchor, notificationRect, Res.GetString("b3a04ced-335b-458d-a883-da0d1e6185e9", "There are {0} on this row.", notifications.GetHighestSeverityNotificationType().NotificationTypeName(CargoWise.ResourceStrings.Grammar.PluralState.Plural, CargoWise.ResourceStrings.Grammar.Capitalisation.LowerCase)), Res.GetString("29e9052e-0d6a-46c6-8476-e20ff778d924", "These {0} may be on another grid or fields that shows further information about this row.", notifications.GetHighestSeverityNotificationType().NotificationTypeName(CargoWise.ResourceStrings.Grammar.PluralState.Plural, CargoWise.ResourceStrings.Grammar.Capitalisation.LowerCase)), notifications.GetUniqueNotifications());
					Balloon.Instance.Show(descriptor);
				}
			}
		}
#if DEBUG
		internal
#endif
		Rectangle NotificationRectForBalloon = Rectangle.Empty;

		int GetFirstBusinessObjectRowWithINotificationType(INotificationType state)
		{
			if (ListForNotifications != null)
			{
				for (var index = 0; index < ListForNotifications.Count; index++)
				{
					var current = ListForNotifications[index] as BusinessObject;
					if (current != null && current.GetHighestSeverityNotificationType() == state)
					{
						return index;
					}
				}
			}
			return -1;
		}

		#endregion

		#region Tab Stop

		internal void SetTabStop()
		{
			if (AllowReadOnlyToModifyTabStop)
			{
				TabStop = !AllColumnsReadOnly;
			}
		}

		protected virtual bool ColumnHasTabStop(int row, int column)
		{
			return !IsCellReadOnly(row, column);
		}

		bool ProcessTabKeyWithReadOnlySkipping(bool forward)
		{
			var handled = false;

			if (!AllColumnsReadOnly)
			{
				var currentRow = CurrentCell.RowNumber;
				var currentColumn = CurrentCell.ColumnNumber;
				var oldCurrentRow = currentRow;
				var oldCurrentColumn = currentColumn;
				var columnFound = false;

				if (TableStyles.Count > 0)
				{
					if (TableStyles[0].GridColumnStyles[CurrentCell.ColumnNumber] is ZTextBoxColumnStyle currentColumnStyle && !currentColumnStyle.EditControl.Visible && !IsCellReadOnly(currentRow, currentColumn))
					{
						columnFound = true;
						BeginEdit(currentColumnStyle, CurrentCell.RowNumber);
					}
					else
					{
						EndEdit(LastFocusedColumn, CurrentCell.RowNumber, false);
						if (ListManager != null && ListManager.Position > -1)
						{
							columnFound = FindNextCellToTabInto(forward, ref currentRow, ref currentColumn);
						}
						else
						{
							currentRow = 0;
							currentColumn = GetFirstEditableColumnIndex(currentRow);
							columnFound = true;
						}
					}
				}

				if (columnFound)
				{
					ResetSelection();

					try
					{
						CurrentCell = new DataGridCell(currentRow, currentColumn);
					}
					catch (IndexOutOfRangeException)
					{
						try
						{
							FixCreateScrollableRegionException();
							CurrentCell = new DataGridCell(currentRow, currentColumn);
						}
						catch (IndexOutOfRangeException ex)
						{
							IndexOutOfRangeExceptionInfo(ex, oldCurrentRow, oldCurrentColumn, currentRow, currentColumn);
						}
					}

					BeginEdit(TableStyles[0].GridColumnStyles[CurrentCell.ColumnNumber], CurrentCell.RowNumber);
					handled = true;
				}
				else
				{
					TabOutOfGrid(forward);
					handled = true;
				}
			}

			return handled;
		}

		internal void FixCreateScrollableRegionException()
		{
			var numTotallyVisibleRows = (int)typeof(DataGrid).GetField("numTotallyVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			var numVisibleRows = (int)typeof(DataGrid).GetField("numVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			var dataGridRows = (object[])typeof(DataGrid).GetField("dataGridRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			var dataGridRowsCount = dataGridRows?.Length ?? DataGridRowsLength;
			if (numTotallyVisibleRows > dataGridRowsCount)
			{
				typeof(DataGrid).GetField("numTotallyVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, dataGridRowsCount);
			}
			if (numVisibleRows > dataGridRowsCount)
			{
				typeof(DataGrid).GetField("numVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, dataGridRowsCount);
			}
			if (numTotallyVisibleRows < 0)
			{
				typeof(DataGrid).GetField("numTotallyVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, 0);
			}
			if (numVisibleRows < 0)
			{
				typeof(DataGrid).GetField("numVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, 0);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error report, Developer exception, Error report for Issues 01096292, 01107014, and 01147425")]
		internal void IndexOutOfRangeExceptionInfo(IndexOutOfRangeException ex, int oldRow, int oldCol, int currentRow, int currentCol)
		{
			var firstVisibleRow = (int)typeof(DataGrid).GetField("firstVisibleRow", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			var firstVisibleCol = (int)typeof(DataGrid).GetField("firstVisibleCol", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

			var numTotallyVisibleRows = (int)typeof(DataGrid).GetField("numTotallyVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			var lastTotallyVisibleCol = (int)typeof(DataGrid).GetField("lastTotallyVisibleCol", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

			var numVisibleRows = (int)typeof(DataGrid).GetField("numVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			var numVisibleCols = (int)typeof(DataGrid).GetField("numVisibleCols", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			var dataGridRows = (object[])typeof(DataGrid).GetField("dataGridRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

			var message = string.Format(CultureInfo.InvariantCulture,
				" ControlPath: {0}; oldCurrentRow: {1}; oldCurrentColumn: {2}; currentRow: {3}; currentColumn: {4}; RowsCount: {5}; ColumnsCount: {6}; FirstVisibleRow: {7}; FirstVisibleColumn: {8}; TotallyVisibleRowsCount: {9}; lastTotallyVisibleColumn: {10}; VisibleRowsCount: {11}; VisibleColumnsCount: {12}; DataGridRows: {13}; ",
				ControlDescription.GetControlPath(this), oldRow, oldCol, currentRow, currentCol, DataGridRowsLength, Columns.Count, firstVisibleRow, firstVisibleCol, numTotallyVisibleRows, lastTotallyVisibleCol, numVisibleRows, numVisibleCols, dataGridRows?.Length);
			Globals.Message.ShowDeveloperException("IndexOutOfRange exception in Grid <" + Name + ">, Cell <" + CurrentCell.ToString() + ">", ex);
			ErrorReporter.ReportOnce("ZGrid_IndexOutOfRangeException", ex.Message + message, ex);
		}

		void TabOutOfGrid(bool forward)
		{
			var containerControl = GetContainerControl();
			if (containerControl != null)
			{
				((ContainerControl)containerControl).SelectNextControlNonTabStopNonReadOnly(containerControl.ActiveControl, forward, true, true);
				var activeControl = containerControl.ActiveControl;
				if (activeControl == this || Contains(activeControl))
				{
					if (Contains(activeControl))
					{
						activeControl.Visible = false;
					}

					var form = FindForm();
					if (form != null)
					{
						form.SelectNextControlNonTabStopNonReadOnly(containerControl.ActiveControl, forward, true, true);
					}
				}
			}
		}

		protected bool FindNextCellToTabInto(bool forward, ref int currentRow, ref int currentColumn)
		{
			var increment = forward ? 1 : -1;
			var columnFound = false;

			while (true)
			{
				var validIndex = ((currentColumn + increment) >= 0) && ((currentColumn + increment) < TableStyles[0].GridColumnStyles.Count);
				if (validIndex && ColumnHasTabStop(currentRow, currentColumn + increment))
				{
					columnFound = true;
					currentColumn += increment;
					break;
				}

				if (IsNextColumnValid(forward, currentColumn + increment))
				{
					currentColumn += increment;
				}
				else if (IsNextRowValid(forward, currentRow + increment))
				{
					currentRow += increment;
					currentColumn = forward ? GetFirstEditableColumnIndex(currentRow) : GetLastEditableColumnIndex(currentRow);
					columnFound = true;
					break;
				}
				else
				{
					break;
				}
			}

			return columnFound;
		}

		#endregion

		#region DragDrop

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log message, exception message, Developer exception")]
		protected virtual void DoDragDrop()
		{
#if WINZOR
			StringBuilder logBuilder = null;
#else
			var trackingInfoForm = GetDragDropTrackingInfoForm();
			var logBuilder = TrackingInfoLogger.Instance.HasListener ? new StringBuilder() : null;
#endif
			try
			{
				if (CopySelectedRowsAllowed && IsValidSource(ListManager) && AllowBeginDrag && (AllowDragDropWithChanges || !IsDataSourceChanged))
				{
					Log(logBuilder, "##################################");
					Log(logBuilder, FormattableString.Invariant($"Drag-Drop to local starts. {SelectedElements.Length} rows selected."));
					var dataObject = GetDataObject(SelectedElements);
					if (dataObject.Elements.Count > 0)
					{
						Log(logBuilder, FormattableString.Invariant($"{dataObject.Elements.Count} dataobjects found."));
						var result = DragDropEffects.None;

						IsDragging = true;
						try
						{
#if !WINZOR
							if (ShouldDragDropToLocal(logBuilder))
							{
								Log(logBuilder, "Is remote session and drag to local is supported.");
								Log(logBuilder, "Creating DRTL message.");
								var selectedItems = GetSelectedItems(dataObject);
								if (selectedItems != null)
								{
									Log(logBuilder, "DRTL message created.");
									EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.DragToLocal, selectedItems);
									Log(logBuilder, "DRTL message sent.");
								}
							}
#endif
							Log(logBuilder, "Local DoDragDrop starts.");
							result = DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Move);
							Log(logBuilder, "Local DoDragDrop completed.");
						}
						finally
						{
							IsDragging = false;
						}

						OnDragDropCompleted(new DragDropCompletedEventArgs(result, dataObject));
					}
				}
			}
			catch (Exception ex)
			{
				Log(logBuilder, $"Exception thrown: {ex.Message}.\r\n\tStackTrace: {ex.StackTrace}");
				if (ex.IsCriticalException())
				{
					throw;
				}
				else if (ex.IsIOErrorHandleInvalidException())
				{
					Globals.Message.ShowWarning(Res.GetString("D819171C-A801-4A3E-B284-535809F94785", "There is an issue with uploading file. Please close all opening windows and reset the current application and try again."));
				}
				else if (ex.IsOutOfDiskSpaceException())
				{
					Globals.Message.ShowWarning(Res.GetString("78387f1c-dd34-40d2-b8a7-ef36123976f5", "There is not enough space on the disk or you have exceeded your quota. Please contact your system administrator."));
				}
				else if (ex.IsErrorIODeviceException())
				{
					Globals.Message.ShowWarning(Res.GetString("7467dd5B-7b3b-45fe-9af6-818e808bdd70", "The request could not be performed because of an I/O device error."));
				}
				else if (ex is UnauthorizedAccessException || ex is DirectoryNotFoundException)
				{
					Globals.Message.ShowWarning(ex.Message + System.Environment.NewLine + Res.GetString("47b3ecf9-4788-49b1-a49c-114405f3bccd", "Please contact your system administrator."));
				}
				else
				{
					Globals.Message.ShowDeveloperException("DragDrop Failed", ex);
				}
			}
			finally
			{
#if !WINZOR

				if (trackingInfoForm != null)
				{
					if (!trackingInfoForm.Visible)
					{
						trackingInfoForm.Show();
						TrackingInfoLogger.Instance.NewLog(() => logBuilder?.ToString());
					}
				}
#endif
			}
		}

		void Log(StringBuilder logBuilder, string text)
		{
			logBuilder?.AppendLine(text);
		}
#if !WINZOR
		protected virtual bool ShouldDragDropToLocal(StringBuilder builder)
		{
			var isRemoteAppSession = ObjectFactory.Get<TerminalService>().IsRemoteAppSession;
			Log(builder, FormattableString.Invariant($"IsRemoteAppSession: {isRemoteAppSession}, EnterpriseChannel Connected: {EnterpriseChannel.Instance != null && EnterpriseChannel.Instance.IsConnected}, DragToLocal Supported: {InitializationMessageHandler.RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.DragToLocal)}")); // Developer message
			return isRemoteAppSession && EnterpriseChannel.Instance != null && EnterpriseChannel.Instance.IsConnected && InitializationMessageHandler.RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.DragToLocal);
		}
		//readonly Lazy<bool> isRemoteAppSession = new Lazy<bool>(new TerminalService().IsRemoteAppSession);

		protected virtual DragDropTrackingForm GetDragDropTrackingInfoForm()
		{
			return EnterpriseChannel.Instance?.DragDropTrackingInfoForm;
		}

		RemoteSelectedItemMessages GetSelectedItems(GridRowsDataObject dataObject)
		{
			var fileNames = dataObject.GetData(DataFormats.FileDrop) as string[];
			var text = dataObject.GetData(DataFormats.Text) as string;

			if ((fileNames != null && fileNames.Any()) || !string.IsNullOrEmpty(text))
			{
				var selectedItems = new RemoteSelectedItemMessages();

				if (fileNames != null)
				{
					foreach (var path in fileNames)
					{
						var fileInfo = new FileInfo(path);
						var newItem = new RemoteSelectedItemMessage()
						{
							FileName = fileInfo.Name,
							SourceFileName = fileInfo.FullName,
							FileSize = fileInfo.Length,
							WriteTime = fileInfo.LastWriteTime
						};
						selectedItems.SelectedItemMessages.Add(newItem);
					}
				}

				selectedItems.SelectedItemCollectionAsText = text;

				return selectedItems;
			}

			return null;
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);
			TrackingInfoLogger.Instance.LogDragDropEvents("ZGrid_DragEnter", GetType().Name, Name);
		}

		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);
			TrackingInfoLogger.Instance.LogDragDropEvents("ZGrid_DragLeave", GetType().Name, Name);
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			base.OnDragDrop(drgevent);
			TrackingInfoLogger.Instance.LogDragDropEvents("ZGrid_DragDrop", GetType().Name, Name);
		}
#else
		protected override void OnDragStart(DragEventArgs args)
		{
			var hitInfo = HitTest(args.X, args.Y);
			DraggedColumn = new DraggedDataGridColumn(hitInfo.Column, Rectangle.Empty);
			base.OnDragStart(args);
		}

		protected override void OnDragDrop(DragEventArgs args)
		{
			if (DraggedColumn != null)
			{
				var hitInfo = HitTest(args.X, args.Y);
				SortColumns(hitInfo.Column, DraggedColumn.Index);
				DraggedColumn = null;
				Invalidate();

				object previousSelection = null;
				object[] previousSelections = null;

				if (ListManager != null && ListManager.List != null && ListManager.Position >= 0 && ListManager.Position < ListManager.List.Count)
				{
					previousSelection = ListManager.List[ListManager.Position];
				}
				if (hitInfo.Type == HitTestType.ColumnHeader && !CheckGridRowsOutOfSyncWithListManager())
				{
					previousSelections = SelectedElements;
				}

				if (previousSelections != null)
				{
					UpdatePreviousSelections(previousSelection, previousSelections);
				}
			}
			base.OnDragDrop(args);
		}
#endif

		protected virtual GridRowsDataObject GetDataObject(ICollection<BusinessObject> selectedElements)
		{
			return new GridRowsDataObject(this, selectedElements);
		}

		protected virtual void OnDragDropCompleted(DragDropCompletedEventArgs dragDropArgs)
		{
			DragDropCompleted?.Invoke(this, dragDropArgs);
		}

		public class DragDropCompletedEventArgs : EventArgs
		{
			public DragDropCompletedEventArgs(DragDropEffects dragDropEffect, GridRowsDataObject data)
			{
				DragDropEffect = dragDropEffect;
				Data = data;
			}

			public readonly DragDropEffects DragDropEffect;
			public readonly GridRowsDataObject Data;
		}

		public event EventHandler<DragDropCompletedEventArgs> DragDropCompleted;

		#endregion

		#region Selecting

		bool movingSelection;

		internal void EndEdit()
		{
#if !WINZOR
			if (isMouseWheeling)
			{
				Invalidate();
			}
#endif

			if (endEditMethod == null)
			{
				endEditMethod = typeof(DataGrid).GetMethod("EndEdit", BindingFlags.Instance | BindingFlags.NonPublic);
			}
			endEditMethod.Invoke(this, null);
		}
		MethodInfo endEditMethod;

		internal new void ResetSelection()
		{
			base.ResetSelection();
		}

		static bool IsChangingCurrentCellInSameRow(Keys keyData)
		{
			var keyCode = keyData & Keys.KeyCode;
			return keyCode == Keys.Home || keyCode == Keys.End || keyCode == Keys.Left || keyCode == Keys.Right;
		}

		static bool IsSelectingUp(Keys keyData)
		{
			return keyData == (Keys.Up | Keys.Shift) || keyData == (Keys.Up | Keys.Shift | Keys.Alt) ||
				keyData == (Keys.Up | Keys.Shift | Keys.Alt | Keys.Control);
		}

		static bool IsSelectingDown(Keys keyData)
		{
			return keyData == (Keys.Down | Keys.Shift) || keyData == (Keys.Down | Keys.Shift | Keys.Alt) ||
				keyData == (Keys.Down | Keys.Shift | Keys.Alt | Keys.Control);
		}

		static bool IsSelectingToTop(Keys keyData)
		{
			return keyData == (Keys.Up | Keys.Shift | Keys.Control) || keyData == (Keys.PageUp | Keys.Shift | Keys.Control) ||
				keyData == (Keys.Home | Keys.Shift | Keys.Control) || keyData == (Keys.PageUp | Keys.Shift) ||
				keyData == (Keys.PageUp | Keys.Shift | Keys.Alt) || keyData == (Keys.PageUp | Keys.Shift | Keys.Alt | Keys.Control);
		}

		static bool IsSelectingToBottom(Keys keyData)
		{
			return keyData == (Keys.Down | Keys.Shift | Keys.Control) || keyData == (Keys.PageDown | Keys.Shift | Keys.Control) ||
				keyData == (Keys.End | Keys.Shift | Keys.Control) || keyData == (Keys.PageDown | Keys.Shift) ||
				keyData == (Keys.PageDown | Keys.Shift | Keys.Alt) || keyData == (Keys.PageDown | Keys.Shift | Keys.Alt | Keys.Control);
		}

		/// <summary>
		/// Prevents Clearing of RowSelection when at edges of the ZGrid.
		/// </summary>
		/// <returns>True if the Key is Handled, otherwise False.</returns>
		bool IsWholeRowSelectedOnClickKeys(Keys keyData)
		{
			if (ListManager != null && ListManager.Count > 0)
			{
				switch (keyData)
				{
					case Keys.Home:
						CurrentCell = new DataGridCell(0, 0);
						Select(0);
						return true;

					case Keys.End:
						CurrentCell = new DataGridCell(ListManager.Count - 1, TotalColumnsInColumnStyleCollection - 1);
						Select(ListManager.Count - 1);
						return true;

					case Keys.PageDown:
					case Keys.Down:
						return (CurrentCell.RowNumber == ListManager.Count - 1);

					case Keys.PageUp:
					case Keys.Up:
						return (CurrentCell.RowNumber == 0);

					case Keys.Left:
						return (CurrentCell.RowNumber == 0 && CurrentCell.ColumnNumber == 0);

					case Keys.Right:
						return (CurrentCell.ColumnNumber == TotalColumnsInColumnStyleCollection - 1 && CurrentCell.RowNumber == ListManager.Count - 1);

					default:
						return false;
				}
			}

			return false;
		}

		void CopySelectedRows()
		{
			var selected = new List<BusinessObject>(SelectedElements);
			if (ListManager != null && selected.Count > 0)
			{
				var selectionText = GetSelectedRowsAsText(selected, false);

				if (!SafeClipboard.SetDataObject(selectionText, true))
				{
					Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
				}
			}
		}

		internal string GetSelectedRowsAsText(ICollection<BusinessObject> selected, bool includeHeader)
		{
			var selectionBuilder = new StringBuilder();
			var columnStylesCollection = TableStyles[0].GridColumnStyles.Cast<ZGridColumnStyle>().Where(style => style.PropertyDescriptor != null).ToList();
			var delimiter = GetOutputTextDelimiter();

			if (includeHeader)
			{
				for (var y = 0; y < columnStylesCollection.Count; y++)
				{
					var style = columnStylesCollection[y];
					selectionBuilder.Append(style.HeaderText);

					if (y < (columnStylesCollection.Count - 1))
					{
						selectionBuilder.Append(delimiter);
					}
				}
				selectionBuilder.AppendLine();
			}

			if (selected.Any())
			{
				for (var i = 0; i < ListManager.Count; i++)
				{
					if (ListManager.List[i] is BusinessObject && selected.Contains(ListManager.List[i]))
					{
						for (var y = 0; y < columnStylesCollection.Count; y++)
						{
							var style = columnStylesCollection[y];
							var columnText = style.IsSensitiveValue ? "***" : style.GetValueAsString(ListManager, i);
							selectionBuilder.Append(columnText);

							if (y < (columnStylesCollection.Count - 1))
							{
								selectionBuilder.Append(delimiter);
							}
						}
						selectionBuilder.AppendLine();
					}
				}
			}

			return selectionBuilder.ToString();
		}

		#region Selected Element(s)

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public BusinessObject[] SelectedElements
		{
			get { return GetSelectedElements<BusinessObject>(); }
		}

		public virtual T[] GetSelectedElements<T>() where T : BusinessObject
		{
			var selectedElementList = new List<T>();
			if (ListManager != null)
			{
				CheckGridRowsOutOfSyncWithListManager();

				var lastRow = Math.Min(ListManager.Count, DataGridRowsLength); // callbacks seem to get us out of sync, the safest is the smallest.
				for (var i = 0; i < lastRow; i++)
				{
					if (IsSelected(i))
					{
						var businessObject = ListManager.List[i] as T;
						if (businessObject != null)
						{
							selectedElementList.Add(businessObject);
						}
					}
				}
			}
			return selectedElementList.ToArray();
		}

		bool CheckGridRowsOutOfSyncWithListManager()
		{
			// If there is one more element in list then rows in data grid,
			// it is likely new element was just added into collection in position before current row.
			// That moved 'current' row one position down and fired method OnPositionChanged().
			// But data grid rows were not yet recreated.
			// So we can only ignore this situation and say that nothing is selected, which will be true as soon as data grid rows are recreated.
			// We should not return here empty array to be consistent with property SelectedRowsCount.

			if (IsHandleCreated && ListManager != null)
			{
				var listManagerCount = ListManager.Count;
				var dataGridRowsLength = DataGridRowsLength;
				var lastBizo = listManagerCount == 0 ? null : ListManager.List[ListManager.Count - 1] as INeedRow;
				var isLastBizoUncommitted = lastBizo != null && lastBizo.Row != null && lastBizo.Row.RowState == DataRowState.Detached;

				return listManagerCount > dataGridRowsLength &&
					!isLastBizoUncommitted &&
					(dataGridRowsLength > 0 || typeof(DataGrid).GetField("dataGridRows", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(this) != null);
			}

			return false;
		}

		public void SelectAllElements()
		{
			for (var i = 0; i < DataGridRowsLength; i++)
			{
				Select(i);
			}
		}

		public void SelectAllElements(Func<BusinessObject, bool> predicate)
		{
			var list = List;
			for (var i = 0; i < list.Count; i++)
			{
				if (predicate((BusinessObject)list[i]))
				{
					Select(i);
				}
				else
				{
					UnSelect(i);
				}
			}
		}

		public void SelectSingleElement(BusinessObject bizO)
		{
			if (bizO != null)
			{
				var list = List ?? throw new InvalidOperationException("Collection not bound properly, IBindingList should not be null");

				for (var i = 0; i < list.Count; i++)
				{
					if (((BusinessObject)list[i]).PK == bizO.PK)
					{
						ListManager.Position = i;
						Select(i);
					}
					else
					{
						UnSelect(i);
					}
				}
			}
		}

		public void UnSelectAll()
		{
			for (var i = 0; i < DataGridRowsLength; i++)
			{
				UnSelect(i);
			}
		}

		#endregion

		#endregion

		#region Custom Design

		#region Custom Row Background Colour

		internal Guid CurrentLoadColorsVersion = Guid.NewGuid();
		Guid LastLoadColorsVersion;

		[DefaultValue(false)]
		public bool IsBackgroundColourLoaded => CurrentLoadColorsVersion.Equals(LastLoadColorsVersion);

		BackgroundWorker LoadColorsWorker
		{
			get
			{
				if (loadColorsWorker == null && !this.IsDisposed)
				{
					loadColorsWorker = new BackgroundWorker();
					loadColorsWorker.WorkerSupportsCancellation = true;
					loadColorsWorker.DoWork += new DoWorkEventHandler(DoLoadBackgroundColour);
					loadColorsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnLoadBackgroundColourFinish);
				}

				return loadColorsWorker;
			}
		}
		BackgroundWorker loadColorsWorker;

		public void StartLoadBackgroundColour()
		{
			if (IsDisposing || IsDisposed)
			{
				return;
			}
			if (LoadColorsWorker.IsBusy)
			{
				return;
			}

			if (ListManager == null)
			{
				return;
			}

			var colourScheme = GetLastUsedColourSchemeForCurrentUser;
			if (colourScheme == null)
			{
				return;
			}

			var dataObjectForLoadColour = gridColourSchemeManager?.GetPrecalculateColorsForAllRowsInGridQueries(colourScheme);
			dataObjectForLoadColour.VersionOfLoad = CurrentLoadColorsVersion;
			if (dataObjectForLoadColour.IsValid)
			{
				LoadColorsWorker.RunWorkerAsync(dataObjectForLoadColour);
			}
			else
			{
				LastLoadColorsVersion = CurrentLoadColorsVersion;
			}
		}

		void DoLoadBackgroundColour(object sender, DoWorkEventArgs e)
		{
			var dataObject = (LoadColorsData)e.Argument;

			if (e.Cancel)
			{
				e.Result = null;
				return;
			}

			try
			{
				e.Result = gridColourSchemeManager.LoadGridColors(dataObject);
			}
			catch (Exception ex)
			{
				throw new LoadColourException(dataObject.VersionOfLoad, ex);
			}

			if (LoadColorsWorker == null || LoadColorsWorker.CancellationPending)
			{
				e.Cancel = true;
			}
		}

		void OnLoadBackgroundColourFinish(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				return;
			}

			if (IsDisposing || IsDisposed)
			{
				return;
			}

			if (e.Error != null)
			{
				if (e.Error is LoadColourException loadColourException)
				{
					if (loadColourException.VersionOfLoad.Equals(CurrentLoadColorsVersion))
					{
						gridColourSchemeManager.HandleExceptionWhenDecidingBackgroundColour(loadColourException.InnerException, null);
						LastLoadColorsVersion = CurrentLoadColorsVersion;
					}
					else
					{
						StartLoadBackgroundColour();
					}
				}
				else if (e.Error is not LoadColourException)
				{
					gridColourSchemeManager.HandleExceptionWhenDecidingBackgroundColour(e.Error, null);
				}

				return;
			}

			var result = (LoadColorsData)e.Result;
			if (!result.VersionOfLoad.Equals(CurrentLoadColorsVersion))
			{
				if (!LoadColorsWorker.IsBusy)
				{
					StartLoadBackgroundColour();
				}
				return;
			}
			gridColourSchemeManager.ApplyColors(result);
			LastLoadColorsVersion = CurrentLoadColorsVersion;

			base.Invalidate();
#if WINZOR
			this.ReadyToRender();
			this.NotifyRenderRequired();
#endif
		}

		internal Color GetCustomRowBackgroundColour(int rowNumber)
		{
			return GetCustomRowBackgroundColour(rowNumber, false);
		}

		internal Color GetCustomRowBackgroundColour(int rowNumber, bool readOnlyColor)
		{
			if (CustomRowBackgroundColourDeciding == null && ColourDeciding == null)
			{
				return Color.Empty;
			}

			var objectAtRow = ListManager.List[rowNumber];

			var arg = new ColourDecidingEventArgs(objectAtRow)
			{
				Pk = (objectAtRow as BusinessObject) != null ? (objectAtRow as BusinessObject).PK : ZGuid.Empty,
				Colour = Color.Empty
			};

			CustomRowBackgroundColourDeciding?.Invoke(this, arg);

			//There's no grid color scheme for this module grid or if it's a data grid then CustomRowBackgroundColourDeciding event won't be handled anyway
			//so if the color is empty then fire this event. This means user created color schemes will override developer created colors in code for module grids.
			if (ColourDeciding != null && arg.Colour == Color.Empty)
			{
				ColourDeciding(this, arg);
			}

			return (readOnlyColor && !arg.ReadOnlyColour.IsEmpty) ? arg.ReadOnlyColour : arg.Colour;
		}

		internal Dictionary<ZGuid, Color> ColorByPK = new Dictionary<ZGuid, Color>();
		internal bool ColorByPKIsExpired;

		internal bool RowColorsAreDataViewOptimisable { get; set; }

		/// <summary>
		/// Subscribe to this event to have custom row background colours for your grid.
		/// Set the Colour property on the EventArgs to the colour you want. Setting it to Color.Empty will display the row in the default colour.
		/// </summary>
		public event EventHandler<ColourDecidingEventArgs> ColourDeciding;

		/// <summary>
		/// Only hook up to this event from GridColourSchemeManager.
		/// </summary>
		internal event EventHandler<ColourDecidingEventArgs> CustomRowBackgroundColourDeciding;

		#endregion

		#region Custom Text Font

		internal Font GetCustomCellFont(int rowNumber, string dataMember)
		{
			Font result = null;

			if (FontDeciding != null)
			{
				var objectAtRow = ListManager.List[rowNumber];
				var eventArgs = new FontDecidingEventArgs(objectAtRow, dataMember, Font);
				FontDeciding(this, eventArgs);
				result = eventArgs.Font;
			}

			return result;
		}

		/// <summary>
		/// Subscribe to this event to have custom font for your grid.
		/// Set the Font property on the EventArgs to the font you want. Setting it to null will display the column in the default font.
		/// </summary>
		public event EventHandler<FontDecidingEventArgs> FontDeciding;

		#endregion

#if !WINZOR
		protected internal virtual Brush ReadOnlyBrushFromRowNum(int rowNum)
		{
			return BrushProvider.FromColor(SystemDataRegistry.Instance.ColorTheme.GridReadOnlyColor);
		}
#endif

		protected internal virtual Color ReadOnlyColorForRowNum(int rowNum)
		{
			return SystemDataRegistry.Instance.ColorTheme.GridReadOnlyColor;
		}

		#endregion

		#region Scrolling

		protected bool IsScrolling;
		protected bool ResetFocusToCell;
		protected bool IsRowHeaderClicked;

#if !WINZOR
#if DEBUG
		internal void GridVScrolledExposed(object sender, ScrollEventArgs e)
		{
			GridVScrolled(sender, e);
		}
#endif
		protected override void GridVScrolled(object sender, ScrollEventArgs e)
		{
			try
			{
				if (e.NewValue > e.OldValue)
				{
					Invalidate();
				}
				HandleGridScroll(e);
				base.GridVScrolled(sender, e);
			}
			catch (IndexOutOfRangeException ex)
			{
				var columnNames = string.Join(",", Columns);
				var numVisibleCols = (int)typeof(DataGrid).GetField("numVisibleCols", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

				var message = FormattableString.Invariant($@"
Additional info: 
CurrentRowIndex: {CurrentRowIndex}
DataGridRowsLength: {DataGridRowsLength}
ListManager's List Count: {ListManager?.List?.Count ?? -1}
CurrentColumnIndex: {CurrentCell.ColumnNumber}
CurrentColumnName: {this.GetCurrentColumnStyle()?.MappingName}
ZGridColumns: {columnNames}
ZGridColumnCount: {Columns.Count}
numVisibleCols: {numVisibleCols}
VisibleColumnCount: {VisibleColumnCount}
HorizScrollBarVisible: {HorizScrollBar.Visible}
VertScrollBarVisible: {VertScrollBar.Visible}
Control Path: {ControlDescription.GetControlPath(this)}"); // Developer exception

				Globals.Message.ShowDeveloperException("Exception in GridVScrolled.", message, ex);
			}
		}

		bool isMouseWheeling;

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (this.IsDisposed || this.IsDisposing)
			{
				return;
			}

			isMouseWheeling = true;

			try
			{
				base.OnMouseWheel(e);

				if (e.Delta < 0)
				{
					Invalidate();
				}
			}
			catch (IndexOutOfRangeException)
			{
				ShowErrorWhenCurrentVisibleRowsIndexOutOfRange(nameof(OnMouseWheel), () => base.OnMouseWheel(e));
			}

			isMouseWheeling = false;
		}

		void ShowErrorWhenCurrentVisibleRowsIndexOutOfRange(string methodName, Action action)
		{
			var info = typeof(DataGrid).GetMethod("ComputeVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic);
			info.Invoke(this, Array.Empty<object>());
			try
			{
				action();
			}
			catch (IndexOutOfRangeException ex)
			{
				var numVisibleRows = (int)typeof(DataGrid).GetField("numVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
				var firstVisibleRow = (int)typeof(DataGrid).GetField("firstVisibleRow", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
				var numTotallyVisibleRows = (int)typeof(DataGrid).GetField("numTotallyVisibleRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
				Globals.Message.ShowDeveloperException($"Exception in {methodName}.", FormattableString.Invariant(
					$"Additional info: CurrentRowIndex: {CurrentRowIndex}. DataGridRowsLength:{DataGridRowsLength}.ListManager's List Count: {ListManager?.List?.Count ?? -1}. numVisibleRows={numVisibleRows}. firstVisibleRow={firstVisibleRow}. numTotallyVisibleRows={numTotallyVisibleRows}."
					), ex); // Developer exception
			}
		}
#if DEBUG
		internal void GridHScrolledExposed(object sender, ScrollEventArgs e)
		{
			GridHScrolled(sender, e);
		}
#endif

		protected override void GridHScrolled(object sender, ScrollEventArgs e)
		{
			try
			{
				if (e.NewValue > e.OldValue)
				{
					Invalidate();
				}
				HandleGridScroll(e);
				base.GridHScrolled(sender, e);
			}
			catch (IndexOutOfRangeException)
			{
				ShowErrorWhenCurrentVisibleRowsIndexOutOfRange(nameof(GridHScrolled), () => base.GridHScrolled(sender, e));
			}

			if (e.Type == ScrollEventType.EndScroll)
			{
				Invalidate();
			}
		}

		protected void HandleGridScroll(ScrollEventArgs e)
		{
			IsScrolling = e.Type != ScrollEventType.EndScroll;

			if (LastFocusedColumn != null)
			{
				if (!IsScrolling && ResetFocusToCell)
				{
					BeginEdit(LastFocusedColumn, CurrentCell.RowNumber);
				}
				else if (IsScrolling)
				{
					LastFocusedColumn.HideEditControl();
				}
			}
		}

#endif
		#endregion

		#region Refresh Manager

		protected void RefreshManager()
		{
			if (!ContainsFocus && DataSource != null && IsValidSource(ListManager))
			{
				if (ListManager.GetCurrent() is DataRowView)
				{
					var selectedRows = GetSelectedRowIndexes();
					ListManager.Refresh();
					SetSelectedRows(selectedRows);
				}
			}
		}

		protected void ForceRefreshManager()
		{
			if (IsValidSource(ListManager))
			{
				EndCurrentEdit();
				ListManager.Refresh();
#if DEBUG
				dataGridRowsPropertyInfo = typeof(DataGrid).GetProperty("DataGridRowsLength", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
			}
		}

		protected bool ShouldRefreshManager()
		{
			var isValidCell = (IsCellEdited && IsValidSource(ListManager) && ListManager.Count > CurrentCell.RowNumber);
			var isLastFocusedCellReadOnly = (isValidCell && LastFocusedColumn != null && LastFocusedColumn.IsCellReadOnly(ListManager, CurrentCell.RowNumber));

			return !(AllColumnsReadOnly || isLastFocusedCellReadOnly);
		}

#if DEBUG
		internal
#else
		protected
#endif
 int[] GetSelectedRowIndexes()
		{
			var selectedRows = new List<int>();

			for (var i = 0; i < DataGridRowsLength; i++)
			{
				if (IsSelected(i))
				{
					selectedRows.Add(i);
				}
			}

			return selectedRows.ToArray();
		}

#if DEBUG
		internal
#else
		protected
#endif
 void SetSelectedRows(int[] rowsToSelect)
		{
			ResetSelection();

			foreach (var row in rowsToSelect)
			{
				Select(row);
			}
		}

		#endregion

		#region FindBox Column Module Showing

		internal void OnFindBoxColumnModuleShowing(ZGridColumnStyle columnStyle, ModuleShowingEventArgs e)
		{
			if (columnStyle == null)
			{
				throw new ArgumentNullException(nameof(columnStyle));
			}

			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}

			if (FindBoxColumnModuleShowing != null)
			{
				var currentBizObj = ListManager != null ? (BusinessObject)ListManager.GetCurrent() : null;
				if (currentBizObj != null)
				{
					var args = new FindBoxColumnModuleShowingEventArgs(columnStyle, currentBizObj, e.ModuleID);
					FindBoxColumnModuleShowing(this, args);
					e.ModuleID = args.ModuleID;
				}
			}
		}

		public event EventHandler<FindBoxColumnModuleShowingEventArgs> FindBoxColumnModuleShowing;

		#endregion

		#region TextIndexSearch

		public void SetIndexFromSearchString(string textToSearchFor, string nameOfColumnToSearch)
		{
			var searchColumnIndex = 0;
			for (var c = 0; c < Columns.Count; c++)
			{
				if (Columns[c].ColumnStyle.MappingName == nameOfColumnToSearch)
				{
					searchColumnIndex = c;
					break;
				}
			}
			SetIndexFromStringOnColumn(textToSearchFor, searchColumnIndex);
		}

		void SetIndexFromStringOnColumn(ZString textToSearchFor, int indexOfColumnToSearch)
		{
			for (var i = 0; i < List.Count; i++)
			{
				if (this[i, indexOfColumnToSearch].ToString().IndexOf(textToSearchFor, StringComparison.InvariantCultureIgnoreCase) > -1)
				{
					CurrentRowIndex = i;
					break;
				}
			}
		}

		#endregion

		#region Data Import/Export

		#region Export to Excel

		internal virtual bool ShowExportToExcelMenuItem
		{
			get { return List is IBusinessObjectCollection; }
		}

		protected internal virtual void ExportVisibleIntoAndOpenExcel()
		{
			if (CanContinueWithExport)
			{
				ExcelExporter.ExportVisibleIntoAndOpenExcel(ListManager, Columns);
			}
			else
			{
				ShowNoRecordsToExportMessage();
			}
		}

		protected internal virtual void ExportIntoAndOpenExcel()
		{
			if (CanContinueWithExport)
			{
				ExcelExporter.ExportIntoAndOpenExcel(ListManager, Columns);
			}
			else
			{
				ShowNoRecordsToExportMessage();
			}
		}

		protected virtual bool CanContinueWithExport
		{
			get { return (List != null && List.Count > 0) || List is IHaveZQueryForZGridExcelExport; }
		}

		protected void ShowNoRecordsToExportMessage()
		{
			GetNewExcelExporterGuiNotifications().ShowNoRecordsToExportError(ExcelExporter.ExportMessages.NoRecordsToExport);
		}

		void AddExportToExcelMenuItem()
		{
			ExportAllColumnsToExcelMenuItem = new ZMenuItem(ResString.GetMultilingualString("B1C5D2CA-C3D6-4B7A-A9BC-0FCF3D9FABEE", "Export All Columns To Excel"), ExportAllColumnsToExcelClick);
			ContextMenu.MenuItems.Add(ExportAllColumnsToExcelMenuItem);
			ExportVisibleColumnsToExcelMenuItem = new ZMenuItem(ResString.GetMultilingualString("73A341F9-0921-4254-B9E2-B445DF92B077", "Export Visible Columns To Excel"), ExportVisibleColumnsToExcelClick);
			ContextMenu.MenuItems.Add(ExportVisibleColumnsToExcelMenuItem);
		}

		void ExportAllColumnsToExcelClick(object sender, EventArgs e)
		{
			ExportIntoAndOpenExcel();
		}

		void ExportVisibleColumnsToExcelClick(object sender, EventArgs e)
		{
			ExportVisibleIntoAndOpenExcel();
		}

		internal virtual ExcelExporter ExcelExporter
		{
			get
			{
				return new ExcelExporter(ReaderForExcelExport, GetNewColumnsForExport(), GetNewExcelExporterGuiNotifications());
			}
		}

		protected internal virtual BusinessObjectReader ReaderForExcelExport
		{
			get
			{
				if (SelectedRowCount > 0)
				{
					return new ArrayBusinessObjectReader(GetSelectedRows(), ((IBusinessObjectCollection)List).TypeOfElements);
				}
				else if (List is IHaveZQueryForZGridExcelExport)
				{
					return new BusinessObjectListReader(((IHaveZQueryForZGridExcelExport)List).Query, ((IBusinessObjectCollection)List).TypeOfElements, new BusinessObjectFactory());
				}
				else
				{
					return new CollectionWrapperBusinessObjectReader((IBusinessObjectCollection)List);
				}
			}
		}

		protected ExcelExporterGuiNotifications GetNewExcelExporterGuiNotifications()
		{
			return new ExcelExporterGuiNotifications(FindForm());
		}

#if DEBUG
		public
#else
		protected
#endif
		List<ExcelExportColumnBase> GetNewColumnsForExport()
		{
			var result = new List<ExcelExportColumnBase>();
			var badColumns = new List<string>();

#if DEBUG
			if (DesignModeFinder.IsDesigning)
			{
				return result;
			}
#endif

			var bizoToCheckAgainst = GetFirstBizOInList();
			foreach (var gridColumn in Columns)
			{
				if (IsColumnBoundToProperty(gridColumn, bizoToCheckAgainst))
				{
					var exportColumn = GetNewExcelExportColumn(gridColumn.ColumnStyle, bizoToCheckAgainst);
					result.Add(exportColumn);
				}
				else
				{
					badColumns.Add(gridColumn.ColumnStyle.MappingName);
				}
			}

			ReportDeveloperErrorForBadColumns(bizoToCheckAgainst, badColumns);

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer error")]
		void ReportDeveloperErrorForBadColumns(BusinessObject firstBizoInTheList, List<string> badColumns)
		{
			if (firstBizoInTheList != null && badColumns.Count > 0)
			{
				var nl = System.Environment.NewLine;

				var msg = "The following properties were not found: " + string.Join(", ", badColumns.ToArray());
				msg += nl + nl +
					"Grid Name: " + Name + nl +
					"Type of element from List: " + ElementTypeFromCollection.Name + nl +
					"Type of FirstBizO: " + firstBizoInTheList.GetType() + nl + nl +
					"FirstBizO.GetPropertiesIncludingFlattened():" + nl;

				foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(firstBizoInTheList).Sort())
				{
					msg += string.Format(CultureInfo.InvariantCulture, nl + "{0} [{1}]", pd.Name, pd.PropertyType.Name);
				}

				msg += nl + nl + "Properties in ZCustomTypeDescriptor:" + nl + ZCustomTypeDescriptor.GetProperties(ElementTypeFromCollection).Cast<KPropertyDescriptor>().Select(x => x.Name).Aggregate((x, y) => x + nl + y);
				msg += nl + nl + "Properties in GetItemProperties (should be the same):" + nl + ListManager.GetItemProperties().Cast<PropertyDescriptor>().Select(x => x.Name).Aggregate((x, y) => x + nl + y);
				msg += nl + nl + "Obviously if ZCustomTypeDescriptor for an interface doesn't record every property on that interface, something Bad is happening. Check PropertyDescriptorCollectionFactory.cs and try to figure out how it might fail.";

				ErrorReporter.ReportOnce("ZGrid.GetNewColumnsForExport." + Name + "." + ElementTypeFromCollection.Name, msg);
			}
		}
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded column name suffix")]
#if DEBUG
		internal
#endif
		ExcelExportColumnBase GetNewExcelExportColumn(DataGridColumnStyle columnStyle, BusinessObject bizObjToCheckAgainst)
		{
			var schemaColumn = GetSchemaColumnForExcelExport(columnStyle, bizObjToCheckAgainst);

			var description = columnStyle.HeaderText;
			if (char.IsControl(description[description.Length - 1])) // remove bad chars at the end of the string
			{
				description = description.Remove(description.Length - 2, 2);
			}

			if (schemaColumn.ColumnType == SchemaColumnType.DateTime
				&& schemaColumn.Name.EndsWith("Utc", StringComparison.OrdinalIgnoreCase)
				&& !Regex.IsMatch(description, "\\bUtc\\b", RegexOptions.IgnoreCase))
			{
				description += " " + Res.GetString("b822e1a4-53a8-4dfc-a432-0d1166ed3a0c", "(UTC)");
			}

			if (columnStyle.PropertyDescriptor == null && ListManager != null)
			{
				columnStyle.PropertyDescriptor = ListManager.GetItemProperties()
					.Cast<PropertyDescriptor>()
					.FirstOrDefault(prop => !typeof(IList).IsAssignableFrom(prop.PropertyType) && prop.Name.Equals(columnStyle.MappingName));
			}

			ExcelExportColumnBase result;
			if (columnStyle is ZGuidFindBoxColumnStyle guidFindBoxColumnStyle && columnStyle.PropertyDescriptor != null)
			{
				result = ExcelExportCustomValueColumn.New(new ExcelExportGuidDisplayValue(guidFindBoxColumnStyle));
			}
			else
			{
				result = ExcelExportColumn.New(schemaColumn, description);
			}

			if (schemaColumn.ColumnType == SchemaColumnType.DateTime)
			{
				if (columnStyle is IZDateTimePickerFormat dateEditColumnStyle) // some ppl use textbox columns for date fields, thus we can't get formatting info
				{
					((ExcelExportDateTimeColumn)result).DateTimeFormat = dateEditColumnStyle.DateTimeFormat;
				}
			}
			else if (schemaColumn.ColumnType == SchemaColumnType.Decimal)
			{
				if (columnStyle is ZCalcEditColumnStyle decimalColumnStyle) // some ppl use textbox columns for decimal fields, thus we can't get formatting info
				{
					((ExcelExportDecimalColumn)result).Decimals = decimalColumnStyle.Decimals;
				}
			}

			result.Width = columnStyle.Width * 48;
			result.PropertyDescriptor = columnStyle.PropertyDescriptor;

			return result;
		}

		#region ExcelExportGuidDisplayValue
#if DEBUG
		internal
#endif
		class ExcelExportGuidDisplayValue : IExcelExportCustomValue
		{
			public ExcelExportGuidDisplayValue(ZGuidFindBoxColumnStyle columnStyle)
			{
				this.columnStyle = columnStyle;
			}

			#region IExcelExportCustomValue Members

			public IZType GetCustomValue(BusinessObject bizObj)
			{
				var findBox = (ZGridFindBox)columnStyle.FindBox;
				var oldCurrentItem = ((ZGridFindBox)columnStyle.FindBox).CurrentItem;
				var oldList = findBox.List;
				try
				{
					findBox.CurrentItem = bizObj;
					findBox.DataPropertyName = columnStyle.MappingName;
					findBox.PullList();
					return (ZString)columnStyle.FindBox.ListProvider.CodeFromPrimaryKey((ZGuid)bizObj[columnStyle.MappingName]);
				}
				finally
				{
					findBox.CurrentItem = oldCurrentItem;
					findBox.List = oldList;
				}
			}

			public ZString GetValueFormat(IZType value)
			{
				return ZString.Empty;
			}

			public ZString GetDescription()
			{
				return columnStyle.HeaderText;
			}

			#endregion

			readonly ZGuidFindBoxColumnStyle columnStyle;
		}

		#endregion

#if DEBUG
		internal
#endif
		SchemaColumn GetSchemaColumnForExcelExport(DataGridColumnStyle columnStyle, BusinessObject bizObjToCheckAgainst)
		{
			SchemaColumn result = null;
			var columnName = columnStyle.MappingName;

			var schemaTable = EnterpriseSchema.GetTableSchema(TableNameFromVisibleColumns);
			if (schemaTable != null)
			{
				result = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(columnName, schemaTable.TableName);
			}
			else
			{
				schemaTable = CargoWise.Schema.Schema.GenericTableSchema;
			}

			if (result == null)
			{
				if (columnStyle is ZMultiControlColumnStyle)
				{
					result = new SchemaMultiColumn(columnName, schemaTable);
				}
				else
				{
					var boundZType = GetBoundZType(columnStyle, bizObjToCheckAgainst);
					if (boundZType == typeof(ZDateTime))
					{
						var dateTimeColumnStyle = columnStyle as ZDateEditColumnStyle;
						if (dateTimeColumnStyle != null && IsColumnTimeStyled(dateTimeColumnStyle))
						{
							result = new SchemaDateTimeColumn(schemaTable, columnName, 0, SqlDbType.Time, null, false);
						}
						else
						{
							result = new SchemaDateTimeColumn(schemaTable, columnName, 0, SqlDbType.DateTime, null, false);
						}
					}
					else if (boundZType == typeof(ZDateTimeOffset))
					{
						result = new SchemaDateTimeOffsetColumn(schemaTable, columnName, 0, SqlDbType.DateTimeOffset, null, false, 0);
					}
					else if (boundZType == typeof(ZDate))
					{
						result = new SchemaDateColumn(schemaTable, columnName, 0, SqlDbType.Date, null, false);
					}
					else if (boundZType == typeof(ZTime))
					{
						result = new SchemaTimeColumn(schemaTable, columnName, 0, SqlDbType.DateTime, null, false);
					}
					else if (boundZType == typeof(ZDecimal))
					{
						result = new SchemaDecimalColumn(schemaTable, columnName, 0, SqlDbType.Decimal, 0m, false, 0, 0);
					}
					else if (boundZType == typeof(ZGuid))
					{
						result = new SchemaGuidColumn(schemaTable, columnName, 0, null, true);
					}
					else if (boundZType == typeof(ZGeography))
					{
						result = new SchemaGeographyColumn(schemaTable, columnName, 0, null, false);
					}
					else
					{
						result = new SchemaStringColumn(schemaTable, columnName, 0, SqlDbType.VarChar, "", false, 10);
					}
				}
			}

			return result;
		}

		bool IsColumnTimeStyled(ZDateEditColumnStyle dateTimeColumnStyle)
		{
			return dateTimeColumnStyle.DateTimeFormat == ZDateTimePickerFormat.TimeIncludingSeconds
				|| dateTimeColumnStyle.DateTimeFormat == ZDateTimePickerFormat.Time;
		}

#if DEBUG
		internal
#endif
		bool IsColumnBoundToProperty(ZGridColumn gridColumn, BusinessObject bizObjToCheckAgainst)
		{
			return GetBoundZType(gridColumn.ColumnStyle, bizObjToCheckAgainst) != null;
		}

		protected virtual Type GetBoundZType(DataGridColumnStyle columnStyle, BusinessObject bizObjToCheckAgainst)
		{
			var result = ((columnStyle.PropertyDescriptor ?? TypeDescriptor.GetProperties(bizObjToCheckAgainst)[columnStyle.MappingName])?.PropertyType) ?? (ListManager.GetItemProperties().Cast<PropertyDescriptor>().FirstOrDefault(prop => !typeof(IList).IsAssignableFrom(prop.PropertyType) && prop.Name.Equals(columnStyle.MappingName))?.PropertyType);
			if (result == null)
			{
				//maybe cache is corrupt - try again
				ZCustomTypeDescriptor.RemoveFromCache(ElementTypeFromCollection);
				result = ListManager.GetItemProperties().Cast<PropertyDescriptor>().FirstOrDefault(prop => !typeof(IList).IsAssignableFrom(prop.PropertyType) && prop.Name.Equals(columnStyle.MappingName))?.PropertyType;
			}

			return result;
		}
#if DEBUG
		internal
#endif
		protected virtual BusinessObject GetFirstBizOInList()
		{
			return (BusinessObject)List[0];
		}

		ZString TableNameFromVisibleColumns
		{
			get
			{
				if (tableNameFromVisibleColumns == null)
				{
					var elementType = ElementTypeFromCollection;
					tableNameFromVisibleColumns = (elementType != null && BusinessObjectFactory.HasTableName(elementType)) ?
						BusinessObjectFactory.GetTableNameFromType(elementType) :
						string.Empty;
				}

				return tableNameFromVisibleColumns;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Type ElementTypeFromCollection
		{
			get
			{
				if (List != null)
				{
					var result = List is IBusinessObjectCollection boColleciton ? boColleciton.TypeOfElements : typeof(object);
					if (result == typeof(object) && BindingContext != null)
					{
						var bindingMember = new KBindingMemberInfo(DataMember);
						var bm = BindingContext[DataSource, bindingMember.BindingPath];
						var property = bm.GetItemProperties()[bindingMember.BindingField];
						if (property != null)
						{
							result = ListUtil.GetListElementType(property.PropertyType) ?? property.PropertyType;
						}
					}

					return result;
				}

				if (!string.IsNullOrEmpty(BindTo))
				{
					var site = ZEnumerable.Iterate<Control>(this, control => control.Parent)
						.TakeWhile(control => control != null)
						.Select(control => control.Site)
						.FirstOrDefault(s => s != null);

					if (site != null)
					{
						var host = (IDesignerHost)site.GetService(typeof(IDesignerHost));
						var dataSourceTypeHolder = host == null ? null : host.RootComponent as ITopLevelDataSourceType;
						var dataSourceType = dataSourceTypeHolder?.DataSourceType;

						return dataSourceType == null ? null : FinalTypeRetriever.Retrieve(dataSourceType, BindTo);
					}
				}

				return null;
			}
		}
#if DEBUG
		internal
#endif
		string tableNameFromVisibleColumns;

#endregion

		#region Import Data Tool

#if DEBUG
		internal
#else
		protected
#endif
		virtual bool ShowImportDataMenuItem
		{
			get
			{
				return !DisableImportDataMenuItem && !ReadOnly && List is IBusinessObjectCollection
					&& (List.AllowNew || (!string.IsNullOrEmpty(GridId) && List.Count > 0) && !ElementType.IsSubclassOf(typeof(DynamicBusinessObject)))
					&& !HasSensitiveColumn;
			}
		}

		[DefaultValue(false)]
		public bool DisableImportDataMenuItem { get; set; } = false;

#if DEBUG
		internal
#endif
		bool HasSensitiveColumn => ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.IsSensitiveValue);

#if DEBUG
		protected internal
#else
		protected
#endif
 virtual IImportCollectionInfoProvider GetImportCollectionInfoProvider()
		{
			if (!(List is IImportCollectionInfoProvider importCollectionInfoProvider))
			{
				importCollectionInfoProvider = new ZGridImportCollectionInfoProvider(this);
			}

			return importCollectionInfoProvider;
		}

		internal void RunImportDataTool(bool modal = true, string moduleName = "")
		{
			var importCollectionInfoProvider = GetImportCollectionInfoProvider();
			var collectionInfo = importCollectionInfoProvider.ImportCollectionInfo;

			if (collectionInfo.Collection.Count == 0 && collectionInfo.Collection is IActiveBusinessObjectCollection aboc && aboc.Relationship.SupportsAddToRelationship())
			{
				foreach (var bizo in this.List.Cast<BusinessObject>())
				{
					aboc.Relationship.AddToRelationship(bizo);
				}
				aboc.Refresh();
			}
			else
			{
				for (var i = 0; i < collectionInfo.Collection.Count; i++)
				{
					var bizObj = collectionInfo.Collection[i] as BusinessObject;
					if (bizObj != null && !bizObj.IsDeleted && ((IBusinessObjectInternals)bizObj).IsUnCommittedRow)
					{
						((ICancelAddNew)collectionInfo.Collection).EndNew(i);
					}
				}
			}

			var contextKey = importCollectionInfoProvider.ContextKey;
			if (string.IsNullOrEmpty(contextKey))
			{
				contextKey = new LegacyDataGridLayoutContextKeyProvider(this).ContextKeyForStmData;
			}

			if (modal)
			{
				var modalForm = new DataImportWizardForm(collectionInfo, contextKey) as ZForm;
				modalForm.StartPosition = FormStartPosition.CenterParent;
				ZFormModaliser.Show(modalForm, FindForm());
			}
			else
			{
				var nonModalForm = new DataImportWizardForm(collectionInfo, contextKey, moduleName) as ZForm;
				nonModalForm.Show();
			}
		}

		public string DataImportWizardKey
		{
			get
			{
				var importCollectionInfoProvider = GetImportCollectionInfoProvider();
				var contextKey = importCollectionInfoProvider.ContextKey;
				if (string.IsNullOrEmpty(contextKey) && !DesignModeFinder.IsDesigning)
				{
					contextKey = new LegacyDataGridLayoutContextKeyProvider(this).ContextKeyForStmData;
				}

				return contextKey;
			}
		}

		void AddImportDataMenuItem()
		{
			var eventHandler = importLicenceCheck ? (EventHandler)LicenceCheckerForImportingData(ImportDataClick).OnClick : ImportDataClick;
			importDataMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.ImportData", "&Import Data..."), eventHandler);
			ContextMenu.MenuItems.Add(importDataMenuItem);

			adawMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.AdvancedDataAutomationWizard", "Advanced Data Automation &Wizard"));
			adawMenuItem.Name = ImportWizardContextMenuName;
			ContextMenu.MenuItems.Add(adawMenuItem);
		}

		void ShowImportMappingWizard(IBusinessObjectCollection collection, IDataTransferMapping mapping = null)
		{
			var importMappingWizardResult = ShowImportMappingWizardCore(collection, mapping);

			if (!string.IsNullOrEmpty(importMappingWizardResult))
			{
				Globals.Message.ShowError(importMappingWizardResult);
			}
		}

#if DEBUG
		internal virtual
#endif
		string ShowImportMappingWizardCore(IBusinessObjectCollection collection, IDataTransferMapping mapping = null)
		{
			return GlowDataWizardIntegration.ShowImportMappingWizard(collection.TypeOfElements, mapping);
		}

		void ShowImportWizard(IBusinessObjectCollection collection, IDataTransferMapping mapping)
		{
			var importWizardResult = ShowImportWizardCore(collection, mapping);

			if (!string.IsNullOrEmpty(importWizardResult))
			{
				Globals.Message.ShowError(importWizardResult);
			}
		}

#if DEBUG
		internal virtual
#endif
		string ShowImportWizardCore(IBusinessObjectCollection collection, IDataTransferMapping mapping)
		{
			var dataSource = DataSource;
			var dataMember = DataMember;
			SuspendLayout();
			SetDataBinding(null, string.Empty);
			try
			{
				return GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mapping);
			}
			finally
			{
				SetDataBinding(dataSource, dataMember);
				ResumeLayout();
			}
		}
#if DEBUG
		internal
#endif
		MenuItem importDataMenuItem;
		ZMenuItem adawMenuItem;
		bool adawInitialised;

		void ImportDataClick(object sender, EventArgs e)
		{
			if (IsAbleToImportData)
			{
				VertScrollBar.Visible = true;
				RunImportDataTool();
			}
		}

		protected virtual bool IsAbleToImportData => true;

		void SetImportDataMenuItemVisibleAndEnabled()
		{
			var enabled = List != null && ShowImportDataMenuItem && !HasSensitiveColumn;

			SetMenuItemVisibleAndEnabled(importDataMenuItem, enabled);

			var adawEnabled = enabled && GlowDataWizardIntegration.IsADAWEnabled();
			SetMenuItemVisibleAndEnabled(adawMenuItem, adawEnabled);

			if (adawEnabled && !adawInitialised)
			{
				adawMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("8fdc6f86-bfff-4a6b-bd7c-5af9850f1d6f", "Loading...")));
				adawMenuItem.Popup += (s, a) =>
				{
					if (!typeof(ShortcutPopupEventArgs).IsAssignableFrom(a.GetType()))
					{
						var collection = (IBusinessObjectCollection)List;
						GlowDataWizardIntegration.HandleADAWMenuItemPopup(adawMenuItem, collection.TypeOfElements, m => ShowImportWizard(collection, m), m => ShowImportMappingWizard(collection, m));
					}
				};
				adawInitialised = true;
			}
		}

		void SetMenuItemVisibleAndEnabled(MenuItem menuItem, bool enabled)
		{
			if (menuItem != null)
			{
				menuItem.Visible = enabled;
				menuItem.Enabled = enabled;
			}
		}

		static ImportSecurityChecker LicenceCheckerForImportingData(EventHandler handler)
		{
			return new ImportSecurityChecker(handler, null);
		}

		#endregion

#endregion

		#region Mass Update Data Tool

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool ShowMassUpdateMenuItem
		{
			get { return showMassUpdateMenuItem && !HasSensitiveColumn; }
			set { showMassUpdateMenuItem = value; }
		}
		bool showMassUpdateMenuItem = true;

		void RunMassUpdateTool()
		{
			using (Form form = new MassUpdateWizardForm(new ZGridImportCollectionInfo(this)))
			{
				form.ShowDialog();
			}
		}

		void AddMassUpdateMenuItem()
		{
			MassUpdateMenuItem = new ZMenuItem(ResString.GetMultilingualString("Grid.MassUpdate", "&Mass Update Data..."), MassUpdateClick);

			ContextMenu.MenuItems.Add(MassUpdateMenuItem);
		}
		MenuItem MassUpdateMenuItem;

		void MassUpdateClick(object sender, EventArgs e)
		{
			RunMassUpdateTool();
		}

		void SetMassUpdateMenuItemVisibleAndEnabled()
		{
			if (MassUpdateMenuItem != null)
			{
				var enabled = !ReadOnly && List != null && List.Count > 0 && (List.AllowNew || List.AllowEdit) && List is IBusinessObjectCollection && !AllColumnsReadOnly
					&& ShowMassUpdateMenuItem && !HasSensitiveColumn;
				MassUpdateMenuItem.Visible = enabled;
				MassUpdateMenuItem.Enabled = enabled;
			}
		}

		#endregion

		#region Grid Events Handlers

		#region Suspend Cancel Of Non-Edited Row On Leaving

		public IDisposable SuspendCancelOfNonEditedRowOnLeaving()
		{
			return new OnLeavingCancelEditSuspender(this);
		}

		class OnLeavingCancelEditSuspender : Disposable
		{
			public OnLeavingCancelEditSuspender(ZGrid grid)
			{
				this.grid = grid;
				grid.IsLeavingCancelEditSuspendCount++;
			}

			readonly ZGrid grid;

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					grid.IsLeavingCancelEditSuspendCount--;
				}
			}
		}

		int IsLeavingCancelEditSuspendCount;

		bool IsLeavingCancelEditSuspended
		{
			get { return IsLeavingCancelEditSuspendCount > 0; }
		}

		#endregion

		#region Suspend Grid State

		public static DisposableGridState SuspendGridState(params ZGrid[] grids)
		{
			return new DisposableGridState(grids);
		}

		public class DisposableGridState : Disposable
		{
			internal DisposableGridState(ZGrid[] grids)
			{
				foreach (var grid in grids)
				{
					var listPosition = grid.ListManager != null ? grid.ListManager.Position : grid.CurrentRowIndex;
					gridSelectionStates.Add(grid, new GridSelectionState { Grid = grid, GridName = grid.Name, CurrentRowIndex = grid.CurrentRowIndex, ListManagerPosition = listPosition });
				}
			}

			public GridSelectionState GetGridState(ZGrid grid)
			{
				return gridSelectionStates.TryGetValue(grid, out var result) ? result : null;
			}

			readonly Dictionary<ZGrid, GridSelectionState> gridSelectionStates = new Dictionary<ZGrid, GridSelectionState>();

			protected override void Dispose(bool isDisposing)
			{
				foreach (var gridStateSuspend in gridSelectionStates.Values)
				{
					var grid = gridStateSuspend.Grid;

					if (grid != null && gridStateSuspend.CurrentRowIndex >= 0)
					{
						grid.CurrentRowIndex = gridStateSuspend.CurrentRowIndex;

						if (grid.ListManager != null)
						{
							grid.ListManager.Position = gridStateSuspend.ListManagerPosition;
						}
					}
				}
			}
		}

		public class GridSelectionState
		{
			public ZGrid Grid { get; internal set; }
			public string GridName { get; internal set; }
			public int CurrentRowIndex { get; internal set; }
			public int ListManagerPosition { get; internal set; }
		}

		#endregion

		protected DataGridCell LastEditedCell;
		protected bool IsChangingCell;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int CurrentRowIndex
		{
			get
			{
				return base.CurrentRowIndex;
			}
			set
			{
				var previousCell = CurrentCell;
				var previousPosition = ListManager?.Position;
				var previousListCount = List?.Count;
				var previousRowIndex = CurrentRowIndex;
				base.CurrentRowIndex = value;
				if (IsRationalRowNumber(previousRowIndex) && !IsRationalRowNumber(CurrentRowIndex))
				{
					ErrorReporter.ReportOnce("ZGrid Array Index Out Of Range Exception in CurrentRowIndex.Set(). ",
						$@"Previous Row Index: {previousRowIndex},
Previous Cell Row Number: {previousCell.RowNumber},
Previous Cell Column Number: {previousCell.ColumnNumber},
Previous Position: {previousPosition},
Previous List Count: {previousListCount},
Current Row Index: {CurrentRowIndex},
Current Cell Row Number: {CurrentCell.RowNumber},
Current Cell Column Number: {CurrentCell.ColumnNumber},
Current Position: {ListManager?.Position},
Current List Count: {List?.Count},
DataGrid Rows Length: {DataGridRowsLength},
firstVisibleCol:{(int)GetValue("firstVisibleCol")},
firstVisibleRow:{(int)GetValue("firstVisibleRow")},
lastTotallyVisibleCol:{(int)GetValue("lastTotallyVisibleCol")},
dataGridRows.Length:{((object[])GetValue("dataGridRows"))?.Length},
DataGrid Visible Row Count: {VisibleRowCount},
value: {value},
Total Columns: {Columns?.Count},
ZGrid Name: {NameForDebugging}
ZGrid Path and Location: {ControlDescription.GetControlPathAndLocation(this)}");
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new DataGridCell CurrentCell
		{
			get
			{
				return base.CurrentCell;
			}
			set
			{
				var previousCell = CurrentCell;
				var previousPosition = ListManager?.Position;
				var previousListCount = List?.Count;

				// Suppress DatabaseUpgradedException since the WinForms DataGrid base property will just catch it. It catches all exceptions.
				// Causing an issue report about it not being handled properly.
				// Ideally DataGrid would allow derived classes to override the exception behaviour.
				var connection = Db.Connection;
				var connectionWasOpen = connection.State == ConnectionState.Open;
				using (connection.IsUpgradeCheckDisabled ? null : new DisposableAction(
					() => connection.IsUpgradeCheckDisabled = true,
					() => connection.IsUpgradeCheckDisabled = false))
				{
					base.CurrentCell = value;
				}
				if (!connectionWasOpen &&
					connection.State == ConnectionState.Open &&
					!connection.IsInTransaction)
				{
					connection.Dispose();
				}

				if (IsRationalRowNumber(previousCell.RowNumber) && !IsRationalRowNumber(CurrentCell.RowNumber))
				{
					ErrorReporter.ReportOnce("ZGrid Array Index Out Of Range Exception in CurrentCell.Set(). ",
						$@"Previous Cell Row Number: {previousCell.RowNumber},
Previous Cell Column Number: {previousCell.ColumnNumber},
Previous Position: {previousPosition},
Previous List Count: {previousListCount},
Current Cell Row Number: {CurrentCell.RowNumber},
Current Cell Column Number: {CurrentCell.ColumnNumber},
Current Position: {ListManager?.Position},
Current List Count: {List?.Count},
DataGrid Rows Length: {DataGridRowsLength},
firstVisibleCol:{(int)GetValue("firstVisibleCol")},
firstVisibleRow:{(int)GetValue("firstVisibleRow")},
lastTotallyVisibleCol:{(int)GetValue("lastTotallyVisibleCol")},
dataGridRows.Length:{((object[])GetValue("dataGridRows"))?.Length},
DataGrid Visible Row Count: {VisibleRowCount},
value Row Number: {value.RowNumber},
value Column Number: {value.ColumnNumber},
Total Columns: {Columns?.Count},
ZGrid Name: {NameForDebugging},
ZGrid Path and Location: {ControlDescription.GetControlPathAndLocation(this)},
Last List Changed Stack Trace: {lastListChangedStackTrace}");
				}
			}
		}

		bool IsRationalRowNumber(int rowNumber)
		{
			return rowNumber > -1 && rowNumber < DataGridRowsLength;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Track info for developers")]
		protected override void OnCurrentCellChanged(EventArgs e)
		{
			using (SuspendResetingRowColorOnItemChanged())
			{
				resetRowsOnNextPositionChanged = false;

				var isOkToChangeCell = true;
				if (LastFocusedColumn != null)
				{
					var lastCell = LastFocusedColumn.LastFocusedCell;
					var lastCellWasEdited = IsCellEdited && LastEditedCell.RowNumber == lastCell.RowNumber;
					if (lastCellWasEdited && !IsChangingCell)
					{
						try
						{
							IsChangingCell = true;
							isOkToChangeCell = LastFocusedColumn.IsValid();
							if (!isOkToChangeCell)
							{
								CurrentCell = lastCell;
							}
						}
						finally
						{
							IsChangingCell = false;
						}
					}
				}

				if (isOkToChangeCell)
				{
					if (LastFocusedColumn != null)
					{
						LastFocusedColumn.HideEditControl();
					}

					if (IsWholeRowSelectedOnClick)
					{
						if (ModifierKeys == Keys.None)
						{
							ResetSelection();
						}

						if (!changingCellByCtrlRowHeaderClick && !movingSelection)
						{
							if (IsSelected(CurrentCell.RowNumber))
							{
								UnSelect(CurrentCell.RowNumber);
							}
							else
							{
								Select(CurrentCell.RowNumber);
							}
						}
					}

					base.OnCurrentCellChanged(e);

					if (IsValidSource(ListManager))
					{
						var isStillOnSameRow = (LastEditedCell.RowNumber == CurrentCell.RowNumber);
						if (ListManager.GetCurrent() is DataRowView rowView && rowView.IsEdit && isStillOnSameRow && IsCellEdited)
						{
							var info = typeof(DataView).GetMethod("OnListChanged", BindingFlags.Instance | BindingFlags.NonPublic);
							info.Invoke(ListManager.List, new object[] { new ListChangedEventArgs(ListChangedType.ItemChanged, ListManager.Position) });
						}

						IsCellEdited = false;
						IsRowEdited &= isStillOnSameRow;
					}

					if (TableStyles.Count > 0 && TableStyles[0].GridColumnStyles[CurrentCell.ColumnNumber] != null)
					{
						UserEventTracker.Instance.AddUserEvent(
							this,
							"DataGrid Cell Changed",
							TableStyles[0].GridColumnStyles[CurrentCell.ColumnNumber].MappingName + " <" + CurrentCell.ToString() + ">");
					}
				}

				if (LastFocusedColumn != null)
				{
					var propertyName = LastFocusedColumn.MappingName;
					var lastFocusedCell = LastFocusedColumn.LastFocusedCell;

					if (lastFocusedCell.ColumnNumber != CurrentCell.ColumnNumber || lastFocusedCell.RowNumber != CurrentCell.RowNumber)
					{
						var rowIndex = lastFocusedCell.RowNumber;
						if (rowIndex < List.Count) // protected against if row gets removed since no user entry
						{
							var bizObj = List[rowIndex] as BusinessObject;
							if (bizObj != null && !bizObj.IsDeleted)
							{
								var property = ZPropertyInfoRetriever.GetZPropertyInfo(LastFocusedColumn, bizObj);
								if (property != null && !property.IsBusinessObjectValidationSuspended())
								{
									((IBusinessObjectInternals)bizObj).Validate(propertyName);
								}
							}
						}
					}
				}

				if (TableStyles.Count > 0)
				{
					LastFocusedColumn = TableStyles[0].GridColumnStyles[CurrentCell.ColumnNumber] as ZTextBoxColumnStyle;
				}
			}
		}
		bool changingCellByCtrlRowHeaderClick;

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			PerformLayout();
		}

		protected override void ColumnStartedEditing(Rectangle bounds)
		{
			if (IsAllowedToStartEditing && !IgnoreNextKeyStroke)
			{
				if (IsAtMaximumRowCount)
				{
					SetCurrentRowNotNew();
				}

				var isCellAlreadyEdited = IsCellEdited;
				IsCellEdited = true;
				IsRowEdited = true;

				LastEditedCell = CurrentCell;
				IsColumnStartingEdit = true;

				if (List != null && List.Count > 0)
				{
					base.ColumnStartedEditing(bounds);
				}

				IsColumnStartingEdit = false;

				if (!isCellAlreadyEdited)
				{
					NotifyColumnEditStart();
				}
			}

			IgnoreNextKeyStroke = false;
		}

		/// <summary>
		/// When the BusinessObject is edited by the grid, set HasChanges to true in case the change has the same value 
		/// as the default value.
		/// </summary>
		protected virtual void NotifyColumnEditStart()
		{
			if (List is IBusinessObjectCollection && List.Count > 0 && CurrentCell.RowNumber < List.Count)
			{
				((BusinessObject)List[CurrentCell.RowNumber]).HasChanges = true;
			}
		}

		internal bool IsAllowedToStartEditing { get; set; }

		internal bool IgnoreNextKeyStroke { get; set; }

		protected bool IsEntering;
		internal bool IsHidingControl;
		bool occuredJustAfterEntering;

		public event EventHandler Entering;

		void OnEntering(EventArgs e) => Entering?.Invoke(this, e);

		protected override void OnEnter(EventArgs e)
		{
			if (!IsEntering && !DesignModeFinder.IsDesigning)
			{
				OnEntering(e);

				IsEntering = true;
				AllowFocus = true;
				occuredJustAfterEntering = true;

				try
				{
					if (!IsHidingControl && !IsChangingEditControl)
					{
						SetInitialCellPosition();
					}

					base.OnEnter(e);
				}
				finally
				{
					IsEntering = false;
				}
			}
		}

		internal bool IsChangingEditControl
		{
			get
			{
				var gridStateField = typeof(DataGrid).GetField("gridState", BindingFlags.NonPublic | BindingFlags.Instance);
				var gridState = (BitVector32)gridStateField.GetValue(this);

				return gridState[0x10000];
			}
		}

		bool IsRowEdited;

		void SetAllowFocusToFalse()
		{
			AllowFocus = false;
		}

		protected virtual void SetInitialCellPosition()
		{
			var cellSetFromMouse = false;
			var mb = MouseButtonState;
			if (mb == MouseButtons.Left || mb == MouseButtons.Right)
			{
				cellSetFromMouse = SetCellFromMouseClick();
			}

			if (!cellSetFromMouse)
			{
				if (IsShiftTabbingIntoControl())
				{
					SetCurrentCellToEndOfGrid();
				}
				else
				{
					if (ListManager != null)
					{
						SetCurrentCellToFirstNonReadOnlyColumn(true);
					}
				}
			}
		}

		static bool IsShiftTabbingIntoControl()
		{
			var tabDown = ((UnsafeNativeMethods.GetAsyncKeyState((int)Keys.Tab) & 0x8000) != 0);
			return (tabDown && ModifierKeys == Keys.Shift);
		}

		bool SetCellFromMouseClick()
		{
			var handled = false;

			var hit = HitTest(LocalMousePosition);
			if (hit.Type == HitTestType.Cell && ListManager != null)
			{
				var previousCell = CurrentCell;
				var previousPosition = ListManager?.Position;
				var previousListCount = List?.Count;
				try
				{
					CurrentCell = new DataGridCell(hit.Row, hit.Column);
				}
				catch (ArgumentException)
				{
					// sometimes it fails to set CurrentCell and we get ArgumentException with message:"CurrentCell cannot be set at this time. Moving your code to the Form.Load event should solve this problem." 
					// in this case it's better just do not set current cell and don't have Runtime Exception thrown to user. WI00066288.
				}
				catch (IndexOutOfRangeException e)
				{
					ErrorReporter.ReportOnce("ZGrid Array Index Out Of Range Exception in SetCellFromMouseClick(). ",
						$@"Previous Cell Row Number: {previousCell.RowNumber},
Previous Cell Column Number: {previousCell.ColumnNumber},
Previous Position: {previousPosition},
Previous List Count: {previousListCount},
Current Cell Row Number: {CurrentCell.RowNumber},
Current Cell Column Number: {CurrentCell.ColumnNumber},
Current Position: {ListManager?.Position},
Current List Count: {List?.Count},
Target Row Number: {hit.Row},
Target Column Number: {hit.Column},
DataGrid Rows Length: {DataGridRowsLength},
firstVisibleCol:{(int)GetValue("firstVisibleCol")},
firstVisibleRow:{(int)GetValue("firstVisibleRow")},
lastTotallyVisibleCol:{(int)GetValue("lastTotallyVisibleCol")},
dataGridRows.Length:{((object[])GetValue("dataGridRows"))?.Length},
DataGrid Visible Row Count: {VisibleRowCount},
Total Columns: {Columns?.Count},
ZGrid Name: {NameForDebugging}
ZGrid Path and Location: {ControlDescription.GetControlPathAndLocation(this)}
Last List Changed Stack Trace: {lastListChangedStackTrace}", e);
				}
				catch (NullReferenceException nullEx)
				{
					ErrorReporter.ReportOnce("ZGridNullReferenceExceptionInSetCellFromMouseClick", string.Format("ListManager.List: {0}; CurrentCell: {1},{2}; hit: {3},{4}; GridNameForDebugging: {5}, Datasource: {6}", ListManager.List?.Count, CurrentCell.RowNumber, CurrentCell.ColumnNumber, hit.Row, hit.Column, NameForDebugging, DataSource?.GetType().FullName), nullEx);
				}
				finally
				{
					handled = true;
				}
			}

			if (hit.Type == HitTestType.RowHeader || hit.Type == HitTestType.ColumnHeader)
			{
				// we don't want to set the current cell to the first cell in case this causes the grid to scroll
				handled = true;
			}

			return handled;
		}

#if DEBUG
		protected virtual
#endif
		MouseButtons MouseButtonState
		{
			get { return MouseButtons; }
		}

#if DEBUG
		protected virtual
#endif
		Point LocalMousePosition
		{
			get { return PointToClient(MousePosition); }
		}

		#region PositionChanged EventHandler Wrapper

		void WrapPositionChangedEventHandler()
		{
			DataGridPositionChangedHandler = (EventHandler)GetValue("positionChangedHandler");
			SetValue("positionChangedHandler", new EventHandler(HandlePositionChanged));
		}

		bool IsCalledFromParentRefreshBinding()
		{
			const int topNLines = 5;

			if (!isInOnMouseDown)
			{
				if (StackTraceUtils.IsCalledFrom("RelatedCurrencyManager", "ParentManager_CurrentItemChanged", topNLines))
				{
					return true;
				}
			}
			return false;
		}

		void HandlePositionChanged(object sender, EventArgs e)
		{
			bool? isCalledFromParentRefreshBinding = null;

			if (ListManager.Position == 0 && previousPosition > 0)
			{
				if ((bool)(isCalledFromParentRefreshBinding = IsCalledFromParentRefreshBinding()))
				{
					ListManager.Position = previousPosition;
				}
			}

			var recreateRows = resetRowsOnNextPositionChanged;
			resetRowsOnNextPositionChanged = false;

			if (IsHandleCreated)
			{
				DataGridPositionChangedHandler.DynamicInvoke(new object[] { sender, e });
				if (recreateRows && DataGridRowsLength > ListManager.Count)
				{
					// todo - don't run this if current cell cannot be set?
					typeof(DataGrid).GetMethod("DataSource_Changed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { sender, e });
				}

				if (EnvProxy.Instance.Registry.LightValidationEnabled && ListManager.Position != previousPosition)
				{
					ValidateOnPositionChangedIfIncreasesSavePerformance();
				}
			}

			if (ListManager.Position != previousPosition && (isCalledFromParentRefreshBinding ?? (bool)(isCalledFromParentRefreshBinding = IsCalledFromParentRefreshBinding())))
			{
				ResetSelection();
			}

			if (IsRationalPosition(previousPosition) && !IsRationalPosition(ListManager.Position))
			{
				ErrorReporter.ReportOnce("ZGrid Array Index Out Of Range Exception in HandlePositionChanged. ",
					$@"Previous Position: {previousPosition},
Current Cell Row Number: {CurrentCell.RowNumber},
Current Cell Column Number: {CurrentCell.ColumnNumber},
Current Position: {ListManager?.Position},
Current List Count: {List?.Count},
DataGrid Rows Length: {DataGridRowsLength},
DataGrid Visible Row Count: {VisibleRowCount},
firstVisibleCol:{(int)GetValue("firstVisibleCol")},
firstVisibleRow:{(int)GetValue("firstVisibleRow")},
lastTotallyVisibleCol:{(int)GetValue("lastTotallyVisibleCol")},
dataGridRows.Length:{((object[])GetValue("dataGridRows"))?.Length},
Total Columns: {Columns?.Count},
ZGrid Name: {NameForDebugging},
ZGrid Path and Location: {ControlDescription.GetControlPathAndLocation(this)}", bindingManagerDataError);
			}

			previousPosition = ListManager.Position;
		}

		bool IsRationalPosition(int position)
		{
			return position > -1 && position < ListManager.Count;
		}

		Exception bindingManagerDataError;

		void ListManager_DataError(object sender, BindingManagerDataErrorEventArgs e)
		{
			bindingManagerDataError = e.Exception;
			if (e.Exception is DatabaseUpgradedException)
			{
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.Exception)
					.Throw();
			}
		}

		protected virtual void ValidateOnPositionChangedIfIncreasesSavePerformance()
		{
			if (ListForNotifications != null && previousPosition >= 0 && previousPosition < ListForNotifications.Count)
			{
				var bo = ListForNotifications[previousPosition] as BusinessObject;
				if (bo != null && ((INeedRow)bo).Row != null && ((INeedRow)bo).Row.RowState != DataRowState.Detached)
				{
					((IBusinessObjectInternals)bo).ValidateShallowIfImprovesPreSaveValidationPerformance();
				}
			}
		}

		int previousPosition;
		bool resetRowsOnNextPositionChanged;
		EventHandler DataGridPositionChangedHandler;

		#endregion

		#region CurrentChanged EventHandler Wrapper

		void WrapCurrentChangedEventHandler()
		{
			dataGridCurrentChangedHandler = (EventHandler)GetValue("currentChangedHandler");
			SetValue("currentChangedHandler", new EventHandler(HandleCurrentChanged));
		}

		void HandleCurrentChanged(object sender, EventArgs e)
		{
			var firstVisibleRow = (int)GetValue("firstVisibleRow");
			var dataGridRows = (object[])GetValue("dataGridRows");

			if (dataGridRows != null && firstVisibleRow < dataGridRows.Length)
			{
				dataGridCurrentChangedHandler.DynamicInvoke(new[] { sender, e });
			}
		}

		object GetValue(string valueName)
		{
			var valueField = typeof(DataGrid).GetField(valueName, BindingFlags.Instance | BindingFlags.NonPublic);
			return valueField.GetValue(this);
		}

		void SetValue(string valueName, object value)
		{
			var valueField = typeof(DataGrid).GetField(valueName, BindingFlags.Instance | BindingFlags.NonPublic);
			valueField.SetValue(this, value);
		}

		EventHandler dataGridCurrentChangedHandler;

		#endregion

		#endregion

		#region Mouse and Keyboard Handlers

		#region Mouse Handlers

		protected HitTestInfo MouseUpInfo;

#if !WINZOR
#if DEBUG
		internal void OnMouseMoveExposed(MouseEventArgs e)
		{
			OnMouseMove(e);
		}
#endif

		bool reEntrantOnMouseMove;

		// drag drop now started in MouseMove rather than MouseClick
		// because this occasionally resulted in inadvertent dragdrops
		// between elements of the same grid if you clicked on a different row.
		// Testing whether the LastDragSucceeded before calling to base has also been canned.
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Points not used for rendering")]
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (IsDisposed || IsDisposing)
			{
				return;
			}

			var hitInfo1 = HitTest(e.X, e.Y);
			if (hitInfo1.Type != HitTestType.RowResize)
			{
				var hitInfo = HitTest(new Point(e.X, e.Y));
				var lastMouseDownHitInfo = HitTest(lastMouseDownPoint);

				if (hitInfo.Type == HitTestType.Cell && !IsSelectionArea(lastMouseDownHitInfo) && IsWholeRowSelectedOnClick)
				{
					try
					{
						reEntrantOnMouseMove = true;
						OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, 1, e.Y, e.Delta));
					}
					finally
					{
						reEntrantOnMouseMove = false;
					}
				}

				if (AllowBeginDrag && e.Button == MouseButtons.Left && MouseMovedFarEnough(e.Location))
				{
					var movedFromHitInfo = HitTest(movedFrom);
					if (movedFromHitInfo.Type == HitTestType.RowHeader && DraggedColumn == null)
					{
						if (isClickingRowHeader && !isOriginatedFromClickingCell)
						{
							DoDragDrop();
						}
						else if (!isClickingLineBetweenRowHeaders && !reEntrantOnMouseMove)
						{
							ShowDragAndDropTips();
						}
					}

					if (isClickingColumnHeader && !isOriginatedFromClickingCell && DraggedColumn != null)
					{
						UpdateDraggedColumn(hitInfo.Column, e.Location);
					}
				}

				base.OnMouseMove(e);
			}

			HideBalloon(e);

			if (LastMouseMoveHitTest != null && (hitInfo1.Row != LastMouseMoveHitTest.Row || hitInfo1.Column != LastMouseMoveHitTest.Column))
			{
				GetValidColumnStyle(LastMouseMoveHitTest.Column)?.OnMouseLeave(ListManager, LastMouseMoveHitTest.Row);
			}

			if (IsValidCellHitInfo(hitInfo1))
			{
				LastMouseMoveHitTest = hitInfo1;
			}
			else
			{
				LastMouseMoveHitTest = null;
			}

			if (DraggedColumn != null)
			{
				System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.Hand);
			}
		}

		bool MouseMovedFarEnough(Point movedTo)
		{
			return Math.Abs(movedTo.X - movedFrom.X) > System.Windows.SystemParameters.MinimumHorizontalDragDistance || Math.Abs(movedTo.Y - movedFrom.Y) > System.Windows.SystemParameters.MinimumVerticalDragDistance;
		}

		void ShowDragAndDropTips()
		{
			if (!isClickingLineBetweenRowHeaders && !reEntrantOnMouseMove && (bool)RawDataRegistry.Instance.ShowZGridDragAndDropInformation.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty))
			{
				using (var messageBox = new ZMessageBoxWithCheckbox(Res.GetString("9c1021ed-626a-4297-9714-54763ece1173", "To perform a drag-and-drop function please drag from the row header. Please check update note below for detailed information."), Res.GetString("e915ca10-ec8e-4bf2-98eb-8a30620888ae", "Drag & Drop Tip"),
					 MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20120627.pdf", RawDataRegistry.Instance.ShowZGridDragAndDropInformation))
				{
					messageBox.ShowDialog();
				}
			}
		}

		HitTestInfo LastMouseMoveHitTest;

		static bool IsSelectionArea(HitTestInfo hitInfo)
		{
			return (hitInfo.Type == HitTestType.None || hitInfo.Type == HitTestType.RowHeader || hitInfo.Type == HitTestType.RowResize);
		}

		int GetRowFromY(int y)
		{
			if (getRowFromYMethod == null)
			{
				getRowFromYMethod = typeof(DataGrid).GetMethod("GetRowFromY", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			return (int)getRowFromYMethod.Invoke(this, new object[] { y });
		}
		MethodInfo getRowFromYMethod;

#endif

		protected bool isClickingRowHeader;
		protected bool isClickingColumnHeader;
#if !WINZOR
		bool isClickingLineBetweenRowHeaders;
#endif
#if DEBUG
		internal
#endif
		bool isMultipleRowsSelectedWhenMouseDown;
		bool isOriginatedFromClickingCell;
		bool mouseDownWasOnExteriorFeature;
		bool mouseDownOnExteriorWhileDropEditOpen;
#if !WINZOR
		Point movedFrom;
#endif

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (IsDisposed || IsDisposing)
			{
				return;
			}

			var initialCanFocus = AllowFocus;
			var hitInfo = HitTest(e.X, e.Y);

			if (hitInfo.Type == HitTestType.RowHeader && e.Clicks == 1)
			{
				Focus();
			}

			if (hitInfo.Type != HitTestType.Cell && (hitInfo.Type != HitTestType.None || occuredJustAfterEntering))
			{
				mouseDownWasOnExteriorFeature = true;
			}
			else
			{
				mouseDownWasOnExteriorFeature = false;
			}

#if WINZOR
			CheckMouseDownOnExteriorWhileDropEditOpen(hitInfo, e);
#else
			isClickingLineBetweenRowHeaders = hitInfo.Type == HitTestType.RowResize;
#endif
			if (!isOriginatedFromClickingCell)
			{
				isClickingRowHeader = hitInfo.Type == HitTestType.RowHeader;
				isClickingColumnHeader = hitInfo.Type == HitTestType.ColumnHeader;
				isMultipleRowsSelectedWhenMouseDown = SelectedRowCount > 1;
			}

			if (e.Button == MouseButtons.Left && hitInfo.Type == HitTestType.ColumnHeader && TableStyles.Count > 0 && hitInfo.Column > -1)
			{
#if !WINZOR
				DraggedColumn = CaptureDraggedColumn(hitInfo.Column);
#endif
				var columnStyle = TableStyles[0].GridColumnStyles[hitInfo.Column] as ZGridColumnStyle;
				if (columnStyle is ZTextBoxColumnStyle textBoxColumnStyle && DevInfoPopupManager.InDevelopInformationMode())
				{
					DevInfoPopupManager.ShowDevelopInfoForm(textBoxColumnStyle.EditControl);
				}
				if (columnStyle != null && !columnStyle.IsSortable)
				{
					return;
				}
			}

			try
			{
				AllowFocus = false;

				ResetFocusToCell = hitInfo.Type == HitTestType.Cell;
				if (hitInfo.Type == HitTestType.RowHeader || hitInfo.Type == HitTestType.RowResize)
				{
					IsRowHeaderClicked = true;
				}

				if (hitInfo.Type != HitTestType.Cell && LastFocusedColumn != null)
				{
					if (!(occuredJustAfterEntering && hitInfo.Type == HitTestType.None && DataGridRowsLength > 1) && !mouseDownOnExteriorWhileDropEditOpen)
					{
						LastFocusedColumn.HideEditControl();
					}
					ValidateIfNotNewRow();
				}

				if (hitInfo.Type != HitTestType.RowResize)
				{
#if !WINZOR
					lastMouseDownPoint = ControlDpiScalingHelper.NewScaledPoint(e.X, e.Y, false);
#endif
					// right mouse button click will also change current selected row 
					// so context menus are more meaningful
					// unless some rows are already selected and right click happens in the selection area
					if (e.Button == MouseButtons.Right)
					{
						cellLocation = MousePosition;
						if ((hitInfo.Type == HitTestType.Cell || hitInfo.Type == HitTestType.RowHeader) && Array.IndexOf(GetSelectedRowIndexes(), hitInfo.Row) == -1)
						{
							OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, e.X, e.Y, e.Delta));
						}
					}
					else if (hitInfo.Type == HitTestType.Cell && IsWholeRowSelectedOnClick)
					{
						ResetSelection();
						if (ListManager.Position != hitInfo.Row)
						{
							ListManager.Position = hitInfo.Row;
						}

						var args = new MouseEventArgs(e.Button, e.Clicks, RowHeaderClickX, e.Y, e.Delta);
#if WINZOR
						// Set temp current hit test data torecreate the CW behaviour
						var hitTest = currentHitTest;
						currentHitTest = new HitTestInfo() { type = HitTestType.RowHeader, col = -1, row = currentHitTest.Row };
#endif
						if (HitTest(args.X, args.Y).Type == HitTestType.RowResize)
						{
							if (HitTest(RowHeaderClickX, e.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(5)).Type == HitTestType.RowHeader)
							{
								args = new MouseEventArgs(e.Button, e.Clicks, RowHeaderClickX, e.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(5), e.Delta);
							}
							else
							{
								args = new MouseEventArgs(e.Button, e.Clicks, RowHeaderClickX, e.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), e.Delta);
							}
						}

						CurrentCell = new DataGridCell(CurrentCell.RowNumber, hitInfo.Column);
						isOriginatedFromClickingCell = true;
						OnMouseDown(args);
#if WINZOR
						// Reset the temp hit test
						currentHitTest = hitTest;
#endif
						OnSelectedRowsChangedInMouseDown();
					}
					else
					{
						try
						{
							changingCellByCtrlRowHeaderClick = ModifierKeys.HasFlag(Keys.Control);

#if !WINZOR
							if (DoesGridHaveTopLeftNotificationIconShowing && IsTopRightClicked(e))
							{
								Invalidate(TopLeftCornerNotificationRectangle);
								NavigateToFirstCellWithHighestPriorityNotification();
							}
							else if (TopLeftCornerGridLayoutRectangle.Contains(e.X, e.Y) &&
									 IsGridLayoutConfigurable &&
									 DataSource != null)
							{
								CustomiseColumns();
							}
#else
							if (DoesGridHaveTopLeftNotificationIconShowing && IsClickingTopLeft)
							{
								NavigateToFirstCellWithHighestPriorityNotification();
							}
#endif
							if (isClickingRowHeader && isMultipleRowsSelectedWhenMouseDown && ModifierKeys == Keys.None)
							{
								var selectedRowIndexes = GetSelectedRowIndexes();
								base.OnMouseDown(e);
								if (selectedRowIndexes.Contains(CurrentRowIndex))
								{
									SetSelectedRows(selectedRowIndexes);
								}
							}
							else
							{
								isInOnMouseDown = true;
								base.OnMouseDown(e);
							}
#if DEBUG
							if (ExceptionForTest != null)
							{
								throw ExceptionForTest;
							}
#endif
							OnSelectedRowsChangedInMouseDown(); //have to call it again for pieces of code that expects selected row list to be up to date now
						}
						catch (ArgumentOutOfRangeException ex)
						{
							var totalColumns = Columns == null ? "null" : Columns.Count.ToString(CultureInfo.CurrentCulture);
							var currentCol = (int)typeof(DataGrid).GetField("currentCol", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
							var gridColumnStylesInfo = new StringBuilder();
							var allColumns = new StringBuilder();
							foreach (DataGridTableStyle tableStyle in TableStyles)
							{
								if (tableStyle != null)
								{
									gridColumnStylesInfo.AppendLine($"	Table Mapping Name: {tableStyle.MappingName}, Column Style Count: {tableStyle.GridColumnStyles?.Count}");
								}
							}
							foreach (var column in Columns)
							{
								if (column != null)
								{
									allColumns.AppendLine($"	[Column Name:{column.ColumnName}, Is Visible: {column.IsVisible}]");
								}
							}
							ErrorReporter.ReportOnce("ArgumentOutOfRangeException_InZGridOnMouseDown", $@"{ex.Message}
ZGrid:{ControlDescription.GetControlPath(this)}
Total Columns: {totalColumns}
First Visible Column: {FirstVisibleColumn}
Current cell: {CurrentCell}
DataGrid CurrentCol: {currentCol}
HitInfo: {hitInfo}
EventArgs: ({e.Button}, {e.Location})
GridColumnStyles:{gridColumnStylesInfo}
Is Originated From Clicking Cell: {isOriginatedFromClickingCell}
Is Clicking RowHeader: {isClickingRowHeader}
Is Multiple Rows Selected When MouseDown: {isMultipleRowsSelectedWhenMouseDown}
All Columns: {allColumns} 
", ex);
						}
						catch (IndexOutOfRangeException ex)
						{
							var currentRowFieldInfo = typeof(DataGrid).GetField("currentRow", BindingFlags.Instance | BindingFlags.NonPublic);
							var currentRow = (int)currentRowFieldInfo.GetValue(this);
							var dataGridRows = (object[])typeof(DataGrid).GetField("dataGridRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
							var dataGridRowsDotLength = dataGridRows?.Length ?? -1;
							ErrorReporter.ReportOnce("IndexOutOfRangeException_InZGridOnMouseDown",
																			string.Format("{0}\r\nVisible {1} rows\r\nDataGridRowsLength: {2}\r\nDataGridRows.Length: {3}\r\nTotal Columns: {4}\r\nList Count: {5}\r\nCurrent row: {6}\r\nHitInfo: {7}\r\nEventArgs: {{{8}, {9}}}\r\nStack:\r\n{10}",
																			ex.Message, VisibleRowCount, DataGridRowsLength, dataGridRowsDotLength,
																			this.Columns == null ? "null" : this.Columns.Count.ToString(CultureInfo.CurrentCulture),
																			this.List == null ? "null" : this.List.Count.ToString(CultureInfo.CurrentCulture), currentRow, hitInfo.ToString(),
																			e.Button, e.Location, ex.StackTrace), ex);
						}
						finally
						{
							changingCellByCtrlRowHeaderClick = false;
							isInOnMouseDown = false;
						}
					}
				}

				IsRowHeaderClicked = false;
			}
			finally
			{
				AllowFocus = initialCanFocus;
			}

#if !WINZOR
			movedFrom = e.Location;
#endif
		}

#if DEBUG
		internal Exception ExceptionForTest;
#endif

		void OnSelectedRowsChangedInMouseDown()
		{
			SelectedRowsChangedInMouseDown?.Invoke(this, new EventArgs());
		}

		public event EventHandler SelectedRowsChangedInMouseDown;

#if !WINZOR

		static protected bool IsTopRightClicked(MouseEventArgs e)
		{
			return TopLeftCornerNotificationRectangle.Contains(e.X, e.Y);
		}
#if DEBUG
		internal
#endif
		int RowHeaderClickX
		{
			get { return Math.Max(0, TableStyles[0].RowHeaderWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(15)); }
		}

#endif

#if !WINZOR
		Point lastMouseDownPoint;
#endif

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Points not used for rendering")]
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (IsDisposed || IsDisposing)
			{
				return;
			}

			if (translationFeedbackManager.InMode)
			{
				translationFeedbackManager.HandleClick();
				return;
			}

			if (CurrentRowIndex >= DataGridRowsLength)
			{
				return;
			}

			var newSorts = NewSorts(e);

			var hitInfo = HitTest(e.X, e.Y);

#if WINZOR
			// Simulates the CW behaviour where drop edit blocks the behaviour of clicking a coloumn header
			if (mouseDownOnExteriorWhileDropEditOpen && hitInfo.Type == HitTestType.ColumnHeader)
			{
				mouseDownOnExteriorWhileDropEditOpen = false;
				base.OnMouseUp(e);
				return;
			}
#endif
			mouseDownOnExteriorWhileDropEditOpen = false;

			if (hitInfo.Type == HitTestType.RowHeader && isMultipleRowsSelectedWhenMouseDown && ModifierKeys == Keys.None && e.Button == MouseButtons.Left)
			{
				ResetSelection();
				if (CurrentRowIndex != -1)
				{
					Select(CurrentRowIndex);
					typeof(DataGrid).GetField("lastRowSelected", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, CurrentRowIndex); //Fixes cases where click into shift+click didn't make the selection after ending a previous shift+click selection
				}
			}

			isMultipleRowsSelectedWhenMouseDown = false;
			isClickingRowHeader = false;
			isClickingColumnHeader = false;
			isOriginatedFromClickingCell = false;

			if (hitInfo.Type == HitTestType.ColumnHeader)
			{
				//HACK: Fix .NET Bug where detached row disappears on sort of grid bound to data relation
				if (IsValidSource(ListManager))
				{
					FinishAnyUncommittedRow();
				}
			}

			object previousSelection = null;
			object[] previousSelections = null;

			if (hitInfo.Type == HitTestType.Cell && IsWholeRowSelectedOnClick && !IsTrackingColumnResize)
			{
#if WINZOR
				// Set temp current hit test data torecreate the CW behaviour
				var hitTest = currentHitTest;
				currentHitTest = new HitTestInfo() { type = HitTestType.RowHeader, col = -1, row = currentHitTest.Row };
#endif
				OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, 1, e.Y, e.Delta));
#if WINZOR
				// Reset the hit test info
				currentHitTest = hitTest;
#endif
			}
			else
			{
#if !WINZOR
				lastMouseDownPoint = new Point(0, 0);
#endif
				if (e.Button == MouseButtons.Left && HitTest(e.X, e.Y).Type == HitTestType.ColumnHeader)
				{
					Columns.HasLayoutChanged = true;
					SaveLastSelectedLayout = true;
				}
				if (ListManager != null && ListManager.List != null && ListManager.Position >= 0 && ListManager.Position < ListManager.List.Count)
				{
					previousSelection = ListManager.List[ListManager.Position];
				}

				if (hitInfo.Type == HitTestType.ColumnHeader && !CheckGridRowsOutOfSyncWithListManager())
				{
					previousSelections = SelectedElements;
				}

				base.OnMouseUp(e);
			}

			if (AllowSorting)
			{
				if (newSorts != null)
				{
					ApplyNewSorts(newSorts);
				}
				else
				{
					RemoveNewSorts();
				}
			}

			if (hitInfo.Type == HitTestType.ColumnHeader)
			{
				UpdateFrozenSort();
#if WINZOR
				Invalidate();
#endif
				if (previousSelections != null)
				{
					UpdatePreviousSelections(previousSelection, previousSelections);
				}
			}

#if !WINZOR
			if (DraggedColumn != null)
			{
				SortColumns(hitInfo.Column, DraggedColumn.Index);
				System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.Arrow);
				DraggedColumn = null;
				Invalidate();
			}
#endif
			if (mouseDownWasOnExteriorFeature && SelectedRowCount < 1 && DataGridRowsLength > 1 && TableStyles.Count > 0 &&
				CurrentCell.ColumnNumber >= 0 && CurrentCell.ColumnNumber < TableStyles[0].GridColumnStyles.Count &&
				!IsCellReadOnly(CurrentCell.RowNumber, CurrentCell.ColumnNumber))
			{
				var currentColumnStyle = TableStyles[0].GridColumnStyles[CurrentCell.ColumnNumber];
				if (currentColumnStyle != null)
				{
					BeginEdit(currentColumnStyle, CurrentCell.RowNumber);
				}
				mouseDownWasOnExteriorFeature = false;
			}

			occuredJustAfterEntering = false;
		}
#if DEBUG
		internal ScrollBar HorizScrollBarExposed => HorizScrollBar;
#endif

#if DEBUG
		protected virtual
#endif
		void ApplyNewSorts(ListSortDescriptionCollection newSorts)
		{
			((IBindingListView)ListManager.List).ApplySort(newSorts);
		}

#if DEBUG
		protected virtual
#endif
		void RemoveNewSorts()
		{
			if (ListManager?.List is BusinessObjectCollection collection)
			{
				collection.RemoveSorts();
			}
		}

		bool isInOnMouseDown;

		void UpdateFrozenSort()
		{
			var activeCollection = ListManager == null ? null : ListManager.List as IActiveBusinessObjectCollection;
			if (activeCollection != null &&
				activeCollection.SortComparer != null &&
				!(activeCollection.SortComparer is IFreezeSortOnElementModifyComparer) &&
				FreezeSortOnGridCollectionElementModifyAttribute.IsEnabled(ListManager.List))
			{
				activeCollection.ApplySort(FreezeSortOnElementModifyComparer.Wrap(activeCollection.SortComparer));
			}
		}

		int OneOffWithinRange(int index, int min, int max)
		{
			if (min >= max)
			{
				return min;
			}
			if (index <= min)
			{
				return min + 1;
			}
			if (index >= max)
			{
				return max - 1;
			}
			return index - 1;
		}

		void UpdatePreviousSelections(object previousSelection, object[] previousSelections)
		{
			if ((previousSelections != null && previousSelections.Length > 0) || !ReadOnly)
			{
				var originalHorizScrollBar = HorizScrollBar.Value;
				UnSelectAll();

				if (previousSelection != null)
				{
					var index = ListManager.List.IndexOf(previousSelection);
					if (index >= 0)
					{
						if (ListManager.Position == index)
						{
							//the ZGrid will not scroll unless we change the position to a NEW value
							ListManager.Position = OneOffWithinRange(index, 0, ListManager.Count - 1);
						}
						ListManager.Position = index;
					}
				}

				foreach (var selection in previousSelections)
				{
					var index = List.IndexOf(selection);
					if (index >= 0)
					{
						Select(index);
					}
				}
				// re-scroll back to where it was after reselecting the row
				GridHScrolled(this, new ScrollEventArgs(ScrollEventType.LargeIncrement, originalHorizScrollBar));
				GridHScrolled(this, new ScrollEventArgs(ScrollEventType.EndScroll, originalHorizScrollBar));
			}
		}

		bool IsTrackingColumnResize
		{
			get
			{
				var gridStateField = typeof(DataGrid).GetField("gridState", BindingFlags.NonPublic | BindingFlags.Instance);
				var gridState = (BitVector32)gridStateField.GetValue(this);

				return gridState[8];
			}
		}

#if !WINZOR
#if DEBUG
		internal void OnMouseHoverExposed(EventArgs e)
		{
			OnMouseHover(e);
		}
#endif

		protected override void OnMouseHover(EventArgs e)
		{
			if (IsDisposed || IsDisposing)
			{
				return;
			}

			base.OnMouseHover(e);

			SafeNativeMethods.TrackMouse(Handle);

			var mousePosition = GetMousePositionInGrid();
			if (TopLeftCornerNotificationRectangle.Contains(mousePosition))
			{
				ShowGridCornerBalloon();
			}
			else
			{
				var info = HitTest(mousePosition);

				if (info.Type == HitTestType.RowHeader && info.Row >= 0 && ShouldShowNotifications)
				{
					var columnNotificationRect = GetRowNotificationRectangle(info.Row);
					if (columnNotificationRect.Contains(mousePosition) &&
						ListForNotifications != null &&
						info.Row < ListForNotifications.Count)
					{
						var bO = (IBusiness)ListForNotifications[info.Row];
						if (bO.HasNotifications())
						{
							ShowGridColumnBalloon(bO, columnNotificationRect, this);
						}
					}
				}
				else if (IsValidCellHitInfo(info))
				{
					var cellBounds = GetCellBounds(info.Row, info.Column);
					var mouseLocationInCell = ControlDpiScalingHelper.NewScaledPoint(mousePosition.X - cellBounds.Left, mousePosition.Y - cellBounds.Top, false);
					var column = GetValidColumnStyle(info.Column);
					if (column != null)
					{
						column.OnMouseHover(ListManager, info.Row, mouseLocationInCell, cellBounds);
					}
				}
				InvalidateTopLeftCornerGridLayoutRectangle(mousePosition);
			}
		}

		void InvalidateTopLeftCornerGridLayoutRectangle(Point mousePosition)
		{
			if (IsGridLayoutConfigurable)
			{
				if (TopLeftCornerGridLayoutRectangle.Contains(mousePosition))
				{
					Invalidate(TopLeftCornerGridLayoutRectangle);
				}
				else
				{
					if (isActiveGridLayoutIconDrawn)
					{
						Invalidate(TopLeftCornerGridLayoutRectangle);
					}
				}
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (IsDisposed || IsDisposing)
			{
				return;
			}

			base.OnMouseLeave(e);

			if (isActiveGridLayoutIconDrawn)
			{
				Invalidate(TopLeftCornerGridLayoutRectangle);
			}

			isOriginatedFromClickingCell = false;
			isClickingRowHeader = false;
			isClickingColumnHeader = false;
		}

#endif

		protected override void OnLeave(EventArgs e)
		{
			try
			{
				inOnLeave = true;
				if (!IsDisposing && !DesignModeFinder.IsDesigning)
				{
					occuredJustAfterEntering = false;
					AllowFocus = false;

					if (!ContainsFocus)
					{
						ResetFocusToCell = false;
					}

					FinishAnyUncommittedRow();

					if (LastFocusedColumn != null && !IsDisposed)
					{
						LastFocusedColumn.HideEditControl();
					}
				}

				if (!IsLeavingCancelEditSuspended)
				{
					if (!IsDisposed)
					{
						ValidateIfNotNewRow();
					}

					try
					{
						base.OnLeave(e);
					}
					catch (IndexOutOfRangeException)
					{
						// nothing can be done, this may be caused by the BindingContext changing due to changing of tab visibility or similar
					}
				}

				if (IsHandleCreated && !DesignModeFinder.IsDesigning && !IsDisposing)
				{
					BeginInvoke(new MethodInvoker(SetAllowFocusToFalse)); // base sets this back to true in an async callback
				}
			}
			finally
			{
				inOnLeave = false;
				IsCellEdited = false;
				IsRowEdited = false;
			}
		}

		void IMenuParent.BeforeProcessingMenuItem()
			=> FinishAnyUncommittedRow(true);

		void FinishAnyUncommittedRow(bool preferCommit = false)
		{
			if (ListManager != null)
			{
				if (ShouldRefreshManager() && (preferCommit || IsCellEdited || IsRowEdited))
				{
					TryToEndCurrentEdit();
					RefreshManager();
				}
				else
				{
					CancelCurrentEditIfNotEdited();
				}
			}
		}

		internal void CancelCurrentEditIfNotEdited()
		{
			if (!IsCellEdited && !IsRowEdited && !IsLeavingCancelEditSuspended)
			{
				try
				{
					ListManager.CancelCurrentEdit();
				}
				catch (IndexOutOfRangeException)
				{
					// nothing can be done, this may be caused by the BindingContext changing due to changing of tab visibility or similar
				}
			}
		}

		[ThreadStatic]
		static bool inOnLeave;

#endregion

		#region Keyboard Handlers

#if DEBUG
		internal bool processDialogKeyResult;
		internal bool ProcessDialogKeyExposed(Keys keyData)
		{
			return ProcessDialogKey(keyData);
		}
#endif

		[SuppressMessage("CargoWiseOne", "CW1118:UseKeysKeyCodeAndKeysModifiersBitmask", Justification = "The user may type more than one modifiers to trigger a copy operation.")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "The user may type more than one modifiers to trigger a copy operation., Developer exception")]
		protected override bool ProcessDialogKey(Keys keyData)
		{
			var result = false;

			if ((keyData & Keys.KeyCode) == Keys.C && (keyData & Keys.Control) != Keys.None && (keyData & Keys.Alt) == Keys.None)
			{
				if (SelectedElements.Length > 0) // Skip when no row is selected. Otherwise it will mark the key stroke as handled here when copying value from a cell.
				{
					if (CopySelectedRowsAllowed)
					{
						CopySelectedRows();
						result = true;
					}
					else
					{
						ShowCannotCopyMessage();
					}
				}
			}
			else
			{
				var canNavigateGrid = true;

				if (LastFocusedColumn is INavigatingGridColumn currentColumn)
				{
					canNavigateGrid = !currentColumn.ShouldColumnHandleKey(keyData);
				}

				if (canNavigateGrid)
				{
					try
					{
						movingSelection = IsSelectingUp(keyData) || IsSelectingDown(keyData) || IsSelectingToTop(keyData) ||
											IsSelectingToBottom(keyData);

						if (ProcessKeyDataWhenNavigatingGrid(keyData))
						{
							result = true;
						}
						else if (keyData == Keys.Enter && !IsWholeRowSelectedOnClick)
						{
							EndEdit(); // end the edit to make sure previous changes work before we change current cell.

							var firstEditableColumn = GetFirstEditableColumnIndex(CurrentCell.RowNumber + 1);
							if (firstEditableColumn >= VisibleColumnCount)
							{
								firstEditableColumn = 0;
							}

							var previousCell = CurrentCell;
							var previousPosition = ListManager?.Position;
							var previousListCount = List?.Count;
							try
							{
								CurrentCell = new DataGridCell(CurrentCell.RowNumber + 1, firstEditableColumn);
								result = true;
							}
							catch (IndexOutOfRangeException e)
							{
								ErrorReporter.ReportOnce("ZGrid Array Index Out Of Range Exception in ProcessDialogKey() 1. ",
									$@"Previous Cell Row Number: {previousCell.RowNumber},
Previous Cell Column Number: {previousCell.ColumnNumber},
Previous Position: {previousPosition},
Previous List Count: {previousListCount},
Current Cell Row Number: {CurrentCell.RowNumber},
Current Cell Column Number: {CurrentCell.ColumnNumber},
Current Position: {ListManager?.Position},
Current List Count: {List?.Count},
DataGrid Rows Length: {DataGridRowsLength},
DataGrid Visible Row Count: {VisibleRowCount},
FirstEditableColumn: {firstEditableColumn},
Total Columns: {Columns?.Count},
firstVisibleCol:{(int)GetValue("firstVisibleCol")},
firstVisibleRow:{(int)GetValue("firstVisibleRow")},
lastTotallyVisibleCol:{(int)GetValue("lastTotallyVisibleCol")},
dataGridRows.Length:{((object[])GetValue("dataGridRows"))?.Length},
ZGrid Name: {NameForDebugging},
ZGrid Path and Location: {ControlDescription.GetControlPathAndLocation(this)},
Keys: {keyData}", e);
							}
						}
						else if (IsShiftSpaceCombination(keyData))
						{
							// they also have ProcessKeyPreview overriden
							// we need to hard reset so it will not trap in ProcessKeyPreview
							var originStateOfShiftKey = ResetShiftKeyboardState();
							result = base.ProcessDialogKey(Keys.Space);
							SetShiftKeyboardState(originStateOfShiftKey);
						}
						else if (keyData != Keys.Enter)
						{
							result = base.ProcessDialogKey(keyData);
							if (movingSelection)
							{
								OnSelectedRowsChangedInMouseDown(); //have to call it again for pieces of code that expects selected row list to be up to date now
							}
						}
					}
					catch (IndexOutOfRangeException e)
					{
						ErrorReporter.ReportOnce("ZGrid Array Index Out Of Range Exception. in ProcessDialogKey() 2",
							$@"Previous Position: {previousPosition},
Current Cell Row Number: {CurrentCell.RowNumber},
Current Cell Column Number: {CurrentCell.ColumnNumber},
Current Position: {ListManager?.Position},
Current List Count: {List?.Count},
DataGrid Rows Length: {DataGridRowsLength},
DataGrid Visible Row Count: {VisibleRowCount},
Total Columns: {Columns?.Count},
ZGrid Name: {NameForDebugging},
Keys: {keyData}", e);
					}
					catch (NullReferenceException ex)
					{
						Globals.Message.ShowDeveloperException("CommitEdit failure - NullReferenceException", ex);
					}
					catch (InvalidCastException)
					{
						try
						{
							typeof(DataGrid).GetMethod("RecreateDataGridRows", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, Array.Empty<object>());
							result = base.ProcessDialogKey(keyData);
						}
						catch (InvalidCastException e2)
						{
							var dataGridRows = (object[])typeof(DataGrid).GetField("dataGridRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
							var policy = typeof(DataGrid).GetField("policy", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
							bool allowAdd = (bool)policy.GetType().GetField("allowAdd", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(policy);
							var rowsInformation = string.Format(
								"policy.AllowAdd: {0}\r\nAllowNew: {1}\r\nthis.List.AllowNew: {2}\r\nInformation in DataGridRows:",
								allowAdd, AllowNew, this.List?.AllowNew ?? false);
							foreach (var row in dataGridRows)
							{
								rowsInformation += string.Format("\r\n{0}: {1}", row.GetType().GetField("number", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(row), row.GetType().ToString());
							}
							ErrorReporter.ReportOnce("InvalidCastException in ProcessDialogKey", rowsInformation, e2);
						}
					}
					finally
					{
						movingSelection = false;
					}
				}
			}
			occuredJustAfterEntering = false;

#if DEBUG
			processDialogKeyResult = result;
#endif
			return result;
		}

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.Tab, GoToNextCellHotkey, Res.GetString("ea401a8a-e807-4d66-8fe2-24cae8b68efe", "Go to next cell"));
			Hotkeys.RegisterHotKey(Keys.Shift | Keys.Tab, GoToNextCellHotkey, Res.GetString("4f0555e2-6edd-45a6-afb5-4e10c1e212b7", "Go to previous cell"));

			Hotkeys.RegisterHotKey(Keys.F9, CopyPreviousValueHotkey, Res.GetString("14a49117-0ab6-4aad-90aa-8e5a93637c1d", "Copy previous value"));
			Hotkeys.RegisterHotKey(Keys.F2, SelectEndColumnHotkey, Res.GetString("bdd6c0d3-8fff-4de1-bbcb-eb6ccf05e2e3", "Go to end"));

			Hotkeys.RegisterHotKey(Keys.Alt | Keys.F9, CopyAboveValueAndMoveDownHotkey, Res.GetString("08753760-9631-4EB2-B3F2-3FB8A9038CDA", "Copy above value and move down"));
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.F8, CopyBelowValueAndMoveUpHotkey, Res.GetString("EF9A2610-18B2-44E4-BF6F-C394A119FB04", "Copy below value and move up"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Shift | Keys.L, SelectCurrentRowHotkey, Res.GetString("51F7181B-77AD-4505-9EDC-CB5773BC176B", "Select current row"));

			Hotkeys.AddDescription(Keys.Delete, Res.GetString("af03abb5-d636-496a-b1ca-3ee882bab38e", "Delete current row"));
		}

		IEnumerable<IHotkeyProvider> IAdditionalHotkeyProviders.OtherProviders
		{
			get
			{
				if (LastFocusedColumn is IHotkeyProvider currentColumn)
				{
					yield return currentColumn;
				}
			}
		}
#if DEBUG
		internal bool ProcessCmdKeyExposed(ref Message msg, Keys keyData)
		{
			return ProcessCmdKey(ref msg, keyData);
		}
#endif
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			try
			{
				inProcessCmdKey = true;
				occuredJustAfterEntering = false;

				if (LastFocusedColumn is ICustomKeyHandlingGridColumn currentColumn && currentColumn.ShouldProcessCmdKey(ref msg, keyData))
				{
					return currentColumn.ProcessCmdKey(ref msg, keyData);
				}

				if (IsWholeRowSelectedOnClick && IsWholeRowSelectedOnClickKeys(keyData))
				{
					return true;
				}

				if ((Keys.KeyCode & keyData) == Keys.Delete && DeleteCurrentRowHotkey(keyData))
				{
					return true;
				}

				if ((keyData & Keys.Modifiers) == (Keys.Control | Keys.Shift))
				{
					if (ListManager != null)
					{
						ListManager.EndCurrentEdit();
					}
				}

				return Hotkeys.ProcessCmdKey(this, keyData) || base.ProcessCmdKey(ref msg, keyData);
			}
			finally
			{
				inProcessCmdKey = false;
			}
		}

		protected virtual bool ProcessKeyDataWhenNavigatingGrid(Keys keyData)
		{
			return false;
		}

		protected virtual void ShowCannotCopyMessage()
		{
		}

		bool SelectCurrentRowHotkey(object sender, Keys keyPressed)
		{
			if (DataGridRowsLength > 0)
			{
				Select(CurrentCell.RowNumber);
			}
			return true;
		}

		bool CopyPreviousValueHotkey(object sender, Keys keyPressed)
		{
			if (CopyAdjacentValueIfPossible(comparativeRowIndex: -1))
			{
				ProcessTabKeyWithReadOnlySkipping(forward: true);
				return true;
			}

			return false;
		}

		bool CopyAboveValueAndMoveDownHotkey(object sender, Keys keyPressed)
		{
			CopyAdjacentValueIfPossible(comparativeRowIndex: -1);
			CurrentCell = new DataGridCell(CurrentRowIndex + 1, CurrentCell.ColumnNumber);
			return true;
		}

		bool CopyBelowValueAndMoveUpHotkey(object sender, Keys keyPressed)
		{
			CopyAdjacentValueIfPossible(comparativeRowIndex: 1);
			if (CurrentRowIndex - 1 >= 0)
			{
				CurrentCell = new DataGridCell(CurrentRowIndex - 1, CurrentCell.ColumnNumber);
			}
			return true;
		}

		bool SelectEndColumnHotkey(object sender, Keys keyPressed)
		{
			return ReadOnly || LastFocusedColumn == null || LastFocusedColumn.SelectEnd();
		}

		bool DeleteCurrentRowHotkey(Keys keyPressed)
		{
			UserEventTracker.Instance.AddUserEvent(this, "KeyPress", keyPressed.ToString());
			return OnDeleteKeyPressed();
		}

#if DEBUG
		public
#endif
		bool OnDeleteKeyPressed()
		{
			var result = false;

			if (SelectedRowCount > 0)
			{
				result = true;

				if (List.AllowRemove && CanDeleteCurrentRow(CurrentCell.RowNumber))
				{
					EndEdit();

					var rowsDeleted = HandleDelete(CurrentCell.RowNumber);
					if (rowsDeleted > 0 && LastFocusedColumn != null && DataGridRowsLength > 1) //open new edit if we can
					{
						BeginEdit(LastFocusedColumn, CurrentCell.RowNumber);
					}
					else if (rowsDeleted > 0 && CurrentCell.RowNumber == 0 && LastFocusedColumn != null)
					{
						OnLeave(new EventArgs()); //fix a bug where if you open an edit on row 0, ctrl+a then delete, there is still an editable control open
					}
				}
			}
			// Disable processing Delete key on readonly cells
			else if (SelectedRowCount == 0 && IsCellReadOnly(CurrentCell.RowNumber, CurrentCell.ColumnNumber))
			{
				result = true;
			}

			return result;
		}

		bool GoToNextCellHotkey(object sender, Keys keyPressed)
		{
			return ProcessTabKeyWithReadOnlySkipping(forward: !keyPressed.HasFlag(Keys.Shift));
		}

		[ThreadStatic]
		static bool inProcessCmdKey;

		bool CopyAdjacentValueIfPossible(int comparativeRowIndex)
		{
			if (ReadOnly)
			{
				return false;
			}

			var columnNo = CurrentCell.ColumnNumber;
			var rowNo = CurrentRowIndex;
			if (rowNo + comparativeRowIndex < 0 || rowNo + comparativeRowIndex >= ListManager.Count)
			{
				return false;
			}

			var copyBO = ListManager.List[rowNo + comparativeRowIndex] as BusinessObject;
			var thisBO = ListManager.GetCurrent() as BusinessObject;
			if (thisBO == null || copyBO == null)
			{
				return false;
			}

			if (!(GetValidColumnStyle(columnNo) is ZGridColumnStyle columnStyle) || columnStyle.ReadOnly)
			{
				return false;
			}

			if (thisBO.GetPossiblyCustomPropertyReadOnly(columnStyle.MappingName))
			{
				return false;
			}

			var info = ZPropertyInfoRetriever.GetZPropertyInfo(columnStyle, thisBO);
			if (info != null && info.ReadOnly)
			{
				return false;
			}

			var value = copyBO.GetPossiblyCustomProperty(columnStyle.MappingName);
			if (value == null)
			{
				return false;
			}

			ColumnStartedEditing(GetCellBounds(CurrentCell));

			if (columnStyle is ZMultiControlColumnStyle controlColumnStyle)
			{
				controlColumnStyle.EditControl.CurrentEditor.Text = columnStyle.GetValueAsString(ListManager, rowNo + comparativeRowIndex);
			}
			else
			{
				thisBO.SetPossiblyCustomProperty(columnStyle.MappingName, value);
			}

			if (info != null)
			{
				info.RefreshBinding();
			}
			else
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Current property info is null. Column propertyDescriptor: {0}, column mapping name: {1}", columnStyle.PropertyDescriptor != null ? columnStyle.PropertyDescriptor.GetType().ToString() : "NULL", columnStyle.MappingName));
			}

			return true;
		}

		/// <summary>
		/// Prevents the Column Edit from occurring by Cell Change when Selecting a Row
		/// </summary>
		protected override bool ProcessKeyPreview(ref Message m)
		{
			var canNavigateGrid = true;

			var keyData = KeyDataFromMessage(m);
			if (LastFocusedColumn is INavigatingGridColumn currentColumn)
			{
				canNavigateGrid = !currentColumn.ShouldColumnHandleKey(keyData);
			}

			if (canNavigateGrid)
			{
				movingSelection = IsSelectingUp(keyData) || IsSelectingDown(keyData) || IsSelectingToTop(keyData) || IsSelectingToBottom(keyData);

				bool result;
				try
				{
					using (List is IBusinessObjectCollectionView businessObjectCollectionView && IsChangingCurrentCellInSameRow(keyData) ? businessObjectCollectionView.SuspendRemovingWhenOnlyOneElementLeftForListChanged() : null)
					{
						result = base.ProcessKeyPreview(ref m);
					}
				}
				finally
				{
					movingSelection = false;
				}

				return result;
			}
			return false;
		}

		static protected Keys KeyDataFromMessage(Message m)
		{
			return (Keys)((int)m.WParam | (int)ModifierKeys);
		}

		#region API Invoke

		[DllImport("user32.dll")]
		public static extern int GetKeyboardState(byte[] lpKeyState);

		[DllImport("user32.dll")]
		public static extern int SetKeyboardState(byte[] lppbKeyState);

		static bool IsShiftSpaceCombination(Keys keyData)
		{
			return keyData == (Keys.Shift | Keys.Space);
		}
#if DEBUG
		internal
#endif
		static byte ResetShiftKeyboardState()
		{
			byte originValueOfShiftKey;
			var keystates = new byte[256];
			GetKeyboardState(keystates);    // query keyboard state
			originValueOfShiftKey = keystates[16];
			keystates[16] = 0;              // turn off the shift key
			SetKeyboardState(keystates);    // reset the keyboard
			return originValueOfShiftKey;
		}

		internal static void SetShiftKeyboardState(byte originValueOfShiftKey)
		{
			var keystates = new byte[256];
			GetKeyboardState(keystates);    // query keyboard state
			keystates[16] = originValueOfShiftKey;          // turn on the shift key
			SetKeyboardState(keystates);    // reset the keyboard
		}

		#endregion

		#endregion

#endregion

		#region DragDropColumns

		protected class DraggedDataGridColumn
		{
			public DraggedDataGridColumn(int index, Rectangle rect, Bitmap image = null)
			{
				Index = index;
				InitialRect = rect;
				ColumnImage = image;
			}

			public int Index { get; }
			public Rectangle InitialRect { get; }
			public Bitmap ColumnImage { get; }

			public Rectangle CurrentRect { get; set; }

			public Rectangle MouseOverColumnRect { get; set; }
			public int MouseOverColumnIndex { get; set; }
		}

		DraggedDataGridColumn DraggedColumn;

#if !WINZOR

		DraggedDataGridColumn CaptureDraggedColumn(int columnNum)
		{
			var columnRect = GetHeaderBounds(columnNum);
			var columnImage = ZScreenShotGrabber.CaptureSection(Handle, columnRect.Location, columnRect.Size);

			return new DraggedDataGridColumn(columnNum, columnRect, columnImage)
			{
				CurrentRect = columnRect
			};
		}

		void UpdateDraggedColumn(int columnAtMouse, Point mousePos)
		{
			Invalidate();

			if (columnAtMouse >= 0)
			{
				if (DraggedColumn.MouseOverColumnRect != Rectangle.Empty)
				{
					Invalidate(DraggedColumn.MouseOverColumnRect);
				}

				DraggedColumn.MouseOverColumnIndex = columnAtMouse == DraggedColumn.Index ? -1 : columnAtMouse;
				DraggedColumn.MouseOverColumnRect = GetHeaderBounds(columnAtMouse);

				Invalidate(DraggedColumn.MouseOverColumnRect);

				DraggedColumn.CurrentRect = ControlDpiScalingHelper.NewScaledRectangle(mousePos.X, DraggedColumn.InitialRect.Y, DraggedColumn.InitialRect.Width, DraggedColumn.InitialRect.Height, false);
			}
			else
			{
				Invalidate(DraggedColumn.CurrentRect);
				DraggedColumn.MouseOverColumnRect = Rectangle.Empty;
				DraggedColumn.MouseOverColumnIndex = -1;
			}
		}

		Rectangle GetHeaderBounds(int columnNum)
		{
			var width = Columns[columnNum].ColumnStyle.Width;
			var x = TableStyles[0].RowHeaderWidth + Enumerable.Range(0, columnNum).Select(i => Columns[i].ColumnStyle.Width).Sum();
			return ControlDpiScalingHelper.NewScaledRectangle(x, 0, width, ColumnHeaderHeight, false);
		}

		[DpiState(DpiState.ScaleY)]
		int ColumnHeaderHeight
		{
			get
			{
				var layout = layoutField.GetValue(this);
				var headers = (Rectangle)layout.GetType().InvokeMember("ColumnHeaders", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, layout, null, CultureInfo.InvariantCulture);
				return headers.Height;
			}
		}

		void PaintDraggedColumn(Graphics g)
		{
			if (DraggedColumn != null && DraggedColumn.ColumnImage != null)
			{
				using (var darkGreyBrush = new SolidBrush(Color.FromArgb(150, SystemDataRegistry.Instance.ColorTheme.GridReadOnlyColor)))
				using (var blackPen = new Pen(Brushes.Black, 2F))
				{
					g.FillRectangle(darkGreyBrush, DraggedColumn.InitialRect);
					g.DrawRectangle(blackPen, DraggedColumn.InitialRect);
				}

				if (DraggedColumn.MouseOverColumnIndex != -1)
				{
					using (var b = new SolidBrush(Color.FromArgb(100, SystemDataRegistry.Instance.ColorTheme.GridReadOnlyColor)))
					{
						g.FillRectangle(b, DraggedColumn.MouseOverColumnRect);
					}
				}

				g.DrawImage(DraggedColumn.ColumnImage, DraggedColumn.CurrentRect.X, DraggedColumn.CurrentRect.Y);

				using (var filmFill = new SolidBrush(Color.FromArgb(150, SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1)))
				using (var filmBorder = new Pen(filmFill, 2F))
				{
					g.FillRectangle(filmFill, DraggedColumn.CurrentRect);
					g.DrawRectangle(filmBorder, DraggedColumn.CurrentRect.X, DraggedColumn.CurrentRect.Y + Convert.ToInt16(filmBorder.Width), DraggedColumn.CurrentRect.Width, DraggedColumn.CurrentRect.Height);
				}
			}
		}

#endif

		void SortColumns(int destination, int original)
		{
			if (destination >= 0 && original >= 0 && destination != original)
			{
				var tableStyles = TableStyles[0].GridColumnStyles;
				Columns.Move(Columns[original], destination);

				var columns = tableStyles.Cast<DataGridColumnStyle>().ToList();

				columns.Remove(columns[original]);
				columns.Insert(destination, tableStyles[original]);

				SuspendLayout();
				tableStyles.Clear();
				tableStyles.AddRange(columns.ToArray());
				ResumeLayout();
			}
		}

		#endregion

		#region WndProc

		bool copySelectedRowsAllowed = true;
		[DefaultValue(true)]
		public bool CopySelectedRowsAllowed
		{
			get { return copySelectedRowsAllowed; }
			set { copySelectedRowsAllowed = value; }
		}

#if !WINZOR
#if DEBUG
		internal void WndProcExposed(ref Message m)
		{
			WndProc(ref m);
		}
#endif
		protected override void WndProc(ref Message m)
		{
			menuFeedback.WndProc(this, ref m);
			if (m.Msg == WindowsMessage.WM_COPY)
			{
				if (CopySelectedRowsAllowed)
				{
					CopySelectedRows();
				}
				else
				{
					ShowCannotCopyMessage();
				}
			}
			else
			{
				using (var tracker = MenuItemClickTracker.TrackClick(m, this))
				{
					if ((tracker == null || tracker.ShouldProcessCommandInvoke) && IsHandledVisibleControl(m.Msg))
					{
						try
						{
							base.WndProc(ref m);
						}
						catch (ObjectDisposedException)
						{
							if (IsDisposing || this.IsDisposedOrHasDisposedParent())
							{
								return;
							}
							throw;
						}
					}
				}
			}
		}

		bool IsHandledVisibleControl(int messageCode)
		{
			return messageCode != WindowsMessage.WM_CONTEXTMENU || Visible;
		}

		readonly MenuFeedback menuFeedback = new MenuFeedback();

		protected override void OnHandleDestroyed(EventArgs e)
		{
			try
			{
				base.OnHandleDestroyed(e);
			}
			catch (NullReferenceException)
			{
				// occasional error with ToolTipProvider
			}
		}

#endif
#endregion

		#region OnPaint

		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "It is a text of system exception message")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory., It is a text of system exception message, Not COM related, needs refactor in the future, Not COM related")]
		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				PreFetch();

				if (ToolTipField.GetValue(this) != null)
				{
					using (SuspendRefreshTableStyles())
					{
						var form = FindForm();
						using (PerformanceStatisticsCollector.StartMonitoring("PaintGrid", form?.GetType().FullName))
						{
							try
							{
								base.OnPaint(e);
							}
							catch (TargetException ex)
							{
								ErrorReporter.ReportOnce("TargetException_ZGrid_OnPaint" + Name, FormattableString.Invariant($"TargetException happening on paint for ZGrid. The control path is: {ControlDescription.GetControlPath(this)}"), ex);
								throw;
							}
							catch (NullReferenceException ex)
							{
								ErrorReporter.ReportOnce("NullReferenceException_OnPaint" + Name,
									FormattableString.Invariant($@"NullReferenceException happening on paint for ZGrid. The control path is: {ControlDescription.GetControlPath(this)}
DataSource Type: {this.ListManager?.List.GetType().Name}"),
									ex);
								throw;
							}
						}
					}
					paintFailedLastTime = false;
				}

				if (!DesignModeFinder.IsDesigning && ShouldShowNotifications)
				{
					DrawGridNotification(e.Graphics);
					DrawRowHeaderNotification(e.Graphics);
				}

#if !WINZOR
				if (!DesignModeFinder.IsDesigning && IsGridLayoutConfigurable)
				{
					DrawGridLayoutButton(e.Graphics);
				}

				PaintColumnHeaderBordersAndSortTriangles(e);
				PaintRowHeaderBorders(e);
				PaintTopLeftHeaderBorder(e);
				PaintGridBorder(e);
#if DEBUG
				PaintDesignerColumnHeaders(e);
#endif
				PaintDraggedColumn(e.Graphics);
				PaintRowHeaderIcons(e);
#endif
				if (ShouldStartLoadBackgroundRowColorOnPaint())
				{
					StartLoadBackgroundColour();
				}
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (ArgumentException ex) { if (ex.Message != "Parameter is not valid.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (IndexOutOfRangeException ex)
			{
				if (IsDisposing || this.IsDisposedOrHasDisposedParent())
				{
					return;
				}

				const string NoValueAtIndexString = "No value at index ";
				const string IndexDoesNotHaveAValue = " does not have a value";
				const string Index_ = "Index ";

				var failed = true;
				var noValueAtIndexLocation = ex.Message.IndexOf(NoValueAtIndexString);
				var indexDoesNotHaveAValueLocation = ex.Message.IndexOf(IndexDoesNotHaveAValue);
				if (noValueAtIndexLocation >= 0 || indexDoesNotHaveAValueLocation >= 0 && ex.Message.StartsWith(Index_))
				{
					string invalidRowRequested;

					if (noValueAtIndexLocation >= 0)
					{
						var indexLocation = noValueAtIndexLocation + NoValueAtIndexString.Length;
						invalidRowRequested = ex.Message.Substring(indexLocation).Replace(".", "");
					}
					else
					{
						invalidRowRequested = ex.Message.Substring(Index_.Length, indexDoesNotHaveAValueLocation - Index_.Length);
					}

					try
					{
						var invalidRowNumber = int.Parse(invalidRowRequested);
						if (ListManager.List.Count <= invalidRowNumber)
						{
							if (paintFailedLastTime)
							{
								EndCurrentEdit();
							}
							paintFailedLastTime = true;
							Invalidate();
							failed = false;
						}
					}
					catch (FormatException) { }
				}

				if (failed
					&& ex.Message == "Index was outside the bounds of the array."
					&& ex.TargetSite.DeclaringType.FullName == "System.Windows.Forms.DataGrid"
					&& ex.TargetSite.Name == "PaintRows")
				{
					var dataGridRowsLengthField = typeof(DataGrid).GetField("dataGridRowsLength", BindingFlags.NonPublic | BindingFlags.Instance);
					var dataGridRowsField = typeof(DataGrid).GetField("dataGridRows", BindingFlags.NonPublic | BindingFlags.Instance);

					var dataGridRowsLength = (int)dataGridRowsLengthField.GetValue(this);
					var dataGridRows = (object[])dataGridRowsField.GetValue(this);

					if (dataGridRowsLength == dataGridRows?.Length)
					{
						failed = false;
					}
				}

				if (failed)
				{
					throw new IndexOutOfRangeException(ex.Message + "; ReadOnly=" + this.ReadOnly + "; AllowNew=" + List == null ? "<null>" : List.AllowNew.ToString(), ex);
				}
			}
		}

		internal bool ShouldStartLoadBackgroundRowColorOnPaint()
		{
			return !IsBackgroundColourLoaded 
					&& List != null && gridColourSchemeManager != null 
					&& gridColourSchemeManager.ShouldPrecalculateColorsForAllRowsInGrid()
					&& CurrentLoadColorsVersion != LastLoadColorsVersion
					&& !LoadColorsWorker.IsBusy;
		}

#if !WINZOR
#if DEBUG
		void PaintDesignerColumnHeaders(PaintEventArgs e)
		{
			if (ColumnStyles.Count > 0 && DesignModeFinder.IsDesigning)
			{
				var dataGridLayout = layoutField.GetValue(this);

				var columnHeaderLayout = (Rectangle)dataGridLayout.GetType().GetField("ColumnHeaders").GetValue(dataGridLayout);
				var nextLocation = ControlDpiScalingHelper.NewScaledPoint(1, 1);

				using (var sf = new StringFormat(StringFormatFlags.NoWrap))
				{
					foreach (ZGridColumnInfo columnInfo in ColumnStyles)
					{
						var paintingRect = ControlDpiScalingHelper.NewScaledRectangle(nextLocation.X, nextLocation.Y, columnInfo.Width, columnHeaderLayout.Height, false);
						e.Graphics.FillRectangle(SystemBrushes.Control, paintingRect);
						ControlPaint.DrawBorder3D(e.Graphics, paintingRect, Border3DStyle.RaisedInner);

						ControlDpiScalingHelper.SetX(ref paintingRect, paintingRect.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
						e.Graphics.DrawString(columnInfo.ColumnName, Font, Brushes.Black, paintingRect.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), paintingRect.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(2));

						ControlDpiScalingHelper.SetY(ref paintingRect, paintingRect.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(20), false);
						e.Graphics.DrawString(columnInfo.ColumnStyleType.Name, Font, Brushes.White, paintingRect, sf);

						ControlDpiScalingHelper.SetX(ref nextLocation, nextLocation.X + paintingRect.Width, false);
					}
				}
			}
		}
#endif
#endif

		bool paintFailedLastTime;

		[DefaultValue(false)]
		public bool PaintFailedLastTime => paintFailedLastTime;

		#region PreFetch

		void PreFetch()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var currentCachePoint = new CachePoint(this);
				if (LastCachePoint == null || LastCachePoint.IsDifferent(currentCachePoint))
				{
					LastCachePoint = currentCachePoint;
					if (List is IBusinessObjectCollection)
					{
						var numberOfRowsToAddFetchHints = Math.Min(VisibleRowCount, List.Count - FirstVisibleRow);
						if (numberOfRowsToAddFetchHints > 0)
						{
							var bizOs = new List<BusinessObject>();
							for (var visibleRowIndex = FirstVisibleRow; visibleRowIndex < FirstVisibleRow + numberOfRowsToAddFetchHints; visibleRowIndex++)
							{
								var bizO = List[visibleRowIndex] as BusinessObject;
								if (bizO != null)
								{
									bizOs.Add(bizO);
								}
							}
							AddFetchHintHints(bizOs.ToArray(), Columns);
						}
					}
				}
			}
		}

		CachePoint LastCachePoint;
		TableColumn[] TableColumns;

		#region class CachePoint

		class CachePoint
		{
			public CachePoint(ZGrid grid)
			{
				firstVisibleRow = grid.FirstVisibleRow;
				VisibleRowCount = grid.VisibleRowCount - 1;
			}

			public bool IsDifferent(CachePoint otherCachePoint)
			{
				return firstVisibleRow != otherCachePoint.firstVisibleRow
					|| VisibleRowCount != otherCachePoint.VisibleRowCount;
			}

			readonly int firstVisibleRow;
			readonly int VisibleRowCount;
		}

		#endregion

		public void ForcePreFetch()
		{
			LastCachePoint = null;
		}

#if !WINZOR

		#region Reflection Hack

		int FirstVisibleRow
		{
			get
			{
				ComputeLayout();
				if (firstVisibleRowInfo == null)
				{
					firstVisibleRowInfo = typeof(DataGrid).GetField("firstVisibleRow", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				return (int)firstVisibleRowInfo.GetValue(this);
			}
		}

		void ComputeLayout()
		{
			var checkHierarchyStateInfo = typeof(DataGrid).GetMethod("CheckHierarchyState", BindingFlags.Instance | BindingFlags.NonPublic);
			checkHierarchyStateInfo.Invoke(this, null);
			var layoutInfo = typeof(DataGrid).GetField("layout", BindingFlags.Instance | BindingFlags.NonPublic);
			var layout = layoutInfo.GetValue(this);
			var dirtyInfo = layout.GetType().GetField("dirty", BindingFlags.Instance | BindingFlags.NonPublic);
			var isDirty = (bool)dirtyInfo.GetValue(layout);
			if (isDirty)
			{
				typeof(DataGrid).GetMethod("ComputeLayout", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);
			}
		}

		FieldInfo firstVisibleRowInfo;

		#endregion

#endif

#if DEBUG
		protected virtual
#endif
		void AddFetchHintHints(BusinessObject[] businessObjects, ZGridColumns columns)
		{
			if (businessObjects.Length > 0)
			{
				if (TableColumns == null)
				{
					TableColumns = new TableColumnCalculator().GetTableColumnsOnThisObject(businessObjects[0], columns);
				}

				((IBusinessObjectCollection)List).FetchStrategy.FetchForView(businessObjects, TableColumns);

				foreach (var bizO in businessObjects)
				{
					if (bizO != null && bizO.FetchStrategy != null)
					{
						bizO.FetchStrategy.FetchForView(TableColumns);
					}
				}

				OnFetchingForView(businessObjects, TableColumns);
			}
		}

		#endregion

#if !WINZOR

		#region PaintColumnHeaderBordersAndSortTriangles

		void PaintColumnHeaderBordersAndSortTriangles(PaintEventArgs e)
		{
			Dictionary<PropertyDescriptor, ListSortDirection> sortProperties = null;
			var bindingList = ListManager != null ? ListManager.List as IBindingList : null;
			if (bindingList is IBindingListView bindingListView && bindingListView.SupportsAdvancedSorting && bindingListView.Count > 0)
			{
				sortProperties = bindingListView.SortDescriptions.Cast<ListSortDescription>().ToDictionary(item => item.PropertyDescriptor, item => item.SortDirection);
			}
			else if (bindingList != null && bindingList.SupportsSorting && bindingList.SortProperty != null)
			{
				sortProperties = new Dictionary<PropertyDescriptor, ListSortDirection>
				{
					{ bindingList.SortProperty, bindingList.SortDirection }
				};
			}

			if (TableStyles.Count > 0)
			{
				var dataGridLayout = layoutField.GetValue(this);
				var rectangle = (Rectangle)dataGridLayout.GetType().GetField("ColumnHeaders").GetValue(dataGridLayout);
				using (var clip = e.Graphics.Clip)
				{
					e.Graphics.SetClip(rectangle);

					var negOffsetField = typeof(DataGrid).GetField("negOffset", BindingFlags.Instance | BindingFlags.NonPublic);
					var negOffset = (int)negOffsetField.GetValue(this);
					ControlDpiScalingHelper.SetX(ref rectangle, rectangle.X - negOffset, false);
					ControlDpiScalingHelper.SetWidth(ref rectangle, rectangle.Width + negOffset, false);
					for (var i = FirstVisibleColumn; i < TableStyles[0].GridColumnStyles.Count; i++)
					{
						if (i > FirstVisibleColumn)
						{
							ControlDpiScalingHelper.SetX(ref rectangle, rectangle.X + rectangle.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
						}
						else
						{
							ControlDpiScalingHelper.SetX(ref rectangle, rectangle.X - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
						}
						ControlDpiScalingHelper.SetWidth(ref rectangle, TableStyles[0].GridColumnStyles[i].Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);

						ControlPaint.DrawBorder(e.Graphics, rectangle, HeaderBorderColor, ButtonBorderStyle.Solid);

						if ((sortProperties != null && TableStyles[0].GridColumnStyles[i].PropertyDescriptor != null && sortProperties.ContainsKey(TableStyles[0].GridColumnStyles[i].PropertyDescriptor)))
						{
							var trianglerect = ControlDpiScalingHelper.NewScaledRectangle(rectangle.Right - rectangle.Height, rectangle.Y, rectangle.Height, rectangle.Height, false);
							var offset = Math.Max(0, (rectangle.Height - ControlDpiScalingHelper.ScaleToCurrentDpiX(5)) / 2);
							trianglerect.Inflate(-offset, -offset);
							var dir = sortProperties[TableStyles[0].GridColumnStyles[i].PropertyDescriptor] == ListSortDirection.Descending ? TriangleDirection.Down : TriangleDirection.Up;
							using (var brush = new SolidBrush(HeaderBorderColor))
							using (var pen = new Pen(brush))
							{
								Triangle.Paint(e.Graphics, trianglerect, dir, brush, pen, pen, pen, true);
							}
						}
					}

					e.Graphics.Clip = clip;
				}
			}
		}

		#endregion

		#region PaintRowHeaderBorders

		void PaintRowHeaderBorders(PaintEventArgs e)
		{
			var firstVisibleRow = GetFirstVisibleRow();
			var lastVisibleRow = firstVisibleRow + VisibleRowCount;
			if (firstVisibleRow >= 0 && this.DataGridRowsLength > 0)
			{
				for (var row = firstVisibleRow; row < lastVisibleRow; row++)
				{
					try
					{
						var rectangle = (Rectangle)getRowRectMethod.Invoke(this, new object[] { row });
						ControlDpiScalingHelper.SetWidth(ref rectangle, TableStyles[0].RowHeaderWidth, false);
						ControlDpiScalingHelper.SetY(ref rectangle, rectangle.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
						ControlDpiScalingHelper.SetHeight(ref rectangle, rectangle.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
						ControlPaint.DrawBorder(e.Graphics, rectangle, HeaderBorderColor, ButtonBorderStyle.Solid);
					}
					catch (TargetInvocationException ex) //IndexOutOfRangeException in DataGrid.GetRowRect. Safe to ignore as no state change occurs in DataGrid
					{
						if (ex.InnerException == null || !(ex.InnerException is IndexOutOfRangeException))
						{
							throw;
						}
					}
				}
			}
		}

		#endregion

		#region PaintTopLeftHeaderBorder

		void PaintTopLeftHeaderBorder(PaintEventArgs e)
		{
			var dataGridLayout = layoutField.GetValue(this);
			var rectangle = (Rectangle)dataGridLayout.GetType().GetField("TopLeftHeader").GetValue(dataGridLayout);
			ControlPaint.DrawBorder(e.Graphics, rectangle, HeaderBorderColor, ButtonBorderStyle.Solid);
		}

		#endregion

		#region PaintGridBorder

		void PaintGridBorder(PaintEventArgs e)
		{
			ControlPaint.DrawBorder(e.Graphics, ClientRectangle, HeaderBorderColor, ButtonBorderStyle.Solid);
		}

		readonly static FieldInfo layoutField = typeof(DataGrid).GetField("layout", BindingFlags.Instance | BindingFlags.NonPublic);
		readonly static MethodInfo getRowRectMethod = typeof(DataGrid).GetMethod("GetRowRect", BindingFlags.NonPublic | BindingFlags.Instance);

		Color HeaderBorderColor
		{
			get { return Color.FromArgb(OffsetColourComponent(HeaderBackColor.R), OffsetColourComponent(HeaderBackColor.G), OffsetColourComponent(HeaderBackColor.B)); }
		}

		int OffsetColourComponent(int value)
		{
			var result = value - 30;
			if (result < 0)
			{
				result = value + 30;
			}
			return result;
		}

		#endregion

		#region DrawGridLayoutButton

		void DrawGridLayoutButton(Graphics graphics)
		{
			isActiveGridLayoutIconDrawn = TopLeftCornerGridLayoutRectangle.Contains(GetMousePositionInGrid());
			var icon = isActiveGridLayoutIconDrawn ? Icons.GetIcon(IconTypes.DownArrowActive) : Icons.GetIcon(IconTypes.DownArrowRest);

			using (var brush = new SolidBrush(HeaderBackColor))
			{
				graphics.FillRectangle(brush, TopLeftCornerGridLayoutRectangle);
			}
			graphics.DrawIcon(icon, TopLeftCornerGridLayoutRectangle);
		}
		internal bool isActiveGridLayoutIconDrawn;

		#endregion

		#region PaintRowHeaderIcons

		void PaintRowHeaderIcons(PaintEventArgs e)
		{
			var firstVisibleRow = GetFirstVisibleRow();
			var lastVisibleRow = firstVisibleRow + VisibleRowCount;
			if (firstVisibleRow >= 0 && this.DataGridRowsLength > 0)
			{
				for (var row = firstVisibleRow; row < lastVisibleRow; row++)
				{
					try
					{
						var image = GetRowHeaderImageFromRowIndex(row);
						var rectangle = GetRowHeaderIconsRectangleFromRowIndex(row);
						DrawRowHeaderIcon(e, image, rectangle);
					}
					catch (TargetInvocationException ex) //IndexOutOfRangeException in DataGrid.GetRowRect. Safe to ignore as no state change occurs in DataGrid
					{
						if (ex.InnerException == null || !(ex.InnerException is IndexOutOfRangeException))
						{
							throw;
						}
					}
				}
			}
		}

		void DrawRowHeaderIcon(PaintEventArgs e, Image image, Rectangle rectangle)
		{
			var offset = ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
			using (var brush = new SolidBrush(HeaderBackColor))
			{
				e.Graphics.FillRectangle(brush, rectangle);
			}
			if (image != null)
			{
				rectangle.Inflate(-offset, -offset);
				e.Graphics.DrawImage(image, rectangle);
			}
		}

#if DEBUG
		internal
#endif
		Rectangle GetRowHeaderIconsRectangleFromRowIndex(int row)
		{
			var borderWidth = ControlDpiScalingHelper.ScaleToCurrentDpiY(1);
			var rectangle = (Rectangle)getRowRectMethod.Invoke(this, new object[] { row });

			var maxLength = ControlDpiScalingHelper.ScaleToCurrentDpiX(16);
			var iconLength = rectangle.Width > rectangle.Height ? rectangle.Height : rectangle.Width;

			iconLength = iconLength > maxLength ? maxLength : iconLength;

			ControlDpiScalingHelper.SetX(ref rectangle, rectangle.X + borderWidth, false);
			ControlDpiScalingHelper.SetY(ref rectangle, rectangle.Y + borderWidth, false);
			ControlDpiScalingHelper.SetHeight(ref rectangle, iconLength - 2 * borderWidth, false);
			ControlDpiScalingHelper.SetWidth(ref rectangle, iconLength - 2 * borderWidth, false);

			return rectangle;
		}

		Image GetRowHeaderImageFromRowIndex(int rowIndex)
		{
			Image image = null;
			var dataGridRows = (object[])typeof(DataGrid).GetField("dataGridRows", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			var dataGridRowsCount = dataGridRows?.Length ?? DataGridRowsLength;
			if (rowIndex < dataGridRowsCount && dataGridRows[rowIndex].GetType().FullName.EndsWith("DataGridAddNewRow"))
			{
				image = Icons.GetImage(IconTypes.Star);
			}
			if (CurrentCell.RowNumber == rowIndex)
			{
				image = GetState(GridState.IsEditing) ? Icons.GetImage(IconTypes.Edit) : Icons.GetImage(IconTypes.RightArrow);
			}
			return image;
		}

		#endregion

#endif

		#region IsGridLayoutConfigurable

		protected internal virtual bool IsGridLayoutConfigurable
		{
			get { return true; }
		}

		#endregion

		#region Paint Notifications

		const int RowHeaderErrorIconOffset = 18;
		const int NotificationIconSize = 16;

		protected virtual void DrawGridNotification(Graphics graphics)
		{
			DrawGridNotificationIcon(graphics, notificationType != null ? NotificationIconScheme.Instance.GetMiniImage(notificationType) : null);
		}

		protected void DrawRowHeaderNotification(Graphics graphics)
		{
			DrawRowHeaderNotification(graphics, false);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected void DrawRowHeaderNotification(Graphics graphics, bool paintWhenNoNotification)
		{
			if (List != null && List.Count > 0)
			{
				var firstVisibleRow = GetFirstVisibleRow();
				var lastVisibleRow = firstVisibleRow + VisibleRowCount;

				if (lastVisibleRow > List.Count)
				{
					lastVisibleRow = List.Count;
				}
				try
				{
					for (var paintingRowIndex = firstVisibleRow; paintingRowIndex < lastVisibleRow; paintingRowIndex++)
					{
#if !WINZOR
						var notificationIcon = GetRowNotificationImageFromListIndex(paintingRowIndex);

						if (notificationIcon != null || paintWhenNoNotification)
						{
							var notificationRect = GetRowNotificationRectangle(paintingRowIndex);

							if (notificationRect.Top != 0)
							{
								using (var brush = new SolidBrush(HeaderBackColor))
								{
									graphics.FillRectangle(brush, ControlDpiScalingHelper.NewScaledRectangle(
										notificationRect.X,
										notificationRect.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(1),
										notificationRect.Width,
										notificationRect.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false));
								}
								if (notificationIcon != null)
								{
									var iconArea = GridColumnNotificationProvider.VAlignRect(notificationRect, notificationIcon.Size, 0, 1);
									graphics.DrawImageUnscaled(notificationIcon, iconArea);
								}
#if DEBUG
								LastDrawnRowIconForTesting = notificationIcon;
#endif
							}
						}
#endif
					}
				}
				catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
				catch (ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			}
		}

		void DrawGridNotificationIcon(Graphics graphics, Image icon)
		{
			using (var brush = new SolidBrush(HeaderBackColor))
			{
				graphics.FillRectangle(brush, TopLeftCornerNotificationRectangle);
			}
#if DEBUG
			LastDrawnCornerIconForTesting = icon;
#endif
			if (icon != null)
			{
				DoesGridHaveTopLeftNotificationIconShowing = true;
				graphics.DrawImageUnscaled(icon, TopLeftCornerNotificationRectangle.Left, TopLeftCornerNotificationRectangle.Top);
			}
			else
			{
				DoesGridHaveTopLeftNotificationIconShowing = false;
			}
		}

#if DEBUG
		public Image LastDrawnRowIconForTesting;
#endif

		protected virtual Image GetRowNotificationImageFromListIndex(int listIndex)
		{
			Image result = null;

			if (ListForNotifications != null && listIndex >= 0 && listIndex < ListForNotifications.Count)
			{
				var current = ListForNotifications[listIndex] as BusinessObject;
				var notification = current?.GetHighestSeverityNotificationType();

				if (current != null && notification != null)
				{
					result = NotificationIconScheme.Instance.GetMiniImage(notification);
				}
			}

			return result;
		}

#if DEBUG
		public
#else
		protected
#endif
Rectangle GetRowNotificationRectangle(int row)
		{
			var result = (Rectangle)typeof(DataGrid).GetMethod("GetRowRect", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] { row });
			ControlDpiScalingHelper.SetX(ref result, result.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(RowHeaderErrorIconOffset + RowHeaderNotificationOffset), false);
			ControlDpiScalingHelper.SetWidth(ref result, NotificationIconSize, true);
			ControlDpiScalingHelper.SetHeight(ref result, NotificationIconSize, true);
			return result;
		}

		[DpiState(DpiState.Unscaled)]
		protected virtual int RowHeaderNotificationOffset
		{
			get { return -2; }
		}

		static Rectangle TopLeftCornerNotificationRectangle
		{
			get { return ControlDpiScalingHelper.NewScaledRectangle(3, 3, 12, 16); }
		}

		internal static Rectangle TopLeftCornerGridLayoutRectangle
		{
			get { return ControlDpiScalingHelper.NewScaledRectangle(18, 3, 16, 16); }
		}

#if !WINZOR
		static bool IsValidCellHitInfo(HitTestInfo info)
		{
			return info != null && info.Type == HitTestType.Cell && info.Row >= 0 && info.Column >= 0;
		}
#endif

#if DEBUG
		protected virtual
#endif
		Point GetMousePositionInGrid()
		{
			return PointToClient(MousePosition);
		}

		#endregion

		#endregion

		#region Design-Mode ColumnStyles

		[Browsable(true), Category("Design"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Description("")]
		[Editor(ZGUIConstants.ColumnStyleEditor, typeof(UITypeEditor))]
		[SuppressWeaklyTypedCollectionMessage]
		public virtual ArrayList ColumnStyles
		{
			get { return columnStyles; }
		}
		readonly ArrayList columnStyles = new ArrayListWithNullDetection();

		class ArrayListWithNullDetection : ArrayList
		{
			public override object this[int index]
			{
				get
				{
					return base[index];
				}
				set => base[index] = value ?? throw new ArgumentNullException(nameof(value));
			}
			public override int Add(object value)
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				return base.Add(value);
			}
		}

		#endregion

		#region Set ReadOnly When No Parent Rows

		protected void HookRelatedCurrencyManager()
		{
			if (ListManager != null && ListManager.GetType().Name == "RelatedCurrencyManager")
			{
				var parentManagerField = ListManager.GetType().GetField("parentManager", BindingFlags.NonPublic | BindingFlags.Instance);
				if (parentManagerField.GetValue(ListManager) is CurrencyManager parentManager)
				{
					parentManager.PositionChanged += ParentManager_PositionChanged;
					ParentManager_PositionChanged(parentManager, EventArgs.Empty);
				}
			}
		}

		void ParentManager_PositionChanged(object sender, EventArgs e)
		{
			if (sender is CurrencyManager parentManager)
			{
				IsEmptyParentList = (parentManager.Count == 0);
				if (ReadOnly && LastFocusedColumn != null)
				{
					LastFocusedColumn.EditControl.Hide();
				}
			}
		}

		#endregion

		#region Grid State

		public enum GridState
		{
			AllowSorting = 1, //0x1
			ColumnHeadersVisible = 2, //0x2
			RowHeadersVisible = 4, //0x4
			TrackColResize = 8, //0x8
			TrackRowResize = 16, //0x10
			IsLedgerStyle = 32, //0x20
			IsFlatMode = 64, //0x40
			ListHasErrors = 128, //0x80
			Dragging = 256, //0x100
			InListAddNew = 512, //0x200
			InDeleteRow = 1024, //0x400
			CanFocus = 2048, //0x800
			ReadOnlyMode = 4096, //0x1000
			AllowNavigation = 8192, //0x2000
			IsNavigating = 16384, //0x4000
			IsEditing = 32768, //0x8000
			EditControlChanging = 65536, //0x10000
			IsScrolling = 131072, //0x20000
			OverCaption = 262144, //0x40000
			ChildLinkFocused = 524288, //0x80000
			InAddNewRow = 1048576, //0x100000
			InSetListManager = 2097152, //0x200000
			MetaDataChanged = 4194304, //0x400000
			ExceptionInPaint = 8388608 //0x800000
		}

		void SetState(GridState stateFlag, bool value)
		{
			var gridStateField = typeof(DataGrid).GetField("gridState", BindingFlags.Instance | BindingFlags.NonPublic);
			var gridState = (BitVector32)gridStateField.GetValue(this);
			gridState[(int)stateFlag] = value;
			gridStateField.SetValue(this, gridState);
		}

		internal bool GetState(GridState stateFlag)
		{
			var gridStateField = typeof(DataGrid).GetField("gridState", BindingFlags.Instance | BindingFlags.NonPublic);
			var gridState = (BitVector32)gridStateField.GetValue(this);
			return gridState[(int)stateFlag];
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZGrid>().Result;
		}

		void PokePropertiesForFlattenHierarchy(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				if (BindingContext != null)
				{
					if (BindingContext[dataSource, dataMember] is CurrencyManager listManager)
					{
						var cachedPropertyDescriptorCollection = listManager.GetItemProperties();
						// find the flattened properties here so foreach on PropertyDescriptorCollection works
						foreach (ZGridColumnInfo column in ColumnStyles)
						{
							if (column.ColumnName.IndexOf("+") != -1)
							{
								cachedPropertyDescriptorCollection.Find(column.ColumnName, false);
							}
						}
					}
				}
				else
				{
					throw new InvalidOperationException("No BindingContext available for PokePropertiesForFlattenHierarchy");
				}
			}
		}

		#endregion

		#region Fetch Hints

		protected void OnFetchingForView(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			FetchingForView?.Invoke(this, new FetchForViewEventArgs(businessObjects, columns));
		}

		public event EventHandler<FetchForViewEventArgs> FetchingForView;

		#endregion

#endregion

		#region IDisposable Members

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			IsDisposing = true;

			try
			{
				if (disposing)
				{
					if (ListManager != null)
					{
						ListManager.CurrentChanged -= ListManager_CurrentChanged;
						if (ListManager.GetType().Name == "RelatedCurrencyManager")
						{
							var parentManagerField = ListManager.GetType().GetField("parentManager", BindingFlags.NonPublic | BindingFlags.Instance);
							if (parentManagerField.GetValue(ListManager) is CurrencyManager parentManager)
							{
								parentManager.PositionChanged -= ParentManager_PositionChanged;
							}
						}
					}
					fParentForm = null;

					//swap back to original position changed event handler
					var valueField = typeof(DataGrid).GetField("positionChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic);
					valueField.SetValue(this, DataGridPositionChangedHandler);

					if (DataSource != null && !DesignModeFinder.IsDesigning && isBound)
					{
						SaveUserLayoutSettings();
					}

					SuspendLayout();
					try
					{
						if (lastList != null)
						{
							lastList.ListChanged -= OGrid_ListChanged;
							lastList = null;
						}

						if (ListManager != null)
						{
							ListManager.CurrentChanged -= ListManager_CurrentChanged;
							if (ListManagerBase != null)
							{
								UnWireDataSourceMethodInfo.Invoke(this, Array.Empty<object>());
							}
						}
						//fixing memory leak of WI00228602 by severing link between ZGrid and GenericOrderCollection
						if (lastListManagerList != null)
						{
							lastListManagerList = null;
						}
					}
					finally
					{
						ResumeLayout();
					}

					DataBindings.Clear();
					((IDisposable)Columns).Dispose();
					if (defaultColumns != null)
					{
						((IDisposable)defaultColumns).Dispose();
						defaultColumns = null;
					}
					if (oldDefaultColumns != null)
					{
						((IDisposable)oldDefaultColumns).Dispose();
						oldDefaultColumns = null;
					}
					DisposeRemovedColumns();
					if (ContextMenu != null)
					{
						MenuDisposer.DisposeMenu(ContextMenu);
						DisposeMenu();
					}

					if (translationFeedbackManager != null)
					{
						translationFeedbackManager.Dispose();
					}
					if (devInfoPopupManager != null)
					{
						devInfoPopupManager.Dispose();
					}

					if (gridRowFinderForm != null)
					{
						gridRowFinderForm.Close();
					}

					if (notificationsViewerForm != null)
					{
						notificationsViewerForm.Close();
					}

					DisposableLeakListener.Instance.UnRegisterDisposable(this);

					gridColourSchemeManager?.Dispose();
					gridColourSchemeManager = null;

					if (loadColorsWorker != null)
					{
						if (loadColorsWorker.IsBusy)
						{
							loadColorsWorker.CancelAsync();
						}
						loadColorsWorker.Dispose();
						loadColorsWorker = null;
					}

					gridLayoutManager = null;
				}
				base.Dispose(disposing);

				if (disposing)
				{
					if (fCachedColums != null)
					{
						foreach (var cachedColumn in CachedColums.Values)
						{
							cachedColumn.ColumnStyle.Dispose();
						}
					}
					UnhookNotificationsChanged();
					Extensions.Dispose();
					CustomRowBackgroundColourDeciding = null;
					CurrentColumnLayout = null;
#if !WINZOR
					ZForm.NukeAllEvents(this);
#endif
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce(
					"ErrorInDisposingGrid_" + Name,
					string.Format("Control name: {0}\r\nBind To: {1}\r\nMessage: {2}", Name, BindTo, ex.Message),
					ex);
			}
		}
		protected bool IsDisposing;

		internal void RegisterRemovedColumn(ZGridColumn column)
		{
			RemovedColumns.Add(column);
		}

		internal void DisposeRemovedColumns()
		{
			if (removedColumns != null)
			{
				foreach (var column in removedColumns)
				{
					column.ColumnStyle.Dispose();
				}
			}
		}

		HashSet<ZGridColumn> RemovedColumns
		{
			get { return removedColumns ?? (removedColumns = new HashSet<ZGridColumn>()); }
		}
		HashSet<ZGridColumn> removedColumns;

		/// <summary>
		/// Use this when changing lots of grid columns.
		/// </summary>
		public IDisposable SuspendRefreshTableStylesAndRefreshAtDisposal()
		{
			needsRefreshTableStyles = true;
			return SuspendRefreshTableStyles();
		}

		public IDisposable SuspendRefreshTableStylesAndNotRefreshAtDisposal()
		{
			return new RefreshTableStylesSuspender(this, false);
		}

		public IDisposable SuspendRefreshTableStyles()
		{
			return new RefreshTableStylesSuspender(this);
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsRefreshTableStylesSuspended
		{
			get { return refreshTableStylesIndex > 0; }
		}

		int refreshTableStylesIndex;
		bool needsRefreshTableStyles;

		class RefreshTableStylesSuspender : IDisposable
		{
			public RefreshTableStylesSuspender(ZGrid grid, bool shouldRefreshTableStylesAtDisposal = true)
			{
				this.grid = grid;
				grid.refreshTableStylesIndex++;
				this.shouldRefreshTableStylesAtDisposal = shouldRefreshTableStylesAtDisposal;
			}

			readonly ZGrid grid;
			readonly bool shouldRefreshTableStylesAtDisposal;

			public void Dispose()
			{
				if (!grid.IsDisposed)
				{
					grid.refreshTableStylesIndex--;
					if (!grid.IsRefreshTableStylesSuspended && grid.needsRefreshTableStyles && shouldRefreshTableStylesAtDisposal)
					{
						grid.RefreshTableStyles();
					}
				}
			}
		}

#endregion

		#region Column Layout

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IGridLayoutStorage CurrentColumnLayout
		{
			get
			{
				if (!currentColumnLayoutInitialized)
				{
					if (currentColumnLayout == null)
					{
						currentColumnLayout = GridLayoutManager.GetLastSavedLayout(this);
					}
					currentColumnLayoutInitialized = true;
				}
				if (currentColumnLayout == null)
				{
					currentColumnLayout = GridLayoutManager.GetDefaultLayout(this);
				}
				return currentColumnLayout;
			}
			set
			{
				currentColumnLayout = value;
			}
		}
		IGridLayoutStorage currentColumnLayout;
		bool currentColumnLayoutInitialized;

		internal string IdentifierRootID
		{
			get
			{
				if (identifierRootID == null && isBound)
				{
					identifierRootID = "";
					var parentControl = this as Control;

					while (parentControl != null)
					{
						if (parentControl is IDataGridLayoutIdentifierRoot identifierRootParentControl)
						{
							identifierRootID = identifierRootParentControl.ID;
							break;
						}

						parentControl = parentControl.Parent;
					}
				}
				return identifierRootID;
			}
		}
		string identifierRootID;

		/// <summary>
		/// If a grid is contained in two different containers which need to have its own column settings, then this should be set for each grid in different containers.
		/// eg. InvoiceLineGrid in a BaseInvoiceLineUserControl inherited by Import and ExportInvoiceLineUserControl
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ColumnLayoutContext
		{
			get { return columnLayoutContext; }
			set
			{
				var shouldSaveAndRefreshLayout =
					isBound &&
					ColumnLayoutContext != value &&
					!string.IsNullOrEmpty(this.GridId);

				if (shouldSaveAndRefreshLayout)
				{
					GridLayoutManager.SaveDefaultLayout(this);
				}

				columnLayoutContext = value;

				if (shouldSaveAndRefreshLayout)
				{
					RefreshCurrentColumnLayoutName();
					GridLayoutManager.LoadUserLayoutSettings(this);
				}
			}
		}

		string columnLayoutContext;

		[DefaultValue("")]
		public string LayoutKey
		{
			get { return string.IsNullOrEmpty(layoutKey) ? Name : layoutKey; }
			set { layoutKey = value; }
		}
		string layoutKey = "";

		[DefaultValue("")]
		[Description("Specifies context key, which is used for filtering available colour schemes. Allows to share colour schemes between all grids with same ColorContextKey.")]
		public string ColorContextKey { get; set; }

		[DefaultValue(false)]
		[Description("Specifies if selected color scheme should be shared between all grids with same ColorContextKey and ShareActiveColorScheme set to true.")]
		public bool ShareActiveColorScheme { get; set; }

		[DefaultValue(false)]
		[Description("Specifies whether FilterBusinessObject on parent FilterStripControl should be ignored (i.e. due to different data types in control and grid).")]
		public bool IgnoreParentFilterControl { get; set; }

		internal ZFilterStripCommonControl GetParentFilterControl()
		{
			return !IgnoreParentFilterControl ? this.GetRootContainer() as ZFilterStripCommonControl : null;
		}

		/// <summary>
		/// The category which the layout data will be stored against, e.g. use Env.CurrentDepartment.PK to
		/// save and restore layout data against each department the user logs in to.
		/// BaseInvoiceLineUserControl has extra columns that are configured against an importer. ImporterPK is set here
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Guid LayoutCategoryPK
		{
			get { return layoutCategoryPK; }
			set
			{
				if (LayoutCategoryPK != value)
				{
					layoutCategoryPK = value;
					RefreshCurrentColumnLayoutName();
				}
			}
		}
		Guid layoutCategoryPK = Guid.Empty;

		public delegate bool QueryHasCustomisedColumnsEventHandler(string[] columnsToSave);

		public QueryHasCustomisedColumnsEventHandler QueryHasCustomisedColumns;
		public QueryHasCustomisedColumnsEventHandler QueryHasNoOtherCustomisedColumns;

		void RefreshCurrentColumnLayoutName()
		{
			currentColumnLayout = null;
			currentColumnLayoutInitialized = false;
		}

		public DataGridLayoutManager GridLayoutManager => gridLayoutManager ?? (gridLayoutManager = new DataGridLayoutManager(NameForDebugging));

		DataGridLayoutManager gridLayoutManager;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debug information")]
		public string NameForDebugging => string.Format(CultureInfo.InvariantCulture, "Grid: {0}; Form: {1}", Name, ParentForm?.Text);

		#endregion

		#region IPastableControl Members

#if !WINZOR

		bool IPastableControl.TryPaste()
		{
			var parentForm = ParentForm as KForm;
			if (LastFocusedColumn is IPastableControl column)
			{
				column.TryPaste();
			}
			else if (parentForm != null)
			{
				var activeChildControl = parentForm.GetFrontMostActiveControl();
				if (activeChildControl != null)
				{
					UnsafeNativeMethods.PostMessage(new HandleRef(activeChildControl, activeChildControl.Handle), WindowsMessage.WM_PASTE, IntPtr.Zero, IntPtr.Zero);
				}
			}

			return true;
		}

#endif

		#endregion

		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{
			base.BeginInit();
			IsInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			IsInitializing = false;
			base.EndInit();
			OnEndInit();
		}

		protected bool IsInitializing { get; set; }

		protected virtual void OnEndInit()
		{
			Initialized?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler Initialized;

		#endregion

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set
			{
				if (!IsInitializing || KBindingSource.GetBindingSource(this) == null || DesignModeFinder.IsDesigning)
				{
					BindingMemberHelper.BindingMember = value;
				}
				else
				{
					bindingMemberForLateAssign = value;
					Initialized += SetBindingMemberOnInitialized;
				}
			}
		}
		string bindingMemberForLateAssign;

		void SetBindingMemberOnInitialized(object sender, EventArgs e)
		{
			BindingMemberHelper.BindingMember = bindingMemberForLateAssign;
			Initialized -= SetBindingMemberOnInitialized;
		}

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this)?.DataSourceType; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		#endregion

		#region IDataBoundControl Members

		protected string TableName;

		Type IDataBoundControl.DataSourceType
		{
			get { return typeof(IList); }
		}

		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnly")]
		public new object DataSource
		{
			get { return base.DataSource; }
			set { base.DataSource = value; }
		}

		public new void SetDataBinding(object dataSource, string dataMember)
		{
			string tableName = null;
			IList list = null;
			IBusinessObjectCollection bizoCollection = null;
			BindingContext context = null;
			CurrencyManager currencyManager = null;
			void Bind()
			{
				tableName = dataMember;
				list = null;
				bizoCollection = List as IBusinessObjectCollection;
				using ((bizoCollection as IBindingTrackingBusinessObjectCollection)?.Binding())
				using ((bizoCollection != null && !(bizoCollection is IDoNotHaveFactory) && bizoCollection.Factory != null)
						? ActiveBusinessObjectCollection.DelayListChangedEvents(bizoCollection.Factory)
						: null)
				{
					using (TraceDataSource(dataSource))
					{
						if (dataSource != null)
						{
							context = BindingContext;
							currencyManager = context != null ? context[dataSource, dataMember] as CurrencyManager : null;
							list = currencyManager?.List;
							if (list is IBusinessObjectCollection || list is ZBindingContext.TempList)
							{
								tableName = "BusinessObjectCollection";
							}
						}

						using ((list as IBindingTrackingBusinessObjectCollection)?.Binding())
						{
							SetDataBinding(dataSource, dataMember, tableName);
						}
					}
				}
			}
			try
			{
				Bind();
			}
			catch (IndexOutOfRangeException)
			{
				BindingManagerBase formerManager = null;
				PropertyDescriptor prop = null;

				try
				{
					Bind();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					var fullName = "";
					var isCurrencyManager = false;
					var isICurrencyManagerProvider = false;

					try
					{
						fullName = context[dataSource, dataMember].GetType().FullName;
						isCurrencyManager = context[dataSource, dataMember] is CurrencyManager;
						isICurrencyManagerProvider = dataSource is ICurrencyManagerProvider provider;

						var lastDot = dataMember.LastIndexOf(".");
						var dataPath = (lastDot == -1) ? "" : dataMember.Substring(0, lastDot);
						var dataField = dataMember.Substring(lastDot + 1);

						formerManager = context[dataSource, dataPath];

						prop = formerManager?.GetItemProperties().Find(dataField, true);
					}
					catch (Exception)
					{
					}

					throw new InvalidOperationException($@"Binding fail on retry.
IsDisposing: {this.IsDisposing} 
IsDisposed: {this.IsDisposed} 
IsHandleCreated: {this.IsHandleCreated} 
ParentForm.IsDisposing: {ParentForm?.IsDisposing} 
ParentForm.IsDisposed: {ParentForm?.IsDisposed} 
ParentForm.IsHandleCreated: {ParentForm?.IsHandleCreated} 
dataSource:{dataSource.GetType().FullName}
dataSource isICurrencyManagerProvider:{isICurrencyManagerProvider}
dataMember:{dataMember}

tableName:{tableName}
list:{list?.GetType().FullName}, null:{list == null}, Count:{list?.Count}
bizoCollection:{bizoCollection?.GetType().FullName},null:{bizoCollection == null}, Count:{bizoCollection?.Count}
context:{context}
currencyManager:{currencyManager}
context[dataSource, dataMember]:{fullName},isCurrencyManager:{isCurrencyManager}

formerManager:{formerManager}
formerManager Count:{formerManager?.Count}
formerManager Position:{formerManager?.Position}
prop: {prop}
prop.PropertyType: {prop?.PropertyType?.FullName}

controlInfo:{ControlDescription.GetControlPathAndLocation(this)}
{(currencyManager == null ? string.Empty :
$@"shouldBind：{(bool)typeof(CurrencyManager).GetField("shouldBind", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
bound：{(bool)typeof(CurrencyManager).GetField("bound", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
AllowAdd：{(bool)typeof(CurrencyManager).GetProperty("AllowAdd", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
AllowEdit：{(bool)typeof(CurrencyManager).GetProperty("AllowEdit", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
AllowRemove：{(bool)typeof(CurrencyManager).GetProperty("AllowRemove", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
pullingData：{(bool)typeof(CurrencyManager).GetField("pullingData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
inChangeRecordState：{(bool)typeof(CurrencyManager).GetField("inChangeRecordState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
suspendPushDataInCurrentChanged：{(bool)typeof(CurrencyManager).GetField("suspendPushDataInCurrentChanged", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
Position：{currencyManager.Position}
listposition：{(int)typeof(CurrencyManager).GetField("listposition", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
lastGoodKnownRow：{(int)typeof(CurrencyManager).GetField("lastGoodKnownRow", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currencyManager)}
Count：{currencyManager.Count}
ListIsNull：{currencyManager.List == null}"
)}"
					, e);
				}
			}
		}

		IDisposable TraceDataSource(object dataSource)
		{
			if (dataSource is IBusinessObjectCollectionView dataView)
			{
				dataView.IsInDataBinding = true;
				return new DisposableAction(() => { dataView.IsInDataBinding = false; });
			}
			return null;
		}

		bool isBound;

		GridHumanReadableNameProvider humanReadableNameProvider;

		public virtual void SetDataBinding(object dataSource, string dataMember, string tableName)
		{
			PokePropertiesForFlattenHierarchy(dataSource, dataMember);

			if (humanReadableNameProvider != null)
			{
				if (DataSource is IBusiness business && business.Factory != null)
				{
					HumanReadableNameProviderManager.UnregisterCaptionProvider(humanReadableNameProvider, business.Factory);
					humanReadableNameProvider = null;
				}
			}

			if (dataSource == null)
			{
				if (!DesignModeFinder.IsDesigning && isBound)
				{
					SaveUserLayoutSettings();
				}
				SetDataBindingInternal(null, "", "");
				ClearTableStyle();
				Columns.DisposeAllColumns();
				UnhookNotificationsChanged();
			}
			else
			{
				foreach (var column in Columns)
				{
					if (!(column is IZColumn || column.ColumnStyle is IZColumn))
					{
						deprecatedColumnErrors.Add("\n   " + column.ColumnStyle.MappingName + " <" + column.GetType().Name + ">       ");
					}
				}

				columnsNeedReordering &= Columns.Count > 0;
				var thereWereNoColumns = Columns.Count == 0;

				using (SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					if (LimitedColumns != null)
					{
						if (!string.IsNullOrEmpty(LimitedColumns.CodeColumnName) && !Columns.Contains(LimitedColumns.CodeColumnName))
						{
							var codeColumn = new ZTextBoxColumnStyleInfo
							{
								CaptionResourceString = Res.GetData("4ff18233-20b0-4a6d-8de0-417bd8c5a353", "Code"),
								ColumnName = LimitedColumns.CodeColumnName
							};
							Columns.Add(codeColumn);
						}

						if (!string.IsNullOrEmpty(LimitedColumns.DescriptionColumnName) && !Columns.Contains(LimitedColumns.DescriptionColumnName))
						{
							var descriptionColumn = new ZTextBoxColumnStyleInfo
							{
								CaptionResourceString = Res.GetData("ca2a9a74-55e4-4221-a013-71724d44a7cd", "Description"),
								ColumnName = LimitedColumns.DescriptionColumnName
							};

							Columns.Add(descriptionColumn);
						}
					}
					else
					{
						foreach (ZGridColumnInfo columnStyle in ColumnStyles)
						{
							if (!(columnStyle is IZColumnStyleInfo))
							{
								deprecatedColumnErrors.Add("\n   " + columnStyle.ColumnName + " <" + columnStyle.GetType().Name + ">       ");
							}

							if (!Columns.Contains(columnStyle.ColumnName) && !CachedColums.ContainsKey(columnStyle.ColumnName))
							{
								Columns.Add(columnStyle);
							}
							SetAvailability(!columnStyle.IsUnavailable, columnStyle.ColumnName);
						}
					}
				}

				if ((thereWereNoColumns || LastFocusedColumn == null) && Columns.Count > 0)
				{
					LastFocusedColumn = Columns[0].ColumnStyle as ZTextBoxColumnStyle;
				}

				if (columnsNeedReordering)
				{
					ReOrderColumsBaseOnColumnStyleOrder();
				}
				ShowDeprecatedColumnErrors();
				deprecatedColumnErrors.Clear();

				SetDataBindingInternal(dataSource, dataMember, tableName);
				SetupAdditionalColumns();

				HookRelatedCurrencyManager();
				ReadOnly = guiReadOnly;

				if (ListManager != null)
				{
					ListManager.CurrentChanged += ListManager_CurrentChanged;
					ListManager_CurrentChanged(this, EventArgs.Empty);
				}
				HookNotificationsChanged();

				OnAfterBind();

				if (CopyColumnCaptionsToBoundFields
#if DEBUG
 && (!Globals.IsTest || CopyCaptionsToPropertyHumanReadableNameForTest)
#endif
)
				{
					if (dataSource is IBusiness bizo && bizo.Factory != null)
					{
						humanReadableNameProvider = new GridHumanReadableNameProvider(this);
						HumanReadableNameProviderManager.RegisterCaptionProvider(humanReadableNameProvider, bizo.Factory);
					}
				}
			}
			UpdateFrozenSort();
		}

		#region SetupAdditionalColumns

		void SetupAdditionalColumns()
		{
			foreach (ZGridColumnInfo columnInfo in ColumnStyles.ToArray())
			{
				ZGridColumn codeColumn;
				if (columnInfo is ZCodeFindBoxColumnStyleInfo && !(columnInfo is ZDescriptionCodeFindBoxColumnStyleInfo) && !columnInfo.ColumnName.Contains(".") &&
					(codeColumn = Columns[columnInfo.ColumnName]) != null && ElementType != null)
				{
					var propertyDescriptor = codeColumn.ColumnStyle.PropertyDescriptor ?? ZCustomTypeDescriptor.GetProperties(ElementType)[columnInfo.ColumnName];
					var listMember = propertyDescriptor != null ? MetadataAccessor.GetListMember(null, propertyDescriptor, columnInfo.ColumnName) : null;
					if (!string.IsNullOrEmpty(listMember))
					{
						var listType = GetPropertyType(ElementType, listMember.Split('.', '+'));
						if (listType != null && typeof(IBusinessObjectCollection).IsAssignableFrom(listType) && typeof(BusinessObjectCollection) != listType)
						{
							var listElementType = BusinessObjectCollection.GetElementTypeFromCollectionType(listType);
							if (listElementType != null && DescriptionPropertyAttribute.CanBeReferencedByDescription(listElementType))
							{
								var propertyType = propertyDescriptor.PropertyType;
								var descriptionColumnName = columnInfo.ColumnName + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix;
								if (!Columns.Contains(descriptionColumnName) && !CachedColums.ContainsKey(descriptionColumnName))
								{
									var descriptionInfo = propertyType == typeof(ZGuid) ? new ZDescriptionPkFindBoxColumnStyleInfo(propertyDescriptor) : new ZDescriptionCodeFindBoxColumnStyleInfo(propertyDescriptor);
									descriptionInfo.ColumnName = descriptionColumnName;
									descriptionInfo.DescriptionColumnName = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(listElementType);
									descriptionInfo.IsVisible = false;
									ControlDpiScalingHelper.SetWidth(ref descriptionInfo, 120, true);

									var descriptionColumnCaptionAttribute = propertyDescriptor.Attributes.OfType<ListColumnCaptionAttribute>().FirstOrDefault();
									if (descriptionColumnCaptionAttribute != null)
									{
										descriptionInfo.Caption = descriptionColumnCaptionAttribute.Caption;
									}
									else
									{
										descriptionInfo.Caption = codeColumn.ColumnStyle.HeaderText + " (" +
											DataBoundResourceStrings.GetColumnDescriptiveName(GetTableName(descriptionInfo.DescriptionColumnName), descriptionInfo.DescriptionColumnName) + ")";
									}
									ColumnStyles.Add(descriptionInfo);
									Columns.Add(descriptionInfo);
									SetAvailability(!descriptionInfo.IsUnavailable, descriptionInfo.ColumnName);
								}
							}
						}
					}
				}
			}

			LoadUserLayoutSettings();
			GridLayoutManager.SetupNewGroupColumnsToBeVisibleAndCorrectlyOrdered(this);
		}

		string GetTableName(string dataMember)
		{
			if (dataMember.Length < 3 || dataMember[2] != '_')
			{
				return "";
			}

			var tablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(dataMember);
			var tableSchema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(tablePrefix);
			return tableSchema?.TableName;
		}

		internal Type ElementType
		{
			get
			{
				if (felementType == null && List is IBusinessObjectCollection)
				{
					felementType = BusinessObjectCollection.GetElementTypeFromCollectionType(List.GetType());
				}
				return felementType;
			}
		}
		Type felementType;

#if DEBUG
		internal
		void SetElementTypeForTest(Type newType)
		{
			felementType = newType;
		}
#endif

		Type GetPropertyType(Type component, string[] propertyPath)
		{
			var currentType = component;
			for (var i = 0; i < propertyPath.Length && currentType != null; i++)
			{
				currentType = GetPropertyType(currentType, propertyPath[i]);
			}
			return currentType;
		}

		Type GetPropertyType(Type component, string propertyName)
		{
			PropertyInfo property = null;

			try
			{
				property = component.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
			}
			catch (AmbiguousMatchException)
			{
				while (property == null && component != null)
				{
					property = component.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
					component = component.BaseType;
				}
			}

			return property?.PropertyType;
		}

		#endregion

		#region CopyCaptionsToPropertyHumanReadableName

#if DEBUG
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CopyCaptionsToPropertyHumanReadableNameForTest { get; set; }
#endif

		#endregion

		public virtual void ResetCachedBindingContext()
		{
			bindingContext = null;
			_ = BindingContext;
		}

#if DEBUG
		internal
#endif
		void SetDataBindingInternal(object dataSource, string dataMember, string tableName)
		{
			if (dataSource == null)
			{
				DataBindings.Clear();
				isBound = false;
				base.SetDataBinding(null, "");
			}
			else
			{
				if (Columns.Count > 0)
				{
					SuspendLayout();

					try
					{
						TableName = tableName;
						DefaultColumns = Columns.Clone();
						RefreshTableStyles();

						if (TableStyles.Count == 0)
						{
							ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, ControlDescription.GetControlPath(this) + " - Please set the TableStyle before binding."));
						}

						base.SetDataBinding(dataSource, dataMember);

						ListManager.DataError += ListManager_DataError;

						isBound = true;
						OnAfterDataBound();

						LoadUserLayoutSettings();
					}
					finally
					{
						ResumeLayout();
					}
				}
				else if (LimitedColumns == null)
				{
					throw new InvalidOperationException("Please use the Grid Designer to setup your columns in the ZGrid.");
				}

				ResetCachedBindingContext();
			}
		}

		#endregion

		#region IEditableControl Members

		bool IEditableControl.IsEditing
		{
			get { return GetState(GridState.IsEditing) || IsCurrentRowUncommittedBusinessObject; }
		}

		bool IsCurrentRowUncommittedBusinessObject
		{
			get
			{
				var result = false;
				if (ListManager != null && ListManager.Count > 0 && ListManager.Position != -1)
				{
					result = IsUncommittedBusinessObject(ListManager.GetCurrent() as BusinessObject);
				}
				return result;
			}
		}

		bool IsUncommittedBusinessObject(BusinessObject element)
		{
			return element != null && ((INeedRow)element).Row != null && ((INeedRow)element).Row.RowState == DataRowState.Detached;
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region IListEditableControl

		IDisposable IListEditableControl.PreserveCurrentSelection()
		{
			var preservedCell = CurrentCell;

			var result = new DisposableAction(delegate
			{
				if (ListManager != null && preservedCell.RowNumber < ListManager.Count)
				{
					try
					{
						CurrentCell = preservedCell;
					}
					catch (ArgumentException)
					{
						// Sometimes it fails to set CurrentCell and we get ArgumentException with message:"CurrentCell cannot be set at this time. Moving your code to the Form.Load event should solve this problem." 
						// In this case it's better just do not set current cell and don't have Runtime Exception thrown to user.
						// This solves WI00067626 and is a copy of the behavior used on 'SetCellFromMouseClick', which presented the same problem
					}
				}
			});

			return result;
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(
			Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			dataMember = dataMember.Trim();

			foreach (ZGridColumnInfo column in ColumnStyles)
			{
				if (string.IsNullOrWhiteSpace(column.ColumnName))
				{
					result.Add(new CompileTimeCheckBindingMember(
						"Grid column of type '" + column.GetType().FullName +
						"' doesn't have a required ColumnName specified."));
				}
				else
				{
					if (column is IBindingMemberForCompileTimeCheckProvider columnCompileTimeCheck)
					{
						result.AddRange(columnCompileTimeCheck.GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember));
					}
				}
			}
			return result;
		}

		#endregion

		#region ICaptionedComponents

		public object GetCaptionedComponentAt(Point p)
		{
			var hitTestInfo = HitTest(p);
			return hitTestInfo.Type == HitTestType.ColumnHeader ? (int?)hitTestInfo.Column : null;
		}

		public Rectangle GetCaptionedComponentRect(object component)
		{
			var x0 = this.TableStyles[0].RowHeaderWidth + ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
			var x = x0;
			for (var i = 0; i < (int)component; i++)
			{
				x += this.Columns[i].ColumnStyle.Width;
			}
			x -= this.HorizScrollBar.Value;
			var dw = 0;
			if (x < x0)
			{
				dw = x0 - x;
				x = x0;
			}
			return ControlDpiScalingHelper.NewScaledRectangle(x, ControlDpiScalingHelper.ScaleToCurrentDpiY(3), this.Columns[(int)component].ColumnStyle.Width - dw, this.TableStyles[0].HeaderFont.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(3), false);
		}

		public object GetCaptionedComponentData(object component)
		{
			return this.Columns[(int)component];
		}

		#endregion

		#region Hotkeys

		string IHotkeyProvider.TypeNameForDisplay
		{
			get { return Res.GetString("Hotkeys|Grid", "Grid"); }
		}

		public HotkeyRegister Hotkeys { get; } = new HotkeyRegister();

		#endregion

		#region GetCurrentColumnStyle

		public DataGridColumnStyle GetCurrentColumnStyle()
		{
			var columnNumber = CurrentCell.ColumnNumber;
			return columnNumber >= 0 && TableStyles.Count > 0 && columnNumber < TableStyles[0].GridColumnStyles.Count ?
				TableStyles[0].GridColumnStyles[columnNumber] :
				null;
		}

		#endregion

		public void SetGridColourLayout(ZGuid gridColourScheme)
		{
			var scheme = GridLayoutManager.GetGridColourLayout(gridColourScheme);
			SetGridColourLayout(scheme);
		}

		public void SetGridColourLayout(GridColourScheme gridColourScheme)
		{
			if (gridColourSchemeManager != null && gridColourScheme != null)
			{
				gridColourSchemeManager.SetActiveGridColour(gridColourScheme);
			}
		}

		public virtual GridColourScheme GetLastUsedColourSchemeForCurrentUser
		{
			get { return gridColourSchemeManager?.GridColourFactory.GetLastUsedSchemeForCurrentUser(gridColourSchemeManager.FilterBusinessObject); }
		}

		readonly internal TranslationFeedbackManager translationFeedbackManager;
		readonly internal DevInfoPopupManager devInfoPopupManager;

		#region Test Tools
#if DEBUG

		public void OnPopup_CallForTesting()
		{
			typeof(ContextMenu).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, ContextMenu, new object[] { EventArgs.Empty });
		}

		[DefaultValue(100)]
		public int HorizontalScrollBarMaximum
		{
			get { return HorizScrollBar.Maximum; }
		}

		internal INotificationType NotificationTypeForTesting
		{
			get { return notificationType; }
		}

		internal int UpdateNonCellNotificationsCountForTesting;
		internal int UpdateGridNotificationTypeCountForTesting;
		internal bool IsTestingUpdateNonCellNotifications;

		internal Image LastDrawnCornerIconForTesting;

		public Point MousePositionForTesting;

		/// <summary>
		/// Invokes OnDoubleClick event
		/// </summary>
		public void PerformDoubleClickForTest(int row = 0, int column = -1)
		{
			SetCurrentHitTestForTest(row, column);
			OnDoubleClick(EventArgs.Empty);
		}

		/// <summary>
		/// Triggers the OnMouseDoubleClick event for a specified row, invoking all subscribed event handlers for the MouseDoubleClick event.
		/// </summary>
		public void PerformMouseDoubleClickForTest(int row, int column = -1)
		{
			SetCurrentHitTestForTest(row, column);
			OnMouseDoubleClick(GetMouseArgsFormRow(row, 2));
		}

		/// <summary>
		/// Invokes OnMouseDown event with specified row and clicks
		/// </summary>
		public void PerformMouseDownForTest(int row, int clicks, int column = -1) => PerformMouseDownForTest(GetMouseArgsFormRow(row, clicks), row, column);

		/// <summary>
		/// Invokes OnMouseDown event with specified MouseEventArgs
		/// </summary>
		public void PerformMouseDownForTest(MouseEventArgs e, int row = 0, int column = -1)
		{
			SetCurrentHitTestForTest(row, column);
			OnMouseDown(e);
		}

		/// <summary>
		/// Invokes OnMouseUp event
		/// </summary>
		public void PerformMouseUpForTest(int row = 0, int column = -1) => PerformMouseUpForTest(GetMouseArgsFormRow(row), row, column);

		/// <summary>
		/// Invokes OnMouseUp event
		/// </summary>
		public void PerformMouseUpForTest(MouseEventArgs e, int row = 0, int column = -1)
		{
			SetCurrentHitTestForTest(row, column);
			OnMouseUp(e);
		}

		MouseEventArgs GetMouseArgsFormRow(int row, int clicks = 1)
		{
			var rowNotificationsRectangle = GetRowNotificationRectangle(row);
			return new MouseEventArgs(MouseButtons.Left, clicks, rowNotificationsRectangle.X, rowNotificationsRectangle.Y, 0);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Points not used for rendering")]
		public void SetCurrentHitTestForTest(int rowNum, int colNum)
		{
#if WINZOR
			var hitTestType = HitTestType.Cell;

			if (rowNum == -1)
			{
				hitTestType = HitTestType.ColumnHeader;
			}
			else if (colNum == -1)
			{
				hitTestType = HitTestType.RowHeader;
			}

			SetCurrentHitTestInfo(rowNum, colNum, hitTestType);
#else
			if (colNum != -1 && rowNum != -1)
			{
				var rowRect = RectangleToScreen(GetCellBounds(rowNum, colNum));
				MousePositionForTesting = new Point((rowRect.Left + rowRect.Right) / 2, (rowRect.Top + rowRect.Bottom) / 2);
			}
#endif
		}

#if !WINZOR

		public void HorizontalScrollToOffset(int pixelOffset)
		{
			GridHScrolled(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, pixelOffset));
		}

		public bool MouseMovedFarEnoughForTest(Point movedTo)
		{
			return MouseMovedFarEnough(movedTo);
		}

		public Point GetMovedFromForTest()
		{
			return movedFrom;
		}

#else

		public void HorizontalScrollToOffset(int pixelOffset)
		{
			Invalidate();
		}

		public bool MouseMovedFarEnoughForTest(Point movedTo) => false;

		public Point GetMovedFromForTest() => Point.Empty;

		internal bool isActiveGridLayoutIconDrawn;

#endif

#endif
#if WINZOR

#if DEBUG
		protected void GridVScrolled(object sender, ScrollEventArgs e)
		{
			var scrollTop = e.NewValue * 16;
			OnVerticalScroll(scrollTop);
		}
#endif

		int RowHeaderClickX => 0;
#endif
#endregion
	}

	public enum RemoveAction
	{
		NoRemovePossible,
		Remove,
		RemoveAndDelete
	}

	#region Custom Event Args

	public class RowsDeletingEventArgs : CancelEventArgs
	{
		public RowsDeletingEventArgs(IEnumerable<BusinessObject> objects)
		{
			Objects = objects ?? Enumerable.Empty<BusinessObject>();
		}

		public IEnumerable<BusinessObject> Objects
		{
			get;
			private set;
		}
	}

	public sealed class GridColourSchemeManagerDecidingEventArgs : EventArgs
	{
		public GridColourSchemeManager GridColourSchemeManager { get; set; }
	}

	public sealed class ColourDecidingEventArgs : EventArgs
	{
		public ColourDecidingEventArgs(object objectAtRow)
		{
			ObjectAtRow = objectAtRow;
			if (objectAtRow is BusinessObject bizo)
			{
				Pk = bizo.PK;
			}
		}

		public object ObjectAtRow { get; private set; }
		public Color Colour { get; set; }
		public Color ReadOnlyColour { get; set; }
		internal ZGuid Pk;
		internal int RowNumber;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not a code smell, and also used for debugging only")]
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"ObjectAtRow:{0} Colour:{1} ReadOnlyColour:{2} Pk:{3}",
				ObjectAtRow.ToString(), Colour.ToString(), ReadOnlyColour.ToString(), Pk);
		}
	}

	public sealed class FontDecidingEventArgs : EventArgs
	{
		public FontDecidingEventArgs(object objectAtRow, string dataMember, Font originalFont)
		{
			ObjectAtRow = objectAtRow;
			DataMember = dataMember;
			OriginalFont = originalFont;
		}

		public object ObjectAtRow { get; private set; }
		public Font Font { get; set; }
		public Font OriginalFont { get; private set; }
		public string DataMember { get; private set; }
	}

	public class GridRowOnDeletedEventArgs : EventArgs
	{
		public GridRowOnDeletedEventArgs(BusinessObject[] deletedBusinessObjects)
		{
			this.DeletedBusinessObjects = deletedBusinessObjects;
		}
		public readonly BusinessObject[] DeletedBusinessObjects;
	}

	#endregion
}
