using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

sealed class CGMManifestLayout : IPanelLayoutProvider
{
	public CGMManifestLayout()
	{
		ManifestDetails = CreateManifestDetailsLayout();
	}

	PanelLayout ManifestDetails { get; }

	PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

	PanelLayout CreateManifestDetailsLayout()
	{
		var builder = new ManifestLayoutBuilder<CGMAsycudaManifestHeader>();
		var common = builder.CommonBag;
		var inBag = CGMManifestControlBag.Instance;
		builder.AddControlBag(inBag);

		builder.AddColumn();
		builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
		builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
		builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
		builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
		builder.Add(common.BuyersConsolidationCheckBox, ControlWidthClass.Long);
		builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
		builder.Add(common.LloydsNumberTextBox, ControlWidthClass.Long);
		builder.Add(common.RadioCallSignTextBox, ControlWidthClass.Medium);
		builder.Add(common.CarrierReferenceTextBox, ControlWidthClass.Long);
		builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.DestinationCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.CustomsDischargePortCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
		builder.Add(common.Trailer1RegNoTextBox, ControlWidthClass.Medium);
		builder.Add(common.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.Trailer2RegNoTextBox, ControlWidthClass.Medium);
		builder.Add(common.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
		builder.Add(common.EstArrivalDateEdit, ControlWidthClass.Auto);
		builder.Add(common.CustomsOfficeCodeFindBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
		builder.Add(inBag.MessageAndCustomsStatusWithOverrideUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(inBag.ActionDropEdit, ControlWidthClass.Long);
		builder.Add(common.SpecialCargoCodeDropEdit, ControlWidthClass.Long);
		builder.Add(inBag.ImportGeneralManifestNumberTextBox, ControlWidthClass.Long);
		builder.Add(inBag.ImportGeneralManifestDateEdit, ControlWidthClass.Auto);
		builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
		builder.Add(common.MasterBOLTextBox, ControlWidthClass.Medium);
		builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);
		builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
		builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
		builder.Add(common.CarrierCodeTextBox, ControlWidthClass.Medium);
		builder.Add(common.ShippingAgentAddressControl, ControlWidthClass.Long);
		builder.Add(inBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(inBag.ManifestQtyCalcDropEdit, ControlWidthClass.Long);

		builder.SetVisibility(common.CustomsDischargePortCodeFindBox, x => x.IsSea, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(common.CarrierReferenceTextBox, x => x.IsSea, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(common.CarrierCodeTextBox, x => x.IsSea, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(common.CarrierAddressControl, x => x.IsSea, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(common.ShippingAgentAddressControl, x => x.IsSea, x => x.AMA_TransportModeInfo);

		builder.SetVisibility(common.SpecialCargoCodeDropEdit, x => x.IsAir, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(common.GoodsDescriptionTextBox, x => x.IsAir, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(common.DestinationCodeFindBox, x => x.IsAir, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(inBag.GrossWeightCalcDropEdit, x => x.IsAir, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(inBag.ManifestQtyCalcDropEdit, x => x.IsAir, x => x.AMA_TransportModeInfo);
		builder.SetVisibility(inBag.ActionDropEdit, x => x.IsAir, x => x.AMA_TransportModeInfo);

		return builder.Build();
	}
}
