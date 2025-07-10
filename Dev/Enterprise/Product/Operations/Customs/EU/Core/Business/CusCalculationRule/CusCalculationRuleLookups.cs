using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class CusCalculationRuleLookups : Customs.Business.CusCalculationRuleLookups
	{
		public CusCalculationRuleLookups(AutoCusCalculationRule parent) : base(parent)
		{
			this.parent = (CusCalculationRule)parent;
		}

		readonly CusCalculationRule parent;

		public override CodeDescriptionPairList RuleTypeList => Factory.GetCachedValue<CusCalculationRuleTypeList>();

		public override CodeDescriptionPairList TransportModeList => Factory.GetCachedValue("EU|CusCalculationRuleLookups|TransportModeList", delegate
		{
			var transportModeList = new CodeDescriptionPairList();
			transportModeList.AddPair(TransportTypeList.Codes.Air, TransportTypeList.Descriptions.Air);
			transportModeList.AddPair(TransportTypeList.Codes.Sea, TransportTypeList.Descriptions.Sea);
			transportModeList.AddPair(TransportTypeList.Codes.Mail, TransportTypeList.Descriptions.Mail);
			return transportModeList;
		});

		public CodeDescriptionPairList BasedOnList
		{
			get
			{
				var isINSrule = parent.CCR_RuleType == CusCalculationRuleTypeList.Codes.INS;
				return Factory.GetCachedValue($"EU|CusCalculationRuleLookups|BasedOnList|INSrule-{isINSrule}", delegate
				{
					var basedOnList = new CodeDescriptionPairList();
					if (isINSrule)
					{
						basedOnList.AddPair(Core.Constants.IncoTerms.CostAndFreight, Core.Constants.IncoTerms.Descriptions.CostAndFreight);
						basedOnList.AddPair(Core.Constants.IncoTerms.FreeOnBoard, Core.Constants.IncoTerms.Descriptions.FreeOnBoard);
					}
					return basedOnList;
				});
			}
		}
	}
}
