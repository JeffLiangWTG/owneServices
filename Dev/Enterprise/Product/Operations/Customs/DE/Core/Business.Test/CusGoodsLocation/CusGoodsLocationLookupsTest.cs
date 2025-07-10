using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQualifierList()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			var list = cusGoodsLocation.Lookups.QualifierList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "U, V, W, Y, Z", list.CodesAsString);
				AssertSame("Cached", list, cusGoodsLocation.Lookups.QualifierList);
			});
		}

		public void TestUnlocodeList()
		{
			CombineAssertions(() =>
			{
				var cusGoodsLocation = Factory.New<CusGoodsLocation>();
				var list = cusGoodsLocation.Lookups.UnlocodeList;
				AssertType<RefUNLOCOCollection>("Type", list);
				AssertSame("Cached", list, cusGoodsLocation.Lookups.UnlocodeList);
				var filterDefaults = ((RefUNLOCOCollection)list).FilterBusinessObjectDefaults.ToList<FilterBusinessObjectDefault>();
				AssertEquals("Filter 'Economic Group'", "Economic Group:Property", filterDefaults[0].Key);
				AssertEquals("Filter 'Economic Group' equals 'EUN'", EconomicGroupList.Codes.EuropeanUnion, filterDefaults[0].Value);
			});
		}

		public void TestAdditionalIdentifierList_Declarant()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			declaration.JE_OA_Representative = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.JE_CustomsOffice = "DE003202";

			var authorization = declaration.Declarant.Header.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "AUTH123");
			authorization.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			var locationRule = authorization.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "LOC1");
			locationRule.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");
			var locationRule2 = authorization.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "LOC2");
			locationRule2.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");
			var invalidRule1 = authorization.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "INV1");
			invalidRule1.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE111111");
			var invalidRule2 = authorization.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.BusinessReference, "INV2");
			invalidRule2.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");
			var invalidRule3 = authorization.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "INV3");
			invalidRule3.CreateLinkedAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "DE003202");
			authorization.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "INV4");

			var authorization2 = declaration.Representative.Header.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "AUTH456");
			authorization2.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			var locationRule3 = authorization2.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "LOC3");
			locationRule3.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");

			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
			instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0000;
			var lookups = instruction.GoodsLocation.Lookups;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				AssertEquals("JE_MessageType = 'IMP'", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				var list = lookups.AdditionalIdentifierList;
				AssertEquals("CodesAsString", "LOC1, LOC2", list.CodesAsString);
				AssertEquals("Cached", list, lookups.AdditionalIdentifierList);

				instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1010;
				AssertEquals("Not getting from representative since declarant has authorisations", "LOC1, LOC2", lookups.AdditionalIdentifierList.CodesAsString);

				instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0000;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_GB = ZGuid.Empty;
				AssertEquals("Only from declarant and declarant is null", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);
			});
		}

		public void TestAdditionalIdentifierList_Representative()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			declaration.JE_OA_Representative = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.JE_CustomsOffice = "DE003202";

			var authorization = declaration.Representative.Header.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "AUTH123");
			authorization.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			var locationRule = authorization.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "LOC1");
			locationRule.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");

			var invalidAuthorization = declaration.Representative.Header.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "INVAUTH1");
			invalidAuthorization.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			var rule = invalidAuthorization.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "LOC2");
			rule.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");

			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
			instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0000;
			var lookups = instruction.GoodsLocation.Lookups;
			CombineAssertions(() =>
			{
				AssertEquals("ZG_PartyConstellation is '*00*'", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);

				instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				AssertEquals("ZG_PartyConstellation is '*01*'", "LOC1", lookups.AdditionalIdentifierList.CodesAsString);

				instruction.ZG_PartyConstellation = ZString.Empty;
				AssertEquals("ZG_PartyConstellation isn't matched", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);

				instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
				declaration.JE_OA_Representative = ZGuid.Empty;
				AssertEquals("Representative is null", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);
			});
		}

		public void TestAdditionalIdentifierList_FilterRuleCode()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			declaration.JE_CustomsOffice = "DE003202";

			var authorization = declaration.Declarant.Header.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "AUTH123");
			authorization.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			var locationRule = authorization.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "LOC1");
			locationRule.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");

			var authorization2 = declaration.Declarant.Header.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "AUTH123");
			authorization2.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			var locationRule2 = authorization2.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "LOC2");
			locationRule2.CreateLinkedAuthorisationRule(Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");

			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
			instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0000;
			var lookups = instruction.GoodsLocation.Lookups;
			CombineAssertions(() =>
			{
				AssertEquals("CEI_Style is '111***'", "LOC1", lookups.AdditionalIdentifierList.CodesAsString);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001300;
				AssertEquals("CEI_Style is '*01***'", "LOC2", lookups.AdditionalIdentifierList.CodesAsString);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				AssertEquals("CEI_Style isn't matched", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<Declaration.JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}
		Declaration.CusEntryInstruction instruction;
		Declaration.JobDeclaration declaration;
	}
}
