using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class MultipleCusMAWBSWithSameMAWBNumberTest : TestCaseWithFactory
	{
		[TestDate(2012, 8, 31)]
		public void TestConsignmentCARSTisAttachedToCusHAWB()
		{
			var cARSTMessage = Factory.New<CMRCARSTMessage>();
			cARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20120827144456899275:ZZZ'
DTM+132:20120901:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:3102 G5G8 D8FC::1'
RFF+MWB:08155550010'
RFF+HWB:HAWB4'
DOC+1'
PAC+0000002'
UNT+165+000001'".Replace("\r\n", "");

			cARSTMessage.SetEM_LinkedObject();
			AssertEquals("Consignment CARST attached to CusHAWB", houseBill4.PK, cARSTMessage.EM_LinkUniqueID);
		}

		[TestDate(2012, 8, 31)]
		public void TestMasterCARSTIsAttachedToCorrectCusMAWB()
		{
			var cARSTMessage = Factory.New<CMRCARSTMessage>();
			cARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20120827144456899275:ZZZ'
DTM+132:20120901:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:3102 G5G8 D8FC::1'
RFF+MWB:08155550010'
DOC+1'
PAC+0000002'
UNT+15+000001'".Replace("\r\n", "");

			cARSTMessage.SetEM_LinkedObject();
			AssertEquals("Unrelated CusMAWB not affected", 0, unrelatedMAWB.Messages.Count);
			AssertEquals("Old CusMAWB not affected", 0, oldMAWB.Messages.Count);
			AssertEquals("Master CARST attached to prime CusMAWB", primeMAWB.PK, cARSTMessage.EM_LinkUniqueID);
		}

		[TestDate(2012, 8, 31)]
		public void TestSubMasterCARSTIsAttachedToCorrectCusHAWB()
		{
			var cARSTMessage = Factory.New<CMRCARSTMessage>();
			cARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20120827144456899275:ZZZ'
DTM+132:20120901:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:3102 G5G8 D8FC::1'
RFF+MWB:08155550010'
RFF+HWB:SUBMASTER2'
DOC+1'
PAC+0000002'
UNT+15+000001'".Replace("\r\n", "");

			cARSTMessage.SetEM_LinkedObject();
			AssertEquals("Unrelated CusMAWB not affected", 0, unrelatedMAWB.Messages.Count);
			AssertEquals("Old CusMAWB not affected", 0, oldMAWB.Messages.Count);
			AssertEquals("Sub-Master CARST attached to correct Sub-Master CusMAWB", subMaster2.PK, cARSTMessage.EM_LinkUniqueID);
			AssertEquals("Sub-Master CARST should be cloned and attached to the prime CusHAWB for the submaster", 1, subMaster2ConsolHouse.Messages.Count);
			Assert("Sub-Master CARST should be cloned and attached to the prime CusHAWB for the submaster", subMaster2ConsolHouse.Messages[0].EM_MessageText.Contains("BGM+34:::CARST+1615 EB9A 64G5:1+8'"));
		}

		[TestDate(2012, 8, 31)]
		public void TestExpectedArrivalAttachedWithExistingUnderbond()
		{
			AssertWithExistingUnderbond("EXPECTED CARGO ARRIVAL ADVICE", CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived);
		}

		[TestDate(2012, 8, 31)]
		public void TestExpectedArrivalAttachedWithoutExistingUnderbond()
		{
			AssertWithoutExistingUnderbond("EXPECTED CARGO ARRIVAL ADVICE", CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived);
		}

		[TestDate(2012, 8, 31)]
		public void TestUnderbondApprovalAttachedWithExistingUnderbond()
		{
			AssertWithExistingUnderbond("UNDERBOND APPROVAL", CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived);
		}

		[TestDate(2012, 8, 31)]
		public void TestUnderbondApprovalAttachedWithoutExistingUnderbond()
		{
			AssertWithoutExistingUnderbond("UNDERBOND APPROVAL", CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived);
		}

		[TestDate(2012, 8, 31)]
		public void TestExpectedArrivalRescindAttachedWithExistingUnderbond()
		{
			AssertWithExistingUnderbond("EXPECTED CARGO ARRIVAL RESCIND NTCE", CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived);
		}

		[TestDate(2012, 8, 31)]
		public void TestExpectedArrivalRescindAttachedWithoutExistingUnderbond()
		{
			AssertWithoutExistingUnderbond("EXPECTED CARGO ARRIVAL RESCIND NTCE", CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived);
		}

		[TestDate(2012, 8, 31)]
		public void TestUnderbondApprovalRescindAttachedWithExistingUnderbond()
		{
			AssertWithExistingUnderbond("UNDERBOND APPROVAL RESCIND NOTICE", CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived);
		}

		[TestDate(2012, 8, 31)]
		public void TestUnderbondApprovalRescindAttachedWithoutExistingUnderbond()
		{
			AssertWithoutExistingUnderbond("UNDERBOND APPROVAL RESCIND NOTICE", CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived);
		}

		void AssertWithExistingUnderbond(string uBMREQRText, string expectedStatus)
		{
			var underbond = primeMAWB.Underbonds.AddNew();
			underbond.C4_SendersMessageReference = "U00006666";
			underbond.C4_OriginPremiseID = "9920A";
			underbond.C4_DischargePremiseID = "9920A";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_IsMoveFromDischarge = ZBool.True;
			underbond.C4_FlightNo = flight;
			Factory.Save();

			var cMRUBMREQRMessage = Factory.New<CMRUBMREQRMessage>();
			cMRUBMREQRMessage.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3GFE 3A85 E414:1+32'
DTM+9:20120919115609282820:ZZZ'
DTM+132:20120901:102'
TDT+20+032++6+QF::3'
TDT+1++ROA'
LOC+5+9920A::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ANX:(?)'
RFF+ABO:U00006666/DAT1::001'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000001'
RFF+MWB:08155550010'
UNT+18+000002'".Replace("\r\n", "").Replace("(?)", uBMREQRText);

			cMRUBMREQRMessage.SetEM_LinkedObject();
			Factory.Save();// note factory save sets status via status calculator
			AssertEquals("Unrelated CusMAWB not affected, for " + uBMREQRText, 0, unrelatedMAWB.Messages.Count);
			AssertEquals("Unrelated CusMAWB not affected, for " + uBMREQRText, 0, unrelatedMAWB.AllUnderbonds.Count);
			AssertEquals("Old CusMAWB not affected, for " + uBMREQRText, 0, oldMAWB.Messages.Count);
			AssertEquals("Old CusMAWB not affected, for " + uBMREQRText, 0, oldMAWB.AllUnderbonds.Count);
			AssertEquals("Prime master underbond should have essage, for " + uBMREQRText, 1, underbond.Messages.Count);
			Assert("Prime master underbond should have cloned message, for " + uBMREQRText, underbond.Messages[0].EM_MessageText.Contains("BGM+961:::UBMREQR+3GFE 3A85 E414:1+32'"));
			AssertEquals("Primt master Underbond status, for " + uBMREQRText, expectedStatus, underbond.UnderbondStatus.Code);
			AssertEquals("Sub-master1 should have underbond created, for " + uBMREQRText, 1, subMaster1.Underbonds.Count);
			AssertEquals("Sub-master1 underbond should have cloned message, for " + uBMREQRText, 1, subMaster1.Underbonds[0].Messages.Count);
			Assert("Sub-master1 underbond should have cloned message, for " + uBMREQRText, subMaster1.Underbonds[0].Messages[0].EM_MessageText.Contains("BGM+961:::UBMREQR+3GFE 3A85 E414:1+32'"));
			AssertEquals("Sub-master1 Underbond status, for " + uBMREQRText, expectedStatus, subMaster1.Underbonds[0].UnderbondStatus.Code);
			AssertEquals("Sub-master2 should have underbond created, for " + uBMREQRText, 1, subMaster2.Underbonds.Count);
			AssertEquals("Sub-master2 underbond should have cloned message, for " + uBMREQRText, 1, subMaster2.Underbonds[0].Messages.Count);
			Assert("Sub-master2 underbond should have cloned message, for " + uBMREQRText, subMaster2.Underbonds[0].Messages[0].EM_MessageText.Contains("BGM+961:::UBMREQR+3GFE 3A85 E414:1+32'"));
			AssertEquals("Sub-master2 Underbond status, for " + uBMREQRText, expectedStatus, subMaster2.Underbonds[0].UnderbondStatus.Code);
		}

		void AssertWithoutExistingUnderbond(string uBMREQRText, string expectedStatus)
		{
			var cMRUBMREQRMessage = Factory.New<CMRUBMREQRMessage>();
			cMRUBMREQRMessage.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3GFE 3A85 E412:1+32'
DTM+9:20120919115609282820:ZZZ'
DTM+132:20120901:102'
TDT+20+032++6+QF::3'
TDT+1++ROA'
LOC+5+9920A::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ANX:(?)'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000001'
RFF+MWB:08155550010'
UNT+17+000002'".Replace("\r\n", "").Replace("(?)", uBMREQRText);

			cMRUBMREQRMessage.SetEM_LinkedObject();
			Factory.Save();// note factory save sets status via status calculator
			AssertEquals("Unrelated CusMAWB not affected, for " + uBMREQRText, 0, unrelatedMAWB.Messages.Count);
			AssertEquals("Unrelated CusMAWB not affected, for " + uBMREQRText, 0, unrelatedMAWB.AllUnderbonds.Count);
			AssertEquals("Old CusMAWB not affected, for " + uBMREQRText, 0, oldMAWB.Messages.Count);
			AssertEquals("Old CusMAWB not affected, for " + uBMREQRText, 0, oldMAWB.AllUnderbonds.Count);
			AssertEquals("Prime CusMAWB should have underbond created, for " + uBMREQRText, 1, primeMAWB.Underbonds.Count);
			AssertEquals("Prime CusMAWB underbond should have cloned message, for " + uBMREQRText, 1, primeMAWB.Underbonds[0].Messages.Count);
			Assert("Prime CusMAWB underbond should have cloned message, for " + uBMREQRText, primeMAWB.Underbonds[0].Messages[0].EM_MessageText.Contains("BGM+961:::UBMREQR+3GFE 3A85 E412:1+32'"));
			AssertEquals("Prime CusMAWB Underbond status, for " + uBMREQRText, expectedStatus, primeMAWB.Underbonds[0].UnderbondStatus.Code);
			AssertEquals("Sub-master1 should have underbond created, for " + uBMREQRText, 1, subMaster1.Underbonds.Count);
			AssertEquals("Sub-master1 underbond should have cloned message, for " + uBMREQRText, 1, subMaster1.Underbonds[0].Messages.Count);
			Assert("Sub-master1 underbond should have cloned message, for " + uBMREQRText, subMaster1.Underbonds[0].Messages[0].EM_MessageText.Contains("BGM+961:::UBMREQR+3GFE 3A85 E412:1+32'"));
			AssertEquals("Sub-master1 Underbond status, for " + uBMREQRText, expectedStatus, subMaster1.Underbonds[0].UnderbondStatus.Code);
			AssertEquals("Sub-master2 should have underbond created, for " + uBMREQRText, 1, subMaster2.Underbonds.Count);
			AssertEquals("Sub-master2 underbond should have cloned message, for " + uBMREQRText, 1, subMaster2.Underbonds[0].Messages.Count);
			Assert("Sub-master2 underbond should have cloned message, for " + uBMREQRText, subMaster2.Underbonds[0].Messages[0].EM_MessageText.Contains("BGM+961:::UBMREQR+3GFE 3A85 E412:1+32'"));
			AssertEquals("Sub-master2 Underbond status, for " + uBMREQRText, expectedStatus, subMaster2.Underbonds[0].UnderbondStatus.Code);
		}

		#region Implementation

		readonly ZDateTime arrivalDate = new ZDateTime(2012, 9, 1, 14, 0, 0);
		const string mAWBNumber = "08155550010";
		const string flight = "QF032";
		CusMAWB primeMAWB;
		CusHAWB subMaster2ConsolHouse;
		CusMAWB subMaster1;
		CusMAWB subMaster2;
		CusHAWB houseBill2;
		CusHAWB houseBill3;
		CusHAWB houseBill4;
		CusHAWB houseBill5;
		CusMAWB unrelatedMAWB;
		CusMAWB oldMAWB;

		protected override void SetUp()
		{
			base.SetUp();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = mAWBNumber;
			var transport = consol.Transports[0];
			transport.JW_ETA = arrivalDate;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_VoyageFlight = flight;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HAWB1";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "SUBMASTER1";
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "SUBMASTER2";
			primeMAWB = CusMAWB.CreateNew(consol);
			CusHAWB.CreateNew(primeMAWB, shipment1);
			CusHAWB.CreateNew(primeMAWB, shipment2);
			subMaster2ConsolHouse = CusHAWB.CreateNew(primeMAWB, shipment3);

			subMaster1 = Factory.New<CusMAWB>();
			subMaster1.CM_MAWB = mAWBNumber;
			subMaster1.CM_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			subMaster1.CM_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
			subMaster1.CM_ArrivalDate = arrivalDate;
			subMaster1.CM_MasterHouseBill = shipment2.JS_HouseBill;
			subMaster1.CM_FlightNo = transport.JW_VoyageFlight;
			houseBill2 = subMaster1.ChildBills.AddNew();
			houseBill2.CS_HAWB = "HAWB2";
			houseBill3 = subMaster1.ChildBills.AddNew();
			houseBill3.CS_HAWB = "HAWB3";

			subMaster2 = Factory.New<CusMAWB>();
			subMaster2.CM_MAWB = mAWBNumber;
			subMaster2.CM_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			subMaster2.CM_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
			subMaster2.CM_ArrivalDate = arrivalDate;
			subMaster2.CM_MasterHouseBill = shipment3.JS_HouseBill;
			subMaster2.CM_FlightNo = transport.JW_VoyageFlight;
			houseBill4 = subMaster1.ChildBills.AddNew();
			houseBill4.CS_HAWB = "HAWB4";
			houseBill5 = subMaster1.ChildBills.AddNew();
			houseBill5.CS_HAWB = "HAWB5";

			unrelatedMAWB = Factory.New<CusMAWB>();
			unrelatedMAWB.CM_MAWB = "08166660010";
			unrelatedMAWB.CM_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			unrelatedMAWB.CM_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
			unrelatedMAWB.CM_ArrivalDate = arrivalDate;
			unrelatedMAWB.CM_FlightNo = transport.JW_VoyageFlight;
			unrelatedMAWB.ChildBills.AddNew().CS_HAWB = "XXXXXX";

			oldMAWB = Factory.New<CusMAWB>();
			oldMAWB.CM_MAWB = mAWBNumber;
			oldMAWB.CM_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			oldMAWB.CM_RL_NKDischargePort = transport.JW_RL_NKDiscPort;
			oldMAWB.CM_ArrivalDate = arrivalDate.AddYears(-2);
			oldMAWB.CM_FlightNo = transport.JW_VoyageFlight;
			oldMAWB.ChildBills.AddNew().CS_HAWB = "ZZZZZ";

			Factory.Save();
		}

		#endregion
	}
}
