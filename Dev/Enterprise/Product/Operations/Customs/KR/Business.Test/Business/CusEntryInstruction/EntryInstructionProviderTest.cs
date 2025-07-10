using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EntryInstructionProviderTest : TestCaseWithFactory
	{
		public void TestCustomsEntryInstructions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var provider = new EntryInstructionProvider(declaration);
			AssertType<CusEntryInstructionCollection<CusEntryInstruction>>(provider.CustomsEntryInstructions);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			provider = new EntryInstructionProvider(declaration);
			AssertType<CusEntryInstructionCollection<CusEntryInstruction>>(provider.CustomsEntryInstructions);
		}
	}
}
