using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(NACCSMessageInterpreter))]
sealed class NACCSMessageInterpreterTest : TestCaseWithFactory
{
	public void TestFieldsInterpretedAlongWithDefinitions_BareData()
	{
		CombineAssertions(() =>
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockHeader = Mock.Of<IJPOutboundMessageHeader>(h =>
				h.ProcedureCode == "Outbound Procedure Code" &&
				h.InputReference == "Input Reference" &&
				h.MessageTag == "Message Reference"
			);
			mockParser.Setup(m => m.ParseOutbound(It.Is<byte[]>(i => i[0] == 20))).Returns((mockHeader, new (FieldDefinition, string)[] {
					JPMessageTestHelper.CreateField(2, "EN1", "JN1", "F1", "", "", "f1"),
					JPMessageTestHelper.CreateField(3, "EN2", "JN2", "F2", "", "", "f2"),
					JPMessageTestHelper.CreateField(4, "EN3", "JN3", "F3_1", "1", "", "f3_1"),
					JPMessageTestHelper.CreateField(5, "EN5", "JN5", "F5_1_1", "1", "1", "f5_1_1"),
					JPMessageTestHelper.CreateField(5, "EN5", "JN5", "F5_1_2", "1", "2", "f5_1_2"),
					JPMessageTestHelper.CreateField(4, "EN3", "JN3", "F3_2", "2", "", "f3_2"),
					JPMessageTestHelper.CreateField(5, "EN5", "JN5", "F5_2_1", "2", "1", "f5_2_1"),
					JPMessageTestHelper.CreateField(5, "EN5", "JN5", "F5_2_2", "2", "2", "f5_2_2"),
					JPMessageTestHelper.CreateField(4, "EN3", "JN3", "F3_3", "3", "", "f3_3"),
					JPMessageTestHelper.CreateField(5, "EN5", "JN5", "F5_3_1", "3", "1", "f5_3_1"),
					JPMessageTestHelper.CreateField(5, "EN5", "JN5", "F5_3_2", "3", "2", "f5_3_2"),
					JPMessageTestHelper.CreateField(4, "EN3", "JN3", "F3_4", "4", "", "f3_4"),
					JPMessageTestHelper.CreateField(5, "EN5", "JN5", "F5_4", "4", "1", "f5_4"),
				}));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_MessageData = new byte[] { 20 };
			var dehydratedInterpretion = DehydrateInterpretation(message);
			AssertContainsExactElementsInExactOrder(new[] { "OutboundCommonHeader",
					"ProcedureCode", "OutboundProcedureCode",
					"MessageReference", "MessageReference",
					"InputReference", "InputReference",
					"MessageHeader",
					"NO", "項目名", "ID", "&nbsp;",
					"2", "JN1", "F1", "f1",
					"3", "JN2", "F2", "f2",
					"MessageDetails-1",
					"NO", "項目名", "ID", "&nbsp;",
					"4", "JN3", "F3_1", "f3_1",
					"5", "JN5", "F5_1_1", "f5_1_1",
					"5", "JN5", "F5_1_2", "f5_1_2",
					"MessageDetails-2",
					"NO", "項目名", "ID", "&nbsp;",
					"4", "JN3", "F3_2", "f3_2",
					"5", "JN5", "F5_2_1", "f5_2_1",
					"5", "JN5", "F5_2_2", "f5_2_2",
					"MessageDetails-3",
					"NO", "項目名", "ID", "&nbsp;",
					"4", "JN3", "F3_3", "f3_3",
					"5", "JN5", "F5_3_1", "f5_3_1",
					"5", "JN5", "F5_3_2", "f5_3_2",
					"MessageDetails-4",
					"NO", "項目名", "ID", "&nbsp;",
					"4", "JN3", "F3_4", "f3_4",
					"5", "JN5", "F5_4", "f5_4",
				}, dehydratedInterpretion.Split("<<<".ToCharArray()).Where(s => !string.IsNullOrEmpty(s)));
		});
	}

	public void TestInterpretedWithJPName_BareData()
	{
		using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
		{
			CombineAssertions(() =>
			{
				var mockParser = new Mock<IJPMessageFlatParser>();
				var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr =>
					pr.ResponseHeader.ProcedureCode == "Inbound Procedure Code" &&
					pr.ResponseHeader.OutputInformationCode == "Inbound Response Code" &&
					pr.ResponseHeader.Subject == "Inbound Subject" &&
					!pr.HasResultCode
				);
				mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 20))).Returns((mockParseResult, new (FieldDefinition, string)[] {
						JPMessageTestHelper.CreateField(2, "EN1", "JN1", "", "", "", "f1"),
						JPMessageTestHelper.CreateField(3, "EN2", "JN2", "", "", "", "f2"),
						JPMessageTestHelper.CreateField(4, "EN3", "JN3", "", "1", "", "f3_1"),
						JPMessageTestHelper.CreateField(5, "EN5", "JN5", "", "1", "1", "f5_1_1"),
						JPMessageTestHelper.CreateField(5, "EN5", "JN5", "", "1", "2", "f5_1_2"),
						JPMessageTestHelper.CreateField(4, "EN3", "JN3", "", "2", "", "f3_2"),
						JPMessageTestHelper.CreateField(5, "EN5", "JN5", "", "2", "1", "f5_2_1"),
						JPMessageTestHelper.CreateField(5, "EN5", "JN5", "", "2", "2", "f5_2_2"),
						JPMessageTestHelper.CreateField(4, "EN3", "JN3", "", "3", "", "f3_3"),
						JPMessageTestHelper.CreateField(5, "EN5", "JN5", "", "3", "1", "f5_3_1"),
						JPMessageTestHelper.CreateField(5, "EN5", "JN5", "", "3", "2", "f5_3_2"),
						JPMessageTestHelper.CreateField(4, "EN3", "JN3", "", "4", "", "f3_4"),
						JPMessageTestHelper.CreateField(5, "EN5", "JN5", "", "4", "1", "f5_4"),
					}));
				NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

				var message = Factory.New<EDIMessage>();
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				message.EM_MessageData = new byte[] { 20 };
				var dehydratedInterpretion = DehydrateInterpretation(message);
				AssertContainsExactElementsInExactOrder(new[] { "InboundCommonHeader",
						"ProcedureCode", "InboundProcedureCode",
						"OutputInformationCode", "InboundResponseCode",
						"Subject", "InboundSubject",
						"MessageHeader",
						"NO", "項目名", "SEQ", "&nbsp;",
						"2", "JN1", "&nbsp;", "f1",
						"3", "JN2", "&nbsp;", "f2",
						"MessageDetails-1",
						"NO", "項目名", "SEQ", "&nbsp;",
						"4", "JN3", "&nbsp;", "f3_1",
						"5", "JN5", "1", "f5_1_1",
						"5", "JN5", "2", "f5_1_2",
						"MessageDetails-2",
						"NO", "項目名", "SEQ", "&nbsp;",
						"4", "JN3", "&nbsp;", "f3_2",
						"5", "JN5", "1", "f5_2_1",
						"5", "JN5", "2", "f5_2_2",
						"MessageDetails-3",
						"NO", "項目名", "SEQ", "&nbsp;",
						"4", "JN3", "&nbsp;", "f3_3",
						"5", "JN5", "1", "f5_3_1",
						"5", "JN5", "2", "f5_3_2",
						"MessageDetails-4",
						"NO", "項目名", "SEQ", "&nbsp;",
						"4", "JN3", "&nbsp;", "f3_4",
						"5", "JN5", "1", "f5_4",
					}, dehydratedInterpretion.Split("<<<".ToCharArray()).Where(s => !string.IsNullOrEmpty(s)));
			});
		}
	}

	public void TestInboundMessageInterpretedWithNoErrors()
	{
		CombineAssertions(() =>
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr =>
				pr.ResponseHeader.ProcedureCode == "IDA" &&
				pr.ResponseHeader.OutputInformationCode == "Inbound Response Code" &&
				pr.ResponseHeader.Subject == "Inbound Subject" &&
				pr.HasResultCode &&
				pr.IsSuccess &&
				pr.ResultCode == "xxxx" &&
				pr.Errors == new Error[]
				{
						new Error("00000", "", 0),
				}
			);
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 20))).Returns((mockParseResult, System.Array.Empty<(FieldDefinition, string)>()));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);
			var cusCodeListHelper = new UniversalReferenceTestDataHelper(Factory);
			cusCodeListHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NACCSResultCode, "IDA__00000", "正常終了 不要", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(2));
			Factory.Save();

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = new byte[] { 20 };
			var dehydratedInterpretion = DehydrateInterpretation(message);
			AssertContainsExactElementsInExactOrder(new[] { "Results:xxxx",
									"ResultCode", "Description/Disposition", "ElementID", "ElementDescription", "LineNumber",
									"00000", "正常終了不要", "&nbsp;", "&nbsp;", "&nbsp;",
									"InboundCommonHeader",
									"ProcedureCode", "IDA",
									"OutputInformationCode", "InboundResponseCode",
									"Subject", "InboundSubject",
				}, dehydratedInterpretion.Split("<<<".ToCharArray()).Where(s => !string.IsNullOrEmpty(s)));
		});
	}

	public void TestInboundMessageInterpretedWithErrors()
	{
		CombineAssertions(() =>
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr =>
				pr.ResponseHeader.ProcedureCode == "IDA" &&
				pr.ResponseHeader.OutputInformationCode == "Inbound Response Code" &&
				pr.ResponseHeader.Subject == "Inbound Subject" &&
				pr.HasResultCode &&
				!pr.IsSuccess &&
				pr.ResultCode == "xxxx" &&
				pr.Errors == new Error[]
				{
						new Error("Z0001", "AA1", 123, JPMessageTestHelper.CreateField(1, "AA", "エーエー", "AA_", "", "", "").FieldDefinition),
						new Error("Z0002", "BB1", 456, JPMessageTestHelper.CreateField(1, "BB", "ベーベー", "BB_", "", "", "").FieldDefinition),
				}
			);
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 20))).Returns((mockParseResult, System.Array.Empty<(FieldDefinition, string)>()));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var cusCodeListHelper = new UniversalReferenceTestDataHelper(Factory);
			CreateErrorCode(cusCodeListHelper, "IDA__Z0001", "IDA__Z0001 Description");
			CreateErrorCode(cusCodeListHelper, "IDA__Z0002", "IDA__Z0002 Description");
			CreateErrorCode(cusCodeListHelper, "IDA01Z0001", "IDA01Z0001 Description");
			CreateErrorCode(cusCodeListHelper, "IDA01Z0002", "IDA01Z0002 Description");
			CreateErrorCode(cusCodeListHelper, "Z0001", "Incorrect Z0001 Description");
			CreateErrorCode(cusCodeListHelper, "Z0002", "Incorrect Z0002 Description");

			Factory.Save();

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = new byte[] { 20 };
			var dehydratedInterpretion = DehydrateInterpretation(message);
			AssertContainsExactElementsInExactOrder(new[] { "Results:xxxx",
									"ResultCode", "Description/Disposition", "ElementID", "ElementDescription", "LineNumber",
									"Z0001", "IDA__Z0001Description", "AA1", "AA", "123",
									"Z0002", "IDA__Z0002Description", "BB1", "BB", "456",
									"InboundCommonHeader",
									"ProcedureCode", "IDA",
									"OutputInformationCode", "InboundResponseCode",
									"Subject", "InboundSubject",
								}, dehydratedInterpretion.Split("<<<".ToCharArray()).Where(s => !string.IsNullOrEmpty(s)));

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				message = Factory.New<EDIMessage>();
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				message.EM_MessageData = new byte[] { 20 };
				dehydratedInterpretion = DehydrateInterpretation(message);
				AssertContainsExactElementsInExactOrder(new[] { "Results:xxxx",
									"ResultCode", "Description/Disposition", "ElementID", "ElementDescription", "LineNumber",
									"Z0001", "IDA__Z0001Description", "AA1", "エーエー", "123",
									"Z0002", "IDA__Z0002Description", "BB1", "ベーベー", "456",
									"InboundCommonHeader",
									"ProcedureCode", "IDA",
									"OutputInformationCode", "InboundResponseCode",
									"Subject", "InboundSubject",
								}, dehydratedInterpretion.Split("<<<".ToCharArray()).Where(s => !string.IsNullOrEmpty(s)));
			}
		});
	}

	public void TestInboundMessageInterpretedWithExtraResults()
	{
		CombineAssertions(() =>
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr =>
				pr.ResponseHeader.ProcedureCode == "IDA" &&
				pr.ResponseHeader.OutputInformationCode == "Inbound Response Code" &&
				pr.ResponseHeader.Subject == "Inbound Subject" &&
				!pr.HasResultCode &&
				pr.IsSuccess
			);

			var messageBody = new (FieldDefinition, string)[]
			{
					(JPMessageTestHelper.CreateField(1, "Result Code", "処理結果コード", "AAA", "", "", "").FieldDefinition, "Z0001-AAAA-0123"),
					(JPMessageTestHelper.CreateField(2, "Result Code", "処理結果コード", "BBB", "", "", "").FieldDefinition, "Z0002-BBBB-0456"),
					(JPMessageTestHelper.CreateField(3, "Container Code", "コンテナ番号", "CCC", "", "", "").FieldDefinition, "Z0001-CCCC-0789")
			};

			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 20))).Returns((mockParseResult, messageBody));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var cusCodeListHelper = new UniversalReferenceTestDataHelper(Factory);
			CreateErrorCode(cusCodeListHelper, "IDA__Z0001", "IDA Z0001 Description");
			CreateErrorCode(cusCodeListHelper, "IDA__Z0002", "IDA Z0002 Description");
			CreateErrorCode(cusCodeListHelper, "IDA01Z0001", "IDA 01 Z0001 Description");
			CreateErrorCode(cusCodeListHelper, "IDA01Z0002", "IDA 01 Z0002 Description");
			CreateErrorCode(cusCodeListHelper, "Z0001", "Incorrect Z0001 Description");
			CreateErrorCode(cusCodeListHelper, "Z0002", "Incorrect Z0002 Description");

			Factory.Save();

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = new byte[] { 20 };
			var dehydratedInterpretion = DehydrateInterpretation(message);
			AssertContainsExactElementsInExactOrder(new[] {
									"InboundCommonHeader",
									"ProcedureCode",
									"IDA",
									"OutputInformationCode",
									"InboundResponseCode",
									"Subject",
									"InboundSubject",
									"MessageHeader",
									"NO",
									"項目名",
									"SEQ",
									"3",
									"コンテナ番号",
									"Z0001-CCCC-0789",
									"ResultCode",
									"Description/Disposition",
									"ElementID",
									"ElementDescription",
									"LineNumber",
									"Z0001",
									"IDAZ0001Description",
									"AAAA",
									"ResultCode",
									"123",
									"Z0002",
									"IDAZ0002Description",
									"BBBB",
									"ResultCode",
									"456"
								}, dehydratedInterpretion.Split("<<<".ToCharArray()).Select(c => c.Replace("&nbsp;", " ")).Where(s => !string.IsNullOrWhiteSpace(s)));
		});
	}

	public void TestGetErrorCodeDescription()
	{
		var message = Factory.New<EDIMessage>();
		var methodInfo = typeof(NACCSMessageInterpreter).GetMethod("GetErrorCodeDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var cusCodeListHelper = new UniversalReferenceTestDataHelper(Factory);
		CreateErrorCode(cusCodeListHelper, "IDC__Z0001", "IDC__Z0001 Description");

		Factory.Save();

		var interpreter = new NACCSMessageInterpreter(message);
		var result = methodInfo.Invoke(interpreter, new string[] { "1DA__", "Z0001" });
		AssertEquals("IDC__Z0001 Description", result);

		result = methodInfo.Invoke(interpreter, new string[] { "IDC__", "Z0001" });
		AssertEquals("IDC__Z0001 Description", result);

		result = methodInfo.Invoke(interpreter, new string[] { "XXX__", "Z0001" });
		AssertEquals(ZString.Empty, result);
	}

	void CreateErrorCode(UniversalReferenceTestDataHelper helper, string code, string description)
	{
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NACCSResultCode, code, description, ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(2));
	}

	public void TestTableWithStyles()
	{
		CombineAssertions(() =>
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockHeader = Mock.Of<IJPOutboundMessageHeader>();
			mockParser.Setup(m => m.ParseOutbound(It.Is<byte[]>(i => i[0] == 20))).Returns((mockHeader, System.Array.Empty<(FieldDefinition, string)>()));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_MessageData = new byte[] { 20 };
			var interpretation = new NACCSMessageInterpreter(message).Interprete();
			AssertContains("DefaultStyle", NACCSMessageInterpreter.DefaultStyle, interpretation);
			AssertContains("font-size", "font-size: 30px;", interpretation);
			AssertContains("body width", "body {width: 100%;}", interpretation);
		});
	}

	public void TestWriteRowWithFormatting()
	{
		using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
		{
			var mockParser = new Mock<IJPMessageFlatParser>();
			var mockParseResult = Mock.Of<IJPInboundMessageParseResult>(pr =>
				pr.ResponseHeader.ProcedureCode == "Inbound Procedure Code" &&
				pr.ResponseHeader.OutputInformationCode == "Inbound Response Code" &&
				pr.ResponseHeader.Subject == "Inbound Subject" &&
				!pr.HasResultCode
			);
			mockParser.Setup(m => m.ParseInbound(It.Is<byte[]>(i => i[0] == 20))).Returns((mockParseResult, new (FieldDefinition, string)[] {
						JPMessageTestHelper.CreateField(2, "EN1", "JN1", "", "", "", "f1"),
						JPMessageTestHelper.CreateField(3, "EN2", "JN2", "", "", "", "f2"),
					}));
			NACCSFactoryServiceTestHelper.SetMessageFlatParser(Factory, mockParser.Object);

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageData = new byte[] { 20 };
			var interpretation = new NACCSMessageInterpreter(message).Interprete();
			var tableString = @"<tr><th colspan=""1"">NO</th><th colspan=""2"">項目名</th><th colspan=""1"">SEQ</th><th colspan=""3"">&nbsp;</th></tr><tr><td colspan=""1"">2</td><td colspan=""2"">JN1</td><td colspan=""1"">&nbsp;</td><td colspan=""3"">f1</td></tr><tr><td colspan=""1"">3</td><td colspan=""2"">JN2</td><td colspan=""1"">&nbsp;</td><td colspan=""3"">f2</td></tr>";
			AssertContains(tableString, interpretation);
		}
	}

	public void TestConvertXERXmlToHtmlTable()
	{
		var xmlContent = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>WebPrint_JLUTESTTEST_NO_CLIENT</SenderID>
		<RecipientID>NACCS@MAIL.TEST.NACCS6</RecipientID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
			<Event>
				<EventTime>2025-02-26T08:43:15</EventTime>
				<EventType>IRJ</EventType>
				<EventParameters>
					<MessageType>XER</MessageType>
					<Type>TransmissionError</Type>
					<Reason>
SMTP connection fails. Please check the followings:
	1.You are connected to the NACCS internet via a NACCS router. 
	2.The domain CDQWBK3.WISETECHGLOBAL.COM is valid. To update this value, visit CargoWise &gt; Maintain &gt; System &gt; Registry &gt; Customs &gt; Country or Region Specific &gt; Japan &gt; NACCS Messaging &gt; Remote WebPrint Client Configurations. 
	3.The mailbox NACCS@MAIL.TEST.NACCS6 is valid. To update this value, visit CargoWise &gt; Maintain &gt; User Admin &gt; Staff and Resources &gt; Staff &gt; Credential. 
If the issue persists. Please raise an eRequest (Product: CargoWise; Module: Customs; Country: Japan) and provide the following exception message to the support. You can raise an eRequest by pressing F1 in CargoWise.
[1] System.Net.Sockets.SocketException (0x80004005): No such host is known&
</Reason>
				</EventParameters>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		var message = Factory.New<EDIMessage>();
		message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		message.EM_MessageData = Encoding.ASCII.GetBytes(xmlContent);
		message.EM_MessageText = xmlContent;
		var actualHtml = new NACCSMessageInterpreter(message).ConvertXERXmlToHtmlTable();
		var tableString = @"<!DOCTYPE html><style>
			table {
			table-layout: fixed;
			margin-top: 10px;
			width: 100%;
			border-collapse: collapse;
			}

			td {
			font-size: 30px;
			padding-left: 10px;
			padding-right: 10px;
			vertical-align: top;
			white-space: pre-wrap;
			word-wrap: break-word;
			}
		</style>
<table border=""3"">
  <tr>
    <td>EventTime</td>
    <td colspan=""4"">2025-02-26T08:43:15</td>
  </tr>
  <tr>
    <td>EventType</td>
    <td colspan=""4"">IRJ</td>
  </tr>
  <tr>
    <td>MessageType</td>
    <td colspan=""4"">XER</td>
  </tr>
  <tr>
    <td>Type</td>
    <td colspan=""4"">TransmissionError</td>
  </tr>
  <tr>
    <td>Reason</td>
    <td colspan=""4""><br/>SMTP connection fails. Please check the followings:<br/>    1.You are connected to the NACCS internet via a NACCS router. <br/>    2.The domain CDQWBK3.WISETECHGLOBAL.COM is valid. To update this value, visit CargoWise &gt; Maintain &gt; System &gt; Registry &gt; Customs &gt; Country or Region Specific &gt; Japan &gt; NACCS Messaging &gt; Remote WebPrint Client Configurations. <br/>    3.The mailbox NACCS@MAIL.TEST.NACCS6 is valid. To update this value, visit CargoWise &gt; Maintain &gt; User Admin &gt; Staff and Resources &gt; Staff &gt; Credential. <br/>If the issue persists. Please raise an eRequest (Product: CargoWise; Module: Customs; Country: Japan) and provide the following exception message to the support. You can raise an eRequest by pressing F1 in CargoWise.<br/>[1] System.Net.Sockets.SocketException (0x80004005): No such host is known&<br/></td>
  </tr>
</table>";

		AssertContainsExactLinesInExactOrder(tableString, actualHtml);
	}

	string DehydrateInterpretation(EDIMessage message) => Regex.Replace(Regex.Replace(Regex.Replace(new NACCSMessageInterpreter(message).Interprete(), "(?s)<style>.*</style>", ""), "<[^<>]+>", "<<<"), "\\s+", "");
}
