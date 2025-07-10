using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	sealed class UCC6AndExportSupportingDocumentFieldsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public UCC6AndExportSupportingDocumentFieldsLayout()
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
			builder.SetVisibility(euBag.ReferenceNumberCodeFindBox, doc => doc.ShowCodeFindBoxForReferenceNumber, doc => doc.CSI_CodeInfo);
			builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			builder.SetVisibility(euBag.ReferenceNumberTextBox, doc => doc.ShowTextBoxForReferenceNumber, doc => doc.CSI_CodeInfo);
			builder.Add(euBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto);
			builder.Add(euBag.DocumentLineNoCalcEdit, ControlWidthClass.Auto);
			builder.AddColumn();
			builder.Add(euBag.QuantityCalcEdit, ControlWidthClass.Auto);
			builder.Add(euBag.UnitOfQuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.ValueCalcEdit, ControlWidthClass.Auto);
			builder.Add(euBag.CurrencyCodeFindBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
