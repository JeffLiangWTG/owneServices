using System.Collections.Immutable;
using System.Linq;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI
{
	public readonly struct AssessRequirementsState
	{
		public static AssessRequirementsState AllRequirementsUnmet(TaskCompetencyRequirements allRequirements, ImmutableArray<GlbStaff> staffForWhomAssessIsRequired)
			=> new AssessRequirementsState(allRequirements, allRequirements, staffForWhomAssessIsRequired);

		public static AssessRequirementsState WithUnmetRequirements(TaskCompetencyRequirements allRequirements, TaskCompetencyRequirements unmetRequirements, ImmutableArray<GlbStaff> staffForWhomAssessIsRequired)
			=> new AssessRequirementsState(allRequirements, unmetRequirements, staffForWhomAssessIsRequired);

		public static AssessRequirementsState Empty
			=> new AssessRequirementsState(TaskCompetencyRequirements.Empty, TaskCompetencyRequirements.Empty, ImmutableArray<GlbStaff>.Empty);

		AssessRequirementsState(TaskCompetencyRequirements allRequirements, TaskCompetencyRequirements unmetRequirements, ImmutableArray<GlbStaff> staffForWhomAssessIsRequired)
		{
			AllRequirements = allRequirements;
			UnmetRequirements = unmetRequirements;
			CompletedRequirements = new TaskCompetencyRequirements(allRequirements.CompetencyRequirements.Except(unmetRequirements.CompetencyRequirements).OrderBy(r => r).ToImmutableArray());
			StaffForWhomAssessIsRequired = staffForWhomAssessIsRequired;
		}

		public TaskCompetencyRequirements AllRequirements { get; }
		public TaskCompetencyRequirements UnmetRequirements { get; }
		public TaskCompetencyRequirements CompletedRequirements { get; }

		public ImmutableArray<GlbStaff> StaffForWhomAssessIsRequired { get; }

		public bool IsEmpty => AllRequirements.IsEmpty;
	}
}
