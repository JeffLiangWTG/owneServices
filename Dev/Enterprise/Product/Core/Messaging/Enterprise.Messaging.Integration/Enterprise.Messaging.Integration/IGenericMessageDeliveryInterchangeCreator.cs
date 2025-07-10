using CargoWise.Types;

namespace Enterprise.Messaging.Integration
{
	public interface IGenericMessageDeliveryInterchangeCreator
	{
		string[] SupportedInterchangeTypes { get; }
		IEDIInterchange Create(ZGuid branchPK, ZString senderId, ZString recipientId, ZString interchangeType, ZString body);
	}
}
