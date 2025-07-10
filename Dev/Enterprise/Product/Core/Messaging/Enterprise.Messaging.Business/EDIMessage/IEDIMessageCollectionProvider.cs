using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	public interface IEDIMessageCollectionProvider
	{
		EDIMessageCollection Messages { get; }
		BusinessObjectFactory Factory { get; }
	}
}
