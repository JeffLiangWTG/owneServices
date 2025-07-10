using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;

namespace Enterprise.DocumentVisualizer.Delivery
{
	sealed class DocumentDeliverableCollection : NonPersistentBusinessObjectCollection<DocumentDeliverable>, IDeliverableCollection
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore => false;
	}
}
