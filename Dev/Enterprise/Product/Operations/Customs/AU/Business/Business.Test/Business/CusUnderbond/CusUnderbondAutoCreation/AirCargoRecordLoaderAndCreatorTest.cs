using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AirCargoRecordLoaderAndCreatorTest : TestCaseWithFactory
	{
		public void TestRegisterSyncEvent()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "40691102981";
			mawb.CM_FlightNo = "QF1";
			mawb.CM_ArrivalDate = new ZDateTime(2014, 10, 29);

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "ATL001437XXXXXXXXXXXX";
			hawb.CS_MessageReference = "S00001843";

			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2BDG H3H4 0F1F:1+8'
DTM+9:20051102140706724348:ZZZ'
DTM+132:20141030:102'
FTX+AHN+++CONSOLIDATED STATUS:DCLALLOWED'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+6901++6+5X::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:S00001843/BNE1::1'
RFF+MWB:40691102981'
RFF+HWB:ATL001437XXXXXXXXXXXX'
DOC+1'
PAC+0000003'
UNT+16+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			Factory.Save();

			hawb.Reload();

			var exportLog = hawb.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.DataExportCode).FirstOrDefault();
			AssertNotNull("Should be registered for publish the universal event to transit warehouse.", exportLog);

			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, StmALogSchema.Constants.Prefix);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, exportLog.PK);
			query.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);

			var pivot = NewFactory().LoadTop1<GenPivot>(query);
			AssertNotNull("Should be registered for publish the universal event to transit warehouse.", pivot);

			var ediMessage = Factory.Load<EDIMessage>(pivot.XX_Relation2ID);
			AssertEquals("Should create an UDM message.", ApplicationCodeList.Codes.UniversalDataMessaging, ediMessage.EM_ApplicationCode);
			AssertEquals("The message type should be internal for processing directly.", ReceiveTransmitList.Codes.Internal, ediMessage.EM_ReceiveTransmit);
		}

		[TestDate(2009, 01, 22)]
		public void TestCARSTUpdatesFlightDetailsIfAnExistingUnderbondNotExpectedOrAccepted()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165437621";
			mAWB.CM_FlightNo = "QF022";
			mAWB.CM_ArrivalDate = new ZDateTime(2009, 01, 19);
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_OriginPremiseID = "9920A";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_IsMoveFromDischarge = ZBool.True;

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20051026144456899275:ZZZ'
DTM+132:20090120:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:QFQF032/26OCT05/081652126630::1'
RFF+MWB:08165437621'
DOC+1'
PAC+0000002'
UNT+16+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			AssertEquals("QF032", mAWB.CM_FlightNo);
			AssertEquals(new ZDateTime(2009, 01, 20), mAWB.CM_ArrivalDate);
			AssertEquals("QF032", underbond.C4_FlightNo);
			AssertEquals(new ZDateTime(2009, 01, 20), underbond.C4_ArrivalDate);
		}

		[TestDate(2012, 03, 1)]
		public void TestCARSTDoesNotUpdateFlightDetailsWhenMAWBTooOld()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165437621";
			mAWB.CM_FlightNo = "QF022";
			mAWB.CM_ArrivalDate = new ZDateTime(2009, 01, 19);
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_OriginPremiseID = "9920A";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_IsMoveFromDischarge = ZBool.True;

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20051026144456899275:ZZZ'
DTM+132:20090120:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:QFQF032/26OCT05/081652126630::1'
RFF+MWB:08165437621'
DOC+1'
PAC+0000002'
UNT+16+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			AssertEquals("QF022", mAWB.CM_FlightNo);
			AssertEquals(new ZDateTime(2009, 01, 19), mAWB.CM_ArrivalDate);
			AssertEquals(ZString.Empty, underbond.C4_FlightNo);
			AssertEquals(ZDateTime.Empty, underbond.C4_ArrivalDate);
		}

		public void TestCARSTDoesNotUpdatesFlightDetailsIfAnExistingUnderbondApproved()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165437621";
			mAWB.CM_FlightNo = "QF022";
			mAWB.CM_ArrivalDate = new ZDateTime(2009, 01, 19);
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_OriginPremiseID = "9920A";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_IsMoveFromDischarge = ZBool.True;
			underbond.C4_FlightNo = mAWB.CM_FlightNo;
			underbond.C4_ArrivalDate = mAWB.CM_ArrivalDate;
			underbond.UnderbondStatus.Code = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20051026144456899275:ZZZ'
