using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DeliverableCollectionViewForTesting : DeliverableCollectionView
	{
		public DeliverableCollectionViewForTesting(IDeliverableCollection deliverables, DocDeliveryContactCollection recipients)
			: base(deliverables, recipients)
		{
		}

		protected override void RebuildCore()
		{
			NumberOfCallsToRebuildCore++;
			base.RebuildCore();
		}

		public int NumberOfCallsToRebuildCore;
	}
}
