using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusCalculationRuleLookups : Customs.Business.CusCalculationRuleLookups
	{
		public CusCalculationRuleLookups(AutoCusCalculationRule parent) : base(parent)
		{
			this.parent = (CusCalculationRule)parent; 
		}

		readonly CusCalculationRule parent;

		public override CodeDescriptionPairList RuleTypeList => Factory.GetCachedValue<CusCalculationRuleTypeList>();

		public override CodeDescriptionPairList TransportModeList => Factory.GetCachedValue("CusCalculationRuleLookups|TransportModeList", delegate
		{
			var transportModeList = new CodeDescriptionPairList();
			transportModeList.AddPair(TransportTypeList.Codes.Air, TransportTypeList.Descriptions.Air);
			transportModeList.AddPair(TransportTypeList.Codes.Sea, TransportTypeList.Descriptions.Sea);
			transportModeList.AddPair(TransportTypeList.Codes.Mail, TransportTypeList.Descriptions.Mail);
			return transportModeList;
		});

		public CodeDescriptionPairList BasedOnList => Factory.GetCachedValue("CusCalculationRuleLookups|BasedOnList", delegate
		{
			var basedOnList = new CodeDescriptionPairList();
			if (parent.CCR_RuleType == CusCalculationRuleTypeList.Codes.INS)
			{
				basedOnList.AddPair(CusCalculationBasedOnList.Codes.CFR, CusCalculationBasedOnList.Descriptions.CFR);
				basedOnList.AddPair(CusCalculationBasedOnList.Codes.FOB, CusCalculationBasedOnList.Descriptions.FOB);
			}
			return basedOnList;
		});
	}
}
