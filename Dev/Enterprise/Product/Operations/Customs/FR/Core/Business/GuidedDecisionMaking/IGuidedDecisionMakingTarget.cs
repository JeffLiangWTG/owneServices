using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.GDM
{
	public interface IGuidedDecisionMakingTarget : EU.Business.IGuidedDecisionMakingTarget
	{
		ZString RegionOrTerritoryOfDestination { set; }
	}
}
