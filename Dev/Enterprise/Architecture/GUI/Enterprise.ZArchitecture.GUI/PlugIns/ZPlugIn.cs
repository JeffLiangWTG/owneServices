using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Res = Enterprise.ZArchitecture.GUI.Res;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.ZArchitecture.PlugIn
{
	/// <summary>
	/// GUI Level PlugIn - allows a tab page and menu to be plugged in to another module.
	/// Useful as it uses ControllerIDs and thus can use reflection to access un-referenced assemblies.
	/// </summary>
	public abstract class ZPlugIn :
		IPlugInInternals,
		ILicensedComponent,
		IShowPreSaveDialog,
		IDisposable
	{
		protected ZPlugIn(IBusiness hostBusinessEntity)
		{
			this.HostBusinessEntity = hostBusinessEntity;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		#region Controller ID

		public ControllerID ControllerID
		{
			get { return Controller != null ? Controller.ID : null; }
		}

		public ZController Controller
		{
			get
			{
				return controller;
			}
			set
			{
				this.controller = value;
				if (fTabPage != null)
				{
					fTabPage.CaptionResourceString = value.PluginTabPageCaption;
				}
			}
		}
		ZController controller;

		#endregion

		#region Name

		public abstract string Name { get; }

		/// <summary>
		/// Special case Text for tab pages. If possible, consider use control-alt-right click of the tab page to enter a caption.
		/// Implementation should be be translatable (use Res.GetString() method).
		/// </summary>
		protected internal virtual string TextOverride { get { return null; } }

		#endregion

		#region GUI

		/// <summary>
		/// Called every time the PlugIn's menu is clicked on/shown.
		/// </summary>
		public virtual void OnMenuShown()
		{
			OnGUIShown();
		}

		/// <summary>
		/// Called every time the PlugIn's user control is displayed. 
		/// Eg, user goes to the PlugIn's tab.
		/// </summary>
		public virtual void OnUserControlShown()
		{
			OnGUIShown();
		}

		/// <summary>
		/// This is called every time the PlugIn is shown
		/// (eg, form is shown and plug in is first tab, or selected tab 
		/// is changed to show the plug in, or menu clicked on). 
		/// </summary>
		public virtual void OnGUIShown()
		{
			if (IsCurrentDependent)
			{
				HookupCurrentChanged();
			}
		}

		public virtual bool ShouldHideTopLevelMenuWithTab => true;

		/// <summary>
		/// Gets a menu item to add to the containing form's main menu. 
		/// Null means no menu add.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public MenuItem TopLevelMenu
		{
			get
			{
				if (null == fTopLevelMenu)
				{
					try
					{
						fTopLevelMenu = GetNewTopLevelMenu();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("Exception caught in GetNewTopLevelMenu, please fix the plugin.\r\n", ex);
					}
				}

				return fTopLevelMenu;
			}
		}

		protected virtual MenuItem GetNewTopLevelMenu()
		{
			return null;
		}

		MenuItem fTopLevelMenu;

		/// <summary>
		/// All menus to add to the form. By default, this is simply
		/// TopLevelMenu. Override this if your plug-in has more than
		/// one TopLevelMenu to add to the form.
		/// Null or Count of 0 means no menus.
		/// </summary>
		protected virtual MenuItem[] AllTopLevelMenus
		{
			get
			{
				var menus = Array.Empty<MenuItem>();
				if (TopLevelMenu != null)
				{
					menus = new MenuItem[] { TopLevelMenu };
				}
				return menus;
			}
		}

		/// <summary>
		/// Gets the UserControl for the plugged in module.
		/// Will be added to a tab page on the containing form.
		/// This will be lazy added to the GUI.
		/// Null means no UserControl.
		/// </summary>
		public Control UserControl
		{
			get
			{
				if (fUserControl == null || fUserControl.IsDisposed)
				{
					fUserControl = GetNewUserControl();
				}
				return fUserControl;
			}
		}

		protected
#if DEBUG
		internal
#endif
		virtual Control GetNewUserControl()
		{
			return null;
		}

		protected
#if DEBUG
		internal
#endif
		void DiscardCurrentUserControl()
		{
			if (fUserControl != null)
			{
				var controlToBeRemoved = fUserControl;
				fUserControl = null;
				TabPage.RemovePlugInUserControl(controlToBeRemoved);
				controlToBeRemoved.Dispose();
			}
			fIsSetup = false;
			// This does need to be removed, however caused a failing test in edocs
			// to be removed soon BS 28/06/09
			// Work Item WI00017506
			//fTopLevelTabControl = null;
		}

#if DEBUG
		internal
#endif
		Control fUserControl;

		protected bool IsUserControlVisible => fUserControl != null && fUserControl.Visible;

		protected
#if DEBUG
		internal
#endif
		abstract ZBool HasUserControl { get; }

		/// <summary>
		/// Brings Plug-In's tab page to the top if the plug-in is 
		/// enabled and has a user control.
		/// </summary>
		public void SelectTabPage()
		{
			if (TopLevelTabControl != null && UserControl != null && Enabled)
			{
				if (IsInSubTabControl)
				{
					RecursivelySelectParentTabPage(TopLevelTabControl);
				}

				TopLevelTabControl.SelectedTab = TabPage;
			}
		}

		void RecursivelySelectParentTabPage(ZTabControl tabControl)
		{
			var tabPage = tabControl.Parent as ZTabPage;

			if (tabPage?.ParentTabControl != null)
			{
				RecursivelySelectParentTabPage(tabPage.ParentTabControl);
				tabPage.ParentTabControl.SelectedTab = tabPage;
			}
		}

		internal ODisplayMode DisplayMode
		{
			get { return fDisplayMode; }
			set
			{
				if (fDisplayMode != value)
				{
					var oldIsFormEditable = IsFormEditable;
					fDisplayMode = value;
					if (oldIsFormEditable != IsFormEditable)
					{
						OnIsFormEditableChanged();
					}
				}
			}
		}

		ODisplayMode fDisplayMode;

		protected virtual void OnIsFormEditableChanged()
		{
		}

		ZTabControl fTopLevelTabControl;
#if DEBUG
		protected
#endif
		ZForm fForm;
		MenuItem[] fTopLevelMenus;
		ZTabPagePlugIn fTabPage;
		ZBool fTabPageVisible = true;
		readonly Dictionary<ZTabControl, ZTabPage> parentTabs = new Dictionary<ZTabControl, ZTabPage>();

		internal ZBool HasUserControlInternal
		{
			get { return HasUserControl; }
		}

		internal void InitializePlugin(ZTabControl topLevelTabControl, ZForm form)
		{
			if (topLevelTabControl != null)
			{
				TopLevelTabControl = topLevelTabControl;
				fForm = topLevelTabControl.FindForm() as ZForm;
			}
			else if (form != null)
			{
				fForm = form;
				TopLevelTabControl = form.TopLevelTabControl;
			}
		}

		protected internal ZTabControl TopLevelTabControl
		{
			get { return fTopLevelTabControl; }
			private set
			{
				if (fTopLevelTabControl != value)
				{
					foreach (var tabControl in parentTabs.Keys)
					{
						tabControl.SelectedIndexChanging -= ParentTabControlSelectedIndexChanging;
					}

					parentTabs.Clear();
					parentTabs[value] = TabPage;

					Control control = value;
					while ((control = control.Parent) != null)
					{
						var tabPage = control as ZTabPage;
						if (tabPage == null)
						{
							continue;
						}

						parentTabs[tabPage.ParentTabControl] = tabPage;
					}

					foreach (var tabControl in parentTabs.Keys)
					{
						tabControl.SelectedIndexChanging += ParentTabControlSelectedIndexChanging;
					}

					fTopLevelTabControl = value;
					SetTabPageVisible(Enabled);
				}
			}
		}

		protected internal ZForm Form
		{
			get { return fForm; }
		}

		internal MenuItem[] TopLevelMenusInternal
		{
			get
			{
				if (fTopLevelMenus == null)
				{
					fTopLevelMenus = AllTopLevelMenus;
					if (fTopLevelMenus != null)
					{
						foreach (var topLevelMenu in fTopLevelMenus)
						{
							if (topLevelMenu != null)
							{
								topLevelMenu.Popup += new EventHandler(TopLevelMenu_PopupOrClick); // if there's sub-items
								topLevelMenu.Click += new EventHandler(TopLevelMenu_PopupOrClick); // if there's no sub-items
							}
						}
					}
					else
					{
						fTopLevelMenus = Array.Empty<MenuItem>();
					}
				}
				return fTopLevelMenus;
			}
		}

		public ZTabPagePlugIn TabPage
		{
			get
			{
				if (fTabPage == null && HasUserControl)
				{
					fTabPage = GetTabPage();
					if (Controller != null)
					{
						fTabPage.CaptionResourceString = Controller.PluginTabPageCaption;
					}
				}
				return fTabPage;
			}
		}

		public virtual bool ShouldBeReadOnly { get; }

		protected
#if DEBUG
		internal
#endif
		virtual ZTabPagePlugIn GetTabPage()
		{
			return new ZTabPagePlugIn(this);
		}

		void ParentTabControlSelectedIndexChanging(object sender, EventArgs e)
		{
			SynchroniseIfTabPageVisible();
		}

		protected internal void SynchroniseIfTabPageVisible()
		{
			if (HasUserControl && parentTabs.Any() && parentTabs.All(pair => GetSelectedTab(pair.Key) == pair.Value) && Enabled)
			{
				if (IsLicenceAndSecurityValid)
				{
					if (QueryUserShouldPlugInGUIAndBusinessEntityBeCreated())
					{
						if (LicenceCheckPoint != null)
						{
							LicenceCheckPoint.Login(this);
						}
						Setup();
						SetupUserControl();
						RunWhenVisible(TopLevelTabControl, OnUserControlShown);
						RegisterPlugInAsEditable();

						if (ShowingPlugInNotDisplayedLabel)
						{
							HideCoveringLabel();
							ShowingPlugInNotDisplayedLabel = false;
						}
					}
					else
					{
						ShowCoveringLabel(PlugInNotDisplayedMessage);
						ShowingPlugInNotDisplayedLabel = true;
					}
				}
				else
				{
					var message = "";
					if (!IsSecurityGranted)
					{
						message = SecurityCheckpoint.ErrorMessageForNotAllowed;
					}
					else if (!TryAquireLicence() && !AllowPlugInDisplayWithNoLicence)
					{
						message = LicenceCheckPoint.LastReasonForNotAllowing;
					}

					ShowCoveringLabel(message);
					ShowingPlugInNotDisplayedLabel = true;
				}

				if (!IsSecurityGrantedEdit || ShouldBeReadOnly)
				{
					TabPage.SetReadOnlyIncludingChildren();
				}
			}
		}

		void RunWhenVisible(Control control, MethodInvoker method)
		{
			if (control != null)
			{
				if (control.Visible)
				{
					method();
				}
				else
				{
					var handler = new EventHandlerHolder();
					handler.Handler = delegate
					{
						control.VisibleChanged -= handler.Handler;
						method();
					};
					control.VisibleChanged += handler.Handler;
				}
			}
		}

		class EventHandlerHolder
		{
			public EventHandler Handler;
		}

		TabPage GetSelectedTab(TabControl tabControl)
		{
			TabPage result = null;

			if (tabControl != null && tabControl.TabPages.Count > 0)
			{
				result = tabControl.SelectedTab ?? tabControl.TabPages[0];
			}

			return result;
		}

#if DEBUG
		virtual
#endif
 internal bool DelayBinding
		{
			get { return ShowingPlugInNotDisplayedLabel; }
		}

		bool ShowingPlugInNotDisplayedLabel;

		protected internal bool ShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("ShouldPlugInGUIAndBusinessEntityBeCreated", Name))
			{
				return IsSecurityGranted && ShouldPlugInGUIAndBusinessEntityBeCreatedCore();
			}
		}

		/// <summary>
		/// Get whether the plug in gui and business object should be created.
		/// This is used when the user clicks on the tab or the UserIdleWorker binds tab.
		/// </summary>
		protected virtual bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return true;
		}

		/// <summary>
		/// Query the user whether they want to for example 'create brokerage'.
		/// Returns whether the plug in gui and business object should be created.
		/// This is used when the user clicks on the tab.
		/// </summary>
		protected
