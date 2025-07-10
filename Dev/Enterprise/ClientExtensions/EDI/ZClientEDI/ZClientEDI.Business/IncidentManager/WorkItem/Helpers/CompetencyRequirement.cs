using System;

namespace Enterprise.Client.EDI;

public readonly struct CompetencyRequirement : IComparable<CompetencyRequirement>
{
	public CompetencyRequirement(Guid aspectPK)
	{
		AspectPK = aspectPK;
	}

	public Guid AspectPK { get; }

	public int CompareTo(CompetencyRequirement obj)
	{
		return AspectPK.CompareTo(obj.AspectPK);
	}

	public override bool Equals(object obj)
	{
		return obj is CompetencyRequirement other
			&& AspectPK == other.AspectPK;
	}

	public override int GetHashCode()
	{
		return AspectPK.GetHashCode();
	}
}
