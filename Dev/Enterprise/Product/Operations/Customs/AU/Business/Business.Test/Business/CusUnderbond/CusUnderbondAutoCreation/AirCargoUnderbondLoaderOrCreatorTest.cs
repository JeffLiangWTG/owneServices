using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AirCargoUnderbondLoaderOrCreatorTest : TestCaseWithFactory
	{
		public void TestUnderbondCreation()
		{
			ZDateTime arrivalDate = new ZDateTime(2005, 10, 24);
			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			CusUnderbond underbond = creator.CreateRecord("08145823934", "", "QF123", arrivalDate, "AUSYD", 5, "MOV", "AIR", "9919N", "123TT", true);
			Factory.Save();
			AssertEquals(arrivalDate, underbond.C4_ArrivalDate);
			AssertEquals("08145823934", underbond.C4_MAWB);
			Assert(!underbond.C4_SendersMessageReference.IsEmpty);
			AssertEquals("QF123", underbond.C4_FlightNo);
			AssertEquals("AUSYD", underbond.C4_RL_NKDischargePort);
			AssertEquals("5", underbond.C4_PiecesManifested.ToString());
			AssertEquals("MOV", underbond.C4_MovementReason);
			AssertEquals("AIR", underbond.C4_ModeOfMovement);
			AssertEquals("9919N", underbond.C4_OriginPremiseID);
			AssertEquals("123TT", underbond.C4_DestinationPremiseID);
		}

		public void TestUnderbondCreationWithPartShipmentsDifferentArrivalDates()
		{
			ZDateTime arrivalDate = new ZDateTime(2005, 10, 24, 11, 11, 0);
			ZDateTime arrivalDate2 = new ZDateTime(2005, 10, 25, 11, 11, 0);
			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			CusUnderbond underbond = creator.CreateRecord("08145823934", "", "QF123", arrivalDate, "AUSYD", 5, "MOV", "AIR", "9919N", "123TT", true);
			CusUnderbond underbond2 = creator.CreateRecord("08145823934", "", "QF123", arrivalDate2, "AUSYD", 2, "MOV", "AIR", "9919N", "123TT", true);
			Factory.Save();

			AssertEquals(arrivalDate, underbond.C4_ArrivalDate);
			AssertEquals("08145823934", underbond.C4_MAWB);
			Assert(!underbond.C4_SendersMessageReference.IsEmpty);
			AssertEquals("QF123", underbond.C4_FlightNo);
			AssertEquals("AUSYD", underbond.C4_RL_NKDischargePort);
			AssertEquals("5", underbond.C4_PiecesManifested.ToString());
			AssertEquals("MOV", underbond.C4_MovementReason);
			AssertEquals("AIR", underbond.C4_ModeOfMovement);
			AssertEquals("9919N", underbond.C4_OriginPremiseID);
			AssertEquals("123TT", underbond.C4_DestinationPremiseID);

			AssertEquals(arrivalDate2, underbond2.C4_ArrivalDate);
			AssertEquals("08145823934", underbond2.C4_MAWB);
			Assert(!underbond2.C4_SendersMessageReference.IsEmpty);
			AssertEquals("QF123", underbond2.C4_FlightNo);
			AssertEquals("AUSYD", underbond2.C4_RL_NKDischargePort);
			AssertEquals("2", underbond2.C4_PiecesManifested.ToString());
			AssertEquals("MOV", underbond2.C4_MovementReason);
			AssertEquals("AIR", underbond2.C4_ModeOfMovement);
			AssertEquals("9919N", underbond2.C4_OriginPremiseID);
			AssertEquals("123TT", underbond2.C4_DestinationPremiseID);
		}

		public void TestUnderbondCreationWithPartShipmentsDifferentFlights()
		{
			ZDateTime arrivalDate = new ZDateTime(2005, 10, 24, 11, 11, 0);
			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			CusUnderbond underbond = creator.CreateRecord("08145823934", "", "QF123", arrivalDate, "AUSYD", 5, "MOV", "AIR", "9919N", "123TT", true);
			CusUnderbond underbond2 = creator.CreateRecord("08145823934", "", "QF124", arrivalDate, "AUSYD", 2, "MOV", "AIR", "9919N", "123TT", true);
			Factory.Save();

			AssertEquals(arrivalDate, underbond.C4_ArrivalDate);
			AssertEquals("08145823934", underbond.C4_MAWB);
			Assert(!underbond.C4_SendersMessageReference.IsEmpty);
			AssertEquals("QF123", underbond.C4_FlightNo);
			AssertEquals("AUSYD", underbond.C4_RL_NKDischargePort);
			AssertEquals("5", underbond.C4_PiecesManifested.ToString());
			AssertEquals("MOV", underbond.C4_MovementReason);
			AssertEquals("AIR", underbond.C4_ModeOfMovement);
			AssertEquals("9919N", underbond.C4_OriginPremiseID);
			AssertEquals("123TT", underbond.C4_DestinationPremiseID);

			AssertEquals(arrivalDate, underbond2.C4_ArrivalDate);
			AssertEquals("08145823934", underbond2.C4_MAWB);
			Assert(!underbond2.C4_SendersMessageReference.IsEmpty);
			AssertEquals("QF124", underbond2.C4_FlightNo);
			AssertEquals("AUSYD", underbond2.C4_RL_NKDischargePort);
			AssertEquals("2", underbond2.C4_PiecesManifested.ToString());
			AssertEquals("MOV", underbond2.C4_MovementReason);
			AssertEquals("AIR", underbond2.C4_ModeOfMovement);
			AssertEquals("9919N", underbond2.C4_OriginPremiseID);
			AssertEquals("123TT", underbond2.C4_DestinationPremiseID);
		}

		public void TestUnderbondNotCreationWithSameDetails()
		{
			ZDateTime arrivalDate = new ZDateTime(2005, 10, 24, 11, 11, 0);
			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			CusUnderbond underbond = creator.CreateRecord("08145823934", "", "QF123", arrivalDate, "AUSYD", 5, "MOV", "AIR", "9919N", "123TT", true);
			CusUnderbond underbond2 = creator.CreateRecord("08145823934", "", "QF123", arrivalDate, "AUSYD", 2, "DCL", "AIR", "9919N", "123TT", true);
			Factory.Save();

			AssertEquals(underbond, underbond2);

			AssertEquals(arrivalDate, underbond.C4_ArrivalDate);
			AssertEquals("08145823934", underbond.C4_MAWB);
			Assert(!underbond.C4_SendersMessageReference.IsEmpty);
			AssertEquals("QF123", underbond.C4_FlightNo);
			AssertEquals("AUSYD", underbond.C4_RL_NKDischargePort);
			AssertEquals("2", underbond.C4_PiecesManifested.ToString());
			AssertEquals("DCL", underbond.C4_MovementReason);
			AssertEquals("AIR", underbond.C4_ModeOfMovement);
			AssertEquals("9919N", underbond.C4_OriginPremiseID);
			AssertEquals("123TT", underbond.C4_DestinationPremiseID);
		}

		public void TestDefaults()
		{
			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			CusUnderbond underbond = creator.CreateRecord("08145823934", "", "", ZDateTime.Empty, "", 0, "", "", "", "", true);
			AssertEquals(CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, underbond.C4_MovementReason);
			AssertEquals(CMRUnderbondModeOfMovement.Codes.Road, underbond.C4_ModeOfMovement);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID, underbond.C4_DestinationPremiseID);
		}

		public void TestUnderbondCreationAtHAWBLevel()
		{
			ZDateTime arrivalDate = new ZDateTime(2005, 10, 24);
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08145823934";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "01523";
			CusUnderbond mAWBUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			Factory.Save();
			AssertEquals(0, hAWB.AllUnderbonds.Count);
			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			CusUnderbond underbond = creator.CreateRecord("08145823934", "01523", "QF123", arrivalDate, "AUSYD", 5, "MOV", "AIR", "9919N", "123TT", true);
			AssertEquals(arrivalDate, underbond.C4_ArrivalDate);
			AssertEquals("QF123", underbond.C4_FlightNo);
			AssertEquals("AUSYD", underbond.C4_RL_NKDischargePort);
			AssertEquals("5", underbond.C4_PiecesManifested.ToString());
			AssertEquals("MOV", underbond.C4_MovementReason);
			AssertEquals("AIR", underbond.C4_ModeOfMovement);
			AssertEquals("9919N", underbond.C4_OriginPremiseID);
			AssertEquals("123TT", underbond.C4_DestinationPremiseID);
			AssertEquals(1, hAWB.AllUnderbonds.Count);
			Assert(mAWBUnderbond.C4_FlightNo.IsEmpty);
			Assert(mAWBUnderbond.C4_RL_NKDischargePort.IsEmpty);
			Assert(mAWBUnderbond.C4_OriginPremiseID.IsEmpty);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID, mAWBUnderbond.C4_DestinationPremiseID);
		}

		public void TestUnderbondLoad()
		{
			ZDateTime arrivalDate = new ZDateTime(2005, 10, 24);
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_SendersMessageReference = "08145823934";
			underbond.C4_FlightNo = "QF123";
			underbond.C4_MAWB = "08145823934";
			Factory.Save();
			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			creator.CreateRecord("08145823934", "", "QF123", arrivalDate, "AUSYD", 5, "MOV", "AIR", "9919N", "123TT", true);
			AssertEquals(arrivalDate, underbond.C4_ArrivalDate);
			AssertEquals("08145823934", underbond.C4_SendersMessageReference);
			AssertEquals("QF123", underbond.C4_FlightNo);
			AssertEquals("AUSYD", underbond.C4_RL_NKDischargePort);
			AssertEquals("5", underbond.C4_PiecesManifested.ToString());
			AssertEquals("MOV", underbond.C4_MovementReason);
			AssertEquals("AIR", underbond.C4_ModeOfMovement);
			AssertEquals("9919N", underbond.C4_OriginPremiseID);
			AssertEquals("123TT", underbond.C4_DestinationPremiseID);
		}

		public void TestUnderbondLoadAtHAWBLevel()
		{
			ZDateTime arrivalDate = new ZDateTime(2005, 10, 24);
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08145823934";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "01523";
			CusUnderbond mAWBUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			CusUnderbond hAWBUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)hAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			hAWB.AllUnderbonds.Load();
			hAWBUnderbond.C4_OriginPremiseID = "9919N";
			hAWBUnderbond.C4_DestinationPremiseID = "123TT";
			hAWBUnderbond.C4_ArrivalDate = arrivalDate;
			Factory.Save();
			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			CusUnderbond underbond = creator.CreateRecord("08145823934", "01523", "QF123", arrivalDate, "AUSYD", 5, "MOV", "AIR", "9919N", "123TT", true);
			AssertEquals(arrivalDate, underbond.C4_ArrivalDate);
			AssertEquals("QF123", underbond.C4_FlightNo);
			AssertEquals("QF123", hAWBUnderbond.C4_FlightNo);
			AssertEquals("AUSYD", underbond.C4_RL_NKDischargePort);
			AssertEquals("AUSYD", hAWBUnderbond.C4_RL_NKDischargePort);
			AssertEquals("5", underbond.C4_PiecesManifested.ToString());
			AssertEquals("MOV", underbond.C4_MovementReason);
			AssertEquals("AIR", underbond.C4_ModeOfMovement);
			AssertEquals("9919N", underbond.C4_OriginPremiseID);
			AssertEquals("123TT", underbond.C4_DestinationPremiseID);
		}

		public void TestUpdateOfCurrentUnderbond()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "40691102955";
			CusUnderbond underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			underbond.C4_ParentTableCode = "CM";
			underbond.C4_ArrivalDate = new ZDateTime(2005, 11, 6);
			underbond.C4_OriginPremiseID = "9914N";
			underbond.C4_DestinationPremiseID = "22Q3";
			underbond.C4_ParentID = mAWB.PK;
			Factory.Save();

			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			creator.CreateRecord("40691102955", "", "QF123", new ZDateTime(2005, 11, 6), "AUSYD", 23, "", "AIR", "9914N", "22Q3", true);
			AssertEquals("QF123", underbond.C4_FlightNo);
			AssertEquals("AIR", underbond.C4_ModeOfMovement);
		}

		public void TestTwoCARSTMessagesWithTwoExistingUnderbonds()
		{
			CusUnderbond underbond1 = Factory.New<CusUnderbond>();
			CusUnderbond underbond2 = Factory.New<CusUnderbond>();

			underbond1.C4_MAWB = "08134635134";
			underbond1.C4_FlightNo = "QF016";

			underbond2.C4_MAWB = "08134635134";
			underbond2.C4_FlightNo = "QF020";

			Factory.Save();

			CMRCARSTMessage message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3CBD BHF7 751F:1+8'
DTM+9:20051104171710062728:ZZZ'
DTM+132:20051104:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+016++6+QF::3'
LOC+12+AUPER::6'
LOC+4+5961P::95'
NAD+MR+FGC344L::95'
NAD+UD+75066013018::95'
RFF+ABO:B00005683/1/PER2::2'
RFF+MWB:08134635134'
DOC+1'
PAC+0000029'
UNT+16+000001'".Replace("\r\n", "");
			message1.SetEM_LinkedObject();

			CMRCARSTMessage message2 = Factory.New<CMRCARSTMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3CBD BHF7 751F:1+8'
DTM+9:20051104171710062728:ZZZ'
DTM+132:20051104:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+020++6+QF::3'
LOC+12+AUPER::6'
LOC+4+5961P::95'
NAD+MR+FGC344L::95'
NAD+UD+75066013018::95'
RFF+ABO:B00005683/1/PER2::2'
RFF+MWB:08134635134'
DOC+1'
PAC+0000029'
UNT+16+000001'".Replace("\r\n", "");
			message2.SetEM_LinkedObject();

			AssertEquals(1, underbond1.Messages.Count);
			AssertEquals(1, underbond2.Messages.Count);
		}

		public void TestTwoCARSTMessagesWithOneExistingUnderbond()
		{
			CusUnderbond underbond1 = Factory.New<CusUnderbond>();

			underbond1.C4_MAWB = "08134635134";
			underbond1.C4_FlightNo = "QF016";

			Factory.Save();

			CMRCARSTMessage message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3CBD BHF7 751F:1+8'
DTM+9:20051104171710062728:ZZZ'
DTM+132:20051104:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+016++6+QF::3'
LOC+12+AUPER::6'
LOC+4+5961P::95'
NAD+MR+FGC344L::95'
NAD+UD+75066013018::95'
RFF+ABO:B00005683/1/PER2::2'
RFF+MWB:08134635134'
DOC+1'
PAC+0000029'
UNT+16+000001'".Replace("\r\n", "");
			message1.SetEM_LinkedObject();

			CMRCARSTMessage message2 = Factory.New<CMRCARSTMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3ABD BHF7 751F:1+8'
DTM+9:20051104171710062728:ZZZ'
DTM+132:20051104:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+016++6+QF::3'
LOC+12+AUPER::6'
LOC+4+5961P::95'
NAD+MR+FGC344L::95'
NAD+UD+75066013018::95'
RFF+ABO:B00005683/1/PER2::2'
RFF+MWB:08134635134'
DOC+1'
PAC+0000029'
UNT+16+000001'".Replace("\r\n", "");
			message2.SetEM_LinkedObject();

			AssertEquals(2, underbond1.Messages.Count);
		}

		public void TestTwoCARSTMessagesWithDifferentFlightNumbersOnOneExistingUnderbond()
		{
			CusUnderbond underbond1 = Factory.New<CusUnderbond>();

			underbond1.C4_MAWB = "08134635134";
			underbond1.C4_FlightNo = "QF016";

			Factory.Save();

			CMRCARSTMessage message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3CBD BHF7 751F:1+8'
DTM+9:20051104171710062728:ZZZ'
DTM+132:20051104:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+016++6+QF::3'
LOC+12+AUPER::6'
LOC+4+5961P::95'
NAD+MR+FGC344L::95'
NAD+UD+75066013018::95'
RFF+ABO:B00005683/1/PER2::2'
RFF+MWB:08134635134'
DOC+1'
PAC+0000029'
UNT+16+000001'".Replace("\r\n", "");
			message1.SetEM_LinkedObject();

			CMRCARSTMessage message2 = Factory.New<CMRCARSTMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3CBD BHF7 751F:1+8'
DTM+9:20051104171710062728:ZZZ'
DTM+132:20051104:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+020++6+QF::3'
LOC+12+AUPER::6'
LOC+4+5961P::95'
NAD+MR+FGC344L::95'
NAD+UD+75066013018::95'
RFF+ABO:B00005683/1/PER2::2'
RFF+MWB:08134635134'
DOC+1'
PAC+0000029'
UNT+16+000001'".Replace("\r\n", "");
			message2.SetEM_LinkedObject();

			AssertEquals(1, underbond1.Messages.Count);
			var underbond2 = Factory.LoadTop1<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_FlightNo, "QF020"));
			AssertNull(underbond2);
		}

		public void TestThreeURRMessagesWithTwoFlightNumbersAndNoExistingUnderbonds()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "P026P";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CMRUBMREQRMessage message1 = Factory.New<CMRUBMREQRMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+402G DF4F 6CF6:1+32'
