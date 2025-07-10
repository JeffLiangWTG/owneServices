using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IObligationGuarantee
	{
		ZString SecurityDetailsCode { get; }
		ZString ReferenceID { get; }
		ZString ID { get; }
		IAmountAndCurrency AmountAmount { get; }
		ZString AccessCode { get; }
		ZString GuaranteeOfficeId { get; }
	}

	class ObligationGuaranteeWrapper : IObligationGuarantee
	{
		ObligationGuaranteeWrapper(ZString securityDetailsCode, ZString referenceId, ZString id, ZString accessCode, ZString guaranteeOfficeCode, IAmountAndCurrency amountAmount)
		{
			this.securityDetailsCode = securityDetailsCode;
			this.referenceId = referenceId;
			this.id = id;
			this.accessCode = accessCode;
			this.guaranteeOfficeCode = guaranteeOfficeCode;
			this.amountAmount = amountAmount;
		}

		public static ObligationGuaranteeWrapper New(GBGuarantee guarantee)
		{
			return new ObligationGuaranteeWrapper(guarantee.PW_Password
				, guarantee.PW_BondNumber
				, guarantee.PW_BondNumber2
				, guarantee.PW_Password
				, guarantee.PW_BondFiledPort
				, AmountAndCurrencyWrapper.New(guarantee.PW_BondAmount, guarantee.PW_RX_NKCurrency));
		}

		ZString IObligationGuarantee.SecurityDetailsCode => securityDetailsCode;

		ZString IObligationGuarantee.ReferenceID => referenceId;

		ZString IObligationGuarantee.ID => id;

		IAmountAndCurrency IObligationGuarantee.AmountAmount => amountAmount;

		ZString IObligationGuarantee.AccessCode => accessCode;

		ZString IObligationGuarantee.GuaranteeOfficeId => guaranteeOfficeCode;

		readonly ZString securityDetailsCode;
		readonly ZString referenceId;
		readonly ZString id;
		readonly ZString accessCode;
		readonly ZString guaranteeOfficeCode;
		readonly IAmountAndCurrency amountAmount;
	}
}
