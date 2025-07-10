using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FRCINIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEDIMessagesCreatedFromInterchanges()
		{
			var cust = Factory.NewWithValidTestData<OrgHeader>();
			cust.OH_Code = "UNITTEST";

			var jobHeader = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeFRC);
			jobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.SJH_JobReference = "UNITTEST";
			jobHeader.SJH_OH_Customer = cust.PK;

			var dec = jobHeader.CusTempStorageDec;
			var line = dec.CusTempStorageLines.AddNew();
			line.TSL_ReferenceNumber = "CIN-REF001";

			var message1 = Factory.New<EDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_Status = EDIMessage.Status.Sent;
			message1.EM_ApplicationCode = "FRC";
			message1.EM_MessageType = MessageTypeList.Codes.CIN;
			message1.EM_MessageText = "Not Important at this point";
			message1.MessageNumberStrategy = new FRMessageNumberStrategy(jobHeader.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_Status = EDIMessage.Status.Sent;
			message2.EM_ApplicationCode = "FRC";
			message2.EM_MessageType = MessageTypeList.Codes.CIN;
			message2.EM_MessageText = "Not Important at this point";
			message2.MessageNumberStrategy = new FRMessageNumberStrategy(jobHeader.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);

			dec.Messages.Add(message1);
			dec.Messages.Add(message2);

			Factory.Save();

			var interchangeMessageText = @"<CinMessage type=""WarehouseMovement-In"">
<Header from=""CIN"" to=""PUT-CIN-ID-HERE"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""{0}"" />
<WarehouseMovementInResponse>Expecting some kind of response in this format but that has not been defined as yet</WarehouseMovementInResponse>
</CinMessage>";

			var intchg1 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg1.EI_From = "FRCUS";
			intchg1.EI_To = "TEST";
			intchg1.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg1.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsCIN;
			intchg1.EI_InterchangeNum = "0000000000000000001";
			intchg1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg1.EI_Status = EDIInterchange.Status.Queued;
			intchg1.EI_IsActive = true;
			intchg1.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg1.EI_BodyText = string.Format(CultureInfo.InvariantCulture, interchangeMessageText, message1.EM_MessageNum);

			var intchg2 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg2.EI_From = "FRCUS";
			intchg2.EI_To = "TEST";
			intchg2.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg2.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsCIN;
			intchg2.EI_InterchangeNum = "0000000000000000002";
			intchg2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg2.EI_Status = EDIInterchange.Status.Queued;
			intchg2.EI_IsActive = true;
			intchg2.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg2.EI_BodyText = string.Format(CultureInfo.InvariantCulture, interchangeMessageText, message2.EM_MessageNum);

			Factory.Save();

			var processor = new FRCINImportIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg1.Reload();
			intchg2.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg1.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg2.EI_Status);

			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			var msg2 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg2.PK));
			AssertEquals(1, msg1.Length);
			AssertEquals(1, msg2.Length);

			AssertEquals(1, intchg1.ContainedMessages.Count);
			AssertEquals(msg1[0].PK, intchg1.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg1.PK, msg1[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg1[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.CIN, msg1[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.CIN, msg1[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg1[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "0000000000000000001", msg1[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg1[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg1[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg1[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", string.Format(CultureInfo.InvariantCulture, interchangeMessageText, message1.EM_MessageNum), msg1[0].EM_MessageText);
			AssertEquals("EM_LinkTable", "CusTempStorageDec", msg1[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", dec.PK, msg1[0].EM_LinkUniqueID);

			AssertEquals(1, intchg2.ContainedMessages.Count);
			AssertEquals(msg2[0].PK, intchg2.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg2.PK, msg2[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg2[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.CIN, msg2[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.CIN, msg2[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg2[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "0000000000000000002", msg2[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg2[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg2[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg2[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", string.Format(CultureInfo.InvariantCulture, interchangeMessageText, message2.EM_MessageNum), msg2[0].EM_MessageText);
			AssertEquals("EM_LinkTable", "CusTempStorageDec", msg2[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", dec.PK, msg2[0].EM_LinkUniqueID);
		}
	}
}
