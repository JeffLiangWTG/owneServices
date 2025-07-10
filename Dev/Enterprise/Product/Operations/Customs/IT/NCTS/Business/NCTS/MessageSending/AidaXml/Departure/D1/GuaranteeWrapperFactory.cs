using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;
using NctsGuaranteeCodes = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class GuaranteeWrapperFactory
{
	public IGuarantee GetNewGuaranteeWrapper(NctsGuarantee guarantee)
	{
		Argument.NotNull(guarantee, nameof(guarantee));

		return guaranteeTypesRequiringDetails.Contains(guarantee.PW_BondType)
			? new GuaranteeWrapperWithDetails(guarantee)
			: new GuaranteeWrapper(guarantee);
	}

	readonly ImmutableArray<ZString> guaranteeTypesRequiringDetails = new ZString[]
	{
		NctsGuaranteeCodes.GuaranteeWaiver,
		NctsGuaranteeCodes.ComprehensiveGuarantee,
		NctsGuaranteeCodes.IndividualGuaranteeByGuarantor,
		NctsGuaranteeCodes.CashDepositGuarantee,
		NctsGuaranteeCodes.FlatRateVoucher,
		NctsGuaranteeCodes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur,
		NctsGuaranteeCodes.IndividualGuaranteeWithMultipleUsage
	}.ToImmutableArray();
}
