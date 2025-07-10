using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

sealed class CGMBillLayout : IPanelLayoutProvider
{
	public CGMBillLayout()
	{
		BillDetails = CreateBillLayout();
	}

	PanelLayout BillDetails { get; }

	PanelLayout IPanelLayoutProvider.Layout => BillDetails;

	PanelLayout CreateBillLayout()
	{
		var builder = new BillLayoutBuilder<CGMAsycudaBill>();
		var common = builder.CommonBag;
		var inBag = CGMBillControlBag.Instance;
		builder.AddControlBag(inBag);

		builder.AddColumn();
		builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
		builder.Add(common.CarrierReferenceTextBox, ControlWidthClass.Long);
		builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
		builder.Add(inBag.FinalDestinationDetailsUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.SpecialCargoCodesDropEdit, ControlWidthClass.Long);
		builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
		builder.Add(inBag.BondDetailsUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(common.CargoStatusDropEdit, ControlWidthClass.Long);
		builder.Add(inBag.TransshipmentDetailsUserControl, ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(common.BillIssueDateEdit, ControlWidthClass.Auto);
		builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
		builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
		builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
		builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
		builder.Add(common.BuyerAddressControl, ControlWidthClass.Long);
		builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
		builder.Add(common.AgentAddressControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);

		builder.SetVisibility(common.BuyerAddressControl, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.AgentAddressControl, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.CarrierReferenceTextBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.CargoStatusDropEdit, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.ContainerModeDropEdit, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(inBag.BondDetailsUserControl, x => x.IsSea && x.IsContainerModeCorCP(), x => x.ABL_ContainerModeInfo, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(inBag.FinalDestinationDetailsUserControl, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(inBag.TransshipmentDetailsUserControl, x => x.IsSea && x.IsTranshipment, x => x.ABL_CargoStatusInfo, x => x.Header?.AMA_TransportModeInfo);

		builder.SetVisibility(common.OriginCodeFindBox, x => x.IsAir, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.FinalDestinationCodeFindBox, x => x.IsAir, x => x.Header?.AMA_TransportModeInfo);

		builder.AddControlBehaviour<CGMFinalDestinationDetailsUserControl>(inBag.FinalDestinationDetailsUserControl,
			(control, bill) => control.ChangeVisibility(bill), x => x.ABL_LocationInformationInfo);
		builder.SetCaptions(inBag.TransshipmentDetailsUserControl, x => new Dictionary<string, ResourceStringData>
		{
			{ nameof(CGMTransshipmentDetailsUserControl.LocalTransportCarrierOrganisationFindBox), x.LocalTransportCarrierCaption },
		}, x => x.ABL_InlandTransportModeInfo);

		return builder.Build();
	}
}
