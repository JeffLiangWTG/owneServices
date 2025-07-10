using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class EntryLinesTabUserControl : ZUserControl
	{
		public EntryLinesTabUserControl()
		{
			InitializeComponent();
			ChangeVisibilities();
		}

		void ChangeVisibilities()
		{
			if (DataSource is Business.JobDeclaration declaration)
			{
				if (!declaration.IsCADEnabled)
				{
					CenterPanel.Visible = false;
					BottomPanel.Visible = false;
					this.Controls.Remove(CenterPanel);
					this.Controls.Remove(BottomPanel);
					this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				}
			}
		}

		public void ChangeVisibilities(bool isCAD)
		{
			if (isCAD)
			{
				CenterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 168, true);
				BottomPanel.Visible = true;
			}
			else
			{
				CenterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 268, true);
				BottomPanel.Visible = false;
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			ChangeVisibilities();
		}
	}
}
