using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(MultiJobHeaderEditorForm))]
	class MultiJobHeaderEditorFormTest : ZFormBasherTest
	{
		#region Mass Update

		[TestDate(2015, 7, 14)]
		public void TestMassUpdate_WhenNoRowsSelected_ShouldDisplayDialog()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { jobHeader }, Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;

				viewModel.EarliestStartDateLocal = ZDateTime.Now;
				form.FindAndClickButton("UpdateESDButton");

				AssertEquals(ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);
				AssertEquals("Please select one or more rows in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				UnitTestUserNotification.Instance.ClearMessages();
				var grid = form.SchedulesGrid;

				grid.Select(0);
				Application.DoEvents();

				form.FindAndClickButton("UpdateESDButton");
				form.FireSaveButton();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDateTime.Now, jobHeader.DoNotStartBeforeDateLocal);
			}
		}

		[TestDate(2016, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestMassUpdate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { loadedJobHeader }.Concat(loadedJobHeader.ProcessHeaders), Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;

				AssertNotNull("Form business entity needs a factory", viewModel.Factory);
				AssertNotEquals("Form business entity factory should not be the same as the workflow providers are loaded with. This ensures we actually save the right factory since the module menu item creates a new factory to avoid excessive OnFactorySaving calls.",
					newFactory, viewModel.Factory);

				viewModel.EarliestStartDateLocal = new ZDateTime(2016, 6, 23);
				viewModel.AgreedDeliveryDateLocal = new ZDateTime(2016, 7, 23);

				form.FireSaveButton();

				AssertEquals(ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);

				AssertEquals(ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow1.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow3.AgreedDeliveryDateLocal);

				var grid = form.SchedulesGrid;

				grid.Select(0);
				grid.Select(1);
				Application.DoEvents();

				((ZButton)form.Controls.Find("UpdateESDButton", true)[0]).PerformClick();

				AssertEquals(new ZDateTime(2016, 6, 23), viewModel.JobHeaderViews[0].EarliestStartDateLocal);
				AssertEquals(new ZDateTime(2016, 6, 23), viewModel.JobHeaderViews[1].EarliestStartDateLocal);
				AssertEquals(ZDateTime.Empty, viewModel.JobHeaderViews[2].EarliestStartDateLocal);
				AssertEquals(ZDateTime.Empty, viewModel.JobHeaderViews[3].EarliestStartDateLocal);

				form.FireSaveButton();

				AssertEquals(new ZDateTime(2016, 6, 23), jobHeader.DoNotStartBeforeDateLocal);
				AssertEquals(new ZDateTime(2016, 6, 23), workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);

				AssertEquals(ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow1.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow3.AgreedDeliveryDateLocal);

				grid.Select(2);
				Application.DoEvents();
				((ZButton)form.Controls.Find("UpdateESDButton", true)[0]).PerformClick();

				AssertEquals(new ZDateTime(2016, 6, 23), viewModel.JobHeaderViews[0].EarliestStartDateLocal);
				AssertEquals(new ZDateTime(2016, 6, 23), viewModel.JobHeaderViews[1].EarliestStartDateLocal);
				AssertEquals(new ZDateTime(2016, 6, 23), viewModel.JobHeaderViews[2].EarliestStartDateLocal);
				AssertEquals(ZDateTime.Empty, viewModel.JobHeaderViews[3].EarliestStartDateLocal);

				form.FireSaveButton();

				AssertEquals(new ZDateTime(2016, 6, 23), jobHeader.DoNotStartBeforeDateLocal);
				AssertEquals(new ZDateTime(2016, 6, 23), workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(new ZDateTime(2016, 6, 23), workflow2.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);

				grid.Select(0);
				((ZButton)form.Controls.Find("UpdateADDButton", true)[0]).PerformClick();

				AssertEquals(new ZDateTime(2016, 7, 23), viewModel.JobHeaderViews[0].AgreedDeliveryDateLocal);
				AssertEquals(new ZDateTime(2016, 7, 23), viewModel.JobHeaderViews[1].AgreedDeliveryDateLocal);
				AssertEquals(new ZDateTime(2016, 7, 23), viewModel.JobHeaderViews[2].AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, viewModel.JobHeaderViews[3].AgreedDeliveryDateLocal);

				form.FireSaveButton();

				AssertEquals(new ZDateTime(2016, 7, 23), jobHeader.AgreedDeliveryDateLocal);
				AssertEquals(new ZDateTime(2016, 7, 23), workflow1.AgreedDeliveryDateLocal);
				AssertEquals(new ZDateTime(2016, 7, 23), workflow2.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow3.AgreedDeliveryDateLocal);
			}
		}

		[TestDate(2016, 1, 1)]
		public void TestMassUpdate_NoValueEntered()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");

			jobHeader.DoNotStartBeforeDateLocal = new ZDateTime(2016, 6, 23);
			jobHeader.AgreedDeliveryDateLocal = new ZDateTime(2016, 7, 23);

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { jobHeader }, Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;

				var grid = form.SchedulesGrid;

				grid.Select(0);
				Application.DoEvents();

				((ZButton)form.Controls.Find("UpdateESDButton", true)[0]).PerformClick();
				AssertEquals(ZDateTime.Empty, viewModel.JobHeaderViews[0].EarliestStartDateLocal);

				form.FireSaveButton();

				AssertEquals(ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);
				AssertEquals(new ZDateTime(2016, 7, 23), jobHeader.AgreedDeliveryDateLocal);

				((ZButton)form.Controls.Find("UpdateADDButton", true)[0]).PerformClick();
				AssertEquals(ZDateTime.Empty, viewModel.JobHeaderViews[0].AgreedDeliveryDateLocal);

				form.FireSaveButton();

				AssertEquals(ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
			}
		}

		public void TestDoubleClick_OpenForm()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");
			Factory.Save();

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { jobHeader }, Factory))
			{
				form.Show();
				var control = form.FindAll<MultiJobHeaderEditorUserControl>().Single();
				var grid = form.SchedulesGrid;

				grid.Select(0);
				control.SchedulesZGrid_DoubleClick(null, new MouseEventArgs(MouseButtons.Left, 2, 10, 30, 0));

				using (var organisationsForm = Application.OpenForms.OfType<ZOrganisationsForm>().FirstOrDefault())
				{
					AssertNotNull(organisationsForm);
				}
			}
		}

		public void TestDoubleClick_HandleWithTemplate()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var jobHeader = template.GetJobHeader();

			Factory.Save();

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { jobHeader }, Factory))
			{
				form.Show();
				var control = form.FindAll<MultiJobHeaderEditorUserControl>().Single();
				var grid = form.SchedulesGrid;

				grid.Select(0);
				control.SchedulesZGrid_DoubleClick(null, new MouseEventArgs(MouseButtons.Left, 2, 10, 30, 0));
				using (var templateForm = Application.OpenForms.OfType<ProcessTaskTemplateForm>().FirstOrDefault())
				{
					AssertNotNull(templateForm);
					AssertEquals("Edit Workflow Template", templateForm.Text);
				}
			}
		}

		#endregion

		#region Incremental Mass Updates

		[TestDate(2015, 7, 14)]
		public void TestMassUpdateIncrementalValues_WhenNoRowsSelected_ShouldDisplayDialog()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { jobHeader }, Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;

				viewModel.AgreedDeliveryDateDays = 1;
				form.FindAndClickButton("IncrementADDButton");

				AssertEquals(ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
				AssertEquals("Please select one or more rows in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				UnitTestUserNotification.Instance.ClearMessages();
				var grid = form.SchedulesGrid;

				grid.Select(0);
				Application.DoEvents();

				form.FindAndClickButton("IncrementADDButton");
				form.FireSaveButton();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(new ZDateTime(2015, 7, 15), jobHeader.AgreedDeliveryDateLocal);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestMassUpdateIncrementalValues()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { workflow1, workflow2 }, Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;
				var grid = form.SchedulesGrid;

				grid.Select(0);
				Application.DoEvents();

				viewModel.AgreedDeliveryDateDays = 1;
				viewModel.AgreedDeliveryDateOffset = new ZInt(20).GetDateTimeFromMinutes();

				viewModel.EarliestStartDateDays = -1;
				viewModel.EarliestStartDateOffset = new ZInt(10).GetDateTimeFromMinutes();

				form.FindAndClickButton("IncrementADDButton");
				form.FindAndClickButton("IncrementESDButton");

				form.FireSaveButton();

				AssertEquals(new ZDateTime(2015, 7, 15, 0, 20, 0), workflow1.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);

				AssertEquals(new ZDateTime(2015, 7, 13, 0, 10, 0), workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestMassUpdateIncrementalValues_WithEmptyOffsets()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { workflow1, workflow2 }, Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;
				var grid = form.SchedulesGrid;

				grid.Select(0);
				Application.DoEvents();

				viewModel.AgreedDeliveryDateDays = 1;
				viewModel.EarliestStartDateDays = -1;

				form.FindAndClickButton("IncrementADDButton");
				form.FindAndClickButton("IncrementESDButton");

				form.FireSaveButton();

				AssertEquals(new ZDateTime(2015, 7, 15), workflow1.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);

				AssertEquals(new ZDateTime(2015, 7, 13), workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestMassUpdateIncrementalValues_WithLargeValues()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { workflow1, workflow2 }, Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;
				var grid = form.SchedulesGrid;

				grid.Select(0);
				Application.DoEvents();

				viewModel.AgreedDeliveryDateDays = 99999;
				viewModel.EarliestStartDateDays = -99999;

				form.FindAndClickButton("IncrementADDButton");
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.FindAndClickButton("IncrementESDButton");
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.FireSaveButton();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(ZDateTime.Empty, workflow1.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);

				AssertEquals(ZDateTime.Empty, workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);

				viewModel.AgreedDeliveryDateDays = 1;
				viewModel.EarliestStartDateDays = -1;

				form.FindAndClickButton("IncrementADDButton");
				form.FindAndClickButton("IncrementESDButton");

				form.FireSaveButton();

				AssertEquals(new ZDateTime(2015, 7, 15), workflow1.AgreedDeliveryDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);

				AssertEquals(new ZDateTime(2015, 7, 13), workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
			}
		}

		#endregion

		#region Validation

		[TestDate(2015, 7, 14)]
		public void TestSave_WhenDateWouldBeBeyondRangeOfProcessHeaderColumns()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var earliestAllowedDate = ZDateTime.MinSmallDateTimeValue;
			var latestAllowedDate = ZDateTime.MaxSmallDateTimeValue;

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { workflow1, workflow2 }, Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;
				var grid = form.SchedulesGrid;

				grid.Select(0);
				Application.DoEvents();

				viewModel.EarliestStartDateLocal = earliestAllowedDate.AddDays(-1);
				viewModel.AgreedDeliveryDateLocal = latestAllowedDate.AddDays(1);

				form.FindAndClickButton("UpdateESDButton");
				form.FindAndClickButton("UpdateADDButton");

				form.FireSaveButton();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDateTime.Empty, workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow1.AgreedDeliveryDateLocal);

				viewModel.EarliestStartDateLocal = earliestAllowedDate;
				viewModel.AgreedDeliveryDateLocal = latestAllowedDate;

				form.FindAndClickButton("UpdateESDButton");
				form.FindAndClickButton("UpdateADDButton");

				form.FireSaveButton();

				AssertNoErrors(viewModel);

				AssertEquals(earliestAllowedDate, workflow1.FH_DoNotStartBeforeDate);
				AssertEquals(latestAllowedDate.AddHours(-10), workflow1.FH_AgreedDeliveryDate);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestSaveAfterMultipleValueChanges_WhenDateWouldBeBeyondRangeOfProcessHeaderColumns()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { workflow1, workflow2 }, Factory))
			{
				form.Show();
				Application.DoEvents();

				var viewModel = (MultiJobHeaderEditorViewModel)form.BusinessEntity;
				var grid = form.SchedulesGrid;

				grid.Select(0);
				Application.DoEvents();

				viewModel.AgreedDeliveryDateDays = -9999;
				viewModel.EarliestStartDateDays = -9999;

				for (var i = 0; i < 10; i++)
				{
					form.FindAndClickButton("IncrementADDButton");
					form.FindAndClickButton("IncrementESDButton");
				}

				form.FireSaveButton();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				var view = viewModel.JobHeaderViews.Cast<JobHeaderView>().Single(v => v.ProcessHeader == workflow1);
				AssertHasError(view.AgreedDeliveryDateLocalInfo, "The date '08-Oct-1741' is earlier than '01-Jan-1900', the limit for this field.");
				AssertHasError(view.EarliestStartDateLocalInfo, "The date '08-Oct-1741' is earlier than '01-Jan-1900', the limit for this field.");

				AssertEquals(ZDateTime.Empty, workflow1.DoNotStartBeforeDateLocal);
				AssertEquals(ZDateTime.Empty, workflow1.AgreedDeliveryDateLocal);

				viewModel.EarliestStartDateLocal = ZDateTime.MinSmallDateTimeValue;
				viewModel.AgreedDeliveryDateLocal = ZDateTime.MaxSmallDateTimeValue;

				form.FindAndClickButton("UpdateESDButton");
				form.FindAndClickButton("UpdateADDButton");

				form.FireSaveButton();

				AssertNoErrors(viewModel);

				AssertEquals(ZDateTime.MinSmallDateTimeValue, workflow1.FH_DoNotStartBeforeDate);
				AssertEquals(ZDateTime.MaxSmallDateTimeValue.AddHours(-10), workflow1.FH_AgreedDeliveryDate);
			}
		}

		#endregion

		#region Security

		public void TestShowForm_WhenNoSecurity()
		{
			Env.Security.WorkflowHeadersMultiJobScheduling.IsAllowed = false;

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(System.Array.Empty<BusinessObject>(), Factory))
			{
				AssertNull(form);
				AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Workflow & Process -> Job Workflows -> Edit Job Schedules", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.WorkflowHeadersMultiJobScheduling.IsAllowed = true;

			using (var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(System.Array.Empty<BusinessObject>(), Factory))
			{
				AssertNotNull(form);
			}
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637617 - This is needed to have a valid BMSystem, see http://crikey.wtg.zone/TestResults/ee1d3757-f61e-40ab-9941-fa3c4b90abcc")]
		SchematicTestConfig config;

		protected override Form GetFormToBashCore()
		{
			var dummy = (BusinessObject)BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			return MultiJobHeaderEditorForm.ShowFormIfAllowed(new[] { dummy }, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestCaseWithFactory.EnableBMSInRegistry();
			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
		}

		#endregion
	}
}
