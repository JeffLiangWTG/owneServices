using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class InvoiceLineLayoutSupportingDocumentsFieldsControl : EU.GUI.PlugIn.InvoiceLineLayoutSupportingDocumentsFieldsControl
	{
		public InvoiceLineLayoutSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
			InitializeComponent();
		}

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override void SetCaption()
		{
			base.SetCaption();
			if (JobDeclaration.IsUCC5AndIsImport)
			{
				SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("41AEC765-DA69-4356-B75E-0252EAB8BC0D", "[2/3] Supporting Documents");
			}
			else
			{
				SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("A9F72A64-503D-47B4-BE92-BE155AE5E102", "Supporting Documents");
			}
		}
	}
}
