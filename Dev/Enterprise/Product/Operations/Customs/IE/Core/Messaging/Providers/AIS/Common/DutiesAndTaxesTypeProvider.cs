using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class DutiesAndTaxesTypeProvider : ITaxTypeProvider
	{
		public DutiesAndTaxesTypeProvider(MDutiesAndTaxesType03 dutiesAndTaxesType)
		{
			this.dutiesAndTaxesType = dutiesAndTaxesType;
		}

		readonly MDutiesAndTaxesType03 dutiesAndTaxesType;

		public ZString TaxType => dutiesAndTaxesType.TaxType;
		public ZString Unit => TaxBase?.MeasurementUnitAndQualifier;
		public ZDecimal Quantity => TaxBase?.Quantity ?? ZDecimal.Zero;
		public ZDecimal Amount => TaxBase?.Amount ?? ZDecimal.Zero;
		public ZDecimal TaxRate => TaxBase?.TaxRate ?? ZDecimal.Zero;
		public ZDecimal TaxAmount => TaxBase?.TaxAmount ?? ZDecimal.Zero;
		public ZString MethodOfPayment => dutiesAndTaxesType.MethodOfPayment;

		MTaxBaseType01 TaxBase => dutiesAndTaxesType.TaxBase.FirstOrDefault();
	}
}