DTM+132:20090120:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:QFQF032/26OCT05/081652126630::1'
RFF+MWB:08165437621'
DOC+1'
PAC+0000002'
UNT+16+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			AssertEquals("QF022", mAWB.CM_FlightNo);
			AssertEquals(new ZDateTime(2009, 01, 19), mAWB.CM_ArrivalDate);
			AssertEquals("QF022", underbond.C4_FlightNo);
			AssertEquals(new ZDateTime(2009, 01, 19), underbond.C4_ArrivalDate);
		}

		public void TestCARSTDoesNotUpdatesFlightDetailsIfAnExistingUnderbondExpected()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165437621";
			mAWB.CM_FlightNo = "QF022";
			mAWB.CM_ArrivalDate = new ZDateTime(2009, 01, 19);
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_OriginPremiseID = "9920A";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_IsMoveFromDischarge = ZBool.True;
			underbond.C4_FlightNo = mAWB.CM_FlightNo;
			underbond.C4_ArrivalDate = mAWB.CM_ArrivalDate;
			underbond.UnderbondStatus.Code = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20051026144456899275:ZZZ'
DTM+132:20090120:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:QFQF032/26OCT05/081652126630::1'
RFF+MWB:08165437621'
DOC+1'
PAC+0000002'
UNT+16+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			AssertEquals("QF022", mAWB.CM_FlightNo);
			AssertEquals(new ZDateTime(2009, 01, 19), mAWB.CM_ArrivalDate);
			AssertEquals("QF022", underbond.C4_FlightNo);
			AssertEquals(new ZDateTime(2009, 01, 19), underbond.C4_ArrivalDate);
		}

		[TestDate(2010, 9, 1)]
		public void TestCARSTUpdatesFlightDetailsOnUnderbondToAnotherDomesticPort()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9913C";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165437621";
			mAWB.CM_FlightNo = "QF022";
			mAWB.CM_ArrivalDate = new ZDateTime(2010, 09, 01);
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_OriginPremiseID = "9919A";
			underbond.C4_DestinationPremiseID = "9920A";
			underbond.C4_FlightNo = "QF510";
			underbond.C4_ArrivalDate = new ZDateTime(2010, 09, 03);
			underbond.C4_RL_NKDischargePort = "AUMEL";
			underbond.C4_IsMoveFromDischarge = false;

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20100902134640332079:ZZZ'
DTM+132:20100902:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+218++6+SQ::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:QFQF032/26OCT05/081652126630::1'
RFF+MWB:08165437621'
DOC+1'
PAC+0000002'
UNT+16+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			AssertEquals("MAWB Flight no. should be updated", "SQ218", mAWB.CM_FlightNo);
			AssertEquals("MAWBFlight date should be updated", new ZDateTime(2010, 09, 02), mAWB.CM_ArrivalDate);
			AssertEquals("Underbond Flight no. should be updated", "SQ218", underbond.C4_FlightNo);
			AssertEquals("UnderbondFlight date should be updated", new ZDateTime(2010, 09, 02), underbond.C4_ArrivalDate);
			AssertEquals("Underbond discharge port should not be changed as Move from Discharge is not set to true", "AUMEL", underbond.C4_RL_NKDischargePort);
		}

		[TestDate(2012, 10, 1)]
		public void TestCARSTDoesNotUpdatesFlightDetailsWhenMAWBTooOLd()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9913C";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165437621";
			mAWB.CM_FlightNo = "QF022";
			mAWB.CM_ArrivalDate = new ZDateTime(2010, 09, 01);
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_OriginPremiseID = "9919A";
			underbond.C4_DestinationPremiseID = "9920A";
			underbond.C4_FlightNo = "QF510";
			underbond.C4_ArrivalDate = new ZDateTime(2010, 09, 03);
			underbond.C4_RL_NKDischargePort = "AUMEL";
			underbond.C4_IsMoveFromDischarge = false;

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20100902134640332079:ZZZ'
DTM+132:20100902:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+218++6+SQ::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:QFQF032/26OCT05/081652126630::1'
RFF+MWB:08165437621'
DOC+1'
PAC+0000002'
UNT+16+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			AssertEquals("MAWB Flight no. should NOT be be updated", "QF022", mAWB.CM_FlightNo);
			AssertEquals("MAWBFlight date should NOT be updated", new ZDateTime(2010, 09, 01), mAWB.CM_ArrivalDate);
			AssertEquals("Underbond Flight no. should NOT be updated", "QF510", underbond.C4_FlightNo);
			AssertEquals("UnderbondFlight date should NOT be updated", new ZDateTime(2010, 09, 03), underbond.C4_ArrivalDate);
			AssertEquals("Underbond discharge port should not be changed", "AUMEL", underbond.C4_RL_NKDischargePort);
		}

		public void TestSetMAWBInfo()
		{
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08148273048";
			Creator.SetMAWBInfo(mAWB, "QF123", new ZDateTime(2005, 6, 21), "08110984526", "AUSYD", "12345", message.EM_MessageNum);
			AssertEquals("MAWB number should never be updated", "08148273048", mAWB.CM_MAWB);
			AssertEquals("FlightNo", "QF123", mAWB.CM_FlightNo);
			AssertEquals("ArrivalDate", new ZDateTime(2005, 6, 21), mAWB.CM_ArrivalDate);
			AssertEquals("Port of Discharge", "AUSYD", mAWB.CM_RL_NKDischargePort);
			AssertEquals("CTO Premise ID", "12345", mAWB.DischargeCTOID);
		}

		public void TestMawbLevelUnderbondSentFromHAWBLevelCarst()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165437621";
			mAWB.CM_FlightNo = "QF022";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_OriginPremiseID = "8553P";
			underbond.C4_DestinationPremiseID = "A200K";
			underbond.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;

			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "48031034";
			hAWB.CS_MessageReference = "S00211181";
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+16BJ JG00 FBF6:1+8'
DTM+9:20060315121447384223:ZZZ'
DTM+132:20060315:102'
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
FTX+AHN+++IMPORT DECLARATIONS MATCHED:YES'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:YES'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++IMPORT DECLARATION PAID:NO'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+022++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+8553P::95'
NAD+MR+FGA664N::95'
NAD+UD+85003404091::95'
RFF+ABO:S00211181/SYD1::1'
RFF+MWB:08165437621'
RFF+HWB:48031034'
UNT+34+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			Factory.Save();
			AssertEquals(true, hAWB.Messages.Contains(message));
			AssertEquals(1, underbond.Messages.Count);
			AssertEquals(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, underbond.UnderbondStatus.Code);
		}

		[TestDate(2005, 10, 27)]
		public void TestNoDuplicationOfCARSTs()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "08164549800";
			underbond.C4_FlightNo = "QF088";

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9536D";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CMRUBMREQRMessage uBMMessage = Factory.New<CMRUBMREQRMessage>();
			uBMMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+150I HF4A 151F:1+32'
