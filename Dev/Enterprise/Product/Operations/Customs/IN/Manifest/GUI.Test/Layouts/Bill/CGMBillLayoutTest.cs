using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AsycudaBill = Enterprise.Customs.ASYCUDA.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMBillLayout))]
sealed class CGMBillLayoutTest : LayoutsAbstractTest
{
	public void TestControlsVisiblilityDependOnTransportMode()
	{
		GetLayout().AssertControlsVisiblilityDependOnTransportMode(Header, Bill, visiblilityForSea: true, visiblilityForAir: false,
			new[]
			{
				CommonBag.BuyerAddressControl,
				CommonBag.AgentAddressControl,
				CommonBag.CarrierReferenceTextBox,
				CommonBag.CargoStatusDropEdit,
				CommonBag.ContainerModeDropEdit,
				InBag.FinalDestinationDetailsUserControl,
			});

		GetLayout().AssertControlsVisiblilityDependOnTransportMode(Header, Bill, visiblilityForSea: false, visiblilityForAir: true,
			new[]
			{
				CommonBag.OriginCodeFindBox,
				CommonBag.FinalDestinationCodeFindBox,
			});
	}

	public void TestBondDetailsUserControlVisibility()
	{
		var layout = GetLayout();
		var bondDetailsUsserControl = InBag.BondDetailsUserControl;
		CombineAssertions(() =>
		{
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.C;
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("BondDetailsUserControl - AIR CGM", false, layout.IsVisible(bondDetailsUsserControl, Bill));

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("BondDetailsUserControl - SEA CGM", true, layout.IsVisible(bondDetailsUsserControl, Bill));

			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.LB;
			AssertEquals("BondDetailsUserControl - IsContainerModeCorCP() false", false, layout.IsVisible(bondDetailsUsserControl, Bill));
		});
	}
	public void TestTransshipmentDetailsUserControlVisibility()
	{
		var layout = GetLayout();
		var transshipmentDetailsUserControl = InBag.TransshipmentDetailsUserControl;
		CombineAssertions(() =>
		{
			Bill.ABL_CargoStatus = CargoMovementList.Codes.TranshipmentCargo;
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("TransshipmentDetailsUserControl - AIR CGM", false, layout.IsVisible(transshipmentDetailsUserControl, Bill));

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("TransshipmentDetailsUserControl - SEA CGM", true, layout.IsVisible(transshipmentDetailsUserControl, Bill));

			Bill.ABL_CargoStatus = CargoMovementList.Codes.LocalCargo;
			AssertEquals("TransshipmentDetailsUserControl - IsTranshipment false", false, layout.IsVisible(transshipmentDetailsUserControl, Bill));
		});
	}

	public void TestLocalTransportCarrierOrganisationFindBoxCaptions()
	{
		var layout = GetLayout();
		var transshipmentDetailsUserControl = InBag.TransshipmentDetailsUserControl;
		CombineAssertions(() =>
		{
			AssertLocalTransportCarrierOrganisationFindBoxCaptions(ModeOfTransportList.Codes.Train, "Rail Opt.");
			AssertLocalTransportCarrierOrganisationFindBoxCaptions(ModeOfTransportList.Codes.Road, "Transporter");
			AssertLocalTransportCarrierOrganisationFindBoxCaptions(ModeOfTransportList.Codes.Ship, "Carrier");
		});
		void AssertLocalTransportCarrierOrganisationFindBoxCaptions(string inlandTransportMode, string expectCaption)
		{
			Bill.ABL_InlandTransportMode = inlandTransportMode;
			AssertEquals($"SetCaptions for transshipmentDetailsUserControl - {inlandTransportMode}", true, layout.TryGetCaptionData(transshipmentDetailsUserControl, Bill, out var captionsData));
			var captionData = captionsData[nameof(CGMTransshipmentDetailsUserControl.LocalTransportCarrierOrganisationFindBox)];
			AssertEquals(expectCaption, captionData.Caption);
		}
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBag.BillNumberTextBox, ControlWidthClass.Long);
			yield return (CommonBag.CarrierReferenceTextBox, ControlWidthClass.Long);
			yield return (CommonBag.OriginCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			yield return (InBag.FinalDestinationDetailsUserControl, ControlWidthClass.LongNoCaption);
			yield return (CommonBag.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.VolumeCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.SpecialCargoCodesDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			yield return (InBag.BondDetailsUserControl, ControlWidthClass.LongNoCaption);
			yield return (CommonBag.CargoStatusDropEdit, ControlWidthClass.Long);
			yield return (InBag.TransshipmentDetailsUserControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonBag.BillIssueDateEdit, ControlWidthClass.Auto);
			yield return (CommonBag.GoodsDescriptionTextBox, ControlWidthClass.Long);
			yield return (CommonBag.MarksAndNumbersTextBox, ControlWidthClass.Long);
			yield return (CommonBag.RemarksTextBox, ControlWidthClass.Long);
			yield return (CommonBag.ShipperAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.BuyerAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.ConsigneeAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.AgentAddressControl, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (CommonBag.PrepaidCollectDropEdit, ControlWidthClass.Long);
		}
	}

	CommonBillControlBag CommonBag => CommonBillControlBag.Instance;

	CGMBillControlBag InBag => CGMBillControlBag.Instance;

	IPanelLayoutProvider GetLayoutProvider() => new CGMBillLayout();
	PanelLayout GetLayout() => GetLayoutProvider().Layout;

	AsycudaBill Bill => bill ??= Header.Bills.AddNew();
	AsycudaBill bill;

	AsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	AsycudaManifestHeader header;
}