#if DEBUG
		internal
#endif
		virtual bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			return ShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		protected
#if DEBUG
		internal
#endif
		virtual bool ShouldPluginDropdownMenuBeCreated()
		{
			return true;
		}

		public virtual ZString PlugInNotDisplayedMessage
		{
			get { return Res.GetString("175223da-64b5-4882-8820-2f651939b11a", "{0} cannot be shown at this time.", Name); }
		}

		bool IsLicenceAndSecurityValid
		{
			get { return IsSecurityGranted && ((LicenceCheckPoint == null || LicenceCheckPoint.Login(null) != LicenceLoginResponse.Denied) || AllowPlugInDisplayWithNoLicence); }
		}

		void SetTabPageVisible(ZBool value)
		{
			if (fTabPageVisible != value && TabPage != null && TopLevelTabControl != null)
			{
				if (value)
				{
					if (TabPageIndex == -1)
					{
						var requestedTabPageIndex = RequestedTabPageIndex;
						if (requestedTabPageIndex < 0 || requestedTabPageIndex >= TopLevelTabControl.TabPages.Count)
						{
							TopLevelTabControl.TabPages.Add(TabPage);
						}
						else
						{
							TopLevelTabControl.TabPages.Insert(TabPage, requestedTabPageIndex);
						}
					}
					else
					{
						var hiddenCount = TopLevelTabControl.AllTabPages.Take(TabPageIndex).Count(page => !TopLevelTabControl.TabPages.Contains(page));
						TopLevelTabControl.TabPages.Insert(TabPage, TabPageIndex - hiddenCount);
					}
				}
				else
				{
					if (TopLevelTabControl.TabPages.Contains(TabPage))
					{
						TabPageIndex = TopLevelTabControl.AllTabPages.ToList().IndexOf(TabPage);
						TopLevelTabControl.TabPages.Remove(TabPage);
					}
				}
				fTabPageVisible = value;
				if (!value)
				{
					TopLevelTabControl.RemoveFromAllTabPages(TabPage);
				}
			}
		}

		void SetMenusVisible(ZBool value)
		{
			foreach (var menu in TopLevelMenusInternal)
			{
				if (menu.Visible != value)
				{
					menu.Visible = value;
				}
			}
		}

		void TopLevelMenu_PopupOrClick(object sender, EventArgs e)
		{
			// Bit of a hack to fix .NET bug that does not commit value when
			// menu item is clicked.
			var mainMenu = ((MenuItem)sender).GetMainMenu();
			if (mainMenu != null)
			{
				var form = mainMenu.GetForm() as ZForm;
				if (form != null)
				{
					ZFormUtilities.EnsureSelectedControlValueCommitted(form);
				}
			}

			if (IsSecurityGranted && TryAquireLicence())
			{
				if (ShouldPluginDropdownMenuBeCreated())
				{
					ConfigureMenuForLicenceAndSecurityGranted();
					Setup();
					OnMenuShown();
					RegisterPlugInAsEditable();
				}
				else
				{
					ConfigureMenuForDropDownMenuNotCreated();
				}
			}
			else
			{
				ConfigureMenuForLicenceOrSecurityDenied();
			}
		}

		MenuItem AccessDeniedMenuItem;
		readonly ArrayList HiddenMenus = new ArrayList();

		internal int RequestedTabPageIndex
		{
			get { return RequestedTabPageIndexDeterminer == null ? -1 : RequestedTabPageIndexDeterminer(); }
		}

		internal GetRequestedTabPageIndexdDelegate RequestedTabPageIndexDeterminer;
		public delegate int GetRequestedTabPageIndexdDelegate();

		internal bool IsInSubTabControl { get; set; }

		int TabPageIndex = -1;

		#endregion

		#region Saving

		/// <summary>
		/// Called when form is being saved, if plug-in is AlwaysLoad PlugIn 
		/// or plug in Menu/UserControl has been shown.
		/// </summary>
		public virtual void OnSaving()
		{
		}

		/// <summary>
		/// Called when the form finished saving or the save was aborted, if plug-in is AlwasyLoad Plugin
		/// or plug in Menu/UserControl has been shown.
		/// </summary>
		public virtual void OnSaveCompletedOrAborted(bool saved)
		{
		}

		public ContinueWithSave ShowPreSaveDialogs()
		{
			var result = ContinueWithSave.Yes;
			if (IsActive || ShowPreSaveDialogsWhenInactive)
			{
				result = ShowPreSaveDialogsCore();
				if (result == ContinueWithSave.Yes && fBusinessEntity != null)
				{
					result = BusinessEntity.CanContinueWithSave ? ContinueWithSave.Yes : ContinueWithSave.No;
				}
			}
			return result;
		}

		/// <summary>
		/// Override this function to show message boxes/dialogs 
		/// before saving. If returned value is ContinueWithSave.No,
		/// the save process will be halted.
		/// </summary>
		public virtual ContinueWithSave ShowPreSaveDialogsCore()
		{
			return ContinueWithSave.Yes;
		}

		protected
