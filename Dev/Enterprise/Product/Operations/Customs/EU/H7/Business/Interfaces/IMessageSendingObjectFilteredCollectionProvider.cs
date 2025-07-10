using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business;

public interface IMessageSendingObjectFilteredCollectionProvider
{
	BusinessObjectCollection MessageSendingObjectFilteredCollection { get; }
}
