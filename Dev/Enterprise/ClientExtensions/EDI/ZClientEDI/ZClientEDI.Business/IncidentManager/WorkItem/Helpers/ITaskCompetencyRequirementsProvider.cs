using System.Collections.Generic;

namespace Enterprise.Client.EDI
{
	public interface ITaskCompetencyRequirementsProvider
	{
		IEnumerable<CompetencyRequirement> GetPendingCompetencyRequirements();
	}
}
