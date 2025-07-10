using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public abstract class JobPostingWorkflowProcessorTest : TestCaseWithFactory
	{
		public void TestJobIsAlwaysLoadedInPluginFactoryButFactoryNeverSavedDuringProcess()
		{
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var shipment = factory.Load<ForwardingShipment>(Shipment.PK);
			var jobQuery = new ZQuery(JobHeaderSchema.PK, Job.PK) { FetchOnlyFromLocalCache = true };
			var job = factory.LoadTop1<Job>(jobQuery);
			AssertNull("Precondition: Job is not yet loaded into Plugin Factory", job);
			var invoicesQuery = new ZQuery(AccTransactionHeaderSchema.AH_JH, Job.PK) { FetchOnlyFromLocalCache = true };
			var invoices = factory.Load<AccTransactionHeader>(invoicesQuery);
			AssertEquals("Precondition: No invoices exist in Plugin Factory", 0, invoices.Length);

			jobPostingProcessor = CreateJobPostingProcessor(shipment);
			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));
			job = factory.LoadTop1<Job>(jobQuery);
			AssertNotNull("Job has been loaded for processing in same Factory as that of the Plugin", job);
			invoices = factory.Load<AccTransactionHeader>(invoicesQuery);
			AssertGreaterThan("Invoices are created in Plugin Factory and linked to the Job", invoices.Length, 0);
			AssertEquals("Factory in which Job has been loaded is not saved after successful processing", 0, factory.SaveCount);
		}

		public void TestExceptionThrownWhenCannotContinuePostingTransactions()
		{
			Job.JH_OA_AgentCollectAddr = ZGuid.Empty;
			Job.JH_OA_LocalChargesAddr = ZGuid.Empty;

			Factory.Save();

			Job.MarkAsNeedingValidation();
			Assert("Job should have errors", Job.HasErrors);
			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var jobPostingWorkflowProcessor = jobPostingProcessor as JobPostingWorkflowProcessor;

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => jobPostingProcessor.Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);

			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();
			AssertNoErrors(Job);
		}

		public void TestCreateOrSendEmail()
		{
			Job.JH_OA_AgentCollectAddr = ZGuid.Empty;
			Job.JH_OA_LocalChargesAddr = ZGuid.Empty;

			Factory.Save();

			Job.MarkAsNeedingValidation();
			Assert("Job should have errors", Job.HasErrors);
			AssertEquals("Should not have email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var jobPostingWorkflowProcessor = jobPostingProcessor as JobPostingWorkflowProcessor;
			var mailQueryText = "SELECT * FROM dbo.MailDBItems";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => jobPostingProcessor.Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			exceptionThrown.Emails[0].Create(Factory);
			AssertEquals("Created email count", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			collection.Load(mailQueryText);
			AssertEquals("Create but not save", 0, collection.Count);
			Factory.Save();

			collection.Load(mailQueryText);
			AssertEquals("Saved", 1, collection.Count);
		}

		public void TestChargePostingWhenJobHasErrors()
		{
			Job.JH_OA_AgentCollectAddr = ZGuid.Empty;
			Job.JH_OA_LocalChargesAddr = ZGuid.Empty;

			Factory.Save();

			Job.MarkAsNeedingValidation();
			Assert("Job should have errors", Job.HasErrors);
			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => jobPostingProcessor.Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);

			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();
			AssertNoErrors(Job);
			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));

			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestChargePostingWhenChargeHasErrors()
		{
			Charge.JR_Desc = ZString.Empty;
			Assert("Charge 1 should have errors", Charge.HasErrors);
			Factory.Save();
			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => jobPostingProcessor.Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);

			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestJobOnHold()
		{
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			Job.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			Factory.Save();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => jobPostingProcessor.Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);

			AssertContains("You cannot post because this job is on hold.\r\nTo post, change the job status from 'WHL'.", Notifications.AsString);

			Job.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));

			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNothingWasPosted()
		{
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var debtor = Factory.Load<OrgHeader>(Charge.JR_OH_SellAccount);
			OrgInvoiceType invoiceType = debtor.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_RS_NKServiceLevel = "STD";

			ToggleChargePostable(false);
			Factory.Save();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => jobPostingProcessor.Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			if (string.IsNullOrWhiteSpace(ExpectedMessageForNothingWasPosted))
			{
				AssertEquals(string.Empty, Notifications.AsString);
			}
			else
			{
				AssertContains(ExpectedMessageForNothingWasPosted, Notifications.AsString);
			}
			ToggleChargePostable(true);
			Factory.Save();

			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));

			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessMultipleProcessorsWithRegistryOff()
		{
			Shipment = TestObjectCreator.CreateShipment("S00010010", "AUSYD", "NZAKL", null);
			Job = TestObjectCreator.CreateJob(Shipment, true, false);
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.LocalClient);
			Charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			Charge.JR_APInvoiceNum = "AP001";
			Charge.JR_APInvoiceDate = ZDateTime.Today;
			Charge.JR_PaymentDate = ZDateTime.Today;

			Factory.Save();

			var revenuePoster = new JobCostPoster(Shipment);
			var costPoster = new JobRevenuePoster(Shipment);

			using (AccountingMasterFilesRegistry.Instance.AllowTriggersToSkipSaveEverythingBeforePostingValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNoExceptionThrown(() => revenuePoster.Process(Notifications));
				Assert(!Factory.HasContext(BusinessContext.PostingChargesFromLogWalker));

				var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => costPoster.Process(Notifications));
				AssertNotNull("Email in exception", exceptionThrown.Emails);
				Assert(!Factory.HasContext(BusinessContext.PostingChargesFromLogWalker));
			}
		}

		public void TestProcessMultipleProcessorsWithRegistryOn()
		{
			Shipment = TestObjectCreator.CreateShipment("S00010010", "AUSYD", "NZAKL", null);
			Job = TestObjectCreator.CreateJob(Shipment, true, false);
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.LocalClient);
			Charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			Charge.JR_APInvoiceNum = "AP001";
			Charge.JR_APInvoiceDate = ZDateTime.Today;
			Charge.JR_PaymentDate = ZDateTime.Today;

			Factory.Save();

			var revenuePoster = new JobCostPoster(Shipment);
			var costPoster = new JobRevenuePoster(Shipment);

			using (AccountingMasterFilesRegistry.Instance.AllowTriggersToSkipSaveEverythingBeforePostingValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNoExceptionThrown(() => revenuePoster.Process(Notifications));
				Assert(!Factory.HasContext(BusinessContext.PostingChargesFromLogWalker));

				AssertNoExceptionThrown(() => costPoster.Process(Notifications));
				Assert(!Factory.HasContext(BusinessContext.PostingChargesFromLogWalker));
			}
		}

		protected abstract string ExpectedMessageForNothingWasPosted { get; }

		public void TestReasonNotToAllowPosting()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Factory.Save();
			AssertEquals("Pre condition", string.Empty, job.ReasonNotToAllowPosting);
			var dummyShipment = Factory.New<DummyShipment>();
			job.JH_ParentID = dummyShipment.PK;
			Factory.Save();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var poster = CreateJobPostingProcessor(dummyShipment);
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => poster.Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Some Reason Not To Allow Posting", Notifications.AsString.Trim());

			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNotifyUserWarningMessage()
		{
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var debtor = Factory.Load<OrgHeader>(Charge.JR_OH_SellAccount);
			OrgInvoiceType invoiceType = debtor.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_RS_NKServiceLevel = "STD";

			ToggleChargePostable(true);
			Factory.Save();
			((JobPostingWorkflowProcessor)jobPostingProcessor).UseDummyInvoicingPostManager_ForTestOnly = true;
			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));
			AssertContains("this is only for test warning message!", Notifications.AsString);
		}

		protected virtual void ToggleChargePostable(bool shouldBePostable)
		{
			if (shouldBePostable)
			{
				Charge.JR_InvoiceType = "FIN";
			}
			else
			{
				Charge.JR_InvoiceType = "FID";
			}
		}

		void SetPostingNotificationGroup()
		{
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK)));
			var currentStaffMember = Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK));
			currentStaffMember.GS_EmailAddress = "blahblah@whatever.example";
			Factory.Save();
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		#region Data for Test Charge Posting When Organisation is deactivated

		protected void SetUpDataForTestChargePostingWhenOrgIsDeactivated()
		{
			Debtor = TestObjectCreator.CreateOrgHeader("TESTORG", false, true, "AUSYD");
			var companyAU = TestObjectCreator.CreateNewCompany("AU2", "AU");
			companyAU.GC_OH_OrgProxy = Debtor.PK;

			OrganisationActivator.ActivateOrDeactivate(new[] { Debtor.PK }, false);

			ShipmentWhenOrgIsDeactivated = TestObjectCreator.CreateShipment("S00010002", "AUSYD", "NZAKL", null);
			var job = TestObjectCreator.CreateJob(ShipmentWhenOrgIsDeactivated, true, false);
			job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, 100M, Debtor);

			var currentStaffMember = TestObjectCreator.CreateStaff("ABC");
			currentStaffMember.GS_EmailAddress = "testmail@wisetechglobal.com";
			var group = TestObjectCreator.CreateStaffGroup("STF");
			group.Staff.Add(currentStaffMember);
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var shipmentWhenOrgIsDeactivated = factory.Load<ForwardingShipment>(ShipmentWhenOrgIsDeactivated.PK);
			JobPostingProcessorWhenOrgIsDeactivated = CreateJobPostingProcessor(shipmentWhenOrgIsDeactivated);
			NotificationsWhenOrgIsDeactivated = new NotificationBuffer();
		}

		protected NotificationBuffer NotificationsWhenOrgIsDeactivated;
		protected ForwardingShipment ShipmentWhenOrgIsDeactivated;
		protected IProcessor JobPostingProcessorWhenOrgIsDeactivated;
		protected OrgHeader Debtor;

		#endregion

		#region Implementation

		protected ForwardingShipment Shipment;
		protected Job Job;
		protected Charge Charge;

		protected NotificationBuffer Notifications;
		protected IProcessor jobPostingProcessor;

		protected override void SetUp()
		{
			base.SetUp();

			jobPostingProcessor = CreateJobPostingProcessor(Shipment);
			Notifications = new NotificationBuffer();
			SetPostingNotificationGroup();
		}

		protected abstract IProcessor CreateJobPostingProcessor(IJobInvoicingPlugIn plugin);

		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		#endregion

		class DummyShipment : ForwardingShipment
		{
			public DummyShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
			{
				return new DummyJobInvoicingSupporter(this);
			}
		}

		class DummyJobInvoicingSupporter : CommonShipmentInvoicingSupporter
		{
			public DummyJobInvoicingSupporter(CommonShipment parent)
				: base(parent)
			{
			}

			public override string GetReasonNotToAllowPosting()
			{
				return "Some Reason Not To Allow Posting";
			}
		}
	}
}
