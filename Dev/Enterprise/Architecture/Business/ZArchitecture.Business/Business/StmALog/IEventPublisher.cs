using System.Collections.Generic;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public interface IEventPublisher
	{
		/// <summary>
		///		Gets a query which returns subscriptions (<see cref="StmEventSubscriptionSchema"/>) related to a publisher.
		/// </summary>
		/// <returns>
		///		A query which returns a set containing Subscription and TargetID (SES_PK and SL_ParentID).
		///		
		/// </returns>
		string GetSubscriptionsQuery();
		IEnumerable<string> GetPublisherTableNames();
	}
}
