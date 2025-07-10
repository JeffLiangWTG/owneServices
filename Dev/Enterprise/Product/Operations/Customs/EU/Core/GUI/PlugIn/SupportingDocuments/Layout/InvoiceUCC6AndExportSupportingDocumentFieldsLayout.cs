using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	sealed class InvoiceUCC6AndExportSupportingDocumentFieldsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public InvoiceUCC6AndExportSupportingDocumentFieldsLayout()
		{
			Layout = CreateInvoiceDetailsLayout();
		}

		static PanelLayout CreateInvoiceDetailsLayout()
		{
			var builder = new SupportingDocumentFieldsLayoutBuilder<Business.Declaration.MultiLineAddInfos.SupportingDocument>();

			var euBag = SupportingDocumentFieldsControlBag.Instance;

			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(euBag.CodeCodeFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.ReferenceNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto);
			builder.Add(euBag.DocumentLineNoCalcEdit, ControlWidthClass.Auto);

			builder.SetVisibility(euBag.ReferenceNumberCodeFindBox, doc => doc.ShowCodeFindBoxForReferenceNumber, doc => doc.CSI_CodeInfo);
			builder.SetVisibility(euBag.ReferenceNumberTextBox, doc => doc.ShowTextBoxForReferenceNumber, doc => doc.CSI_CodeInfo);

			return builder.Build();
		}
	}
}
