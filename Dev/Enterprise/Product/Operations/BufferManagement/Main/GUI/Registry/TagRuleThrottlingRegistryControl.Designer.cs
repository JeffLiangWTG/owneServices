
namespace Enterprise.BufferManagement.GUI
{
	partial class TagRuleThrottlingRegistryControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ThresholdGrid = new Enterprise.ZArchitecture.ZGrid();
			this.IsThrottlingEnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ThresholdGrid)).BeginInit();
			this.ThresholdGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.TagRuleThrottlingHeader);
			// 
			// ThresholdGrid
			// 
			this.ThresholdGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ThresholdGrid, "ThresholdCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagRuleThrottlingHeader)(null)).ThresholdCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagRuleThrottlingThreshold)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagRuleThrottlingHeader)(null)).ThresholdCollection)).SyncRoot)).RunTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagRuleThrottlingThreshold)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagRuleThrottlingHeader)(null)).ThresholdCollection)).SyncRoot)).RunInterval)));
			this.ThresholdGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("acbcd2d2-733c-4984-8e63-f5824be40ef6", "Run Time", "Min Rule Run Time (seconds)", "Minimum Rule Run Time (seconds)");
			zCalcEditColumnStyleInfo1.ColumnName = "RunTime";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4bff9850-1828-4534-a1b4-33d002cf3c24", "Interval", "Rule Run Interval (minutes)", "");
			zCalcEditColumnStyleInfo2.ColumnName = "RunInterval";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.ThresholdGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ThresholdGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ThresholdGrid.CopySelectedRowsAllowed = true;
			this.ThresholdGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ThresholdGrid.GridId = "2b59de9e-1973-41b2-9fef-6bcca6477f99";
			this.ThresholdGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ThresholdGrid.LayoutKey = "AssignmentGrid";
			this.ThresholdGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ThresholdGrid.Name = "ThresholdGrid";
			this.ThresholdGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 266, true);
			this.ThresholdGrid.TabIndex = 0;
			// 
			// IsThrottlingEnabledCheckBox
			// 
			this.IsThrottlingEnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsThrottlingEnabledCheckBox, "IsThrottlingEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagRuleThrottlingHeader)(null)).IsThrottlingEnabled)));
			this.IsThrottlingEnabledCheckBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("e5bfbf64-65ad-4e74-be26-fb5d437070db", "Enable Tag Rule Frequency Throttling");
			this.IsThrottlingEnabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsThrottlingEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IsThrottlingEnabledCheckBox.Name = "IsThrottlingEnabledCheckBox";
			this.IsThrottlingEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 17, true);
			this.IsThrottlingEnabledCheckBox.TabIndex = 1;
			this.IsThrottlingEnabledCheckBox.UseVisualStyleBackColor = true;
			this.IsThrottlingEnabledCheckBox.CheckStateChanged += new System.EventHandler(this.IsThrottlingEnabledCheckBox_CheckStateChanged);
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.kSplitContainer1.IsSplitterFixed = true;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.IsThrottlingEnabledCheckBox);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.zGroupBox1);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 314, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			this.kSplitContainer1.TabIndex = 2;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("fdda2b19-043f-4d59-a565-73148e8d4e58", "Frequency Settings");
			this.zGroupBox1.Controls.Add(this.ThresholdGrid);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 285, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// TagRuleThrottlingRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "TagRuleThrottlingRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 314, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ThresholdGrid)).EndInit();
			this.ThresholdGrid.ResumeLayout(false);
			this.ThresholdGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel1.PerformLayout();
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ThresholdGrid;
		private ZArchitecture.GUI.ZCheckBox IsThrottlingEnabledCheckBox;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;

	}
}
