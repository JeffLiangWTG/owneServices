using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class StandAloneAirCargoCARSTBusinessObjectLoaderOrCreatorTest : TestCaseWithFactory
	{
		public void TestIsInterestedInCARST()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
			AssertEquals("IsInterestedInCARST", false, Creator.IsInterestedInCARST(HouseBillCMRCARSTMessage));
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(HAWBCMRCARSTMessage));
		}

		public void TestPiecesManifestedDoesNotGetDeleted()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08143636165";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_PiecesManifested = 250;
			hAWB.CS_HAWB = "830263711";
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+2DA7 0G5D G65:1+8'DTM+9:20050907111210105108:ZZZ'" +
				"DTM+132:20050907:102'FTX+AHN+++CONSOLIDATED STATUS:TRANSHIP'FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4GCRP'FTX+AHN+++CARGO REPORT SAC:N/A'" +
				"TDT+20+1002++6+QF::3'LOC+12+AUSYD::6'LOC+4+9163D::95'NAD+MR+AAA394E::95'NAD+UD+41000495269::95'RFF+ABO:A00000907/SYD1::1'RFF+MWB:08143636165'" +
				"RFF+HWB:830263711'UNT+16+000001'";

			Creator.LoadOrCreateRecordForMessageCore(message);
			AssertEquals("Pieces Manifest is not 0", (short)250, hAWB.CS_PiecesManifested);
		}

		public void TestPiecesManifestedDoesNotGetUpdatedWithGhostCARST()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08143636165";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_PiecesManifested = 0;
			hAWB.CS_HAWB = "830263711";
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
RFF+MWB:08143636165'
RFF+HWB:830263711'
DOC+1'
PAC+0000144'
UNT+16+000001'".Replace("\r\n", "");

			Creator.LoadOrCreateRecordForMessageCore(message);
			AssertEquals("Pieces Manifest is updated with valid CARST message", (short)144, hAWB.CS_PiecesManifested);

			var ghostMessage = Factory.New<CMRCARSTMessage>();
			ghostMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
RFF+ABO:3HAE AAJA 6F66::1'
RFF+MWB:08143636165'
RFF+HWB:830263711'
DOC+1'
PAC+0000075'
UNT+16+000001'".Replace("\r\n", "");

			Creator.LoadOrCreateRecordForMessageCore(ghostMessage);
			AssertEquals("Pieces Manifest should not be changed when the Ghost CARST message is processed", (short)144, hAWB.CS_PiecesManifested);
		}

		[TestDate(2005, 12, 2)]
		public void TestCARSTMatchesForCTOs()
		{
			CTOCusMAWB header = Factory.New<CTOCusMAWB>();
			header.CM_FlightNo = "OS001";
			header.CM_ArrivalDate = new ZDateTime(2005, 11, 30);

			CTOCusHAWB hAWB = (CTOCusHAWB)header.AllChildBills.AddNew();
			hAWB.CS_HAWB = "25762955723";
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+15CC DF1J 336F:1+8'
DTM+9:20051201162608864650:ZZZ'
DTM+132:20051130:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+001++6+OS::3'
LOC+12+AUSYD::6'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:615125::1'
RFF+MWB:25762955723'
DOC+1'
PAC+0000001'
UNT+15+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();

			Factory.Save();
			Assert(hAWB.Messages.Contains(message));

			AssertEquals("SUB", hAWB.CMRCargoStatus.Code);
		}

		public void TestUBMREQRLinksUpCorrectlyToUnderbond()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "17678053172";
			mAWB1.CM_FlightNo = "EK420";
			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB1).Underbonds.AddNew();
			underbond1.C4_SendersMessageReference = "U00000829";
			underbond1.C4_FlightNo = "EK420";
			underbond1.C4_OriginPremiseID = "EM18N";
			underbond1.C4_DestinationPremiseID = "EJ93A";
			Factory.Save();

			UBMMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+483J AE5D DF1F:1+32'
DTM+9:20051027091411651824:ZZZ'
DTM+132:20051026:102'
FTX+AAH+++FGA376XU00000829/PER1'
TDT+20+420++6+EK::3'
TDT+1++ROA'
LOC+5+EM18N::95'
LOC+4+EJ93A::95'
NAD+MR+FGA376X::95'
NAD+UD+88081675701::95'
RFF+ABO:U00000829/PER1::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000001'
RFF+MWB:17678053172'
UNT+19+000001'".Replace("\r\n", "");
			UBMMessage.SetEM_LinkedObject();

			CusUnderbond[] underbonds = (CusUnderbond[])Factory.Load(typeof(CusUnderbond), new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, "U00000829"));
			AssertEquals(1, underbonds.Length);
			AssertEquals(true, underbonds[0].Messages.Contains(UBMMessage.PK));
		}

		public void TestDestinationPortGetsMapped()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08112347775";
			mAWB.CM_RL_NKDischargePort = "";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "830263711";
			Factory.Save();

			Creator.LoadOrCreateRecordForMessageCore(HAWBCMRCARSTMessage);
			AssertNull(hAWB.Logs.MostRecentLogByEventTime(Events.StatusUpdated));
			AssertEquals("Port should be set", "AUSYD", mAWB.CM_RL_NKDischargePort);
		}

		public void TestDestinationIDGetsMapped()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08112347775";
			mAWB.DischargeCTOID = "";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "830263711";
			Factory.Save();

			Creator.LoadOrCreateRecordForMessageCore(HAWBCMRCARSTMessage);
			AssertEquals("Destination ID should be set", "9914N", mAWB.DischargeCTOID);
		}

		#region Implementation

		StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator fCreator;
		StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator();
				}
				return fCreator;
			}
		}

		CMRUBMREQRMessage fUBMMessage;
		CMRUBMREQRMessage UBMMessage
		{
			get
			{
				if (fUBMMessage == null)
				{
					fUBMMessage = Factory.New<CMRUBMREQRMessage>();
				}
				return fUBMMessage;
			}
		}

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
RFF+MB:OB0987123'
RFF+BH:HB4000'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
				}
				return fHouseBillCMRCARSTMessage;
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
RFF+MWB:08112347775'
RFF+HWB:AH3'
DOC+1'
PAC+0000200'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fHAWBCMRCARSTMessage;
			}
		}

		#endregion
	}
}
