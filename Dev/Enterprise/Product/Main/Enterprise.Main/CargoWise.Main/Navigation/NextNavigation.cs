using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Windows.UI;
using Enterprise.Core.Modules;
using Enterprise.EConversation.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#if WINZOR
using System.IO;
using System.Linq;
using Enterprise.MasterFiles.Business;
#endif

namespace CargoWise.Main.Navigation;

public partial class NextNavigation : ModuleNavigation
{
	public NextNavigation()
	{
		InitializeComponent();
		navigationViewModel.RecentMessagesViewModel.OpenJobCommand = new RelayCommand(OpenJobDelegate);

#if !WINZOR
		HomeControl = new()
		{
			Padding = new System.Windows.Thickness(0.0),
			DataContext = navigationViewModel,
			FontFamily = new System.Windows.Media.FontFamily(OFont.NavigationMenuFontName)
		};
		HomeControl.InitializeComponent();
		HostControl.Child = HomeControl;
#else
		var newsViewModeltop = new NewsViewModel { SectionsToLoad = NewsViewModel.TopOrBottom.Top };
		var newsViewModelbottom = new NewsViewModel { SectionsToLoad = NewsViewModel.TopOrBottom.Bottom };
		var myTasksViewModel = new MyTasksViewModel()
		{
			OpenTaskModuleForm = () => OpenModuleInNewWindow(new MainFormModule(ModuleIDs.ProcessTasks), null)
		};
		WinzorDispatcher.Queue(() =>
		{
			newsViewModeltop.LoadNewsItems();
			newsViewModelbottom.LoadNewsItems();
			myTasksViewModel.LoadMyTasks();
		});
		HostControl.Child = new NextHomeUserControl
		{
			FeaturesManager = HomeFeaturesManagerProvider.GetInstance(),
			NavigationViewModel = navigationViewModel,
			SessionContextViewModel = new()
			{
				UserName = GlbStaff.CurrentUser.GS_FullName,
				Branch = GlbBranch.CurrentBranch.GB_BranchName,
				Company = GlbCompany.CurrentCompany.GC_Name,
				Department = GlbDepartment.CurrentDepartment.GE_Desc,
				UserImage = ImageToByteArray(GlbStaff.CurrentUser.ProfileImage)
			},
			NewsViewModelTop = newsViewModeltop,
			NewsViewModelBottom = newsViewModelbottom,
			MyTasksViewModel = myTasksViewModel,
		};
#endif
		Controls.Add(rightBorder);
		RefreshTheme();

		ShowModuleUrlHandler.Instance.OnFormInitialized = OnModuleFormInitialized;
	}

	static void OpenJobDelegate(object jobConversationId)
	{
		if (jobConversationId is Guid id && id != Guid.Empty)
		{
			var conversation = new BusinessObjectFactory().Load<JobConversation>(id);
			if (conversation is null)
			{
				return;
			}

			var job = (conversation.Parent is IGlobalSearchBusinessObjectProvider p) ? p.BusinessObjectForController : conversation.Parent;
			if (job is not null)
			{
				ZControllerFactory.Instance.GetControllerForBizo(job).ShowEditForm(job);
			}
		}
	}

#if WINZOR
	static byte[] ImageToByteArray(Image image)
	{
		if (image == null)
		{
			return null;
		}

		using var memoryStream = new MemoryStream();
		image.Save(memoryStream, image.RawFormat);
		return memoryStream.ToArray();
	}
#endif

	public void RefreshTheme()
	{
		var colorTheme = SystemDataRegistry.Instance.ColorTheme;
		HostControl.BackColor = colorTheme.NavBarBackgroundColor;
#if WINZOR
		(HostControl.Child as NextHomeUserControl)?.SetTheme(SystemDataRegistry.Instance.ColorTheme);
#else
		ThemeColors.Instance.BackgroundColor = colorTheme.MainFormBackgroundColor;
		ThemeColors.Instance.MenuBackgroundColor = colorTheme.TitleBarBackground;
		ThemeColors.Instance.MenuSelectedColor = colorTheme.NavBarGroupSelected1;
		ThemeColors.Instance.MenuTextColor = colorTheme.TitleBarText;
		ThemeColors.Instance.GroupHeaderBackgroundColor = colorTheme.NavBarGroupHeaderBackground;
		ThemeColors.Instance.GroupBackgroundColor = colorTheme.NavBarGroupBackground1;
		ThemeColors.Instance.RecentPanelBackgroundColor = colorTheme.NavBarRecentPanelBackground;
		ThemeColors.Instance.ShortCutIndexTextColor = colorTheme.NavBarTextColor;
#endif
	}

