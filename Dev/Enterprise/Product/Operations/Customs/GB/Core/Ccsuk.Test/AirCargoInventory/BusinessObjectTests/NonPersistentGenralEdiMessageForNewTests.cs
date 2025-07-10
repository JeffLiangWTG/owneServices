using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(NonPersistentGenralEdiMessageForNew))]
	class NonPersistentGenralEdiMessageForNewTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NonPersistentGenralEdiMessageForNew(Factory);
		}

		public void TestPimaGeneration()
		{
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			npbo.ShedOrBadge = "CAR";
			npbo.Airport = "LHR";
			npbo.IsRecipientShed = false;
			npbo.IsRecipientAgent = true;
			AssertEquals("CUKFFW98000CAR", npbo.Pima);
			npbo.IsRecipientAgent = false;
			npbo.IsRecipientShed = true;
			AssertEquals("CUKAIR98LHRCAR", npbo.Pima);
		}

		public void TestReadOnlyAndPimaList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsPimaOrTerminalAddress, "PIMA for customs");

			var pima1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsPimaOrTerminalAddress, "CUKCTM98000XXX/DAN69", "Daniel XXX 69", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var pima2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsPimaOrTerminalAddress, "CUKCTM98222YYY/DJC69", "Daniel YYY 69", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);

			//Agent
			npbo.IsRecipientShed = false;
			npbo.IsRecipientAgent = true;
			AssertEquals(true, npbo.PimaInfo.ReadOnly);
			AssertEquals(true, npbo.AirportInfo.ReadOnly);
			AssertEquals(false, npbo.ShedOrBadgeInfo.ReadOnly);
			AssertEquals(1, npbo.RecipientPimasList.Count);
			AssertEquals("CUKFFW98000", npbo.Pima.Trim());

			//Customs
			npbo.IsRecipientAgent = false;
			npbo.IsRecipientCustoms = true;
			AssertEquals(false, npbo.PimaInfo.ReadOnly);
			AssertEquals(true, npbo.AirportInfo.ReadOnly);
			AssertEquals(true, npbo.ShedOrBadgeInfo.ReadOnly);
			Assert("More than one customs pima in list", npbo.RecipientPimasList.Count > 1);
			AssertContains("CUKCTM98000XXX/DAN69", npbo.RecipientPimasList.CodesAsString);
			AssertContains("CUKCTM98222YYY/DJC69", npbo.RecipientPimasList.CodesAsString);

			//Shed
			npbo.IsRecipientCustoms = false;
			npbo.IsRecipientShed = true;
			AssertEquals(true, npbo.PimaInfo.ReadOnly);
			AssertEquals(false, npbo.AirportInfo.ReadOnly);
			AssertEquals(false, npbo.ShedOrBadgeInfo.ReadOnly);
			AssertEquals(1, npbo.RecipientPimasList.Count);
			AssertEquals("CUKAIR98", npbo.Pima.Trim());

			//Roll you own
			npbo.IsRecipientShed = false;
			npbo.IsRecipientRollYourOwn = true;
			AssertEquals(false, npbo.PimaInfo.ReadOnly);
			AssertEquals(true, npbo.AirportInfo.ReadOnly);
			AssertEquals(true, npbo.ShedOrBadgeInfo.ReadOnly);
			AssertEquals(1, npbo.RecipientPimasList.Count);
		}

		public void TestShowDepShedsToo()
		{
			using (GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var badgeCcsuk = badges.AddNew();
				badgeCcsuk.BadgeCode = "XDC";
				badgeCcsuk.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
				GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
				var credential = CredentialsSetting.GetCredentialsForBadge(badgeCcsuk.BadgeCode, GlbBranch.CurrentBranch.PK.ToGuid());
				AssertNull(credential);
				var cred = new CredentialsSetting();
				cred.BadgeCode = "XDC";
				cred.Company = "XDC";
				cred.Printer = "CUKAIR98LHR" + badgeCcsuk.BadgeCode;
				var creds = new CredentialsSettingCollection();
				creds.Add(cred);
				GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);

				var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
				npbo.SendingProfile = "CUKAIR98LHR";
				AssertHasErrorContaining(npbo.SendingProfileInfo, "valid");
				npbo.SendingProfile = "";
				AssertHasErrorContaining(npbo.SendingProfileInfo, "enter");
				npbo.SendingProfile = "CUKAIR98LHRXDC";
				AssertNoErrorContaining(npbo.SendingProfileInfo, "valid");
				AssertNoErrorContaining(npbo.SendingProfileInfo, "enter");
			}
		}

		public void TestValidationOfSendingBadge()
		{
			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeMcp = badges.AddNew();
			badgeMcp.BadgeCode = "FEY";
			badgeMcp.CSPCode = GatewayList.Codes.MCP_CUSDECOnly;
			var badgeCcsuk = badges.AddNew();
			badgeCcsuk.BadgeCode = "ZPE";
			badgeCcsuk.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var cred = new CredentialsSetting();
			cred.BadgeCode = "ZPE";
			cred.Company = "ZPE";
			cred.Printer = "CUKFFW98000ZPE";
			var creds = new CredentialsSettingCollection();
			creds.Add(cred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);

			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			npbo.SendingProfile = "XXX";
			AssertHasErrorContaining(npbo.SendingProfileInfo, "valid");
			npbo.SendingProfile = "FEY";
			AssertHasErrorContaining(npbo.SendingProfileInfo, "valid");
			npbo.SendingProfile = "";
			AssertHasErrorContaining(npbo.SendingProfileInfo, "enter");
			npbo.SendingProfile = "CUKFFW98000ZPE";
			AssertNoErrorContaining(npbo.SendingProfileInfo, "valid");
			AssertNoErrorContaining(npbo.SendingProfileInfo, "enter");
		}

		public void TestValidationOfShedAgent()
		{
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			npbo.IsRecipientShed = false;
			npbo.IsRecipientAgent = true;
			npbo.ShedOrBadge = "DAN";
			AssertNoErrorContaining(npbo.ShedOrBadgeInfo, "enter");
			npbo.ShedOrBadge = "";
			AssertHasErrorContaining(npbo.ShedOrBadgeInfo, "enter");
			npbo.IsRecipientAgent = false;
			npbo.IsRecipientCustoms = true;
			AssertNoErrorContaining(npbo.ShedOrBadgeInfo, "enter");
		}

		public void TestValidationOfAirport()
		{
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			npbo.Airport = "LHR";
			AssertNoErrorContaining(npbo.AirportInfo, "airport");
			npbo.Airport = "";
			AssertHasErrorContaining(npbo.AirportInfo, "airport");
			npbo.Airport = "X";
			AssertHasErrorContaining(npbo.AirportInfo, "airport");
			npbo.IsRecipientShed = false;
			npbo.IsRecipientAgent = true;
			npbo.Airport = "";
			AssertNoErrorContaining(npbo.AirportInfo, "airport");
		}

		public void TestValidationOfBodyAndPreformatted()
		{
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			var oneLineOf69Chars = "asdfghyujiklqwerty7uiopxcvbnmrftgyhjkljdfjhdfjhdjfhjfhdjfhdjfhjjjjswt";
			npbo.Payload = oneLineOf69Chars;
			AssertNoErrorContaining(npbo.PayloadInfo, "20 lines");
			AssertNoErrorContaining(npbo.PayloadInfo, "70 characters");
			AssertNoErrorContaining(npbo.PreformattedLinesOf70Info, "20 lines");
			AssertNoErrorContaining(npbo.PreformattedLinesOf70Info, "70 characters");
			npbo.Payload = oneLineOf69Chars + "XX";
			AssertNoErrorContaining(npbo.PayloadInfo, "20 lines");
			AssertNoErrorContaining(npbo.PayloadInfo, "70 characters");
			AssertNoErrorContaining(npbo.PreformattedLinesOf70Info, "20 lines");
			AssertNoErrorContaining(npbo.PreformattedLinesOf70Info, "70 characters");
			npbo.PreformattedLinesOf70 = true;
			AssertNoErrorContaining(npbo.PayloadInfo, "20 lines");
			AssertHasErrorContaining(npbo.PayloadInfo, "70 characters");
			AssertNoErrorContaining(npbo.PreformattedLinesOf70Info, "20 lines");
			AssertHasErrorContaining(npbo.PreformattedLinesOf70Info, "70 characters");
			npbo.Payload = string.Join(System.Environment.NewLine, Enumerable.Repeat("short line", 21));
			AssertHasErrorContaining(npbo.PayloadInfo, "20 lines");
			AssertNoErrorContaining(npbo.PayloadInfo, "70 characters");
			AssertHasErrorContaining(npbo.PreformattedLinesOf70Info, "20 lines");
			AssertNoErrorContaining(npbo.PreformattedLinesOf70Info, "70 characters");
		}

		public void TestPrepareNewMessageSentFromAwb()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000CAR";
			basic.CM_MAWB = "12312345678";
			basic.CargoTerminalOperator = "BAC";
			basic.CargoTerminalOperatorAirport = "LHR";
			var genral = new NonPersistentGenralEdiMessageForNew(basic.Factory);
			genral.PrepareNewMessageSentFromAwb(basic);
			var manager = new NewGenralMessageManager(genral, basic);
			manager.ExecuteMakingRealEdiMessageFromNonPersistentHelper();
			AssertEquals(1, basic.Messages.Count);
			var message = basic.Messages[0];
			AssertEquals("Has been saved", true, message.IsInDatabase);
			AssertEquals("Type", "GEN", message.EM_MessageType);
			AssertContains("+GENRAL", message.EM_MessageText);
			AssertContains("BGM+TXT:ZZZ'MSG+USER", message.EM_MessageText);
			AssertContains("REGARDING CCS-UK BASIC AIR WAYBILL 123-12345678", message.EM_MessageText);
			AssertContains("To shed", "CUKAIR98LHRBAC", message.EM_ApplicationReference);
			AssertContains("From agent", "CUKFFW98000CAR", message.EM_MessageOwner);

			basic.Profile = "CUKAIR98LHRBAC";
			genral = new NonPersistentGenralEdiMessageForNew(basic.Factory);
			genral.PrepareNewMessageSentFromAwb(basic);
			manager = new NewGenralMessageManager(genral, basic);
			manager.ExecuteMakingRealEdiMessageFromNonPersistentHelper();
			AssertEquals(2, basic.Messages.Count);
			message = basic.Messages[1];
			AssertContains("To agent", "CUKFFW98000CAR", message.EM_ApplicationReference);
			AssertContains("From shed", "CUKAIR98LHRBAC", message.EM_MessageOwner);
		}

		public void TestValidationOfSendingProfile()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			npbo.SendingProfile = "X";
			AssertHasErrorContaining(npbo.SendingProfileInfo, "Enter a valid");
			npbo.SendingProfile = "";
			AssertHasErrorContaining(npbo.SendingProfileInfo, "Please enter");
			npbo.SendingProfile = "CUKFFW98000LXA";
			AssertNoErrorContaining(npbo.SendingProfileInfo, "Enter a valid");
			AssertNoErrorContaining(npbo.SendingProfileInfo, "Please enter");

			AssertNoErrorContaining(npbo.SendingProfileInfo, "security right");
			Environment.Env.Security.AirCcsukShed.IsAllowed = false;
			npbo.SendingProfile = "CUKAIR98LHRBAC";
			AssertHasErrorContaining(npbo.SendingProfileInfo, "security right");
			npbo.SendingProfile = "CUKFFW98000LXA";
			AssertNoErrorContaining(npbo.SendingProfileInfo, "security right");
			Environment.Env.Security.AirCcsukShed.IsAllowed = true;
			npbo.SendingProfile = "CUKFFW98000LXA";
			AssertNoErrorContaining(npbo.SendingProfileInfo, "security right");
			npbo.SendingProfile = "CUKAIR98LHRBAC";
			AssertNoErrorContaining(npbo.SendingProfileInfo, "security right");
		}
	}
}

