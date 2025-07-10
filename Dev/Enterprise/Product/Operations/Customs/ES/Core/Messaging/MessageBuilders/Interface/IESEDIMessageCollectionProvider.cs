using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IESEDIMessageCollectionProvider : IEDIMessageCollectionProvider, ICertificateProvider
	{
		ZBool IsTest { get; }
		ZString BusinessObjectReference { get; }
	}
}
