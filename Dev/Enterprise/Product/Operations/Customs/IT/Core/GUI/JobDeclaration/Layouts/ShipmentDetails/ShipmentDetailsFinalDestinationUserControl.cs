using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ShipmentDetailsFinalDestinationUserControl : ZUserControl
{
	public ShipmentDetailsFinalDestinationUserControl()
	{
		InitializeComponent();

#if DEBUG
		TypeDescriptor.AddAttributes(FinalDestinationFindBox, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(GoodsDestinationDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(EstimatedArrivalDateEdit, new SuppressControlRequiresTextBasherAttribute());
#endif
	}
}