DTM+9:20060113152512156441:ZZZ'
DTM+132:20060114:102'
FTX+AAH+++FFK334E00000000696872'
TDT+20+003++6+OS::3'
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
			message1.SetEM_LinkedObject();

			CMRUBMREQRMessage message2 = Factory.New<CMRUBMREQRMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
PAC+0000004'
RFF+MWB:25771563030'
UNT+19+000001'".Replace("\r\n", "");
			message2.SetEM_LinkedObject();

			CMRUBMREQRMessage message3 = Factory.New<CMRUBMREQRMessage>();
			message3.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+402G DF4F 6CF6:1+32'
DTM+9:20060113152512156441:ZZZ'
DTM+132:20060114:102'
FTX+AAH+++FFK334E00000000696872'
TDT+20+003++6+OS::3'
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
PAC+0000007'
RFF+MWB:25771563030'
UNT+19+000001'".Replace("\r\n", "");
			message3.SetEM_LinkedObject();

			var underbond1 = Factory.LoadTop1<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_FlightNo, "OS003"));
			AssertEquals(2, underbond1.Messages.Count);
			var underbond2 = Factory.LoadTop1<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_FlightNo, "OS001"));
			AssertEquals(1, underbond2.Messages.Count);
		}

		[TestDate(2005, 10, 26)]
		public void TestAddNewUnderbondUnderMAWB()
		{
			ZDateTime arrivalDate = new ZDateTime(2005, 10, 24);
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "40691102955";
			mAWB.CM_ArrivalDate = arrivalDate.AddDays(-1);
			Factory.Save();
			AssertEquals(0, mAWB.AllUnderbonds.Count);

			AirCargoUnderbondLoaderOrCreator creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			CusUnderbond testUnderbond = creator.CreateRecord("40691102955", "", "QF123", arrivalDate, "AUSYD", 23, "", "AIR", "9914N", "22Q3", true);
			AssertEquals("QF123", testUnderbond.C4_FlightNo);
			AssertEquals("AIR", testUnderbond.C4_ModeOfMovement);
			AssertEquals(1, mAWB.AllUnderbonds.Count);
			AssertEquals(mAWB.PK, mAWB.AllUnderbonds[0].C4_ParentID);
		}

		[TestDate(2005, 10, 26)]
		public void TestDoesNotAddNewUnderbondToOldMAWB()
		{
			var arrivalDate = new ZDateTime(2005, 10, 24);
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "40691102955";
			mAWB.CM_ArrivalDate = arrivalDate.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value - 1);
			Factory.Save();
			AssertEquals(0, mAWB.AllUnderbonds.Count);

			var creator = new AirCargoUnderbondLoaderOrCreator(Factory);
			var testUnderbond = creator.CreateRecord("40691102955", "", "QF123", arrivalDate, "AUSYD", 23, "", "AIR", "9914N", "22Q3", true);
			AssertEquals("QF123", testUnderbond.C4_FlightNo);
			AssertEquals("AIR", testUnderbond.C4_ModeOfMovement);
			AssertEquals(0, mAWB.AllUnderbonds.Count);
		}
	}
}
