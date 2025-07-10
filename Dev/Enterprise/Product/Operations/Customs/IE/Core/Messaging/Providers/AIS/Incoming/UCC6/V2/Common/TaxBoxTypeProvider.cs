using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class TaxBoxTypeProvider : ITaxTypeProvider
	{
		public TaxBoxTypeProvider(TaxBoxType taxBoxType)
		{
			this.taxBoxType = taxBoxType;
		}
		readonly TaxBoxType taxBoxType;

		public ZString TaxType => taxBoxType.BoxTaxType;
		public ZString Unit => taxBoxType.BoxTaxBaseUnit;
		public ZDecimal Quantity => taxBoxType.BoxQuantity ?? ZDecimal.Zero;
		public ZDecimal Amount => taxBoxType.BoxAmount ?? ZDecimal.Zero;
		public ZDecimal TaxRate => taxBoxType.BoxTaxRate ?? ZDecimal.Zero;
		public ZDecimal TaxAmount => taxBoxType.BoxTaxPayableAmount ?? ZDecimal.Zero;
		public ZString MethodOfPayment => taxBoxType.BoxTaxPaymentMethod;
	}
}
