using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Testing.MessageProcessors;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class Message1171ProcessorTest : BaseILBranchCustomsApplicationTypeMessageProcessorTest<Message1171Processor, ILMAN171ResponseMessage>
	{
		public void TestProcessMessage_Discarded_WhenNoOceanManifestHeaderFound()
		{
			header.AMA_ManifestNumber = ZString.Empty;
			bill.ABL_SequenceNumber = 0;
			bill.TransportDocuments.RemoveAndDeleteAll();
			Factory.Save();

			var message = GetResponseMessage("Ocean");

			SetupHeader(Factory, "241169", "I025544301");
			AssertMessageDiscarded("When No Manifest Header Found - consider Transport Document", message, discardReasonManifestNotFound);

			SetupHeader(Factory, "241168", "I025544300");
			AssertMessageDiscarded("When No Manifest Header Found - consider Manifest Number", message, discardReasonManifestNotFound);
		}

		public void TestProcessMessage_Discarded_WhenNoRoadManifestHeaderFound()
		{
			var message = GetResponseMessage("Road");
			header.AMA_ManifestNumber = ZString.Empty;
			bill.ABL_SequenceNumber = 0;
			bill.TransportDocuments.RemoveAndDeleteAll();
			Factory.Save();

			SetupHeader(Factory, "I123456789123457", "I025544300");
			AssertMessageDiscarded("When No Manifest Header Found - consider Manifest Number", message, discardReasonManifestNotFound);
		}

		public void TestProcessMessage_ProcessedOK_WhenOceanManifestHeaderFound()
		{
			var factory = Factory;
			var message = GetResponseMessage("Ocean");
			message.EM_LinkedObject = header;

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Manifest Header Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The header message Status has changed to Acknowledged", "ACK", header.AMA_MessageStatus);
				AssertEquals("The header Customs Status updated from the received Message", "1", header.RegistrationStatus);
				AssertEquals("The bill's customs status is taken from the section with a single <Pointer> having <DocumentSectionCode> = 28A.", "11", bill.ABL_BillStatus);
			});
		}

		public void TestProcessMessage_ProcessedOK_WhenManifestHeaderFoundWithoutNotification()
		{
			var responseMessage = GetResponseMessage("Ocean");
			responseMessage.EM_LinkedObject = header;

			Processor.ProcessMessage(responseMessage);

			CombineAssertions("When Manifest Header Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", responseMessage.EM_Status);
				AssertEquals("The header message Status has changed to Acknowledged", "ACK", header.AMA_MessageStatus);
				AssertEquals("The header Customs Status updated from the received Message", "1", header.RegistrationStatus);
				AssertEquals("The bill's customs status is taken from the section with a single <Pointer> having <DocumentSectionCode> = 28A.", "11", bill.ABL_BillStatus);
				AssertEquals("When Request Message without Create User", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		public void TestProcessMessage_ProcessedOK_WhenRoadManifestHeaderFound()
		{
			var factory = Factory;
			var message = GetResponseMessage("Road");
			message.EM_LinkedObject = header;

			SetupHeader(factory, "I123456789123456", "I2222");

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Manifest Header Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The header message Status has changed to Acknowledged", "ACK", header.AMA_MessageStatus);
				AssertEquals("The header Customs Status updated from the received Message", "1", header.RegistrationStatus);
				AssertEquals("The bill's customs status is taken from the section with a single <Pointer> having <DocumentSectionCode> = 28A.", "11", bill.ABL_BillStatus);
			});
		}

		public void TestGeneratedMail_StaffBySessionGUID()
		{
			var factory = Factory;

			var group = factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z1";
			staff1.GS_LoginName = "Z1";
			staff1.GS_EmailAddress = "test@test.mail.com";

			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "Z2";
			staff2.GS_LoginName = "Z2";
			staff2.GS_EmailAddress = "test2@test2.mail.com";

			factory.Save();

			var requestMessage = GetRequestMessage();
			requestMessage.EM_MessageNum = "99";
			requestMessage.EM_SystemCreateUser = staff1.GS_Code;
			var logger = new LoggingInformation();
			var packer = new ILMessagePacker();
			var requestInterchange = factory.New<EDIInterchange>();
			packer.Pack(requestMessage, requestInterchange, logger);
			requestMessage.Interchange.EI_SessionGUID = Guid.NewGuid();

			var requestMessage2 = GetRequestMessage();
			requestMessage2.EM_MessageNum = "100";
			requestMessage2.EM_SystemCreateUser = staff2.GS_Code;
			var requestInterchange2 = factory.New<EDIInterchange>();
			packer.Pack(requestMessage, requestInterchange2, logger);
			requestMessage.Interchange.EI_SessionGUID = Guid.NewGuid();

			var responseMessage = GetResponseMessage("Ocean");
			var responseInterchange = factory.New<EDIInterchange>();
			packer.Pack(responseMessage, responseInterchange, logger);
			responseMessage.Interchange.EI_SessionGUID = requestMessage.Interchange.EI_SessionGUID;
			responseMessage.EM_LinkedObject = header;
			header.AMA_ManifestNumber = "241169";
			bill.ABL_SequenceNumber = 6556;

			var addInfo = bill.AdditionalInfos.AddNew();
			addInfo.CSI_Type = "OTH";
			addInfo.CSI_SubType = "TRA";
			addInfo.CSI_Code = "IL2";
			addInfo.CSI_ReferenceNumber = "I025544300";
			factory.Save();

			const string sendEmailStaffMember = "ESM";
			SetupRegistryData(sendEmailStaffMember, group.PK, false);
			AssertSuccessMail(header, responseMessage, staff1);
		}

		public void TestGeneratedMail()
		{
			var factory = Factory;

			var group = factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z1";
			staff1.GS_LoginName = "Z1";
			staff1.GS_EmailAddress = "test@test.mail.com";
			factory.Save();

			var requestMessage = GetRequestMessage();
			requestMessage.EM_MessageNum = "99";
			requestMessage.EM_SystemCreateUser = staff1.GS_Code;

			var responseMessage = GetResponseMessage("Ocean");
			responseMessage.EM_LinkedObject = header;

			header.AMA_ManifestNumber = "241169";
			bill.ABL_SequenceNumber = 6556;

			var addInfo = bill.AdditionalInfos.AddNew();
			addInfo.CSI_Type = "OTH";
			addInfo.CSI_SubType = "TRA";
			addInfo.CSI_Code = "IL2";
			addInfo.CSI_ReferenceNumber = "I025544300";
			factory.Save();

			const string sendEmailStaffMember = "ESM";
			SetupRegistryData(sendEmailStaffMember, group.PK, false);
			AssertSuccessMail(header, responseMessage, staff1);
			SetupRegistryData(sendEmailStaffMember, group.PK, true);
			AssertNullMail(header, responseMessage);

			const string sendEmailNominatedGroup = "ENG";
			SetupRegistryData(sendEmailNominatedGroup, group.PK, false);
			AssertSuccessMail(header, responseMessage, staff1);
			SetupRegistryData(sendEmailNominatedGroup, group.PK, true);
			AssertNullMail(header, responseMessage);

			const string sendEmailStaffMemberandNominatedGroup = "ESG";
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, false);
			AssertSuccessMail(header, responseMessage, staff1);
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, true);
			AssertNullMail(header, responseMessage);

			const string sendEmailStaffMemberOrNominatedGroupForGroup = "EOG";
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, false);
			AssertSuccessMail(header, responseMessage, staff1);
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, true);
			AssertNullMail(header, responseMessage);

			const string sendNoEmail = "NOE";
			SetupRegistryData(sendNoEmail, group.PK, false);
			AssertNullMail(header, responseMessage);
			SetupRegistryData(sendNoEmail, group.PK, true);
			AssertNullMail(header, responseMessage);
		}

		public void TestProcessMessage_UpdateRequestedSupportingDocument_WithRequestDocument()
		{
			var responseMessage = GetResponseMessage("WithRequestDocument");
			responseMessage.EM_LinkedObject = header;

			header.AMA_ManifestNumber = "241276";
			bill.ABL_SequenceNumber = 10;

			var bill11 = header.Bills.AddNew();
			bill11.ABL_SequenceNumber = 11;

			var addInfo = bill.TransportDocuments.AddNew();
			addInfo.CSI_Code = "IL2";
			addInfo.CSI_ReferenceNumber = "I025510500";
			Factory.Save();

			Processor.ProcessMessage(responseMessage);

			CombineAssertions("When Manifest Header Found", () =>
			{
				AssertEquals("The message status should be ProcessedOK", "PRS", responseMessage.EM_Status);
				AssertEquals("The header message status should change to Acknowledged", "ACK", header.AMA_MessageStatus);
				AssertEquals("The bill customs status should be updated from the received message", "11", bill.ABL_BillStatus);
				AssertEquals("There should be no emails created when request message is without create user", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

				var requestSupportingDocument = bill.SupportingDocuments.Cast<CusSupportingInfo>().FirstOrDefault(d => d.CSI_Status == "REQ");
				AssertNotNull("Bill 10 should have a request supporting document", requestSupportingDocument);
				AssertEquals("Bill 10 request supporting document should have CSI_Code 'IL_287'", "IL_287", requestSupportingDocument.CSI_Code);
				AssertEquals("Bill 10 request supporting document should have CSI_ReferenceNumber2 '1025710875'", "1025710875", requestSupportingDocument.CSI_ReferenceNumber2);
				AssertEquals("Bill 10 request supporting document should have CSI_AdditionalDescription 'Hello World שלום'", "Hello World שלום", requestSupportingDocument.CSI_AdditionalDescription);

				requestSupportingDocument = bill11.SupportingDocuments.Cast<CusSupportingInfo>().FirstOrDefault(d => d.CSI_Status == "REQ");
				AssertNotNull("Bill 11 should have a request supporting document", requestSupportingDocument);
				AssertEquals("Bill 11 request supporting document should have CSI_Code '707'", "707", requestSupportingDocument.CSI_Code);
				AssertEquals("Bill 11 request supporting document should have CSI_ReferenceNumber2 '1025710874'", "1025710874", requestSupportingDocument.CSI_ReferenceNumber2);
				AssertEquals("Bill 11 request supporting document should have CSI_AdditionalDescription ...", "The consignment amendment in stage : is awaiting manuel approval, possible after the submission of the documents : XYZ", requestSupportingDocument.CSI_AdditionalDescription);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		public void TestProcessMessage_SetDiscardedStatus_WhenCouldNotLocateBillWithSequenceNumber()
		{
			var responseMessage = GetResponseMessage("WithRequestDocument");

			SetupHeader(Factory, "241276", "I025510500", 10);

			AssertMessageDiscarded("When Response Without Declaration Section", responseMessage, string.Format(discardReasonBillNotFound, 11));
		}

		public void TestProcessMessage_ShouldNotRaiseException_WhenResponseWithoutDeclarationSection()
		{
			var message = GetResponseMessage("WithoutDeclarationSection");
			SetupHeader(Factory, "241169", "I025544301");
			AssertMessageDiscarded("When Response Without Declaration Section", message, discardReasonManifestNotFound);
		}

		public void TestProcessMessage_ShouldNotRaiseException_DeclarationWithoutConsignment()
		{
			var message = GetResponseMessage("WithoutConsignmentSection");
			SetupHeader(Factory, "241169", "I025544301");
			AssertMessageDiscarded("When Response Without Consignment Section", message, discardReasonManifestNotFound);
		}

		public void TestProcessMessage_Discarded_WhenWithoutIL2AndNoLinkedObject()
		{
			var message = GetResponseMessage("Ocean_WithoutIL2");
			SetupHeader(Factory, "241169", "I025544301");
			AssertMessageDiscarded("When No Manifest Header Found - consider Transport Document", message, discardReasonManifestNotFound);
		}

		public void TestProcessMessage_ProcessedOK_WhenWithoutIL2AndLinkedObjectIsValid()
		{
			var factory = Factory;
			var message = GetResponseMessage("Ocean_WithoutIL2");

			SetupHeader(factory, "250115", "I025011500");
			message.EM_LinkedObject = header;
			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Manifest Header Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The header message Status has changed to Acknowledged", "ACK", header.AMA_MessageStatus);
				AssertEquals("The header Customs Status updated from the received Message", "1", header.RegistrationStatus);
				AssertEquals("the bill Customs Status updated from the received Message", "1", bill.ABL_BillStatus);
			});
		}

		public void TestProcessMessage_Discarded_WhenWithoutIL2AndLinkedObjectIsNotValid()
		{
			var factory = Factory;
			bill.TransportDocuments.RemoveAndDeleteAll();
			factory.Save();

			var message = GetResponseMessage("Ocean_WithoutIL2");
			AssertMessageDiscarded("When Ocean manifest without IL2", message, discardReasonManifestNotFound);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			bill = header.Bills.AddNew();
			SetupHeader(Factory, "241169", "I025544300");
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
			disposableActionILMANGroupNotification?.Dispose();
		}

		void SetupHeader(BusinessObjectFactory factory, ZString manifestNumber, ZString transportReferenceNumber, short billSequenceNumber = 6556)
		{
			header.AMA_ManifestNumber = manifestNumber;
			bill.ABL_SequenceNumber = billSequenceNumber;

			var addInfo = bill.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL2") ?? bill.TransportDocuments.AddNew();
			addInfo.CSI_Code = "IL2";
			addInfo.CSI_ReferenceNumber = transportReferenceNumber;
			factory.Save();
		}

		void AssertMessageDiscarded(string scenarioName, ILMAN171ResponseMessage message, string discardReason)
		{
			var metaData = Processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());

			CombineAssertions(scenarioName, () =>
			{
				Assert("Discard reason was provided", !metaData.DiscardReason.IsEmpty);
				AssertEquals("Discard reason is as expected", discardReason, metaData.DiscardReason);
			});
		}

		ILMAN171ResponseMessage GetResponseMessage(string suffix)
		{
			var message = Factory.New<ILMAN171ResponseMessage>();
			message.EM_ApplicationCode = "ILC";
			message.EM_MessageType = "MAN";
			message.EM_MessageSubType = "171";
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = "RCV";

			message.EM_MessageText = new EmbeddedResourceRetriever().GetString($"Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.Manifest_1171_{suffix}.xml");
			return message;
		}

		EDIMessage GetRequestMessage()
		{
			var message = Factory.NewWithValidTestData<ILEDIMessage>();
			message.EM_ApplicationCode = "ILC";
			message.EM_MessageType = "MAN";
			message.EM_MessageSubType = "171";
			message.EM_Status = "SNT";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = header.PK;
			message.EM_MessageText = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.Manifest_1171_Ocean.xml");
			return message;
		}

		void SetupRegistryData(ZString sendMode, ZGuid sendGroupPK, ZBool sendErrorOnly)
		{
			disposableActionILMANGroupNotification = ILCustomsDataRegistry.Instance.ILMANGroupNotification.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(sendMode, sendGroupPK, sendErrorOnly));
		}

		void AssertSuccessMail(AsycudaManifestHeader header, EDIMessage responseMessage, GlbStaff staff1)
		{
			Processor.ProcessMessage(responseMessage);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Message Response for " + header.AMA_JobReference);
			var bodyText = email.Body;

			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", staff1.GS_EmailAddress, email.Recipients[0].Email);

				Assert("Contains Issuing Date", bodyText.Contains("Issuing Date"));
				Assert("Contains Functional Reference", bodyText.Contains("Functional Reference"));
				Assert("Contains Manifest Message Response for", bodyText.Contains("Manifest Message Response for"));

				AssertEquals("The message status is ProcessedOK", "PRS", responseMessage.EM_Status);
				AssertEquals("The header message Status has changed to Acknowledged", "ACK", header.AMA_MessageStatus);
				AssertEquals("The header Customs Status updated from the received Message", "1", header.RegistrationStatus);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		void AssertNullMail(AsycudaManifestHeader header, EDIMessage message)
		{
			Processor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		protected override Message1171Processor CreateProcessor(LoggingInformation loggingInformation) => new Message1171Processor(new LoggingInformation());

		IDisposable disposableAction;
		AsycudaManifestHeader header;
		AsycudaBill bill;
		IDisposable disposableActionILMANGroupNotification;

		protected override string BasicSuccessfulMessageText => new EmbeddedResourceRetriever().GetString($"Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.Manifest_1171_Ocean.xml");

		protected override string ExpectedMessageFriendlyName => "IL Manifest Response Message";

		protected override string ExpectedMessageTypesToInclude => "MAN";

		protected override string ExpectedMessageSubTypesToInclude => "171";

		protected override BusinessObject ExpectedLinkedObject => header;

		protected override ZGuid ExpectedBranchPk => header.Branch.PK;

		readonly string discardReasonManifestNotFound = "Couldn't locate Job using provided Manifest #";
		readonly string discardReasonBillNotFound = "Couldn't locate Bill with Sequence Number {0}";
	}
}
