using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class CcsukAgentRemovalTestScenarios : CcsukRemovalTestScenariosBase
	{
		public void TestCwClearanceDocumentShowsOrHidesDeclarationSection()
		{
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			var basic = AssertAwbExists<CusMAWB>("210-42011010");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+363'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CC/10/21APR1546/AGENTREF/CLEARED'UNT+3+363'");
			AssertPrintQueued("Clearance", "210-42011010", new string[] { "CLEARED", "AGENTREF" }, new string[] { "DUCR" });  // when no declaration linked, do not show any declaration details

			SimulatePurgingOfPrintQueue();
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011011+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			var basic2 = AssertAwbExists<CusMAWB>("210-42011011");
			var declaration = ((ICcsukCusAwb)basic2).CreateNewStandaloneCDSDeclaration();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_UCR = "DUCR123";
			declaration.JE_OwnerRef = "BOX7REF";
			declaration.SingleEntry.CH_RouteOfEntry = "69";
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+363'FTX+CIM+++FSN:LHRCAX:210-42011011:CSN/CC/10/21APR1546/AGENTREF/CLEARED2'UNT+3+363'");
			AssertPrintQueued("Clearance", "210-42011011", new string[] { "CLEARED", "AGENTREF", "69", "DUCR123", "BOX7REF" }, Array.Empty<string>());  // When a declaration is linked, show details on clearance
		}

		public void TestRemoval04_C1AfterCancelledRequest()
		{
			// Do not assert the read-only nature of fields here. Here is not the right place. 
			// When the service task changes branch context using DisposibleEnvironment.ForBranch() to execute as each UK branch, it also (stupidly, IMO) changes the user.  
			//	So in this test the task is executed and the context and therefore user changes. 
			// The read-only nature of fields is overridden (i.e. made not read-only) when the user is ediSupport, but in this test the context and user changes to the service user. 
			// Test method TestPropertiesReadOnlyFromCAC() makes these checks for us. 

			// Step 1, create received AWB
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			var basic = AssertAwbExists<CusMAWB>("210-42011010");
			AssertEquals(false, basic.Status1Date.IsEmpty);

			// Step two, TSR and FSN/CA
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(basic.TSRs);
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+363/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CA/10/21APR1546/00000001/REQUEST TFR LGW'UNT+3+363'");
			basic.Reload();
			AssertEquals("CA", basic.CustomsActionCode);

			// Step 3, FSN/CX
			ProcessReceivedMessage("UNH+364+CIMFSN:0:0:Z1:IATA+364/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CX/10/21APR1546/00000001/DELETED'UNT+3+364'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("CX", basic.CustomsActionCode);

			// Step 4, new TSR, CUSDEC, CUSRES and FSN/CT
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(basic.TSRs);
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+2/U00000002'BGM+TSR+21042011010'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CT:120:ZZZ'FTX+CAT+++OK TRANSFER SYD'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+365+CIMFSN:0:0:Z1:IATA+365/U00000002'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CT/10/22APR1546/00000002/OK TRANSFER SYD'UNT+3+365'");
			basic.Reload();
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			basic.TSRs[1].Reload();
			AssertEquals("T002120", basic.TSRs[1].TranshipmentEntryNumber);
			AssertEquals("CT", basic.CustomsActionCode);

			// Step 5, (auto-)produce C1
			AssertPrintQueued("C1", "210-42011010", new string[] { "OK TRANSFER SYD", "C1 reprint", "AGENT'S TRAVELLING COPY REMOVAL AUTHORITY", "CAR", "10KG", "PENCILS", "aod SYD", "T002120" }, new string[] { "PART RELEASE" });

			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false);  // it's already been made, so don't show menu			
		}

		public void TestRemoval24_BasicAwbC1RemovalDocs()
		{
			// Step 1, four FRIs with NPR=0
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE1+CUSCAR:1:912:UN'BGM+:::FRI+21042011011+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'MEA+WT++KGM:10'UNT+12+JRI2BISB5GVRE1'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE2+CUSCAR:1:912:UN'BGM+:::FRI+21042011012+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS2'QTY+118:10'MEA+WT++KGM:10'UNT+12+JRI2BISB5GVRE2'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE3+CUSCAR:1:912:UN'BGM+:::FRI+21042011013+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS3'QTY+118:10'MEA+WT++KGM:10'UNT+12+JRI2BISB5GVRE3'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE4+CUSCAR:1:912:UN'BGM+:::FRI+21042011014+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS4'QTY+118:10'MEA+WT++KGM:10'UNT+12+JRI2BISB5GVRE4'");

			var basic1 = AssertAwbExists<CusMAWB>("210-42011011");
			var basic2 = AssertAwbExists<CusMAWB>("210-42011012");
			var basic3 = AssertAwbExists<CusMAWB>("210-42011013");
			var basic4 = AssertAwbExists<CusMAWB>("210-42011014");

			// Step 2, receive pieces for all
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE5+CUSCAR:1:912:UN'BGM+:::FRC+21042011011+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE5'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE6+CUSCAR:1:912:UN'BGM+:::FRC+21042011012+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS2'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE6'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE7+CUSCAR:1:912:UN'BGM+:::FRC+21042011013+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS3'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE7'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE8+CUSCAR:1:912:UN'BGM+:::FRC+21042011014+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS4'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE8'");

			// Step 3, ISR... omitted because this cannot be done by agent.  But we can get an FSN for it in step 7.

			// Step 4, IAR
			CreateRemovalObjectAndMessage<InterAirportRemoval, CcsukTransmissionMessageFunction.CUSDEC.IAR>(basic2.IARs, delegate(InterAirportRemoval ub)
			{ ub.AirportOrCountryOfDestination = "LGW"; ub.NewShedId = "BAC"; });

			//Step 5, TSR, LRI=no
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(basic3.TSRs);

			// Step 6, TSR YES
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(basic4.TSRs, delegate(TranshipmentRemoval ub)
			{ ub.LicenseRestrictionInd = "Y"; ub.AirportOrCountryOfDestination = "MEL"; });

			// Step 7 - receive FSNs and CUSRES
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+3/U00000003'BGM+TSR+21042011014'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CA:120:ZZZ'FTX+CAT+++REQUEST TRANSFER MEL'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRCAX:210-42011011:CSN/CB/100/21APR1546/00000001/OK TRANSFER BAC'UNT+3+363'");
			ProcessReceivedMessage("UNH+364+CIMFSN:0:0:Z1:IATA+364/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011012:CSN/CW/10/21APR1546/00000001/OK TRANSFER LGW'UNT+3+364'");
			ProcessReceivedMessage("UNH+365+CIMFSN:0:0:Z1:IATA+365/U00000002'FTX+CIM+++FSN:LHRCAX:210-42011013:CSN/CT/10/21APR1546/00000002/OK TRANSFER SYD'UNT+3+365'");
			ProcessReceivedMessage("UNH+366+CIMFSN:0:0:Z1:IATA+366/U00000003'FTX+CIM+++FSN:LHRCAX:210-42011014:CSN/CA/10/21APR1546/00000003/REQUEST TRANSFER MEL'UNT+3+366'");
			basic1 = new BusinessObjectFactory().Load<CusMAWB>(basic1.PK);
			basic2 = new BusinessObjectFactory().Load<CusMAWB>(basic2.PK);
			basic3 = new BusinessObjectFactory().Load<CusMAWB>(basic3.PK);
			basic4 = new BusinessObjectFactory().Load<CusMAWB>(basic4.PK);
			AssertEquals("CB", basic1.CustomsActionCode);
			AssertEquals("CW", basic2.CustomsActionCode);
			AssertEquals("CT", basic3.CustomsActionCode);
			AssertEquals("CA", basic4.CustomsActionCode);

			// GR print
			AssertPrintQueued("GR", "210-42011014", new string[] { "REQUEST TRANSFER MEL", "GR", "ADVICE OF SELECTED REMOVAL REQUEST" }, Array.Empty<string>());

			// Step 10 (nb out of sequence to show that C1s are from step 7 and not from steps 8/9
			AssertPrintQueued("C1", "210-42011012", new string[] { "OK TRANSFER LGW", "C1 reprint", "AGENT'S TRAVELLING COPY REMOVAL AUTHORITY", "CAR", "10KG", "PENCILS2", "aod LGW", "shed BAC" }, new string[] { "PART RELEASE" });
			AssertPrintQueued("C1", "210-42011013", new string[] { "OK TRANSFER SYD", "C1 reprint", "AGENT'S TRAVELLING COPY REMOVAL AUTHORITY", "CAR", "10KG", "PENCILS3", "aod SYD" }, new string[] { "PART RELEASE" });

			AssertNoPrintQueued("C1", "210-42011011");
			AssertNoPrintQueued("C1", "210-42011014");

			AssertWhetherReleaseDocumentPossibleFromMenu(basic1, false); // CB (ISR) should not allow C1 for agent
			AssertWhetherReleaseDocumentPossibleFromMenu(basic2, false); // Already made
			AssertWhetherReleaseDocumentPossibleFromMenu(basic3, false); // Already made
			AssertWhetherReleaseDocumentPossibleFromMenu(basic4, false); // CA

			// Steps 8 & 9 - FSN/CS
			ProcessReceivedMessage("UNH+367+CIMFSN:0:0:Z1:IATA+367/U00000003'FTX+CIM+++FSN:LHRCAX:210-42011014:CSN/CS/10/21APR1546/00000004/SEIZED'UNT+3+367'");
			basic4 = new BusinessObjectFactory().Load<CusMAWB>(basic4.PK);
			AssertNoPrintQueued("C1", "210-42011014");
			AssertWhetherReleaseDocumentPossibleFromMenu(basic4, false); // CS locks job
			AssertEquals("CS", basic4.CustomsActionCode);
			AssertEquals(true, basic4.AirportOfOriginInfo.ReadOnly);
		}

		public void TestRemoval25_BasicAwbSplitLevelC1RemovalDocuments()
		{
			// Steps 1 & 2, create received AWB
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			var basic = AssertAwbExists<CusMAWB>("210-42011010");
			AssertEquals(false, basic.AirportOfOriginInfo.ReadOnly);

			// Step 2 - process FCS to make four splits. 
			ProcessReceivedMessage("UNH+JCS1BSJXKFYXQ0+CUSCAR:1:912:UN'BGM+:::FCS+21042011010'GIS+S2Y'TDT+20'LOC+11:LHR:145:3::CAX:129:ZZZ'GID+01'QTY+118:1'MEA+WT++KGM:1'GID+02'QTY+118:2'MEA+WT++KGM:2'GID+03'QTY+118:3'MEA+WT++KGM:3'GID+04'QTY+118:4'MEA+WT++KGM:4'UNT+18+JCS1BSJXKFYXQ0'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);  // split collection is cached
			AssertEquals(true, basic.AirportOfOriginInfo.ReadOnly);
			Assert(basic.HasSplits);
			var split1 = basic.Splits["01"];
			var split2 = basic.Splits["02"];
			var split3 = basic.Splits["03"];
			var split4 = basic.Splits["04"];

			// Steps 4, 5, & 6
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(basic.TSRs, delegate(TranshipmentRemoval ub)
			{ ub.SplitReferenceToWhichThisRemovalPertains = "01"; ub.AirportOrCountryOfDestination = "BGW"; });
			CreateRemovalObjectAndMessage<InterAirportRemoval, CcsukTransmissionMessageFunction.CUSDEC.IAR>(basic.IARs, delegate(InterAirportRemoval ub)
			{ ub.SplitReferenceToWhichThisRemovalPertains = "03"; ub.AirportOrCountryOfDestination = "LGW"; ub.NewShedId = "BAC"; });
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(basic.TSRs, delegate(TranshipmentRemoval ub)
			{ ub.SplitReferenceToWhichThisRemovalPertains = "04"; ub.AirportOrCountryOfDestination = "ZRH"; });

			// Implied step 6b - make chief entry.  Can be omitted for test.			

			// Step 7 - receive CUSRES and FSNs 
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+3/U00000003'BGM+TSR+21042011010+++ACD::04'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CT:120:ZZZ'FTX+CAT+++OK TRANSFER ZRH'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+1/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CS-01/10/21APR1546/00000001/SEIZED'UNT+3+363'");
			ProcessReceivedMessage("UNH+364+CIMFSN:0:0:Z1:IATA'            FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CC-02/10/21APR1546/B1234567/CLEARED'UNT+3+364'".Replace(" ", ""));
			ProcessReceivedMessage("UNH+365+CIMFSN:0:0:Z1:IATA+2/U00000002'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CW-03/10/21APR1546/00000002/OK TRANSFER LGW'UNT+3+365'");
			ProcessReceivedMessage("UNH+366+CIMFSN:0:0:Z1:IATA+3/U00000003'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CT-04/10/21APR1546/00000003/OK TRANSFER ZRH'UNT+3+366'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			split1 = new BusinessObjectFactory().Load<SplitBasic>(split1.PK);
			split2 = new BusinessObjectFactory().Load<SplitBasic>(split2.PK);
			split3 = new BusinessObjectFactory().Load<SplitBasic>(split3.PK);
			split4 = new BusinessObjectFactory().Load<SplitBasic>(split4.PK);
			AssertEquals("CS", split1.CustomsActionCode);
			AssertEquals("CC", split2.CustomsActionCode);
			AssertEquals("CW", split3.CustomsActionCode);
			AssertEquals("CT", split4.CustomsActionCode);
			basic.TSRs[1].Reload();
			AssertEquals("T002120", basic.TSRs[1].TranshipmentEntryNumber);

			// Step 8 - check no C1 for CS
			AssertNoPrintQueued("C1", "210-42011010/01");
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false);
			AssertWhetherReleaseDocumentPossibleFromMenu(split1, false);

			// Step 9 - remaining C1s
			AssertPrintQueued("C1", "210-42011010/03", new string[] { "OK TRANSFER LGW", "C1 reprint", "AGENT'S TRAVELLING COPY REMOVAL AUTHORITY", "CAR", "3KG", "PENCILS", "aod LGW", "aod shed BAC" }, new string[] { "PART RELEASE", "T002120" });
			AssertPrintQueued("C1", "210-42011010/04", new string[] { "OK TRANSFER ZRH", "C1 reprint", "AGENT'S TRAVELLING COPY REMOVAL AUTHORITY", "CAR", "4KG", "PENCILS", "aod ZRH", "T002120" }, new string[] { "PART RELEASE" });
			AssertWhetherReleaseDocumentPossibleFromMenu(split3, false);
			AssertWhetherReleaseDocumentPossibleFromMenu(split4, false);

			AssertWhetherReleaseDocumentPossibleFromMenu(split2, false); //CC
		}

		public void TestRemoval26_InhibitC1RemovalOfExcessNpr()
		{
			// Step 1 - no NPR
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE1+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'MEA+WT++KGM:10'UNT+12+JRI2BISB5GVRE1'");
			var basic = AssertAwbExists<CusMAWB>("210-42011010");

			// Step 2 - TSR
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(basic.TSRs);

			// step 3 - CUSRES and FSN/CT
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+1/U00000001'BGM+TSR+21042011010'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CT:120:ZZZ'FTX+CAT+++OK TRANSFER SYD'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+1/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CT/10/21APR1546/00000001/OK TRANSFER SYD'UNT+3+363'");

			// Step 3b (not in script, but prudent to test
			AssertNoPrintQueued("C1", "210-42011010");
			AssertNoPrintQueued("GR", "210-42011010");
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false);  // no pieces received yet

			// Step 4 FRC for NPR=11
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+97:1104211559:201+++'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:11'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(11, (int)basic.NumberOfPiecesReceived);
			Assert(basic.Status1Date.IsEmpty);

			// Step 5 - forbid C1
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false); // Excess NPR
			AssertNoPrintQueued("C1", "210-42011010");

			// Step 6a - FRC to set NPR=10
			GBCustomsDataRegistry.Instance.CcsukAutoPrintC1WhenAllPiecesReceivedAndReleased.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(10, (int)basic.NumberOfPiecesReceived);
			Assert(!basic.Status1Date.IsEmpty);
			AssertNoPrintQueued("C1", "210-42011010");  // Rego set to not autoprint...
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, true);  //... so user can print at will

			// Step 6b - repeat 6a but with rego set to autoprint
			basic.NumberOfPiecesReceived = 9;  // C1 only autoprinted from FRC when Status1 being set and NPR is increasing (not decreasing). 
			basic.Factory.Save();
			GBCustomsDataRegistry.Instance.CcsukAutoPrintC1WhenAllPiecesReceivedAndReleased.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(10, (int)basic.NumberOfPiecesReceived);
			Assert(!basic.Status1Date.IsEmpty);
			AssertPrintQueued("C1", "210-42011010", new string[] { "OK TRANSFER SYD", "C1 reprint", "AGENT'S TRAVELLING COPY REMOVAL AUTHORITY", "CAR", "10KG", "PENCILS", "aod SYD", "T002120" }, new string[] { "PART RELEASE", "BAC" });  // Rego set to autoprint, so should exist in queue 
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false);  //... auto printed, user cannot
		}

		public void TestRemoval27_ExcessNprAfterClearance()
		{
			// Steps 1 & 2, NPX=2, NPR=2
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE1+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:2'QTY+48:2'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE1'");
			var basic = AssertAwbExists<CusMAWB>("210-42011010");

			// Step 3 - IAR
			CreateRemovalObjectAndMessage<InterAirportRemoval, CcsukTransmissionMessageFunction.CUSDEC.IAR>(basic.IARs);

			// step 4 - CUSRES and FSN/CW
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+1/U00000001'BGM+TSR+21042011010'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CW:120:ZZZ'FTX+CAT+++OK TRANSFER LGW'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+1/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CW/10/21APR1546/00000001/OK TRANSFER LGW'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			Assert(!basic.Status1Date.IsEmpty);
			AssertEquals("CW", basic.CustomsActionCode);

			// Step 5 - receive additional package
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE1+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:2'QTY+48:3'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE1'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(3, (int)basic.NumberOfPiecesReceived);
		}

		public void TestRemoval28_BasicAwbPartRelease()
		{
			// Step 1, NPR=0
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE1+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+97:1104211559:201+++'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'MEA+WT++KGM:10'UNT+12+JRI2BISB5GVRE1'");
			var basic = AssertAwbExists<CusMAWB>("210-42011010");

			// Step 2, TSR
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(basic.TSRs);

			// Step 3, CUSRES and FSN/CT	 
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+1/U00000001'BGM+TSR+21042011010'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CT:120:ZZZ'FTX+CAT+++OK TRANSFER SYD'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+1/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CT/10/21APR1546/00000001/OK TRANSFER SYD'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			Assert(basic.Status1Date.IsEmpty);
			AssertEquals("CT", basic.CustomsActionCode);

			// Step 4,out-of-sequence FSN/CA
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+1/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CA/10/21APR1546/00000001/REQUEST TRANSFER SYD'UNT+3+363'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			Assert(basic.Status1Date.IsEmpty);
			AssertEquals("CT", basic.CustomsActionCode);  // not CA, second message is not used to update job
			AssertEquals("OK TRANSFER SYD", basic.LatestCustomsActionText);
			AssertEquals(5, basic.Messages.Count);

			// Step 5, forbid C1
			AssertNoPrintQueued("C1", "210-42011010");
			AssertNoPrintQueued("GR", "210-42011010");
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false); // NPR=0

			// Step 6, receive 8 pieces
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE1+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:8'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE1'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertNoPrintQueued("C1", "210-42011010");  // C1 not automatically produced
			AssertNoPrintQueued("GR", "210-42011010");

			// Step 7, try to print C1 for more than 8 pieces
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(basic, 8); // OK to release 8 pieces

			// Step 8, print C1 for 8 pieces
			AssertEquals(8, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			AssertPrintQueued("C1", "210-42011010", new string[] { "PART RELEASE FOR 8 OF 10", "OK TRANSFER SYD", "C1 reprint", "AGENT'S TRAVELLING COPY REMOVAL AUTHORITY", "CAR", "10KG", "PENCILS", "aod SYD", "T002120" }, new string[] { "BAC", "LAST PART RELEASE", });  // Rego set to autoprint, so should exist in queue 

			SimulatePurgingOfPrintQueue();

			// step 9, receive all 10 pieces
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE1+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE1'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);

			// Step 10, try to print for more than 2 pieces
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false);  // Menu not visible because last part release automatically printed
			AssertPrintQueued("C1", "210-42011010", new string[] { "LAST PART RELEASE", "PART RELEASE FOR 2 OF 10", "OK TRANSFER SYD", "C1 reprint", "AGENT'S TRAVELLING COPY REMOVAL AUTHORITY", "CAR", "10KG", "PENCILS", "aod SYD", "T002120" }, new string[] { "BAC" });  // Rego set to autoprint, so should exist in queue 
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false); // all already released
			AssertEquals(10, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
		}

		public void TestRemoval29_SplitC1RemovalByParts()
		{
			// Steps 1, create unreceived AWB
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+97:1104211559:201+++50:1104211559:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			var basic = AssertAwbExists<CusMAWB>("210-42011010");

			// Step 2 - process FCS to make two splits. 
			ProcessReceivedMessage("UNH+JCS1BSJXKFYXQ0+CUSCAR:1:912:UN'BGM+:::FCS+21042011010'GIS+S2Y'TDT+20'LOC+11:LHR:145:3::CAX:129:ZZZ'GID+01'QTY+118:6'MEA+WT++KGM:6'GID+02'QTY+118:4'MEA+WT++KGM:4'UNT+12+JCS1BSJXKFYXQ0'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);  // split collection is cached
			Assert(basic.HasSplits);
			var split1 = basic.Splits["01"];
			var split2 = basic.Splits["02"];
			AssertEquals(6, (int)split1.NumberOfPiecesExpected);
			AssertEquals(4, (int)split2.NumberOfPiecesExpected);
			AssertEquals(0, (int)split1.NumberOfPiecesReceived);
			AssertEquals(0, (int)split2.NumberOfPiecesReceived);

			// Step 3, IAR for split 1
			CreateRemovalObjectAndMessage<InterAirportRemoval, CcsukTransmissionMessageFunction.CUSDEC.IAR>(basic.IARs, delegate(InterAirportRemoval ub)
			{ ub.SplitReferenceToWhichThisRemovalPertains = "01"; });

			// Step 4, CUSRES and FSN/CA for split 1
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+1/U00000001'BGM+IAR+21042011010+++ACD::01'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CA:120:ZZZ'FTX+CAT+++REQUEST TRANSFER LGW'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+1/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CA-01/10/21APR1546/00000001/REQUEST TRANSFER LGW'UNT+3+363'");
			split1 = new BusinessObjectFactory().Load<SplitBasic>(split1.PK);
			AssertEquals("CA", split1.CustomsActionCode);
			AssertPrintQueued("GR", "210-42011010/01", new string[] { "REQUEST TRANSFER LGW", "GR", "ADVICE OF SELECTED REMOVAL REQUEST" }, Array.Empty<string>());
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false);
			AssertWhetherReleaseDocumentPossibleFromMenu(split1, false);

			SimulatePurgingOfPrintQueue();

			// Step 5, FSN/CW for split 1
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+1/U00000001'BGM+TSR+21042011010+++ACD::01'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CW:120:ZZZ'FTX+CAT+++OK TRANSFER LGW'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+1/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CW-01/10/21APR1546/00000001/OK TRANSFER LGW'UNT+3+363'");
			split1 = new BusinessObjectFactory().Load<SplitBasic>(split1.PK);
			AssertEquals("CW", split1.CustomsActionCode);
			// No prints... NPR=0
			AssertNoPrintQueued("C1", "210-42011010/01");
			AssertWhetherReleaseDocumentPossibleFromMenu(basic, false);
			AssertWhetherReleaseDocumentPossibleFromMenu(split1, false);

			// Step 6, receive half pieces on both splits
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++ACD::01'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:6'QTY+48:4'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++ACD::02'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:4'QTY+48:2'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			split1 = new BusinessObjectFactory().Load<SplitBasic>(split1.PK);
			split2 = new BusinessObjectFactory().Load<SplitBasic>(split2.PK);

			// Steps 7 and 8a
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(split1, 4);
			split1 = new BusinessObjectFactory().Load<SplitBasic>(split1.PK);
			AssertEquals(4, split1.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			AssertPrintQueued("C1", "210-42011010/01", new string[] { "OK TRANSFER LGW", "C1", "PART RELEASE FOR 4 OF 6" }, new string[] { "LAST PART RELEASE" });

			SimulatePurgingOfPrintQueue();

			// Step 8b, receive 2 more packages (one on each)
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++ACD::01'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:6'QTY+48:5'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++ACD::02'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:4'QTY+48:3'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			split1 = new BusinessObjectFactory().Load<SplitBasic>(split1.PK);

			// Step 8c, try to release 5 for split 1 (not allowed - 4 already released)
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(split1, 1, false);

			// Step 9, produce part for 1 piece for split 01
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(split1, 1, true);
			AssertPrintQueued("C1", "210-42011010/01", new string[] { "OK TRANSFER LGW", "C1", "PART RELEASE FOR 1 OF 6" }, new string[] { "LAST PART RELEASE" });

			SimulatePurgingOfPrintQueue();

			//Step 10, receive FRC for PARENT record, setting status 1 on all splits
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			split1 = new BusinessObjectFactory().Load<SplitBasic>(split1.PK);
			split2 = new BusinessObjectFactory().Load<SplitBasic>(split2.PK);

			// Step 11, see C1 options, then see C1 for last part of split 1 and all of split 2
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(split1, 1, true);
			AssertPrintQueued("C1", "210-42011010/01", new string[] { "OK TRANSFER LGW", "C1", "PART RELEASE FOR 1 OF 6", "LAST PART RELEASE" }, Array.Empty<string>());

			// Not in script - auto print C1 for split 2
			CreateRemovalObjectAndMessage<InterAirportRemoval, CcsukTransmissionMessageFunction.CUSDEC.IAR>(basic.IARs, delegate(InterAirportRemoval ub)
			{ ub.SplitReferenceToWhichThisRemovalPertains = "02"; });
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+2/U00000002'BGM+IAR+21042011010+++ACD::02'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CW:120:ZZZ'FTX+CAT+++OK TRANSFER LGW'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+2/U00000002'FTX+CIM+++FSN:LHRCAX:210-42011010:CSN/CW-02/10/21APR1546/00000001/OK TRANSFER LGW'UNT+3+363'");
			split2 = new BusinessObjectFactory().Load<SplitBasic>(split2.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(split2, false);//  autoreleased already
			AssertPrintQueued("C1", "210-42011010/02", new string[] { "OK TRANSFER LGW", "C1" }, new string[] { "PART RELEASE" });
		}

		[SnailTest]
		public void TestRemoval30_31_32_HawbAndSplitRelease()
		{
			TestRemoval30_HawbPartRelease();
			TestRemoval31_HawbC1Removal();
			TestRemoval32_HawbSplitRelease();
		}

		void TestRemoval30_HawbPartRelease()
		{
			// Step 1, create consol and 7 houses
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+++HWB:M'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++CONSOL'QTY+118:70'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+++HWB:HAWB0001'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+++HWB:HAWB0002'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS2'QTY+118:10'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+++HWB:HAWB0003'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS3'QTY+118:10'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+++HWB:HAWB0004'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS4'QTY+118:10'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+++HWB:HAWB0005'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS5'QTY+118:10'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+++HWB:HAWB0006'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS6'QTY+118:10'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRI+21042011010+++HWB:HAWB0007'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS7'QTY+118:10'QTY+48:0'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");

			// Step 2, TSR for hawb 1
			var hawb1 = AssertAwbExists<CusHAWB>("210-42011010-HAWB0001");
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(hawb1.TSRs);

			// Step 3, CUSRES and FSN/CT	 
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+1/U00000001'BGM+TSR+21042011010+++HWB:HAWB0001'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CT:120:ZZZ'FTX+CAT+++OK TRANSFER SYD'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+1/U00000001'FTX+CIM+++FSN:LHRCAX:210-42011010-HAWB0001:CSN/CT/10/21APR1546/00000001/OK TRANSFER SYD'UNT+3+363'");

			// Step 4, forbid C1
			hawb1 = new BusinessObjectFactory().Load<CusHAWB>(hawb1.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(hawb1, false);
			AssertNoPrintQueued("C1", "210-42011010-HAWB0001");

			// Step 5, FRC for hawb 1 setting NPR = 11 (excess)
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0001'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:11'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");

			// Steps 6 & 7, forbid C1 when excess NPR
			hawb1 = new BusinessObjectFactory().Load<CusHAWB>(hawb1.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(hawb1, false);
			AssertNoPrintQueued("C1", "210-42011010-HAWB0001");

			// Step 8, FRC sets NPR=8.
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0001'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:8'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");

			// Steps 9 & 10, try to produce C1 for more than 8, then produce for 8
			hawb1 = new BusinessObjectFactory().Load<CusHAWB>(hawb1.PK);
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(hawb1, 8, true);
			AssertPrintQueued("C1", "210-42011010-HAWB0001", new string[] { "C1", "OK TRANSFER SYD", "PART RELEASE FOR 8 OF 10" }, new string[] { "LAST PART" });

			SimulatePurgingOfPrintQueue();

			// Step 11, FRC to set NPR 	
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0001'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");

			// Steps 11 & 12, cannot produce C1 for 8 or 2 pieces, it's been autoprinted
			hawb1 = new BusinessObjectFactory().Load<CusHAWB>(hawb1.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(hawb1, false);
			AssertPrintQueued("C1", "210-42011010-HAWB0001", new string[] { "C1", "OK TRANSFER SYD", "PART RELEASE FOR 2 OF 10", "LAST PART" }, Array.Empty<string>());
		}

		void TestRemoval31_HawbC1Removal()
		{
			// Step 1, set NPR in hawbs 2-6
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0002'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0003'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0004'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0005'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0006'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");

			var hawb2 = AssertAwbExists<CusHAWB>("210-42011010-HAWB0002");
			var hawb3 = AssertAwbExists<CusHAWB>("210-42011010-HAWB0003");
			var hawb4 = AssertAwbExists<CusHAWB>("210-42011010-HAWB0004");
			var hawb5 = AssertAwbExists<CusHAWB>("210-42011010-HAWB0005");
			var hawb6 = AssertAwbExists<CusHAWB>("210-42011010-HAWB0006");

			// Step 2a, ISR for hawb 2.  Omitted for agent script

			// Step 2b, TSR for hawb 3
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(hawb2.TSRs);

			// Step 2c, IAR for hawb 4
			CreateRemovalObjectAndMessage<InterAirportRemoval, CcsukTransmissionMessageFunction.CUSDEC.IAR>(hawb3.IARs);

			// Step 3a - FSN/CB for hawb 2
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRCAX:210-42011010-HAWB0002:CSN/CB/10/21APR1546/POOP/OK TRANSFER BAC'UNT+3+363'");
			hawb2 = new BusinessObjectFactory().Load<CusHAWB>(hawb2.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(hawb2, false);
			AssertNoPrintQueued("C1", "210-42011010-HAWB0002");

			// Step 3b - CUSRES and FSN/CT for hawb 3
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+1/U00000001'BGM+TSR+21042011010+++HWB:HAWB0003'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CT:120:ZZZ'FTX+CAT+++OK TRANSFER SYD'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRCAX:210-42011010-HAWB0003:CSN/CT/10/21APR1546/00000001/OK TRANSFER SYD'UNT+3+363'");
			hawb3 = new BusinessObjectFactory().Load<CusHAWB>(hawb3.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(hawb3, false); // autoprinted
			AssertPrintQueued("C1", "210-42011010-HAWB0003", new string[] { "C1", "OK TRANSFER SYD" }, new string[] { "PART RELEASE" });

			// Step 3c - CUSRES and FSN/CW for hawb 4
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+2/U00000002'BGM+TSR+21042011010+++HWB:HAWB0004'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CW:120:ZZZ'FTX+CAT+++OK TRANSFER LGW'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRCAX:210-42011010-HAWB0004:CSN/CW/10/21APR1546/00000002/OK TRANSFER LGW'UNT+3+363'");
			hawb4 = new BusinessObjectFactory().Load<CusHAWB>(hawb4.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(hawb4, false); // autoprinted
			AssertPrintQueued("C1", "210-42011010-HAWB0004", new string[] { "C1", "OK TRANSFER LGW" }, new string[] { "PART RELEASE" });

			// Step 3d - CC for hawb 5
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRCAX:210-42011010-HAWB0005:CSN/CC/10/21APR1546/WHATEVER/CLEARED'UNT+3+363'");
			hawb5 = new BusinessObjectFactory().Load<CusHAWB>(hawb5.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(hawb5, false);
			AssertNoPrintQueued("C1", "210-42011010-HAWB0005");

			// Step 3e - FSN/CS for hawb 6
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA'FTX+CIM+++FSN:LHRCAX:210-42011010-HAWB0006:CSN/CS/10/21APR1546/WHATEVER/SEIZED'UNT+3+363'");
			hawb6 = new BusinessObjectFactory().Load<CusHAWB>(hawb6.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(hawb6, false);
			AssertNoPrintQueued("C1", "210-42011010-HAWB0006");
		}

		void TestRemoval32_HawbSplitRelease()
		{
			// Step 1, split hawb 7 
			ProcessReceivedMessage("UNH+JCS1BSJXKFYXQ0+CUSCAR:1:912:UN'BGM+:::FCS+21042011010+++HWB:HAWB0007'GIS+S2Y'TDT+20'LOC+11:LHR:145:3::CAX:129:ZZZ'GID+01'QTY+118:6'MEA+WT++KGM:6'GID+02'QTY+118:4'MEA+WT++KGM:4'UNT+12+JCS1BSJXKFYXQ0'");
			var hawb7 = AssertAwbExists<CusHAWB>("210-42011010-HAWB0007");
			Assert(hawb7.HasSplits);
			var split1 = hawb7.Splits["01"];
			var split2 = hawb7.Splits["02"];

			// Step 2, FRC for HAWB7 to set NPR=8
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0007'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:8'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");

			// Step 3, TSRs for the splits
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(hawb7.TSRs, delegate(TranshipmentRemoval ub)
			{ ub.SplitReferenceToWhichThisRemovalPertains = "01"; });
			CreateRemovalObjectAndMessage<TranshipmentRemoval, CcsukTransmissionMessageFunction.CUSDEC.TSR>(hawb7.TSRs, delegate(TranshipmentRemoval ub)
			{ ub.SplitReferenceToWhichThisRemovalPertains = "02"; });

			//Step 4, CUSRES and FSN/CT for the splits
			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+4/U00000004'BGM+TSR+21042011010+++HWB:HAWB0007:01'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CT:120:ZZZ'FTX+CAT+++OK TRANSFER SYD'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+4/U00000004'FTX+CIM+++FSN:LHRCAX:210-42011010-HAWB0007:CSN/CT-01/10/21APR1546/00000004/OK TRANSFER SYD'UNT+3+363'");

			ProcessReceivedMessage("UNH+11695+CUSRES:2:912:UN:109606+5/U00000005'BGM+TSR+21042011010+++HWB:HAWB0007:02'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:JFK:145:3'PAC+6'RFF+TN:T002120'GIS+CT:120:ZZZ'FTX+CAT+++OK TRANSFER SYD'FTX+AAA+++FOUR'UNT+10+11695");
			ProcessReceivedMessage("UNH+363+CIMFSN:0:0:Z1:IATA+5/U00000005'FTX+CIM+++FSN:LHRCAX:210-42011010-HAWB0007:CSN/CT-02/10/21APR1546/00000005/OK TRANSFER SYD'UNT+3+363'");

			// Step 5, forbid produce C1
			split1 = new BusinessObjectFactory().Load<SplitHouse>(split1.PK);
			split2 = new BusinessObjectFactory().Load<SplitHouse>(split2.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(split1, false);
			AssertWhetherReleaseDocumentPossibleFromMenu(split2, false);
			AssertNoPrintQueued("C1", "210-42011010-HAWB0007/01");
			AssertNoPrintQueued("C1", "210-42011010-HAWB0007/02");

			// Step 6, FRCs to allocate some NPR
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0007:01'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:6'QTY+48:4'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0007:02'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:4'QTY+48:4'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");

			// Step 7, C1 for split 2
			split2 = new BusinessObjectFactory().Load<SplitHouse>(split2.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(split2, false);
			AssertPrintQueued("C1", "210-42011010-HAWB0007/02", new string[] { "C1", "OK TRANSFER SYD", "HAWB0007/02" }, new string[] { "PART RELEASE" });

			// Step 8, forbid full release split 1
			AssertNoPrintQueued("C1", "210-42011010-HAWB0007/01");  // no autoprint			
			split1 = new BusinessObjectFactory().Load<SplitHouse>(split1.PK);
			AssertWhetherReleaseDocumentPossibleFromMenu(split1, true);  // some pieces received
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(split1, 4, false);

			// Step 9 - part release for split 1
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(split1, 4, true);
			AssertPrintQueued("C1", "210-42011010-HAWB0007/01", new string[] { "C1", "OK TRANSFER SYD", "HAWB0007/01", "PART RELEASE FOR 4 OF 6" }, new string[] { "LAST PART RELEASE" });

			SimulatePurgingOfPrintQueue();

			// Step 10 - FRC to set St1 on HAWB, does not print C1s as update is to superior
			ProcessReceivedMessage("UNH+JRI2BISB5GVRE0+CUSCAR:1:912:UN'BGM+:::FRC+21042011010+++HWB:HAWB0007'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:<<ARRIVALDATE>>:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++PENCILS1'QTY+118:10'QTY+48:10'MEA+WT++KGM:10'UNT+13+JRI2BISB5GVRE0'");
			AssertNoPrintQueued("C1", "210-42011010-HAWB0007");
			AssertNoPrintQueued("C1", "210-42011010-HAWB0007/01");
			AssertNoPrintQueued("C1", "210-42011010-HAWB0007/02");

			// Steps 11 & 12, forbid full release of split 1, all last part release
			split1 = new BusinessObjectFactory().Load<SplitHouse>(split1.PK);
			AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(split1, 2, true);
			AssertPrintQueued("C1", "210-42011010-HAWB0007/01", new string[] { "C1", "OK TRANSFER SYD", "HAWB0007/01", "PART RELEASE FOR 2 OF 6", "LAST PART RELEASE" }, Array.Empty<string>());
		}

		void AssertTryToReleaseTooManyPiecesThenReleaseCorrectNumber(ICcsukCusAwb awb, int correctNumberToRelease, bool alsoPressButtonToReleaseCorrectNumber = true)
		{
			var c1Menu = AssertWhetherReleaseDocumentPossibleFromMenu(awb, true);

			// This simulates c1Menu.PerformClick()
			var controller = new NonPersistentC1ReleaseOrchestrator(awb);
			using (var userControl = new C1ReleaseUserControl(controller, null))
			{
				controller.C1ReleaseHelper.NumberOfPieces = correctNumberToRelease;
				AssertEquals(correctNumberToRelease, controller.C1ReleaseHelper.NumberOfPieces);
				controller.C1ReleaseHelper.NumberOfPieces = correctNumberToRelease + 1;
				AssertEquals(correctNumberToRelease, controller.C1ReleaseHelper.NumberOfPieces);
				if (alsoPressButtonToReleaseCorrectNumber)
				{
					userControl.ReleaseNowButton_Click(null, null);
					awb.Factory.Save();
				}
			}
		}

		protected override string ExpectedReleaseDocumentMenuTitle
		{
			get { return CcsukMenu.C1MenuTitle; }
		}
		protected override string RecipientPima
		{
			get { return "CUKFFW98000CAR"; }
		}
	}
}

