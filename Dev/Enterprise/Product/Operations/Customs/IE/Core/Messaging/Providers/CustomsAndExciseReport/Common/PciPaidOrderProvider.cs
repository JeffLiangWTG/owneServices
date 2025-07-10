using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class PciPaidOrderProvider : PaidOrderProvider
	{
		public PciPaidOrderProvider(PciPaidOrder pciPaidOrder) : base(pciPaidOrder)
		{
			this.pciPaidOrder = Argument.NotNull(pciPaidOrder, nameof(pciPaidOrder));
		}

		readonly PciPaidOrder pciPaidOrder;

		[XlsxField(-1, "Payer Name")]
		public ZString PayerName => pciPaidOrder.PayerName;
	}
}
