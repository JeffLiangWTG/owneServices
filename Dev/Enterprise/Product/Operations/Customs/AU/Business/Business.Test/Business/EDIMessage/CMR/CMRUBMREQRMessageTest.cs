using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRUBMREQRMessage))]
	sealed class CMRUBMREQRMessageTest : CMRCUSRESMessageTest
	{
		#region OurPremiseID

		public void TestOurPremiseID()
		{
			AssertEquals("Goes from destination for an expected arrival", "9914N", ExpectedCargoAdviceMessage.OurPremiseID);
			AssertEquals("Goes from origin for an approval", "9122P", MovementApprovedMessage.OurPremiseID);
			AssertEquals("blank for other status", ZString.Empty, OtherStatusMessage.OurPremiseID);
		}

		#endregion

		#region IsExpectedArrival

		public void TestIsExpectedArrival()
		{
			AssertEquals("Expected arrival", true, ExpectedCargoAdviceMessage.IsExpectedArrival);
			AssertEquals("Expected arrival rescinded", true, ExpectedCargoArrivalRescindMessage.IsExpectedArrival);
			AssertEquals("Approval", false, MovementApprovedMessage.IsExpectedArrival);
			AssertEquals("Approval rescinded", false, MovementApprovedRescindMessage.IsExpectedArrival);
			AssertEquals("other status", false, OtherStatusMessage.IsExpectedArrival);
		}

		#endregion

		#region IsApproval

		public void TestIsApproval()
		{
			AssertEquals("Approval", true, MovementApprovedMessage.IsApproval);
			AssertEquals("Approval rescinded", true, MovementApprovedRescindMessage.IsApproval);
			AssertEquals("Expected arrival", false, ExpectedCargoAdviceMessage.IsApproval);
			AssertEquals("Expected arrival rescinded", false, ExpectedCargoArrivalRescindMessage.IsApproval);
			AssertEquals("other status", false, OtherStatusMessage.IsApproval);
		}

		#endregion

		#region GetLinkedObjectHAWBUnderbond

		[TestDate(2005, 3, 8)]
		public void TestGetLinkedObjectHAWBUnderbond()
		{
			CusUnderbond expectedLink = MAWBUnderbond;
			Factory.Save();
			ApprovedAirUBMREQR.SetEM_LinkedObject();
			AssertEquals("Stand alone underbond so linked object is MAWBUnderbond", expectedLink, ApprovedAirUBMREQR.EM_LinkedObject);
		}

		[TestDate(2005, 3, 8)]
		public void TestGetLinkedObject_NewUnderbondOnAUMAWB()
		{
			var setupFactory = new BusinessObjectFactory();

			var org = GlbBranch.GetCurrentBranch(setupFactory).OrgProxy;
			org.MainAddress.LocalControlledPremisesID = "9920A";
			org.OH_IsUnpackDepot = true;

			var mawbAU = setupFactory.New<CusMAWB>();
			mawbAU.CM_MAWB = "08126284355";
			mawbAU.CM_FlightNo = "QF262";
			mawbAU.CM_ArrivalDate = new ZDateTime(2005, 3, 8);
			setupFactory.Save();

			ApprovedAirUBMREQR.SetEM_LinkedObject();
			var underbond = (CusUnderbond)ApprovedAirUBMREQR.EM_LinkedObject;
			AssertEquals("Creates a new Underbond against the existing AU MAWB", mawbAU.PK, underbond.MAWB.PK);
		}

		[TestDate(2005, 3, 8)]
		public void TestGetLinkedObject_StandAloneUnderbond()
		{
			var setupFactory = new BusinessObjectFactory();

			var org = GlbBranch.GetCurrentBranch(setupFactory).OrgProxy;
			org.MainAddress.LocalControlledPremisesID = "9920A";
			org.OH_IsUnpackDepot = true;

			var mawbNZ = setupFactory.New<CusMAWB>();
			mawbNZ.CM_ApplicationCode = "TSW";
			mawbNZ.CM_MAWB = "08126284355";
			mawbNZ.CM_FlightNo = "QF262";
			mawbNZ.CM_ArrivalDate = new ZDateTime(2005, 3, 8);
			setupFactory.Save();

			ApprovedAirUBMREQR.SetEM_LinkedObject();
			var underbond = (CusUnderbond)ApprovedAirUBMREQR.EM_LinkedObject;
			AssertEquals("Ignores NZ MAWB and creates a Stand alone underbond", null, underbond.MAWB);
			AssertEquals("MAWB number is stored in Underbond", "08126284355", underbond.C4_MAWB);
		}

		[TestDate(2005, 3, 8)]
		public void TestGetLinkedObject_ExistingNZUnderbondIsIgnored()
		{
			var setupFactory = new BusinessObjectFactory();
			var org = GlbBranch.GetCurrentBranch(setupFactory).OrgProxy;
			org.MainAddress.LocalControlledPremisesID = "9920A";
			org.OH_IsUnpackDepot = true;

			var nzMawb = setupFactory.New<CusMAWB>();
			nzMawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			nzMawb.CM_MAWB = "08126284355";
			nzMawb.CM_FlightNo = "QF262";
			nzMawb.CM_ArrivalDate = new ZDateTime(2005, 3, 8);
			var nzUnderbond = nzMawb.Underbonds.AddNew();
			nzUnderbond.C4_ApplicationCode = Customs.Business.CusUnderbondApplicationCodeList.Codes.NZTranshipmentRequest;
			nzUnderbond.C4_OriginPremiseID = "9920A";
			nzUnderbond.C4_DestinationPremiseID = "9932A";
			nzUnderbond.C4_SendersMessageReference = "08126284355";
			setupFactory.Save();

			ApprovedAirUBMREQR.SetEM_LinkedObject();
			var underbond = (CusUnderbond)ApprovedAirUBMREQR.EM_LinkedObject;
			AssertEquals("Ignores NZ underbond and creates a Stand alone underbond", Customs.Business.CusUnderbondApplicationCodeList.Codes.AUUnderbond, underbond.C4_ApplicationCode);
			AssertEquals("Ignores NZ underbond and creates a Stand alone underbond", null, underbond.MAWB);
		}

		#endregion

		#region ContainerNumber

		public void TestContainerNumber()
		{
			AssertEquals("ContainerNumber", "112233", ((ICusSCAContainerInformationProvider)ApprovedContainerUBMREQR).ContainerNumber);
		}

		#endregion

		#region LloydsNumber

		public void TestLloydslNumber()
		{
			AssertEquals("LloydsNumber", "8811924", ((ICusSCAContainerInformationProvider)ApprovedContainerUBMREQR).LloydsNumber);
		}

		#endregion

		#region CTOUnderbondAcceptanceWithNoUnderbond

		public void TestCTOUnderbondAcceptanceWithNoUnderbond()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM18N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "AD066";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 11, 7);
			mAWB.CM_RL_NKDischargePort = "AUPER";
			CTOCusHAWB hAWB = (CTOCusHAWB)mAWB.AllChildBills.AddNew();
			hAWB.CS_HAWB = "81700063081";
			hAWB.CS_MessageReference = "M00001330";
			Factory.Save();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+40H0 I5E3 2F6F:1+32'
