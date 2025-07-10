using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class CMRSeaDepotMessageProcessorTestCase : TestCaseWithFactory
	{
		#region Test Messages

		readonly string messageText1 = "UNH+000003+CUSRES:D:99B:UN'BGM+34:::CARST+4GJ5 BJA7 IAFG:1+8'DTM+9:20060518162923119738:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'FTX+AHN+++CARGO NOT A CONSOLIDATION:N/A'FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:NO'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+PS15S::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:H60001003/CM21::1'RFF+MB:OBL111'RFF+AAQ:UBUB6969696'DOC+1'PAC+++FCL:67:95'UNT+34+000003'";
		readonly string messageText2 = "UNH+000004+CUSRES:D:99B:UN'BGM+34:::CARST+1G94 CCHI AFG:1+8'DTM+9:20060518162923867244:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'FTX+AHN+++CARGO NOT A CONSOLIDATION:N/A'FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:NO'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+PS15S::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:H60001003/CM21::1'RFF+MB:OBL111'RFF+AAQ:ABCD1234560'DOC+1'PAC+++FCL:67:95'UNT+34+000004'";
		readonly string messageText3 = "UNH+000005+CUSRES:D:99B:UN'BGM+34:::CARST+3B40 HF2H IAFG:1+8'DTM+9:20060518162927012502:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:NO'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+PS15S::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:H60001004/CM21::1'RFF+MB:OBL222'RFF+AAQ:DCBA7654325'DOC+1'PAC+++FCL:67:95'UNT+34+000005'";
		readonly string messageText4 = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+46AB IJGE G0FG:1+8'DTM+9:20060518165909561109:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'FTX+AHN+++LCL UNDERBOND SATISFIED:YES'FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+PS15S::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:L60001006/CM21::1'RFF+MB:OBL111'RFF+BH:HOUSE222'RFF+AAQ:ABCD1234560'DOC+1'PAC+++LCL:67:95'UNT+35+000001'";
		readonly string messageText5 = "UNH+000002+CUSRES:D:99B:UN'BGM+961:::UBMREQR+39C6 ECCJ G0FG:1+32'DTM+9:20060518165923678635:ZZZ'FTX+AAH+++AAA374MU60001073/CM21'TDT+20+101S++11++++9227297::11'TDT+1++ROA'LOC+5+PS15S::95'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:U60001073/CM21::1'RFF+ANX:UNDERBOND APPROVAL'RFF+ACD:MOV'DOC+1'PAC+++FCL:67:95'PAC+0000001++YC:185:95'RFF+AAQ:UBUB6969696'PCI+28+STUFF'UNT+19+000002'";
		readonly string messageText6 = "UNH+000003+CUSRES:D:99B:UN'BGM+961:::UBMREQR+1H5J A209 G0FG:1+32'DTM+9:20060518165923939608:ZZZ'FTX+AAH+++AAA374MU60001071/CM21'TDT+20+101S++11++++9227297::11'TDT+1++ROA'LOC+5+PS15S::95'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:U60001071/CM21::1'RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'RFF+ACD:MOV'DOC+1'PAC+++FCL:67:95'PAC+0000001++YC:185:95'RFF+AAQ:ABCD1234560'PCI+28+STUFF'UNT+19+000003'";
		readonly string messageText7 = "UNH+000004+CUSRES:D:99B:UN'BGM+961:::UBMREQR+22I4 8AF9 G0FG:1+32'DTM+9:20060518165923905535:ZZZ'FTX+AAH+++AAA374MU60001071/CM21'TDT+20+101S++11++++9227297::11'TDT+1++ROA'LOC+5+PS15S::95'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:U60001071/CM21::1'RFF+ANX:UNDERBOND APPROVAL'RFF+ACD:MOV'DOC+1'PAC+++FCL:67:95'PAC+0000001++YC:185:95'RFF+AAQ:ABCD1234560'PCI+28+STUFF'UNT+19+000004'";
		readonly string messageText8 = "UNH+000005+CUSRES:D:99B:UN'BGM+961:::UBMREQR+4D47 0209 G0FG:1+32'DTM+9:20060518165923745912:ZZZ'FTX+AAH+++AAA374MU60001073/CM21'TDT+20+101S++11++++9227297::11'TDT+1++ROA'LOC+5+PS15S::95'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:U60001073/CM21::1'RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'RFF+ACD:MOV'DOC+1'PAC+++FCL:67:95'PAC+0000001++YC:185:95'RFF+AAQ:UBUB6969696'PCI+28+STUFF'UNT+19+000005'";
		readonly string messageText9 = "UNH+000006+CUSRES:D:99B:UN'BGM+34:::CARST+3HA2 0J44 G0FG:1+8'DTM+9:20060518165924256845:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'FTX+AHN+++CARGO NOT A CONSOLIDATION:N/A'FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:H60001003/CM21::1'RFF+MB:OBL111'RFF+AAQ:UBUB6969696'DOC+1'PAC+++FCL:67:95'UNT+34+000006'";
		readonly string messageText10 = "UNH+000007+CUSRES:D:99B:UN'BGM+34:::CARST+3C0C 4359 G0FG:1+8'DTM+9:20060518165925296194:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'FTX+AHN+++CARGO NOT A CONSOLIDATION:N/A'FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:H60001003/CM21::1'RFF+MB:OBL111'RFF+AAQ:ABCD1234560'DOC+1'PAC+++FCL:67:95'UNT+34+000007'";
		readonly string messageText11 = "UNH+000008+CUSRES:D:99B:UN'BGM+961:::UBMREQR+4EBD 94GE G0FG:1+32'DTM+9:20060518165926636321:ZZZ'FTX+AAH+++AAA374MU60001072/CM22'TDT+20+101S++11++++9227297::11'TDT+1++ROA'LOC+5+9914N::95'LOC+4+FB60H::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:U60001072/CM22::1'RFF+ANX:UNDERBOND APPROVAL'RFF+ACD:MOV'DOC+1'PAC+++LCL:67:95'PAC+0000025++PK:185:95'RFF+AAQ:ABCD1234560'RFF+MB:OBL111'RFF+BH:HOUSE111'PCI+28+THINGS'UNT+21+000008'";
		readonly string messageText12 = "UNH+000009+CUSRES:D:99B:UN'BGM+34:::CARST+2G95 DH09 G0FG:1+8'DTM+9:20060518165927392328:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'FTX+AHN+++LCL UNDERBOND SATISFIED:YES'FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'FTX+AHN+++RELEASE PREMISE IN DESTINATION:NO'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+FB60H::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:L60001005/CM21::1'RFF+MB:OBL111'RFF+BH:HOUSE111'RFF+AAQ:ABCD1234560'DOC+1'PAC+++LCL:67:95'UNT+35+000009'";
		readonly string messageText13 = "UNH+000010+CUSRES:D:99B:UN'BGM+34:::CARST+281B CDEH 60FG:1+8'DTM+9:20060518165938923729:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'FTX+AHN+++LCL UNDERBOND SATISFIED:YES'FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+101S++11++++9227297::11'LOC+12+AUSYD::6'LOC+4+9914N::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:L60001007/CM21::1'RFF+MB:OBL111'RFF+BH:HOUSE333'RFF+AAQ:UBUB6969696'DOC+1'PAC+++LCL:67:95'UNT+35+000010'";
		readonly string messageText14 = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+3FJ6 B52A H8BB:1+8'DTM+9:20111215131800698909:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+057++11++++9407146::11'LOC+12+AUMEL::6'LOC+4+FY09D::95'NAD+MR+FFT333W::95'NAD+UD+19000871330::95'RFF+ABO:S00074399/MEL1::1'RFF+MB:TBA'RFF+BH:STCBLRMEL235'RFF+AAQ:TCKU9058690'DOC+1'PAC+++LCL:67:95'PAC+0094825++YC:185:95'UNT+18+000001'";

		CMRCUSRESMessage GetMessageFromText(string messageText)
		{
			CMRCUSRESMessage result = null;
			if (messageText.Contains("BGM+34:::CARST"))
			{
				result = Factory.New<CMRCARSTMessage>();
			}
			else if (messageText.Contains("BGM+961:::UBMREQR"))
			{
				result = Factory.New<CMRUBMREQRMessage>();
			}
			else
			{
				throw new Exception("Invalid messageText has been passed in.");
			}
			result.EM_MessageText = messageText;
			result.EM_Status = "QUE";
			result.EM_ReceiveTransmit = "RCV";
			return result;
		}

		void AssertProcesses(string messageText)
		{
			CMRCUSRESMessage message = GetMessageFromText(messageText);
			message.SetEM_LinkedObject();
			AssertNotNull(message.EM_LinkedObject);
			Factory.Save();
		}

		#endregion

		#region Misc

		public void TestEndToEnd()
		{
			#region Constants

			const string voyageNumber = "101S";
			const string lloydsNumber = "9227297";
			const string ourPremiseID = "9914N";
			const string ctoPremiseID = "PS15S";
			const string otherPremiseID = "FB60H";
			const string oceanBill1 = "OBL111";
			const string oceanBill2 = "OBL222";
			const string houseBill1 = "HOUSE111";
			const string houseBill2 = "HOUSE222";
			const string houseBill3 = "HOUSE333";
			const string container1 = "ABCD1234560";
			const string container2 = "UBUB6969696";

			#endregion

			#region Set Up

			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = ourPremiseID;

			#endregion

			#region Import Manifest

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "WANG";
			vessel.RV_LloydsNumber = lloydsNumber;

			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_VoyageNum = voyageNumber;
			tranHead.BT_LloydsIMO = lloydsNumber;
			tranHead.BT_VesselName = "WANG";

			CusSeaManOBLHeader tranOcean1 = tranHead.OceanBills.AddNew();
			tranOcean1.BO_OceanBill = oceanBill1;
			tranOcean1.BO_SendersMessageReference = "H60001003";

			CusSeaManOBLHeader tranOcean2 = tranHead.OceanBills.AddNew();
			tranOcean2.BO_OceanBill = oceanBill2;
			tranOcean2.BO_SendersMessageReference = "H60001004";

			CusSeaManOBLDetail tranContainer1 = tranOcean1.Details.AddNew();
			tranContainer1.BD_ContainerNumber = container1;
			tranContainer1.BD_LineCargoType = "FCL";
			tranContainer1.BD_PackType = "YC";
			tranContainer1.BD_NoOfPacks = 1;

			CusSeaManOBLDetail tranContainer2 = tranOcean1.Details.AddNew();
			tranContainer2.BD_ContainerNumber = container2;
			tranContainer2.BD_LineCargoType = "FCL";
			tranContainer2.BD_PackType = "YC";
			tranContainer2.BD_NoOfPacks = 1;

			CusUnderbond tranUnderbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)tranContainer1).Underbonds.AddNew();
			tranUnderbond1.C4_DestinationPremiseID = ourPremiseID;
			tranUnderbond1.C4_OriginPremiseID = ctoPremiseID;
			tranUnderbond1.C4_MovementReason = "MOV";
			tranUnderbond1.C4_SendersMessageReference = "U60001071";

			CusUnderbond tranUnderbond2 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)tranContainer2).Underbonds.AddNew();
			tranUnderbond2.C4_DestinationPremiseID = ourPremiseID;
			tranUnderbond2.C4_OriginPremiseID = ctoPremiseID;
			tranUnderbond2.C4_MovementReason = "MOV";
			tranUnderbond2.C4_SendersMessageReference = "U60001073";

			#endregion

			#region Sea Cargo Forwarder

			CusSCAOceanBill scaOcean1 = Factory.New<CusSCAOceanBill>();
			scaOcean1.CB_LloydsIMO = lloydsNumber;
			scaOcean1.CB_MasterHouseBill = oceanBill1;
			scaOcean1.CB_Voyage = voyageNumber;

			CusSCAContainer scaContainer1 = scaOcean1.Containers.AddNew();
			scaContainer1.CN_ContainerMode = "LCL";
			scaContainer1.CN_ContainerNumber = container1;

			CusSCAContainer scaContainer2 = scaOcean1.Containers.AddNew();
			scaContainer2.CN_ContainerMode = "LCL";
			scaContainer2.CN_ContainerNumber = container2;

			CusSCAHouse scaHouse1 = scaOcean1.HouseBills.AddNew();
			scaHouse1.CA_HouseBill = houseBill1;
			scaHouse1.CA_BGMReference = "L60001005";

			CusSCAHouse scaHouse2 = scaOcean1.HouseBills.AddNew();
			scaHouse2.CA_HouseBill = houseBill2;
			scaHouse2.CA_BGMReference = "L60001006";

			CusSCAHouse scaHouse3 = scaOcean1.HouseBills.AddNew();
			scaHouse3.CA_HouseBill = houseBill3;
			scaHouse3.CA_BGMReference = "L60001007";

			scaHouse1.Pivot.AddNew().CV_CN = scaContainer1.PK;
			scaHouse2.Pivot.AddNew().CV_CN = scaContainer1.PK;
			scaHouse3.Pivot.AddNew().CV_CN = scaContainer2.PK;

			scaHouse1.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			scaHouse2.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			scaHouse3.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			var scaPivot1 = scaHouse1.Pivot[0];
			scaPivot1.CV_PackageCount = 25;
			scaPivot1.CV_PackageType = "PK";

			CusUnderbond scaUnderbond1 = scaPivot1.Underbonds.AddNew();
			scaUnderbond1.C4_SendersMessageReference = "U60001072";
			scaUnderbond1.C4_OriginPremiseID = ourPremiseID;
			scaUnderbond1.C4_DestinationPremiseID = otherPremiseID;
			scaUnderbond1.C4_MovementReason = "MOV";

			#endregion

			Factory.Save();

			#region Message Processing!

			AssertProcesses(messageText1);
			AssertProcesses(messageText2);
			AssertProcesses(messageText3);
			AssertProcesses(messageText4);
			AssertProcesses(messageText5);
			AssertProcesses(messageText6);
			AssertProcesses(messageText7);
			AssertProcesses(messageText8);
			AssertProcesses(messageText9);
			AssertProcesses(messageText10);
			AssertProcesses(messageText11);
			AssertProcesses(messageText12);
			AssertProcesses(messageText13);

			ZQuery headerQuery = new ZQuery();
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, lloydsNumber);
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, voyageNumber);
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, ourPremiseID);

			CusOutturnHeader[] headers = Factory.Load<CusOutturnHeader>(headerQuery);
			AssertEquals(1, headers.Length);
			CusOutturnHeader header = headers[0];

			AssertEquals(4, header.Outturns.Count);
			AssertEquals(2, header.Underbonds.Count);
			var outturn = header.Outturns.Cast<CusOutturn>().FirstOrDefault(x => x.C5_HouseBill == houseBill1);
			AssertNotNull(outturn);
			AssertNull(outturn.Underbond);
			outturn = header.Outturns.Cast<CusOutturn>().FirstOrDefault(x => x.C5_HouseBill == houseBill3);
			AssertNotNull(outturn);
			AssertNull(outturn.Underbond);

			#endregion
		}

		public void Test00442700()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "FY09D";
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = messageText14;
			message.EM_EI = interchange.PK;
			var processor = new CMRSeaDepotMessageProcessorForTest(message);
			AssertNoExceptionThrown(delegate
			{ processor.Process(); });
		}

		public void TestPivotClearanceDateIsUpdatedFromCARSTClearStatus()
		{
			string messageText = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+3FJ6 B52A H8BB:1+8'DTM+9:20111215131800698909:ZZZ'" +
				"FTX+AHN+++CONSOLIDATED STATUS:CLEAR'FTX+AHN+++CARGO REPORT SAC:NO'" + // status
				"TDT+20+057++11++++9407146::11'" + // voyage, lloyds
				"LOC+12+AUMEL::6'LOC+4+FY09D::95'" + // PremiseID
				"NAD+MR+FFT333W::95'NAD+UD+19000871330::95'RFF+ABO:S00074399/MEL1::1'" +
				"RFF+MB:OBL0112162'" + // ocean
				"RFF+BH:STCBLRMEL235'" + // house
				"RFF+AAQ:TCKU9058690'" + // container
				"DOC+1'PAC+++LCL:67:95'PAC+0094825++YC:185:95'UNT+18+000001'";

			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "FY09D";
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = messageText;
			message.EM_EI = interchange.PK;

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OBL0112162";
			oceanBill.CB_Voyage = "057";
			oceanBill.CB_LloydsIMO = "9407146";
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "STCBLRMEL235";

			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "TCKU9058690";
			var pivot = houseBill.Pivot.AddNew();
			pivot.CV_CN = container.PK;

			Factory.Save();

			var processor = new CMRSeaDepotMessageProcessorForTest(message);
			AssertNoExceptionThrown(delegate
			{ processor.Process(); });

			var depotEventLog = pivot.Logs.MostRecentLogByEventTime(Events.SeaCargoDepotEvent);
			AssertEquals("CLEAR FY09D", depotEventLog.SL_Reference);
			AssertEquals("Pivot clearance date is updated", depotEventLog.SL_EventTime, pivot.CV_ClearanceDate);
		}

		public void TestProcessWithSEIMessage()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			header.C6_SendersMessageReference = "O00000007";
			header.C6_OutturningPremiseID = "9914N";
			header.C6_LloydsIMO = "7631456";
			header.C6_VoyageNum = "6990";

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			var message = Factory.New<CMRSEIMessage>();
			message.EM_MessageText = CMRSEIMessageTest.SampleSEIMessage;
			message.EM_EI = interchange.PK;

			CMRSeaDepotMessageProcessor processor = new CMRSeaDepotMessageProcessorForTest(message);
			processor.Process();

			Factory.Save();

			ZQuery query = new ZQuery();
			query.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, "9914N");
			query.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, "7631456");
			query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, "6990");
			CusOutturnHeader[] outturnHeaders = Factory.Load<CusOutturnHeader>(query);
			AssertEquals("header exists", 1, outturnHeaders.Length);

			query = new ZQuery();
			query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.FullContainerLoad);
			query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, "OCLU8911239");
			query.AddToFilter(CusOutturnSchema.C5_HouseBill, "");
			query.AddToFilter(CusOutturnSchema.C5_MasterBill, "");
			CusOutturn[] outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("fcl outturn created", 1, outturns.Length);
			AssertEquals("Manifested qty", (ZInt)100, outturns[0].C5_OuterPacks);
			AssertEquals("Manifested qty units", "BX", outturns[0].C5_OuterPackUnits);
			AssertEquals("Description", "CONSOLIDATED CARGO", outturns[0].C5_GoodsDescription);
			AssertEquals("Message attached", 1, outturns[0].Messages.Count);
			AssertNull("Should not access CUSRES during clone before EM_MessageText set to a shortened version", ((CMRCUSRESMessage)outturns[0].Messages[0]).CUSRESCacheForTesting);

			const string cutDowntext1 = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEI+1G79 7IAF 71F9:1++11'DTM+9:20090317154645464587:ZZZ'TDT+20+6990++11++++7631456::11'" +
