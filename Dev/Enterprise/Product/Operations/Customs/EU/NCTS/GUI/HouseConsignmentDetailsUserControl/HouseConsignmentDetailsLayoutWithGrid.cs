using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
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
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ReferenceNumberUCRTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.TransportMoPDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.LinePriceCurrencyDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ConsignorDocAddressControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
