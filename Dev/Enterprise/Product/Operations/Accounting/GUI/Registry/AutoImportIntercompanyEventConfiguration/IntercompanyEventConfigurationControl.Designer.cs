namespace Enterprise.Accounting.Registry.GUI
{
	partial class IntercompanyEventConfigurationControl
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
			this.dropEditEventCode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zDateEditStartDate = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.enableEventConfiguration = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.disableEventConfiguration = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntercompanyEventSettingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IntercompanyEventSettingGrid)).BeginInit();
			this.IntercompanyEventSettingGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.IntercompanyEventConfiguration);
			// 
			// IntercompanyEventSettingGrid
			// 
			this.IntercompanyEventSettingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IntercompanyEventSettingGrid, "IntercompanyEventSettingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.IntercompanyEventConfiguration)(null)).IntercompanyEventSettingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.IntercompanyEventSetting)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.IntercompanyEventConfiguration)(null)).IntercompanyEventSettingCollection)).SyncRoot)).StmEventCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.IntercompanyEventSetting)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.IntercompanyEventConfiguration)(null)).IntercompanyEventSettingCollection)).SyncRoot)).StmEventDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Registry.Business.IntercompanyEventSetting)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.IntercompanyEventConfiguration)(null)).IntercompanyEventSettingCollection)).SyncRoot)).StartDate)));
			this.IntercompanyEventSettingGrid.CaptionVisible = false;
			dropEditEventCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			dropEditEventCode.ColumnName = "StmEventCode";
			dropEditEventCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo1.ColumnName = "StmEventDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zDateEditStartDate.ColumnName = "StartDate";
			zDateEditStartDate.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73);
			zDateEditStartDate.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.IntercompanyEventSettingGrid.ColumnStyles.Add(dropEditEventCode);
			this.IntercompanyEventSettingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IntercompanyEventSettingGrid.ColumnStyles.Add(zDateEditStartDate);
			this.IntercompanyEventSettingGrid.GridId = "d12b9d7b-8ede-446c-99f5-d0a9f3243095";
			this.IntercompanyEventSettingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IntercompanyEventSettingGrid.LayoutKey = "IntercompanyEventSettingGrid";
			this.IntercompanyEventSettingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 99, true);
			this.IntercompanyEventSettingGrid.Name = "IntercompanyEventSettingGrid";
			this.IntercompanyEventSettingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 228, true);
			this.IntercompanyEventSettingGrid.TabIndex = 0;
			// 
			// enableBranchLevelPosting
			// 
			this.enableEventConfiguration.AutoCheck = false;
			this.enableEventConfiguration.AutoSize = true;
			this.enableEventConfiguration.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7184e6f8-c106-44bc-b223-6b92735a34ab", "Yes");
			this.enableEventConfiguration.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enableEventConfiguration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 27, true);
			this.enableEventConfiguration.Name = "enableEventConfiguration";
			this.enableEventConfiguration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.enableEventConfiguration.TabIndex = 1;
			this.enableEventConfiguration.TabStop = true;
			this.enableEventConfiguration.UseVisualStyleBackColor = true;
			this.enableEventConfiguration.CheckedChanged += new System.EventHandler(this.enableEventConfiguration_CheckedChanged);
			// 
			// disableBranchLevelPosting
			// 
			this.disableEventConfiguration.AutoCheck = false;
			this.disableEventConfiguration.AutoSize = true;
			this.disableEventConfiguration.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("04c0d351-82f0-4d66-98f1-28e0f97d0013", "No");
			this.disableEventConfiguration.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.disableEventConfiguration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 27, true);
			this.disableEventConfiguration.Name = "disableEventConfiguration";
			this.disableEventConfiguration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 16, true);
			this.disableEventConfiguration.TabIndex = 2;
			this.disableEventConfiguration.TabStop = true;
			this.disableEventConfiguration.UseVisualStyleBackColor = true;
			this.disableEventConfiguration.CheckedChanged += new System.EventHandler(this.disableEventConfiguration_CheckedChanged);
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
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("275380ED-8A77-4B93-BD22-3BE3D6763E52", "Enable Event Configuration");
			this.zGroupBox1.Controls.Add(this.enableEventConfiguration);
			this.zGroupBox1.Controls.Add(this.disableEventConfiguration);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 5, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 59, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// IntercompanyEventConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.IntercompanyEventSettingGrid);
			this.Name = "IntercompanyEventConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 370, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IntercompanyEventSettingGrid)).EndInit();
			this.IntercompanyEventSettingGrid.ResumeLayout(false);
			this.IntercompanyEventSettingGrid.PerformLayout();
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
		private ZArchitecture.GUI.ZRadioButton enableEventConfiguration;
		private ZArchitecture.GUI.ZRadioButton disableEventConfiguration;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		public ZArchitecture.ZGrid IntercompanyEventSettingGrid;
		private Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo dropEditEventCode;
		Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditStartDate;
	}
}
