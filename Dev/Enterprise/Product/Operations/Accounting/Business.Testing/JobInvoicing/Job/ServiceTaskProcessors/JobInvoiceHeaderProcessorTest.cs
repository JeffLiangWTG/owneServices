using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using static Enterprise.Accounting.Business.JobInvoicing.Job;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobInvoiceHeaderProcessorTest : TestCaseWithFactory
	{
		public void TestProcessJobInvoiceHeaderWhenJobHeaderDoesNotExist()
		{
			int dBCountBeforeProcess = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertNull("Precondition - Shipment has no job invoice header", Shipment.Job);
			JobInvoiceHeaderProcessor.Process(Notifications);
			Assert(!Notifications.HasErrors);
			int dBCountAfterProcess = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertNotNull("Shipment has job invoice header", Shipment.Job);
			AssertEquals("Created jobheader is not saved to DB", dBCountBeforeProcess, dBCountAfterProcess);

			Factory.Save();

			dBCountAfterProcess = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Created jobheader is not saved to DB", dBCountBeforeProcess + 1, dBCountAfterProcess);
		}

		public void TestProcessJobInvoiceHeaderWhenJobHeaderDoesExist()
		{
			int dBCountBeforeProcess = Factory.GetDatabaseCount(typeof(JobHeader));
			Job = TestObjectCreator.CreateJob(Shipment, true, false);
			AssertNotNull("Precondition - Shipment has no job invoice header", Shipment.Job);
			JobInvoiceHeaderProcessor.Process(Notifications);
			Assert(!Notifications.HasErrors);
			int dBCountAfterProcess = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Should NOT create any jobheader", dBCountBeforeProcess, dBCountAfterProcess);
			AssertNotNull("Shipment has job invoice header", Shipment.Job);
			Job.Dispose();
		}

		public void TestProcessJobInvoiceHeaderWhenMutexErrorOccurs()
		{
			int dBCountBeforeProcess = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertNull("Precondition - Shipment has no job invoice header", Shipment.Job);
			Assert(!Notifications.HasErrors);

			using (ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Shipment.PK))
			{
				mutex.Lock();

				JobInvoiceHeaderProcessor.Process(Notifications);
				Assert(Notifications.HasErrors);
				AssertEquals("Critical Post Errors", @"You have created the job S00010001 on another form, but haven't saved it yet.
Please close or save other forms that use job S00010001 to continue.
", Notifications.AsString);
				int dBCountAfterProcess = Factory.GetDatabaseCount(typeof(JobHeader));
				AssertNull("Shipment has no job invoice header", Shipment.Job);
				AssertEquals("Should create 0 jobheader", dBCountBeforeProcess, dBCountAfterProcess);

				AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email subject", "The invoice header creation errors for Job S00010001", email.Subject);
				AssertEquals("Email body", @"<html>
<body>
<p>The invoice header S00010001 creation was ran as a workflow action. There were errors during the operation.</p>
<p>Errors:</p>
<ul><li>You have created the job S00010001 on another form, but haven't saved it yet.</li><li>Please close or save other forms that use job S00010001 to continue.</li></ul>
</body>
</html>", email.Body);
			}
		}

		public void TestProcessJobInvoiceHeaderWhenJobValidationErrorOccurs()
		{
			int dBCountBeforeProcess = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertNull("Precondition - Shipment has no job invoice header", Shipment.Job);
			Assert(!Notifications.HasErrors);

			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(Enterprise.ZArchitecture.Schema.GlbDepartmentSchema.GE_Code, "FES"));
			department.GE_Misc = true;

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>("Expected exception", "Create Job Invoice Header processing is not successful.", () => JobInvoiceHeaderProcessor.Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Email definition count", 1, exceptionThrown.Emails.Length);
			AssertEquals("EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert(Notifications.HasErrors);

			int dBCountAfterProcess = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertNull("The invalid Job Header should be deleted", Shipment.Job);
			AssertEquals("Should create no new jobheader", dBCountBeforeProcess, dBCountAfterProcess);
		}

		public void TestProcessJobInvoiceHeader_NotSaveWhenProcess()
		{
			JobInvoiceHeaderProcessor.Process(Notifications);
			Assert(!Notifications.HasErrors);
			AssertNotNull("Shipment has job invoice header", Shipment.Job);

			var newFactory = Factory.CreateNewFactory();
			var jobInDatabase = newFactory.Load<Job>(Shipment.Job.PK);
			AssertNull("Job should not be saved to DB", jobInDatabase);

			Factory.Save();

			jobInDatabase = newFactory.Load<Job>(Shipment.Job.PK);
			AssertNotNull("Job should be saved to DB", jobInDatabase);
		}

		public void TestProcessJobInvoiceHeader_DisposeAfterProcessSuccessfulAndSave()
		{
			JobInvoiceHeaderProcessor.Process(Notifications);
			Assert(!Notifications.HasErrors);
			AssertNotNull("Shipment has job invoice header", Shipment.Job);

			var newFactory = Factory.CreateNewFactory();
			var loader = new Job.Loader(newFactory, Shipment);

			using (var job = loader.TryCreateWithMutex())
			{
				AssertNull("Job not created due as the mutex could not be acquired", job);
			}

			Factory.Save();

			using (var job = loader.TryCreateWithMutex())
			{
				AssertNotNull("Job should be created because job created in Job Invoice Header Process has been dispose after Factory Save", job);
			}
		}

		public void TestProcessJobInvoiceHeader_DisposeAfterProcessFailed()
		{
			AssertNull("Precondition - Shipment has no job invoice header", Shipment.Job);
			Assert(!Notifications.HasErrors);

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(Enterprise.ZArchitecture.Schema.GlbDepartmentSchema.GE_Code, "FES"));
			department.GE_Misc = true;

			AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => JobInvoiceHeaderProcessor.Process(Notifications));
			Assert(Notifications.HasErrors);

			AssertNull("Job should be disposed and deleted after process failure.", Shipment.Job);

			var newFactory = Factory.CreateNewFactory();
			var loader = new Job.Loader(newFactory, Shipment);

			using (var job = loader.TryCreateWithMutex())
			{
				AssertNotNull("Job should be created because job created in Job Invoice Header Process has been dispose when Job Invoice Header Process failed", job);
			}
		}

		public void TestProcess_DisposeAndDeleteInvalidJob()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var plugin1 = testObjectCreator.CreateShipment("S000002");
			Factory.Save();

			var processor = ObjectFactory.Get<IJobInvoiceHeaderProcessorCreator>().CreateJobInvoiceHeaderProcessor(plugin1) as JobInvoiceHeaderProcessor;

			using (new DisposableAction(() => processor.ShouldFailJobPreSaveValidation_ForTestOnly = true, () => processor.ShouldFailJobPreSaveValidation_ForTestOnly = false))
			{
				AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>("Simulate Job Header validation error.", "Create Job Invoice Header processing is not successful.", () => processor.Process(Notifications));
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var plugin2 = newFactory.Load<ForwardingShipment>(plugin1.PK);
			var loader2 = new Loader(plugin2);
			var job2 = loader2.TryCreateWithMutex(GlbBranch.CurrentBranch);
			newFactory.Save();

			processor.Process(Notifications);

			AssertNoExceptionThrown("Should NOT have duplicate FK_UC__JH_GC_JH_ParentID index error.", () => Factory.Save());
		}

		#region Implementation

		ForwardingShipment Shipment;
		Job Job;
		NotificationBuffer Notifications;
		JobInvoiceHeaderProcessor JobInvoiceHeaderProcessor;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));
			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);

			JobInvoiceHeaderProcessor = new JobInvoiceHeaderProcessor(Shipment);
			Notifications = new NotificationBuffer();

			var currentStaffMember = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaffMember.GS_EmailAddress = "somebody@somedomain.com";
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
