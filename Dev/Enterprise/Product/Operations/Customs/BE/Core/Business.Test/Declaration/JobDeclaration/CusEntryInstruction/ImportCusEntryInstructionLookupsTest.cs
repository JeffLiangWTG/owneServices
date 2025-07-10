using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ImportCusEntryInstructionLookupsTest : CusEntryInstructionLookupsAbstractTest<ImportCusEntryInstructionLookups>
{
	public void TestDeclarationTypesList()
	{
		CombineAssertions(() =>
		{
			AssertEquals("list codes", "H1, H2, H3, H4, H5, H6, I1", lookups.DeclarationTypeList.CodesAsString);
			AssertSame("cache", lookups.DeclarationTypeList, lookups.DeclarationTypeList);
		});
	}

	protected override string MessageType => JobMessageTypeList.Codes.Import;

	protected override ImportCusEntryInstructionLookups GetLookups() => new ImportCusEntryInstructionLookups(instruction);
}
