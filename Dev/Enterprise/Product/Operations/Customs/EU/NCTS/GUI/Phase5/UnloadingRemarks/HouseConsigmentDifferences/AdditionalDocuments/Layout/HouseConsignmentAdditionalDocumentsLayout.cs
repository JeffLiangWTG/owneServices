using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentAdditionalDocumentsLayout : IPanelLayoutWithGridProvider
	{
		public HouseConsignmentAdditionalDocumentsLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		public Type GridUserControlType => typeof(HouseConsignmentAdditionalDocumentsOverviewUserControl);

		PanelLayout CreateLayout()
		{
			var builder = new HouseConsignmentAdditionalDocumentsLayoutBuilder<NctsBillAdditionalDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SequenceNumberTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.StatusLabel, ControlWidthClass.Long);
			builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DocTypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.TextTextBox, ControlWidthClass.Long);

			builder.SetCaption(commonBag.StatusLabel, GetStatusLabelCaption, d => d.CSI_StatusInfo);

			return builder.Build();
		}

		ResourceStringData GetStatusLabelCaption(NctsBillAdditionalDocument document)
		{
			switch (document.CSI_Status)
			{
				case NctsUnloadedStateList.Codes.DEC:
					return Res.GetData("46FDE316-AB2C-46B6-A6DD-A67E0A22908D", "Declared Value");
				case NctsUnloadedStateList.Codes.NEW:
					return Res.GetData("9062CCB7-386D-4001-BD8C-E5FC57E76FBF", "New Value");
				default:
					return new ResourceStringData();
			}
		}
	}
}
