using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class EntryInstructionProviderTest : TestCaseWithFactory
{
	public void TestCustomsEntryInstructions()
	{
		var declaration = Factory.New<JobDeclaration>();
		var provider = new EntryInstructionProvider(declaration);
		AssertType<CusEntryInstructionCollection>(provider.CustomsEntryInstructions);
	}
}
