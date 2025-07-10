using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM429TaxBoxTypeProvider : ITaxTypeProvider
	{
		public IM429TaxBoxTypeProvider(TaxBoxType taxBoxType)
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
