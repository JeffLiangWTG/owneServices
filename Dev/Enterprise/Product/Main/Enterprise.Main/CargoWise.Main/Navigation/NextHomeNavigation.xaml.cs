#if !WINZOR
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Main.Navigation.WPF;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Image = System.Drawing.Image;

namespace CargoWise.Main.Navigation;

/// <summary>
/// Interaction logic for NextHomeNavigation.xaml
/// </summary>
public partial class NextHomeNavigation : UserControl
{
	public NextHomeNavigation()
	{
		InitializeComponent();
		GetLoginDetails();
		SetFeatures();

		Loaded += OnLoadedHandler;
		SizeChanged += UserControl_SizeChanged;
	}

	#region Features
	public bool IsSnapshotsEnabled
	{
		get { return (bool)GetValue(IsSnapshotsEnabledProperty); }
		set { SetValue(IsSnapshotsEnabledProperty, value); }
	}

	public static readonly DependencyProperty IsSnapshotsEnabledProperty =
		DependencyProperty.Register("IsSnapshotsEnabled", typeof(bool), typeof(NextHomeNavigation), new PropertyMetadata(false));

	public bool IsNextToolMenuEnabled
	{
		get { return (bool)GetValue(IsNextToolMenuEnabledProperty); }
		set { SetValue(IsNextToolMenuEnabledProperty, value); }
	}

	public static readonly DependencyProperty IsNextToolMenuEnabledProperty =
		DependencyProperty.Register("IsNextToolMenuEnabled", typeof(bool), typeof(NextHomeNavigation), new PropertyMetadata(false));

	void SetFeatures()
	{
		IsSnapshotsEnabled = HomeFeaturesManagerProvider.GetInstance()?.IsEnabled(HomeFeature.Snapshots) ?? true;
		IsNextToolMenuEnabled = HomeFeaturesManagerProvider.GetInstance()?.IsEnabled(HomeFeature.NextToolMenu) ?? false;
	}

	#endregion

	public bool ShowNewsPanel
	{
		get { return (bool)GetValue(ShowNewsPanelProperty); }
		set { SetValue(ShowNewsPanelProperty, value); }
	}

	// Using a DependencyProperty as the backing store for ShowNewsPanel.  This enables animation, styling, binding, etc...
	public static readonly DependencyProperty ShowNewsPanelProperty =
		DependencyProperty.Register("ShowNewsPanel", typeof(bool), typeof(NextHomeNavigation), new PropertyMetadata(true, new PropertyChangedCallback(OnShowPanelChanged)));

	public MultilingualString ToggleNewsButtonToolTip
	{
		get { return (MultilingualString)GetValue(ToggleNewsButtonToolTipProperty); }
		set { SetValue(ToggleNewsButtonToolTipProperty, value); }
	}

	// Using a DependencyProperty as the backing store for ToggleNewsButtonToolTip.  This enables animation, styling, binding, etc...
	public static readonly DependencyProperty ToggleNewsButtonToolTipProperty =
		DependencyProperty.Register("ToggleNewsButtonToolTip", typeof(MultilingualString), typeof(NextHomeNavigation), new PropertyMetadata(null));

