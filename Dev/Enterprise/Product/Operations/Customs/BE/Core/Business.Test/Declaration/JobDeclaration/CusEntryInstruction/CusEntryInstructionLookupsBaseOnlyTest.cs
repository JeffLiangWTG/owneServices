using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class CusEntryInstructionLookupsBaseOnlyTest : CusEntryInstructionLookupsAbstractTest<CusEntryInstructionLookups>
{
	public void TestEntrySubStyleList()
	{
		CombineAssertions(() =>
		{
			instruction.CEI_Style = ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportAndReExport;
			AssertEquals("Sub styles for B1", "A, D, R, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ExportCusEntryInstructionsDeclarationTypeList.Codes.OutwardProcessing;
			AssertEquals("Sub styles for B2", "A, D, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ExportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousingOfUnionGoods;
			AssertEquals("Sub styles for B3", "A, D, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ExportCusEntryInstructionsDeclarationTypeList.Codes.DispatchOfGoodsForTradeWithSpecialTerritories;
			AssertEquals("Sub styles for B4", "A, D, R, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportSimplified;
			AssertEquals("Sub styles for C1", "B, C, E, F, U, X, Y", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ExportCusEntryInstructionsDeclarationTypeList.Codes.T2L_T2LF;
			AssertEquals("Sub styles for E1", "A, B, C, D, E, F, R, U, V, X, Y, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation;
			AssertEquals("Sub styles for H1", "A, D, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing;
			AssertEquals("Sub styles for H2", "A, D, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission;
			AssertEquals("Sub styles for H3", "A, D, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing;
			AssertEquals("Sub styles for H4", "A, D, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods;
			AssertEquals("Sub styles for H5", "A, B, C, D, E, F, R, U, V, X, Y, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired;
			AssertEquals("Sub styles for H6", "A, D, V, Z", lookups.EntrySubStyleList.CodesAsString);

			instruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified;
			AssertEquals("Sub styles for I1", "B, C, E, F, U, X, Y", lookups.EntrySubStyleList.CodesAsString);
		});
	}

	public void TestParent() => AssertType<CusEntryInstruction>(lookups.Parent);

	protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

	protected override CusEntryInstructionLookups GetLookups() => new CusEntryInstructionLookups(instruction);
}
