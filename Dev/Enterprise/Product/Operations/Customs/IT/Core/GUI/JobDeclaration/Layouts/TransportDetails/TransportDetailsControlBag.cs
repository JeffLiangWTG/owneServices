using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class TransportDetailsControlBag : ControlBag
{
	TransportDetailsControlBag()
	{
		TransportInlandAirUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandAirUserControl));
		TransportInlandRoadUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandRoadUserControl));
		InlandTransportModeAndMeansUserControl = RegisterControl(nameof(TransportDetailsUserControl.InlandTransportModeAndMeansUserControl));
	}

	public static TransportDetailsControlBag Instance => instance ?? (instance = new TransportDetailsControlBag());

	[ThreadStatic]
	static TransportDetailsControlBag instance;

	protected override Control CreateTemplate() => new TransportDetailsUserControl();

	public ControlReference TransportInlandAirUserControl { get; }

	public ControlReference TransportInlandRoadUserControl { get; }

	public ControlReference InlandTransportModeAndMeansUserControl { get; }
}
