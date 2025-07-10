using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNLevelingRuleChannelLinkCollection : ActiveBusinessObjectCollection<BMNCNLevelingRuleChannelLink>
	{
		public BMNCNLevelingRuleChannelLinkCollection(BMNCNLevelingRule rule)
			: base(rule.Factory, rule, new ZQuery(), BMNCNLevelingRuleChannelLinkSchema.BNK_BNR_Rule)
		{
		}
	}
}
