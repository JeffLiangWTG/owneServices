using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DbUpgrader.Shared
{
	public class UpgradeAction : IUpgradeAction
	{
		public UpgradeAction(Action action)
			: this(null, action)
		{
		}

		public UpgradeAction(string name, Action action)
		{
			Name = name;
			this.action = action;

			EstimatedNumberOfTasks = 1;
		}

		public void Run()
		{
			action();
		}

		public string Name { get; set; }
		public bool RequiresApplicationLockout { get; set; }
		public int EstimatedNumberOfTasks { get; set; }

		public IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get
			{
				if (secondaryDatabasesToUpgrade != null)
				{
					return secondaryDatabasesToUpgrade.ToArray();
				}

				return Array.Empty<string>();
			}

			set
			{
				secondaryDatabasesToUpgrade = (value != null) ? value.ToArray() : null;
			}
		}
		IEnumerable<string> secondaryDatabasesToUpgrade;

		#region Implementation

		readonly Action action;

		#endregion // Implementation
	}
}