DTM+9:20051108103504730190:ZZZ'
DTM+132:20051107:102'
FTX+AAH+++FFF369CU00000533/PER1'
TDT+20+066++6+AD::3'
TDT+1++ROA'
LOC+5+EM18N::95'
LOC+4+EK87K::95'
NAD+MR+FGA376X::95'
NAD+UD+88081675701::95'
RFF+ABO:U00000533/PER1::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000079'
RFF+MWB:81700063081'
UNT+19+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();

			AssertEquals("CusUnderbond", Message.EM_LinkTable);
			AssertEquals("Message should have been assigned to a newly created underbond", 1, mAWB.AllUnderbonds[0].Messages.Count);
		}

		#endregion

		#region ExpectedArrivalCreatesOutturnLines

		public void TestExpectedArrivalCreatesOutturnLines()
		{
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			currentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "P026P";
			currentBranch.OrgProxy.OH_IsUnpackDepot = true;

			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2FAA 18A2 46BF:1+32'
DTM+9:20051230053428776284:ZZZ'
DTM+132:20051230:102'
FTX+AAH+++FFK334E00000000675289'
TDT+20+601++6+OZ::3'
TDT+1++ROA'
LOC+5+EF33J::95'
LOC+4+P026P::95'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:00000000675289::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000001'
RFF+MWB:98823240932'
UNT+19+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();

			var underbond = Factory.LoadTop1<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_MAWB, "98823240932"));
			AssertNotNull(underbond);
			AssertEquals("OZ601", underbond.C4_FlightNo);
		}

		#endregion

		#region UBMREQRDoesNotSetDischargePortOnMAWB

		public void TestUBMREQRDoesNotSetDischargePortOnMAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "26015186990";
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_FlightNo = "FJ9110";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			underbond.C4_SendersMessageReference = "U00000061";
			underbond.C4_RL_NKDischargePort = "SGSIN";
			mAWB.AllUnderbonds.Load();
			Factory.Save();

			Message.EM_MessageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+219I J989 7965:1+32'
DTM+9:20051006090327339837:ZZZ'
DTM+132:20051005:102'
FTX+AAH+++AAA394EU00000061/SYD1'
TDT+20+9110++6+FJ::3'
TDT+1++ROA'
LOC+5+A011E::95'
LOC+4+DK34C::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:U00000061/SYD1::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000011'
RFF+MWB:26015186990'
UNT+19+000001'
".Replace("\r\n", "");

			Message.SetEM_LinkedObject();
			AssertEquals("AUSYD", mAWB.CM_RL_NKDischargePort);
			AssertEquals("DK34C", underbond.C4_DestinationPremiseID);
			AssertEquals("A011E", underbond.C4_OriginPremiseID);
			AssertEquals("SGSIN", underbond.C4_RL_NKDischargePort);
		}

		#endregion

		#region StandAloneUBMWhenRecordAlreadyExists

		public void TestStandAloneUBMWhenRecordAlreadyExists()
		{
			var currentCompanyProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			currentCompanyProxy.MainAddress.LocalControlledPremisesID = "9536D";
			currentCompanyProxy.OH_IsUnpackDepot = true;
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_SendersMessageReference = "08164549601";
			underbond.C4_MAWB = "08164549601";
			underbond.C4_FlightNo = "QF088";
			Factory.Save();

			CMRCARSTMessage cARSTMessage = Factory.New<CMRCARSTMessage>();
			cARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3F41 4HC5 JJB5:1+8'
DTM+9:20051016153748970517:ZZZ'
DTM+132:20051016:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+088++6+QF::3'
LOC+12+AUMEL::6'
LOC+4+9536D::95'
NAD+MR+FGG393E::95'
NAD+UD+37005316307::95'
RFF+ABO:200510160096::1'
RFF+MWB:08164549601'
RFF+HWB:102048'
DOC+1'
PAC+0000001'
UNT+17+000001'".Replace("\r\n", "");
			cARSTMessage.SetEM_LinkedObject();

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+18J5 D4FJ JB5:1+32'
DTM+9:20051016153746928782:ZZZ'
DTM+132:20051016:102'
FTX+AAH+++FFN973X200510160369'
TDT+20+088++6+QF::3'
TDT+1++ROA'
LOC+5+9532M::95'
LOC+4+9536D::95'
NAD+MR+FGG393E::95'
NAD+UD+37005316307::95'
RFF+ABO:200510160369::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000270'
RFF+MWB:08164549601'
UNT+19+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();

			CusMAWB[] mAWBs = (CusMAWB[])Factory.Load(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, SQLComparisonOperator.Contains, "08164549601"));
			AssertEquals(0, mAWBs.Length);

			CusUnderbond[] underbonds = (CusUnderbond[])Factory.Load(typeof(CusUnderbond), new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, SQLComparisonOperator.Contains, "08164549601"));
			AssertEquals(1, underbonds.Length);
			AssertEquals("9536D", underbond.C4_DestinationPremiseID);
			AssertEquals("QF088", underbond.C4_FlightNo);
			AssertEquals("102048", underbond.Outturns[0].C5_HouseBill);
			AssertEquals("Should have CARST assocated with outturn", 1, underbond.Outturns[0].Messages.Count);
		}

		#endregion

		#region StandAloneUBM

		[TestDate(2005, 10, 16)]
		public void TestStandAloneUBM()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9536D";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+18J5 D4FJ JB5:1+32'
DTM+9:20051016153746928782:ZZZ'
DTM+132:20051016:102'
FTX+AAH+++FFN973X200510160369'
TDT+20+088++6+QF::3'
TDT+1++ROA'
LOC+5+9532M::95'
LOC+4+9536D::95'
NAD+MR+FGG393E::95'
NAD+UD+37005316307::95'
RFF+ABO:200510160369::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000270'
RFF+MWB:08164549601'
UNT+19+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();

			CMRCARSTMessage cARSTMessage = Factory.New<CMRCARSTMessage>();
			cARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3F41 4HC5 JJB5:1+8'
DTM+9:20051016153748970517:ZZZ'
DTM+132:20051016:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+088++6+QF::3'
LOC+12+AUMEL::6'
LOC+4+9536D::95'
NAD+MR+FGG393E::95'
NAD+UD+37005316307::95'
RFF+ABO:200510160096::1'
RFF+MWB:08164549601'
RFF+HWB:102048'
DOC+1'
PAC+0000001'
UNT+17+000001'".Replace("\r\n", "");
			cARSTMessage.SetEM_LinkedObject();

			CusMAWB[] mAWBs = (CusMAWB[])Factory.Load(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, SQLComparisonOperator.Contains, "08164549601"));
			AssertEquals(0, mAWBs.Length);

			CusUnderbond[] underbonds = (CusUnderbond[])Factory.Load(typeof(CusUnderbond), new ZQuery(CusUnderbondSchema.C4_MAWB, SQLComparisonOperator.Contains, "08164549601"));
			AssertEquals(1, underbonds.Length);
			AssertEquals("9536D", underbonds[0].C4_DestinationPremiseID);
			AssertEquals("QF088", underbonds[0].C4_FlightNo);
			AssertEquals("102048", underbonds[0].Outturns[0].C5_HouseBill);
			AssertEquals("270", underbonds[0].C4_PiecesManifested.ToString());
			AssertEquals("Should have CARST message assocated with outturn", 1, underbonds[0].Outturns[0].Messages.Count);
		}

		#endregion

		#region URRTriggeredBy3rdPartyDoesNotUpdateOurUnderbondAtHAWBLevel

		public void TestURRTriggeredBy3rdPartyDoesNotUpdateOurUnderbondAtHAWBLevel()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM10M";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165620951";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "811883";

			CusUnderbond underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			underbond.C4_ParentID = mAWB.PK;
			mAWB.AllUnderbonds.Load();
			underbond.C4_OriginPremiseID = "8553P";
			underbond.C4_DestinationPremiseID = "EM10M";
			underbond.C4_ArrivalDate = new ZDateTime(2005, 11, 29);
			underbond.C4_FlightNo = "QF7554";

			Factory.Save();
			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2831 IFD9 7I6F:1+32'
