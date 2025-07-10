
namespace Enterprise.ZArchitecture.Business
{
	using System;
	using System.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Integration;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Schema;

	public class StmEventSubscription : AutoStmEventSubscription, IStmEventSubscription
	{
		public StmEventSubscription(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		/// <summary>
		///		Creates a subscription for entities in one module to react on events from entities in another module.
		/// </summary>
		/// <param name="registrar">
		///		A business object that manages subscription for its publishers. Should have workflow.
		/// </param>
		/// <param name="agent">
		///		A business object that supposed to be notified on events. Should have workflow which implements <see cref="IEventSubscriptionAgent"/>.
		/// </param>
		/// <param name="publisherType">
		///		A workflow descriptor code of the entities to subscribe for events from. Should implement <see cref="IEventPublisher"/>.
		/// </param>
		/// <param name="subscriberType">
		///		A workflow descriptor code of the entities to subscribe for events.
		/// </param>
		/// <remarks>
		///		Basically, it means the following: my entities with type <paramref name="subscriberType"/> are interested in events from
		///		all entities of type <paramref name="publisherType"/> which belong to <paramref name="registrar"/>. If events occur, please
		///		notify an entity <paramref name="agent"/> and it will notify all its entities of type <paramref name="subscriberType"/>.
		/// </remarks>
		public static void CreateIfNotExists(BusinessObject registrar, BusinessObject agent, ZString publisherType, ZString subscriberType)
		{
			var registrarWorkflowProvider = registrar as IWorkflowProviderCore
				?? throw new ArgumentException("Registrar doesn't have workflow and thus cannot be used as a registrar", nameof(registrar));

			var agentWorkflowProvider = agent as IWorkflowProviderCore
				?? throw new ArgumentException("Agent doesn't have workflow and thus cannot be used as an agent", nameof(agent));

			var query = new ZQuery();
			query.AddToFilter(StmEventSubscriptionSchema.SES_PublisherDescriptor, publisherType);
			query.AddToFilter(StmEventSubscriptionSchema.SES_SubscriberDescriptor, subscriberType);
			query.AddToFilter(StmEventSubscriptionSchema.SES_AgentDescriptor, agentWorkflowProvider.WorkflowType);
			query.AddToFilter(StmEventSubscriptionSchema.SES_RegistrarDescriptor, registrarWorkflowProvider.WorkflowType);
			query.AddToFilter(StmEventSubscriptionSchema.SES_AgentParentId, agent.PK);
			query.AddToFilter(StmEventSubscriptionSchema.SES_RegistrarParentId, registrar.PK);

			var subscription = registrar.Factory.LoadTop1<StmEventSubscription>(query);
			if (subscription == null)
			{
				subscription = registrar.Factory.New<StmEventSubscription>();
				subscription.SES_PublisherDescriptor = publisherType;
				subscription.SES_SubscriberDescriptor = subscriberType;
				subscription.SES_AgentDescriptor = agentWorkflowProvider.WorkflowType;
				subscription.SES_RegistrarDescriptor = registrarWorkflowProvider.WorkflowType;
				subscription.SES_AgentParentId = agent.PK;
				subscription.SES_RegistrarParentId = registrar.PK;
			}
		}
	}
}
