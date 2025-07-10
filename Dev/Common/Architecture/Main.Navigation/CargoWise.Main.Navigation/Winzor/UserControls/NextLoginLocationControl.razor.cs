using System.Diagnostics.CodeAnalysis;
using CargoWise.Main.Navigation.ViewModels;

namespace CargoWise.Main.Navigation;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in RecentModules.razor")]
public partial class NextLoginLocationControl : BaseUserControl
{
	[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Called in NextLoginLocationViewModel")]
	NextLoginLocationService? Service { get; set; }

	INextLoginLocationViewModel? viewModel;
	public INextLoginLocationViewModel NextLoginLocationViewModel
	{
		get => viewModel!;
		set
		{
			if (viewModel != value)
			{
				viewModel = value;
				Service = new NextLoginLocationService(viewModel, this);
			}
		}
	}
}
