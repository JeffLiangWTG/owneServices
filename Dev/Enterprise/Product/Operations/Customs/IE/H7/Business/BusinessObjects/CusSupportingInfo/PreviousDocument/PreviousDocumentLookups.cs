namespace Enterprise.Customs.IE.H7.Business
{
	public class PreviousDocumentLookups : EU.H7.Business.PreviousDocumentLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent)
			: base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;
	}
}