DTM+9:20051027114435385041:ZZZ'
DTM+132:20051027:102'
TDT+20+088++6+QF::3'
TDT+1++ROA'
LOC+5+9532M::95'
LOC+4+9536D::95'
NAD+MR+FGG393E::95'
NAD+UD+37005316307::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000002'
RFF+MWB:08164549800'
UNT+17+000001'".Replace("\r\n", "");
			uBMMessage.SetEM_LinkedObject();

			CMRCARSTMessage message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1JJ6 JJ3I 3F1F:1+8'
DTM+9:20051027115841763297:ZZZ'
DTM+132:20051027:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+088++6+QF::3'
LOC+12+AUMEL::6'
LOC+4+9536D::95'
NAD+MR+FGG393E::95'
NAD+UD+37005316307::95'
RFF+ABO:200510270175::1'
RFF+MWB:08164549800'
RFF+HWB:328819'
DOC+1'
PAC+0000002'
UNT+17+000001'".Replace("\r\n", "");
			message1.SetEM_LinkedObject();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+ICBE 6FCD 01F:1+8'
DTM+9:20051031120459367450:ZZZ'
DTM+132:20051027:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:NO'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:YES'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++IMPORT DECLARATION PAID:YES'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+088++6+QF::3'
LOC+12+AUMEL::6'
LOC+4+9536D::95'
NAD+MR+FGG393E::95'
NAD+UD+37005316307::95'
RFF+ABO:B00001460/1/MEL1::1'
RFF+MWB:08164549800'
RFF+HWB:328819'
DOC+1'
PAC+0000002'
UNT+28+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();

			var outturn = Factory.LoadTop1<CusOutturn>(new ZQuery(CusOutturnSchema.C5_HouseBill, "328819"));
			AssertEquals(2, outturn.Messages.Count);
			AssertEquals(2, outturn.C5_PackagesOutturned);
			AssertEquals("Outturn result should have defaulted to nill", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);
			AssertEquals(true, outturn.Messages.Contains(Message));
		}

		[TestDate(2006, 01, 14)]
		public void TestStandAloneOutturnForPartShipments()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "P026P";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusUnderbond[] underbonds = Factory.Load<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_MAWB, "25771563030"));
			int initialUnderbonds = underbonds.Length;

			CMRUBMREQRMessage uBMREQRMessage1 = Factory.New<CMRUBMREQRMessage>();
			uBMREQRMessage1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+402G DF4F 6CF6:1+32'
