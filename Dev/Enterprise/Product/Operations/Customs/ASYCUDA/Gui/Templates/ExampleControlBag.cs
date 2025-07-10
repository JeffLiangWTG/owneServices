using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class ExampleControlBag : ControlBag
	{
		public static ExampleControlBag Instance => exampleControlBag.Value;

		ExampleControlBag()
		{
			ManifestTypeDropEdit = RegisterControl(nameof(ExampleControlTemplate.ManifestTypeDropEdit));
			NatureDropEdit = RegisterControl(nameof(ExampleControlTemplate.NatureDropEdit));
			TransportModeDropEdit = RegisterControl(nameof(ExampleControlTemplate.TransportModeDropEdit));
			ContainerModeDropEdit = RegisterControl(nameof(ExampleControlTemplate.ContainerModeDropEdit));
			BuyersConsolidationCheckBox = RegisterControl(nameof(ExampleControlTemplate.BuyersConsolidationCheckBox));
			AgentTypeDropEdit = RegisterControl(nameof(ExampleControlTemplate.AgentTypeDropEdit));
			MasterBOLTextBox = RegisterControl(nameof(ExampleControlTemplate.MasterBOLTextBox));
			VehicleRegistrationTextBox = RegisterControl(nameof(ExampleControlTemplate.VehicleRegistrationTextBox));
			VesselCodeFindBox = RegisterControl(nameof(ExampleControlTemplate.VesselCodeFindBox));
			RadioCallSignTextBox = RegisterControl(nameof(ExampleControlTemplate.RadioCallSignTextBox));
			VoyageFlightTextBox = RegisterControl(nameof(ExampleControlTemplate.VoyageFlightTextBox));
			ConveyanceCountryCodeFindBox = RegisterControl(nameof(ExampleControlTemplate.ConveyanceCountryCodeFindBox));
			MastersNameTextBox = RegisterControl(nameof(ExampleControlTemplate.MastersNameTextBox));
			PortOfLoadingCodeFindBox = RegisterControl(nameof(ExampleControlTemplate.PortOfLoadingCodeFindBox));
			CustomsLoadPortCodeFindBox = RegisterControl(nameof(ExampleControlTemplate.CustomsLoadPortCodeFindBox));
			EstDepartureDateEdit = RegisterControl(nameof(ExampleControlTemplate.EstDepartureDateEdit));
			PortOfFirstArrivalCodeFindBox = RegisterControl(nameof(ExampleControlTemplate.PortOfFirstArrivalCodeFindBox));
			PortOfDischargeCodeFindBox = RegisterControl(nameof(ExampleControlTemplate.PortOfDischargeCodeFindBox));
			CustomsDischargePortCodeFindBox = RegisterControl(nameof(ExampleControlTemplate.CustomsDischargePortCodeFindBox));
			EstArrivalDateEdit = RegisterControl(nameof(ExampleControlTemplate.EstArrivalDateEdit));
			CarrierAddressControl = RegisterControl(nameof(ExampleControlTemplate.CarrierAddressControl));
			CarrierCodeTextBox = RegisterControl(nameof(ExampleControlTemplate.CarrierCodeTextBox));
			ManifestNumberFromMasterBillTextBox = RegisterControl(nameof(ExampleControlTemplate.ManifestNumberFromMasterBillTextBox));
			IssueDateDateEdit = RegisterControl(nameof(ExampleControlTemplate.IssueDateDateEdit));
			CustomsOfficeDropEdit = RegisterControl(nameof(ExampleControlTemplate.CustomsOfficeDropEdit));
			ShippingAgentAddressControl = RegisterControl(nameof(ExampleControlTemplate.ShippingAgentAddressControl));
			DeconsolidateAddressControl = RegisterControl(nameof(ExampleControlTemplate.DeconsolidateAddressControl));
			DischargeTerminalAddressControl = RegisterControl(nameof(ExampleControlTemplate.DischargeTerminalAddressControl));
		}

		public ControlReference ManifestTypeDropEdit { get; }
		public ControlReference NatureDropEdit { get; }
		public ControlReference TransportModeDropEdit { get; }
		public ControlReference ContainerModeDropEdit { get; }
		public ControlReference BuyersConsolidationCheckBox { get; }
		public ControlReference AgentTypeDropEdit { get; }
		public ControlReference MasterBOLTextBox { get; }
		public ControlReference VehicleRegistrationTextBox { get; }
		public ControlReference VesselCodeFindBox { get; }
		public ControlReference RadioCallSignTextBox { get; }
		public ControlReference VoyageFlightTextBox { get; }
		public ControlReference ConveyanceCountryCodeFindBox { get; }
		public ControlReference MastersNameTextBox { get; }
		public ControlReference PortOfLoadingCodeFindBox { get; }
		public ControlReference CustomsLoadPortCodeFindBox { get; }
		public ControlReference EstDepartureDateEdit { get; }
		public ControlReference PortOfFirstArrivalCodeFindBox { get; }
		public ControlReference PortOfDischargeCodeFindBox { get; }
		public ControlReference CustomsDischargePortCodeFindBox { get; }
		public ControlReference EstArrivalDateEdit { get; }
		public ControlReference CarrierAddressControl { get; }
		public ControlReference CarrierCodeTextBox { get; }
		public ControlReference ManifestNumberFromMasterBillTextBox { get; }
		public ControlReference IssueDateDateEdit { get; }
		public ControlReference CustomsOfficeDropEdit { get; }
		public ControlReference ShippingAgentAddressControl { get; }
		public ControlReference DeconsolidateAddressControl { get; }
		public ControlReference DischargeTerminalAddressControl { get; }

		protected override Control CreateTemplate() => new ExampleControlTemplate();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<ExampleControlBag> exampleControlBag = new Lazy<ExampleControlBag>(() => new ExampleControlBag());
	}
}
