using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public sealed class UCC5DutiesAndTaxesTypeProvider : ITaxTypeProvider
	{
		public UCC5DutiesAndTaxesTypeProvider(TaxBoxType dutiesAndTaxesType)
		{
			this.dutiesAndTaxesType = dutiesAndTaxesType;
		}

		readonly TaxBoxType dutiesAndTaxesType;

		public ZString TaxType => dutiesAndTaxesType.BoxTaxType;
		public ZString Unit => dutiesAndTaxesType.BoxTaxBaseUnit;
		public ZDecimal Quantity => dutiesAndTaxesType.BoxQuantity ?? ZDecimal.Zero;
		public ZDecimal Amount => dutiesAndTaxesType.BoxAmount ?? ZDecimal.Zero;
		public ZDecimal TaxRate => dutiesAndTaxesType.BoxTaxRate ?? ZDecimal.Zero;
		public ZDecimal TaxAmount => dutiesAndTaxesType.BoxTaxPayableAmount ?? ZDecimal.Zero;
		public ZString MethodOfPayment => dutiesAndTaxesType.BoxTaxPaymentMethod;
	}
}
