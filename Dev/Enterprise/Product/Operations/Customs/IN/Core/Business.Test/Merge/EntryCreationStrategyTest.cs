using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business.Testing;

sealed class EntryCreationStrategyTest : Customs.Business.Testing.EntryCreationStrategyTest
{
	public void TestGetKeyForLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var strategy = new EntryCreationStrategy(declaration);
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		var key = strategy.GetKeyForLine(invoiceLine);
		AssertEquals(1, key.Keys.Count);
		Assert(key.Contains(invoiceLine.PK));
	}

	public override void TestGetKeyForHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var provider = declaration.CustomsEntryInstructionProvider;
		var entryInstr = provider.CustomsEntryInstructions.AddNew();
		var invHead1 = declaration.Invoices.AddNew();
		var invLine1 = invHead1.JobComInvoiceLines.AddNew();
		invLine1.JI_CEI = entryInstr.PK;

		var entryCreationStrategy = new EntryCreationStrategy(declaration);
		AssertEquals("Entry Instruction in header", false, provider.IsNoEntryInstruction);
		Assert("Entry Instruction pk", entryCreationStrategy.GetKeyForHeader(invLine1).Contains(entryInstr.PK));

		var entryManager = new EntryManager(declaration, entryCreationStrategy);
		var entryLine = entryManager.GetOrCreateEntryLine(invLine1);
		AssertEquals(entryInstr.PK, entryLine.Header.CH_CEI_Instruction);
	}
}
