using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public interface IEventSubscriptionAgent
	{
		/// <summary>
		///		Get log parents that will notify subscribers defined in <paramref name="subscription"/> about <paramref name="evnt"/> occurred.
		/// </summary>
		/// <param name="subscription">
		///		An object which defines subscription parameters.
		/// </param>
		/// <param name="evnt">
		///		An instance of event published to the subscribers defined in <paramref name="subscription"/>.
		/// </param>
		/// <returns>
		///		A collection of log parents which to be notified.
		/// </returns>
		IEnumerable<IStmALogParent> GetLogParentsToFireWorkflowForEvent(IStmEventSubscription subscription, IStmALog evnt);
	}
}
