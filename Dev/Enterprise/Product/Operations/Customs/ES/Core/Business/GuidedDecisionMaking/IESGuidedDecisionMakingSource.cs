using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public interface IESGuidedDecisionMakingSource : EU.Business.IGuidedDecisionMakingSource
	{
		ZBool DestinationStateIsCanaryIsland { get; }
	}
}
