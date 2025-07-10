using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	class ItfsEtsfDeclarationHelperForChiefTest : TestCaseWithFactory
	{
		public void TestUpdate()
		{
			ShedTest.CreateShed(Factory, "GB", "MANCAX", "Manchester", acpCode: "M");
			Factory.Save();

			var badge = new BadgeCodeSetting();
			badge.BadgeCode = "ABC";
			badge.Direction = "IMP";
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badge.MasterUcrCalculationMode = MucrGenerationStyles.Codes.GemsCcsuk;
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = "33344444444";
			declaration.JE_CustomsProfile = "ABC";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOC123456", Core.Constants.CountryCodes.UnitedKingdom);
			var awb = Factory.New<CusMAWB>();
			awb.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry = declaration.PK;
			var p5 = awb.Messages.AddNew();
			p5.EM_MessageSubType = "P5";
			p5.EM_MessageText = "UNH+JISRBWAAYWH3C0+CUKFSA:1:912:BT'BGM+:::P5+05072012001+7:1207051102:201++HWB:DANIEL50'DOC+703+DANIEL50+++++OLD'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+001+40+++BA:172:3++178:120705:101'LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CWE:129:ZZZ'TDT+12++40'NAD+CB+CAR'GDS+2'QTY+118:10'MEA+WT++KGM:10'FTX+AAA+++HOUSE DAN'DOC+703+DANIEL50+++++NEW'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+001+40+++BA:172:3++178:120705:101'LOC+84:ATL:145:3+85:MAN:145:3+11:MAN:145:3::CAX:129:ZZZ'TDT+12++40'NAD+CB+CAR'GDS+2'QTY+118:10'MEA+WT++KGM:10'FTX+AAA+++HOUSE DAN'UNT+25+JISRBWAAYWH3C0'";
			var helper = new ItfsEtsfDeclarationHelperForChief(declaration);

			Assert(helper.IsEligibleForItsfEntry);
			helper.Update();
			AssertEquals("CWE", declaration.SubLocation);
			AssertEquals("LHR", declaration.JE_LocationOfGoods);
			var ai = declaration.InvoiceLines[0].AdditionalInfos[0];
			AssertEquals("MANCAX GBAEOC123456", ai.CSI_Description);
			AssertEquals("GEN51", ai.CSI_Code);
			AssertEquals("MCAX33344444444", declaration.JE_MasterUCR);
		}

		public void TestEligibleAndConfirmationText()
		{
			var declaration = Factory.New<JobDeclaration>();
			var helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			AssertEquals(false, helper.IsEligibleForItsfEntry);
			AssertContains("inventory", helper.ReasonForNotEligible);

			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			AssertEquals(false, helper.IsEligibleForItsfEntry);
			AssertContains("AEO", helper.ReasonForNotEligible);

			declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOC123456", Core.Constants.CountryCodes.UnitedKingdom);
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			AssertEquals(false, helper.IsEligibleForItsfEntry);
			AssertContains("linked", helper.ReasonForNotEligible);

			var awb = Factory.New<CusMAWB>();
			awb.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry = declaration.PK;
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			AssertEquals(false, helper.IsEligibleForItsfEntry);
			AssertContains("P5", helper.ReasonForNotEligible);

			var p5 = awb.Messages.AddNew();
			p5.EM_MessageSubType = "P5";
			p5.EM_MessageText = "UNH+JISRBWAAYWH3C0+CUKFSA:1:912:BT'BGM+:::P5+05072012001+7:1207051102:201++HWB:DANIEL50'DOC+703+DANIEL50+++++OLD'UNT+25+JISRBWAAYWH3C0'";  // Illegal
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			AssertEquals(false, helper.IsEligibleForItsfEntry);
			AssertContains("Shed codes", helper.ReasonForNotEligible);

			p5.EM_MessageText = "UNH+JISRBWAAYWH3C0+CUKFSA:1:912:BT'BGM+:::P5+05072012001+7:1207051102:201++HWB:DANIEL50'DOC+703+DANIEL50+++++OLD'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+001+40+++BA:172:3++178:120705:101'LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CWE:129:ZZZ'TDT+12++40'NAD+CB+CAR'GDS+2'QTY+118:10'MEA+WT++KGM:10'FTX+AAA+++HOUSE DAN'DOC+703+DANIEL50+++++NEW'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+001+40+++BA:172:3++178:120705:101'LOC+84:ATL:145:3+85:MAN:145:3+11:MAN:145:3::CAX:129:ZZZ'TDT+12++40'NAD+CB+CAR'GDS+2'QTY+118:10'MEA+WT++KGM:10'FTX+AAA+++HOUSE DAN'UNT+25+JISRBWAAYWH3C0'";
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			AssertEquals(true, helper.IsEligibleForItsfEntry);
			AssertEquals("", helper.ReasonForNotEligible);
			AssertContains("from LHRCWE to MANCAX", helper.GetConfirmationText());
		}

		public void TestRemoveHawbWhenOnlyParentBasicHasP5()
		{
			var etsfBasic = Factory.New<CusMAWB>();
			etsfBasic.CM_MAWB = "05072012001";
			etsfBasic.CargoTerminalOperatorAirportAndShed = "MANCAX";
			var p5 = etsfBasic.Messages.AddNew();
			p5.EM_MessageSubType = "P5";
			p5.EM_MessageText = "UNH+JISRBWAAYWH3C0+CUKFSA:1:912:BT'BGM+:::P5+05072012001+7:1207051102:201'DOC+703+05072012001+++++OLD'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+001+40+++BA:172:3++178:120705:101'LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CWE:129:ZZZ'TDT+12++40'NAD+CB+CAR'GDS+2'QTY+118:10'MEA+WT++KGM:10'FTX+AAA+++HOUSE DAN'DOC+703+05072012001+++++NEW'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+001+40+++BA:172:3++178:120705:101'LOC+84:ATL:145:3+85:MAN:145:3+11:MAN:145:3::CAX:129:ZZZ'TDT+12++40'NAD+CB+CAR'GDS+2'QTY+118:10'MEA+WT++KGM:10'FTX+AAA+++HOUSE DAN'UNT+25+JISRBWAAYWH3C0'";

			// Now deconsolidate the basic at the ETSF:
			var mawb = etsfBasic;
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();

			// And now add a declaration for one hawb
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOC123456", Core.Constants.CountryCodes.UnitedKingdom);
			hawb2.CS_JE_CustomsFormalEntry = declaration.PK;
			var helper = new ItfsEtsfDeclarationHelperForChief(declaration);

			// Check the function works
			AssertEquals("", helper.ReasonForNotEligible);
			AssertEquals(true, helper.IsEligibleForItsfEntry);
			AssertContains("from LHRCWE to MANCAX", helper.GetConfirmationText());
			AssertContains("Master Air Waybill 050-72012001", helper.GetConfirmationText());
		}

		public void TestFindAwbThreeWays()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRABC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			var basic = Factory.New<CusMAWB>();
			basic.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry = declaration.PK;
			helper.FindAwb();
			AssertEquals(basic, helper.Awb);

			basic.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			helper.FindAwb();
			AssertEquals(null, helper.Awb);

			basic.CM_MAWB = "125-12345678";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.CargoTerminalOperator = "ABC";
			declaration.JE_MasterUCR = "HABC12512345678";
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			helper.FindAwb();
			AssertEquals(basic, helper.Awb);

			var hawb = basic.ChildBills.AddNew();
			hawb.CS_HAWB = "DANIEL00";
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			helper.FindAwb();
			AssertEquals(basic, helper.Awb);
			declaration.JE_MasterUCR = "HABC12512345678DANIEL00";
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			helper.FindAwb();
			AssertEquals(hawb, helper.Awb);

			basic.ChildBills.RemoveAndDelete(hawb);
			helper = new ItfsEtsfDeclarationHelperForChief(declaration);
			helper.FindAwb();
			AssertEquals(null, helper.Awb);
		}
	}
}
