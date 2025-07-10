using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ShipmentDetailsOriginUserControl : ZUserControl
{
	public ShipmentDetailsOriginUserControl()
	{
		InitializeComponent();

#if DEBUG
		TypeDescriptor.AddAttributes(EstimatedDepartureDateEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(GoodsOriginDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(OriginFindBox, new SuppressControlRequiresTextBasherAttribute());
#endif
	}
}
