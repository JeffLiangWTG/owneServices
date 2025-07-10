using System;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class HouseConsignmentDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public HouseConsignmentDetailsLayoutWithGrid()
	{
		Layout = CreateHouseConsignmentDetailsLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(HouseConsignmentsGridUserControl);

	PanelLayout CreateHouseConsignmentDetailsLayout()
	{
		var builder = new HouseConsignmentDetailsLayoutBuilder<Business.NctsBill>();
		var itBag = builder.CommonBag;
		var euBag = EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.CustomsStatusUserControl, ControlWidthClass.Long);
		builder.Add(euBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceNumberUCRTextBox, ControlWidthClass.Long);
		builder.Add(euBag.TransportMoPDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(euBag.ConsignorDocAddressControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(euBag.ConsigneeDocAddressControl, ControlWidthClass.Long);

		builder.SetVisibility(euBag.CountryOfDestinationDropEdit, x => !x.IsInPhase5TransitionPeriod);
		builder.SetVisibility(euBag.TransportMoPDropEdit, x => !x.IsInPhase5TransitionPeriod);

		return builder.Build();
	}
}
