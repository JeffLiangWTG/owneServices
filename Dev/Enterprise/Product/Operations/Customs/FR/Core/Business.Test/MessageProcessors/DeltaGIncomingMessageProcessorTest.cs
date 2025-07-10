using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaGIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageOrder()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var validMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			var baeMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");

			var intchg1 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg1.EI_From = "FRCUS";
			intchg1.EI_To = "TEST";
			intchg1.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg1.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg1.EI_InterchangeNum = "1.BAE.1";
			intchg1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg1.EI_Status = EDIInterchange.Status.Queued;
			intchg1.EI_IsActive = true;
			intchg1.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg1.EI_BodyText = baeMessageText;

			var intchg2 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg2.EI_From = "FRCUS";
			intchg2.EI_To = "TEST";
			intchg2.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg2.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg2.EI_InterchangeNum = "1.VLD.1";
			intchg2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg2.EI_Status = EDIInterchange.Status.Queued;
			intchg2.EI_IsActive = true;
			intchg2.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg2.EI_BodyText = validMessageText;
			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());
			Factory.Save();

			intchg1.Reload();
			intchg2.Reload();
			var msg1 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			var msg2 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg2.PK));
			AssertEquals("EM_InterchangeNumber", "1.BAE.1", msg1.EM_InterchangeNumber);
			AssertEquals("EM_InterchangeNumber", "1.VLD.1", msg2.EM_InterchangeNumber);

			entry.Reload();
			AssertEquals("100", entry.CH_EntryStatus);
			var log_CES060 = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CES").AddToFilter(StmALogSchema.SL_Reference, "060"));
			var log_CES100 = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CES").AddToFilter(StmALogSchema.SL_Reference, "100"));
			AssertEquals("Entry Status changed to 060 and then to 100 because VAL message was processed before the BAE message.", entry.PK, log_CES060.SL_Parent);
			AssertEquals("Entry Status changed to 060 and then to 100 because VAL message was processed before the BAE message.", entry.PK, log_CES100.SL_Parent);
		}

		public void TestEDIMessageCreatedCorrectlyForEachInterchange()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportErrorMessage.xml");
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
			intchg.EI_BodyText = messageText;

			var intchg1 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg1.EI_From = "FRCUS";
			intchg1.EI_To = "TEST";
			intchg1.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg1.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg1.EI_InterchangeNum = "0000000000000000002";
			intchg1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg1.EI_Status = EDIInterchange.Status.Queued;
			intchg1.EI_IsActive = true;
			intchg1.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg1.EI_BodyText = messageText;

			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();
			intchg1.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg1.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			AssertEquals(1, msg.Length);
			AssertEquals(1, msg1.Length);

			AssertEquals(1, intchg.ContainedMessages.Count);
			AssertEquals(msg[0].PK, intchg.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg.PK, msg[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.EXC, msg[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.EXC, msg[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "0000000000000000001", msg[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", messageText, msg[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg[0].EM_LinkUniqueID);

			AssertEquals(1, intchg1.ContainedMessages.Count);
			AssertEquals(msg1[0].PK, intchg1.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg1.PK, msg1[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg1[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.EXC, msg1[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.EXC, msg1[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg1[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "0000000000000000002", msg1[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg1[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg1[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg1[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", messageText, msg1[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg1[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg1[0].EM_LinkUniqueID);
		}

		public void TestEDIMessageCreatedCorrectlyForInterchange_ECS()
		{
			var helper = new ECSMessageTestHelper();
			var exitDetail1 = helper.CreateCusExitDetail(Factory, "T1", "ECS-MRN001", "TTT");
			var exitDetail2 = helper.CreateCusExitDetail(Factory, "T2", "ECS-MRN002", "TTT");

			var intchg1Text = helper.CreateEIMessageBodyWithReponseDatasNode("MessageReponseIE507", "ECS-MRN001", "ARRIVE DEST", ZDate.Today);
			var intchg1 = helper.CreateTestEDIInterchange(Factory, GlbBranch.CurrentBranch.PK, "FRCUS", "TEST", ApplicationCodeList.Codes.GenericMessageDelivery, EDIInterchange.Direction.Receive, GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms,
				"0000000000000000001", EDIInterchange.Status.Queued, intchg1Text);
			var intchg2Text = helper.CreateEIMessageBodyWithReponseEtatNode("MessageNotificationEtat", "ECS-MRN002", "ARRIVE DEST", ZDate.Today);
			var intchg2 = helper.CreateTestEDIInterchange(Factory, GlbBranch.CurrentBranch.PK, "FRCUS", "TEST", ApplicationCodeList.Codes.GenericMessageDelivery, EDIInterchange.Direction.Receive, GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms,
				"0000000000000000002", EDIInterchange.Status.Queued, intchg2Text);
			var intchg3Text = helper.CreateEIMessageBodyWithReponseDatasNode("MessageReponseIE618", "ECS-MRN002", "ARRIVE DEST", ZDate.Today);
			var intchg3 = helper.CreateTestEDIInterchange(Factory, GlbBranch.CurrentBranch.PK, "FRCUS", "TEST", ApplicationCodeList.Codes.GenericMessageDelivery, EDIInterchange.Direction.Receive, GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms,
				"0000000000000000003", EDIInterchange.Status.Queued, intchg3Text);
			var intchg4Text = helper.CreateEIMessageBodyWithReponseDatasNode("OtherSchemaID", "ECS-MRN002", "ARRIVE DEST", ZDate.Today);
			var intchg4 = helper.CreateTestEDIInterchange(Factory, GlbBranch.CurrentBranch.PK, "FRCUS", "TEST", ApplicationCodeList.Codes.GenericMessageDelivery, EDIInterchange.Direction.Receive, GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms,
				"0000000000000000004", EDIInterchange.Status.Queued, intchg4Text);
			var intchg5Text = helper.CreateEIMessageBodyWithReponseDatasNode("MessageReponseIE618", "NOMATCH", "ARRIVE DEST", ZDate.Today);
			var intchg5 = helper.CreateTestEDIInterchange(Factory, GlbBranch.CurrentBranch.PK, "FRCUS", "TEST", ApplicationCodeList.Codes.GenericMessageDelivery, EDIInterchange.Direction.Receive, GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms,
				"0000000000000000005", EDIInterchange.Status.Queued, intchg5Text);
			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg1.Reload();
			intchg2.Reload();
			intchg3.Reload();
			intchg4.Reload();
			intchg5.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg1.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg2.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg3.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg4.EI_Status);
			AssertEquals(EDIInterchange.Status.Received, intchg5.EI_Status);

			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			var msg2 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg2.PK));
			var msg3 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg3.PK));
			var msg4 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg4.PK));
			var msg5 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg5.PK));

			AssertEquals(1, msg1.Length);
			AssertEquals(1, msg2.Length);
			AssertEquals(1, msg3.Length);
			AssertEquals(0, msg4.Length);
			AssertEquals(1, msg5.Length);

			helper.AssertEDIMessage(msg1[0], intchg1, "FRC", "RCV", "ECS", "XXX", "0000000000000000001", "", ZDateTime.Empty, "PRS", "CusExitDetail", exitDetail1.PK, intchg1Text);
			helper.AssertEDIMessage(msg2[0], intchg2, "FRC", "RCV", "ECS", "XXX", "0000000000000000002", "", ZDateTime.Empty, "PRS", "CusExitDetail", exitDetail2.PK, intchg2Text);
			helper.AssertEDIMessage(msg3[0], intchg3, "FRC", "RCV", "ECS", "XXX", "0000000000000000003", "", ZDateTime.Empty, "PRS", "CusExitDetail", exitDetail2.PK, intchg3Text);
			helper.AssertEDIMessage(msg5[0], intchg5, "FRC", "RCV", "ECS", "XXX", "0000000000000000005", "", ZDateTime.Empty, "ERR", "", ZGuid.Empty, intchg5Text);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
