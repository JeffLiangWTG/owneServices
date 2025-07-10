using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class GuaranteeForEntryInstructionValidation : EU.Business.Declaration.GuaranteeForEntryInstructionValidation
{
	public GuaranteeForEntryInstructionValidation(GuaranteeForEntryInstruction parent) : base(parent)
	{
	}

	new GuaranteeForEntryInstruction Parent => (GuaranteeForEntryInstruction)base.Parent;

	protected override void CheckPW_BondNumber()
	{
		base.CheckPW_BondNumber();

		CheckGuaranteeNumberShouldBeEnteredInReference2(Parent.PW_BondNumberInfo);
	}

	void CheckGuaranteeNumberShouldBeEnteredInReference2(ZPropertyInfo targetPropertyInfo)
	{
		var requireGuaranteeNumberInReference2 = Parent.EntryInstruction?.RequireGuaranteeNumberInReference2 ?? false;
		if (requireGuaranteeNumberInReference2 && !Parent.PW_BondNumber.IsEmpty && Parent.PW_BondNumber2.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.GuaranteeForEntryInstruction.GuaranteeNumberShouldBeEnteredInReference2);
		}
	}
}
