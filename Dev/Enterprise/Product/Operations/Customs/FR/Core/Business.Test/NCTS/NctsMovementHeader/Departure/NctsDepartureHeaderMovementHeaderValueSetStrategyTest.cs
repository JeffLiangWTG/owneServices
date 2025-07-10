using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class NctsDepartureHeaderMovementHeaderValueSetStrategyTest : TestCaseWithFactory
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

			var consignorWithOkAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			consignorWithOkAuthorisation.OH_Code = "CONOK";

			var consignorAddressWithOKaddress = consignorWithOkAuthorisation.Addresses.AddNew();
			consignorAddressWithOKaddress.AddressCode = "consignorMatchAddress";
			consignorAddressWithOKaddress.Address1 = "consignorMatchAddress";

			var consignorAddressWithKOaddress = consignorWithOkAuthorisation.Addresses.AddNew();
			consignorAddressWithKOaddress.AddressCode = "consignorNotMatchAddress";
			consignorAddressWithKOaddress.Address1 = "consignorNotMatchAddress";

			var consignorWithKOAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			consignorWithKOAuthorisation.OH_Code = "CONKO";

			var consignorKOAddress = consignorWithKOAuthorisation.Addresses.AddNew();
			consignorKOAddress.AddressCode = "consignorMatchAddresstest";
			consignorKOAddress.Address1 = "consignorMatchAddresstest";

			var authorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = declarantWithKOAuthorisation.PK;
			authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authorisation.CPH_Number = "KOAddress";
			authorisation.CPH_OA_AppliesTo = consignorAddressWithKOaddress.PK;
			authorisation.CPH_Type = "ACR";

			var authorisation2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisation2.CPH_OH_PermitHolder = declarantWithOkAuthorisation.PK;
			authorisation2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authorisation2.CPH_Number = "OKAddress";
			authorisation2.CPH_OA_AppliesTo = consignorAddressWithOKaddress.PK;
			authorisation2.CPH_Type = "ACR";

			var authorisation3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisation3.CPH_OH_PermitHolder = declarantWithKOAuthorisation.PK;
			authorisation3.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authorisation3.CPH_Number = "KOcphtype";
			authorisation3.CPH_OA_AppliesTo = consignorKOAddress.PK;
			authorisation3.CPH_Type = "OTH";

			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;

			departureMovement.IsSimplifiedNctsProcedure = false;

			departureMovement.IsSimplifiedNctsProcedure = true;

			Factory.Save();
			AssertEquals("BM_LocationOfGoodsCode is Empty as there is no declarant or consignor in the nctsheader", "", departureMovement.BM_LocationOfGoodsCode);

			nctsHeader.Declarant.OrganisationPK = declarantWithOkAuthorisation.PK;
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there IsSimplifiedNctsProcedure = false | value used are = no consignor && declarantWithOkAuthorisation", "", departureMovement.BM_LocationOfGoodsCode);
			departureMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there is no consignor in the nctsheader", "", departureMovement.BM_LocationOfGoodsCode);

			nctsHeader.Consignor.OrganisationPK = consignorWithOkAuthorisation.PK;
			nctsHeader.Consignor.E2_OA_Address = consignorAddressWithOKaddress.PK;
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there IsSimplifiedNctsProcedure = false | value used are =consignorAddressWithOKaddress && declarantWithOkAuthorisation", "", departureMovement.BM_LocationOfGoodsCode);
			departureMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is valued OKAddress as there as there is an authorisation corresponding to the pair of consignor/declarant of the nctsheader and the cph type is ACR", "OKAddress", departureMovement.BM_LocationOfGoodsCode);

			nctsHeader.Consignor.OrganisationPK = consignorWithKOAuthorisation.PK;
			nctsHeader.Consignor.E2_OA_Address = consignorAddressWithKOaddress.PK;
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there IsSimplifiedNctsProcedure = false | value used are = consignorAddressWithKOaddress && declarantWithOkAuthorisation", "", departureMovement.BM_LocationOfGoodsCode);
			departureMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there is no authorisation corresponding to the pair of consignor/declarant of the nctsheader | value used are = consignorAddressWithKOaddress && declarantWithOkAuthorisation", "", departureMovement.BM_LocationOfGoodsCode);

			nctsHeader.Consignor.OrganisationPK = consignorWithOkAuthorisation.PK;
			nctsHeader.Consignor.E2_OA_Address = consignorAddressWithOKaddress.PK;
			nctsHeader.Declarant.OrganisationPK = declarantWithKOAuthorisation.PK;
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there IsSimplifiedNctsProcedure = false | value used are = consignorAddressWithOKaddress && declarantWithKOAuthorisation", "", departureMovement.BM_LocationOfGoodsCode);
			departureMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there is no authorisation corresponding to the pair of consignor/declarant of the nctsheader | value used are = consignorAddressWithOKaddress && declarantWithKOAuthorisation", "", departureMovement.BM_LocationOfGoodsCode);

			nctsHeader.Consignor.OrganisationPK = consignorWithKOAuthorisation.PK;
			nctsHeader.Consignor.E2_OA_Address = consignorKOAddress.PK;
			nctsHeader.Declarant.OrganisationPK = declarantWithKOAuthorisation.PK;
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there IsSimplifiedNctsProcedure = false | value used are = consignorKOAddress && declarantWithKOAuthorisation", "", departureMovement.BM_LocationOfGoodsCode);
			departureMovement.IsSimplifiedNctsProcedure = true;
			AssertEquals("BM_LocationOfGoodsCode is Empty as there is an authorisation corresponding to the pair of consignor/declarant of the nctsheader but cph type is not ACR", "", departureMovement.BM_LocationOfGoodsCode);
		}

		public void TestHarbourFeesRecalculatedWhenBM_GrossWeightChange()
		{
			var movementHeader = PrepareDataForBM_GrossWeightValueSetStrategyTest();
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);

			var bill = movementHeader.Header.Bills.FirstOrDefault();
			var goodsItem = bill.GoodsItems.FirstOrDefault();
			var fee = goodsItem.Fees.FirstOrDefault();

			AssertEquals(1, goodsItem.Fees.Count);
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(5m, fee.BFE_ChargeAmount);

			movementHeader.BM_GrossWeight = 10000m;
			fee = goodsItem.Fees.FirstOrDefault();

			AssertEquals(1, goodsItem.Fees.Count);
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(13m, fee.BFE_ChargeAmount);
		}

		NctsDepartureMovementHeader PrepareDataForBM_GrossWeightValueSetStrategyTest()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "94", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TFCL] > 1, MAX(5.1, [TFCL] * 1.2911), 0)", "FR");
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var headerContainer = header.DepartureHeaderContainers.AddNew();
			headerContainer.BC_Mode = "FCL";
			headerContainer.BC_ContainerNum = "1";

			var movementHeader = header.MovementHeader;
			movementHeader.BM_GrossWeight = 2000m;
			movementHeader.ChargePaymentOrDestinationID = "94";

			var goodsItem = movementHeader.Header.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();

			var containersPivot = package.ContainersPivotsForBindingOnly[0];
			containersPivot.ContainerSelected = true;

			Factory.Save();
			return movementHeader;
		}
	}
}
