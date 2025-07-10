using System.Collections.Immutable;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public readonly struct TaskCompetencyRequirements
	{
		public static TaskCompetencyRequirements Empty => new(ImmutableArray<CompetencyRequirement>.Empty);

		public TaskCompetencyRequirements(ImmutableArray<CompetencyRequirement> competencyRequirements)
		{
			CompetencyRequirements = competencyRequirements;
		}

		public ImmutableArray<CompetencyRequirement> CompetencyRequirements { get; }

		public bool IsEmpty => CompetencyRequirements.IsEmpty;
	}
}
