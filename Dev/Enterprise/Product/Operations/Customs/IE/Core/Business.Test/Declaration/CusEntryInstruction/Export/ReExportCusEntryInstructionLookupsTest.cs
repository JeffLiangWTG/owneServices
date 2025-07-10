using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ReExportCusEntryInstructionLookupsTest : CusEntryInstructionLookupsAbstractTest<ReExportCusEntryInstructionLookups>
	{
		protected override string MessageType => IEJobMessageTypeList.Codes.ReExport;

		protected override (string code, string desc)[] GetExpectedData() => new (string code, string desc)[]
		{
			(ReExportDeclarationTypeList.Codes.A3, ReExportDeclarationTypeList.Descriptions.A3)
		};
	}
}
