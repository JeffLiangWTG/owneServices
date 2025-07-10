using System.Collections.Generic;

namespace Enterprise.DbUpgrader.Shared
{
	public interface IUpgradeAction
	{
		void Run();
		string Name { get; set; }
		bool RequiresApplicationLockout { get; set; }
		int EstimatedNumberOfTasks { get; set; }
		IEnumerable<string> SecondaryDatabasesToUpgrade { get; set; }
	}
}