#if DEBUG
		internal
#endif
		virtual bool ShowPreSaveDialogsWhenInactive
		{
			get { return false; }
		}

		internal void OnSavingInternal()
		{
			if (IsActive)
			{
				Setup();
				OnSaving();
				RegisterPlugInAsEditable();
			}
		}

		internal void OnSaveCompletedOrAbortedInternal(bool saved)
		{
			try
			{
				if (IsActive)
				{
					if (TopLevelTabControl != null && TopLevelTabControl.SelectedTab == TabPage)
					{
						RefreshData();
					}

					OnSaveCompletedOrAborted(saved);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("{37F83564-E1F7-4013-97AC-5F18ACCEE8EF}", "Exception caught in OnSaveCompletedOrAborted", e);
			}
		}

		/// <summary>
		/// Refresh data on save if plugin is shown
		/// </summary>
		public virtual void RefreshData()
		{
#if DEBUG
			refreshCount++;
#endif
		}
#if DEBUG
		internal int refreshCount;
#endif

		#endregion

		#region Setup

		protected internal void Setup()
		{
			if (!fIsSetup)
			{
				fIsSetup = true;
				RegisterPlugInAsEditable();
			}
		}

		protected internal void SetupUserControl()
		{
			if (!hasSetupUserControl && TabPage != null && UserControl != null && !TabPage.IsDisposed && !UserControl.IsDisposed)
			{
				TabPage.AddPlugInUserControl();
				if (TabPage.IsAutoSized)
				{
					UpdateTabPageMinimumAutoSized();
				}
				hasSetupUserControl = true;
			}
		}

		public virtual void UpdateTabPageMinimumAutoSized()
		{
		}

#if DEBUG
		internal
#endif
		ZBool fIsSetup;

		bool hasSetupUserControl;

		internal void HookFormEvents()
		{
			HookFormEventsCore();
		}

		protected virtual void HookFormEventsCore()
		{
		}

		void UnhookFormEvents()
		{
			UnHookFormEventsCore();
		}

		protected virtual void UnHookFormEventsCore()
		{
		}

		#endregion

		#region Current Dependent

		protected BusinessObject Current
		{
			get
			{
				if (CurrentGrid == null)
				{
					throw new NotSupportedException("Cannot access current. PlugIn has not been set up with Grid. Use PlugIns.AddCurrentDependentPlugIn()");
				}

				BusinessObject result = null;

				if (CurrentGrid.ListManager != null)
				{
					result = (BusinessObject)CurrentGrid.ListManager.GetCurrent();
				}

				return result;
			}
		}

		public bool IsCurrentDependent
		{
			get { return CurrentGrid != null; }
		}

		protected virtual void OnCurrentChanged()
		{
		}

		internal ZGrid CurrentGrid
		{
			get { return fCurrentGrid; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value), "Cannot set grid to null");
				}

				if (fCurrentGrid != null && fCurrentGrid.ListManager != null)
				{
					fCurrentGrid.ListManager.CurrentChanged -= new EventHandler(fCurrentGrid_CurrentChanged);
				}

				fCurrentGrid = value;
				CurrentChangedHookedUp = false;
				HookupCurrentChanged();
			}
		}

		ZGrid fCurrentGrid;
		bool CurrentChangedHookedUp;

		void fCurrentGrid_CurrentChanged(object sender, EventArgs e)
		{
			OnCurrentChanged();
		}

		void HookupCurrentChanged()
		{
			if (!CurrentChangedHookedUp && CurrentGrid != null && CurrentGrid.ListManager != null)
			{
				CurrentGrid.ListManager.CurrentChanged += new EventHandler(fCurrentGrid_CurrentChanged);
				CurrentChangedHookedUp = true;
				OnCurrentChanged();
			}
		}

		#endregion

		#region Business Entity

		/// <summary>
		/// Gets the BusinessObject for the plugged in module.
		/// Null means no BusinessObject. If there's no BusinessObject, 
		/// this plug-in will not be validated on save and the save button will not be
		/// enabled/disabled by this plugin.
		/// </summary>
		public IBusiness BusinessEntity
		{
			get
			{
				if (fBusinessEntity == null)
				{
					fBusinessEntity = GetBusinessEntityForPlugIn();
					if (fBusinessEntity != null && fRegisterEditableChildOnAccessOfBusinessEntity)
					{
						RegisterPlugInAsEditableCore();
					}
				}
				return fBusinessEntity;
			}
		}

		protected internal virtual ITransactionParticipant[] FactoriesToBeSaved
		{
			get
			{
				var result = new List<ITransactionParticipant>();
				if (fBusinessEntity != null && fBusinessEntity.Factory != null)
				{
					result.Add(fBusinessEntity.Factory);
				}
				return result.ToArray();
			}
		}

		protected