DTM+9:20060113152512156441:ZZZ'
DTM+132:20060113:102'
FTX+AAH+++FFK334E00000000696872'
TDT+20+001++6+OS::3'
TDT+1++AIR'
LOC+5+EF33J::95'
LOC+4+P026P::95'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:00000000696872::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000002'
RFF+MWB:25771563030'
UNT+19+000001'".Replace("\r\n", "");
			uBMREQRMessage1.SetEM_LinkedObject();
			Factory.Save();

			CMRUBMREQRMessage uBMREQRMessage2 = Factory.New<CMRUBMREQRMessage>();
			uBMREQRMessage2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+402G DF4F 6CF6:1+32'
DTM+9:20060113152512156441:ZZZ'
DTM+132:20060114:102'
FTX+AAH+++FFK334E00000000696872'
TDT+20+001++6+OS::3'
TDT+1++AIR'
LOC+5+EF33J::95'
LOC+4+P026P::95'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:00000000696872::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000002'
RFF+MWB:25771563030'
UNT+19+000001'".Replace("\r\n", "");
			uBMREQRMessage2.SetEM_LinkedObject();
			Factory.Save();

			underbonds = Factory.Load<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_MAWB, "25771563030"));
			AssertEquals("Part Ship carst should have created a seperate Outturn", initialUnderbonds + 2, underbonds.Length);
		}

		[TestDate(2005, 11, 01, 01, 01, 01)]
		public void TestCARSTForDifferentSendersRefDoesNotUpdateOldRecordAndTestAttachToNewMatchingHouse()
		{
			AUCustomsDataRegistry.Instance.AttachOrphanedCARSTsWhenCusHAWBCreated.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CusMAWB oldMAWB = Factory.New<CusMAWB>();
			oldMAWB.CM_MAWB = "12345";
			CusHAWB oldHAWB = oldMAWB.ChildBills.AddNew();
			oldHAWB.CS_HAWB = "OLD";
			oldHAWB.CS_MessageReference = "S00001843";

			CusMAWB mAWBforUpdate = Factory.New<CusMAWB>();
			mAWBforUpdate.CM_MAWB = "40691102981";
			CusHAWB hAWBforUpdate = mAWBforUpdate.ChildBills.AddNew();
			hAWBforUpdate.CS_HAWB = "001437";

			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2BDG H3H4 0F1F:1+8'
DTM+9:20051102140706724348:ZZZ'
DTM+132:20051030:102'
FTX+AHN+++CONSOLIDATED STATUS:DCLALLOWED'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+6901++6+5X::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:S00001843/BNE1::1'
RFF+MWB:40691102981'
RFF+HWB:ATL001437XXXXXXXXXXXX'
DOC+1'
PAC+0000003'
UNT+16+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();
			Factory.Save();

			AssertEquals("Old HAWB should not have been updated", "0", oldHAWB.CS_PiecesManifested.ToString());
			AssertEquals("Old HAWB should not have been updated", "OLD", oldHAWB.CS_HAWB);
			AssertEquals(false, oldHAWB.Messages.Contains(Message));

			AssertEquals("New MAWB should have been updated", "5X6901", mAWBforUpdate.CM_FlightNo);
			Assert("ATL001437 doesn't exist, so attached to CusMAWB", mAWBforUpdate.Messages.Contains(Message));

			var newHAWB = mAWBforUpdate.ChildBills.AddNew();
			newHAWB.CS_HAWB = "ATL001437XXXXXXXXXXXX";
			Factory.Save();
			AssertEquals("One message attached", 1, newHAWB.Messages.Count);
			Assert("New house now contains message", newHAWB.Messages.Contains(Message));
			Assert("Detached from master", !mAWBforUpdate.Messages.Contains(Message));
			AssertEquals("Status derived", "DCL", newHAWB.CS_CustomsStatus);
		}

		[TestDate(2014, 11, 01, 01, 01, 01)]
		public void TestHouseCARSTChangeOtherMatchingMAWBs()
		{
			Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "40691102981";
			mawb.CM_FlightNo = "QF1";
			mawb.CM_ArrivalDate = new ZDateTime(2014, 10, 29);

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "ATL001437XXXXXXXXXXXX";
			hawb.CS_MessageReference = "S00001843";

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "40691102981";
			mawb2.CM_FlightNo = "QF1";
			mawb2.CM_ArrivalDate = new ZDateTime(2014, 10, 29);

			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_HAWB = "001437XXXXXXXXXXXY";
			hawb2.CS_MessageReference = "S00001842";

			var oldMawb = Factory.New<CusMAWB>();
			oldMawb.CM_MAWB = "40691102981";
			oldMawb.CM_FlightNo = "QF1";
			oldMawb.CM_ArrivalDate = new ZDateTime(2013, 10, 30);

			var hawb3 = oldMawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "001437XXXXXXXXXXXY";
			hawb3.CS_MessageReference = "S00001842";

			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2BDG H3H4 0F1F:1+8'
DTM+9:20051102140706724348:ZZZ'
DTM+132:20141030:102'
FTX+AHN+++CONSOLIDATED STATUS:DCLALLOWED'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+6901++6+5X::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:S00001843/BNE1::1'
RFF+MWB:40691102981'
RFF+HWB:ATL001437XXXXXXXXXXXX'
DOC+1'
PAC+0000003'
UNT+16+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();
			Factory.Save();

			mawb.Reload();
			mawb2.Reload();
			oldMawb.Reload();

			AssertEquals("5X6901", mawb.CM_FlightNo);
			AssertEquals("5X6901", mawb2.CM_FlightNo);
			AssertEquals("QF1", oldMawb.CM_FlightNo);
			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			filter.AddToFilter(StmALogSchema.SL_Reference, "Flight Details changed based on CARST " + Message.EM_MessageNum);
			StmALog[] logs1 = mawb.Logs.Find(filter);
			AssertEquals(1, logs1.Length);
			StmALog[] logs2 = mawb2.Logs.Find(filter);
			AssertEquals(1, logs2.Length);
			AssertEquals(new ZDateTime(2014, 10, 30), mawb.CM_ArrivalDate);
			AssertEquals(new ZDateTime(2014, 10, 30), mawb2.CM_ArrivalDate);
			AssertEquals(new ZDateTime(2013, 10, 30), oldMawb.CM_ArrivalDate);
		}

		public void TestCarstGetsProcessedAndAssigned()
		{
			var foundUnderbond = Factory.New<CusUnderbond>();
			foundUnderbond.C4_MAWB = "08165212663";
			foundUnderbond.C4_FlightNo = "QF032";

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20051026144456899275:ZZZ'
DTM+132:20051026:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:QFQF032/26OCT05/081652126630::1'
RFF+MWB:08165212663'
DOC+1'
PAC+0000002'
UNT+15+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();

			foundUnderbond = Factory.LoadTop1<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_MAWB, "08165212663"));
			AssertNotNull(foundUnderbond);
			AssertEquals(true, foundUnderbond.Messages.Contains(Message.PK));
		}

		public void TestAirCTOMAWBdoesNotGetMAWBWipedOut()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM18N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "CX171";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 10, 25);
			mAWB.CM_RL_NKDischargePort = "AUPER";
			CTOCusHAWB hAWB = (CTOCusHAWB)mAWB.AllChildBills.AddNew();
			hAWB.CS_HAWB = "16085216331";
			hAWB.CS_MessageReference = "M00000709";

			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+EGG3 H4F8 C65:1+8'
