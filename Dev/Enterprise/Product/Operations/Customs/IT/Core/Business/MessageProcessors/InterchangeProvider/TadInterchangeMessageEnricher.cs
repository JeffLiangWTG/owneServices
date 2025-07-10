using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class TadInterchangeMessageEnricher : IEDIInterchangeEnricher
{
	EDIInterchange IEDIInterchangeEnricher.Enrich(EDIInterchange ediInterchange)
	{
		_ = Argument.NotNull(ediInterchange, nameof(ediInterchange));
		ediInterchange.EI_InterchangeType = EDIMessageTypeList.Codes.TadRequest;
		return ediInterchange;
	}
}
