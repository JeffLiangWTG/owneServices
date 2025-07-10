namespace Enterprise.Registry.GUI.eHub
{
	partial class ScavengingSettingsUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.ScavengingSettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ScavengingSettingsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.eHub.ScavengingSetting);
			// 
			// ScavengingSettingsGrid
			// 
			this.ScavengingSettingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ScavengingSettingsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.eHub.ScavengingSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.eHub.ScavengingSetting)(null)).TaskName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.eHub.ScavengingSetting)(null)).PeriodStart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.eHub.ScavengingSetting)(null)).PeriodEnd)));
			this.ScavengingSettingsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3c89fc24-c713-465b-b9b2-e603dc5d49e7", "Task Name");
			zTextBoxColumnStyleInfo1.ColumnName = "TaskName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9aafa17f-13bd-4e2a-a8e0-586c57eda941", "Period Start");
			zDateEditColumnStyleInfo1.ColumnName = "PeriodStart";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6fb51243-66f0-4151-853f-6fc8fb23ac95", "Period End");
			zDateEditColumnStyleInfo2.ColumnName = "PeriodEnd";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.ScavengingSettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ScavengingSettingsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ScavengingSettingsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ScavengingSettingsGrid.CopySelectedRowsAllowed = true;
			this.ScavengingSettingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScavengingSettingsGrid.GridId = "81d418b9-a109-4b10-b7a9-067c82fb835f";
			this.ScavengingSettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ScavengingSettingsGrid.LayoutKey = "ScavengingSettingsGrid";
			this.ScavengingSettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ScavengingSettingsGrid.Name = "ScavengingSettingsGrid";
			this.ScavengingSettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.ScavengingSettingsGrid.TabIndex = 0;
			// 
			// ScavengingSettingsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ScavengingSettingsGrid);
			this.Name = "ScavengingSettingsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ScavengingSettingsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid ScavengingSettingsGrid;

	}
}
