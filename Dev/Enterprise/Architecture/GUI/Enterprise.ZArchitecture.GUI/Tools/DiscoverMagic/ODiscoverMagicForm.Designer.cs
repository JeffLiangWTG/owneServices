namespace Enterprise.Core.Forms
{
	public partial class ODiscoverMagicForm
	{
		ZArchitecture.GUI.ZButton ShowOriginalButton;
		ZArchitecture.GUI.ZButton ShowRowsInDBButton;
		ZArchitecture.GUI.ZDataSetListBoxWithGrid listBoxWithGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listBoxWithGrid = new Enterprise.ZArchitecture.GUI.ZDataSetListBoxWithGrid();
			this.ShowRowsInDBButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShowOriginalButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ListBoxWithGrid
			// 
			this.listBoxWithGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.listBoxWithGrid.DataSource = null;
			this.listBoxWithGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 37, true);
			this.listBoxWithGrid.Name = "listBoxWithGrid";
			this.listBoxWithGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 445, true);
			this.listBoxWithGrid.TabIndex = 3;
			// 
			// ShowRowsInDBButton
			// 
			this.ShowRowsInDBButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			this.ShowRowsInDBButton.Name = "ShowRowsInDBButton";
			this.ShowRowsInDBButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.ShowRowsInDBButton.TabIndex = 2;
			this.ShowRowsInDBButton.Text = "Show Rows in DB";
			this.ShowRowsInDBButton.Click += new System.EventHandler(this.OnShowRowsInDB_Click);
			// 
			// ShowOriginalButton
			// 
			this.ShowOriginalButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ShowOriginalButton.Name = "ShowOriginalButton";
			this.ShowOriginalButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.ShowOriginalButton.TabIndex = 1;
			this.ShowOriginalButton.Text = "Show Original";
			this.ShowOriginalButton.Click += new System.EventHandler(this.OnShowOriginal_Click);
			// 
			// ODiscoverMagicForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 494, true);
			this.Controls.Add(this.listBoxWithGrid);
			this.Controls.Add(this.ShowRowsInDBButton);
			this.Controls.Add(this.ShowOriginalButton);
			this.Name = "ODiscoverMagicForm";
			this.Text = "Discover MagicForm";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
