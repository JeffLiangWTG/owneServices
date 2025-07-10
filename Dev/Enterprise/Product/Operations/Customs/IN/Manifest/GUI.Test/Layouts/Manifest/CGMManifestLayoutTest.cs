using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMManifestLayout))]
sealed class CGMManifestLayoutTest : LayoutsAbstractTest
{
	public void TestControlsVisiblilityDependOnTransportMode()
	{
		GetLayout().AssertControlsVisiblilityDependOnTransportMode(Header, Header, visiblilityForSea: true, visiblilityForAir: false,
			new[]
			{
				CommonBag.CustomsDischargePortCodeFindBox,
				CommonBag.CarrierReferenceTextBox,
				CommonBag.CarrierCodeTextBox,
				CommonBag.CarrierAddressControl,
				CommonBag.ShippingAgentAddressControl,
			});

		GetLayout().AssertControlsVisiblilityDependOnTransportMode(Header, Header, visiblilityForSea: false, visiblilityForAir: true,
			new[]
			{
				CommonBag.SpecialCargoCodeDropEdit,
				CommonBag.GoodsDescriptionTextBox,
				CommonBag.DestinationCodeFindBox,
				InBag.GrossWeightCalcDropEdit,
				InBag.ManifestQtyCalcDropEdit,
				InBag.ActionDropEdit,
			});
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<CGMAsycudaManifestHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBag.CountryTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.TransportModeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.ManifestTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.NatureDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.AgentTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.BuyersConsolidationCheckBox, ControlWidthClass.Long);
			yield return (CommonBag.VesselCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.VoyageFlightTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.LloydsNumberTextBox, ControlWidthClass.Long);
			yield return (CommonBag.RadioCallSignTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.CarrierReferenceTextBox, ControlWidthClass.Long);
			yield return (CommonBag.OriginCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.DestinationCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.CustomsDischargePortCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.EstDepartureDateEdit, ControlWidthClass.Auto);
			yield return (CommonBag.EstArrivalDateEdit, ControlWidthClass.Auto);
			yield return (CommonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonBag.JobReferenceTextBox, ControlWidthClass.Medium);
			yield return (InBag.MessageAndCustomsStatusWithOverrideUserControl, ControlWidthClass.LongNoCaption);
			yield return (InBag.ActionDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.SpecialCargoCodeDropEdit, ControlWidthClass.Long);
			yield return (InBag.ImportGeneralManifestNumberTextBox, ControlWidthClass.Long);
			yield return (InBag.ImportGeneralManifestDateEdit, ControlWidthClass.Auto);
			yield return (CommonBag.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			yield return (CommonBag.MasterBOLTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.IssueDateDateEdit, ControlWidthClass.Auto);
			yield return (CommonBag.GoodsDescriptionTextBox, ControlWidthClass.Long);
			yield return (CommonBag.CarrierAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.CarrierCodeTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.ShippingAgentAddressControl, ControlWidthClass.Long);
			yield return (InBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (InBag.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
		}
	}

	CommonManifestControlBag CommonBag => CommonManifestControlBag.Instance;
	CGMManifestControlBag InBag => CGMManifestControlBag.Instance;

	IPanelLayoutProvider GetLayoutProvider() => new CGMManifestLayout();
	PanelLayout GetLayout() => GetLayoutProvider().Layout;

	AsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	AsycudaManifestHeader header;
}
