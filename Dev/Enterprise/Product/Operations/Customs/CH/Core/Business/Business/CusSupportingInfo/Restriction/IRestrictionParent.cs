using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public interface IRestrictionParent
{
	RestrictionCollection Restrictions { get; }

	HugeSequenceNumberGenerator RestrictionsLineNumberGenerator { get; }

	ZDateTime EffectiveAssessmentDate { get; }

	ZGuid PK { get; }

	string TablePrefix { get; }
}
