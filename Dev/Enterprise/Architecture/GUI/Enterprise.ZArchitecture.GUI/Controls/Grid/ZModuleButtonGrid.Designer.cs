using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZModuleButtonGrid
	{

		#region Component Designer generated code

		System.ComponentModel.Container components = null;
		protected CargoWise.Windows.UI.KTableLayoutPanel mainLayoutPanel;
		Enterprise.ZArchitecture.GUI.ZGridWithoutColumnStylesSerialisation Grid;

		void InitializeComponent()
		{
			this.Grid = CreateNewGrid();
			this.mainLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.mainLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.Grid.BindTo = "";
			this.Grid.CaptionVisible = false;
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.Dock = DockStyle.Fill;
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 145, true);
			this.Grid.TabIndex = 0;
			this.InitializeButtons();
			// 
			// mainLayoutPanel
			// 
			this.mainLayoutPanel.ColumnCount = 1;
			this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainLayoutPanel.Controls.Add(this.Grid, 0, 0);
			this.mainLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainLayoutPanel.Name = LayoutControlName;
			this.mainLayoutPanel.RowCount = 2;
			this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, ControlDpiScalingHelper.ScaleToCurrentDpiY(32)));
			this.mainLayoutPanel.AutoSize = true;
			this.mainLayoutPanel.Dock = DockStyle.Fill;
			this.mainLayoutPanel.TabIndex = 6;
			// 
			// ZButtonGrid
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainLayoutPanel);
			this.Name = "ZButtonGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 184, true);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.mainLayoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}
