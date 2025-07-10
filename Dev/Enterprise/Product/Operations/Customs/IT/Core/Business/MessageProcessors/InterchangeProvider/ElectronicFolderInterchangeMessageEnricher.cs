using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class ElectronicFolderInterchangeMessageEnricher : IEDIInterchangeEnricher
{
	public EDIInterchange Enrich(EDIInterchange ediInterchange)
	{
		Argument.NotNull(ediInterchange, nameof(ediInterchange));

		ediInterchange.EI_InterchangeType = EDIMessageTypeList.Codes.ElectronicFolderQuery;
		ediInterchange.EI_To = InterchangeToSoap;
		return ediInterchange;
	}

	const string InterchangeToSoap = "ITCustomsSOAP";
}
