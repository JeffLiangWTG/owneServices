using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	using System;
	using CargoWise.Types;
	using Enterprise.Environment;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.Data.Mutex;

	class JobInvoiceHeaderCreationErrorEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		public void TestEmailSubjectAndBody()
		{
			using (ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(Shipment.PK))
			{
				mutex.Lock();
				var processor = new JobInvoiceHeaderProcessor(Shipment);
				processor.Process(new NotificationBuffer());
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

		public void TestInvalidArguments()
		{
			AssertNoExceptionThrown(() => new JobInvoiceHeaderCreationErrorEmail(Shipment, new NotificationCollection()));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobInvoiceHeaderCreationErrorEmail(null, new NotificationCollection()));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobInvoiceHeaderCreationErrorEmail(Shipment, null));
		}

		protected override Type EmailDefType
		{
			get
			{
				return typeof(JobInvoiceHeaderCreationErrorEmail);
			}
		}

		ForwardingShipment Shipment;
		protected override void SetUp()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));
			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			var currentStaffMember = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaffMember.GS_EmailAddress = "somebody@somedomain.com";
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}

		TestObjectCreator fTestObjectCreator;
	}
}
