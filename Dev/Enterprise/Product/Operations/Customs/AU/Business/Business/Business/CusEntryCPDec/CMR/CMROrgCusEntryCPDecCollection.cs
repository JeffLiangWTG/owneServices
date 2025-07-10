namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMROrgCusEntryCPDecCollection : CMRCusEntryCPDecCollection
	{
		public CMROrgCusEntryCPDecCollection(OrganisationCPQA orgAttachee)
			: base(orgAttachee, orgAttachee.Organisation)
		{
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}
	}
}