DTM+9:20051129101346501878:ZZZ'
DTM+132:20051129:102'
TDT+20+7554++6+QF::3'
TDT+1++ROA'
LOC+5+EM10M::95'
LOC+4+8534B::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000005'
RFF+MWB:08165620951'
RFF+HWB:811883'
UNT+18+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();

			AssertEquals("8553P", underbond.C4_OriginPremiseID);
			AssertEquals("EM10M", underbond.C4_DestinationPremiseID);
			AssertEquals(1, hAWB.AllUnderbonds[0].Messages.Count);
			AssertEquals("EM10M", hAWB.AllUnderbonds[0].C4_OriginPremiseID);
			AssertEquals("8534B", hAWB.AllUnderbonds[0].C4_DestinationPremiseID);
		}

		#endregion

		#region URRTriggeredBy3rdPartyDoesNotUpdateOurUnderbondAtMAWBLevel

		public void TestURRTriggeredBy3rdPartyDoesNotUpdateOurUnderbondAtMAWBLevel()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM10M";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165620951";

			CusUnderbond underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			underbond.C4_ParentID = mAWB.PK;
			mAWB.AllUnderbonds.Load();
			underbond.C4_OriginPremiseID = "8553P";
			underbond.C4_DestinationPremiseID = "EM10M";
			underbond.C4_FlightNo = "QF7554";
			underbond.C4_ArrivalDate = new ZDateTime(2005, 11, 29);

			Factory.Save();
			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2831 IFD9 7I6F:1+32'
DTM+9:20051129101346501878:ZZZ'
DTM+132:20051129:102'
TDT+20+7554++6+QF::3'
TDT+1++ROA'
LOC+5+EM10M::95'
LOC+4+8534B::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000005'
RFF+MWB:08165620951'
UNT+17+000001'".Replace("\r\n", "");
			Message.SetEM_LinkedObject();

			AssertEquals("8553P", underbond.C4_OriginPremiseID);
			AssertEquals("EM10M", underbond.C4_DestinationPremiseID);
			AssertEquals(2, mAWB.AllUnderbonds.Count);
			AssertEquals("EM10M", mAWB.AllUnderbonds[1].C4_OriginPremiseID);
			AssertEquals("8534B", mAWB.AllUnderbonds[1].C4_DestinationPremiseID);
		}

		#endregion

		#region NumberOfPackagesFromCARSTandUBMREQ

		[TestDate(2005, 10, 17)]
		public void TestNumberOfPackagesFromCARSTandUBMREQ()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM10M";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;

			Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+313G 8GE3 CC65:1+32'
DTM+9:20051017083909695996:ZZZ'
DTM+132:20051017:102'
FTX+AAH+++FFJ773R132'
TDT+20+412++6+EK::3'
TDT+1++ROA'
LOC+5+8554J::95'
LOC+4+EM10M::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:132::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000011'
RFF+MWB:17678224300'
UNT+19+000001'".Replace("\r\n", "");

			Message.SetEM_LinkedObject();

			CMRCARSTMessage cARST1 = Factory.New<CMRCARSTMessage>();
			cARST1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+C6F0 CJ82 EG5:1+8'
DTM+9:20051018101038633987:ZZZ'
DTM+132:20051017:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+412++6+EK::3'
LOC+12+AUSYD::6'
LOC+4+EM10M::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:353::1'
RFF+MWB:17678224300'
RFF+HWB:00007898'
DOC+1'
PAC+0000002'
UNT+17+000001'".Replace("\r\n", "");
			cARST1.SetEM_LinkedObject();

			CMRCARSTMessage cARST2 = Factory.New<CMRCARSTMessage>();
			cARST2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+386G AJ9A CEG5:1+8'
DTM+9:20051018105646519426:ZZZ'
DTM+132:20051017:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+412++6+EK::3'
LOC+12+AUSYD::6'
LOC+4+EM10M::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:351::1'
RFF+MWB:17678224300'
RFF+HWB:00007889'
DOC+1'
PAC+0000006'
UNT+17+000001'".Replace("\r\n", "");
			cARST2.SetEM_LinkedObject();

			CMRCARSTMessage cARST3 = Factory.New<CMRCARSTMessage>();
			cARST3.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+33FG F21E F4G5:1+8'
DTM+9:20051018165339822738:ZZZ'
DTM+132:20051017:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+412++6+EK::3'
LOC+12+AUSYD::6'
LOC+4+EM10M::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:355::1'
RFF+MWB:17678224300'
RFF+HWB:00007900'
DOC+1'
PAC+0000001'
UNT+17+000001'".Replace("\r\n", "");
			cARST3.SetEM_LinkedObject();

			CMRCARSTMessage cARST4 = Factory.New<CMRCARSTMessage>();
			cARST4.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3099 6BEA F765:1+8'
