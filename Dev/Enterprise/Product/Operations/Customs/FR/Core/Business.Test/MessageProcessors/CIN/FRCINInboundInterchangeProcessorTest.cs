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
	public class FRCINInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestEDIMessageCreated()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "USC";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = Factory.New<GlbBranch>();
			usBranch.GB_Code = "USB";
			usBranch.GB_GC = usCompany.PK;

			var cust = Factory.NewWithValidTestData<OrgHeader>();
			cust.OH_Code = "UNITTEST";

			var jobHeader = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeFRC);
			jobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.SJH_JobReference = "UNITTEST";
			jobHeader.SJH_OH_Customer = cust.PK;

			var dec = jobHeader.CusTempStorageDec;
			var line = dec.CusTempStorageLines.AddNew();
			line.TSL_ReferenceNumber = "CIN-REF001";

			var message = Factory.New<CINImportResponseFREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.CIN;
			message.EM_MessageText = "Not Important at this point";
			message.MessageNumberStrategy = new FRMessageNumberStrategy(jobHeader.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);

			dec.Messages.Add(message);

			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "FRCUS";
			intchg.EI_To = "TEST";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg.EI_InterchangeNum = "0000000000000000001";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = "FR Branch will not be processed";
			Factory.Save();

			var intchg1 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg1.EI_From = "FRCUS";
			intchg1.EI_To = "TEST";
			intchg1.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg1.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg1.EI_InterchangeNum = "0000000000000000002";
			intchg1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg1.EI_Status = EDIInterchange.Status.Queued;
			intchg1.EI_IsActive = true;
			intchg1.EI_GB = usBranch.PK;
			intchg1.EI_BodyText = "US Branch Wont be processed";
			Factory.Save();

			var intchg2 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg2.EI_From = "FRCUS";
			intchg2.EI_To = "TEST";
			intchg2.EI_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			intchg2.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg2.EI_InterchangeNum = "0000000000000000003";
			intchg2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg2.EI_Status = EDIInterchange.Status.Queued;
			intchg2.EI_IsActive = true;
			intchg2.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg2.EI_BodyText = "EI_ApplicationCode FRC Wont be processed";
			Factory.Save();

			var intchg3 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg3.EI_From = "FRCUS";
			intchg3.EI_To = "TEST";
			intchg3.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg3.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAtlasSystem;
			intchg3.EI_InterchangeNum = "0000000000000000004";
			intchg3.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg3.EI_Status = EDIInterchange.Status.Queued;
			intchg3.EI_IsActive = true;
			intchg3.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg3.EI_BodyText = "EI_InterchangeType ATS Wont be processed";
			Factory.Save();

			var intchg4 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg4.EI_From = "FRCUS";
			intchg4.EI_To = "TEST";
			intchg4.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg4.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg4.EI_InterchangeNum = "0000000000000000005";
			intchg4.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			intchg4.EI_Status = EDIInterchange.Status.Queued;
			intchg4.EI_IsActive = true;
			intchg4.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg4.EI_BodyText = "EI_ReceiveTransmit Transmit Wont be processed";
			Factory.Save();

			var intchg5 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg5.EI_From = "FRCUS";
			intchg5.EI_To = "TEST";
			intchg5.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg5.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg5.EI_InterchangeNum = "0000000000000000006";
			intchg5.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg5.EI_Status = EDIInterchange.Status.Received;
			intchg5.EI_IsActive = true;
			intchg5.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg5.EI_BodyText = "EI_Status Received Wont be processed";
			Factory.Save();

			var interchangeMessageText = string.Format(CultureInfo.InvariantCulture, @"<CinMessage type=""WarehouseMovement-In"">
<Header from=""CIN"" to=""PUT-CIN-ID-HERE"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""{0}"" />
<WarehouseMovementInResponse>Expecting some kind of response in this format but that has not been defined as yet</WarehouseMovementInResponse>
</CinMessage>", message.EM_MessageNum);

			var intchg6 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg6.EI_From = "FRCUS";
			intchg6.EI_To = "TEST";
			intchg6.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg6.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsCIN;
			intchg6.EI_InterchangeNum = "0000000000000000007";
			intchg6.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg6.EI_Status = EDIInterchange.Status.Queued;
			intchg6.EI_IsActive = true;
			intchg6.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg6.EI_BodyText = interchangeMessageText;

			Factory.Save();

			var processor = new FRCINImportIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();
			intchg1.Reload();
			intchg2.Reload();
			intchg3.Reload();
			intchg4.Reload();
			intchg5.Reload();
			intchg6.Reload();

			AssertEquals(EDIInterchange.Status.Queued, intchg.EI_Status);
			AssertEquals(EDIInterchange.Status.Queued, intchg1.EI_Status);
			AssertEquals(EDIInterchange.Status.Queued, intchg2.EI_Status);
			AssertEquals(EDIInterchange.Status.Queued, intchg3.EI_Status);
			AssertEquals(EDIInterchange.Status.Queued, intchg4.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg5.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg6.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			var msg2 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg2.PK));
			var msg3 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg3.PK));
			var msg4 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg4.PK));
			var msg5 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg5.PK));
			var msg6 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg6.PK));

			AssertEquals(0, msg.Length);
			AssertEquals(0, msg1.Length);
			AssertEquals(0, msg2.Length);
			AssertEquals(0, msg3.Length);
			AssertEquals(0, msg4.Length);
			AssertEquals(0, msg5.Length);
			AssertEquals(1, msg6.Length);

			AssertEquals(1, intchg6.ContainedMessages.Count);

			AssertEquals(msg6[0].PK, intchg6.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg6.PK, msg6[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg6[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.CIN, msg6[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.CIN, msg6[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg6[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "0000000000000000007", msg6[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg6[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg6[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg6[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", interchangeMessageText, msg6[0].EM_MessageText);
			AssertEquals("EM_LinkTable", "CusTempStorageDec", msg6[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", dec.PK, msg6[0].EM_LinkUniqueID);
		}
	}
}
