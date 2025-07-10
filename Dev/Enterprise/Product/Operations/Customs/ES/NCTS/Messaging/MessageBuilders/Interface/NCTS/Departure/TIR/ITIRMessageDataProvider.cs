using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface ITIRMessageDataProvider : INctsBaseDepartureMessageProvider
	{
		ZString CountryOfOrigin { get; }
		INctsCustomsTransitOfficeProvider CustomsOfficeOfTransit { get; }
		ZString TIRCarnetNumber { get; }
		ZDateTime TIRCarnetExpiryDate { get; }
		ITransportMediumInfoCommon LoadingTransport { get; }
		IPartyProvider Holder { get; }
		IReadOnlyCollection<ITIRLine> Lines { get; }
	}
}