DTM+9:20051025062345669583:ZZZ'
DTM+132:20051025:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:YES'
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
TDT+20+171++6+CX::3'
LOC+12+AUPER::6'
LOC+4+EM18N::95'
NAD+MR+FGA376X::95'
NAD+UD+88081675701::95'
RFF+ABO:M00000709/PER1::1'
RFF+MWB:16085216331'
UNT+33+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();

			Factory.Save();

			ZQuery filter = new ZQuery(CusHAWBSchema.CS_MessageReference, "M00000709");

			CTOCusHAWB[] foundHAWBs = Factory.Load<CTOCusHAWB>(filter);

			AssertEquals(1, foundHAWBs.Length);
			AssertEquals("16085216331", foundHAWBs[0].CS_HAWB);
		}

		public void TestSetHAWBInfo()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = cusMAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "2352";
			Creator.SetHAWBInfoExtend(hAWB, 50, "A023854");
			AssertEquals("HAWB", "A023854", hAWB.CS_TranshipmentEntryNum);
			AssertEquals("PiecesManifested", (short)50, hAWB.CS_PiecesManifested);
		}

		#region Implementation

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

		AirCargoRecordLoaderAndCreatorForTest fCreator;
		AirCargoRecordLoaderAndCreatorForTest Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new AirCargoRecordLoaderAndCreatorForTest(Factory);
				}
				return fCreator;
			}
		}

		sealed class AirCargoRecordLoaderAndCreatorForTest : AirCargoRecordLoaderAndCreator
		{
			public AirCargoRecordLoaderAndCreatorForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public void SetHAWBInfoExtend(CusHAWBBase hawb, short numberOfPackages, ZString transhipmentNumber)
			{
				SetHAWBInfo(hawb, numberOfPackages, transhipmentNumber);
			}
		}

		#endregion
	}
}
