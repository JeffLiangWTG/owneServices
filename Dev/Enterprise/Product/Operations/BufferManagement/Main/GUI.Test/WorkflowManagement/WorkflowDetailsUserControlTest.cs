using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test.WorkflowManagement
{
	class WorkflowDetailsUserControlTest : BMSTestCaseWithFactory
	{
		public void TestSequencingGroupBox_WhenReleaseSequencesModuleEnabled_ShouldBeVisible()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var control = new WorkflowDetailsUserControl())
			{
				AssertEquals(true, control.SequencingGroupBox.Visible);
			}
		}

		public void TestSequencingGroupBox_WhenReleaseSequenceModuleDisabled_ShouldNotBeVisible()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var control = new WorkflowDetailsUserControl())
			{
				AssertEquals(false, control.SequencingGroupBox.Visible);
			}
		}

		public void TestWorkflowDetailsUserControl_ShouldResizeToFitGroupBoxes()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var form = new ZForm())
			using (var control = new WorkflowDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.SequencingGroupBox.Visible);
				var heightWithVisible = control.Height;

				control.SequencingGroupBox.Visible = false;
				AssertLessThan(control.Height, heightWithVisible);

				control.SequencingGroupBox.Visible = true;
				AssertEquals(control.Height, heightWithVisible);
			}
		}

		public void TestOpenSequenceButton_WhenWorkflowNotLinkedToSequence_ShouldBeDisabled()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(workflow, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.OpenSequenceButton.Enabled);
			}
		}

		public void TestOpenSequenceButton_OnClick()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://EagleDatamationInternational/Portals");

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			BMSTestHelper.CreateReleaseSequenceItem(sequence, jobHeader, position: 2);

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(workflow, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.SequencingGroupBox.Visible);
				AssertEquals(true, control.OpenSequenceButton.Enabled);

				control.OpenSequenceButton.PerformClick();

				var recentLink = RecentItemManager.Instance.GetRecentItems(string.Empty).First();
				AssertEquals(recentLink.STL_ModuleID, ModuleIDs.BMReleaseSequence.Name);
				AssertEquals(recentLink.STL_ItemPK, sequence.PK);
				AssertEquals(recentLink.STL_ItemUrl, ShowEditFormUrlHandler.Instance.Create(ControllerIDs.BMReleaseSequence, sequence.PK));
			}
		}

		public void TestParentJobLink_OnClick()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://EagleDatamationInternational/Portals");

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			BMSTestHelper.CreateReleaseSequenceItem(sequence, jobHeader, position: 2);

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(workflow, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.SequencingGroupBox.Visible);

				var formsOpen = Application.OpenForms.Count;

				control.SequenceParentJobLinkLabel.PerformClick_ForTest();

				AssertEquals("One new form should have been opened", formsOpen + 1, Application.OpenForms.Count);
				AssertEquals("Should open JobHeader form", jobHeader.Parent.PK, ((Application.OpenForms[1] as ZOrganisationsForm).BusinessEntity as OrgHeader).PK);

				Application.OpenForms[1].Close();
			}
		}

		public void TestReleaseGroup_ReadOnlyness()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			Factory.Save();

			Environment.Env.Security.JLWReleaseGroupEdit.IsAllowed = false;

			using (var form = new ZForm(jobHeader))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.OpenSequenceButton.PerformClick();
				AssertEquals(true, control.ReleaseGroupFindBox.ReadOnly);
			}

			using (var form = new ZForm(workflow))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(workflow, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.OpenSequenceButton.PerformClick();
				AssertEquals(false, control.ReleaseGroupFindBox.ReadOnly);
			}

			Environment.Env.Security.JLWReleaseGroupEdit.IsAllowed = true;

			using (var form = new ZForm(jobHeader))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.OpenSequenceButton.PerformClick();
				AssertEquals(false, control.ReleaseGroupFindBox.ReadOnly);
			}

			using (var form = new ZForm(workflow))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(workflow, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.OpenSequenceButton.PerformClick();
				AssertEquals(false, control.ReleaseGroupFindBox.ReadOnly);
			}
		}

#if WINZOR
		public void TestWorkflowRelationshipDesignerButton_OnlyExists_ForPlanningManagement()
		{
			BMSRegistry.Instance.WorkflowManagementMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(workflow, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertNull(control.WorkflowRelationshipDesignerButton);
			}
		}
		public void TestWorkflowRelationshipDesignerButton_Visiblity_And_OnClick()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new WorkflowDetailsUserControl())
			{
				control.SetDataBinding(workflow, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.WorkflowRelationshipDesignerButton.Visible);
				AssertEquals(true, control.WorkflowRelationshipDesignerButton.Enabled);

				control.WorkflowRelationshipDesignerButton.PerformClick();
				AssertEquals(Application.OpenForms.Count, 2);
				AssertEquals(Application.OpenForms.Last().Name, "ZForm");
				Application.OpenForms[1].Close();
			}
		}
#endif

	}
}