	public override void OpenModule(MainFormModule module, bool isANewModuleClick = true)
	{
		var forms = ZApplication.GetOpenForms();
		if (forms != null && forms.Length > 0)
		{
			foreach (var openedForm in forms)
			{
				if ((openedForm as IMainForm)?.CurrentModule?.ID == module.ID)
				{
					RestoreAndActivateModuleWindow(openedForm);
					UpdateRecentModules(module);
					return;
				}
			}

			OpenModuleInNewWindow(module, null);
		}
	}

	void RestoreAndActivateModuleWindow(Form openedForm)
	{
#if WINZOR
		(openedForm as ZForm)?.RestoreLastWindowStateFromMinimized();
		openedForm.Activate();
#else
		FormWindowPairs.TryGetValue(openedForm, out var window);
		window?.RestoreLastWindowStateFromMinimized();
		window?.Activate();
#endif
	}

#if !WINZOR
	protected Dictionary<Form, ModuleWindow> FormWindowPairs { get; } = new Dictionary<Form, ModuleWindow>();

	protected internal override Form OpenModuleInNewWindowCoreInternal(ZModule zmodule)
	{
		ZForm popup = null;
		popup = zmodule.GetForm() as ZForm;
		if (popup != null)
		{
			popup.Text = zmodule.Description;
		}
		popup ??= zmodule.ShowPopup() as ZForm;
		if (popup != null)
		{
			popup.Disposed += (o, e) => zmodule.Dispose();
		}
		return popup;
	}
#endif

	protected override void OnModuleFormInitialized(Form form)
	{
		if (form is null)
		{
			return;
		}

		var vm = new NavigationViewModel();
		vm.Categories = navigationViewModel.Categories;
		vm.InitializeSearch(navigationViewModel.GlobalSearch);
		for (var i = 0; i < navigationViewModel.PopupMenus?.Count; i++)
		{
			var item = navigationViewModel.PopupMenus[i];
			vm.PopupMenus.Add(new PopupMenu
			{
				Title = item.Title,
				ToolStripDropDown = item.ToolStripDropDown,
				IconGeometry = item.IconGeometry,
				DropDownOffset = new(-18 * (i * 7), -32), // adjust the dropdown position for RDP
				SubMenuItems = item.SubMenuItems,
				PopupMenuEvent = item.PopupMenuEvent
			});
		}
		InitializeAppBar(form, vm);
	}

	#region DISPOSE
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "components")]
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			((IExtendedControl)this).Extensions.Dispose();
#if !WINZOR
			// clean up the WPF controls
			if (HomeControl != null)
			{
				navigationViewModel.Dispose();
				HomeControl.DataContext = null;
			}
#endif
			components?.Dispose();
		}

		ShowModuleUrlHandler.Instance.OnFormInitialized = null;
		base.Dispose(disposing);
	}
	#endregion

	internal ZToolStrip NavBarToolStrip
	{
		get
		{
#if !WINZOR
			return HomeControl.NavBarToolStrip;
#else
			return null;
#endif
		}

		set
		{
#if !WINZOR
			HomeControl.NavBarToolStrip = value;
			HomeControl.NavBarToolStrip.Location = ControlDpiScalingHelper.NewScaledPoint(800, 0);
			HomeControl.NavBarToolStrip.Dock = DockStyle.None;
			HomeControl.NavBarToolStrip.Size = ControlDpiScalingHelper.NewScaledSize(10, 10);

			foreach (ZToolStripMenuItem toolstripMenuItem in HomeControl.NavBarToolStrip.Items)
			{
				Geometry iconGeometry = null;
				Point offset = new();
				switch (toolstripMenuItem.Name)
				{
					case "SettingsToolStripMenuItem":
						iconGeometry = HomeControl.TryFindResource("OptionsGeometry") as Geometry;
						offset = new Point(-16, -32);
						break;
					case "HelpToolStripMenuItem":
						iconGeometry = HomeControl.TryFindResource("HelpGeometry") as Geometry;
						offset = new Point(0, -16);
						break;
					case "TestingToolStripMenuItem":
						iconGeometry = HomeControl.TryFindResource("registerGeometry") as Geometry;
						offset = new(-112, -16);
						break;
				}

				var popupMenu = toolstripMenuItem.ToPopupMenu(AfterCreatePopupMenuHandler);
				popupMenu.IconGeometry = iconGeometry;
				popupMenu.DropDownOffset = offset;

				navigationViewModel.PopupMenus.Add(popupMenu);
			}
#else
			foreach (ZToolStripMenuItem toolstripMenuItem in value.Items)
			{
				navigationViewModel.PopupMenus.Add(toolstripMenuItem.ToPopupMenu(AfterCreatePopupMenuHandler));
			}
#endif
		}
	}

	public void AfterCreatePopupMenuHandler(PopupMenu p)
	{
		var menuItem = p.ToolStripDropDown as ToolStripMenuItem;
		if ((menuItem is IConvertedFromMenuItem converted) && converted.SourceMenuItem is ZTrainingModeMenuItem)
		{
			p.IsChecked = EnvProxy.Instance.Registry.TraningModeEnabled;
			menuItem.Click += (s, e) => p.IsChecked = !p.IsChecked;
		}
	}
