using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IEDIFACTMessageDataProvider : IESEDIMessageCollectionProvider
	{
		ZString DeclarantIdForUNBSegment { get; }
	}
}
