using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MX.Business.Testing
{
	class EntryInstructionProviderTest : TestCaseWithFactory
	{
		public void TestCustomsEntryInstructions()
		{
			var provider = new EntryInstructionProvider(Factory.New<JobDeclaration>());
			AssertType<CusEntryInstructionCollection<CusEntryInstruction>>(provider.CustomsEntryInstructions);
		}
	}
}
