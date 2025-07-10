using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	[CodeAlive("We will be needing this in the next workflow")]
	public sealed class Phase5HouseConsignmentDifferencesAdditionalDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5HouseConsignmentDifferencesAdditionalDocumentLayoutWithGrid()
		{
			Layout = CreateGoodsItemAdditionalDocumentLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateGoodsItemAdditionalDocumentLayout()
		{
			var builder = new AdditionalDocumentLayoutBuilder<NctsAdditionalInfo>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.LineNoCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.StatusLabel, ControlWidthClass.Long);
			builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.ReferenceNumberTextBox, x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference, x => x.CSI_SubTypeInfo);
			builder.SetVisibility(commonBag.DescriptionTextBox, x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation, x => x.CSI_SubTypeInfo);

			builder.SetCaption(commonBag.StatusLabel, GetStatusLabelCaption, d => d.CSI_StatusInfo);

			return builder.Build();
		}

		ResourceStringData GetStatusLabelCaption(NctsAdditionalInfo info)
		{
			switch (info.CSI_Status)
			{
				case NctsUnloadedStateList.Codes.DEC:
					return Res.GetData("46FDE316-AB2C-46B6-A6DD-A67E0A22908D", "Declared Value");
				case NctsUnloadedStateList.Codes.NEW:
					return Res.GetData("9062CCB7-386D-4001-BD8C-E5FC57E76FBF", "New Value");
				default:
					return new ResourceStringData();
			}
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(Phase5HouseConsignmentDifferencesAdditionalDocumentGridUserControl);
	}
}
