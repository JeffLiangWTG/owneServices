using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportActiveBorderGroupWithEntryInstructionTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertExceptionThrown<ArgumentNullException>(() => new Ucc6ExportActiveBorderGroupWithEntryInstruction(declaration, entryInstruction: null));
		AssertNoExceptionThrown(() => new Ucc6ExportActiveBorderGroupWithEntryInstruction(declaration, Factory.New<CusEntryInstruction>()));
	}

	public void TestGetEntryInstructions()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var activeBorderGroupWithEntryInstruction = new Ucc6ExportActiveBorderGroupWithEntryInstructionForTest(declaration, entryInstruction);
		var entryInstructions = activeBorderGroupWithEntryInstruction.GetEntryInstructions_Exposed().ToArray();
		AssertEquals("Instruction with procedure code", 1, entryInstructions.Length);
		AssertEquals("Entry Instruction", entryInstruction.PK, entryInstructions[0].PK);
	}
}

sealed class Ucc6ExportActiveBorderGroupWithEntryInstructionForTest : Ucc6ExportActiveBorderGroupWithEntryInstruction
{
	public Ucc6ExportActiveBorderGroupWithEntryInstructionForTest(JobDeclaration declaration, CusEntryInstruction entryInstruction) : base(declaration, entryInstruction)
	{
	}

	internal IEnumerable<CusEntryInstruction> GetEntryInstructions_Exposed() => GetEntryInstructions();
}
