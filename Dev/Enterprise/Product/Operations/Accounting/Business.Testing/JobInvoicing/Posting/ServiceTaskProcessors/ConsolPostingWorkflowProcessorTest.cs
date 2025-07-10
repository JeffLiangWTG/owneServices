using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	abstract class ConsolPostingWorkflowProcessorTest : TestCaseWithFactory
	{
		public void TestReasonNotToAllowPostingIsReported()
		{
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var dummyShipment1 = Factory.New<DummyShipment>();
			job1.JH_ParentID = dummyShipment1.PK;
			job1.JH_ParentTableCode = "JS";
			job1.JH_JobNum = "S00001000";

			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var dummyShipment2 = Factory.New<DummyShipment>();
			job2.JH_ParentID = dummyShipment2.PK;
			job2.JH_ParentTableCode = "JS";
			job2.JH_JobNum = "S00001001";

			Factory.Save();

			consol.Shipments.RemoveAll();
			consol.Shipments.Add(dummyShipment1);
			consol.Shipments.Add(dummyShipment2);

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			int aPInvoiceCountBeforeProcess = CountAPInvoice();

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => consolPostingProcessor.Process(Notifications));
			AssertNotNull("Emails in exception", exceptionThrown.Emails);

			int aPInvoiceCountAfterProcess = CountAPInvoice();
			AssertEquals("Expect No AP invoice is created", aPInvoiceCountBeforeProcess, aPInvoiceCountAfterProcess);

			var expectedReasonsMessage = @"S00001000 - Some Reason Not To Allow Posting
S00001001 - Some Reason Not To Allow Posting";

			AssertEquals(expectedReasonsMessage, Notifications.AsString.Trim());
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = (AccountingEmailDef)exceptionThrown.Emails[0];
			email.Create(Factory);

			AssertContains(expectedReasonsMessage, email.Body.Trim());
		}

		public void TestValidationErrorIsReported()
		{
			job2.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job2.JH_OA_AgentCollectAddr = ZGuid.Empty;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			Notifications.Clear();

			int aPInvoiceCountBeforeProcess = CountAPInvoice();

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => consolPostingProcessor.Process(Notifications));
			AssertNotNull("Emails in exception", exceptionThrown.Emails);

			int aPInvoiceCountAfterProcess = CountAPInvoice();
			AssertEquals("Expect No AP invoice is created in postingFactory", aPInvoiceCountBeforeProcess, aPInvoiceCountAfterProcess);

			var expectedMessage = @"You cannot post because job S00010002 has errors. Please fix errors before posting.
 - Overseas Agent: Please enter Local Client or Overseas Agent.
 - Local Client: Please enter Local Client or Overseas Agent.";

			AssertEquals(expectedMessage.Trim(), Notifications.AsString.Trim());
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = (AccountingEmailDef)exceptionThrown.Emails[0];
			email.Create(Factory);

			AssertContains(expectedMessage, email.Body.Trim());
		}

		public void TestNothingPostedIsReported_WhenAllowZeroValueARInvoices()
		{
			AssertNothingPostedIsReported(true);
		}

		public void TestNothingPostedIsReported_WhenDisallowZeroValueARInvoices()
		{
			AssertNothingPostedIsReported(false);
		}

		void AssertNothingPostedIsReported(bool allowZeroValueARInvoices)
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowZeroValueARInvoices);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			Notifications.Clear();

			int aPInvoiceCountBeforeProcess = CountAPInvoice();

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => consolPostingProcessor.Process(Notifications));
			AssertNotNull("Emails in exception", exceptionThrown.Emails);

			int aPInvoiceCountAfterProcess = CountAPInvoice();
			AssertEquals("Expect No AP invoice is created in postingFactory", aPInvoiceCountBeforeProcess, aPInvoiceCountAfterProcess);

			var expectedZeroValueMessage = allowZeroValueARInvoices
				? string.Empty
				: System.Environment.NewLine + "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices.";

			var expectedMessage = $@"No appropriate charges were found for posting. This may be because:{expectedZeroValueMessage}
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).";

			AssertEquals(expectedMessage.Trim(), Notifications.AsString.Trim());
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = (AccountingEmailDef)exceptionThrown.Emails[0];
			email.Create(Factory);

			AssertContains(expectedMessage, email.Body.Trim());
		}

		public void TestJobOnHoldIsReported_WhenAllowZeroValueARInvoices()
		{
			AssertJobOnHoldIsReported(true);
		}

		public void TestJobOnHoldIsReported_WhenDisallowZeroValueARInvoices()
		{
			AssertJobOnHoldIsReported(false);
		}

		public void AssertJobOnHoldIsReported(bool allowZeroValueARInvoices)
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowZeroValueARInvoices);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			Notifications.Clear();

			consolCost.E6_InvoiceNum = "324343";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			charge1.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge1.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			charge2.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge2.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			job1.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			job2.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			Factory.Save();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			int aPInvoiceCountBeforeProcess = CountAPInvoice();

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => consolPostingProcessor.Process(Notifications));
			AssertNotNull("Emails in exception", exceptionThrown.Emails);

			int aPInvoiceCountAfterProcess = CountAPInvoice();
			AssertEquals("Expect No AP invoice is created in postingFactory", aPInvoiceCountBeforeProcess, aPInvoiceCountAfterProcess);

			var expectedOnHoldMessage = @"The following jobs are on hold and their associated job charges will not be posted. 
