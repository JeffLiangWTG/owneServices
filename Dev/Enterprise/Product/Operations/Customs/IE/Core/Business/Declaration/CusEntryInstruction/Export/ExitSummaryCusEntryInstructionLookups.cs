using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExitSummaryCusEntryInstructionLookups : CusEntryInstructionLookups
	{
		public ExitSummaryCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		protected override CodeDescriptionPairList DeclarationTypeListCore => Factory.GetCachedValue("IEExitSummaryDeclarationTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ExitSummaryDeclarationTypeList.Codes.A1, ExitSummaryDeclarationTypeList.Descriptions.A1);
			result.AddPair(ExitSummaryDeclarationTypeList.Codes.A2, ExitSummaryDeclarationTypeList.Descriptions.A2);
			return result;
		});
	}
}
