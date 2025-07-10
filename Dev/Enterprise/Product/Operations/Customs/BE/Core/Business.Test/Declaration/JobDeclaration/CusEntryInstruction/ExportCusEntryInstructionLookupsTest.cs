using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ExportCusEntryInstructionLookupsTest : CusEntryInstructionLookupsAbstractTest<ExportCusEntryInstructionLookups>
{
	public void TestDeclarationTypesList()
	{
		CombineAssertions(() =>
		{
			AssertEquals("list codes", "B1, B2, B3, B4, C1, E1", lookups.DeclarationTypeList.CodesAsString);
			AssertSame("cache", lookups.DeclarationTypeList, lookups.DeclarationTypeList);
		});
	}

	protected override string MessageType => JobMessageTypeList.Codes.Export;

	protected override ExportCusEntryInstructionLookups GetLookups() => new ExportCusEntryInstructionLookups(instruction);
}
