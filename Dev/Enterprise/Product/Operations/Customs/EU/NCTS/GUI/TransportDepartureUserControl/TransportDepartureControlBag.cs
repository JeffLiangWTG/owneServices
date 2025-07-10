using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class TransportDepartureControlBag : ControlBag
	{
		public TransportDepartureControlBag()
		{
			InlandTransportModeDropEdit = RegisterControl(nameof(TransportDepartureUserControl.InlandTransportModeDropEdit));
			TransportAtDepartureTextBox = RegisterControl(nameof(TransportDepartureUserControl.TransportAtDepartureTextBox));
			TransportAtDepartureCountryCodeFindBox = RegisterControl(nameof(TransportDepartureUserControl.TransportAtDepartureCountryCodeFindBox));
			TransportAtDepartureTrailer1RegNoTextBox = RegisterControl(nameof(TransportDepartureUserControl.TransportAtDepartureTrailer1RegNoTextBox));
			TransportAtDepartureTrailer1NationalityCodeFindBox = RegisterControl(nameof(TransportDepartureUserControl.TransportAtDepartureTrailer1NationalityCodeFindBox));
			TransportAtDepartureTrailer2RegNoTextBox = RegisterControl(nameof(TransportDepartureUserControl.TransportAtDepartureTrailer2RegNoTextBox));
			TransportAtDepartureTrailer2NationalityCodeFindBox = RegisterControl(nameof(TransportDepartureUserControl.TransportAtDepartureTrailer2NationalityCodeFindBox));
			TransportAtDepartureTypeDropEdit = RegisterControl(nameof(TransportDepartureUserControl.TransportAtDepartureTypeDropEdit));
			AircraftIDAtDepartureTextBox = RegisterControl(nameof(TransportDepartureUserControl.AircraftIDAtDepartureTextBox));
			VesselCodeFindBox = RegisterControl(nameof(TransportDepartureUserControl.VesselCodeFindBox));
			VesselCountryCodeFindBox = RegisterControl(nameof(TransportDepartureUserControl.VesselCountryCodeFindBox));
			AdditionalWagonNumbersButton = RegisterControl(nameof(TransportDepartureUserControl.AdditionalWagonNumbersButton));
			PlaceHolderLabel = RegisterControl(nameof(TransportDepartureUserControl.PlaceHolderLabel));
			PlaceHolder2Label = RegisterControl(nameof(TransportDepartureUserControl.PlaceHolder2Label));
		}

		public static TransportDepartureControlBag Instance => instance ?? (instance = new TransportDepartureControlBag());

		[ThreadStatic]
		static TransportDepartureControlBag instance;

		public ControlReference InlandTransportModeDropEdit { get; }

		public ControlReference TransportAtDepartureTextBox { get; }

		public ControlReference TransportAtDepartureCountryCodeFindBox { get; }

		public ControlReference TransportAtDepartureTrailer1RegNoTextBox { get; }

		public ControlReference TransportAtDepartureTrailer1NationalityCodeFindBox { get; }

		public ControlReference TransportAtDepartureTrailer2RegNoTextBox { get; }

		public ControlReference TransportAtDepartureTrailer2NationalityCodeFindBox { get; }

		public ControlReference TransportAtDepartureTypeDropEdit { get; }

		public ControlReference AircraftIDAtDepartureTextBox { get; }

		public ControlReference VesselCodeFindBox { get; }

		public ControlReference VesselCountryCodeFindBox { get; }

		public ControlReference AdditionalWagonNumbersButton { get; }

		public ControlReference PlaceHolderLabel { get; }

		public ControlReference PlaceHolder2Label { get; }

		protected override Control CreateTemplate() => new TransportDepartureUserControl();
	}
}