DTM+9:20051019125038643319:ZZZ'
DTM+132:20051017:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+412++6+EK::3'
LOC+12+AUSYD::6'
LOC+4+EM10M::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:354::1'
RFF+MWB:17678224300'
RFF+HWB:00007899'
DOC+1'
PAC+0000001'
UNT+17+000001'".Replace("\r\n", "");
			cARST4.SetEM_LinkedObject();

			CusMAWB[] mAWBs = (CusMAWB[])Factory.Load(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, SQLComparisonOperator.Contains, "17678224300"));
			AssertEquals(0, mAWBs.Length);

			CusUnderbond[] underbonds = (CusUnderbond[])Factory.Load(typeof(CusUnderbond), new ZQuery(CusUnderbondSchema.C4_MAWB, SQLComparisonOperator.Contains, "17678224300"));
			AssertEquals(1, underbonds.Length);
			AssertEquals("EM10M", underbonds[0].C4_DestinationPremiseID);
			AssertEquals("11", underbonds[0].C4_PiecesManifested.ToString());
			AssertEquals("EK412", underbonds[0].C4_FlightNo);

			AssertEquals(4, underbonds[0].Outturns.Count);
			AssertEquals("00007898", underbonds[0].Outturns[0].C5_HouseBill);
			AssertEquals("2", underbonds[0].Outturns[0].C5_PackagesOutturned.ToString());
			AssertEquals("6", underbonds[0].Outturns[1].C5_PackagesOutturned.ToString());
		}

		#endregion

		#region UBMSendersReference

		public void TestUBMSendersReference()
		{
			Message.EM_MessageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+219I J989 7965:1+32'
DTM+9:20051006090327339837:ZZZ'
DTM+132:20051005:102'
FTX+AAH+++AAA394EU00000061/SYD1'
TDT+20+9110++6+FJ::3'
TDT+1++ROA'
LOC+5+A011E::95'
LOC+4+DK34C::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:U00000061/SYD1::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000011'
RFF+MWB:26015186990'
RFF+HWB:80091896'
UNT+20+000001'
".Replace("\r\n", "");

			AssertEquals("U00000061", Message.UBMSendersReference);
		}

		#endregion

		#region Implementation - Message

		CMRUBMREQRMessage fMessage;
		CMRUBMREQRMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = Factory.New<CMRUBMREQRMessage>();
				}
				return fMessage;
			}
		}

		#endregion

		#region Property Tests

		public void TestVoyageNumber()
		{
			AssertEquals("VoyageNumber", "32123", ((ICusSCAContainerInformationProvider)ApprovedContainerUBMREQR).VoyageNumber);
		}

		public void TestDestinationPort()
		{
			AssertEquals("Not supposed to have a DestinationPort", "", HouseUnderbondApprovalMessage.DestinationPort);
		}

		public void TestMovementApprovedMessageStatus()
		{
			AssertEquals(CMRUnderbondStatuses.Descriptions.UnderbondApprovalAdviceReceived, MovementApprovedMessage.GetStatus());
			AssertEquals(CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived, MovementApprovedMessage.GetStatusCode());
		}

		public void TestExpectedCargoAdviceMessageStatus()
		{
			AssertEquals(CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalAdviceReceived, ExpectedCargoAdviceMessage.GetStatus());
			AssertEquals(CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived, ExpectedCargoAdviceMessage.GetStatusCode());
		}

		public void TestMovementApprovedRescindMessageStatus()
		{
			AssertEquals(CMRUnderbondStatuses.Descriptions.UnderbondApprovalRescindAdviceReceived, MovementApprovedRescindMessage.GetStatus());
			AssertEquals(CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived, MovementApprovedRescindMessage.GetStatusCode());
		}

		public void TestExpectedCargoAdviceRescindMessageStatus()
		{
			AssertEquals(CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalRescindAdviceReceived, ExpectedCargoArrivalRescindMessage.GetStatus());
			AssertEquals(CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived, ExpectedCargoArrivalRescindMessage.GetStatusCode());
		}

		public void TestRequestReason()
		{
			AssertEquals("RequestReason", CMRUnderbondRequestCodes.Codes.OtherMovement, ApprovedContainerUBMREQR.RequestReason);
		}

		public void TestModeOfMovement()
		{
			AssertEquals("RequestReason", CMRUnderbondModeOfMovement.Codes.Road, ApprovedContainerUBMREQR.ModeOfMovement);
		}

		public void TestUnderbondBySeaVesselID()
		{
			AssertEquals("UnderbondBySeaVesselID", "7038678", UBMREQBySeaMessage.UnderbondBySeaVesselID);
		}

		public void TestUnderbondBySeaVoyage()
		{
			AssertEquals("UnderbondBySeaVoyage", "37", UBMREQBySeaMessage.UnderbondBySeaVoyage);
		}

		public void TestICSModeOfMovement()
		{
			AssertEquals("ModeOfMovement", CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel, UBMREQBySeaMessage.ModeOfMovement);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals("NumberOfPackages", 10, ExpectedCargoAdviceMessage.GetNumberOfPackages(1));
		}

		public void TestGetContainerNumber()
		{
			AssertEquals("GetContainerNumber", "RRRS0000002", UBMREQBySeaMessage.GetContainerNumber(2));
		}

		public void TestGetOceanBillOfLading()
		{
			AssertEquals("OceanBill", "OCEAN20050613DD", HouseUnderbondApprovalMessage.GetOceanBillOfLading(1));
		}

		public void TestGetHouseBillOfLading()
		{
			AssertEquals("HouseBill", "HOUSE45454545", HouseUnderbondApprovalMessage.GetHouseBillOfLading(1));
		}

		public void TestGetContainerMode()
		{
			AssertEquals("GetContainerNumber", Core.Constants.ContainerModes.FCL, UBMREQBySeaMessage.GetContainerMode(3));
		}

		public void TestGetPackageType()
		{
			AssertEquals("GetPackageType", "CT", UBMREQBySeaMessage.GetPackageType(4));
		}

		public void TestGetGoodsDescription()
		{
			AssertEquals("GetGoodsDescription", "BB GOODS", GoodsAndMarksUBMREQR.GetGoodsDescription(1));
		}

		public void TestGetMarksAndNumbers()
		{
			AssertEquals("GetMarksAndNumbers", "NM", GoodsAndMarksUBMREQR.GetMarksAndNumbers(1));
		}

		#endregion

		#region StandAloneSeaCargoDepot

		public void TestStandAloneSeaCargoDepot()
		{
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			currentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			currentBranch.OrgProxy.OH_IsUnpackDepot = true;
			ExpectedCargoAdviceMessage.EM_MessageNum = TestMessageNumber;
			ExpectedCargoAdviceMessage.SetEM_LinkedObject();
			CusUnderbond parent = GetUnderbondParent(TestMessageNumber, CusSCADepotContainerSchema.Constants.Prefix);
			AssertEquals("Message should connect up to a CusSCADepot Object", CusSCADepotContainerSchema.Constants.Prefix, parent.C4_ParentTableCode);
		}

		#endregion

		#region Implementation - GetUnderbondParent

		CusUnderbond GetUnderbondParent(ZString messageNumber, ZString tableCode)
		{
			CusUnderbond result = null;
			ZQuery filter = new ZQuery(EDIMessageSchema.EM_MessageNum, messageNumber);
			EDIMessage[] messages = (EDIMessage[])Factory.Load(typeof(EDIMessage), filter);
			foreach (EDIMessage message in messages)
			{
				DepotCusOutturn relatedOutturn = Factory.Load<DepotCusOutturn>(message.EM_LinkUniqueID);
				CusUnderbond relatedUnderbond = null;
				if (relatedOutturn != null)
				{
					relatedUnderbond = relatedOutturn.Underbond;
				}

				if (relatedUnderbond != null)
				{
					if (relatedUnderbond.C4_ParentTableCode == tableCode)
					{
						result = relatedUnderbond;
					}
				}
			}
			return result;
		}

		#endregion

		#region IntegragedUnpackCFSSeaCargoDepot

		public void TestIntegragedUnpackCFSSeaCargoDepot()
		{
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			currentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			currentBranch.OrgProxy.OH_IsUnpackDepot = true;

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();

			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing("8811924", "32123").PK;

			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "112233";

			ExpectedCargoAdviceMessage.SetEM_LinkedObject();
			DepotCusOutturn messageParent = (DepotCusOutturn)ExpectedCargoAdviceMessage.EM_LinkedObject;
			AssertNotNull("DepotCusOutturn has underbond", messageParent.Underbond);
			AssertNotNull("depotcusoutturn has header", messageParent.Header);
			AssertEquals("Underbond should exist for the outturn header", messageParent.C5_C6, messageParent.Underbond.C4_C6);
			AssertEquals("Message should connect up to a Freight Object", JobContainerSchema.Constants.Prefix, messageParent.Underbond.C4_ParentTableCode);
		}

		#endregion

		#region GetReportUnderbondApproval

		public void TestGetReportUnderbondApproval()
		{
			ZString report = MovementApprovedMessage.GetReport();
			AssertContains("Action - Status", "Underbond Approval", report);
			AssertContains("Container Number", "112233", report);
			AssertContains("Container Mode", "FCL", report);
			AssertContains("Container Description", "Full Container load", report);
			AssertContains("Transportation Mode", "Road", report);
			AssertContains("Movement Reason", "Other Movement", report);
			AssertContains("Package Count", "10", report);
			AssertContains("Vessel Lloyds", "8811924", report);
			AssertContains("Vessel Name", "ADMIRALENGRACHT", report);
			AssertContains("Voyage Num", "32123", report);
			AssertContains("Origin Premise Id", "9122P", report);
			AssertContains("Destination Premise ID", "9914N", report);
			AssertContains("Package Units", CMRPackageTypes.Codes.Bolt, report);
			AssertContains("Package Units", CMRPackageTypes.Descriptions.Bolt, report);
			AssertContains("Date of Message", "27-Jun-05", report);
			AssertContains("Time of Message", "17:46", report);
		}

		#endregion

		#region GetReportExpectedCargoArrival

		public void TestGetReportExpectedCargoArrival()
		{
			ZString report = ExpectedCargoAdviceMessage.GetReport();
			AssertContains("Action - Status", "Expected Cargo Arrival Advice", report);
			AssertContains("Container Number", "112233", report);
			AssertContains("Container Mode", "FCL", report);
			AssertContains("Container Description", "Full Container load", report);
			AssertContains("Transportation Mode", "Road", report);
			AssertContains("Movement Reason", "Other Movement", report);
			AssertContains("Package Count", "10", report);
			AssertContains("Vessel Lloyds", "8811924", report);
			AssertContains("Vessel Name", "ADMIRALENGRACHT", report);
			AssertContains("Voyage Num", "32123", report);
			AssertContains("Origin Premise Id", "9122P", report);
			AssertContains("Destination Premise ID", "9914N", report);
			AssertContains("Package Units", CMRPackageTypes.Codes.Bolt, report);
			AssertContains("Package Units", CMRPackageTypes.Descriptions.Bolt, report);
			AssertContains("Date of Message", "27-Jun-05", report);
			AssertContains("Time of Message", "17:46", report);
		}

		#endregion

		#region GetReportHouseUnderbondApproval

		public void TestGetReportHouseUnderbondApproval()
		{
			ZString report = HouseUnderbondApprovalMessage.GetReport();
			AssertContains("Action - Status", "Underbond Approval Advice Received", report);
			AssertContains("Container Number", "HALE2525260", report);
			AssertContains("Container Mode", "LCL", report);
			AssertContains("Container Description", "Less than Container load", report);
			AssertContains("House Bill Number", "HOUSE45454545", report);
			AssertContains("Ocean Bill Number", "OCEAN20050613DD", report);
			AssertContains("Transportation Mode", "Road", report);
			AssertContains("Movement Reason", "Other Movement", report);
			AssertContains("Package Count", "20", report);
			AssertContains("Vessel Lloyds", "7619422", report);
			AssertContains("Vessel Name", "ANRO TEMASEK", report);
			AssertContains("Voyage Num", "156", report);
			AssertContains("Origin Premise Id", "8136B", report);
			AssertContains("Destination Premise ID", "8139A", report);
			AssertContains("Package Units", CMRPackageTypes.Codes.Carton, report);
			AssertContains("Package Units", CMRPackageTypes.Descriptions.Carton, report);
		}

		#endregion

		#region GetReportUnderbondApprovalRescind

		public void TestGetReportUnderbondApprovalRescind()
		{
			ZString report = MovementApprovedRescindMessage.GetReport();
			AssertContains("Action - Status", "Underbond Approval Rescind Advice Received", report);
			AssertContains("Container Number", "OCLU10000010", report);
			AssertContains("Container Mode", "FCL", report);
			AssertContains("Container Description", "Full Container load", report);
			AssertContains("Transportation Mode", "Road", report);
			AssertContains("Movement Reason", "Unpack LCL at Destination", report);
			AssertContains("Vessel Lloyds", "8811924", report);
			AssertContains("Vessel Name", "ADMIRALENGRACHT", report);
			AssertContains("Voyage Num", "9186", report);
			AssertContains("Origin Premise Id", "9122P", report);
			AssertContains("Destination Premise ID", "9932A", report);
		}

		#endregion

		#region GetReportExpectedCargoArrivalRescind

		public void TestGetReportExpectedCargoArrivalRescind()
		{
			ZString report = ExpectedCargoArrivalRescindMessage.GetReport();
			AssertContains("Action - Status", "Expected Cargo Arrival Rescind Advice Received", report);
			AssertContains("MAWB", "08110984526", report);
			AssertContains("Transportation Mode", "Road", report);
			AssertContains("Movement Reason", "Other Movement", report);
			AssertContains("Package Count", "100", report);
			AssertContains("Flight No", "QF123", report);
			AssertContains("Flight Date", "21-Jun-05", report);
			AssertContains("Origin Premise Id", "9920A", report);
			AssertContains("Destination Premise ID", "9914N", report);
		}

		#endregion

		#region GetReportUnderbondApprovalWithRelatedUnderbond

		public void TestGetReportUnderbondApprovalWithRelatedUnderbond()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_LloydsIMO = "8811924";
			oceanBill.CB_Voyage = "32123";
			oceanBill.CB_OceanBill = "OBL123456";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";

			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "112233";
			CusUnderbond underbondRequest = container.Underbonds.AddNew();

			MovementApprovedMessage.EM_LinkedObject = underbondRequest;
			ZString report = MovementApprovedMessage.GetReport();
			AssertContains("Ocean Bill from Linked object", "OBL123456", report);
		}

		#endregion

		#region PremiseAddress

		public void TestPremiseAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Premise Org Name";
			header.MainAddress.OA_Address1 = "Premise Org Main Address";
			OrgAddress premiseAddressWithCode = header.Addresses.AddNew();
			premiseAddressWithCode.OA_Address1 = "Coded Address 9914N";
			premiseAddressWithCode.OA_City = "Sydney";
			premiseAddressWithCode.OA_State = "NSW";
			premiseAddressWithCode.OA_PostCode = "2134";
			premiseAddressWithCode.LocalControlledPremisesID = "9914N";

			ZString report = ExpectedCargoAdviceMessage.GetReport();
			AssertContains("Org Name", header.OH_FullName, report);
			AssertContains("Address 1", premiseAddressWithCode.OA_Address1, report);
			AssertContains("State", premiseAddressWithCode.OA_State, report);
			AssertContains("City", premiseAddressWithCode.OA_City, report);
			AssertContains("Post Code", premiseAddressWithCode.OA_PostCode, report);
		}

		#endregion

		#region Incident

		public void TestProcessUnderbondApprovalFromIncident()
		{
			string messageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+27DF AA57 665H:1+32'
DTM+9:20070530071754334302:ZZZ'
DTM+132:20070530:102'
FTX+AAH+++FGF376KU00007442/MEL1'
TDT+20+412++6+EK::3'
TDT+1++ROA'
LOC+5+A011E::95'
LOC+4+FD69N::95'
NAD+MR+FFM969M::95'
NAD+UD+32003890328::95'
RFF+ABO:U00007442/MEL1::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000057'
RFF+MWB:17601348060'
RFF+HWB:80169724'
UNT+20+000001'".Replace("\r\n", "");

			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_MessageText = messageText;
			message.EM_ReceiveTransmit = CMRMessage.Direction.Receive;

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "A011E";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB mawb = Factory.New<CusMAWB>();
			CusHAWB hawb = mawb.ChildBills.AddNew();

			mawb.CM_MAWB = "17601348060";
			mawb.CM_FlightNo = "EK412";

			hawb.CS_HAWB = "80169724";

			CusMAWB wrongMawb = Factory.New<CusMAWB>();
			CusHAWB wrongHawb = wrongMawb.ChildBills.AddNew();

			wrongMawb.CM_MAWB = "1234567";
			wrongMawb.CM_FlightNo = "AA394";

			wrongHawb.CS_HAWB = "321";

			CusUnderbond wrongUnderbond = wrongHawb.Underbonds.AddNew();
			wrongUnderbond.C4_SendersMessageReference = "U00007442";

			BatchProcessor.LoggingInformation logger = new BatchProcessor.LoggingInformation();
			UBMREQRMessageProcessor processor = new UBMREQRMessageProcessor(logger);
			Factory.Save();

			AssertNull(message.EM_LinkedObject);
			processor.ProcessMessage(message);
			AssertNotNull(message.EM_LinkedObject);
			AssertNotEquals(wrongUnderbond, message.EM_LinkedObject);
			AssertEquals(hawb, ((CusUnderbond)message.EM_LinkedObject).HAWBLinked);
		}

		[TestDate(2006, 1, 1)]
		public void TestIncomingPartShipmentExpectedArrivalWithDifferentDate()
		{
			CMRUBMREQRMessage firstMessage = GetExpectedArrivalMessage(@"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2FAA 18A2 46BF:1+32'
DTM+9:20051230053428776284:ZZZ'
DTM+132:20051230:102'
FTX+AAH+++FFK334E00000000675289'
TDT+20+601++6+OZ::3'
TDT+1++ROA'
LOC+5+EF33J::95'
LOC+4+P026P::95'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:00000000675289::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000002'
RFF+MWB:98823240932'
UNT+19+000001'");

			CMRUBMREQRMessage secondMessage = GetExpectedArrivalMessage(@"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2FAA 18A2 46BF:1+32'
DTM+9:20051230053428776284:ZZZ'
DTM+132:20051231:102'
FTX+AAH+++FFK334E00000000675289'
TDT+20+601++6+OZ::3'
TDT+1++ROA'
LOC+5+EF33J::95'
LOC+4+P026P::95'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:00000000675289::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000001'
RFF+MWB:98823240932'
UNT+19+000001'");

			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "P026P";

			StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory underbondFactory = new StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory();

			CusMAWB mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "98823240932";
			mawb.CM_ArrivalDate = new ZDateTime(2005, 12, 30);
			mawb.CM_FlightNo = "OZ601";

			CusUnderbond underbond = Factory.New<CusUnderbond>();
			mawb.Underbonds.Add(underbond);
			underbond.SetDefaultValuesFromParent(); // What the hell...
			underbond.C4_ArrivalDate = new ZDateTime(2005, 12, 30);
			AssertNotEquals(ZString.Empty, underbond.C4_ArrivalDate);
			AssertNotEquals(ZString.Empty, underbond.C4_FlightNo);

			underbond.C4_OriginPremiseID = "EF33J";
			underbond.C4_DestinationPremiseID = "P026P";

			Factory.Save();

			underbondFactory.ProcessIncomingUBMREQR(firstMessage);
			CusUnderbond firstUnderbond = (CusUnderbond)firstMessage.EM_LinkedObject;
			AssertNotNull(firstUnderbond);
			AssertEquals(Diff(underbond, firstUnderbond), underbond, firstUnderbond);
			Factory.Save();

			underbondFactory.ProcessIncomingUBMREQR(secondMessage);
			CusUnderbond secondUnderbond = (CusUnderbond)secondMessage.EM_LinkedObject;
			AssertNotNull(secondUnderbond);
			AssertNotEquals(firstUnderbond, secondUnderbond);

			Factory.Save();
			AssertNotEquals(ZString.Empty, secondUnderbond.C4_SendersMessageReference);
			AssertNotEquals(firstUnderbond.C4_SendersMessageReference, secondUnderbond.C4_SendersMessageReference);

			AssertEquals(firstUnderbond.C4_DestinationPremiseID, secondUnderbond.C4_DestinationPremiseID);
			AssertEquals(firstUnderbond.C4_FlightNo, secondUnderbond.C4_FlightNo);
			AssertEquals(firstUnderbond.C4_MAWB, secondUnderbond.C4_MAWB);
			AssertEquals(firstUnderbond.C4_ModeOfMovement, secondUnderbond.C4_ModeOfMovement);
			AssertEquals(firstUnderbond.C4_MovementReason, secondUnderbond.C4_MovementReason);
			AssertEquals(firstUnderbond.C4_OriginPremiseID, secondUnderbond.C4_OriginPremiseID);
			AssertEquals(firstUnderbond.C4_ParentID, secondUnderbond.C4_ParentID);
			AssertEquals(firstUnderbond.C4_PackageType, secondUnderbond.C4_PackageType);

			AssertNotEquals(firstUnderbond.C4_SendersMessageReference, secondUnderbond.C4_SendersMessageReference);
			AssertNotEquals(firstUnderbond.C4_PiecesManifested, secondUnderbond.C4_PiecesManifested);
			AssertNotEquals(firstUnderbond.C4_ArrivalDate, secondUnderbond.C4_ArrivalDate);

			AssertEquals(new ZDateTime(2005, 12, 30), firstUnderbond.C4_ArrivalDate);
			AssertEquals(2, firstUnderbond.C4_PiecesManifested);

			AssertEquals(new ZDateTime(2005, 12, 31), secondUnderbond.C4_ArrivalDate);
			AssertEquals(1, secondUnderbond.C4_PiecesManifested);
		}

		#endregion

		#region Air Cargo Transhipment Warehouse Publishing
		public void TestTriggersXUSOnEXP()
		{
			CombineAssertions(() =>
			{
				var underbond = Factory.New<CusUnderbond>();
				underbond.C4_MAWB = "347295735";

				// Air Cargo
				var message = Factory.New<CMRUBMREQRMessage>();
				message.EM_LinkedObject = underbond;
				message.EM_MessageText = ExpMessageText.Replace("\r\n", "");
				Factory.Save();
				message.EM_Status = EDIMessage.Status.Received;

				Factory.Save();

				AssertMAWBXUS(underbond, "347295735", "EXP for Air Cargo Outturn");

				// Failed message
				underbond = Factory.New<CusUnderbond>();
				message = Factory.New<CMRUBMREQRMessage>();
				message.EM_LinkedObject = underbond;
				message.EM_MessageText = ExpMessageText.Replace("\r\n", "");
				Factory.Save();
				message.EM_Status = EDIMessage.Status.Error;

				Factory.Save();

				AssertNoXUS(underbond, "Failed EXP Message does not trigger XUP");

				// No linked object
				message = Factory.New<CMRUBMREQRMessage>();
				message.EM_LinkedObject = GlbCompany.CurrentCompany;
				message.EM_MessageText = ExpMessageText.Replace("\r\n", "");
				Factory.Save();
				message.EM_Status = EDIMessage.Status.Received;

				Factory.Save();

				AssertNoXUS(underbond, "Unlinked EXP Message does not trigger XUP");

				// Not an EXP message
				underbond = Factory.New<CusUnderbond>();
				message = Factory.New<CMRUBMREQRMessage>();
				message.EM_LinkedObject = underbond;
				message.EM_MessageText = ExpMessageText.Replace("\r\n", "").Replace("RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE", "RFF+ANX:UNDERBOND APPROVAL");
				Factory.Save();
				message.EM_Status = EDIMessage.Status.Received;

				Factory.Save();

				AssertNoXUS(underbond, "UBM Response message that is not EXP does not trigger XUP");

				// Sea Cargo
				underbond = Factory.New<CusUnderbond>();
				message = Factory.New<CMRUBMREQRMessage>();
				message.EM_LinkedObject = underbond;
				message.EM_MessageText = ExpMessageText.Replace("\r\n", "").Replace("TDT+20+601++6", "TDT+20+601++11");
				Factory.Save();
				message.EM_Status = EDIMessage.Status.Received;

				Factory.Save();

				AssertNoXUS(underbond, "Sea cargo EXP Message does not trigger XUP here");
			});
		}

		public void TestTriggersXUSOnEXP_ForMAWB()
		{
			CombineAssertions(() =>
			{
				var mawb = Factory.New<CusMAWB>();
				mawb.CM_MAWB = "347295735";
				var underbond = mawb.Underbonds.AddNew();
				underbond.C4_MAWB = "123456789";

				var message = Factory.New<CMRUBMREQRMessage>();
				message.EM_LinkedObject = underbond;
				message.EM_MessageText = ExpMessageText.Replace("\r\n", "");
				Factory.Save();
				message.EM_Status = EDIMessage.Status.Received;

				Factory.Save();

				AssertMAWBXUS(mawb, "347295735", "EXP for Air Cargo Report");
			});
		}

		void AssertNoXUS(EnterpriseBusinessObject bo, string messageHeader = "")
		{
			messageHeader = string.IsNullOrEmpty(messageHeader) ? messageHeader : messageHeader + ":";
			AssertEquals(messageHeader, false, bo.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.DataExport.Code));
		}

		void AssertMAWBXUS(EnterpriseBusinessObject bo, string mawb, string messageHeader = "")
		{
			var logs = bo.Logs.GetAllLogs();
			logs.Reload(true);

			messageHeader = string.IsNullOrEmpty(messageHeader) ? messageHeader : messageHeader + ":";

			var dexEvent = logs.Cast<StmALog>().Single(x => x.SL_SE_NKEvent == Events.DataExport.Code);
			AssertNotNull(messageHeader + "One [DEX] event should be linked to CusMAWB", dexEvent);
			AssertNotNull(messageHeader, dexEvent.RelatedEDIMessage);
			AssertNotNull(messageHeader, dexEvent.RelatedEDIMessage.Message);
			AssertNotNullOrEmpty(messageHeader, dexEvent.RelatedEDIMessage.Message.EM_MessageTextDetail);

			var universalShipmentMessage = dexEvent.RelatedEDIMessage.Message;
			AssertNotNull(messageHeader, universalShipmentMessage);

			var logger = new DummyLogger();
			var outMAWB = universalShipmentMessage.GetEM_MessageTextReader().Parse<UniversalShipment>(logger, new CodeMappingManager(logger));
			AssertEquals(messageHeader + "Air Manifest is generated as per CusMAWB", mawb, outMAWB.WayBillNumber);
			AssertEquals(messageHeader + "The recipient should be Arrival Transit Warehouse", true, outMAWB.DataContext.RecipientRoleCollection.Any(x => x.Code == RecipientRoleType.ATW));
			AssertEquals(messageHeader + "The service code should be Transit Warehouse Receive", true, outMAWB.DataContext.RecipientRoleCollection.Any(x => x.ServiceCode == ServiceCodeType.TWR));
		}

		const string ExpMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2FAA 18A2 46BF:1+32'
DTM+9:20051230053428776284:ZZZ'
DTM+132:20051230:102'
FTX+AAH+++FFK334E00000000675289'
TDT+20+601++6+OZ::3'
TDT+1++ROA'
LOC+5+EF33J::95'
LOC+4+P026P::95'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:00000000675289::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000001'
RFF+MWB:98823240932'
UNT+19+000001'";
		#endregion

		#region Implementation

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

		CMRUBMREQRMessage fMovementApprovedRescindMessage;
		CMRUBMREQRMessage MovementApprovedRescindMessage
		{
			get
			{
				if (fMovementApprovedRescindMessage == null)
				{
					fMovementApprovedRescindMessage = Factory.New<CMRUBMREQRMessage>();
					fMovementApprovedRescindMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3715 BB6B F05F:1+32'
TDT+20+9186++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9932A::95'
NAD+MR+AAA374M::95'
RFF+ANX:UNDERBOND APPROVAL RESCIND NOTICE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
RFF+AAQ:OCLU10000010'
UNT+13+000001'
".Replace("\r\n", "");
				}
				return fMovementApprovedRescindMessage;
			}
		}

		CMRUBMREQRMessage fExpectedCargoArrivalRescindMessage;
		CMRUBMREQRMessage ExpectedCargoArrivalRescindMessage
		{
			get
			{
				if (fExpectedCargoArrivalRescindMessage == null)
				{
					fExpectedCargoArrivalRescindMessage = Factory.New<CMRUBMREQRMessage>();
					fExpectedCargoArrivalRescindMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+4DJH 7EG0 HJ5F:1+32'
DTM+132:20050621:102'
FTX+AAH+++AAA374MU00000054/SYD1'
TDT+20+123++6+QF::3'
TDT+1++ROA'
LOC+5+9920A::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
RFF+ANX:EXPECTED CARGO ARRIVAL RESCIND NTCE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000100'
RFF+MWB:08110984526'
UNT+16+000001'".Replace("\r\n", "");
				}
				return fExpectedCargoArrivalRescindMessage;
			}
		}

		CMRUBMREQRMessage fMovementApprovedMessage;
		CMRUBMREQRMessage MovementApprovedMessage
		{
			get
			{
				if (fMovementApprovedMessage == null)
				{
					fMovementApprovedMessage = Factory.New<CMRUBMREQRMessage>();
					fMovementApprovedMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+29DD 0520 9GFF:1+32'
DTM+9:20050627174621948489:ZZZ'
FTX+AAH+++AAA374MU00000058/SYD3'
TDT+20+32123++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000010++BM:185:95'
RFF+AAQ:112233'
UNT+16+000001'
".Replace("\r\n", "");
				}
				return fMovementApprovedMessage;
			}
		}

		CMRUBMREQRMessage fOtherStatusMessage;
		CMRUBMREQRMessage OtherStatusMessage
		{
			get
			{
				if (fOtherStatusMessage == null)
				{
					fOtherStatusMessage = Factory.New<CMRUBMREQRMessage>();
					fOtherStatusMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+29DD 0520 9GFF:1+32'
DTM+9:20050627174621948489:ZZZ'
FTX+AAH+++AAA374MU00000058/SYD3'
TDT+20+32123++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
RFF+ANX:BLAH BLAH'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000010++BM:185:95'
RFF+AAQ:112233'
UNT+16+000001'
".Replace("\r\n", "");
				}
				return fOtherStatusMessage;
			}
		}

		CMRUBMREQRMessage fHouseExpectedArrivalMessage;
		CMRUBMREQRMessage HouseUnderbondApprovalMessage
		{
			get
			{
				if (fHouseExpectedArrivalMessage == null)
				{
					fHouseExpectedArrivalMessage = Factory.New<CMRUBMREQRMessage>();
					fHouseExpectedArrivalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+156++11++++7619422::11'
TDT+1++ROA'
LOC+5+8136B::95'
LOC+4+8139A::95'
NAD+MR+CJM436P::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:HALE2525260'
RFF+MB:OCEAN20050613DD'
RFF+BH:HOUSE45454545'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fHouseExpectedArrivalMessage;
			}
		}

		CMRUBMREQRMessage fUBMREQBySeaMessage;
		CMRUBMREQRMessage UBMREQBySeaMessage
		{
			get
			{
				if (fUBMREQBySeaMessage == null)
				{
					fUBMREQBySeaMessage = Factory.New<CMRUBMREQRMessage>();
					fUBMREQBySeaMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+9538 A830 45F:1+32'
FTX+AAH+++CJM436P20417'
TDT+20+156++11++++7619422::11'
TDT+1++IVS'
TDT+1+37++11++++7038678::11'
LOC+5+KR15S::95'
LOC+4+8135H::95'
NAD+MR+CJN977M::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000001'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000002'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000003'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000004'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000005'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000006'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000007'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000008'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000009'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++CT:185:95'
RFF+AAQ:RRRS0000010'
UNT+52+000001'".Replace("\r\n", "");
				}
				return fUBMREQBySeaMessage;
			}
		}

		CMRUBMREQRMessage fExpectedCargoAdviceMessage;
		CMRUBMREQRMessage ExpectedCargoAdviceMessage
		{
			get
			{
				if (fExpectedCargoAdviceMessage == null)
				{
					fExpectedCargoAdviceMessage = Factory.New<CMRUBMREQRMessage>();
					fExpectedCargoAdviceMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3E65 FG0F 9GFF:1+32'
DTM+9:20050627174622089471:ZZZ'
FTX+AAH+++AAA374MU00000058/SYD3'
TDT+20+32123++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000010++BM:185:95'
RFF+AAQ:112233'
UNT+16+000001'
".Replace("\r\n", "");
				}
				return fExpectedCargoAdviceMessage;
			}
		}

		CMRUBMREQRMessage GetExpectedArrivalMessage(string message)
		{
			CMRUBMREQRMessage result = Factory.New<CMRUBMREQRMessage>();
			result.EM_MessageText = message.Replace("\r\n", "");
			return result;
		}

		CMRUBMREQRMessage fApprovedContainerUBMREQR;
		CMRUBMREQRMessage ApprovedContainerUBMREQR
		{
			get
			{
				if (fApprovedContainerUBMREQR == null)
				{
					fApprovedContainerUBMREQR = Factory.New<CMRUBMREQRMessage>();
					fApprovedContainerUBMREQR.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::UBMREQR+3E65 FG0F 9GFF:1+32'DTM+9:20050627174622089471:ZZZ'FTX+AAH+++AAA374MU00000058/SYD3'TDT+20+32123++11++++8811924::11'TDT+1++ROA'LOC+5+9122P::95'LOC+4+9914N::95'NAD+MR+AAA374M::95'RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'RFF+ACD:MOV'DOC+1'PAC+++FCL:67:95'PAC+0000010++BM:185:95'RFF+AAQ:112233'UNT+16+000001'";
				}
				return fApprovedContainerUBMREQR;
			}
		}

		CMRUBMREQRMessage fApprovedAirUBMREQR;
		CMRUBMREQRMessage ApprovedAirUBMREQR
		{
			get
			{
				if (fApprovedAirUBMREQR == null)
				{
					fApprovedAirUBMREQR = Factory.New<CMRUBMREQRMessage>();
					fApprovedAirUBMREQR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+1A2I 59J4 8905:1+32'
DTM+132:20050308:102'
FTX+AAH+++AAA447YL3020014M000021'
TDT+20+262++6+QF::3'
TDT+1++ROA'LOC+5+9920A::95'
LOC+4+9932A::95'
NAD+MR+AAA374M::95'
RFF+ABO:08126284355::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000010'
RFF+MWB:08126284355'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fApprovedAirUBMREQR;
			}
		}

		CMRUBMREQRMessage goodsAndMarksUBMREQR;
		CMRUBMREQRMessage GoodsAndMarksUBMREQR
		{
			get
			{
				if (goodsAndMarksUBMREQR == null)
				{
					goodsAndMarksUBMREQR = Factory.New<CMRUBMREQRMessage>();
					goodsAndMarksUBMREQR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2FE7 C8F2 5D6F:1+32'
DTM+9:20051201163021365534:ZZZ'
FTX+AAH+++AAA374MU00000560/CMT4'
TDT+20+A1++11++++8208684::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:U00000560/CMT4::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++B/B:67:95'
PAC+0000010++PK:185:95'
RFF+MB:A4-BB'
PCI+28+NM'
FTX+AAA+++BB GOODS'
UNT+20+000001'".Replace("\r\n", "");
				}
				return goodsAndMarksUBMREQR;
			}
		}

		CusUnderbond fMAWBUnderbond;
		CusUnderbond MAWBUnderbond
		{
			get
			{
				if (fMAWBUnderbond == null)
				{
					fMAWBUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)MAWB).Underbonds.AddNew();
					fMAWBUnderbond.C4_OriginPremiseID = "9920A";
					fMAWBUnderbond.C4_DestinationPremiseID = "9932A";
					fMAWBUnderbond.C4_SendersMessageReference = MAWB.CM_MAWB;
				}
				return fMAWBUnderbond;
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
					MAWB.CM_MAWB = "08126284355";
					MAWB.CM_FlightNo = "QF262";
					MAWB.CM_ArrivalDate = new ZDateTime(2005, 3, 8);
				}
				return fMAWB;
			}
		}

		#endregion
	}
}
