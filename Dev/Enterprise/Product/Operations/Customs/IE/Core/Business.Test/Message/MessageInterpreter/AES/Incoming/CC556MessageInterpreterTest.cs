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
	[TestedType(typeof(CC556MessageInterpreter))]
	class CC556MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC556MessageInterpreter, CC556CProvider>
	{
		public void TestGetMessageInterpretation_WithMatchingCodeAndDescriptionPairList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL560", "Business Rejection Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL560", "ERR", "Test 560 Rejection Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "12", "Error Code Item12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var message = CreateIncomingMessageToTest();
			message.EM_MessageType = MessageType;
			Factory.Save();
			var interpreter = GetInterpreter(message);
			AssertXMLEquals("Interpretation with description", GetExpectedInterpretation(" - Test 560 Rejection Type", " - Error Code Item12"), interpreter.GetInterpretation());
		}

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE556;

		string GetExpectedInterpretation(string businessRejectionTypeDesc, string errorCodeDesc) => $@"A Rejection message has been received from Customs for Job B00000012 through the IE556 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Business Rejection Type</td><td>ERR{businessRejectionTypeDesc}</td></tr><tr><td>Rejection Date and Time</td><td>22-Feb-22 22:00</td></tr><tr><td>Rejection Code</td><td>01</td></tr><tr><td>Rejection Reason</td><td>Test Reason</td></tr><tr><td>Error Pointer</td><td>Error Pointer 1</td></tr><tr><td>Error Code</td><td>12{errorCodeDesc}</td></tr><tr><td>Error Reason</td><td>REASON1</td></tr></table>";
		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => GetExpectedInterpretation(string.Empty, string.Empty);

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc556Text = AESInterchangeProcessorTestHelper.GetStandardCC556CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc556Text, includeResponseWrap: false);

			return message;
		}

		protected override CC556CProvider GetProvider(TextReader reader) => new CC556CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC556C.Cc556C>(reader).Message);
	}
}
