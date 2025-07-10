using System;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class DeclarationAndAwbMUCRValidationTests : TestCaseWithFactory
	{
		public void TestValidationRegisteredAcrossMultipleDeclarations()
		{
			// Factory.Validation.MainGroup.RegisterValidationType(typeof(CusEntryNumber), MyValidationType) could have registered MyValidationType to handle the validation of ALL CusEntryNumber objects it holds. 
			// So MyValidationType will be fired even for non-GB declarations.   
			// Check we don't fail

			JobDeclaration euDec = null;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				euDec = Factory.New<JobDeclaration>();
				Factory.Validation.MainGroup.RegisterValidationType(typeof(CusEntryNumber), typeof(Helpers.GbCcsukMUCREntryNumValidation));
			}

			var gbDec = Factory.New<JobDeclaration>();
			euDec.JE_MasterUCR = "Anything";
			gbDec.JE_MasterUCR = "AnotherThing";
			Assert("No explosions running GB validation wired to a non-GB declaration", true);
		}

		public void TestMultipleAWBsLinked()
		{
			var declaration = GetCcsukDeclaration();
			declaration.ZG_ShipmentType = ShipmentTypeList.Codes.HouseConsignment;
			var shipment = GetShipment(declaration);
			declaration.JE_MasterUCR = "UCR";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "The AWB that is linked to this declaration");
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This declaration is linked to a shipment but there is no HAWB linked to it.");

			var hawb = GetHawb();
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			hawb.CS_JS = shipment.PK;
			declaration.Validation.ValidateJE_MasterUCR();
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "The AWB that is linked to this declaration");
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This declaration is linked to a shipment but there is no HAWB linked to it.");

			var hawb2 = GetHawb();
			hawb2.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			hawb2.CS_JS = shipment.PK;
			declaration.Validation.ValidateJE_MasterUCR();
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "The AWB that is linked to this declaration");
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This declaration is linked to a shipment but there is no HAWB linked to it.");
		}

		public void TestMUCR_WhenAJobDeclarationIsNotAttached()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.Direction = "IMP";
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.GemsCcsuk;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var hawb = GetHawb();
			hawb.CS_HAWB = "87654321";
			var mawb = hawb.MAWB;
			mawb.CM_MAWB = "00012345678";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = mawb.CM_MAWB;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = hawb.CS_HAWB;

			mawb.CM_JK = consol.PK;
			hawb.CS_JS = shipment.PK;

			var declaration = GetCcsukDeclaration();
			declaration.JE_JS = shipment.PK;
			declaration.JE_LocationOfGoods = "LBA";
			declaration.JE_MasterBill = "08112345678";
			declaration.JE_HouseBill = "12345";
			declaration.JE_LocationOfGoods = "LBA";
			declaration.SubLocation = "ELF";
			declaration.JE_CustomsProfile = "DSK";

			AssertEquals("YELF0811234567800012345", declaration.JE_MasterUCR);
			var oldMUCR = declaration.JE_MasterUCR;
			declaration.JE_MasterUCR = " " + declaration.JE_MasterUCR.Substring(1);
			AssertHasMessageError(declaration.JE_MasterUCRInfo, "This MUCR appears to be invalid for a CCSUK air import. It should look like HBAC11122222222(33333333)(44).");
			declaration.JE_MasterUCR = oldMUCR;
			AssertNoMessageError(declaration.JE_MasterUCRInfo, "This MUCR appears to be invalid for a CCSUK air import. It should look like HBAC11122222222(33333333)(44).");
		}

		public void TestMUCR_ShouldCamparedToRecaculatedValue()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.Direction = "IMP";
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.GemsCcsuk;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var declaration = GetCcsukDeclaration();
			declaration.JE_CustomsProfile = "DSK";
			declaration.JE_MasterBill = "08112345678";
			declaration.JE_HouseBill = "12345";

			AssertEquals("YELF0811234567800012345", declaration.JE_MasterUCR);

			var oldMUCR = declaration.JE_MasterUCR;
			var newMUCR = "A" + declaration.JE_MasterUCR.Substring(1);
			declaration.JE_MasterUCR = newMUCR;
			var messageError =
				$"Using the data available to {BrandingFactory.Instance.ProductName}, the MUCR has been calculated as {oldMUCR} but the value is {newMUCR}. Please check that your value is correct.";
			AssertHasMessageError(declaration.JE_MasterUCRInfo, messageError);
			declaration.JE_MasterUCR = oldMUCR;
			AssertNoMessageError(declaration.JE_MasterUCRInfo, messageError);
		}

		public void TestMUCR_ShouldCamparedToRecaculatedValueWithTrailingSpaces()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.Direction = "IMP";
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.GemsCcsuk;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var declaration = GetCcsukDeclaration();
			declaration.JE_CustomsProfile = "DSK";
			declaration.JE_MasterBill = "08112345678";
			declaration.JE_HouseBill = "";

			AssertEquals("YELF08112345678", declaration.JE_MasterUCR);

			AssertEquals(false, declaration.JE_MasterUCRInfo.Notifications.GetMessageErrors().Contains($"Using the data available to {BrandingFactory.Instance.ProductName}, the MUCR has been calculated as YELF08112345678         but the value is YELF08112345678. Please check that your value is correct."));
			AssertEquals(false, declaration.JE_MasterUCRInfo.Notifications.GetMessageErrors().Contains($"Using the data available to {BrandingFactory.Instance.ProductName}, the MUCR has been calculated as YELF08112345678 but the value is YELF08112345678. Please check that your value is correct."));

			var newMUCR = "A" + declaration.JE_MasterUCR.Substring(1);
			declaration.JE_MasterUCR = newMUCR;

			AssertEquals(false, declaration.JE_MasterUCRInfo.Notifications.GetMessageErrors().Contains($"Using the data available to {BrandingFactory.Instance.ProductName}, the MUCR has been calculated as YELF08112345678         but the value is AELF08112345678. Please check that your value is correct."));
			AssertEquals(true, declaration.JE_MasterUCRInfo.Notifications.GetMessageErrors().Contains($"Using the data available to {BrandingFactory.Instance.ProductName}, the MUCR has been calculated as YELF08112345678 but the value is AELF08112345678. Please check that your value is correct."));
		}

		public void TestStrongLinkUnknownPresence()
		{
			var declaration = GetCcsukDeclaration();
			var hawb = GetHawb();
			hawb.CS_JE_CustomsFormalEntry = declaration.PK;
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
			declaration.JE_MasterUCR = "X";
			AssertHasWarningContaining(declaration.JE_MasterUCRInfo, "not known to be on the CCSUK database");
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "Y";
			AssertNoWarningContaining(declaration.JE_MasterUCRInfo, "not known to be on the CCSUK database");
		}

		public void TestStrongLinkMismatchingSerialNumbersIncludingForItsf()
		{
			var declaration = GetCcsukDeclaration();
			var hawb = GetHawb();
			hawb.CS_JE_CustomsFormalEntry = declaration.PK;
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "X";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "serial number that is not related");
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertNoMessageError(declaration.JE_MasterUCRInfo, "serial number that is not related");

			declaration.JE_CustomsProfile = "POO";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_MasterUCR = "x";
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "whose Badge");
			declaration.JE_CustomsProfile = "DAN";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_MasterUCR = "x";
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "whose Badge");

			declaration.SubLocation = "XXX";
			declaration.JE_MasterUCR = "HYYY0001234567887654321";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "whose Shed");  // XXX & BAC nor YYY & BAC match
			var itfsDeclarationHelper = new Messaging.ItfsEtsfDeclarationHelperForChief(declaration);
			hawb.CargoTerminalOperator = "ELX";
			itfsDeclarationHelper.Update();  // Create the GEN51 statement
			declaration.SubLocation = "BAC";
			declaration.JE_MasterUCR = "HELX0001234567887654321";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "whose Shed");  // JE_Shed mismatches Awb.CargoTerminalOperator, but JE_MasterUCR comprises Awb.CargoTerminalOperator, so no error
		}

		public void TestStrongLinkToShipmentWithoutHawb()
		{
			var declaration = GetCcsukDeclaration();
			declaration.ZG_ShipmentType = ShipmentTypeList.Codes.HouseConsignment;
			var shipment = GetShipment(declaration);
			declaration.JE_MasterUCR = "X";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "shipment but there is no HAWB");
			declaration.ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
			declaration.JE_MasterUCR = "Z";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "shipment but there is no HAWB");
			declaration.ZG_ShipmentType = ShipmentTypeList.Codes.HouseConsignment;
			var hawb = GetHawb();
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			hawb.CS_JS = shipment.PK;
			declaration.JE_MasterUCR = "Y";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "shipment but there is no HAWB");
		}

		public void TestWeakLinkMucrIsSplitBasic()
		{
			var declaration = GetCcsukDeclaration();
			var basic = GetBasic();
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC5554444444433";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a split basic");
			declaration.JE_MasterUCR = "HBAC55544444444        33";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a split basic");
			declaration.JE_MasterUCR = "HBAC55544444444        XX";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR appears to be invalid for a CCSUK air import");
			var split = basic.Splits.AddNew();
			declaration.JE_MasterUCR = "HBAC5554444444466";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a split basic");
			declaration.JE_MasterUCR = "HBAC55544444444        66";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR appears to be invalid for a CCSUK air import");
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "MUCR specifies split 66 on basic");
			split.SplitReference = "66";
			declaration.JE_MasterUCR = "";
			declaration.JE_MasterUCR = "HBAC55544444444        66";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "MUCR specifies split 66 on basic");
		}

		public void TestWeakLinkMucrIsWholeBasic()
		{
			var declaration = GetCcsukDeclaration();
			var basic = GetBasic();
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC55544444444";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a whole basic");
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "3 splits");
			var split01 = basic.Splits.AddNew();
			split01.SplitReference = "01";
			basic.Splits.AddNew();
			basic.Splits.AddNew();
			declaration.JE_MasterUCR = "";
			declaration.JE_MasterUCR = "HBAC55544444444";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a whole basic");
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "3 splits");
			declaration.JE_MasterUCR = "HBAC5554444444401";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a whole basic");

			declaration.JE_CustomsProfile = "POO";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_MasterUCR = "HBAC55544444444";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "Badge");
			declaration.JE_CustomsProfile = "DAN";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_MasterUCR = "";
			declaration.JE_MasterUCR = "HBAC55544444444";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "Badge");

			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "No MAWB could be found from the MUCR");
			declaration.JE_MasterUCR = "QBAC0001234567887654321"; // Q is a crap ACP code
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "No MAWB could be found from the MUCR");
		}

		public void TestWeakLinkMultipleInventoryRecords()
		{
			var declaration = GetCcsukDeclaration();
			var basicBAC = GetBasic();
			var basicCAX = GetBasic();
			basicCAX.CargoTerminalOperator = "CAX";
			basicCAX.AgentBadge = "CCC";
			basicCAX.CargoTerminalOperator = "CAX";
			basicBAC.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			basicBAC.AgentBadge = "BBB";
			basicCAX.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_CustomsProfile = "BBB";
			declaration.JE_TotalNoOfPacks = 68;
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_MasterUCR = "HBAC55544444444";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "No MAWB could be found from the MUCR");
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "Badge");  // checks finds right awb
			declaration.JE_CustomsProfile = "CCC";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_MasterUCR = "HCAX55544444444";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "No MAWB could be found from the MUCR");
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "Badge");  // checks finds right awb
			declaration.JE_MasterUCR = "QCAX55544444444";  // Q is a crap ACP code
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "No MAWB could be found from the MUCR");

			var packagesName = declaration.CaptionForProperty(nameof(JobDeclaration.JE_TotalNoOfPacks));
			declaration.JE_CustomsProfile = "DDD";
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_MasterUCR = "HCAX55544444444";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "Badge");
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, packagesName);
			declaration.SubLocation = "CAX";
			declaration.JE_TotalNoOfPacks = 69;
			declaration.JE_MasterUCR = "";
			declaration.JE_MasterUCR = "HCAX55544444444";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, packagesName);
			// Shed is checked in method TestStrongLinkMismatchingSerialNumbersIncludingForItsf()
		}

		public void TestConsigmentWithShedEqualToAgentDoesNotCauseADictonaryToExplode()
		{
			var declaration = GetCcsukDeclaration();
			var basic = GetBasic();
			basic.AgentBadge = basic.CargoTerminalOperator;
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC5554444444433";
			Assert("Did not explode when validating MUCR", true);
		}

		public void TestNothingButBaseForExports()
		{
			var declaration = GetCcsukDeclaration("EXP");
			var basic = GetBasic();
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "CRAP";
			AssertEquals(false, declaration.JE_MasterUCRInfo.HasMessageError("This MUCR appears to be invalid for a CCSUK air import. It should look like HBAC11122222222(33333333)(44)."));
		}

		public void TestWeakLinkMucrIsBasic()
		{
			var declaration = GetCcsukDeclaration();
			var hawb = GetHawb();
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC00012345678";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a basic");
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a basic");
		}

		public void TestWeakLinkFailedHouse()
		{
			var declaration = GetCcsukDeclaration();
			var hawb = GetHawb();
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC0000000000033333333";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "No MAWB could be found from the MUCR");
			declaration.JE_MasterUCR = "HBAC0001234567833333333";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "No HAWB could be found from the MUCR");
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "No HAWB could be found from the MUCR");
		}

		public void TestWeakLinkFailedBasic()
		{
			var declaration = GetCcsukDeclaration();
			var hawb = GetHawb();
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC11122222222";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "No MAWB could be found from the MUCR");
			declaration.JE_MasterUCR = "HBAC00012345678";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "No MAWB could be found from the MUCR");
		}

		public void TestThroughAwb()
		{
			var declaration = GetCcsukDeclaration();
			var hawb = GetHawb();
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "through AWB");
			hawb.AirportOfArrival = "AAA";
			hawb.AirportOfDestination = "BBB";
			declaration.JE_MasterUCR = "X";
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "through AWB");
		}

		public void TestWeakLinkMucrIsWholeHouse()
		{
			var declaration = GetCcsukDeclaration();
			var house = GetHawb();
			house.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a whole house");
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "2 splits");
			var split01 = house.Splits.AddNew();
			split01.SplitReference = "01";
			var split02 = house.Splits.AddNew();
			split02.SplitReference = "02";
			declaration.JE_MasterUCR = "";
			declaration.JE_MasterUCR = "HBAC0001234567887654321";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a whole house");
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "2 splits");
			declaration.JE_MasterUCR = "HBAC000123456788765432101";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a whole house");
		}

		public void TestWeakLinkMucrIsSplitHouse()
		{
			var declaration = GetCcsukDeclaration();
			var house = GetHawb();
			house.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			declaration.JE_MasterUCR = "HBAC000123456788765432199";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a split house");
			var split = house.Splits.AddNew();
			declaration.JE_MasterUCR = "";
			declaration.JE_MasterUCR = "HBAC000123456788765432199";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies a split house");
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies split 99 on house");
			split.SplitReference = "99";
			declaration.JE_MasterUCR = "";
			declaration.JE_MasterUCR = "HBAC000123456788765432199";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "This MUCR specifies split 99 on house");
		}

		public void TestTotalNoOfPacks()
		{
			const string expectedErrorMessage = "This job references an inventory record whose Total number of Packages and Unit does not match";

			var declaration = GetCcsukDeclaration();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var house = GetHawb();
			house.CS_JE_CustomsFormalEntry = declaration.PK;
			house.CS_PiecesManifested = 20;
			declaration.JE_MasterUCR = "HBAC0001234567887654321";

			declaration.JE_TotalNoOfPacks = 10;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, expectedErrorMessage);
			var message = declaration.JE_MasterUCRInfo.Notifications.GetMessageErrors().ToMessageListString();
			AssertContains("AWB:20", message);
			AssertContains("Declaration:10", message);

			declaration.JE_TotalNoOfPacks = 20;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, expectedErrorMessage);

			var split1 = house.Splits.AddNew();
			split1.SplitReference = "01";
			split1.CG_PiecesManifested = 5;
			var split2 = house.Splits.AddNew();
			split2.SplitReference = "02";
			split2.CG_PiecesManifested = 10;

			declaration.JE_MasterUCR = "HBAC000123456788765432101";

			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, expectedErrorMessage);
			message = declaration.JE_MasterUCRInfo.Notifications.GetMessageErrors().ToMessageListString();
			AssertContains("AWB:5", message);
			AssertContains("Declaration:20", message);

			declaration.JE_TotalNoOfPacks = 5;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, expectedErrorMessage);
		}

		public void TestTotalNoOfPacks_Basic()
		{
			const string expectedErrorMessage = "This job references an inventory record whose Total number of Packages and Unit does not match";

			var declaration = GetCcsukDeclaration();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var basic = GetBasic();
			basic.NumberOfPiecesExpected = 20;
			declaration.JE_MasterUCR = "HBAC55544444444";

			declaration.JE_TotalNoOfPacks = 10;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, expectedErrorMessage);
			var message = declaration.JE_MasterUCRInfo.Notifications.GetMessageErrors().ToMessageListString();
			AssertContains("AWB:20", message);
			AssertContains("Declaration:10", message);

			declaration.JE_TotalNoOfPacks = 20;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, expectedErrorMessage);

			var split1 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split1.CG_PiecesManifested = 5;
			var split2 = basic.Splits.AddNew();
			split2.SplitReference = "02";
			split2.CG_PiecesManifested = 10;

			declaration.JE_MasterUCR = "HBAC55544444444        01";

			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, expectedErrorMessage);
			message = declaration.JE_MasterUCRInfo.Notifications.GetMessageErrors().ToMessageListString();
			AssertContains("AWB:5", message);
			AssertContains("Declaration:20", message);

			declaration.JE_TotalNoOfPacks = 5;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, expectedErrorMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			ShedTest.CreateShed(Factory, "GB", "LHRCAX", "OTHER at Heathrow", acpCode: "H");
			ShedTest.CreateShed(Factory, "GB", "LBAELF", "Leeds/Bradford", acpCode: "Y");
			Factory.Save();
		}

		ForwardingShipment GetShipment(JobDeclaration declaration)
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			return shipment;
		}

		CusHAWB GetHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "00012345678";
			mawb.CargoTerminalOperator = "BAC";
			mawb.CargoTerminalOperatorAirport = "LHR";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			hawb.AgentBadge = "DAN";
			return hawb;
		}

		CusMAWB GetBasic()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "55544444444";
			basic.CargoTerminalOperator = "BAC";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.AgentBadge = "DAN";
			basic.NumberOfPiecesExpected = 69;
			return basic;
		}

		JobDeclaration GetCcsukDeclaration(string messageType = "IMP")
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = messageType;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			declaration.JE_LocationOfGoods = "LBA";
			declaration.SubLocation = "ELF";

			return declaration;
		}
	}
}
