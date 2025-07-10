using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportCusEntryInstructionLookupsTest : CusEntryInstructionLookupsAbstractTest<ImportCusEntryInstructionLookups>
	{
		public void TestEntrySubStyleList_Cached()
		{
			AssertSame(lookups.EntrySubStyleList, lookups.EntrySubStyleList);
		}

		public void TestEntrySubStyleList_DeclarationTypeIsEmpty() => AssertEntrySubStyleList(ZString.Empty, ZString.Empty);

		public void TestEntrySubStyleList_DeclarationTypeIsH1() => AssertEntrySubStyleList(ImportDeclarationTypeList.Codes.H1, "A, D, Y, Z");

		public void TestEntrySubStyleList_DeclarationTypeIsH2() => AssertEntrySubStyleList(ImportDeclarationTypeList.Codes.H2, "A, D, Y, Z");

		public void TestEntrySubStyleList_DeclarationTypeIsH3() => AssertEntrySubStyleList(ImportDeclarationTypeList.Codes.H3, "A, D, Y, Z");

		public void TestEntrySubStyleList_DeclarationTypeIsH4() => AssertEntrySubStyleList(ImportDeclarationTypeList.Codes.H4, "A, D, Y, Z");

		public void TestEntrySubStyleList_DeclarationTypeIsH5() => AssertEntrySubStyleList(ImportDeclarationTypeList.Codes.H5, ZString.Empty);

		public void TestEntrySubStyleList_DeclarationTypeIsI1() => AssertEntrySubStyleList(ImportDeclarationTypeList.Codes.I1, "C, F, Z");

		public void TestGetDefinedDeclarationTypeList_CodesAsString()
		{
			AssertEquals("UCC6 Declaration Type List", "H1, H2, H3, H4, H5, H6, I1", instruction.Lookups.DeclarationTypeList.CodesAsString);
		}

		protected override string MessageType => MessageTypeList.Codes.Import;

		void AssertEntrySubStyleList(ZString declarationType, ZString expectedCodesAsString)
		{
			jobDeclaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			instruction.CEI_Style = declarationType;
			AssertEquals($"Expecting EntrySubStyleList {expectedCodesAsString} when CEI_Style {declarationType} under UCC5", expectedCodesAsString, lookups.EntrySubStyleList.CodesAsString);

			jobDeclaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			instruction.CEI_Style = declarationType;
			AssertEquals($"Expecting EntrySubStyleList {expectedCodesAsString} when CEI_Style {declarationType} under UCC6", expectedCodesAsString, lookups.EntrySubStyleList.CodesAsString);
		}

		protected override (string code, string desc)[] GetExpectedData() => new (string code, string desc)[]
		{
			(ImportDeclarationTypeList.Codes.H1, ImportDeclarationTypeList.Descriptions.H1),
			(ImportDeclarationTypeList.Codes.H2, ImportDeclarationTypeList.Descriptions.H2),
			(ImportDeclarationTypeList.Codes.H3, ImportDeclarationTypeList.Descriptions.H3),
			(ImportDeclarationTypeList.Codes.H4, ImportDeclarationTypeList.Descriptions.H4),
			(ImportDeclarationTypeList.Codes.H5, ImportDeclarationTypeList.Descriptions.H5),
			(ImportDeclarationTypeList.Codes.H6, ImportDeclarationTypeList.Descriptions.H6),
			(ImportDeclarationTypeList.Codes.I1, ImportDeclarationTypeList.Descriptions.I1)
		};
	}
}
