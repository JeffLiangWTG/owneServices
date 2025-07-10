using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class GoodsLocationFUserControl : ZUserControl
{
	public GoodsLocationFUserControl()
	{
		InitializeComponent();

#if DEBUG
		TypeDescriptor.AddAttributes(LocationQualifierDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(GoodsLocationAddressZDocAddressControl, new SuppressControlRequiresTextBasherAttribute());
#endif
	}
}
