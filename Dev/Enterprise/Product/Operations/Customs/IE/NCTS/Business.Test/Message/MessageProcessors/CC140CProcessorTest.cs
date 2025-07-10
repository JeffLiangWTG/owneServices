using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC140C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC140CProcessor))]
	class CC140CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC140CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC140CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"A Request on Non-Arrived Movement Message (IE140) has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Request on Non-Arrived Movement Date</td><td>18-Sep-71</td></tr><tr><td>Limit for Response Date</td><td>19-Sep-71</td></tr><tr><td>Customs Office of Enquiry at Departure</td><td>IESNN456 - Customs Office 6</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request on Non-Arrived Movement Message (IE140) has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC140C: REQUEST ON NON-ARRIVED MOVEMENT";

		protected override CC140CProcessor Processor => new CC140CProcessor(logger, typeof(Cc140CType));

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE140;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC140CText();

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "IESNN456",
				description: "Customs Office 6",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			return base.CreateSetupData();
		}
	}
}
