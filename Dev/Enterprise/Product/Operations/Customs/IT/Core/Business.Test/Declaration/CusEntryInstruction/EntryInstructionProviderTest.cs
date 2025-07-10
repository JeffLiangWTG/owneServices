namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryInstructionProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
{
	public void TestCustomsEntryInstructions()
	{
		var declaration = Factory.New<JobDeclaration>();
		var provider = new EntryInstructionProvider(declaration);
		AssertType<CusEntryInstructionCollection>(provider.CustomsEntryInstructions);
	}
}