#if DEBUG
		internal
#endif
		virtual IBusiness GetBusinessEntityForPlugIn()
		{
			return null;
		}

		protected internal void ResetBusinessEntityToNullInternal() => ResetBusinessEntityToNull();
		protected void ResetBusinessEntityToNull()
		{
			fBusinessEntity = null;
			UnRegisterPlugInAsEditable();
		}

		/// <summary>
		/// Register the BusinessObject returned by the Plug-In as a
		/// RegisteredEditableChildObject of the top level business 
		/// entity of the Form the plug-in is shown on.
		/// </summary>
		protected virtual bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return true; }
		}

		public void RegisterPlugInAsEditable()
		{
			if (BusinessEntity != null)
			{
				RegisterPlugInAsEditableCore();
			}
			else
			{
				fRegisterEditableChildOnAccessOfBusinessEntity = true;
			}
		}

		void RegisterPlugInAsEditableCore()
		{
			if (RegisterPlugInBusinessEntityAsEditable && !IsBusinessObjectRegistered
				&& BusinessEntity != null && BusinessEntity != HostBusinessEntity)
			{
				if (HostBusinessEntity != null)
				{
					if (!(HostBusinessEntity is BusinessObject))
					{
						throw new Exception("Top level object is not a BusinessObject - unable to register PlugIn as editable.");
					}
					fBusinessEntityRegisteredEditable = BusinessEntity;
					((BusinessObject)HostBusinessEntity).RegisterEditableChildObject(fBusinessEntityRegisteredEditable);
					IsBusinessObjectRegistered = true;
				}
			}
		}

		void UnRegisterPlugInAsEditable()
		{
			fRegisterEditableChildOnAccessOfBusinessEntity = false;
			if (IsBusinessObjectRegistered)
			{
				IsBusinessObjectRegistered = false;
				((BusinessObject)HostBusinessEntity).UnRegisterEditableChildObject(fBusinessEntityRegisteredEditable);
				fHasBeenUnregistered = true;
			}
		}

		internal bool IsBusinessObjectRegistered;
		protected readonly IBusiness HostBusinessEntity;

		bool fHasBeenUnregistered;

