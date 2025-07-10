using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemSupportingDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5GoodsItemSupportingDocumentLayoutWithGrid()
		{
			Layout = CreateGoodsItemSupportingDocumentLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout CreateGoodsItemSupportingDocumentLayout()
		{
			var builder = new SupportingDocumentLayoutBuilder<Business.NctsSupportingDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(GoodsItemSupportingDocumentsGridUserControl);
	}
}
