using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusContainerOnEntryInstructionValidation))]
sealed class CusContainerOnEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckIsForEntry()
	{
		var message = "Only 99 containers are allowed per Entry Instructions.";

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var container = Declaration.CusContainers.AddNew();
		var result = new CusContainerOnEntryInstruction(Instruction);
		result.Container = container;
		result.IsForEntry = true;
		CombineAssertions(() =>
		{
			result.Validation.ValidateIsForEntry();
			AssertNoMessageError("Not greater than 99", result.IsForEntryInfo, message);

			CusContainer newContainer;
			CusContainerOnEntryInstruction newResult = null;
			for (int i = 0; i < 99; i++)
			{
				newContainer = Declaration.CusContainers.AddNew();
				newResult = new CusContainerOnEntryInstruction(Instruction);
				newResult.Container = newContainer;
				newResult.IsForEntry = true;
			}
			newResult.Validation.ValidateIsForEntry();
			AssertHasMessageError("Greater than 99", newResult.IsForEntryInfo, message);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			newResult.Validation.ValidateIsForEntry();
			AssertNoMessageError("Not Export", newResult.IsForEntryInfo, message);
		});
	}

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
