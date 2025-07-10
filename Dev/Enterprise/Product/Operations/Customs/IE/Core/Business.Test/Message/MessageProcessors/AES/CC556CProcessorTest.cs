using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC556C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC556CProcessor))]
	class CC556CProcessorTest : EntryHeaderMessageProcessorTest<CC556CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC556CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Not change Entry Status", string.Empty, entry.CH_EntryStatus);
			AssertEquals("Expected Logical Status Invalid", LogicalStatusList.Codes.Invalid, entry.CH_Status);

			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Rejection message has been received from Customs for Job {jobNumber} through the IE556 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Business Rejection Type</td><td>ERR - Test 560 Rejection Type</td></tr><tr><td>Rejection Date and Time</td><td>22-Feb-22 22:00</td></tr><tr><td>Rejection Code</td><td>01</td></tr><tr><td>Rejection Reason</td><td>Test Reason</td></tr><tr><td>Error Pointer</td><td>Error Pointer 1</td></tr><tr><td>Error Code</td><td>12 - ErrorCodeItem12</td></tr><tr><td>Error Reason</td><td>REASON1</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC556C: REJECTION FROM OFFICE OF EXPORT";

		protected override CC556CProcessor Processor => new CC556CProcessor(logger, typeof(Cc556C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE556;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC556CText("LRN123456789", "21IEDUB11A782454R2");

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL560", "Business Rejection Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL560", "ERR", "Test 560 Rejection Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "12", "ErrorCodeItem12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "13", "ErrorCodeItem13", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "14", "ErrorCodeItem14", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			return base.CreateSetupData();
		}
	}
}
