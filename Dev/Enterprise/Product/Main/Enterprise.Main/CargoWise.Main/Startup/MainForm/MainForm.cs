#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async.AsyncTaskContext.Public;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Main.Navigation;
using CargoWise.Main.Startup.Login;
using CargoWise.Windows.UI;
#if !WINZOR
using Enterprise.BlazorWinFormsInterop;
#endif
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Main.ModuleTreeLoader;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Startup.Login;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Forms;
using Enterprise.ZArchitecture.Modules;
using Res = CargoWise.Main.Res;
using ResString = CargoWise.Main.ResString;
#if WINZOR
using Microsoft.JSInterop;
using WinzorFramework.RemoteClientServices;
using WTG.OpenIDConnect.Login;
#endif
#endregion Usings

namespace Enterprise.Startup
{
	public partial class MainForm : ZMainForm,
		IModuleOpener,
		ILicensedComponent,
		IMainForm,
		IPositionSaveProvider,
		IHotkeyProvider
	{
		[ThreadStatic]
		internal static bool IsNextAppBarEnabled;
		public MainForm()
		{
#if WINZOR
			IsCWNext = true;
#else
			IsCWNext = CWNextFeatureHelper.IsCWNextEnabled();
#endif

			IsNextAppBarEnabled = false;   //We should set it to false after function review pass for workitem https://svc-ediprod.wtg.zone/Services/link/ShowEditForm/WorkItem/c8eec385-3400-4807-ab1b-81d796e721d0. we can open it after all features finished for nextappbar.

			LocationChanged += FixLocation;
			VisibleChanged += UnhookFixLocationOnVisibleChanged;
			InitializeComponent();
#if !WINZOR
			FormClosing += (sender, e) => ObjectFactory.Get<IWinFormsListener>().ExitHybridMode();
#endif
			if (!BrandingFactory.Instance.ProductBrandingName.IsNullOrEmpty())
			{
				Text = BrandingFactory.Instance.ProductBrandingName;
			}
			InitializeBeforeLogin();

			if (!this.IsDesignMode())
			{
				EnterpriseFormLookStrategy.RestorePositionAndSize(this);
			}

			if (HomeButton?.Image != null && Math.Abs(HomeButton.Image.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(16)) > 1)
			{
				HomeButton.Image = new Bitmap(HomeButton.Image, ControlDpiScalingHelper.NewScaledSize(HomeButton.Image.Size));
			}
		}

		internal void FixLocation(object o, EventArgs e)
		{
			if (!CachedScreenInfo.Instance.BoundsInfos.Any())
			{ return; }

			foreach (Rectangle bounds in CachedScreenInfo.Instance.BoundsInfos)
			{
				if (bounds.Contains(this.DesktopBounds))
				{
					return;
				}
			}
			//if we're here then the form is either partially out of bounds or overlapping 2+ monitors, so reposition it.
			var mainBounds = CachedScreenInfo.Instance.BoundsInfos[0];
			this.Location = ControlDpiScalingHelper.NewScaledPoint(mainBounds.Left + mainBounds.Width / 2 - this.Width / 2, mainBounds.Top + mainBounds.Height / 2 - this.Height / 2, false);
		}

		internal void UnhookFixLocationOnVisibleChanged(object o, EventArgs e)
		{
			LocationChanged -= FixLocation;
			VisibleChanged -= UnhookFixLocationOnVisibleChanged;
		}

		public bool SearchBoxContainsFocus => SearchBox.ContainsFocus;

		HotkeyRegister IHotkeyProvider.Hotkeys => Hotkeys;
		string IHotkeyProvider.TypeNameForDisplay => Res.GetString("HotkeyTypeName|Form", "Form");

		#region Login Related

		void InitializeBeforeLogin()
		{
			Icon = BrandingFactory.Instance.ProductIcon;
			SetCaption();
#if !WINZOR
			backgroundAppDomainWorkerNotifier = new BackgroundAppDomainWorkerNotifier();
#endif
		}

		public void ShowLoginUserControl(bool isSSOWith2FA = false)
		{
			SuspendLayout();
			UnloadForLogin();

#if !WINZOR
			if (IsCWNext)
			{
				if (loginUserControl == null)
				{
					var host = new KElementHost();
					host.Child = new NextLoginUserControl();
					host.Name = "LoginUserControl";
					host.Dock = DockStyle.Fill;
					BaseWorkspaceAreaPanel.Controls.Clear();
					BaseWorkspaceAreaPanel.Controls.Add(host);
					loginUserControl = host;
				}

				ResumeLayout();

				return;
			}
#endif

			if (loginUserControl == null)
			{
				loginUserControl = new LoginUserControl(isSSOWith2FA);
				loginUserControl.Name = "LoginUserControl";
				DockLeft(loginUserControl);
			}

			ResumeLayout();
			HandleCreated += LoginUserControlFocus;
		}
		void LoginUserControlFocus(object sender, EventArgs args) => BeginInvoke(new Action(() => loginUserControl.Focus()));

		public void ShowSupportLoginUserControl()
		{
			SuspendLayout();
			UnloadForLogin();

#if !WINZOR
			if (IsCWNext)
			{
				var host = new KElementHost();
				host.Child = new NextLoginUserControl();
				host.Name = "LoginUserControl";
				host.Dock = DockStyle.Fill;
				BaseWorkspaceAreaPanel.Controls.Clear();
				BaseWorkspaceAreaPanel.Controls.Add(host);
				loginUserControl = host;
				ResumeLayout();
				return;
			}
#endif
			loginUserControl = new LoginUserControl(isSSOWith2FA: false, isSupportLogin: true);
			loginUserControl.Name = "LoginUserControl";
			DockLeft(loginUserControl);
			ResumeLayout();
			HandleCreated += LoginUserControlFocus;
		}

		void UnloadLoginUserControl()
		{
			if (loginUserControl != null)
			{
				BaseWorkspaceAreaPanel.Controls.Remove(loginUserControl);
				loginUserControl.Dispose();
				HandleCreated -= LoginUserControlFocus;
				loginUserControl = null;
			}
		}

		Control loginUserControl;

		public void ShowLoginLocationControl()
		{
			SuspendLayout();
			UnloadForLogin();
			UnloadLoginUserControl();

			if (loginLocationControl == null)
			{
				if (IsCWNext)
				{
#if WINZOR
					loginLocationControl = new NextLoginLocationControl()
					{
						NextLoginLocationViewModel = new NextLoginLocationViewModel(new LoginService()),
						Name = "LoginLocationControl",
						Dock = DockStyle.Fill
					};
					BaseWorkspaceAreaPanel.Controls.Clear();
					BaseWorkspaceAreaPanel.Controls.Add(loginLocationControl);
#else
					var host = new KElementHost();
					host.Child = new NextLoginLocationUserControl();
					host.Name = "LoginLocationControl";
					host.Dock = DockStyle.Fill;
					BaseWorkspaceAreaPanel.Controls.Clear();
					BaseWorkspaceAreaPanel.Controls.Add(host);
					loginLocationControl = host;
#endif
				}
				else
				{
					loginLocationControl = new LoginLocationControl();
					loginLocationControl.Name = "LoginLocationControl";
					DockLeft(loginLocationControl);
				}
			}
			ResumeLayout();
		}

		internal void UnloadLoginLocationControl()
		{
			if (loginLocationControl != null)
			{
				BaseWorkspaceAreaPanel.Controls.Remove(loginLocationControl);
				loginLocationControl.Dispose();
				loginLocationControl = null;
			}
		}

		Control loginLocationControl;

		internal void UnloadForLogin()
		{
			UninitialiseGlobalSearch();

			UnloadNavigationBar();

			if (Menu != null)
			{
				Menu = null;
			}

			RemoveStartupControl();
			ToolBarPanel.Hide();
			ShowStartUpScreen(allowAutoLogin: false);
		}

		void UnloadNavigationBar()
		{
			if (NavigationBar != null)
			{
				NavigationBar.CategoryChanged -= NavigationBar_CategoryChanged;
				BaseWorkspaceAreaPanel.Controls.Remove(NavigationBar);
				NavigationBar.Dispose();
				NavigationBar = null;
			}
		}

		void DockLeft(Control control)
		{
			control.Dock = DockStyle.Left;
			control.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);

			ControlDpiScalingHelper.SetWidth(ref control, 275, true);
			BaseWorkspaceAreaPanel.Controls.Add(control);
			try
			{
				control.Focus();
			}
			catch (NullReferenceException)
			{
				// WI00180235: possible WPF bugs caused by null keyboard source.
			}
		}

