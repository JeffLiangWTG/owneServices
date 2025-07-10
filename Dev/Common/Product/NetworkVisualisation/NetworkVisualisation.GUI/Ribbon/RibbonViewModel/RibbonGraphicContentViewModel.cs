#if !WINZOR
using System.Windows.Media;
#endif

namespace CargoWise.NetworkVisualisation.GUI
{
	public abstract class RibbonGraphicContentViewModel : RibbonViewModelBase
	{
		protected RibbonGraphicContentViewModel(RibbonViewModel ribbon, string key, string iconName, RibbonImageLayout imageLayout)
			: base(key)
		{
#if WINZOR
			Image64 = ribbon.GetResource(iconName + "/base64");
#endif
			Image = ribbon.GetResource(iconName);
			ImageLayout = imageLayout;
		}

		public
#if !WINZOR
		Drawing
#else
		string
#endif
		Image
		{
			get => image;
			set
			{
				image = value;
				OnPropertyChanged(nameof(Image));
			}
		}
#if !WINZOR
		Drawing
#else
		string
#endif
		image;

		public RibbonImageLayout ImageLayout { get; }

#if WINZOR
		public string Image64;
#endif
	}
}
