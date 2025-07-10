using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ShipmentDetailsUserControl : ZUserControl
{
	public ShipmentDetailsUserControl()
	{
		InitializeComponent();

#if DEBUG
		TypeDescriptor.AddAttributes(ShipmentDetailsOriginUserControl, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(ShipmentDetailsFinalDestinationUserControl, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(LocationQualifierDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(GoodsLocationDUserControl, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(GoodsLocationFUserControl, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(GoodsLocationFCUserControl, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(GoodsLocationLBLCUserControl, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(SubLocationTextBox, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(AdditionalDeliveryTermsTextBox, new SuppressControlRequiresTextBasherAttribute());
#endif
	}
}
