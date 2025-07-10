using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	sealed class CusHawbValidationTests : BusinessObjectValidationTestCase
	{
		public void TestValidationWhenHAWBProfileDifferentFromMAWBProfile()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "AAA";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_FolioReference = "AAA";
			AssertNoWarning(hawb.CS_FolioReferenceInfo, "There is a mismatch between this HAWB's Profile and its parent MAWB's. The parent MAWB has value 'AAA'.");
			hawb.CS_FolioReference = "BBB";
			AssertHasWarning(hawb.CS_FolioReferenceInfo, "There is a mismatch between this HAWB's Profile and its parent MAWB's. The parent MAWB has value 'AAA'.");
		}

		public void TestValidationWhenHAWBCTODifferentFromMAWBShed()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CargoTerminalOperatorAirportAndShed = "AAA";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CargoTerminalOperatorAirportAndShed = "AAA";
			AssertNoMessageError(hawb.CargoTerminalOperatorAirportAndShedInfo, "There is a mismatch between this HAWB's CTO and its parent MAWB's. The parent MAWB has value 'AAA'.");
			hawb.CargoTerminalOperatorAirportAndShed = "BBB";
			AssertHasMessageError(hawb.CargoTerminalOperatorAirportAndShedInfo, "There is a mismatch between this HAWB's CTO and its parent MAWB's. The parent MAWB has value 'AAA'.");
		}

		public void TestValidationWhenNoMawbIsAttachedEgWhenTheMawbIsDeleted()
		{
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_IsMasterHouse = true;
			hawb.Validation.ValidateAll();
			Assert("All OK, validation ran without explosion", true);
		}

		public void TestShipmentDescriptionCodeSDCValidation()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Validation.ValidateAll();
			AssertHasErrorContaining(hawb.ShipmentDescriptionCodeInfo, "Please enter a Shipment Description Code");
			hawb.ShipmentDescriptionCode = "X";
			AssertHasErrorContaining(hawb.ShipmentDescriptionCodeInfo, "Please select a valid Shipment Description Code");
			hawb.ShipmentDescriptionCode = "T";
			AssertNoErrorContaining(hawb.ShipmentDescriptionCodeInfo, "Please select a valid Shipment Description Code");

			mawb.ShipmentDescriptionCode = "M";
			hawb.ShipmentDescriptionCode = "T";
			AssertHasMessageErrorContaining(hawb.ShipmentDescriptionCodeInfo, "SDC cannot be T");

			mawb.CM_ArrivalDate = ZDateTime.Empty;
			hawb.CS_PiecesLanded = 0;
			Assert(hawb.IsPrearrival);
			hawb.ShipmentDescriptionCode = "M";
			AssertHasErrorContaining(hawb.ShipmentDescriptionCodeInfo, "SDC=T");
			AssertHasErrorContaining(hawb.ShipmentDescriptionCodeInfo, "SDC");
			hawb.ShipmentDescriptionCode = "T";
			AssertNoErrorContaining(hawb.ShipmentDescriptionCodeInfo, "SDC=T");
			mawb.CM_ArrivalDate = ZDateTime.BrettsBirthday;
			hawb.CS_PiecesLanded = 1;
			Assert(!hawb.IsPrearrival);
			hawb.ShipmentDescriptionCode = "M";
			AssertNoErrorContaining(hawb.ShipmentDescriptionCodeInfo, "SDC=T");
			TestSdcForEurope(hawb, hawb.ShipmentDescriptionCodeInfo);
		}

		public void TestValidateNprForUfo()
		{
			var ufo = Factory.New<CusMAWB>();
			ufo.InitialiseUFO();
			ufo.Validation.ValidateAll();
			AssertHasError(ufo.NumberOfPiecesReceivedInfo, "NPR cannot be zero for a UFO. ");
			AssertNoErrorContaining(ufo.NumberOfPiecesReceivedInfo, "NPR cannot be zero for a UFO. You should cancel your changes without saving");
			ufo.NumberOfPiecesReceived = 10;
			ufo.Validation.ValidateAll();
			AssertNoError(ufo.NumberOfPiecesReceivedInfo, "NPR cannot be zero for a UFO. ");
			AssertNoErrorContaining(ufo.NumberOfPiecesReceivedInfo, "NPR cannot be zero for a UFO. You should cancel your changes without saving");
			Factory.Save();
			ufo.NumberOfPiecesReceived = 0;
			AssertHasErrorContaining(ufo.NumberOfPiecesReceivedInfo, "NPR cannot be zero for a UFO. You should cancel your changes without saving");
		}

		internal static void TestSdcForEurope(ICcsukCusAwb awb, ZPropertyInfo info)
		{
			awb.AirportOfOrigin = "DEFRA";
			awb.AirportOfArrival = "LHR";
			awb.AirportOfDestination = "LHR";
			foreach (var s in new ZString[] { "C", "E", "M", "T" })
			{
				awb.ShipmentDescriptionCode = s;
				AssertNoErrorContaining(info, "SDC");
			}

			awb.AirportOfOrigin = "USATL";
			foreach (var s in new ZString[] { "C", "E" })
			{
				awb.ShipmentDescriptionCode = s;
				AssertHasErrorContaining(info, "Origin outside EU requires SDC T or M");
			}
			foreach (var s in new ZString[] { "M", "T" })
			{
				awb.ShipmentDescriptionCode = s;
				AssertNoErrorContaining(info, "Origin outside EU requires SDC T or M");
			}

			awb.AirportOfOrigin = "DEFRA";
			awb.AirportOfArrival = "LHR";
			awb.AirportOfDestination = "ATL";
			foreach (var s in new ZString[] { "M", "T" })
			{
				awb.ShipmentDescriptionCode = s;
				AssertNoErrorContaining(info, "Origin inside EU with destination outside requires SDC T or M");
			}
			foreach (var s in new ZString[] { "C", "E" })
			{
				awb.ShipmentDescriptionCode = s;
				AssertHasErrorContaining(info, "Origin inside EU with destination outside requires SDC T or M");
			}
		}

		public void TestHawbUsesOwnLookupsForShedAndAirportForValidation()
		{
			ShedTest.CreateShed(Factory, "GB", "LTNLCS", "LONDON LUTON CARGO at Luton", portName: "Luton");
			Factory.Save();

			var mawb = Factory.New<CusMAWB>();
			mawb.CargoTerminalOperator = "LCS";  // only at Luton
			mawb.CargoTerminalOperatorAirport = "LBA";  //Leeds
			AssertHasMessageErrorContaining(mawb.CargoTerminalOperatorAirportAndShedInfo, "not in the list");
			var hawb = mawb.ChildBills.AddNew();
			hawb.CargoTerminalOperator = "LCS";
			hawb.CargoTerminalOperatorAirport = "LTN";  //Luton
			AssertNoMessageErrorContaining(hawb.CargoTerminalOperatorAirportAndShedInfo, "not in the list");

			mawb.CargoTerminalOperator = "LCS";
			mawb.CargoTerminalOperatorAirport = "LTN";
			AssertNoMessageErrorContaining(mawb.CargoTerminalOperatorAirportAndShedInfo, "not in the list");
			hawb.CargoTerminalOperator = "LCS";
			hawb.CargoTerminalOperatorAirport = "LBA";
			AssertHasMessageErrorContaining(hawb.CargoTerminalOperatorAirportAndShedInfo, "not in the list");
		}

		public void TestCheckCS_RL_NKLoadPort()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			CusMawbValidationTests.RunTestPort(cusHawb.CS_RL_NKLoadPortInfo, "AUSYD", "GBLHR", "X");
			cusMawb.MasterLevelHouseHelper.Validation.ValidateCS_RL_NKLoadPort();
			AssertEquals("Worker's load port has no notifications when empty", 0, cusMawb.MasterLevelHouseHelper.CS_RL_NKLoadPortInfo.Notifications.Count());
		}

		public void TestCheckCS_RL_NKDischargePort()
		{
			CusHAWB.CS_RL_NKDischargePort = "LON";
			AssertHasErrorContaining(CusHAWB.CS_RL_NKDischargePortInfo, "LON is not acceptable");
			CusHAWB.CS_RL_NKDischargePort = "LHR";
			AssertNoErrorContaining(CusHAWB.CS_RL_NKDischargePortInfo, "LON is not acceptable");
		}

		public void TestCheckCS_HAWBWhenChangingSerialNumber()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.CS_HAWB = "12345678";
			cusHawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			Factory.Save();
			cusHawb.CS_HAWB = "23456789";

			AssertHasMessageErrorContaining(cusHawb.CS_HAWBInfo, "You should first check whether you need to delete the record from CCS-UK and recreate it with the new serial number. As a controller, you may proceed with caution.");  // blue for CW1 support etc
			var originalLogin = GlbStaff.CurrentUser.GS_LoginName;
			var originalController = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				GlbStaff.CurrentUser.GS_LoginName = "DJC";
				GlbStaff.CurrentUser.GS_IsController = false;
				cusHawb.Validation.ValidateCS_HAWB();
				AssertHasErrorContaining(cusHawb.CS_HAWBInfo, "You should first check whether you need to delete the record from CCS-UK and recreate it with the new serial number. Only your administrator can save this change."); // red error for Joe User
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = originalLogin;
				GlbStaff.CurrentUser.GS_IsController = originalController;
			}
		}

		public void TestCheckCS_HAWB()
		{
			CusHAWB.CS_HAWB = "";
			AssertNoMessageErrors(CusHAWB.CS_HAWBInfo);
			CusHAWB.CS_HAWB = "X";
			AssertNoMessageError(CusHAWB.CS_HAWBInfo, "HAWB must be 8 characters long");
			AssertEquals("HAWB padded to 8 characters", "0000000X", CusHAWB.CS_HAWB);
			CusHAWB.CS_HAWB = "12345678";
			AssertNoMessageError(CusHAWB.CS_HAWBInfo, "HAWB must be 8 characters long");
			CusHAWB.CS_HAWB = "12345678XXXX";
			AssertHasMessageError(CusHAWB.CS_HAWBInfo, "HAWB must be 8 characters long");

			var mawb = Factory.New<CusMAWB>();
			var one = mawb.ChildBills.AddNew();
			one.CS_HAWB = "ABCD1234";
			var duplicate = mawb.ChildBills.AddNew();
			duplicate.CS_HAWB = one.CS_HAWB;
			AssertHasMessageErrorContaining(duplicate.CS_HAWBInfo, "duplicate");  // blue for CW1 support etc
			var originalLogin = GlbStaff.CurrentUser.GS_LoginName;
			var originalController = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				GlbStaff.CurrentUser.GS_LoginName = "DJC";
				GlbStaff.CurrentUser.GS_IsController = false;
				duplicate.Validation.ValidateCS_HAWB();
				AssertHasErrorContaining(duplicate.CS_HAWBInfo, "duplicate"); // red error for Joe User
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = originalLogin;
				GlbStaff.CurrentUser.GS_IsController = originalController;
			}
		}

		public void TestProfilePimaFolioValidationForShedSecurityRight()
		{
			Environment.Env.Security.AirCcsukShed.IsAllowed = false;
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			Factory.Save();
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKAIR98LHRCAX"; // shed
			var hawb = mawb.ChildBills.AddNew();
			hawb.Validation.ValidateAll();
			AssertHasErrorContaining(hawb.ProfileInfo, "You do not have the security right to use shed functions");
			AssertHasErrorContaining(mawb.ProfileInfo, "You do not have the security right to use shed functions");
			mawb.Profile = "CUKFFW98000LXA";
			hawb.Profile = "CUKFFW98000LXA";
			AssertNoErrorContaining(hawb.ProfileInfo, "You do not have the security right to use shed functions");
			AssertNoErrorContaining(mawb.ProfileInfo, "You do not have the security right to use shed functions");

			Environment.Env.Security.AirCcsukShed.IsAllowed = true;
			mawb.Profile = "CUKAIR98LHRCAX"; // shed
			hawb.Profile = "CUKAIR98LHRCAX"; // shed
			AssertNoErrorContaining(hawb.ProfileInfo, "You do not have the security right to use shed functions");
			AssertNoErrorContaining(mawb.ProfileInfo, "You do not have the security right to use shed functions");
		}

		public void TestProfilePimaFolioValidationForUnrelatedEntity()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var hawb = cusMawb.ChildBills.AddNew();
			hawb.Profile = "CUKFFW98000DAN";
			AssertNoErrorContaining(hawb.ProfileInfo, "unrelated entity");
			hawb.Profile = "CUKAIR98LHRCAX";
			AssertNoErrorContaining(hawb.ProfileInfo, "unrelated entity");

			hawb.Factory.Save();
			hawb.Profile = "CUKFFW98000DAN";
			AssertNoErrorContaining(hawb.ProfileInfo, "unrelated entity");
			hawb.Profile = "CUKAIR98LHRCAX";
			AssertNoErrorContaining(hawb.ProfileInfo, "unrelated entity");

			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			AssertEquals("Pre req - current agent is DAN", "DAN", hawb.AgentBadge);
			AssertEquals("Pre req - current shed LHRCAX", "LHRCAX", hawb.CS_WarehouseLocation);
			hawb.Profile = "CUKFFW98000BBB";
			AssertHasErrorContaining(hawb.ProfileInfo, "unrelated entity");
			AssertEquals("PreReq - agent not affected by the last change to pima", "DAN", hawb.AgentBadge);
			hawb.Profile = "CUKFFW98000DAN";
			AssertNoErrorContaining(hawb.ProfileInfo, "unrelated entity");  // change back to original is OK
			hawb.Profile = "CUKAIR98LHRCAX";
			AssertNoErrorContaining(hawb.ProfileInfo, "unrelated entity");  // change back to original is OK
			hawb.Profile = "CUKAIR98LHRDDD";
			AssertHasErrorContaining(hawb.ProfileInfo, "unrelated entity");

			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck;
			hawb.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			AssertEquals("Pre req - current agent is DAN", "DAN", hawb.AgentBadge);
			AssertEquals("Pre req - current shed LHRCAX", "LHRCAX", hawb.CS_WarehouseLocation);
			hawb.Profile = "CUKFFW98000BBB";
			AssertHasErrorContaining(hawb.ProfileInfo, "unrelated entity");
			AssertEquals("Agent not updated friom new pima", "DAN", hawb.AgentBadge);
			hawb.Profile = "CUKFFW98000DAN";
			AssertNoErrorContaining(hawb.ProfileInfo, "unrelated entity");  // change back to original is OK
			hawb.Profile = "CUKAIR98LHRCAX";
			AssertNoErrorContaining(hawb.ProfileInfo, "unrelated entity");  // change back to original is OK
			hawb.Profile = "CUKAIR98LHRDDD";
			AssertHasErrorContaining(hawb.ProfileInfo, "unrelated entity");
			AssertEquals("Shed not updated friom new pima", "LHRCAX", hawb.CS_WarehouseLocation);
		}

		public void TestCheckCargoTerminalOperator()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", portName: "Heathrow");
			Factory.Save();

			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.ChildBills.Add(CusHAWB);
			CusHAWB.Validation.ValidateAll();
			AssertHasMessageErrorContaining(CusHAWB.CargoTerminalOperatorAirportAndShedInfo, "enter");
			CusHAWB.CargoTerminalOperatorAirportAndShed = "XXXXXX";
			AssertHasMessageErrorContaining(CusHAWB.CargoTerminalOperatorAirportAndShedInfo, "not in the list");
			CusHAWB.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			AssertNoMessageErrorContaining(CusHAWB.CargoTerminalOperatorAirportAndShedInfo, "not in the list");
		}

		public void TestGoodsDescription()
		{
			var cusHawb = Factory.New<CusHAWB>();
			DescriptionOfGoodsTestRunner(cusHawb.CS_GoodsDescriptionInfo);
		}

		internal static void DescriptionOfGoodsTestRunner(ZPropertyInfo descriptionInfo, bool theWordConsolIsBanned = true)
		{
			descriptionInfo.Value = new ZString("Stuff");
			AssertNoMessageErrorContaining(descriptionInfo, "Desc");
			AssertNoMessageErrorContaining(descriptionInfo, "description 'consol' is not acceptable");
			descriptionInfo.Value = new ZString("");
			AssertHasMessageErrorContaining(descriptionInfo, "Desc");
			if (theWordConsolIsBanned)
			{
				descriptionInfo.Value = new ZString("consol");
				AssertHasMessageErrorContaining(descriptionInfo, "description 'consol' is not acceptable");
				descriptionInfo.Value = new ZString("consolidation");
				AssertHasMessageErrorContaining(descriptionInfo, "description 'consol' is not acceptable");
			}
		}

		public void TestCheckCS_ShipmentType()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.AirportOfOrigin = "USATL";
			cusHawb.ShipmentDescriptionCode = "E";
			AssertHasErrorContaining(cusHawb.ShipmentDescriptionCodeInfo, "Prearrivals must have SDC=T");
			cusHawb.CS_PiecesLanded = 10;
			cusHawb.MAWB.CM_ArrivalDate = ZDateTime.Now;
			cusHawb.ShipmentDescriptionCode = "";
			cusHawb.ShipmentDescriptionCode = "E";
			AssertNoErrorContaining(cusHawb.ShipmentDescriptionCodeInfo, "Prearrivals must have SDC=T");
		}

		public void TestCheckCS_ResponsiblePartyID()
		{
			var badges = new BadgeCodeSettingCollection();
			var mcpBadge = new BadgeCodeSetting();
			mcpBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.MCP_CUSDECOnly;
			mcpBadge.BadgeCode = "AMY";
			var ccsukBadge = new BadgeCodeSetting();
			ccsukBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW;
			ccsukBadge.BadgeCode = "ZPE";
			badges.Add(mcpBadge);
			badges.Add(ccsukBadge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);

			AirCargoInventory.Testing.LicencingAndShedRestrictionsTests.EnsureAgentLxa();
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.ChildBills.Add(CusHAWB);
			CusHAWB.Validation.ValidateCS_ResponsiblePartyID();
			AssertHasMessageErrorContaining(CusHAWB.CS_ResponsiblePartyIDInfo, "You have not entered a badge");
			CusHAWB.AgentBadge = "ABC";
			AssertHasMessageErrorContaining(CusHAWB.CS_ResponsiblePartyIDInfo, "not in the list");
			CusHAWB.AgentBadge = "LXA";
			AssertNoMessageErrors(CusHAWB.CS_ResponsiblePartyIDInfo);
		}

		public void TestValidateNPX()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			mawb.NumberOfPiecesExpected = 0;
			AssertHasErrorContaining(mawb.NumberOfPiecesExpectedInfo, "NPX");
			AssertNoWarningContaining(mawb.NumberOfPiecesExpectedInfo, "UFO");
			mawb.NumberOfPiecesExpected = 1;
			AssertNoErrorContaining(mawb.NumberOfPiecesExpectedInfo, "NPX");
			hawb.CS_PiecesManifested = 0;
			AssertHasErrorContaining(hawb.CS_PiecesManifestedInfo, "NPX");
			hawb.CS_PiecesManifested = 1;
			AssertNoErrorContaining(hawb.CS_PiecesManifestedInfo, "NPX");
			var ufo = Factory.New<CusMAWB>();
			ufo.InitialiseUFO();
			ufo.NumberOfPiecesExpected = 0;
			AssertNoErrorContaining(ufo.NumberOfPiecesExpectedInfo, "NPX");
			AssertHasWarningContaining(ufo.NumberOfPiecesExpectedInfo, "UFO");
		}

		public void TestIsSpent()
		{
			CusHAWB.SetCustomsActionCode("", ZDateTime.Now);
			Assert(!CusHAWB.IsSpent);

			CusHAWB.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestAccepted, ZDateTime.Now);
			Assert(CusHAWB.IsSpent);

			CusHAWB.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestCancelled, ZDateTime.Now);
			Assert(!CusHAWB.IsSpent);
		}

		CusHAWB CusHAWB
		{
			get { return cusHAWB ?? (cusHAWB = Factory.New<CusHAWB>()); }
		}
		CusHAWB cusHAWB;
	}
}
