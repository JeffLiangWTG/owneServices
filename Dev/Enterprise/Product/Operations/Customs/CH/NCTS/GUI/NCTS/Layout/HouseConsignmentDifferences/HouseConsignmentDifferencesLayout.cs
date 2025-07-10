using System;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class HouseConsignmentDifferencesLayout : IPanelLayoutWithGridProvider
{
	public HouseConsignmentDifferencesLayout()
	{
		HouseConsignmentDifferencesColumn = CreateHouseConsignmentDifferencesColumn();
	}

	public PanelLayout Layout => HouseConsignmentDifferencesColumn;

	PanelLayout HouseConsignmentDifferencesColumn { get; }

	public Type GridUserControlType => typeof(HouseConsignmentDifferencesGridUserControl);

	PanelLayout CreateHouseConsignmentDifferencesColumn()
	{
		var builder = new HouseConsignmentDifferencesLayoutBuilder<NctsBill>();
		var commonBag = builder.CommonBag;
		builder.AddControlBag(commonBag);
		var chBag = HouseConsignmentDifferencesControlBag.Instance;
		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(commonBag.SequenceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.SecurityCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.HouseConsignmentTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.UnloadedStateDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.UnloadingRemarkCodeDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.UnloadingRemarkTextTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclaredValueLabel, ControlWidthClass.Long);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ArrivalTransportInfosUserControl, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.UnloadedValueLabel, ControlWidthClass.Long, commonBag.DeclaredValueLabel);
		builder.Add(commonBag.GrossWeightUnloadedCalcDropEdit, ControlWidthClass.Long, commonBag.GrossWeightCalcDropEdit);

		builder.AddColumn();
		builder.Add(commonBag.ConsignorDocAddressControl, ControlWidthClass.LongControl);
		builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.LongControl);

		builder.SetVisibility(commonBag.UnloadedValueLabel, x => x.HasDifference, x => x.MovementDetail.B9_UnloadedStateInfo);
		builder.SetVisibility(commonBag.GrossWeightUnloadedCalcDropEdit, x => x.HasDifference, x => x.MovementDetail.B9_UnloadedStateInfo);

		builder.SetCaption(commonBag.DeclaredValueLabel, EU.NCTS.GUI.HouseConsignmentDifferencesLayout.GetStatusLabelCaption, d => d.MovementDetail.B9_UnloadedStateInfo);

		return builder.Build();
	}
}
