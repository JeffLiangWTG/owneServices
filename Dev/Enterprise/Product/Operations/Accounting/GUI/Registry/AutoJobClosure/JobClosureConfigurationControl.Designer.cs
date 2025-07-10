namespace Enterprise.Accounting.Registry.GUI
{
	partial class JobClosureConfigurationControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ConfigurationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JobClosureConfigGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfigurationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobClosureConfigGrid)).BeginInit();
			this.JobClosureConfigGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobClosureConfigurationHeader);
			// 
			// ConfigurationGroupBox
			// 
			this.ConfigurationGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("24ff165b-b5dc-401f-bffa-e28e24e67e35", "Job Closure Configuration");
			this.ConfigurationGroupBox.Controls.Add(this.JobClosureConfigGrid);
			this.ConfigurationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
			this.ConfigurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 425, true);
			this.ConfigurationGroupBox.TabIndex = 6;
			this.ConfigurationGroupBox.TabStop = false;
			// 
			// JobClosureConfigGrid
			// 
			this.JobClosureConfigGrid.AllowNavigation = false;
			this.JobClosureConfigGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobClosureConfigGrid, "ConfigurationCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).DirectionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).DepartmentPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).ConfigurationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).FromJobStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).ToJobStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).CloseJobsWithOpenAcr)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).CloseJobsWithOpenWip)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).JobChargeRecognitionFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).JobClosureDateOptionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).JobClosureDateOptionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).Offset)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).OffsetType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).ReopenRestrictionOffset)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobClosureConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobClosureConfigurationHeader)(null)).ConfigurationCollection)).SyncRoot)).ReopenRestrictionOffsetType)));
			this.JobClosureConfigGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7121582a-81ed-4f94-a795-7d2358f0958a", "Job Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5a977a9d-2027-4cf9-a12d-e9974e0162ae", "Direction");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cb519a05-6b1f-4d5f-ab39-a296ba935e08", "Mode");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "Mode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "DepartmentPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "ConfigurationType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "FromJobStatus";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "ToJobStatus";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "CloseJobsWithOpenAcr";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "CloseJobsWithOpenWip";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.ColumnName = "JobChargeRecognitionFilter";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("80188c6c-f97f-4c9d-b1c9-fada85245719", "Relevant Date", "Relevant Date", "Relevant Date for Job Closure", "");
			zDropEditColumnStyleInfo7.ColumnName = "JobClosureDateOptionCode";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("722d270a-138a-44a0-b214-c4cf300e1759", "Description", "Relevant Date Description", "Relevant Date Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "JobClosureDateOptionDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Offset";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo8.ColumnName = "OffsetType";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5A2913AA-C3EA-4287-8ACC-E58F047F9DF2", "Re-Open Restriction Offset", "Re-Open Restriction Offset", "Re-Open Restriction Offset", "");
			zCalcEditColumnStyleInfo2.ColumnName = "ReopenRestrictionOffset";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo9.ColumnName = "ReopenRestrictionOffsetType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.JobClosureConfigGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.JobClosureConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.JobClosureConfigGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.JobClosureConfigGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.JobClosureConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobClosureConfigGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.JobClosureConfigGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.JobClosureConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.JobClosureConfigGrid.GridId = "653AE84D-C494-458D-9990-6A8BEFC473C0";
			this.JobClosureConfigGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobClosureConfigGrid.LayoutKey = "RevenueRecognitionGrid";
			this.JobClosureConfigGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.JobClosureConfigGrid.Name = "JobClosureConfigGrid";
			this.JobClosureConfigGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 374, true);
			this.JobClosureConfigGrid.TabIndex = 5;
			// 
			// JobClosureConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConfigurationGroupBox);
			this.Name = "JobClosureConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 425, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfigurationGroupBox.ResumeLayout(false);
			this.ConfigurationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobClosureConfigGrid)).EndInit();
			this.JobClosureConfigGrid.ResumeLayout(false);
			this.JobClosureConfigGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ConfigurationGroupBox;
		private ZArchitecture.ZGrid JobClosureConfigGrid;
	}
}
