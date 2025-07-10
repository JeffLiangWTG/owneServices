using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.WCB
{
	internal class InvHeadFixedCollection : InvoiceHeaderActiveCollection
	{
		public InvHeadFixedCollection(JobDeclarationWithFixedInvHeads jobDec)
			: base(jobDec, false)
		{
		}

		public new InvHeadWithFixedInvLines this[int index]
		{
			get { return (InvHeadWithFixedInvLines)base[index]; }
		}
	}
}
