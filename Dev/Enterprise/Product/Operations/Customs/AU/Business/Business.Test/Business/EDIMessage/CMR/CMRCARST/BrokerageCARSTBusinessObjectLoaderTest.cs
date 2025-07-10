using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class BrokerageCARSTBusinessObjectLoaderTest : TestCaseWithFactory
	{
		public void TestIsInterestedInCARST()
		{
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(SeaMasterAndHouseCARSTMessage));
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(SeaMasterCARSTMessage));
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(AirMasterCARSTMessage));
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(AirMasterAndHouseCARSTMessage));

			AssertEquals("IsInterestedInCARST", false, Creator.IsInterestedInCARST(InvalidCARSTNoMaster));
			AssertEquals("IsInterestedInCARST", false, Creator.IsInterestedInCARST(InvalidCARSTNoMAWB));
			AssertEquals("IsInterestedInCARST", false, Creator.IsInterestedInCARST(InvalidCARSTNoEntryNumber));
		}

		public void TestLoadOrCreateRecordForMessageCoreWhenNoRecord()
		{
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(SeaMasterAndHouseCARSTMessage)[0]);
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(SeaMasterCARSTMessage)[0]);
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(AirMasterCARSTMessage)[0]);
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(AirMasterAndHouseCARSTMessage)[0]);

			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(InvalidCARSTNoMaster)[0]);
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(InvalidCARSTNoMAWB)[0]);
		}

		public void TestLoadForSeaMasterAndHouseCARSTMessage()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_MasterBill = "OB0987123";
			Declaration.JE_HouseBill = "HB4000";
			Declaration.JE_VoyageFlightNo = "4365";
			Declaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			Declaration.DoMerge();
			Factory.Save();
			AssertNotNull("Found Object", Creator.LoadOrCreateRecordForMessageCore(SeaMasterAndHouseCARSTMessage));
		}

		public void TestLoadForSeaMasterCARSTMessage()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_MasterBill = "OB0987123";
			Declaration.JE_VoyageFlightNo = "4365";
			Declaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			Declaration.DoMerge();
			Factory.Save();
			AssertNotNull("Found Object", Creator.LoadOrCreateRecordForMessageCore(SeaMasterCARSTMessage));
		}

		public void TestLoadForAirMasterCARSTMessage()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_MasterBill = "08112347775";
			Declaration.DoMerge();
			Factory.Save();
			AssertNotNull("Found Object", Creator.LoadOrCreateRecordForMessageCore(AirMasterCARSTMessage));
		}

		public void TestLoadForAirMasterAndHouseCARSTMessage()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_MasterBill = "08112347775";
			Declaration.JE_HouseBill = "AH3";
			Declaration.DoMerge();
			Factory.Save();
			AssertNotNull("Found Object", Creator.LoadOrCreateRecordForMessageCore(AirMasterAndHouseCARSTMessage));
		}

		public void TestFindPackingGroupForSea()
		{
			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Bill masterBill = Declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "Master Bill";
			Bill houseBill1 = Declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_MasterBill = "Master Bill";
			houseBill1.CU_HouseBill = "House Bill1";
			Bill houseBill2 = Declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_MasterBill = "Master Bill";
			houseBill2.CU_HouseBill = "House Bill2";
			Bill houseBill3 = Declaration.Bills.AddNew();
			houseBill3.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill3.CU_MasterBill = "Master Bill";
			houseBill3.CU_HouseBill = "House Bill3";
			Bill houseBill4 = Declaration.Bills.AddNew();
			houseBill4.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill4.CU_MasterBill = "Master Bill";
			houseBill4.CU_HouseBill = "House Bill3";
			PackingGroup packGroup1 = Declaration.PackingGroups.AddNew();
			PackingGroup packGroup2 = Declaration.PackingGroups.AddNew();
			PackingGroup packGroup3 = Declaration.PackingGroups.AddNew();
			PackingGroup packGroup4 = Declaration.PackingGroups.AddNew();
			PackingGroup packGroup5 = Declaration.PackingGroups.AddNew();
			Package pack1 = packGroup1.Packages.AddNew();
			Package pack2 = packGroup2.Packages.AddNew();
			Package pack3 = packGroup3.Packages.AddNew();
			Package pack4 = packGroup4.Packages.AddNew();
			Package pack5 = packGroup5.Packages.AddNew();
			packGroup1.CR_CU_HouseBill = houseBill1.PK;
			packGroup2.CR_CU_HouseBill = houseBill2.PK;
			packGroup3.CR_CU_HouseBill = houseBill3.PK;
			packGroup4.CR_CU_HouseBill = houseBill4.PK;
			packGroup5.CR_CU_HouseBill = houseBill3.PK;
			CusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1";
			CusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			CusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "C3";
			packGroup1.CR_CO_Container = container1.PK;
			packGroup2.CR_CO_Container = container2.PK;
			packGroup3.CR_CO_Container = container3.PK;
			packGroup4.CR_CO_Container = container2.PK;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders[0];
			AssertEquals("Copes with null entry header", null, Creator.FindPackingGroup(null, "Master Bill", "House Bill1", "C1"));
			AssertEquals("Match to correct pack group", packGroup1, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill1", "C1"));
			AssertEquals("Match to correct pack group", packGroup2, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill2", "C2"));
			AssertEquals("Match to correct pack group", packGroup3, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill3", "C3"));
			AssertEquals("Match to correct pack group", packGroup4, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill3", "C2"));
			AssertEquals("Match to correct pack group", packGroup5, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill3", ""));
			AssertEquals("HBL does not match", null, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill4", ""));
			AssertEquals("OBL is ignored for Sea", packGroup5, Creator.FindPackingGroup(entryHeader, "Master Bill2", "House Bill3", ""));
			AssertEquals("Container in call but none on pack group", null, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill3", "C1"));

			AssertEquals("OBL is ignored for Sea", packGroup1, Creator.FindPackingGroup(entryHeader, "Master BillXX", "House Bill1", "C1"));
			AssertEquals("OBL is ignored for Sea", packGroup1, Creator.FindPackingGroup(entryHeader, "", "House Bill1", "C1"));
			houseBill1.CU_HouseBill = ZString.Empty;
			AssertEquals("OBL is ignored for Sea", packGroup1, Creator.FindPackingGroup(entryHeader, "Master Bill", "", "C1"));
			AssertEquals("OBL is ignored for Sea", packGroup1, Creator.FindPackingGroup(entryHeader, "Master Billxx", "", "C1"));
			AssertEquals("HBL does not match", null, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill1", "C1"));
			AssertEquals("HBL does not match", null, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill2", "C1"));
			AssertEquals("Container does not match to blank HBL pack group", null, Creator.FindPackingGroup(entryHeader, "Master Bill", "", "C2"));
			AssertEquals("No Container does not match to blank HBL pack group", null, Creator.FindPackingGroup(entryHeader, "Master Bill", "", ""));
			AssertEquals("No match on container", null, Creator.FindPackingGroup(entryHeader, "", "House Bill2", "C1"));
			AssertEquals("No match on HBL", null, Creator.FindPackingGroup(entryHeader, "", "House Bill", "C2"));
		}

		public void TestFindPackingGroupForAir()
		{
			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Bill masterBill = Declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "Master Bill";
			Bill houseBill1 = Declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_MasterBill = "Master Bill";
			houseBill1.CU_HouseBill = "House Bill1";
			Bill houseBill2 = Declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_MasterBill = "Master Bill";
			houseBill2.CU_HouseBill = "House Bill2";
			Bill houseBill3 = Declaration.Bills.AddNew();
			houseBill3.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill3.CU_MasterBill = "Master Bill";
			houseBill3.CU_HouseBill = "House Bill3";
			PackingGroup packGroup1 = Declaration.PackingGroups.AddNew();
			PackingGroup packGroup2 = Declaration.PackingGroups.AddNew();
			PackingGroup packGroup3 = Declaration.PackingGroups.AddNew();
			Package pack1 = packGroup1.Packages.AddNew();
			Package pack2 = packGroup2.Packages.AddNew();
			Package pack3 = packGroup3.Packages.AddNew();
			packGroup1.CR_CU_HouseBill = houseBill1.PK;
			packGroup2.CR_CU_HouseBill = houseBill2.PK;
			packGroup3.CR_CU_HouseBill = houseBill3.PK;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders[0];
			AssertEquals("Copes with null entry header", null, Creator.FindPackingGroup(null, "Master Bill", "House Bill1", ""));
			AssertEquals("Match to correct pack group", packGroup1, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill1", ""));
			AssertEquals("Match to correct pack group", packGroup2, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill2", ""));
			AssertEquals("Match to correct pack group", packGroup3, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill3", ""));
			AssertEquals("No Match to HBL", null, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill4", ""));
			AssertEquals("Master must match for Air", null, Creator.FindPackingGroup(entryHeader, "Master Bill2", "House Bill3", ""));

			AssertEquals("Master must match for Air", null, Creator.FindPackingGroup(entryHeader, "Master BillXX", "House Bill1", ""));
			AssertEquals("Master must match for Air", null, Creator.FindPackingGroup(entryHeader, "", "House Bill1", ""));
			houseBill1.CU_HouseBill = ZString.Empty;
			AssertEquals("No House so match on Master only", packGroup1, Creator.FindPackingGroup(entryHeader, "Master Bill", "", ""));
			AssertEquals("No Match to Master", null, Creator.FindPackingGroup(entryHeader, "Master Billxx", "", ""));
			AssertEquals("No Match to House", null, Creator.FindPackingGroup(entryHeader, "Master Bill", "House Bill1", ""));
			AssertEquals("No Match", null, Creator.FindPackingGroup(entryHeader, "", "House Bill1", ""));
			AssertEquals("No Match", null, Creator.FindPackingGroup(entryHeader, "", "", ""));
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
					fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					fDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					fDeclaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
					JobComInvoiceHeader invHeader = fDeclaration.Invoices.AddNew();
					JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();
					SendsMessagesToCustomsShutterUpperer shutterUpper = new SendsMessagesToCustomsShutterUpperer();
					fDeclaration.MessageInitiator = shutterUpper;
				}

				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		BrokerageCARSTBusinessObjectLoader fCreator;
		BrokerageCARSTBusinessObjectLoader Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new BrokerageCARSTBusinessObjectLoader();
				}
				return fCreator;
			}
		}

		#region Sea CARST Messages

		CMRCARSTMessage SeaMasterAndHouseCARSTMessage
		{
			get
			{
				if (fSeaMasterAndHouseCARSTMessage == null)
				{
					fSeaMasterAndHouseCARSTMessage = Factory.New<CMRCARSTMessage>();
					fSeaMasterAndHouseCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
RFF+MB:OB0987123'
RFF+BH:HB4000'
RFF+AAQ:C001'
RFF+ABT:AAAFHA6GH'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
				}
				return fSeaMasterAndHouseCARSTMessage;
			}
		}

		CMRCARSTMessage SeaMasterCARSTMessage
		{
			get
			{
				if (fSeaMasterCARSTMessage == null)
				{
					fSeaMasterCARSTMessage = Factory.New<CMRCARSTMessage>();
					fSeaMasterCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
RFF+MB:OB0987123'
RFF+AAQ:C001'
RFF+ABT:AAAFHA6GH'
DOC+1'
PAC+++LCL:67:95'
UNT+33+000001'".Replace("\r\n", "");
				}
				return fSeaMasterCARSTMessage;
			}
		}

		CMRCARSTMessage fSeaMasterAndHouseCARSTMessage;
		CMRCARSTMessage fSeaMasterCARSTMessage;

		#endregion

		#region Air CARST Messages

		CMRCARSTMessage AirMasterCARSTMessage
		{
			get
			{
				if (fAirMasterCARSTMessage == null)
				{
					fAirMasterCARSTMessage = Factory.New<CMRCARSTMessage>();
					fAirMasterCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+21A6 FB43 0AB5:1+8'
DTM+9:20050812082715331950:ZZZ'
DTM+132:20050722:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+270++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+ABT:AAAFHA6GH'
RFF+MWB:08112347775'
DOC+1'
PAC+0000200'
UNT+16+000001'".Replace("\r\n", "");
				}
				return fAirMasterCARSTMessage;
			}
		}

		CMRCARSTMessage AirMasterAndHouseCARSTMessage
		{
			get
			{
				if (fAirMasterAndHouseCARSTMessage == null)
				{
					fAirMasterAndHouseCARSTMessage = Factory.New<CMRCARSTMessage>();
					fAirMasterAndHouseCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+21A6 FB43 0AB5:1+8'
DTM+9:20050812082715331950:ZZZ'
DTM+132:20050722:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+270++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+ABT:AAAFHA6GH'
RFF+MWB:08112347775'
RFF+HWB:AH3'
DOC+1'
PAC+0000200'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fAirMasterAndHouseCARSTMessage;
			}
		}

		CMRCARSTMessage fAirMasterCARSTMessage;
		CMRCARSTMessage fAirMasterAndHouseCARSTMessage;

		#endregion

		#region Invalid CARST

		CMRCARSTMessage InvalidCARSTNoMAWB
		{
			get
			{
				if (fInvalidCARSTNoMAWB == null)
				{
					fInvalidCARSTNoMAWB = Factory.New<CMRCARSTMessage>();
					fInvalidCARSTNoMAWB.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+21A6 FB43 0AB5:1+8'
DTM+9:20050812082715331950:ZZZ'
DTM+132:20050722:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+270++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+ABT:AAAFHA6GH'
RFF+HWB:08112347775'
DOC+1'
PAC+0000200'
UNT+16+000001'".Replace("\r\n", "");
				}
				return fInvalidCARSTNoMAWB;
			}
		}

		CMRCARSTMessage fInvalidCARSTNoMAWB;

		CMRCARSTMessage InvalidCARSTNoMaster
		{
			get
			{
				if (fInvalidCARSTNoMaster == null)
				{
					fInvalidCARSTNoMaster = Factory.New<CMRCARSTMessage>();
					fInvalidCARSTNoMaster.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
RFF+BH:HB4000'
RFF+AAQ:C001'
RFF+ABT:AAAFHA6GH'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
				}
				return fInvalidCARSTNoMaster;
			}
		}

		CMRCARSTMessage fInvalidCARSTNoMaster;

		CMRCARSTMessage InvalidCARSTNoEntryNumber
		{
			get
			{
				if (fInvalidCARSTNoEntryNumber == null)
				{
					fInvalidCARSTNoEntryNumber = Factory.New<CMRCARSTMessage>();
					fInvalidCARSTNoEntryNumber.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
RFF+MB:OB0987123'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
UNT+33+000001'".Replace("\r\n", "");
				}
				return fInvalidCARSTNoEntryNumber;
			}
		}

		CMRCARSTMessage fInvalidCARSTNoEntryNumber;

		#endregion

		#endregion
	}
}
