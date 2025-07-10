using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR20MessageProcessorTest : XMLMessageTestHelper<GOVCBRR20MessageProcessorTest>
	{
		public void TestEM_MessageTypeIs830()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("830", "6N00220000052X", "EXP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_830.xml");
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message ApplicationReference is Empty", ZString.Empty, incomingMessage.EM_ApplicationReference);
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, entry.CH_Status);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "830", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entry.CH_Status);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._830);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수출신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06 12:30:12", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N00220000052X", incomingMessage.EM_MessageInterpretation);
			AssertContains("1", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06 11:30:12", incomingMessage.EM_MessageInterpretation);
			AssertContains("[01020] 서울세관 내륙기지통관과", incomingMessage.EM_MessageInterpretation);

			AssertContains("오류내역", incomingMessage.EM_MessageInterpretation);
			AssertContains("오류내역입니다.", incomingMessage.EM_MessageInterpretation);
			AssertContains("오류문서 Key1 내역을", incomingMessage.EM_MessageInterpretation);
			AssertContains("오류문서 Key2 내역을", incomingMessage.EM_MessageInterpretation);
			AssertContains("오류문서 Key3 내역을", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출신고서", email.Body);
			AssertContains("2014-05-06 12:30:12", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("1", email.Body);
			AssertContains("2014-05-06 11:30:12", email.Body);
			AssertContains("[01020] 서울세관 내륙기지통관과", email.Body);

			AssertContains("오류내역", email.Body);
			AssertContains("오류내역입니다.", email.Body);
			AssertContains("오류문서 Key1 내역을", email.Body);
			AssertContains("오류문서 Key2 내역을", email.Body);
			AssertContains("오류문서 Key3 내역을", email.Body);
		}

		public void TestEM_MessageTypeIs5AS_and_MessageSubTypeIsA_or_C()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5AS", "6N00220000052X", "EXP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5AS.xml");
			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5AS);
			outgoingMessage.EM_MessageSubType = "A";
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			outgoingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5AS", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ARJ", CustomsMessageStatusTypeList.Codes.AmendmentRejected, entry.CH_Status);

			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수출정정신고서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출정정신고서", email.Body);
		}

		public void TestEM_MessageTypeIsDKJ()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("DKJ", "6N00220000052X", "EXP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_DKJ.xml");
			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._DKJ);
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			outgoingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "DKJ", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to CRJ", CustomsMessageStatusTypeList.Codes.CancellationRejected, entry.CH_Status);

			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수출취하신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출취하신청서", email.Body);
		}

		public void TestEM_MessageTypeIs5DP()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5DP", "6N00220000052X", "LEX");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5DP.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5DP", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entry.CH_Status);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DP);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("환급대상수출물품 반입확인 제출", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 반입확인 제출", email.Body);
		}

		public void TestEM_MessageTypeIs5DQ()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5DQ", "6N00220000052X", "LEX");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5DQ.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5DQ", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entry.CH_Status);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DQ);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("환급대상수출물품 적재신청 제출", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 적재신청 제출", email.Body);
		}

		public void TestEM_MessageTypeIs5DR_and_MessageSubTypeIs1()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5DR", "6N00220000052X", "LEX");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5DR.xml");
			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DR);
			outgoingMessage.EM_MessageSubType = "1";
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			outgoingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5DR", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ARJ", CustomsMessageStatusTypeList.Codes.AmendmentRejected, entry.CH_Status);

			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("환급대상수출물품 반입확인 정정/취하 제출", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 반입확인 정정/취하 제출", email.Body);
		}

		public void TestEM_MessageTypeIs5DS_and_MessageSubTypeIs2()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5DS", "6N00220000052X", "LEX");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5DS.xml");
			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DS);
			outgoingMessage.EM_MessageSubType = "2";
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			outgoingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5DS", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to CRJ", CustomsMessageStatusTypeList.Codes.CancellationRejected, entry.CH_Status);

			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("환급대상수출물품 적재신청 정정/취하 제출", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 적재신청 정정/취하 제출", email.Body);
		}

		public void TestEM_MessageTypeIs929HasNotOutgoingMessage()
		{
			CreateEntryForImport(true, "1234520000045M", KRJobMessageTypeList.Codes.Import);
			var incomingMessage = CreateMessageForTest("GOVCBRR20_929.xml");
			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));
		}

		public void TestEM_MessageTypeIs929()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("929", "1234520000045M", "IMP", "1");
			var chargeVersion1 = entry.Charges.AddNew();
			chargeVersion1.C1_ChargeType = Universal.Constants.RateTypes.Duty;
			chargeVersion1.C1_ChargeAmount = 1000m;
			chargeVersion1.C1_RateOverrideReasonCode = "1";
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var feeVersion1 = entryLine.Fees.AddNew();
			feeVersion1.CF_ChargeType = Universal.Constants.RateTypes.Duty;
			feeVersion1.CF_ChargeAmount = 1000m;
			feeVersion1.CF_RateOverrideReasonCode = "1";

			var incomingMessage = CreateMessageForTest("GOVCBRR20_929.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "929", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entry.CH_Status);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);

			var loadEntry = LoadCusEntryHeaderForNewFactory();
			AssertEquals(1, loadEntry.Charges.Count);
			AssertEquals(Universal.Constants.RateTypes.Duty, loadEntry.Charges[0].C1_ChargeType);
			AssertEquals(true, loadEntry.Charges[0].C1_RateOverrideReasonCode.IsEmpty);
			AssertEquals(1, loadEntry.MergedLines[0].Fees.Count);
			AssertEquals(Universal.Constants.RateTypes.Duty, loadEntry.MergedLines[0].Fees[0].CF_ChargeType);
			AssertEquals(true, loadEntry.MergedLines[0].Fees[0].CF_RateOverrideReasonCode.IsEmpty);
		}

		public void TestEM_MessageTypeIs5SM()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5SM", "1234520000045M", "5SM");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5SM.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5SM", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entry.CH_Status);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SM);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("포괄가격신고서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("포괄가격신고서", email.Body);
		}

		public void TestEM_MessageTypeIs008()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("008", "1234520000045M", "008");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_008.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "008", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entry.CH_Status);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._008);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("거주이전 및 주요물품신고서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("거주이전 및 주요물품신고서", email.Body);
		}

		public void TestEM_MessageTypeIsD87()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("D87", "1234520000045M", "D87");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_D87.xml");
			entry.Declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Message SubType is Empty", "", incomingMessage.EM_MessageSubType);
			AssertEquals("PreCondition: entry Status is Empty", ZString.Empty, entry.CH_Status);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			incomingMessage = CreateMessageForTest("GOVCBRR20_D87.xml");
			entry.Declaration.JE_AgentsReference = "1234520000045M";
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "D87", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entry.CH_Status);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._D87);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("까르네 일시수입증서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("까르네 일시수입증서", email.Body);
		}

		public void TestEM_MessageTypeIs5FEHasNotOutgoingMessage()
		{
			{
				CreateEntryForImport(true, "1234520000045M", KRJobMessageTypeList.Codes.Import);
				var incomingMessage = CreateMessageForTest("GOVCBRR20_5FE.xml");
				AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));
			}
		}

		public void TestEM_MessageTypeIs5FE()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryForImport(true, "1234520000045M", KRJobMessageTypeList.Codes.Import);
			CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FE, "2");

			var chargeVersion1 = importEntry.Charges.AddNew();
			chargeVersion1.C1_ChargeType = Universal.Constants.RateTypes.Duty;
			chargeVersion1.C1_ChargeAmount = 1000m;
			chargeVersion1.C1_RateOverrideReasonCode = "1";
			var chargeVersion2 = importEntry.Charges.AddNew();
			chargeVersion2.C1_ChargeType = Universal.Constants.RateTypes.Duty;
			chargeVersion2.C1_ChargeAmount = 500m;
			chargeVersion2.C1_RateOverrideReasonCode = "2";
			var currentCharge = importEntry.Charges.AddNew();
			currentCharge.C1_ChargeType = Universal.Constants.RateTypes.Duty;
			currentCharge.C1_ChargeAmount = 300m;
			var entryLine = importEntry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var feeVersion1 = entryLine.Fees.AddNew();
			feeVersion1.CF_ChargeType = Universal.Constants.RateTypes.Duty;
			feeVersion1.CF_ChargeAmount = 1000m;
			feeVersion1.CF_RateOverrideReasonCode = "1";
			var feeVersion2 = entryLine.Fees.AddNew();
			feeVersion2.CF_ChargeType = Universal.Constants.RateTypes.Duty;
			feeVersion2.CF_ChargeAmount = 500m;
			feeVersion2.CF_RateOverrideReasonCode = "2";
			var currentFee = entryLine.Fees.AddNew();
			currentFee.CF_ChargeType = Universal.Constants.RateTypes.Duty;
			currentFee.CF_ChargeAmount = 300m;

			var incomingMessage = CreateMessageForTest("GOVCBRR20_5FE.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			outgoingEdiMessage.Reload();
			incomingMessage.Reload();

			var loadEntry = LoadCusEntryHeaderForNewFactory();
			AssertEquals(2, loadEntry.Charges.Count);
			AssertEquals(1000m, loadEntry.Charges.Where(x => x.C1_ChargeType == Universal.Constants.RateTypes.Duty && x.C1_RateOverrideReasonCode == "1").Sum(x => x.C1_ChargeAmount));
			AssertEquals(800m, loadEntry.Charges.Where(x => x.C1_ChargeType == Universal.Constants.RateTypes.Duty && x.C1_RateOverrideReasonCode.IsEmpty).Sum(x => x.C1_ChargeAmount));
			AssertEquals(2, loadEntry.MergedLines[0].Fees.Count);
			AssertEquals(1000m, loadEntry.MergedLines[0].Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == Universal.Constants.RateTypes.Duty && x.CF_RateOverrideReasonCode == "1").Sum(x => x.CF_ChargeAmount));
			AssertEquals(800m, loadEntry.MergedLines[0].Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == Universal.Constants.RateTypes.Duty && x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount));

			CombineAssertions("When 5FE doesn't have 5UA.", () =>
			{
				AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5FE", incomingMessage.EM_MessageSubType);
				AssertEquals("Message ApplicationReference is updated", outgoingEdiMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
				AssertContains("수입정정신고서", incomingMessage.EM_MessageInterpretation);

				AssertEquals("entry status is updated correctly to ARJ", CustomsMessageStatusTypeList.Codes.AmendmentRejected, importEntry.CH_Status);
				AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, outgoingEdiMessage.MessageOrEntryStatus);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				var recipient = email.Recipients[0];
				AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
				AssertContains("수입정정신고서", email.Body);
			});

			outgoingEdiMessage.EM_MessageOwner = "1";
			incomingMessage = CreateMessageForTest("GOVCBRR20_5FE.xml");
			CreateEntryNumForTest(importEntry, "1234520000045M", ElectronicDocumentTypeList.Codes._5UA, outgoingEdiMessage.EM_MessageOwner);

			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			importEntry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = 2;
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData.CSI_LineNo = 1;
			penaltyExemptionSessionalData.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;

			Factory.Save();

			amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 2);
			penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			amendmentSessionalData.Reload();
			penaltyExemptionSessionalData.Reload();

			CombineAssertions("When 5FE has 5UA.", () =>
			{
				AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
				var entry = LoadCusEntryHeaderForNewFactory();
				AssertNull(entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UA));
				AssertEquals(ZInt.Zero, penaltyExemptionSessionalData.CSI_LineNo);
				AssertEquals(ZString.Empty, penaltyExemptionSessionalData.CSI_Code);
			});

			incomingMessage = CreateMessageForTest("GOVCBRR20_5FE.xml");
			CreateEntryNumForTest(importEntry, "1234520000045M", ElectronicDocumentTypeList.Codes._5UA, "X");
			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();

			CombineAssertions("When 5FE doesn't have matched 5UA.", () =>
			{
				AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
				var entry = LoadCusEntryHeaderForNewFactory();
				AssertNotNull(entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UA));
			});
		}

		[TestDate(2014, 05, 06)]
		public void Test5FEWith5UAWhenAlready5FEIsSentInThePast()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("5FE", "1234520000045M", "IMP", "2");
			entry.CH_VersionID = 2;
			var message5FE_1 = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5FE);

			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime(2014, 05, 05);
			messageR99.EM_SystemCreateUser = "ORG";
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_MessageSubType = message5FE_1.EM_MessageType;

			var message5FE_2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FE, "3");

			var messageR20 = CreateMessageForTest("GOVCBRR20_5FE.xml");
			Factory.Save();
			messageR99.EM_ApplicationReference = message5FE_1.EM_MessageNum;

			AssertEquals("PreCondition: Next Customs Version No is 1", 1u, entry.CalculateNextCustoms5FEVersionNumber());
			AssertEquals("PreCondition: VersionID is 2", 2u, entry.CH_VersionID);
			AssertEquals("PreCondition: CW1 Version No is 3", 3u, entry.GetCW1VersionNumberFromCustoms5FEVersionNumber(entry.CalculateNextCustoms5FEVersionNumber(), ZDate.Today));

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			message5FE_2.Reload();
			messageR20.Reload();

			AssertEquals("Next Customs Version No is 1", 1u, entry.CalculateNextCustoms5FEVersionNumber());
			AssertEquals("VersionID is 2", 2u, entry.CH_VersionID);
			AssertEquals("CW1 Version No is 3", 3u, entry.GetCW1VersionNumberFromCustoms5FEVersionNumber(entry.CalculateNextCustoms5FEVersionNumber(), ZDate.Today));

			AssertEquals(messageR20.EM_ApplicationReference, message5FE_2.EM_MessageNum);
		}

		public void TestEM_MessageTypeIs5BF()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5BF", "1234520000045M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5BF.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5BF", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to CRJ", CustomsMessageStatusTypeList.Codes.CancellationRejected, entry.CH_Status);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BF);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수입취하 신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입취하 신청서", email.Body);
		}

		public void TestEM_MessageTypeIsDF3()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("DF3", "1234520000045M", "LEX");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_DF3.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "DF3");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "DF3", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._DF3);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("환급대상수출물품 적재 완료보고서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 적재 완료보고서", email.Body);
		}

		public void TestEM_MessageTypeIs934()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport(ElectronicDocumentTypeList.Codes._934, "1234520000045M", KRJobMessageTypeList.Codes.Import);
			var incomingMessage = CreateMessageForTest("GOVCBRR20_934.xml");
			var entryNum = CreateEntryNumForTest(entry, "9876520000045M", ElectronicDocumentTypeList.Codes._934);
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", ElectronicDocumentTypeList.Codes._934, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._934);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("가격신고서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("가격신고서", email.Body);
		}

		public void TestEM_MessageTypeIs5FN()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5FN", "1234520000045M", "IMP");

			var message1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN, "1");
			entry.Messages.Add(message1);
			var message2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "2");
			entry.Messages.Add(message2);
			var message3 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN, "2");
			entry.Messages.Add(message3);
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5FN.xml");
			var entryNum1 = CreateEntryNumForTest(entry, "1234520000045M", "5FN");
			entryNum1.CE_EntryLineReference = "1";
			var entryNum2 = CreateEntryNumForTest(entry, "1234520000045M", "5UA");
			entryNum2.CE_EntryLineReference = "2";
			Factory.Save();

			SampleCodeType();

			Assert("entry status is empty", entryNum1.CE_EntryStatus.IsEmpty);
			Assert("entry status is empty", entryNum2.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum1.Reload();
			entryNum2.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5FN", incomingMessage.EM_MessageSubType);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			incomingMessage = CreateMessageForTest("GOVCBRR20_5FN.xml");
			var entryNum3 = CreateEntryNumForTest(entry, "1234520000045M", "5FN");
			entryNum3.CE_EntryLineReference = "2";
			Factory.Save();
			Assert("entry status is empty", entryNum3.CE_EntryStatus.IsEmpty);
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum3.Reload();

			Assert("entry status is empty", entryNum1.CE_EntryStatus.IsEmpty);
			Assert("entry status is empty", entryNum2.CE_EntryStatus.IsEmpty);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum3.CE_EntryStatus);

			AssertEquals("Message ApplicationReference is updated", message3.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, message3.MessageOrEntryStatus);

			AssertContains("감면분납용도세율", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("감면분납용도세율", email.Body);
		}

		public void TestEM_MessageTypeIs5FN_GetOutGoingMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var entry = CreateEntryWithOutgoingMessageForImport("5FN", "1234520000045M", "IMP");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "5FN");
			entryNum.CE_EntryLineReference = "2";
			var message1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN, "2");
			entry.Messages.Add(message1);
			var incomingMessage1 = CreateMessageForTest("GOVCBRR20_5FN.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage1.Reload();
			message1.Reload();
			AssertEquals(entry.PK, incomingMessage1.EM_LinkUniqueID);
			AssertEquals(message1.EM_MessageNum, incomingMessage1.EM_ApplicationReference);

			var message2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN, "2");
			entry.Messages.Add(message2);
			var incomingMessage2 = CreateMessageForTest("GOVCBRR20_5FN.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage2.Reload();
			message2.Reload();
			AssertEquals(entry.PK, incomingMessage2.EM_LinkUniqueID);
			AssertEquals(message2.EM_MessageNum, incomingMessage2.EM_ApplicationReference);
		}

		public void TestEM_MessageTypeIs5SC()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5SC", "1234520000045M", "IMP", "1");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5SC.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "5SC");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5SC", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);
			AssertEquals("CE_LineReference is reverted", ZString.Empty, entryNum.CE_EntryLineReference);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("협정관세 적용신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("협정관세 적용신청서", email.Body);
		}

		public void TestEM_MessageTypeIsDHR()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("DHR", "1234520000045M", "IMP", "1");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_DHR.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "DHR");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "DHR", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);
			AssertEquals("CE_LineReference is reverted", ZString.Empty, entryNum.CE_EntryLineReference);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._DHR);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("협정관세적용신청서(자료교환용)", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("협정관세적용신청서(자료교환용)", email.Body);
		}

		public void TestEM_MessageTypeIs5BD()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport(ElectronicDocumentTypeList.Codes._5BD, "1234520000045M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5BD.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", ElectronicDocumentTypeList.Codes._5BD);
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", ElectronicDocumentTypeList.Codes._5BD, incomingMessage.EM_MessageSubType);
			AssertEquals("entry number status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BD);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수입수리전반출신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입수리전반출신청서", email.Body);
		}

		public void TestEM_MessageTypeIs5SI()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5SI", "1234520000045M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5SI.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "5SI");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5SI", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SI);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수입신고 우편물 목록 제출", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고 우편물 목록 제출", email.Body);
		}

		public void TestEM_MessageTypeIs5TE()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5TE", "1234520000045M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5TE.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "5TE");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5TE", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5TE);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수입동기화요청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입동기화요청서", email.Body);
		}

		public void TestEM_MessageTypeIs5TM()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5TM", "1234520000045M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5TM.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "5TM");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5TM", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5TM);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("부가가치세 금거래계좌 납부신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("부가가치세 금거래계좌 납부신청서", email.Body);
		}

		public void TestEM_MessageTypeIs5UA()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryForImport(true, "1234520000045M", KRJobMessageTypeList.Codes.Import);

			var incomingMessage = CreateMessageForTest("GOVCBRR20_5UA.xml");
			var response = KRXmlObjectSerializer.Deserialize<CargoWise.Customs.KR.MessageDefinitions.GOVCBRR20.Response>(incomingMessage.GetEM_MessageTextReader());
			var versionID = response.Declaration.VersionId.Value;
			AssertEquals("2", versionID);

			var oldMessage5UA = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "1");
			CreateEntryNumForTest(importEntry, "1234520000045M", ElectronicDocumentTypeList.Codes._5UA, "1", CustomsMessageStatusTypeList.Codes.OriginalAccepted);
			var currentMessage5UA = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, versionID);
			CreateEntryNumForTest(importEntry, "1234520000045M", ElectronicDocumentTypeList.Codes._5UA, versionID, CustomsMessageStatusTypeList.Codes.OriginalSent);
			SampleCodeType();

			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			importEntry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData.CSI_LineNo = 2;
			penaltyExemptionSessionalData.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;

			Factory.Save();

			Assert(incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert(incomingMessage.EM_MessageSubType.IsEmpty);
			Assert(incomingMessage.EM_ApplicationReference.IsEmpty);
			AssertNotNull(importEntry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryLineReference == versionID));
			AssertEquals(2, penaltyExemptionSessionalData.CSI_LineNo);
			AssertEquals(DutyPenaltyExemptionCodeList.Codes.Y, penaltyExemptionSessionalData.CSI_Code);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			penaltyExemptionSessionalData.Reload();

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5UA", incomingMessage.EM_MessageSubType);
			AssertEquals(currentMessage5UA.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertContains("가산세(보정이자)면제 신청서", incomingMessage.EM_MessageInterpretation);
			AssertEquals(ZInt.Zero, penaltyExemptionSessionalData.CSI_LineNo);
			AssertEquals(ZString.Empty, penaltyExemptionSessionalData.CSI_Code);

			var entry = LoadCusEntryHeaderForNewFactory();
			AssertNull(entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryLineReference == versionID));

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("가산세(보정이자)면제 신청서", email.Body);
		}

		public void TestWhenNoFoundMatched5UAEntryNumAndMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryForImport(true, "1234520000045M", KRJobMessageTypeList.Codes.Import);

			var message5UA = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "1");
			var entryNum5UA = CreateEntryNumForTest(importEntry, "1234520000045M", ElectronicDocumentTypeList.Codes._5UA, "1");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5UA.xml");
			SampleCodeType();
			Factory.Save();

			Assert(incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert(incomingMessage.EM_MessageSubType.IsEmpty);
			Assert(incomingMessage.EM_ApplicationReference.IsEmpty);
			Assert(entryNum5UA.CE_EntryStatus.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());

			importEntry.Reload();
			incomingMessage.Reload();
			entryNum5UA.Reload();

			AssertEquals(importEntry.PK, incomingMessage.EM_LinkUniqueID);
			Assert(incomingMessage.EM_ApplicationReference.IsEmpty);
			Assert(entryNum5UA.CE_EntryStatus.IsEmpty);
		}

		public void TestEM_MessageTypeIsD72()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("D72", "1234520000045M", "IMP");
			outgoingEdiMessage.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR20_D72.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "D72");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "D72", incomingMessage.EM_MessageSubType);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._D72);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("재수출면세 이행기간연장 신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("재수출면세 이행기간연장 신청서", email.Body);
		}

		public void TestEM_MessageTypeIs105()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("105", "1234520000045M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_105.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "5SC");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "105", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ARJ", CustomsMessageStatusTypeList.Codes.AmendmentRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._105);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("협정관세 정정신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("협정관세 정정신청서", email.Body);
		}

		public void TestEM_MessageTypeIsDHS()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("DHS", "1234520000045M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_DHS.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "DHR");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "DHS", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ARJ", CustomsMessageStatusTypeList.Codes.AmendmentRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._DHS);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("협정관세적용신청 정정신청서(자료교환용)", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("협정관세적용신청 정정신청서(자료교환용)", email.Body);
		}

		public void TestEM_MessageTypeIs5BA()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5BA", "1234520000045M", "IMP", "1");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5BA.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "5BA");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5BA", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);
			AssertEquals("CE_LineReference is reverted", ZString.Empty, entryNum.CE_EntryLineReference);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("합의세율 신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("합의세율 신청서", email.Body);
		}

		public void TestEM_MessageTypeIs5BB()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport(ElectronicDocumentTypeList.Codes._5BB, "1234520000045M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5BB.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", ElectronicDocumentTypeList.Codes._5BA);
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", ElectronicDocumentTypeList.Codes._5BB, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ARJ", CustomsMessageStatusTypeList.Codes.AmendmentRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("합의세율 정정신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("합의세율 정정신청서", email.Body);
		}

		public void Test_CustomersOfficerIsEmpty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport("830", "6N00220000052X", "EXP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_CustomersOfficerIsEmpty.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			AssertContains("수출신고서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("[]", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("[]", email.Body);
		}

		public void Test_ErrorElementIsEmpty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport("830", "6N00220000052X", "EXP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_ErrorElementIsEmpty.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			AssertContains("수출신고서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("오류내역", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("오류내역", email.Body);
		}

		public void Test_ErrorElementLengthIs11()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport("830", "6N00220000052X", "EXP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_ErrorElementLengthIs11.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			AssertContains("수출신고서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오. ", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오. ", email.Body);
		}

		public void TestImportR20_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBRR20_830.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [사전검증 오류통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImportR20_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR20_830.xml");
				CreateEntryForImport(false, "6N00220000052X", "EXP");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[사전검증 오류통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImportR20_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR20_830.xml");
				CreateEntryForImport(true, "6N00220000052X", "EXP");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[사전검증 오류통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestEM_MessageTypeIs5GW()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var requestHeader = CreateEntryWithOutgoingMessage(ElectronicDocumentTypeList.Codes._5GW, "41634200000072U", ElectronicDocumentTypeList.Codes._5GW);
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5GW.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			requestHeader.Reload();
			incomingMessage.Reload();

			AssertEquals("requestHeader is located", requestHeader.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("requestHeader.CMR_Status is Updated", CustomsMessageStatusTypeList.Codes.OriginalRejected, requestHeader.CMR_Status);

			var outgoingMessage = (EDIMessage)requestHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5GW);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수입 임시개청 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:55:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634200000072U", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:55:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("[03017] 부산세관 부두통관2과", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입 임시개청 신청서", email.Body);
			AssertContains("2020-09-25 17:55:17", email.Body);
			AssertContains("41634200000072U", email.Body);
			AssertContains("2020-09-25 17:55:17", email.Body);
			AssertContains("[03017] 부산세관 부두통관2과", email.Body);
		}

		public void TestEM_MessageTypeIs5AC()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var requestHeader = CreateEntryWithOutgoingMessage(ElectronicDocumentTypeList.Codes._5AC, "23625200000056U", ElectronicDocumentTypeList.Codes._5AC);
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5AC.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			requestHeader.Reload();
			incomingMessage.Reload();

			AssertEquals("requestHeader is located", requestHeader.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("requestHeader.CMR_Status is Updated", CustomsMessageStatusTypeList.Codes.OriginalRejected, requestHeader.CMR_Status);

			var outgoingMessage = (EDIMessage)requestHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5AC);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("수출 임시개청 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:54:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("23625200000056U", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:54:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02015] 인천세관 수출(자유지역)과", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출 임시개청 신청서", email.Body);
			AssertContains("2020-09-25 17:54:17", email.Body);
			AssertContains("23625200000056U", email.Body);
			AssertContains("2020-09-25 17:54:17", email.Body);
			AssertContains("[02015] 인천세관 수출(자유지역)과", email.Body);
		}

		public void TestEM_MessageTypeIs5SG()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var requestHeader = CreateEntryWithOutgoingMessage(ElectronicDocumentTypeList.Codes._5SG, "41634200000072U", ElectronicDocumentTypeList.Codes._5SG);
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5SG.xml");
			SampleCodeType();
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			requestHeader.Reload();
			incomingMessage.Reload();

			AssertEquals("requestHeader is located", requestHeader.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("requestHeader.CMR_Status is Updated", CustomsMessageStatusTypeList.Codes.OriginalRejected, requestHeader.CMR_Status);

			var outgoingMessage = (EDIMessage)requestHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SG);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);

			AssertContains("확정가격신고 기간연장신청", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:55:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634200000072U", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:55:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("[03017] 부산세관 부두통관2과", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("확정가격신고 기간연장신청", email.Body);
			AssertContains("2020-09-25 17:55:17", email.Body);
			AssertContains("41634200000072U", email.Body);
			AssertContains("2020-09-25 17:55:17", email.Body);
			AssertContains("[03017] 부산세관 부두통관2과", email.Body);
		}
		public void TestEM_MessageTypeIs5UL()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("5UL", "1234520000044M", "IMP");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5UL.xml");
			var entryNum = CreateEntryNumForTest(entry, "1234520000045M", "5UL");
			outgoingEdiMessage.EM_MessageOwner = "1234520000045M";
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5UL", incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, entryNum.CE_EntryStatus);

			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, outgoingMessage.MessageOrEntryStatus);
		}
		public void TestEM_MessageTypeIs5UL_RefundDeclaration()
		{
			var refundDeclaration = new TestDataSetupHelper(Factory).CreateRefundDeclarationWithOutgoingMessage("1234520000045M");
			var incomingMessage = CreateMessageForTest("GOVCBRR20_5UL.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			refundDeclaration.Reload();
			incomingMessage.Reload();

			AssertEquals("RefundDeclaration is located", refundDeclaration.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageSubType Updated By response/Declaration/TypeCode in XML", "5UL", incomingMessage.EM_MessageSubType);
			AssertEquals("RefundDeclaration status is updated correctly to ORJ", CustomsMessageStatusTypeList.Codes.OriginalRejected, refundDeclaration.CRD_MessageStatus);

			var outgoingMessage = (EDIMessage)refundDeclaration.Messages.LastOutgoingMessage;
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("과오납 환급신청서", email.Body);
			AssertContains("1234520000045M", email.Body);
		}

		CusEntryHeader LoadCusEntryHeaderForNewFactory()
		{
			return new BusinessObjectFactory().Load<CusEntryHeader>(importEntry.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ORG";
			staff1.GS_LoginName = "Origin";
			staff1.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "Test2";
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";

			Factory.Save();
		}

		GlbGroup importGroup;

		void CreateEntry(bool broker, string entryNumber, string entryType)
		{
			requestHeader = Factory.New<CusMiscRequestHeader>();
			requestHeader.CMR_JobNumber = "B00001000";
			requestHeader.CMR_MessageType = entryType;
			requestHeader.CMR_RequestDate = new ZDateTime(2020, 09, 25, 17, 54, 17);
			requestHeader.CMR_CustomsOffice = "020";
			requestHeader.CMR_GB = Env.CurrentBranch.PK;
			if (broker)
			{
				requestHeader.CMR_GS_NKBroker = "AG";
			}
			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryNum = entryNumber;
			entryNum.CE_EntryType = entryType;
			entryNum.CE_ParentID = requestHeader.PK;
			entryNum.CE_ParentTable = requestHeader.TableName;
			Factory.Save();
		}
		CusMiscRequestHeader requestHeader;
		CusMiscRequestHeader CreateEntryWithOutgoingMessage(string em_MessageType, string entryNumber, string entryType)
		{
			if (requestHeader == null)
			{
				CreateEntry(true, entryNumber, entryType);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_MessageType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusMiscRequestHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = requestHeader;

			return requestHeader;
		}

		void CreateEntryForImport(bool setCusAgent, string entryNumber, string entryType)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			declaration.JE_DeclarationReference = "B00001000";

			importEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = importEntry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = entryNumber;
			entryNum.CE_EntryType = entryType;
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum.CE_ParentID = importEntry.PK;
			entryNum.CE_ParentTable = CusEntryHeader.Schema.TableName;
		}
		CusEntryHeader importEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForImport(string em_MessageType, string entryNumber, string entryType, string applicationReference = "")
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true, entryNumber, entryType);
			}

			CreateOutgoingMessage(em_MessageType, applicationReference);
			Factory.Save();

			return importEntry;
		}
		EDIMessage CreateOutgoingMessage(string em_MessageType, string applicationReference = "")
		{
			outgoingEdiMessage = Factory.New<EDIMessage>();
			outgoingEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingEdiMessage.EM_MessageType = em_MessageType;
			outgoingEdiMessage.EM_SystemCreateUser = "ORG";
			outgoingEdiMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingEdiMessage.EM_LinkedObject = importEntry;
			outgoingEdiMessage.EM_MessageNum = "62252";
			outgoingEdiMessage.EM_ApplicationReference = applicationReference;
			return outgoingEdiMessage;
		}
		EDIMessage outgoingEdiMessage;

		CusEntryNumber CreateEntryNumForTest(CusEntryHeader entry, ZString ce_EntryNum, ZString ce_EntryType, string ce_entryLineReference = "", string ce_EntryStatus = "")
		{
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = ce_EntryNum;
			entryNum.CE_EntryType = ce_EntryType;
			entryNum.CE_EntryStatus = ce_EntryStatus;
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNum.CE_ParentID = entry.PK;
			entryNum.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNum.CE_EntryLineReference = ce_entryLineReference;

			return entryNum;
		}

		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "020", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "15", "수출(자유지역)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "17", "부두통관2과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR20MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Common.Incoming";
	}
}
