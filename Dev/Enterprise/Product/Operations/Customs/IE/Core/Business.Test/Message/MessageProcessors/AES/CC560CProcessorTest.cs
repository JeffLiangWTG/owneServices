using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC560C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC560CProcessor))]
	class CC560CProcessorTest : EntryHeaderMessageProcessorTest<CC560CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC560CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("CH_EntryStatus", AESEntryStatusList.Codes.ControlledForExport, entry.CH_EntryStatus);

			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"An Export Control message has been received from Customs for Job {jobNumber} through the IE560 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Controlled for Export (CON1)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Control Notification Date &amp; Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Control Notification Type</td><td>1 - Additional documents request</td></tr><tr><td>Notification Text</td><td>Ex Op Control Text</td></tr><tr><td>Control Type</td><td>40 - Physical controls</td></tr><tr><td>Control Text</td><td>Documentary Control Type 1</td></tr><tr><td>Requested Document Type</td><td>A001 - Certificate of authenticity fresh &#39;EMPEROR&#39; table grapes</td></tr><tr><td>Requested Document Description</td><td>Commercial Invoice</td></tr><tr><td>Control Type</td><td>10 - Documentary controls</td></tr><tr><td>Control Text</td><td>Documentary Control Type 2</td></tr><tr><td>Requested Document Type</td><td>A004 - Certificate of authenticity Tobacco</td></tr><tr><td>Requested Document Description</td><td>Air waybill</td></tr><tr><td>Control Type</td><td>50 - Other</td></tr><tr><td>Control Text</td><td>Documentary Control Type 3</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "10", "Documentary controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "40", "Physical controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "50", "Other");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "0", "Control notification (and requested documents if needed)");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "1", "Additional documents request");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "2", "Intention to Control");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.SupportingDocumentExportType, "A001", "Certificate of authenticity fresh 'EMPEROR' table grapes");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.SupportingDocumentExportType, "A004", "Certificate of authenticity Tobacco");
			Factory.Save();
			return base.CreateSetupData();
		}

		protected override ZString MessageFriendlyName => "CC560C: EXPORT CONTROL DECISION NOTIFICATION";

		protected override CC560CProcessor Processor => new CC560CProcessor(logger, typeof(Cc560C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE560;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC560CText("LRN123456789", "21IEDUB11A782454R2");
	}

	[TestedType(typeof(CC560CProcessor))]
	class CC560_Type2_ProcessorTest : CC560CProcessorTest
	{
		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC560C_Type2Text("LRN123456789", "21IEDUB11A782454R2");
		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "10", "Documentary controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "40", "Physical controls");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, "50", "Other");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "0", "Control notification (and requested documents if needed)");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "1", "Additional documents request");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType, "2", "Intention to Control");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.SupportingDocumentExportType, "A001", "Certificate of authenticity fresh 'EMPEROR' table grapes");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.SupportingDocumentExportType, "A004", "Certificate of authenticity Tobacco");
			Factory.Save();
			return base.CreateSetupData();
		}
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("CH_EntryStatus", AESEntryStatusList.Codes.PendingControl, entry.CH_EntryStatus);
			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"An Export Control message has been received from Customs for Job {jobNumber} through the IE560 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Controlled for Export (CON1)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Control Notification Date &amp; Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Control Notification Type</td><td>2 - Intention to Control</td></tr><tr><td>Notification Text</td><td>Ex Op Control Text</td></tr><tr><td>Control Type</td><td>10 - Documentary controls</td></tr><tr><td>Control Text</td><td>Documentary Control Type 1</td></tr><tr><td>Requested Document Type</td><td>A001 - Certificate of authenticity fresh &#39;EMPEROR&#39; table grapes</td></tr><tr><td>Requested Document Description</td><td>Commercial Invoice</td></tr><tr><td>Control Type</td><td>10 - Documentary controls</td></tr><tr><td>Control Text</td><td>Documentary Control Type 2</td></tr><tr><td>Requested Document Type</td><td>A004 - Certificate of authenticity Tobacco</td></tr><tr><td>Requested Document Description</td><td>Air waybill</td></tr><tr><td>Control Type</td><td>50 - Other</td></tr><tr><td>Control Text</td><td>Documentary Control Type 3</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}
	}
}
