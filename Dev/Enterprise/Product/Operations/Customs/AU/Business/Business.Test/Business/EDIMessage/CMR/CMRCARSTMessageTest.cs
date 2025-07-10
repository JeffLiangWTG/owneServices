using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCARSTMessage))]
	sealed class CMRCARSTMessageTest : CMRCUSRESMessageTest
	{
		public void TestContainerModes()
		{
			Assert(GetContainerModeMessage(CMRImportCargoTypes.Codes.FullContainerLoad).IsFCL);
			Assert(GetContainerModeMessage(CMRImportCargoTypes.Codes.BreakBulk).IsBreakBulk);
			Assert(GetContainerModeMessage(CMRImportCargoTypes.Codes.Bulk).IsBulk);
			Assert(GetContainerModeMessage(CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills).IsFCX);
			Assert(GetContainerModeMessage(CMRImportCargoTypes.Codes.LessThanContainerLoad).IsLCL);
		}

		public void TestProcessOrphanedMessage()
		{
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+436S++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB0987123'
RFF+BH:HB4000'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");

			message.SetEM_LinkedObject();

			AssertEquals(CusSCAOceanBill.Schema.TableName, message.EM_LinkTable);
			AssertEquals("OB0987123", message.EM_ApplicationReference);
			AssertEquals("HB4000", message.EM_MessageOwner);
		}

		[TestDate(2005, 4, 5)]
		public void TestMessageLinksToCusHAWB()
		{
			BusinessObject expectedLinkedObject = HAWB;
			Factory.Save();
			HAWBCMRCARSTMessage.SetEM_LinkedObject();
			AssertEquals("LinkedObject", HAWB, HAWBCMRCARSTMessage.EM_LinkedObject);
		}

		[TestDate(2005, 4, 5)]
		public void TestMessageLinksToCTOCusHAWB()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_IsAirCTO = true;
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = DepotPremiseID;
			BusinessObject expectedLinkedObject = CTOHAWB;
			Factory.Save();
			MAWBCMRCARSTMessage.SetEM_LinkedObject();
			AssertEquals("LinkedObject", CTOHAWB, MAWBCMRCARSTMessage.EM_LinkedObject);
		}

		public void TestStatusDescription()
		{
			ZString statusDescription = HAWBCMRCARSTMessage.GetStatusDescription();
			AssertEquals("Status Description", @"
	CONSOLIDATED STATUS: HELD
	DEPARTURE FROM LAST OVERSEAS PORT: YES
	QUOTED MASTER / OCEAN BILL EXISTS: YES
	IAR ACS CLEARED: YES
	COMPLETE UNDERBOND SERIES APPROVED: N/A
	LCL UNDERBOND SATISFIED: N/A
	DECONSOLIDATION UNDERBOND SATISFIED: YES
	CARGO REPORT ACS EVALUATED: YES
	IAR AQIS CLEARED: YES
	CARGO REPORT AQIS EVALUATED: YES
	IMPORT DECLARATIONS MATCHED: NO
	IMPORT DECLARATION ACS EVALUATED: NO
	IMPORT DECLARATION AQIS EVALUATED: YES
	ACS EVALUATION COMPLETE: YES
	AQIS CARGO REPORT EVALUATION COMPLETE: YES
	ACS IMPORT DECLARATION EVALUATION COMPLETE: NO
	AQIS IMPORT DECLARATION EVALUATION COMPLETE: YES
	IMPORT DECLARATION PAID: N/A
", statusDescription);
		}

		//		public void TestDepotReceivingUBMREQRAndCARSTScenario()
		//		{
		//			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9122P";
		//			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
		//			LoggingInformation Logger = new LoggingInformation();
		//			new UBMREQRMessageProcessor(Logger).ProcessMessage(SeaDepotUBMREQRMessage);
		//			var OceanBill = Factory.LoadTop1<CusSCAOceanBill>(new ZQuery());
		//			AssertNotNull("OceanBill", OceanBill);
		//			AssertEquals(1, OceanBill.Containers.Count);
		//			AssertEquals(0, OceanBill.HouseBills.Count);
		//			new CARSTMessageProcessor(Logger).ProcessMessage(SeaDepotCARSTMessage);
		//			AssertEquals(1, OceanBill.HouseBills.Count);
		//		}
		//
		//
		public void TestContainerNumber()
		{
			AssertEquals("C001", HouseBillCMRCARSTMessage.ContainerNumber);
		}

		public void TestContainerMode()
		{
			AssertEquals("LCL", HouseBillCMRCARSTMessage.ContainerMode);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals(200, HouseBillCMRCARSTMessage.NumberOfPackages);
		}

		public void TestPackageType()
		{
			AssertEquals("BX", HouseBillCMRCARSTMessage.PackageType);
		}

		public void TestPortOfDischarge()
		{
			AssertEquals("AUSYD", HouseBillCMRCARSTMessage.PortOfDischarge);
		}

		public void TestPortOfDischargeWithNull()
		{
			AssertEquals("", HouseBillCMRCARSTMessageWithNull.PortOfDischarge);
		}

		public void TestPremiseLocation()
		{
			AssertEquals(DepotPremiseID, HouseBillCMRCARSTMessage.PremiseID);
		}

		#region ICusSCAHouseInfoProvider Tests

		public void TestVoyageNumber()
		{
			AssertEquals("4365", ((ICusSCAHouseInfoProvider)HouseBillCMRCARSTMessage).VoyageNumber);
		}

		public void TestLloydsNumber()
		{
			AssertEquals("8811924", ((ICusSCAHouseInfoProvider)HouseBillCMRCARSTMessage).LloydsNumber);
		}

		public void TestOceanBillNumber()
		{
			AssertEquals("OB0987123", ((ICusSCAHouseInfoProvider)HouseBillCMRCARSTMessage).OceanBillNumber);
		}

		public void TestHouseBillNumber()
		{
			AssertEquals("HB4000", ((ICusSCAHouseInfoProvider)HouseBillCMRCARSTMessage).HouseBillNumber);
		}

		public void TestGetArrivalDate()
		{
			AssertEquals(new ZDateTime(2005, 4, 4), ((ICusHAWBInformationProvider)HAWBCMRCARSTMessage).ArrivalDate);
		}

		#endregion

		#region CFS/Depot Stand Alone/Integrated

		public void TestIntegragedUnpackCFSSeaCargoDepot()
		{
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			currentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = DepotPremiseID;
			currentBranch.OrgProxy.OH_IsUnpackDepot = true;

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing(LloydsNumber, VoyageNumber).PK;

			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "C001";

			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			header.C6_OutturningPremiseID = DepotPremiseID;
			header.C6_LloydsIMO = LloydsNumber;
			header.C6_VoyageNum = VoyageNumber;

			CusUnderbond expectedCargoArrival = Factory.New<CusUnderbond>();
			expectedCargoArrival.C4_DestinationPremiseID = DepotPremiseID;
			expectedCargoArrival.C4_C6 = header.PK;

			HouseBillCMRCARSTMessage.EM_MessageNum = TestMessageNumber;
			HouseBillCMRCARSTMessage.SetEM_LinkedObject();

			ZQuery filter = new ZQuery(JobShipmentSchema.JS_HouseBill, "HB4000");
			CFSShipment[] shipments = (CFSShipment[])Factory.Load(typeof(CFSShipment), filter);
			AssertEquals("Should have created 1 house bill", 1, shipments.Length);
		}

		#endregion

		#region Implementation

		const string DepotPremiseID = "9914N";
		const string LloydsNumber = "8811924";
		const string VoyageNumber = "4365";

		CMRCARSTMessage fHouseBillCMRCARSTMessage;
		CMRCARSTMessage HouseBillCMRCARSTMessage
		{
			get
			{
				if (fHouseBillCMRCARSTMessage == null)
				{
					fHouseBillCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
					fHouseBillCMRCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+4365++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:S00023948/CMT::1'
RFF+MB:OB0987123'
RFF+BH:HB4000'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
PAC+0000200++BX:185:95'
UNT+35+000001'".Replace("\r\n", "");
				}
				return fHouseBillCMRCARSTMessage;
			}
		}

		CMRCARSTMessage fHouseBillCMRCARSTMessageWithNull;
		CMRCARSTMessage HouseBillCMRCARSTMessageWithNull
		{
			get
			{
				if (fHouseBillCMRCARSTMessageWithNull == null)
				{
					fHouseBillCMRCARSTMessageWithNull = Factory.New<CMRCARSTMessage>();
					fHouseBillCMRCARSTMessageWithNull.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+4365++11++++8811924::11'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB0987123'
RFF+BH:HB4000'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
PAC+0000200++BX:185:95'
UNT+35+000001'".Replace("\r\n", "");
				}
				return fHouseBillCMRCARSTMessageWithNull;
			}
		}

		CMRCARSTMessage fHAWBCMRCARSTMessage;
		CMRCARSTMessage HAWBCMRCARSTMessage
		{
			get
			{
				if (fHAWBCMRCARSTMessage == null)
				{
					fHAWBCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
					fHAWBCMRCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+5AFE B52J 755:1+8'
DTM+9:20050725203721536492:ZZZ'
DTM+132:20050404:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:NO'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:NO'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:YES'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
TDT+20+300++6+QF::3'
LOC+12+AUSYD::6'
NAD+MR+AAA374M::95'
RFF+MWB:08130038433'
RFF+HWB:1'
UNT+28+000001'".Replace("\r\n", "");
				}
				return fHAWBCMRCARSTMessage;
			}
		}

		CMRCARSTMessage fMAWBCMRCARSTMessage;
		CMRCARSTMessage MAWBCMRCARSTMessage
		{
			get
			{
				if (fMAWBCMRCARSTMessage == null)
				{
					fMAWBCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
					fMAWBCMRCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+5AFE B52J 755:1+8'
DTM+9:20050725203721536492:ZZZ'
DTM+132:20050404:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:NO'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:NO'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:YES'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
TDT+20+300++6+QF::3'
LOC+12+AUSYD::6'
NAD+MR+AAA374M::95'
RFF+MWB:08130038433'
UNT+27+000001'".Replace("\r\n", "");
				}
				return fMAWBCMRCARSTMessage;
			}
		}

		CTOCusHAWB fCTOHAWB;
		CTOCusHAWB CTOHAWB
		{
			get
			{
				if (fCTOHAWB == null)
				{
					CTOCusMAWB cTOMAWB = Factory.New<CTOCusMAWB>();
					cTOMAWB.CM_FlightNo = "QF300";
					cTOMAWB.CM_ArrivalDate = new ZDateTime(2005, 4, 4, 12, 23, 45);

					fCTOHAWB = cTOMAWB.ChildBills.AddNew();
					fCTOHAWB.CS_HAWB = "08130038433";
				}
				return fCTOHAWB;
			}
		}

		CusHAWB fHAWB;
		CusHAWB HAWB
		{
			get
			{
				if (fHAWB == null)
				{
					fHAWB = MAWB.ChildBills.AddNew();
					fHAWB.CS_HAWB = "1";
				}
				return fHAWB;
			}
		}

		CusMAWB fMAWB;
		CusMAWB MAWB
		{
			get
			{
				if (fMAWB == null)
				{
					fMAWB = Factory.New<CusMAWB>();
					MAWB.CM_MAWB = "08130038433";
					MAWB.CM_FlightNo = "QF300";
					MAWB.CM_ArrivalDate = new ZDateTime(2005, 4, 4, 12, 23, 45);
				}
				return fMAWB;
			}
		}

		CMRCARSTMessage GetContainerModeMessage(ZString containerMode)
		{
			CMRCARSTMessage result = Factory.New<CMRCARSTMessage>();
			result.EM_MessageText = (@"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+1J43 IGEF AAB5:1+32'
DTM+9:20050812082119805537:ZZZ'
FTX+AAH+++AAA447YL5040039C0001/OOLU1234567'
TDT+20+022S++11++++9275385::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++" + containerMode + @":67:95'
PAC+0001200++BX:185:95'
RFF+AAQ:OOLU1234567'
UNT+17+000001'").Replace("\r\n", "");
			return result;
		}

		JobSailing CreateSailing(ZString lloydsNumber, ZString voyageNumber)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageNumber;
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			destination.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#endregion

	}
}