"TDT+1++ROA'NAD+MR+AAA374M::95'NAD+VW+41065894724::95'RFF+ABO:O00000007/DAT8::8'" +
"DOC+1'PAC+++FCL:67:95'PAC+100++BX:185:95'RFF+MB:OBLDPT001'RFF+AAQ:OCLU8911239'" +
"FTX+AAA+++CONSOLIDATED CARGO'GIS+FFO:109:95'MEA+AAE+G+KG:0000000023000.00'MEA+AAE+AAL+KG:0000000020000.00'MEA+AAE+ABJ+CM:0000000000020.00'NAD+CN++LOCAL FORWARDER'" +
"UNT+43+000001'";
			AssertEquals("Cut-down text", cutDowntext1, outturns[0].Messages[0].EM_MessageText);

			query = new ZQuery();
			query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.LessThanContainerLoad);
			query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, "OCLU8911239");
			query.AddToFilter(CusOutturnSchema.C5_HouseBill, "HBL001");
			query.AddToFilter(CusOutturnSchema.C5_MasterBill, "OBLDPT001");
			outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("lcl house 1 created", 1, outturns.Length);
			AssertEquals("Manifested qty", (ZInt)20, outturns[0].C5_OuterPacks);
			AssertEquals("Manifested qty units", "BX", outturns[0].C5_OuterPackUnits);
			AssertEquals("Description", "STUFF TYPE 1", outturns[0].C5_GoodsDescription);
			AssertEquals("Message attached", 1, outturns[0].Messages.Count);
			const string cutDowntext2 = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEI+1G79 7IAF 71F9:1++11'DTM+9:20090317154645464587:ZZZ'TDT+20+6990++11++++7631456::11'" +
