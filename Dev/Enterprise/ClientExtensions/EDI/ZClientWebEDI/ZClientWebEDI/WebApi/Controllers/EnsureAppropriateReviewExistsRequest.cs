using System;
using System.Collections.Generic;
using Enterprise.Client.EDI;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class EnsureAppropriateReviewExistsRequest : ITaskCompetencyRequirementsProvider
	{
		public Guid TaskPK { get; set; }
		public IReadOnlyList<Guid> AspectPKs { get; set; }
		public IReadOnlyList<Guid> RequiredSkills { get; set; }
		public IReadOnlyList<string> WTASubjectCodes { get; set; }

		IEnumerable<CompetencyRequirement> ITaskCompetencyRequirementsProvider.GetPendingCompetencyRequirements()
		{
			foreach (var aspectPK in AspectPKs)
			{
				yield return new CompetencyRequirement(aspectPK);
			}
		}
	}
}
