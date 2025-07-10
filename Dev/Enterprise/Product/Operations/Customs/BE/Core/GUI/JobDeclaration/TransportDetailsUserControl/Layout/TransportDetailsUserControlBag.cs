using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

sealed class TransportDetailsUserControlBag : ControlBag
{
	TransportDetailsUserControlBag()
	{
		FlightAndNationalityUserControl = RegisterControl(nameof(TransportDetailsUserControl.FlightAndNationalityUserControl));
	}

	public static TransportDetailsUserControlBag Instance => instance ??= new TransportDetailsUserControlBag();

	[ThreadStatic]
	static TransportDetailsUserControlBag instance;

	protected override Control CreateTemplate() => new TransportDetailsUserControl();

	public ControlReference FlightAndNationalityUserControl { get; }
}