#if !WINZOR
	internal NextHomeNavigation HomeControl;
#endif

#if WINZOR
	IEnumerable<Control> GetAllControls(Control parent)
	{
		return parent.Controls.Cast<Control>()
			.Where(control => control.Visible && control.ZIndex == 0)
			.SelectMany(control => GetAllControls(control))
			.Concat(parent.Controls.Cast<Control>()
			.Where(control => control.Visible && control.ZIndex == 0));
	}

	void InitializeAppBar(Form newModuleForm, NavigationViewModel navigationViewModel)
	{
		var appBar = new NextAppBarUserControl(navigationViewModel);
		appBar.SetTheme(SystemDataRegistry.Instance.ColorTheme);
		appBar.Dock = DockStyle.Fill;
#if DEBUG
		appBarForTest = appBar;
#endif
		var elementHost = new ZPanel();
		elementHost.Name = "NextAppBarHost";
		elementHost.Dock = DockStyle.Fill;
		elementHost.Controls.Add(appBar);
		elementHost.BringToFront();

		var offSetPanel = new ZPanel();
		offSetPanel.Size = ControlDpiScalingHelper.NewScaledSize(1084, 50);
		offSetPanel.Dock = DockStyle.Top;
		offSetPanel.Name = "OffsetPanel";

		var controls = GetAllControls(newModuleForm);
		foreach (var control in controls)
		{
			control.ZIndex++;
		}

		newModuleForm.Controls.Add(offSetPanel);
		newModuleForm.Controls.Add(elementHost);

		var stripControl = newModuleForm.FindAll<StripControl>().FirstOrDefault();
		if (stripControl is not null)
		{
			stripControl.RecentItemsPanelLocationOffsetY = 54;
			stripControl.UpdateRecentItemsPanelLocation(null, null);
		}

		KeyEventHandler appKeyDownEvent = (s, e) =>
		{
			appBar.TriggerKeyDown(e);
			ToolStripMenuItem targetMenu = null;
			foreach (var menu in navigationViewModel.PopupMenus)
			{
				targetMenu = FindToolStripMenuItemByShortcutKeys(e.KeyData, menu);
				if (targetMenu != null)
				{
					targetMenu.PerformClick();
					return;
				}
			}
		};
		newModuleForm.KeyDown += appKeyDownEvent;
		newModuleForm.FormClosed += (s, e) => newModuleForm.KeyDown -= appKeyDownEvent;
		newModuleForm.MinimumSize = ControlDpiScalingHelper.NewScaledSize(1366, 730, true);
	}

	ToolStripMenuItem FindToolStripMenuItemByShortcutKeys(Keys keys, PopupMenu menu)
	{
		if (menu.ToolStripDropDown is ToolStripMenuItem tItem && tItem.ShortcutKeys == keys)
		{
			return menu.ToolStripDropDown as ToolStripMenuItem;
		}

		foreach (var subMenu in menu.SubMenuItems)
		{
			var foundItem = FindToolStripMenuItemByShortcutKeys(keys, subMenu);
			if (foundItem != null)
			{
				return foundItem;
			}
		}

		return null;
	}
#else
	void InitializeAppBar(Form newModuleForm, NavigationViewModel navigationViewModel)
	{
		var zForm = newModuleForm as ZForm;
		var moduleWindow = new ModuleWindow();
		zForm.TopLevel = false;
		zForm.FormBorderStyle = FormBorderStyle.None;
		zForm.Dock = DockStyle.Fill;
		zForm.MinimumSize = Size.Empty;
		moduleWindow.Title = zForm.TextIncludingSuffix;
		moduleWindow.winFormsHost.Child = zForm;
		moduleWindow.DataContext = navigationViewModel;
		ElementHost.EnableModelessKeyboardInterop(moduleWindow);

		moduleWindow.Closed += (o, e) =>
		{
			FormWindowPairs.Remove(zForm);
			zForm.Close();
			zForm = null;
			moduleWindow.winFormsHost.Dispose();
			moduleWindow.DataContext = null;
			moduleWindow = null;
#if DEBUG
			newModuleWindowForTest = moduleWindow;
#endif
		};
#if DEBUG
		newModuleWindowForTest = moduleWindow;
#endif
		zForm.FormClosing += (o, e) =>
		{
			if (moduleWindow != null)
			{
				moduleWindow.Close();
			}
		};
		FormWindowPairs.Add(zForm, moduleWindow);
		moduleWindow.Show();
	}
#endif
}
