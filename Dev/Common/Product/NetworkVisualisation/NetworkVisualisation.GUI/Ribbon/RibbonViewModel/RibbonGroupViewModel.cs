using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Represents a view model for a Ribbon Group.
	/// Extends from the RibbonGraphicContentViewModel
	/// It is used extensively by BMRibbonViewModel and DefaultNetworkRibbonViewModel to create Ribbon groups.
	/// Also used by RibbonTabViewModel to create collection of Ribbon groups.
	/// </summary>
	public class RibbonGroupViewModel : RibbonGraphicContentViewModel
	{
		/// <summary>
		/// Constructor for  RibbonGroupViewModel. This model is used to create Ribbon groupd containing the Ribbon buttons with a header, name, icon and layout.
		/// </summary>
		/// <param name="ribbonViewModel">The ribbon view model to be add to the group</param>
		/// <param name="header">Name of the Header</param>
		/// <param name="iconName">Icon name associated with this Ribbon View model</param>
		/// <param name="imageLayout">The kind of layout is an enum - BothLargeAndSmallImages or SmallImageOnly</param>
		public RibbonGroupViewModel(RibbonViewModel ribbonViewModel, ResourceString header, string iconName, RibbonImageLayout imageLayout = RibbonImageLayout.BothLargeAndSmallImages)
			: base(ribbonViewModel, header?.ResourceKey, iconName, imageLayout)
		{
			Header = header;
		}

		/// <summary>
		/// Represents the Header which is the name of the Ribbon button
		/// </summary>
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

		/// <summary>
		/// Represents a collection of RibbonButtons associated with this Ribbon group.
		/// </summary>
		public RibbonCollection<RibbonButtonViewModel> Items { get; } = new RibbonCollection<RibbonButtonViewModel>();
	}
}
