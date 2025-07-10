using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.WCB
{
	internal class InvLineFixedCollection : JobComInvoiceLineViewCollection
	{
		public InvLineFixedCollection(InvHeadWithFixedInvLines invHead, Customs.Business.InvoiceLineCompleteCollection invLines)
			: base(invHead, invLines)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
