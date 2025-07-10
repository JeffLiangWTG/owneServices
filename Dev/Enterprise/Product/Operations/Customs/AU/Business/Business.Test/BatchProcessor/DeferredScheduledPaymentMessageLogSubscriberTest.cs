using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeferredScheduledMessageLogSubscriber))]
	[TestDate(2023, 12, 12, 6, 0, 0)]
	sealed class DeferredScheduledPaymentMessageLogSubscriberTest : LogSubscriberTest<DeferredScheduledMessageLogSubscriber>
	{
		public void TestDeferredScheduledMessageLogSubscriberNoErrors()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				NotifiedEventList.Clear();
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
				Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();

				var entryHeader = SetupDeferredScheduledMessage(nameof(CMRMessageTypes.Payment));

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				RunLogWalkerCycleForTest();

				AssertEquals("Message should have been generated", 2, entryHeader.Messages.Count);
				var outMessage = entryHeader.Messages[1];
				AssertEquals("TSU", outMessage.EM_SystemCreateUser);
				var messageText = outMessage.EM_MessageText;
				AssertContains("message is Payment", "'BGM+481:::PAYSTD+", messageText);
				AssertContains("message includes payment amount", "'MOA+128:200.90'", messageText);

				var notifications = string.Join("\r\n", NotifiedEventList) + "\r\n";
				AssertContains("[DSM Log Subscriber] Generating payment messages for Job " + entryHeader.Declaration.JobNumber, notifications);
				AssertContains("[DSM Log Subscriber] 1 messages successfully created.\r\n", notifications);

				AssertEquals("Emails Created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		public void TestDeferredScheduledMessageLogSubscriberNoErrors_ConsolidatedDeclaration()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				NotifiedEventList.Clear();
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
				Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();

				var user = Factory.New<GlbStaff>();
				user.GS_LoginName = "TEST USER";
				user.GS_Code = "TSU";
				user.GS_FullName = "TSU";
				user.GS_EmailAddress = "tsu@abc.com";
				user.GS_IsController = true;
				Factory.Save();

				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				leadDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				leadDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				leadDeclaration.JE_CustomsDischargePort = "AUSYN";
				leadDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				leadDeclaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var otherDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
				otherDeclaration.JE_ApplicationCode = AUCustoms.ImportMessagingMode.ForceCMRMessages;
				otherDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var entryHeader = leadDeclaration.CustomsEntryHeaders[0];
				entryHeader.EntryNumber = "AAA";
				entryHeader.ScheduledPaymentDate = ZDateTime.Now;
				entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.NotPaid;
				entryHeader.AddInfo.ZA_CustomsPayNow_Hidden = 200.90m;

				var entryLine = entryHeader.MergedLines.AddNew();
				var invoiceLine = leadDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var imdrResponse = Factory.New<CMRIMDRMessage>();
				imdrResponse.EM_MessageText = TestMessages.IMDRMessageText;
				imdrResponse.EM_ReceiveTransmit = CMRIMDRMessage.Direction.Receive;
				imdrResponse.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-7);
				consolidatedDeclaration.Messages.Add(imdrResponse);

				using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					consolidatedDeclaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.Payment), eventTime: ZDateTime.Today.AddDays(-5).ToDateTime(), isEstimate: false));
					leadDeclaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledPayment.Code;
					Factory.Save();
				}

				var mockQuerier = new Mock<IServiceManagerQuerier>();
				mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (ObjectFactory.Substitute(mockQuerier.Object))
				{
					RunLogWalkerCycleForTest();

					CombineAssertions(() =>
					{
						AssertEquals("Message should have been generated", 2, consolidatedDeclaration.Messages.Count);
						var outMessage = consolidatedDeclaration.Messages[1];
						AssertEquals("TSU", outMessage.EM_SystemCreateUser);
						var messageText = outMessage.EM_MessageText;
						AssertContains("message is Payment", "'BGM+481:::PAYSTD+", messageText);
						AssertContains("message includes payment amount", "'MOA+128:200.90'", messageText);
						AssertEquals("JE_MessageStatus of lead declaration", CustomsEntryStatus.AwaitingPayment.Code, leadDeclaration.JE_MessageStatus);
						AssertEquals("EntryHeader MessageStatus of lead declaration", CustomsEntryStatus.AwaitingPayment.Description, entryHeader.MessageStatusDescription);
						AssertEquals("JE_MessageStatus of non-lead declaration", CustomsEntryStatus.AwaitingPayment.Code, otherDeclaration.JE_MessageStatus);
						AssertEquals("EntryHeader MessageStatus of non-lead declaration", CustomsEntryStatus.AwaitingPayment.Description, otherDeclaration.ActiveEntryHeaders[0].MessageStatusDescription);

						var notifications = string.Join("\r\n", NotifiedEventList) + "\r\n";
						AssertContains("[DSM Log Subscriber] Generating payment messages for Job " + consolidatedDeclaration.CRD_JobReferenceNumber, notifications);
						AssertContains("[DSM Log Subscriber] 1 messages successfully created.\r\n", notifications);
					});
				}
			}
		}

		public void TestDeferredScheduledMessageLogSubscriberReportsErrors()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				NotifiedEventList.Clear();
				Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
				var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";

				var entryHeader = SetupDeferredScheduledMessage(nameof(CMRMessageTypes.Payment));

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				RunLogWalkerCycleForTest();

				AssertEquals("Message should have been generated", 2, entryHeader.Messages.Count);
				var messageText = entryHeader.Messages[1].EM_MessageText;
				AssertContains("message is Payment", "'BGM+481:::PAYSTD+", messageText);
				AssertContains("message includes payment amount", "'MOA+128:200.90'", messageText);

				var notifications = string.Join("\r\n", NotifiedEventList);
				AssertContains("[DSM Log Subscriber] Generating payment messages for Job " + entryHeader.Declaration.JobNumber, notifications);
				AssertContains("[DSM Log Subscriber] 1 messages successfully created. Errors or warnings occurred:\r\n You cannot send a message because the Local Customs Branch Id has not been entered. Please enter this through the registry.\r\n", notifications);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "AU Import Declaration Send errors or warnings");
				AssertNotNull("Errors email sent", email);
			}
		}

		public void TestDeferredScheduledMessageLogSubscriberInvalidReference()
		{
			NotifiedEventList.Clear();
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();

			var entryHeader = SetupDeferredScheduledMessage("XXX");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			RunLogWalkerCycleForTest();

			var notifications = string.Join("\r\n", NotifiedEventList);
			AssertContains("[DSM Log Subscriber] XXX is not a recognised IMD Message Type.", notifications);
			AssertEquals("Message should not have been generated", 1, entryHeader.Messages.Count);
		}

		public void TestDeferredScheduledMessageLogSubscriberInvalidPayment()
		{
			NotifiedEventList.Clear();
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();

			var entryHeader = SetupDeferredScheduledMessage(nameof(CMRMessageTypes.Payment));
			entryHeader.AddInfo.ZA_AQISPayNow_Hidden = 0m;
			entryHeader.AddInfo.ZA_CustomsPayNow_Hidden = 0m;
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			RunLogWalkerCycleForTest();

			var notifications = string.Join("\r\n", NotifiedEventList);
			AssertContains("Payment Message has no Amounts to Pay.", notifications);

			entryHeader.Reload();
			entryHeader.Declaration.Reload();
			AssertEquals("Entry Status is Failed Payment", CustomsEntryStatus.FailPayment.Code, entryHeader.CH_Status);
			AssertEquals("Declaration Status is Failed Payment", CustomsEntryStatus.FailPayment.Code, entryHeader.Declaration.JE_MessageStatus);
			AssertEquals("Message should not have been generated", 1, entryHeader.Messages.Count);
		}

		CusEntryHeader SetupDeferredScheduledMessage(string logReference)
		{
			var user = Factory.New<GlbStaff>();
			user.GS_LoginName = "TEST USER";
			user.GS_Code = "TSU";
			user.GS_FullName = "TSU";
			user.GS_EmailAddress = "tsu@abc.com";
			user.GS_IsController = true;
			Factory.Save();

			var jobDeclaration = JobDeclaration.New(Factory);
			jobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_CustomsDischargePort = "AUSYN";
			jobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			jobDeclaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC";
			jobDeclaration.JE_OH_Importer = importer.PK;

			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA";
			entryHeader.ScheduledPaymentDate = ZDateTime.Now;
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.NotPaid;

			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var imdrResponse = Factory.New<CMRIMDRMessage>();
			imdrResponse.EM_MessageText = TestMessages.IMDRMessageText;
			imdrResponse.EM_LinkedObject = entryHeader;
			imdrResponse.EM_ReceiveTransmit = CMRIMDRMessage.Direction.Receive;
			AssertEquals("Prerec: Total payable in the response", 191.90m, new OutstandingAmountRetriever(imdrResponse).OutstandingAmount);
			entryHeader.AddInfo.ZA_CustomsPayNow_Hidden = 200.90m;
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				jobDeclaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: logReference, eventTime: entryHeader.ScheduledPaymentDate.ToDateTime(), isEstimate: false));
				entryHeader.CH_Status = CustomsEntryStatus.ScheduledPayment.Code;
				jobDeclaration.JE_MessageStatus = entryHeader.CH_Status;
				Factory.Save();
			}

			AssertEquals("Prerec", "TSU", jobDeclaration.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.DeferredScheduledMessage).First().SL_GS_NKUser);

			return entryHeader;
		}
	}
}
