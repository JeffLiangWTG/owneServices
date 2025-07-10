using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class AirCTOCARSTBusinessObjectLoaderOrCreatorTestCase : TestCaseWithFactory
	{
		#region Auto Underbond Tests
		public void TestMAWBLevelUnderbondRequestWhenNotPort()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9920A";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "UL777";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 12, 13);
			mAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			CTOCusHAWB hAWB = (CTOCusHAWB)mAWB.AllChildBills.AddNew();
			hAWB.CS_HAWB = "77765465131";
			hAWB.CS_MessageReference = "M00000059";
			hAWB.CS_RL_NKLoadPort = "JPTYO";
			hAWB.CS_RL_NKOrigin = "JPTYO";
			hAWB.CS_RL_NKDestination = "AUMEL";
			hAWB.CS_PiecesManifested = 50;
			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)hAWB).Underbonds.AddNew();
			CusUnderbond underbond2 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)hAWB).Underbonds.AddNew();
			hAWB.AllUnderbonds.Load();
			underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			underbond2.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			CusUnderbondUBMREQManager manager = new CusUnderbondUBMREQManager(underbond2);
			EDIMessage[] newMessage = manager.GenerateOriginalMessages(underbond2);
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'
BGM+34:::CARST+B5C7 DCH4 G1F:1+8'
DTM+9:20051214151330715953:ZZZ'
DTM+132:20051213:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:NO'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:HRM COMPLIANT'
FTX+AHN+++ACS EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+777++6+UL::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:M00000059/CMT1::1'
RFF+MWB:77765465131'
UNT+35+000002'".Replace("\r\n", "");
			message.SetEM_LinkedObject();

			AssertEquals(true, hAWB.Messages.Contains(message));
			AssertEquals(2, hAWB.AllUnderbonds.Count);
			AssertEquals(1, hAWB.AllUnderbonds[0].Messages.Count);
			AssertEquals(1, hAWB.AllUnderbonds[1].Messages.Count);
		}

		public void TestMAWBLevelUnderbondRequestWhenIsPort()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9920A";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "UL444";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 12, 14);
			mAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			CTOCusHAWB hAWB = (CTOCusHAWB)mAWB.AllChildBills.AddNew();
			hAWB.CS_HAWB = "44498798464";
			hAWB.CS_MessageReference = "M00000060";
			hAWB.CS_RL_NKLoadPort = "USLAX";
			hAWB.CS_RL_NKOrigin = "USLAX";
			hAWB.CS_RL_NKDestination = "AUSYD";
			hAWB.CS_PiecesManifested = 50;
			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)hAWB).Underbonds.AddNew();
			CusUnderbond underbond2 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)hAWB).Underbonds.AddNew();
			hAWB.AllUnderbonds.Load();
			underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			underbond2.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			CusUnderbondUBMREQManager manager = new CusUnderbondUBMREQManager(underbond2);
			EDIMessage[] newMessage = manager.GenerateOriginalMessages(underbond2);
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'
BGM+34:::CARST+2E48 7G94 EIBF:1+8'
DTM+9:20051215155318223016:ZZZ'
DTM+132:20051214:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+444++6+UL::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:M00000060/CMT1::1'
RFF+MWB:44498798464'
UNT+33+000002'".Replace("\r\n", "");
			message.SetEM_LinkedObject();

			AssertEquals(true, hAWB.Messages.Contains(message));
			AssertEquals(2, hAWB.AllUnderbonds.Count);
			AssertEquals(1, hAWB.AllUnderbonds[0].Messages.Count);
			AssertEquals(1, hAWB.AllUnderbonds[1].Messages.Count);
		}
		#endregion

		public void TestIsInterestedInCARST()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsAirCTO = true;
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(HAWBCMRCARSTMessage));
		}

		public void TestLoadOrCreateRecordForMessageCoreWhenNoRecord()
		{
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(HAWBCMRCARSTMessage)[0]);
		}

		public void TestLoadOrCreateRecordForMessageCoreWhenRecordExists()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			mAWB.CM_FlightNo = "QF270";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 7, 22);
			hAWB.CS_HAWB = "08112347775";
			Factory.Save();
			AssertEquals("Created Object Type", hAWB, Creator.LoadOrCreateRecordForMessageCore(HAWBCMRCARSTMessage)[0]);
		}

		[TestDate(2005, 7, 23)]
		public void TestLoadOrCreateRecordForMessageAutoSendHAWBUnderbond()
		{
			var auUnderbond = CreateUnderbondForMessageAutoSend("CMR", ZDate.Today.AddDays(1));
			var oldAUUnderbond = CreateUnderbondForMessageAutoSend("CMR", ZDate.Today.AddMonths(-13));
			var nonAUUnderbond = CreateUnderbondForMessageAutoSend(Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff, ZDate.Today.AddDays(1));

			Creator.LoadOrCreateRecordForMessageCore(HAWBCMRCARSTMessage);
			Factory.Save();

			AssertEquals(1, auUnderbond.Messages.Count);
			AssertEquals(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, auUnderbond.UnderbondStatus.Code);

			AssertEquals(0, oldAUUnderbond.Messages.Count);
			AssertNotEquals(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, oldAUUnderbond.UnderbondStatus.Code);

			AssertEquals(0, nonAUUnderbond.Messages.Count);
			AssertNotEquals(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, nonAUUnderbond.UnderbondStatus.Code);
		}

		public void TestLoadOrCreateRecordForMessageCoreWhenRecordExistsTranshipment()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			mAWB.CM_FlightNo = "QF270";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 7, 22);
			hAWB.CS_HAWB = "08112347775";
			Factory.Save();
			AssertEquals("Created Object Type", hAWB, Creator.LoadOrCreateRecordForMessageCore(HAWBCMRCARSTMessage)[0]);
			AssertEquals("AAAA4NNTA", hAWB.CS_TranshipmentEntryNum);
		}

		#region Implementation

		Customs.Business.CusUnderbond CreateUnderbondForMessageAutoSend(ZString mawbAppCode, ZDate mawbArrivalDate)
		{
			var mawb = Factory.New<CTOCusMAWB>();
			mawb.CM_FlightNo = "QF270";
			mawb.CM_ArrivalDate = mawbArrivalDate;
			mawb.CM_MAWB = "08112347775";
			mawb.CM_ApplicationCode = mawbAppCode;

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			message.EM_MessageText = "<<MSGNO PLACEHOLDER>>";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "001";
			hawb.Messages.Add(message);

			var underbond = ((ICusUnderbondDependentCollectionParent)hawb).Underbonds.AddNew();
			underbond.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;

			Factory.Save();

			return underbond;
		}

		AirCTOCARSTBusinessObjectLoaderOrCreator fCreator;
		AirCTOCARSTBusinessObjectLoaderOrCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new AirCTOCARSTBusinessObjectLoaderOrCreator();
				}
				return fCreator;
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
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4NNTA'
TDT+20+270++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+MWB:08112347775'
DOC+1'
PAC+0000200'
UNT+16+000001'".Replace("\r\n", "");
				}
				return fHAWBCMRCARSTMessage;
			}
		}

		#endregion
	}
}
