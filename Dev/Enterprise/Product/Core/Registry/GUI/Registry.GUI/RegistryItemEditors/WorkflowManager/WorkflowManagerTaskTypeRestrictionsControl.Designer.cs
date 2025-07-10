namespace Enterprise.Registry.GUI
{
	partial class WorkflowManagerTaskTypeRestrictionsControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RestrictionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RestrictionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TaskTypesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TaskTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RestrictionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RestrictionsGrid)).BeginInit();
			this.RestrictionsGrid.SuspendLayout();
			this.TaskTypesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaskTypesGrid)).BeginInit();
			this.TaskTypesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.TaskTypeRestrictions);
			// 
			// RestrictionsGroupBox
			// 
			this.RestrictionsGroupBox.Controls.Add(this.RestrictionsGrid);
			this.RestrictionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RestrictionsGroupBox.Name = "RestrictionsGroupBox";
			this.RestrictionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 185, true);
			this.RestrictionsGroupBox.TabIndex = 0;
			this.RestrictionsGroupBox.TabStop = false;
			this.RestrictionsGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("cf0413d1-5d9f-452f-8de3-f11a231e0522", "Restrictions");
			this.RestrictionsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			// 
			// RestrictionsGrid
			// 
			this.RestrictionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RestrictionsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).Active)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).WorkflowType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).WorkflowTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).TaskType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).TaskTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).RestrictionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).RestrictionTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).Scope)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).ScopeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).NotificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).NotificationTypeList)));
			this.RestrictionsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Active";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "WorkflowType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "TaskType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "RestrictionType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "Scope";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "NotificationType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.RestrictionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RestrictionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RestrictionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RestrictionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RestrictionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RestrictionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RestrictionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RestrictionsGrid.CopySelectedRowsAllowed = true;
			this.RestrictionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RestrictionsGrid.GridId = "163ec2f1-8114-48b3-8fdb-6c868187a9f9";
			this.RestrictionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RestrictionsGrid.LayoutKey = "RestrictionsGrid";
			this.RestrictionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
			this.RestrictionsGrid.Name = "RestrictionsGrid";
			this.RestrictionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 165, true);
			this.RestrictionsGrid.TabIndex = 0;
			// 
			// TaskTypesGroupBox
			// 
			this.TaskTypesGroupBox.Controls.Add(this.TaskTypesGrid);
			this.TaskTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 189, true);
			this.TaskTypesGroupBox.Name = "TaskTypesGroupBox";
			this.TaskTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 186, true);
			this.TaskTypesGroupBox.TabIndex = 1;
			this.TaskTypesGroupBox.TabStop = false;
			this.TaskTypesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0d32f4be-c627-4c7d-a748-2b47219b4414", "Task Types");
			this.TaskTypesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// TaskTypesGrid
			// 
			this.TaskTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaskTypesGrid, "TaskTypesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).TaskTypesCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RestrictedTaskTypes)(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).TaskTypesCollection)).SyncRoot)).WorkflowType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RestrictedTaskTypes)(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).TaskTypesCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RestrictedTaskTypes)(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).TaskTypesCollection)).SyncRoot)).TaskTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RestrictedTaskTypes)(((System.Collections.IList)(((Enterprise.Registry.Business.TaskTypeRestrictions)(null)).TaskTypesCollection)).SyncRoot)).EnglishDescription)));
			this.TaskTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "WorkflowType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "Code";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.TaskTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TaskTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.TaskTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TaskTypesGrid.CopySelectedRowsAllowed = true;
			this.TaskTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaskTypesGrid.GridId = "91b0ae44-46f8-43ac-b6dd-6360b0435bee";
			this.TaskTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaskTypesGrid.LayoutKey = "TaskTypesGrid";
			this.TaskTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.TaskTypesGrid.Name = "TaskTypesGrid";
			this.TaskTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 165, true);
			this.TaskTypesGrid.TabIndex = 0;
			// 
			// WorkflowManagerTaskTypeRestrictionsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TaskTypesGroupBox);
			this.Controls.Add(this.RestrictionsGroupBox);
			this.Name = "WorkflowManagerTaskTypeRestrictionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 378, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RestrictionsGroupBox.ResumeLayout(false);
			this.RestrictionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RestrictionsGrid)).EndInit();
			this.RestrictionsGrid.ResumeLayout(false);
			this.RestrictionsGrid.PerformLayout();
			this.TaskTypesGroupBox.ResumeLayout(false);
			this.TaskTypesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaskTypesGrid)).EndInit();
			this.TaskTypesGrid.ResumeLayout(false);
			this.TaskTypesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox RestrictionsGroupBox;
		private ZArchitecture.ZGrid RestrictionsGrid;
		private ZArchitecture.GUI.ZGroupBox TaskTypesGroupBox;
		private ZArchitecture.ZGrid TaskTypesGrid;
	}
}
