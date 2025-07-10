using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirCTOExportCustomsManifestHeader))]
	sealed class AirCTOExportCustomsManifestHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAbleToSaveUnrelatedLineWhenALineIsAwaitingAReceivalResponse()
		{
			header.ED_RN_NKCountryOfDestination = "NZ";
			var line = header.Lines.AddNew();
			line.EL_AirWayBill = "321";
			line.EL_CAN = "1";

			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			var manager = new AirCTOExportMessageManager(header);
			var sender = new SendsMessagesToCustomsShutterUpperer();
			sender.ReturnAllForWhichMessagesShouldWeSend = true;
			manager.SendOriginalMessages(sender);
			AssertEquals(1, line.Messages.Count);
			AssertEquals(typeof(CMRCTORECMessage), line.Messages[0].GetType());
			Factory.Save();
			AssertEquals(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, line.CTORECStatus.Code);

			var line2 = header.Lines.AddNew();
			line2.EL_AirWayBill = "123";
			line2.EL_CAN = "2";

			Customs.Business.RequiredMessagesInformation info = manager.DetermineRequiredMessagesWithPendingChanges();
			AssertEquals(false, info.HasMessagesToSend);
			AssertEquals(0, info.AllNotifications.Count);
		}

		public void TestHumanReadableName()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			header.ED_BGMReference = "K00004438";
			AssertEquals("Air CTO - Export K00004438", header.HumanReadableName);
		}

		public void TestDocumentSupporter()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			var supporter = header.DocumentSupporter;

			AssertNotNull(supporter);
			AssertEquals(typeof(AirCTOExportCustomsManifestHeaderDocumentSupporter), supporter.GetType());
		}

		public void TestDefaultValues()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			AssertEquals(Core.Constants.TransportModes.Air, header.ED_TransportMode);
			AssertEquals(true, header.ED_TransportModeInfo.ReadOnly);
			AssertEquals(AirManifestTypeList.Codes.ExportMainManifest, header.ED_ManifestType);
		}

		public void TestIMessageManageableBizObj()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			Customs.Business.IMessageManageableBizObj bizObj = header;

			AssertEquals("MessageManager", typeof(AirCTOExportMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestMessageRejection()
		{
			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ctoOrg.OH_IsUnpackDepot = true;
			ctoOrg.OH_IsAirCTO = true;
			ctoOrg.MainAddress.LocalControlledPremisesID = "DP41B";

			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			header.ED_OA_CTOAddress = ctoOrg.MainAddress.PK;
			header.ED_DepartureDate = new ZDateTime(2007, 02, 07, 12, 38, 00);
			header.ED_FlightNumber = "QF293";
			header.ED_NoOfPacks = 1000;
			header.ED_RL_NKPortOfDestination = "NZAKL";
			header.ED_RL_NKPortOfDeparture = "AUMEL";
			header.ED_RN_NKCountryOfDestination = "NZ";

			var line = header.Lines.AddNew();
			line.EL_AirWayBill = "MUH33333333";
			line.EL_CAN = "3949393";
			line.EL_GoodsDescription = "blah";
			line.EL_NumberOfPackages = 1000;
			line.EL_RN_NKCountryOfDestination = "NZ";

			Factory.Save();

			var ctoremMessage = (CMRCTOREMMessage)line.Messages.AddNew(typeof(CMRCTOREMMessage));
			ctoremMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN
BGM+111:::CTOREM+" + EDIMessage.MessageNumberPlaceHolder + @":1+9
LOC+202+DP41B::95
TDT+20+++6
CNI+1
RFF+AWB:MUH33333333
GID+1
UNT+8+1
".Replace("\r\n", "'");
			ctoremMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ctoremMessage.EM_MessageType = CMRMessage.CMRMessageTypes.CTOREM;
			ctoremMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;

			Factory.Save();

			AssertEquals(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, line.CTOREMStatus.Code);

			var message = Factory.New<CMRCTOREMRMessage>();
			message.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'BGM+961:::CTOREMR+40G6 H59E 8AF7:001+11'FTX+AHN+++REJECTED:THE TRANSACTION HAS BEEN REJECTED DUE TO ERRORS. PLEASE CORRECT AND RE-SEND THE MESSAGE.'NAD+MR+41065894724::95'RFF+ABO:" + line.EL_UserReferenceNum + @"/CMT1::001'RFF+ACW:CTOREM'RFF+AFM:9'ERP+::0001'ERC+XCL016::95'FTX+AAO+++EITHER CUSTOMS AUTHORITY NUMBER OR EXPORT DECLARATION EXEMPT CODE MUST BE PROVIDED'CNT+55:01'UNT+12+000002'";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var logger = new LoggingInformation();
			var processor = new CMRAllMessageProcessor(logger);
			processor.ProcessMessage(message);

			Factory.Save();

			AssertEquals(CMRBaseStatuses.Codes.OriginalRejected, line.CTOREMStatus.Code);
		}

		public void TestLookups()
		{
			AssertNotNull(header.Lookups);
			AssertEquals(typeof(AirCTOExportCustomsManifestHeaderLookups), header.Lookups.GetType());
		}

		public void TestManifestTypeReadOnlyWhenMessagesSent()
		{
			var line = header.Lines.AddNew();

			AssertEquals(false, header.ED_ManifestTypeInfo.ReadOnly);

			line.Messages.AddNew();

			AssertEquals(true, header.ED_ManifestTypeInfo.ReadOnly);
		}

		public void TestIsAirCTOHeader()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			AssertEquals(true, header.IsAirCTOHeader);
		}

		public void TestCTOReceivalMessage()
		{
			header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			var line = header.Lines.AddNew();

			// Make sure our validation is correct for this message
			header.Validation.ValidateAll();
			line.Validation.ValidateAll();

			AssertHasErrors(header.ED_OA_CTOAddressInfo);
			AssertHasErrors(header.ED_DepartureDateInfo);
			AssertHasMessageErrors(line.EL_AirWayBillInfo);
			AssertHasMessageErrors(line.EL_CANInfo);
			AssertHasMessageErrors(line.EL_GoodsOwnerInfo);
			AssertHasMessageErrors(line.EL_GoodsOwnerPartyIDInfo);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG1";
			org.MainAddress.LocalControlledPremisesID = "9122P";
			header.ED_OA_CTOAddress = org.MainAddress.PK;

			var today = ZDate.Today;
			header.ED_DepartureDate = today;

			line.EL_AirWayBill = "08112345616";
			line.EL_CAN = "AAACFFH9X";
			line.EL_GoodsOwner = "TEST OWNER";

			// Assert header and line are valid
			header.Validation.ValidateAll();
			line.Validation.ValidateAll();

			AssertNoNotifications(header);
			AssertNoNotifications(line);

			// Send a message
			AssertEquals(0, header.Messages.Count);
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			var manager = new AirCTOExportMessageManager(header);
			var sender = new SendsMessagesToCustomsShutterUpperer(true);
			sender.ReturnAllForWhichMessagesShouldWeSend = true;
			manager.SendOriginalMessages(sender);
			AssertEquals(1, line.Messages.Count);

			AssertContains("BGM+44:::CTOREC", line.Messages[0].EM_MessageText);

			AssertContains("LOC+202+9122P::95'TDT+20+++6'CNI+1+:::I'RFF+AWB:08112345616'GID+1'RFF+TN:AAACFFH9X'NAD+GO+++TEST OWNER'DTM+133:" + today.ToString("yyyyMMdd") + ":102'GID+1'UNT+12+", line.Messages[0].EM_MessageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AirCTOExportCustomsManifestHeader>();
		}

		AirCTOExportCustomsManifestHeader header;
	}
}
