using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusEntryInstructionValidation))]
sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCEI_DeclarationPurpose() => ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Instruction.CEI_DeclarationPurposeInfo, "3", "1");

	public void TestCEI_DeclarationPurposeDetails() => ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(Instruction.CEI_DeclarationPurposeDetailsInfo, Instruction.CEI_DeclarationPurposeInfo, (ZString)AEConstants.RefCusCodeList.Codes.DeclarationPurpose.Others);

	public void TestCEI_TradeType() => ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Instruction.CEI_TradeTypeInfo, "5", "2");

	protected override void SetUp()
	{
		base.SetUp();

		Declaration.JE_ApplicationCode = Core.Constants.CurrencyCodes.UnitedArabEmirates;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose, AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose, "1", "Commercial Samples for Exhibition", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose, "4", "Others", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CurrencyCodes.UnitedArabEmirates);
		Factory.Save();
	}

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
