using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid()
		{
			Layout = CreateGoodsItemAdditionalDocumentLayout();
		}

		public PanelLayout Layout { get; }

		static PanelLayout CreateGoodsItemAdditionalDocumentLayout()
		{
			var builder = new AdditionalDocumentLayoutBuilder<NctsAdditionalInfo>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.LineNoCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.StatusLabel, ControlWidthClass.Long);
			builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.AddColumn();
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionMultilineTextBox, ControlWidthClass.Long);

			builder.SetCaption(commonBag.StatusLabel, GetStatusLabelCaption, d => d.CSI_StatusInfo);
			builder.SetVisibility(commonBag.LineNoCalcEdit, x => x.IsPhase5Arrival);
			builder.SetVisibility(commonBag.StatusLabel, x => x.IsPhase5Arrival && (x.CSI_Status == NctsUnloadedStateList.Codes.DEC || x.CSI_Status == NctsUnloadedStateList.Codes.NEW), d => d.CSI_StatusInfo);
			builder.SetVisibility(commonBag.ReferenceNumberTextBox, x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference || x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument, x => x.CSI_SubTypeInfo);
			builder.SetVisibility(commonBag.DescriptionMultilineTextBox, x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation, x => x.CSI_SubTypeInfo);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(ArrivalGoodsItemAdditionalDocumentsGridUserControl);

		static ResourceStringData GetStatusLabelCaption(NctsAdditionalInfo info)
		{
			switch (info.CSI_Status)
			{
				case NctsUnloadedStateList.Codes.DEC:
					return Res.GetData("F3DA8F8E-BD9A-41FA-B467-FA8D81A9F319", "Declared Value");
				case NctsUnloadedStateList.Codes.NEW:
					return Res.GetData("B7D8913C-2DB4-4691-BF64-E15A391B55BB", "New Value");
				default:
					return new ResourceStringData();
			}
		}
	}
}
