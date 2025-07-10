using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	sealed class UCC6ExportEntryInstructionValidationDecider : IEntryInstructionValidationDecider
	{
		public bool IsRuleC0619ActiveForGoodsLocationDescription => false;
		public bool IsRuleC0626ActiveForCEI_OA_Warehouse2 => false;
		public bool IsRuleC0628ActiveForGoodsLocationDescription => false;
		public bool IsRuleC0829ActiveForCEI_OA_Warehouse2 => false;
		public bool IsRuleC0853ActiveForCEI_OA_Warehouse => false;
		public bool IsRuleC0382ActiveForGoodsLocationAddressHouseNumber => true;
		public bool IsMaximumEntryLinesAllowedRuleActive => false;
	}
}
