using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	class EntryHeaderChargeWrapper : ITax
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public EntryHeaderChargeWrapper(CusEntryHeaderCharges cusEntryHeaderCharge)
		{
			this.cusEntryHeaderCharge = Argument.NotNull(cusEntryHeaderCharge, "CusEntryHeaderCharge cannot be null ");
		}

		public ZString TaxCode => cusEntryHeaderCharge?.C1_ChargeType ?? ZString.Empty;

		public ZString TaxType => FeeTypeCodeConverter.StandardAdvalorumOrSecondQuantiy;

		public ZDecimal TaxRate => 1;

		public ZDecimal TaxAssessed => cusEntryHeaderCharge?.C1_ChargeAmount ?? ZDecimal.Zero;

		public ZDecimal TaxAmount => cusEntryHeaderCharge?.C1_ChargeAmount ?? ZDecimal.Zero;

		public ZString TaxMethodOfPayment => cusEntryHeaderCharge?.C1_MethodOfPayment ?? ZString.Empty;

		public ZString ChargePaymentOrDestinationID => cusEntryHeaderCharge.EntryHeader?.Declaration?.ChargePaymentOrDestinationID ?? ZString.Empty;

		public ZString LiquidationStatus => ZString.Empty;

		public ZString EUTaxCode => ZString.Empty;

		readonly CusEntryHeaderCharges cusEntryHeaderCharge;
	}
}
