using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class InvoiceLayoutSupportingDocumentsFieldsControl : LayoutSupportingDocumentsFieldsControl
	{
		public InvoiceLayoutSupportingDocumentsFieldsControl(JobDeclaration declaration) : base(declaration)
		{
			InitializeComponent();
		}

		protected override IPanelLayoutProvider GetLayout()
		{
			if (JobDeclaration is JobDeclaration declaration && declaration.IsUCC6)
			{
				if (declaration.IsExport)
				{
					return new InvoiceUCC6AndExportSupportingDocumentFieldsLayout();
				}

				if (declaration.IsImport)
				{
					return new InvoiceUCC6AndImportSupportingDocumentFieldsLayout(JobDeclaration);
				}
			}

			return new NonUCC6SupportingDocumentFieldsLayout();
		}
	}
}
