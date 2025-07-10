using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class CusEntryInstructionLookupsBaseOnlyTest : CusEntryInstructionLookupsAbstractTest<CusEntryInstructionLookups>
	{
		public void TestParent() => AssertType<CusEntryInstruction>(lookups.Parent);

		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override (string code, string desc)[] GetExpectedData() => new (string code, string desc)[]
		{
			(ImportDeclarationTypeList.Codes.H1, ImportDeclarationTypeList.Descriptions.H1),
			(ImportDeclarationTypeList.Codes.H2, ImportDeclarationTypeList.Descriptions.H2),
			(ImportDeclarationTypeList.Codes.H3, ImportDeclarationTypeList.Descriptions.H3),
			(ImportDeclarationTypeList.Codes.H4, ImportDeclarationTypeList.Descriptions.H4),
			(ImportDeclarationTypeList.Codes.H5, ImportDeclarationTypeList.Descriptions.H5),
			(ImportDeclarationTypeList.Codes.H6, ImportDeclarationTypeList.Descriptions.H6),
			(ImportDeclarationTypeList.Codes.H7, ImportDeclarationTypeList.Descriptions.H7),
			(ImportDeclarationTypeList.Codes.I1, ImportDeclarationTypeList.Descriptions.I1)
		};
	}
}
