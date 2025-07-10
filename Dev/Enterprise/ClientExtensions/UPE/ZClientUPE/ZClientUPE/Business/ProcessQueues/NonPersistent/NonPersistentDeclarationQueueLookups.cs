namespace Enterprise.Client.UPE.Business
{
	public class NonPersistentDeclarationQueueLookups : NonPersistentProcessQueueLookups
	{
		public NonPersistentDeclarationQueueLookups(NonPersistentDeclarationQueue queue)
			: base(queue)
		{
		}

		protected override UPEProcessQueueLookupsHelper GetNewUPEProcessQueueLookupsHelper()
		{
			return new UPEDeclarationQueueLookupsHelper(Parent);
		}

		protected new NonPersistentDeclarationQueue Parent
		{
			get { return (NonPersistentDeclarationQueue)base.Parent; }
		}
	}
}