	static void OnShowPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is NextHomeNavigation nextHomeNavigation)
		{
			var showNewsPanel = (bool)e.NewValue;
			if (showNewsPanel)
			{
				nextHomeNavigation.NewsPanelColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
				nextHomeNavigation.NewsPanelColumnDefinition.MinWidth = 300;
				nextHomeNavigation.ToggleNewsButtonToolTip = ResString.GetMultilingualString("899b0ca8-06fc-4027-8cfc-ef05f3e7948c", "Hide News");
			}
			else
			{
				nextHomeNavigation.NewsPanelColumnDefinition.Width = new GridLength(0, GridUnitType.Pixel);
				nextHomeNavigation.NewsPanelColumnDefinition.MinWidth = 0;
				nextHomeNavigation.ToggleNewsButtonToolTip = ResString.GetMultilingualString("119e744b-0752-473f-a4f3-69aca732a591", "Show News");
			}
		}
	}

	void OnLoadedHandler(object sender, RoutedEventArgs e)
	{
		Loaded -= OnLoadedHandler;
		if (DataContext is NavigationViewModel viewModel)
		{
			_ = Dispatcher.BeginInvoke(() => viewModel.MyTasksViewModel?.Refresh());
			_ = Dispatcher.BeginInvoke(() => viewModel.RecentMessagesViewModel?.Refresh());
			_ = Dispatcher.BeginInvoke(() => viewModel.PublicHolidaysViewModel?.Refresh());
			_ = Dispatcher.BeginInvoke(() => viewModel.SnapshotsViewModel?.RefreshAndUpdateSnapshots());
		}
	}

	public NewsViewModel NewsViewModel { get; private set; } = new NewsViewModel();

	#region Dependecy properties
	public string UserName
	{
		get { return this.GetValue<string>(UserNameProperty); }
		set { SetValue(UserNameProperty, value); }
	}

	public static readonly DependencyProperty UserNameProperty =
		DependencyProperty.Register(nameof(UserName), typeof(string), typeof(NextHomeNavigation), new PropertyMetadata(null));

	public string Branch
	{
		get { return this.GetValue<string>(BranchProperty); }
		set { SetValue(BranchProperty, value); }
	}

	public static readonly DependencyProperty BranchProperty =
		DependencyProperty.Register(nameof(Branch), typeof(string), typeof(NextHomeNavigation), new PropertyMetadata(null));

	public string Company
	{
		get { return this.GetValue<string>(CompanyProperty); }
		set { SetValue(CompanyProperty, value); }
	}

	public static readonly DependencyProperty CompanyProperty =
		DependencyProperty.Register(nameof(Company), typeof(string), typeof(NextHomeNavigation), new PropertyMetadata(null));

	public string Department
	{
		get { return this.GetValue<string>(DepartmentProperty); }
		set { SetValue(DepartmentProperty, value); }
	}

	public static readonly DependencyProperty DepartmentProperty =
		DependencyProperty.Register(nameof(Department), typeof(string), typeof(NextHomeNavigation), new PropertyMetadata(string.Empty));

	public BitmapImage UserImage
	{
		get { return this.GetValue<BitmapImage>(UserImageProperty); }
		set { SetValue(UserImageProperty, value); }
	}

	public static readonly DependencyProperty UserImageProperty =
		DependencyProperty.Register(nameof(UserImage), typeof(BitmapImage), typeof(NextHomeNavigation), new PropertyMetadata(null));

	public bool IsSmallScreen
	{
		get { return this.GetValue<bool>(IsSmallScreenProperty); }
		set { SetValue(IsSmallScreenProperty, value); }
	}

	public static readonly DependencyProperty IsSmallScreenProperty =
		DependencyProperty.Register(nameof(IsSmallScreen), typeof(bool), typeof(NextHomeNavigation), new PropertyMetadata(false));

	#endregion
#pragma warning disable CW1021 // Static Fields Are Thread Static Rule
	public static RoutedCommand NavigateSearchResultsCommand = new RoutedCommand();
	public static RoutedCommand CloseSearchWindowCommand = new RoutedCommand();
	public static RoutedCommand ExecuteSearchItemHyperlinkCommand = new RoutedCommand();
	public static RoutedCommand RefreshRecentMessagesCommand = new RoutedCommand();
	public static RoutedCommand CloseMainMenuCommand = new RoutedCommand();
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule

	void NavigateSearchResults(object target, ExecutedRoutedEventArgs e)
	{
		if (e.Parameter is SearchResultNavigateDirection direction
			&& DataContext is NavigationViewModel viewModel
			&& viewModel.IsInSearchMode)
		{
			viewModel.SearchViewModel.Navigate(direction);

			NextAppBar.QuickSearch.Focus();
		}
	}

	void ExecuteSearchItemHyperlink(object target, ExecutedRoutedEventArgs e)
	{
		if (DataContext is NavigationViewModel viewModel && viewModel.IsInSearchMode)
		{
			ExecuteCurrentItem(viewModel.SearchViewModel.SearchResults);
		}
	}

	void ExecuteCurrentItem(ObservableCollection<SearchResultSection> searchResultSection)
	{
		if (DataContext is NavigationViewModel viewModel
				&& viewModel.SearchViewModel.CurrentSectionIndex >= 0
				&& viewModel.SearchViewModel.CurrentSectionIndex < searchResultSection.Count)
		{
			var section = searchResultSection[viewModel.SearchViewModel.CurrentSectionIndex];
			if (viewModel.SearchViewModel.CurrentItemIndex >= 0 && viewModel.SearchViewModel.CurrentItemIndex < section.Items.Count)
			{
				section.Items[viewModel.SearchViewModel.CurrentItemIndex].LinkAction?.Execute(null);
			}
		}
		else
		{
			var item = searchResultSection
			.FirstOrDefault(s => s.Items.Count > 0)?
			.Items.FirstOrDefault();
			item?.LinkAction?.Execute(null);
		}
	}

	void ClearSelections()
	{
		if (DataContext is NavigationViewModel viewModel)
		{
			viewModel.ForceClearSearch();
			viewModel.HideAllMenus();
			NextAppBar.QuickSearch.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}
	}

	void CloseSearchWindow(object target, ExecutedRoutedEventArgs e)
	{
		if (e.Parameter is string parameter
			&& parameter == "Escape"
			&& DataContext is NavigationViewModel viewModel
			&& !string.IsNullOrEmpty(viewModel.SearchViewModel.SearchValue))
		{
			viewModel.SearchViewModel.SearchValue = string.Empty;
			NextAppBar.QuickSearch.Focus();
			e.Handled = true;
			return;
		}
		ClearSelections();
		e.Handled = false;
	}

	void GetLoginDetails()
	{
		UserName = GlbStaff.CurrentUser.GS_FullName;
		Branch = GlbBranch.CurrentBranch.GB_BranchName;
		Company = GlbCompany.CurrentCompany.GC_Name;
		Department = GlbDepartment.CurrentDepartment.GE_Desc;
		UserImage = GetBitmapImage(GlbStaff.CurrentUser.ProfileImage);
	}

	/// <summary>
	/// Converts Drawing.Image to BitmapImage, used to populate UserImage property displayed in footer.
	/// </summary>
	public BitmapImage GetBitmapImage(Image image)
	{
		if (image == null)
		{
			return null;
		}

		var bitmapImage = new BitmapImage();
		using (var memory = new MemoryStream())
		{
			image.Save(memory, ImageFormat.Png);
			memory.Position = 0;

			bitmapImage.BeginInit();
			bitmapImage.StreamSource = memory;
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.EndInit();

			return bitmapImage;
		}
	}

	public void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		IsSmallScreen = e.NewSize.Width < 1920;
	}

	void HandleSearchShortcut(object sender, ExecutedRoutedEventArgs e)
	{
		if (DataContext is NavigationViewModel viewModel && !viewModel.IsInSearchMode)
		{
			viewModel.IsInSearchMode = true;
		}
		if (!NextAppBar.QuickSearch.IsFocused)
		{
			NextAppBar.QuickSearch.Focus();
		}
	}

	#region POPUP MENU

	internal ZToolStrip NavBarToolStrip { set; get; }

