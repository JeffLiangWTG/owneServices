using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	class TestComplianceDocumentForm : ComplianceDocumentForm
	{
		public TestComplianceDocumentForm(AccComplianceDocumentHeader businessEntity) : base(businessEntity)
		{
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			return ContinueWithDelete.Yes;
		}
	}
}
