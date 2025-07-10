using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.AU.Declaration.Business.CMRCARSTMessage;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CMRCARSTMessageTestCase : TestCaseWithFactory
	{
		public void TestAllCreators()
		{
			Type[] expectedCreatorType = new Type[]
				{
						typeof(StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator),
						typeof(AirCTOCARSTBusinessObjectLoaderOrCreator),
						typeof(VoyageManifestCARSTBusinessObjectLoaderOrCreator),
						typeof(BrokerageCARSTBusinessObjectLoader) };

			ArrayList actualTypes = new ArrayList();
			foreach (CARSTBusinessObjectLoaderOrCreator creator in Message.AllCreators)
			{
				actualTypes.Add(creator.GetType());
			}
			foreach (Type expectedType in expectedCreatorType)
			{
				AssertEquals("Type " + expectedType + " included", true, actualTypes.Contains(expectedType));
			}
		}

		[TestDate(2006, 1, 23)]
		public void TestCheckUnderbondsAndSendOnCARST()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "23266036283";
			mAWB.CM_FlightNo = "MH123";
			mAWB.CM_ArrivalDate = new ZDateTime(2006, 01, 22);
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_RL_NKLoadPort = "FRCDG";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "059045617";
			hAWB.CS_RL_NKOrigin = "FRCDG";
			hAWB.CS_RL_NKDestination = "AUSYD";

			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			Factory.Save();

			CusUnderbondUBMREQManager manager = new CusUnderbondUBMREQManager(underbond1);
			EDIMessage[] message = manager.GenerateOriginalMessages(underbond1);
			Factory.Save();
			AssertEquals(underbond1.UnderbondStatus.Code, CMRBaseStatuses.Codes.AwaitingResponseToOriginal);
			AssertEquals("Underbond should have been sent", 1, underbond1.Messages.Count);

			CMRCARSTMessage cARST = Factory.New<CMRCARSTMessage>();
			cARST.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+744F 3G80 806:1+8'DTM+9:20060123084448948754:ZZZ'DTM+132:20060122:102'FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:N/A'FTX+AHN+++CARGO NOT A CONSOLIDATION:NO'FTX+AHN+++RELEASE PREMISE IN DESTINATION:NO'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+123++6+MH::3'LOC+12+AUSYD::6'LOC+4+8553P::95'NAD+MR+FGE743G::95'NAD+UD+28078835604::95'RFF+ABO:QFMH123/22JAN06/232660362830::1'RFF+MWB:23266036283'UNT+33+000001'";
			cARST.SetEM_LinkedObject();
			Assert(cARST.Logs.MostRecentLogByEventTime(Events.SubjectToUnderbondMovement) != null);
			AssertEquals(1, mAWB.Messages.Count);
			AssertEquals("Underbond should not have automatically been sent", 1, underbond1.Messages.Count);

			CMRCARSTMessage cARST2 = Factory.New<CMRCARSTMessage>();
			cARST2.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+2BHD 2ID5 A806:1+8'DTM+9:20060123084455082493:ZZZ'DTM+132:20060122:102'FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'FTX+AHN+++IAR ACS CLEARED:YES'FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:N/A'FTX+AHN+++CARGO NOT A CONSOLIDATION:NO'FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'FTX+AHN+++IAR AQIS CLEARED:YES'FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'FTX+AHN+++ACS EVALUATION COMPLETE:YES'FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'FTX+AHN+++IMPORT DECLARATION PAID:N/A'FTX+AHN+++CARGO REPORT SAC:NO'TDT+20+123++6+MH::3'LOC+12+AUSYD::6'LOC+4+EM10M::95'NAD+MR+FGE743G::95'NAD+UD+28078835604::95'RFF+ABO:QFMH123/22JAN06/232660362830::1'RFF+MWB:23266036283'UNT+33+000001'";
			cARST2.SetEM_LinkedObject();
			AssertEquals("Should not have sent because one has already been sent", 1, underbond1.Messages.Count);
		}

		#region Discharge port test

		public void TestDischargePortDoesNotGetWipedOut()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "26015186990";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "608116955";
			Factory.Save();

			Message.EM_MessageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3HJF 8ICG H965:1+8'
