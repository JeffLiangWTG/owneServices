using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobManagement.Testing
{
	[TestedType(typeof(BulkJobCloseForm))]
	public class BulkJobCloseFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = Factory.New<CommonShipment>();
			consol.Shipments.Add(shipment);

			Factory.Save();

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_JobNum = shipment.JS_UniqueConsignRef;
			job.JH_GB = Env.CurrentBranch.PK;
			job.JH_GE = Env.CurrentDepartment.PK;

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = new TestObjectCreator(Factory).CC1.PK;
			charge.JR_LocalSellAmt = 1818m;

			Factory.Save();

			var processor = new BulkJobCloseProcessor(null);
			return new BulkJobCloseForm(processor);
		}

		public void TestOverallFunctionality()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.CreateJob(testObjectCreator.CreateShipment("S0001"), false);
			job.JH_A_JOP = Env.Time.CurrentLocalDate.AddDays(-1);
			job.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var processor = new BulkJobCloseProcessor(null);
			AttemptToShowForm(processor, delegate(BulkJobCloseForm form)
			{
				form.BtnFind_ForTestOnly.PerformClick();
				AssertEquals("No Date filter selected message", @"Please enter a value.
You must fill at least one of the following filters
- Job Open Date
- Job Last Edit Time", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Number of Jobs", "<Please set required search conditions and click Find>", form.ZlblTotalNumberOfJobMessage_ForTestOnly.Text);
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			processor.JobStatusFilter = JobHeaderStatus.Working.Code;
			processor.JobOpenDateFilter.PropertySearch = "Date Range";
			processor.JobOpenDateFilter.Property1 = Env.Time.CurrentLocalDate.AddDays(-7);
			processor.JobOpenDateFilter.Property2 = Env.Time.CurrentLocalDate;
			AttemptToShowForm(processor, delegate(BulkJobCloseForm form)
			{
				AssertEquals("Number of Jobs", "<One or more Search conditions have changed. Please Click Find>", form.ZlblTotalNumberOfJobMessage_ForTestOnly.Text);
				form.BtnFind_ForTestOnly.PerformClick();
				Assert("No Error Message", string.IsNullOrWhiteSpace(UnitTestUserNotification.Instance.LastMessage.Text));
				Assert(form.ZlblTotalNumberOfJobMessage_ForTestOnly.Visible);
				AssertEquals("Number of Jobs", @"Total Number of Job Found: 1", form.ZlblTotalNumberOfJobMessage_ForTestOnly.Text);

				form.BtnCloseJob_ForTestOnly.PerformClick();
				AssertEquals("Job Close Message", "1 out of 1 Job(s) have been closed.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.BtnCloseJob_ForTestOnly.PerformClick();
				AssertEquals("Job Close Message", "There is no Job to close. Please click 'Find' to select jobs that you want to close", UnitTestUserNotification.Instance.LastMessage.Text);

				form.BtnClear_ForTestOnly.PerformClick();
				AssertEquals(string.Empty, processor.JobStatusFilter);
				AssertEquals(true, processor.JobOpenDateFilter.IsEmpty);
				AssertEquals(true, processor.JobLastEditDateFilter.IsEmpty);

				form.BtnCloseJob_ForTestOnly.PerformClick();
				AssertEquals("There is no Job to close. Please click 'Find' to select jobs that you want to close", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestForm_EnableBulkDisbursementJobsClosure()
		{
			var processor = new BulkJobCloseProcessor(null);

			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BulkJobCloseForm form = new BulkJobCloseForm(processor))
			{
				form.Show();
				Assert(form.ZNote4_ForTestOnly.Visible);
				AssertEquals("Notes Search Result", "2. Jobs that contain posted disbursement clearing balance will be automatically excluded.", form.ZNote4_ForTestOnly.Text);
			}

			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (BulkJobCloseForm form = new BulkJobCloseForm(processor))
			{
				form.Show();
				Assert(!form.ZNote4_ForTestOnly.Visible);
				AssertNullOrEmpty(form.ZNote4_ForTestOnly.Text);
			}
		}

		void AttemptToShowForm(BulkJobCloseProcessor processor, Action<BulkJobCloseForm> action)
		{
			using (BulkJobCloseForm form = new BulkJobCloseForm(processor))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("GroupBox Caption", "Search Conditions", form.ZGroupBoxSrchCondition_ForTestOnly.Text);
				AssertEquals("GroupBox Caption", "Search Result", form.ZGroupBoxSrchResult_ForTestOnly.Text);
				AssertEquals("GroupBox Caption", "Set ‘Job Close Date’", form.ZGroupBoxJobCloseDate_ForTestOnly.Text);
				AssertEquals("Notes", "Notes:", form.ZNoteLabel1_ForTestOnly.Text);
				AssertEquals("Notes", "Notes:", form.ZNoteLabel2_ForTestOnly.Text);
				AssertEquals("Notes Search Result", "1. Jobs with open WIP/ACR or Unrecognized Revenue/Cost will be automatically excluded.", form.ZNote1_ForTestOnly.Text);
				AssertEquals("Notes 1 Job Close Date", "1. If Job Open Date for a job is later than the Job Close Date specified above, then the later date will be used as Job Close Date.", form.ZNote2_ForTestOnly.Text);
				AssertEquals("Notes 2 Job Close Date", "2. Closing Jobs will not run workflow trigger.", form.ZNote3_ForTestOnly.Text);
				AssertEquals("Notes 3 Job Close Date", "3. Inactive Jobs will not be closed.", form.ZNote5_ForTestOnly.Text);
				action(form);
			}
		}
	}
}
