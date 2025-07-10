using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentDifferencesLayout : IPanelLayoutWithGridProvider
	{
		public HouseConsignmentDifferencesLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(HouseConsignmentDifferencesGridUserControl);

		static PanelLayout CreateLayout()
		{
			var builder = new HouseConsignmentDifferencesLayoutBuilder<NctsBill>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SequenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.SecurityCheckBox, ControlWidthClass.Auto);
			builder.Add(commonBag.HouseConsignmentTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.UnloadedStateDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.DeclaredValueLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);

			builder.Add(commonBag.ArrivalTransportInfosUserControl, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.UnloadedValueLabel, ControlWidthClass.Auto, commonBag.DeclaredValueLabel);
			builder.Add(commonBag.GrossWeightUnloadedCalcDropEdit, ControlWidthClass.Auto, commonBag.GrossWeightCalcDropEdit);

			builder.AddColumn();
			builder.Add(commonBag.ConsignorDocAddressControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.LongControl);

			builder.SetVisibility(commonBag.UnloadedValueLabel, x => x.HasDifference, x => x.MovementDetail.B9_UnloadedStateInfo);
			builder.SetVisibility(commonBag.GrossWeightUnloadedCalcDropEdit, x => x.HasDifference, x => x.MovementDetail.B9_UnloadedStateInfo);

			builder.SetCaption(commonBag.DeclaredValueLabel, GetStatusLabelCaption, d => d.MovementDetail.B9_UnloadedStateInfo);

			return builder.Build();
		}

		public static ResourceStringData GetStatusLabelCaption(NctsBill bill)
		{
			switch (bill.MovementDetail.B9_UnloadedState)
			{
				case NctsUnloadedStateList.Codes.NEW:
					return Res.GetData("F40F278B-4484-4E7A-91DE-8D6EB6D3CD33", "New Value");
				default:
					return Res.GetData("05F3B03C-486A-4BB0-BA75-74C66D7EEB66", "Declared Value");
			}
		}
	}
}
