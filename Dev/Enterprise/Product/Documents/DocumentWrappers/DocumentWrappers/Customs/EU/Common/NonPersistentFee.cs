using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class NonPersistentFee : NonPersistentBusinessObject, IDocSADHLineTaxBoxSupporter
	{
		public NonPersistentFee(CusEntryLineFee fee, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(fee, nameof(fee));
			Argument.NotNull(factory, nameof(factory));

			Type = fee.CF_ChargeType;
			MethodOfPayment = fee.CF_MethodOfPayment;

			amountInDeclarationCurrency = fee.CF_ChargeAmount;
		}

		public ZString Type { get; }

		public ZString TaxBase => ZString.Empty;

		public ZString Rate => ZString.Empty;

		public ZString RateDuty => ZString.Empty;

		public ZString RateOverride => ZString.Empty;

		public ZString AmountInDeclarationCurrency => amountInDeclarationCurrency.ToString(DecimalsFormat);
		decimal amountInDeclarationCurrency;

		public ZString MethodOfPayment { get; }

		public ZString NationalFeeTypeCode => ZString.Empty;

		public ZString DeclarationMethodOfPayment => ZString.Empty;

		public void AddAmountToTax(ZDecimal chargeAmount) => amountInDeclarationCurrency += chargeAmount;

		const string DecimalsFormat = "N2";
	}
}
