using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC057C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC057CProcessor))]
	class CC057CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC057CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC057CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Invalid, movementHeader.BM_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"A Rejection from Office of Destination (IE057) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Rejection Code</td><td>22 - Rejection Code 22</td></tr><tr><td>Rejection Reason</td><td>Invalid CC057C</td></tr><tr><td>Error Pointer</td><td>Test Pointer</td></tr><tr><td>Error Code</td><td>93 - Error Code 93</td></tr><tr><td>Error Reason</td><td>Test</td></tr><tr><td>Original Attribute Value</td><td>Test Attribute Value</td></tr><tr><td>Error Pointer</td><td>Test Pointer 2</td></tr><tr><td>Error Code</td><td>14 - Error Code 14</td></tr><tr><td>Error Reason</td><td>Test 2</td></tr><tr><td>Original Attribute Value</td><td>Test Attribute Value 2</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Rejection from Office of Destination (IE057) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE057;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC057CText("21IEDUB11A782454R2");

		protected override ZString MessageFriendlyName => "CC057C: REJECTION FROM OFFICE OF DESTINATION";

		protected override CC057CProcessor Processor => new CC057CProcessor(logger, typeof(Cc057CType));

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);

			helper.CreateNewOrGetExistingCusCodeType("CL227", "Rejection Type");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL227",
				code: "22",
				description: "Rejection Code 22",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL180",
				code: "14",
				description: "Error Code 14",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: "CL180",
				code: "93",
				description: "Error Code 93",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			return base.CreateSetupData();
		}
	}
}
