using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class ShipmentDetailsIncoTermsPlaceUserControl : ZUserControl
	{
		public ShipmentDetailsIncoTermsPlaceUserControl()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(AgreedPlaceCodeDropEdit, new SuppressControlRequiresTextBasherAttribute());
#endif
		}
	}
}
