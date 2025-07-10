using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC557MessageInterpreter))]
	class CC557MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC557MessageInterpreter, CC557CProvider>
	{
		public void TestGetMessageInterpretation_WithMatchingCodeAndDescriptionPairList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL570", "Business Rejection Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL570", "557", "Test 570 Rejection Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "12", "Error Code Item12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var message = CreateIncomingMessageToTest();
			message.EM_MessageType = MessageType;
			Factory.Save();
			var interpreter = GetInterpreter(message);
			AssertXMLEquals("Interpretation with description", GetExpectedInterpretation(" - Test 570 Rejection Type", " - Error Code Item12"), interpreter.GetInterpretation());
		}

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE557;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000557";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc557Text = AESInterchangeProcessorTestHelper.GetStandardAESCC557CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc557Text, includeResponseWrap: false);
			return message;
		}

		string GetExpectedInterpretation(string businessRejectionTypeDesc, string errorCodeDesc) => $@"A rejection message from office of Exit has been received from Customs for Job B00000557 through the IE557 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>REJ - Rejected</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Business Rejection Type</td><td>557{businessRejectionTypeDesc}</td></tr><tr><td>Rejection Date and Time</td><td>01-Jul-22 00:00</td></tr><tr><td>Rejection Code</td><td>10</td></tr><tr><td>Rejection Reason</td><td>Reason 1</td></tr><tr><td>Error Pointer</td><td>Error Pointer 1</td></tr><tr><td>Error Code</td><td>12{errorCodeDesc}</td></tr><tr><td>Error Reason</td><td>REASON1</td></tr></table>";
		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => GetExpectedInterpretation(string.Empty, string.Empty);

		protected override CC557CProvider GetProvider(TextReader reader) => new CC557CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC557C.Cc557C>(reader).Message);
	}
}
