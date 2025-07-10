using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class EntryInstructionProviderTest : TestCaseWithFactory
{
	public void TestCusEntryInstructionCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstructionProvider = new EntryInstructionProvider(declaration);
		AssertType<CusEntryInstructionCollection>(entryInstructionProvider.CustomsEntryInstructions);
	}
}
