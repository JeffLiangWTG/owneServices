using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface INctsHeaderMessageProvider : IEDIFACTMessageDataProvider
	{
		ZString LocalReferenceNumber { get; }
		ZString CountryOfDestination { get; }
		IPartyProvider Consignor { get; }
		IPartyProvider Consignee { get; }
	}
}