DTM+9:20051006090334307578:ZZZ'
DTM+132:20051005:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+9110++6+FJ::3'
LOC+12+AUSYD::6'
LOC+4+DK34C::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:A00003105/SYD1::1'
RFF+MWB:26015186990'
RFF+HWB:608116955'
UNT+15+000001'
".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			AssertEquals("AUSYD", mAWB.CM_RL_NKDischargePort);
		}

		#endregion

		#region Air Cargo CARST Tests

		public void TestCARSTForHAWBDoesntExclusivelyGoToCTOHAWB()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9920A";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CTOCusMAWB cTOMAWB = Factory.New<CTOCusMAWB>();
			cTOMAWB.CM_FlightNo = "HW777";
			cTOMAWB.CM_DateOfFirstArrival = new ZDateTime(2005, 12, 12);
			cTOMAWB.CM_RL_NKDischargePort = "AUSYD";
			cTOMAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			CTOCusHAWB cTOHAWB = (CTOCusHAWB)cTOMAWB.AllChildBills.AddNew();
			cTOHAWB.CS_HAWB = "77752342430";
			cTOHAWB.CS_MessageReference = "M00000056";

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "77752342430";
			mAWB.CM_FlightNo = "HW777";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 12, 12);
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "H23409SD93";
			hAWB.CS_MessageReference = "A00002062";
			Factory.Save();

			Message.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'
BGM+34:::CARST+2BJ5 4D26 C3BF:1+8'
DTM+9:20051213154959101813:ZZZ'
DTM+132:20051212:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:NO'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
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
TDT+20+777++6+HW::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:A00002062/CMT1::1'
RFF+MWB:77752342430'
RFF+HWB:H23409SD93'
UNT+34+000002'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();

			AssertEquals(true, hAWB.Messages.Contains(Message));
		}

		public void TestMasterUnderbondMovementIsUpdated()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08122220052";
			mAWB.CM_FlightNo = "QF52";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			underbond.C4_IsMoveFromDischarge = true;
			underbond.C4_FlightNo = "QF52";

			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:N/A'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4XMFL'
FTX+AHN+++ACS EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220052'
UNT+34+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDeclaration = factory2.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("QF9980", mAWB.CM_FlightNo);
			AssertEquals("Flight Number on underbond should be updated", "QF9980", underbond.C4_FlightNo);
			AssertEquals("Discharge port on underbond should be updated", "AUSYD", underbond.C4_RL_NKDischargePort);
			AssertEquals("Date on underbond should be updated", new ZDateTime(2005, 09, 08), underbond.C4_ArrivalDate);
		}

		public void TestWrongCoLoadMaster()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "21765966471";
			mAWB1.CM_MasterHouseBill = "30431702";
			mAWB1.CM_FlightNo = "TG993";
			mAWB1.CM_RL_NKDischargePort = "AUSYD";

			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_MAWB = "21765966471";
			mAWB2.CM_MasterHouseBill = "30221732";
			mAWB2.CM_FlightNo = "TG993";
			mAWB2.CM_RL_NKDischargePort = "AUSYD";

			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew();
			hAWB1.CS_HAWB = "1004757585";
			hAWB1.CS_MessageReference = "A00000550";

			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3DG6 JIDB EC15:1+8'
