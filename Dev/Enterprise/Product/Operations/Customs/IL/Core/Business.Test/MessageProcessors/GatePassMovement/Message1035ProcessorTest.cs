using System;
using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class Message1035ProcessorTest
		: BaseMessageProcessorForSingleNumberSupportConsolTest<Message1035Processor, GpNg1035Msg2GatepassFeedbackMessage, ILGPM135ResponseMessage>
	{
		public void TestEmailGeneration_StaffBySessionGUID()
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

			var requestMessage = GetRequestMessage(shipment);
			requestMessage.EM_MessageNum = "99";
			requestMessage.EM_SystemCreateUser = staff1.GS_Code;

			var logger = new LoggingInformation();
			var packer = new ILMessagePacker();
			var requestInterchange = factory.New<ILEDIInterchange>();
			packer.Pack(requestMessage, requestInterchange, logger);
			requestMessage.Interchange.EI_SessionGUID = Guid.NewGuid();

			var requestMessage2 = GetRequestMessage(shipment);
			requestMessage2.EM_MessageNum = "100";
			requestMessage2.EM_SystemCreateUser = staff2.GS_Code;
			var requestInterchange2 = factory.New<ILEDIInterchange>();
			packer.Pack(requestMessage, requestInterchange2, logger);
			requestMessage.Interchange.EI_SessionGUID = Guid.NewGuid();

			var responseMessage = GetMessage(false);
			responseMessage.EM_LinkedObject = shipment;

			var responseInterchange = factory.New<ILEDIInterchange>();
			packer.Pack(responseMessage, responseInterchange, logger);
			responseMessage.Interchange.EI_SessionGUID = requestMessage.Interchange.EI_SessionGUID;
			factory.Save();

			const string sendEmailStaffMember = "ESM";
			SetupRegistryData(sendEmailStaffMember, group.PK, false);
			AssertSuccessMail(shipment, responseMessage, staff1);
		}

		public void TestEmailGeneration()
		{
			var factory = Factory;
			var group = factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z1";
			staff1.GS_LoginName = "Z1";
			staff1.GS_EmailAddress = "test@test.mail.com";
			factory.Save();

			var requestMessage = GetRequestMessage(shipment);
			requestMessage.EM_MessageNum = "99";
			requestMessage.EM_SystemCreateUser = staff1.GS_Code;

			var responseMessage = GetMessage(false);
			responseMessage.EM_LinkedObject = shipment;

			Factory.Save();

			const string sendEmailStaffMember = "ESM";
			SetupRegistryData(sendEmailStaffMember, group.PK, false);
			AssertSuccessMail(shipment, responseMessage, staff1);
			SetupRegistryData(sendEmailStaffMember, group.PK, true);
			AssertNullMail(shipment, responseMessage);

			const string sendEmailNominatedGroup = "ENG";
			SetupRegistryData(sendEmailNominatedGroup, group.PK, false);
			AssertSuccessMail(shipment, responseMessage, staff1);
			SetupRegistryData(sendEmailNominatedGroup, group.PK, true);
			AssertNullMail(shipment, responseMessage);

			const string sendEmailStaffMemberandNominatedGroup = "ESG";
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, false);
			AssertSuccessMail(shipment, responseMessage, staff1);
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, true);
			AssertNullMail(shipment, responseMessage);

			const string sendEmailStaffMemberOrNominatedGroupForGroup = "EOG";
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, false);
			AssertSuccessMail(shipment, responseMessage, staff1);
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, true);
			AssertNullMail(shipment, responseMessage);

			const string sendNoEmail = "NOE";
			SetupRegistryData(sendNoEmail, group.PK, false);
			AssertNullMail(shipment, responseMessage);
			SetupRegistryData(sendNoEmail, group.PK, true);
			AssertNullMail(shipment, responseMessage);
		}

		protected override Message1035Processor CreateProcessor(LoggingInformation loggingInformation) => new Message1035Processor(new LoggingInformation());

		protected override string ExpectedMessageFriendlyName => "IL Gate Pass Movement Response Message";

		protected override string ExpectedMessageTypesToInclude => "GPM";

		protected override string ExpectedMessageSubTypesToInclude => ZString.Empty;

		protected override string BasicSuccessfulMessageText => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatePassMovementResponse_1035.xml"));

		protected override string NoReferenceMessageText => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassFeedbackMessage_WithoutReference_Interchange.xml"));

		protected override string WithdrawCancelMessageText => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatePassMovementResponse_1035_WithdrawlClearing.xml"));

		protected override string NumberType => "GMN";

		protected override string CouldNotLocateMessage => "Could not locate Shipment by Gate Pass Number #";

		protected override string MoreThanOneMessage => "Found more than one shipment with the same Gate Pass Number – please check";

		protected override string EntryNum => "38788";

		protected override string DocumentName => ILMessageEventParameter.GatePassMovementDocumentName;

		protected override string GetExpectedCustomsEntryStatusLogReferenceFreeText => "38788 Gatepass Approved By Source Or TargetSite";

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("EN", "English");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType("C1557", "IL GatePass Movement Status", Core.Constants.CountryCodes.Israel);
			var refCusCodeListC1557_2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILGatePassMovementStatus, "2", "תקין - התקבלה בקשה תקינה", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(refCusCodeListC1557_2, "EN", "Correct");

			helper.CreateNewOrGetExistingCusCodeType("C1589", "IL Gate Pass Returned Code", Core.Constants.CountryCodes.Israel);
			var refCusCodeListC1589_10 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILGatePassReturnedCode, "10", "ביטול בקשה אוטומטי", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(refCusCodeListC1589_10, "EN", "Gatepass Automaticly Rejected");
			var refCusCodeListC1589_2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILGatePassReturnedCode, "2", "אישור בקשת העברה ע\"י אתר מקור/יעד", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(refCusCodeListC1589_2, "EN", "Gatepass Approved By Source Or TargetSite");

			factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableActionILGPMGroupNotification?.Dispose();
		}

		protected override EDIMessage GetRequestMessage(ForwardingShipment forwardingShipment)
		{
			var message = Factory.New<ILGPM130RequestMessage>();
			message.EM_Status = "SNT";
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = forwardingShipment.PK;
			return message;
		}

		void SetupRegistryData(ZString sendMode, ZGuid sendGroupPK, ZBool sendErrorOnly)
		{
			disposableActionILGPMGroupNotification = ILCustomsDataRegistry.Instance.ILGPMGroupNotification.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(sendMode, sendGroupPK, sendErrorOnly));
		}

		void AssertSuccessMail(ForwardingShipment shipment, EDIMessage responseMessage, GlbStaff staff1)
		{
			var processor = new Message1035Processor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Shipment Message Response for " + shipment.JS_UniqueConsignRef);

			CombineAssertions(() =>
			{
				AssertNotNull(email);

				var bodyText = email.Body;
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", staff1.GS_EmailAddress, email.Recipients[0].Email);

				Assert("Contains Shipment Message Response for", bodyText.Contains("Shipment Message Response for"));
				Assert("Contains Gatepass Number", bodyText.Contains("Gatepass Number"));
				Assert("Contains Returned Code", bodyText.Contains("Returned Code"));

				AssertEquals("The message status is ProcessedOK", "PRS", responseMessage.EM_Status);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		void AssertNullMail(ForwardingShipment shipment, EDIMessage message)
		{
			var processor = new Message1035Processor(new LoggingInformation());
			processor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Shipment Message Response for " + shipment.JS_UniqueConsignRef);
			AssertNull(email);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		IDisposable disposableActionILGPMGroupNotification;
	}
}
