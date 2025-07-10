using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(WorkflowTransferDiagnosisForm))]
	class WorkflowTransferDiagnosisFormTest : ZFormBasherTest
	{
		public void TestTransferAndBufferReleaseSplitContainer_Panel2_HasScrollbar()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.Save();

			using (var form = new WorkflowTransferDiagnosisForm(new WorkflowTransferDiagnosisViewModel(workflow)))
			{
				var container = (KSplitContainer)form.Controls.Find("TransferAndBufferReleaseSplitContainer", true)[0];
				AssertEquals(true, container.Panel2.AutoScroll);
			}
		}

		public void TestFilterRuleMatchingStatusShouldBeRenderedInBold_WhenFilterRulesHavePassed()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", sequence: 2);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.Save();

			using (var form = new WorkflowTransferDiagnosisForm(new WorkflowTransferDiagnosisViewModel(workflow)))
			{
				form.Show();

				var grid = (ZGrid)form.Controls.Find("TransferFailureGrid", true)[0];
				var transferFailure = new WorkflowTransferDiagnosis(link1_2, workflow);

				FieldInfo fontDecidingEventField = typeof(ZGrid).GetField("FontDeciding", BindingFlags.NonPublic | BindingFlags.Instance);
				System.EventHandler<FontDecidingEventArgs> fontDecidingEventHandler = (System.EventHandler<FontDecidingEventArgs>)fontDecidingEventField.GetValue(grid);

				FontDecidingEventArgs eventArgs = new FontDecidingEventArgs(transferFailure, string.Empty, form.Font);
				fontDecidingEventHandler(grid, eventArgs);
				AssertNull(eventArgs.Font);

				transferFailure.TryTransfer();
				eventArgs = new FontDecidingEventArgs(transferFailure, string.Empty, form.Font);
				fontDecidingEventHandler(grid, eventArgs);
				Assert(eventArgs.Font.Bold);
			}
		}

		public void TestFilterRuleMatchingStatus_ColumnShouldBeReadOnly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", sequence: 2);
			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.Save();

			using (var form = new WorkflowTransferDiagnosisForm(new WorkflowTransferDiagnosisViewModel(workflow)))
			{
				var grid = (ZGrid)form.Controls.Find("TransferFailureGrid", true)[0];
				var transferFailure = new WorkflowTransferDiagnosis(link1_2, workflow);

				AttributeCollection attributes = TypeDescriptor.GetProperties(transferFailure)["FilterRuleMatchingStatus"].Attributes;
				AssertEquals(ReadOnlyAttribute.Yes, attributes[typeof(ReadOnlyAttribute)]);
			}
		}

		public void TestShow()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			Factory.Save();

			WorkflowTransferDiagnosisForm.Show(workflow);
			AssertType<WorkflowTransferDiagnosisForm>(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestShow_NotInDatabase()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();

			WorkflowTransferDiagnosisForm.Show(workflow);
			AssertEquals("Please save the form first.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestShow_Deleted()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			Factory.Save();
			workflow.Delete();

			WorkflowTransferDiagnosisForm.Show(workflow);
			AssertEquals("This workflow has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestFilterRulesFilterStrips_ShouldBindCorrectly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3", sequence: 3);
			var bucket4 = BMSTestHelper.CreateBucket(system, "bucket4", sequence: 4);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2);
			var link1_3 = BMSTestHelper.LinkComponents(bucket1, bucket3);
			var link1_4 = BMSTestHelper.LinkComponents(bucket1, bucket4);

			FilterStripsTestHelper.AddFilterStrips(link1_3.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.QueueStatus });

			FilterStripsTestHelper.AddFilterStrips(link1_4.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.ReleaseGroup },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.PrerequisiteStatus },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.ResourceAssignedToAnyTask });

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.Save();

			using (var form = new WorkflowTransferDiagnosisForm(new WorkflowTransferDiagnosisViewModel(workflow)))
			{
				form.Show();

				AssertEquals("Should display empty strip control", 1, form.StripControlsCount_ForTest);

				form.ToComponentGrid_ForTest.ListManager.Position = 1;
				AssertEquals(2, form.StripControlsCount_ForTest);

				form.ToComponentGrid_ForTest.ListManager.Position = 2;
				AssertEquals(3, form.StripControlsCount_ForTest);

				form.ToComponentGrid_ForTest.ListManager.Position = 0;
				AssertEquals("Should display empty strip control", 1, form.StripControlsCount_ForTest);
			}
		}

		public void TestFiltersMatch_WhenReadOnly_ShouldLeavePopupButtonEnabled()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", sequence: 2);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2);

			FilterStripsTestHelper.AddFilterStrips(link1_2.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.ReleaseGroup,
					ComparisonOperatorSetter = f => ((ModuleGuidFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch,
				},
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow });

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = BMSTestHelper.CreateJobHeader(enquiry, addDefaultProcessHeaderIfNone: false);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var workflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow", releaseDateTime: ZDateTime.Now.AddDays(-3), description: "task 1", staffCode: "EXT", capability: capability);
			Factory.Save();

			using (var form = new WorkflowTransferDiagnosisForm(new WorkflowTransferDiagnosisViewModel(workflow)))
			{
				form.Show();
				form.FindSingle<BMFilterStripWrapperControl>().SetReadOnly(true);
				Application.DoEvents();

				AssertEquals("Should be 2 filter strips on the form", 2, form.StripControlsCount_ForTest);
				var findBox = form.FindSingle<ZFilterCollectionFindBox>();
				var button = findBox.PopupButton;
				AssertEquals("The popup button should always be enabled, even when its control is read-only, and yet...", true, button.Enabled);

				int checkBoxCounter = 0;
				foreach (var checkBox in form.FindAll<ZCheckBox>())
				{
					AssertEquals("All checkboxes should be not useable and yet...", true, checkBox.GetReadOnly() || !checkBox.Enabled);
					checkBoxCounter++;
				}

				AssertEquals("There should be 4 checkboxes on the form, and yet...", 4, checkBoxCounter);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			Factory.Save();

			return new WorkflowTransferDiagnosisForm(new WorkflowTransferDiagnosisViewModel(workflow));
		}
	}
}
