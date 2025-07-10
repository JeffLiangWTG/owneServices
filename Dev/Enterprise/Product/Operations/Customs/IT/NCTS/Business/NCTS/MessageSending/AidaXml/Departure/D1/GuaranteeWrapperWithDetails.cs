using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class GuaranteeWrapperWithDetails : IGuarantee
{
	public GuaranteeWrapperWithDetails(NctsGuarantee guarantee)
	{
		this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
	}

	string IGuarantee.GuaranteeType => guarantee.PW_BondType;

	string IGuarantee.OtherGuaranteeReference => guarantee.PW_BondNumber2;

	string IGuarantee.GRN => guarantee.PW_BondNumber;

	string IGuarantee.AccessCode => guarantee.PW_Password;

	string IGuarantee.Currency => guarantee.PW_RX_NKCurrency;

	decimal? IGuarantee.AmountToBeCovered => guarantee.PW_RX_NKCurrency.IsEmpty
		? guarantee.PW_BondAmount.GetValueOrNullIfZero()
		: guarantee.PW_BondAmount;

	string IGuarantee.CustomsOfficeOfGuarantee => guarantee.PW_BondFiledPort;

	readonly NctsGuarantee guarantee;
}
