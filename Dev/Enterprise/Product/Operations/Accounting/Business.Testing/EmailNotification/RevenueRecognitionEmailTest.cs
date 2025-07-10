using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class RevenueRecognitionEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(RevenueRecognitionEmail);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestArgumentExceptionWhenNullPassed1()
		{
			new RevenueRecognitionEmail(null, "");
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestArgumentExceptionWhenNullPassed2()
		{
			new RevenueRecognitionEmail(Job, null);
		}

		public void TestSubject()
		{
			AssertEquals("Subject of email", "Revenue recognition errors for Job JOB1", Email.GetSubject_ForTestOnly());
		}

		public void TestBody()
		{
			Job.RunPreSaveValidation();
			AssertEquals("Precondition: Job should has errors.", true, Job.HasErrors);
			ZString expectedBody = string.Format(@"<html>
<body>
<p>Revenue recognition was run as workflow action for the job. There were errors during the operation. It might be done successfully for valid dates and skipped for invalid dates.</p>
<p>Errors:</p>
<ul><li>Error - JH_GE: Cannot issue job charges for a miscellaneous department.</li><li>Error - JH_OA_AgentCollectAddr: Please enter Local Client or Overseas Agent.</li><li>Error - JH_OA_LocalChargesAddr: Please enter Local Client or Overseas Agent.</li><li>More errors here</li></ul>
<p>Job revenue recognition dates after this operation: IMM.</p>
<p>Revenue recognition can be done manually from Job Invoicing menu. <a href=""{0}"">JOB1</a></p>
</body>
</html>", ShowEditFormUrlHandler.Instance.Create(ControllerIDs.JobShipment, Shipment.PK));
			AssertEquals("Body of email", expectedBody, Email.GetBody_ForTestOnly());
		}

		public override void TestSend()
		{
			Guid groupPk = StaffGroupPK.ToGuid();
			Factory.Save();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			Email.Send();
			AssertEquals("Shouldn't have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AccountingConfigurationRegistry.Instance.RevenueRecognitionNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPk); // to check empty handling
			Email.Send();
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			SetupStaffMemberEmailAddress();
			AdditionalErrors = "More errors here";
			Shipment = TestObjectCreator.CreateShipment("JOB1");
			Job = TestObjectCreator.CreateJob(Shipment);
			TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 10M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10M, TestObjectCreator.LocalClient);
			Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			Factory.Save();
			Email = new RevenueRecognitionEmail(Job, AdditionalErrors);
		}

		protected override void TearDown()
		{
			base.TearDown();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Job.Dispose();
		}

		TestObjectCreator TestObjectCreator;
		RevenueRecognitionEmail Email;
		Job Job;
		CommonShipment Shipment;
		string AdditionalErrors;
		protected ZGuid StaffGroupPK
		{
			get
			{
				GlbGroup group = Factory.New<GlbGroup>();
				group.Staff.Add(Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK)));
				return group.PK;
			}
		}

		protected ZString EmailAddress
		{
			get
			{
				return new ZString("blahblah@whatever.example");
			}
		}

		protected void SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK));
			currentStaffMember.GS_EmailAddress = EmailAddress;
			staffMemberFactory.Save();
		}
	}
}