#pragma warning disable CW1021 // Static Fields Are Thread Static Rule
	public static RoutedCommand PopupMenuClick = new();
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule

	void HandlePopupMenu(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Parameter is System.Windows.Controls.MenuItem menuItem
			&& DataContext is NavigationViewModel viewModel)
		{
			viewModel.SelectedPopupMenu = menuItem.DataContext as PopupMenu;
			menuItem.Focus();
			PopupMenu(menuItem, viewModel.SelectedPopupMenu);
		}
	}

	// Takes the old CW1 main form nav menu (which is hidden), repositions it, then simulates opening the dropdown
	// when the new CW Next nav menu is clicked.
	// The offsets are manually calculated fudge factors to position the menu correctly.
	void PopupMenu(object sender, PopupMenu popupMenu)
	{
		if (popupMenu != null
			&& popupMenu.ToolStripDropDown is System.Windows.Forms.ToolStripDropDownItem toolStripDropDownItem
			&& sender is System.Windows.Controls.MenuItem menuItem)
		{
			var host = NavBarToolStrip.Parent as NextNavigation;
			var point = menuItem.PointToScreen(new Point(0, menuItem.ActualHeight));
			var clientPoint = host.PointToClient(new System.Drawing.Point((int)point.X + popupMenu.DropDownOffset.X, (int)point.Y + popupMenu.DropDownOffset.Y));
			NavBarToolStrip.Location = clientPoint;

			// This is important to ensure the existing event handlers are called to populate the menus.
			// Merely calling Show() on the ToolStripDropDown (instead of ToolStripDropDownItem) will not raise the event handlers.
			toolStripDropDownItem.ShowDropDown();
		}
	}

	static class NativeWindows
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ReleaseCapture();

		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return")]
		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "3")]
		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2")]
		[DllImport("user32.dll")]
		internal static extern Int32 SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, POINTS pos);

		[StructLayout(LayoutKind.Sequential)]
		internal struct POINTS // point structure with SHORT x and y
		{
			public short X;
			public short Y;
		}
		public const int WM_NCLBUTTONDOWN = 0x00A1;
		public const int HTCAPTION = 2;
	}

	void TopPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		//close all the menus when the top panel is clicked
		// send a WM_NCLBUTTONDOWNWM_NCLBUTTONDOWN with HitTestValues.HTCAPTION to Windows to simulate a click on the title bar
		ClearSelections();
		var host = ZApplication.GetOpenForms()[0]; // the first form opened is always mainform
		var point = PointToScreen(e.GetPosition(this));
		var winFormsPoint = host.PointToClient(new System.Drawing.Point((int)point.X, (int)point.Y));
		NativeWindows.ReleaseCapture();
		var pt = new NativeWindows.POINTS { X = (short)winFormsPoint.X, Y = (short)winFormsPoint.Y };
		NativeWindows.SendMessage(host.Handle, NativeWindows.WM_NCLBUTTONDOWN, NativeWindows.HTCAPTION, pt);
		e.Handled = true;
	}
	#endregion

	void RefreshTask(object sender, ExecutedRoutedEventArgs e)
	{
		if (DataContext is NavigationViewModel viewModel)
		{
			viewModel.MyTasksViewModel?.Refresh();
		}
	}

	void RefreshRecentMessages(object sender, ExecutedRoutedEventArgs e)
	{
		if (DataContext is NavigationViewModel viewModel)
		{
			viewModel.RecentMessagesViewModel?.Refresh();
		}
	}
}
#endif
