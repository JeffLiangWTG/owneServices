using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface ISummaryDeclarationsCommonMessageDataProvider : IESEDIMessageCollectionProvider
	{
		ZString SenderId { get; }
	}
}