"TDT+1++ROA'NAD+MR+AAA374M::95'NAD+VW+41065894724::95'RFF+ABO:O00000007/DAT8::8'" +
"DOC+1'PAC+++LCL:67:95'PAC+20++BX:185:95'RFF+MB:OBLDPT001'RFF+BH:HBL001'RFF+AAQ:OCLU8911239'FTX+AAA+++STUFF TYPE 1'MEA+AAE+G+KG:0000000005000.00'" +
"MEA+AAE+AAL+KG:0000000005000.00'MEA+AAE+ABJ+CM:0000000000005.00'NAD+CN++CONSIGNEE'" +
"UNT+43+000001'";
			AssertEquals("Cut-down text", cutDowntext2, outturns[0].Messages[0].EM_MessageText);

			query = new ZQuery();
			query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.LessThanContainerLoad);
			query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, "OCLU8911239");
			query.AddToFilter(CusOutturnSchema.C5_HouseBill, "HBL002");
			query.AddToFilter(CusOutturnSchema.C5_MasterBill, "OBLDPT001");
			outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("lcl house 2 created", 1, outturns.Length);
			AssertEquals("Manifested qty", (ZInt)30, outturns[0].C5_OuterPacks);
			AssertEquals("Manifested qty units", "BX", outturns[0].C5_OuterPackUnits);
			AssertEquals("Description", "STUFF TYPE 2", outturns[0].C5_GoodsDescription);
			AssertEquals("Marks", CMRSEIMessageTest.ExpectedMarks, outturns[0].C5_MarksAndNumbers);
			AssertEquals("Message attached", 1, outturns[0].Messages.Count);
			const string cutDowntext3 = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEI+1G79 7IAF 71F9:1++11'DTM+9:20090317154645464587:ZZZ'TDT+20+6990++11++++7631456::11'" +