		public static void InitializeAfterLogin()
		{
			var clientHook = ClientHookLoader.Instance.ClientHook;
			if (clientHook != null && !clientHook.IsInitialised)
			{
				clientHook.Initialise(true);
			}

			if (StartupOpenMainFormTask.MainFormInstance != null)
			{
				StartupOpenMainFormTask.MainFormInstance.DoInitializeAfterLogin();
			}
			else
			{
				RunPostLoginTasks();
			}
		}

		internal GlobalSearch GlobalSearch => globalSearch = globalSearch ?? new GlobalSearch(ModuleOpener);
		internal GlobalSearch globalSearch;

		internal void ModuleOpener(ModuleOpenerInfo openerInfo)
		{
			if (NavigationBar == null)
			{
				throw new InvalidOperationException("Opening a module requires the NavigationBar to be not null");
			}

			var module = NavigationBar.GetModuleInfoForOpener(openerInfo).module;
			OpenModule(module, true);
		}

		internal void EnableGlobalSearchBar(bool visible)
		{
			if (IsCWNext)
			{
				// Searchbox is hidden temperary, as search functionality is moved to new all in one search
				visible = false;
			}

			if (Globals.IsWinzor)
			{
				TitleBar.Enabled = visible;
				TitleBar.Visible = visible;
			}

			SearchBox.Enabled = visible;
			SearchBox.Visible = visible;
			SearchBox.AutoSearch = true;
			SearchBox.Clear();
			SearchBox.Search = visible ? GlobalSearchProxy : null;

			IEnumerable<ZArchitecture.GUI.SearchBox.IDisplayItem> GlobalSearchProxy(string searchVal)
			{
				var results = GlobalSearch.Search(searchVal);
				return results.ToSearchBoxDisplayItems();
			}
		}

		internal void InitialiseGlobalSearch() => EnableGlobalSearchBar(GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.Value);

		internal void UninitialiseGlobalSearch() => EnableGlobalSearchBar(false);

		internal static void LoadModuleTree()
		{
			//TODO: PERFORMANCE: I tried generating Env.Security on a second thread, and I tried generating the module tree on a second thread. Both failed unit tests for unknown reasons.
			//If you can figure out how to make it work correctly, as well as investigate if it can be started even sooner than this point, you could shave on the order of a second (!!)
			//off of CW1 startup times!
			var loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();
		}

		void SetCaption()
		{
			var caption = BrandingFactory.Instance.ProductBrandingName;

			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			var regoStatus = productRegistration.LocalVerify();

			if (regoStatus == ProductRegistrationVerifyResult.Unregistered)
			{
				caption += " " + ResString.GetMultilingualString("4E06B005-A676-4393-97E8-ECE31DE62A10", "(Unregistered)");
			}
			else if (regoStatus == ProductRegistrationVerifyResult.Fail || regoStatus == ProductRegistrationVerifyResult.NotFound)
			{
				caption += " " + ResString.GetMultilingualString("F83688B9-3587-4D1D-9DA4-BB5BB2CC90F7", "(Unlicensed)");
			}
			else if (productRegistration.Key.DatabaseType == DatabaseTypes.Codes.WisecloudTrial)
			{
				caption += " " + ResString.GetMultilingualString("DA573D70-7A06-4A08-92FA-534EB8817ADD", "(WiseCloud Trial)");
			}

			Text = caption;
		}

		internal void DoInitializeAfterLogin()
		{
			Exception threadException = null;

			LoadModuleTreeHandlingKnownErrors();

			RegistryItemDictionary.Instance.PurgeAll();

			var currentThreadRegistryInstance = SystemDataRegistry.Instance;
			var threadToResetColorTheme = new Thread(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						currentThreadRegistryInstance.InitializeColorTheme();
					}
				}
				catch (Exception e)
				{
					threadException = e;
				}
			});
			threadToResetColorTheme.Start();

			var threadToSetUpFactoryForNews = new Thread(() =>
			{
				try
				{
					BusinessObjectFactory newsFactory = null;
					using (Db.DisposableActionForDbConnection())
					{
						newsFactory = new BusinessObjectFactory();
						StartupUserControl.GetNews(newsFactory, null, false);
					}
				}
				catch (Exception e)
				{
					threadException = e;
				}
				finally
				{
					newsFactory?.RelinquishThreadOwnership();
				}
			});
			threadToSetUpFactoryForNews.Start();

			SuspendLayout();

