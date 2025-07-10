using System.Threading;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[UseSnapshotProtection]
	sealed class DocDeliveryFormBackgroundDeliveryTest : TestCase
	{
		public void TestDeliver_Report_BackgroundDelivery()
		{
			var factory = new BusinessObjectFactory();
			var command = factory.NewWithValidTestData<ReportCommand>();
			factory.Save();

			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			var testReport = new Report(pack, null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			instructions.DeliverablesToBePrinted.Add(testReport);
			instructions.Recipients.RemoveAndDeleteAll();
			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.DeliveryAddress = "test@edi.com.au";

			Env.Registry.DeliverReportsInBackground = true;
			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertEquals("No errors", false, instructions.HasErrors);
				AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DialogResult", DialogResult.Cancel, form.DialogResult);
				AssertNotNull("Schedule created", form.scheduleTask);
				AssertEquals(true, form.scheduleTask.IsInDatabase);
				AssertEquals(false, form.scheduleTask.IsDeleted);
				AssertEquals(true, form.scheduleTask.S5_IsActive);
				AssertEquals(true, form.scheduleTask.S5_IsPrivate);
				AssertEquals(false, testReport.ShouldUpdateSchedulableFilters);
				Assert(form.scheduleTask.S5_NextScheduledPrintRunTimeUtc <= ZDateTime.UtcNow);
				var notifications = new NotificationBuffer();
				new ScheduleTaskRunner().Process(StmMenuItemSchema.Constants.Prefix, true, notifications, new CancellationToken());
				Assert(notifications.AsString, !notifications.HasErrors);
				var scheduleTask = new BusinessObjectFactory().Load(form.scheduleTask.GetType(), form.scheduleTask.PK);
				AssertNull(scheduleTask);
			}
		}

		[TestTimeZoneUNLOCO("NZAKL")]
		public void TestScheduleTaskRunsInBackgroundInNZ()
		{
			AssertScheduleTaskRunsImmediately();
		}

		[TestTimeZoneUNLOCO("AUPER")]
		public void TestScheduleTaskRunsInBackgroundInPerth()
		{
			AssertScheduleTaskRunsImmediately();
		}

		[TestTimeZoneUNLOCO("USLAX")]
		public void TestScheduleTaskRunsInBackgroundInLosAngeles()
		{
			AssertScheduleTaskRunsImmediately();
		}

		void AssertScheduleTaskRunsImmediately()
		{
			DeliveryInstructions instructions = DocDeliveryFormTestHelper.CreateInstructionsWithValidData(new BusinessObjectFactory());
			instructions.Recipients[0].DeliveryMethod = "xxx";

			try
			{
				using (var form = new MockDocDeliveryForm(instructions))
				{
					form.Show();
					instructions.BackgroundDelivery = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

					form.DeliverButton.PerformClick();
					AssertNull("scheduleTask not created", form.scheduleTask);

					instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					instructions.Recipients[0].DeliveryAddress = "bob@bob.com";
					form.DeliverButton.PerformClick();
					AssertNotNull("scheduleTask created", form.scheduleTask);
					Assert("scheduleTask saved", form.scheduleTask.IsInDatabase);
					Assert(!form.scheduleTask.IsDeleted);
					new ScheduleTaskRunner().Process(StmMenuItemSchema.Constants.Prefix, true, new NotificationBuffer(), new CancellationToken());
					var scheduleTask = new BusinessObjectFactory().Load(form.scheduleTask.GetType(), form.scheduleTask.PK);
					AssertNull("scheduleTask.IsDeleted - Will not be deleted unless the task was run.", scheduleTask);
				}
			}
			finally
			{
				instructions?.DocPack?.Dispose();
			}
		}
	}
}
