namespace Enterprise.BufferManagement.GUI
{
	partial class ProcessHeaderLinkForm
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
		protected new void InitializeComponent()
		{
			this.LinkTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SyncBufferPenetrationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TimeDelayFactorCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReleaseDelayTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.ToWorkflowTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWorkflowTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWorkflowSelectorButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToWorkflowSelectorButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OpenPrereqButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OpenDependentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StaggeredStartsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LinkTypeDropEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.StaggeredStartsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 199, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.StaggeredStartsGroupBox);
			this.MainTabPage.Controls.Add(this.DetailsGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 172, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 172, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 172, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 199, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ProcessHeaderLink);
			// 
			// LinkTypeDropEdit
			// 
			this.LinkTypeDropEdit.AllowDrop = true;
			this.LinkTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LinkTypeDropEdit, "FP_LinkType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(null)).FP_LinkType)));
			this.LinkTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 66, true);
			this.LinkTypeDropEdit.Name = "LinkTypeDropEdit";
			this.LinkTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 20, true);
			this.LinkTypeDropEdit.TabIndex = 6;
			// 
			// SyncBufferPenetrationCheckBox
			// 
			this.SyncBufferPenetrationCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SyncBufferPenetrationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SyncBufferPenetrationCheckBox, "FP_SynchroniseBufferPenetration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(null)).FP_SynchroniseBufferPenetration)));
			this.SyncBufferPenetrationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SyncBufferPenetrationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 68, true);
			this.SyncBufferPenetrationCheckBox.Name = "SyncBufferPenetrationCheckBox";
			this.SyncBufferPenetrationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 17, true);
			this.SyncBufferPenetrationCheckBox.TabIndex = 7;
			this.SyncBufferPenetrationCheckBox.UseVisualStyleBackColor = true;
			// 
			// TimeDelayFactorCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TimeDelayFactorCalcEdit, "FP_TimeDelayFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(null)).FP_TimeDelayFactor)));
			this.TimeDelayFactorCalcEdit.DecimalPlaces = 1;
			this.TimeDelayFactorCalcEdit.Decimals = 1;
			this.TimeDelayFactorCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 18, true);
			this.TimeDelayFactorCalcEdit.Name = "TimeDelayFactorCalcEdit";
			this.TimeDelayFactorCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TimeDelayFactorCalcEdit.TabIndex = 8;
			this.TimeDelayFactorCalcEdit.Text = "0.0";
			this.TimeDelayFactorCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReleaseDelayTimeEdit
			// 
			this.ReleaseDelayTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReleaseDelayTimeEdit, "StaggeredReleaseTimeDelay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(null)).StaggeredReleaseTimeDelay)));
			this.ReleaseDelayTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 18, true);
			this.ReleaseDelayTimeEdit.Name = "ReleaseDelayTimeEdit";
			this.ReleaseDelayTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ReleaseDelayTimeEdit.TabIndex = 9;
			// 
			// ToWorkflowTextBox
			// 
			this.ToWorkflowTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ToWorkflowTextBox, "ToHeaderDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(null)).ToHeaderDescription)));
			this.ToWorkflowTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ToWorkflowTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 40, true);
			this.ToWorkflowTextBox.Name = "ToWorkflowTextBox";
			this.ToWorkflowTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 20, true);
			this.ToWorkflowTextBox.TabIndex = 3;
			// 
			// FromWorkflowTextBox
			// 
			this.FromWorkflowTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FromWorkflowTextBox, "FromHeaderDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(null)).FromHeaderDescription)));
			this.FromWorkflowTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FromWorkflowTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 14, true);
			this.FromWorkflowTextBox.Name = "FromWorkflowTextBox";
			this.FromWorkflowTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 20, true);
			this.FromWorkflowTextBox.TabIndex = 0;
			// 
			// FromWorkflowSelectorButton
			// 
			this.FromWorkflowSelectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FromWorkflowSelectorButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 13, true);
			this.FromWorkflowSelectorButton.Name = "FromWorkflowSelectorButton";
			this.FromWorkflowSelectorButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 22, true);
			this.FromWorkflowSelectorButton.TabIndex = 1;
			this.FromWorkflowSelectorButton.Text = "...";
			this.FromWorkflowSelectorButton.UseVisualStyleBackColor = false;
			this.FromWorkflowSelectorButton.Click += new System.EventHandler(this.FromWorkflowSelectorButton_Click);
			// 
			// ToWorkflowSelectorButton
			// 
			this.ToWorkflowSelectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ToWorkflowSelectorButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 39, true);
			this.ToWorkflowSelectorButton.Name = "ToWorkflowSelectorButton";
			this.ToWorkflowSelectorButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 22, true);
			this.ToWorkflowSelectorButton.TabIndex = 4;
			this.ToWorkflowSelectorButton.Text = "...";
			this.ToWorkflowSelectorButton.UseVisualStyleBackColor = false;
			this.ToWorkflowSelectorButton.Click += new System.EventHandler(this.ToWorkflowSelectorButton_Click);
			// 
			// OpenPrereqButton
			// 
			this.OpenPrereqButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenPrereqButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("f2a28f1c-b0f0-49f1-a26f-4bda63f75cba", "Open");
			this.OpenPrereqButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 13, true);
			this.OpenPrereqButton.Name = "OpenPrereqButton";
			this.OpenPrereqButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 22, true);
			this.OpenPrereqButton.TabIndex = 2;
			this.OpenPrereqButton.UseVisualStyleBackColor = false;
			this.OpenPrereqButton.Click += new System.EventHandler(this.OpenPrereqButton_Click);
			// 
			// OpenDependentButton
			// 
			this.OpenDependentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenDependentButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("91186551-e8df-452d-9e63-37962ea91c70", "Open");
			this.OpenDependentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 39, true);
			this.OpenDependentButton.Name = "OpenDependentButton";
			this.OpenDependentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 22, true);
			this.OpenDependentButton.TabIndex = 5;
			this.OpenDependentButton.UseVisualStyleBackColor = false;
			this.OpenDependentButton.Click += new System.EventHandler(this.OpenDependentButton_Click);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4dd55e58-ee54-42d5-8dfc-d455e36105c5", "Details");
			this.DetailsGroupBox.Controls.Add(this.FromWorkflowTextBox);
			this.DetailsGroupBox.Controls.Add(this.OpenDependentButton);
			this.DetailsGroupBox.Controls.Add(this.LinkTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.OpenPrereqButton);
			this.DetailsGroupBox.Controls.Add(this.SyncBufferPenetrationCheckBox);
			this.DetailsGroupBox.Controls.Add(this.ToWorkflowSelectorButton);
			this.DetailsGroupBox.Controls.Add(this.ToWorkflowTextBox);
			this.DetailsGroupBox.Controls.Add(this.FromWorkflowSelectorButton);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 94, true);
			this.DetailsGroupBox.TabIndex = 10;
			this.DetailsGroupBox.TabStop = false;
			// 
			// StaggeredStartsGroupBox
			// 
			this.StaggeredStartsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StaggeredStartsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("e0719b12-d9eb-4ab3-9121-f758b7c9b5e8", "Staggered Release");
			this.StaggeredStartsGroupBox.Controls.Add(this.TimeDelayFactorCalcEdit);
			this.StaggeredStartsGroupBox.Controls.Add(this.ReleaseDelayTimeEdit);
			this.StaggeredStartsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 103, true);
			this.StaggeredStartsGroupBox.Name = "StaggeredStartsGroupBox";
			this.StaggeredStartsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 65, true);
			this.StaggeredStartsGroupBox.TabIndex = 11;
			this.StaggeredStartsGroupBox.TabStop = false;
			// 
			// ProcessHeaderLinkForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 255, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.ProcessHeaderLink);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 294, true);
			this.Name = "ProcessHeaderLinkForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LinkTypeDropEdit.ResumeLayout(true);
			this.LinkTypeDropEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.StaggeredStartsGroupBox.ResumeLayout(false);
			this.StaggeredStartsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZCheckBox SyncBufferPenetrationCheckBox;
		private ZArchitecture.GUI.ZDropEdit LinkTypeDropEdit;
		private ZArchitecture.GUI.ZTimeEditEx ReleaseDelayTimeEdit;
		private ZArchitecture.ZCalcEdit TimeDelayFactorCalcEdit;
		private ZArchitecture.ZTextBox ToWorkflowTextBox;
		private ZArchitecture.ZTextBox FromWorkflowTextBox;
		private ZArchitecture.GUI.ZButton FromWorkflowSelectorButton;
		private ZArchitecture.GUI.ZButton ToWorkflowSelectorButton;
		private ZArchitecture.GUI.ZButton OpenDependentButton;
		private ZArchitecture.GUI.ZButton OpenPrereqButton;
		private ZArchitecture.GUI.ZGroupBox StaggeredStartsGroupBox;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
	}
}