using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	[TestsSubclassesOf(typeof(AFREventProcessor))]
	abstract class AFREventProcessorAbstractTest<T> : TestCaseWithFactoryAndMessagingHelpers
		where T : AFREventProcessor
	{
		protected abstract T GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory);

		public void TestBannerAndStyleAndFooterComeFromJobBranch()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.SaveForTesting();

			using (SystemDataRegistry.Instance.HtmlEmailBannerImage.SetTemporaryValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, new Bitmap(1, 2)))
			using (SystemDataRegistry.Instance.HtmlEmailFooterImage.SetTemporaryValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, new Bitmap(2, 1)))
			using (SystemDataRegistry.Instance.HtmlEmailStyleSheet.SetTemporaryValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, "HtmlEmailStyleSheet Bla Bla"))
			{
				var logger = new TestErrorLogger();
				var eventDataObject = new UniversalEvent
				{
					ContextCollection = new List<Context>()
					{
						CreateContext("HBOLNumber", "HB3242"),
						CreateContext("NotificationDetails", "HELLO WORLD")
					}
				};
				var processor = GetNewProcessor(eventDataObject, logger, Factory.BOFactory);
				var header = Factory.New<JPAFRHeader>();
				header.JPH_JobReference = "AFR23423";
				header.JPH_GB_Branch = newBranch.PK;
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				processor.Process(header);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains("HtmlEmailStyleSheet Bla Bla", email.Body);
				var banner = email.Attachments.Cast<AttachmentDef>().Single(x => x.DisplayName == "Banner.jpg");
				var footer = email.Attachments.Cast<AttachmentDef>().Single(x => x.DisplayName == "Footer.jpg");
				var bannerImage = new Bitmap(new MemoryStream(banner.Data));
				var footerImage = new Bitmap(new MemoryStream(footer.Data));
				AssertEquals(1, bannerImage.Width);
				AssertEquals(2, footerImage.Width);
			}
		}

		public void TestEventProcessorEmailsPostMasterGroupOnErrorWhenMissingRecipients()
		{
			afrGroup.Staff.RemoveAndDeleteAll();
			var postMaster = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
			postMaster.GS_EmailAddress = ZString.Empty;

			JPAFRRegistry.Instance.SendMessageAcknowledgements.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, null);
			JPAFRRegistry.Instance.SendMessageErrors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, null);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEvent();
			eventDataObject.ContextCollection = new List<Context>()
			{
				CreateContext("HBOLNumber", "HB3242"),
				CreateContext("NotificationDetails", "HELLO WORLD")
			};

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			eventDataObject.EventReference = "-ACCEPTED";
			ProcessDataObject(eventDataObject, new TestErrorLogger(), "AFR23423");
			AssertEquals("Should not send email on accepted.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			if (ExpectEmailToPostmasterOnError)
			{
				eventDataObject.EventReference = "-ERROR";
				var logger = new TestErrorLogger();
				ProcessDataObject(eventDataObject, logger, "AFR23424");
				AssertEquals("Should not send email on error when postmaster email not set.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertContains("creates log entry", "This email could not be delivered to the Post Masters Group as it is either empty or no members have an email address specified.", logger.Logs);
				AssertContains("log entry has subject", "Response (Failure) for AFR23424') This email could not be delivered to the requested group", logger.Logs);

				postMaster.GS_EmailAddress = "unit.test@wisetechglobal.com";
				Factory.SaveForTesting();
				ProcessDataObject(eventDataObject, new TestErrorLogger(), "AFR23425");
				AssertEquals("Should send email on error as postmaster email is set.", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("Recipient is postmaster", "unit.test@wisetechglobal.com", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
			}
		}

		void ProcessDataObject(UniversalEvent eventDataObject, IXmlImportLogger logger, ZString jobReference)
		{
			var processor = GetNewProcessor(eventDataObject, logger, Factory.BOFactory);
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = jobReference;
			processor.Process(header);
		}

		protected virtual bool ExpectEmailToPostmasterOnError => true;

		protected RefVessel Vessel1
		{
			get
			{
				if (vessel1 == null)
				{
					vessel1 = Factory.New<RefVessel>();
					vessel1.RV_Code = "VESSEL TEST 1";
					vessel1.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Singapore;
					vessel1.RV_LloydsNumber = "8732342";
					vessel1.RV_RadioCallSign = "CALLME";
				}
				return vessel1;
			}
		}

		RefVessel vessel1;

		protected GlbStaff Staff1
		{
			get { return staff1 ?? (staff1 = CreateStaff("B1!", "BOB", "BOB THE BUILDER", "BOB@WHERE.COM")); }
		}
		GlbStaff staff1;

		protected GlbStaff Staff2
		{
			get { return staff2 ?? (staff2 = CreateStaff("W1!", "WENDY", "WENDY THE DESTROYER", "WENDY@WHERE.COM")); }
		}
		GlbStaff staff2;

		protected GlbStaff Staff3
		{
			get { return staff3 ?? (staff3 = CreateStaff("J1!", "JOHN", "JOHN THE PEACEMARKER", "JOHN@WHERE.COM")); }
		}
		GlbStaff staff3;

		protected GlbGroup afrGroup;

		protected GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			return staff;
		}

		protected override void SetUp()
		{
			base.SetUp();
			afrGroup = Factory.New<GlbGroup>();
			afrGroup.GG_Code = "G!1";
			afrGroup.GG_Desc = "AFR GROUP";
			afrGroup.Staff.Add(Staff1);
			Factory.SaveForTesting();
			JPAFRRegistry.Instance.SendMessageAcknowledgementsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, afrGroup.PK.ToGuid());
			JPAFRRegistry.Instance.SendMessageAcknowledgements.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EmailTo.StaffMemberAndNominatedGroup);

			JPAFRRegistry.Instance.SendMessageErrorsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, afrGroup.PK.ToGuid());
			JPAFRRegistry.Instance.SendMessageErrors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
		}

		protected void AssertHasEmail(string subject, string bodyText, params ZString[] recipientEmails)
		{
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject == subject);
			AssertContains(bodyText, email.Body);
			if (recipientEmails != null)
			{
				foreach (var recipientEmail in recipientEmails)
				{
					AssertEquals(recipientEmail, true, email.Recipients.Contains(recipientEmail));
				}
			}
		}

		protected void SetupOriginalMessageAndSave(JPAFRHeader header, ZString messageType)
		{
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_From = AFRMessageGenerator.EnterpriseSenderID;
			interchange.EI_To = Enterprise.Customs.JP.AFR.Business.Constants.JapanCustomsReceipientID;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_GB = header.JPH_GB_Branch;
			var message1 = interchange.ContainedMessages.AddNew(typeof(XmlEDIMessage));
			message1.FillWithValidTestData();
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message1.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message1.EM_Status = EDIMessageStatusList.Codes.Sent;
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message1.EM_MessageOwner = messageType;
			message1.EM_GB = header.JPH_GB_Branch;
			message1.EM_SystemCreateUser = Staff2.GS_Code;
			header.Messages.Add(message1);
			Factory.SaveForTesting();
			var message2 = interchange.ContainedMessages.AddNew(typeof(XmlEDIMessage));
			message2.FillWithValidTestData();
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message2.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message2.EM_Status = EDIMessageStatusList.Codes.Sent;
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message2.EM_MessageOwner = messageType;
			message2.EM_GB = header.JPH_GB_Branch;
			message2.EM_SystemCreateUser = Staff3.GS_Code;
			header.Messages.Add(message2);
			Factory.SaveForTesting();
		}

		protected Context CreateContext(string type, string value)
		{
			return new Context() { Type = type, Value = value };
		}

		protected AFRHeaderDataEventParentFinder GetNewEventParentFinder(TestErrorLogger logger = null)
		{
			return GetNewEventParentFinder(Factory.BOFactory, logger);
		}

		protected AFRHeaderDataEventParentFinder GetNewEventParentFinder(BusinessObjectFactory factory, TestErrorLogger logger = null)
		{
			return AFREventParentFinderHelper.GetNewEventParentFinder(factory, logger);
		}

		protected string GetHumanReadableID(BusinessObject businessObject)
		{
			return AFREventParentFinderHelper.GetHumanReadableID(businessObject);
		}
	}

	[TestedType(typeof(AFREventProcessorForTest))]
	sealed class AFREventProcessorBaseOnlyTest : AFREventProcessorAbstractTest<AFREventProcessorForTest>
	{
		protected override AFREventProcessorForTest GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new AFREventProcessorForTest(eventDataObject, logger, factory);
		}

		public void TestGetLastTransmitMessage()
		{
			var header = Factory.New<JPAFRHeader>();
			var testInterchange1 = Factory.New<EDIInterchange>();
			testInterchange1.EI_From = Constants.JapanCustomsReceipientID;
			testInterchange1.EI_To = Constants.JapanCustomsReceipientID;
			var testInterchange2 = Factory.New<EDIInterchange>();
			testInterchange2.EI_From = Constants.JapanCustomsReceipientID;
			testInterchange2.EI_To = Constants.JapanCustomsReceipientID;
			var testMessage1 = header.AFRMessages.AddNew();
			testMessage1.FillWithValidTestData();
			testMessage1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			testMessage1.EM_MessageOwner = MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse;
			testMessage1.EM_SystemCreateUser = Staff1.GS_Code;
			testMessage1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testMessage1.EM_EI = testInterchange1.PK;
			testMessage1.EM_LinkTable = "JPH";
			testMessage1.EM_LinkUniqueID = header.PK;
			testMessage1.EM_ApplicationCode = "UDM";
			Factory.SaveForTesting();
			var testMessage2 = header.AFRMessages.AddNew();
			testMessage2.FillWithValidTestData();
			testMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			testMessage1.EM_MessageOwner = MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion;
			testMessage2.EM_SystemCreateUser = User.ServiceUserCode;
			testMessage2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testMessage2.EM_EI = testInterchange2.PK;
			testMessage2.EM_LinkTable = "JPH";
			testMessage2.EM_LinkUniqueID = header.PK;
			testMessage2.EM_ApplicationCode = "UDM";
			Factory.SaveForTesting();
			AssertNotEquals(testMessage2.EM_MessageNum, testMessage1.EM_MessageNum);
			AssertEquals(testMessage2.EM_MessageNum, header.Messages.OfType<XmlEDIMessage>().Where(x => x.IsAFRTransmitMessage()).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault().EM_MessageNum);
			AssertEquals(testMessage1.EM_MessageNum, (new AFREventProcessorForTest(Factory.BOFactory)).GetLastTransmitMessage(header).EM_MessageNum);
		}

		public void TestGetEmailAddressToSendTo()
		{
			var header = Factory.New<JPAFRHeader>();
			var testInterchange1 = Factory.New<EDIInterchange>();
			testInterchange1.EI_From = Constants.JapanCustomsReceipientID;
			testInterchange1.EI_To = Constants.JapanCustomsReceipientID;
			var testInterchange2 = Factory.New<EDIInterchange>();
			testInterchange2.EI_From = Constants.JapanCustomsReceipientID;
			testInterchange2.EI_To = Constants.JapanCustomsReceipientID;
			var testMessage1 = header.AFRMessages.AddNew();
			testMessage1.FillWithValidTestData();
			testMessage1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			testMessage1.EM_MessageOwner = MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse;
			testMessage1.EM_SystemCreateUser = Staff2.GS_Code;
			testMessage1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testMessage1.EM_EI = testInterchange1.PK;
			testMessage1.EM_LinkTable = "JPH";
			testMessage1.EM_LinkUniqueID = header.PK;
			testMessage1.EM_ApplicationCode = "UDM";
			Factory.SaveForTesting();
			var testMessage2 = header.AFRMessages.AddNew();
			testMessage2.FillWithValidTestData();
			testMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			testMessage1.EM_MessageOwner = MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion;
			testMessage2.EM_SystemCreateUser = User.ServiceUserCode;
			testMessage2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testMessage2.EM_EI = testInterchange2.PK;
			testMessage2.EM_LinkTable = "JPH";
			testMessage2.EM_LinkUniqueID = header.PK;
			testMessage2.EM_ApplicationCode = "UDM";
			Factory.SaveForTesting();

			AssertNotEquals(testMessage2.EM_MessageNum, testMessage1.EM_MessageNum);
			var testEvent = new UniversalEvent();
			testEvent.ContextCollection = new List<Context>()
			{
				CreateContext("InternalTranscationNumber", testMessage1.EM_MessageNum)
			};
			var eventProcessor = new AFREventProcessorForTest(Factory.BOFactory, testEvent);
			AssertEquals(Staff2.GS_EmailAddress, eventProcessor.GetEmailAddressToSendTo(header));

			testEvent = new UniversalEvent();
			testEvent.ContextCollection = new List<Context>()
			{
				CreateContext("InternalTranscationNumber", testMessage2.EM_MessageNum)
			};
			eventProcessor = new AFREventProcessorForTest(Factory.BOFactory, testEvent);
			AssertEquals(Staff2.GS_EmailAddress, eventProcessor.GetEmailAddressToSendTo(header));

			testEvent = new UniversalEvent();
			testEvent.ContextCollection = new List<Context>()
			{
				CreateContext("InternalTranscationNumber", testMessage2.EM_MessageNum)
			};
			eventProcessor = new AFREventProcessorForTest(Factory.BOFactory, testEvent);
			AssertEquals(string.Empty, eventProcessor.GetEmailAddressToSendTo(Factory.New<JPAFRHeader>()));
		}

		public void TestGetEmailAddressToSendToWithSystemUser()
		{
			CreateStaff("SU1", "SU", "System User", "").GS_IsSystemAccount = true;
			CreateStaff("JSH", "JohnSmith", "John Smith", "join.smith@company.com");
			CreateStaff("HPR", "HarryPotter", "Harry Potter", "harry.potter@hogwarts.edu.uk");
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = Constants.JapanCustomsReceipientID;
			interchange.EI_To = Constants.JapanCustomsReceipientID;
			var header = Factory.New<JPAFRHeader>();
			Factory.SaveForTesting();

			var receivedMessage = header.AFRMessages.AddNew();
			receivedMessage.FillWithValidTestData();
			receivedMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			receivedMessage.EM_EI = interchange.PK;
			receivedMessage.EM_ApplicationCode = "UDM";
			receivedMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			receivedMessage.EM_SystemCreateUser = "JSH";
			receivedMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 7, 6, 11, 11, 01);
			receivedMessage.EM_LinkedObject = header;
			var messageCreatedBySystemUser = header.AFRMessages.AddNew();
			messageCreatedBySystemUser.FillWithValidTestData();
			messageCreatedBySystemUser.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			messageCreatedBySystemUser.EM_EI = interchange.PK;
			messageCreatedBySystemUser.EM_ApplicationCode = "UDM";
			messageCreatedBySystemUser.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			messageCreatedBySystemUser.EM_SystemCreateUser = "SU1";
			messageCreatedBySystemUser.EM_SystemCreateTimeUtc = new ZDateTime(2020, 7, 6, 11, 11, 02);
			messageCreatedBySystemUser.EM_LinkedObject = header;
			var earlierMessage = header.AFRMessages.AddNew();
			earlierMessage.FillWithValidTestData();
			earlierMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			earlierMessage.EM_EI = interchange.PK;
			earlierMessage.EM_ApplicationCode = "UDM";
			earlierMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			earlierMessage.EM_SystemCreateUser = "JSH";
			earlierMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 7, 6, 11, 11, 03);
			earlierMessage.EM_LinkedObject = header;
			var laterMessage = header.AFRMessages.AddNew();
			laterMessage.FillWithValidTestData();
			laterMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			laterMessage.EM_EI = interchange.PK;
			laterMessage.EM_ApplicationCode = "UDM";
			laterMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			laterMessage.EM_SystemCreateUser = "HPR";
			laterMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 7, 6, 11, 11, 04);
			laterMessage.EM_LinkedObject = header;

			var @event = new UniversalEvent();
			@event.ContextCollection = new List<Context> { new Context { Type = "InternalTransactionNumber", Value = "XXXXXXX" } };
			Factory.SaveForTesting();

			var processor = new AFREventProcessorForTest(@event, new TestErrorLogger(), Factory.BOFactory);

			var emailAddress = processor.GetEmailAddressToSendTo(header);

			AssertEquals("No original message, should find: latest, TRX, created by non-system user.", "harry.potter@hogwarts.edu.uk", emailAddress);

			var originalMessageCreatedBySystemUser = header.AFRMessages.AddNew();
			originalMessageCreatedBySystemUser.FillWithValidTestData();
			originalMessageCreatedBySystemUser.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			originalMessageCreatedBySystemUser.EM_EI = interchange.PK;
			originalMessageCreatedBySystemUser.EM_ApplicationCode = "UDM";
			originalMessageCreatedBySystemUser.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalMessageCreatedBySystemUser.EM_SystemCreateUser = "SU1";
			originalMessageCreatedBySystemUser.EM_SystemCreateTimeUtc = new ZDateTime(2020, 7, 6, 11, 11, 05);
			originalMessageCreatedBySystemUser.EM_LinkedObject = header;
			Factory.SaveForTesting();

			header.Messages.Load();
			@event.ContextCollection = new List<Context> { new Context { Type = "InternalTransactionNumber", Value = originalMessageCreatedBySystemUser.EM_MessageNum } };
			processor = new AFREventProcessorForTest(@event, new TestErrorLogger(), Factory.BOFactory);
			emailAddress = processor.GetEmailAddressToSendTo(header);
			AssertEquals("Message which is original but created by system user should NOT be used.", "harry.potter@hogwarts.edu.uk", emailAddress);

			var originalMessage = header.AFRMessages.AddNew();
			originalMessage.FillWithValidTestData();
			originalMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			originalMessage.EM_EI = interchange.PK;
			originalMessage.EM_ApplicationCode = "UDM";
			originalMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalMessage.EM_SystemCreateUser = "JSH";
			originalMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 7, 6, 11, 11, 06);
			originalMessage.EM_LinkedObject = header;
			Factory.SaveForTesting();

			header.Messages.Load();
			@event.ContextCollection = new List<Context> { new Context { Type = "InternalTransactionNumber", Value = originalMessage.EM_MessageNum } };
			processor = new AFREventProcessorForTest(@event, new TestErrorLogger(), Factory.BOFactory);
			emailAddress = processor.GetEmailAddressToSendTo(header);

			AssertEquals("Message which is original and created by non-system user should be used.", "join.smith@company.com", emailAddress);
		}
	}

	class AFREventProcessorForTest : AFREventProcessor
	{
		public AFREventProcessorForTest(BusinessObjectFactory factory)
			: base(new UniversalEvent(), new TestErrorLogger(), factory)
		{
		}

		public AFREventProcessorForTest(BusinessObjectFactory factory, UniversalEvent eventForTesting)
			: base(eventForTesting, new TestErrorLogger(), factory)
		{
		}

		public AFREventProcessorForTest(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
			: base(eventDataObject, logger, factory)
		{
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return string.Empty;
		}

		internal new ZString GetEmailAddressToSendTo(JPAFRHeader header)
		{
			return base.GetEmailAddressToSendTo(header);
		}
	}
}