#if DEBUG
			Application.Idle += delegate
			{ ActionCounter.IncreaseActionCount(); };
#endif

			UnloadLoginUserControl();
			UnloadLoginLocationControl();
			EnableGlobalFont();
			RemoveStartupControl();
			InitializeNavigationBar(); //strictly has to be done before AddAdditionalFileMenuItemsForAppropriateUsers and CreateFastUserSwitchingMenu and InitialiseGlobalSearch
			InitialiseGlobalSearch();
			AddAdditionalFileMenuItemsForAppropriateUsers();
#if DEBUG
			CreateFastUserSwitchingMenu();
#endif
			SetCaption();
			KeyPreview = true;
			threadToResetColorTheme.Join(); //strictly has to be done before SetColorTheme
			SetColorTheme(); //strictly has to be done before ResumeLayout
			FinishNavigationBar(); //strictly has to be done before OpenStartUpModule
			threadToSetUpFactoryForNews.Join(); //strictly has to be done before OpenStartUpModule
			OpenStartUpModule();
			SetAutoOpenModule();
			ResumeLayout();
			new RequirementChecker().DoCheck();
			RunPostLoginTasks();
			InitialiseHotKeyMessageFilter();

			if (threadException != null)
			{
				var sqlEx = threadException.Find<System.Data.Common.DbException>();
				var friendlyMessage = (sqlEx == null) ? null : new DbErrorMatch(sqlEx).GetUserFriendlyMessage(Db.Connection);

				if (string.IsNullOrWhiteSpace(friendlyMessage))
				{
					throw threadException;
				}

				Globals.Message.ShowError(friendlyMessage);
			}
		}

		void LoadModuleTreeHandlingKnownErrors()
		{
			try
			{
				LoadModuleTree();
			}
			catch (Exception ex)
			{
				var sqlEx = ex.Find<System.Data.Common.DbException>();
				var friendlyMessage = sqlEx == null ? null : new DbErrorMatch(sqlEx).GetUserFriendlyMessage(Db.Connection);

				if (string.IsNullOrWhiteSpace(friendlyMessage))
				{
					throw;
				}

				Globals.Message.ShowError(friendlyMessage);
			}
		}

		void EnableGlobalFont()
		{
#if !WINZOR
			TextRendererHelper.UseTextRendererFromReg();
#endif
			OFont.UseFontFromReg();
			Font = OFont.GetFont();
			ModuleHeadingLabel.Font = OFont.GetFontBold();
			MainFormToolbarStrip.Font = new Font(OFont.NormalFontName, 9F);
			ToolbarRightPanel.Font = new Font(OFont.NormalFontName, 9F);
		}

		internal void SetColorTheme()
		{
			ToolBarPanel.BackColor = SystemDataRegistry.Instance.ColorTheme.ToolbarColor;
			HomeButton.BackColor = SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1;
			HomeButton.ForeColor = SystemDataRegistry.Instance.ColorTheme.NavBarTextColor;
			HomeButton.FlatAppearance.MouseOverBackColor = SystemDataRegistry.Instance.ColorTheme.NavBarGroupSelected1;

			WindowTheme = new WindowTheme(
				SystemDataRegistry.Instance.ColorTheme.FormBackgroundColor,
				SystemDataRegistry.Instance.ColorTheme.TitleBarBackground,
				DefaultWindowTheme.Close,
				SystemDataRegistry.Instance.ColorTheme.TitleBarText,
				SystemDataRegistry.Instance.ColorTheme.TitleBarBackground);

#if WINZOR
			(NavigationBar.HostControl.Child as NextHomeUserControl)?.SetTheme(SystemDataRegistry.Instance.ColorTheme);
#endif
		}

		void RemoveStartupControl()
		{
			if (startupControl != null)
			{
				startupControl.Dispose();
				startupControl = null;
			}
		}

		static void RunPostLoginTasks()
		{
			foreach (var postLoginTask in PostLoginTasks.GetPostLoginTasks())
			{
				if (postLoginTask.ShouldExecute())
				{
					postLoginTask.Execute();
				}
			}
		}

		#endregion Login Related

		#region Module Management

		internal void OpenStartUpModule()
		{
			if (!openModuleInProgress)
			{
				openModuleInProgress = true;

				var originalCursor = Cursor.Current;
				Cursor.Current = Cursors.WaitCursor;

				try
				{
					ShowStartUpScreen();
				}
				finally
				{
					Cursor.Current = originalCursor;
					openModuleInProgress = false;
				}
			}
		}

		public void SwitchCategory(ModuleCategory category)
		{
			if (NavigationBar != null)
			{
				if (CurrentModule != null)
				{
					ShowStartUpScreen();
				}

				NavigationBar.SwitchCategory(category);
			}
		}

		internal void ShowStartUpScreen(bool allowAutoLogin = true)
		{
			if (ZModulePanel.IsDisposed && !IsDisposed)
			{
				ErrorReporter.ReportOnce("DisposedZModulePanelInShowStartUpScreen", "ZModulePanel is already disposed before ShowStartUpScreen() call while MainForm is not yet disposed.");
				return;
			}

			try
			{
				ZModulePanel.SuspendDrawing();
				ZModulePanel.SuspendLayout();

				DisposePreviousEmbeddedModule();
				ClearModulePanel(false);
				ClearMainFormToolbarStrip();
				ToolBarPanel.Visible = false;
				CreateOrRefreshStartupControl();
				ModuleHeadingLabel.Text = string.Empty;
				currentModule = null;

				if (NavigationBar != null)
				{
					NavigationBar.IsShown = true;
				}

				startupControl.AllowAutoLogin = allowAutoLogin;
				startupControl.Visible = true;
			}
			finally
			{
				if (ZModulePanel.IsDisposed && !IsDisposed)
				{
					ErrorReporter.ReportOnce("DisposedZModulePanelInShowStartUpScreen", "ZModulePanel was disposed during ShowStartUpScreen() call while MainForm is not yet disposed.");
				}
				else
				{
					ZModulePanel.ResumeLayout();
					ZModulePanel.ResumeDrawing();
				}
			}
		}

		BusinessObjectFactory newsFactory;

		void CreateOrRefreshStartupControl()
		{
			if (startupControl == null)
			{
				startupControl = new StartupUserControl(newsFactory);
				newsFactory = null;
				ZModulePanel.Visible = false;
				ZModulePanel.Controls.Add(startupControl);

				startupControl.Dock = DockStyle.Fill;
				startupControl.Visible = true;
				ZModulePanel.Visible = true;
			}
			else
			{
				startupControl.ReloadNews();
			}
		}

		internal StartupUserControl startupControl;

		public void OpenModule(MainFormModule module, bool isANewModuleClick)
		{
			if (!IsDisposed && module != null && !openModuleInProgress && CheckLicenceAndSecurityPermissions(module) && !HasUnReadItemsMandatoryToRead())
			{
				openModuleInProgress = true;

				var originalCursor = Cursor.Current;
				Cursor.Current = Cursors.WaitCursor;

				try
				{
					var blazorHybridSucceeded = false;
					if (!string.IsNullOrEmpty(module.RecordUrl))
					{
#if !WINZOR
						if (GlbStaff.CurrentUser.CheckWinzorEnabledForUser() && module.RecordUrl.StartsWith(UrlHandler.EdiUrlPrefix))
						{
							var queryString = UrlHandler.GetQueryStringTextFromUrl(module.RecordUrl);
							ObjectFactory.Get<IWinFormsListener>().OpenModule(module.ModuleID.Name, queryString, out blazorHybridSucceeded);
						}
#endif

						if (!blazorHybridSucceeded)
						{
							WindowPersister.OpenFormsFromUrls(module.RecordUrl);
						}
					}
					else
					{
#if !WINZOR
						if (GlbStaff.CurrentUser.CheckWinzorEnabledForUser())
						{
							var moduleUrl = ShowModuleUrlHandler.Instance.Create(module.ID);
							var queryStringText = ShowModuleUrlHandler.GetQueryStringTextFromUrl(moduleUrl);
							ObjectFactory.Get<IWinFormsListener>().OpenModule(module.ID, queryStringText, out blazorHybridSucceeded);
						}
#endif

						if (!blazorHybridSucceeded)
						{
							OpenZModule(module);
						}

						if (isANewModuleClick)
						{
							UpdateRecentModules(module);
						}

						if (currentModule != null && module.IsPopup && module.LicenceCheckpoint != currentModule.LicenceCheckpoint)
						{
							module.LicenceCheckpoint.Logout(this);
						}
					}
				}
				finally
				{
					Cursor.Current = originalCursor;
					openModuleInProgress = false;
				}
			}
		}

		MainFormModule currentModule;
		internal bool openModuleInProgress;

		public INamedModule CurrentModule => currentModule;

		public string CurrentModuleLicenceCheckPointName => currentModule?.LicenceCheckpoint?.Name ?? string.Empty;

		void ClearMainFormToolbarStrip()
		{
			if (MainFormToolbarStrip != null)
			{
				for (var i = MainFormToolbarStrip.Items.Count - 1; i >= 0; i--)
				{
					DisposeToolStripItemAndChildren(MainFormToolbarStrip.Items[i]);
				}

				MainFormToolbarStrip.Items.Clear();
				MainFormToolbarStrip.PerformLayout();   // this fixes the MainFormBasher memory leak / RowFactory test failure
			}
		}

		void ClearModulePanel(bool isShowingModule)
		{
			if (startupControl != null)
			{
				startupControl.Visible = false;
				ZModulePanel.Controls.Remove(startupControl);
			}

			for (var i = ZModulePanel.Controls.Count - 1; i >= 0; i--)
			{
				ZModulePanel.Controls[i].Dispose();
			}

			ZModulePanel.Controls.Clear();

			if (startupControl != null)
			{
				ZModulePanel.Controls.Add(startupControl);
			}
		}

		void DisposeToolStripItemAndChildren(ToolStripItem item)
		{
			if (item is ToolStripDropDownItem dropDownItem)
			{
				for (var i = dropDownItem.DropDown.Items.Count - 1; i >= 0; i--)
				{
					DisposeToolStripItemAndChildren(dropDownItem.DropDown.Items[i]);
				}
			}

			item.Dispose();
			item = null;
		}

		internal void OpenZModule(MainFormModule mainFormModule)
		{
			if (ZModulePanel.IsDisposed && !IsDisposed)
			{
				ErrorReporter.ReportOnce("DisposedZModulePanelInOpenZModule", "ZModulePanel is already disposed before OpenZModule() call while MainForm is not yet disposed.");
				return;
			}

			try
			{
				RightWorkspaceAreaPanel.SuspendDrawing();
				ZModulePanel.SuspendDrawing();
				RightWorkspaceAreaPanel.SuspendLayout();
				ZModulePanel.SuspendLayout();

				var module = mainFormModule.CreateZModule();
				if (module != null)
				{
					if (module is ZFilterGridModule filterGridModule)
					{
						filterGridModule.AllowLoadTemplateRecords = true;
					}

					ZCurrentModules.Instance.SetCurrentModule(module);

					var embeddedModule = module as ZEmbeddedModule;
					var popupModule = module as ZPopupModule;
					if (embeddedModule != null)
					{
						if (currentModule != mainFormModule)
						{
							ZModulePanel.Visible = false;
							DisposePreviousEmbeddedModule();
							OpenZEmbeddedModule(embeddedModule);
							currentModule = mainFormModule;
						}

						ToolBarPanel.Visible = true;
						ZModulePanel.Visible = true;
						ZModulePanel.Focus();
					}
					else if (popupModule != null)
					{
						popupModule.Show();
						popupModule.Dispose();
					}
					else if (module is ZSimpleUrlLauncherModule webUrlModule)
					{
						webUrlModule.Show();
						webUrlModule.Dispose();
					}
					else
					{
						throw new ApplicationException("Unknown module type - " + module.GetType().FullName);
					}
				}
			}
			finally
			{
				ZModulePanel.ResumeLayout();
				RightWorkspaceAreaPanel.ResumeLayout();
				RightWorkspaceAreaPanel.ResumeDrawing();
				ZModulePanel.ResumeDrawing();
			}
		}

