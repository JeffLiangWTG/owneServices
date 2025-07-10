using CargoWise.GUI.TileBar;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Media = System.Windows.Media;

namespace CargoWise.Main.Navigation;
public partial class TileNavigationBar : ModuleNavigation
{
	public TileNavigationBar()
	{
		InitializeComponent();

		TileControl = new TileBarControl();
		#if !WINZOR
		TileControl.FontFamily = new Media.FontFamily(OFont.NavigationMenuFontName);
		TileControl.Padding = new System.Windows.Thickness(2.0, 0.0, 2.0, 2.0);
#endif
		TileControl.InitializeComponent();
		TileControl.DataContext = navigationViewModel;
		navigationViewModel.InitializeSearch(navigationViewModel.GlobalSearch);
		HostControl.Child = TileControl;

		Controls.Add(rightBorder);
		RefreshTheme();
		HookCategoryChangedEvent();
	}

	public void RefreshTheme()
	{
		var colorTheme = SystemDataRegistry.Instance.ColorTheme;
		HostControl.BackColor = colorTheme.NavBarBackgroundColor;
		TileControl.TileBackgroundColor = colorTheme.NavBarButtonColor1;
		TileControl.TileSelectedColor = colorTheme.NavBarGroupSelected1;
		TileControl.TileTextColor = colorTheme.NavBarTextColor;
		TileControl.GroupBackgroundColor = colorTheme.NavBarGroupBackground1;
		TileControl.GroupHeaderBackgroundColor = colorTheme.NavBarGroupHeaderBackground;
		TileControl.RecentPanelBackgroundColor = colorTheme.NavBarRecentPanelBackground;
	}

	public override bool SelectFindbox(string initialText = "")
	{
		if (navigationViewModel.SelectedCategory != null && navigationViewModel.SelectedCategory.Name == ModuleTreeLoaderConstant.Category.Jump.Name)
		{
			navigationViewModel.IsInSearchMode = true;
			navigationViewModel.SearchViewModel.SearchValue = initialText;
			TileControl.SetCaretPosition(initialText.Length);

			return true;
		}

		return false;
	}
	#region Dispose

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "components")]
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			((IExtendedControl)this).Extensions.Dispose();

			if (TileControl != null)
			{
				navigationViewModel.Dispose();
				TileControl.DataContext = null;
			}

			components?.Dispose();
		}

		base.Dispose(disposing);
	}
	#endregion
	internal TileBarControl TileControl;
}
