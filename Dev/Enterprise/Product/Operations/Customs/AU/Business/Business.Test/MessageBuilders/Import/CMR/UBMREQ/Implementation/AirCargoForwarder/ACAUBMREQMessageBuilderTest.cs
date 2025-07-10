using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ACAUBMREQMessageBuilderTest : UBMREQMessageBuilderAbstractTest
	{
		public override void TestCreateEndToEnd()
		{
			#region ExpectedMessage

			ZString expectedMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+31:::UBMREQ+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9'
RFF+ACD:DCL'
NAD+VW+41065894724::95'
TDT+1++ROA'
TDT+20+122++6+QF::3'
LOC+4+9914N::95'
LOC+5+9920A::95'
LOC+11+9920A::95'
DTM+132:20050405:102'
CNI++:::I'
RFF+MWB:08112287041'
GID+1'
PAC+10'
UNT+15+<<MSGNO PLACEHOLDER>>'
";
			#endregion

			underbond.LinkedObject = hAWB;
			underbond.C4_MovementReason = "DCL";
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "41065894724");
			underbond.C4_ModeOfMovement = "ROA";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 4, 5);
			mAWB.CM_MAWB = "08112287041";
			mAWB.CM_FlightNo = "QF122";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_OriginPremiseID = "9920A";
			underbond.C4_IsMoveFromDischarge = true;
			underbond.C4_DischargePremiseID = "9920A";
			hAWB.CS_PiecesManifested = 10;

			AssertMultilineEquals("ExpectedMessage", expectedMessage.Replace("\r\n", ""), GeneratedMessage, '\'');
		}

		public override void TestWithdrawEndToEnd()
		{
			#region ExpectedMessage

			ZString expectedMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+31:::UBMREQ+<<SENDERS REFERENCE PLACE HOLDER>>/DAT0:1+50'
NAD+VW+41065894724::95'
TDT+20+122++6+QF::3'
LOC+4+9914N::95'
LOC+5+9920A::95'
DTM+132:20050405:102'
UNT+8+<<MSGNO PLACEHOLDER>>'
";
			#endregion

			mAWB.CM_FlightNo = "QF122";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 4, 5);
			mAWB.CM_MAWB = "123";
			hAWB.CS_HAWB = "321";
			hAWB.CS_PiecesManifested = 10;
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_OriginPremiseID = "9920A";
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "41065894724");
			messageSubType = Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertMultilineEquals("ExpectedMessage", expectedMessage.Replace("\r\n", ""), GeneratedMessage, '\'');
		}

		public override void TestEstimatedDateOfArrival()
		{
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 3, 1);
			string expected = "DTM+132:20050301:102'";
			Assert("DTM", Builder.GeneratedMessageStrings[0].Contains(expected));
		}

		public override void TestPopulateTDT()
		{
			underbond.C4_ModeOfMovement = "AIR";
			mAWB.CM_FlightNo = "QF008";
			string expected = "TDT+1++AIR'TDT+20+008++6+QF::3'";
			Assert("TDT", Builder.GeneratedMessageStrings[0].Contains(expected));
		}

		public void TestPopulateLocationsForTransit()
		{
			hAWB.CS_RL_NKDestination = "NZAKL";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			string expected = "LOC+20+NZAKL::6'";
			Assert("Ultimate destination", Builder.GeneratedMessageStrings[0].Contains(expected));
		}

		#region Implementation

		CusMAWB mAWB;
		CusHAWB hAWB;

		protected override void SetUp()
		{
			base.SetUp();
			mAWB = Factory.New<CusMAWB>();
			hAWB = mAWB.ChildBills.AddNew();
			underbond.LinkedObject = hAWB;
			underbond.C4_ModeOfMovement = "AIR";//Segment group4 is generated
		}

		UBMREQMessageBuilder fBuilder;
		protected override UBMREQMessageBuilder Builder
		{
			get
			{
				if (fBuilder == null)
				{
					fBuilder = new UBMREQMessageBuilder(new CusHAWBUnderbondMovementRequestHeader(underbond, underbond.HAWBLinked));
					fBuilder.MessageSubType = messageSubType;
					fBuilder.Messages = underbond.Messages;
				}
				return fBuilder;
			}
		}

		#endregion
	}
}
