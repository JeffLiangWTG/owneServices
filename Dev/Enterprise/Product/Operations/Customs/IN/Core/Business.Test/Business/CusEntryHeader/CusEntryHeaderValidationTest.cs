using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusEntryHeaderValidation))]
sealed class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
{
	public void TestCheckCH_Status()
	{
		var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = Instruction.PK;
		Instruction.StatusOverride = true;
		ValidationTestHelper.AssertErrorIfInvalidCode(entryHeader.CH_StatusInfo, "ABC", "SNT");

		Instruction.MessageStatus = "ABC";
		AssertHasErrorContaining(Instruction.MessageStatusInfo, "Enter a valid Msg. Status");

		Instruction.StatusOverride = false;
		entryHeader.CH_Status = "ABC";
		AssertNoErrors(entryHeader.CH_StatusInfo);
	}

	public void TestCheckCH_EntryStatus()
	{
		var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = Instruction.PK;
		Instruction.StatusOverride = true;
		ValidationTestHelper.AssertErrorIfInvalidCode(entryHeader.CH_EntryStatusInfo, "DEF", "SBA");

		Instruction.CustomsStatus = "DEF";
		AssertHasErrorContaining(Instruction.CustomsStatusInfo, "Enter a valid Ent. Status");

		Instruction.StatusOverride = false;
		entryHeader.CH_EntryStatus = "DEF";
		AssertNoErrors(entryHeader.CH_EntryStatusInfo);
	}

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
