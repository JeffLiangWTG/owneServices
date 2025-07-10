using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmEventSubscription))]
	sealed class StmEventSubscriptionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateIfNotExists()
		{
			var registrar = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var agent = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			StmEventSubscription.CreateIfNotExists(registrar, agent, "PUB", "SBR");

			var subscription = Factory.LoadTop1<StmEventSubscription>(new ZQuery(StmEventSubscriptionSchema.SES_RegistrarParentId, registrar.PK));
			AssertNotNull("Subscription", subscription);
			AssertEquals("SES_PublisherDescriptor", "PUB", subscription.SES_PublisherDescriptor);
			AssertEquals("SES_SubscriberDescriptor", "SBR", subscription.SES_SubscriberDescriptor);
			AssertEquals("SES_AgentDescriptor", "SHP", subscription.SES_AgentDescriptor);
			AssertEquals("SES_RegistrarDescriptor", "CON", subscription.SES_RegistrarDescriptor);
			AssertEquals("SES_AgentParentId", agent.PK, subscription.SES_AgentParentId);
		}
	}
}
