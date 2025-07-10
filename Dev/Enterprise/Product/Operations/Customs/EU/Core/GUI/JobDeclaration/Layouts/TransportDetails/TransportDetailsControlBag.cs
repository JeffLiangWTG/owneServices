using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class TransportDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new TransportDetailsUserControl();

		public static TransportDetailsControlBag Instance => instance.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TransportDetailsControlBag> instance = new(() => new());

		TransportDetailsControlBag()
		{
			InlandTransportDetailsUserControl = RegisterControl(nameof(TransportDetailsUserControl.InlandTransportDetailsUserControl));
			FlightAndNationalityUserControl = RegisterControl(nameof(TransportDetailsUserControl.FlightAndNationalityUserControl));
			AircraftRegistrationNumberTextBox = RegisterControl(nameof(TransportDetailsUserControl.AircraftRegistrationNumberTextBox));
			TransportIDAndNationalityUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportIDAndNationalityUserControl));
			TransportIDAndNationalityRailUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportIDAndNationalityRailUserControl));
			TransportIDAndNationalityInlandWaterwayUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportIDAndNationalityInlandWaterwayUserControl));
			TransportIDAndNationalityInlandWaterwayENIUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportIDAndNationalityInlandWaterwayENIUserControl));
			AdditionalWagonNumbersUserControl = RegisterControl(nameof(TransportDetailsUserControl.AdditionalWagonNumbersUserControl));
			TransportNationalityCodeFindBox = RegisterControl(nameof(TransportDetailsUserControl.TransportNationalityCodeFindBox));
			VesselUserControl = RegisterControl(nameof(TransportDetailsUserControl.VesselUserControl));
		}

		public ControlReference InlandTransportDetailsUserControl;
		public ControlReference AircraftRegistrationNumberTextBox { get; }
		public ControlReference FlightAndNationalityUserControl { get; }
		public ControlReference TransportIDAndNationalityUserControl { get; }
		public ControlReference TransportIDAndNationalityRailUserControl { get; }
		public ControlReference TransportIDAndNationalityInlandWaterwayUserControl { get; }
		public ControlReference TransportIDAndNationalityInlandWaterwayENIUserControl { get; }
		public ControlReference TransportNationalityCodeFindBox { get; }
		public ControlReference AdditionalWagonNumbersUserControl { get; }
		public ControlReference VesselUserControl { get; }
	}
}
