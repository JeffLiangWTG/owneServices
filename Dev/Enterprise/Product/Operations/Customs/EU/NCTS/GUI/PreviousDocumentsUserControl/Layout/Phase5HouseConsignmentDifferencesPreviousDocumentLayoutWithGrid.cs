using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid()
		{
			Layout = CreateHouseConsignmentDifferencesPreviousDocumentLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout CreateHouseConsignmentDifferencesPreviousDocumentLayout()
		{
			var builder = new PreviousDocumentLayoutBuilder<Business.NctsPreviousDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.LineNoCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.StatusTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(HouseConsignmentDifferencesPreviousDocumentsGridUserControl);
	}
}
