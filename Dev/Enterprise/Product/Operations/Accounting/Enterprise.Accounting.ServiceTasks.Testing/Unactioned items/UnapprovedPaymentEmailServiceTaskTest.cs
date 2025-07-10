using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Accounting.Business.EmailNotification.UnapprovedPaymentNotificationEmail;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(UnapprovedPaymentEmailServiceTask))]
	class UnapprovedPaymentEmailServiceTaskTest : ServiceTaskTestCase<UnapprovedPaymentEmailServiceTask>
	{
		public void TestEmailHasCorrectRecipent()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1 = TestObjectCreator.CreateCompanyAndBranch("USLAX");
			company1.CompanyName = "Company Name1";
			var company2 = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			company2.CompanyName = "Company Name2";

			var user1 = TestObjectCreator.CreateStaff("US1");
			user1.GS_EmailAddress = "This@example.com";
			var recipientGroup1 = SetupUserWithNotificationGroup(user1, company1);

			var user2 = TestObjectCreator.CreateStaff("US2");
			user2.GS_EmailAddress = "That@example.com";
			user2.Groups.Add(recipientGroup1);

			var user3 = TestObjectCreator.CreateStaff("AU1");
			user3.GS_EmailAddress = "TT@example.com";
			SetupUserWithNotificationGroup(user3, company2);

			SetUpAuthRegistryForCompany(company1);

			Factory.Save();

			var apApprovalDetails = new List<PaymentApprovalDetails>();

			using (GetCompanyContext(company1))
			{
				apApprovalDetails.Add(CreateAPPaymentApproval().details);
				apApprovalDetails.Add(CreateAPPaymentApproval(ZDateTime.Today.AddDays(2)).details);
				CreateInvaidApprovals();
			}

			Factory.Save();

			var task = new UnapprovedPaymentEmailServiceTask();
			InitialiseAndRunTaskSchedule(task);

			AssertEquals("Pre-requisite", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated.First();
			AssertEmail(email, BuildExpectedBody(null, apApprovalDetails, company1), new[] { user1.GS_EmailAddress, user2.GS_EmailAddress });
			AssertNotContains(user3.GS_EmailAddress, email.Recipients.RecipientsAsDelimitedString("; "));

			using (Env.Instance.TemporaryServiceTaskContext(UnapprovedPaymentEmailServiceTask.Code, canRunInAnyBranch: true))
			{
				task.RunTask();
			}
		}

		public void TestEmailCreationWhenNoAwaitingApprovals()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var user = TestObjectCreator.CreateStaff("US1");
			SetupUserWithNotificationGroup(user);

			SetUpAuthRegistryForCompany(GlbCompany.CurrentCompany);
			CreateInvaidApprovals();
			Factory.Save();

			AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(0, Factory.Load<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.AV_Status, PaymentApprovalStatus.AwaitingApproval)).Length);
			var logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Email should not be created because Notification Group is not set", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString());
		}

		public void TestEmailCreationWhenNoAwaitingApprovalsForCompany()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var companyWithApprovals = TestObjectCreator.CreateCompanyAndBranch("USLAX");
			companyWithApprovals.CompanyName = "With";
			var companyWithoutApprovals = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			companyWithoutApprovals.CompanyName = "Without";

			var user1 = TestObjectCreator.CreateStaff("US1");
			user1.GS_EmailAddress = "This@that.com";
			SetupUserWithNotificationGroup(user1);
			Factory.Save();

			SetUpAuthRegistryForCompany(companyWithApprovals);
			SetUpAuthRegistryForCompany(companyWithoutApprovals);
			Factory.Save();

			using (GetCompanyContext(companyWithApprovals))
			{
				CreateInvaidApprovals();
				CreateAPPaymentApproval();
				CreateARPaymentApproval();
				Factory.Save();
			}
			using (GetCompanyContext(companyWithoutApprovals))
			{
				CreateInvaidApprovals();
			}
			Factory.Save();

			AssertNotEquals("Precondition: both companies have a notification group", Guid.Empty, AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetFallBackValueAtAllLevels(companyWithApprovals.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNotEquals("Precondition: both companies have a notification group", Guid.Empty, AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetFallBackValueAtAllLevels(companyWithoutApprovals.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var awaQuery = new ZQuery(AccPaymentApprovalSchema.AV_Status, PaymentApprovalStatus.AwaitingApproval);
			AssertEquals(0, Factory.Load<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.AV_GB, companyWithoutApprovals.FirstActiveBranch.PK).AddToFilter(awaQuery)).Length);
			AssertEquals(2, Factory.Load<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.AV_GB, companyWithApprovals.FirstActiveBranch.PK).AddToFilter(awaQuery)).Length);

			var logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails are only created for companies with AWA status approvals", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), successfulCompanyNames: new[] { companyWithApprovals.CompanyName }, companiesNotRun: new[] { companyWithoutApprovals.CompanyName });

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (GetCompanyContext(companyWithoutApprovals))
			{
				CreateAPPaymentApproval();
				CreateARPaymentApproval();
				Factory.Save();
			}

			AssertEquals(2, Factory.Load<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.AV_GB, companyWithoutApprovals.FirstActiveBranch.PK).AddToFilter(awaQuery)).Length);
			AssertEquals(2, Factory.Load<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.AV_GB, companyWithApprovals.FirstActiveBranch.PK).AddToFilter(awaQuery)).Length);

			logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails are now created for both companies as both now have AWA status approvals", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), successfulCompanyNames: new[] { companyWithApprovals.CompanyName, companyWithoutApprovals.CompanyName });
		}

		public void TestEmailCreationWhenNotificationGroupNotSet()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var companyWithGroup = TestObjectCreator.CreateCompanyAndBranch("USLAX");
			companyWithGroup.CompanyName = "With";
			var companyWithoutGroup = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			companyWithoutGroup.CompanyName = "Without";

			var user1 = TestObjectCreator.CreateStaff("US1");
			user1.GS_EmailAddress = "This@that.com";
			SetupUserWithNotificationGroup(user1, companyWithGroup);
			Factory.Save();

			SetUpAuthRegistryForCompany(companyWithGroup);
			SetUpAuthRegistryForCompany(companyWithoutGroup);
			Factory.Save();

			using (GetCompanyContext(companyWithGroup))
			{
				CreateAPPaymentApproval();
				CreateARPaymentApproval();
			}
			using (GetCompanyContext(companyWithoutGroup))
			{
				CreateAPPaymentApproval();
				CreateARPaymentApproval();
			}
			Factory.Save();

			AssertEquals("Precondition: companyWithoutGroup has no notification group", Guid.Empty, AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetValueWithoutFallback(companyWithoutGroup.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNotEquals("Precondition: companyWithGroup has a notification group", Guid.Empty, AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetValueWithoutFallback(companyWithGroup.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails only created for companies with a notification group", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), successfulCompanyNames: new[] { companyWithGroup.CompanyName }, companiesNotRun: new[] { companyWithoutGroup.CompanyName });
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var user2 = TestObjectCreator.CreateStaff("US2");
			user2.GS_EmailAddress = "That@this.com";
			SetupUserWithNotificationGroup(user2, companyWithoutGroup);
			Factory.Save();

			AssertNotEquals("Precondition: both companies now have a notification group", Guid.Empty, AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetValueWithoutFallback(companyWithoutGroup.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNotEquals("Precondition: both companies now have a notification group", Guid.Empty, AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetValueWithoutFallback(companyWithGroup.PK.ToGuid(), Guid.Empty, Guid.Empty));

			logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails generated for each company as both have a group", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), successfulCompanyNames: new[] { companyWithGroup.CompanyName, companyWithoutGroup.CompanyName });
		}

		public void TestEmailCreationWhenNotificationEmailCreatedButSentUnsuccessfully()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var companyWithGroup = TestObjectCreator.CreateCompanyAndBranch("USLAX");
			companyWithGroup.CompanyName = "With";

			var user1 = TestObjectCreator.CreateStaff("US1");
			SetupUserWithNotificationGroup(user1, companyWithGroup);
			Factory.Save();

			AssertNullOrEmpty("Pre-condition: no users used should have email addresses", user1.GS_EmailAddress);

			SetUpAuthRegistryForCompany(companyWithGroup);
			Factory.Save();

			using (GetCompanyContext(companyWithGroup))
			{
				CreateAPPaymentApproval();
				CreateARPaymentApproval();
			}
			Factory.Save();

			AssertNotEquals("Precondition: companyWithGroup has a notification group", Guid.Empty, AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetValueWithoutFallback(companyWithGroup.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails only created for companies with a notification group", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), unsuccessfulCompanyNames: new[] { companyWithGroup.GC_Name });
		}

		public void TestEmailContent_AROnly()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company = TestObjectCreator.CreateCompanyAndBranch("USLAX");
			company.CompanyName = "Company Name";

			var user1 = TestObjectCreator.CreateStaff("US1");
			user1.GS_EmailAddress = "This@that.com";
			var recipientGroup = SetupUserWithNotificationGroup(user1, company);

			var user2 = TestObjectCreator.CreateStaff("US2");
			user2.GS_EmailAddress = "That@this.com";
			user2.Groups.Add(recipientGroup);

			SetUpAuthRegistryForCompany(company);

			Factory.Save();

			var arApprovalDetails = new List<PaymentApprovalDetails>();
			using (GetCompanyContext(company))
			{
				arApprovalDetails.Add(CreateARPaymentApproval().details);
				arApprovalDetails.Add(CreateARPaymentApproval(ZDateTime.Today.AddDays(2)).details);
				CreateInvaidApprovals();
			}

			Factory.Save();

			var logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails only created for companies with a notification group", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), successfulCompanyNames: new[] { company.CompanyName });
			AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(), BuildExpectedBody(arApprovalDetails, null, company), new[] { user1.GS_EmailAddress, user2.GS_EmailAddress });
		}

		public void TestEmailContent_APOnly()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company = TestObjectCreator.CreateCompanyAndBranch("USLAX");
			company.CompanyName = "Company Name";

			var user1 = TestObjectCreator.CreateStaff("US1");
			user1.GS_EmailAddress = "This@that.com";
			var recipientGroup = SetupUserWithNotificationGroup(user1, company);

			var user2 = TestObjectCreator.CreateStaff("US2");
			user2.GS_EmailAddress = "That@this.com";
			user2.Groups.Add(recipientGroup);

			SetUpAuthRegistryForCompany(company);

			Factory.Save();

			var apApprovalDetails = new List<PaymentApprovalDetails>();

			using (GetCompanyContext(company))
			{
				apApprovalDetails.Add(CreateAPPaymentApproval().details);
				apApprovalDetails.Add(CreateAPPaymentApproval(ZDateTime.Today.AddDays(2)).details);
				CreateInvaidApprovals();
			}

			Factory.Save();

			var logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails only created for companies with a notification group", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), successfulCompanyNames: new[] { company.CompanyName });
			AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(), BuildExpectedBody(null, apApprovalDetails, company), new[] { user1.GS_EmailAddress, user2.GS_EmailAddress });
		}

		public void TestEmailContent_Both()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company = TestObjectCreator.CreateCompanyAndBranch("USLAX");
			company.CompanyName = "Company Name";

			var user1 = TestObjectCreator.CreateStaff("US1");
			user1.GS_EmailAddress = "This@that.com";
			var recipientGroup = SetupUserWithNotificationGroup(user1, company);

			var user2 = TestObjectCreator.CreateStaff("US2");
			user2.GS_EmailAddress = "That@this.com";
			user2.Groups.Add(recipientGroup);

			SetUpAuthRegistryForCompany(company);

			Factory.Save();

			var arApprovalDetails = new List<PaymentApprovalDetails>();
			var apApprovalDetails = new List<PaymentApprovalDetails>();

			using (GetCompanyContext(company))
			{
				arApprovalDetails.Add(CreateARPaymentApproval().details);
				arApprovalDetails.Add(CreateARPaymentApproval(ZDateTime.Today.AddDays(2)).details);
				apApprovalDetails.Add(CreateAPPaymentApproval().details);
				apApprovalDetails.Add(CreateAPPaymentApproval(ZDateTime.Today.AddDays(2)).details);
				CreateInvaidApprovals();
			}

			Factory.Save();

			var logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails only created for companies with a notification group", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), successfulCompanyNames: new[] { company.CompanyName });
			AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(), BuildExpectedBody(arApprovalDetails, apApprovalDetails, company), new[] { user1.GS_EmailAddress, user2.GS_EmailAddress });
		}

		public void TestEmailContent_ForPaymentBatch()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company = TestObjectCreator.CreateCompanyAndBranch("USLAX");
			company.CompanyName = "Company Name";

			var user1 = TestObjectCreator.CreateStaff("US1");
			user1.GS_EmailAddress = "This@that.com";
			var recipientGroup = SetupUserWithNotificationGroup(user1, company);

			var user2 = TestObjectCreator.CreateStaff("US2");
			user2.GS_EmailAddress = "That@this.com";
			user2.Groups.Add(recipientGroup);

			SetUpAuthRegistryForCompany(company);

			Factory.Save();

			var apApprovalDetails = new List<PaymentApprovalDetails>();
			var arApprovalDetails = new List<PaymentApprovalDetails>();

			using (GetCompanyContext(company))
			{
				var approval1 = CreateARPaymentApproval();
				var approval2 = CreateARPaymentApproval(ZDateTime.Today.AddDays(2));
				approval2.approval.AV_APB_PaymentBatch = Factory.NewWithValidTestData<AccPaymentBatch>().PK;

				var approval3 = CreateAPPaymentApproval();
				var approval4 = CreateAPPaymentApproval(ZDateTime.Today.AddDays(2));
				approval4.approval.AV_APB_PaymentBatch = Factory.NewWithValidTestData<AccPaymentBatch>().PK;
				Factory.Save();

				arApprovalDetails.Add(CreatePaymentApprovalDetails(approval1.approval));
				arApprovalDetails.Add(CreatePaymentApprovalDetails(approval2.approval, ZDateTime.Today.AddDays(2)));
				apApprovalDetails.Add(CreatePaymentApprovalDetails(approval3.approval));
				apApprovalDetails.Add(CreatePaymentApprovalDetails(approval4.approval, ZDateTime.Today.AddDays(2)));

				CreateInvaidApprovals();
			}

			var logger = InitialiseAndRunTaskSchedule(new UnapprovedPaymentEmailServiceTask());
			AssertEquals("Emails only created for companies with a notification group", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertLog(logger.ToString(), successfulCompanyNames: new[] { company.CompanyName });
			AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(), BuildExpectedBody(arApprovalDetails, apApprovalDetails, company), new[] { user1.GS_EmailAddress, user2.GS_EmailAddress });
		}

		public void TestExceptionAndErrorReporterHandling()
		{
			var serviceTask = new UnapprovedPaymentEmailServiceTaskWithException(new NotImplementedException("This is an not implemented exception created for testing"));
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			var log = logger.ToString();

			AssertLog(log, exceptionName: "System.NotImplementedException", exceptionDescription: "This is an not implemented exception created for testing.");
			Assert("The UPA service task ErrorReporter should not include 'UPA Service Task Invalid Behavior Error'", !ErrorReporter.LastMessageReported.Contains("UPA Service Task Invalid Behavior Error"));
			ErrorReporter.Clear();

			serviceTask = new UnapprovedPaymentEmailServiceTaskWithException(new InvalidOperationException("This is an invalid opreation exception created for testing"));
			logger = InitialiseAndRunTaskSchedule(serviceTask);
			log = logger.ToString();

			AssertLog(log, exceptionName: "System.InvalidOperationException", exceptionDescription: "This is an invalid opreation exception created for testing.");
			Assert("The UPA service task ErrorReporter should include 'UPA Service Task Invalid Behavior Error'", ErrorReporter.LastMessageReported.Contains("UPA Service Task Invalid Behavior Error"));
			ErrorReporter.Clear();

			serviceTask = new UnapprovedPaymentEmailServiceTaskWithException(new NullReferenceException("This is an null reference exception created for testing"));
			logger = InitialiseAndRunTaskSchedule(serviceTask);
			log = logger.ToString();

			AssertLog(log, exceptionName: "System.NullReferenceException", exceptionDescription: "This is an null reference exception created for testing.");
			Assert("The UPA service task ErrorReporter should include 'UPA Service Task Invalid Behavior Error'", ErrorReporter.LastMessageReported.Contains("UPA Service Task Invalid Behavior Error"));
			ErrorReporter.Clear();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void AssertLog(string log, IEnumerable<ZString> successfulCompanyNames = null, IEnumerable<ZString> unsuccessfulCompanyNames = null, IEnumerable<ZString> companiesNotRun = null, string exceptionName = null, string exceptionDescription = null)
		{
			var lines = log.SplitByLine();
			AssertEquals("Information|Payment Approvals Notification Email service task started.", lines.First());
			AssertEquals("Information|Payment Approvals Notification Email service task completed.", lines.Last());

			successfulCompanyNames?.ForEach(companyName => AssertNotContains($"Error|Payment Approvals Notification Email was not sent for {companyName}.", log));

			companiesNotRun?.ForEach(companyName => AssertNotContains($"Error|Payment Approvals Notification Email was not sent for {companyName}.", log));

			unsuccessfulCompanyNames?.ForEach(companyName => AssertContains($"Error|Payment Approvals Notification Email was not sent for {companyName}.", log));

			if (exceptionName == null)
			{
				AssertNotContains("Error|Payment Approvals Notification Email service task ended abruptly.", log);
			}
			else
			{
				var expectedMessage = $@"Error|Payment Approvals Notification Email service task ended abruptly.
Exception: {exceptionName}
Exception Message: {exceptionDescription}";

				AssertContains(expectedMessage, log);
			}
		}

		void AssertEmail(EmailDef email, ZString expectedBody, IEnumerable<ZString> expectedRecipients)
		{
			AssertEquals(EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Default@edi.com.au", email.FromAddress);
			AssertEquals("Unactioned Payment Approvals", email.Subject);
			AssertContains(expectedBody, email.Body);
			AssertEquals(expectedRecipients.Count(), email.Recipients.Count);
			expectedRecipients.ForEach(x => AssertContains(x, email.Recipients.RecipientsAsDelimitedString("; ")));
		}

		ZString BuildExpectedBody(List<PaymentApprovalDetails> arApprovals, List<PaymentApprovalDetails> apApprovals, GlbCompany company)
		{
			var messageBuilder = new ZStringBuilder();
			using (GetCompanyContext(company))
			{
				messageBuilder.Append("<br/>");
				messageBuilder.Append("<p>The following Payment Approvals require Authorisation.</p>");
				if (arApprovals?.Any() ?? false)
				{
					messageBuilder.Append("<h3>Payments to Debtor Organizations:</h3>");
					AddTableForApprovals(arApprovals, ControllerIDs.ARPaymentProcessing, messageBuilder);
					messageBuilder.Append($"<p>To manage all requests, please navigate to <a href=\"{ShowModuleUrlHandler.Instance.Create(ModuleIDs.ARPaymentProcessing)}\">Manage > Receivables > Payment Processing</a>.</p>");
					messageBuilder.Append("<br/>");
				}
				if (apApprovals?.Any() ?? false)
				{
					messageBuilder.Append("<h3>Payments to Creditor Organizations:</h3>");
					AddTableForApprovals(apApprovals, ControllerIDs.APPaymentProcessing, messageBuilder);
					messageBuilder.Append($"<p>To manage all requests, please navigate to <a href=\"{ShowModuleUrlHandler.Instance.Create(ModuleIDs.APPaymentProcessing)}\">Manage > Payables > Payment Processing</a>.</p>");
					messageBuilder.Append("<br/>");
				}
			}

			return messageBuilder.ToStringWithNewLineBetweenAppends();
		}

		void AddTableForApprovals(List<PaymentApprovalDetails> approvals, ControllerID controllerID, ZStringBuilder messageBuilder)
		{
			messageBuilder.Append("<table>");
			messageBuilder.Append("<tbody>");
			messageBuilder.Append("<tr>");
			messageBuilder.Append("<th align=\"left\">Approval Request:</th>");
			messageBuilder.Append("<th align=\"left\">Currency:</th>");
			messageBuilder.Append("<th align=\"left\">Amount:</th>");
			messageBuilder.Append("<th align=\"left\">Bank:</th>");
			messageBuilder.Append("<th align=\"left\">Cheque Book Branch:</th>");
			messageBuilder.Append("<th align=\"left\">Payment Batch:</th>");
			messageBuilder.Append("<th align=\"left\">Organization:</th>");
			messageBuilder.Append("<th align=\"left\">Organization Name:</th>");
			messageBuilder.Append("<th align=\"left\">Payment Date:</th>");
			messageBuilder.Append("<th align=\"left\">Created By:</th>");
			messageBuilder.Append("<th align=\"left\">Authorization Level Required:</th>");
			messageBuilder.Append("<th align=\"left\">Previous Actions:</th>");
			messageBuilder.Append("</tr>");
			foreach (var approval in approvals)
			{
				messageBuilder.Append("<tr>");
				var linkForApproval = ShowEditFormUrlHandler.Instance.Create(controllerID, approval.ApprovalPK);
				var linkForPaymentBatch = approval.PaymentBatchPK.IsEmpty ? string.Empty : ShowEditFormUrlHandler.Instance.Create(ControllerIDs.PaymentBatch, approval.PaymentBatchPK);

				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\"><a href=\"{linkForApproval}\">{approval.ApprovalName}</a></td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.CurrencyCode}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.Amount}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.BankCode}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.ChequeBookBranch}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\"><a href=\"{linkForPaymentBatch}\">{approval.PaymentBatch}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.OrgCode}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.OrgName}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.PaymentDate.ToShortDateString()}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.CreateUserCode}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.AuthLevel}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.PrevActions}</td>"));
				messageBuilder.Append("</tr>");
			}
			messageBuilder.Append("</tbody>");
			messageBuilder.Append("</table>");
			messageBuilder.Append("<br/>");
		}

		(APPaymentApprovalWithAuthorisation approval, PaymentApprovalDetails details) CreateAPPaymentApproval(ZDateTime? paymentDate = null) => CreatePaymentApproval<APPaymentApprovalWithAuthorisation>(paymentDate ?? ZDateTime.Today);
		(ARPaymentApprovalWithAuthorisation approval, PaymentApprovalDetails details) CreateARPaymentApproval(ZDateTime? paymentDate = null) => CreatePaymentApproval<ARPaymentApprovalWithAuthorisation>(paymentDate ?? ZDateTime.Today);

		(T approval, PaymentApprovalDetails details) CreatePaymentApproval<T>(ZDateTime paymentDate) where T : PaymentApprovalWithAuthorisation
		{
			var approval = Factory.New<T>();
			approval.AV_OH = TestObjectCreator.AALSHI.PK;
			approval.AV_PaymentComment = $"{typeof(T).ToString()} PAYMENT DESCRIPTION";
			approval.AV_PaymentType = ReceiptTypes.Cheque;
			approval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval.AV_PaymentDate = paymentDate;
			approval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			approval.AV_Amount = 1000M;
			approval.AV_PayExRate = 1;

			var logATH1 = approval.Logs.AddNew(Events.AuthorisationRejected, ZDateTimeOffset.Today.AddDays(-2));
			logATH1.SL_GS_NKUser = "US1";
			var logREJ = approval.Logs.AddNew(Events.Authorised, ZDateTimeOffset.Today);
			logREJ.SL_GS_NKUser = "US2";
			var logATH2 = approval.Logs.AddNew(Events.Authorised, ZDateTimeOffset.Today.AddDays(-4));
			logATH2.SL_GS_NKUser = "US3";

			var expectedDetails = CreatePaymentApprovalDetails(approval, paymentDate);

			return (approval, expectedDetails);
		}

		PaymentApprovalDetails CreatePaymentApprovalDetails(PaymentApprovalWithAuthorisation approval, ZDateTime? paymentDate = null)
		{
			return new PaymentApprovalDetails(
				approval.HumanReadableShortcutName,
				GlbCompany.CurrentCompany.GC_Code,
				approval.PK,
				Core.Constants.CurrencyCodes.UnitedStates,
				"1000.00",
				TestObjectCreator.AUDBankAccount.AB_Code,
				TestObjectCreator.AALSHI.OH_Code,
				TestObjectCreator.AALSHI.OH_FullName,
				paymentDate ?? ZDateTime.Today,
				approval.AuthorisationRequired.AuthorisationRequirementMultilingual,
				TestObjectCreator.AUDChequeBook.Branch.GB_Code,
				approval.PaymentBatch?.PK ?? ZGuid.Empty,
				approval.PaymentBatchNumber,
				GlbStaff.CurrentUser.GS_Code,
				"US3 (ATH), US1 (ATR), US2 (ATH)"
			);
		}

		IDisposable GetCompanyContext(GlbCompany company) => Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		void CreateInvaidApprovals()
		{
			var apApproval1 = CreateAPPaymentApproval().approval;
			apApproval1.AV_Status = PaymentApprovalStatus.Rejected;
			Factory.Save();
			var apApproval2 = CreateAPPaymentApproval().approval;
			apApproval2.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();
			var arApproval = CreateARPaymentApproval().approval;
			arApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
		}

		GlbGroup SetupUserWithNotificationGroup(GlbStaff staff, GlbCompany company = null)
		{
			var userGroup = TestObjectCreator.CreateStaffGroup(company?.GC_Code ?? "G1");
			AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.SetValue(company?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, userGroup.PK.ToGuid());
			staff.Groups.Add(userGroup);
			return userGroup;
		}

		protected void SetUpAuthRegistryForCompany(GlbCompany company)
		{
			var valuesForTest = new PaymentAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 0;
			newSetting.AuthorisationRequirement = AuthorisationCodes.AllThreeApprovalRequired;
			newSetting.Range = RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		class UnapprovedPaymentEmailServiceTaskWithException : UnapprovedPaymentEmailServiceTask
		{
			readonly Exception exception;
			public UnapprovedPaymentEmailServiceTaskWithException(Exception ex)
			{
				exception = ex;
			}
			protected override void RunTaskCore()
			{
				throw exception;
			}
		}
	}
}
