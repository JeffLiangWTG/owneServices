using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IMessageKeySetExtractor
	{
		MessageKeySet ExtractKeys(EDIMessage message);
	}
}
