using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using GuaranteeTypeList = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsGuaranteeValidation : EU.NCTS.Business.NctsGuaranteePhase5Validation
{
	public NctsGuaranteeValidation(NctsGuarantee parent) : base(parent)
	{
	}

	public new NctsGuarantee Parent => (NctsGuarantee)base.Parent;

	protected override void CheckPW_BondType()
	{
		base.CheckPW_BondType();
		var parent = Parent;
		var nctsHeader = parent.NctsHeader;
		ValidateGuaranteesRuleBE0039();
		if (nctsHeader.MovementHeader.Guarantees.Count > 1)
		{
			var guarantees = nctsHeader.MovementHeader.Guarantees.Cast<NctsGuarantee>().ToArray();
			switch (parent.PW_BondType)
			{
				case GuaranteeTypeList.Codes.GuaranteeWaiverByAgreement:
					if (guarantees.Any(x => x.PW_BondType != GuaranteeTypeList.Codes.GuaranteeWaiverByAgreement))
					{
						parent.PW_BondTypeInfo.AddError(Res.GetString("C04C62EA-00C5-4FE8-8180-A28EDE96AE2E", "All Guarantee Types must be A, or none must be A."));
					}
					break;
				case GuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention:
				case GuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur:
					var types = new ZString[] { GuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention, GuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur, GuaranteeTypeList.Codes.CashDepositGuarantee };
					if (guarantees.Any(x => !types.Contains(x.PW_BondType)))
					{
						parent.PW_BondTypeInfo.AddError(Res.GetString("18386BBC-07B1-403D-A3E9-6244DA479AA4", "Guarantee Type {0} can only be combined with types 3, 5 or B.", parent.PW_BondType));
					}
					break;
				case GuaranteeTypeList.Codes.GuaranteeWaiver:
				case GuaranteeTypeList.Codes.ComprehensiveGuarantee:
				case GuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor:
				case GuaranteeTypeList.Codes.FlatRateVoucher:
				case GuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage:
					var allowedTypes = new ZString[] { Parent.PW_BondType, GuaranteeTypeList.Codes.CashDepositGuarantee };
					if (guarantees.Any(x => !allowedTypes.Contains(x.PW_BondType)))
					{
						parent.PW_BondTypeInfo.AddError(Res.GetString("96E01123-B035-472B-8714-8735DE262944", "Guarantee Type {0} can only be combined with type 3.", parent.PW_BondType));
					}
					break;
			}
		}
	}

	protected override void CheckPW_BondNumber()
	{
		base.CheckPW_BondNumber();
		ValidateGuaranteesRuleB0054();
	}

	void ValidateGuaranteesRuleBE0039()
	{
		var bondType = Parent.PW_BondType;
		if (bondType != GuaranteeTypeList.Codes.GuaranteeWaiver && bondType != GuaranteeTypeList.Codes.ComprehensiveGuarantee && Parent.NctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader && movementHeader.IsSimplifiedNctsProcedure)
		{
			Parent.PW_BondTypeInfo.AddMessageError(Res.GetString("632EE558-A278-4134-A8DD-CB35628C34F6", "Type of guarantee must be 0 or 1. Please correct type."));
		}
	}

	void ValidateGuaranteesRuleB0054()
	{
		var parent = Parent;
		if (!parent.PW_BondNumber.IsEmpty && ValidationExtendMethods.GuaranteeTypesApplicable.Contains(parent.PW_BondType))
		{
			var currentGuaranteeNationality = parent.PW_BondNumber.SubstringSafe(2, 2);
			var guarantees = parent.NctsHeader.MovementHeader.Guarantees.Cast<NctsGuarantee>().ToArray();
			if (guarantees.Any(x => ValidationExtendMethods.GuaranteeTypesApplicable.Contains(x.PW_BondType) &&
									x.PK != parent.PK &&
									x.PW_BondNumber.SubstringSafe(2, 2) != currentGuaranteeNationality))
			{
				parent.PW_BondNumberInfo.AddError(Res.GetString("2AC40E47-4490-47DD-8756-4623EF965A33", "You cannot combine a national guarantee with an international guarantee for type 0, 1, 2, 4 or 9."));
			}
		}
	}
}
