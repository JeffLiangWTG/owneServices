using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public sealed class DctPaidOrderProvider : PaidOrderProvider
{
	public DctPaidOrderProvider(DctPaidOrder dctPaidOrder) : base(dctPaidOrder)
	{
		this.dctPaidOrder = Argument.NotNull(dctPaidOrder, nameof(dctPaidOrder));
	}

	readonly DctPaidOrder dctPaidOrder;

	[XlsxField(-2, "Importer Name")]
	public ZString ImporterName => dctPaidOrder.ImporterName;

	[XlsxField(-1, "Period")]
	public ZDateTime Period
	{
		get
		{
			new ZString(dctPaidOrder.Period).TryParseToDate(out var requestDate);
			return requestDate;
		}
	}
}
