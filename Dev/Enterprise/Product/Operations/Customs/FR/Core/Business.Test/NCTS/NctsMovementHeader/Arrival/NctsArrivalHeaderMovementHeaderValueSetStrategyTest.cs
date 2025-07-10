using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class NctsArrivalHeaderMovementHeaderValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestCPHNumberChangedWhenMovementHeaderIsSimplifiedNctsProcedureChange()
		{
			var declarantWithOkAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			declarantWithOkAuthorisation.OH_Code = "DECOK";

			var declarantAddressWithOkAuthorisation = declarantWithOkAuthorisation.Addresses.AddNew();
			declarantAddressWithOkAuthorisation.AddressCode = "DeclarantMatchAddress";
			declarantAddressWithOkAuthorisation.Address1 = "DeclarantMatchAddress";

			var declarantWithKOAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			declarantWithKOAuthorisation.OH_Code = "DECKO";

			var declarantAddressWithKOAuthorisation = declarantWithOkAuthorisation.Addresses.AddNew();
			declarantAddressWithKOAuthorisation.AddressCode = "DeclarantNotMatchAddress";
			declarantAddressWithKOAuthorisation.Address1 = "DeclarantNotMatchAddress";

			var destinationTraderWithOkAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			destinationTraderWithOkAuthorisation.OH_Code = "CONOK";

			var destinationTraderAddressWithOKaddress = destinationTraderWithOkAuthorisation.Addresses.AddNew();
			destinationTraderAddressWithOKaddress.AddressCode = "destMatchAddress";
			destinationTraderAddressWithOKaddress.Address1 = "destMatchAddress";

			var destinationTraderAddressWithKOaddress = destinationTraderWithOkAuthorisation.Addresses.AddNew();
			destinationTraderAddressWithKOaddress.AddressCode = "destNotMatchAddress";
			destinationTraderAddressWithKOaddress.Address1 = "destNotMatchAddress";

			var destinationTraderWithKOAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			destinationTraderWithKOAuthorisation.OH_Code = "CONKO";

			var destinationTraderKOAddress = destinationTraderWithKOAuthorisation.Addresses.AddNew();
			destinationTraderKOAddress.AddressCode = "destMatchAddresstest";
			destinationTraderKOAddress.Address1 = "destMatchAddresstest";

			var authorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = declarantWithKOAuthorisation.PK;
			authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authorisation.CPH_Number = "KOAddress";
			authorisation.CPH_OA_AppliesTo = destinationTraderAddressWithKOaddress.PK;
			authorisation.CPH_Type = "ACE";

			var authorisation2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisation2.CPH_OH_PermitHolder = declarantWithOkAuthorisation.PK;
			authorisation2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authorisation2.CPH_Number = "OKAddress";
			authorisation2.CPH_OA_AppliesTo = destinationTraderAddressWithOKaddress.PK;
			authorisation2.CPH_Type = "ACE";

			var authorisation3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisation3.CPH_OH_PermitHolder = declarantWithKOAuthorisation.PK;
			authorisation3.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authorisation3.CPH_Number = "KOcphtype";
			authorisation3.CPH_OA_AppliesTo = destinationTraderKOAddress.PK;
			authorisation3.CPH_Type = "OTH";

			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var arrivalMovement = nctsHeader.ArrivalMovementHeader;

			arrivalMovement.IsSimplifiedNctsProcedure = false;
			arrivalMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there is no declarant or destinationTrader in the nctsheader", "", arrivalMovement.BM_LocationOfGoodsCode);

			nctsHeader.Declarant.OrganisationPK = declarantWithOkAuthorisation.PK;
			arrivalMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there IsSimplifiedNctsProcedure = false | value used are = no destination trader && declarantWithOkAuthorisation", "", arrivalMovement.BM_LocationOfGoodsCode);
			arrivalMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there is no destinationTrader in the nctsheader", "", arrivalMovement.BM_LocationOfGoodsCode);

			nctsHeader.DestinationTrader.OrganisationPK = destinationTraderWithOkAuthorisation.PK;
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTraderAddressWithOKaddress.PK;
			arrivalMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there IsSimplifiedNctsProcedure = false | value used are = destinationTraderAddressWithOKaddress && declarantWithOkAuthorisation", "", arrivalMovement.BM_LocationOfGoodsCode);
			arrivalMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is valued OKAddress as there is an authorisation corresponding to the pair of destinationTrader/declarant of the nctsheader and the cph type is ACE", "OKAddress", arrivalMovement.BM_LocationOfGoodsCode);

			nctsHeader.DestinationTrader.OrganisationPK = destinationTraderWithOkAuthorisation.PK;
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTraderAddressWithKOaddress.PK;
			arrivalMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is OKAddress as there IsSimplifiedNctsProcedure = false | value used are = destinationTraderAddressWithKOaddress && declarantWithOkAuthorisation", "OKAddress", arrivalMovement.BM_LocationOfGoodsCode);
			arrivalMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is OKAddress as there is no authorisation corresponding to the pair of destinationTrader/declarant of the nctsheader | value use are = destinationTraderAddressWithKOaddress && declarantWithOkAuthorisation", "OKAddress", arrivalMovement.BM_LocationOfGoodsCode);

			nctsHeader.DestinationTrader.OrganisationPK = destinationTraderWithOkAuthorisation.PK;
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTraderAddressWithOKaddress.PK;
			nctsHeader.Declarant.OrganisationPK = declarantWithKOAuthorisation.PK;
			arrivalMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is OKAddress as there IsSimplifiedNctsProcedure = false | value used are = destinationTraderAddressWithOKaddress && declarantWithKOAuthorisation", "OKAddress", arrivalMovement.BM_LocationOfGoodsCode);
			arrivalMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is OKAddress as there is no authorisation corresponding to the pair of destinationTrader/declarant of the nctsheader | value used are = destinationTraderAddressWithOKaddress && declarantWithKOAuthorisation", "OKAddress", arrivalMovement.BM_LocationOfGoodsCode);

			nctsHeader.DestinationTrader.OrganisationPK = destinationTraderWithKOAuthorisation.PK;
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTraderKOAddress.PK;
			nctsHeader.Declarant.OrganisationPK = declarantWithKOAuthorisation.PK;
			arrivalMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is OKAddress as there IsSimplifiedNctsProcedure = false | value used are = destinationTraderKOAddress && declarantWithKOAuthorisation", "OKAddress", arrivalMovement.BM_LocationOfGoodsCode);
			arrivalMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is OKAddress as there is an authorisation corresponding to the pair of destinationTrader/declarant of the nctsheader but cph type is not ACE", "OKAddress", arrivalMovement.BM_LocationOfGoodsCode);
		}

		public void TestChangeNctsPropertiesWhenBMLocationOfGoodsCodeHasChanged()
		{
			var number = "FR7758258";
			var type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;

			var orgHeader = Factory.New<OrgHeader>();
			var authHeader = orgHeader.SetupAuthorisationHeader(type);
			authHeader.CPH_Number = number;
			authHeader.CPH_OA_AppliesTo_ZAddress.OrgPK = orgHeader.PK;
			authHeader.CPH_OH_PermitHolder = ZGuid.NewZGuid();

			var authRule = Factory.New<CusAuthorisationRule>();
			authRule.CPR_CPH_PermitHeader = authHeader.PK;
			authRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
			authRule.CPR_ValueFrom = "Wonderland";

			var arrivalMovementHeader = Factory.NewWithValidTestData<NctsArrivalMovementHeader>();
			arrivalMovementHeader.BM_LocationOfGoodsCode = number;

			var nctsHeader = arrivalMovementHeader.Header;
			AssertEquals(authRule.CPR_ValueFrom, arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival);
			AssertEquals(authHeader.CPH_OA_AppliesTo_ZAddress.OrgPK, nctsHeader.DestinationTrader.OrganisationPK);
			AssertEquals(authHeader.CPH_OH_PermitHolder, nctsHeader.Declarant.OrganisationPK);
		}
	}
}
