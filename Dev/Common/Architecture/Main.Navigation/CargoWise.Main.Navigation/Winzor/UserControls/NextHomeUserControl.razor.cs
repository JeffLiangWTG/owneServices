using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using CargoWiseNext.Blazor.Components.Services;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.Main.Navigation;

public partial class NextHomeUserControl : BaseUserControl, IWinzorSupportedWPFContent, IMainPageModelService
{
	public Theme? Theme { get; private set; }

	public HomeFeaturesManager? FeaturesManager { get; set; }
	public SessionContextViewModel? SessionContextViewModel { get; set; }
	public INewsViewModel? NewsViewModelTop { get; set; }
	public INewsViewModel? NewsViewModelBottom { get; set; }
	public IMyTasksViewModel? MyTasksViewModel { get; set; }
	public KeyboardService KeyboardService { get; } = new();

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		KeyboardService.NotifyKeyDown(this, new KeyboardEventArgs()
		{
			CtrlKey = e.Control,
			Key = ((char)e.KeyCode).ToString(),
			Code = e.KeyCode.ToString(),
		});
	}

	NavigationViewModel? navigationViewModel;
	public NavigationViewModel? NavigationViewModel
	{
		get => navigationViewModel;
		set
		{
			navigationViewModel = value;
			if (navigationViewModel is not null)
			{
				navigationViewModel.PropertyChanged += OnNavigationViewModelPropertyChanged;
				InitializeServices(navigationViewModel);
			}
		}
	}

	public void SetTheme(IColorTheme colorTheme)
	{
		Theme = new Theme
		{
			Palette = new Palette()
			{
				BackgroundColor = colorTheme.MainFormBackgroundColor,
				AppBarBackgroundColor = colorTheme.TitleBarBackground,
				AppBarTextColor = colorTheme.TitleBarText,
				RecentFavBackgroundColor = colorTheme.NavBarRecentPanelBackground,
				NavBarGroupSelected = colorTheme.NavBarGroupSelected1,
				NavBarTextColor = colorTheme.NavBarTextColor,
			}
		};
	}

	protected override bool ProcessDialogKey(Keys keyData) => false;

	protected override void Dispose(bool disposing)
	{
		RecentModulesService?.Dispose();

		if (NavigationViewModel is not null)
		{
			NavigationViewModel.PropertyChanged -= OnNavigationViewModelPropertyChanged;
			if (NavigationViewModel.MainViewModel is not null)
			{
				NavigationViewModel.MainViewModel.Buttons.CollectionChanged -= OnNavigationButtonsChanged;
			}
		}

		base.Dispose(disposing);
	}

	public bool IsSnapshotsEnabled => FeaturesManager?.IsEnabled(HomeFeature.Snapshots) ?? true;

	#region Services
	IRecentModulesService? RecentModulesService { get; set; }
	IMenuService? RecentItemsService { get; set; }
	IFavoritesService? FavoritesService { get; set; }
	INavBarToolStripService? NavBarToolStripService { get; set; }
	IQuickSearchService? QuickSearchService { get; set; }
	IMainMenuService? MainMenuService { get; set; }
	IRecentMessagesService? RecentMessagesService { get; set; }
	IPublicHolidayService? PublicHolidayService { get; set; }
	ISnapshotsService? SnapshotsService { get; set; }

	void InitializeServices(NavigationViewModel navigationViewModel)
	{
		MainMenuService ??= InitializeMainMenuService(navigationViewModel);
		RecentModulesService ??= InitializeRecentModulesService(navigationViewModel);
		RecentItemsService ??= InitializeRecentItemsServices(navigationViewModel);
		FavoritesService ??= InitializeFavoritesServices(navigationViewModel);
		RecentMessagesService ??= InitializeRecentMessagesService(navigationViewModel);
		PublicHolidayService ??= InitializePublicHolidayService(navigationViewModel);
		SnapshotsService ??= InitializeSnapshotsService(navigationViewModel);
		NavBarToolStripService ??= InitializeNavBarToolStripService(navigationViewModel);
		QuickSearchService ??= InitializeQuickSearchService();
	}

	ISnapshotsService? InitializeSnapshotsService(NavigationViewModel navigationViewModel)
	{
		if (navigationViewModel.SnapshotsViewModel is null)
		{
			return null;
		}

		return new SnapshotsService(navigationViewModel.SnapshotsViewModel, this);
	}

	IRecentMessagesService? InitializeRecentMessagesService(NavigationViewModel navigationViewModel)
	{
		if (navigationViewModel?.RecentMessagesViewModel is null)
		{
			return null;
		}

		return new RecentMessagesService(navigationViewModel.RecentMessagesViewModel, this);
	}

	IPublicHolidayService? InitializePublicHolidayService(NavigationViewModel navigationViewModel)
	{
		if (navigationViewModel?.PublicHolidaysViewModel is null)
		{
			return null;
		}

		return new PublicHolidayService(navigationViewModel.PublicHolidaysViewModel, this);
	}

	IMainMenuService? InitializeMainMenuService(NavigationViewModel navigationViewModel)
	{
		if (navigationViewModel is null)
		{
			return null;
		}

		return new MainMenuService(navigationViewModel, this);
	}

	IQuickSearchService InitializeQuickSearchService()
	{
		return new QuickSearchService(this);
	}

	INavBarToolStripService? InitializeNavBarToolStripService(NavigationViewModel navigationViewModel)
	{
		if (navigationViewModel is null)
		{
			return null;
		}

		return new NavBarToolStripService(navigationViewModel);
	}

	IRecentModulesService? InitializeRecentModulesService(NavigationViewModel navigationViewModel)
	{
		var recentModules = navigationViewModel.MainViewModel?.RecentModules;
		if (recentModules?.Count > 0)
		{
			var noItemsMessage = navigationViewModel?.MainViewModel?.NoRecentModulesText ?? string.Empty;
			return new RecentModulesService((MenuSection)recentModules[0], winzorControl: this, noItemsMessage);
		}

		return null;
	}

	IMenuService? InitializeRecentItemsServices(NavigationViewModel navigationViewModel)
	{
		var recentItems = navigationViewModel?.MainViewModel?.RecentItems;
		IMenuService? service = null;
		if (recentItems?.Count > 0)
		{
			var noItemsMessage = navigationViewModel?.MainViewModel?.NoRecentItemsText ?? string.Empty;
			service = new MenuService((MenuSection)recentItems[0], this, noItemsMessage);
			service.Items = navigationViewModel?.MainViewModel?.RecentMenuItems;
		}
		return service;
	}

	IFavoritesService? InitializeFavoritesServices(NavigationViewModel navigationViewModel)
	{
		var dragToReorderText = navigationViewModel?.MainViewModel?.DragToReorderText ?? string.Empty;
		var favoriteModules = navigationViewModel?.MainViewModel?.Favorites;
		if (favoriteModules?.Count > 0)
		{
			var noItemsMessage = navigationViewModel?.MainViewModel?.NoFavoritesText ?? string.Empty;
			return new FavoritesService((MenuSection)favoriteModules[0], this, noItemsMessage, dragToReorderText);
		}
		return null;
	}
	#endregion

	void OnNavigationViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(CargoWise.Main.Navigation.ViewModels.NavigationViewModel.MainViewModel))
		{
			if (NavigationViewModel?.MainViewModel is null)
			{
				return;
			}

			NavigationViewModel.MainViewModel.Buttons.CollectionChanged += OnNavigationButtonsChanged;
			InitializeServices(NavigationViewModel);
		}
	}

	void OnNavigationButtonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (NavigationViewModel is not null)
		{
			InitializeServices(NavigationViewModel);
		}
	}
}
