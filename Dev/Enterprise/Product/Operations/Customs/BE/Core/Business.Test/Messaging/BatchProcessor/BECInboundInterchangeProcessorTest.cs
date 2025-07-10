using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BE.Business.Testing;

class BECInboundInterchangeProcessorTest : DtbBookingSharedTestCaseWithFactory
{
	public void TestIsInterchangeNotDeleted()
	{
		CombineAssertions(() =>
		{
			var processor = new BECInboundInterchangeProcessor();
			var interchange = Factory.New<EDIInterchange>();
			AssertEquals("Not ERR", true, processor.IsInterchangeNotDeleted(interchange));
			interchange.EI_Status = EDIInterchange.Status.Error;
			AssertEquals("ERR", false, processor.IsInterchangeNotDeleted(interchange));
		});
	}

	public void TestCreateMessagesFromInterchange()
	{
		var bodyText = @"<?xml version=""1.0"" encoding=""UTF-8""?><CC019C><messageSender>CW1</messageSender><messageRecipient>NCTS.BE</messageRecipient><preparationDateAndTime>2023-07-25T12:01:00</preparationDateAndTime>"
						+ "<messageIdentification>1</messageIdentification><messageType>CC019C</messageType><correlationIdentifier>2204528148060000000001</correlationIdentifier></CC019C>";
		var processedInterchange = CreateAndProcessInterchange(SendMessageTypes.Codes.NCT, bodyText);
		var ediMessage = processedInterchange.ContainedMessages[0];

		CombineAssertions(() =>
		{
			AssertEquals("MessageType", ediMessage.EM_MessageType, processedInterchange.EI_InterchangeType);
			AssertEquals("MessageSubType", ediMessage.EM_MessageSubType, "019");
			AssertEquals("ReceiveTransmit is Receive", ediMessage.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
			AssertEquals("Status is Queued", ediMessage.EM_Status, EDIMessageStatusList.Codes.Queued);
			AssertEquals("MessageText", ediMessage.EM_MessageText, bodyText);
			AssertEquals("MessageNum", ediMessage.EM_MessageNum, processedInterchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
		});
	}

	public void TestCreateUniversalErrorMessagesFromInterchange()
	{
		var bodyText = UniversalEventTestDataHelper.CreateUniversalInterchangeXml(Events.InterchangeRejectedCode, "XER", "TEST", "The MRN 21ES00999930MXRZN5 exceeded the retry count of 5 and still receives no update");
		var expectedBodyText = string.Format(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0""><Event><EventTime>2022-11-08T07:08:36</EventTime><EventType>IRJ</EventType><EventParameters><MessageType>XER</MessageType><Type>TEST</Type><Reason>The MRN 21ES00999930MXRZN5 exceeded the retry count of 5 and still receives no update</Reason></EventParameters><ContextCollection><Context><Type>ResponseMessage</Type><Value><![CDATA[]]></Value></Context></ContextCollection></Event></UniversalEvent>");

		var processedInterchange = CreateAndProcessInterchange("XER", bodyText, true);
		var ediMessage = processedInterchange.ContainedMessages[0];

		CombineAssertions(() =>
		{
			AssertNotEquals("MessageType", processedInterchange.EI_InterchangeType, ediMessage.EM_MessageType);
			AssertEquals("MessageType", SendMessageTypes.Codes.AES, ediMessage.EM_MessageType);
			AssertType<CusEntryHeader>(ediMessage.EM_LinkedObject);
			AssertEquals("MessageSubType", ediMessage.EM_MessageSubType, "UER");
			AssertEquals("ReceiveTransmit is Receive", ediMessage.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
			AssertEquals("Status is Queued", ediMessage.EM_Status, EDIMessageStatusList.Codes.Queued);
			AssertEquals("MessageText", expectedBodyText, ediMessage.EM_MessageText);
			AssertEquals("MessageNum", ediMessage.EM_MessageNum, processedInterchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
		});
	}

	public void TestWrongInterchange()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ZACustoms;
		interchange.EI_From = "TestFrom";
		interchange.EI_To = "TestTo";
		Factory.Save();
		var processor = new BECInboundInterchangeProcessor();
		processor.ExecuteBatch();
		interchange.Reload();

		AssertEquals("No contained messages", 0, interchange.ContainedMessages.Count);
	}

	public void TestInterchangeWithEmptyXml()
	{
		var processedInterchange = CreateAndProcessInterchange(SendMessageTypes.Codes.NCT, "");

		CombineAssertions(() =>
		{
			AssertEquals("Interchange status is Error", EDIInterchange.Status.Error, processedInterchange.EI_Status);
			AssertEquals("No contained messages", 0, processedInterchange.ContainedMessages.Count);
			Assert("Log for Empty Xml",
				processedInterchange.Logs.HasLogWith(log => log.SL_Reference.Contains("The Message XML is Empty")));
		});
	}

	public void TestInterchangeWithInvalidXml()
	{
		var processedInterchange = CreateAndProcessInterchange(SendMessageTypes.Codes.NCT, "INVALID XML TEXT");

		CombineAssertions(() =>
		{
			AssertEquals("Interchange status is Error", EDIInterchange.Status.Error, processedInterchange.EI_Status);
			AssertEquals("No contained messages", 0, processedInterchange.ContainedMessages.Count);
			Assert("Log for invalid Xml",
				processedInterchange.Logs.HasLogWith(log => log.SL_Reference.Contains("The Message XML is not a valid XML")));
		});
	}

	public void TestInterchangeWithInvalidType()
	{
		var bodyText = "<CC015C><messageSender>CW1</messageSender><messageRecipient>NCTS.BE</messageRecipient><preparationDateAndTime>2023-07-25T12:01:00</preparationDateAndTime>"
						+ "<messageIdentification>1</messageIdentification><correlationIdentifier>2204528148060000000001</correlationIdentifier></CC015C>";
		var processedInterchange = CreateAndProcessInterchange(SendMessageTypes.Codes.NCT, bodyText);

		CombineAssertions(() =>
		{
			AssertEquals("Interchange status is Error", EDIInterchange.Status.Error, processedInterchange.EI_Status);
			AssertEquals("No contained messages", 0, processedInterchange.ContainedMessages.Count);
			Assert("Log for invalid messageType",
				processedInterchange.Logs.HasLogWith(log => log.SL_Reference.Contains("Can not determine Message Sub Type for the Received Interchange")));
		});
	}

	EDIInterchange CreateAndProcessInterchange(string messageType, string bodyText, bool createOutgoingFirst = false)
	{
		var sessionGUID = ZGuid.BrettsGuid;
		if (createOutgoingFirst)
		{
			var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var outgoingEdiMessage = Factory.New<BEMessage>();
			outgoingEdiMessage.EM_Status = "SNT";
			outgoingEdiMessage.EM_LinkTable = cusEntryHeader.TableName;
			outgoingEdiMessage.EM_LinkUniqueID = cusEntryHeader.PK;
			var interchangeOutgoing = Factory.New<BECInterchange>();
			interchangeOutgoing.EI_SessionGUID = sessionGUID;
			interchangeOutgoing.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchangeOutgoing.EI_Status = LogicalStatusList.Codes.Sent;
			interchangeOutgoing.EI_From = "Origin";
			interchangeOutgoing.EI_To = "Destination";
			outgoingEdiMessage.EM_EI = interchangeOutgoing.PK;
		}

		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_IsActive = true;
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.BECustoms;
		interchange.EI_InterchangeType = messageType;
		interchange.EI_From = "Destination";
		interchange.EI_To = "Origin";
		interchange.EI_BodyText = bodyText;
		interchange.EI_GB = GlbBranch.CurrentBranch.PK;
		interchange.EI_SessionGUID = sessionGUID;
		Factory.Save();

		var processor = new BECInboundInterchangeProcessor();
		processor.ExecuteBatch();
		var processedInterchange = interchange;
		processedInterchange.Reload();
		return processedInterchange;
	}
}