#if DEBUG
		internal
#endif 
		IBusiness fBusinessEntity;
		bool fRegisterEditableChildOnAccessOfBusinessEntity;
		IBusiness fBusinessEntityRegisteredEditable;

		/// <summary>
		/// Method to be called right after the ZPlugIn buisiness entity was cancelled.
		/// Only for ICancellable business entity.
		/// </summary>
		public virtual void OnBusinessObjectIsCancelledChanged(ZBool isCancelled)
		{
		}

		/// <summary>
		/// Factory for the plug in. This will be saved, along with the 
		/// Form's factory, in a single transaction.
		/// </summary>
		public virtual BusinessObjectFactory Factory
		{
			get { return HostBusinessEntity.Factory; }
		}

		#endregion

		#region Delete

		/// <summary>
		/// If CanDelete, this will be called to allow you to delete any BusinessObjects that
		/// should be deleted.
		/// </summary>
		public virtual void Delete()
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.Delete();
			}
		}

		/// <summary>
		/// Signifies if it is possible for delete of form to occur when this module is plugged in.
		/// </summary>
		public virtual bool CanDelete
		{
			get { return !(IsActive && BusinessEntity != null && !(BusinessEntity is NonPersistentBusinessObject)) || BusinessEntity.IsInDatabaseIncludingChildren; }
		}

		/// <summary>
		/// Message to show the user if the record cannot be deleted.
		/// </summary>
		public virtual string CannotDeleteMessage
		{
			get { return Res.GetString("a4f6d38a-76f0-4aca-b4a5-cfd785f3a8ec", "This entry cannot be deleted as changes have been saved to the {0} system.", Name) + " "; }
		}

		internal void DeleteInternal()
		{
			if (IsActive)
			{
				if (!CanDelete)
				{
					throw new Exception("Cannot delete PlugIn: " + Name + " as CanDelete() returned false");
				}

				Delete();
			}
		}

		#endregion

		#region Covering Label

		/// <summary>
		/// Use this when BusinessObject cannot be created such as Mutex locked
		/// </summary>
		/// <param name="text">Text for a covering label to explain to users why user control is blank</param>
		protected void ShowCoveringLabel(string text)
		{
			CoveringLabel.Text = text;
			CoveringLabel.Visible = true;
			if (fUserControl != null)
			{
				fUserControl.Visible = false;
			}
		}

		protected void HideCoveringLabel()
		{
			CoveringLabel.Visible = false;
			if (fUserControl != null)
			{
				fUserControl.Visible = true;
			}
		}

		ZLabel CoveringLabel
		{
			get
			{
				if (fCoveringLabel == null)
				{
					fCoveringLabel = new ZLabel();
					fCoveringLabel.Dock = DockStyle.Fill;
					fCoveringLabel.Visible = false;
					fCoveringLabel.IsFontBold = true;
					fCoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
					TabPage.Controls.Add(fCoveringLabel);
#if DEBUG
					TypeDescriptor.AddAttributes(fCoveringLabel, new SuppressFormsLocalizedTestAttribute());
#endif
				}
				return fCoveringLabel;
			}
		}
		ZLabel fCoveringLabel;

