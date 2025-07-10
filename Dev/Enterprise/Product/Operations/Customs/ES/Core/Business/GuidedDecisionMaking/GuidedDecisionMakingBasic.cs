using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class GuidedDecisionMakingBasic : EU.Business.GuidedDecisionMakingBasic
	{
		public GuidedDecisionMakingBasic(IESGuidedDecisionMakingSource source, BusinessObjectFactory factory) : base(source, factory)
		{
			DestinationStateIsCanaryIsland = source.DestinationStateIsCanaryIsland;
		}

		internal ZBool DestinationStateIsCanaryIsland { get; private set; }

		protected override EU.Business.GuidedDecisionMakingVATCollection GetNewGuidedDecisionMakingVATCollectionCore() => new GuidedDecisionMakingVATCollection(this);
	}
}
