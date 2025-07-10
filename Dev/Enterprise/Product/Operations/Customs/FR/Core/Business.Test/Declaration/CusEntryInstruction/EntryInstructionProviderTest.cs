using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class EntryInstructionProviderTest : TestCaseWithFactory
	{
		public void TestCusEntryInstructionCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstructionProvider = new EntryInstructionProvider(declaration);
			AssertType<CusEntryInstructionCollection>(entryInstructionProvider.CustomsEntryInstructions);
		}
	}
}
