using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class ShipmentDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

	public static ShipmentDetailsControlBag Instance => instance ??= new ShipmentDetailsControlBag();

	[ThreadStatic]
	static ShipmentDetailsControlBag instance;

	ShipmentDetailsControlBag()
	{
		TypeOfGoodsDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.TypeOfGoodsDropEdit));
		OperationalStatusDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.OperationalStatusDropEdit));
	}

	public ControlReference TypeOfGoodsDropEdit;
	public ControlReference OperationalStatusDropEdit;
}
