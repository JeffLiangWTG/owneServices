using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class AirCTOExpectedArrivalCusUnderbondFactoryTestCase : TestCaseWithFactory
	{
		public void TestLoadOrCreateUnderbondParentForMAWBMovementWhereMAWBExists()
		{
			CTOCusHAWB master = CreateCTOHAWB("QF123", new ZDateTime(2005, 6, 21), "08110984526");
			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			AssertEquals("Result", master, result);
		}

		public void TestLoadOrCreateUnderbondParentForMAWBMovementWhereMAWBDoesntExist()
		{
			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			AssertEquals("Result", null, result);
		}

		public void TestIsInterestedInUBMREQR()
		{
			AssertEquals("IsInterestedInUBMREQR", false, UnderbondFactory.IsInterestedInUBMREQRInternal(HAWBExpectedArrivalMessage));
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9913C";
			GlbBranch.CurrentBranch.OrgProxy.OH_IsAirCTO = true;
			AssertEquals("IsInterestedInUBMREQR", true, UnderbondFactory.IsInterestedInUBMREQRInternal(MAWBExpectedArrivalMessage));
		}

		public void TestIssue8834()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "EM18N";
			GlbBranch.CurrentBranch.OrgProxy.OH_IsAirCTO = true;

			CTOCusMAWB cusMAWB = Factory.New<CTOCusMAWB>();
			cusMAWB.CM_FlightNo = "CX171";
			cusMAWB.CM_ArrivalDate = new ZDateTime(2005, 10, 27);
			CTOCusHAWB result = cusMAWB.ChildBills.AddNew();
			result.CS_HAWB = "16056092761";

			HAWBIssue8834Message.SetEM_LinkedObject();
			AssertNotNull("Failed to process Underbond adn set parent", HAWBIssue8834Message.EM_LinkedObject);
		}

		#region Implementation

		CTOCusHAWB CreateCTOHAWB(ZString flightNumber, ZDateTime estArrivalDate, ZString mAWB)
		{
			CTOCusMAWB cusMAWB = Factory.New<CTOCusMAWB>();
			cusMAWB.CM_FlightNo = flightNumber;
			cusMAWB.CM_ArrivalDate = estArrivalDate;
			CTOCusHAWB result = cusMAWB.ChildBills.AddNew();
			result.CS_HAWB = mAWB;
			return result;
		}

		AirCTOExpectedArrivalCusUnderbondFactory fUnderbondFactory;
		AirCTOExpectedArrivalCusUnderbondFactory UnderbondFactory
		{
			get
			{
				if (fUnderbondFactory == null)
				{
					fUnderbondFactory = new AirCTOExpectedArrivalCusUnderbondFactory();
				}
				return fUnderbondFactory;
			}
		}

		CMRUBMREQRMessage fHAWBExpectedArrivalMessage;
		CMRUBMREQRMessage HAWBExpectedArrivalMessage
		{
			get
			{
				if (fHAWBExpectedArrivalMessage == null)
				{
					fHAWBExpectedArrivalMessage = Factory.New<CMRUBMREQRMessage>();
					fHAWBExpectedArrivalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+516H I417 EAF:1+32'
DTM+9:20050630220525888545:ZZZ'
DTM+132:20050621:102'
FTX+AAH+++AAA374MU00000055/SYD2'
TDT+20+123++6+QF::3'
TDT+1++ROA'
LOC+5+9914N::95'
LOC+4+9913C::95'
NAD+MR+AAA374M::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:AQS'
DOC+1'
PAC+++AIR:67:95'
PAC+0000050'
RFF+MWB:08110984526'
RFF+HWB:4'
UNT+18+000001'".Replace("\r\n", "");
				}
				return fHAWBExpectedArrivalMessage;
			}
		}

		CMRUBMREQRMessage fMAWBExpectedArrivalMessage;
		CMRUBMREQRMessage MAWBExpectedArrivalMessage
		{
			get
			{
				if (fMAWBExpectedArrivalMessage == null)
				{
					fMAWBExpectedArrivalMessage = Factory.New<CMRUBMREQRMessage>();
					fMAWBExpectedArrivalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+516H I417 EAF:1+32'
DTM+9:20050630220525888545:ZZZ'
DTM+132:20050621:102'
FTX+AAH+++AAA374MU00000055/SYD2'
TDT+20+123++6+QF::3'
TDT+1++ROA'
LOC+5+9914N::95'
LOC+4+9913C::95'
NAD+MR+AAA374M::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:AQS'
DOC+1'
PAC+++AIR:67:95'
PAC+0000050'
RFF+MWB:08110984526'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fMAWBExpectedArrivalMessage;
			}
		}

		CMRUBMREQRMessage HAWBIssue8834Message
		{
			get
			{
				if (fHAWBIssue8834Message == null)
				{
					fHAWBIssue8834Message = Factory.New<CMRUBMREQRMessage>();
					fHAWBIssue8834Message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3857 2F5F HA1F:1+32'
DTM+9:20051027145837261444:ZZZ'
DTM+132:20051027:102'
FTX+AAH+++FGG669CU00000396/PER1'
TDT+20+171++6+CX::3'
TDT+1++ROA'
LOC+5+EM18N::95'
LOC+4+S006C::95'
NAD+MR+FGA376X::95'
NAD+UD+88081675701::95'
RFF+ABO:U00000396/PER1::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000038'
RFF+MWB:16056092761'
UNT+19+000001'".Replace("\r\n", "");
				}
				return fHAWBIssue8834Message;
			}
		}
		CMRUBMREQRMessage fHAWBIssue8834Message;

		#endregion
	}
}
