using CargoWise.Common;

namespace Enterprise.Customs.IT.Business;

public class CustomsStatusOrder
{
	public CustomsStatusOrder(string entryStatusCode, int? order)
	{
		EntryStatusCode = Argument.NotNull(entryStatusCode, nameof(entryStatusCode));
		Order = order;
	}

	public string EntryStatusCode { get; }
	public int? Order { get; }
}
