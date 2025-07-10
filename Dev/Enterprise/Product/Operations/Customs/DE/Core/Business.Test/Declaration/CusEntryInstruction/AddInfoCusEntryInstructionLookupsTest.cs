using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class AddInfoCusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPartyConstellationCodeList_Export()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			var addInfoLookups = instruction.AddInfoLookups;
			var list = addInfoLookups.PartyConstellationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "0000, 0001, 0010, 0011, 0100, 0101, 0110, 0111, 1000, 1010, 1100, 1110", list.CodesAsString);
				AssertSame("Cached", list, addInfoLookups.PartyConstellationCodeList);
			});
		}

		public void TestPartyConstellationCodeList_Export_CEI_SubStyleIs20AndCEI_StyleIs000000()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
			var addInfoLookups = instruction.AddInfoLookups;
			var list = addInfoLookups.PartyConstellationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "0000, 0100, 1000, 1100", list.CodesAsString);
				AssertSame("Cached", list, addInfoLookups.PartyConstellationCodeList);
			});
		}

		public void TestPartyConstellationCodeList_Export_CEI_StyleStartsWith111()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
			var addInfoLookups = instruction.AddInfoLookups;
			var list = addInfoLookups.PartyConstellationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "0000, 0010, 0100, 0110, 1000, 1010, 1100, 1110", list.CodesAsString);
				AssertSame("Cached", list, addInfoLookups.PartyConstellationCodeList);
			});
		}

		public void TestPartyConstellationCodeList_Export_TheFourthNumberOfCEI_StyleIs9()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
			var addInfoLookups = instruction.AddInfoLookups;
			var list = addInfoLookups.PartyConstellationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "0000, 0010, 0100, 0110, 1000, 1010, 1100, 1110", list.CodesAsString);
				AssertSame("Cached", list, addInfoLookups.PartyConstellationCodeList);
			});
		}

		public void TestInwardProcessingAuthorizationNumberList()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", ZString.Empty, inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		public void TestInwardProcessingAuthorizationNumberList_WithoutDeclaration()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			AssertEquals("List values", ZString.Empty, inwardProcessingAuthorizationNumberList.CodesAsString);
		}

		public void TestInwardProcessingAuthorizationNumberList_Export()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			AssertEquals("List values", ZString.Empty, inwardProcessingAuthorizationNumberList.CodesAsString);
		}

		public void TestInwardProcessingAuthorizationNumberList_AuthorizationTypeIPO()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "EDIBRNWIS";
			organisation.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			organisation.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			Factory.Save();

			var orgAddress = organisation.Addresses.AddNew();
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.JobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;

			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;

			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER1, NUMBER2", inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		public void TestInwardProcessingAuthorizationNumberList_CEI_StyleAAVOrVAV_DeclarantTypeSEL_Declarant()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			Factory.Save();

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var declarantAddress = declarant.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER1, NUMBER2", inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		public void TestInwardProcessingAuthorizationNumberList_CEI_StyleAAVOrVAV_DeclarantTypeDIR_Declarant()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			Factory.Save();

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var declarantAddress = declarant.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER1, NUMBER2", inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		public void TestInwardProcessingAuthorizationNumberList_CEI_StyleAAVOrVAV_DeclarantTypeDIR_Representative()
		{
			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "EDIBRNFRA";
			representative.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER3");
			representative.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER4");
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var declarantAddress = declarant.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			var representativeAddress = representative.Addresses.AddNew();
			declaration.JE_OA_Representative = representativeAddress.PK;

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER3, NUMBER4", inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		public void TestInwardProcessingAuthorizationNumberList_CEI_StyleAAVOrVAV_DeclarantTypeIND_Declarant()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			Factory.Save();

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var declarantAddress = declarant.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER1, NUMBER2", inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		public void TestInwardProcessingAuthorizationNumberList_CEI_StyleAAVOrVAV_DeclarantTypeIND_RepresentedParty()
		{
			var representedParty = Factory.New<OrgHeader>();
			representedParty.OH_Code = "EDIBRNFRA";
			representedParty.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER3");
			representedParty.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER4");
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var declarantAddress = declarant.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			var representedPartyAddress = representedParty.Addresses.AddNew();
			declaration.JE_OA_BuyingAgentAddress = representedPartyAddress.PK;

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;

			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER3, NUMBER4", inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		public void TestInwardProcessingAuthorizationNumberList_CEI_StyleAAV()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var authorisation1 = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			authorisation1.CPH_StartDate = new ZDate(2020, 12, 1);
			authorisation1.CPH_EndDate = new ZDate(2020, 12, 31);
			var authorisation2 = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			authorisation2.CPH_StartDate = new ZDate(2021, 1, 1);
			authorisation2.CPH_EndDate = new ZDate(2021, 1, 31);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.CEI_LocalClearanceDate = new ZDate(2021, 1, 31);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK; // prior change to declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self triggers autopopulation of declarant

			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER2", inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		[TestDate(2020, 12, 15)]
		public void TestInwardProcessingAuthorizationNumberList_CEI_StyleVAV()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			var authorisation1 = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			authorisation1.CPH_StartDate = new ZDate(2020, 12, 1);
			authorisation1.CPH_EndDate = new ZDate(2020, 12, 31);
			var authorisation2 = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			authorisation2.CPH_StartDate = new ZDate(2021, 1, 1);
			authorisation2.CPH_EndDate = new ZDate(2021, 1, 31);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK; // prior change to declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self triggers autopopulation of declarant

			var inwardProcessingAuthorizationNumberList = instruction.Lookups.InwardProcessingAuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER1", inwardProcessingAuthorizationNumberList.CodesAsString);
				AssertSame("Cached", inwardProcessingAuthorizationNumberList, instruction.Lookups.InwardProcessingAuthorizationNumberList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}
		CusEntryInstruction instruction;
		JobDeclaration declaration;
	}
}
