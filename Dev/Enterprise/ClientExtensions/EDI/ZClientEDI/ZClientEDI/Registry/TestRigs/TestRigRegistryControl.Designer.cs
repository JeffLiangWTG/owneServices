namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class TestRigRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.OptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OptionsGrid)).BeginInit();
			this.OptionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.TestRigRegistryHeader);
			// 
			// OptionsGrid
			// 
			this.OptionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OptionsGrid, "OptionsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryHeader)(null)).OptionsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryOptions)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryHeader)(null)).OptionsCollection)).SyncRoot)).Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryOptions)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryHeader)(null)).OptionsCollection)).SyncRoot)).ProductArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryOptions)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryHeader)(null)).OptionsCollection)).SyncRoot)).Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryOptions)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryHeader)(null)).OptionsCollection)).SyncRoot)).ChangeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryOptions)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryHeader)(null)).OptionsCollection)).SyncRoot)).BackupFile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryOptions)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TestRigRegistryHeader)(null)).OptionsCollection)).SyncRoot)).AdditionalOptions)));
			this.OptionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "Product";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "ProductArea";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "Module";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "ChangeType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "BackupFile";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zMultiLineTextBoxColumnInfo1.ColumnName = "AdditionalOptions";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 100;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.OptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OptionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.OptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OptionsGrid.GridId = "f5b810ae-8748-4950-834f-8097b938e4f2";
			this.OptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OptionsGrid.LayoutKey = "OptionsGrid";
			this.OptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionsGrid.Name = "OptionsGrid";
			this.OptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 313, true);
			this.OptionsGrid.TabIndex = 0;
			// 
			// TestRigRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OptionsGrid);
			this.Name = "TestRigRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 313, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OptionsGrid)).EndInit();
			this.OptionsGrid.ResumeLayout(false);
			this.OptionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid OptionsGrid;
	}
}
