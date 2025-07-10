using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.GDM
{
	public interface IGuidedDecisionMakingSource : EU.Business.IGuidedDecisionMakingSource
	{
		ZString RegionOrTerritoryOfDestination { get; }
	}
}
