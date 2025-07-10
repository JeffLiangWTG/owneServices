using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class HBLDeliveryPriorityControl : RegistryZUserControl
	{
		public HBLDeliveryPriorityControl()
		{
			InitializeComponent();
			prioritySettingsGrid.AllowSorting = false;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			hblDeliveryConfigurationGrid.ReadOnly = readOnly;
			prioritySettingsGrid.ReadOnly = readOnly;
		}

		void UpButton_Click(object sender, EventArgs e)
		{
			MovePriority(true);
		}

		void DownButton_Click(object sender, EventArgs e)
		{
			MovePriority(false);
		}

		protected void MovePriority(bool up)
		{
			if (!ReadOnly
				&& prioritySettingsGrid.List.Count > 0
				&& prioritySettingsGrid.SelectedRowCount <= 1
				&& (prioritySettingsGrid.CurrentRowIndex != 0 || !up)
				&& (prioritySettingsGrid.CurrentRowIndex != prioritySettingsGrid.List.Count - 1 || up))
			{
				int oldIndex = prioritySettingsGrid.CurrentRowIndex;
				int newIndex = up ? oldIndex - 1 : oldIndex + 1;
				var temp = prioritySettingsGrid.List[newIndex];
				prioritySettingsGrid.List[newIndex] = prioritySettingsGrid.ListManager.GetCurrent();
				prioritySettingsGrid.List[oldIndex] = temp;
				prioritySettingsGrid.CurrentRowIndex = newIndex;

				if (prioritySettingsGrid.SelectedRowCount > 0)
				{
					prioritySettingsGrid.UnSelect(oldIndex);
					prioritySettingsGrid.Select(newIndex);
				}
			}
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
			=> hblDeliveryConfigurationGrid.ReadOnly;

#endif
	}
}
