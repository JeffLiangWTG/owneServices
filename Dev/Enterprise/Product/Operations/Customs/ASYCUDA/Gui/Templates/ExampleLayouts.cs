using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	internal static class ExampleLayouts
	{
		public static PanelLayout CreateExampleLayout1()
		{
			var common = ExampleControlBag.Instance;

			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(109);

			layout.Include(ruler1, common.ManifestTypeDropEdit);
			layout.Include(ruler1, common.NatureDropEdit);
			layout.Include(ruler1, common.TransportModeDropEdit);
			layout.Include(ruler1, common.ContainerModeDropEdit);
			layout.Include(ruler1, common.BuyersConsolidationCheckBox);
			layout.Include(ruler1, common.AgentTypeDropEdit);
			layout.Include(ruler1, common.MasterBOLTextBox);
			layout.Include(ruler1, common.VehicleRegistrationTextBox);
			layout.Include(ruler1, common.VesselCodeFindBox);
			layout.Include(ruler1, common.RadioCallSignTextBox);
			layout.Include(ruler1, common.VoyageFlightTextBox);
			layout.Include(ruler1, common.ConveyanceCountryCodeFindBox);
			layout.Include(ruler1, common.MastersNameTextBox);
			layout.Include(ruler1, common.PortOfLoadingCodeFindBox);
			layout.Include(ruler1, common.CustomsLoadPortCodeFindBox);
			layout.Include(ruler1, common.EstDepartureDateEdit);
			layout.Include(ruler1, common.PortOfFirstArrivalCodeFindBox);
			layout.Include(ruler1, common.PortOfDischargeCodeFindBox);
			layout.Include(ruler1, common.CustomsDischargePortCodeFindBox);
			layout.Include(ruler1, common.EstArrivalDateEdit);
			layout.Include(ruler1, common.CarrierAddressControl);
			layout.Include(ruler1, common.CarrierCodeTextBox);
			layout.Include(ruler1, common.ManifestNumberFromMasterBillTextBox);
			layout.Include(ruler1, common.IssueDateDateEdit);
			layout.Include(ruler1, common.CustomsOfficeDropEdit);
			layout.Include(ruler1, common.ShippingAgentAddressControl);
			layout.Include(ruler1, common.DeconsolidateAddressControl);
			layout.Include(ruler1, common.DischargeTerminalAddressControl);

			return layout;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static PanelLayout CreateExampleLayout2()
		{
			var common = ExampleControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(109);
			var ruler2 = layout.CreateRuler(199);
			var ruler3 = layout.CreateRuler(348);
			var ruler4 = layout.CreateRuler(522);

			layout.Include(ruler1, common.ManifestTypeDropEdit, ruler4, common.NatureDropEdit);
			layout.Include(ruler1, common.TransportModeDropEdit, ruler3, common.ContainerModeDropEdit, common.BuyersConsolidationCheckBox);
			layout.Include(ruler1, common.AgentTypeDropEdit);
			layout.Include(ruler1, common.MasterBOLTextBox);
			layout.Include(ruler1, common.VoyageFlightTextBox, ruler2, common.VesselCodeFindBox, ruler4, common.RadioCallSignTextBox, ruler1, common.VehicleRegistrationTextBox);
			layout.Include(ruler1, common.MastersNameTextBox);
			layout.Include(ruler1, common.PortOfLoadingCodeFindBox, ruler3, common.EstDepartureDateEdit);
			layout.Include(ruler1, common.PortOfFirstArrivalCodeFindBox);
			layout.Include(ruler1, common.PortOfDischargeCodeFindBox, ruler3, common.EstArrivalDateEdit);
			layout.Include(ruler1, common.CarrierAddressControl, ruler4, common.CarrierCodeTextBox);
			layout.Include(ruler1, common.ManifestNumberFromMasterBillTextBox, ruler3, common.IssueDateDateEdit);
			layout.Include(ruler1, common.CustomsOfficeDropEdit);
			layout.Include(ruler1, common.ShippingAgentAddressControl);

			layout.SetCaption<AsycudaManifestHeader>(common.MasterBOLTextBox, h => Enterprise.Customs.ASYCUDA.Gui.Res.GetData("724679C7-08B9-463B-BB65-407A494050D4", "Parent Bill"));
			layout.SetCaption<AsycudaManifestHeader>(common.VoyageFlightTextBox, h => h.VoyageFlightNoLabel, h => h.AMA_TransportModeInfo);

			layout.SetVisibility<AsycudaManifestHeader>(common.ContainerModeDropEdit, h => !h.IsAir, h => h.AMA_TransportModeInfo);
			layout.SetVisibility<AsycudaManifestHeader>(common.VoyageFlightTextBox, h => !h.IsRoad, h => h.AMA_TransportModeInfo);
			layout.SetVisibility<AsycudaManifestHeader>(common.VesselCodeFindBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			layout.SetVisibility<AsycudaManifestHeader>(common.RadioCallSignTextBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			layout.SetVisibility<AsycudaManifestHeader>(common.VehicleRegistrationTextBox, h => h.IsRoad, h => h.AMA_TransportModeInfo);

			return layout;
		}

		public static PanelLayout CreateExampleLayout3()
		{
			var common = ExampleControlBag.Instance;
			var additional = ExampleAdditionalControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);
			layout.RegisterControlBag(additional);

			var ruler1 = layout.CreateRuler(109);

			layout.Include(ruler1, common.ManifestTypeDropEdit);
			layout.Include(ruler1, additional.ManifestTypeDropEdit);
			layout.Include(ruler1, common.MasterBOLTextBox);
			layout.Include(ruler1, additional.MasterBOLTextBox);

			return layout;
		}

		public static PanelLayout CreateExampleLayout4()
		{
			var common = ExampleControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			layout.CollapseEmptyRows = true;

			int c2 = layout.AddColumn();
			var x1 = layout.CreateRuler(109);

			var shortRuler = layout.CreateRightRuler(x1.Position + 85); // date edit
			var mediumRuler = layout.CreateRightRuler(x1.Position + 116); // datetime edit
			var longRuler = layout.CreateRightRuler(x1.Position + 217); // manifest type
			var extraLongRuler = layout.CreateRightRuler(x1.Position + 292); // address

			layout.Include(x1, common.PortOfFirstArrivalCodeFindBox, longRuler);
			layout.Include(x1, common.PortOfDischargeCodeFindBox, longRuler);
			layout.Include(x1, common.ManifestTypeDropEdit, longRuler);
			layout.Include(x1, common.NatureDropEdit, longRuler);
			layout.Include(x1, common.TransportModeDropEdit, longRuler);
			layout.Include(x1, common.ContainerModeDropEdit, longRuler);
			layout.Include(x1, common.AgentTypeDropEdit, mediumRuler, common.BuyersConsolidationCheckBox, longRuler);
			layout.Include(x1, common.MasterBOLTextBox, mediumRuler);
			layout.Include(x1, common.VehicleRegistrationTextBox, longRuler);
			layout.Include(x1, common.VesselCodeFindBox, longRuler);
			layout.Include(x1, common.RadioCallSignTextBox, mediumRuler);
			layout.Include(x1, common.VoyageFlightTextBox, mediumRuler);
			layout.Include(x1, common.ConveyanceCountryCodeFindBox, longRuler);
			layout.Include(x1, common.MastersNameTextBox, mediumRuler);
			layout.Include(x1, common.PortOfLoadingCodeFindBox, longRuler);
			layout.Include(x1, common.CustomsLoadPortCodeFindBox, longRuler);
			layout.Include(c2, x1, common.EstDepartureDateEdit, shortRuler);
			layout.Include(c2, x1, common.CustomsDischargePortCodeFindBox, extraLongRuler);
			layout.Include(c2, x1, common.EstArrivalDateEdit, mediumRuler);
			layout.Include(c2, x1, common.CarrierAddressControl, extraLongRuler);
			layout.Include(c2, x1, common.CarrierCodeTextBox, mediumRuler);
			layout.Include(c2, x1, common.ManifestNumberFromMasterBillTextBox, mediumRuler);
			layout.Include(c2, x1, common.IssueDateDateEdit, shortRuler);
			layout.Include(c2, x1, common.CustomsOfficeDropEdit, extraLongRuler);
			layout.Include(c2, x1, common.ShippingAgentAddressControl, extraLongRuler);
			layout.Include(c2, x1, common.DeconsolidateAddressControl, extraLongRuler);
			layout.Include(c2, x1, common.DischargeTerminalAddressControl, extraLongRuler);

			layout.SetVisibility<AsycudaManifestHeader>(common.VehicleRegistrationTextBox, h => h.IsRoad, h => h.AMA_TransportModeInfo);

			return layout;
		}
	}
}
