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

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeferredScheduledMessageLogSubscriber))]
	sealed class DeferredScheduledEntryLodgementMessageLogSubscriberTest : LogSubscriberTest<DeferredScheduledMessageLogSubscriber>
	{
		[ExpectNoExceptions]
		[TestDate(2023, 11, 01, 12, 00, 00, 000)]
		public void TestDeferredScheduledMessageLogSubscriberWithNoErrors()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
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
				var brokerLicence = user.Certificates.AddNew();
				brokerLicence.XZ_RefNumber = "54321";
				brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
				Factory.Save();

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "ABC";
				var deliveryAddress = importer.Addresses.AddNew();
				deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
				deliveryAddress.OA_Address1 = "Addr 1";
				deliveryAddress.OA_Address2 = "Addr 2";
				deliveryAddress.OA_City = "CTY";
				deliveryAddress.OA_State = "NSW";
				deliveryAddress.OA_PostCode = "2001";

				var jobDeclaration = JobDeclaration.New(Factory);
				jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				jobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				jobDeclaration.JE_AgentsReference = "AGENT123";
				jobDeclaration.JE_CustomsDischargePort = "AUSYD";
				jobDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddHours(8);
				jobDeclaration.JE_OH_Importer = importer.PK;
				jobDeclaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
				var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "AAA";

				var entryLine = entryHeader.MergedLines.AddNew();
				var invoiceLine = entryHeader.Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				entryHeader.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

				using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var messageType = CMRMessageTypes.LodgeWithPay;
					var isLodgeWithPay = messageType == CMRMessageTypes.LodgeWithPay;
					jobDeclaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithPay), eventTime: jobDeclaration.JE_EDITransmitDate.ToDateTime(), isEstimate: false));
					entryHeader.CH_Status = isLodgeWithPay ? CustomsEntryStatus.ScheduledLodgeWithPayment.Code : CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
					jobDeclaration.JE_MessageStatus = entryHeader.CH_Status;
					Factory.Save();
				}

				AssertEquals("TSU", jobDeclaration.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.DeferredScheduledMessage).First().SL_GS_NKUser);

				var mockQuerier = new Mock<IServiceManagerQuerier>();
				mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (ObjectFactory.Substitute(mockQuerier.Object))
				{
					RunLogWalkerCycleForTest();

					CombineAssertions(() =>
					{
						AssertEquals("Message should have been generated", 1, entryHeader.Messages.Count);
						var outMessageText = entryHeader.Messages[0].EM_MessageText;
						AssertContains("Senders Reference", $"BGM+929:::IMD+{jobDeclaration.JE_DeclarationReference}/1/DAT1:1+9", outMessageText);
						AssertContains("Agent Reference", $"RFF+ADU:{jobDeclaration.JE_DeclarationReference}/1 AGENT123", outMessageText);
						AssertContains("Importer Delivery Address", "NAD+DP++CTY++ADDR 1::ADDR 2++:::NSW+2001+AU", outMessageText);
						AssertContains("Broker licence number", "NAD+CB+54321::95", outMessageText);

						var notifications = string.Join("\r\n", NotifiedEventList) + "\r\n";
						AssertContains("[DSM Log Subscriber] Generating entry lodgement messages for Job " + jobDeclaration.JobNumber, notifications);
						AssertContains("[DSM Log Subscriber] 1 messages successfully created.\r\n", notifications);
					});
				}
			}
		}

		[ExpectNoExceptions]
		[TestDate(2023, 11, 01, 12, 00, 00, 000)]
		public void TestDeferredScheduledMessageLogSubscriberWithSomeErrors()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				NotifiedEventList.Clear();
				Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
				var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";

				var user = Factory.New<GlbStaff>();
				user.GS_LoginName = "TEST USER";
				user.GS_Code = "TSU";
				user.GS_FullName = "TSU";
				user.GS_EmailAddress = "tsu@abc.com";
				user.GS_IsController = true;
				Factory.Save();

				var importer = Factory.New<OrgHeader>();
				importer.LocalBusinessRegNo = GlbCompany.CurrentCompany.GC_BusinessRegNo;
				importer.OH_Code = "ABC";
				var jobDeclaration = JobDeclaration.New(Factory);
				jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				jobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				jobDeclaration.JE_CustomsDischargePort = "AUSYD";
				jobDeclaration.JE_OH_Importer = importer.PK;
				jobDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddHours(8);
				var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "AAA";

				var entryLine = entryHeader.MergedLines.AddNew();
				var invoiceLine = entryHeader.Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var messageType = CMRMessageTypes.LodgeWithPay;
					var isLodgeWithPay = messageType == CMRMessageTypes.LodgeWithPay;
					jobDeclaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithPay), eventTime: jobDeclaration.JE_EDITransmitDate.ToDateTime(), isEstimate: false));
					entryHeader.CH_Status = isLodgeWithPay ? CustomsEntryStatus.ScheduledLodgeWithPayment.Code : CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
					jobDeclaration.JE_MessageStatus = entryHeader.CH_Status;
					Factory.Save();
				}

				var mockQuerier = new Mock<IServiceManagerQuerier>();
				mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (ObjectFactory.Substitute(mockQuerier.Object))
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					RunLogWalkerCycleForTest();

					AssertEquals("Message should have been generated", 1, entryHeader.Messages.Count);

					var notifications = string.Join("\r\n", NotifiedEventList) + "\r\n";
					AssertContains("[DSM Log Subscriber] Generating entry lodgement messages for Job " + jobDeclaration.JobNumber, notifications);
					AssertContains("[DSM Log Subscriber] 1 messages successfully created. Errors or warnings occurred:\r\n You cannot send a message because the Local Customs Branch Id has not been entered. Please enter this through the registry.\r\n", notifications);

					var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "AU Import Declaration Send errors or warnings");
					AssertNotNull("Errors email sent", email);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestDeferredScheduledMessageLogSubscriberWithNoErrors_ConsolidatedDeclaration()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
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
				var brokerLicence = user.Certificates.AddNew();
				brokerLicence.XZ_RefNumber = "54321";
				brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
				Factory.Save();

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "ABC";
				var deliveryAddress = importer.Addresses.AddNew();
				deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
				deliveryAddress.OA_Address1 = "Addr 1";
				deliveryAddress.OA_Address2 = "Addr 2";
				deliveryAddress.OA_City = "CTY";
				deliveryAddress.OA_State = "NSW";
				deliveryAddress.OA_PostCode = "2001";

				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				leadDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				leadDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				leadDeclaration.JE_AgentsReference = "AGENT123";
				leadDeclaration.JE_CustomsDischargePort = "AUSYD";
				leadDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddHours(8);
				leadDeclaration.JE_OH_Importer = importer.PK;
				leadDeclaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
				var entryHeader = leadDeclaration.CustomsEntryHeaders[0];
				entryHeader.EntryNumber = "AAA";

				var entryLine = entryHeader.MergedLines.AddNew();
				var invoiceLine = entryHeader.Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				entryHeader.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

				using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					consolidatedDeclaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithPay), eventTime: ZDateTime.Today.AddDays(-5).ToDateTime(), isEstimate: false));
					leadDeclaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
					Factory.Save();
				}

				var mockQuerier = new Mock<IServiceManagerQuerier>();
				mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (ObjectFactory.Substitute(mockQuerier.Object))
				{
					RunLogWalkerCycleForTest();

					CombineAssertions(() =>
					{
						var notifications = string.Join("\r\n", NotifiedEventList) + "\r\n";
						AssertContains("[DSM Log Subscriber] Generating entry lodgement messages for Job " + consolidatedDeclaration.CRD_JobReferenceNumber, notifications);
						AssertContains("[DSM Log Subscriber] 1 messages successfully created.\r\n", notifications);

						AssertEquals("Message should have been generated", 1, consolidatedDeclaration.Messages.Count);
						var outMessageText = consolidatedDeclaration.Messages[0].EM_MessageText;
						AssertContains("Senders Reference", $"BGM+929:::IMD+{consolidatedDeclaration.CRD_JobReferenceNumber}/DAT1:1+9", outMessageText);
						AssertContains("Agent Reference", $"RFF+ADU:{consolidatedDeclaration.CRD_JobReferenceNumber} AGENT123", outMessageText);
						AssertContains("Importer Delivery Address", "NAD+DP++CTY++ADDR 1::ADDR 2++:::NSW+2001+AU", outMessageText);
						AssertContains("Broker licence number", "NAD+CB+54321::95", outMessageText);
						AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingFormalLodge.Code, leadDeclaration.JE_MessageStatus);
						AssertEquals("EntryHeader MessageStatusDescription", CustomsEntryStatus.AwaitingFormalLodge.Description, entryHeader.MessageStatusDescription);
					});
				}
			}
		}
	}
}
