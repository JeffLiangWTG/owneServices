using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// This view model is used to create tab pages and manages the header property within the application's ribbon interface
	/// and inherits from <see cref="RibbonViewModelBase"/> abstract class
	/// </summary>
	/// <remarks>
	public class RibbonTabViewModel : RibbonViewModelBase
	{
		public RibbonTabViewModel(ResourceString header)
			: base(header?.ResourceKey)
		{
			Header = header;
		}

		public MultilingualString Header
		{
			get => header;
			set
			{
				header = value;
				OnPropertyChanged(nameof(Header));
			}
		}

		MultilingualString header;

		public RibbonCollection<RibbonGroupViewModel> Groups { get; } = new RibbonCollection<RibbonGroupViewModel>();
	}
}
