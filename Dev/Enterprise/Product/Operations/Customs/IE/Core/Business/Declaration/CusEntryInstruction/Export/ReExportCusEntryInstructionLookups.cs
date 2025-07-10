using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ReExportCusEntryInstructionLookups : CusEntryInstructionLookups
	{
		public ReExportCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		protected override CodeDescriptionPairList DeclarationTypeListCore => Factory.GetCachedValue("IEReExportDeclarationTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ReExportDeclarationTypeList.Codes.A3, ReExportDeclarationTypeList.Descriptions.A3);
			return result;
		});
	}
}
