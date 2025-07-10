using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(PrintJobModule))]
	class PrintJobModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PrintJob;
		}

		public void TestModuleID()
		{
			using (PrintJobModule module = GetNewModule())
			{
				AssertEquals("ModuleID", GetModuleID(), module.ID);
			}
		}

		public void TestCheckpoints()
		{
			using (PrintJobModule module = GetNewModule())
			{
				AssertEquals("SecurityCheckpoint", ExpectedSecurityCheckpoint, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		protected virtual SecurityCheckpointNonOperationalAllowed ExpectedSecurityCheckpoint
		{
			get { return Env.Security.PrintJobs; }
		}

		protected virtual PrintJobModule GetNewModule()
		{
			return new PrintJobModule();
		}

		protected virtual IPrintJobModuleForTest GetNewModuleForTest()
		{
			return new PrintJobModuleForTest();
		}

		#region Properties

		public void TestGetNewFilterControl()
		{
			using (var module = GetNewModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is PrintJobFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = GetNewModuleForTest())
			{
				IBusinessObjectCollection printJobsCollection = module.NewGridCollection;
				Assert("Invalid type", printJobsCollection is StmPrintJobCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = GetNewModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is PrintJobFilterBusinessObject);
			}
		}

		#endregion

		public void TestDontAllowNewEdit()
		{
			using (PrintJobModule module = GetNewModule())
			{
				AssertEquals(false, module.AllowNew);
				AssertEquals(false, module.AllowEdit);
				AssertEquals(true, module.AllowDelete);
			}
		}

		public void TestViewMenuItemRemoved()
		{
			using (var module = GetNewModuleForTest())
			{
				AssertNull("View menu should not be available", module.GetNewStandardMenuItems().FindByText("&View"));
			}
		}

		public void TestResetStatusToQUEShouldResetRetryAttempts()
		{
			var job = Factory.NewWithValidTestData<StmPrintJob>();
			job.SP_RetryAttempts = 3;
			Factory.Save();

			AssertEquals("status should be FAL as the retryattempts is 3.", "FAL", job.SP_Status);

			using (var module = GetNewModuleForTest())
			{
				var queAction = module.GetNewActionMenuItems().FindByText("&Reset Status To QUE");
				AssertNotNull(queAction);

				module.PublicPerformSearch();
				module.SelectedJobs.Clear();
				module.SelectedJobs.Add(job);
				AssertEquals(1, ((IPrintJobView)module).GetSelectedJobs().Length);
				queAction.PerformClick();

				AssertEquals("status should be reset to QUE", nameof(PrintJobStatus.QUE), job.SP_Status);
				AssertEquals("RetryAttempts should be set to 0.", (byte)0, job.SP_RetryAttempts);
			}
		}

		public void TestResetStatusTo()
		{
			var job1 = Factory.NewWithValidTestData<StmPrintJob>();
			var job2 = Factory.NewWithValidTestData<StmPrintJob>();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var module = GetNewModuleForTest())
			{
				var queAction = module.GetNewActionMenuItems().FindByText("&Reset Status To QUE");
				var falAction = module.GetNewActionMenuItems().FindByText("&Reset Status To FAL");
				AssertNotNull(queAction);
				AssertNotNull(falAction);

				module.PublicPerformSearch();
				AssertEquals(0, ((IPrintJobView)module).GetSelectedJobs().Length);
				queAction.PerformClick();
				AssertEquals("Please select one or more Jobs to reset the Status.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				module.SelectedJobs.Add(job1);
				module.SelectedJobs.Add(job2);
				AssertEquals(2, ((IPrintJobView)module).GetSelectedJobs().Length);
				queAction.PerformClick();
				AssertEquals("No Jobs changed Status to QUE from 2 selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				falAction.PerformClick();
				AssertEquals("Reset Status to FAL for 2 Job(s) from 2 selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				module.SelectedJobs.Clear();
				module.SelectedJobs.Add(job1);
				AssertEquals(1, ((IPrintJobView)module).GetSelectedJobs().Length);
				queAction.PerformClick();
				AssertEquals("Reset Status to QUE for 1 Job(s) from 1 selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(nameof(PrintJobStatus.QUE), job1.SP_Status);
				AssertEquals(nameof(PrintJobStatus.FAL), job2.SP_Status);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;
	}
}