#if DEBUG

		public
#endif
		void OpenZEmbeddedModule(ZEmbeddedModule embeddedModule)
		{
			if (ZModulePanel.IsDisposed && !IsDisposed)
			{
				ErrorReporter.ReportOnce("DisposedZModulePanelInOpenZEmbeddedModule", "ZModulePanel is already disposed before OpenZEmbeddedModule() call while MainForm is not yet disposed.");
				return;
			}

			previousEmbeddedModule = embeddedModule;

			ClearModulePanel(true);

			ClearMainFormToolbarStrip();

			var actionMenu = embeddedModule.FormActionMenu;
			if (actionMenu != null)
			{
				foreach (var item in actionMenu)
				{
					var result = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(item, embeddedModule);
					MainFormToolbarStrip.Items.Add(result);
				}
			}

			ZModulePanel.Controls.Add(embeddedModule.EmbeddedControl);

			if (embeddedModule.EmbeddedControl is IFilterControl filterControl)
			{
				filterControl.HookFormEvents();
			}

			ModuleHeadingLabel.Text = embeddedModule.ID.ExtendedDescription;

			if (NavigationBar != null)
			{
				NavigationBar.IsShown = false;
			}
		}

		void HomeButton_ButtonClicked(object sender, EventArgs e)
		{
			ShowStartUpScreen();
		}

		public void UpdateToolBarDeleteButton(ZEmbeddedModule embeddedModule)
		{
			var filterGridModule = embeddedModule as ZFilterGridModule;

			var deleteCaptions = new[] {
				KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Delete),
				KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Activate),
				KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Deactivate) };

			foreach (ToolStripItem toolStripItem in MainFormToolbarStrip.Items)
			{
				var currentCaption = KMenuItem.StripAcceleratorKeys(toolStripItem.Text);
				if (deleteCaptions.Contains(currentCaption))
				{
					toolStripItem.Text = filterGridModule.DeleteButtonText;
					toolStripItem.ToolTipText = filterGridModule.DeleteButtonToolTipText;

					var imageActive = filterGridModule.DeleteButtonImageActive;
					if (imageActive != IconTypes.None)
					{
						// We need to set Image instead of ImageIndex because setting ImageIndex will show empty icon.
						//	the icon only shown on hovering; while setting Image will show the icon
						toolStripItem.Image = Icons.GetImage(imageActive);

						var zToolbarButton = deleteCaptions.Select(c => (ZToolBarButton)embeddedModule.ToolBarButtons.FindByText(c)).FirstOrDefault();
						if (zToolbarButton != null)
						{
							zToolbarButton.ImageIndex = Icons.GetImageIndex(filterGridModule.DeleteButtonImage);

							// We need to set ActiveIconType instead of Image
							//	because the UI code will call MenuItemToToolStripItemConverter.SetNewItemImageAndVisibility that set ToolStripItem.Image to ActiveIconType
							zToolbarButton.ActiveIconType = imageActive;
						}
					}
					break;
				}
			}
		}