DTM+9:20051013174511199268:ZZZ'
DTM+132:20051013:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
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
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+995++6+TG::3'
LOC+12+AUSYD::6'
LOC+4+FC48P::95'
NAD+MR+FGF376K::95'
NAD+UD+31006604191::95'
RFF+ABO:A00000550/MEL1::1'
RFF+MWB:21765966471'
RFF+HWB:1004757585'
UNT+34+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			AssertEquals("Should be updated because the hawb sits on this", "TG995", mAWB1.CM_FlightNo);
			AssertEquals("Shouldn't have been updated because the hawb is on mawb1", "TG995", mAWB2.CM_FlightNo);
			AssertEquals("Should not have added any hawbs to MAWB2", 0, mAWB2.ChildBills.Count);
			AssertEquals("Should Still have only 1 hawb on MAWB1", 1, mAWB1.ChildBills.Count);
		}

		public void TestConsolidatedStatus()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "06391228045";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "940337348";

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4F5E B6FF D665:1+8'
DTM+9:20050913105419238093:ZZZ'
DTM+132:20050913:102'
FTX+AHN+++CONSOLIDATED STATUS:TRANSHIP'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4NNTA'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+144++6+SB::3'
LOC+12+AUSYD::6'
LOC+4+9163D::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:A00001104/SYD1::1'
RFF+MWB:06391228045'
RFF+HWB:940337348'
UNT+16+000001'".Replace("\r\n", "");

			AssertEquals("Consolidated Status", Message.ConsolidatedStatus, CMRConsolidatedCargoStatuses.ShortDescriptions.Tranship);
		}

		public void TestTranshipmentNumberHAWBlvl()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "06391228045";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "940337348";
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4F5E B6FF D665:1+8'
DTM+9:20050913105419238093:ZZZ'
DTM+132:20050913:102'
FTX+AHN+++CONSOLIDATED STATUS:TRANSHIP'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4NNTA'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+144++6+SB::3'
LOC+12+AUSYD::6'
LOC+4+9163D::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:A00001104/SYD1::1'
RFF+MWB:06391228045'
RFF+HWB:940337348'
UNT+16+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			AssertEquals("AAAA4NNTA", hAWB.CS_TranshipmentEntryNum);
		}

		public void TestTranshipmentNumberMAWBlvl()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9938N";
			GlbBranch.CurrentBranch.OrgProxy.OH_IsAirCTO = true;
			CTOCusMAWB cTOMAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB mAWB = (CTOCusHAWB)cTOMAWB.AllChildBills.AddNew();
			mAWB.CS_HAWB = "08122220052";
			cTOMAWB.CM_FlightNo = "QF9980";
			cTOMAWB.CM_ArrivalDate = new ZDateTime(2005, 9, 8);

			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:N/A'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4XMFL'
