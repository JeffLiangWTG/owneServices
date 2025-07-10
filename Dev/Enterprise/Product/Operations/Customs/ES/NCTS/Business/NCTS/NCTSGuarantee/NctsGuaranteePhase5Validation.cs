using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsGuaranteePhase5Validation : EU.NCTS.Business.NctsGuaranteePhase5Validation
{
	public NctsGuaranteePhase5Validation(EU.NCTS.Business.NctsGuarantee cusBondDetail) : base(cusBondDetail)
	{
	}

	protected override bool CheckBondTypeIsEmptyIsActive => !Parent.NctsHeader.IsArrivalMovement;

	protected override bool IsGuaranteeTypeWithReferenceC0085(ZString guaranteeType)
	{
		switch (guaranteeType)
		{
			case ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver:
			case ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee:
			case ESNCTS5GuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor:
			case ESNCTS5GuaranteeTypeList.Codes.CashDepositGuarantee:
			case ESNCTS5GuaranteeTypeList.Codes.FlatRateVoucher:
				return true;
			default:
				return false;
		}
	}
}
