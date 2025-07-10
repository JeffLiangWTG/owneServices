using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class EntryInstructionProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestCustomsEntryInstructions()
		{
			AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(provider.CustomsEntryInstructions);
		}

		public void TestEntryInstructionComparer()
		{
			AssertType<CusEntryInstructionComparer>(provider.EntryInstructionComparer);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new EntryInstructionProvider(Factory.New<JobDeclaration>(), new CusEntryInstructionComparer());
		}
		EntryInstructionProvider provider;
	}
}
