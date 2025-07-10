namespace Enterprise.Customs.IE.H7.Business
{
	public class SupportingDocumentLookups : EU.H7.Business.SupportingDocumentLookups
	{
		public SupportingDocumentLookups(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;
	}
}
