using System;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public class GoodsItemPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public GoodsItemPreviousDocumentLayoutWithGrid()
		{
			Layout = CreateGoodsItemPreviousDocumentLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateGoodsItemPreviousDocumentLayout()
		{
			var builder = new PreviousDocumentLayoutBuilder<Business.NctsPreviousDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Auto);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(GoodsItemPreviousDocumentsGridUserControl);
	}
}