FTX+AHN+++ACS EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220052'
UNT+34+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			AssertEquals("AAAA4XMFL", mAWB.CS_TranshipmentEntryNum);
		}

		public void TestPartShipmentDoesNotOverwriteMAWBWithHAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08143485750";
			mAWB.CM_FlightNo = "QF52";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			underbond.C4_FlightNo = "QF21";
			underbond.UnderbondStatus.Code = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "917181329";
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+38D3 7BBC DBG5:1+8'
DTM+9:20051005122105260705:ZZZ'
DTM+132:20051004:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+21++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+DK34C::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:3C08 CIDB 61G5::1'
RFF+MWB:08143485750'
RFF+HWB:917181329'
UNT+15+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			AssertEquals("QF52", mAWB.CM_FlightNo);
		}

		public void TestCARSTStillUpdatesMAWB()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9938N";
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08143485750";
			mAWB.CM_FlightNo = "QF51";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "917150834";
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+F7C5 CI9E E65:1+8'
DTM+9:20051004151724895269:ZZZ'
DTM+132:20051004:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+52++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+DK34C::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:A00003050/SYD1::1'
RFF+MWB:08143485750'
RFF+HWB:917150834'
UNT+15+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			AssertEquals("QF52", mAWB.CM_FlightNo);
			AssertEquals("AUSYD", hAWB.CS_DischargePort);
		}

		#endregion

		#region Brokerage CARST Test

		public void TestBrokerageDoesNotAffectAirMAWBWithHAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08143485750";
			mAWB.CM_FlightNo = "QF52";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			underbond.C4_FlightNo = "QF21";
			underbond.UnderbondStatus.Code = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "917181329";

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_MasterBill = "08143485750";
			Declaration.JE_HouseBill = "917181329";
			Declaration.DoMerge();
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+38D3 7BBC DBG5:1+8'
DTM+9:20051005122105260705:ZZZ'
DTM+132:20051004:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+21++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+DK34C::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:3C08 CIDB 61G5::1'
RFF+ABT:AAAFHA6GH'
RFF+MWB:08143485750'
RFF+HWB:917181329'
UNT+15+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDeclaration = factory2.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("QF52", mAWB.CM_FlightNo);
			AssertEquals("Count of messages on dec", 1, reloadedDeclaration.CustomsEntryHeaders[0].Messages.Count);
		}

		public void TestBrokerageForSeaMasterAndHouse()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_MasterBill = "OB0987123";
			Declaration.JE_HouseBill = "HB4000";
			Declaration.JE_VoyageFlightNo = "4365";
			Declaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			Declaration.DoMerge();
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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

			Message.SetEM_LinkedObject();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDeclaration = factory2.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("Count of messages on dec", 1, reloadedDeclaration.CustomsEntryHeaders[0].Messages.Count);
		}

		public void TestBrokerageForSeaMaster()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_MasterBill = "OB0987123";
			Declaration.JE_VoyageFlightNo = "4365";
			Declaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			Declaration.DoMerge();
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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

			Message.SetEM_LinkedObject();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDeclaration = factory2.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("Count of messages on dec", 1, reloadedDeclaration.CustomsEntryHeaders[0].Messages.Count);
		}

		public void TestBrokerageForAirMaster()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_MasterBill = "08112347775";
			Declaration.DoMerge();
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08112347775";
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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

			Message.SetEM_LinkedObject();
			Factory.Save();
			AssertEquals("Count of messages on dec", 1, Declaration.CustomsEntryHeaders[0].Messages.Count);
		}

		public void TestBrokerageForAirMasterAndHouse()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_MasterBill = "08112347775";
			Declaration.JE_HouseBill = "AH3";
			Declaration.DoMerge();
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08112347775";
			Factory.Save();

			Message.EM_MessageText =
				@"UNH+000001+CUSRES:D:99B:UN'
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
UNT+16+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDeclaration = factory2.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("Count of messages on dec", 1, reloadedDeclaration.CustomsEntryHeaders[0].Messages.Count);
		}

		public void TestAdditionalInfoForStatusSectionOfReport()
		{
			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
RFF+BH:HBL111'
RFF+MB:OB0987123'
RFF+HWB:HAWB222'
RFF+MWB:MAWB333'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
UNT+33+000001'".Replace("\r\n", "");

			string expectedResult =
"***CONSOLIDATED CARGO STATUS: ***HELD***\r\n" +
"HBL / HAWB / OBL / MAWB / Container: HBL111 / HAWB222 / OB0987123 / MAWB333 / C001\r\n" +
"ACSDec/ACSCR/AQISDec/AQISCR: N/Y/N/Y\r\n";
			AssertEquals("AdditionalInfoForStatusSectionOfReport", expectedResult, Message.AdditionalInfoForStatusSectionOfReport());
		}

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

		#endregion

		public void TestGetFTXSegmentInfo()
		{
			Message.EM_MessageText = CARSTMessageWithMultipleLineACSAQISImpedimentDetails;
			AssertEquals("LAUGHING IS A GOOD EXERCISE", Message.GetFTXSegmentInfo("ACS/AQIS IMPEDIMENT DETAILS"));
			AssertEquals("HELD", Message.GetFTXSegmentInfo("CONSOLIDATED STATUS"));
		}

		public void TestGetFTXSegmentInfos()
		{
			Message.EM_MessageText = CARSTMessageWithMultipleLineACSAQISImpedimentDetails;
			ZString[] values = Message.GetFTXSegmentInfos("ACS/AQIS IMPEDIMENT DETAILS");
			AssertEquals("LAUGHING IS A GOOD EXERCISETHIS IS HOW WE LAUGHMEH MEH MEH", ZString.Join(values));

			values = Message.GetFTXSegmentInfos("CONSOLIDATED STATUS");
			AssertEquals(1, values.Length);
			AssertEquals("HELD", values[0]);
		}

		public void TestGetWrappedObjectCallForAutoUnderbonds()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08122220052";
			mAWB.CM_FlightNo = "QF52";
			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			CusUnderbond underbond2 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			CusUnderbond underbond3 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			CusUnderbond underbond4 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond1.C4_IsMoveFromDischarge = true;
			underbond1.C4_FlightNo = "QF52";
			underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			underbond2.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			underbond2.C4_MessageStatus = CMRBaseStatuses.Codes.OriginalRejected;
			CusUnderbondUBMREQManager manager = new CusUnderbondUBMREQManager(underbond3);
			EDIMessage[] newMessage = manager.GenerateOriginalMessages(underbond3);
			underbond3.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:N/A'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4XMFL'