#if DEBUG
		ZLabel IPlugInInternals.CoveringLabel
		{
			get
			{
				if (!Globals.IsTest)
				{
					throw new NotSupportedException("This is only available for testing purposes.");
				}

				return CoveringLabel;
			}
		}
#endif

		#endregion

		#region Enabled

		/// <summary>
		/// True => Menu and TabPage visible, PlugIn factory is saved, PlugIn is validated on saving.
		/// False => Menu and TabPage hidden, PlugIn factory is not saved, PlugIn is not validated on saving.
		/// Default is True.
		/// </summary>
		public ZBool Enabled
		{
			get { return fEnabled; }
			set
			{
				if (value != fEnabled)
				{
					fEnabled = value;
					SetTabPageVisible(value);
					SetMenusVisible(value);

					if (fEnabled)
					{
						if (fHasBeenUnregistered)
						{
							RegisterPlugInAsEditable();
						}
					}
					else
					{
						UnRegisterPlugInAsEditable();
					}
				}
			}
		}

#if DEBUG
		internal
#endif
		ZBool fEnabled = true;

		protected ZBool IsFormEditable
		{
			get
			{
				return
					DisplayMode == ODisplayMode.Browse ||
					DisplayMode == ODisplayMode.Edit ||
					DisplayMode == ODisplayMode.New ||
					DisplayMode == ODisplayMode.NewSaved;
			}
		}

		protected internal virtual ZBool IsActive
		{
			get { return Enabled && fIsSetup; }
		}

		#endregion

		#region Get Other PlugIns

		/// <summary>
		/// Finds PlugIns on the same form which implement a particular interface.
		/// </summary>
		/// <param name="interfaceType">typeof(InterfaceToSearchFor)</param>
		/// <returns>An array of PlugIns that implement InterfaceType. If no PlugIns found, then returns empty array.</returns>