#if DEBUG

		public ToolStripItem FindMainFormToolbarStripItemByText(string caption)
		{
			foreach (ToolStripItem toolStripItem in MainFormToolbarStrip.Items)
			{
				if (KMenuItem.StripAcceleratorKeys(toolStripItem.Text) == KMenuItem.StripAcceleratorKeys(caption))
				{
					return toolStripItem;
				}
			}
			return null;
		}

#endif

		internal IDisposable previousEmbeddedModule;

		#endregion Module Management

		#region Favorites/Recent Modules

		static ModuleCategory JumpCategory => ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];

		static ModuleSection RecentItemsSection => JumpCategory?.Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name];

		static ModuleSection RecentModulesSection => JumpCategory?.Sections[ModuleTreeLoaderConstant.Section.RecentModules.Name];

		/// <summary>
		/// Add or remove the module from the favorites list.
		/// </summary>
		/// <param name="module">module to be added or removed from favorites list</param>
		/// <returns>true if added, else removed</returns>
		public bool AddOrRemoveFromFavorites(MainFormModule module)
		{
			var addedToFavorite = false;
			var shortcut = new LinkWrapper(module.ModuleID.Name, module.RecordKey, module.RecordUrl, module.RecordDescription);
			if (RecentItemManager.Instance.IsInFavoriteModules(shortcut))
			{
				FavoriteProvider.DeleteFromFavorites(shortcut);

				if (shortcut.IsModule)
				{
					UpdateRecentModules(module);
				}
				else
				{
					FavoriteProvider.AddToRecentItems(shortcut);
				}
			}
			else
			{
				if (FavoriteProvider.AddToFavorites(shortcut))
				{
					RemoveFromRecentModule(module, shortcut);
					addedToFavorite = true;
				}
				else
				{
					addedToFavorite = false;
				}
			}

			return addedToFavorite;
		}

		void RemoveFromRecentModule(MainFormModule module, LinkWrapper shortcut)
		{
			if (string.IsNullOrEmpty(module.RecordUrl))
			{
				NavigationBar.RemoveModule(JumpCategory, RecentModulesSection, module);
				RecentItemManager.Instance.RemoveFromRecentModules(shortcut);
			}
		}

		void RemoveFromRecent(MainFormModule module, LinkWrapper shortcut)
		{
			RemoveFromRecentModule(module, shortcut);

			if (!string.IsNullOrEmpty(module.RecordUrl))
			{
				NavigationBar.RemoveModule(JumpCategory, RecentItemsSection, module);
				RecentItemManager.Instance.RemoveFromRecentItems(string.Empty, shortcut);
			}
		}

		public void RemoveSingleLink(MainFormModule module)
		{
			var shortcut = new LinkWrapper(module.ModuleID.Name, module.RecordKey, module.RecordUrl, module.RecordDescription);
			if (RecentItemManager.Instance.IsInFavoriteModules(shortcut))
			{
				FavoriteProvider.DeleteFromFavorites(shortcut);
			}
			else
			{
				RemoveFromRecent(module, shortcut);
			}
		}

		public void UpdateRecentModules(MainFormModule module)
		{
			var shortcut = new LinkWrapper(module.ModuleID.Name, module.RecordKey, module.RecordUrl, module.RecordDescription);

			if (!RecentItemManager.Instance.IsInFavoriteModules(shortcut))
			{
				NavigationBar.AddOrUpdateModule(JumpCategory, RecentModulesSection, module);
				RecentItemManager.Instance.AddOrUpdateRecentModules(shortcut);
			}
		}

		internal IFavoriteProvider FavoriteProvider => ObjectFactory.Get<IFavoriteProvider>();

		#endregion Favorites/Recent Modules

		#region Form Load/Close

#if !WINZOR

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			OnActivated_ForNotifyIcon(e);
		}

#endif

		internal void DisposePreviousEmbeddedModule()
		{
#if DEBUG
			if (Globals.IsTest && DisposePreviousEmbeddedModuleDoingEvents != null)
			{
				DisposePreviousEmbeddedModuleDoingEvents(this, EventArgs.Empty);
			}
#endif

			if (previousEmbeddedModule != null)
			{
				ClearMainFormToolbarStrip();
				previousEmbeddedModule.Dispose();
				previousEmbeddedModule = null;
			}
		}

