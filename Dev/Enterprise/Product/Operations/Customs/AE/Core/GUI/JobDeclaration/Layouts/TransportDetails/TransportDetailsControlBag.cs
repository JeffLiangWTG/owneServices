using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class TransportDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new TransportDetailsUserControl();

	public static TransportDetailsControlBag Instance => instance ??= new TransportDetailsControlBag();

	[ThreadStatic]
	static TransportDetailsControlBag instance;

	TransportDetailsControlBag()
	{
		PlaceOfDischargeDropEdit = RegisterControl(nameof(TransportDetailsUserControl.PlaceOfDischargeDropEdit));
	}

	public ControlReference PlaceOfDischargeDropEdit;
}
