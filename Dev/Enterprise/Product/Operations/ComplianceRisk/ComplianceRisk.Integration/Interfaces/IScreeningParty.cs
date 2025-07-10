using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface IScreeningParty
	{
		BusinessObject Parent { get; }
		ZString Description { get; }
		BusinessObject Party { get; }
		INaturalPerson NaturalPerson { get; }
	}
}
