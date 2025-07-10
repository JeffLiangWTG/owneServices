using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory_Test : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestMessageAlreadyLinkedToOtherObjectOnFoundUnderbond()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9913C";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusMAWB randomMawb = Factory.New<CusMAWB>();

			CusHAWB randomHawb = randomMawb.ChildBills.AddNew();
			randomHawb.Messages.Add(HAWBExpectedArrivalMessage);

			CusMAWB mawbWithHawbWithUnderbond = Factory.New<CusMAWB>();
			mawbWithHawbWithUnderbond.CM_MAWB = "08110984526";
			mawbWithHawbWithUnderbond.CM_FlightNo = "QF123";
			CusHAWB hawbWithUnderbond = mawbWithHawbWithUnderbond.ChildBills.AddNew();
			hawbWithUnderbond.CS_HAWB = "4";
			CusUnderbond matchingUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)hawbWithUnderbond).Underbonds.AddNew();
			matchingUnderbond.C4_OriginPremiseID = "9914N";
			matchingUnderbond.C4_DestinationPremiseID = "9913C";

			matchingUnderbond.C4_SendersMessageReference = "U0000234";

			Factory.Save();

			AssertEquals(hawbWithUnderbond, matchingUnderbond.HAWBLinked);

			UnderbondFactory.ProcessIncomingUBMREQR(HAWBExpectedArrivalMessage);

			AssertEquals(1, matchingUnderbond.Messages.Count);
			AssertNotEquals(HAWBExpectedArrivalMessage, matchingUnderbond.Messages[0]);
			AssertEquals(HAWBExpectedArrivalMessage.EM_MessageText, matchingUnderbond.Messages[0].EM_MessageText);
		}

		public void TestLoadOrCreateUnderbondParentWhereMAWBNotExist()
		{
			var messageParent = (CusUnderbond)UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			AssertEquals("Created AU underbond", Customs.Business.CusUnderbondApplicationCodeList.Codes.AUUnderbond, messageParent?.C4_ApplicationCode);
			AssertNull("Created standalone underbond", messageParent.MAWB);
		}

		public void TestLoadOrCreateUnderbondParentForMAWBMovementWhereMAWBExists()
		{
			var auMawb = CreateMAWB("08110984526");
			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			Assert(auMawb.AllUnderbonds[0].Messages.Contains(MAWBExpectedArrivalMessage));
		}

		public void TestLoadOrCreateUnderbondParentIgnoresNZUnderbond()
		{
			var setupFactory = new BusinessObjectFactory();
			var nzMawb = CreateMAWB(setupFactory, "08110984526", withUnderbond: true);

			nzMawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			nzMawb.Underbonds[0].C4_ApplicationCode = Customs.Business.CusUnderbondApplicationCodeList.Codes.NZTranshipmentRequest;
			setupFactory.Save();

			var messageParent = (CusUnderbond)UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			AssertEquals("Created AU underbond", Customs.Business.CusUnderbondApplicationCodeList.Codes.AUUnderbond, messageParent?.C4_ApplicationCode);
			AssertNull("Created standalone underbond", messageParent.MAWB);
		}

		public void TestLoadOrCreateUnderbondParentForMAWBMovementWhereMultipleMAWBExists()
		{
			var mawb1 = CreateMAWB("08110984526", withUnderbond: false);
			mawb1.CM_MasterHouseBill = "SUBMASTER";
			var mawb2 = CreateMAWB("08110984526", withUnderbond: false);
			Factory.Save();
			var result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			Assert(mawb1.AllUnderbonds[0].Messages.ContainsByKeyFields(MAWBExpectedArrivalMessage));
			Assert(mawb2.AllUnderbonds[0].Messages.ContainsByKeyFields(MAWBExpectedArrivalMessage));
		}

		public void TestLoadOrCreateUnderbondParentForMAWBMovementWithNonNestedSubMasters()
		{
			var mawb1 = CreateMAWB("08110984526", withUnderbond: false);
			mawb1.CM_MasterHouseBill = "SUBMASTER1";
			var mawb2 = CreateMAWB("08110984526", withUnderbond: false);
			mawb2.CM_MasterHouseBill = "SUBMASTER2";
			Factory.Save();
			MAWBExpectedArrivalMessage.EM_MessageText = MAWBExpectedArrivalMessage.EM_MessageText.Replace("RFF+MWB:08110984526'", "RFF+MWB:08110984526'RFF+HWB:SUBMASTER1'");
			var result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			Assert(mawb1.AllUnderbonds[0].Messages.ContainsByKeyFields(MAWBExpectedArrivalMessage));
			AssertEquals(0, mawb2.AllUnderbonds.Count);
		}

		public void TestLoadOrCreateUnderbondParentForMAWBMovementWithNestedSubMasters()
		{
			var mawb1 = CreateMAWB("08110984526", withUnderbond: false);
			mawb1.CM_MasterHouseBill = "SUBMASTER1";
			var subMasterHouse1 = mawb1.ChildBills.AddNew();
			subMasterHouse1.CS_HAWB = "SUBMASTER2";
			var subMasterHouse2 = mawb1.ChildBills.AddNew();
			subMasterHouse2.CS_HAWB = "SUBMASTER3";
			var mawb2 = CreateMAWB("08110984526", withUnderbond: false);
			mawb2.CM_MasterHouseBill = "SUBMASTER2";
			var mawb3 = CreateMAWB("08110984526", withUnderbond: false);
			mawb3.CM_MasterHouseBill = "SUBMASTER3";
			var mawb4 = CreateMAWB("08110984526", withUnderbond: false);
			mawb4.CM_MasterHouseBill = "SUBMASTER4";
			Factory.Save();
			MAWBExpectedArrivalMessage.EM_MessageText = MAWBExpectedArrivalMessage.EM_MessageText.Replace("RFF+MWB:08110984526'", "RFF+MWB:08110984526'RFF+HWB:SUBMASTER1'");
			var result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			Assert(mawb1.AllUnderbonds[0].Messages.ContainsByKeyFields(MAWBExpectedArrivalMessage));
			Assert(mawb2.AllUnderbonds[0].Messages.ContainsByKeyFields(MAWBExpectedArrivalMessage));
			Assert(mawb3.AllUnderbonds[0].Messages.ContainsByKeyFields(MAWBExpectedArrivalMessage));
			AssertEquals(0, mawb4.AllUnderbonds.Count);
		}

		public void TestExternalUnderbondRequestsDontUpdateOurUnderbond()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM10N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "26028539582";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "008201";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			underbond.C4_FlightNo = "FJ911";
			underbond.C4_SendersMessageReference = "U00002132";
			underbond.C4_DestinationPremiseID = "EM10M";
			underbond.C4_OriginPremiseID = "8553P";
			underbond.C4_PiecesManifested = 15;
			Factory.Save();

			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+1GIC 3IHH 781F:1+32'
