namespace Enterprise.Customs.ES.GUI
{
	partial class AnnexesTabUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AnnexPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AnnexGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AnnexPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AnnexGrid)).BeginInit();
			this.AnnexGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.CusStorageDocPivotCollection);
			// 
			// AnnexPanel
			// 
			this.AnnexPanel.Controls.Add(this.AnnexGrid);
			this.AnnexPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AnnexPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AnnexPanel.Name = "AnnexPanel";
			this.AnnexPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.AnnexPanel.TabIndex = 0;
			// 
			// AnnexGrid
			// 
			this.AnnexGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AnnexGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusStorageDocPivot)(null)))));
			this.AnnexGrid.CaptionVisible = false;
			this.AnnexGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AnnexGrid.GridId = "15ADB4CF-3B9C-46C0-8317-0529FC84EA36";
			this.AnnexGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AnnexGrid.LayoutKey = "AnnexGrid";
			this.AnnexGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AnnexGrid.Name = "AnnexGrid";
			this.AnnexGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.AnnexGrid.TabIndex = 0;
			// 
			// AnnexesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AnnexPanel);
			this.Name = "AnnexesTabUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AnnexPanel.ResumeLayout(false);
			this.AnnexPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AnnexGrid)).EndInit();
			this.AnnexGrid.ResumeLayout(false);
			this.AnnexGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel AnnexPanel;
		public ZArchitecture.ZGrid AnnexGrid;
	}
}
