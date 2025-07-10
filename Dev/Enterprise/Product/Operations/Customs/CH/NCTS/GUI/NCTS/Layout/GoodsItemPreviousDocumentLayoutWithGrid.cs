using System;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class GoodsItemPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public GoodsItemPreviousDocumentLayoutWithGrid()
	{
		Layout = CreateGoodsItemPreviousDocumentLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout CreateGoodsItemPreviousDocumentLayout()
	{
		var builder = new EU.NCTS.GUI.PreviousDocumentLayoutBuilder<NctsPreviousDocument>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Long);

		return builder.Build();
	}

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(EU.NCTS.GUI.GoodsItemPreviousDocumentsGridUserControl);
}
