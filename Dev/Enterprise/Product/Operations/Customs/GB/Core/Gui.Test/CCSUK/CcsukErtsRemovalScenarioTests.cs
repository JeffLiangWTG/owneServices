using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class CcsukErtsRemovalTestScenarios : CcsukRemovalTestScenariosBase
	{
		public void TestImp37InterShedThenCancelThenNewIntershed_ManualPrint_Status1()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			// Step one create pre-arrival record
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "21042011011";
			basic.Profile = RecipientPima;
			basic.CM_FlightNo = "BA001";
			basic.DescriptionOfGoods = "BOOKS";
			basic.Weight = 10m;
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 0;
			basic.AgentBadge = "LXA";
			basic.AirportOfOrigin = "USATL";
			basic.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.TotalConsignmentManifested;
			basic.CM_ArrivalDate = ZDateTime.Now;

			// Pre-req: should be able to delete
			AssertWhetherOptionPossibleFromMenu(basic, true, CcsukMenu.DeleteSendFrxMenuText);

			// Step 2, FSN/CB for removal from BAC to CAX
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CB/10/21APR1546/DANIEL/OK TRANSFER CAX'UNT+3+363'");
			basic.Reload();
			AssertEquals("CB", basic.CustomsActionCode);
			AssertEquals("OK TRANSFER CAX", basic.LatestCustomsActionText);

			// Step 3, check in pieces
			basic.OutTurns.AddNew().C5_PackagesOutturned = 10;
			Factory.Save();
			// FRC omitted;

			// Step 4, cannot delete 
			AssertWhetherOptionPossibleFromMenu(basic, false, CcsukMenu.DeleteSendFrxMenuText);
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true); // St1, CB, no autoprint yet, should be able to release

			// Step 5, FSN/CX
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CX/10/21APR1546/DANIEL/REQUEST CANCELLED'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("CX", basic.CustomsActionCode);
			AssertEquals("Receipt of CX deletes info about thus-far-released pieces to allow new CAC to work", 0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true); // Status 1, but not status 3

			// Step 6, FSN/CB new shed (POO)
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CB/10/21APR1546/DANIEL/OK TRANSFER POO'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK); // GenAddOn (for CAT) is cached in old factory
			AssertEquals("CB", basic.CustomsActionCode);
			AssertEquals("OK TRANSFER POO", basic.LatestCustomsActionText);

			// Check no auto print, and check can manually release
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);// The FSN processing will see St1 is set but will NOT automatically print for us
			AssertNoPrintQueued("Release/Removal Authority", "210-42011011");

			// Step 7, manual release to NEW shed 
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(basic, 10, true);
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);  // Manually print
			AssertPrintQueued("Release/Removal Authority", "210-42011011", new string[] { "OK TRANSFER POO" }, new string[] { "OK TRANSFER CAX", "PART RELEASE" });
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestImp37InterShedThenCancelThenNewIntershed_WithIntermediateReleases()
		{
			// Deviation from script - release some pieces after first CB status in step 3/4
			// Checks that the second CB status gives correct date on record (i.e. not original message's date) and that second release uses correct message for eDocs parent (not first)

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			// Step one create pre-arrival record
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "21042011011";
			basic.Profile = RecipientPima;
			basic.CM_FlightNo = "BA001";
			basic.DescriptionOfGoods = "BOOKS";
			basic.Weight = 10m;
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 0;
			basic.AgentBadge = "LXA";
			basic.AirportOfOrigin = "USATL";
			basic.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.TotalConsignmentManifested;
			basic.CM_ArrivalDate = ZDateTime.Now;

			// Pre-req: should be able to delete
			AssertWhetherOptionPossibleFromMenu(basic, true, CcsukMenu.DeleteSendFrxMenuText);

			// Step 2, FSN/CB for removal from BAC to CAX
			var cb1Message = ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CB/10/21FEB1546/DANIEL1 CB/OK TRANSFER CAX'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK); // StmLogs are cached				
			AssertEquals("CB", basic.CustomsActionCode);
			AssertEquals("OK TRANSFER CAX", basic.LatestCustomsActionText);
			AssertEquals(new ZDateTime(1986, 2, 21, 15, 46, 0), basic.CustomsActionDate);

			// Step 3, check in pieces
			basic.OutTurns.AddNew().C5_PackagesOutturned = 10;
			basic.Factory.Save();
			// FRC omitted;

			// Deviation - release the pieces
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(basic, 10, true);
			AssertPrintQueued("Release/Removal Authority", "210-42011011", new string[] { "OK TRANSFER CAX", "DANIEL1" }, System.Array.Empty<string>());

			// Step 4, cannot delete 
			AssertWhetherOptionPossibleFromMenu(basic, false, CcsukMenu.DeleteSendFrxMenuText);
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true); // All pieces already released

			// Step 5, FSN/CX
			var cxMessage = ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CX/10/22FEB1646/DANIEL2 CB/REQUEST CANCELLED'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("CX", basic.CustomsActionCode);
			AssertEquals("REQUEST CANCELLED", basic.LatestCustomsActionText);
			AssertEquals(new ZDateTime(1986, 2, 22, 16, 46, 0), basic.CustomsActionDate);  // new date from new CX fsn

			// Step 6, FSN/CB new shed (POO)
			var cb2Message = ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CB/10/23FEB1746/DANIEL3 CB/OK TRANSFER POO'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK); // GenAddOn (for CAT) is cached in old factory
			AssertEquals("CB", basic.CustomsActionCode);
			AssertEquals("OK TRANSFER POO", basic.LatestCustomsActionText);
			AssertEquals(new ZDateTime(1986, 2, 23, 17, 46, 0), basic.CustomsActionDate);  // new date from second CB fsn

			// Check can manually (re-)release
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);

			// Step 7, manual release to NEW shed 
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(basic, 10, true);
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);  // all gone
			var printInQueue = AssertPrintQueued("Release/Removal Authority", "210-42011011", new string[] { "OK TRANSFER POO", "DANIEL3" }, new string[] { "OK TRANSFER CAX", "PART RELEASE", "DANIEL1" });
			AssertEquals("New RRA doc hangs from second, not first CB message", cb2Message.PK, printInQueue.SP_ParentGuid);
		}

		public void TestImp37InterShedThenCancelThenNewIntershed()
		{
			// Slight variation from script - do not set status 1 in step 3, receive only some pieces
			// Step one create pre-arrival record 
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "21042011011";
			basic.Profile = RecipientPima;
			basic.CM_FlightNo = "BA001";
			basic.DescriptionOfGoods = "BOOKS";
			basic.Weight = 10m;
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 0;
			basic.AgentBadge = "LXA";
			basic.AirportOfOrigin = "USATL";
			basic.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.TotalConsignmentManifested;
			basic.CM_ArrivalDate = ZDateTime.Now;

			// Pre-req: should be able to delete
			AssertWhetherOptionPossibleFromMenu(basic, true, CcsukMenu.DeleteSendFrxMenuText);

			// Step 2, FSN/CB for removal from BAC to CAX
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CB/10/21APR1546/DANIEL/OK TRANSFER CAX'UNT+3+363'");
			basic.Reload();
			AssertEquals("CB", basic.CustomsActionCode);
			AssertEquals("OK TRANSFER CAX", basic.LatestCustomsActionText);

			// Step 3, check in pieces, BUT NOT ALL
			basic.OutTurns.AddNew().C5_PackagesOutturned = 9;
			Factory.Save();
			// FRC omitted;

			// Step 4, cannot delete 
			AssertWhetherOptionPossibleFromMenu(basic, false, CcsukMenu.DeleteSendFrxMenuText);

			// Step 5, FSN/CX
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CX/10/21APR1546/DANIEL/REQUEST CANCELLED'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("CX", basic.CustomsActionCode);
			AssertEquals("Receipt of CX deletes info about thus-far-released pieces to allow new CAC to work", 0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));

			// Step 6, FSN/CB new shed (POO)
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CB/10/21APR1546/DANIEL/OK TRANSFER POO'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK); // GenAddOn (for CAT) is cached in old factory
			AssertEquals("CB", basic.CustomsActionCode);
			AssertEquals("OK TRANSFER POO", basic.LatestCustomsActionText);
			AssertNoPrintQueued("Release/Removal Authority", "210-42011011"); // no autoprint

			// Step 7, release to NEW shed 
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(basic, 9, true);  // The FSN processing will see St1 is set and automatically print for us
			AssertPrintQueued("Release/Removal Authority", "210-42011011", new string[] { "OK TRANSFER POO", "PART RELEASE", "9 OF 10" }, new string[] { "OK TRANSFER CAX", "LAST PART RELEASE" });
		}

		public void TestInterShedThenCancel_CheckReleaseCountsAreReset()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			// Step one create pre-arrival record
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "21042011011";
			basic.Profile = RecipientPima;
			basic.CM_FlightNo = "BA001";
			basic.DescriptionOfGoods = "BOOKS";
			basic.Weight = 10m;
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 0;
			basic.AgentBadge = "LXA";
			basic.AirportOfOrigin = "USATL";
			basic.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.TotalConsignmentManifested;
			basic.CM_ArrivalDate = ZDateTime.Now;
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CB/10/21APR1546/DANIEL/OK TRANSFER CAX'UNT+3+363'");
			basic.Reload();
			AssertEquals("CB", basic.CustomsActionCode);
			basic.NumberOfPiecesReceived = 10;
			basic.ReleaseThisNumberOfPieces(3, NumberOfPiecesReleasedHelper.ShedEvent);
			basic.ReleaseThisNumberOfPieces(4, NumberOfPiecesReleasedHelper.ShedEvent);
			AssertEquals(7, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent)); // from shed's POV
			basic.Profile = "CUKFFW98000LXA";
			basic.ReleaseThisNumberOfPieces(8, NumberOfPiecesReleasedHelper.AgentC1Event);
			AssertEquals(8, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event)); // from agent's POV
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CX/10/21APR1546/DANIEL/CANCELLED'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event)); // from agents's POV
			basic.Profile = RecipientPima;
			AssertEquals(0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event)); // from shed's POV
			AssertEquals("All release counts reset for CX update. Old values: agent C1=8, shed RRA=7. Include cancelled logs to see old values.", basic.Logs.MostRecentLog.SL_Reference);
		}

		public void TestEc01CStatusBasic()
		{
			// Steps 1 create AWB 
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "21042011010";
			basic.Profile = RecipientPima;
			basic.AgentBadge = "CAR";
			basic.AirportOfArrival = "LHR";
			basic.AirportOfDestination = "LHR";
			basic.AirportOfOrigin = "FRA";
			basic.CM_ArrivalDate = ZDateTime.Now;
			basic.ShipmentDescriptionCode = "C";
			basic.NumberOfPiecesExpected = 10;
			basic.Weight = 10m;
			basic.DescriptionOfGoods = "PENCILS";
			Factory.Save();

			// Step 2, set status 1.
			var ot = basic.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 10;
			Factory.Save();

			AssertEquals(false, basic.Status1Date.IsEmpty);
			AssertEquals(ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport, basic.ShipmentDescriptionCode);

			// Step 3, "produce EC release note"
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);  // Not yet EC released
			((ICcsukCusAwb)basic).SetEcStatusRelease(true);
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(basic, 10, true);
			AssertPrintQueued("Release/Removal Authority", "210-42011010", new string[] { "RELEASE NOTE", "EC Status", "CAR", "10KG", "PENCILS" }, new string[] { "PART RELEASE" });

			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);  // it's already been made, so don't show menu			 
		}

		public void TestImp08PreArrivalThenCcThenReleaseNote_Shed()
		{
			// Step one create pre-arrival record			
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "21042011011";
			basic.Profile = RecipientPima;
			basic.CM_FlightNo = "BA001";
			basic.DescriptionOfGoods = "BOOKS";
			basic.Weight = 10m;
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 0;
			basic.AgentBadge = "LXA";
			basic.AirportOfOrigin = "USATL";
			basic.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport;
			AssertHasErrorContaining(basic.ShipmentDescriptionCodeInfo, "Prearrivals must have SDC=T"); // Note #1
			basic.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.TotalConsignmentManifested;
			AssertNoErrorContaining(basic.ShipmentDescriptionCodeInfo, "Prearrivals must have SDC=T"); // Note #1
																									   // FRI message omitted

			// Step two, arrival
			basic.CM_ArrivalDate = ZDateTime.Now;
			// FRC message omitted

			// Step 3, agent does clearance and we receive FSN/CC 
			//  NB... ask Navinder why we get clearance without NPR. 
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CC/10/21APR1546/DANIEL/CLEARED BY DJC'UNT+3+363'");
			basic.Reload();
			AssertEquals(CustomsStatusCodes.Codes.ClearedByCustoms, basic.CustomsActionCode);
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true); // no pieces received yet

			// Step 4
			basic.NumberOfPiecesReceived = 10;
			// FRC message omitted

			//Step 5, release (clearance) note
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);  // Release note option is shown to the shed once the goods are clear
			AssertNoPrintQueued("Release", "210-42011011");
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(basic, 10, true);
			AssertPrintQueued("Release/Removal Authority", "210-42011011", new string[] { "Release", "CLEARED BY DJC" }, new string[] { "PART RELEASE" });
		}

		public void TestReleaseFindsCorrectFsnWhenTwoExistOneForShedAndOneForAgent()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "21042011011";
			basic.Profile = RecipientPima;
			basic.CM_FlightNo = "BA001";
			basic.DescriptionOfGoods = "BOOKS";
			basic.Weight = 10m;
			basic.NumberOfPiecesExpected = 10;
			basic.OutTurns.AddNew().C5_PackagesOutturned = 10;
			basic.AgentBadge = "LXA";
			basic.AirportOfOrigin = "USATL";
			basic.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.TotalConsignmentManifested;
			basic.CM_ArrivalDate = ZDateTime.Now;

			// TWO FSN messages are received into database, one for shed and one for agent. 
			var fsnText = "UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRBAC:210-42011011:CSN/CW/10/21APR1546/DANIEL/CLEARED BY DJC'UNT+3+363'";
			var messageToAgent = CreateReceivedMessage(fsnText, "CUKFFW98000LXA");
			messageToAgent.EM_Status = "RCV";
			Factory.Save();
			var messageToShed = ProcessReceivedMessage(fsnText);
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(basic, 10, true);
			var rraPrintJob = AssertPrintQueued("Release/Removal Authority", "210-42011011", new string[] { "CLEARED BY DJC" }, System.Array.Empty<string>());
			AssertEquals(messageToShed.PK, rraPrintJob.SP_ParentGuid);
		}

		void AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(ICcsukCusAwb awb, int correctNumberToRelease, bool alsoPressButtonToReleaseCorrectNumber = true)
		{
			var rraMenu = AssertWhetherReleaseDocumentPossibleFromMenu(awb, true);

			// This simulates Menu.PerformClick()
			var controller = new NonPersistentErtsReleaseOrchestrator(awb);
			using (var userControl = new ErtsReleaseUserControl(controller, null))
			{
				controller.ErtsReleaseHelper.NumberOfPieces = correctNumberToRelease;
				AssertEquals(correctNumberToRelease, controller.ErtsReleaseHelper.NumberOfPieces);
				controller.ErtsReleaseHelper.NumberOfPieces = correctNumberToRelease + 1;
				AssertEquals(correctNumberToRelease, controller.ErtsReleaseHelper.NumberOfPieces);
				if (alsoPressButtonToReleaseCorrectNumber)
				{
					userControl.ReleaseNowButton_Click(null, null);
					awb.Factory.Save();
				}
			}
		}

		protected override string ExpectedReleaseDocumentMenuTitle
		{
			get { return CcsukMenu.RraMenuTitle; }
		}

		protected override string RecipientPima
		{
			get { return "CUKAIR98LHRBAC"; }
		}
	}
}

