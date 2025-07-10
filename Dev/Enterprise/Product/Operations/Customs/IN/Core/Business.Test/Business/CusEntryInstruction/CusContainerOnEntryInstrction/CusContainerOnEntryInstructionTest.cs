using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusContainerOnEntryInstruction))]
sealed class CusContainerOnEntryInstructionTest : NonPersistentBusinessObjectTestCase
{
	public void TestInstructionType()
	{
		AssertType<CusEntryInstruction>((GetNewBusinessObject() as CusContainerOnEntryInstruction).Instruction);
	}

	public void TestContainerType()
	{
		AssertType<CusContainer>((GetNewBusinessObject() as CusContainerOnEntryInstruction).Container);
	}

	public void TestValidation()
	{
		AssertType<CusContainerOnEntryInstructionValidation>(new CusContainerOnEntryInstruction(Instruction).Validation);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var container = Declaration.CusContainers.AddNew();
		Declaration.CustomsEntryInstructions.Add(Instruction);
		var result = new CusContainerOnEntryInstruction(Instruction);
		result.Container = container;
		result.IsForEntry = true;
		return result;
	}

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
