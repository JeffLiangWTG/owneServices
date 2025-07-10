using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputDocumentDeliveryRequestDataProvider : IDocumentDeliveryRequest
{
	public CharteraOutputDocumentDeliveryRequestDataProvider(IEnumerable<CusPollingTransaction> transactions)
	{
		DocumentIds = Argument.NotNull(transactions, nameof(transactions)).Select(x => x.CPT_TransactionID.ToString()).ToArray();
	}

	public string ProcessId { get; } = Guid.NewGuid().ToString();

	public IReadOnlyCollection<string> DocumentIds { get; }
}