#if DEBUG
		virtual
#endif
 protected ZPlugIn[] GetOtherPlugInsWhichImplement(Type interfaceType)
		{
			if (ParentCollection == null)
			{
				throw new NotSupportedException("You must be accessing a PlugIn added to a ZForm to use this functionality.");
			}

			return ParentCollection.GetOtherPlugInsWhichImplement(interfaceType, this);
		}

		internal PlugIns ParentCollection;

		#endregion

		#region Licence / Security Management

		bool TryAquireLicence()
		{
			return LicenceCheckPoint == null || LicenceCheckPoint.Login(this) != LicenceLoginResponse.Denied;
		}

		protected internal LicenceCheckpoint LicenceCheckPointInternal => LicenceCheckPoint;
		protected abstract LicenceCheckpoint LicenceCheckPoint { get; }

		protected bool IsSecurityGranted
		{
			get { return SecurityCheckpoint == null || SecurityCheckpoint.IsAllowed; }
		}

		protected bool IsSecurityGrantedEdit
		{
			get { return SecurityCheckpointEdit == null || SecurityCheckpointEdit.IsAllowed; }
		}

		public void SetSecurityCheckpoint(SecurityCheckpoint securityCheckpoint, SecurityCheckpoint securityCheckpointEdit)
		{
			this.SecurityCheckpoint = securityCheckpoint;
			this.SecurityCheckpointEdit = securityCheckpointEdit;
		}

