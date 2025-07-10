namespace Enterprise.Client.UPE.Business
{
	public class NonPersistentCalloutQueueLookups : NonPersistentProcessQueueLookups
	{
		public NonPersistentCalloutQueueLookups(NonPersistentCalloutQueue queue)
			: base(queue)
		{
		}

		protected override UPEProcessQueueLookupsHelper GetNewUPEProcessQueueLookupsHelper()
		{
			return new UPECommercialQueueLookupsHelper(Parent);
		}

		protected new NonPersistentCalloutQueue Parent
		{
			get { return (NonPersistentCalloutQueue)base.Parent; }
		}
	}
}
