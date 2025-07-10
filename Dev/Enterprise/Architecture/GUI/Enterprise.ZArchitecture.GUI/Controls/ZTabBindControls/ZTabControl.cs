using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.Core.Environment;
using Enterprise.Core.Forms;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.PlugIn;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	public class ZTabControl : KTabControl, ITabOrderExtendedToTabPages, ICaptionedComponents, IExtendedControl
	{
		public ZTabControl()
		{
			ImageList = Icons.ImageList;
			UserEventTracker.Instance.AddUserEventToControl(this);
			DisposableLeakListener.Instance.RegisterDisposable(this);
			Anchor = FullyAnchored;
			SetTabPageCollection();
			TabPageVisibilityManager = new TabPageVisibilityManager(this);
			TabPageVisibilityManager.Enabled = true;
			translationFeedbackManager = new TranslationFeedbackManager(this);
			devInfoPopupManager = new DevInfoPopupManager(this);
			Extensions = NewExtensionCollection();
		}

		#region Selected Tab

		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			PreviousTab = SelectedTab;
		}

		Control GetFocusedControl()
		{
#if !WINZOR
			Control focusedControl = null;
			var focusedHandle = UnsafeNativeMethods.GetFocus();
			if (focusedHandle != IntPtr.Zero)
			{
				focusedControl = Control.FromHandle(focusedHandle);
			}
			return focusedControl;
#else
			return this.FindForm()?.LastFocusedControl;
#endif
		}

		bool ControlHasZGrid_WithUncommittedBusinessObject_WithinThreeParents(Control control)
		{
			if (control == null)
			{
				return false;
			}

			for (var i = 0; i < 3; ++i)
			{
				control = control.Parent;
				if (control == null)
				{
					return false;
				}
				var zGrid = control as ZGrid;
				if (zGrid != null)
				{
					var bizO = zGrid.GetCurrent();
					if (bizO == null || !bizO.IsInDatabase)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal exception message")]
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			if (SelectedIndex >= TabPages.Count)
			{
				SelectedIndex = TabPages.Count > 0 ? 0 : -1;
			}

			InSelectedIndexChanged = true;
			try
			{
				if (!IsDisposing)
				{
					OnSelectedIndexChanging(e);
					if (SelectedTab != null)
					{
						SelectedTab.NotifyBindingOrShowing();
					}

					try
					{
						base.OnSelectedIndexChanged(e);
					}
					catch (InvalidOperationException ex)
					{
						var errorMessage = new StringBuilder(ex.Message);
						errorMessage.AppendLine("PreviousTab is: ").Append(PreviousTab?.Text);
						errorMessage.AppendLine("SelectedTab is: ").Append(SelectedTab?.Text);
						errorMessage.AppendLine("IsHandleCreated is: ").Append(IsHandleCreated);
						ErrorReporter.ReportOnce("InvalidOperationException_OnSelectedIndexChanged", errorMessage.ToString(), ex);
						return;
					}

					//Because base.OnSelectedIndexChanged(e) does focusing (including OnEnter) of ZGrids, which opens uncommitted rows, before binding then loads collections and boots out said uncommitted row, we have to do one more focus to fix this.
					var focusedControl = GetFocusedControl();
					if (ControlHasZGrid_WithUncommittedBusinessObject_WithinThreeParents(focusedControl))
					{
						this.Focus();
						focusedControl.Focus();
					}
				}
			}
			finally
			{
				InSelectedIndexChanged = false;
			}

			if (!DesignModeFinder.IsDesigning)
			{
				BindSelectedBindingTab();
				SetFormSize();
				EnsureReadOnlySecurity();
			}

			PreviousTab = SelectedTab;
		}

		protected bool InSelectedIndexChanged { get; private set; }

#if WINZOR
		public string TabControlAlignmentStyle { get; set; }

		public string TabControlNavigationAdditionalStyles { get; set; }

		public string TabControlButtonAdditionalStyles { get; set; }

		public string TabPageAdditionalStyles { get; set; }

		protected override string TabControlAlignment
		{
			get
			{
				return DrawMode is TabDrawMode.OwnerDrawFixed ? TabControlAlignmentStyle : "";
			}
		}

		protected override string TabControlNavigationStyleString
		{
			get
			{
				return DrawMode is TabDrawMode.OwnerDrawFixed ? TabControlNavigationAdditionalStyles : "";
			}
		}

		protected override string TabControlButtonStyleString
		{
			get
			{
				return DrawMode is TabDrawMode.OwnerDrawFixed ? TabControlButtonAdditionalStyles : "";
			}
		}

		protected override string TabControlTabPageStyleString
		{
			get
			{
				return DrawMode is TabDrawMode.OwnerDrawFixed ? TabPageAdditionalStyles : "";
			}
		}

#endif

		void EnsureReadOnlySecurity()
		{
			if (Secured && SelectedTab != null && !TabPermissionChecker.IsEditAllowed(SelectedTab.Name))
			{
				SelectedTab.SetReadOnlyIncludingChildren(true);
			}
		}

		protected virtual void OnSelectedIndexChanging(EventArgs e)
		{
			if (!IsDisposing)
			{
				if (SelectedIndexChanging != null)
				{
					SelectedIndexChanging(this, e);
				}
			}
		}

		ZTabPage PreviousTab;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ZTabPage SelectedTab
		{
			get
			{
				return base.SelectedTab as ZTabPage;
			}
			set { base.SelectedTab = value; }
		}

		#endregion

		#region Anchor

		[DefaultValue(FullyAnchored)]
		public override AnchorStyles Anchor
		{
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		protected const AnchorStyles FullyAnchored = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

		#endregion

		#region PlugIns

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public PlugIns PlugIns
		{
			get
			{
				if (fPlugIns == null)
				{
					fPlugIns = new PlugIns(GetTopLevelBusinessEntityForPlugIns(), this);
				}
				return fPlugIns;
			}
		}

		protected virtual IBusiness GetTopLevelBusinessEntityForPlugIns()
		{
			return Form != null ? Form.GetTopLevelBusinessEntityForPlugIn() : null;
		}

		void SetupPlugInTabPagesAndMenus()
		{
			SetupPlugInMenus();
			SetupPlugInTabPages();
		}

		void SetupPlugInTabPages()
		{
			if (!PlugInTabsSetup)
			{
				PlugIns.AddPlugInTabPages(this);
				PlugIns.SynchronisePlugInWithVisibleTabPage();
				PlugInTabsSetup = true;
			}
		}

		void SetupPlugInMenus()
		{
			if (!PlugInMenusSetup && Form != null)
			{
				using (PerformanceStatisticsCollector.StartMonitoring("SetupPluginMenus", Form.GetType().FullName))
				{
					ZFormPlugInStrategy.InsertPlugInMenuItems(Form, PlugIns);
					PlugInMenusSetup = true;
				}
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (Parent != null)
			{
				SynchronizationContext.Current.Post(delegate
				{
					if (!DesignModeFinder.IsDesigning && !IsDisposed && Form != null)
					{
						SetupPlugInMenus();
					}
				}, null);
			}
		}

		bool PlugInTabsSetup;
		internal bool PlugInMenusSetup;
		PlugIns fPlugIns;

		#endregion

		#region Tab Page Collection

		protected override Type TabPageType
		{
			get { return typeof(ZTabPage); }
		}

		public new TabControl.ControlCollection Controls
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (TabControl.ControlCollection)base.Controls; }
		}

		[SmartTagVisible]
		[Editor(typeof(ZTabPageCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public new TabPageCollection TabPages
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (TabPageCollection)base.TabPages; }
		}

		protected void SetTabPageCollection()
		{
#if NETFRAMEWORK || WINZOR
			const string fieldName = "tabCollection";
#else
			const string fieldName = "_tabCollection";
#endif

			typeof(TabControl).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, CreateTabPagesInstance());
		}

		protected virtual TabPageCollection CreateTabPagesInstance()
		{
			return new TabPageCollection(this);
		}

		public new class TabPageCollection : TabControl.TabPageCollection
		{
			public TabPageCollection(ZTabControl owner)
				: base(owner)
			{
				this.Owner = owner;
			}

			readonly ZTabControl Owner;

			/// <summary>
			/// Inserts a ZTabPage at the specified Index.
			/// If the Index is greater than the TabControl.SelectedIndex, the Visibility of your TabPages will not be affected.
			/// </summary>
			/// <param name="tabPage">The ZTabPage to insert.</param>
			/// <param name="index">The Index to insert at.</param>
			public void Insert(ZTabPage tabPage, int index)
			{
				this.InsertPage(tabPage, index);

				Owner.SuspendLayout();
				try
				{
					var bindInsertedTab = (index == Owner.SelectedIndex || Owner.SelectedIndex == -1);
					if (bindInsertedTab)
					{
						Owner.BindSelectedBindingTab();
					}
				}
				finally
				{
					Owner.ResumeLayout();
				}
			}
		}

		public void SelectNextTabPage()
		{
			var nextTabIndex = SelectedIndex + 1;
			if (nextTabIndex >= TabCount)
			{
				nextTabIndex = 0;
			}

			SelectedIndex = nextTabIndex;

			Focus();
		}

		public void SelectPreviousTabPage()
		{
			var previousTabIndex = SelectedIndex - 1;
			if (previousTabIndex < 0)
			{
				previousTabIndex = TabCount - 1;
			}

			SelectedIndex = previousTabIndex;

			Focus();
		}

		public event EventHandler SelectedIndexChanging;

		#endregion

		#region All Tab Page Collection

		public TabPage[] AllTabPages
		{
			get { return TabPageVisibilityManager.AllTabPages; }
		}

		internal void RemoveFromAllTabPages(TabPage tabPage)
		{
			TabPageVisibilityManager.RemoveFromAllTabPages(tabPage);
		}

		#endregion

		#region Focus

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			if (!fSelectedControlForInitialTabPageFocus &&
				Created &&
				IsHandleCreated &&
				!DesignModeFinder.IsDesigning)
			{
				fSelectedControlForInitialTabPageFocus = true;
				if (SelectedTab != null &&
					!SelectedTab.ContainsFocus &&
					!InSelectedIndexChanged)
				{
					var nextControl = SelectedTab.GetNextControl(SelectedTab, true);
					if (nextControl != null)
					{
						nextControl.Focus();
					}
				}
			}
		}

		bool fSelectedControlForInitialTabPageFocus;

		#endregion

		#region Form Size

		void SetFormSize()
		{
			if (SelectedTab != null && FormIsAutoSizedByTabControl) // can be null if the tab is removed
			{
				if (Anchor != FullyAnchored && Dock != DockStyle.Fill)
				{
					Anchor = FullyAnchored;
				}

				SetFormInitialValues();

				if (SelectedTab.IsAutoSized)
				{
					ResizeFormToFitAutoSizedTab(SelectedTab.MinimumAutoSizedWidth + SelectedTab.Left, SelectedTab.MinimumAutoSizedHeight + SelectedTab.Top);
				}
				else
				{
					ResetForm();
				}
			}
		}

		void SetFormInitialValues()
		{
			if (Form != null && ((PreviousTab != null && !PreviousTab.IsAutoSized) || (PreviousTab == null && SelectedTab != null && SelectedTab.IsAutoSized) || isFormResized))
			{
				ParentFormInitialSize = Form.WindowState != FormWindowState.Maximized ? Form.Size : Form.RestoreBounds.Size;
				ParentFormInitialMinimumSize = Form.MinimumSize;
				ParentFormInitialBorderStyle = Form.FormBorderStyle;
				isFormResized = false;
			}
		}

		void ResetForm()
		{
			if (FormInitialSizeSet)
			{
				SuspendLayout();
				try
				{
					if (Form.WindowState == FormWindowState.Maximized &&
						ParentFormInitialBorderStyle != FormBorderStyle.Sizable &&
						ParentFormInitialBorderStyle != FormBorderStyle.SizableToolWindow)
					{
						Form.WindowState = FormWindowState.Normal;
					}

					Form.FormBorderStyle = ParentFormInitialBorderStyle;
					Form.MinimumSize = ParentFormInitialMinimumSize;
					Form.Size = ParentFormInitialSize;

					ParentFormInitialSize = new Size();
					ParentFormInitialMinimumSize = new Size();
					ParentFormInitialBorderStyle = FormBorderStyle.None;
				}
				finally
				{
					ResumeLayout();
				}
			}
		}

		void ResizeFormToFitAutoSizedTab(int minimumAutoSizedTabPageWidth, int minimumAutoSizedTabPageHeight)
		{
			var deltaWidth = Form.WindowState == FormWindowState.Maximized ? Form.Bounds.Width - Form.RestoreBounds.Width : 0;
			var deltaHeight = Form.WindowState == FormWindowState.Maximized ? Form.Bounds.Height - Form.RestoreBounds.Height : 0;

			var sourceWidth = Width - deltaWidth;
			var sourceHeight = Height - deltaHeight;

			var tabWidthIsTooSmall = sourceWidth < minimumAutoSizedTabPageWidth;
			var tabHeightIsTooSmall = sourceHeight < minimumAutoSizedTabPageHeight;

			if (tabWidthIsTooSmall || tabHeightIsTooSmall)
			{
				SuspendLayout();
				try
				{
					Form.FormBorderStyle = FormBorderStyle.Sizable;

					var formBounds = Form.WindowState != FormWindowState.Maximized ? Form.Bounds : Form.RestoreBounds;
					var currentScreenInfo = GetCurrentScreenInfo(formBounds.Location);

					if (tabWidthIsTooSmall)
					{
						ControlDpiScalingHelper.SetWidth(ref formBounds, formBounds.Width + minimumAutoSizedTabPageWidth - sourceWidth, false);
						if (formBounds.Right > currentScreenInfo.Right) // if width exceeds right edge of screen
						{
							ControlDpiScalingHelper.SetX(ref formBounds, formBounds.X - (formBounds.Right - currentScreenInfo.Right), false);
							if (formBounds.X < 0)
							{
								ControlDpiScalingHelper.SetX(ref formBounds, 0, true);
							}
						}
					}

					if (tabHeightIsTooSmall)
					{
						ControlDpiScalingHelper.SetHeight(ref formBounds, formBounds.Height + minimumAutoSizedTabPageHeight - sourceHeight, false);
						if (formBounds.Bottom > currentScreenInfo.Bottom) // if height exceeds bottom edge of screen
						{
							ControlDpiScalingHelper.SetY(ref formBounds, formBounds.Y - (formBounds.Bottom - currentScreenInfo.Bottom), false);
							if (formBounds.Y < 0)
							{
								ControlDpiScalingHelper.SetY(ref formBounds, 0, true);
							}
						}
					}

					Form.Bounds = formBounds;
					Form.MinimumSize = formBounds.Size;
				}
				finally
				{
					ResumeLayout();
				}
			}
		}

		Rectangle GetCurrentScreenInfo(Point position)
		{
			foreach (var screenInfo in CachedScreenInfo.Instance.ScreenInfos)
			{
				if (screenInfo.Contains(position))
				{
					return screenInfo;
				}
			}
			return CachedScreenInfo.Instance.PrimaryScreenInfo;
		}

		bool FormIsAutoSizedByTabControl
		{
			get
			{
				var tabControlIsTopLevelTabControl = (Form != null && this == Form.TopLevelTabControl);
				var formHasBeenAutoSizedByTab = (ParentFormInitialBorderStyle != FormBorderStyle.None);
				var formBorderIsFixed = (Form != null && Form.FormBorderStyle != FormBorderStyle.Sizable && Form.FormBorderStyle != FormBorderStyle.SizableToolWindow);
				var formIsResizableByTabPageAllowed = (Form != null && Form.IsResizableByTabPageAllowed);

				return tabControlIsTopLevelTabControl && (formHasBeenAutoSizedByTab || formBorderIsFixed || formIsResizableByTabPageAllowed);
			}
		}

		bool FormInitialSizeSet
		{
			get { return ParentFormInitialSize.Height != 0 && ParentFormInitialSize.Width != 0; }
		}

#if DEBUG
		internal
#endif
		void FormResizeComplete(object sender, EventArgs e)
		{
			isFormResized = true;
		}
		bool isFormResized;

		protected ZForm Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = FindForm() as ZForm;
					if (fForm != null)
					{
						fForm.ResizeComplete += FormResizeComplete;
					}
				}
				return fForm ?? (fForm = FindForm() as ZForm);
			}
		}
		ZForm fForm;

		Size ParentFormInitialSize;
		Size ParentFormInitialMinimumSize;
		FormBorderStyle ParentFormInitialBorderStyle = FormBorderStyle.None;

		#endregion

		#region Minimum Tab Width

		const int MinimumTabWidth = 10;

		protected override void OnHandleCreated(EventArgs e)
		{
			try
			{
				base.OnHandleCreated(e);
				if (!DesignModeFinder.IsDesigning)
				{
#if !WINZOR
					UnsafeNativeMethods.PostMessage(new HandleRef(this, this.Handle), SafeNativeMethods.TCM_SETMINTABWIDTH, new IntPtr(0), new IntPtr(MinimumTabWidth));
#else
					SetMinTabWidth(MinimumTabWidth);
#endif
					if (Visible)
					{
						if (!IsHandleCreated)
						{
							//read from https://stackoverflow.com/questions/5932836/when-is-the-window-handle-created that this can help...
							//remove this code if it doesn't seem to do anything useful ofc
							_ = this.Handle;
						}

						RunWhenDataSourceAvailable(delegate
						{ BindSelectedBindingTab(); EnsureReadOnlySecurity(); });
						BeginInvoke(new MethodInvoker(delegate
						{ RunWhenDataSourceAvailable(SetupPlugInTabPagesAndMenus); }));
					}
				}
			}
			catch (InvalidOperationException ex)
			{
#if !WINZOR
				var terminalService = ObjectFactory.Get<TerminalService>();
				var terminalServiceMessage = $@"TerminalService.IsCitrix: {terminalService.IsCitrixICA}, TerminalService.IsRemoteAppSession: {terminalService.IsRemoteAppSession}, TerminalService.IsWTSSession: {terminalService.IsWTSSession}
TerminalService.LastWin32Error: {terminalService.LastWin32Error}";
#else
				var terminalServiceMessage = (NoResString)"TerminalService is not supported in Winzor";
#endif
				var errorMessage = $@"Message: {ex.Message}
Selected Control Path: {ControlDescription.GetControlPath(this)}
Select Tab: {SelectedTab?.Name}
IsHandleCreated: {IsHandleCreated}
IsDisposed: {IsDisposed}
IsDisposing: {IsDisposing}
Invoke Required: {InvokeRequired}
Visible: {Visible}
{terminalServiceMessage}
DestroyHandleStackTrace: 
{destroyHandleStackTrace}";
				ErrorReporter.ReportOnce("InvalidOperationException_OnHandleCreated", errorMessage, ex);
			}
		}

		protected override void DestroyHandle()
		{
			destroyHandleStackTrace = System.Environment.StackTrace;
			base.DestroyHandle();
		}
		string destroyHandleStackTrace;

		#endregion

		#region Get Tab Page

		/// <summary>
		/// Gets the TabPage with the passed name.
		/// </summary>
		/// <param name="tabPageName">name of TabPage you are looking for</param>
		/// <returns>Found TabPage, or null if the requested TabPage was not found</returns>
		public ZTabPage GetTabPage(string tabPageName)
		{
			foreach (ZTabPage tab in TabPages)
			{
				if (tab.Name == tabPageName)
				{
					return tab;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the TabPage with the passed name or text.
		/// </summary>
		/// <param name="tabPageText">name or text of TabPage you are looking for</param>
		/// <returns>Found TabPage, or null if the requested TabPage was not found</returns>
		public ZTabPage GetTabPageByNameOrText(string tabPageText)
		{
			return TabPages.Cast<ZTabPage>().FirstOrDefault(tab => tab.Name == tabPageText || tab.Text == tabPageText);
		}

		#endregion

		#region ControlCollection

		protected override Control.ControlCollection CreateControlsInstance()
		{
			return new ControlCollection(this);
		}

		public new class ControlCollection : TabControl.ControlCollection
		{
			public ControlCollection(TabControl owner)
				: base(owner)
			{
			}

			public override void Add(Control value)
			{
				base.Add(value);

				var page = value as ZTabPage;
				if (page != null)
				{
					page.DoAdded();
				}
			}
		}

		#endregion

		#region Binding

		protected override void OnParentVisibleChanged(EventArgs e)
		{
			base.OnParentVisibleChanged(e);

			if (!DesignModeFinder.IsDesigning && Visible && Form != null && FindForm() != null)
			{
				BindSelectedBindingTabAndSetupPlugInsWhenDataSourceAvailable();
				UpdateInitialTabImageForAllTabPages();

				if (Secured && SelectedTab != null && !TabPermissionChecker.IsViewAllowed(SelectedTab.Name)) //TODO: have to put this at the appropriate place
				{
					SelectedTab.ReplaceAllWithCoveringLabel(TabPermissionChecker.ErrorMessageForViewNotAllowed(SelectedTab.Name));
				}
			}
		}

		void BindSelectedBindingTabAndSetupPlugInsWhenDataSourceAvailable()
		{
			RunWhenDataSourceAvailable(delegate
			{
				SetupPlugInTabPagesAndMenus();
				BindSelectedBindingTab();
			});
		}

		void RunWhenDataSourceAvailable(MethodInvoker invoker)
		{
			var bindingSource = KBindingSource.GetBindingSource(this);
			if (bindingSource != null)
			{
				if (bindingSource.DataSource != null)
				{
					invoker();
				}
				else
				{
					var handler = new EventHandlerReference();
					handler.Value = delegate
					{
						invoker();
						bindingSource.DataSourceChanged -= handler.Value;
					};
					bindingSource.DataSourceChanged += handler.Value;
				}
			}
		}

		class EventHandlerReference
		{
			public EventHandler Value;
		}

		void UpdateInitialTabImageForAllTabPages()
		{
			if (Parent != null && Parent.Visible)
			{
				foreach (TabPage tabPage in TabPages)
				{
					var zTabPage = tabPage as ZTabPage;
					if (zTabPage != null)
					{
						zTabPage.UpdateInitialTabImage();
					}
				}
			}
		}

		[DefaultValue(true)]
		public bool BindingEnabled
		{
			get { return BindingEnabledCore; }
		}

		protected virtual bool BindingEnabledCore
		{
			get { return true; }
		}

#if DEBUG
		protected
#endif
		void BindSelectedBindingTab()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("BindSelectedTab", Name))
			{
				var tab = SelectedBindingTab;

				if (tab != null &&
					Form != null &&
					!DesignModeFinder.IsDesigning &&
					Visible &&
					!tab.IsBound &&
					!tab.DelayBinding)
				{
					tab.Bind();
					tab.HasBeenMadeVisible = true;
				}
			}
		}

		ZBindingTabPage SelectedBindingTab
		{
			get { return SelectedTab as ZBindingTabPage; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				Extensions.Dispose();
				if (translationFeedbackManager != null)
				{
					translationFeedbackManager.Dispose();
				}
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}

				if (fPlugIns != null)
				{
					((IDisposable)fPlugIns).Dispose();
				}
				CanTabPageBeVisibleOnSetRelevantDelegate = null;

				if (fForm != null)
				{
					fForm.ResizeComplete -= FormResizeComplete;
					fForm = null;
				}

				IsDisposing = true;
			}

			base.Dispose(isNotFinalizing);
		}

		protected bool IsDisposing;

		#endregion

		#region Drag and Drop

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragDrop(this, drgevent);
			base.OnDragDrop(drgevent);
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragOver(this, drgevent);
			base.OnDragOver(drgevent);
		}

		[DefaultValue(true)]
		public override bool AllowDrop
		{
			get { return true; }
		}

		#endregion

		#region Visibility

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		internal TabPageVisibilityManager TabPageVisibilityManager { get; private set; }

		public delegate bool CanTabPageBeVisibleOnSetRelevantChecker(ZTabPage tabPage);

		public CanTabPageBeVisibleOnSetRelevantChecker CanTabPageBeVisibleOnSetRelevantDelegate;

		#endregion

		#region Security

		[Category("(K-Architecture)")]
		[DefaultValue(false)]
		public bool Secured { get; set; }

		protected override void OnSelecting(TabControlCancelEventArgs e)
		{
			base.OnSelecting(e);

			if (translationFeedbackManager.InMode || devInfoPopupManager.InMode)
			{
				e.Cancel = true;
			}
			else if (!DesignModeFinder.IsDesigning)
			{
				if (Secured && e.TabPage != null && !e.Cancel && !TabPermissionChecker.IsViewAllowed(e.TabPage.Name))
				{
					(e.TabPage as ZTabPage)?.ReplaceAllWithCoveringLabel(TabPermissionChecker.ErrorMessageForViewNotAllowed(e.TabPage.Name));
				}
			}

			if (!e.Cancel && !DesignModeFinder.IsDesigning && e.TabPage != null && !e.TabPage.IsHandleCreated && NativeMethods.GetWindowHandlesForCurrentProcess() > NativeMethods.GuiResourcesThreshold)
			{
				Globals.Message.Show(Res.GetString("357c7782-f3f0-45c4-9916-5a1b36b1a38f", "There are too many windows and/or graphical elements open by the application. Please close some unused windows and repeat this operation again."));
				e.Cancel = true;
			}
		}

		TabPermissionChecker TabPermissionChecker
		{
			get
			{
				return tabPermissionChecker
					?? (tabPermissionChecker = new TabPermissionChecker(FindZForm().SecurityToken, GetSecurity()));
			}
		}

		ZForm FindZForm()
		{
			return FindForm() as ZForm;
		}

		protected internal virtual IZSecurity GetSecurity()
		{
			return EnvProxy.Instance.Security;
		}

		TabPermissionChecker tabPermissionChecker;

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZTabControl>().Result;
		}

		#endregion

		#region ITabOrderExtendedToTabPages Members

		bool ITabOrderExtendedToTabPages.TabOrderExtendedToTabPages
		{
			get { return true; }
		}

		#endregion

		#region ICaptionedComponents

		public object GetCaptionedComponentAt(Point p)
		{
			int? tabIndex = null;
			for (var i = 0; i < this.TabPages.Count; i++)
			{
				if (GetTabRect(i).Contains(p))
				{
					tabIndex = i;
					break;
				}
			}
			return tabIndex;
		}

		public Rectangle GetCaptionedComponentRect(object component)
		{
			return GetTabRect((int)component);
		}

		public object GetCaptionedComponentData(object component)
		{
			return TabPages[(int)component];
		}

		#endregion

		#region IExtendedControl

		Control IExtendedControl.Host => this;

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		readonly internal TranslationFeedbackManager translationFeedbackManager;
		readonly internal DevInfoPopupManager devInfoPopupManager;

		IControlExtensionCollection NewExtensionCollection()
		{
			return new ControlExtensionCollection(this);
		}
	}

	#region Collection Editor

#if !WINZOR

	public class ZTabPageCollectionEditor : CollectionEditor
	{
		public ZTabPageCollectionEditor(Type collectionType) : base(collectionType) { }

		protected override Type CreateCollectionItemType()
		{
			return typeof(ZTabPage);
		}
	}

#else

	public class ZTabPageCollectionEditor
	{
	}

#endif

	#endregion
}
