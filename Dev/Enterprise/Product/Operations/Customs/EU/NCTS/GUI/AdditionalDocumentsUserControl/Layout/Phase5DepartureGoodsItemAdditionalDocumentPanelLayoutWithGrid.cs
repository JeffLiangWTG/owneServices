using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid()
		{
			Layout = CreateGoodsItemAdditionalDocumentLayout();
		}

		public PanelLayout Layout { get; }

		static PanelLayout CreateGoodsItemAdditionalDocumentLayout()
		{
			var builder = new AdditionalDocumentLayoutBuilder<NctsAdditionalInfo>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.AddColumn();
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionMultilineTextBox, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.ReferenceNumberTextBox, x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference || x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument, x => x.CSI_SubTypeInfo);
			builder.SetVisibility(commonBag.DescriptionMultilineTextBox, x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation, x => x.CSI_SubTypeInfo);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(DepartureGoodsItemAdditionalDocumentsGridUserControl);
	}
}
