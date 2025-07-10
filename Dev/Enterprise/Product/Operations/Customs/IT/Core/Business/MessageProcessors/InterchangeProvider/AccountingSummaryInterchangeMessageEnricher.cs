using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class AccountingSummaryInterchangeMessageEnricher : IEDIInterchangeEnricher
{
	EDIInterchange IEDIInterchangeEnricher.Enrich(EDIInterchange ediInterchange)
	{
		Argument.NotNull(ediInterchange, nameof(ediInterchange));
		ediInterchange.EI_InterchangeType = EDIMessageTypeList.Codes.AccountingSummaryRequest;
		return ediInterchange;
	}
}
