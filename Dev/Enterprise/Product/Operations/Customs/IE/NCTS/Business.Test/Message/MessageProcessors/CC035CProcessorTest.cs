using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC035C;
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
	[TestedType(typeof(CC035CProcessor))]
	class CC035CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC035CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC035CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure, messageAttachee.BM_CustomsStatus);
			AssertMessageInterpretation(incomingMessage, @"Recovery Notification Message (IE035) has been received. A Competent Authority of Recovery has initiated Recovery of Transit Declaration for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Accepted Date</td><td>18-Sep-71</td></tr><tr><td>Recovery Notification Date</td><td>18-Sep-71</td></tr><tr><td>Recovery Notification Text</td><td>Test Recovery Notification</td></tr><tr><td>Amount Claimed</td><td>22.22IEC</td></tr><tr><td>Customs Office of Recovery at Departure</td><td>IE000001 - Customs Office 1</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "Recovery Notification Message (IE035) has been received. A Competent Authority of Recovery has initiated Recovery of Transit Declaration for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE035;

		protected override ZString MessageFriendlyName => "CC035C: RECOVERY NOTIFICATION";

		protected override CC035CProcessor Processor => new CC035CProcessor(logger, typeof(Cc035CType));

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC035CText("21IEDUB11A782454R2");

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeList = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "IE000001",
				description: "Customs Office 1",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			return base.CreateSetupData();
		}
	}
}
