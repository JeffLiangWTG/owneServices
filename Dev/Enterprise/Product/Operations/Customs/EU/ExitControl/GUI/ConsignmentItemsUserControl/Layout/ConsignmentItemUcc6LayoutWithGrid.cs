using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ConsignmentItemUcc6LayoutWithGrid : IPanelLayoutWithGridProvider
{
	public ConsignmentItemUcc6LayoutWithGrid()
	{
		Layout = CreateConsignmentItemsLayoutWithGrid();
	}

	PanelLayout Layout { get; }

	PanelLayout CreateConsignmentItemsLayoutWithGrid()
	{
		var builder = new ConsignmentItemLayoutBuilder<Business.CusExitConsignmentItem>();
		var commonBag = builder.CommonBag;
		var euBag = ConsignmentItemControlBag.Instance;

		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(commonBag.ConsignmentItemPackingDetailsUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.AdditionalDocumentsLabel, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ReportAdditionalDocumentsGridUserControl, ControlWidthClass.LongNoCaption);

		return builder.Build();
	}

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(ConsignmentItemsGridUserControl);
}