DTM+9:20051125171942862345:ZZZ'
DTM+132:20051124:102'
TDT+20+911++6+FJ::3'
TDT+1++ROA'
LOC+5+EM10M::95'
LOC+4+8559M::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000002'
RFF+MWB:26028539582'
RFF+HWB:008201'
UNT+18+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();

			AssertEquals("8553P", underbond.C4_OriginPremiseID);
			AssertEquals("EM10M", underbond.C4_DestinationPremiseID);
			AssertEquals(1, hAWB.AllUnderbonds.Count);
			AssertEquals("EM10M", hAWB.AllUnderbonds[0].C4_OriginPremiseID);
			AssertEquals("8559M", hAWB.AllUnderbonds[0].C4_DestinationPremiseID);
		}

		public void TestActualArrivalTriggeredUBMRRUpdates()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM10M";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsAirCTO = true;
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "40691102955";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "007933";
			CusUnderbond underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			underbond.C4_ParentTableCode = "CM";
			underbond.C4_ArrivalDate = new ZDateTime(2005, 11, 6);
			underbond.C4_ParentID = mAWB.PK;

			Factory.Save();

			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+1880 J225 02GF:1+32'
DTM+9:20051107102554048694:ZZZ'
DTM+132:20051106:102'
TDT+20+032++6+5X::3'
TDT+1++ROA'
LOC+5+EM10M::95'
LOC+4+A018N::95'
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000071'
RFF+MWB:40691102955'
RFF+HWB:007933'
UNT+18+000001'".Replace("\r\n", "");

			message.SetEM_LinkedObject();
			AssertEquals(0, underbond.Messages.Count);
			AssertEquals(1, hAWB.AllUnderbonds[0].Messages.Count);
		}

		public void TestLoadOrCreateUnderbondParentForHAWBMovementWhereHAWBExists()
		{
			CusHAWB hAWB = CreateHAWB("08110984526", "4");
			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(HAWBExpectedArrivalMessage, 1);
			Assert(hAWB.MAWB.AllUnderbonds[0].Messages.Contains(HAWBExpectedArrivalMessage));
		}

		public void TestLoadOrCreateUnderbondParentForHAWBMovementWithNoSendersReference()
		{
			CusMAWB mAWB = CreateMAWB("08110984526");
			mAWB.CM_FlightNo = "";
			mAWB.Underbonds.RemoveAndDeleteAll();
			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(MAWBExpectedArrivalMessage, 1);
			Assert(mAWB.AllUnderbonds[0].Messages.Contains(MAWBExpectedArrivalMessage));

			AssertEquals("QF123", mAWB.AllUnderbonds[0].C4_FlightNo);
		}

		public void TestLoadOrCreateUnderbondParentForHAWBMovementWhereMAWBExists()
		{
			CusMAWB mAWB = CreateMAWB("08110984526");
			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(HAWBExpectedArrivalMessage, 1);
			Assert(mAWB.AllUnderbonds[0].Messages.Contains(HAWBExpectedArrivalMessage));
		}

		public void TestLoadOrCreateUnderbondParentForHAWBMovementWhereMAWBDoesExist()
		{
			AirOrStandAloneCusUnderbondCollection underbonds = new AirOrStandAloneCusUnderbondCollection(Factory);
			underbonds.Load();
			int count = underbonds.Count;

			ICusUnderbondDependentCollectionParent result = UnderbondFactory.LoadOrCreateUnderbondParentInternal(HAWBExpectedArrivalMessage, 1);
			AssertNull("Created new underbond", result);
			underbonds.Load();
			AssertEquals(count + 1, underbonds.Count);
		}

		public void TestUBMREQRDoesNotSetDischargePortOnMAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "26015186990";
			mAWB.CM_FlightNo = "FJ9110";
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			CusUnderbond underbond = mAWB.Underbonds.AddNew();
			underbond.C4_SendersMessageReference = "U00000061";
			underbond.C4_RL_NKDischargePort = "SGSIN";
			underbond.C4_OriginPremiseID = "A011E";
			underbond.C4_DestinationPremiseID = "DK34C";

			Factory.Save();

			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_MessageText = @"
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

			message.SetEM_LinkedObject();
			AssertEquals("AUSYD", mAWB.CM_RL_NKDischargePort);
			AssertEquals("DK34C", underbond.C4_DestinationPremiseID);
			AssertEquals("A011E", underbond.C4_OriginPremiseID);
			AssertEquals("SGSIN", underbond.C4_RL_NKDischargePort);
		}

		public void TestIsInterestedInUBMREQR()
		{
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			currentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9913C";
			currentBranch.OrgProxy.OH_IsUnpackDepot = true;
			AssertEquals("IsInterestedInUBMREQR", true, UnderbondFactory.IsInterestedInUBMREQRInternal(HAWBExpectedArrivalMessage));
		}

		#region Implementation

		CusMAWB CreateMAWB(ZString mawbNumber, bool withUnderbond = true)
		{
			return CreateMAWB(Factory, mawbNumber, withUnderbond);
		}

		CusMAWB CreateMAWB(BusinessObjectFactory factory, ZString mawbNumber, bool withUnderbond = true)
		{
			var mawb = factory.New<CusMAWB>();
			mawb.CM_MAWB = mawbNumber;
			mawb.CM_FlightNo = "QF123";

			if (withUnderbond)
			{
				var underbond = mawb.Underbonds.AddNew();
				underbond.C4_SendersMessageReference = "U0000234";
				underbond.C4_DestinationPremiseID = "9913C";
				underbond.C4_OriginPremiseID = "9914N";
			}

			return mawb;
		}

		CusHAWB CreateHAWB(ZString mAWB, ZString hAWB)
		{
			CusMAWB cusMAWB = CreateMAWB(mAWB);
			CusHAWB result = cusMAWB.ChildBills.AddNew();
			result.CS_HAWB = hAWB;

			return result;
		}

		StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory fUnderbondFactory;
		StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory UnderbondFactory
		{
			get
			{
				if (fUnderbondFactory == null)
				{
					fUnderbondFactory = new StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory();
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
RFF+ABO:U0000234::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:AQS'
DOC+1'
PAC+++AIR:67:95'
PAC+0000050'
RFF+MWB:08110984526'
RFF+HWB:4'
UNT+19+000001'".Replace("\r\n", "");
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
RFF+ABO:U0000234::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:AQS'
DOC+1'
PAC+++AIR:67:95'
PAC+0000050'
RFF+MWB:08110984526'
UNT+18+000001'".Replace("\r\n", "");
				}
				return fMAWBExpectedArrivalMessage;
			}
		}

		#endregion
	}
}
