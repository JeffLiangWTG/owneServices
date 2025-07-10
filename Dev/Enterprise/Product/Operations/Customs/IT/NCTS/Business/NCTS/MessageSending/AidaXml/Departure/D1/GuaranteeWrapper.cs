using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class GuaranteeWrapper : IGuarantee
{
	public GuaranteeWrapper(NctsGuarantee guarantee)
	{
		this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
	}

	string IGuarantee.GuaranteeType => guarantee.PW_BondType;

	string IGuarantee.OtherGuaranteeReference => guarantee.PW_BondNumber2;

	string IGuarantee.GRN => null;

	string IGuarantee.AccessCode => null;

	string IGuarantee.Currency => null;

	decimal? IGuarantee.AmountToBeCovered => null;

	string IGuarantee.CustomsOfficeOfGuarantee => null;

	readonly NctsGuarantee guarantee;
}
