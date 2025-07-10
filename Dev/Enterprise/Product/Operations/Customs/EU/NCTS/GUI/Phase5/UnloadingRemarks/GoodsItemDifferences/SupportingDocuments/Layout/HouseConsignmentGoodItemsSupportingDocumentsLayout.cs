using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentGoodItemsSupportingDocumentsLayout : IPanelLayoutWithGridProvider
	{
		public HouseConsignmentGoodItemsSupportingDocumentsLayout()
		{
			Layout = CreateLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		public Type GridUserControlType => typeof(HouseConsignmentGoodItemsSupportingDocumentsGridUserControl);

		static PanelLayout CreateLayout()
		{
			var builder = new HouseConsignmentGoodItemsSupportingDocumentsLayoutBuilder<NctsSupportingDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SequenceNumberTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.StatusLabel, ControlWidthClass.Long);
			builder.Add(commonBag.DocTypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ComplementInfoTextBox, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.StatusLabel, x => x.IsPhase5Arrival && (x.CSI_Status == NctsUnloadedStateList.Codes.DEC || x.CSI_Status == NctsUnloadedStateList.Codes.NEW), d => d.CSI_StatusInfo);

			builder.SetCaption(commonBag.StatusLabel, GetStatusLabelCaption, d => d.CSI_StatusInfo);

			return builder.Build();
		}

		static ResourceStringData GetStatusLabelCaption(NctsSupportingDocument supportingDocument)
		{
			switch (supportingDocument.CSI_Status)
			{
				case NctsUnloadedStateList.Codes.DEC:
					return Res.GetData("DE7C437A-7B05-4735-BEB9-8ED627597EA1", "Declared Value");
				case NctsUnloadedStateList.Codes.NEW:
					return Res.GetData("B4083AFB-9E98-49C7-928B-AA42152A558D", "New Value");
				default:
					return new ResourceStringData();
			}
		}
	}
}
