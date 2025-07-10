using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class TransportDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new TransportDetailsUserControl();

	public static TransportDetailsControlBag Instance => instance ?? (instance = new TransportDetailsControlBag());

	[ThreadStatic]
	static TransportDetailsControlBag instance;

	TransportDetailsControlBag()
	{
		VehicleTypeDropEdit = RegisterControl(nameof(TransportDetailsUserControl.VehicleTypeDropEdit));
		DispatchCountryUserControl = RegisterControl(nameof(TransportDetailsUserControl.DispatchCountryUserControl));
		SpecificCircumstanceIndicatorDropEdit = RegisterControl(nameof(TransportDetailsUserControl.SpecificCircumstanceIndicatorDropEdit));
		TransportModeDropEdit = RegisterControl(nameof(TransportDetailsUserControl.TransportModeDropEdit));
	}

	public ControlReference VehicleTypeDropEdit;
	public ControlReference DispatchCountryUserControl;
	public ControlReference SpecificCircumstanceIndicatorDropEdit;
	public ControlReference TransportModeDropEdit;
}
