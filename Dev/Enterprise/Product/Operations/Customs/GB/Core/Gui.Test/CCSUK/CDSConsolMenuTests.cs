using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	sealed class CDSConsolMenuTests : TestCaseWithFactory
	{
		[TestDate(2011, 12, 13)]
		public void TestSendToCDS_BetweenEffectiveDates()
		{
			var isDevOrSupport = EDIMenuZZCustomsFunctionalityTest.SetupCurrentUserAsDeveloper(Factory, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				AssertIsCDSFunctionalityEnabled(true, isDevOrSupport);
			}
		}

		[TestDate(2011, 12, 21)]
		public void TestSendToCDS_NotBetweenEffectiveDates()
		{
			var isDevOrSupport = EDIMenuZZCustomsFunctionalityTest.SetupCurrentUserAsDeveloper(Factory, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
			{
				AssertIsCDSFunctionalityEnabled(false, isDevOrSupport);
			}
		}

		[TestDate(2011, 12, 16)]
		public void TestSendToCDS_PilotCompany()
		{
			var isDevOrSupport = EDIMenuZZCustomsFunctionalityTest.SetupCurrentUserAsDeveloper(Factory, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				AssertIsCDSFunctionalityEnabled(true, isDevOrSupport);
			}
		}

		[TestDate(2011, 12, 16)]
		public void TestSendToCDS_NotPilotCompany()
		{
			var isDevOrSupport = EDIMenuZZCustomsFunctionalityTest.SetupCurrentUserAsDeveloper(Factory, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				var branchPK = EU.NCTS.Business.Testing.NCTSTestHelper.MakeDoverBranchForTest(Factory).PK.ToGuid();
				using (DisposableEnvironment.ForBranch(branchPK))
				{
					AssertIsCDSFunctionalityEnabled(false, isDevOrSupport);
				}
			}
		}

		[TestDate(2011, 12, 16)]
		public void TestSendToCDS_DeveloperOrSupport()
		{
			var isDevOrSupport = EDIMenuZZCustomsFunctionalityTest.SetupCurrentUserAsDeveloper(Factory, true);
			AssertIsCDSFunctionalityEnabled(true, isDevOrSupport);
		}

		void AssertIsCDSFunctionalityEnabled(ZBool assertCDSIsEnabledAndNoMessage, bool isDevOrSupport)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var consol = GbCDSConsolIntegrationWrapperTests.CreateSampleWrapper(Factory);
			var consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			var cdsConsolMenu = new CDSExportConsolIntegrationMenu(consolWrapper);
			var cdsMenuItem = cdsConsolMenu.MenuItems[0];
			AssertEquals("Pre-req Current User is dev or support?:", isDevOrSupport, GlbStaff.CurrentUser.GS_IsDeveloper);

			cdsMenuItem.PerformClick();

			if (assertCDSIsEnabledAndNoMessage)
			{
				AssertNotContains("CDS is not yet enabled for your company.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertContains("CDS is not yet enabled for your company.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2018, 6, 7)]
		public void TestMenuClick()
		{
			var consol = GbCDSConsolIntegrationWrapperTests.CreateSampleWrapper(Factory);
			var consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			using (var cdsMenu = new CDSExportConsolIntegrationMenu(consolWrapper))
			{
				foreach (ZMenuItem childMenuItem in cdsMenu.MenuItems)
				{
					childMenuItem.PerformClick();
				}

				var messages = consol.Messages;
				AssertEquals(5, messages.Count);
				CombineAssertions(() =>
				{
					var owner = consolWrapper.MawbExportHelper.ME_Profile;

					AssertMessage<CDSInventoryLinkingConsolidationRequestEDIMessage>("Close Master", messages[0], @"<inventoryLinkingConsolidationRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>CST</messageCode>
  <masterUCR>A:MUCR1234567</masterUCR>
</inventoryLinkingConsolidationRequest>", owner, "LCQ", "CLS", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Inventory Linking Consolidation Request to CDS:</H3><p><strong>Close consol : </strong>A:MUCR1234567</p>");
					AssertMessage<CDSInventoryLinkingQueryRequestEDIMessage>("Query Master", messages[1], @"<inventoryLinkingQueryRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <queryUCR>
    <ucr>A:MUCR1234567</ucr>
    <ucrType>M</ucrType>
  </queryUCR>
</inventoryLinkingQueryRequest>", owner, "LQQ", "M", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Inventory Linking Query Request to CDS:</H3><p><strong>UCR: </strong>A:MUCR1234567<br><strong>UCR Type: </strong>M</p>");

					AssertMessage<CDSInventoryLinkingMovementRequestEDIMessage>("Anticipate Goods’ Arrival", messages[2], @"<inventoryLinkingMovementRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAA</messageCode>
  <ucrBlock>
    <ucr>A:MUCR1234567</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>LOC</goodsLocation>
  <shedOPID>She</shedOPID>
  <masterUCR>A:MUCR1234567</masterUCR>
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>", owner, "LMQ", "EAA", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Movement request message of type (EAL, EAA, EDL)</H3><p><strong>Message Code: </strong>EAA<br><strong>MUCR: </strong>A:MUCR1234567<br><strong>DUCR: </strong>A:MUCR1234567<br><strong>Goods Location: </strong>LOC<br><strong>Movement Reference Number: </strong>06Jun0000PART<br><strong>Shed Operator: </strong>She<br><strong>Transport ID: </strong>TransportId<br><strong>Transport Mode: </strong>4<br><strong>Transport Nationality: </strong>GB</p>");

					AssertMessage<CDSInventoryLinkingMovementRequestEDIMessage>("Arrive Goods", messages[3], @"<inventoryLinkingMovementRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAL</messageCode>
  <ucrBlock>
    <ucr>A:MUCR1234567</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>LOC</goodsLocation>
  <goodsArrivalDateTime>2018-06-06T00:00:00+08:00</goodsArrivalDateTime>
  <shedOPID>She</shedOPID>
  <masterUCR>A:MUCR1234567</masterUCR>
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>", owner, "LMQ", "EAL", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Movement request message of type (EAL, EAA, EDL)</H3><p><strong>Message Code: </strong>EAL<br><strong>MUCR: </strong>A:MUCR1234567<br><strong>DUCR: </strong>A:MUCR1234567<br><strong>Goods Arrival Date Time: </strong>6/06/2018 12:00:00 AM<br><strong>Goods Location: </strong>LOC<br><strong>Movement Reference Number: </strong>06Jun0000PART<br><strong>Shed Operator: </strong>She<br><strong>Transport ID: </strong>TransportId<br><strong>Transport Mode: </strong>4<br><strong>Transport Nationality: </strong>GB</p>");

					AssertMessage<CDSInventoryLinkingMovementRequestEDIMessage>("Depart Goods", messages[4], @"<inventoryLinkingMovementRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EDL</messageCode>
  <ucrBlock>
    <ucr>A:MUCR1234567</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>LOC</goodsLocation>
  <goodsDepartureDateTime>2018-06-06T00:00:00+08:00</goodsDepartureDateTime>
  <shedOPID>She</shedOPID>
  <masterUCR>A:MUCR1234567</masterUCR>
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>", owner, "LMQ", "EDL", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Movement request message of type (EAL, EAA, EDL)</H3><p><strong>Message Code: </strong>EDL<br><strong>MUCR: </strong>A:MUCR1234567<br><strong>DUCR: </strong>A:MUCR1234567<br><strong>Goods Departure Date Time: </strong>6/06/2018 12:00:00 AM<br><strong>Goods Location: </strong>LOC<br><strong>Movement Reference Number: </strong>06Jun0000PART<br><strong>Shed Operator: </strong>She<br><strong>Transport ID: </strong>TransportId<br><strong>Transport Mode: </strong>4<br><strong>Transport Nationality: </strong>GB</p>");
				});

				Factory.Save();

				consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
				messages = consolWrapper.MawbExportHelper.Messages;
				AssertEquals(5, messages.Count);
				AssertEquals(5, messages.OfType<CDSEDIMessage>().Count());
			}
		}

		public void TestWarningForDirectInventoryLinkingRequestWithoutSendingAgent()
		{
			var consol = GbCDSConsolIntegrationWrapperTests.CreateSampleWrapper(Factory);
			var consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			using var cdsMenu = new CDSExportConsolIntegrationMenu(consolWrapper);

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			AssertNull("Pre-requisite: SendingForwarder", consol.SendingForwarder);
			AssertShowsError("SendingForwarder not set: ", expected: true);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			AssertNullOrEmpty("Pre-requisite: SendingForwarder has no EORI", consol.SendingForwarder.GetEORI());
			AssertShowsError("SendingForwarder set to org without EORI: ", expected: true);

			MawbTestHelper.MakeBadge("ZPG", GatewayList.Codes.CNS_CUSDECOnly, "CUKFFW98000", true, "ZPG", false, false, BadgeDirectionList.Codes.EXP, "GBLHR", MucrGenerationStyles.Codes.Air);
			consolWrapper.MawbExportHelper.ME_Profile = "ZPG";
			AssertShowsError("With not direct-to-CDS badge: ", expected: false);
			consol.Reload();
			AssertEquals("consol.Messages.Count", 5, consol.Messages.Count);

			void AssertShowsError(string message, bool expected)
			{
				var errorMessageText = "Messaging directly to CDS is not possible when the Sending Forwarder is missing or when this organisation lacks an EORI.";

				CombineAssertions(message, () =>
				{
					foreach (var childMenuItem in cdsMenu.MenuItems.Cast<ZMenuItem>())
					{
						UnitTestUserNotification.Instance.ClearMessages();
						childMenuItem.PerformClick();
						if (expected)
						{
							AssertContains(childMenuItem.GetType().ToString(), errorMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
						}
						else
						{
							AssertNotContains(childMenuItem.GetType().ToString(), errorMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				});
			}
		}

		public void TestMenuEnabledForLoader()
		{
			var consol = GbCDSConsolIntegrationWrapperTests.CreateSampleWrapper(Factory);
			var consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			using (var cdsMenu = new CDSExportConsolIntegrationMenu(consolWrapper))
			{
				cdsMenu.RefreshMenu();
				var eaaMenuItem = cdsMenu.MenuItems.ToList<ZMenuItem>().FirstOrDefault(x => x.Text.Contains("Anticipate Goods' Arrival"));
				Assert("With Maritime Loader False", !eaaMenuItem.Enabled);
			}

			consol.JK_RL_NKLoadPort = "GBMAN";
			consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			using (var cdsMenu = new CDSExportConsolIntegrationMenu(consolWrapper))
			{
				cdsMenu.RefreshMenu();
				var eaaMenuItem = cdsMenu.MenuItems.ToList<ZMenuItem>().FirstOrDefault(x => x.Text.Contains("Anticipate Goods' Arrival"));
				Assert("With Maritime Loader True", !eaaMenuItem.Enabled);
			}

			GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var cdsMenu = new CDSExportConsolIntegrationMenu(consolWrapper))
			{
				cdsMenu.RefreshMenu();
				var eaaMenuItem = cdsMenu.MenuItems.ToList<ZMenuItem>().FirstOrDefault(x => x.Text.Contains("Anticipate Goods' Arrival"));
				Assert("With Maritime Loader True", eaaMenuItem.Enabled);
			}
		}

		static void AssertMessage<T>(ZString testCase, EDIMessage message, ZString messageText, ZString messageOwner, 
			ZString messageType, ZString messageSubType, ZString interpretation)
		{
			var str = ZDateTime.Now.ToString("o");
			var timeZonePostfix = str.Substring(str.IndexOf("+", StringComparison.OrdinalIgnoreCase));

			Assert(testCase, message is T);
			AssertEquals(testCase + ".EM_MessageText", messageText.Replace("+08:00", timeZonePostfix), message.EM_MessageText);
			AssertEquals(testCase + ".EM_MessageOwner", messageOwner, message.EM_MessageOwner);
			AssertEquals(testCase + ".EM_ApplicationCode", EDIMessage.ApplicationCodes.GbCustomsDeclarationServices, message.EM_ApplicationCode);
			AssertEquals(testCase + ".EM_MessageType", messageType, message.EM_MessageType);
			AssertEquals(testCase + ".EM_MessageSubType", messageSubType, message.EM_MessageSubType);
			AssertEquals(testCase + ".EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(testCase + ".EM_MessageInterpretation", interpretation, message.EM_MessageInterpretation);
			AssertEquals(testCase + ".EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}
	}
}
