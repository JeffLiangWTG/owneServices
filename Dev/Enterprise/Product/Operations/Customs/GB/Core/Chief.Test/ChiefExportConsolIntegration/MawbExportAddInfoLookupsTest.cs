using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	sealed class MawbExportAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsChiefExportsBadges()
		{
			using (GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_RL_NKLoadPort = "GBLHR";
				MawbTestHelper.MakeBadge("ZPE", GatewayList.Codes.CDS, "", true, "", false, false, BadgeDirectionList.Codes.IMP, "GBLHR", applicationCode: "CDS");
				MawbTestHelper.MakeBadge("DAN", GatewayList.Codes.CNS_CUSDECOnly, "", true, "AAA", false, false, BadgeDirectionList.Codes.IMP, "GBMAN", applicationCode: "CDS");
				MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CCSUKviaNTMsgGW, "", false, "", false, false, BadgeDirectionList.Codes.Both, "GBLHR");
				MawbTestHelper.MakeBadge("DEF", GatewayList.Codes.MCP_CUSDECOnly, "", false, "AAA", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
				MawbTestHelper.MakeBadge("XDC", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKAIR98LHR", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
				MawbTestHelper.MakeBadge("CDP", GatewayList.Codes.Pentant, "", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR", applicationCode: "CDS");
				MawbTestHelper.MakeBadge("CDM", GatewayList.Codes.MCP_CUSDECOnly, "", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR", applicationCode: "CDS");
				MawbTestHelper.MakeBadge("CDS", GatewayList.Codes.CDS, "", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR", applicationCode: "CDS");

				TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "CD1", "12345123451234", PasswordTypesList.Codes.CDS);
				TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "CD2", "12345123451234", PasswordTypesList.Codes.CDS);

				var mawbExportAddInfo = GetNewBusinessObject();

				var profiles = mawbExportAddInfo.Lookups.ProfilesListForExports;
				var codes = profiles.CodesAsString;
				CombineAssertions(() =>
				{
					Assert($"ZPE should be excluded ({codes})", !profiles.ContainsCode("ZPE"));
					Assert($"DAN should be excluded ({codes})", !profiles.ContainsCode("DAN"));
					Assert($"ABC missing ({codes})", profiles.ContainsCode("ABC"));
					Assert($"DEF missing ({codes})", profiles.ContainsCode("DEF"));
					Assert($"XDC missing ({codes})", profiles.ContainsCode("XDC"));
					Assert($"CD1 missing ({codes})", profiles.ContainsCode("12345123451234.CD1"));
					Assert($"CD2 missing ({codes})", profiles.ContainsCode("12345123451234.CD2"));
					AssertEquals("DEP shed XDC description", "DEP shed XDC", profiles.GetDescriptionFromCode("XDC"));
					Assert($"CDP missing ({codes})", profiles.ContainsCode("CDP"));
					Assert($"CDM missing ({codes})", profiles.ContainsCode("CDM"));
					Assert($"CDS missing ({codes})", profiles.ContainsCode("CDS"));
				});
			}
		}

		public void TestLookupsSheds()
		{
			PortTest.CreatePort(Factory, "GB", "BLE", "Agility Logistics Ltd", portType: "SEA");
			PortTest.CreatePort(Factory, "GB", "FZO", "FILTON AERODROME", portType: "DES");
			ShedTest.CreateShed(Factory, "GB", "LTNLCS", "LONDON LUTON CARGO at Luton", portName: "Luton");
			ShedTest.CreateShed(Factory, "GB", "LTNDHX", "DHL EXPRESS UK LTD at Luton", portName: "Luton");
			ShedTest.CreateShed(Factory, "GB", "LHRADX", "AIR & CARGO SERVICES LTD at Heathrow", portName: "Heathrow");

			Factory.Save();

			var mawbExportAddInfo = GetNewBusinessObject();
			mawbExportAddInfo.ME_TransportMode = "SEA";
			AssertEquals(0, mawbExportAddInfo.Lookups.ShedsList.Count);
			mawbExportAddInfo.ME_TransportMode = "AIR";
			mawbExportAddInfo.ME_ExportLocation = "";
			AssertEquals(3, mawbExportAddInfo.Lookups.ShedsList.Count);
			mawbExportAddInfo.ME_ExportLocation = "LTN";
			AssertEquals("Shed at Luton", 2, mawbExportAddInfo.Lookups.ShedsList.Count);
			AssertEquals("LONDON LUTON CARGO at Luton", mawbExportAddInfo.Lookups.ShedsList.GetDescriptionFromCode("LCS"));
			AssertEquals("DHL EXPRESS UK LTD at Luton", mawbExportAddInfo.Lookups.ShedsList.GetDescriptionFromCode("DHX"));
		}

		public void TestLookupsLocationOfGoodsList()
		{
			PortTest.CreatePort(Factory, "GB", "BLE", "Agility Logistics Ltd", portType: "SEA");
			PortTest.CreatePort(Factory, "GB", "FZO", "FILTON AERODROME", portType: "DES");
			ShedTest.CreateShed(Factory, "GB", "LTNLCS", "LONDON LUTON CARGO at Luton", portName: "Luton");

			Factory.Save();

			var mawbExportAddInfo = GetNewBusinessObject();
			mawbExportAddInfo.ME_TransportMode = "SEA";
			AssertEquals("Sea Ports", 1, mawbExportAddInfo.Lookups.LocationOfGoodsList.Count);
			AssertEquals("Agility Logistics Ltd", mawbExportAddInfo.Lookups.LocationOfGoodsList.GetDescriptionFromCode("BLE"));
			mawbExportAddInfo.ME_TransportMode = "AIR";
			AssertEquals("Air Ports", 2, mawbExportAddInfo.Lookups.LocationOfGoodsList.Count);
			AssertEquals("FILTON AERODROME", mawbExportAddInfo.Lookups.LocationOfGoodsList.GetDescriptionFromCode("FZO"));
			AssertEquals("Luton", mawbExportAddInfo.Lookups.LocationOfGoodsList.GetDescriptionFromCode("LTN"));
			mawbExportAddInfo.ME_ExportShed = "LCS";
			AssertEquals(1, mawbExportAddInfo.Lookups.LocationOfGoodsList.Count);
			AssertEquals("Luton", mawbExportAddInfo.Lookups.LocationOfGoodsList.GetDescriptionFromCode("LTN"));
		}

		public void TestLookupsCountries()
		{
			var mawbExportAddInfo = GetNewBusinessObject();
			Assert(mawbExportAddInfo.Lookups.CountriesList.Count > 200);
			AssertEquals(1, mawbExportAddInfo.Lookups.CountriesList.Count(x => x.RN_Code == Core.Constants.CountryCodes.UnitedKingdom));
		}

		public void TestLookupsModes()
		{
			var list = GetNewBusinessObject().Lookups.TransportTypeList;
			AssertEquals(8, list.Count);
			AssertEquals(list, Factory.GetCachedValue<TransportTypeList>());
		}

		public void TestLookupsMasterOpt()
		{
			AssertEquals(4, GetNewBusinessObject().Lookups.MasterOptList.Count);
		}

		public void TestLookupsCommunityTransit()
		{
			AssertEquals(12, GetNewBusinessObject().Lookups.CommunityTransitList.Count);
		}

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

		ForwardingConsol consol;
	}
}
