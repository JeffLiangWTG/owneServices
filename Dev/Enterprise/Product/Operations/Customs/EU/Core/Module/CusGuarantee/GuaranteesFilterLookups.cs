using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module
{
	public class GuaranteesFilterLookups : Customs.Module.GuaranteesFilterLookups
	{
		public GuaranteesFilterLookups(FilterStripBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public CodeDescriptionPairList GuaranteeRuleCodeList => GetGuaranteeRuleCodeList();

		protected virtual CodeDescriptionPairList GetGuaranteeRuleCodeList()
		{
			var codeList = new CodeDescriptionPairList();
			codeList.AddPair(PermitRuleCodeList.Codes.TSP, PermitRuleCodeList.Descriptions.TSP);
			return codeList;
		}
	}
}
