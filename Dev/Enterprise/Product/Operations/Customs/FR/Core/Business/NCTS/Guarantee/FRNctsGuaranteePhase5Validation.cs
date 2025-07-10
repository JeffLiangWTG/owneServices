using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class FRNctsGuaranteePhase5Validation : NctsGuaranteePhase5Validation
	{
		public FRNctsGuaranteePhase5Validation(NctsGuarantee cusBondDetail) : base(cusBondDetail)
		{
		}

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			var parent = Parent;
			if (parent.ValidationDecider is INctsGuaranteeDeparturePhase5ValidationDecider departureValidationDecider && departureValidationDecider.IsRuleNAT085Active)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.PW_BondTypeInfo);

				var forbiddenGuaranteeTypes = new ZString[]
				{
					EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType5,
					EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType9,
					EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeA,
					EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeJ,
				};

				if (forbiddenGuaranteeTypes.Contains(parent.PW_BondType))
				{
					parent.PW_BondTypeInfo.AddMessageError(Res.GetString("AD9F5C91-493A-4873-A7F5-211B407C44C5", "[NAT085] This guarantee type is not allowed.")); 
				}
			}
		}

		protected override void CheckPW_BondNumber()
		{
			base.CheckPW_BondNumber();
			var parent = Parent;
			var departureValidationDecider = parent.ValidationDecider as INctsGuaranteeDeparturePhase5ValidationDecider;
			if (departureValidationDecider.IsRuleNAT085Active && parent.PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention)
			{
				MandatoryValidation.MessageErrorIfIsEntered(parent.PW_BondNumberInfo);
			}
		}

		protected override void CheckPW_BondNumber2()
		{
			base.CheckPW_BondNumber2();
			var parent = Parent;
			var departureValidationDecider = parent.ValidationDecider as INctsGuaranteeDeparturePhase5ValidationDecider;
			if (departureValidationDecider.IsRuleNAT086Active && parent.PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee)
			{
				MandatoryValidation.CheckEntered(parent.PW_BondNumber2Info);
			}
		}

		protected override void CheckPW_RX_NKCurrency()
		{
			base.CheckPW_RX_NKCurrency();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_RX_NKCurrencyInfo);
		}

		protected override ZString BondNumberForR0318Check => Parent.CusGuarantee?.GetApplicationSpecificReference(FRConstants.GuaranteeAppTypes.TR) ?? ZString.Empty;

		protected override bool IsGuaranteeTypeWithReferenceC0085(ZString guaranteeType)
		{
			switch (guaranteeType)
			{
				case "3":
					return false;
				default:
					return base.IsGuaranteeTypeWithReferenceC0085(guaranteeType);
			}
		}
	}
}
