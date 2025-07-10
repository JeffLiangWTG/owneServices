using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class TaxWrapper : ITax
	{
		public TaxWrapper(CusEntryLineFee fee)
		{
			this.fee = Argument.NotNull(fee, "Fee cannot be null ");
		}

		public ZString TaxCode => fee.NationalFeeTypeCode;

		public ZString TaxType => FeeTypeCodeConverter.GetFeeCodeTaxType(TaxCode);

		public ZDecimal TaxRate => fee.CF_Rate;

		public ZDecimal TaxAssessed => fee.CF_BaseValue;

		public ZDecimal TaxAmount => fee.CF_ChargeAmount;

		public ZString TaxMethodOfPayment => fee.CF_MethodOfPayment;

		public ZString ChargePaymentOrDestinationID => ZString.Empty;

		public ZString LiquidationStatus => ZString.Empty;

		public ZString EUTaxCode => ZString.Empty;

		readonly CusEntryLineFee fee;
	}
}
