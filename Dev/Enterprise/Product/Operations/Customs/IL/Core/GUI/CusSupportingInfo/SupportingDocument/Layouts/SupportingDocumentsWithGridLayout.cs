using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public class SupportingDocumentsWithGridLayout : IPanelLayoutWithGridProvider
	{
		public SupportingDocumentsWithGridLayout()
		{
			supportingDocumentDetailLayout = CreateSupportingDocumentDetailLayout();
		}

		public Type GridUserControlType => typeof(SupportingDocumentGridControl);

		public PanelLayout Layout => supportingDocumentDetailLayout;

		PanelLayout CreateSupportingDocumentDetailLayout()
		{
			var builder = new SupportingDocumentDetailsBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.EDocGuidDropEditGuidDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.StatusTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.AdditionalDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsDocIDTextBox, ControlWidthClass.Long);
			return builder.Build();
		}

		readonly PanelLayout supportingDocumentDetailLayout;
	}
}
