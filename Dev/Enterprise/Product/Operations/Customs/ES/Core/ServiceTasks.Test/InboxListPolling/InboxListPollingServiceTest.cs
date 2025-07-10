using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Customs.ES.Business.ESConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.ES.ServiceTasks.Testing
{
	[TestedType(typeof(InboxListPollingService))]
	sealed class InboxListPollingServiceTest : ServiceTaskTestCase<InboxListPollingService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ESI", hostedServiceAttribute.Code);
				AssertEquals("Description", "ES Customs Inbox List Polling Service", hostedServiceAttribute.Description);
				AssertEquals("Category", "ESC", hostedServiceAttribute.Category);
				AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Spain, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("MinimumPeriod", "20minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("DefaultScheduleRunEvery", "30minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
			});
		}

		public void TestInitialiseSchedule()
		{
			var serviceTask = new InboxListPollingService();
			InitialiseTaskSchedule(serviceTask, out StmServiceTask taskSchedule);

			CombineAssertions(() =>
			{
				AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
				Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
				AssertEquals("TaskPeriodCount", 30, taskSchedule.Recurrence.Period);
				AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
				AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
			});
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(InboxListPollingService).GetMethod(nameof(InboxListPollingService.CheckInboxxTIsActive));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestCheckInboxxTIsActive()
		{
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
				{
					AssertEquals("CheckCompanyInSpain, when xT active", string.Empty, InboxListPollingService.CheckInboxxTIsActive());
				}
				using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
				{
					AssertEquals("CheckCompanyInSpain, when xT not active", "This service requires InboxMessagesThroughDirectxT to be enabled in Registry: Customs -> Country or Region Specific -> Spain -> Enable ES Inbox Messages Through Direct xT Interface", InboxListPollingService.CheckInboxxTIsActive());
				}
			});
		}

		public void TestNoActiveBranch()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "VKO";
			company.GC_Name = "COMPANY TEST";
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Spain;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "TST";
			branch.GB_IsActive = false;

			Factory.Save();

			DeactivateSpanishBranches();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				CombineAssertions(() =>
				{
					AssertEquals("[PRE-CONDITION] Does Current Company have active branches?", false, Env.CurrentCompany.ActiveBranches.Any());
					var serviceTask = new InboxListPollingService();
					AssertNoExceptionThrown("Running the service task should not throw an exception even if there are no active branches", () => InitialiseAndRunTaskSchedule(serviceTask));
				});
			}
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestEsActiveBranch()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "COM";
			company.GC_Name = "COMPANY TEST";
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Italy;
			var noActiveBranch = company.Branches.AddNew();
			noActiveBranch.FillWithValidTestData();
			noActiveBranch.GB_Code = "STS";
			noActiveBranch.GB_IsActive = false;

			var spanishCompany = Factory.New<GlbCompany>();
			spanishCompany.GC_Code = "VKO";
			spanishCompany.GC_Name = "ES COMPANY TEST";
			spanishCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Spain;
			var spanishActiveBranch = spanishCompany.Branches.AddNew();
			spanishActiveBranch.FillWithValidTestData();
			spanishActiveBranch.GB_Code = "TST";
			spanishActiveBranch.GB_RL_NKHomePort = Enterprise.Core.Constants.CountryCodes.Spain;

			Factory.Save();

			DeactivateSpanishBranches();
			spanishActiveBranch.GB_IsActive = true;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, noActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				CombineAssertions(() =>
				{
					AssertEquals("[PRE-CONDITION] Does Current Company have active branches?", false, Env.CurrentCompany.ActiveBranches.Any());
					AssertEquals("[PRE-CONDITION] An ES company with active branches exist", true, GlbBranch.FindAnyBranchInSameCountry(Factory, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Spain)).GB_IsActive);

					var expiredTransaction = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-19), "AAA");
					Factory.Save();

					AssertEquals("Before running the service task there is 1 Polling Transactions", 1, GetCusPollingTransactions().Length);

					var serviceTask = new InboxListPollingService();
					var logs = InitialiseAndRunTaskSchedule(serviceTask);

					AssertEquals("After running the service task there are no Polling Transactions, the OPN & expired ones has been deleted", 0, GetCusPollingTransactions().Length);

					AssertEquals("Log Count", 3, logs.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Removed 1 Polling Transaction");
					expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: AAA, System Create Time (universal time): 21-Dec-19 15:13");
					expectedLogs.AppendLine("Information|Created 0 EDI Messages");

					AssertEquals("Deleted transactions have been recorder in the log", expectedLogs.ToString(), logs.ToString());
				});
			}
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestRunTask_RemovedExpired()
		{
			var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA");
			var correctTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB");
			var correctTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "CCC");

			var expiredTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-19), "DDD");
			var expiredTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-8), "EEE");
			var expiredTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-10), "FFF");

			var incorrectApplicationCodeTransaction = AddCusPollingTransaction("TRC", "OPN", ZDateTime.Now.AddDays(-10), "GGG", type: "T2O");
			var incorrectStatusTransaction = AddCusPollingTransaction("ESC", "CLS", ZDateTime.Now.AddDays(-10), "HHH");

			Factory.Save();

			ErrorReporter.Clear();

			CombineAssertions(() =>
			{
				AssertEquals("Before running the service task there are 7 ESC Polling Transactions", 7, GetCusPollingTransactions().Length);
				AssertEquals("Before running the service task there is 1 TRC Polling Transactions", 1, GetCusPollingTransactions("TRC").Length);

				var serviceTask = new InboxListPollingService();
				var logs = InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals("After running the service task there are only 4 ESC Polling Transactions, the OPN & expired ones have been deleted", 4, GetCusPollingTransactions().Length);
				AssertEquals("After running the service task there is still 1 TRC Polling Transactions", 1, GetCusPollingTransactions("TRC").Length);

				AssertTransactionStatus("correctTransaction1's status was not changed because it is not expired", "OPN", correctTransaction1);
				AssertTransactionStatus("correctTransaction2's status was not changed because it is not expired", "OPN", correctTransaction2);
				AssertTransactionStatus("correctTransaction3's status was not changed because it is not expired", "OPN", correctTransaction3);
				AssertTransactionStatus("incorrectApplicationCodeTransaction's status was not changed because it is expired but the ApplicationCode is not correct", "OPN", incorrectApplicationCodeTransaction);
				AssertTransactionStatus("incorrectStatusTransaction's status was not changed because it is already CLS", "CLS", incorrectStatusTransaction);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				AssertEquals("Log Count", 5, logs.Count);

				var expectedLogs = new StringBuilder();
				expectedLogs.AppendLine("Information|Removed 3 Polling Transactions");
				expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: DDD, System Create Time (universal time): 21-Dec-19 15:13");
				expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: FFF, System Create Time (universal time): 30-Dec-19 15:13");
				expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: EEE, System Create Time (universal time): 01-Jan-20 15:13");
				expectedLogs.AppendLine("Information|Created 0 EDI Messages");

				AssertEquals("Deleted transactions have been recorder in the log", expectedLogs.ToString(), logs.ToString());
			});
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestRunTask_RemovedExpired_SetExpirationDays()
		{
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-7), "BBB");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "CCC");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-19), "DDD");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-15), "EEE");
			AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-10), "FFF");

			Factory.Save();

			ErrorReporter.Clear();

			using (RegistryTemporarySetterHelper.SetInboxXTExpirationPeriodDays(10))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Before running the service task there are 6 ESC Polling Transactions", 6, GetCusPollingTransactions().Length);

					var serviceTask = new InboxListPollingService();
					var logs = InitialiseAndRunTaskSchedule(serviceTask);

					AssertEquals("After running the service task there are only 4 ESC Polling Transactions, the OPN & expired ones have been deleted", 4, GetCusPollingTransactions().Length);

					AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
					AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();

					AssertEquals("Log Count", 4, logs.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Removed 2 Polling Transactions");
					expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: DDD, System Create Time (universal time): 21-Dec-19 15:13");
					expectedLogs.AppendLine("Information|Polling Transaction removed with Type: RES, Transaction ID: EEE, System Create Time (universal time): 25-Dec-19 15:13");
					expectedLogs.AppendLine("Information|Created 0 EDI Messages");

					AssertEquals("Deleted transactions have been recorder in the log", expectedLogs.ToString(), logs.ToString());
				});
			}
		}

		public void TestRunTask_CreateListPollingEDIMessage_NotxT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				var staffCode = "AZ";
				var staff = SetUpStaffWithCertificate(staffCode, "CERTNAME");
				var declarant = SetUpDeclarant();
				var declaration = SetUpDeclarationWithDeclarant(staffCode, declarant);
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

				var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader);

				Factory.Save();

				CombineAssertions(() =>
				{
					var serviceTask = new InboxListPollingService();
					var logs = InitialiseAndRunTaskSchedule(serviceTask);

					AssertTransactionStatus("correctTransaction1's status was not changed because registry item (sent inbox through xt) is false", "OPN", correctTransaction1);

					ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
					messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					EDIMessage[] newRequestMessages = Factory.Load<EDIMessage>(messagesQuery);

					AssertEquals("No messages created", false, newRequestMessages.Any());

					AssertEquals("Log Count", 2, logs.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Removed 0 Polling Transactions");
					expectedLogs.AppendLine("Information|Created 0 EDI Messages");

					AssertEquals("Functionality has been recorded in the log", expectedLogs.ToString(), logs.ToString());
				});
			}
		}

		public void TestRunTask_CreateListPollingEDIMessage_xT_SingleTypeCertificate()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var staffCode = "AZ";
				var certName = "CERTNAME";
				var staff = SetUpStaffWithCertificate(staffCode, certName);
				var declarant = SetUpDeclarant();
				var declaration = SetUpDeclarationWithDeclarant(staffCode, declarant);
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

				var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader);
				var correctTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader);
				var correctTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "CCC", bo: entryHeader);

				var incorrectApplicationCodeTransaction = AddCusPollingTransaction("TRC", "OPN", ZDateTime.Now.AddDays(-10), "DDD", type: "T2O", bo: entryHeader);
				var incorrectStatusTransaction = AddCusPollingTransaction("ESC", "CLS", ZDateTime.Now.AddDays(-10), "EEE", bo: entryHeader);

				Factory.Save();

				CombineAssertions(() =>
				{
					var serviceTask = new InboxListPollingService();
					var logs = InitialiseAndRunTaskSchedule(serviceTask);

					AssertTransactionStatus("correctTransaction1's status was changed to PND", "PND", correctTransaction1);
					AssertTransactionStatus("correctTransaction2's status was changed to PND", "PND", correctTransaction2);
					AssertTransactionStatus("correctTransaction3's status was changed to PND", "PND", correctTransaction3);
					AssertTransactionStatus("incorrectApplicationCodeTransaction's status was not changed because ApplicationCode is not correct", "OPN", incorrectApplicationCodeTransaction);
					AssertTransactionStatus("incorrectStatusTransaction's status was not changed because it is already CLS", "CLS", incorrectStatusTransaction);

					AssertNewInboxMessagesPerCertificateAndIsTest("For certName and not test", new ZString[] { InboxNotificationResponseTypes.AESExitResult }, certName);

					AssertEquals("Log Count", 3, logs.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Removed 0 Polling Transactions");
					expectedLogs.AppendLine("Information|Created 1 EDI Message");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: RES and Certificate: CERTNAME");

					AssertEquals("Functionality has been recorded in the log", expectedLogs.ToString(), logs.ToString());
				});
			}
		}

		public void TestRunTask_CreateListPollingEDIMessage_xT_MultipleTypeCertificate()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var staffCode1 = "AZ1";
				var certName1 = "CERTNAME1";
				var staff1 = SetUpStaffWithCertificate(staffCode1, certName1);
				var declarant = SetUpDeclarant();

				var declaration1 = SetUpDeclarationWithDeclarant(staffCode1, declarant);

				var entryHeader1 = (CusEntryHeader)declaration1.ActiveEntryHeaders.AddNew();
				var entryHeader2 = (CusEntryHeader)declaration1.ActiveEntryHeaders.AddNew();

				var correctTransaction1 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader1);
				var correctTransaction2 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader1);
				var correctTransaction3 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "CCC", type: DeclarationMessageTypeList.Codes.InBoxNotificationForExport, bo: entryHeader1);
				var correctTransaction4 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "DDD", type: DeclarationMessageTypeList.Codes.InBoxNotificationForExport, bo: entryHeader1);
				var correctTransaction5 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-3), "EEE", bo: entryHeader2);
				var correctTransaction6 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "FFF", type: DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, bo: entryHeader2);

				var incorrectApplicationCodeTransaction = AddCusPollingTransaction("TRC", "OPN", ZDateTime.Now.AddDays(-10), "GGG", type: "T2O", bo: entryHeader1);
				var incorrectStatusTransaction = AddCusPollingTransaction("ESC", "CLS", ZDateTime.Now.AddDays(-10), "HHH", bo: entryHeader1);

				var staffCode2 = "AZ2";
				var certName2 = "CERTNAME2";
				var staff2 = SetUpStaffWithCertificate(staffCode2, certName2);
				var declaration2 = SetUpDeclarationWithDeclarant(staffCode2, declarant);

				var entryHeader3 = (CusEntryHeader)declaration2.ActiveEntryHeaders.AddNew();

				var correctTransaction7 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader3);
				var correctTransaction8 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader3);

				var declaration3 = SetUpDeclarationWithDeclarant(staffCode1, declarant, true);
				var entryHeader4 = (CusEntryHeader)declaration3.ActiveEntryHeaders.AddNew();

				var correctTransaction9 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: entryHeader4);
				var correctTransaction10 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", bo: entryHeader4);

				var report = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(staffCode1, certName1, declarant.MainAddress.PK));

				var correctTransaction11 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now, "AAA", bo: report);
				var correctTransaction12 = AddCusPollingTransaction("ESC", "OPN", ZDateTime.Now.AddDays(-5), "BBB", type: DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, bo: report);

				Factory.Save();

				CombineAssertions(() =>
				{
					var serviceTask = new InboxListPollingService();
					var logs = InitialiseAndRunTaskSchedule(serviceTask);

					AssertTransactionStatus("correctTransaction1's status was changed to PND", "PND", correctTransaction1);
					AssertTransactionStatus("correctTransaction2's status was changed to PND", "PND", correctTransaction2);
					AssertTransactionStatus("correctTransaction3's status was changed to PND", "PND", correctTransaction3);
					AssertTransactionStatus("correctTransaction4's status was changed to PND", "PND", correctTransaction4);
					AssertTransactionStatus("correctTransaction5's status was changed to PND", "PND", correctTransaction5);
					AssertTransactionStatus("correctTransaction6's status was changed to PND", "PND", correctTransaction6);
					AssertTransactionStatus("incorrectApplicationCodeTransaction's status was not changed because ApplicationCode is not correct", "OPN", incorrectApplicationCodeTransaction);
					AssertTransactionStatus("incorrectStatusTransaction's status was not changed because it is already CLS", "CLS", incorrectStatusTransaction);
					AssertTransactionStatus("correctTransaction7's status was changed to PND", "PND", correctTransaction7);
					AssertTransactionStatus("correctTransaction8's status was changed to PND", "PND", correctTransaction8);
					AssertTransactionStatus("correctTransaction9's status was changed to PND", "PND", correctTransaction9);
					AssertTransactionStatus("correctTransaction10's status was changed to PND", "PND", correctTransaction10);
					AssertTransactionStatus("correctTransaction11's status was changed to PND", "PND", correctTransaction11);
					AssertTransactionStatus("correctTransaction12's status was changed to PND", "PND", correctTransaction12);

					var expectedUrlsCert1 = new ZString[] { InboxNotificationResponseTypes.AESExitResult,
															InboxNotificationResponseTypes.Export,
															InboxNotificationResponseTypes.DVD,
															InboxNotificationResponseTypes.AESExitClearance };
					AssertNewInboxMessagesPerCertificateAndIsTest("For certName1 and not test", expectedUrlsCert1, certName1);

					AssertNewInboxMessagesPerCertificateAndIsTest("For certName2 and not test", new ZString[] { InboxNotificationResponseTypes.AESExitResult }, certName2);

					AssertNewInboxMessagesPerCertificateAndIsTest("For certName1 and test", new ZString[] { InboxNotificationResponseTypes.AESExitResult }, certName1, true);

					AssertEquals("Log Count", 8, logs.Count);

					var expectedLogs = new StringBuilder();
					expectedLogs.AppendLine("Information|Removed 0 Polling Transactions");
					expectedLogs.AppendLine("Information|Created 6 EDI Messages");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: RES and Certificate: CERTNAME1");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: NPE and Certificate: CERTNAME1");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: NPD and Certificate: CERTNAME1");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: RES and Certificate: CERTNAME2");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: Y for Polling Transaction.Type: RES and Certificate: CERTNAME1");
					expectedLogs.AppendLine("Information|EDI message created with Message Type: LDI, Receive Transmit: TRX, Is Test Message: N for Polling Transaction.Type: LVS and Certificate: CERTNAME1");

					AssertEquals("Functionality has been recorded in the log", expectedLogs.ToString(), logs.ToString());
				});
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void AssertTransactionStatus(ZString assertMessage, ZString expectedStatus, CusPollingTransaction transaction)
		{
			transaction.Reload();
			AssertEquals(assertMessage, expectedStatus, transaction.CPT_Status);
		}

		void AssertNewInboxMessagesPerCertificateAndIsTest(ZString assertMessage, ZString[] urls, ZString certName, bool isTest = false)
		{
			ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ApplicationReference, certName);
			messagesQuery.AddToFilter(EDIMessageSchema.EM_IsTestMessage, isTest);
			EDIMessage[] newRequestMessages = NewFactory().Load<EDIMessage>(messagesQuery);

			AssertEquals(assertMessage + " messages count is correct", urls.Length, newRequestMessages.Length);

			var newRequestMessagesText = new List<ZString>();
			foreach (EDIMessage message in newRequestMessages)
			{
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_MessageType", DeclarationMessageTypeList.Codes.InboxPendingList, message.EM_MessageType);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, message.EM_MessageSubType);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_IsTestMessage", isTest, message.EM_IsTestMessage);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals(assertMessage + " " + message.EM_MessageType + " message.EM_ApplicationReference", certName, message.EM_ApplicationReference);
				AssertNull(assertMessage + " " + message.EM_MessageType + " message doesn't have interchange", message.Interchange);

				var url = urls.FirstOrDefault(x => message.EM_MessageText.Contains(x));
				AssertMultilineASCIIEquals(assertMessage + " " + message.EM_MessageType + " message.EM_MessageText", GetExpectedNewInboxMessageBodyText(url), message.EM_MessageText);
			}
		}

