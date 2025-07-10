using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNLevelingRuleLookups : AutoBMNCNLevelingRuleLookups
	{
		public BMNCNLevelingRuleLookups(AutoBMNCNLevelingRule parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList RuleTypes => Factory.GetCachedValue<LevelingRuleTypeList>();

		public ColorList ColorList => Factory.GetCachedValue<ColorList>();
	}
}
