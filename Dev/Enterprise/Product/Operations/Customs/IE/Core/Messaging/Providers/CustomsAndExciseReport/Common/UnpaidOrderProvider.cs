using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class UnpaidOrderProvider : IXlsxProvider
	{
		public UnpaidOrderProvider(UnpaidOrder unpaidOrder)
		{
			this.unpaidOrder = Argument.NotNull(unpaidOrder, nameof(unpaidOrder));
		}

		readonly UnpaidOrder unpaidOrder;

		[XlsxField(1, "MRN")]
		public ZString Mrn => unpaidOrder.Mrn;

		[XlsxField(2, "Version")]
		public ZInt Version => unpaidOrder.Version;

		[XlsxField(3, "Tax Total")]
		public ZDecimal TaxTotal => unpaidOrder.TaxTotal;
	}
}
