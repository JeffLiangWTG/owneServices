using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExitSummaryCusEntryInstructionLookupsTest : CusEntryInstructionLookupsAbstractTest<ExitSummaryCusEntryInstructionLookups>
	{
		protected override string MessageType => IEJobMessageTypeList.Codes.ExitSummary;

		protected override (string code, string desc)[] GetExpectedData() => new (string code, string desc)[]
		{
			(ExitSummaryDeclarationTypeList.Codes.A1, ExitSummaryDeclarationTypeList.Descriptions.A1),
			(ExitSummaryDeclarationTypeList.Codes.A2, ExitSummaryDeclarationTypeList.Descriptions.A2)
		};
	}
}
