using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class GlowOpportunityStatusRegistryControl : RegistryZUserControl
	{
		public GlowOpportunityStatusRegistryControl()
		{
			InitializeComponent();
			GlowOpportunityStatusGrid.AllowSorting = false;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GlowOpportunityStatusGrid.ReadOnly = readOnly;
		}

		protected void UpButton_Click(object sender, EventArgs e)
		{
			MoveStatus(true);
		}

		protected void DownButton_Click(object sender, EventArgs e)
		{
			MoveStatus(false);
		}

		protected void MoveStatus(bool up)
		{
			if (!ReadOnly
				&& GlowOpportunityStatusGrid.List.Count > 0
				&& GlowOpportunityStatusGrid.SelectedRowCount <= 1
				&& (GlowOpportunityStatusGrid.CurrentRowIndex != 0 || !up)
				&& (GlowOpportunityStatusGrid.CurrentRowIndex != GlowOpportunityStatusGrid.List.Count - 1 || up))
			{
				int oldIndex = GlowOpportunityStatusGrid.CurrentRowIndex;
				int newIndex = up ? oldIndex - 1 : oldIndex + 1;
				var temp = GlowOpportunityStatusGrid.List[newIndex];
				GlowOpportunityStatusGrid.List[newIndex] = GlowOpportunityStatusGrid.ListManager.GetCurrent();
				GlowOpportunityStatusGrid.List[oldIndex] = temp;
				GlowOpportunityStatusGrid.CurrentRowIndex = newIndex;

				if (GlowOpportunityStatusGrid.SelectedRowCount > 0)
				{
					GlowOpportunityStatusGrid.UnSelect(oldIndex);
					GlowOpportunityStatusGrid.Select(newIndex);
				}

				NotifyChanges();
			}
		}
	}
}
