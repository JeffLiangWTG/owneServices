using System.Collections.Generic;

namespace Enterprise.DbUpgrader.Startup
{
	public class BiServerRequirementChecker : RequirementChecker
	{
		public BiServerRequirementChecker(IEnumerable<string> dbsBeingUpgraded)
			: base(dbsBeingUpgraded, dbsBeingUpgraded)
		{
		}
	}
}
