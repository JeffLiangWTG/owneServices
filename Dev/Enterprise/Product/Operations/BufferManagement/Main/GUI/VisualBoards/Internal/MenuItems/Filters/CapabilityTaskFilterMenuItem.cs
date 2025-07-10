using System;
using System.Collections.Generic;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class CapabilityTaskFilterMenuItem : MultiChildCheckedFilterMenuItem<Enum>
	{
		public CapabilityTaskFilterMenuItem(BMBoardSectionViewModel sectionViewModel)
			: base(Res.GetString("18ed4313-b33d-4e53-bfca-19955e90935c", "Capability Tasks"), sectionViewModel)
		{
		}

		#region Filter Application

		void OnRemovedFromFilterManager()
		{
			if (RemovedFromFilterManager != null)
			{
				RemovedFromFilterManager(this, EventArgs.Empty);
			}
		}

		internal event EventHandler RemovedFromFilterManager;

		#endregion

		#region MultiChildCheckedFilterMenuItem Overrides

		protected override IMultiOptionFilter GetOrCreateMultiOptionFilter()
		{
			var filter = (IMultiOptionFilter)GetCurrentFilter(typeof(CapabilityTaskFilter)) ?? new CapabilityTaskFilter();
			filter.RestoreVisualStateAfterFilterRemovedAction = OnRemovedFromFilterManager;
			return filter;
		}

		protected override IEnumerable<Enum> GetMenuItemValues()
		{
			yield return TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks;
			yield return TaskCapabilityFilter.ShowOnlyUnassignedCapabilityTasks;
		}

		protected override string GetSubMenuItemText(Enum value)
		{
			switch ((TaskCapabilityFilter)value)
			{
				case TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks:
					return Res.GetString("d6da11be-9f13-4c48-b8c1-3657d1a588a0", "Exclude Capability Tasks");

				case TaskCapabilityFilter.ShowOnlyUnassignedCapabilityTasks:
					return Res.GetString("512d559d-888e-4ffe-92a4-344a3afe9562", "Show Only Capability Tasks");

				default:
					throw new ArgumentException("Invalid ShowTaskOption: " + value);
			}
		}

		protected override string GetSubMenuItemTooltip(Enum value)
		{
			switch ((TaskCapabilityFilter)value)
			{
				case TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks:
					return Res.GetString("395e5798-e09f-4c23-b895-b7b4f8d27fb2", "Hides all tasks which require a Capability and have no Resource assigned.");

				case TaskCapabilityFilter.ShowOnlyUnassignedCapabilityTasks:
					return Res.GetString("acae238d-9298-4367-99e8-d1d949e0fb00", "Hides all tasks except those which require a Capability and have no Resource assigned.");

				default:
					throw new ArgumentException("Invalid ShowTaskOption: " + value);
			}
		}

		#endregion
	}
}
