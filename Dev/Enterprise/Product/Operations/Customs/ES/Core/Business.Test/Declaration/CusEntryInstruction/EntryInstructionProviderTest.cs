using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class EntryInstructionProviderTest : TestCaseWithFactory
	{
		public void TestCusEntryInstructionCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstructionProvider = new EntryInstructionProvider(declaration);
			AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(entryInstructionProvider.CustomsEntryInstructions);
		}
	}
}
