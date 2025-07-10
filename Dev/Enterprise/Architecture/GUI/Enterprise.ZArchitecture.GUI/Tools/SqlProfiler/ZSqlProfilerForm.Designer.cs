namespace Enterprise.ZArchitecture.Tools
{
	partial class ZSqlProfilerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.commandsInfoListGrid = new Enterprise.ZArchitecture.Tools.CommandsInfoListGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Tools.SqlCommandsManager);
			// 
			// commandsInfoListGrid
			// 
			this.commandsInfoListGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.commandsInfoListGrid, ".");
			this.commandsInfoListGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commandsInfoListGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.commandsInfoListGrid.Name = "commandsInfoListGrid";
			this.commandsInfoListGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 605, true);
			this.commandsInfoListGrid.TabIndex = 0;
			// 
			// ZSqlProfilerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 605, true);
			this.Controls.Add(this.commandsInfoListGrid);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 644, true);
			this.Name = "ZSqlProfilerForm";
			this.Text = "Sql Profiler";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CommandsInfoListGrid commandsInfoListGrid;
	}
}