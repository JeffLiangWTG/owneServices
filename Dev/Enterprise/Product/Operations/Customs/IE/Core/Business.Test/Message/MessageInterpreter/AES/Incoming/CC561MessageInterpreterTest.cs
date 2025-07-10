using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC561MessageInterpreter))]
	class CC561MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC561MessageInterpreter, CC561CProvider>
	{
		public void TestGetMessageInterpretation_WithMatchingCodeAndDescriptionPairList()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "10", "Documentary controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "20", "Documentary controls 2");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "40", "Physical controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "50", "Other");
			var message = CreateIncomingMessageToTest();
			message.EM_MessageType = MessageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			var interpreter = GetInterpreter(message);
			AssertXMLEquals("Interpretation with description", GetExpectedInterpretation(" - Documentary controls", " - Documentary controls 2", " - Other"), interpreter.GetInterpretation());
		}

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE561;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000561";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc561Text = AESInterchangeProcessorTestHelper.GetStandardAESCC561CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc561Text, includeResponseWrap: false);
			return message;
		}

		string GetExpectedInterpretation(string controlTypeDesc, string controlType2Desc, string controlType3Desc) => $@"An Exit Control Decision Notification message has been received from Customs for Job B00000561 through the IE561 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Control Notification Date &amp; Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Control Type</td><td>10{controlTypeDesc}</td></tr><tr><td>Control Text</td><td>Documentary Control Type 1</td></tr><tr><td>Control Type</td><td>20{controlType2Desc}</td></tr><tr><td>Control Text</td><td>Documentary Control Type 2</td></tr><tr><td>Control Type</td><td>50{controlType3Desc}</td></tr><tr><td>Control Text</td><td>Documentary Control Type 3</td></tr></table>";
		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => GetExpectedInterpretation(string.Empty, string.Empty, string.Empty);

		protected override CC561CProvider GetProvider(TextReader reader) => new CC561CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC561C.Cc561C>(reader).Message);
	}
}
