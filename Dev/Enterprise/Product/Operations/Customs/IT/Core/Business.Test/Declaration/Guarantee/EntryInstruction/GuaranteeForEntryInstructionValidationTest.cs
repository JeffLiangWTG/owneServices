using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class GuaranteeForEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckPW_BondNumber_GuaranteeNumberShouldBeEnteredInReference2_WhenInstructionIsH3()
	{
		CombineAssertions(() => CheckGuaranteeNumberShouldBeEnteredInReference2Validation(entryInstructionStyle: "H3"));
	}

	public void TestCheckPW_BondNumber_GuaranteeNumberShouldBeEnteredInReference2_WhenInstructionIsH4()
	{
		CombineAssertions(() => CheckGuaranteeNumberShouldBeEnteredInReference2Validation(entryInstructionStyle: "H4"));
	}

	public void TestCheckPW_BondNumber_NoMessageErrors_WhenInstructionIsNotH3NorH4()
	{
		entryInstruction.CEI_Style = "H1";
		guarantee.PW_BondNumber2 = "";
		guarantee.PW_BondNumber = "XYZ";
		AssertNoMessageErrors(guarantee.PW_BondNumberInfo);
	}

	public void TestCheckPW_BondNumber_NoMessageErrors_WhenGuaranteeDoesNotHaveAnInstruction()
	{
		var guarantee = Factory.New<GuaranteeForEntryInstruction>();
		guarantee.PW_BondNumber2 = "";
		guarantee.PW_BondNumber = "XYZ";
		AssertNoMessageErrors(guarantee.PW_BondNumberInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		guarantee = entryInstruction.Guarantees.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	GuaranteeForEntryInstruction guarantee;

	void CheckGuaranteeNumberShouldBeEnteredInReference2Validation(string entryInstructionStyle)
	{
		const string expectedMessageError = "For H3 or H4 message the Guarantee Number should be entered in Reference 2";

		entryInstruction.CEI_Style = entryInstructionStyle;
		guarantee.PW_BondNumber2 = "";
		guarantee.PW_BondNumber = "";
		AssertNoMessageErrorContaining("When both PW_BondNumber and PW_BondNumber2 are empty", guarantee.PW_BondNumberInfo, expectedMessageError);

		guarantee.PW_BondNumber2 = "";
		guarantee.PW_BondNumber = "XYZ";
		AssertHasMessageErrorContaining("When only PW_BondNumber is filled", guarantee.PW_BondNumberInfo, expectedMessageError);

		guarantee.PW_BondNumber2 = "ABC";
		guarantee.PW_BondNumber = "";
		AssertNoMessageErrorContaining("When only PW_BondNumber2 is filled", guarantee.PW_BondNumberInfo, expectedMessageError);

		guarantee.PW_BondNumber2 = "ABC";
		guarantee.PW_BondNumber = "DEF";
		AssertNoMessageErrorContaining("When both PW_BondNumber and PW_BondNumber2 are filled", guarantee.PW_BondNumberInfo, expectedMessageError);
	}
}