#if DEBUG
		internal EventHandler DisposePreviousEmbeddedModuleDoingEvents;
#endif
		bool isClosing;
		protected override void OnClosing(CancelEventArgs e)
		{
			if (!isClosing)
			{
				isClosing = true;
				try
				{
					e.Cancel =
#if !WINZOR
						BackgroundAppDomainWorkerNotifier.NotifyWorkItemsInProgress() ||
#endif
						!ConfirmExit()
						|| !OpenedFormCache.GetInstance().CloseAllCachedForms();
					if (!e.Cancel)
					{
						ZFormActivityLogger.Instance.SavePendingLogs(true);
					}

					base.OnClosing(e);
					EnterpriseFormLookStrategy.SavePositionAndSize(this);
				}
				finally
				{
					isClosing = false;
				}
			}
			else
			{
				e.Cancel = true;
			}
		}

		bool ConfirmExit()
		{
			if (NavigationBar != null && !NavigationBar.IsShown)
			{
				var result = Globals.Message.ShowOrDefault(ExitConfirmationControl.DialogDefaultContext, () => new ExitConfirmationControl());
				if (result != DialogResult.Yes)
				{
					if (result == DialogResult.No && !NavigationBar.IsShown)
					{
						ShowStartUpScreen();
					}
					return false;
				}
			}

			return true;
		}

#if !WINZOR
		BackgroundAppDomainWorkerNotifier backgroundAppDomainWorkerNotifier;
#endif

		#endregion Form Load/Close

		public HashSet<string> FormNameInWhiteListWhenCheck
		{
			get
			{
				return new HashSet<string>()
				{
					(NoResString)"Email Diagnostic Testing",
					(NoResString)"New eHub Diagnostics Form",
					(NoResString)"Scanning Diagnostics",
					(NoResString)"Performance Statistics",
					(NoResString)"New Profile Performance",
					(NoResString)"New Profile Memory",
					(NoResString)"Document Sections",
				};
			}
		}

		public HashSet<string> FormTypeInWhiteListWhenCheck
		{
			get
			{
				return new HashSet<string>()
				{
					"Enterprise.ZArchitecture.GUI.Balloons.BalloonWindow",
					"Enterprise.ZArchitecture.GUI.ZPreemptibleMessageBox"
				};
			}
		}

		string GetOpenedFormsName()
		{
			var result = new StringBuilder();
			foreach (var form in ZApplication.GetOpenForms())
			{
				if (this != form &&
					form.Visible &&
					!FormTypeInWhiteListWhenCheck.Contains(form.GetType().FullName) &&
					!FormNameInWhiteListWhenCheck.Contains(form.Text))
				{
					if (form.Text.IsNullOrEmpty())
					{
						ErrorReporter.ReportDeveloperExceptionOnce(
								$"{form.GetType().FullName} without Text still opens when switching login",
								"There is an unknown form still open when users switch login. Please confirm whether this form should be ignored or instantiated with a valid Text.",
									new NotImplementedException($"A form of type '{form.GetType().FullName}' without Text still open when users switch their login.")
							);
					}
					else
					{
						result.AppendLine(form.Text);
					}
				}
			}
			return result.ToString();
		}

		#region Re-Launch App Server

#if WINZOR
		internal void ReLaunchWinzorAppServer(OIDCLoginRequestMessage.LoginPrompt loginPrompt)
		{
			CargoWiseClientInvoker.Invoke((cws) =>
			{
				return cws.LifecycleService.StartNewApplicationAsync(new Uri($"{CargoWiseClientServices.ServerBaseUri}?loginPrompt={loginPrompt}"));
			});
			ExitMenuItem_Click(null, null);
		}
#endif

		#endregion Re-Launch App Server

		#region Auto-Open Module

		class AutoOpenModuleEventArgs : EventArgs
		{
			public AutoOpenModuleEventArgs(ModuleIdentifier moduleIDToOpen)
			{
				ModuleID = moduleIDToOpen;
			}

			public readonly ModuleIdentifier ModuleID;
		}

		delegate void AutoOpenModuleEventHandler(object sender, AutoOpenModuleEventArgs e);

		void AutoOpenModuleForm(object sender, AutoOpenModuleEventArgs e)
		{
			if (ModuleTree.Tree.FindByID(e.ModuleID.ToString()) is MainFormModule module)
			{
				OpenModule(module, true);
			}
		}

		void SetAutoOpenModule()
		{
			ModuleIdentifier moduleIDToOpen = null;

			if (((WinFormsEnvironment)Env.Instance).OpenDocumentScanningForm)
			{
				((WinFormsEnvironment)Env.Instance).OpenDocumentScanningForm = false;
				moduleIDToOpen = ModuleIDs.DocumentAllocation;
			}

			if (moduleIDToOpen != null)
			{
				BeginInvoke(new AutoOpenModuleEventHandler(AutoOpenModuleForm), this, new AutoOpenModuleEventArgs(moduleIDToOpen));
			}
		}

#if !WINZOR

		const int WM_SYSCOMMAND = 0x0112;
		const int SC_KEYMENU = 0xF100;
		const int DocManagerCopyDataFunction = 100;

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WindowsMessage.WM_COPYDATA:
					if (IsRequiredFunction(ref m, DocManagerCopyDataFunction, out var messageData))
					{
						ProcessDocManagerMessage(messageData);
						return;
					}
					break;

				case WindowsMessage.WM_DISPLAYCHANGE:     // https://learn.microsoft.com/en-us/windows/win32/gdi/wm-displaychange
				case WindowsMessage.WM_WINDOWPOSCHANGING: // https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-windowposchanging
					CachedScreenInfo.Instance.PopulateInfo();
					break;
				case WindowsMessage.WM_SETTINGCHANGE:     // https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-settingchange
					const nint SPI_SETWORKAREA = 0x002F;  // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa
					if (m.WParam == SPI_SETWORKAREA)
					{
						CachedScreenInfo.Instance.PopulateInfo();
					}
					break;
			}

			menuFeedback.WndProc(this, ref m);
			base.WndProc(ref m);
			((IPumpedTaskContext)TaskContext.Current).PumpTasks();
		}

		readonly MenuFeedback menuFeedback = new MenuFeedback();

		[StructLayout(LayoutKind.Sequential)]
		struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public int cbData;

			[MarshalAs(UnmanagedType.LPStr)]
			public string lpData;
		}

		bool IsRequiredFunction(ref Message m, int functionNumber, out string messageData)
		{
			COPYDATASTRUCT foundData = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
#pragma warning disable IDE0004 // cast required to compare IntPtr with int
			if (foundData.dwData == (IntPtr)functionNumber)
			{
				messageData = foundData.lpData;
				return true;
			}
#pragma warning restore IDE0004
			messageData = "";
			return false;
		}

		void ProcessDocManagerMessage(string messageString)
		{
			((WinFormsEnvironment)Env.Instance).OpenDocumentScanningForm = true;
			SetAutoOpenModule();
		}

