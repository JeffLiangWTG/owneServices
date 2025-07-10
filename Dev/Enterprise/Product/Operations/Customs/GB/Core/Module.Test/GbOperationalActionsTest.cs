using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.OperationalAction;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.OperationalActions.Testing
{
	[TestedType(typeof(GbDeclarationActionMethodApplicator))]
	sealed class GbDeclarationActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestOperationalActionMucrFunction()
		{
			CreateAndSaveBadges();

			GbDeclarationActionMethodApplicator applicator = new GbDeclarationActionMethodApplicator();

			DummyOperationalActionSectionLog log = new DummyOperationalActionSectionLog();
			JobDeclaration dec1 = Factory.New<JobDeclaration>();
			JobDeclaration dec2 = Factory.New<JobDeclaration>();
			dec1.JE_DeclarationType = "EFD";
			dec2.JE_DeclarationType = "EFD";
			dec1.JE_CustomsProfile = badgeCcsuk.BadgeCode;
			dec2.JE_CustomsProfile = badgeCcsuk.BadgeCode;
			BusinessObject[] targets = new BusinessObject[] { dec1, dec2 };
			Factory.Save(); // Only needed to populate JE_DeclarationReference, without which making a new LogControllerLink barfs. 

			MucrFunctionUpdaterOperationalActionRunner runner = new MucrFunctionUpdaterOperationalActionRunner(log, targets);

			applicator.MucrManual = "I am a new MUCR";
			runner.PerformMucrFunctionOperationalAction(false, false, false, applicator.MucrManual, applicator.Mawp, applicator.Mawn, applicator.Airport, applicator.Shed);
			AssertEquals("Mucr that was manually supplied should go through to dec", "I am a new MUCR", dec1.JE_MasterUCR);

			applicator.MucrManual = string.Empty;
			applicator.Mawp = "125";
			applicator.Mawn = "12345678";
			log = new DummyOperationalActionSectionLog();
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, targets);
			runner.PerformMucrFunctionOperationalAction(false, false, false, applicator.MucrManual, applicator.Mawp, applicator.Mawn, applicator.Airport, applicator.Shed);
			// Mawb parts shoudl reach dec:
			AssertEquals("12512345678", dec1.JE_MasterBill);
			AssertEquals("12512345678", dec2.JE_MasterBill);

			applicator.Shed = "BAC";
			applicator.Airport = "GBLHR";
			log = new DummyOperationalActionSectionLog();
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, targets);
			runner.PerformMucrFunctionOperationalAction(false, false, false, applicator.MucrManual, applicator.Mawp, applicator.Mawn, applicator.Airport, applicator.Shed);
			// Box 30 data should reach dec.
			AssertEquals("LHRBAC", dec1.JE_LocationOfGoods + dec1.SubLocation);
			AssertEquals("LHRBAC", dec2.JE_LocationOfGoods + dec2.SubLocation);

			PrepareInvoiceLines(dec1);
			PrepareInvoiceLines(dec2);
			targets = new BusinessObject[] { dec1, dec2 };

			log = new DummyOperationalActionSectionLog();
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, targets);
			runner.PerformMucrFunctionOperationalAction(false, false, true, applicator.MucrManual, applicator.Mawp, applicator.Mawn, applicator.Airport, applicator.Shed);  // fire the close on BOTH decs
			AssertEquals("No messaging, and therefore no merging, should have occurred on dec2, since we only send a close against ONE of a batch of decs.", 0, dec2.CustomsEntryHeaders.Count);
			var messagesOnDec1 = dec1.CustomsEntryHeaders[0].Messages;
			AssertContains("Dec1 needs an outbound EAC/C message", "BGM+EAC", messagesOnDec1.LastOutgoingMessage.EM_MessageText);
			AssertContains("Dec1 needs an outbound EAC/C message", "RFF+UCN", messagesOnDec1.LastOutgoingMessage.EM_MessageText);
			AssertNotContains("Dec1 needs an outbound EAC/C message", "RFF+ABO", messagesOnDec1.LastOutgoingMessage.EM_MessageText);

			log = new DummyOperationalActionSectionLog();
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, targets);
			runner.PerformMucrFunctionOperationalAction(true, false, false, applicator.MucrManual, applicator.Mawp, applicator.Mawn, applicator.Airport, applicator.Shed);  // fire the associate	
			messagesOnDec1 = dec1.CustomsEntryHeaders[0].Messages;  // we need a reference to Messaging.Biz for this, dammit. 
			var messagesOnDec2 = dec2.CustomsEntryHeaders[0].Messages;
			AssertContains("BGM+EAC", messagesOnDec1.LastOutgoingMessage.EM_MessageText);
			AssertContains("RFF+UCN", messagesOnDec1.LastOutgoingMessage.EM_MessageText);
			AssertContains("RFF+ABO", messagesOnDec1.LastOutgoingMessage.EM_MessageText);
			AssertContains("BGM+EAC", messagesOnDec2.LastOutgoingMessage.EM_MessageText);
			AssertContains("RFF+UCN", messagesOnDec2.LastOutgoingMessage.EM_MessageText);
			AssertContains("RFF+ABO", messagesOnDec2.LastOutgoingMessage.EM_MessageText);

			log = new DummyOperationalActionSectionLog();
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, targets);
			runner.PerformMucrFunctionOperationalAction(false, true, false, applicator.MucrManual, applicator.Mawp, applicator.Mawn, applicator.Airport, applicator.Shed);  // fire the disassociate	
			AssertContains("BGM+EAC", messagesOnDec1.LastOutgoingMessage.EM_MessageText);
			AssertContains("RFF+ABO", messagesOnDec1.LastOutgoingMessage.EM_MessageText);
			AssertNotContains("RFF+UCN", messagesOnDec1.LastOutgoingMessage.EM_MessageText);
			AssertContains("BGM+EAC", messagesOnDec2.LastOutgoingMessage.EM_MessageText);
			AssertContains("RFF+ABO", messagesOnDec2.LastOutgoingMessage.EM_MessageText);
			AssertNotContains("RFF+UCN", messagesOnDec2.LastOutgoingMessage.EM_MessageText);
		}

		public void TestOperationalActionMucrFunctionWithGoodAndBadDeclarations()
		{
			CreateAndSaveBadges();

			GbDeclarationActionMethodApplicator applicator = new GbDeclarationActionMethodApplicator();

			DummyOperationalActionSectionLog log = new DummyOperationalActionSectionLog();
			var dec1Good = Factory.New<JobDeclaration>();
			var dec2Import = Factory.New<JobDeclaration>();
			var dec3NoBadge = Factory.New<JobDeclaration>();
			dec2Import.JE_MessageType = "IMP";
			dec1Good.JE_DeclarationType = "EFD";
			dec2Import.JE_DeclarationType = "IFD";
			dec1Good.JE_CustomsProfile = badgeCcsuk.BadgeCode;
			dec2Import.JE_CustomsProfile = badgeCcsuk.BadgeCode;
			dec3NoBadge.JE_CustomsProfile = "";
			PrepareInvoiceLines(dec1Good);
			PrepareInvoiceLines(dec2Import);
			PrepareInvoiceLines(dec3NoBadge);
			BusinessObject[] targets = new BusinessObject[] { dec2Import, dec3NoBadge, dec1Good };
			Factory.Save();
			MucrFunctionUpdaterOperationalActionRunner runner = new MucrFunctionUpdaterOperationalActionRunner(log, targets);

			applicator.MucrManual = "I am a new MUCR";
			runner.PerformMucrFunctionOperationalAction(false, true, false, applicator.MucrManual, applicator.Mawp, applicator.Mawn, applicator.Airport, applicator.Shed);
			AssertEquals("Mucr that was manually supplied should go through to dec even though it was last in the list and followed two bad jobs", "I am a new MUCR", dec1Good.JE_MasterUCR);
			AssertContains($"INFO: Disassociating [HL {dec2Import.JE_DeclarationReference}] from existing MUCR \nERROR: Cannot send this MUCR message for this declaration, it is not an export job.", log.MessagesString());
			AssertContains($"INFO: Disassociating [HL {dec3NoBadge.JE_DeclarationReference}] from existing MUCR \nERROR: Badge codes need to be supplied in the registry and then one selected on the declaration", log.MessagesString());
			AssertContains($"INFO: Disassociating [HL {dec1Good.JE_DeclarationReference}] from existing MUCR \nWARNING: Not all the required service tasks are currently running.", log.MessagesString());
			AssertContains($"INFO: Updating MUCR on [HL {dec1Good.JE_DeclarationReference}]\nINFO: MUCR on [HL {dec1Good.JE_DeclarationReference}] is now 'I am a new MUCR'", log.MessagesString());
			AssertEquals(1, dec1Good.CustomsEntryHeaders[0].Messages.Count);
		}

		public void TestWarningAboutDifferentBadgeGenerationStyles()
		{
			JobDeclaration decAirStyle = Factory.New<JobDeclaration>();
			JobDeclaration decAirStyle2 = Factory.New<JobDeclaration>();
			JobDeclaration decGemsStyle = Factory.New<JobDeclaration>();

			CreateAndSaveBadges();

			decAirStyle.JE_CustomsProfile = badgeAir.BadgeCode;
			decAirStyle2.JE_CustomsProfile = badgeAir.BadgeCode;
			decGemsStyle.JE_CustomsProfile = badgeCcsuk.BadgeCode;

			Factory.Save(); // Only needed to populate JE_DeclarationReference, without which making a new LogControllerLink barfs. 

			DummyOperationalActionSectionLog log = new DummyOperationalActionSectionLog();
			BusinessObject[] disparateTargets = new BusinessObject[] { decAirStyle, decGemsStyle };
			MucrFunctionUpdaterOperationalActionRunner runner = new MucrFunctionUpdaterOperationalActionRunner(log, disparateTargets);
			runner.PerformMucrFunctionOperationalAction(false, false, false, string.Empty, "125", "12345678", string.Empty, string.Empty);
			AssertNotContains("Decs of differing badges styles allowed because not messaging with auto generation", "Cannot proceed", log.messages[0]);

			log = new DummyOperationalActionSectionLog();
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, disparateTargets);
			runner.PerformMucrFunctionOperationalAction(true, false, false, string.Empty, "125", "12345678", string.Empty, string.Empty);
			AssertContains("Decs of differing badges styles not allowed because messaging requested with auto generation", "Cannot proceed", log.messages[0]);

			log = new DummyOperationalActionSectionLog();
			BusinessObject[] similarTargets = new BusinessObject[] { decAirStyle, decAirStyle2 };
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, similarTargets);
			runner.PerformMucrFunctionOperationalAction(true, false, false, string.Empty, "125", "12345678", string.Empty, string.Empty);
			AssertNotContains("Decs of same badges styles are allowed", "Cannot proceed", log.messages[0]);

			log = new DummyOperationalActionSectionLog();
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, disparateTargets);
			runner.PerformMucrFunctionOperationalAction(false, false, false, "NewManualMucr", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertNotContains("No check performed even on disperate decs if using manual MUCR but no messaging", "Cannot proceed", log.messages[0]);

			// These three lines needed cos this time the call will actually produce a message:
			PrepareInvoiceLines(decGemsStyle);
			PrepareInvoiceLines(decAirStyle);
			decGemsStyle.JE_DeclarationType = "EFD";
			decAirStyle.JE_DeclarationType = "EFD";
			log = new DummyOperationalActionSectionLog();
			runner = new MucrFunctionUpdaterOperationalActionRunner(log, disparateTargets);
			runner.PerformMucrFunctionOperationalAction(true, false, false, "NewManualMucr", string.Empty, string.Empty, string.Empty, string.Empty);
			AssertNotContains("No check performed even on disperate decs if using manual MUCR but messaging wanted", "Cannot proceed", log.messages[0]);
		}

		BadgeCodeSetting badgeAir;
		BadgeCodeSetting badgeCcsuk;
		void CreateAndSaveBadges()
		{
			badgeAir = new BadgeCodeSetting();
			badgeCcsuk = new BadgeCodeSetting();

			badgeAir.BadgeCode = "AIR";
			badgeCcsuk.BadgeCode = "ZPE";
			badgeAir.Direction = "EXP";
			badgeCcsuk.Direction = "EXP";
			badgeCcsuk.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeAir.MasterUcrCalculationMode = MucrGenerationStyles.Codes.Air;
			badgeCcsuk.MasterUcrCalculationMode = MucrGenerationStyles.Codes.Ccsuk;

			BadgeCodeSettingCollection badges = new BadgeCodeSettingCollection();
			badges.Add(badgeAir);
			badges.Add(badgeCcsuk);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badges);

			var ccsukCred = new CredentialsSetting
			{
				BadgeCode = badgeCcsuk.BadgeCode,
				Company = badgeCcsuk.BadgeCode,
				PIMA = "CUKFFW98000" + badgeCcsuk.BadgeCode
			};

			CredentialsSettingCollection allCreds = new CredentialsSettingCollection();
			allCreds.Add(ccsukCred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FOO");
		}

		static void PrepareInvoiceLines(JobDeclaration dec)
		{
			var inv1 = dec.Invoices.AddNew();
			var line1 = inv1.InvoiceLines.AddNew();
			line1.JI_Tariff = "12345678";
		}
	}
}
