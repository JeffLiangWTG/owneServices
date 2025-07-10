using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

interface IEDIInterchangeEnricher
{
	EDIInterchange Enrich(EDIInterchange ediInterchange);
}
