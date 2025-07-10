using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CO.Manifest.GUI
{
	public class COManifestLayouts : IPanelLayoutProvider
	{
		public PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		public COManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			AddControls();
			SetVisibilities();

			return Builder.Build();
		}

		void AddControls()
		{
			var common = Builder.CommonBag;
			var co = COManifestControlBag.Instance;
			Builder.AddControlBag(co);

			Builder.AddColumn();
			builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
			builder.Add(common.RegistrationDateEdit, ControlWidthClass.Medium);
			builder.Add(common.RegistrationNumberTextBox, ControlWidthClass.Long);
			Builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			Builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
			Builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			Builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
			Builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
			Builder.Add(common.BuyersConsolidationCheckBox, ControlWidthClass.Long);
			Builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			Builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			Builder.Add(common.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			Builder.Add(common.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			Builder.Add(common.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
			Builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.EstArrivalDateEdit, ControlWidthClass.Auto);
			Builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);

			Builder.AddColumn();
			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			Builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			Builder.Add(common.MasterBOLTextBox, ControlWidthClass.Medium);
			Builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);
			Builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			Builder.Add(common.ShippingAgentAddressControl, ControlWidthClass.Long);
			Builder.Add(co.TravelDocumentTypeDropEdit, ControlWidthClass.Long);
			Builder.Add(co.CargoDispositionDropEdit, ControlWidthClass.Long);
			Builder.Add(co.DeliveryModeDropEdit, ControlWidthClass.Long);
			builder.Add(co.MultimodalCheckBox, ControlWidthClass.Long);
			builder.Add(co.PrecursorsCheckBox, ControlWidthClass.Long);
			builder.Add(co.CarriersLiabilityCheckBox, ControlWidthClass.Long);
		}

		void SetVisibilities()
		{
			var common = Builder.CommonBag;
			var co = COManifestControlBag.Instance;

			Builder.SetVisibility(co.CargoDispositionDropEdit, b => b.IsSea, b => b?.AMA_TransportModeInfo);
			Builder.SetVisibility(co.TravelDocumentTypeDropEdit, b => b.IsSea, b => b?.AMA_TransportModeInfo);
			Builder.SetVisibility(co.MultimodalCheckBox, b => b.IsSea, b => b?.AMA_TransportModeInfo);
			Builder.SetVisibility(co.PrecursorsCheckBox, b => b.IsSea, b => b?.AMA_TransportModeInfo);
			Builder.SetVisibility(co.CarriersLiabilityCheckBox, b => b.IsSea, b => b?.AMA_TransportModeInfo);
			Builder.SetVisibility(co.DeliveryModeDropEdit, b => b.IsSea, b => b?.AMA_TransportModeInfo);
		}

		ManifestLayoutBuilder<AsycudaManifestHeader> Builder => builder ?? (builder = new ManifestLayoutBuilder<AsycudaManifestHeader>());
		ManifestLayoutBuilder<AsycudaManifestHeader> builder;
	}
}
