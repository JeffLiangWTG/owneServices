using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class InvoiceLayoutSupportingDocumentsUserControl : EU.GUI.PlugIn.InvoiceLayoutSupportingDocumentsUserControl
	{
		public InvoiceLayoutSupportingDocumentsUserControl()
		{
			InitializeComponent();
		}

		public new JobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set => base.JobDeclaration = value;
		}

		protected override EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration) => new InvoiceLayoutSupportingDocumentsFieldsControl(declaration);
	}
}
