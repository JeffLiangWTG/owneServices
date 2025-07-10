using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

public interface IEDIFACTMessageProvider : IEDIMessageCollectionProvider
{
	IMessageHeaderProvider MessageHeader { get; }
}
