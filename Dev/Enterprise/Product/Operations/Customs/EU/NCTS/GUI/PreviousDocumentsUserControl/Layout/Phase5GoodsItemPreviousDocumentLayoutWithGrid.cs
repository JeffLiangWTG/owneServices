using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5GoodsItemPreviousDocumentLayoutWithGrid()
		{
			Layout = CreateGoodsItemPreviousDocumentLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout CreateGoodsItemPreviousDocumentLayout()
		{
			var builder = new PreviousDocumentLayoutBuilder<Business.NctsPreviousDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NumOfPackagesDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.QuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(GoodsItemPreviousDocumentsGridUserControl);
	}
}
