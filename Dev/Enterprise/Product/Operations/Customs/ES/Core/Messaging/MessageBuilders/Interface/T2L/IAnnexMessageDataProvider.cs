using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IAnnexMessageDataProvider : IESEDIMessageCollectionProvider
	{
		IAnnexHeader Header { get; }
		IAnnexDocCommon Document { get; }
	}

	public interface IAnnexHeader
	{
		ZString T2LReferenceNumber { get; }
		IPartyNameProvider Declarant { get; }
		ZBool FinalAnnexIndicator { get; }
	}
}
