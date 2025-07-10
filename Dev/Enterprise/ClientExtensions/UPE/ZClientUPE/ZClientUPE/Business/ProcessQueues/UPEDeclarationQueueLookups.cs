namespace Enterprise.Client.UPE.Business
{
	public class UPEDeclarationQueueLookups : UPEProcessQueueLookups
	{
		public UPEDeclarationQueueLookups(UPEDeclarationQueue parent)
			: base(parent)
		{
		}

		protected new UPEDeclarationQueue Parent
		{
			get { return (UPEDeclarationQueue)base.Parent; }
		}

		protected override UPECustomsQueueLookupsHelper GetNewCustomsProcessQueueLookupsHelper()
		{
			return new UPEDeclarationQueueLookupsHelper(Parent);
		}
	}
}
