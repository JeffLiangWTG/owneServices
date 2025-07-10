using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentAdditionalDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public HouseConsignmentAdditionalDocumentLayoutWithGrid()
		{
			Layout = CreateLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateLayout()
		{
			var builder = new AdditionalDocumentLayoutBuilder<Business.NctsBillAdditionalDocument>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(HouseConsignmentAdditionalDocumentGridUserControl);

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