#endif

		#endregion Auto-Open Module

		#region Show In System Tray

#if !WINZOR

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (((WinFormsEnvironment)Env.Instance).ShowInSystemTray)
			{
				InitialiseNotifyIcon();
			}
		}

		void InitialiseNotifyIcon()
		{
			notifyIcon = new ZNotifyIconEx();
			notifyIcon.Click += new EventHandler(NotifyIcon_Click);
			notifyIcon.Text = Text;
			notifyIcon.Icon = BrandingFactory.Instance.ProductIcon;
			notifyIcon.Visible = true;
		}

		internal ZNotifyIconEx notifyIcon;

		void OnActivated_ForNotifyIcon(EventArgs e)
		{
			base.OnActivated(e);
			if (notifyIcon != null && WindowState != FormWindowState.Minimized)
			{
				ShowInTaskbar = true;
			}
		}

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			if (notifyIcon != null)
			{
				if (WindowState == FormWindowState.Minimized)
				{
					ShowInTaskbar = false;
					if (!NotifyIconBalloonShown)
					{
						NotifyIconBalloonShown = true;
						ShowNotifyIconBalloon(
							Res.GetString("5992e445-f590-4adb-9e40-127dc6ff494a", "{0} is still running!", BrandingFactory.Instance.ProductName),
							Res.GetString("7b6ff424-785e-4c2f-86fa-b1011bd463ac", "{0} has been configured to minimize into the system tray.\r\n\r\nBe aware that this feature has various known graphical issues, which includes the title disappearing and overlapping graphical items.", BrandingFactory.Instance.ProductName),
							ZNotifyIconEx.NotifyInfoFlags.Info, 5000);
					}
				}
			}
		}

		bool NotifyIconBalloonShown;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
#if DEBUG

		protected virtual
#endif
		void ShowNotifyIconBalloon(string title, string text, ZNotifyIconEx.NotifyInfoFlags type, int timeoutInMilliSeconds)
		{
			if (notifyIcon != null)
			{
				notifyIcon.ShowBalloon(title, text, type, timeoutInMilliSeconds);
			}
		}

		void NotifyIcon_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Normal;
			Activate();
		}

