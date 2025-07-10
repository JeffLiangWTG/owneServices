using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	internal class DataRefreshBusSubscription : DataRefreshBus.Subscription
	{
		public DataRefreshBusSubscription(BusinessObjectFactory factory, IDataRefreshBusSubscriber participant)
			: base(factory, participant)
		{
		}

		internal override void DoAction(IEnumerable<BusinessObject> publishedObjects)
		{
			bool result = false;
			IDataRefreshBusSubscriber participant = Participant as IDataRefreshBusSubscriber;
			if (participant != null)
			{
				if (participant.IncludeDeletedObjectsInRefresh)
				{
					participant.UpdatedByDataRefresh(publishedObjects);
				}
				else
				{
					participant.UpdatedByDataRefresh(publishedObjects.Where(publishedObject => !publishedObject.IsDeleted));
				}
				result = true;
			}
			OnSubscriptionUpdate(result);
		}
	}
}
