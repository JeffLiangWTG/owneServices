using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class TransportDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new TransportDetailsUserControl();

		public static TransportDetailsControlBag Instance => instance ?? (instance = new TransportDetailsControlBag());

		[ThreadStatic]
		static TransportDetailsControlBag instance;

		TransportDetailsControlBag()
		{
			CargoArrivalDocUtilizationDropEdit = RegisterControl(nameof(TransportDetailsUserControl.CargoArrivalDocUtilizationDropEdit));
			VesselAndCountryUserControl = RegisterControl(nameof(TransportDetailsUserControl.VesselAndCountryUserControl));
			PlateTextBox = RegisterControl(nameof(TransportDetailsUserControl.PlateTextBox));
		}

		public ControlReference CargoArrivalDocUtilizationDropEdit;
		public ControlReference VesselAndCountryUserControl;
		public ControlReference PlateTextBox;
	}
}