FTX+AHN+++ACS EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220052'
UNT+34+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			Factory.Save();
			AssertEquals(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, underbond1.UnderbondStatus.Code);
			AssertEquals("Underbond should have been sent", 1, underbond1.Messages.Count);
			AssertEquals("Underbond should have been sent", 1, underbond2.Messages.Count);
			AssertEquals("Auto Underbond should NOT have been sent - We already have an original", 1, underbond3.Messages.Count);
			AssertEquals("Underbond should NOT have been sent", 0, underbond4.Messages.Count);
		}

		#region TestACSAQISImpedimentDetails

		public void TestACSAQISImpedimentDetails_NoSupplementaryInfo()
		{
			Message.EM_MessageText = CARSTMessageTextWithNoSupplementaryInfo;
			AssertEquals("Should be an empty array", 0, Message.ACSAQISImpedimentDetails.Length);
		}

		public void TestCARSTWithNoHAWBNumber()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "21765966471";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MessageReference = "A00000550";
			hAWB.CS_HAWB = "1004757585";
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3DG6 JIDB EC15:1+8'
DTM+9:20051013174511199268:ZZZ'
DTM+132:20051013:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
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
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+995++6+TG::3'
LOC+12+AUSYD::6'
LOC+4+FC48P::95'
NAD+MR+FGF376K::95'
NAD+UD+31006604191::95'
RFF+ABO:A00000550/MEL1::1'
RFF+MWB:21765966471'
RFF+HWB:1004757585'
UNT+34+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();

			Message.EM_MessageText = CARSTMessageTextWithNoSupplementaryInfo;
			AssertEquals("HAWB should be updated from senders reference", "TG995", mAWB.CM_FlightNo);
			AssertEquals("HAWB should be updated from senders reference", "1004757585", hAWB.CS_HAWB);

			ZQuery filter = new ZQuery(CusHAWBSchema.CS_HAWB, SQLComparisonOperator.Equal, "1004757585");
			CusHAWB[] hAWBs = (CusHAWB[])Factory.Load(typeof(CusHAWB), filter);
			AssertEquals("Should not have randomly created a hawb", 1, hAWBs.Length);
		}

		public void TestACSAQISImpedimentDetails_SingleLineACSAQISImpedimentDetails()
		{
			Message.EM_MessageText = CARSTMessageWithSingleLineACSAQISImpedimentDetails;
			AssertEquals("Should have one element", 1, Message.ACSAQISImpedimentDetails.Length);
			AssertEquals("Should have one element", "HI MUM", Message.ACSAQISImpedimentDetails[0]);
		}

		public void TestACSAQISImpedimentDetails_MultipleLineACSAQISImpedimentDetails()
		{
			Message.EM_MessageText = CARSTMessageWithMultipleLineACSAQISImpedimentDetails;
			AssertEquals("Should have 3 elements", 3, Message.ACSAQISImpedimentDetails.Length);
			AssertEquals("LAUGHING IS A GOOD EXERCISE", Message.ACSAQISImpedimentDetails[0]);
			AssertEquals("THIS IS HOW WE LAUGH", Message.ACSAQISImpedimentDetails[1]);
			AssertEquals("MEH MEH MEH", Message.ACSAQISImpedimentDetails[2]);
		}

		const string CARSTMessageTextWithNoSupplementaryInfo =
			"UNH+000001+CUSRES:D:99B:UN'" +
			"BGM+34:::CARST+2I9C 2C3F C215:1+8'" +
			"DTM+9:20051013090045839311:ZZZ'" +
			"DTM+132:20051013:102'" +
			"FTX+AHN+++CONSOLIDATED STATUS:HELD'" +
			"FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'" +
			"FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'" +
			"FTX+AHN+++IAR ACS CLEARED:YES'" +
			"FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'" +
			"FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'" +
			"FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'" +
			"FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'" +
			"FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'" +
			"FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'" +
			"FTX+AHN+++IAR AQIS CLEARED:YES'" +
			"FTX+AHN+++CARGO REPORT AQIS EVALUATED:NO'" +
			"FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'" +
			"FTX+AHN+++SUPPLEMENTARY INFORMATION:NO'" +
			"TDT+20+108++6+QF::3'" +
			"LOC+12+AUSYD::6'" +
			"LOC+4+9532M::95'" +
			"NAD+MR+FGH939C::95'" +
			"NAD+UD+83003926181::95'" +
			"RFF+ABO:A00046918/PRD1::1'" +
			"RFF+MWB:08143619354'";

		const string CARSTMessageWithSingleLineACSAQISImpedimentDetails =
			"UNH+000001+CUSRES:D:99B:UN'" +
			"BGM+34:::CARST+2I9C 2C3F C215:1+8'" +
			"DTM+9:20051013090045839311:ZZZ'" +
			"DTM+132:20051013:102'" +
			"FTX+AHN+++CONSOLIDATED STATUS:HELD'" +
			"FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'" +
			"FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'" +
			"FTX+AHN+++IAR ACS CLEARED:YES'" +
			"FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'" +
			"FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'" +
			"FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'" +
			"FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'" +
			"FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'" +
			"FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'" +
			"FTX+AHN+++IAR AQIS CLEARED:YES'" +
			"FTX+AHN+++CARGO REPORT AQIS EVALUATED:NO'" +
			"FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'" +
			"FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'" +
			"FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:HI MUM'" +
			"FTX+AHN+++ACS EVALUATION COMPLETE:YES'" +
			"FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:NO'" +
			"FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" +
			"FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION PAID:N/A'" +
			"FTX+AHN+++CARGO REPORT SAC:YES'" +
			"TDT+20+108++6+QF::3'" +
			"LOC+12+AUSYD::6'" +
			"LOC+4+9532M::95'" +
			"NAD+MR+FGH939C::95'" +
			"NAD+UD+83003926181::95'" +
			"RFF+ABO:A00046918/PRD1::1'" +
			"RFF+MWB:08143619354'";

		const string CARSTMessageWithMultipleLineACSAQISImpedimentDetails =
			"UNH+000001+CUSRES:D:99B:UN'" +
			"BGM+34:::CARST+2I9C 2C3F C215:1+8'" +
			"DTM+9:20051013090045839311:ZZZ'" +
			"DTM+132:20051013:102'" +
			"FTX+AHN+++CONSOLIDATED STATUS:HELD'" +
			"FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'" +
			"FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'" +
			"FTX+AHN+++IAR ACS CLEARED:YES'" +
			"FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'" +
			"FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'" +
			"FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'" +
			"FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'" +
			"FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'" +
			"FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'" +
			"FTX+AHN+++IAR AQIS CLEARED:YES'" +
			"FTX+AHN+++CARGO REPORT AQIS EVALUATED:NO'" +
			"FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'" +
			"FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'" +
			"FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:LAUGHING IS A GOOD EXERCISE'" +
			"FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:THIS IS HOW WE LAUGH'" +
			"FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:MEH MEH MEH'" +
			"FTX+AHN+++ACS EVALUATION COMPLETE:YES'" +
			"FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:NO'" +
			"FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" +
			"FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION PAID:N/A'" +
			"FTX+AHN+++CARGO REPORT SAC:YES'" +
			"TDT+20+108++6+QF::3'" +
			"LOC+12+AUSYD::6'" +
			"LOC+4+9532M::95'" +
			"NAD+MR+FGH939C::95'" +
			"NAD+UD+83003926181::95'" +
			"RFF+ABO:A00046918/PRD1::1'" +
			"RFF+MWB:08143619354'";

		#endregion

		#region TestAQISCargoReportEvaluationComplete

		public void TestAQISCargoReportEvaluationComplete()
		{
			Message.EM_MessageText = ConstructCARSTMessageWithAQISCargoReportEvaluationCompleteIndicator("YES");
			AssertEquals(FTXSegmentAnswerCode.Yes, Message.AQISCargoReportEvaluationComplete);

			fMessage = null;
			Message.EM_MessageText = ConstructCARSTMessageWithAQISCargoReportEvaluationCompleteIndicator("N/A");
			AssertEquals(FTXSegmentAnswerCode.NotAvailable, Message.AQISCargoReportEvaluationComplete);

			fMessage = null;
			Message.EM_MessageText = ConstructCARSTMessageWithAQISCargoReportEvaluationCompleteIndicator("NO");
			AssertEquals(FTXSegmentAnswerCode.No, Message.AQISCargoReportEvaluationComplete);
		}

		string ConstructCARSTMessageWithAQISCargoReportEvaluationCompleteIndicator(ZString aQISCargoReportEvaluationComplete)
		{
			return string.Format(CARSTMessageWithAQISCargoReportEvaluationCompleteIndicator, aQISCargoReportEvaluationComplete);
		}

		const string CARSTMessageWithAQISCargoReportEvaluationCompleteIndicator =
			"UNH+000001+CUSRES:D:99B:UN'" +
			"BGM+34:::CARST+2I9C 2C3F C215:1+8'" +
			"DTM+9:20051013090045839311:ZZZ'" +
			"DTM+132:20051013:102'" +
			"FTX+AHN+++CONSOLIDATED STATUS:HELD'" +
			"FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'" +
			"FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'" +
			"FTX+AHN+++IAR ACS CLEARED:YES'" +
			"FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'" +
			"FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'" +
			"FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'" +
			"FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'" +
			"FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'" +
			"FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'" +
			"FTX+AHN+++IAR AQIS CLEARED:YES'" +
			"FTX+AHN+++CARGO REPORT AQIS EVALUATED:NO'" +
			"FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'" +
			"FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'" +
			"FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:HI MUM'" +
			"FTX+AHN+++ACS EVALUATION COMPLETE:YES'" +
			"FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:{0}'" +
			"FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" +
			"FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'" +
			"FTX+AHN+++IMPORT DECLARATION PAID:N/A'" +
			"FTX+AHN+++CARGO REPORT SAC:YES'" +
			"TDT+20+108++6+QF::3'" +
			"LOC+12+AUSYD::6'" +
			"LOC+4+9532M::95'" +
			"NAD+MR+FGH939C::95'" +
			"NAD+UD+83003926181::95'" +
			"RFF+ABO:A00046918/PRD1::1'" +
			"RFF+MWB:08143619354'";

		#endregion

		CMRCARSTMessage fMessage;
		CMRCARSTMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = Factory.New<CMRCARSTMessage>();
				}
				return fMessage;
			}
		}
	}
}