"TDT+1++ROA'NAD+MR+AAA374M::95'NAD+VW+41065894724::95'RFF+ABO:O00000007/DAT8::8'" +
"DOC+1'PAC+++LCL:67:95'PAC+30++BX:185:95'RFF+MB:OBLDPT001'RFF+BH:HBL002'" +
"RFF+AAQ:OCLU8911239'PCI+28+MARKS1:MARKS2:MARKS3:MARKS4:MARKS5:MARKS6:MARKS7:MARKS8:MARKS9'FTX+AAA+++STUFF TYPE 2'MEA+AAE+G+KG:0000000007000.00'" +
"MEA+AAE+AAL+KG:0000000007001.00'MEA+AAE+ABJ+CM:0000000000007.00'NAD+CN++CONSIGNEE 2'" +
"UNT+43+000001'";
			AssertEquals("Cut-down text", cutDowntext3, outturns[0].Messages[0].EM_MessageText);

			outturns[0].C5_MasterBill = "BADMASTERBILL";
			outturns[0].C5_GoodsDescription = "";
			Factory.Save();
			query = new ZQuery();
			query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.LessThanContainerLoad);
			query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, "OCLU8911239");
			query.AddToFilter(CusOutturnSchema.C5_HouseBill, "HBL002");
			query.AddToFilter(CusOutturnSchema.C5_MasterBill, "OBLDPT001");
			outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("Pre-condition: HBL002 not found on master HBL002", 0, outturns.Length);
			query = new ZQuery();
			query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.LessThanContainerLoad);
			query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, "OCLU8911239");
			query.AddToFilter(CusOutturnSchema.C5_HouseBill, "HBL002");
			query.AddToFilter(CusOutturnSchema.C5_MasterBill, "BADMASTERBILL");
			outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("Pre-condition: HBL002 found on master BADMASTERBILL", 1, outturns.Length);

			processor.Process();
			Factory.Save();

			query = new ZQuery();
			query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.LessThanContainerLoad);
			query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, "OCLU8911239");
			query.AddToFilter(CusOutturnSchema.C5_HouseBill, "HBL002");
			query.AddToFilter(CusOutturnSchema.C5_MasterBill, "OBLDPT001");
			outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("lcl house 2 back on correct master", 1, outturns.Length);
			AssertEquals("Manifested qty", (ZInt)30, outturns[0].C5_OuterPacks);
			AssertEquals("Manifested qty units", "BX", outturns[0].C5_OuterPackUnits);
			AssertEquals("Description", "STUFF TYPE 2", outturns[0].C5_GoodsDescription);
			AssertEquals("Marks", CMRSEIMessageTest.ExpectedMarks, outturns[0].C5_MarksAndNumbers);
			AssertEquals("Message attached", 1, outturns[0].Messages.Count);

			query = new ZQuery();
			query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.LessThanContainerLoad);
			query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, "OCLU8911239");
			query.AddToFilter(CusOutturnSchema.C5_HouseBill, "HBL002");
			query.AddToFilter(CusOutturnSchema.C5_MasterBill, "BADMASTERBILL");
			outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("HBL002 not found on master BADMASTERBILL", 0, outturns.Length);
		}

		public void TestSEIMessageProcessingGeneratesTransitWarehouseUniversalShipment()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OBLDPT001";

			var destinationAddress = OrganizationAddressTestHelper.GetOrganizationBO_WUFSHIJNB(Factory);
			destinationAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "AA33N", Core.Constants.CountryCodes.Australia);
			underbond.C4_OA_DestinationAddress = destinationAddress.MainAddress.PK;
			underbond.C4_DestinationPremiseID = "AA33N";
			Factory.Save();

			var outturn = Factory.New<CusOutturn>();
			outturn.C5_HouseBill = "HBL001";
			outturn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			outturn.C5_DamageIndicator = ZBool.True;
			outturn.C5_PillageIndicator = ZBool.True;
			outturn.C5_OuterPacks = 9;
			outturn.C5_OuterPackUnits = CMRPackageTypes.Codes.Crate;
			outturn.C5_PackagesOutturned = 10;
			outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			outturn.C5_GoodsDescription = "Nike Sneakers";

			underbond.Outturns.Add(outturn);

			Factory.Save();

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_SendersMessageReference = "O00000007";
			outturnHeader.C6_OutturningPremiseID = "9914N";
			outturnHeader.C6_LloydsIMO = "7631456";
			outturnHeader.C6_VoyageNum = "6990";

			var interchange = Factory.New<EDIInterchange>();
			interchange.FillWithValidTestData();

			var message = Factory.New<CMRSEIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "TESTCARST00000";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = CMRSEIMessageTest.SampleSEIMessage;
			message.EM_EI = interchange.PK;

			Factory.Save();

			var processor = new AUCConcurrentMessageProcessor();
			processor.ExecuteBatch();
			Factory.Save();

			var logs = outturnHeader.Logs.GetAllLogs();
			logs.Reload(true);

			var dexEvent = logs.Cast<StmALog>().First(x => x.SL_SE_NKEvent == Events.DataExport.Code);
			AssertNotNull("One [DEX] event should be linked to outturnHeader", dexEvent);
			AssertNotNull(dexEvent.RelatedEDIMessage);
			AssertNotNull(dexEvent.RelatedEDIMessage.Message);
			AssertNotNullOrEmpty(dexEvent.RelatedEDIMessage.Message.EM_MessageTextDetail);

			var universalShipmentMessage = dexEvent.RelatedEDIMessage.Message;
			AssertNotNull(universalShipmentMessage);

			var logger = new DummyLogger();
			var outturnHeaderData = universalShipmentMessage.GetEM_MessageTextReader().Parse<UniversalShipment>(logger, new CodeMappingManager(logger));

			var role = outturnHeaderData.DataContext.RecipientRoleCollection.Single();
			AssertEquals("outturnHeaderData.DataContext.RecipientRoleCollection[0].Code", RecipientRoleType.ATW, role.Code);
			AssertEquals("outturnHeaderData.DataContext.RecipientRoleCollection[0].ServiceCode", ServiceCodeType.TWR, role.ServiceCode);

			AssertEquals("outturnHeaderData.SubShipmentCollection.Count", 2, outturnHeaderData.SubShipmentCollection.Count);
			AssertEquals("outturnHeaderData.ContainerCollection.Count", 1, outturnHeaderData.ContainerCollection.Count);
			var containerData = outturnHeaderData.ContainerCollection[0];
			AssertEquals("Container", "OCLU8911239", containerData.ContainerNumber);

			var shipmentData = outturnHeaderData.SubShipmentCollection[0];
			AssertEquals("Container", "OCLU8911239", shipmentData.ContainerCollection[0].ContainerNumber);
			AssertEquals("Packs", 20, shipmentData.OuterPacks);
			AssertEquals("Pack Type", "BX", shipmentData.OuterPacksPackageType.Code);
			AssertEquals("HouseBill", "HBL001", shipmentData.WayBillNumber);

			shipmentData = outturnHeaderData.SubShipmentCollection[1];
			AssertEquals("Container", "OCLU8911239", shipmentData.ContainerCollection[0].ContainerNumber);
			AssertEquals("Packs", 30, shipmentData.OuterPacks);
			AssertEquals("Pack Type", "BX", shipmentData.OuterPacksPackageType.Code);
			AssertEquals("HouseBill", "HBL002", shipmentData.WayBillNumber);

			AssertEquals("Voyage", "ANRO ASIA", outturnHeaderData.VesselName);
			AssertEquals("Voyage", "6990", outturnHeaderData.VoyageFlightNo);
		}

		public void TestAutoSendsSEQFromCARST()
		{
			string carstMessage = @"UNH+000004+CUSRES:D:99B:UN
BGM+34:::CARST+254D 6HEF GJ59:1+8
DTM+9:20090316133516981699:ZZZ
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES
FTX+AHN+++IAR ACS CLEARED:YES
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A
FTX+AHN+++CARGO NOT A CONSOLIDATION:NO
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO
FTX+AHN+++IAR AQIS CLEARED:YES
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A
FTX+AHN+++ACS EVALUATION COMPLETE:YES
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A
FTX+AHN+++IMPORT DECLARATION PAID:N/A
FTX+AHN+++CARGO REPORT SAC:NO
TDT+20+6990++11++++7631456::11
LOC+12+AUSYD::6
LOC+4+9914N::95
NAD+MR+AAA374M::95
NAD+UD+41065894724::95
RFF+ABO:A96E 9199 59::1
RFF+AAQ:OCLU8911239
DOC+1
PAC+++FCL:67:95
UNT+33+000004
".Replace("\r\n", "'");
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = carstMessage;
			message.EM_MessageNum = "1";
			AssertSEQMessageCreated("9914N", false, 0, message);
			AssertSEQMessageCreated("9999X", true, 0, message);
			AssertSEQMessageCreated("9999X", false, 0, message);
			AssertSEQMessageCreated("", true, 0, message);
			AssertSEQMessageCreated("", true, 0, message);
			AssertSEQMessageCreated("9914N", true, 1, message);

			ZQuery query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, "SEQ");
			query.AddToFilter(EDIMessageSchema.EM_Status, "QUE");
			AssertEquals("Pre-condition for next assert: An SEQ message exists", 1, (Factory.Load<EDIMessage>(query)).Length);
			// AssertSEQMessageCreated should not create an additional message, since one already exists
			AssertSEQMessageCreated("9914N", true, 1, message);

			ZQuery headerQuery = new ZQuery(CusOutturnHeaderSchema.C6_LloydsIMO, "7631456");
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, "9914N");
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, "6990");
			CusOutturnHeader[] headers = Factory.Load<CusOutturnHeader>(headerQuery);
			AssertEquals("Should be a header", 1, headers.Length);
			EDIMessage sEIMessage = headers[0].Messages.AddNew();
			sEIMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			sEIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			sEIMessage.EM_MessageNum = "2";
			// AssertSEQMessageCreated should create an additional message, since a reply has been received to previous SEQ
			AssertSEQMessageCreated("9914N", true, 2, message);
		}

		public void TestAutoSendsSEQFromUBMREQR()
		{
			string uBMREQRMessage = @"UNH+000003+CUSRES:D:99B:UN
BGM+961:::UBMREQR+3JG3 73JF GJ59:1+32
DTM+9:20090316133510833009:ZZZ
TDT+20+6990++11++++7631456::11
TDT+1++ROA
LOC+5+9122P::95
LOC+4+9914N::95
NAD+MR+AAA374M::95
NAD+UD+41065894724::95
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE
RFF+ACD:DCL
DOC+1
PAC+++FCL:67:95
RFF+AAQ:OCLU8911239
UNT+15+000003
".Replace("\r\n", "'");
			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_MessageText = uBMREQRMessage;
			message.EM_MessageNum = "1";
			AssertSEQMessageCreated("9914N", false, 0, message);
			AssertSEQMessageCreated("9999X", true, 0, message);
			AssertSEQMessageCreated("9999X", false, 0, message);
			AssertSEQMessageCreated("", true, 0, message);
			AssertSEQMessageCreated("", true, 0, message);
			AssertSEQMessageCreated("9914N", true, 1, message);

			ZQuery query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, "SEQ");
			query.AddToFilter(EDIMessageSchema.EM_Status, "QUE");
			AssertEquals("Pre-condition for next assert: An SEQ message exists", 1, (Factory.Load<EDIMessage>(query)).Length);
			// AssertSEQMessageCreated should not create an additional message, since one already exists
			AssertSEQMessageCreated("9914N", true, 1, message);

			ZQuery headerQuery = new ZQuery(CusOutturnHeaderSchema.C6_LloydsIMO, "7631456");
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, "9914N");
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, "6990");
			CusOutturnHeader[] headers = Factory.Load<CusOutturnHeader>(headerQuery);
			AssertEquals("Should be a header", 1, headers.Length);
			EDIMessage sEIMessage = headers[0].Messages.AddNew();
			sEIMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			sEIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			sEIMessage.EM_MessageNum = "2";
			// AssertSEQMessageCreated should create an additional message, since a reply has been received to previous SEQ
			AssertSEQMessageCreated("9914N", true, 2, message);
		}

		public void TestHouseCARSTDoesNotTriggerSEQ()
		{
			string carstMessage = @"UNH+000001+CUSRES:D:99B:UN
BGM+34:::CARST+4H67 5J6D J959:1+8
DTM+9:20090316134738836021:ZZZ
FTX+AHN+++CONSOLIDATED STATUS:HELD
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES
FTX+AHN+++IAR ACS CLEARED:YES
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES
FTX+AHN+++LCL UNDERBOND SATISFIED:YES
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES
FTX+AHN+++IAR AQIS CLEARED:YES
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A
FTX+AHN+++ACS EVALUATION COMPLETE:YES
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A
FTX+AHN+++IMPORT DECLARATION PAID:N/A
FTX+AHN+++CARGO REPORT SAC:NO
TDT+20+6990++11++++7631456::11
LOC+12+AUSYD::6
LOC+4+9914N::95
NAD+MR+AAA374M::95
NAD+UD+41065894724::95
RFF+ABO:4293 GEFJ BJ59::1
RFF+MB:OBLDPT001
RFF+BH:HBL001
RFF+AAQ:OCLU8911239
DOC+1
PAC+++LCL:67:95
UNT+35+000001
".Replace("\r\n", "'");
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = carstMessage;
			message.EM_MessageNum = "1";
			AssertSEQMessageCreated("9914N", true, 0, message);
		}

		void AssertSEQMessageCreated(ZString localControlledPremisesID, ZBool isUnpackDepot, int expectedCount, ICMRDepotMessage message)
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = localControlledPremisesID;
			currentCompany.OrgProxy.OH_IsUnpackDepot = isUnpackDepot;
			CMRSeaDepotMessageProcessor processor = new CMRSeaDepotMessageProcessorForTest(message);
			processor.Process();
			ZQuery query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, "SEQ");
			query.AddToFilter(EDIMessageSchema.EM_Status, "QUE");
			query.TableIndexHints.Add(new TableIndexHint("PK_UX__EM_PK"));
			EDIMessage[] queuedSEQs = Factory.Load<EDIMessage>(query);
			AssertEquals
			(
				$"Expected {expectedCount} message(s) created for Establishment = {localControlledPremisesID} IsDepot = {isUnpackDepot}",
				expectedCount,
				queuedSEQs.Length
			);
		}

		public void TestProcessingWhenAlreadyLinkedToAnotherObject()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";

			string messageText = @"UNH+000002+CUSRES:D:99B:UN
