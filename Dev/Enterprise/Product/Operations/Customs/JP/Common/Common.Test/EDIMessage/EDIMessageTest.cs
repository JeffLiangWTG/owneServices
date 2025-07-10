using System;
using System.Text;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.JP.Common.JPOutputInformationCodeList;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(EDIMessage))]
	sealed class EDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals(ApplicationCodeList.Codes.JPCustoms, message.EM_ApplicationCode);
		}

		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals("Invalid message format: Empty Message", message.EM_MessageInterpretation);

			message.EM_MessageInterpretation = "Test string";
			AssertEquals("Test string", message.EM_MessageInterpretation);
		}

		public void TestJPInspectionInformationCodes()
		{
			var message = Factory.New<EDIMessage>();
			var testCodeList = message.JPInspectionInformationCodes;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 21, testCodeList.Count);
				AssertSame("Cached", Factory.GetCachedValue<JPInspectionInformationCodeList>(), testCodeList);
			});
		}

		public void TestIsHBLCargoRegistrationInformationMessage()
		{
			AssertMessageType("SAS0711", expected: true, x => x.IsHBLCargoRegistrationInformationMessage);
			AssertMessageType("AAY2SF3", expected: false, x => x.IsHBLCargoRegistrationInformationMessage);
		}

		public void TestIsMoveInNoticeMessage()
		{
			AssertMessageType("AAT0040", expected: true, x => x.IsMoveInNoticeMessage);
			AssertMessageType("AAY2SF3", expected: false, x => x.IsMoveInNoticeMessage);
		}

		public void TestIsInspectionInformationMessage()
		{
			AssertMessageType("SAD4881", expected: true, x => x.IsInspectionInformationMessage);
			AssertMessageType("AAY2SF3", expected: false, x => x.IsInspectionInformationMessage);
		}

		void AssertMessageType(string outputInfomationCode, bool expected, Func<EDIMessage, ZBool> documentMessageFun)
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr => pr.ResponseHeader.OutputInformationCode == outputInfomationCode);
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 10))).Returns((mockParseResult, System.Array.Empty<(FieldDefinition, string)>()));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = new byte[] { 10 };
			AssertEquals(expected, documentMessageFun.Invoke(message));
		}

		public void TestIsImportPermitMessage()
		{
			AssertMessageType("SAD1AG2", expected: true, x => x.IsImportPermitMessage);
			AssertMessageType("AAY2SF3", expected: false, x => x.IsImportPermitMessage);
		}

		public void TestIsHBLCargoCancellationInformationMessage()
		{
			AssertMessageType("SAS0731", expected: true, x => x.IsHBLCargoCancellationInformationMessage);
			AssertMessageType("SAD1AG2", expected: false, x => x.IsHBLCargoCancellationInformationMessage);
		}

		public void TestIsEACNoticeInformationMessage()
		{
			AssertIsEACNoticeInformationMessage("SAE4431", expected: true);
			AssertIsEACNoticeInformationMessage("AAY2SF3", expected: false);
		}

		void AssertIsEACNoticeInformationMessage(string outputInfomationCode, bool expected)
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr => pr.ResponseHeader.OutputInformationCode == outputInfomationCode);
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 10))).Returns((mockParseResult, System.Array.Empty<(FieldDefinition, string)>()));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = new byte[] { 10 };
			AssertEquals(expected, message.IsEACNoticeInformationMessage);
		}

		public void TestMessageNum()
		{
			CombineAssertions(() =>
			{
				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.JP.IJobDeclaration>();
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "ABC6789012";
				var message = Factory.New<EDIMessage>();
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message.EM_LinkedObject = entryHeader;
				message.EM_MessageType = "XYZ";
				message.EM_MessageSubType = "12";
				Factory.Save();

				var logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
				AssertEquals($"XYZ12ABC678901200000000001", message.EM_MessageNum);
				AssertEquals(EDIMessage.MessageReferenceLength, message.EM_MessageNum.Length);
				AssertEquals(0, logs.Length);

				var message2 = Factory.New<EDIMessage>();
				message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message2.EM_ApplicationReference = EDIMessage.FlatFile;
				message2.EM_LinkedObject = entryHeader;
				message2.EM_MessageType = "JKL";
				Factory.Save();

				logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
				AssertEquals($"JKL00ABC678901200000000002", message2.EM_MessageNum);
				AssertEquals(EDIMessage.MessageReferenceLength, message2.EM_MessageNum.Length);
				AssertEquals(1, logs.Length);
				AssertEquals($"JKL00ABC678901200000000002", logs[0].SL_Reference);
			});
		}

		public void TestGenerateMessageNumber_ImportingMessage_ProcedureCodeIsNull()
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => Encoding.ASCII.GetString(i).StartsWith("ABC01XYZ")))).Returns((Mock.Of<IJPInboundMessageParseResult>(r =>
				r.ResponseHeader.ProcedureCode == null &&
				r.ResponseHeader.OutputInformationCode == "XYZ" &&
				r.ResponseHeader.InputReference == "4567890123"
			), null));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = Encoding.ASCII.GetBytes("ABC01XYZ");

			var parser = NACCSFactoryService.GetMessageFlatParser(Factory);
			var header = JPMessageUtils.ParseInboundHeaderOnly(parser, message.EM_MessageData);

			message.MessageNumberStrategy = new MessageNumberStrategy(message, header);
			Factory.Save();

			AssertEquals("ProcedureCode is null", "JR00000XYZ____4567890123", message.EM_MessageNum.Left(24));
		}

		public void TestGenerateMessageNumber_ImportingMessage_ProcedureCodeIsInvalid()
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => Encoding.ASCII.GetString(i).StartsWith("ABC01XYZ")))).Returns((Mock.Of<IJPInboundMessageParseResult>(r =>
				r.ResponseHeader.ProcedureCode == "XXXXX" &&
				r.ResponseHeader.OutputInformationCode == "XYZ" &&
				r.ResponseHeader.InputReference == "4567890123"
			), null));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = Encoding.ASCII.GetBytes("ABC01XYZ");

			var parser = NACCSFactoryService.GetMessageFlatParser(Factory);
			var header = JPMessageUtils.ParseInboundHeaderOnly(parser, message.EM_MessageData);

			message.MessageNumberStrategy = new MessageNumberStrategy(message, header);
			Factory.Save();

			AssertEquals("ProcedureCode is invalid", "JR00000XYZ____4567890123", message.EM_MessageNum.Left(24));
		}

		public void TestMessageType()
		{
			var message = Factory.New<EDIMessage>();
			message.ProcedureCode = "*1";
			AssertEquals("EM_MessageType is empty", ZString.Empty, message.EM_MessageType);
			AssertEquals("EM_MessageSubType is empty", ZString.Empty, message.EM_MessageSubType);

			message.ProcedureCode = "HCH01";
			AssertEquals("EM_MessageType is empty", "HCH", message.EM_MessageType);
			AssertEquals("EM_MessageSubType is empty", "01", message.EM_MessageSubType);
		}

		[TestDate(2020, 03, 18, 01, 15, 20)]
		public void TestGenerateMessageNumberOnReceivingMessage()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.JP.IJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mockParser = new Mock<IJPMessageFlatParser>();
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => Encoding.ASCII.GetString(i).StartsWith("HCH01XYZ")))).Returns((Mock.Of<IJPInboundMessageParseResult>(r =>
				r.ResponseHeader.ProcedureCode == "HCH01" &&
				r.ResponseHeader.OutputInformationCode == "XYZ" &&
				r.ResponseHeader.InputReference == "4567890123"
			), null));
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => Encoding.ASCII.GetString(i).StartsWith("IDE*XYZ")))).Returns((Mock.Of<IJPInboundMessageParseResult>(r =>
				r.ResponseHeader.ProcedureCode == "IDE" &&
				r.ResponseHeader.OutputInformationCode == "*XYZ" &&
				r.ResponseHeader.InputReference == "5678901234"
			), null));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message1 = Factory.New<EDIMessage>();
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message1.EM_MessageData = Encoding.ASCII.GetBytes("HCH01XYZ");
			message1.EM_LinkedObject = entryHeader;

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message2.EM_MessageData = Encoding.ASCII.GetBytes("IDE*XYZ");
			message2.EM_ApplicationReference = EDIMessage.FlatFile;
			message2.EM_LinkedObject = entryHeader;

			var parser = NACCSFactoryService.GetMessageFlatParser(Factory);
			var header1 = JPMessageUtils.ParseInboundHeaderOnly(parser, message1.EM_MessageData);
			var header2 = JPMessageUtils.ParseInboundHeaderOnly(parser, message2.EM_MessageData);

			message1.MessageNumberStrategy = new MessageNumberStrategy(message1, header1);
			message2.MessageNumberStrategy = new MessageNumberStrategy(message2, header2);
			Factory.Save();

			var logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertContainsExactElementsInExactOrder("Message number sub/type and reference", new[] { "JRHCH01XYZ____45678901", "JRIDE00_XYZ___56789012" }, new[] { message1.EM_MessageNum.Left(22), message2.EM_MessageNum.Left(22) });
			AssertContainsExactElementsInAnyOrder("Message number sequences", new[] { "00000000001", "00000000002" }, new[] { message1.EM_MessageNum.Right(11), message2.EM_MessageNum.Right(11) });
			AssertEquals(1, logs.Length);
			AssertEquals("Only inbound flat file message should create data import log events.", message2.EM_MessageNum, logs[0].SL_Reference);
		}

		public void TestInboundMessageInterpretation()
		{
			CombineAssertions(() =>
			{
				var mockParser = new Mock<IJPMessageFlatParser>();
				var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr =>
					pr.ResponseHeader.ProcedureCode == "Inbound Procedure Code" &&
					pr.ResponseHeader.OutputInformationCode == "Inbound Response Code" &&
					pr.ResponseHeader.Subject == "Inbound Subject"
				);
				mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 10))).Returns((mockParseResult, System.Array.Empty<(FieldDefinition, string)>()));
				NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);
				var message = Factory.New<EDIMessage>();
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				message.EM_MessageData = new byte[] { 10 };
				message.EM_MessageType = JPMessageTypes.Codes.XER;
				AssertEquals(ZString.Empty, message.EM_MessageInterpretation);

				message.EM_MessageType = ZString.Empty;
				var interpretation = message.EM_MessageInterpretation;
				AssertContains("Inbound Procedure Code", interpretation);
				AssertContains("Inbound Response Code", interpretation);
				AssertContains("Inbound Subject", interpretation);
				AssertEquals("Only inbound header table", 1, interpretation.Occurrences("</table>"));

				mockParser.Verify();
			});
		}

		public void TestOutboundMessageInterpretation()
		{
			CombineAssertions(() =>
			{
				var mockParser = new Mock<IJPMessageFlatParser>();
				var mockHeader = Mock.Of<IJPOutboundMessageHeader>(h =>
					h.ProcedureCode == "Outbound Procedure Code"
				);
				mockParser.Setup(m => m.ParseOutbound(It.Is<byte[]>(i => i[0] == 20))).Returns((mockHeader, System.Array.Empty<(FieldDefinition, string)>()));
				NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

				var message = Factory.New<EDIMessage>();
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message.EM_MessageData = new byte[] { 20 };
				message.EM_MessageType = JPMessageTypes.Codes.XER;
				AssertEquals(ZString.Empty, message.EM_MessageInterpretation);

				message.EM_MessageType = ZString.Empty;
				var interpretation = message.EM_MessageInterpretation;
				AssertContains("Outbound Procedure Code", interpretation);
				AssertEquals("Only outbound header table", 1, interpretation.Occurrences("</table>"));
			});
		}

		public void TestFormattedMessageText()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageData = JPMessageUtils.MessageDataEncoding.GetBytes("FORMATTED \r\nの\r\n TEXT");
			AssertEquals("FORMATTED \r\nの\r\n TEXT", message.EM_FormattedMessageText);
		}

		public void TestEM_Calc_ProcedureCodeAndName()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.JP.IJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "ABC6789012";
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageType = "NVC";
			message.EM_MessageSubType = "01";
			Factory.Save();

			var list = Factory.GetCachedValue<JPProcedureCodeJPNameList>();

			AssertEquals("NVC01", message.ProcedureCode);
			AssertEquals(message.EM_Calc_ProcedureName, list.GetDescriptionFromCode(message.ProcedureCode));
		}

		public void TestEM_Calc_OutputInformationCodeAndName()
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr =>
				pr.ResponseHeader.ProcedureCode == "Inbound Procedure Code" &&
				pr.ResponseHeader.OutputInformationCode == "*SNVC01" &&
				pr.ResponseHeader.Subject == "Inbound Subject"
			);
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 10))).Returns((mockParseResult, System.Array.Empty<(FieldDefinition, string)>()));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var list = Factory.GetCachedValue<JPOutputInformationCodeJPNameList>();
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			AssertEquals(ZString.Empty, message.EM_Calc_OutputInformationCode);

			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = new byte[] { 10 };
			AssertEquals("*SNVC01", message.EM_Calc_OutputInformationCode);
			AssertEquals(message.EM_Calc_OutputInformation, list.GetDescriptionFromCode(message.EM_Calc_OutputInformationCode));
		}

		public void TestAAS0180IsProperlySetup()
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr =>
				pr.ResponseHeader.ProcedureCode == "Inbound Procedure Code" &&
				pr.ResponseHeader.OutputInformationCode == "AAS0180" &&
				pr.ResponseHeader.Subject == "Inbound Subject"
			);
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 10))).Returns((mockParseResult, System.Array.Empty<(FieldDefinition, string)>()));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = Direction.Receive;
			message.EM_MessageData = new byte[] { 10 };
			CombineAssertions(() =>
			{
				AssertEquals("EM_Calc_OutputInformationCode", "AAS0180", message.EM_Calc_OutputInformationCode);
				AssertEquals("EM_Calc_OutputInformation", "不突合情報", message.EM_Calc_OutputInformation);
				Assert(message.IsMismatchInformationMessage);
			});
		}

		public void TestExportPermitCodeListIsCorrect()
		{
			var factory = new BusinessObjectFactory();
			var expectedOutputInformationCodes = new ExportClearancePermit().OutputInformationCodes;
			var actualOutputInformationCodes = factory.GetCachedValue<JPExportPermitCodeList>().GetAllCodes();

			AssertContainsExactElementsInAnyOrder(expectedOutputInformationCodes, actualOutputInformationCodes);
		}
	}

	public class EDIMessageNumNoDuplicationTest : TestCase
	{
		[UseSnapshotProtection]
		[TestDate(2021, 4, 10, 22, 22, 22)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods")]
		public void TestMessageReferenceNoDuplicated()
		{
			var factory1 = new BusinessObjectFactory();
			var declaration = (BaseJobDeclaration)factory1.New<Integration.Customs.JP.IJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "abc6789012";
			factory1.Save();

			var message1 = factory1.New<EDIMessage>();
			var messageReference1 = ZString.Empty;
			message1.EM_LinkedObject = entryHeader;

			var connection1 = ((CargoWise.Data.IDbConnected)factory1).Connection;
			try
			{
				connection1.BeginTransaction();
				message1.OnSaving();
				messageReference1 = message1.EM_MessageNum;
			}
			finally
			{
				connection1.RollbackTransaction();
			}

			var factory2 = new BusinessObjectFactory();
			var message2 = factory2.New<EDIMessage>();
			message2.EM_LinkedObject = factory2.Load<CusEntryHeader>(entryHeader.PK);
			factory2.Save();
			AssertEquals("Should return the former reference number for connection1 rolled back.", messageReference1, message2.EM_MessageNum);
			AssertEquals("message1.EM_MessageNum should remain for factory not saved.", messageReference1, message1.EM_MessageNum);

			factory1.Save();
			AssertNotEquals("Should get next reference number from number fountain.", messageReference1, message1.EM_MessageNum);
		}
	}
}
