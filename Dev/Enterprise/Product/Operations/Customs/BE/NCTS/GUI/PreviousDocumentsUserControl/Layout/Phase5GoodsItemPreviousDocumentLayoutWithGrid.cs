using System;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.NCTS.GUI;

public class Phase5GoodsItemPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public Phase5GoodsItemPreviousDocumentLayoutWithGrid()
	{
		Layout = CreateGoodsItemPreviousDocumentLayout();
	}

	PanelLayout Layout { get; }

	static PanelLayout CreateGoodsItemPreviousDocumentLayout()
	{
		var builder = new PreviousDocumentLayoutBuilder<NctsPreviousDocument>();
		var commonBag = builder.CommonBag;
		var euBag = EU.NCTS.GUI.PreviousDocumentControlBag.Instance;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.TypeCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.ReferenceNumberN785UserControl, ControlWidthClass.Long);
		builder.Add(euBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
		builder.Add(euBag.NumOfPackagesDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.QuantityDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.ComplementTextBox, ControlWidthClass.Long);

		return builder.Build();
	}

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(GoodsItemPreviousDocumentsGridUserControl);
}
