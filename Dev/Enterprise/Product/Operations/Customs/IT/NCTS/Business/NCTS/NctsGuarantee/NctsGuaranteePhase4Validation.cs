using System.Collections.Immutable;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

class NctsGuaranteePhase4Validation : EU.NCTS.Business.NctsGuaranteeValidation
{
	public NctsGuaranteePhase4Validation(NctsGuarantee parent) : base(parent)
	{
	}

	public new NctsGuarantee Parent => (NctsGuarantee)base.Parent;
	bool IsTIRDEclaration => Parent.NctsHeader.MovementHeader.IsTIRDeclaration;

	protected override void CheckPW_BondType()
	{
		base.CheckPW_BondType();
		ValidatePW_BondNumber2();
	}

	protected override void CheckPW_BondNumber()
	{
		base.CheckPW_BondNumber();
		if (!Parent.PW_BondNumber.IsEmpty && !guaranteeTypesApplicableForC125C130.Contains(Parent.PW_BondType) && !IsTIRDEclaration)
		{
			Parent.PW_BondNumberInfo.AddMessageError(ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
		}
	}

	protected override void CheckPW_Password()
	{
		base.CheckPW_Password();
		if (!Parent.PW_Password.IsEmpty && !guaranteeTypesApplicableForC125C130.Contains(Parent.PW_BondType) && !IsTIRDEclaration)
		{
			Parent.PW_PasswordInfo.AddMessageError(ValidationCaptions.NctsGuarantee.ShouldBeEmptyForNonApplicableType);
		}
	}

	protected override void CheckPW_BondNumber2()
	{
		base.CheckPW_BondNumber2();
		var isInGuaranteeTypesApplicable = guaranteeTypesApplicableForC125C130.Contains(Parent.PW_BondType);
		var bondNumber2 = Parent.PW_BondNumber2;
		var bondNumber2Info = Parent.PW_BondNumber2Info;
		if (!IsTIRDEclaration)
		{
			if (!bondNumber2.IsEmpty && isInGuaranteeTypesApplicable)
			{
				bondNumber2Info.AddMessageError(ValidationCaptions.NctsGuarantee.ShouldBeEmptyForApplicableType);
			}

			else if (bondNumber2.IsEmpty && !isInGuaranteeTypesApplicable)
			{
				bondNumber2Info.AddMessageError(ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);
			}
		}
	}

	protected override void CheckPW_BondAmount()
	{
		base.CheckPW_BondAmount();
		if (!IsTIRDEclaration && Parent.PW_BondAmount.IsEmpty && !IsInGuaranteeTypesApplicable())
		{
			Parent.PW_BondAmountInfo.AddMessageError(ValidationCaptions.NctsGuarantee.IsRequiredForSelectedType);
		}

		bool IsInGuaranteeTypesApplicable() => guaranteeTypesApplicableForC125C130.Contains(Parent.PW_BondType);
	}

	readonly ImmutableArray<string> guaranteeTypesApplicableForC125C130 = ImmutableArray.Create(
		EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver,
		EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee,
		EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor,
		EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher,
		EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage
	);
}
