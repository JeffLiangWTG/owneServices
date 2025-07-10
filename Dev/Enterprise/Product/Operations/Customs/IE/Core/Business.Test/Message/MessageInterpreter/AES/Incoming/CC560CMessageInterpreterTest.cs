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
	[TestedType(typeof(CC560MessageInterpreter))]
	class CC560CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC560MessageInterpreter, CC560CProvider>
	{
		public void TestGetMessageInterpretation_WithMatchingCodeAndDescriptionPairList()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "10", "Documentary controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "40", "Physical controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "50", "Other");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "0", "Control notification (and requested documents if needed)");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "1", "Additional documents request");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "2", "Intention to Control");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.SupportingDocumentExportType, "A001", "Certificate of authenticity fresh 'EMPEROR' table grapes");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.SupportingDocumentExportType, "A004", "Certificate of authenticity Tobacco");
			var message = CreateIncomingMessageToTest();
			message.EM_MessageType = MessageType;
			Factory.Save();
			var interpreter = GetInterpreter(message);
			AssertXMLEquals("Interpretation with description", GetExpectedInterpretation(" - Additional documents request", " - Physical controls", " - Certificate of authenticity fresh &#39;EMPEROR&#39; table grapes", " - Documentary controls", " - Certificate of authenticity Tobacco", " - Other"), interpreter.GetInterpretation());
		}

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE560;

		string GetExpectedInterpretation(string additionalDocumentsRequestDesc, string controlTypeDesc, string requestedDocumentTypeDesc, string controlType2Desc, string requestedDocumentType2Desc, string controlType3Desc) => $@"An Export Control message has been received from Customs for Job B00000111 through the IE560 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Controlled for Export (CON1)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDU4EX144268149</td></tr><tr><td>Control Notification Date &amp; Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Control Notification Type</td><td>1{additionalDocumentsRequestDesc}</td></tr><tr><td>Notification Text</td><td>Ex Op Control Text</td></tr><tr><td>Control Type</td><td>40{controlTypeDesc}</td></tr><tr><td>Control Text</td><td>Documentary Control Type 1</td></tr><tr><td>Requested Document Type</td><td>A001{requestedDocumentTypeDesc}</td></tr><tr><td>Requested Document Description</td><td>Commercial Invoice</td></tr><tr><td>Control Type</td><td>10{controlType2Desc}</td></tr><tr><td>Control Text</td><td>Documentary Control Type 2</td></tr><tr><td>Requested Document Type</td><td>A004{requestedDocumentType2Desc}</td></tr><tr><td>Requested Document Description</td><td>Air waybill</td></tr><tr><td>Control Type</td><td>50{controlType3Desc}</td></tr><tr><td>Control Text</td><td>Documentary Control Type 3</td></tr></table>";
		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => GetExpectedInterpretation(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000111";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc560Text = AESInterchangeProcessorTestHelper.GetStandardCC560CText("LRN123456789", "21IEDU4EX144268149");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc560Text, includeResponseWrap: false);

			return message;
		}

		protected override CC560CProvider GetProvider(TextReader reader) => new CC560CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC560C.Cc560C>(reader).Message);
	}
}
