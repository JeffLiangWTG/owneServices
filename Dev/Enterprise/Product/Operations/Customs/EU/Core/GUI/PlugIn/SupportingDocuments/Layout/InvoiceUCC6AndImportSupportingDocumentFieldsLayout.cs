using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	sealed class InvoiceUCC6AndImportSupportingDocumentFieldsLayout : IPanelLayoutProvider
	{
		JobDeclaration JobDeclaration { get; }

		public PanelLayout Layout { get; }

		public InvoiceUCC6AndImportSupportingDocumentFieldsLayout(JobDeclaration jobDeclaration)
		{
			JobDeclaration = jobDeclaration;
			Layout = CreateInvoiceDetailsLayout();
		}

		PanelLayout CreateInvoiceDetailsLayout()
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
			if (JobDeclaration is JobDeclaration declaration)
			{
				var configuration = declaration.Configuration;
				if (configuration.UseEucdmSupportingDocumentGoodsShipment)
				{
					builder.SetVisibility(euBag.QuantityCalcEdit, doc => false);
					builder.SetVisibility(euBag.UnitOfQuantityDropEdit, doc => false);
					builder.SetVisibility(euBag.Quantity2CalcEdit, doc => false);
					builder.SetVisibility(euBag.UnitOfQuantity2TextBox, doc => false);
					builder.SetVisibility(euBag.ValueCalcEdit, doc => false);
					builder.SetVisibility(euBag.CurrencyCodeFindBox, doc => false);
				}

				if (configuration.UseEucdmSupportingDocumentGoodsShipmentAndItem)
				{
					builder.SetVisibility(euBag.StatusDropEdit, doc => false);
					builder.SetVisibility(euBag.DateOfIssueDateEdit, doc => false);
				}
			}
			return builder.Build();
		}
	}
}
