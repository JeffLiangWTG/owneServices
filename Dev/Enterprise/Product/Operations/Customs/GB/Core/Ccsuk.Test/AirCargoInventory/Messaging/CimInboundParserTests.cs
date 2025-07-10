using System;
using System.Data;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.InboundParsersTests.Testing
{
	class CimInboundParserTests : CcsukNonChiefResponseBaseMessageProcessorTest
	{
		public void TestFsnProcessingFindsMawbInAnotherCompany()
		{
			// CS00312384
			var companyMAN = Factory.New<GlbCompany>();
			companyMAN.GC_Code = "MAN";
			companyMAN.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var branchMAN = companyMAN.Branches.AddNew();
			branchMAN.GB_RL_NKHomePort = "GBMNC";
			branchMAN.GB_Code = "MAN";

			var companyLHR = Factory.New<GlbCompany>();
			companyLHR.GC_Code = "LHR";
			companyLHR.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var branchLHR = companyLHR.Branches.AddNew();
			branchLHR.GB_RL_NKHomePort = "GBLHR";
			branchLHR.GB_Code = "LHR";
			Factory.Save();

			CusMAWB mawbSavedAsLHR;
			using (DisposableEnvironment.ForBranch(branchLHR.PK.ToGuid()))
			{
				mawbSavedAsLHR = Factory.New<CusMAWB>();
				mawbSavedAsLHR.CM_MAWB = "050-42011002";
				mawbSavedAsLHR.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRVIO";
			}

			Factory.Save();

			using (DisposableEnvironment.ForBranch(branchMAN.PK.ToGuid()))
			{
				var mockMessage = Factory.NewMoq<EDIMessage>();
				var message = mockMessage.Object;
				message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageText = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002:CSN/CA/1/05APR1149/RGHIMP1/HOLD ENTRY'UNT+3+09864713754600'";
				Factory.Save();
				RunProcessors();
				mockMessage.VerifyAll();
			}
			mawbSavedAsLHR.Reload();
			AssertEquals("The MAWB is owned by the LHR company and the message and task run as the MAN company, but the MAWB should still be found and updated",
							"CA", mawbSavedAsLHR.CustomsActionCode);
		}

		public void TestFRD_BasicWithoutPiecesReceived()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var basic = SetUpJobForFrdTestAndRunTask(frdBasic, false);
			AssertFcsMessageCreatedFromInboundFrd(@"GID+01'QTY+118:10'MEA+WT++KGM:33'
													  GID+02'QTY+118:10'MEA+WT++KGM:80'
													  GID+03'QTY+118:20'MEA+WT++KGM:10", basic);
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(3, basic.Splits.Count);
			AssertEquals("No pieces received on AWB, so none on split", 0, (int)basic.Splits["01"].NumberOfPiecesReceived);
			AssertEquals("No pieces received on AWB, so none on split", 0, (int)basic.Splits["02"].NumberOfPiecesReceived);
			AssertEquals("No pieces received on AWB, so none on split", 0, (int)basic.Splits["03"].NumberOfPiecesReceived);
			AssertEquals(10, (int)basic.Splits["01"].NumberOfPiecesExpected);
			AssertEquals(10, (int)basic.Splits["02"].NumberOfPiecesExpected);
			AssertEquals(20, (int)basic.Splits["03"].NumberOfPiecesExpected);
			AssertContains("Handling info is recorded", "ADVERTISING", basic.Splits["01"].HandlingInformation);
			AssertContains("Handling info is recorded", "DEMO", basic.Splits["02"].HandlingInformation);
			AssertContains("Handling info is recorded", "SPARE PARTS", basic.Splits["03"].HandlingInformation);
			var inboundMessage = basic.Messages[0];
			AssertEquals("RCV", inboundMessage.EM_Status);
			AssertEquals(EDIInterchange.Status.Received, inboundMessage.Interchange.EI_Status);
			AssertContains("   MKD ADVERTISING MATERIAL - 1 THRU 10", inboundMessage.EM_MessageInterpretation);
		}

		public void TestFRD_House()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var house = SetUpJobForFrdTestAndRunTask(frdHouse, true);
			AssertFcsMessageCreatedFromInboundFrd(@"GID+01'QTY+118:10'MEA+WT++KGM:33'
													  GID+02'QTY+118:10'MEA+WT++KGM:80'
													  GID+03'QTY+118:20'MEA+WT++KGM:10", house);
			var inboundMessage = house.Messages[0];
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestFRD_NoSplitsExistButHasStatus1()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var house = SetUpJobForFrdTestAndRunTask(frdArrivedHouse, createHawbToo: true, createSplitsToo: false, setStatus1: true);
			house = new BusinessObjectFactory().Load<CusHAWB>(house.PK);
			AssertEquals("FRD assigns NPR to split", (ZShort)10, house.Splits["01"].NumberOfPiecesReceived);
			AssertEquals("FRD assigns NPR to split", (ZShort)10, house.Splits["02"].NumberOfPiecesReceived);
			AssertEquals("FRD assigns NPR to split", (ZShort)15, house.Splits["03"].NumberOfPiecesReceived);
			AssertEquals("MKD ADVERTISING MATERIAL - 1 THRU 10", house.Splits["01"].HandlingInformation);
			AssertEquals("MKD DEMO MODELS - 11 THRU 20", house.Splits["02"].HandlingInformation);
			AssertEquals("BOX SPARE PARTS - 21 THRU 35", house.Splits["03"].HandlingInformation);
			var inboundMessage = house.Messages.LastIncomingMessage;
			AssertEquals("RCV", inboundMessage.EM_Status);
			var fcs = house.Messages.LastOutgoingMessage;
			AssertEquals("QUE", fcs.EM_Status);
			AssertContains("BGM+:::FCS+01512345675+++HWB:09124361+50:8603120427:201'GIS+S2Y'GIS+P:121'TDT+20'LOC+11:LHR:145:3::TWA:129:ZZZ'GID+01'QTY+118:10'MEA+WT++KGM:33'GID+02'QTY+118:10'MEA+WT++KGM:80'GID+03'QTY+118:15'MEA+WT++KGM:10'UNT+16+1'", fcs.EM_MessageText);
		}

		public void TestFRD_ArrivalSplitCountMismatch()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var house = SetUpJobForFrdTestAndRunTask(frdArrivedHouseWithFourSplits, true, true);
			AssertEquals(2, house.Messages.Count);
			var outboundControl = house.Messages[1];
			AssertEquals("UNH+1+CONTRL:1:912:UN'UCI+833856+CUKCTM9800120Z+CUKAIR98LHRXXX/+4'UCM+MSGREF+CIMFRD:0:0:IA+4'UCX+4+6'FTX+AAA+++CHANGES TO EXISTING SPLITS REJECTED. NUMBER OF SPLITS IN FRD (4) GREAT:ER THAN SPLITS ON FILE (3).  TO FIX CORRECT THE NUMBER OF SPLITS OR DO: NOT SEND FLIGHT DATA. CONTACT SHED OPERATOR ON PH 07 3268 2903  FAX'UNT+6+1'", outboundControl.EM_MessageText);
			var inboundMessage = house.Messages[0];
			AssertEquals("ERR", inboundMessage.EM_Status);
		}

		public void TestFRD_ParseError()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var house = SetUpJobForFrdTestAndRunTask(frdCauseParserError, true);
			AssertEquals(2, house.Messages.Count);
			var outboundContrl = house.Messages[1];
			AssertEquals("UNH+1+CONTRL:1:912:UN'UCI+833856+CUKCTM9800120Z+CUKAIR98LHRXXX/+4'UCM+MSGREF+CIMFRD:0:0:IA+4'UCX+4+6'FTX+AAA+++REQUEST REJECTED, NO SPLIT LINES PRESENT IN FRD MESSAGE OR ERROR PARSI:NG MESSAGE. CONTACT SHED OPERATOR ON PH 07 3268 2903  FAX'UNT+6+1'", outboundContrl.EM_MessageText);
			AssertContains("Could not split inbound CIMFRD message, text parsing error", ErrorReporter.LastMessageReported);
			var inboundMessage = house.Messages[0];
			AssertEquals("ERR", inboundMessage.EM_Status);
			ErrorReporter.Clear();
		}

		public void TestFRD_ArrivalSplitReferenceNotFound()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var house = SetUpJobForFrdTestAndRunTask(frdArrivedHouseOneBogusSplit, true, true);
			AssertEquals(2, house.Messages.Count);
			var outboundControl = house.Messages[1];
			AssertEquals("UNH+1+CONTRL:1:912:UN'UCI+833856+CUKCTM9800120Z+CUKAIR98LHRXXX/+4'UCM+MSGREF+CIMFRD:0:0:IA+4'UCX+4+6'FTX+AAA+++CANNOT UPDATE NPR ON SPLIT THE FOLLOWING SPLITS.  NOT FOUND IN LOCAL D:ATABASE. NO RECORDS UPDATED. SPLIT NUMBER(S) 99. CONTACT SHED OPERATOR: ON PH 07 3268 2903  FAX'UNT+6+1'", outboundControl.EM_MessageText);
			var inboundMessage = house.Messages[0];
			AssertEquals("ERR", inboundMessage.EM_Status);
		}

		public void TestFRD_NoAutoSplitEmailOnly()
		{
			var basic = SetUpJobForFrdTestAndRunTask(frdBasic, false);
			AssertEquals("No outbound message added to awb", 1, basic.Messages.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(e => e.Recipients.RecipientsAsDelimitedString().Contains("wisetechglobal"));
			AssertContains("An inbound FRD message was received", email.Body);
			var inboundMessage = basic.Messages[0];
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		public void TestFRD_NoAutoSplitEmailOnlyNotificationsGoToRecipientForSpecificBranch()
		{
			string notificationEmailAddress = null;
			notificationEmailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactCimFrd, GlbBranch.CurrentBranch.PK.ToGuid(), Factory);
			var basic = SetUpJobForFrdTestAndRunTask(frdBasic, false);
			AssertContains("Email recipient for branch", notificationEmailAddress, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		public void TestFRD_NoRecordFound()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var house = SetUpJobForFrdTestAndRunTask(frdNoRecordFound, true);
			AssertEquals(0, house.Messages.Count);
			var outboundContrl = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit));
			AssertEquals("UNH+1+CONTRL:1:912:UN+COMREF1'UCI+833856+CUKCTM9800120Z+CUKAIR98LHRXXX/+4'UCM+MSGREF+CIMFRD:0:0:IA+4'UCX+4+6'FTX+AAA+++CONSIGNMENT NOT FOUND.'UNT+6+1'", outboundContrl.EM_MessageText);
			AssertEquals(0, house.Messages.Count);
			var receivedMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive));
			AssertEquals("", receivedMessage.EM_ApplicationReference);
		}

		public void TestFRD_ProfileNotShedProfile()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var basic = SetUpJobForFrdTestAndRunTask(frdBasic, false, false, "CUKFFW98000DAN");  // agent profile
			AssertEquals(2, basic.Messages.Count);  // FRD and CONTRL
			var outboundContrl = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit));
			AssertEquals("UNH+1+CONTRL:1:912:UN+COMREF1'UCI+833856+CUKCTM9800120Z+CUKAIR98LHRXXX/+4'UCM+MSGREF+CIMFRD:0:0:IA+4'UCX+4+6'FTX+AAA+++CANNOT SPLIT, THIS CONSIGNMENT IS NOT REGISTERED AS A SHED JOB. CONTAC:T SHED OPERATOR ON PH 07 3268 2903  FAX'UNT+6+1'", outboundContrl.EM_MessageText);
		}

		public void TestFSR_MatchingMultipleMAWBs_ShedPIMA()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic1 = Factory.New<CusMAWB>();
			basic1.Profile = "CUKAIR98LHRBAC";
			basic1.CM_MAWB = "111-19051130";

			var basic2 = Factory.New<CusMAWB>();
			basic2.Profile = "CUKAIR98LHRXXX";
			basic2.CM_MAWB = "111-19051130";

			var fsrInterchangeText = "UNB+UNOA:2+CUKCTM98001120:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:111-19051130'UNT+3+833856'UNZ+1+833856'";
			_ = EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();
			var newFactory = new BusinessObjectFactory();

			basic1 = newFactory.Load<CusMAWB>(basic1.PK);
			AssertEquals(0, basic1.Messages.Count);

			basic2 = newFactory.Load<CusMAWB>(basic2.PK);
			AssertEquals(2, basic2.Messages.Count);
			AssertEquals("QUE", basic2.Messages.LastOutgoingMessage.EM_Status);
			AssertEquals("RCV", basic2.Messages.LastIncomingMessage.EM_Status);
			AssertEquals("CUKAIR98LHRXXX", basic2.Messages.LastOutgoingMessage.EM_MessageOwner); // FROM
			AssertEquals("CUKCTM98001120", basic2.Messages.LastOutgoingMessage.EM_ApplicationReference); // TO
		}

		public void TestFSR_MatchingMultipleMAWBs_AgentPIMA()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "413-05091311";
			mawb1.Profile = "CUKFFW98000XXX";

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "413-05091311";
			mawb2.Profile = "CUKFFW98000WIS";

			var interchangeText = "UNB+UNOA:2+CUKAIR98LHRCAX:IATA+CUKFFW98000WIS:IATA+171215:0737+3649'UNH+3631+CIMFSR:0:0:IA+4B68AE10FFD048D3A56E05AD21E3D60B'FTX+CIM+++FSR:413-05091311'UNT+3+3631'UNZ+1+3649'";
			_ = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, "CUK");
			Factory.Save();
			RunProcessors();

			var newFactory = new BusinessObjectFactory();
			mawb1 = newFactory.Load<CusMAWB>(mawb1.PK);
			AssertEquals(0, mawb1.Messages.Count);

			mawb2 = newFactory.Load<CusMAWB>(mawb2.PK);
			AssertEquals(2, mawb2.Messages.Count);
			AssertEquals("QUE", mawb2.Messages.LastOutgoingMessage.EM_Status);
			AssertEquals("RCV", mawb2.Messages.LastIncomingMessage.EM_Status);
			AssertEquals("CUKFFW98000WIS", mawb2.Messages.LastOutgoingMessage.EM_MessageOwner); // FROM
			AssertEquals("CUKAIR98LHRCAX", mawb2.Messages.LastOutgoingMessage.EM_ApplicationReference); // TO
		}

		public void TestFSR_Basic()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.CM_MAWB = "111-19051130";
			basic.ShipmentDescriptionCode = "T";
			basic.CM_RL_NKLoadPort = "USATL";
			basic.CM_RL_NKDischargePort = "GBLGW";
			basic.NumberOfPiecesExpected = 70;
			basic.OutTurns.AddNew().C5_PackagesOutturned = 10;
			basic.SetCustomsActionCode("CC", new ZDateTime(2000, 7, 18, 21, 12, 34));
			basic.LatestCustomsActionText = "CUSTOMS CLEARED";

			var fsrInterchangeText = "UNB+UNOA:2+CUKCTM98001120:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:111-19051130'UNT+3+833856'UNZ+1+833856'";
			_ = EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();

			var basicReloadedBecauseCaching = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(2, basicReloadedBecauseCaching.Messages.Count);
			AssertContains("FSR:111-19051130", basicReloadedBecauseCaching.Messages.LastIncomingMessage.EM_MessageText);
			AssertContains("Common Access Reference number 833856 is missing from the outgoing FSA message", "UNH+1+CIMFSA:2:0:IA+833856'FTX+", basicReloadedBecauseCaching.Messages.LastOutgoingMessage.EM_MessageText);
			AssertContains("FSA/2:111-19051130ATLLGW/P10T70:OSI/SDC T CC CUSTOMS CLEARED 18 JUL 2112 NPR=10'", basicReloadedBecauseCaching.Messages.LastOutgoingMessage.EM_MessageText);
			AssertEquals("QUE", basicReloadedBecauseCaching.Messages.LastOutgoingMessage.EM_Status);
			AssertEquals("RCV", basicReloadedBecauseCaching.Messages.LastIncomingMessage.EM_Status);
			AssertContains("<style>", basicReloadedBecauseCaching.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertContains("CUKCTM98001120", basicReloadedBecauseCaching.Messages.LastOutgoingMessage.EM_ApplicationReference);
		}

		public void TestFSR_Basic_Phantom()
		{
			// AWB exists locally but incorrectly (!) has no PIMA.  Check we can still make a message with the right EM_MessageOwner even if the AWB is question
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "";  // tsk tsk
			basic.CM_MAWB = "111-19051130";
			basic.CM_RL_NKLoadPort = "USATL";
			basic.CM_RL_NKDischargePort = "GBLGW";

			var fsrInterchangeText = "UNB+UNOA:2+CUKCTM98001120:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:111-19051130'UNT+3+833856'UNZ+1+833856'";
			_ = EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();

			var basicReloadedBecauseCaching = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(2, basicReloadedBecauseCaching.Messages.Count);
			AssertEquals("CUKAIR98LHRXXX", basicReloadedBecauseCaching.Messages.LastOutgoingMessage.EM_MessageOwner); // FROM
			AssertEquals("CUKCTM98001120", basicReloadedBecauseCaching.Messages.LastOutgoingMessage.EM_ApplicationReference); // TO

			// Run task again and check that the packaging is correct
			RunProcessors(true);
			var basicReloadedBecauseCaching2 = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			var interchageCreated = basicReloadedBecauseCaching2.Messages.LastOutgoingMessage.Interchange;
			AssertEquals("CUKAIR98LHRXXX", interchageCreated.EI_From);
			AssertEquals("CUKCTM98001120", interchageCreated.EI_To);
			AssertContains("UNB+UNOA:2+CUKAIR98LHRXXX:IATA+CUKCTM98001120:IATA", interchageCreated.EI_HeaderText);
		}

		public void TestFSR_House()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKAIR98LHRBAC";
			mawb.CM_MAWB = "111-19051130";
			mawb.ShipmentDescriptionCode = "T";
			mawb.CM_RL_NKLoadPort = "USATL";
			mawb.CM_RL_NKDischargePort = "GBLGW";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			hawb.CS_PiecesManifested = 70;
			hawb.OutTurns.AddNew().C5_PackagesOutturned = 10;
			hawb.CS_RL_NKLoadPort = "USATL";

			var fsrInterchangeText = "UNB+UNOA:2+CUKCTM9800120Z:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:HWB-87654321'UNT+3+833856'UNZ+1+833856'";
			_ = EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();

			var houseReloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals(2, houseReloaded.Messages.Count);
			AssertContains("FSR:HWB-87654321", houseReloaded.Messages.LastIncomingMessage.EM_MessageText);
			AssertContains("Common Access Reference number 833856 is missing from the outgoing FSA message", "UNH+1+CIMFSA:2:0:IA+833856'FTX+", houseReloaded.Messages.LastOutgoingMessage.EM_MessageText);
			AssertContains("FSA/2:111-19051130-87654321ATLLGW/P10T70:OSI/SDC T NPR=10'UNT", houseReloaded.Messages.LastOutgoingMessage.EM_MessageText);
			AssertEquals("QUE", houseReloaded.Messages.LastOutgoingMessage.EM_Status);
			AssertEquals("RCV", houseReloaded.Messages.LastIncomingMessage.EM_Status);
			AssertContains("<style>", houseReloaded.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertContains("CUKCTM9800120Z", houseReloaded.Messages.LastOutgoingMessage.EM_ApplicationReference);
		}

		public void TestFSR_NoRecordFound()
		{
			var fsrInterchangeText = "UNB+UNOA:2+CUKAIR98LHRJTS:IATA+CUKAIR98LHRCAX:IATA+120612:1335+1039F133537000+++C'UNH+1039F133537001+CIMFSR:0:0:Z1:IATA+COMMONACCESS'FTX+CIM+++FSR:120-62012009'UNT+3+1039F133537001'UNZ+1+1039F133537000'";
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();

			var inboundMessage = interchange.ContainedMessages[0];
			inboundMessage.Reload();
			var sentMessage = Factory.Load<EDIMessage>(inboundMessage.EM_LinkUniqueID);
			AssertContains("Response contains original CAR and reports no record found", "CIMFSA:2:0:IA+COMMONACCESS'FTX+CIM+++FSA/2:120-62012009:OSI/NO RECORD FOUND", sentMessage.EM_MessageText);
			AssertEquals("Sender of message taken from inboundinterchange", "CUKAIR98LHRCAX", sentMessage.EM_MessageOwner);
			AssertEquals("CUK", sentMessage.EM_ApplicationCode);
			AssertEquals("Recipient of message is original sender", "CUKAIR98LHRJTS", sentMessage.EM_ApplicationReference);
			AssertEquals("CIMFSA", sentMessage.EM_MessageType + sentMessage.EM_MessageSubType);
			AssertEquals("QUE", sentMessage.EM_Status);
			AssertEquals("TRX", sentMessage.EM_ReceiveTransmit);

			AssertEquals("CIMFSR", inboundMessage.EM_MessageType + inboundMessage.EM_MessageSubType);
			AssertEquals("RCV", inboundMessage.EM_Status);
			AssertEquals("RCV", inboundMessage.EM_ReceiveTransmit);
		}

		[TestDate(2011, 7, 22, 22, 53, 0)]
		public void TestFSR_BasicWithSplitsGetsFsaAndGenral()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsPimaOrTerminalAddress, "PIMA for customs");
			var pima1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsPimaOrTerminalAddress, "CUKCTM98001120/DAN69", "Daniel XXX 69", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.CM_MAWB = "111-19051130";
			basic.ShipmentDescriptionCode = "T";
			basic.CM_RL_NKLoadPort = "USATL";
			basic.CM_RL_NKDischargePort = "GBLGW";
			basic.NumberOfPiecesExpected = 70;
			var split1 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split1.NumberOfPiecesExpected = 111;
			split1.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			var split2 = basic.Splits.AddNew();
			split2.SplitReference = "02";
			split2.NumberOfPiecesExpected = 222;
			var receiptAndRelivery = basic.OutTurns.AddNew();
			receiptAndRelivery.SplitReferenceToWhichThisPertains = "01";
			receiptAndRelivery.C5_PackagesOutturned = 9;
			receiptAndRelivery.IsDelivered = true;
			var receipt = basic.OutTurns.AddNew();
			receipt.SplitReferenceToWhichThisPertains = "01";
			receipt.C5_PackagesOutturned = 1;
			receipt.IsDelivered = false;

			var fsrInterchangeText = "UNB+UNOA:2+CUKCTM98001120:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:111-19051130'UNT+3+833856'UNZ+1+833856'";
			_ = EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();

			var basicReloadedBecauseCaching = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("Three messages - FSR, FSA, Genral", 3, basicReloadedBecauseCaching.Messages.Count);
			var fsaResponseMessage = (EDIMessage)basicReloadedBecauseCaching.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageSubType, "FSA")).FirstOrDefault();
			var genralResponseMessage = (EDIMessage)basicReloadedBecauseCaching.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageType, "GEN")).FirstOrDefault();
			AssertContains("UNH+1+CIMFSA:2:0:IA+833856'FTX+CIM+++FSA/2:111-19051130ATLLGW/P10T70:OSI", fsaResponseMessage.EM_MessageText);
			AssertContains("SDC=T BASIC CONSIGNMENT WITH SPLITS, SEE PRINT SPOOL", fsaResponseMessage.EM_MessageText);
			AssertContains("UNH+2+GENRAL:0:912:UN+833856'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++FSR RESPONSE FOR", genralResponseMessage.EM_MessageText);
			AssertContains("FSR response for: 111-19051130\r\nFrom:LHRXXX                                     sent: 22/07/2011 22:53\r\n\r\nSTART response to RS interrogation 111-19051130 from LHRXXX\r\nHAWB     SRF NPX NPR NPD DESC                GWT    AGT CAC DATE     EC\r\n(basic)  01  111 10  9                                  CT  19710918   \r\n(basic)  02  222 0   0                                                 \r\nPAGE 01/END", genralResponseMessage.EM_MessageInterpretation);

			AssertEquals("CUKCTM98001120", fsaResponseMessage.EM_ApplicationReference);
			AssertEquals("CUKCTM98001120/DAN69", genralResponseMessage.EM_ApplicationReference);
		}

		[TestDate(2011, 7, 22, 22, 53, 0)]
		public void TestFSR_HouseWithSplitsGetsFsaAndGenral()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "111-19051130";
			mawb.ShipmentDescriptionCode = "M";
			mawb.CM_RL_NKLoadPort = "USATL";
			mawb.CM_RL_NKDischargePort = "GBLGW";
			var house = mawb.ChildBills.AddNew();
			house.CS_PiecesManifested = 70;
			house.CS_PiecesLanded = 10;
			house.ShipmentDescriptionCode = "M";
			var split2 = house.Splits.AddNew();
			split2.SplitReference = "02";
			split2.NumberOfPiecesExpected = 222;
			var split1 = house.Splits.AddNew();
			split1.SplitReference = "01";
			split1.NumberOfPiecesExpected = 111;
			split1.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			house.CS_HAWB = "87654321";

			var fsrInterchangeText = "UNB+UNOA:2+CUKCTM9800120Z:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:HWB-87654321'UNT+3+833856'UNZ+1+833856'";
			_ = EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();

			var houseReloaded = new BusinessObjectFactory().Load<CusHAWB>(house.PK);
			AssertEquals("Three messages - FSR, FSA, Genral", 3, houseReloaded.Messages.Count);
			var fsaResponseMessage = (EDIMessage)houseReloaded.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageSubType, "FSA")).FirstOrDefault();
			var genralResponseMessage = (EDIMessage)houseReloaded.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageType, "GEN")).FirstOrDefault();
			AssertContains("FSA/2:111-19051130-87654321", fsaResponseMessage.EM_MessageText);
			AssertContains("OSI/SDC=M HOUSE CONSIGNMENT WITH SPLITS, SEE PRINT SPOOL", fsaResponseMessage.EM_MessageText);
			AssertContains("UNH+2+GENRAL:0:912:UN+833856'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++FSR RESPONSE FOR", genralResponseMessage.EM_MessageText);
			AssertContains("FSR response for: 111-19051130-87654321\r\nFrom:LHRXXX                                     sent: 22/07/2011 22:53\r\n\r\nSTART response to RS interrogation 111-19051130-87654321 from LHRXXX\r\nHAWB     SRF NPX NPR NPD DESC                GWT    AGT CAC DATE     EC\r\n87654321 01  111 0   0                                  CT  19710918   \r\n87654321 02  222 0   0                                                 \r\nPAGE 01/END", genralResponseMessage.EM_MessageInterpretation);
		}

		public void TestFSR_ProducesOutgoingFsaWithNoRecordFoundOsiText()
		{
			// NB this has been modified from the original sample, which was bogus.
			// It was originally      UNB+UNOA:2+CUKCTM9800120Z:IATA+833856:CUKAIR98LHRXXX :IATA+833856+110519:1458+833856++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:111-19051130'UNT+3+833856'UNZ+1+833856'
			var fsrInterchangeText = "UNB+UNOA:2+CUKCTM9800120Z:IATA+       CUKAIR98LHRXXX/:IATA       +110519:1458+833856++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:111-19051130'UNT+3+833856'UNZ+1+833856'".Replace(" ", "");
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();

			var inboundMessage = interchange.ContainedMessages[0];
			inboundMessage.Reload();
			var sentMessage = Factory.Load<EDIMessage>(inboundMessage.EM_LinkUniqueID);
			AssertContains("FTX+CIM+++FSA/2:111-19051130:OSI/NO RECORD FOUND'", sentMessage.EM_MessageText);
			AssertEquals("Sender of message taken from inboundinterchange", "CUKAIR98LHRXXX", sentMessage.EM_MessageOwner);
			AssertEquals("CUK", sentMessage.EM_ApplicationCode);
			AssertEquals("Recipient of message is LUCAS station", "CUKCTM9800120Z", sentMessage.EM_ApplicationReference);
			AssertEquals("CIMFSA", sentMessage.EM_MessageType + sentMessage.EM_MessageSubType);
			AssertEquals("QUE", sentMessage.EM_Status);
			AssertEquals("TRX", sentMessage.EM_ReceiveTransmit);

			AssertEquals("CIMFSR", inboundMessage.EM_MessageType + inboundMessage.EM_MessageSubType);
			AssertEquals("RCV", inboundMessage.EM_Status);
			AssertEquals("RCV", inboundMessage.EM_ReceiveTransmit);
		}

		[TestDate(1998, 8, 9, 10, 25, 3)]
		public void TestFSR_InboundAndCreateFsaResponse()
		{
			mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			mawb.CM_FlightNo = "SR903";
			mawb.CM_MAWB = "085-22210005";
			mawb.CM_DepartureDate = new ZDate(2000, 3, 22);
			mawb.CM_ArrivalDate = new ZDate(2001, 4, 23);
			mawb.AirportOfOrigin = "OOO";
			mawb.AirportOfDestination = "DDD";
			mawb.AirportOfArrival = "AAA";
			mawb.NumberOfPiecesReceived = 0;
			mawb.NumberOfPiecesExpected = 70;

			var testDate1 = new ZDateTime(2000, 3, 23, 12, 34, 0);
			var dlv1 = mawb.OutTurns.AddNew();
			dlv1.C5_CargoReceiptDate = testDate1;
			dlv1.C5_PackagesOutturned = 7;
			dlv1.IsDelivered = false;
			var testDate2 = new ZDateTime(2000, 3, 24, 12, 34, 0);
			var dlv2 = mawb.OutTurns.AddNew();
			dlv2.C5_CargoReceiptDate = testDate2;
			dlv2.C5_PackagesOutturned = 5;
			dlv2.IsDelivered = false;
			var dlv3 = mawb.OutTurns.AddNew();
			dlv3.C5_CargoReceiptDate = testDate2;
			dlv3.C5_PackagesOutturned = 5;
			dlv3.IsDelivered = false; // not in message

			ZString fsaSimple = @"FSA/2:085-22210005OOODDD/T70:OSI/SDC T";
			ZString fsaReceivedPieces = @"FSA/2:085-22210005OOODDD/P68T70:OSI/SDC T NPR=68";
			ZString fsaReceivedWithCAC = @"FSA/2:085-22210005OOODDD/P68T70:OSI/SDC T CC 18 SEP 0000 NPR=68";
			ZString fsaCAC = @"FSA/2:085-22210005OOODDD/T70:OSI/SDC T CC 18 SEP 0000";
			ZString fsaDelivered = @"FSA/2:085-22210005OOODDD/P50T70:OSI/SDC T CC 18 SEP 0000 NPR=50 NPD=12";

			mawb.NumberOfPiecesReceived = 0;
			RunFsaTest(fsaSimple);
			mawb.NumberOfPiecesReceived = 68;
			RunFsaTest(fsaReceivedPieces);
			mawb.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			RunFsaTest(fsaReceivedWithCAC);
			mawb.NumberOfPiecesReceived = 0;
			RunFsaTest(fsaCAC);
			mawb.NumberOfPiecesReceived = 50;
			dlv1.IsDelivered = true;
			dlv2.IsDelivered = true;
			RunFsaTest(fsaDelivered);
		}

		void RunFsaTest(string expectedPartialFsa)
		{
			var fsrInterchangeText = string.Format("UNB+UNOA:2+CUKCTM9800120Z:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+{0}++833856'UNH+833856+CIMFSR:0:0:Z1:IATA+833856'FTX+CIM+++FSR:085-22210005'UNT+3+833856'UNZ+1+833856'", interchangeNumber++);
			EDIInterchange.CreateNewInterchangeFromString(Factory, fsrInterchangeText, "CUK");
			Factory.Save();
			RunProcessors();
			var mawbReloaded = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertEquals(2, mawbReloaded.Messages.Count);
			var sentMessage = mawbReloaded.Messages.LastOutgoingMessage;
			AssertContains(expectedPartialFsa, sentMessage.EM_MessageText);

			mawbReloaded.Messages.RemoveAndDeleteAllFromTest();
			mawbReloaded.Factory.Save();
		}
		static int interchangeNumber = 833856;
		CusMAWB mawb;

		public void TestFRN_RenominationRequest_Hawb_AutoFRC()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "015-12345675";
			mawb.Profile = "CUKAIR98LHRBAC";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "09124361";
			hawb.AgentBadge = "YYY";
			hawb.CS_PiecesManifested = 10;
			var interchangeText = "UNB+UNOA:2+CUKFFW98000YYY:IATA+CUKAIR98LHRBAC/:IATA+110519:1458+833856++833856'UNH+MSGREF+CIMFRN:0:0:IA'FTX+CIM+++FRN:LHRBAC:015-12345675-09124361:AGT/ABC'UNT+3+MSGREF'UNZ+1+833856'";
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, "CUK");
			Factory.Save();
			RunProcessors();
			var hawbReloadedBecauseCaching = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			var frnInboundMessage = hawbReloadedBecauseCaching.Messages.LastIncomingMessage;
			AssertContains("FRN:LHRBAC:015-12345675-09124361", frnInboundMessage.EM_MessageText);
			AssertEquals("AWB has been updated to new requested agent", "ABC", hawbReloadedBecauseCaching.AgentBadge);
			var outboundFrcMessage = hawbReloadedBecauseCaching.Messages.LastOutgoingMessage;
			AssertEquals("FRC", outboundFrcMessage.EM_MessageSubType);
			AssertContains("FRC shows new agent", "NAD+CB+ABC", outboundFrcMessage.EM_MessageText);
			var expectedHtml = @"<h3>Renomination request for 015-12345675-09124361</h3>
										<h4>The request has been automatically processed.</h4>
										<p>Shed LHRBAC has received an FRN request to renominate consignment 015-12345675-09124361 to <b>ABC</b>.</p>
										<p>This change is from agent YYY and the request was made by 000YYY.</p>";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("renomination"));
			AssertContains(expectedHtml, email.Body);
			AssertContains(expectedHtml, frnInboundMessage.EM_MessageInterpretation);
		}

		public void TestFRN_RenominationRequest_SplitHawb_AutoFRC()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "015-12345675";
			mawb.Profile = "CUKAIR98LHRBAC";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "09124361";
			hawb.AgentBadge = "YYY";
			hawb.CS_PiecesManifested = 10;
			var split01 = hawb.Splits.AddNew();
			var split02 = hawb.Splits.AddNew();
			split01.SplitReference = "01";
			split02.SplitReference = "02";
			split02.NumberOfPiecesExpected = 1;

			var interchangeText = "UNB+UNOA:2+CUKFFW98000YYY:IATA+CUKAIR98LHRBAC/:IATA+110519:1458+833856++833856'UNH+MSGREF+CIMFRN:0:0:IA'FTX+CIM+++FRN:LHRBAC:015-12345675-09124361:SPT/02:AGT/ABC'UNT+3+MSGREF'UNZ+1+833856'";
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, "CUK");
			Factory.Save();
			RunProcessors();
			var hawbReloadedBecauseCaching = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			var split01ReloadedBecauseCaching = new BusinessObjectFactory().Load<SplitHouse>(split01.PK);
			var split02ReloadedBecauseCaching = new BusinessObjectFactory().Load<SplitHouse>(split02.PK);
			var frnInboundMessage = hawbReloadedBecauseCaching.Messages.LastIncomingMessage;
			AssertContains("FRN:LHRBAC:015-12345675-09124361:SPT/02", frnInboundMessage.EM_MessageText);
			AssertEquals("Split 02 has been updated to new requested agent", "ABC", split02ReloadedBecauseCaching.AgentBadge);
			AssertEquals("Split 01 untouched", "YYY", split01ReloadedBecauseCaching.AgentBadge);
			AssertEquals("Hawb untouched", "YYY", hawbReloadedBecauseCaching.AgentBadge);
			var outboundFrcMessage = hawbReloadedBecauseCaching.Messages.LastOutgoingMessage;
			AssertEquals("FRC", outboundFrcMessage.EM_MessageSubType);
			AssertContains("FRC shows new agent", "NAD+CB+ABC", outboundFrcMessage.EM_MessageText);
			AssertContains("FRC is for split 02", "HWB:09124361:02", outboundFrcMessage.EM_MessageText);
			var expectedHtml = @"<h3>Renomination request for 015-12345675-09124361/02</h3>
										<h4>The request has been automatically processed.</h4>
										<p>Shed LHRBAC has received an FRN request to renominate consignment 015-12345675-09124361/02 to <b>ABC</b>.</p>
										<p>This change is from agent YYY and the request was made by 000YYY.</p>";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("renomination"));
			AssertContains(expectedHtml, email.Body);
			AssertContains(expectedHtml, frnInboundMessage.EM_MessageInterpretation);
		}

		public void TestFRN_RenominationRequest_Hawb_Email()
		{
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "*");
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "015-12345675";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRTWA";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "09124361";
			hawb.AgentBadge = "DAN";
			var interchangeText = "UNB+UNOA:2+CUKFFW98000YYY:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+MSGREF+CIMFRN:0:0:IA'FTX+CIM+++FRN:LHRTWA:015-12345675-09124361:AGT/ABC'UNT+3+MSGREF'UNZ+1+833856'";
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, "CUK");
			Factory.Save();
			RunProcessors();
			var hawbReloadedBecauseCaching = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertContains("FRN:LHRTWA:015-12345675-09124361", hawbReloadedBecauseCaching.Messages[0].EM_MessageText);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("renomination"));
			var expectedHtml = @"<h3>Renomination request for 015-12345675-09124361</h3>
										<h4>The request requires an FRC if you approve the change (the requesting agent is not allow to make auto-renominations).</h4>
										<p>Shed LHRXXX has received an FRN request to renominate consignment 015-12345675-09124361 to <b>ABC</b>.</p>
										<p>This change is from agent DAN and the request was made by 000YYY.</p>";
			AssertContains(expectedHtml, email.Body);
			AssertContains(@"To action this request", email.Body);
			AssertContains(@"click here: <a href='edient:Command=ShowEditForm&LicenceCode=EDIDUKDAT&ControllerID=CcsukAirInventoryHouse", email.Body);
			AssertContains(expectedHtml, hawbReloadedBecauseCaching.Messages[0].EM_MessageInterpretation);
		}

		public void TestFRN_RenominationRequest_Hawb_EmailNotificationsGoToRecipientForSpecificBranch()
		{
			var dunstableBranch = CuscarInboundParserTests.GetDunstableBranchPkForTest(Factory);
			string notificationEmailAddress = null;
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "*");

			using (DisposableEnvironment.ForBranch(dunstableBranch.PK.ToGuid()))
			{
				notificationEmailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactCimFrn, dunstableBranch.PK.ToGuid(), Factory);

				var mawb = Factory.New<CusMAWB>();
				mawb.CM_MAWB = "015-12345675";
				mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRTWA";
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_HAWB = "09124361";
				hawb.AgentBadge = "DAN";
				var interchangeText = "UNB+UNOA:2+CUKFFW98000YYY:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+MSGREF+CIMFRN:0:0:IA'FTX+CIM+++FRN:LHRTWA:015-12345675-09124361:AGT/ABC'UNT+3+MSGREF'UNZ+1+833856'";
				var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, "CUK");
				Factory.Save();
				RunProcessors();
			}

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("renomination"));
			AssertContains(notificationEmailAddress, email.Recipients[0].Email);
		}

		public void TestFRN_RenominationRequest_SplitHawb_Email()
		{
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "*");
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "015-12345675";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRTWA";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "09124361";
			hawb.AgentBadge = "DAN";
			var splitHouse = hawb.Splits.AddNew();
			splitHouse.SplitReference = "02";
			var interchangeText = "UNB+UNOA:2+CUKFFW98000YYY:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'UNH+MSGREF+CIMFRN:0:0:IA'FTX+CIM+++FRN:LHRTWA:015-12345675-09124361:SPT/02:AGT/ABC'UNT+3+MSGREF'UNZ+1+833856'";
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, "CUK");
			Factory.Save();
			RunProcessors();
			var hawbReloadedBecauseCaching = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertContains("FRN:LHRTWA:015-12345675-09124361", hawbReloadedBecauseCaching.Messages[0].EM_MessageText);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("renomination"));
			var expectedHtml = @"<h3>Renomination request for 015-12345675-09124361/02</h3>
										<h4>The request requires an FRC if you approve the change (the requesting agent is not allow to make auto-renominations).</h4>
										<p>Shed LHRXXX has received an FRN request to renominate consignment 015-12345675-09124361/02 to <b>ABC</b>.</p>
										<p>This change is from agent DAN and the request was made by 000YYY.</p>";
			AssertContains(expectedHtml, email.Body);
			AssertContains(@"To action this request", email.Body);
			AssertContains(@"click here: <a href='edient:Command=ShowEditForm&LicenceCode=EDIDUKDAT&ControllerID=CcsukSplitHouseController", email.Body);
			AssertContains(expectedHtml, hawbReloadedBecauseCaching.Messages[0].EM_MessageInterpretation);
		}

		public void TestFSA_2IA()
		{
			RunTestParseInboundCimFsa(cimFsa2IaEdifact, cimFsa2IaInterpretaion);
		}

		public void TestFSA_2Z1()
		{
			RunTestParseInboundCimFsa(cimFsa2IaEdifact.Replace("UNH+1051+CIMFSA:2:0:IA", "UNH+1051+CIMFSA:2:0:Z1"), cimFsa2IaInterpretaion);
		}

		public void TestFSA_6Z1()
		{
			RunTestParseInboundCimFsa(cimFsa2IaEdifact.Replace("UNH+1051+CIMFSA:2:0:IA", "UNH+1051+CIMFSA:6:0:Z1"), cimFsa2IaInterpretaion);
		}

		public void TestFSA_6IA_FSA2()
		{
			RunTestParseInboundCimFsa(cimFsa2IaEdifact.Replace("UNH+1051+CIMFSA:2:0:IA", "UNH+1051+CIMFSA:6:0:IA"), cimFsa2IaInterpretaion);
		}

		public void TestFSA_6IA_FSA6()
		{
			// The NFD segment is not use fully understood by our parse so we just fallback to a generic description
			RunTestParseInboundCimFsa("UNB+UNOA:2+CUKAIR98LHRSLS:IATA:147093+CUKFFW98000DBC:IATA+130409:0726+ICCRTLREF'UNH+147093+CIMFSA:6:0:Z1:IATA'FTX+CIM+++FSA/6:170-22012007HKGLHR/T25K187:CCD/08APR/LHR/T25K187:NFD/08APR/LHR/T25K187/KINGSCOTE ROJAY DBC:RCF/EK0005/08APR/LHR/T25'FTX+CIM+++OSI/SDC-T 'UNT+4+147093'UNZ+1+ICCRTLREF'",
				"<th>CargoIMP</th><th>Explanation</th></tr></thead><tr><td>170-22012007HKGLHR/T25K187</td><td>MAWB 170-22012007 from HKG to LHR ; Total 25 pieces; 187 kilos</td></tr><tr><td>CCD/08APR/LHR/T25K187</td><td>Cleared Customs.</td></tr><tr><td>NFD/08APR/LHR/T25K187/KINGSCOTE ROJAY DBC</td><td>Consignee notified, on this date at this location, of the arrival of the consignment.</td></tr><tr><td>RCF/EK0005/08APR/LHR/T25</td><td>Total 25 pieces Received on flight EK0005 at airport LHR at 08APR. </td></tr><tr><td>OSI/SDC-T </td><td>SDC-T</td></tr></table>");
		}

		readonly string cimFsa2IaInterpretaion = "<th>CargoIMP</th><th>Explanation</th></tr></thead><tr><td>170-22012007ATLLHR/P9T10</td><td>MAWB 170-22012007 from ATL to LHR ; Partial 9 of 10 total pieces</td></tr><tr><td>RCF/BA002/13JUN/LHR/P9</td><td>Partial 9 pieces Received on flight BA002 at airport LHR at 13JUN. </td></tr><tr><td>OSI/SDC T</td><td>Shipment description code = T</td></tr><tr><td>DLV/27OCT/NCE/P3K50T4</td><td>Delivered to consignee  at airport NCE at 27OCT. Partial 3 of 4 total pieces; 50 kilos.</td></tr><tr><td>TFD/BA/13JUN/LHR/T9</td><td>Transferred to carrier BA at airport LHR at 13JUN. Total 9 pieces. Reference:.</td></tr><tr><td>TFD/BA/13JUN/LHR/P9/MYREFERENCE</td><td>Transferred to carrier BA at airport LHR at 13JUN. Partial 9 pieces. Reference:MYREFERENCE.</td></tr><tr><td>OSI/SDC T 10 PCS CC</td><td>Shipment description code = T 10 pieces Cleared by Customs</td></tr>";
		readonly string cimFsa2IaEdifact = "UNB+UNOA:2+CUKAIR98LHRCWE:IATA+CUKAIR98LHRCWE:IATA+120217:1338+596'UNH+1051+CIMFSA:2:0:IA+ExternalCAR'FTX+CIM+++FSA/2:170-22012007ATLLHR/P9T10:RCF/BA002/13JUN/LHR/P9:OSI/SDC T'FTX+CIM+++DLV/27OCT/NCE/P3K50T4'FTX+CIM+++TFD/BA/13JUN/LHR/T9'FTX+CIM+++TFD/BA/13JUN/LHR/P9/MYREFERENCE'FTX+CIM+++OSI/SDC T 10 PCS CC'UNT+6+1051'UNZ+1+596'";

		void RunTestParseInboundCimFsa(string interchangeText, string expectedInterpretation)
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "170-22012007";
			_ = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, "CUK");
			Factory.Save();
			RunProcessors();
			var mawbReloadedBecauseCaching = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertEquals(1, mawbReloadedBecauseCaching.Messages.Count);
			AssertContains(expectedInterpretation, mawbReloadedBecauseCaching.Messages.LastIncomingMessage.EM_MessageInterpretation);
		}

		public void TestFSA_OnlyOneMawb()
		{
			// Finds job via AWB number despite no SYS-CAR
			var cimFsaWithoutCar = string.Format(cimFsaResponseWithCarPlaceholder, "");
			RunCimFsaResponseParsing_OnlyOneMawb(cimFsaWithoutCar);
		}

		void RunCimFsaResponseParsing_OnlyOneMawb(string cimFsaWithoutCar)
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "17628739174";
			RunCimFsaCarTest(cimFsaWithoutCar);
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(1, basic.Messages.Count);
			AssertEquals("RCV", basic.Messages[0].EM_Status);
		}

		public void TestFSA_OnlyOneMawb_CAR()
		{
			// Finds job via AWB number despite crap SYS-CAR
			var cimFsaWithCar = string.Format(cimFsaResponseWithCarPlaceholder, "CAR");
			RunCimFsaResponseParsing_OnlyOneMawb(cimFsaWithCar);
		}

		public void TestFSA_GoodSysCar()
		{
			// Finds the job without looking at AWB number, uses CAR
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "rubbish";
			var mockMessage = Factory.New<DummyEDIMessage_CimInboundParserTests>();
			mockMessage.GetMessageReferenceNumberReturns = "msgNum";
			basic.Messages.Add(mockMessage);
			var cimFsaWithoutCar = string.Format(cimFsaResponseWithCarPlaceholder, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockMessage));
			RunCimFsaCarTest(cimFsaWithoutCar);
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(2, basic.Messages.Count);
			AssertEquals("RCV", basic.Messages[1].EM_Status);
		}

		public void TestFSA_GoodSysCar_Hawb()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "rubbish";
			var hawb = mawb.ChildBills.AddNew();
			var mockMessage = Factory.New<DummyEDIMessage_CimInboundParserTests>();
			mockMessage.GetMessageReferenceNumberReturns = "msgNum";
			hawb.Messages.Add(mockMessage);  // Query is sent from HAWB, not MAWB
			var cimFsaWithoutCar = string.Format(cimFsaResponseWithCarPlaceholder, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockMessage));
			RunCimFsaCarTest(cimFsaWithoutCar);
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			hawb = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals(0, mawb.Messages.Count);
			AssertEquals(2, hawb.Messages.Count);
			AssertEquals("RCV", hawb.Messages[1].EM_Status);
		}

		public void TestFSA_MultipleMawbsUseShed()
		{
			// Finds the job using PIMA from sender
			var basic1 = Factory.New<CusMAWB>();
			basic1.CM_MAWB = "17628739174";
			basic1.MasterLevelHouseHelper.CS_WarehouseLocation = "XXXYYY"; // Irrelevant
			var cimFsaWithoutCar = string.Format(cimFsaResponseWithCarPlaceholder, "");  // bad CAR
			var basic2 = Factory.New<CusMAWB>();
			basic2.CM_MAWB = "17628739174";
			basic2.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRSLS"; // sender of FSA
			RunCimFsaCarTest(cimFsaWithoutCar);
			basic1 = new BusinessObjectFactory().Load<CusMAWB>(basic1.PK);
			basic2 = new BusinessObjectFactory().Load<CusMAWB>(basic2.PK);
			AssertEquals("Irrelevant AWB unaffected", 0, basic1.Messages.Count);
			AssertEquals("Right mawb affected", 1, basic2.Messages.Count);
			AssertEquals("RCV", basic2.Messages[0].EM_Status);
		}

		void RunCimFsaCarTest(string cimFsaInbound)
		{
			EDIInterchange.CreateNewInterchangeFromString(Factory, cimFsaInbound, "CUK");
			Factory.Save();
			RunProcessors();
		}

		readonly string cimFsaResponseWithCarPlaceholder = "UNB+UNOA:2+CUKAIR98LHRSLS:IATA:147093+CUKFFW98000DBC:IATA+130409:0726+ICCRTLREF'UNH+147093+CIMFSA:6:0:Z1:IATA+{0}'FTX+CIM+++FSA/6:176-28739174HKGLHR/T25K187:CCD/08APR/LHR/T25K187:NFD/08APR/LHR/T25K187/KINGSCOTE ROJAY DBC:RCF/EK0005/08APR/LHR/T25'FTX+CIM+++OSI/SDC-T 'UNT+4+147093'UNZ+1+ICCRTLREF'";

		public void TestFMA()
		{
			var fma = "UNH+192+CIMFMA:0:0:Z1:IATA+<<SYSCAR>>'FTX+CIM+++FMA:ACK/C.0001:/MESSAGE STORED FOR LATER TRANSMISSION.:FRD:LHRCWE'UNT+3+192'";
			EDIMessage transmitMessage;
			EDIMessage receiveMessage;
			RunFmaFna(fma, out receiveMessage, out transmitMessage);
			AssertEquals("ACK", transmitMessage.EM_Status);

			var expectedInterpretation = @"<p><b>FMA Acknowledgement</b></p>
<p><i>C.0001 MESSAGE STORED FOR LATER TRANSMISSION.</i>  </p>
<p>Original message type: FRD </p> ";
			AssertContains(expectedInterpretation, receiveMessage.EM_MessageInterpretation);
			AssertContains(expectedInterpretation, Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("FMA message received for CCSUK job 123-12345678", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}

		public void TestFMANotificationsGoToRecipientForSpecificBranch()
		{
			SetupFMAFNANotificationsGoToRecipientForSpecificBranchTest(
				"UNH+192+CIMFMA:0:0:Z1:IATA+<<SYSCAR>>'FTX+CIM+++FMA:ACK/C.0001:/MESSAGE STORED FOR LATER TRANSMISSION.:FRD:LHRCWE'UNT+3+192'",
				GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactCimFma);
		}

		public void TestFNANotificationsGoToRecipientForSpecificBranch()
		{
			SetupFMAFNANotificationsGoToRecipientForSpecificBranchTest(
				"UNH+192+CIMFNA:0:0:Z1:IATA+<<SYSCAR>>'FTX+CIM+++FNA:ACK/C.0001:/YOU ARE GREAT.:FRD:LHRCWE'UNT+3+192'",
				GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactCimFna);
		}

		public void SetupFMAFNANotificationsGoToRecipientForSpecificBranchTest(string fmaOrFna, IRegistryItem registryItem)
		{
			var dunstableBranch = CuscarInboundParserTests.GetDunstableBranchPkForTest(Factory);
			string notificationEmailAddress = null;
			EDIMessage transmitMessage;
			EDIMessage receiveMessage;

			using (DisposableEnvironment.ForBranch(dunstableBranch.PK.ToGuid()))
			{
				notificationEmailAddress = CuscarInboundParserTests.SetUpNotificationGroup(registryItem, dunstableBranch.PK.ToGuid(), Factory);
				RunFmaFna(fmaOrFna, out receiveMessage, out transmitMessage);
			}

			AssertContains("Email recipient for branch", notificationEmailAddress, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		public void TestFNA()
		{
			var fna = "UNH+192+CIMFNA:0:0:Z1:IATA+<<SYSCAR>>'FTX+CIM+++FNA:ACK/C.0001:/YOU ARE GREAT.:FRD:LHRCWE'UNT+3+192'";
			EDIMessage transmitMessage;
			EDIMessage receiveMessage;
			RunFmaFna(fna, out receiveMessage, out transmitMessage);
			AssertEquals("REJ", transmitMessage.EM_Status);
			var expectedInterpretation = @"<p><b>FNA Rejection</b></p>
<p><i>C.0001 YOU ARE GREAT.</i>  </p>
<p>Original message type: FRD </p> ";
			AssertContains(expectedInterpretation, receiveMessage.EM_MessageInterpretation);
			AssertContains(expectedInterpretation, Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("FNA message received for CCSUK job 123-12345678", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}

		public void TestFMA_NoSyscar()
		{
			var fma = "UNH+00353220935001+CIMFMA:0:0:Z1:IATA'FTX+CIM+++FMA:ACK/M.845 :/POSITIVE:FRN:ABZAEX'FTX+CIM+++123-12345678:AGT/CAR'UNT+4+00353220935001";
			EDIMessage transmitMessage;
			EDIMessage receiveMessage;
			RunFmaFna(fma, out receiveMessage, out transmitMessage);
			AssertEquals("Unchanged status", "SNT", transmitMessage.EM_Status);

			var expectedInterpretation = @"<p><b>FMA Acknowledgement</b></p>
<p><i>M.845  POSITIVE</i>  </p>
<p>Original message type: FRN </p>";
			AssertContains(expectedInterpretation, receiveMessage.EM_MessageInterpretation);
			AssertContains(expectedInterpretation, Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("response did not contain a valid common access reference", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("123-12345678", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("FMA message received", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}

		public void TestFNA_NoSyscar()
		{
			var fna = "UNH+00353220935001+CIMFNA:0:0:Z1:IATA'FTX+CIM+++FNA:ACK/M.845 :/RECIPIENT UNKNOWN:FRN:ABZAEX'FTX+CIM+++123-12345678:AGT/CAR'UNT+4+00353220935001";
			EDIMessage transmitMessage;
			EDIMessage receiveMessage;
			RunFmaFna(fna, out receiveMessage, out transmitMessage);
			AssertEquals("Unchanged", "SNT", transmitMessage.EM_Status);
			var expectedInterpretation = @"<p><b>FNA Rejection</b></p>
<p><i>M.845  RECIPIENT UNKNOWN</i>  </p>
<p>Original message type: FRN </p>";
			AssertContains(expectedInterpretation, receiveMessage.EM_MessageInterpretation);
			AssertContains(expectedInterpretation, Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("response did not contain a valid common access reference", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("123-12345678", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("FNA message received", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}

		[TestDate(2011, 9, 12, 12, 0, 5)]
		public void TestFSN_Basic()
		{
			string cim = @"UNH+MSGREF+CIMFSN:0:0:IA+07412345675'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675:
CSN/CC/10/12SEP1200/ABC12532/CUSTOMS CLEARED'
UNT+3+MSGREF'".Replace(System.Environment.NewLine, "");

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "074-12345675";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			Factory.Save();

			RunProcessors();

			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals("CC", mawb.CustomsActionCode);
			AssertEquals("CUSTOMS CLEARED", mawb.LatestCustomsActionText);
			AssertEquals(new ZDateTime(2011, 9, 12, 12, 0, 0), mawb.CustomsActionDate);
			AssertEquals(message.PK, mawb.Messages[0].PK);
			var filter = new ZQuery(StmALogSchema.SL_Reference, "CC");
			filter.AddToFilter(StmALogSchema.SL_Parent, mawb.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			var stmLog = Factory.Load<StmALog>(filter);
			AssertNotNull(stmLog);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "G2 Advice of customs action 074-12345675"));
			AssertNull("G2 document should not be queued, this FSN was CC", printJob);
		}

		public void TestFSN_OutOfSequenceCA()
		{
			// CCSUK send FSN/CA and then FSN/CT, but we receive or proce3ss the CT before the CA.  The CA should not update away from CT.

			string cimCA = @"UNH+MSGREF+CIMFSN:0:0:IA+07412345675'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675:
CSN/CA/10/12SEP1200/ABC12532/CHIEF ACCEPTED'
UNT+3+MSGREF'".Replace(System.Environment.NewLine, "");

			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "074-12345675";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.CargoTerminalOperator = "KLM";
			basic.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval, ZDateTime.BrettsBirthday);
			var mockMessageCA = Factory.NewMoq<EDIMessage>();
			var messageCA = mockMessageCA.Object;
			messageCA.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			messageCA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageCA.EM_MessageText = cimCA;
			Factory.Save();

			RunProcessors();

			basic.Reload();
			AssertEquals("AWB's CAC is still CT because the bogus/late CA is not effective", CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval, basic.CustomsActionCode);
			AssertEquals("Status CT cannot be updated to CA via FSN", basic.Logs.MostRecentLog.SL_Reference);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertStartsWith("Right subject advising failure", "FAILED CCSUK FSN", email.Subject);
			AssertContains(" CT ", email.Subject);
			AssertContains("Customs Action Code</td><td>CA", email.Body);
		}

		public void TestFSN_CancelMarksUnderbondAsSpent()
		{
			// CA then CX
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "074-12345675";
			basic.Profile = "CUKAIR98LHRKLM";
			var split01 = basic.Splits.AddNew();
			split01.SplitReference = "01";
			var split02 = basic.Splits.AddNew();
			split02.SplitReference = "02";
			var split03 = basic.Splits.AddNew();
			split03.SplitReference = "03";
			var underbond01 = basic.ISRs.AddNew();
			var underbond02 = basic.IARs.AddNew();
			var underbond03 = basic.TSRs.AddNew();
			underbond01.SplitReferenceToWhichThisRemovalPertains = "01";
			underbond02.SplitReferenceToWhichThisRemovalPertains = "02";
			underbond03.SplitReferenceToWhichThisRemovalPertains = "03";
			underbond01.C4_Status = EDIMessage.Status.Acknowledged;
			underbond02.C4_Status = EDIMessage.Status.Acknowledged;
			underbond03.C4_Status = EDIMessage.Status.Acknowledged;
			var mockMessageCX = Factory.NewMoq<EDIMessage>();
			var messageCX = mockMessageCX.Object;
			messageCX.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			messageCX.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageCX.EM_MessageText = CimInboundParserTestsHelper.fsnBasicCW.Replace("/CW/", "/CX-02/").Replace(System.Environment.NewLine, "");
			Factory.Save();
			RunProcessors();
			split02.Reload();
			AssertEquals("split's CAC is now CX", CustomsStatusCodes.Codes.EntryOrRequestCancelled, split02.CustomsActionCode);
			AssertNull("No print queued", Factory.LoadTop1<StmPrintJob>(new ZQuery()));
			underbond01.Reload();
			underbond02.Reload();
			underbond03.Reload();
			AssertEquals("Underbond is updated to CAN", EDIMessage.Status.Cancelled, underbond02.C4_Status);
			AssertEquals("Untouched", EDIMessage.Status.Acknowledged, underbond01.C4_Status);
			AssertEquals("Untouched", EDIMessage.Status.Acknowledged, underbond03.C4_Status);
		}

		public void TestFSN_CancelAfterStatus3()
		{
			// CT then CX
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "074-12345675";
			basic.Profile = "CUKAIR98LHRKLM";
			basic.NumberOfPiecesExpected = 20;
			basic.NumberOfPiecesReceived = 10;
			basic.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval, ZDateTime.BrettsBirthday);
			basic.ReleaseThisNumberOfPieces(6, NumberOfPiecesReleasedHelper.ShedEvent);
			var outturn1 = basic.OutTurns.AddNew();
			var outturn2 = basic.OutTurns.AddNew();
			var outturn3 = basic.OutTurns.AddNew();
			outturn1.C5_PackagesOutturned = 1;
			outturn2.C5_PackagesOutturned = 2;
			outturn3.C5_PackagesOutturned = 3;
			outturn2.IsReleasedAlready = true;
			AssertEquals(6, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
			var mockMessageCX = Factory.NewMoq<EDIMessage>();
			var messageCX = mockMessageCX.Object;
			messageCX.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			messageCX.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageCX.EM_MessageText = CimInboundParserTestsHelper.fsnBasicCW.Replace("/CW/", "/CX/").Replace(System.Environment.NewLine, "");
			Factory.Save();
			RunProcessors();
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("AWB's CAC is now CX", CustomsStatusCodes.Codes.EntryOrRequestCancelled, basic.CustomsActionCode);
			AssertNull("No print queued", Factory.LoadTop1<StmPrintJob>(new ZQuery()));
			AssertEquals("Release count wiped", 0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
			outturn2.Reload();
			AssertEquals("Out turn is opened up so that it can be released again", false, outturn2.IsReleasedAlready);
		}

		public void TestFSN_ReleaseAfterReleaseFailsAndDoesNotReleasePieces()
		{
			// weird and exceptional scenario where we get an FSN/CT and then an FSN/CW.
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRKLM";
			basic.CM_MAWB = "074-12345675";
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 10;
			basic.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval, ZDateTime.BrettsBirthday);
			basic.ReleaseThisNumberOfPieces(6, NumberOfPiecesReleasedHelper.ShedEvent);
			var mockMessageCX = Factory.NewMoq<EDIMessage>();
			var messageCX = mockMessageCX.Object;
			messageCX.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			messageCX.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageCX.EM_MessageText = CimInboundParserTestsHelper.fsnBasicCW.Replace(System.Environment.NewLine, "");
			Factory.Save();
			RunProcessors();
			basic.Reload();
			AssertEquals("AWB's CAC is still CT, not updated", CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval, basic.CustomsActionCode);
			AssertEquals("Status CT cannot be updated to CW via FSN", basic.Logs.MostRecentLog.SL_Reference);
			AssertEquals("No further pieces released", 6, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
			AssertNull("No print queued", Factory.LoadTop1<StmPrintJob>(new ZQuery()));
		}

		public void TestFSN_House()
		{
			string cim = @"UNH+MSGREF+CIMFSN:0:0:IA+WHATEVER'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675-87654321:
CSN/CC/10/12SEP1200/ABC12532/CUSTOMS CLEARED'
UNT+3+MSGREF'".Replace(System.Environment.NewLine, "");

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "074-12345675";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			var shipment = Factory.New<ForwardingShipment>();
			hawb.CS_JS = shipment.PK;
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			Factory.Save();

			RunProcessors();

			hawb.Reload();
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals("CC", hawb.CustomsActionCode);
			AssertEquals("CUSTOMS CLEARED", hawb.LatestCustomsActionText);
			AssertEquals(message.PK, hawb.Messages[0].PK);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, hawb.PresenceOnNetworkStatus);
			var filter = new ZQuery(StmALogSchema.SL_Reference, "CC");
			filter.AddToFilter(StmALogSchema.SL_Parent, hawb.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			var stmLog = Factory.Load<StmALog>(filter);
			AssertNotNull(stmLog);
			AssertContains("Linked Shipment</td><td>S00001000", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestFSN_HouseWithAlphaMawb()
		{
			string cim = @"UNH+MSGREF+CIMFSN:0:0:IA+POOPY'
FTX+CIM+++
FSN:
LHRKLM:
ABC-12345675-87654321:
CSN/CC/10/12SEP1200/ABC12532/CUSTOMS CLEARED'
UNT+3+MSGREF'".Replace(System.Environment.NewLine, "");

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "ABC-12345675";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			Factory.Save();
			hawb.CS_PiecesLanded = 10;
			hawb.CS_PiecesManifested = 10;
			hawb.Status1Date = ZDateTime.BrettsBirthday;

			Factory.Save();

			RunProcessors();

			hawb.Reload();
			message.Reload();
			AssertEquals("CC", hawb.CustomsActionCode);
			AssertEquals(PresenceOnNetworkList.Codes.CompletedOnCcsUk, hawb.PresenceOnNetworkStatus);
		}

		[TestDate(2011, 04, 01, 01, 02, 03)]
		public void TestFSN_Basic_G2Tawb()
		{
			var printerForCcsuk = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printerForCcsuk.PK.ToGuid());

			// NOte how their UNH is CIMFSN:0:0:Z1:IATA', c.f. the specs which say CIMFSN:0:0:IA'
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002:CSN/CA/1/05APR1149/RGHIMP1/HOLD ENTRY'UNT+3+09864713754600'";
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "050-42011002";
			basic.AirportOfArrival = "LHR";
			basic.AirportOfDestination = "MAN";
			basic.Profile = "CUKAIR98LHRVIO";
			AssertEquals("Pre Req: TAWB", true, basic.IsThroughAwb);
			AssertEquals("Pre Req: Shed", true, basic.IsProfileAShed);
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			var receivedInterchange = Factory.New<EDIInterchange>();
			receivedInterchange.EI_To = basic.Profile; // shed
			receivedInterchange.EI_From = "Anyone";
			receivedInterchange.EI_BodyText = message.EM_MessageText;
			receivedInterchange.EI_ReceiveTransmit = "RCV";
			message.EM_EI = receivedInterchange.PK;
			Factory.Save();

			RunProcessors();

			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals("CA", basic.CustomsActionCode);
			AssertEquals("HOLD ENTRY", basic.LatestCustomsActionText);
			AssertEquals(message.PK, basic.Messages[0].PK);
			AssertContains("CCSUK FSN - LHRVIO-050-42011002 CA HOLD ENTRY", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("CAC date is 5th Apr LAST year because 5th Apr is in the future. Today is 1st Apr.", new ZDateTime(2010, 04, 05, 11, 49, 00), basic.CustomsActionDate);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "G2 Advice of customs action 050-42011002"));
			AssertNotNull("G2 document should be queued, this TAWB's FSN was CA and was delivered to a shed", printJob);
			AssertEquals("Printed to paper via correct printer queue", printerForCcsuk.PK, printJob.SP_SQ);
			mockMessage.VerifyAll();
		}

		[TestDate(2011, 06, 14, 01, 02, 03)]
		public void TestFSN_House_G2Tawb()
		{
			var printerForCcsukUsingSharedRego = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printerForCcsukUsingSharedRego.PK.ToGuid());

			// NOte how their UNH is CIMFSN:0:0:Z1:IATA', c.f. the specs which say CIMFSN:0:0:IA'
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002-87654321:CSN/CA/1/05APR1149/RGHIMP1/HOLD ENTRY'UNT+3+09864713754600'";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "050-42011002";
			mawb.Profile = "CUKAIR98LHRVIO";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			hawb.AirportOfArrival = "LHR";
			hawb.AirportOfDestination = "MAN";
			AssertEquals("Pre Req: TAWB", true, hawb.IsThroughAwb);
			AssertEquals("Pre Req: Shed", true, hawb.IsProfileAShed);
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			var receivedInterchange = Factory.New<EDIInterchange>();
			receivedInterchange.EI_To = hawb.Profile; // shed
			receivedInterchange.EI_From = "Anyone";
			receivedInterchange.EI_BodyText = message.EM_MessageText;
			receivedInterchange.EI_ReceiveTransmit = "RCV";
			message.EM_EI = receivedInterchange.PK;
			Factory.Save();

			RunProcessors();

			message.Reload();
			hawb = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals("CA", hawb.CustomsActionCode);
			AssertEquals("HOLD ENTRY", hawb.LatestCustomsActionText);
			AssertEquals(message.PK, hawb.Messages[0].PK);
			AssertContains("CCSUK FSN", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals(new ZDateTime(2011, 04, 05, 11, 49, 00), hawb.CustomsActionDate);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "G2 Advice of customs action 050-42011002-87654321"));
			AssertNotNull("G2 document should be queued, this TAWB's FSN was CA and was delivered to a shed", printJob);
			AssertEquals("Printed to correct paper printer queue, EVEN THOUGH we do not explicitly set the CCSUK printer, we fall back from the shared printer", printerForCcsukUsingSharedRego.PK, printJob.SP_SQ);
		}

		[TestDate(2011, 06, 14, 01, 02, 03)]
		public void TestFSN_House_TransferManifestForTawb()
		{
			var printerForCcsukUsingSharedRego = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printerForCcsukUsingSharedRego.PK.ToGuid());

			// NOte how their UNH is CIMFSN:0:0:Z1:IATA', c.f. the specs which say CIMFSN:0:0:IA'
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002-87654321:CSN/CU/1/05APR1149/RGHIMP1/I LIKE BIG BUTTS'UNT+3+09864713754600'";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "050-42011002";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRVIO";
			mawb.AirportOfArrival = "LHR";
			mawb.AirportOfDestination = "MAN";
			Assert(mawb.IsThroughAwb);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			mawb.AirportOfArrival = "LHR";
			mawb.AirportOfDestination = "MAN";
			Assert(hawb.IsThroughAwb);
			hawb.Profile = "CUKAIR98LHRAAA";
			AssertEquals("Pre Req: TAWB", true, hawb.IsThroughAwb);
			AssertEquals("Pre Req: Shed", true, hawb.IsProfileAShed);
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			Factory.Save();

			RunProcessors();

			message.Reload();
			hawb = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals("CU", hawb.CustomsActionCode);
			AssertEquals(message.PK, hawb.Messages[0].PK);
			AssertContains("CCSUK FSN", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals(new ZDateTime(2011, 04, 05, 11, 49, 00), hawb.CustomsActionDate);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "Transfer Freight Manifest 050-42011002-87654321"));
			AssertNotNull("TRM document should be queued, this FSN was CU", printJob);
		}

		[TestDate(2011, 06, 14, 01, 02, 03)]
		public void TestFSN_CAButNotTawb()
		{
			var printerForCcsukUsingSharedRego = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printerForCcsukUsingSharedRego.PK.ToGuid());

			// NOte how their UNH is CIMFSN:0:0:Z1:IATA', c.f. the specs which say CIMFSN:0:0:IA'
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002-87654321:CSN/CA/1/05APR1149/RGHIMP1/HOLD ENTRY'UNT+3+09864713754600'";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "050-42011002";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRVIO";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			Factory.Save();

			RunProcessors();

			message.Reload();
			hawb = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals("CA", hawb.CustomsActionCode);
			AssertEquals("HOLD ENTRY", hawb.LatestCustomsActionText);
			AssertEquals(message.PK, hawb.Messages[0].PK);
			AssertNotNull((from EmailDef email in Env.OutgoingCustomsMailManager.EmailsCreated where email.Subject.Contains("CCSUK FSN") select email).FirstOrDefault());
			AssertEquals(new ZDateTime(2011, 04, 05, 11, 49, 00), hawb.CustomsActionDate);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.EndsWith, "G2 Advice of customs action 050-42011002-87654321"));
			AssertNull("G2 document should NOT be queued, this CA but not a TAWB", printJob);
		}

		public void TestFSN_ConsignmentNotFound_NotTAWB()
		{
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactFsnStatusUpdatesCa, Guid.Empty, Factory);
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002:CSN/CA/1/05APR1149/RGHIMP1/HOLD ENTRY'UNT+3+09864713754600'";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "050-0000000";
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			message.EM_MessageNum = "1234";
			Factory.Save();

			RunProcessors();

			mawb.Reload();
			message.Reload();
			AssertContains("CargoFact processor: Could not find or process consignment", log[0]);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals("ERR", message.EM_Status);
			AssertEquals("Existing unrelated mawb is untouched", "", mawb.CustomsActionCode);
			AssertContains("Could not find or process consignment for message number", log[0]);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Could not find CCSUK job LHRVIO-050-42011002 using FSN data", email.Subject);
			AssertContains(BrandingFactory.Instance.ProductName, email.Body);
			AssertContains("Consignment & split number=050-42011002, message number=1234", email.Body);
			AssertContains("This message will be reprocessed a total of five times, five minutes apart, before being finally marked as error.", email.Body);
			AssertContains(emailAddress, email.Recipients[0].Email);
		}

		#region TAWB auto-FSN tests
		// FSN with CAC=CU (TAWB released) may arrived before FRI, in which case we should update the AWB
		public void TestFSN_ConsignmentNotFoundStillCreatesItForTawb_Basic()
		{
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002:CSN/CU/11/05APR1149/RGHIMP1/THROUGH AWB'UNT+3+09864713754600'";
			SaveReceivedInterchangeAndProcess(cim);
			var awb = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery());
			AssertNewAwbPropertiesForTawbAutoFsnBeforeFri(cim, awb, "050-42011002");
		}

		public void TestFSN_ConsignmentNotFoundStillCreatesItForTawb_House()
		{
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002-12345678:CSN/CU/11/05APR1149/RGHIMP1/THROUGH AWB'UNT+3+09864713754600'";
			SaveReceivedInterchangeAndProcess(cim);
			ICcsukCusAwb awb = new BusinessObjectFactory().LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false));
			AssertNewAwbPropertiesForTawbAutoFsnBeforeFri(cim, awb, "050-42011002-12345678");
		}

		public void TestFSN_ConsignmentNotFoundStillCreatesItForTawb_SplitBasic()
		{
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002:CSN/CU-02/11/05APR1149/RGHIMP1/THROUGH AWB'UNT+3+09864713754600'";
			SaveReceivedInterchangeAndProcess(cim);
			var awb = new BusinessObjectFactory().LoadTop1<SplitBasic>(new ZQuery());
			AssertNewAwbPropertiesForTawbAutoFsnBeforeFri(cim, awb, "050-42011002/02");
		}

		public void TestFSN_ConsignmentNotFoundStillCreatesItForTawb_SplitHouse()
		{
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002-12345678:CSN/CU-03/11/05APR1149/RGHIMP1/THROUGH AWB'UNT+3+09864713754600'";
			SaveReceivedInterchangeAndProcess(cim);
			var awb = new BusinessObjectFactory().LoadTop1<SplitHouse>(new ZQuery());
			AssertNewAwbPropertiesForTawbAutoFsnBeforeFri(cim, awb, "050-42011002-12345678/03");
		}

		public void TestFSN_ConsignmentNotFoundStillCreatesItForTawb_SplitHouse_MawbAlreadyExists()
		{
			var existingMawb = Factory.New<CusMAWB>();
			existingMawb.CM_MAWB = "05042011002";
			existingMawb.Profile = "CUKAIR98LHRXXX";
			existingMawb.CargoTerminalOperatorAirport = "LHR";
			existingMawb.CargoTerminalOperator = "VIO";
			Factory.Save();
			var cim = "UNH+09864713754600+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRVIO:050-42011002-12345678:CSN/CU-03/11/05APR1149/RGHIMP1/THROUGH AWB'UNT+3+09864713754600'";
			SaveReceivedInterchangeAndProcess(cim);
			var awb = new BusinessObjectFactory().LoadTop1<SplitHouse>(new ZQuery());
			AssertNewAwbPropertiesForTawbAutoFsnBeforeFri(cim, awb, "050-42011002-12345678/03");
			AssertEquals("No second mawb created", 1, new BusinessObjectFactory().Load<CusMAWB>(new ZQuery()).Length);
			existingMawb.Reload();
			AssertEquals("No second hawb created", 1, new BusinessObjectFactory().Load<CusMAWB>(existingMawb.PK).ChildBills.Count);
		}

		void AssertNewAwbPropertiesForTawbAutoFsnBeforeFri(string cim, ICcsukCusAwb awb, string referenceNumber)
		{
			AssertEquals(referenceNumber, awb.ReferenceNumber);
			AssertEquals(CustomsStatusCodes.Codes.ThroughAirWaybillReleased, awb.CustomsActionCode);
			AssertContains(cim, awb.Messages.LastIncomingMessage.EM_MessageText);
			AssertEquals(11, (int)awb.NumberOfPiecesExpected);
			AssertEquals("VIO", awb.CargoTerminalOperator);
			AssertEquals("LHR", awb.CargoTerminalOperatorAirport);
			AssertEquals("CUKAIR98LHRXXX", awb.Profile);
		}

		#endregion

		public void TestFSN_HouseSplit()
		{
			string cim = @"UNH+MSGREF+CIMFSN:0:0:IA+07412345675'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675-87654321:
CSN/CC-69/10/12SEP1200/ABC12532/CUSTOMS CLEARED'
UNT+3+MSGREF'".Replace(System.Environment.NewLine, "");

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "074-12345675";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			var split = (SplitHouse)hawb.Splits.AddNew();
			split.SplitReference = "69";
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			Factory.Save();

			RunProcessors();

			hawb.Reload();
			split.Reload();
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals(CustomsStatusCodes.Codes._CargoWise_ConsignmentHasSplits, hawb.CustomsActionCode);
			AssertEquals(ZDateTime.Empty, hawb.CustomsActionDate);
			AssertEquals("", hawb.LatestCustomsActionText);
			AssertEquals("CC", split.CustomsActionCode);
			AssertEquals("12Sep", split.CustomsActionDate.ToString("ddMMM"));
			AssertEquals("CUSTOMS CLEARED", split.LatestCustomsActionText);
			AssertEquals(message.PK, hawb.Messages[0].PK);
			var filter = new ZQuery(StmALogSchema.SL_Reference, "CC");
			filter.AddToFilter(StmALogSchema.SL_Parent, split.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			var stmLog = Factory.Load<StmALog>(filter);
			AssertNotNull(stmLog);
		}

		public void TestFSN_BasicSplit()
		{
			string cim = @"UNH+MSGREF+CIMFSN:0:0:IA+07412345675'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675:
CSN/CC-69/10/12SEP1200/ABC12532/CUSTOMS CLEARED'
UNT+3+MSGREF'".Replace(System.Environment.NewLine, "");

			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "074-12345675";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			var split = (SplitBasic)basic.Splits.AddNew();
			split.SplitReference = "69";
			split.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			Factory.Save();

			RunProcessors();

			basic.Reload();
			split.Reload();
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSN", message.EM_MessageSubType);
			AssertEquals(CustomsStatusCodes.Codes._CargoWise_ConsignmentHasSplits, basic.CustomsActionCode);
			AssertEquals("CC", split.CustomsActionCode);
			AssertEquals("", basic.LatestCustomsActionText);
			AssertEquals(ZDateTime.Empty, basic.CustomsActionDate);
			AssertEquals("CUSTOMS CLEARED", split.LatestCustomsActionText);
			AssertEquals("12Sep", split.CustomsActionDate.ToString("ddMMM"));
			AssertEquals(message.PK, basic.Messages[0].PK);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, split.PresenceOnNetworkStatus);
			var filter = new ZQuery(StmALogSchema.SL_Reference, "CC");
			filter.AddToFilter(StmALogSchema.SL_Parent, split.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			var stmLog = Factory.Load<StmALog>(filter);
			AssertNotNull(stmLog);
			AssertContains(@"<h3>074-12345675/69 - CUSTOMS CLEARED</h3>
<p>Freight Status Notification</p>", message.EM_MessageInterpretation);
			AssertContains(@"<tr><td>Customs Action Code</td><td>CC</td></tr><tr><td>Customs Action Text</td><td>CUSTOMS CLEARED</td></tr><tr><td>Customs Action Date</td><td>12SEP1200</td></tr><tr><td>Agent Reference</td><td>ABC12532</td></tr><tr><td>Pieces</td><td>10</td></tr>", message.EM_MessageInterpretation);
		}

		public void TestFSN_Basic_MakeC1PrintFromFsn_ReleaseAllPieces_Agent()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CW", "C1 Removal Authority 074-12345675", NumberOfPiecesReleasedHelper.AgentC1Event, "CUKFFW98000ABC");
		}

		void InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner(string cacToGoInInboundFsn, ZString expectedSubjectOfPrintProduced, Event eventType, string receipientPimaOfInboundInterchange = null, Action<CusMAWB, CusOutTurn> alsoSetOutTurnAsReleaseNowAction = null, int npr = 10)
		{
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());
			GbEDIMessage inboundFsnCwMessage;
			CusMAWB basic;
			CimInboundParserTestsHelper.CreateInboundFsnAndUnderboundAndOutboundCusdec(Factory, out inboundFsnCwMessage, out basic, CimInboundParserTestsHelper.fsnBasicCW.Replace("CW", cacToGoInInboundFsn), false, receipientPimaOfInboundInterchange);
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = (ZShort)npr;
			if (basic.NumberOfPiecesReceived == basic.NumberOfPiecesExpected)
			{
				basic.Status1Date = ZDateTime.BrettsBirthday;
			}
			if (LicenceAndPimaHelper.IsFullShed(basic))
			{
				var outTurn = basic.OutTurns.AddNew();
				outTurn.C5_PackagesOutturned = 10;
				if (alsoSetOutTurnAsReleaseNowAction != null)
				{
					alsoSetOutTurnAsReleaseNowAction(basic, outTurn);
				}
			}

			Factory.Save();

			RunProcessors();

			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			if (!expectedSubjectOfPrintProduced.IsEmpty)
			{
				AssertOriginalAndReprintReleaseDocumentProducedFromFsnOrFrc(expectedSubjectOfPrintProduced, printer);
				AssertEquals("Release count updated", 10, basic.NumberOfPiecesReleasedSoFarCumulative(eventType));
			}
			else
			{
				var printJobs = printer.Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("No prints made", 0, printJobs.Length);
				AssertEquals("Release count not touched", 0, basic.NumberOfPiecesReleasedSoFarCumulative(eventType));
			}
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CW()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CW", "", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM");
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CB()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CB", "", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM");
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CT()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CT", "", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM");
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CT_AutoPrintRRA_OneOutturn()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CT", "Release", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM", alsoSetOutTurnAsReleaseNowAction: delegate (CusMAWB b, CusOutTurn o)
			{
				o.C5_PackagesOutturned = 10;
				o.IsBeingReleasedNow = true;
			});
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CT_AutoPrintRRA_NoStatus1()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CT", "", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM", alsoSetOutTurnAsReleaseNowAction: delegate (CusMAWB b, CusOutTurn o)
			{
				o.IsBeingReleasedNow = true;
			}, npr: 11);
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CT_AutoPrintRRA_NoStatus2()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CT", "", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM", alsoSetOutTurnAsReleaseNowAction: delegate (CusMAWB b, CusOutTurn o)
			{
				o.C5_PackagesOutturned = 10;
				o.IsBeingReleasedNow = true;
				b.Status2Granted = false;
			});
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CT_AutoPrintRRA_NoOutturnsAreFlagged()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CT", "", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM", alsoSetOutTurnAsReleaseNowAction: delegate (CusMAWB b, CusOutTurn o)
			{
				o.C5_PackagesOutturned = 10;
				o.IsBeingReleasedNow = false;
			});
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CT_AutoPrintRRA_FeatureDisabled()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowAutoPrintOfRRA.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CT", "", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM", alsoSetOutTurnAsReleaseNowAction: delegate (CusMAWB b, CusOutTurn o)
			{
				o.C5_PackagesOutturned = 10;
				o.IsBeingReleasedNow = true;
			});
		}

		public void TestFSN_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Shed_CT_AutoPrintRRA_MultipleOutturns()
		{
			InboundCimFsn_Basic_MakeRraPrintFromFsn_ReleaseAllPieces_Runner("CT", "Release", NumberOfPiecesReleasedHelper.ShedEvent, "CUKAIR98LHRKLM", alsoSetOutTurnAsReleaseNowAction: delegate (CusMAWB b, CusOutTurn o)
			{
				o.C5_PackagesOutturned = 9;
				o.IsBeingReleasedNow = true;
				// Prove that the auto-print still works when the status 1 is spread over multiple outturns
				var secondOutTurn = b.OutTurns.AddNew();
				secondOutTurn.C5_PackagesOutturned = 1;
				secondOutTurn.IsBeingReleasedNow = true;
			});
		}

		public void TestFSN_Basic_FSN_NotAllPiecesReceived()
		{
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());

			// Scenario: send cusdec, get FSN/CA and CUSRES/CA, then later get FSN/CW.  Need to make a C1 print from the CW FSN.
			GbEDIMessage inboundFsnCwMessage;
			CusMAWB basic;
			CimInboundParserTestsHelper.CreateInboundFsnAndUnderboundAndOutboundCusdec(Factory, out inboundFsnCwMessage, out basic, CimInboundParserTestsHelper.fsnBasicCW);
			basic.NumberOfPiecesExpected = 69;
			basic.NumberOfPiecesReceived = 25;
			var consol = Factory.New<ForwardingConsol>();
			basic.CM_JK = consol.PK;
			Factory.Save();

			RunProcessors();

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertNull("C1 NOT produced from FSN - pieces are still outstanding", printJob);
			basic.Reload();
			AssertEquals(0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
			AssertContains("Linked Consol</td><td>C00001000", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		void RunTestFSN_BasicSplit_MakePrintFromFsn(out StmPrintQueue printer, out CusMAWB basic, out SplitConsignment splitBasic)
		{
			printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());
			GbEDIMessage inboundFsnCwMessage;

			CimInboundParserTestsHelper.CreateInboundFsnAndUnderboundAndOutboundCusdec(Factory, out inboundFsnCwMessage, out basic, CimInboundParserTestsHelper.fsnBasicCwSplit);
			basic.Profile = "CUKFFW98000LXA";
			splitBasic = basic.Splits.AddNew();
			splitBasic.SplitReference = "03";
			splitBasic.HandlingInformation = "Bite Me Hard";
			splitBasic.NumberOfPiecesReceived = 10;
			splitBasic.NumberOfPiecesExpected = 10;
			Factory.Save();
			RunProcessors();
		}

		public void TestFSN_BasicSplit_MakePrintFromFsn()
		{
			StmPrintQueue printer;
			CusMAWB basic;
			SplitConsignment splitBasic;
			RunTestFSN_BasicSplit_MakePrintFromFsn(out printer, out basic, out splitBasic);
			AssertOriginalAndReprintReleaseDocumentProducedFromFsnOrFrc("C1 Removal Authority 074-12345675/03", printer);
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			splitBasic = basic.Factory.Load<SplitBasic>(splitBasic.PK);
			AssertEquals(0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			AssertEquals(10, splitBasic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
		}

		public void TestFSN_BasicSplit_MakePrintFromFsn_DoNotMakePrintRegistrySetToNo()
		{
			GBCustomsDataRegistry.Instance.CcsukAutoPrintC1WhenAllPiecesReceivedAndReleased.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			StmPrintQueue printer;
			CusMAWB basic;
			SplitConsignment splitBasic;
			RunTestFSN_BasicSplit_MakePrintFromFsn(out printer, out basic, out splitBasic);
			var printsCreated = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("No print created, registry turned off", 0, printsCreated.Length);
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			splitBasic = basic.Factory.Load<SplitBasic>(splitBasic.PK);
			AssertEquals(0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
			AssertEquals("Nothing released", 0, splitBasic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
		}

		public void TestFSN_Export()
		{
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactFsnStatusUpdatesXO, Guid.Empty, Factory);
			var exportFsn = "UNH+MSGREF+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRXCW:413-20081202:CSN/XO/1/20AUG1659//MASTER OPEN'UNT+3+MSGREF'";
			var mockInboundMessage = Factory.NewMoq<GbEDIMessage>();
			var inboundFsn = mockInboundMessage.Object;
			inboundFsn.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			inboundFsn.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundFsn.EM_MessageText = exportFsn;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "413-20081202";
			consol.JK_RL_NKLoadPort = "GBLHR";
			Factory.Save();
			RunProcessors();
			AssertEquals(0, log.Count);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			var helper = wrapper.MawbExportHelper;
			AssertEquals("XO", helper.ME_ChiefCustomsActionCodeFromFsn);
			AssertEquals("MASTER OPEN", helper.ME_ChiefCustomsActionTextFromFsn);
			AssertEquals(20, helper.ME_ChiefCustomsActionDateFromFsn.Day);
			AssertEquals(8, helper.ME_ChiefCustomsActionDateFromFsn.Month);
			AssertEquals(16, helper.ME_ChiefCustomsActionDateFromFsn.Hour);
			AssertEquals(59, helper.ME_ChiefCustomsActionDateFromFsn.Minute);
			inboundFsn.Reload();
			AssertEquals(consol.PK, inboundFsn.EM_LinkUniqueID);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Export FSN update for 413-20081202 - XO MASTER OPEN", email.Subject);
			AssertContains(emailAddress, email.Recipients[0].Email);
		}

		public void TestFSN_Basic_ClearanceFromFsn_ToCDS()
		{
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactFsnStatusUpdatesCc, Guid.Empty, Factory);
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());

			// Scenario: send cusdec, get FSN/CA and CUSRES/CA, then later get FSN/CW.  Need to make a C1 print from the CW FSN.
			GbEDIMessage inboundFsnMessage;
			CusMAWB basic;
			var clearanceFsnText = CimInboundParserTestsHelper.fsnBasicCW.Replace("CW", "CC");
			CimInboundParserTestsHelper.CreateInboundFsnAndUnderboundAndOutboundCusdec(Factory, out inboundFsnMessage, out basic, clearanceFsnText, false, "CUKFFW98000XXX");
			// Badge code LXA needs to be set up in registry
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false); // Makes badge LXA
																			  // And the CCSUK messaging details are needed in order to generate a message to CHIEF via CCSUK:
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Anything");
			basic.Profile = "CUKFFW98000LXA"; // Here is badge LXA
			var declaration = ((ICcsukCusAwb)basic).CreateNewStandaloneCDSDeclaration();
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			Factory.Save();
			RunProcessors();
			var printJobs = printer.Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "Customs Clearance for consignment 074-12345675"));
			AssertEquals(1, printJobs.Length);
			declaration.Reload();
			ceh.Reload();
			AssertEquals(0, ceh.Messages.Count);
		}

		public void TestFSN_Basic_FallbackReleaseFromFsn()
		{
			GBCustomsDataRegistry.Instance.ChiefFallbackImports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());
			var clearanceFsnText = CimInboundParserTestsHelper.fsnBasicCAFallbackReleased.Replace(System.Environment.NewLine, "");
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "074-12345675";
			basic.NumberOfPiecesReceived = 69;
			basic.AgentBadge = "DVG";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			var mockInboundMessage = Factory.NewMoq<GbEDIMessage>();
			var inboundFsnMessage = mockInboundMessage.Object;
			inboundFsnMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			inboundFsnMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundFsnMessage.EM_MessageText = clearanceFsnText;
			var receivedInterchange = basic.Factory.New<EDIInterchange>();
			receivedInterchange.EI_ReceiveTransmit = "RCV";
			receivedInterchange.EI_From = "Anyone";
			receivedInterchange.EI_To = LicenceAndPimaHelper.AgentProfilePrefix + "000DVG";
			receivedInterchange.EI_BodyText = clearanceFsnText;
			inboundFsnMessage.EM_EI = receivedInterchange.PK;
			Factory.Save();

			RunProcessors();
			var printJobs = printer.Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "Customs Clearance for consignment 074-12345675"));
			AssertEquals(1, printJobs.Length);
			basic.Reload();
			AssertEquals("CA", basic.CustomsActionCode);
			mockInboundMessage.VerifyAll();
		}

		public void TestFSN_BasicSplit_ClearanceFromFsn()
		{
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());
			GbEDIMessage inboundFsnMessage;
			CusMAWB basic;
			var clearanceFsnText = CimInboundParserTestsHelper.fsnBasicCwSplit.Replace("CW", "CC");
			CimInboundParserTestsHelper.CreateInboundFsnAndUnderboundAndOutboundCusdec(Factory, out inboundFsnMessage, out basic, clearanceFsnText);
			basic.Profile = "CUKFFW98000XXX";
			var split = basic.Splits.AddNew();
			split.SplitReference = "03";
			var declaration = ((ICcsukCusAwb)split).CreateNewStandaloneCDSDeclaration();
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			RunProcessors();
			var printJobs = printer.Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "Customs Clearance for consignment 074-12345675/03"));
			AssertEquals(1, printJobs.Length);
			split.Reload();
			AssertEquals("CC", split.CustomsActionCode);
			declaration.Reload();
			ceh.Reload();
			AssertEquals(EntryStatusList.Codes.Clear, ceh.CH_EntryStatus);
			AssertEquals(EntryStatusList.Codes.Clear, declaration.JE_EntryStatus);
		}

		void AssertOriginalAndReprintReleaseDocumentProducedFromFsnOrFrc(string subject, StmPrintQueue printer)
		{
			var printJobs = printer.Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, subject));
			AssertEquals("2 prints produced from FSN - original and reprint", 2, printJobs.Length);
			var original = (from StmPrintJob pj in printJobs where !pj.SP_DocumentName.Contains("Reprint") select pj).FirstOrDefault();
			var reprint = (from StmPrintJob pj in printJobs where pj.SP_DocumentName.Contains("Reprint") select pj).FirstOrDefault();
			AssertNotNull(original);
			AssertNotNull(reprint);
			AssertEquals("Reprint is NOT printed to paper, only to eDocs", ZGuid.Empty, reprint.SP_SQ);
			AssertEquals("Original is ONLY printed to paper, not to eDocs", printer.PK, original.SP_SQ);
			AssertEquals("Reprint is NOT printed to paper, only to eDocs", "DDS", reprint.SP_JobType); // add eDocsProcessed ?
			AssertEquals("Original is ONLY printed to paper, not to eDocs", "PRN", original.SP_JobType);
		}

		public void TestFSN_HouseClearance()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "190-42011010";
			cusMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCWE";
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.CS_HAWB = "HOUSE001";
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "120-000626A";
			cusHawb.CS_JE_CustomsFormalEntry = declaration.PK;

			var cim = @"UNH+325+CIMFSN:0:0:Z1:IATA+325/U00000105'FTX+CIM+++FSN:LHRCWE:190-42011010-HOUSE001:CSN/CC/10/19APR1033/00000105/CLEARED TFR CDG'UNT+3+325'";
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			message.EM_MessageSubType = "FSN";
			Factory.Save();

			AssertEquals("This job's status should not be cleared", false, declaration.CustomsEntryHeaders[0].CH_EntryStatus == EntryStatusList.Codes.Clear);
			RunProcessors();
			declaration.CustomsEntryHeaders[0].Reload();
			declaration.Reload();
			AssertEquals("Expected the job's status to be CLR (cleared) got " + declaration.CustomsEntryHeaders[0].CH_EntryStatus, true, declaration.CustomsEntryHeaders[0].CH_EntryStatus == EntryStatusList.Codes.Clear);
		}

		public void TestFSN_SendsNotificationEmailToEntrysUserWhenAwbHasNoUser()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "190-42011010";
			cusMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCWE";
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.CS_HAWB = "HOUSE001";
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "120-000626A";
			cusHawb.CS_JE_CustomsFormalEntry = declaration.PK;
			var user = Factory.New<GlbStaff>();
			user.GS_EmailAddress = "daniel@test.com";
			user.GS_Code = "DJC";
			user.GS_LoginName = "Daniel";
			declaration.JE_GS_NKCusAgent = "DJC";
			Factory.Save();

			var cim = @"UNH+325+CIMFSN:0:0:Z1:IATA+325/U00000105'FTX+CIM+++FSN:LHRCWE:190-42011010-HOUSE001:CSN/CC/10/19APR1033/00000105/CLEARED TFR CDG'UNT+3+325'";
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			message.EM_MessageSubType = "FSN";
			Factory.Save();

			RunProcessors();
			var emailToDjc = Env.OutgoingCustomsMailManager.EmailsCreated.Where(e => e.Recipients.RecipientsAsDelimitedString(",").Contains("daniel@wisetechglobal.com"));
			Assert("We should have sent an email to DJC as he is the user of the dec that is linked to (which cleared) the consignment to which the FSN relates, even if the AWB has no user", emailToDjc.Any());
		}

		[ExpectNoExceptions]
		public void TestFSU_Inbound_Iata60Z1()
		{
			//--------------version six----------V
			FsuRunner("UNH+09864713754600+CIMFSU:6:0:Z1:IATA'FTX+CIM+++FSU/6:ANY CARGOIMP HERE'UNT+3+09864713754600'");
		}

		[ExpectNoExceptions]
		public void TestFSU_Inbound_Iata00Z1()
		{
			//-----------------------------------V
			FsuRunner("UNH+09864713754600+CIMFSU:0:0:Z1:IATA'FTX+CIM+++FSU:LHRVIO:050-42011002:CSN/CA/1/05APR1149/RGHIMP1/HOLD ENTRY'UNT+3+09864713754600'");
		}
		void FsuRunner(string cim)
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = cim;
			Factory.Save();

			RunProcessors();

			message.Reload();
			AssertEquals("CIM", message.EM_MessageType);
			AssertEquals("FSU", message.EM_MessageSubType);
			AssertContains("Message Type 'FSU' is an unexpected message type and will not be processed", log[0]);
			AssertEquals("FAL", message.EM_Status);
		}

		void RunFmaFna(string inboundText, out EDIMessage receiveMessage, out EDIMessage transmitMessage)
		{
			var mockTransmit = Factory.New<DummyEDIMessage_CimInboundParserTests>();
			mockTransmit.GetMessageReferenceNumberReturns = "msgNum";
			transmitMessage = mockTransmit;
			mockTransmit.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			mockTransmit.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			mockTransmit.EM_Status = EDIMessage.Status.Sent;

			var mawb = Factory.New<CusMAWB>();
			mawb.Messages.Add(mockTransmit);
			mawb.CM_MAWB = "12312345678";

			var mockReceive = Factory.New<DummyEDIMessage_CimInboundParserTests>();
			mockReceive.GetMessageReferenceNumberReturns = "msgNum";
			receiveMessage = mockReceive;
			mockReceive.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockReceive.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mockReceive.EM_MessageText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(inboundText, mockTransmit);
			Factory.Save();
			RunProcessors();
			mockReceive.Reload();
			mockTransmit.Reload();
			AssertEquals("RCV", mockReceive.EM_Status);
		}

		ICcsukCusAwb SetUpJobForFrdTestAndRunTask(string inboundMessageText, bool createHawbToo = true, bool createSplitsToo = false, string profile = "CUKAIR98LHRTWA", bool setStatus1 = false)
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, shedCode: "TWA", shedAirport: "LHR");
			var mawbOrBasic = Factory.New<CusMAWB>();
			mawbOrBasic.CM_MAWB = "015-12345675";
			mawbOrBasic.AirportOfArrival = "LHR";
			mawbOrBasic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRTWA";
			mawbOrBasic.Profile = profile;
			mawbOrBasic.NumberOfPiecesExpected = 50;
			ICcsukCusAwb result = mawbOrBasic;
			if (createHawbToo)
			{
				var hawb = mawbOrBasic.ChildBills.AddNew();
				hawb.CS_HAWB = "09124361";
				hawb.CS_PiecesManifested = 50;
				result = hawb;
			}
			if (createSplitsToo)
			{
				var split1 = result.Splits.AddNew();
				split1.Weight = 33m;
				split1.NumberOfPiecesExpected = 10;
				split1.SplitReference = "01";
				var split2 = result.Splits.AddNew();
				split2.Weight = 80m;
				split2.NumberOfPiecesExpected = 10;
				split2.SplitReference = "02";
				var split3 = result.Splits.AddNew();
				split3.Weight = 10m;
				split3.NumberOfPiecesExpected = 15;
				split3.SplitReference = "03";
			}
			if (setStatus1)
			{
				var ot = result.CreateNewOutTurn();
				ot.C5_PackagesOutturned = result.NumberOfPiecesExpected;
				ot.C5_MarksAndNumbers = "Red boxes";
				ot.WarehouseLocationID = ZGuid.NewZGuid();
				result.CalculateNprFromReceiptsIfNecessary();
			}

			SaveReceivedInterchangeAndProcess(inboundMessageText);
			((BusinessObject)result).Reload();
			return result;
		}

		void SaveReceivedInterchangeAndProcess(string inboundMessageText)
		{
			var interchangeText = "UNB+UNOA:2+CUKCTM9800120Z:IATA+CUKAIR98LHRXXX/:IATA+110519:1458+833856++833856'"
										+ inboundMessageText.Replace("\t", "").Replace(System.Environment.NewLine, "")
										+ "UNZ+1+833856'";
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, "CUK");
			Factory.Save();
			RunProcessors();
			interchange.ContainedMessages[0].Reload();
		}

		void AssertFcsMessageCreatedFromInboundFrd(ZString expectedFcsMessageFragment, ICcsukCusAwb awb)
		{
			((BusinessObject)awb).Reload();
			AssertEquals("AWB has not been split locally yet, merely the FCS message created", 0, awb.Splits.Count);
			var query = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.OrderBy = EDIMessageSchema.EM_MessageNum.Name;
			var allOutboundMessages = Factory.Load<EDIMessage>(query);
			var outboundFcsMessage = allOutboundMessages[0];
			AssertEquals("FCS", outboundFcsMessage.EM_MessageSubType);
			AssertContains(expectedFcsMessageFragment.Replace(System.Environment.NewLine, "").Replace("\t", "").Replace(" ", ""),
						   outboundFcsMessage.EM_MessageText);
			AssertCollectionContains(outboundFcsMessage, awb.Messages);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.GS_EmailAddress = "daniel@wisetechglobal.com";
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			var staffGroup = Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup);
			staffGroup.Staff.Add(currentUserInCurrentFactory);
			staffGroup.Staff.Add(anotherStaff);
			var branch = (GlbBranch)DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory);
			branch.GB_Phone = "PH 07 3268 2903  FAX";
			Factory.Save();
			branchEnvironment = DisposableEnvironment.ForBranch(branch.PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDown()
		{
			base.TearDown();
			branchEnvironment.Dispose();
		}

		#region FRD messages
		const string frdHouse = @"UNH+MSGREF+CIMFRD:0:0:IA+COMREF1'
	FTX+CIM+++
		FRD:
		LHRTWA:
		015-12345675-09124361:
		AGT/ABC:
		SPT/01P10K33/MKD ADVERTISING MATERIAL - 1 THRU 10'
	FTX+CIM+++
		SPT/02P10K80/MKD DEMO MODELS - 11 THRU 20:
		SPT/03P20K10/BOX SPARE PARTS - 21 THRU 40'
	UNT+4+MSGREF'";

		const string frdBasic = @"UNH+MSGREF+CIMFRD:0:0:IA+COMREF1'
	FTX+CIM+++
		FRD:
		LHRTWA:
		015-12345675:
		AGT/ABC:
		SPT/01P10K33/MKD ADVERTISING MATERIAL - 1 THRU 10'
	FTX+CIM+++
		SPT/02P10K80/MKD DEMO MODELS - 11 THRU 20:
		SPT/03P20K10/BOX SPARE PARTS - 21 THRU 40'
	UNT+4+MSGREF'";

		const string frdCauseParserError = @"UNH+MSGREF+CIMFRD:0:0:IA'
	FTX+CIM+++
		FRD:
		LHRTWA:
		015-12345675-09124361:
		AGT/ABC'
	FTX+CIM+++
		SPT/01P10KXXXXXXXXXX/MKD ADVERTISING MATERIAL - 1 THRU 10:
	UNT+4+MSGREF'";

		const string frdArrivedHouse = @"UNH+MSGREF+CIMFRD:0:0:IA'
	FTX+CIM+++
		FRD:
		LHRTWA:
		015-12345675-09124361:
		ARR/BA123/25MAY:
		AGT/ABC'
	FTX+CIM+++
		SPT/01P10K33/MKD ADVERTISING MATERIAL - 1 THRU 10:
		SPT/02P10K80/MKD DEMO MODELS - 11 THRU 20:
		SPT/03P15K10/BOX SPARE PARTS - 21 THRU 35'
	UNT+4+MSGREF'";

		const string frdArrivedHouseWithFourSplits = @"UNH+MSGREF+CIMFRD:0:0:IA'
	FTX+CIM+++
		FRD:
		LHRTWA:
		015-12345675-09124361:
		ARR/BA123/25MAY:
		AGT/ABC'
	FTX+CIM+++
		SPT/01P10K33/MKD ADVERTISING MATERIAL - 1 THRU 10:
		SPT/02P10K80/MKD DEMO MODELS - 11 THRU 20:
		SPT/03P15K10/BOX SPARE PARTS - 21 THRU 35:
		SPT/04P15K10/BOGUS SPLIT'
	UNT+4+MSGREF'";

		const string frdArrivedHouseOneBogusSplit = @"UNH+MSGREF+CIMFRD:0:0:IA'
	FTX+CIM+++
		FRD:
		LHRTWA:
		015-12345675-09124361:
		ARR/BA123/25MAY:
		AGT/ABC'
	FTX+CIM+++
		SPT/01P10K33/MKD ADVERTISING MATERIAL - 1 THRU 10:
		SPT/02P10K80/MKD DEMO MODELS - 11 THRU 20:
		SPT/99P15K10/BOX SPARE PARTS - 21 THRU 35'
	UNT+4+MSGREF'";
		const string frdNoRecordFound = @"UNH+MSGREF+CIMFRD:0:0:IA+COMREF1'
	FTX+CIM+++
		FRD:
		LHRTWA:
		000-00000000-00000000:
		ARR/BA123/25MAY:
		AGT/ABC'
	FTX+CIM+++
		SPT/01P10K33/MKD ADVERTISING MATERIAL - 1 THRU 10:
		SPT/02P10K80/MKD DEMO MODELS - 11 THRU 20:
		SPT/03P15K10/BOX SPARE PARTS - 21 THRU 35'
	UNT+4+MSGREF'";

		#endregion
	}

	public static class CimInboundParserTestsHelper
	{
		public const string fsnBasicCW = @"UNH+MSGREF+CIMFSN:0:0:IA+SYSCAR'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675:
CSN/CW/10/12SEP1200/000456/I LIKE SMALL BUTTS'
UNT+3+MSGREF'";

		public const string fsnBasicCAFallbackReleased = @"UNH+MSGREF+CIMFSN:0:0:IA+SYSCAR'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675:
CSN/CA/10/12SEP1200/000456/FALLBACK RELEASED'
UNT+3+MSGREF'";

		public const string fsnBasicCwSplit = @"UNH+MSGREF+CIMFSN:0:0:IA+SYSCAR'
FTX+CIM+++
FSN:
LHRKLM:
074-12345675:
CSN/CW-03/10/12SEP1200/000456/I LIKE SMALL BUTTS'
UNT+3+MSGREF'";

		public static void CreateInboundFsnAndUnderboundAndOutboundCusdec(BusinessObjectFactory factory, out GbEDIMessage inboundFsnCwMessage, out CusMAWB basic, ZString cimText, bool isTsrRatherThanIar = false, string pimaRecipientForInboundInterchange = null)
		{
			cimText = cimText.Replace(System.Environment.NewLine, "");

			basic = factory.New<CusMAWB>();
			basic.CM_MAWB = "074-12345675";
			basic.Profile = pimaRecipientForInboundInterchange;
			basic.NumberOfPiecesReceived = 69;
			basic.AgentBadge = "DVG";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";

			TranshipmentRemoval tsr;
			InterAirportRemoval iar;
			CusUnderbond underbond;

			if (isTsrRatherThanIar)
			{
				tsr = basic.TSRs.AddNew();
				tsr.PortOfShipment = "MAN";
				tsr.LicenseRestrictionInd = Enterprise.Customs.Business.YesNoList.Codes.Yes;
				tsr.TranshipmentEntryNumber = "T012345";
				underbond = tsr;
			}
			else
			{
				iar = basic.IARs.AddNew();
				iar.NewShedId = "BAC";
				underbond = iar;
			}

			underbond.AgentsReference = "U000456"; // agents reference in CUSDEC/FSN

			var mockOutboundMessage = factory.New<DummyEDIMessage_CimInboundParserTests>();
			mockOutboundMessage.GetMessageReferenceNumberReturns = "msgNum";
			basic.Messages.Add(mockOutboundMessage);
			mockOutboundMessage.EM_MessageType = CcsukTransmissionMessageFunction.CUSDEC.Code;
			mockOutboundMessage.EM_MessageSubType = underbond.C4_MovementReason;
			mockOutboundMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			var mockInboundMessage = factory.NewMoq<GbEDIMessage>();
			inboundFsnCwMessage = mockInboundMessage.Object;
			inboundFsnCwMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			inboundFsnCwMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundFsnCwMessage.EM_MessageText = cimText;
			if (pimaRecipientForInboundInterchange != null)
			{
				var receivedInterchange = basic.Factory.New<EDIInterchange>();
				receivedInterchange.EI_ReceiveTransmit = "RCV";
				receivedInterchange.EI_From = "Anyone";
				receivedInterchange.EI_To = pimaRecipientForInboundInterchange;
				receivedInterchange.EI_BodyText = cimText;
				inboundFsnCwMessage.EM_EI = receivedInterchange.PK;
			}
			factory.Save();
		}
	}

	sealed class DummyEDIMessage_CimInboundParserTests : EDIMessage
	{
		public DummyEDIMessage_CimInboundParserTests(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public string GetMessageReferenceNumberReturns { get; set; } = string.Empty;

		protected override string GetMessageReferenceNumber()
		{
			return GetMessageReferenceNumberReturns;
		}
	}
}
