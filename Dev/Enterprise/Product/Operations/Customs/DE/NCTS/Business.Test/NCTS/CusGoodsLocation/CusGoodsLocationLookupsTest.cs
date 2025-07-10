using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using LinkedCusAuthorisationRuleTypeList = Enterprise.Customs.Business.LinkedCusAuthorisationRuleTypeList;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQualifierList()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var lookups = nctsHeader.ArrivalMovementHeader.GoodsLocation.Lookups;
			var list = lookups.QualifierList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "Y", list.CodesAsString);
				AssertSame("Cached", list, lookups.QualifierList);
			});
		}

		public void TestQualifierList_ParentIsNotMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var incidentLookups = nctsHeader.EnRouteIncidents.AddNew().GoodsLocation.Lookups;
			var list = incidentLookups.QualifierList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "U, W, Z", list.CodesAsString);
				AssertSame("Cached", list, incidentLookups.QualifierList);
			});
		}

		public void TestAdditionalIdentifiersList_Arrival()
		{
			var officeCode = "DE010101";
			var cusAuthorisationHeader = CreateCusAuthorisationHeaderForTest(NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit, officeCode);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			var cusGoodsLocation = nctsHeader.ArrivalMovementHeader.GoodsLocation;
			arrivalMovement.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			arrivalMovement.DestinationCustomsOfficeCodeForArrival = officeCode;
			cusGoodsLocation.AddressIdentificationHolderPK = cusAuthorisationHeader.CPH_OH_PermitHolder;
			cusGoodsLocation.AddressAuthorisationNumber = cusAuthorisationHeader.CPH_Number;

			CombineAssertions(() =>
			{
				var lookups = cusGoodsLocation.Lookups;
				AssertEquals("CodeAsString", "T001, T002", lookups.AdditionalIdentifierList.CodesAsString);

				arrivalMovement.DestinationCustomsOfficeCodeForArrival = ZString.Empty;
				AssertEquals("No DestinationCustomsOfficeCodeForArrival", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);

				arrivalMovement.DestinationCustomsOfficeCodeForArrival = officeCode;
				arrivalMovement.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
				AssertEquals("Not ACE type", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);

				arrivalMovement.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
				cusGoodsLocation.AddressAuthorisationNumber = "XXX";
				AssertEquals("Invalid AuthorisationNumber", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);

				cusGoodsLocation.AddressIdentificationHolderPK = ZGuid.Empty;
				cusGoodsLocation.AddressAuthorisationNumber = ZString.Empty;
				AssertEquals("No ACE authorization", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);
			});
		}

		public void TestAdditionalIdentifiersList_Departure()
		{
			var officeCode = "DE010101";
			var cusAuthorisationHeader = CreateCusAuthorisationHeaderForTest(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, officeCode);

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			var office1 = movementHeader.CustomsOfficesForDeparture.AddNew();
			office1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			office1.CY_Data = officeCode;

			var authorizationUsage1 = header.MovementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_Number = cusAuthorisationHeader.CPH_Number;
			authorizationUsage1.AGC_OH_Owner = cusAuthorisationHeader.CPH_OH_PermitHolder;
			authorizationUsage1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			CombineAssertions(() =>
			{
				var cusGoodsLocation = header.MovementHeader.GoodsLocation;
				var lookups = cusGoodsLocation.Lookups;
				AssertEquals("CodeAsString", "T001, T002", lookups.AdditionalIdentifierList.CodesAsString);

				movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
				AssertEquals("No DEP office", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);

				office1 = movementHeader.CustomsOfficesForDeparture.AddNew();
				office1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				office1.CY_Data = officeCode;
				authorizationUsage1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertEquals("No ACR authorization", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);

				authorizationUsage1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				authorizationUsage1.AGC_Number = "XXX";
				AssertEquals("Invalid AuthorisationNumber", string.Empty, lookups.AdditionalIdentifierList.CodesAsString);
			});
		}

		CusAuthorisationHeader CreateCusAuthorisationHeaderForTest(ZString type, ZString officeCode)
		{
			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			cusAuthorisationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusAuthorisationHeader.CPH_Type = type;

			var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule1.CPR_ValueFrom = "T001";
			rule1.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, officeCode);

			var rule2 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			rule2.CPR_ValueFrom = "T002";
			rule2.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, officeCode);

			Factory.Save();

			return cusAuthorisationHeader;
		}
	}
}
