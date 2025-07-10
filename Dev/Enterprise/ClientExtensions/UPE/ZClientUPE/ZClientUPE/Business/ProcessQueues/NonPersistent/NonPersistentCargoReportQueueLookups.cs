namespace Enterprise.Client.UPE.Business
{
	public class NonPersistentCargoReportQueueLookups : NonPersistentProcessQueueLookups
	{
		public NonPersistentCargoReportQueueLookups(NonPersistentCargoReportQueue queue)
			: base(queue)
		{
		}

		protected override UPEProcessQueueLookupsHelper GetNewUPEProcessQueueLookupsHelper()
		{
			return new UPECargoReportQueueLookupsHelper(Parent);
		}

		protected new NonPersistentCargoReportQueue Parent
		{
			get { return (NonPersistentCargoReportQueue)base.Parent; }
		}
	}
}
