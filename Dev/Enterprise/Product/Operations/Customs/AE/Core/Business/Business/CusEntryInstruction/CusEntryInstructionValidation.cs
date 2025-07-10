using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business;

public class CusEntryInstructionValidation : AutoAECusEntryInstructionValidation
{
	public CusEntryInstructionValidation(CusEntryInstruction parent)
		: base(parent)
	{
	}

	public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	protected override void CheckCEI_DeclarationPurpose()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_DeclarationPurposeInfo, Parent.Lookups.DeclarationPurposeList);
	}

	protected override void CheckCEI_DeclarationPurposeDetails()
	{
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(Parent.CEI_DeclarationPurposeDetailsInfo, Parent.CEI_DeclarationPurposeInfo, (ZString)AEConstants.RefCusCodeList.Codes.DeclarationPurpose.Others);
	}

	protected override void CheckCEI_TradeType()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_TradeTypeInfo, Parent.Lookups.TradeTypeList);
	}
}
