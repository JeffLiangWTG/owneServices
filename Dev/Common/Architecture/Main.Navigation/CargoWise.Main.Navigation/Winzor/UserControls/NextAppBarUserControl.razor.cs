using System.Windows.Forms;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using CargoWiseNext.Blazor.Components.Services;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.Main.Navigation;

public partial class NextAppBarUserControl : BaseUserControl , IMainPageInvokeService
{
	public Theme? Theme { get; private set; }

	public NextAppBarUserControl(NavigationViewModel viewModel)
	{
		NavigationViewModel = viewModel;
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
				InitializeServices(navigationViewModel);
			}
		}
	}

#pragma warning disable IDE0052
	public KeyboardService KeyboardService { get; } = new();
	INavBarToolStripService? NavBarToolStripService { get; set; }
	IQuickSearchService? QuickSearchService { get; set; }
	IMainMenuService? MainMenuService { get; set; }
#pragma warning restore IDE0052
	void InitializeServices(NavigationViewModel viewModel)
	{
		MainMenuService = InitializeMainMenuService(viewModel);
		NavBarToolStripService = InitializeNavBarToolStripService(viewModel);
		QuickSearchService = InitializeQuickSearchService();
	}

	IMainMenuService InitializeMainMenuService(NavigationViewModel viewModel)
	{
		return new MainMenuService(viewModel, this);
	}

	IQuickSearchService InitializeQuickSearchService()
	{
		return new QuickSearchService(this);
	}

	INavBarToolStripService? InitializeNavBarToolStripService(NavigationViewModel viewModel)
	{
		return new NavBarToolStripService(viewModel);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		KeyboardService.NotifyKeyDown(this, new KeyboardEventArgs()
		{
			CtrlKey = e.Control,
			Key = ((char)e.KeyCode).ToString(),
		});
	}

	public void TriggerKeyDown(KeyEventArgs e)
	{
		OnKeyDown(e);
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
}
