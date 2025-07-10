using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class ShipmentTypeControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ShipmentTypeUserControl();

	public static ShipmentTypeControlBag Instance => instance ?? (instance = new ShipmentTypeControlBag());

	[ThreadStatic]
	static ShipmentTypeControlBag instance;

	ShipmentTypeControlBag()
	{
		ExitPointDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.ExitPointDropEdit));
		ClearanceLocationDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.ClearanceLocationDropEdit));
	}

	public ControlReference ExitPointDropEdit;
	public ControlReference ClearanceLocationDropEdit;
}
