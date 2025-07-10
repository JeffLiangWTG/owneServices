using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	sealed class NonUCC6SupportingDocumentFieldsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public NonUCC6SupportingDocumentFieldsLayout()
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
			builder.Add(euBag.StatusDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.DateOfIssueDateEdit, ControlWidthClass.Auto);
			builder.Add(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto);
			builder.AddColumn();
			builder.Add(euBag.QuantityCalcEdit, ControlWidthClass.Auto);
			builder.Add(euBag.UnitOfQuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.Quantity2CalcEdit, ControlWidthClass.Auto);
			builder.Add(euBag.UnitOfQuantity2TextBox, ControlWidthClass.Auto);
			builder.Add(euBag.ValueCalcEdit, ControlWidthClass.Auto);
			builder.Add(euBag.CurrencyCodeFindBox, ControlWidthClass.Auto);

			builder.SetVisibility(euBag.ReferenceNumberCodeFindBox, doc => doc.ShowCodeFindBoxForReferenceNumber, doc => doc.CSI_CodeInfo);
			builder.SetVisibility(euBag.ReferenceNumberTextBox, doc => doc.ShowTextBoxForReferenceNumber, doc => doc.CSI_CodeInfo);

			return builder.Build();
		}
	}
}
