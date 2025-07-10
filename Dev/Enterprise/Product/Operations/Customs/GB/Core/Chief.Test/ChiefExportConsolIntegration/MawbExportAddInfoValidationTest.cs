using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	class MawbExportAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		MawbExportAddInfo GetNewBusinessObject()
		{
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			return wrapper.MawbExportHelper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
		}

		public void TestCheckME_Profile()
		{
			consol.JK_RL_NKLoadPort = "GBLHR";
			MawbTestHelper.MakeBadge("FEY", GatewayList.Codes.MCP_CUSDECOnly, "", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			MawbTestHelper.MakeBadge("DVG", GatewayList.Codes.CCSUKviaNTMsgGW, "", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			MawbTestHelper.MakeBadge("ZPE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			var me = GetNewBusinessObject();
			me.ME_Profile = "ZPE";
			AssertEquals(false, me.ME_ProfileInfo.HasNotifications());
			me.ME_Profile = "POOP";
			AssertHasMessageErrorContaining(me.ME_ProfileInfo, "not in");
			me.ME_Profile = "";
			AssertHasMessageErrorContaining(me.ME_ProfileInfo, "not entered");
		}

		public void TestCheckME_Profile_IsBasedOnGlbExternalPassword_OR_IsRegistryGatewayCDS()
		{
			consol.JK_RL_NKLoadPort = "GBLHR";
			MawbTestHelper.MakeBadge("FEY", GatewayList.Codes.MCP_CUSDECOnly, "", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			MawbTestHelper.MakeBadge("DVG", GatewayList.Codes.CDS, "", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			MawbTestHelper.MakeBadge("ZPE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "ABC", "12345123451234", PasswordTypesList.Codes.CDS);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "GBLHR";
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = "EXP";
			declaration1.JE_JS = shipment1.PK;
			declaration1.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var me = GetNewBusinessObject();

			var expectedValue = "12345123451234.ABC";
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
				{
					new BadgeCodeSetting { BadgeCode = "ABC", RL_PortCode = "GBMAN", ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services }
				}))
				{
					consol.JK_RL_NKLoadPort = "GBMAN";
					var lookup = me.Lookups.ProfilesListForExports;

					AssertEquals("Only 1", 1, lookup.Count);
					AssertEquals("Code", expectedValue, lookup[0].Code);
					me.ME_Profile = lookup[0].Code;
					AssertEquals("Full length profile", expectedValue, me.ME_Profile);
					AssertHasMessageErrorContaining(me.ME_ProfileInfo, "CDS Export Inventory Linking is in Phase Two, meaning all messages must go via a CSP. " +
				"Select another profile such that a CSP is used to deliver all messages to CDS.");
				}
				me.ME_Profile = "FEY";
				AssertNoMessageErrorContaining(me.ME_ProfileInfo, "CDS Export Inventory Linking is in Phase Two, meaning all messages must go via a CSP. " +
				"Select another profile such that a CSP is used to deliver all messages to CDS.");
				me.ME_Profile = "DVG";
				AssertHasMessageErrorContaining(me.ME_ProfileInfo, "CDS Export Inventory Linking is in Phase Two, meaning all messages must go via a CSP. " +
				"Select another profile such that a CSP is used to deliver all messages to CDS.");
			}
		}
		public void TestCheckME_Profile_IsPhase2ProfileBasedOn_CHIEFGateway()
		{
			consol.JK_RL_NKLoadPort = "GBLHR";
			MawbTestHelper.MakeBadge("FEY", GatewayList.Codes.MCP_CUSDECOnly, "", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "ABC", "12345123451234", PasswordTypesList.Codes.CDS);
			var me = GetNewBusinessObject();

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
				{
					new BadgeCodeSetting { CSPCode = GatewayList.Codes.MCP_CUSDECOnly, BadgeCode = "FEY", RL_PortCode = "GBMAN", ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF }
				}))
				{
					me.ME_Profile = "FEY";
					AssertHasWarningContaining(me.ME_ProfileInfo, "CDS Export Inventory Dual Running is in Phase Two. It is encouraged to use CDS and not CHIEF. Select a CDS profile/badge");
				}
			}
		}

		public void TestCheckME_Profile_IsPhase1ProfileBasedOn_CDSGateway()
		{
			consol.JK_RL_NKLoadPort = "GBLHR";
			MawbTestHelper.MakeBadge("DVG", GatewayList.Codes.CDS, "", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "ABC", "12345123451234", PasswordTypesList.Codes.CDS);
			var me = GetNewBusinessObject();

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE1, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
				{
					new BadgeCodeSetting { CSPCode = GatewayList.Codes.CDS, BadgeCode = "DVG", RL_PortCode = "GBMAN", ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services }
				}))
				{
					me.ME_Profile = "DVG";
					AssertHasMessageErrorContaining(me.ME_ProfileInfo, "CDS Export Inventory Dual Running is in Phase One. Do not send inventory-linking messages to CDS.  Instead select a CHIEF profile/badge.");
				}
			}
		}

		ForwardingConsol consol;
	}
}
