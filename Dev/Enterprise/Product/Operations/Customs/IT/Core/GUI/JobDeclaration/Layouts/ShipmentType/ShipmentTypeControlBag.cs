using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class ShipmentTypeControlBag : ControlBag
{
	public static ShipmentTypeControlBag Instance => instance ?? (instance = new ShipmentTypeControlBag());

	[ThreadStatic]
	static ShipmentTypeControlBag instance;

	protected override Control CreateTemplate() => new ShipmentTypeUserControl();

	ShipmentTypeControlBag()
	{
		AuthorisationNumberDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.AuthorisationNumberDropEdit));
		MessageVersionDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.MessageVersionDropEdit));
	}

	public ControlReference AuthorisationNumberDropEdit;
	public ControlReference MessageVersionDropEdit;
}
