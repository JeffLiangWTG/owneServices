using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RatesPrioritiesControl : RegistryZUserControl
	{
		public RatesPrioritiesControl()
		{
			InitializeComponent();
			RatesPriorityGrid.AllowSorting = false;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			RatesPriorityGrid.ReadOnly = readOnly;
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
				&& RatesPriorityGrid.List.Count > 0
				&& RatesPriorityGrid.SelectedRowCount <= 1
				&& (RatesPriorityGrid.CurrentRowIndex != 0 || !up)
				&& (RatesPriorityGrid.CurrentRowIndex != RatesPriorityGrid.List.Count - 1 || up))
			{
				int oldIndex = RatesPriorityGrid.CurrentRowIndex;
				int newIndex = up ? oldIndex - 1 : oldIndex + 1;
				var temp = RatesPriorityGrid.List[newIndex];
				RatesPriorityGrid.List[newIndex] = RatesPriorityGrid.ListManager.GetCurrent();
				RatesPriorityGrid.List[oldIndex] = temp;
				RatesPriorityGrid.CurrentRowIndex = newIndex;

				if (RatesPriorityGrid.SelectedRowCount > 0)
				{
					RatesPriorityGrid.UnSelect(oldIndex);
					RatesPriorityGrid.Select(newIndex);
				}
			}
		}
	}
}
