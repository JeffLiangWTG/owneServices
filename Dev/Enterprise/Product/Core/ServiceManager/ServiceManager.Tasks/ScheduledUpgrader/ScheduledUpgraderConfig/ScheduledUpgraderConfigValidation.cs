using System.Diagnostics;

namespace Enterprise.ServiceManager.Tasks.ScheduledUpgrader
{
	public class ScheduledUpgraderConfigValidation : AutoScheduledUpgraderConfigValidation
	{
		public ScheduledUpgraderConfigValidation(AutoScheduledUpgraderConfig parent)
			: base(parent) { }

		#region Implementation

		public new ScheduledUpgraderConfig Parent
		{
			[DebuggerStepThrough]
			get { return (ScheduledUpgraderConfig)base.Parent; }
		}

		#endregion
	}
}
