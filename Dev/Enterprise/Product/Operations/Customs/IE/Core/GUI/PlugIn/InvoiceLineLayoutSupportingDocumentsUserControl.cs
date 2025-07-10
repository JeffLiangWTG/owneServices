using System.Collections.Generic;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class InvoiceLineLayoutSupportingDocumentsUserControl : EU.GUI.PlugIn.InvoiceLineLayoutSupportingDocumentsUserControl
	{
		public InvoiceLineLayoutSupportingDocumentsUserControl()
		{
			InitializeComponent();
		}

		public new JobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set => base.JobDeclaration = value;
		}

		protected override EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration) => new InvoiceLineLayoutSupportingDocumentsFieldsControl(declaration);

		protected override IReadOnlyList<string> AvailableColumnNames => JobDeclaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport ? UCC5AndImportAvailableColumnNames : base.AvailableColumnNames;

		protected IReadOnlyList<string> UCC5AndImportAvailableColumnNames => new[]
		{
			nameof(SupportingDocument.CSI_Code),
			nameof(SupportingDocument.CSI_CodeDescription),
			nameof(SupportingDocument.CSI_ReferenceNumber),
			nameof(SupportingDocument.CSI_AdditionalDescription),
			nameof(SupportingDocument.CSI_ReferenceNumber2),
			nameof(SupportingDocument.CSI_Quantity),
			nameof(SupportingDocument.CSI_UnitOfQuantity),
			nameof(SupportingDocument.CSI_Value),
			nameof(SupportingDocument.CSI_RX_NKCurrency),
			nameof(SupportingDocument.CSI_DateOfExpiry),
		};
	}
}
