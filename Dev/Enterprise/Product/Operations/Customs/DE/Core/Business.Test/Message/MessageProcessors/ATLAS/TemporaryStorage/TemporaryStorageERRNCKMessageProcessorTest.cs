using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageERRNCKMessageProcessor))]
	sealed class TemporaryStorageERRNCKMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageERRNCKMessageProcessor, AtlasInboundEDIMessage<IERRNCK>>
	{
		public void TestLinkedObjectNotFound()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(storageDec, "NOTORIGINALMSG");

			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(storageDec, ReferencedMessageIdentifier);
			ProcessMessage(message);
			AssertEquals(storageDec, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IERRNCK)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";

			var outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(storageDec, ReferencedMessageIdentifier);
			outgoingMessage.EM_MessageType = Messaging.EDIMessageTypeList.Codes.TemporaryStorage;
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ProcessMessage(message);
			CombineAssertions("Set declaration message status to REJ", () =>
			{
				AssertEquals("incomingMessage.EM_LinkUniqueID", storageDec.PK, message.EM_LinkUniqueID);
				AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("dec.STH_MessageStatus", EDIMessageStatusList.Codes.Rejected, storageDec.STH_MessageStatus);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(storageDec, ReferencedMessageIdentifier);
			ProcessMessage(message);
			AssertEquals($"{ReferencedMessageIdentifier}, {ReferenceNumber}", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNumWithEmptyReferenceNummber()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(storageDec, ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals(ReferencedMessageIdentifier, message.GetLogbookRegistrationNumber());
		}

		public void TestSendErrorEMail_StaffMember()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(storageDec, ReferencedMessageIdentifier, "senduser@mail.com");
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				AssertErrorMessageEmail(storageHeader, new ZString[] { "senduser@mail.com" });
			}
		}

		public void TestSendErrorEMail_NominatedGroup()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(storageDec, ReferencedMessageIdentifier, "senduser@mail.com");
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				AssertErrorMessageEmail(storageHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		public void TestSendErrorEMail_StaffMemberAndNominatedGroup()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(storageDec, ReferencedMessageIdentifier, "senduser@mail.com");
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				AssertErrorMessageEmail(storageHeader, new ZString[] { "senduser@mail.com", "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		protected override ZString MessageFriendlyName => "Temporary Storage E_ERR_NCK Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IERRNCK>> Processor => new TemporaryStorageERRNCKMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			storageHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			storageHeader.SJH_JobReference = "REFERENCE1";
			storageDec = CUSPRLCusTempStorageDec.New(storageHeader);

			var err1 = new Mock<IERRNCKError>();
			err1.Setup(m => m.Code).Returns("COM00000");
			err1.Setup(m => m.Pointer).Returns(ZString.Empty);
			err1.Setup(m => m.Text).Returns("duplicate detected");
			err1.Setup(m => m.OriginalValue).Returns("00000000000006");
			var err2 = new Mock<IERRNCKError>();
			err2.Setup(m => m.Code).Returns("COM00002");
			err2.Setup(m => m.Pointer).Returns(ZString.Empty);
			err2.Setup(m => m.Text).Returns("duplicate detected2");
			err2.Setup(m => m.OriginalValue).Returns("00000000000007");

			dataProviderMock = new Mock<IERRNCK>();
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);
			dataProviderMock.Setup(m => m.Errors).Returns(new IERRNCKError[] { err1.Object, err2.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IERRNCK>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<IERRNCK> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IERRNCK>> messageMock;
		AtlasInboundEDIMessage<IERRNCK> message;
		CusTempStorageJobHeader storageHeader;
		CUSPRLCusTempStorageDec storageDec;
		const string MessageIdentifier = "ERRNCK58660000003090084260719145523";
		const string ReferencedMessageIdentifier = "DE899978300000000812";
		const string ReferenceNumber = "ATB150000620520195875";
		const string LocalReferenceNumber = "LocalReference";

		void AssertErrorMessageEmail(CusTempStorageJobHeader jobHeader, ZString[] expectedEmailRecipientsMailAddress)
		{
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var reference = jobHeader.SJH_JobReference;
			var subject = $"SumA ERRNCK - Error message Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>SumA ERRNCK - Error message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=TemporaryStorage&BusinessEntityPK={jobHeader.PK}";
			var bodyMessageSummary = $"Your SumA Declaration for Job {reference} received an Error Message. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
								   + $"<tr><td>Local Reference Number</td><td>{LocalReferenceNumber}</td></tr>"
								   + "</table>";
			CombineAssertions(() => AssertEmailWithTable(ZString.Empty, email, expectedEmailRecipientsMailAddress, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable));
		}
	}
}