#endif

		public new bool ShowInTaskbar
		{
			get { return base.ShowInTaskbar; }
			set
			{
				if (!IsDisposed)
				{
					base.ShowInTaskbar = value;
					if (IsHandleCreated)
					{
						EnsureExistsInApplicationOpenForms();
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Not enumerating Application.OpenForms - Adding to it")]
		void EnsureExistsInApplicationOpenForms()
		{
			var openForms = ZApplication.GetOpenForms();
			if (!openForms.Contains(this))
			{
				Application.OpenForms.GetType().InvokeMember("Add", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Application.OpenForms, new object[] { this });
			}
		}

		#endregion Show In System Tray

		#region Hot Key Message Filter

		internal void InitialiseHotKeyMessageFilter()
		{
			ClearHotKeys();
			HotKeyManager.RegisterGlobalHotKeys(GlobalHotkeys);
#if !WINZOR
			HotKeyManager.RegisterMainFormHotkeys(Hotkeys);
#else
			if (!IsCWNext)
			{
				HotKeyManager.RegisterMainFormHotkeys(Hotkeys);
			}
#endif
		}

		void ClearHotKeys()
		{
			GlobalHotkeys.Clear();
			Hotkeys.Clear();
		}

		MainFormHotKeyManager HotKeyManager => hotKeyManager ?? (hotKeyManager = new MainFormHotKeyManager(this));

		MainFormHotKeyManager hotKeyManager;

		#endregion Hot Key Message Filter

		#region Navigation Bar
		void InitializeNavigationBar()
		{
			UnloadNavigationBar();

			if (IsCWNext)
			{
				NavigationBar = new NextNavigation();
				NavigationBar.SetSearchDelegate(this.GlobalSearch.Search);
				NavigationBar.SetOpenTaskModuleFormAction();
				NavigationBar.SetOpenHolidayModuleFormAction();
				NavigationBar.Name = "NextNavigation";
				this.BaseWorkspaceAreaPanel.Controls.Remove(this.RightWorkspaceAreaPanel);
				NavigationBar.Dock = DockStyle.Fill;
				NavigationBar.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				ControlDpiScalingHelper.SetWidth(ref NavigationBar, 275, true);
				BorderSize = ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
				base.SetBorderColour(Color.FromArgb(0, 92, 91, 87));
			}
			else
			{
				NavigationBar = new TileNavigationBar();
				NavigationBar.Name = "NavigationBar";
				DockLeft(NavigationBar);
			}
			NavigationBar.ModuleOpener = this;
			NavigationBar.TabIndex = 0;
			InitializeNavBarButtons();

			if (IsCWNext)
			{
				((NextNavigation)NavigationBar).NavBarToolStrip = navBarToolStrip;
				navBarToolStrip.Visible = false;
				navBarToolStrip.Dock = DockStyle.Top;
				ResizeBorders();
				ResizeMainWorkPanel();
			}
		}

		void FinishNavigationBar()
		{
			NavigationBar.LoadModuleTree(ModuleTree.Tree);
			if (IsCWNext)
			{
				BaseWorkspaceAreaPanel.Controls.Add(NavigationBar); //strictly has to be done after NavigationBar.LoadModuleTree
			}
			else
			{
				NavigationBar.SelectedCategory = JumpCategory;
			}
		}

		public bool OpenFavorite(int favoriteIndex, bool inNewWindow)
		{
			if (IsCWNext)
			{
				inNewWindow = true;
			}
			var favorite = RecentItemManager.Instance.FavoriteModules.ElementAtOrDefault(favoriteIndex);

			if (favorite != null)
			{
				this.BeginInvokeSafe(() =>
				{
					var module = new LinkMainFormModule(favorite);
					if (inNewWindow && string.IsNullOrEmpty(module.RecordUrl) && NavigationBar != null)
					{
						if (CheckLicenceAndSecurityPermissions(module))
						{
							var form = NavigationBar.OpenModuleInNewWindowCore(module, null);
#if DEBUG
							LastModuleInNewWindow = form;
#endif
						}
					}
					else
					{
						OpenModule(module, true);
					}
				});

				return true;
			}

			return false;
		}

#if DEBUG
		internal Form LastModuleInNewWindow { get; private set; }
#endif

		#endregion Navigation Bar

		#region Activity Logger

		readonly IDisposable activityLogger = ZFormActivityLogger.Instance.ExternalActivityLogger.GetInstance();
		readonly bool activityLoggerIsDisposable = (instanceCounter++) == 0;
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static int instanceCounter;

		#endregion Activity Logger

		#region Licence/Security

#if DEBUG

		virtual
#endif
		internal bool HasLicenceAndSecurityPermissions(MainFormModule module)
		{
			var result = true;

			var securityCheckpoint = module.SecurityCheckpoint == null ? null : Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)module.SecurityCheckpoint);

			if (securityCheckpoint == null || securityCheckpoint.IsAllowed)
			{
				var loginResponse = module.LicenceCheckpoint.Login(this);
				if (loginResponse != LicenceLoginResponse.Granted)
				{
					module.LicenceCheckpoint.ShowLastError();
					result = false;
				}
			}
			else if (securityCheckpoint != null)
			{
				securityCheckpoint.ShowError();
				result = false;
			}

			return result;
		}

		internal bool CheckLicenceAndSecurityPermissions(MainFormModule module)
		{
			if (module == null)
			{
				return false;
			}

			var hasPermissions = HasLicenceAndSecurityPermissions(module);

			if (hasPermissions && currentModule != null && !module.IsPopup && currentModule.LicenceCheckpoint != module.LicenceCheckpoint)
			{
				currentModule.LicenceCheckpoint.Logout(this);
			}

			return hasPermissions;
		}

		Dictionary<string, string[]> GetMandatorySections()
		{
			var mandatoryItems = new Dictionary<string, string[]>();

			if (startupControl != null)
			{
				var unReadItems = startupControl.GetUnReadItems();
				var mandatorySections = startupControl.NewsSections.Where(section => section.MandatoryToRead).ToList();

				foreach (var section in mandatorySections)
				{
					foreach (var item in unReadItems)
					{
						if (item.Value[0] == section.SectionID)
						{
							mandatoryItems.Add(item.Key, item.Value);
						}
					}
				}
			}

			return mandatoryItems;
		}

#if DEBUG

		virtual
#endif
		internal bool HasUnReadItemsMandatoryToRead()
		{
			Dictionary<string, string[]> mandatorySectionDic = GetMandatorySections();
			if (!Env.Security.IgnoreMandatoryToRead.IsAllowed && mandatorySectionDic.Any())
			{
				List<string> mandatorySectionIdList = new List<string>();
				StringBuilder sectionNameInfo = new StringBuilder();
				mandatorySectionDic.Keys.ForEach((string key) =>
				{
					string sectionId = mandatorySectionDic[key][0];
					string sectionName = mandatorySectionDic[key][1];
					if (!mandatorySectionIdList.Contains(sectionId))
					{
						mandatorySectionIdList.Add(sectionId);
						sectionNameInfo.AppendLine(sectionName);
					}
				});

				string caption = ResString.GetMultilingualString("9F595921-9AC1-4656-8175-35282BAC86B0", "Access Denied");

				StringBuilder message = new StringBuilder();
				message.AppendLine(ResString.GetMultilingualString("98BD948E-1AF0-4721-84D1-A7F6023A00EE", "There are unread message(s) mandatory to read in following sections:")).AppendLine();
				message.AppendLine(sectionNameInfo.ToString());
				message.AppendLine(ResString.GetMultilingualString("D5245C74-A195-4846-9A80-95BD94E4ACB6", "If you require access to this function without reading all mandatory messages, ask your system administrator to change either your Staff or Group Security Rights to allow access to:")).AppendLine();
				message.AppendLine(Env.Security.IgnoreMandatoryToRead.DisplayTextPathToSecurityRight);
				Globals.Message.Show(message.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, DialogResult.OK);
				return true;
			}

			return false;
		}

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get { return licensedComponentManager ?? (licensedComponentManager = new LicensedComponentManager(this)); }
		}

		LicensedComponentManager licensedComponentManager;

		#endregion Licence/Security

		#region Cargowise Next
		public bool IsCWNext { get; protected set; }
		#endregion

		protected override void SetVisibleCore(bool value)
		{
			try
			{
				base.SetVisibleCore(value);
			}
			catch (ObjectDisposedException objectDisposedEx) when (!objectDisposedEx.Message.Contains((NoResString)"Object name: 'MainForm'.", StringComparison.Ordinal))
			{
				//The form is gone so the programe is gone but SetVisible message still on the message queue, and we don't exit the program before that message gets processed
			}
		}

		#region Dispose

		public string DisposalStackTrace { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposePreviousEmbeddedModule();

				if (activityLoggerIsDisposable)
				{
					activityLogger.Dispose();
				}

				PerformanceStatisticsCollector.AttemptFlush(true);

				if (components != null)
				{
					components.Dispose();
				}

#if !WINZOR

				if (notifyIcon != null)
				{
					notifyIcon.Click -= new EventHandler(NotifyIcon_Click);
					notifyIcon.Dispose();
				}

#endif

				if (startupControl != null)
				{
					startupControl.Dispose();
				}

				((ILicensedComponent)this).LicensedComponentManager.Dispose();

#if !WINZOR
				if (backgroundAppDomainWorkerNotifier != null)
				{
					((IDisposable)backgroundAppDomainWorkerNotifier).Dispose();
				}
#endif

				DisposalStackTrace = System.Environment.StackTrace;
				hotKeyManager?.Dispose();
			}

			if (restoreDbLoginsForm != null)
			{
				restoreDbLoginsForm.Dispose();
				restoreDbLoginsForm = null;
			}

			base.Dispose(disposing);
		}

		#endregion Dispose

		#region IPositionSaveProvider Members

		Rectangle IPositionSaveProvider.FormPositionRectangle { get; set; }

		bool IPositionSaveProvider.RememberFormPosition
		{
			get { return rememberFormPosition; }
			set { rememberFormPosition = value; }
		}

		bool rememberFormPosition = true;

		bool IPositionSaveProvider.RememberFormSize
		{
			get { return rememberFormSize; }
			set { rememberFormSize = value; }
		}

		bool rememberFormSize = true;

		#endregion IPositionSaveProvider Members

	}
}
