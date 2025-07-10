using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IStmEventSubscription
	{
		ZGuid PK { get; }
		ZString SES_AgentDescriptor { get; }
		ZString SES_PublisherDescriptor { get; }
		ZString SES_RegistrarDescriptor { get; }
		ZGuid SES_AgentParentId { get; }
		ZGuid SES_RegistrarParentId { get; }
		ZString SES_SubscriberDescriptor { get; }
	}
}