BGM+34:::CARST+4DAA AIGC A60G:1+8
DTM+9:20060522102500018187:ZZZ
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES
FTX+AHN+++IAR ACS CLEARED:YES
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A
FTX+AHN+++CARGO NOT A CONSOLIDATION:N/A
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO
FTX+AHN+++IAR AQIS CLEARED:YES
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A
FTX+AHN+++ACS EVALUATION COMPLETE:YES
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A
FTX+AHN+++IMPORT DECLARATION PAID:N/A
FTX+AHN+++CARGO REPORT SAC:NO
TDT+20+102S++11++++8811924::11
LOC+12+AUSYD::6
LOC+4+9914N::95
NAD+MR+AAA374M::95
NAD+UD+41065894724::95
RFF+ABO:H60001005/CM21::1
RFF+MB:OBL200
RFF+AAQ:BBEO3040507
DOC+1
PAC+++FCL:67:95
UNT+34+000002
".Replace("\r\n", "'");

			ZQuery query = new ZQuery();
			query.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, "9914N");
			query.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, "8811924");
			query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, "102S");

			CusOutturnHeader[] outturnHeaders = Factory.Load<CusOutturnHeader>(query);
			AssertEquals("precondition", 0, outturnHeaders.Length);

			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_VoyageNum = "102S";
			tranHead.BT_LloydsIMO = "8811924";

			CusSeaManOBLHeader header = tranHead.OceanBills.AddNew();
			header.BO_SendersMessageReference = "H60001005";
			header.BO_OceanBill = "OBL200";

			CusSeaManOBLDetail detail = header.Details.AddNew();
			detail.BD_ContainerNumber = "BBEO3040507";

			AssertEquals(0, header.Messages.Count);

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = messageText;
			message.SetEM_LinkedObject();

			//AssertEquals(header, Message.EM_LinkedObject);
			header.Messages.Load();
			AssertEquals(1, header.Messages.Count);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			outturnHeaders = factory2.Load<CusOutturnHeader>(query);
			AssertEquals(1, outturnHeaders.Length);
			CusOutturnHeader outturnHeader = outturnHeaders[0];
			AssertEquals(1, outturnHeader.Outturns.Count);
			DepotCusOutturn outturn = outturnHeader.Outturns[0];

			AssertEquals("SUB", outturn.C5_CustomsStatus);
			AssertEquals(false, outturn.HasChanges);

			AssertEquals(1, outturn.Messages.Count);
			AssertEquals(message.EM_MessageText, outturn.Messages[0].EM_MessageText);
		}

		#endregion

		#region Should Process

		//public void TestShouldProcessIfStatusAndOutturnFound()
		//{
		//    dummyMessage.messageType = CMRDepotMessageType.Status;
		//    dummyMessage.ourPremiseID = otherPremiseID;	// O RLY?

		//    dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
		//    dummyLine.containerNumber = containerNumber;

		//    AssertProcessed(0);

		//    DepotCusOutturn outturn = GetOurOutturn();
		//    outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
		//    outturn.C5_ContainerNumber = containerNumber;

		//    AssertProcessed(1);

		//    dummyMessage.messageType = CMRDepotMessageType.Approval;
		//    AssertProcessed(1);

		//    dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
		//    AssertProcessed(1);
		//}

		public void TestShouldProcessIfCFSContainerFound()
		{
			dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
			dummyMessage.ourPremiseID = otherPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			AssertProcessed(0);

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = voyageNumber;
			consol.Transports.MostInterestingTransport.JW_Vessel = GetOurVessel().RV_Code;

			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_IsCFSRegistered = false;
			AssertProcessed(0);

			container.JC_IsCFSRegistered = true;
			AssertProcessed(0);

			container.JC_ContainerNum = otherContainerNumber;
			AssertProcessed(0);
		}

		public void TestShouldProcessIfCFSLoadListConsolFound()
		{
			dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			dummyLine.oceanBillNumber = oceanNumber;

			AssertProcessed(1);

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = voyageNumber;
			consol.Transports.MostInterestingTransport.JW_Vessel = GetOurVessel().RV_Code;
			consol.JK_MasterBillNum = oceanNumber;
			consol.JK_IsCFS = false;
			AssertProcessed(2);

			consol.JK_IsCFS = true;
			AssertProcessed(3);

			consol.JK_MasterBillNum = otherOceanNumber;
			AssertProcessed(4);
		}

		public void TestShouldProcessIfCFSLoadListConsolFoundWithOtherPremiseID()
		{
			dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
			dummyMessage.ourPremiseID = otherPremiseID; // This is different from the above test.

			dummyLine.containerMode = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			dummyLine.oceanBillNumber = oceanNumber;

			AssertProcessed(0);

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = voyageNumber;
			consol.Transports.MostInterestingTransport.JW_Vessel = GetOurVessel().RV_Code;
			consol.JK_MasterBillNum = oceanNumber;
			consol.JK_IsCFS = false;
			AssertProcessed(0);

			consol.JK_IsCFS = true;
			AssertProcessed(0);

			consol.JK_MasterBillNum = otherOceanNumber;
			AssertProcessed(0);
		}

		public void TestShouldProcessIfPremiseIDOurs()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);

			dummyMessage.ourPremiseID = otherPremiseID;
			AssertProcessed(0);

			dummyMessage.ourPremiseID = ourPremiseID;
			AssertProcessed(1);

			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = otherPremiseID;
			AssertProcessed(1);

			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = ourPremiseID;
			AssertProcessed(2);

			currentCompany.OrgProxy.OH_IsUnpackDepot = false;
			AssertProcessed(2);
		}

		#endregion

		#region New Header

		public void TestNewHeader()
		{
			ZQuery headerQuery = new ZQuery();
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, lloydsNumber);
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, ourPremiseID);
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, voyageNumber);

			CusOutturnHeader[] headers = Factory.Load<CusOutturnHeader>(headerQuery);
			AssertEquals("precondition", 0, headers.Length);

			dummyMessage.ourPremiseID = ourPremiseID;
			processor.Process();

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			headers = factory2.Load<CusOutturnHeader>(headerQuery);
			AssertEquals(1, headers.Length);

			CusOutturnHeader header = headers[0];
			AssertEquals(voyageNumber, header.C6_VoyageNum);
			AssertEquals(lloydsNumber, header.C6_LloydsIMO);
			AssertEquals(ourPremiseID, header.C6_OutturningPremiseID);
		}

		#endregion

		#region New Outturn

		public void TestNewOutturn()
		{
			ZQuery outturnQuery = new ZQuery();
			outturnQuery.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.LessThanContainerLoad);
			outturnQuery.AddToFilter(CusOutturnSchema.C5_ContainerNumber, containerNumber);
			outturnQuery.AddToFilter(CusOutturnSchema.C5_HouseBill, houseNumber);
			outturnQuery.AddToFilter(CusOutturnSchema.C5_MasterBill, oceanNumber);

			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			header.C6_VoyageNum = voyageNumber;
			header.C6_LloydsIMO = lloydsNumber;
			header.C6_OutturningPremiseID = ourPremiseID;
			Factory.Save();

			outturnQuery.AddToFilter(CusOutturnSchema.C5_C6, header.PK);

			DepotCusOutturn[] outturns = Factory.Load<DepotCusOutturn>(outturnQuery);
			AssertEquals("precondition", 0, outturns.Length);

			dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			dummyLine.containerNumber = containerNumber;
			dummyLine.goodsDescription = "goods";
			dummyLine.houseBillNumber = houseNumber;
			dummyLine.marksAndNumbers = "marks";
			dummyLine.oceanBillNumber = oceanNumber;
			dummyLine.packageType = CMRPackageTypes.Codes.Package;
			dummyLine.numberOfPackages = 7;

			processor.Process();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			outturns = factory2.Load<DepotCusOutturn>(outturnQuery);

			AssertEquals(1, outturns.Length);
			DepotCusOutturn outturn = outturns[0];

			AssertEquals(true, outturn.IsLCL);
			AssertEquals(containerNumber, outturn.C5_ContainerNumber);
			AssertEquals(oceanNumber, outturn.C5_MasterBill);
			AssertEquals(houseNumber, outturn.C5_HouseBill);
			AssertEquals("goods", outturn.C5_GoodsDescription);
			AssertEquals("marks", outturn.C5_MarksAndNumbers);
			AssertEquals(CMRPackageTypes.Codes.Package, outturn.C5_OuterPackUnits);
			AssertEquals(7, outturn.C5_OuterPacks);

			AssertNotNull(outturn.Parent);
			AssertEquals(typeof(CusSCADepotHouse), outturn.Parent.GetType());
		}

		public void TestNewFCLOutturn()
		{
			var outturnQuery = new ZQuery();
			outturnQuery.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.FullContainerLoad);
			outturnQuery.AddToFilter(CusOutturnSchema.C5_ContainerNumber, containerNumber);

			var header = Factory.New<CusOutturnHeader>();
			header.C6_VoyageNum = voyageNumber;
			header.C6_LloydsIMO = lloydsNumber;
			header.C6_OutturningPremiseID = ourPremiseID;
			Factory.Save();

			outturnQuery.AddToFilter(CusOutturnSchema.C5_C6, header.PK);

			DepotCusOutturn[] outturns = Factory.Load<DepotCusOutturn>(outturnQuery);
			AssertEquals("precondition", 0, outturns.Length);

			dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;
			dummyLine.goodsDescription = "goods";
			dummyLine.houseBillNumber = houseNumber;
			dummyLine.marksAndNumbers = "marks";
			dummyLine.oceanBillNumber = oceanNumber;
			dummyLine.packageType = CMRPackageTypes.Codes.Package;
			dummyLine.numberOfPackages = 7;

			processor.Process();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			outturns = factory2.Load<DepotCusOutturn>(outturnQuery);

			AssertEquals(1, outturns.Length);
			var outturn = outturns[0];

			AssertEquals(true, outturn.IsFCL);
			AssertEquals(containerNumber, outturn.C5_ContainerNumber);
			AssertEquals("Ocean Bill Number should be blank for FCL outturn", ZString.Empty, outturn.C5_MasterBill);
			AssertEquals("House Bill Number should be blank for FCX outturn", ZString.Empty, outturn.C5_HouseBill);
			AssertEquals("goods", outturn.C5_GoodsDescription);
			AssertEquals("marks", outturn.C5_MarksAndNumbers);
			AssertEquals(CMRPackageTypes.Codes.Package, outturn.C5_OuterPackUnits);
			AssertEquals(7, outturn.C5_OuterPacks);

			AssertNotNull(outturn.Parent);
			AssertEquals(typeof(CusSCADepotHouse), outturn.Parent.GetType());
		}

		public void TestNewFCXOutturn()
		{
			var outturnQuery = new ZQuery();
			outturnQuery.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills);
			outturnQuery.AddToFilter(CusOutturnSchema.C5_ContainerNumber, containerNumber);

			var header = Factory.New<CusOutturnHeader>();
			header.C6_VoyageNum = voyageNumber;
			header.C6_LloydsIMO = lloydsNumber;
			header.C6_OutturningPremiseID = ourPremiseID;
			Factory.Save();

			outturnQuery.AddToFilter(CusOutturnSchema.C5_C6, header.PK);

			DepotCusOutturn[] outturns = Factory.Load<DepotCusOutturn>(outturnQuery);
			AssertEquals("precondition", 0, outturns.Length);

			dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			dummyLine.containerNumber = containerNumber;
			dummyLine.goodsDescription = "goods";
			dummyLine.houseBillNumber = houseNumber;
			dummyLine.marksAndNumbers = "marks";
			dummyLine.oceanBillNumber = oceanNumber;
			dummyLine.packageType = CMRPackageTypes.Codes.Package;
			dummyLine.numberOfPackages = 7;

			processor.Process();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			outturns = factory2.Load<DepotCusOutturn>(outturnQuery);

			AssertEquals(1, outturns.Length);
			var outturn = outturns[0];

			AssertEquals(true, outturn.IsFCX);
			AssertEquals(containerNumber, outturn.C5_ContainerNumber);
			AssertEquals("Ocean Bill Number should be blank for FCX outturn", ZString.Empty, outturn.C5_MasterBill);
			AssertEquals("House Bill Number should be blank for FCX outturn", ZString.Empty, outturn.C5_HouseBill);
			AssertEquals("goods", outturn.C5_GoodsDescription);
			AssertEquals("marks", outturn.C5_MarksAndNumbers);
			AssertEquals(CMRPackageTypes.Codes.Package, outturn.C5_OuterPackUnits);
			AssertEquals(7, outturn.C5_OuterPacks);

			AssertNotNull(outturn.Parent);
			AssertEquals(typeof(CusSCADepotHouse), outturn.Parent.GetType());
		}

		#endregion

		#region Find Outturn

		public void TestFindOutturnFCL()
		{
			CusOutturnHeader header = GetOurOutturnHeader();

			DepotCusOutturn outturn1 = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoad, containerNumber, otherOceanNumber, otherHouseNumber);

			DepotCusOutturn outturn2 = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoad, otherContainerNumber, oceanNumber, houseNumber);
			DepotCusOutturn outturn3 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, containerNumber, oceanNumber, houseNumber);

			CusOutturnHeader header2 = Factory.New<CusOutturnHeader>();
			DepotCusOutturn outturn4 = GetOutturn(header2, CMRImportCargoTypes.Codes.FullContainerLoad, containerNumber, oceanNumber, houseNumber);

			dummyMessage.ourPremiseID = ourPremiseID;
			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			AssertProcessed(1);

			AssertEquals(outturn1, dummyMessage.objectsToLink[0]);
		}

		public void TestFindOutturnLCL()
		{
			CusOutturnHeader header = GetOurOutturnHeader();

			DepotCusOutturn outturn1 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, containerNumber, oceanNumber, houseNumber);

			DepotCusOutturn outturn2 = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoad, containerNumber, oceanNumber, houseNumber);
			DepotCusOutturn outturn3 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, otherContainerNumber, otherOceanNumber, otherHouseNumber);

			DepotCusOutturn outturn4 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, containerNumber, otherOceanNumber, otherHouseNumber);
			DepotCusOutturn outturn5 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, otherContainerNumber, oceanNumber, otherHouseNumber);
			DepotCusOutturn outturn6 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, otherContainerNumber, otherOceanNumber, houseNumber);

			DepotCusOutturn outturn7 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, containerNumber, oceanNumber, otherHouseNumber);
			DepotCusOutturn outturn8 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, otherContainerNumber, oceanNumber, houseNumber);
			DepotCusOutturn outturn9 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, containerNumber, otherOceanNumber, houseNumber);

			CusOutturnHeader header2 = Factory.New<CusOutturnHeader>();

			DepotCusOutturn outturn10 = GetOutturn(header2, CMRImportCargoTypes.Codes.LessThanContainerLoad, containerNumber, oceanNumber, houseNumber);

			dummyMessage.ourPremiseID = ourPremiseID;
			dummyLine.containerMode = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			dummyLine.containerNumber = containerNumber;
			dummyLine.oceanBillNumber = oceanNumber;
			dummyLine.houseBillNumber = houseNumber;

			AssertProcessed(1);

			AssertEquals(outturn1, dummyMessage.objectsToLink[0]);
		}

		public void TestFindOutturnFCX()
		{
			CusOutturnHeader header = GetOurOutturnHeader();

			DepotCusOutturn outturn1 = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills, containerNumber, otherOceanNumber, otherHouseNumber);

			DepotCusOutturn outturn2 = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills, otherContainerNumber, oceanNumber, houseNumber);
			DepotCusOutturn outturn3 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, containerNumber, oceanNumber, houseNumber);

			CusOutturnHeader header2 = Factory.New<CusOutturnHeader>();
			DepotCusOutturn outturn4 = GetOutturn(header2, CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills, containerNumber, oceanNumber, houseNumber);

			dummyMessage.ourPremiseID = ourPremiseID;
			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			dummyLine.containerNumber = containerNumber;

			AssertProcessed(1);

			AssertEquals(outturn1, dummyMessage.objectsToLink[0]);
		}

		public void TestFindOutturnBLK()
		{
			CusOutturnHeader header = GetOurOutturnHeader();

			DepotCusOutturn outturn1 = GetOutturn(header, CMRImportCargoTypes.Codes.Bulk, otherContainerNumber, oceanNumber, houseNumber);

			DepotCusOutturn outturn2 = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoad, otherContainerNumber, oceanNumber, houseNumber);
			DepotCusOutturn outturn3 = GetOutturn(header, CMRImportCargoTypes.Codes.Bulk, otherContainerNumber, otherOceanNumber, otherHouseNumber);

			DepotCusOutturn outturn4 = GetOutturn(header, CMRImportCargoTypes.Codes.Bulk, containerNumber, otherOceanNumber, otherHouseNumber);
			DepotCusOutturn outturn5 = GetOutturn(header, CMRImportCargoTypes.Codes.Bulk, otherContainerNumber, oceanNumber, otherHouseNumber);
			DepotCusOutturn outturn6 = GetOutturn(header, CMRImportCargoTypes.Codes.Bulk, otherContainerNumber, otherOceanNumber, houseNumber);

			DepotCusOutturn outturn7 = GetOutturn(header, CMRImportCargoTypes.Codes.Bulk, containerNumber, oceanNumber, otherHouseNumber);
			DepotCusOutturn outturn8 = GetOutturn(header, CMRImportCargoTypes.Codes.Bulk, containerNumber, otherOceanNumber, houseNumber);

			CusOutturnHeader header2 = Factory.New<CusOutturnHeader>();

			DepotCusOutturn outturn9 = GetOutturn(header2, CMRImportCargoTypes.Codes.Bulk, otherContainerNumber, oceanNumber, houseNumber);

			dummyMessage.ourPremiseID = ourPremiseID;
			dummyLine.containerMode = CMRImportCargoTypes.Codes.Bulk;
			dummyLine.oceanBillNumber = oceanNumber;
			dummyLine.houseBillNumber = houseNumber;

			AssertProcessed(1);

			AssertEquals(outturn1, dummyMessage.objectsToLink[0]);
		}

		public void TestFindOutturnBB()
		{
			CusOutturnHeader header = GetOurOutturnHeader();

			DepotCusOutturn outturn1 = GetOutturn(header, CMRImportCargoTypes.Codes.BreakBulk, otherContainerNumber, oceanNumber, houseNumber);

			DepotCusOutturn outturn2 = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoad, otherContainerNumber, oceanNumber, houseNumber);
			DepotCusOutturn outturn3 = GetOutturn(header, CMRImportCargoTypes.Codes.BreakBulk, otherContainerNumber, otherOceanNumber, otherHouseNumber);

			DepotCusOutturn outturn4 = GetOutturn(header, CMRImportCargoTypes.Codes.BreakBulk, containerNumber, otherOceanNumber, otherHouseNumber);
			DepotCusOutturn outturn5 = GetOutturn(header, CMRImportCargoTypes.Codes.BreakBulk, otherContainerNumber, oceanNumber, otherHouseNumber);
			DepotCusOutturn outturn6 = GetOutturn(header, CMRImportCargoTypes.Codes.BreakBulk, otherContainerNumber, otherOceanNumber, houseNumber);

			DepotCusOutturn outturn7 = GetOutturn(header, CMRImportCargoTypes.Codes.BreakBulk, containerNumber, oceanNumber, otherHouseNumber);
			DepotCusOutturn outturn8 = GetOutturn(header, CMRImportCargoTypes.Codes.BreakBulk, containerNumber, otherOceanNumber, houseNumber);

			CusOutturnHeader header2 = Factory.New<CusOutturnHeader>();

			DepotCusOutturn outturn9 = GetOutturn(header2, CMRImportCargoTypes.Codes.BreakBulk, otherContainerNumber, oceanNumber, houseNumber);

			dummyMessage.ourPremiseID = ourPremiseID;
			dummyLine.containerMode = CMRImportCargoTypes.Codes.BreakBulk;
			dummyLine.oceanBillNumber = oceanNumber;
			dummyLine.houseBillNumber = houseNumber;

			AssertProcessed(1);

			AssertEquals(outturn1, dummyMessage.objectsToLink[0]);
		}

		#endregion

		#region Find Header

		[ExpectNoExceptions()]
		public void TestFindHeader()
		{
			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			CusOutturnHeader header1 = GetOutturnHeader(lloydsNumber, voyageNumber, ourPremiseID);

			CusOutturnHeader header2 = GetOutturnHeader(otherLloydsNumber, voyageNumber, ourPremiseID);
			CusOutturnHeader header3 = GetOutturnHeader(lloydsNumber, otherVoyageNumber, ourPremiseID);
			CusOutturnHeader header4 = GetOutturnHeader(lloydsNumber, voyageNumber, otherPremiseID);

			CusOutturnHeader header5 = GetOutturnHeader(otherLloydsNumber, otherVoyageNumber, otherPremiseID);

			AssertProcessed(1);
			DepotCusOutturn outturn = (DepotCusOutturn)dummyMessage.objectsToLink[0];
			AssertEquals(header1, outturn.Header);
		}

		public void TestFindHeaderDuplicates()
		{
			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			// You shouldn't be able to save these anyway...
			CusOutturnHeader header1 = GetOutturnHeader(lloydsNumber, voyageNumber, ourPremiseID);
			CusOutturnHeader header2 = GetOutturnHeader(lloydsNumber, voyageNumber, ourPremiseID);

			processor.Process();
			AssertEquals(string.Format("More than one header found for criteria:  {0} {1} {2}", lloydsNumber, ourPremiseID, voyageNumber), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region New Underbond

		public void TestNewUnderbond()
		{
			CusOutturnHeader header = GetOurOutturnHeader();
			CusOutturn outturn = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoad, containerNumber, ZString.Empty, ZString.Empty);

			dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
			dummyMessage.ourPremiseID = ourPremiseID;
			dummyMessage.originPremiseID = otherPremiseID;
			dummyMessage.destinationPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;
			dummyLine.packageType = CMRPackageTypes.Codes.Package;
			dummyLine.numberOfPackages = 7;

			AssertEquals("precondition", 0, header.Underbonds.Count);

			processor.Process();

			AssertEquals(1, header.Underbonds.Count);
			CusUnderbond underbond = header.Underbonds[0];

			AssertEquals(header, underbond.Header);
			AssertEquals(1, underbond.Outturns.Count);
			AssertEquals(outturn, underbond.Outturns[0]);

			AssertNotNull(outturn.Parent);
			AssertEquals(outturn.Parent, underbond.LinkedObject);

			AssertEquals(ourPremiseID, underbond.C4_DestinationPremiseID);
			AssertEquals(otherPremiseID, underbond.C4_OriginPremiseID);
			AssertEquals(CMRPackageTypes.Codes.Package, underbond.C4_PackageType);
			AssertEquals(7, ((int)underbond.C4_PiecesManifested));
		}

		public void TestNewUnderbondOurIDOrigin()
		{
			CusOutturnHeader header = GetOurOutturnHeader();
			CusOutturn outturn = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoad, containerNumber, ZString.Empty, ZString.Empty);

			dummyMessage.messageType = CMRDepotMessageType.ExpectedArrival;
			dummyMessage.ourPremiseID = ourPremiseID;
			dummyMessage.originPremiseID = ourPremiseID;
			dummyMessage.destinationPremiseID = otherPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			AssertEquals("precondition", 0, header.Underbonds.Count);

			processor.Process();

			AssertEquals(1, header.Underbonds.Count);
			CusUnderbond underbond = header.Underbonds[0];

			AssertEquals(header, underbond.Header);
			AssertEquals(1, underbond.Outturns.Count);
			AssertEquals(outturn, underbond.Outturns[0]);

			AssertNotNull(outturn.Parent);
			AssertEquals(outturn.Parent, underbond.LinkedObject);

			AssertEquals(ourPremiseID, underbond.C4_DestinationPremiseID);
			AssertEquals(ZString.Empty, underbond.C4_OriginPremiseID);
		}

		#endregion

		#region Find Underbond

		[ExpectNoExceptions()]
		public void TestFindUnderbondOnOutturn()
		{
			CusOutturnHeader header = GetOurOutturnHeader();
			DepotCusOutturn outturn = GetOutturn(header, CMRImportCargoTypes.Codes.FullContainerLoad, containerNumber, ZString.Empty, ZString.Empty);
			CusUnderbond underbond = header.Underbonds.AddNew();

			underbond.Outturns.Add(outturn);

			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			processor.Process();

			Factory.Save();
		}

		public void TestFindUnderbondHeaderParentLink()
		{
			CusOutturnHeader header = GetOurOutturnHeader();
			CusUnderbond underbond = header.Underbonds.AddNew();

			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			CFSRecordLoaderAndCreator creator = new CFSRecordLoaderAndCreator(Factory);
			ICusUnderbondDependentCollectionParent parent = creator.GetOrCreateCFSRecord(dummyLine);

			underbond.LinkedObject = parent;

			AssertEquals("precondition", 0, underbond.Outturns.Count);
			AssertEquals("precondition", 0, header.Outturns.Count);
			AssertEquals("precondition", 1, header.Underbonds.Count);

			processor.Process();

			AssertEquals(1, header.Underbonds.Count);
			AssertEquals(1, header.Outturns.Count);
			AssertEquals(1, underbond.Outturns.Count);
			AssertEquals(parent, underbond.LinkedObject);
		}

		public void TestFindUnderbondHeaderParentLinkDouble()
		{
			CusOutturnHeader header = GetOurOutturnHeader();
			CusUnderbond underbond = header.Underbonds.AddNew();
			CusUnderbond underbond2 = header.Underbonds.AddNew();

			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			CFSRecordLoaderAndCreator creator = new CFSRecordLoaderAndCreator(Factory);
			ICusUnderbondDependentCollectionParent parent = creator.GetOrCreateCFSRecord(dummyLine);

			underbond.LinkedObject = parent;
			underbond2.LinkedObject = parent;

			processor.Process();

			AssertEquals(string.Format("More than one underbond found for header: {0}", header.PK.ToString()), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Link Message

		public void TestLinkMessage()
		{
			dummyMessage.ourPremiseID = ourPremiseID;

			dummyLine.containerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			dummyLine.containerNumber = containerNumber;

			AssertEquals("precondition", 0, dummyMessage.objectsToLink.Count);

			processor.Process();

			AssertEquals(1, dummyMessage.objectsToLink.Count);
			AssertEquals(typeof(DepotCusOutturn), dummyMessage.objectsToLink[0].GetType());

			processor.Process();

			AssertEquals(2, dummyMessage.objectsToLink.Count);
			AssertEquals(typeof(DepotCusOutturn), dummyMessage.objectsToLink[0].GetType());
			AssertEquals(typeof(DepotCusOutturn), dummyMessage.objectsToLink[1].GetType());
			AssertEquals(dummyMessage.objectsToLink[0], dummyMessage.objectsToLink[1]);
		}

		#endregion

		#region Test Air Cargo Does Not Get Processed

		public void TestAirCargoCARSTDoesNotGetProcessed()
		{
			string messageText = "UNH+000006+CUSRES:D:99B:UN'BGM+34:::CARST+4B86 G0I9 19AG:1+8'DTM+9:20060630150931012101:ZZZ'DTM+132:20060630:102'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'FTX+AHN+++CARGO REPORT SAC:YES'TDT+20+33++6+QF::3'LOC+12+AUSYD::6'LOC+4+9920A::95'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:A60001009/CM21::1'RFF+MWB:08191919192'RFF+HWB:HBL9292'UNT+15+000006'";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9920A";
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = messageText;

			int outturnCount = Factory.GetDatabaseCount(typeof(CusOutturn));
			CMRSeaDepotMessageProcessor processor = new CMRSeaDepotMessageProcessorForTest(message);
			processor.Process();
			AssertNull(message.EM_LinkedObject);
			AssertEquals(outturnCount, Factory.GetDatabaseCount(typeof(CusOutturn)));
		}

		public void TestOnlyProcessessSeaMessages()
		{
			dummyMessage.isSea = false;
			dummyMessage.ourPremiseID = ourPremiseID;
			AssertProcessed(0);

			dummyMessage.isSea = true;
			AssertProcessed(1);

			dummyMessage.isSea = false;
			AssertProcessed(1);
		}

		public void TestMessageProcessReScaleGrossWeightWhenItOverFlow()
		{
			var messageText = @"UNH+000001+CUSRES:D:99B:UN
BGM+961:::SEI+1C6B J232 II53:1++11
DTM+9:20230131170850850191:ZZZ
TDT+20+029S++11++++9495038::11
TDT+1++ROA
NAD+MR+FJA396R::95
NAD+VW+12089239609::95
RFF+ABO:O00004768/MEL26::1
DOC+1
PAC+++LCL:67:95
PAC+71++CT:185:95
RFF+MB:COSU8030257340
RFF+BH:CQSC23010003
RFF+AAQ:TGHU9697655
PCI+28+TOP MELBOURNE ITEM NO.?: QTY?: BARCOD:E MADE IN CHINA
FTX+AAA+++BAG KEYRINGS
MEA+AAE+G+KG:0000000001024.50
MEA+AAE+AAL+KG:0000000001024.50
MEA+AAE+ABJ+CU:0000000000002.79
NAD+CN++TOPLITE TRADING PTY LTD
DOC+1
PAC+++FCL:67:95
PAC+340++PK:185:95
RFF+MB:COSU8030257340
RFF+AAQ:TGHU9697655
FTX+AAA+++CLUTCH KITS
GIS+FFO:109:95
MEA+AAE+G+KG:0000000023933.00
NAD+CN++HEMISPHERE FREIGHT SERVICES PTY LTD
DOC+1
PAC+++LCL:67:95
PAC+2902++PF:185:95
RFF+MB:COSU8030257340
RFF+BH:AMIGL220544933A
RFF+AAQ:TGHU9697655
FTX+AAA+++GLASS BOTTLE
MEA+AAE+G+KG:0000001170957.00
MEA+AAE+AAL+KG:0000000000807.00
MEA+AAE+ABJ+CU:0000000000003.30
NAD+CN++TAYLOR AND SMITH DISTILLING CO
DOC+1
PAC+++LCL:67:95
PAC+9++PK:185:95
RFF+MB:COSU8030257340
RFF+BH:SMD2212023
RFF+AAQ:TGHU9697655
PCI+28+DESCRIPTION:SIZE:QTY
FTX+AAA+++RIGGING FITTINGS, POLYESTER ROPES, POLYESTER SLINGS, RATCHET STRAPS
MEA+AAE+G+KG:0000000003727.90
MEA+AAE+AAL+KG:0000000003727.90
MEA+AAE+ABJ+CU:0000000000008.12
NAD+CN++ROPE N CHAIN COMPANY P/L
DOC+1
PAC+++LCL:67:95
PAC+3++PF:185:95
RFF+MB:COSU8030257340
RFF+BH:HWMEL2200126
RFF+AAQ:TGHU9697655
PCI+28+N/M
FTX+AAA+++BAKING TRAY
MEA+AAE+G+KG:0000000002650.00
MEA+AAE+AAL+KG:0000000002650.00
MEA+AAE+ABJ+CU:0000000000007.43
NAD+CN++BAKERY TECHNOLOGY SOLUTIONS PTY LTD
UNT+236+000001
".Replace("\r\n", "'");

			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			var header = Factory.New<CusOutturnHeader>();
			header.C6_SendersMessageReference = "O00000007";
			header.C6_OutturningPremiseID = "";
			header.C6_LloydsIMO = "9495038";
			header.C6_VoyageNum = "029S";

			var outturn1 = GetOutturn(header, CMRImportCargoTypes.Codes.LessThanContainerLoad, "TGHU9697655", "COSU8030257340", "AMIGL220544933A");
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "9495038";
			vessel.RV_Code = "Seroja Tiga";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "029S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			voyage.GenerateSailings();

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_MasterBillNum = "COSU8030257340";
			consol.JK_IsCFS = true;
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "029S";
			consol.Transports.MostInterestingTransport.JW_Vessel = vessel.RV_Code;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TGHU9697655";
			container.JC_IsCFSRegistered = true;

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			var message = Factory.New<CMRSEIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			message.EM_EI = interchange.PK;
			message.EM_IsActive = true;
			message.EM_IsTestMessage = true;
			message.EM_SendWithMessageErrors = false;
			Factory.Save();
			var processor = new CMRSeaDepotMessageProcessorForTest(message);
			processor.Process();

			AssertNotNull("EM_LinkUniqueID", message.EM_LinkUniqueID);
			AssertEquals("Inserted Shipment JS_ActualWeight doesn't have data decimal(9,3) overflow", 1170.957m, consol.Shipments[0].JS_ActualWeight);
			AssertEquals("Inserted Shipment JS_ActualChargeable doesn't have data decimal(9,3) overflow", 1170.957m, consol.Shipments[0].JS_ActualChargeable);
			AssertEquals("Inserted Shipment JS_UnitOfWeight", "T", consol.Shipments[0].JS_UnitOfWeight);
			AssertEquals("Inserted Shipment JS_UnitOfVolume", "M3", consol.Shipments[0].JS_UnitOfVolume);
			AssertEquals("Inserted Shipment JS_ActualVolume", 3.3m, consol.Shipments[0].JS_ActualVolume);
		}

		#endregion

		#region Implementation

		void AssertProcessed(int objectsToLink)
		{
			processor.Process();
			AssertEquals(objectsToLink, dummyMessage.objectsToLink.Count);
		}

		CusOutturnHeader GetOurOutturnHeader()
		{
			return GetOutturnHeader(lloydsNumber, voyageNumber, ourPremiseID);
		}

		CusOutturnHeader GetOutturnHeader(ZString lloyds, ZString voyage, ZString premise)
		{
			CusOutturnHeader result = Factory.New<CusOutturnHeader>();
			result.C6_LloydsIMO = lloyds;
			result.C6_VoyageNum = voyage;
			result.C6_OutturningPremiseID = premise;
			return result;
		}

		DepotCusOutturn GetOutturn(CusOutturnHeader header, string cargoType, ZString containerNumber, ZString oceanNumber, ZString houseNumber)
		{
			DepotCusOutturn result;
			result = header.Outturns.AddNew();
			result.C5_CargoType = cargoType;
			result.C5_ContainerNumber = containerNumber;
			result.C5_MasterBill = oceanNumber;
			result.C5_HouseBill = houseNumber;
			return result;
		}

		RefVessel GetOurVessel()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = lloydsNumber;
			vessel.RV_Code = "WANG";
			return vessel;
		}

		ZString currentPremisesId;
		ZBool currentIsUnpackDepot;
		protected override void SetUp()
		{
			base.SetUp();

			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentPremisesId = currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID;
			currentIsUnpackDepot = currentCompany.OrgProxy.OH_IsUnpackDepot;
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = ourPremiseID;
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;

			dummyMessage = new DummyCMRDepotMessage(Factory);
			dummyMessage.lloydsNumber = lloydsNumber;
			dummyMessage.voyageNumber = voyageNumber;
			dummyMessage.isSea = true;

			dummyLine = new DummyCMRDepotMessageLine(dummyMessage);

			processor = new CMRSeaDepotMessageProcessor(dummyMessage);
		}

		protected override void TearDown()
		{
			base.TearDown();
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = currentPremisesId;
			currentCompany.OrgProxy.OH_IsUnpackDepot = currentIsUnpackDepot;
		}

		DummyCMRDepotMessage dummyMessage;
		DummyCMRDepotMessageLine dummyLine;
		CMRSeaDepotMessageProcessor processor;

		const string ourPremiseID = "1000";
		const string otherPremiseID = "2000";

		const string lloydsNumber = "7654321";
		const string voyageNumber = "987S";

		const string otherLloydsNumber = "5555555";
		const string otherVoyageNumber = "333N";

		const string containerNumber = "CONT7654321";
		const string houseNumber = "HOUSE100";
		const string oceanNumber = "OCEAN100";

		const string otherContainerNumber = "CONT1234567";
		const string otherHouseNumber = "HOUSE200";
		const string otherOceanNumber = "OCEAN200";

		#endregion
	}

	class CMRSeaDepotMessageProcessorForTest : CMRSeaDepotMessageProcessor
	{
		public CMRSeaDepotMessageProcessorForTest(ICMRDepotMessage message) : base(message)
		{
		}
	}
}
