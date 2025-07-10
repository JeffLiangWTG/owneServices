using System;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class Phase5GoodsItemPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5GoodsItemPreviousDocumentLayoutWithGrid()
		{
			Layout = CreateGoodsItemPreviousDocumentLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateGoodsItemPreviousDocumentLayout()
		{
			var builder = new PreviousDocumentLayoutBuilder<Business.NCTS.NctsPreviousDocument>();
			var commonBag = builder.CommonBag;
			var euBag = EU.NCTS.GUI.PreviousDocumentControlBag.Instance;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(euBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
			builder.Add(euBag.NumOfPackagesDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.QuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.ComplementTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(GoodsItemPreviousDocumentsGridUserControl);
	}
}
