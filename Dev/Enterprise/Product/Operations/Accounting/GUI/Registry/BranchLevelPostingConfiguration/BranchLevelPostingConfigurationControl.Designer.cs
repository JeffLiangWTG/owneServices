namespace Enterprise.Accounting.Registry.GUI
{
	partial class BranchLevelPostingConfigurationControl
	{
		
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.BranchGroupSettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.enableBranchLevelPosting = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.disableBranchLevelPosting = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BranchGroupSettingsGrid)).BeginInit();
			this.BranchGroupSettingsGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.BranchLevelPostingConfiguration);
			// 
			// BranchGroupSettingsGrid
			// 
			this.BranchGroupSettingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BranchGroupSettingsGrid, "BranchGroupSettingsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.BranchLevelPostingConfiguration)(null)).BranchGroupSettingsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.BranchGroupSettings)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.BranchLevelPostingConfiguration)(null)).BranchGroupSettingsCollection)).SyncRoot)).BranchPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.BranchGroupSettings)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.BranchLevelPostingConfiguration)(null)).BranchGroupSettingsCollection)).SyncRoot)).GroupNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.BranchGroupSettings)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.BranchLevelPostingConfiguration)(null)).BranchGroupSettingsCollection)).SyncRoot)).IsParentBranch)));
			this.BranchGroupSettingsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BranchPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "GroupNumber";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsParentBranch";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.BranchGroupSettingsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BranchGroupSettingsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.BranchGroupSettingsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BranchGroupSettingsGrid.GridId = "e4e5544f-fc19-45a4-8258-1e6283aa9680";
			this.BranchGroupSettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BranchGroupSettingsGrid.LayoutKey = "BranchGroupSettingsGrid";
			this.BranchGroupSettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 99, true);
			this.BranchGroupSettingsGrid.Name = "BranchGroupSettingsGrid";
			this.BranchGroupSettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 228, true);
			this.BranchGroupSettingsGrid.TabIndex = 0;
			// 
			// enableBranchLevelPosting
			// 
			this.enableBranchLevelPosting.AutoCheck = false;
			this.enableBranchLevelPosting.AutoSize = true;
			this.enableBranchLevelPosting.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7184e6f8-c106-44bc-b223-6b92735a34ab", "Yes");
			this.enableBranchLevelPosting.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enableBranchLevelPosting.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 27, true);
			this.enableBranchLevelPosting.Name = "enableBranchLevelPosting";
			this.enableBranchLevelPosting.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.enableBranchLevelPosting.TabIndex = 1;
			this.enableBranchLevelPosting.TabStop = true;
			this.enableBranchLevelPosting.UseVisualStyleBackColor = true;
			this.enableBranchLevelPosting.CheckedChanged += new System.EventHandler(this.enableBranchLevelPosting_CheckedChanged);
			// 
			// disableBranchLevelPosting
			// 
			this.disableBranchLevelPosting.AutoCheck = false;
			this.disableBranchLevelPosting.AutoSize = true;
			this.disableBranchLevelPosting.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("04c0d351-82f0-4d66-98f1-28e0f97d0013", "No");
			this.disableBranchLevelPosting.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.disableBranchLevelPosting.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 27, true);
			this.disableBranchLevelPosting.Name = "disableBranchLevelPosting";
			this.disableBranchLevelPosting.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 16, true);
			this.disableBranchLevelPosting.TabIndex = 2;
			this.disableBranchLevelPosting.TabStop = true;
			this.disableBranchLevelPosting.UseVisualStyleBackColor = true;
			this.disableBranchLevelPosting.CheckedChanged += new System.EventHandler(this.disableBranchLevelPosting_CheckedChanged);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zGroupBox1);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 81, true);
			this.zPanel1.TabIndex = 3;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7857e778-8180-40b5-8399-d18e4826a152", "Enable Branch Level Posting");
			this.zGroupBox1.Controls.Add(this.enableBranchLevelPosting);
			this.zGroupBox1.Controls.Add(this.disableBranchLevelPosting);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 5, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 59, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// BranchLevelPostingConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.BranchGroupSettingsGrid);
			this.Name = "BranchLevelPostingConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 370, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BranchGroupSettingsGrid)).EndInit();
			this.BranchGroupSettingsGrid.ResumeLayout(false);
			this.BranchGroupSettingsGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private ZArchitecture.ZGrid BranchGroupSettingsGrid;
		private ZArchitecture.GUI.ZRadioButton enableBranchLevelPosting;
		private ZArchitecture.GUI.ZRadioButton disableBranchLevelPosting;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
	}
}
