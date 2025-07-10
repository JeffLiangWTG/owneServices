using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid()
		{
			Layout = CreateHouseConsignmentDifferencesSupportingDocumentLayout();
		}

		PanelLayout Layout { get; }

		static PanelLayout CreateHouseConsignmentDifferencesSupportingDocumentLayout()
		{
			var builder = new SupportingDocumentLayoutBuilder<NctsSupportingDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.LineNoCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.StatusLabel, ControlWidthClass.Long);
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Long);

			builder.SetCaption(commonBag.StatusLabel, GetStatusLabelCaption, d => d.CSI_StatusInfo);

			return builder.Build();
		}

		static ResourceStringData GetStatusLabelCaption(NctsSupportingDocument document)
		{
			switch (document.CSI_Status)
			{
				case SupportingDocumentStatusList.Codes.DEC:
					return Res.GetData("46FDE316-AB2C-46B6-A6DD-A67E0A22908D", "Declared Value");
				case SupportingDocumentStatusList.Codes.NEW:
					return Res.GetData("9062CCB7-386D-4001-BD8C-E5FC57E76FBF", "New Value");
				default:
					return Res.GetData("A666AB6B-1CF4-46F9-81C1-573AF4244F45", "Value");
			}
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(HouseConsignmentDifferencesSupportingDocumentGridUserControl);
	}
}