#if NET48
		ZString GetExpectedNewInboxMessageBodyText(ZString url) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<ListaDecV4Ent tipoRespuesta=""{0}"" xmlns=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <declarante>
    <NifDeclarante>{1}</NifDeclarante>
    <NombreDeclarante>{2}</NombreDeclarante>
  </declarante>
</ListaDecV4Ent>
  </soapenv:Body>
</soapenv:Envelope>", url, DeclarantId, DeclarantName);
#else
		ZString GetExpectedNewInboxMessageBodyText(ZString url) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<ListaDecV4Ent tipoRespuesta=""{0}"" xmlns=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <declarante>
    <NifDeclarante>{1}</NifDeclarante>
    <NombreDeclarante>{2}</NombreDeclarante>
  </declarante>
</ListaDecV4Ent>
  </soapenv:Body>
</soapenv:Envelope>", url, DeclarantId, DeclarantName);
#endif

		JobDeclaration SetUpDeclarationWithDeclarant(ZString staffCode, OrgHeader declarant, bool isTest = false)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			declaration.JE_GS_NKCusAgent = staffCode;
			declaration.ZG_IsTrainingDeclaration = isTest;

			declaration.Declarant.OA_OH = declarant.PK;
			return declaration;
		}

		GlbStaff SetUpStaffWithCertificate(ZString staffCode, ZString certName)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = staffCode + "test";
			var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = certName;
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			return staff;
		}

		OrgHeader SetUpDeclarant()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = "MYADDRESS";
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = declarant.PK;
			declarant.OH_FullName = DeclarantName;
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
			Factory.Save();
			return declarant;
		}

		const string DeclarantId = "NIF22222222";
		const string DeclarantName = "Declarant Full Name";

		CusPollingTransaction AddCusPollingTransaction(ZString applicationCode, ZString status, ZDateTime createTime, ZString transactionID, string type = DeclarationMessageTypeList.Codes.ExportExitResultCommunication, BusinessObject bo = null)
		{
			var transaction = Factory.NewWithValidTestData<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = applicationCode;
			transaction.CPT_Status = status;
			transaction.CPT_SystemCreateTimeUtc = createTime;
			transaction.CPT_TransactionID = transactionID;
			transaction.CPT_Type = type;
			transaction.CPT_NumberOfAttempts = 1;
			if (bo != null)
			{
				transaction.CPT_ParentID = bo.PK;
				transaction.CPT_ParentTableCode = bo.TablePrefix;
			}
			return transaction;
		}

		CusPollingTransaction[] GetCusPollingTransactions(string code = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage)
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, code);
			return NewFactory().Load<CusPollingTransaction>(query);
		}

		void DeactivateSpanishBranches()
		{
			var anotherFactory = new BusinessObjectFactory();
			var esBranchQuery = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.Spain);
			var esBranches = anotherFactory.Load<GlbBranch>(esBranchQuery);
			esBranches.ForEach(x => x.GB_IsActive = false);
			anotherFactory.Save();
		}

		ZGuid CreateCusExitReport(string brokerCode, string certName, ZGuid carrierPK)
		{
			var pkHeader = Guid.NewGuid();
			var sqlHeader = @"
INSERT INTO dbo.CusExitHeader (CXH_PK, CXH_ApplicationCode, CXH_AutoVersion, CXH_ClusterKey, CXH_IsValid, CXH_JobReference, CXH_SystemCreateTimeUtc, CXH_SystemCreateUser, CXH_SystemLastEditTimeUtc, CXH_SystemLastEditUser, CXH_GB_Branch, CXH_GC_Company, CXH_GS_NKCustomsAgent, CXH_CustomsProfile, CXH_OA_Carrier)
VALUES
(@CXH_PK, 'XIT', 1, 1, 1, 'E00000001', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', @CXH_GB_Branch, @CXH_GC_Company, @CXH_GS_NKCustomsAgent, @CXH_CustomsProfile, @CXH_OA_Carrier)
";
			using (DbCommand command = Db.Connection.Command(sqlHeader))
			{
				command.AddParameter("@CXH_PK", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@CXH_GB_Branch", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				command.AddParameter("@CXH_GC_Company", SqlDbType.UniqueIdentifier, Env.CurrentCompanyPK);
				command.AddParameter("@CXH_GS_NKCustomsAgent", SqlDbType.VarChar, brokerCode);
				command.AddParameter("@CXH_CustomsProfile", SqlDbType.VarChar, certName);
				command.AddParameter("@CXH_OA_Carrier", SqlDbType.UniqueIdentifier, carrierPK.ToGuid());
				command.ExecuteNonQuery();
			}

			var pkConsignment = Guid.NewGuid();
			var sqlConsginment = @"
INSERT INTO dbo.CusExitConsignment (CXC_PK, CXC_AutoVersion, CXC_ClusterKey, CXC_IsValid, CXC_CXH_Header, CXC_MovementReference, CXC_SystemCreateTimeUtc, CXC_SystemCreateUser, CXC_SystemLastEditTimeUtc, CXC_SystemLastEditUser)
	VALUES (@CXC_PK, 1, 1, 1, @CXC_CXH_Header, 'AAAAA', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sqlConsginment))
			{
				command.AddParameter("@CXC_PK", SqlDbType.UniqueIdentifier, pkConsignment);
				command.AddParameter("@CXC_CXH_Header", SqlDbType.UniqueIdentifier, pkHeader);
				command.ExecuteNonQuery();
			}

			var pkReport = Guid.NewGuid();
			var sqlReport = @"
INSERT INTO dbo.CusExitReport (CER_PK, CER_ClusterKey, CER_IsValid, CER_CXH_Header, CER_Type, CER_OfficeOfExit, CER_SystemCreateTimeUtc, CER_SystemCreateUser, CER_SystemLastEditTimeUtc, CER_SystemLastEditUser, CER_Behavior, CER_CXC_Consignment)
	VALUES (@CER_PK, 1, 1, @CER_CXH_Header, 'PRE', 'IEDUB100', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'DIS', @CER_CXC_Consignment)
";
			using (DbCommand command = Db.Connection.Command(sqlReport))
			{
				command.AddParameter("@CER_PK", SqlDbType.UniqueIdentifier, pkReport);
				command.AddParameter("@CER_CXH_Header", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@CER_CXC_Consignment", SqlDbType.UniqueIdentifier, pkConsignment);
				command.ExecuteNonQuery();
			}
			return (ZGuid)pkReport;
		}
	}
}
