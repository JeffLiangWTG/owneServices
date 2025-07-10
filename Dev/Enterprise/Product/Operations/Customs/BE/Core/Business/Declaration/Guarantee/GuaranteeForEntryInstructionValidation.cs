using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Declaration;

public class GuaranteeForEntryInstructionValidation : EU.Business.Declaration.GuaranteeForEntryInstructionValidation
{
	public GuaranteeForEntryInstructionValidation(GuaranteeForEntryInstruction parent) : base(parent)
	{
	}

	protected override void CheckPW_BondType()
	{
		base.CheckPW_BondType();
		ListValidation.MessageErrorIfInvalidCode(Parent.PW_BondTypeInfo);
	}

	protected override void CheckBondTypeMandatoryValidation()
	{
		var dec = (Parent as GuaranteeForEntryInstruction)?.EntryInstruction?.JobDeclaration;
		if (dec?.IsImport == true)
		{
			base.CheckBondTypeMandatoryValidation();
		}
	}

	protected override void CheckPW_HolderIdentification()
	{
		base.CheckPW_HolderIdentification();
		ListValidation.MessageErrorIfInvalidCode(Parent.PW_HolderIdentificationInfo);
		ValidateMandatoryForImport(Parent.PW_HolderIdentificationInfo);
	}

	protected override void CheckPW_BondEffectiveDate()
	{
		base.CheckPW_BondEffectiveDate();
		ValidateMandatoryForImport(Parent.PW_BondEffectiveDateInfo);
	}

	protected override void CheckPW_BondNumber()
	{
		base.CheckPW_BondNumber();
		ValidateMandatoryForImport(Parent.PW_BondNumberInfo);
	}

	protected override void CheckPW_Password()
	{
		// No validation for 'Password' since it is not on the screen
	}

	protected override void CheckPW_BondFiledPort()
	{
		// No validation for 'Customs office' since it is not on the screen
	}

	void ValidateMandatoryForImport(ZPropertyInfo propertyInfo)
	{
		var dec = (Parent as GuaranteeForEntryInstruction)?.EntryInstruction?.JobDeclaration;
		if (dec?.IsImport == true)
		{
			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
		}
	}

	protected override bool CheckBondNumberOrBondNumber2IsRequired => false;
}
