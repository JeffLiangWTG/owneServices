using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(EDIMessage))]
	class EDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestErrorDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "C01", "Error Description", startDate, endDate);
			Factory.Save();

			var message = Factory.New<EDIMessage>();
			AssertEquals("Error Description", message.GetErrorDescription("C01"));
		}

		public void TestIsPostArrivalMessage()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals(false, message.IsPostArrivalMessage());

			var statuses = new ZString[] { "8000", "0011", "0010" };
			var eventMessage = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now, status: UniversalEventMessageTest.CreateStatusContext(statuses[0]));
			AssertEquals(false, eventMessage.IsPostArrivalMessage());

			eventMessage = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now, status: UniversalEventMessageTest.CreateStatusContext(statuses[1]));
			AssertEquals(true, eventMessage.IsPostArrivalMessage());

			eventMessage = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now, status: UniversalEventMessageTest.CreateStatusContext(statuses[2]));
			var isPost1 = eventMessage.IsPostArrivalMessage();
			AssertEquals(true, isPost1);

			var isPost2 = eventMessage.IsPostArrivalMessage();
			AssertEquals(true, isPost2);
			AssertEquals(isPost1.GetHashCode(), isPost2.GetHashCode());

			var messageText = eventMessage.EM_MessageText;
			eventMessage.EM_MessageText = messageText + "<End/>";
			var key = string.Format("EDIMessage|{0}|IsPostArrivalMessage", eventMessage.PK);
			var cachedValue = Factory.GetCachedValue<ZBool>(key, () => { return false; });
			AssertEquals(false, cachedValue);
			Factory.ClearCachedValue<ZBool>(key);
			isPost1 = eventMessage.IsPostArrivalMessage();
			AssertEquals(true, isPost1);
		}

		public void TestGetNumberFountainNumbersAndFillInPlaceHolders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B12345678";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			var message = entry.Messages.AddNew();
			message.EM_MessageText = EDIMessage.FormKeyPlaceHolder + "Hello, " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";

			AssertEquals("LinkedObjectReference", "B12345678", message.LinkedObjectReference);

			var expectedExceptionMessage = "Client Network ID hasn't been specified. Please set it up in:\r\n" +
				BatchProcessorUtilities.RegistryLocation(CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries);
			AssertExceptionThrown(typeof(ZCannotSaveException), expectedExceptionMessage, () => Factory.Save());

			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDZZ");
			message = entry.Messages.AddNew();
			message.EM_MessageText = EDIMessage.FormKeyPlaceHolder + "Hello, " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";
			message.EM_IsTestMessage = true;
			Factory.Save();
			AssertEquals("message.EM_MessageText", declaration.JE_DeclarationReference.PadRight(11) + "Hello, 001 Bye", message.EM_MessageText);

			var nextNumber = Env.NumberFountains.EDIFACTNumberFountain("M", "CLIENTIDZZ", "CAC").PeekPreliminary(Factory);
			var message2 = entry.Messages.AddNew();
			message2.EM_MessageText = "Batch Number: " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";
			message2.EM_IsTestMessage = true;
			Factory.Save();
			AssertEquals("message2.EM_MessageText", "Batch Number: " + nextNumber.ToString("000") + " Bye", message2.EM_MessageText);
			var message3 = entry.Messages.AddNew();
			message3.EM_MessageText = "Batch Number: " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";
			Factory.Save();
			AssertEquals("message3.EM_MessageText", "Batch Number: 003 Bye", message3.EM_MessageText);
		}

		public void TestReplaceBatchNumberPlaceHolder()
		{
			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDZZ"))
			using (CACustomsDataRegistry.Instance.ShouldBatchNumberBeByInterchange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_DeclarationReference = "B12345678";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
				var message = entry.Messages.AddNew();
				message.EM_MessageText = EDIMessage.FormKeyPlaceHolder + "Hello, " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";
				message.EM_IsTestMessage = true;
				Factory.Save();
				AssertEquals("message.EM_MessageText", declaration.JE_DeclarationReference.PadRight(11) + "Hello, " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye", message.EM_MessageText);
			}
		}

		public void TestLinkedObjectReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B12345678";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S12345678";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C12345678";

			var message = declaration.CustomsEntryHeaders.AddNew().Messages.AddNew();
			AssertEquals("Declaration Linked Object Reference", declaration.JE_DeclarationReference, message.LinkedObjectReference);

			message.EM_LinkedObject = shipment;
			AssertEquals("Shipment Linked Object Reference", shipment.JS_UniqueConsignRef, message.LinkedObjectReference);

			message.EM_LinkedObject = consol;
			AssertEquals("Consol Linked Object Reference", consol.JK_UniqueConsignRef, message.LinkedObjectReference);
		}

		public void TestReplaceMessageInterpretationPlaceHolders()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			var message = Factory.New<B3Message>();
			const string messageText = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:S:99B:UN'BGM+:::AB+<<UNIQUE BATCH NUMBER PLACE HOLDER>>+9'";
			var interpretation = "Message Number: {0}, Batch number : {1}";
			message.EM_MessageText = messageText;
			message.EM_MessageInterpretation = string.Format(interpretation, EDIMessage.MessageNumberPlaceHolderHtml, EDIMessage.UniqueBatchNumberPlaceHolderHtml);
			Factory.Save();
			AssertEquals("EM_MessageInterpretation", "Message Number: 1, Batch number : 001", message.EM_MessageInterpretation);

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1234500000001";
			var message1 = Factory.New<EXPEDIMessage>();
			interpretation = "Entry Number: {0}";
			message1.EM_MessageText = messageText;
			message1.EM_MessageInterpretation = string.Format(interpretation, EDIMessage.EntryNumberPlaceHolderHtml);
			message1.EM_LinkedObject = entryHeader;
			Factory.Save();
			AssertEquals("EM_MessageInterpretation", "Entry Number: 1234500000001", message1.EM_MessageInterpretation);

			var message2 = Factory.New<QueryMessage>();
			interpretation = "Version: {0}";
			message2.EM_MessageText = messageText;
			message2.EM_MessageInterpretation = string.Format(interpretation, EDIMessage.DocumentMessageVersionPlaceHolderHtml);
			Factory.Save();
			AssertEquals("EM_MessageInterpretation", "Version: 0001", message2.EM_MessageInterpretation);
		}

		public void TestDefault()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.CACustoms, message.EM_ApplicationCode);
		}

		public void TestPropertiesFromRelatedMessages()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "~~";
			staff.GS_FullName = "Nobody Here";

			var outgoing = Factory.New<DLMMessage>();
			outgoing.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoing.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			outgoing.EM_MessageNum = "~123";
			outgoing.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			outgoing.EM_SystemCreateUser = "~~";
			outgoing.EM_MessageText = "PPERM1234";
			AssertEquals(false, outgoing.HasRelatedMessage);

			var response = Factory.New<DLMMessage>();
			response.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			response.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			response.EM_MessageNum = "~123";
			response.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddHours(1);
			response.EM_SystemCreateUser = "~BP";
			response.EM_MessageText = "RINV23432";
			AssertEquals(true, outgoing.HasRelatedMessage);

			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "Customs";
			response.EM_EI = interchange.PK;

			AssertEquals("User", "Customs", outgoing.EM_RelatedMessageCreateUser);
			AssertEquals("CreateTime", ZDateTime.BrettsBirthday.AddHours(1), outgoing.EM_RelatedMessageCreateTime);
			AssertMultilineASCIIEquals("EM_RelatedMessageFormattedMessageText", outgoing.EM_RelatedMessageFormattedMessageText, @"RINV23432");
			AssertMultilineASCIIEquals("EM_MessageInterpretation", outgoing.EM_RelatedMessageInterpretationText, @"INBOUND MESSAGE");

			AssertEquals("User", "Nobody Here", response.EM_RelatedMessageCreateUser);
			AssertEquals("CreateTime", ZDateTime.BrettsBirthday, response.EM_RelatedMessageCreateTime);
			AssertMultilineASCIIEquals("EM_RelatedMessageFormattedMessageText", response.EM_RelatedMessageFormattedMessageText, @"PPERM1234");
			AssertMultilineASCIIEquals("EM_RelatedMessageInterpretationText", response.EM_RelatedMessageInterpretationText, @"------DLMPermit-------------------------
 Permit Number (2-36) :PERM1234");
		}

		public void TestResponseMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertNoExceptionThrown(delegate
			{ var accessed = message.ResponseMessage; });
			AssertNull(message.ResponseMessage);

			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			AssertNoExceptionThrown(delegate
			{ var accessed = message.ResponseMessage; });

			var message1 = Factory.New<EDIMessage>();
			message1.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message1.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			message1.EM_MessageNum = "~123";

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			message2.EM_MessageNum = "~123";

			var message3 = Factory.New<EDIMessage>();
			message3.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message3.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			message3.EM_MessageNum = "~123";

			AssertEquals(message2, message1.ResponseMessage);
		}

		public void TestOriginalMessageWhenMessageNumIsEmpty()
		{
			var incoming1 = Factory.New<EDIMessage>();
			incoming1.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incoming1.EM_MessageNum = "";

			var incoming2 = Factory.New<EDIMessage>();
			incoming2.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incoming2.EM_MessageNum = "";

			AssertNull(incoming1.OriginalMessage);
			AssertNull(incoming2.OriginalMessage);
		}

		public void TestFormattedMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "HELLO WORLD";
			AssertEquals("HELLO WORLD", message.EM_FormattedMessageText);
		}

		public void TestDefaultValues()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals(EDIInterchange.ApplicationCodes.CACustoms, message.EM_ApplicationCode);
		}

		public void TestEM_MessageInterpretation()
		{
			AssertEquals("Message.EM_MessageInterpretation", "UNKNOWN MESSAGE DIRECTION", GetMessage(ZString.Empty).EM_MessageInterpretation);
			AssertEquals("Message.EM_MessageInterpretation", "INBOUND MESSAGE", GetMessage(EDIInterchange.Direction.Receive).EM_MessageInterpretation);
			AssertEquals("Message.EM_MessageInterpretation", "OUTBOUND MESSAGE", GetMessage(EDIInterchange.Direction.Transmit).EM_MessageInterpretation);

			const string htmlInterpretation = "HTML INTERPRETATION";
			var message = GetMessage(ZString.Empty);
			message.EM_MessageInterpretation = htmlInterpretation;
			AssertEquals("Message.Notes.HasNotes", true, message.Notes.HasNotes);
			AssertEquals("Message.EM_MessageInterpretation", "UNKNOWN MESSAGE DIRECTION", message.EM_MessageInterpretation);
			message = GetMessage(EDIInterchange.Direction.Receive);
			message.EM_MessageInterpretation = htmlInterpretation;
			AssertEquals("Message.EM_MessageInterpretation", htmlInterpretation, message.EM_MessageInterpretation);
			message = GetMessage(EDIInterchange.Direction.Transmit);
			message.EM_MessageInterpretation = htmlInterpretation;
			AssertEquals("Message.EM_MessageInterpretation", htmlInterpretation, message.EM_MessageInterpretation);

			message.EM_MessageInterpretation = ZString.Empty;
			AssertEquals("Message.Notes.HasNotes", false, message.Notes.HasNotes);
		}

		public void TestOriginalMessage()
		{
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{
				var message = Factory.New<EDIMessage>();
				var exceptionMessage = message.OriginalMessage;
			});

			var originalMessage = Factory.New<EDIMessage>();
			originalMessage.EM_MessageNum = "4";
			originalMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;

			var responseMessage = Factory.New<EDIMessage>();
			responseMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			responseMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseMessage.EM_MessageNum = "4";
			AssertEquals(originalMessage, responseMessage.OriginalMessage);
		}

		public void TestResetToQueuedStatusDoesNotResetMessageType()
		{
			var message = Factory.New<EDIReleaseMessage>();
			AssertEquals("Pre-condition: EM_MessageType", MessageTypeList.Codes.EDIRelease, message.EM_MessageType);
			message.ResetToQueuedStatus();
			AssertEquals("EM_MessageType", MessageTypeList.Codes.EDIRelease, message.EM_MessageType);
		}

		public void TestMessageSubTypeDescription()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			AssertEquals("EM_MessageSubTypeDescription", MessageSubTypeCodes.Descriptions.Original, message.EM_MessageSubTypeDescription);
		}

		public void TestMessageScheduleTypeDescription()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			AssertEquals("MessageScheduleDescription", string.Empty, message.MessageScheduleDescription);
		}

		public void TestReportErrorWhenApplicationCodeIsAMS()
		{
			try
			{
				var message = Factory.New<EDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.AMS;
				message.EM_MessageNum = "TEST123456";
				Factory.Save();
				AssertEquals("ErrorReporter.LastKeyReported", "IncorrectNumberFountainInvokedForMessageAMS", ErrorReporter.LastKeyReported);
				AssertEquals("message.EM_MessageNum", "TEST123456", message.EM_MessageNum);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return GetMessage(EDIInterchange.Direction.Transmit);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		EDIMessage GetMessage(ZString receiveTransmit)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "HELLO WORLD";
			message.EM_ReceiveTransmit = receiveTransmit;
			return message;
		}

		#endregion
	}
}
