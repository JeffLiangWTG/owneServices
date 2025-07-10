using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class InvoiceLayoutSupportingDocumentsFieldsControl : EU.GUI.PlugIn.InvoiceLayoutSupportingDocumentsFieldsControl
	{
		public InvoiceLayoutSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
			InitializeComponent();
		}

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override void SetCaption()
		{
			base.SetCaption();
			if (JobDeclaration.IsUCC5AndIsImport)
			{
				SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("34E1CEFA-BCE0-40DE-9E0F-321EBB1E8488", "[2/3] Supporting Documents");
			}
			else
			{
				SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("23B56FBC-710C-4DAB-B1FD-416BDE53EF59", "Supporting Documents");
			}
		}
	}
}
