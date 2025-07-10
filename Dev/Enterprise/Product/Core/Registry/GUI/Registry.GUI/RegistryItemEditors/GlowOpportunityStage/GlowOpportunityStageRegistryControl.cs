using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class GlowOpportunityStageRegistryControl : RegistryZUserControl
	{
		public GlowOpportunityStageRegistryControl()
		{
			InitializeComponent();
			GlowOpportunityStageRegistryGrid.AllowSorting = false;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GlowOpportunityStageRegistryGrid.ReadOnly = readOnly;
		}

		protected void UpButton_Click(object sender, EventArgs e)
		{
			MoveStage(true);
		}

		protected void DownButton_Click(object sender, EventArgs e)
		{
			MoveStage(false);
		}

		protected void MoveStage(bool up)
		{
			if (!ReadOnly
				&& GlowOpportunityStageRegistryGrid.List.Count > 0
				&& GlowOpportunityStageRegistryGrid.SelectedRowCount <= 1
				&& (GlowOpportunityStageRegistryGrid.CurrentRowIndex != 0 || !up)
				&& (GlowOpportunityStageRegistryGrid.CurrentRowIndex != GlowOpportunityStageRegistryGrid.List.Count - 1 || up))
			{
				int oldIndex = GlowOpportunityStageRegistryGrid.CurrentRowIndex;
				int newIndex = up ? oldIndex - 1 : oldIndex + 1;
				var temp = GlowOpportunityStageRegistryGrid.List[newIndex];
				GlowOpportunityStageRegistryGrid.List[newIndex] = GlowOpportunityStageRegistryGrid.ListManager.GetCurrent();
				GlowOpportunityStageRegistryGrid.List[oldIndex] = temp;
				GlowOpportunityStageRegistryGrid.CurrentRowIndex = newIndex;

				if (GlowOpportunityStageRegistryGrid.SelectedRowCount > 0)
				{
					GlowOpportunityStageRegistryGrid.UnSelect(oldIndex);
					GlowOpportunityStageRegistryGrid.Select(newIndex);
				}

				NotifyChanges();
			}
		}
	}
}
