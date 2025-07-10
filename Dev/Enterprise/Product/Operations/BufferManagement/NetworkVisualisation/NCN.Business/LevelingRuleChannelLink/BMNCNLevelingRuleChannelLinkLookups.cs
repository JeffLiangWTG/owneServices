using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNLevelingRuleChannelLinkLookups : AutoBMNCNLevelingRuleChannelLinkLookups
	{
		public BMNCNLevelingRuleChannelLinkLookups(AutoBMNCNLevelingRuleChannelLink parent)
			: base(parent)
		{
		}

		new BMNCNLevelingRuleChannelLink Parent => (BMNCNLevelingRuleChannelLink)base.Parent;

		BMNCNRootDiagramShape Diagram => Parent.Rule.Diagram;

		public IBusinessObjectCollection Channels => Factory.GetCachedValue("BMNCNLevelingRuleChannelLinkLookups|Channels|" + Diagram.PK, () => new BMNCNChannelCollection(Diagram));
	}
}
