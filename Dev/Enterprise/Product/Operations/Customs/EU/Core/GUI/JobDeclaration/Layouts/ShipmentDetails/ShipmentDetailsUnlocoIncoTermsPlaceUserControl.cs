using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class ShipmentDetailsUnlocoIncoTermsPlaceUserControl : ZUserControl
	{
		public ShipmentDetailsUnlocoIncoTermsPlaceUserControl()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(AgreedPlaceCodeFindBox, new SuppressControlRequiresTextBasherAttribute());
#endif
		}
	}
}
