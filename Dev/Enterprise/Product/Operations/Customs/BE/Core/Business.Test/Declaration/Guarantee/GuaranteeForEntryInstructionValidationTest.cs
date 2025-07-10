using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class GuaranteeForEntryInstructionValidationTest : EU.Business.Declaration.Testing.CommonGuaranteeValidationTest
{
	GuaranteeForEntryInstruction CreateGuarantee()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var guarantee = entryInstruction.Guarantee;

		return guarantee;
	}

	public void TestCheckPW_BondType_MaxLength()
	{
		var guarantee = CreateGuarantee();

		AssertEquals(5, guarantee.PW_BondTypeInfo.MaxLength);

		guarantee.PW_BondType = GuaranteeSubTypeList.Codes.ComprehensiveGuarantee;
		guarantee.Validation.ValidatePW_BondType();
		AssertNoMessageErrorContaining(guarantee.PW_BondTypeInfo, ListValidation.InvalidCodeMessageError);

		guarantee.PW_BondType = null;
		guarantee.Validation.ValidatePW_BondType();
		AssertNoMessageErrorContaining(guarantee.PW_BondTypeInfo, ListValidation.InvalidCodeMessageError);

		guarantee.PW_BondType = "X";
		guarantee.Validation.ValidatePW_BondType();
		AssertHasMessageErrorContaining(guarantee.PW_BondTypeInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckPW_BondNumber()
	{
		var guarantee = CreateGuarantee();

		guarantee.PW_BondNumber = "letters123";
		guarantee.Validation.ValidatePW_BondNumber();
		AssertNoErrors(guarantee.PW_BondNumberInfo);

		AssertEquals(35, guarantee.PW_BondNumberInfo.MaxLength);
	}

	public void TestCheckPW_HolderIdentification()
	{
		var guarantee = CreateGuarantee();

		guarantee.PW_HolderIdentification = "WrongCode";
		guarantee.Validation.ValidatePW_HolderIdentification();
		AssertHasMessageErrors("No custom office will use this code", guarantee.PW_HolderIdentificationInfo);

		AssertEquals(35, guarantee.PW_HolderIdentificationInfo.MaxLength);
	}

	public void TestPW_Password()
	{
		var guarantee = CreateGuarantee();
		AssertNoNotifications("No messages, Password is not on the screen", guarantee.PW_PasswordInfo);
	}

	public void TestPW_BondFiledPort()
	{
		var guarantee = CreateGuarantee();
		AssertNoNotifications("No messages, BondFiledPort (Customs Office) is not on the screen", guarantee.PW_BondFiledPortInfo);
	}

	public void TestMandatoryValidationForImport()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = "IMP";
		var cusEntryInstruction = dec.CustomsEntryInstructions.AddNew();
		var guarantee = cusEntryInstruction.Guarantee;

		guarantee.Validation.ValidatePW_BondType();
		guarantee.Validation.ValidatePW_BondNumber();
		guarantee.Validation.ValidatePW_HolderIdentification();
		guarantee.Validation.ValidatePW_BondEffectiveDate();

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining(guarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(guarantee.PW_BondNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(guarantee.PW_HolderIdentificationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(guarantee.PW_BondEffectiveDateInfo, MandatoryValidation.YouHaveNotEntered);
		});

		dec.JE_MessageType = "EXP";
		AssertEquals("Export declaration should not have a guarantee.", true, guarantee.IsDeleted);
	}

	public void TestCheckBondNumberOrBondNumber2IsRequired()
	{
		var guaranteeValidation = new GuaranteeForEntryInstructionValidationForTest(CreateGuarantee());
		AssertEquals("Only BondNumber is available on screen, no message about BondNumber2.", false, guaranteeValidation.CheckBondNumberOrBondNumber2IsRequired_Exposed);
	}

	sealed class GuaranteeForEntryInstructionValidationForTest : GuaranteeForEntryInstructionValidation
	{
		public GuaranteeForEntryInstructionValidationForTest(GuaranteeForEntryInstruction parent) : base(parent)
		{
		}

		public bool CheckBondNumberOrBondNumber2IsRequired_Exposed => base.CheckBondNumberOrBondNumber2IsRequired;
	}
}
