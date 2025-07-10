using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentPreviousDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public HouseConsignmentPreviousDocumentLayoutWithGrid()
		{
			Layout = CreateLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateLayout()
		{
			var builder = new PreviousDocumentLayoutBuilder<Business.CommonPreviousDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(HouseConsignmentPreviousDocumentsGridUserControl);

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