#if DEBUG
		public
#endif
		SecurityCheckpoint SecurityCheckpoint;

#if DEBUG
		internal
#endif
		SecurityCheckpoint SecurityCheckpointEdit;

		protected virtual ZBool AllowPlugInDisplayWithNoLicence
		{
			get { return false; }
		}

		void ConfigureMenuForLicenceOrSecurityDenied()
		{
			HiddenMenus.AddRange(TopLevelMenu.MenuItems);
			TopLevelMenu.MenuItems.Clear();
			if (AccessDeniedMenuItem == null)
			{
				AccessDeniedMenuItem = new ZMenuItem(ResString.GetMultilingualString("PlugIn.AccessDenied", "Access Denied, click this menu for detail."), new EventHandler(ShowLicenceOrSecurityError));
			}
			TopLevelMenu.MenuItems.Add(AccessDeniedMenuItem);
		}

		void ConfigureMenuForDropDownMenuNotCreated()
		{
			HiddenMenus.AddRange(TopLevelMenu.MenuItems);
			TopLevelMenu.MenuItems.Clear();
			if (AccessDeniedMenuItem == null)
			{
				AccessDeniedMenuItem = new ZMenuItem(Res.GetString("0ecf76e9-5649-40ce-8ce6-a7bd899576c6", "Menu could not be created, click here for detail."), new EventHandler((x, y) => Globals.Message.Show(this.PlugInNotDisplayedMessage)));
			}
			TopLevelMenu.MenuItems.Add(AccessDeniedMenuItem);
		}

		void ConfigureMenuForLicenceAndSecurityGranted()
		{
			if (HiddenMenus.Count > 0 && TopLevelMenu != null)
			{
				foreach (MenuItem item in HiddenMenus)
				{
					TopLevelMenu.MenuItems.Add(item);
				}
				HiddenMenus.Clear();
				if (AccessDeniedMenuItem != null)
				{
					TopLevelMenu.MenuItems.Remove(AccessDeniedMenuItem);
				}
			}
		}

		void ShowLicenceOrSecurityError(object sender, EventArgs e)
		{
			if (!IsSecurityGranted)
			{
				SecurityCheckpoint.ShowError();
			}
			else if (!TryAquireLicence() && !AllowPlugInDisplayWithNoLicence)
			{
				LicenceCheckPoint.ShowLastError();
			}
		}

		#endregion

		#region Dispose

		public void Dispose()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("DisposePlugIn", Name))
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				UnhookFormEvents();

				if (fLicensedComponentManager != null)
				{
					fLicensedComponentManager.Dispose();
				}

				if (fTabPage != null && fTabPage.Parent == null)
				{
					fTabPage.Dispose();
				}

				if (fUserControl != null)
				{
					fUserControl.Dispose();
					fUserControl = null;
				}

				if (fCoveringLabel != null)
				{
					fCoveringLabel.Dispose();
				}

				foreach (MenuItem item in HiddenMenus)
				{
					MenuDisposer.DisposeMenu(item);
					item.Dispose();
				}

				if (fTopLevelMenu != null)
				{
					MenuDisposer.DisposeMenu(fTopLevelMenu);
					fTopLevelMenu.Dispose();
				}

				if (AccessDeniedMenuItem != null)
				{
					MenuDisposer.DisposeMenu(AccessDeniedMenuItem);
					AccessDeniedMenuItem.Dispose();
				}

				fTopLevelTabControl = null;
			}
		}

		#endregion

		#region ILicensedComponent Members

		public LicensedComponentManager LicensedComponentManager
		{
			get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
		}

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get
			{
				if (fLicensedComponentManager == null)
				{
					fLicensedComponentManager = new LicensedComponentManager(this);
				}
				return fLicensedComponentManager;
			}
		}

		LicensedComponentManager fLicensedComponentManager;

		#endregion

		#region IPlugInInternals Members

		IBusiness IPlugInInternals.HostBusinessEntity => HostBusinessEntity;

		LicenceCheckpoint IPlugInInternals.LicenceCheckPoint
		{
			get { return LicenceCheckPoint; }
		}

		void IPlugInInternals.InitializePlugin(ZTabControl topLevelTabControl, ZForm form)
		{
			InitializePlugin(topLevelTabControl, form);
		}

		#endregion
	}
}
