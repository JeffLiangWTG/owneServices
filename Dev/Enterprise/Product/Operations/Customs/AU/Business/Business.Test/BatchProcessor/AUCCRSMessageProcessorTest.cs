using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCCRSMessageProcessorTest : TestCaseWithFactory
	{
		public void TestGetProcessableMessagesOrderMatchIndex()
		{
			var processor = new AUCCRSMessageProcessorForTest();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();
			AssertEquals("EM_SystemCreateTimeUtc, EM_MessageNum", processor.ProcessableMessagesOrder);
			AssertEquals("EM_SystemCreateTimeUtc, EM_MessageNum", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum", hint.IndexName);
		}

		public void TestProcessCARSTMessageInOtherBranch()
		{
			const string AIRCRMessage = "UNH+8+CUSCAR:D:99B:UN'BGM+933:::AIRCR+A26594520/PRD1:1+9'RFF+PQ:PO'RFF+HWB:1Z840R190441790357'RFF+MWB:73847378881'NAD+CN++MICHAEL NEWTON::569 BAYSWATER ROAD  MOUNT LOUISA  4:814 AU'NAD+CZ++GDS VIETNAM LTD::GREYSTONE DATA SYTERM CO.,LTD LOT 6:2A, ROAB B, LINH TRUNG II EPZ HO CH:I MINH  700000 VN'NAD+VW+83003926181::95'TDT+20+773++6+VN::3'LOC+8+AUSYD::6'LOC+76+VNSGN::6'LOC+12+AUSYD::6'LOC+91+VNSGN::6'DTM+178:20200314:102'CNI+1'RFF+UCN:A26594520'MOA+44:168.15:AUD'GIS+SAC:109:95'GID+1'PAC+1'FTX+AAA+++3 1/2 - HARD DISC DRIVE - INTL 3.5'MEA+AAE+G+KG:1.00'UNT+23+8'";
			const string CARSTMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+3G89 DC8B I6A0:0001+8'DTM+9:20200306105538385436:ZZZ'DTM+132:20200305:102'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+0207++6+SQ::3'LOC+12+AUMEL::6'LOC+4+9521E::95'NAD+MR+FFJ996W::95'NAD+UD+89001646420::95'RFF+ABO:A26536447/PRD1::0001'RFF+MWB:61877774830'RFF+HWB:1Z30A59W8695961034'DOC+1'PAC+0000002'UNT+17+000001'";

			var melbourneBranch = Factory.New<GlbBranch>();
			melbourneBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			melbourneBranch.GB_Code = "TML";
			melbourneBranch.GB_RL_NKHomePort = "AUMEL";
			var perthBranch = Factory.New<GlbBranch>();
			perthBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			perthBranch.GB_Code = "TPR";
			perthBranch.GB_RL_NKHomePort = "AUPER";
			Factory.Save();

			using (Environment.DisposableEnvironment.ForBranch(melbourneBranch.PK.ToGuid()))
			{
				var mawb = Factory.New<CusMAWB>();
				mawb.CM_MAWB = "61877774830";
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_MessageReference = "A26536447";
				hawb.CS_ClearanceDate = ZDateTime.Empty;

				var outgoingMessage = hawb.Messages.AddNew(typeof(CMRAIRCRMessage));
				outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
				outgoingMessage.EM_MessageNum = "1";
				outgoingMessage.EM_MessageText = AIRCRMessage;
				outgoingMessage.EM_Status = EDIMessage.Status.Sent;
				Factory.Save();
				AssertEquals("Outgoing Message Branch is Current Branch", melbourneBranch.PK, outgoingMessage.EM_GB);

				EDIMessage processedMessage = null;
				var melbourneBranchTime = ZDateTime.Now;
				using (Environment.DisposableEnvironment.ForBranch(perthBranch.PK.ToGuid()))
				{
					AssertLessThan("No longer in East Coast timezone", ZDateTime.Now, melbourneBranchTime);

					var incomingMessage = Factory.New<CMRCARSTMessage>();
					incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					incomingMessage.EM_MessageNum = "2";
					incomingMessage.EM_MessageText = CARSTMessage;
					incomingMessage.EM_Status = EDIMessage.Status.Queued;
					Factory.Save();
					AssertEquals("Incoming Message Branch is Current Branch", perthBranch.PK, incomingMessage.EM_GB);

					var processor = new AUCCRSMessageProcessor();
					processor.ExecuteBatch();

					processedMessage = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);
				}

				AssertEquals("EM_Status", EDIMessage.Status.Received, processedMessage.EM_Status);
				AssertEquals("Message linked to HAWB", hawb.PK, processedMessage.EM_LinkUniqueID);
				AssertEquals("Branch updated to branch of Original Message", melbourneBranch.PK, processedMessage.EM_GB);

				hawb.Reload();
				var log = hawb.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.StatusChange);
				AssertEquals("CusHAWB Customs Status - CLR", log.SL_Reference);
				AssertGreaterThanOrEqualTo("log should have Melbourne timestamp even though logged in Perth timezone", log.SL_EventTime, melbourneBranchTime);

				log = hawb.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.CustomsEntryStatus);
				AssertEquals("CLR", log.SL_Reference);
				AssertGreaterThanOrEqualTo("log should have Melbourne timestamp even though logged in Perth timezone", log.SL_EventTime, melbourneBranchTime);

				AssertGreaterThanOrEqualTo(hawb.CS_ClearanceDate, melbourneBranchTime);
			}
		}

		public void TestProcessCARSTMessageUpdatesCustomsStatus_Declaration_CS00847261()
		{
			var testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testJobDeclaration.JE_DeclarationReference = "B00168965";
			testJobDeclaration.JE_VoyageFlightNo = "435P";
			testJobDeclaration.JE_VesselName = "MAERSK OCEANIA";
			testJobDeclaration.JE_MasterBill = "WI00132660T2";
			testJobDeclaration.JE_HouseBill = "WI00132660T2HB2";

			var invHeader = testJobDeclaration.Invoices.AddNew();
			var invLine = invHeader.JobComInvoiceLines.AddNew();

			var masterBill = testJobDeclaration.PrimaryMasterBill;
			var houseBill = testJobDeclaration.PrimaryHouseBill;

			var packGroup1 = testJobDeclaration.PackingGroups[0];
			packGroup1.CR_CU_HouseBill = houseBill.PK;
			var pack1 = packGroup1.Packages.AddNew();

			var container1 = testJobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "TESU7676761";
			packGroup1.CR_CO_Container = container1.PK;

			testJobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var cusEntryHeader1 = testJobDeclaration.CustomsEntryHeaders[0];
			cusEntryHeader1.CH_BGMReference = "B00168965/1";

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "TESTCARST00001";
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+40H3 7IE8 JCAA:0001+8'DTM+9:20200416132859099850:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'FTX+AHN+++LCL UNDERBOND SATISFIED:NO'FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:YES'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:YES'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:YES'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:YES'FTX+AHN+++IMPORT DECLARATION PAID:NO'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+435P++11++++9122447::11'LOC+12+AUSYD::6'LOC+4+9122P::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:B00168965/CMT1::0001'RFF+ABT:AAAFHA6GH'RFF+MB:WI00132660T2'RFF+BH:WI00132660T2HB2'RFF+AAQ:TESU7676761'DOC+1'PAC+++LCL:67:95'UNT+35+000001'";
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var processor = new AUCCRSMessageProcessor();
			processor.ExecuteBatch();

			var otherFactory = NewFactory();
			var processedMessage = otherFactory.Load<EDIMessage>(message.PK);
			AssertEquals("EM_Status", EDIMessage.Status.Received, processedMessage.EM_Status);
			AssertEquals("Message should be linked to Entry", cusEntryHeader1.PK, processedMessage.EM_LinkUniqueID);

			var packGroupInOtherFactory = otherFactory.Load<PackingGroup>(packGroup1.PK);
			AssertEquals("PackingGroup Cargo Status updated to HELD", CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, packGroupInOtherFactory.CR_CargoStatus);
			AssertEquals("Copy of Message should be linked to PackingGroup", 1, packGroupInOtherFactory.Messages.Count);

			var decInOtherFactory = otherFactory.Load<JobDeclaration>(testJobDeclaration.PK);
			AssertEquals("Declaration Cargo Status updated to HELD", CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, decInOtherFactory.JE_ConsolidatedCargoStatus);
		}

		public void TestProcessCARSTMessageUpdatesCustomsStatus_Declaration_Sea_NoHouse_ChangedOB()
		{
			var testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testJobDeclaration.JE_DeclarationReference = "B00168965";
			testJobDeclaration.JE_VoyageFlightNo = "435P";
			testJobDeclaration.JE_VesselName = "MAERSK OCEANIA";
			testJobDeclaration.JE_MasterBill = "142001863543";
			testJobDeclaration.JE_HouseBill = "";
			testJobDeclaration.JE_TotalNoOfPacks = 1;

			var invHeader = testJobDeclaration.Invoices.AddNew();
			var invLine = invHeader.JobComInvoiceLines.AddNew();

			var masterBill = testJobDeclaration.PrimaryMasterBill;
			var linkBill = testJobDeclaration.Bills.AddNew();
			linkBill.CU_CU_ParentBill = masterBill.PK;
			linkBill.CU_BillType = "HB";
			var houseBill = testJobDeclaration.PrimaryHouseBill;

			var container1 = testJobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "TESU7676761";

			var packGroup1 = testJobDeclaration.PackingGroups[0];
			packGroup1.CR_CU_HouseBill = houseBill.PK;
			packGroup1.CR_CO_Container = container1.PK;
			var pack1 = packGroup1.Packages.AddNew();

			testJobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var cusEntryHeader1 = testJobDeclaration.CustomsEntryHeaders[0];
			cusEntryHeader1.CH_BGMReference = "B00168965/1";

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "TESTCARST00001";
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+40H3 7IE8 JCAA:0001+8'DTM+9:20200416132859099850:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+435P++11++++9122447::11'LOC+12+AUSYD::6'LOC+4+9122P::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:B00168965/1/CMT1::0001'RFF+ABT:AAAFHA6GH'RFF+MB:EGLV142001863543'RFF+AAQ:TESU7676761'DOC+1'PAC+++LCL:67:95'UNT+34+000001'";
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var processor = new AUCCRSMessageProcessor();
			processor.ExecuteBatch();

			var otherFactory = NewFactory();
			var processedMessage = otherFactory.Load<EDIMessage>(message.PK);
			AssertEquals("EM_Status", EDIMessage.Status.Received, processedMessage.EM_Status);
			AssertEquals("Message should be linked to Entry", cusEntryHeader1.PK, processedMessage.EM_LinkUniqueID);

			var packGroupInOtherFactory = otherFactory.Load<PackingGroup>(packGroup1.PK);
			AssertEquals("PackingGroup Cargo Status updated to CLEAR", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, packGroupInOtherFactory.CR_CargoStatus);
			AssertEquals("Copy of Message should be linked to PackingGroup", 1, packGroupInOtherFactory.Messages.Count);

			var decInOtherFactory = otherFactory.Load<JobDeclaration>(testJobDeclaration.PK);
			AssertEquals("Declaration Cargo Status updated to CLEAR", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, decInOtherFactory.JE_ConsolidatedCargoStatus);
		}

		public void TestProcessCARSTMessageUpdatesCustomsStatus_OceanBill()
		{
			var carstMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+1DB3 460H B06:1+8'DTM+9:20160209143517181888:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'FTX+AHN+++LCL UNDERBOND SATISFIED:YES'FTX+AHN+++CARGO NOT A CONSOLIDATION:N/A'FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+WC0902++11++++9044748::11'LOC+12+AUSYD::6'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:S00048545/CMT2::2'RFF+MB:3438'RFF+BH:S00048545'RFF+AAQ:ANLU7766112'DOC+1'PAC+++LCL:67:95'UNT+35+000001'";

			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "WC0902";
			oceanBill.CB_LloydsIMO = "9044748";

			var container = oceanBill.Containers.AddNew();
			container.FillWithValidTestData();

			var house = Factory.NewWithValidTestData<CusSCAHouse>();
			house.CA_CB = oceanBill.PK;
			house.CA_HouseBill = "S00048545";
			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			var pivot = house.Pivot.AddNew();
			pivot.FillWithValidTestData();
			pivot.CV_CN = container.PK;
			pivot.CN_ContainerNumber = "ANLU7766112";

			var header = Factory.New<CusOutturnHeader>();
			header.C6_VesselName = "AALSMEERGRACHT";
			header.C6_LloydsIMO = "9044748";
			header.C6_OA_OutturningPremise = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			header.C6_OutturningPremiseID = "9914N";
			header.C6_VoyageNum = "WC0902";
			header.C6_ResponsiblePartyID = "41065894724";

			var outturn = header.Outturns.AddNew();
			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			outturn.C5_CargoUnpackDate = ZDateTime.Now;
			outturn.C5_CargoType = "LCL";
			outturn.C5_MasterBill = "3418";
			outturn.C5_HouseBill = "S00048545";
			outturn.C5_ContainerNumber = "ANLU7766112";
			outturn.C5_CustomsStatus = ZString.Empty;

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "TESTCARST00000";
			message.EM_MessageText = carstMessage;
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var processor = new AUCCRSMessageProcessor();
			processor.ExecuteBatch();

			var otherFactory = NewFactory();
			var processedMessage = otherFactory.Load<EDIMessage>(message.PK);
			AssertEquals("EM_Status", EDIMessage.Status.Received, processedMessage.EM_Status);
			AssertEquals("Message should be linked to Pivot", pivot.PK, processedMessage.EM_LinkUniqueID);

			var outturnInNewFactory = otherFactory.Load<DepotCusOutturn>(outturn.PK);
			var customsStatus = outturnInNewFactory.C5_CustomsStatus;
			AssertEquals("Customs Status on Outturn should be calculated", "SUB", customsStatus);

			outturnInNewFactory.StatusCalculator.DeriveStatusNow();
			AssertEquals("Customs Status should not change when recalculated", customsStatus, outturnInNewFactory.C5_CustomsStatus);
		}

		public void TestProcessCARSTMessageUpdatesCustomsStatus_Air_WI00353105()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SEAAUSSYD3";

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SEALORAKL";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "SAC";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_RS_NKServiceLevel = "PTP";
			declaration.JE_DeclarationReference = "B00080049";
			declaration.JE_VoyageFlightNo = "NZ103";
			declaration.JE_VesselName = "";
			declaration.JE_MasterBill = "08657962155";
			declaration.JE_HouseBill = "";
			declaration.JE_TotalNoOfPacks = 1;

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "08657962155";
			invHeader.JZ_InvoiceAmount = 10.00m;
			invHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invHeader.JZ_Weight = 100.000m;
			invHeader.JZ_WeightUQ = "KG";
			invHeader.JZ_NoOfPacks = 0m;
			invHeader.JZ_OH_Supplier = supplier.PK;
			Factory.Save();

			AssertNotEquals(ZGuid.Empty, invHeader.JZ_JZ_GroupInvoiceFK);

			var masterBill = declaration.PrimaryMasterBill;
			var houseBill = declaration.PrimaryHouseBill;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var cusEntryHeader1 = declaration.CustomsEntryHeaders[0];
			cusEntryHeader1.CH_BGMReference = "B00080049/1";

			AssertEquals(1, declaration.PackingGroups.Count);

			var sacOutMessage = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::SAC+B00080049/1/MEL1:1+9'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+90+:::N3155'DTM+178:20200903:102'GIS+Y:153:95'GIS+POR:109:95'FTX+DEL+++SEALORD AUSTRALIA PTY LTD'FTX+AAA+++FROZEN FISH KEEP FROZEN'FTX+ACD+++0017:N:THIS IS THE REFERAL REASON'FTX+ACD+++0018:N:THIS IS THE REFERAL REASON'FTX+ACD+++0019:Y:THIS IS THE REFERAL REASON'RFF+ABQ:80049'RFF+ADU:B00080049/1'RFF+MWB:08657962155'RFF+AMG:0016'TDT+20++A'NAD+AT+19001670864::95'NAD+WP+001::95'NAD+VT+AE79WF::95'NAD+DP++PYRMONT++19 HARRIS STREET++:::NSW+2009+AU'UNS+D'UNS+S'UNT+25+1'";
			var outMessage = Factory.New<CMRSACRMessage>();
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_MessageNum = "OUT001";
			outMessage.EM_MessageText = sacOutMessage;
			outMessage.EM_Status = EDIMessage.Status.Sent;

			// CCL
			var carstCondClearMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+1BIG B2A8 I8B0:0001+8'DTM+9:20200903224148970855:ZZZ'DTM+132:20200903:102'FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:Pending AQIS Action (Quarantine)'FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:Inspection - AIR Freight Inspection'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+0103++6+NZ::3'LOC+12+AUSYD::6'LOC+4+8553P::95'NAD+MR+FGM377W::95'NAD+UD+23354857893::95'RFF+ABO:B00080049/1/MEL1::0001'RFF+ABT:AEHKL77F9'RFF+MWB:08657962155'DOC+1'PAC+0000001'UNT+22+000001'";
			var carstCCLMessage = Factory.New<CMRCARSTMessage>();
			carstCCLMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			carstCCLMessage.EM_MessageNum = "RESP001";
			carstCCLMessage.EM_MessageText = carstCondClearMessage;
			carstCCLMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var processor1 = new AUCCRSMessageProcessor();
			processor1.ExecuteBatch();

			AssertDecStatus(carstCCLMessage, cusEntryHeader1, CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);

			// CLR
			var carstClearMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+1HH1 5H6C DIG0:0001+8'DTM+9:20200907132901716889:ZZZ'DTM+132:20200903:102'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+0103++6+NZ::3'LOC+12+AUSYD::6'LOC+4+8553P::95'NAD+MR+FGM377W::95'NAD+UD+23354857893::95'RFF+ABO:B00080049/1/MEL1::0001'RFF+ABT:AEHKL77F9'RFF+MWB:08657962155'DOC+1'PAC+0000001'UNT+17+000001'";
			var carstCLRMessage = Factory.New<CMRCARSTMessage>();
			carstCLRMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			carstCLRMessage.EM_MessageNum = "RESP002";
			carstCLRMessage.EM_MessageText = carstClearMessage;
			carstCLRMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			// DSA
			var dsaMessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+2622 E167 18G0:1+11'DTM+9:20200907133400000000:ZZZ'TDT+20++A'NAD+MR+FGM377W::95'NAD+VT+AE79WF::95'NAD+IM++SEALORD AUSTRALIA PTY. LIMITED'RFF+ABO:B00080049/1/MEL1::1'RFF+ACW:SAC'RFF+ABT:AEHKL77F9::1'RFF+ABQ:80049'RFF+ADU:B00080049/1'RFF+AMI:FINALISED'DOC+S+1'DTM+192:20200903:102'DTM+192:1139:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'UNT+20+000001'";
			var dsaMessage = Factory.New<CMRDSAMessage>();
			dsaMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			dsaMessage.EM_MessageNum = "RESP003";
			dsaMessage.EM_MessageText = dsaMessageText;
			dsaMessage.EM_Status = EDIMessage.Status.Received;
			dsaMessage.EM_LinkedObject = cusEntryHeader1;
			Factory.Save();

			var processor2 = new AUCCRSMessageProcessor();
			processor2.ExecuteBatch();

			var decInOtherFactory = AssertDecStatus(carstCLRMessage, cusEntryHeader1, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);

			var expectedDetails = @"Status for Packing (Transport) Line: 1
