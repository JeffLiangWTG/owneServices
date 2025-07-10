using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	class EntryLineFeeWrapper : ITax
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public EntryLineFeeWrapper(CusEntryLineFee cusEntryLineFee)
		{
			this.cusEntryLineFee = Argument.NotNull(cusEntryLineFee, "CusEntryLineFee cannot be null ");
		}

		public ZString TaxCode => cusEntryLineFee?.NationalFeeTypeCode ?? ZString.Empty;

		public ZString TaxType => FeeTypeCodeConverter.GetFeeCodeTaxType(TaxCode);

		public ZDecimal TaxRate => cusEntryLineFee?.CF_Rate ?? ZDecimal.Zero;

		public ZDecimal TaxAssessed => cusEntryLineFee?.CF_BaseValue ?? ZDecimal.Zero;

		public ZDecimal TaxAmount => cusEntryLineFee?.CF_ChargeAmount ?? ZDecimal.Zero;

		public ZString TaxMethodOfPayment => cusEntryLineFee?.CF_MethodOfPayment ?? ZString.Empty;

		public ZString ChargePaymentOrDestinationID => ZString.Empty;

		public ZString LiquidationStatus => ZString.Empty;

		public ZString EUTaxCode => ZString.Empty;

		readonly CusEntryLineFee cusEntryLineFee;
	}
}
