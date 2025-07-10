using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class GlowOpportunityScopePrioritySequenceRegistryControl : RegistryZUserControl
	{
		public GlowOpportunityScopePrioritySequenceRegistryControl()
		{
			InitializeComponent();
			GlowOpportunityScopePrioritySequenceGrid.AllowSorting = false;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GlowOpportunityScopePrioritySequenceGrid.ReadOnly = readOnly;
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
				&& GlowOpportunityScopePrioritySequenceGrid.List.Count > 0
				&& GlowOpportunityScopePrioritySequenceGrid.SelectedRowCount <= 1
				&& (GlowOpportunityScopePrioritySequenceGrid.CurrentRowIndex != 0 || !up)
				&& (GlowOpportunityScopePrioritySequenceGrid.CurrentRowIndex != GlowOpportunityScopePrioritySequenceGrid.List.Count - 1 || up))
			{
				int oldIndex = GlowOpportunityScopePrioritySequenceGrid.CurrentRowIndex;
				int newIndex = up ? oldIndex - 1 : oldIndex + 1;
				var temp = GlowOpportunityScopePrioritySequenceGrid.List[newIndex];
				GlowOpportunityScopePrioritySequenceGrid.List[newIndex] = GlowOpportunityScopePrioritySequenceGrid.ListManager.GetCurrent();
				GlowOpportunityScopePrioritySequenceGrid.List[oldIndex] = temp;
				GlowOpportunityScopePrioritySequenceGrid.CurrentRowIndex = newIndex;

				if (GlowOpportunityScopePrioritySequenceGrid.SelectedRowCount > 0)
				{
					GlowOpportunityScopePrioritySequenceGrid.UnSelect(oldIndex);
					GlowOpportunityScopePrioritySequenceGrid.Select(newIndex);
				}
			}
		}
	}
}
