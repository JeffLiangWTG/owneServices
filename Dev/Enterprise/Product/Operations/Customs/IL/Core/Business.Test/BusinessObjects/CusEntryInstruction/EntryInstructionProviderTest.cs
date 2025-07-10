using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class EntryInstructionProviderTest : TestCaseWithFactory
	{
		public void TestCustomsEntryInstructions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var provider = new EntryInstructionProvider(declaration);
			AssertType<CusEntryInstructionCollection<CusEntryInstruction>>(provider.CustomsEntryInstructions);
		}
	}
}