Further, any consol costs with apportionment to these jobs will not be posted.

To post these jobs later, change the job status from 'WHL'.
 S00010001
 S00010002";

			var expectedZeroValueMessage = allowZeroValueARInvoices
				? string.Empty
				: System.Environment.NewLine + "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices.";

			var expectedNothingToPostMessage = $@"No appropriate charges were found for posting. This may be because:{expectedZeroValueMessage}
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).";

			AssertEquals(expectedOnHoldMessage + "\r\n\r\n" + expectedNothingToPostMessage, Notifications.AsString.Trim());
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = (AccountingEmailDef)exceptionThrown.Emails[0];
			email.Create(Factory);

			AssertContains(expectedOnHoldMessage, email.Body.Trim());
		}

		public void TestCriticalErrorIsReported()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Notifications.Clear();

			consolCost.E6_InvoiceNum = "324343";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			charge1.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge1.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			charge2.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge2.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			Factory.Save();

			var anotherInvoice = Factory.NewWithValidTestData<APInvoice>();
			anotherInvoice.AH_TransactionNum = "324343";
			anotherInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => consolPostingProcessor.Process(Notifications));
			AssertNotNull("Emails in exception", exceptionThrown.Emails);

			var expectedMessage = @"AP Invoice number 324343 is already used for the organization: AALSHI. Please use another transaction number.";
			AssertEquals(expectedMessage.Trim(), Notifications.AsString.Trim());
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = (AccountingEmailDef)exceptionThrown.Emails[0];
			email.Create(Factory);

			AssertContains(expectedMessage, email.Body.Trim());
		}

		public void TestPostConsolCostCompleteWhenNoErrors()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Notifications.Clear();

			consolCost.E6_InvoiceNum = "324343";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			charge1.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge1.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			charge2.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge2.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			Factory.Save();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			int aPInvoiceCountBeforeProcess = CountAPInvoice();

			AssertNoExceptionThrown(() => consolPostingProcessor.Process(Notifications));

			int aPInvoiceCountAfterProcess = CountAPInvoice();
			AssertEquals("Expect 1 AP invoice is created in postingFactory", aPInvoiceCountBeforeProcess + 1, aPInvoiceCountAfterProcess);

			Assert(Notifications.AsString, !Notifications.HasErrors);
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var apInvoice = GetAPInvoiceFromLocalCache("324343");
			AssertNotNull("AP Invoice is created in postingFactory", apInvoice);
		}

		public void TestPerformTransactionDescriptionDefaulting()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Notifications.Clear();

			var jobInvoiceDescriptionConfig = AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.Value;
			var item = jobInvoiceDescriptionConfig.AddNew();
			item.JobType = "FCN";
			item.DirectionCode = "ALL";
			item.Mode = "ALL";
			item.InvoiceDescription = "Default Description";
			AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobInvoiceDescriptionConfig);

			consolCost.E6_InvoiceNum = "324343";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			charge1.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge1.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			charge2.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge2.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			Factory.Save();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertNoExceptionThrown(() => consolPostingProcessor.Process(Notifications));
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var apInvoice = GetAPInvoiceFromLocalCache("324343");
			AssertNotNull("AP Invoice is created in postingFactory", apInvoice);
			AssertEquals("AH_Desc should be defaulted", "Default Description", apInvoice.AH_Desc);
		}

		int CountAPInvoice()
		{
			var invoiceQuery = new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)) { FetchOnlyFromLocalCache = true };
			var apInvoice = Factory.Load<APInvoice>(invoiceQuery);

			return apInvoice.Length;
		}

		APInvoice GetAPInvoiceFromLocalCache(ZString consolidatedInvoiceRef)
		{
			var invoiceQuery = new ZQuery(
				new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable)
				.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, consolCost.E6_InvoiceNum)
				.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)
				) { FetchOnlyFromLocalCache = true };

			return Factory.LoadTop1<APInvoice>(invoiceQuery);
		}

		#region Implementation

		protected ForwardingConsol consol;
		protected ForwardingShipment shipment1, shipment2;
		protected Job job1, job2;
		protected JobConsolCost consolCost;
		protected Charge charge1, charge2;
		protected NotificationBuffer Notifications;
		protected IProcessor consolPostingProcessor;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));
			consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001001");
			consol.JK_UniqueConsignRef = "C0001000";
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

			shipment1 = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", consol);
			shipment2 = TestObjectCreator.CreateShipment("S00010002", "AUSYD", "NZAKL", consol);
			job1 = TestObjectCreator.CreateJob(shipment1);
			job2 = TestObjectCreator.CreateJob(shipment2);
			job1.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.FRT, "Desc 1", TestObjectCreator.AUD, 50M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS);
			charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.FRT, "Desc 2", TestObjectCreator.AUD, 50M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS);
			consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
			consolCost.E6_OSCostAmount = 100M;

			var currentStaffMember = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaffMember.GS_EmailAddress = "somebody@somedomain.com";
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Factory.Save();

			consolPostingProcessor = CreateConsolPostingProcessor(consol);
			Notifications = new NotificationBuffer();
		}

		protected abstract IProcessor CreateConsolPostingProcessor(ForwardingConsol consol);

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
