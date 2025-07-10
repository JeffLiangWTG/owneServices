using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class ReleaseBuildContentTest : ReleaseBuildContentTestDevGit
	{
		public void TestRetrieveReleaseInfo()
		{
			var workItem = Factory.New<NewWorkItem>();
			workItem.WKI_WorkItemNumber = "WI00010001";
			var collection = workItem.WorkflowItems;
			var processTaskView = new WorkItemProcessTaskCollectionViewFilter(collection);
			var processTask1 = collection.AddNew();
			processTask1.P9_Type = "CH0";
			processTask1.P9_Status = "CLS";
			var processTask2 = collection.AddNew();
			processTask2.P9_Type = "INV";
			processTask2.P9_Status = "ASN";
			CreateDeploymentBuild(workItem.PK.ToGuid(), processTask1.PK.ToGuid(), new Version(17, 1, 1, 10));
			CreateReleaseBuild(new Version(17, 1, 1, 10));
			using (ZForm form = new ZForm())
			using (EDITaskWithDetailsAndFilterControl control = new EDITaskWithDetailsAndFilterControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				control.SetDataBinding(processTaskView, "");
				AssertEquals("TasksView.Release", control.ReleaseInfoTextBox.BindTo);
				AssertEquals("TasksView.VersionNumber", control.VersionNoTextBox.BindTo);
				Assert(control.ReleaseInfoTextBox.ReadOnly);
				Assert(control.VersionNoTextBox.ReadOnly);
				AssertNotNull(control.RetrieveReleaseInfoButton);
				Assert(control.ReleaseInfoTextBox.Visible);
				Assert(control.VersionNoTextBox.Visible);
				Assert(control.RetrieveReleaseInfoButton.Visible);
				control.TasksGrid.PerformMouseDownForTest(1, 1);
				Assert(!control.ReleaseInfoTextBox.Visible);
				Assert(!control.VersionNoTextBox.Visible);
				Assert(!control.RetrieveReleaseInfoButton.Visible);
				control.TasksGrid.PerformMouseDownForTest(0, 1);
				control.RetrieveReleaseInfoButton.PerformClick();
				AssertReleaseBuild(control, "2017 Jan 01 patch 10", "17.1.1.10");
			}
		}

		public void TestRetrieveReleaseBuild_NoReleaseBuild()
		{
			var workItem = Factory.New<NewWorkItem>();
			workItem.WKI_WorkItemNumber = "WI00010001";
			var collection = workItem.WorkflowItems;
			var processTaskView = new WorkItemProcessTaskCollectionViewFilter(collection);
			var processTask1 = collection.AddNew();
			processTask1.P9_Type = "CH0";
			processTask1.P9_Status = "CLS";
			var processTask2 = collection.AddNew();
			processTask2.P9_Type = "INV";
			processTask2.P9_Status = "ASN";

			var uhPk = Guid.NewGuid();
			var utPk = Guid.NewGuid();
			AddUserTestHeader(uhPk, "DBL", workItem.PK.ToGuid(), processTask1.PK.ToGuid(), "PAS", ZDateTime.Now);
			AddUserTest(utPk, uhPk, null, TrunkDeploymentBranch, "PAS");
			AddBuildJobResult(utPk, "RELEASE", new Version(17, 1, 1, 10), ZDateTime.Now.AddMilliseconds(1));

			CreateDeploymentBuild(Guid.NewGuid(), processTask2.PK.ToGuid(), new Version(17, 1, 1, 11)); // no release build
			CreateDeploymentBuild(Guid.NewGuid(), processTask2.PK.ToGuid(), new Version(17, 1, 1, 12));
			CreateReleaseBuild(new Version(17, 1, 1, 12));
			CreateDeploymentBuild(Guid.NewGuid(), processTask2.PK.ToGuid(), new Version(17, 1, 1, 13));
			CreateReleaseBuild(new Version(17, 1, 1, 13));
			using (var form = new ZForm())
			using (var control = new EDITaskWithDetailsAndFilterControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				control.SetDataBinding(processTaskView, "");
				control.RetrieveReleaseInfoButton.PerformClick();
				AssertReleaseBuild(control, "2017 Jan 01 patch 12", "17.1.1.12");
			}
		}

		static void AssertReleaseBuild(EDITaskWithDetailsAndFilterControl control, string releaseInfoText,
			string releaseVersion)
		{
			AssertEquals(releaseInfoText, control.ReleaseInfoTextBox.Text);
			AssertEquals(releaseVersion, control.VersionNoTextBox.Text);
		}

		public void TestRetrieveCombinedBuildReleaseInfo()
		{
			var workItem = Factory.New<NewWorkItem>();
			workItem.WKI_WorkItemNumber = "WI00010001";
			var collection = workItem.WorkflowItems;
			var processTaskView = new WorkItemProcessTaskCollectionViewFilter(collection);
			var processTask = collection.AddNew();
			processTask.P9_Type = "CH0";
			processTask.P9_Status = "CLS";
			var combinedBuildPk = Guid.NewGuid();
			CreateDeploymentBuild(combinedBuildPk, processTask.PK.ToGuid(), new Version(17, 1, 1, 10));
			CreateReleaseBuild(new Version(17, 1, 1, 10));
			using (ZForm form = new ZForm())
			using (EDITaskWithDetailsAndFilterControl control = new EDITaskWithDetailsAndFilterControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				control.SetDataBinding(processTaskView, "");
				control.RetrieveReleaseInfoButton.PerformClick();
				AssertReleaseBuild(control, "2017 Jan 01 patch 10", "17.1.1.10");
			}
		}
	}
}