***CONSOLIDATED CARGO STATUS: ***CLEAR***
ACSDec/ACSCR/AQISDec/AQISCR: Y/Y/Y/Y
MB:08657962155
Screening Period Expiry: 03Sep2020 11:39 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: CLEAR
";
			AssertMultilineASCIIEquals(expectedDetails, decInOtherFactory.CombinedConsolidatedCargoStatusDetails);
		}

		JobDeclaration AssertDecStatus(EDIMessage message, CusEntryHeader entryHeader, ZString status)
		{
			var otherFactory = NewFactory();
			var processedMessage = otherFactory.Load<EDIMessage>(message.PK);
			AssertEquals("EM_Status", EDIMessage.Status.Received, processedMessage.EM_Status);
			AssertEquals("Message should be linked to Entry", entryHeader.PK, processedMessage.EM_LinkUniqueID);

			var decInOtherFactory = otherFactory.Load<JobDeclaration>(entryHeader.CH_JE);
			AssertEquals("Declaration Cargo Status updated", status, decInOtherFactory.JE_ConsolidatedCargoStatus);

			return decInOtherFactory;
		}

		sealed class AUCCRSMessageProcessorForTest : AUCCRSMessageProcessor
		{
			public string ProcessableMessagesOrder => GetProcessableMessagesOrder();

			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
		}
	}
}
