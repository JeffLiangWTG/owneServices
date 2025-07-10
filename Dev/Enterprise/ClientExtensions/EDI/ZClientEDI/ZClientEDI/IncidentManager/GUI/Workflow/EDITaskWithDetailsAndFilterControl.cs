using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI.Workflow;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class EDITaskWithDetailsAndFilterControl : TaskWithDetailsAndFilterControl
	{
		public EDITaskWithDetailsAndFilterControl()
		{
			this.BindingSource.DataSourceType = typeof(WorkItemProcessTask);
			Control parent = TasksGrid.Parent;
			while (parent != null)
			{
				if (parent is ZUserControl userControl && userControl.BindingSource.DataSourceType == typeof(ProcessTask))
				{
					userControl.BindingSource.DataSourceType = typeof(WorkItemProcessTask);
				}

				parent = parent.Parent;
			}

			ZCalcEditColumnStyleInfo defectsColumn = new ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				ColumnName = "DefectCount",
				Decimals = 0
			};

			ControlDpiScalingHelper.SetWidth(ref defectsColumn, 50, true);
			defectsColumn.Caption = "Defects";
			TasksGrid.ColumnStyles.Insert(5, defectsColumn);
			SetupSkillsTab();
			SetupReleaseInfoControls();
			GitPullRequestMenuItem.AttachToTaskDetailsControl(this);

			TasksGrid.AfterBind += TasksGrid_AfterBind;
		}

		void TasksGrid_AfterBind(object sender, EventArgs e)
		{
			DisplayOrHideReleaseInfoControls();
			TasksGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			DisplayOrHideReleaseInfoControls();
		}

		void RetrieveReleaseInfoButton_Click(object sender, EventArgs e)
		{
			if (SelectedTask != null)
			{
				if (SelectedTask.VersionNumber.IsEmpty)
				{
					var releaseBuildContent = new ReleaseBuildContent(SelectedTask.Factory);
					var releaseBuild = releaseBuildContent.GetReleaseBuild(SelectedTask.PK);
					if (releaseBuild != null)
					{
						SelectedTask.VersionNumber = releaseBuild.VersionNumber.ToString();
						SelectedTask.Release = releaseBuild.ReleaseDisplayText;
					}
				}
			}
		}

		void DisplayOrHideReleaseInfoControls()
		{
			var visible = SelectedTask != null && SelectedTask.IsCheckInTaskEvenWhenAssignedToDAT && SelectedTask.P9_Status == ProcessTaskStatusCodeList.Codes.Closed;
			this.ReleaseInfoTextBox.Visible = visible;
			this.VersionNoTextBox.Visible = visible;
			this.RetrieveReleaseInfoButton.Visible = visible;
		}

		public WorkItemProcessTask SelectedTask => (WorkItemProcessTask)TasksGrid.ListManager?.GetCurrent();

		protected readonly GitPullRequestMenuItem GitPullRequestMenuItem = new GitPullRequestMenuItem();
		internal ZTabPage RequiredSkillsTabPage = new ZTabPage();
		internal ZGroupBox RequiredSkillsGroupBox = new ZGroupBox();
		internal TaskSkillsControl SkillsControl = new TaskSkillsControl();

		internal ZTextBox VersionNoTextBox = new ZTextBox();
		internal ZTextBox ReleaseInfoTextBox = new ZTextBox();
		internal ZButton RetrieveReleaseInfoButton = new ZButton();

		void SetupSkillsTab()
		{
			this.DetailsTabControl.SuspendLayout();
			this.RequiredSkillsTabPage.SuspendLayout();
			this.SkillsControl.SuspendLayout();
			//
			// RequiredSkillsTabPage
			//
			this.RequiredSkillsTabPage.CaptionResourceString = ZClientEDI.Res.GetData("2E255BF2-3201-45D4-AA10-B7D534263181", "Competency Requirements");
			this.RequiredSkillsTabPage.Controls.Add(this.RequiredSkillsGroupBox);
			this.RequiredSkillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RequiredSkillsTabPage.Name = "RequiredSkillsTabPage";
			this.RequiredSkillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 302, true);
			this.RequiredSkillsTabPage.TabIndex = 3;
			//
			// RequiredSkillsGroupBox
			//
			this.RequiredSkillsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("613F62DD-9297-41E4-95CE-EF1CC7A12362", "Required Competency");
			this.RequiredSkillsGroupBox.Controls.Add(this.SkillsControl);
			this.RequiredSkillsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequiredSkillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequiredSkillsGroupBox.Name = "RequiredSkillsGroupBox";
			this.RequiredSkillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 302, true);
			this.RequiredSkillsGroupBox.TabIndex = 0;
			this.RequiredSkillsGroupBox.TabStop = false;
			//
			// SkillsControl
			//
			this.SkillsControl.AllowDrop = true;
			this.SkillsControl.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.SkillsControl, ".");
			this.SkillsControl.BindTo = "TasksView";
			this.SkillsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SkillsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SkillsControl.Name = "SkillsControl";
			this.SkillsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 552, true);
			this.SkillsControl.TabIndex = 0;
			this.SkillsControl.CreateSkillLearningTaskFunction = WorkItemProcessTask.CreateSkillLearningTask;

			DetailsTabControl.Controls.Add(RequiredSkillsTabPage);

			this.SkillsControl.ResumeLayout(true);
			this.SkillsControl.PerformLayout();
			this.RequiredSkillsTabPage.ResumeLayout(true);
			this.RequiredSkillsTabPage.PerformLayout();
			this.DetailsTabControl.ResumeLayout(true);
			this.DetailsTabControl.PerformLayout();
		}

		void SetupReleaseInfoControls()
		{
			this.DetailsTabControl.SuspendLayout();
			this.SimpleViewTabPage.SuspendLayout();
			this.SimpleDetailsGroupBox.SuspendLayout();
			this.SimpleNotesGroupBox.SuspendLayout();
			this.SimpleScheduleGroupBox.SuspendLayout();
			this.TasksControl.SuspendLayout();

			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 349, true);
			this.SimpleViewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 322, true);
			this.SimpleNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 191, true);
			this.SimpleScheduleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 184, true);

			this.SimpleDetailsGroupBox.Controls.Add(this.RetrieveReleaseInfoButton);
			this.SimpleDetailsGroupBox.Controls.Add(this.VersionNoTextBox);
			this.SimpleDetailsGroupBox.Controls.Add(this.ReleaseInfoTextBox);
			this.SimpleDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 184, true);

			// 
			// ReleaseInfoTextBox
			// 
			this.ReleaseInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.TasksControl.BindingSource.SetBindingMember(this.ReleaseInfoTextBox, "TasksView.Release");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WorkItemProcessTask)(null)).Release);
			this.ReleaseInfoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReleaseInfoTextBox.ReadOnly = true;
			this.ReleaseInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 158, true);
			this.ReleaseInfoTextBox.Name = "ReleaseInfoTextBox";
			this.ReleaseInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 17, true);
			this.ReleaseInfoTextBox.TabIndex = 7;
			// 
			// VersionNoTextBox
			// 
			this.VersionNoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																				 | System.Windows.Forms.AnchorStyles.Right);
			this.TasksControl.BindingSource.SetBindingMember(this.VersionNoTextBox, "TasksView.VersionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WorkItemProcessTask)(null)).VersionNumber);
			this.VersionNoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.VersionNoTextBox.ReadOnly = true;
			this.VersionNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 158, true);
			this.VersionNoTextBox.Name = "VersionNoTextBox";
			this.VersionNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.VersionNoTextBox.TabIndex = 8;
			// 
			// RetrieveReleaseInfoButton
			// 
			this.RetrieveReleaseInfoButton.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.RetrieveReleaseInfoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 157, true);
			this.RetrieveReleaseInfoButton.Name = "RetrieveReleaseInfoButton";
			this.RetrieveReleaseInfoButton.CaptionResourceString = ZClientEDI.Res.GetData("b7797c4e-8d81-4a68-af7a-0ae0e95a9004", "Find");
			this.RetrieveReleaseInfoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.RetrieveReleaseInfoButton.TabIndex = 9;
			this.RetrieveReleaseInfoButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RetrieveReleaseInfoButton.ToolTipCaption = null;
			this.RetrieveReleaseInfoButton.UseVisualStyleBackColor = true;
			this.RetrieveReleaseInfoButton.Click += new EventHandler(this.RetrieveReleaseInfoButton_Click);
			this.RetrieveReleaseInfoButton.TabIndex = 9;

			this.TasksControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 535, true);

			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.SimpleViewTabPage.ResumeLayout(false);
			this.SimpleViewTabPage.PerformLayout();
			this.SimpleDetailsGroupBox.ResumeLayout(false);
			this.SimpleDetailsGroupBox.PerformLayout();
			this.SimpleNotesGroupBox.ResumeLayout(false);
			this.SimpleNotesGroupBox.PerformLayout();
			this.SimpleScheduleGroupBox.ResumeLayout(false);
			this.SimpleScheduleGroupBox.PerformLayout();
			this.TasksControl.ResumeLayout(false);
			this.TasksControl.PerformLayout();
		}

		protected override ProcessTaskCollectionViewFilter CreateViewFilter(IWorkflowProvider workflowProvider)
		{
			return new WorkItemProcessTaskCollectionViewFilter((WorkItemProcessTaskCollection)workflowProvider.WorkflowItems);
		}
	}
}
