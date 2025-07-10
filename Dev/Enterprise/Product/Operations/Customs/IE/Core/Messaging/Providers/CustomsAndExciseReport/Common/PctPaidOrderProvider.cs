using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class PctPaidOrderProvider : PaidOrderProvider
	{
		public PctPaidOrderProvider(PctPaidOrder pctPaidOrder) : base(pctPaidOrder)
		{
			this.pctPaidOrder = Argument.NotNull(pctPaidOrder, nameof(pctPaidOrder));
		}

		readonly PctPaidOrder pctPaidOrder;

		[XlsxField(-1, "Importer Name")]
		public ZString ImporterName => pctPaidOrder.ImporterName;
	}
}
