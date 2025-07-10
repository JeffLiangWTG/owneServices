using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class NctsArrivalMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNctsTransitStatusList()
		{
			nctsHeader.BH_ApplicationCode = "NCT";
			AssertContainsExactElementsInAnyOrder(new NctsTransitStatusList().GetAllCodes(), lookups.NctsTransitStatusList.GetAllCodes());
			nctsHeader.BH_ApplicationCode = "NC5";
			AssertContainsExactElementsInAnyOrder(new NCTS5ArrivalCustomsStatusList().GetAllCodes(), lookups.NctsTransitStatusList.GetAllCodes());
		}

		public void TestNctsTransitStatusList_Cached()
		{
			nctsHeader.BH_ApplicationCode = "NCT";
			AssertSame(lookups.NctsTransitStatusList, lookups.NctsTransitStatusList);
			nctsHeader.BH_ApplicationCode = "NC5";
			AssertSame(lookups.NctsTransitStatusList, lookups.NctsTransitStatusList);
		}

		public void TestExpectedNextCustomsProcedureList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.ExpectedNextCustomsProcedure, "Customs Destination Type");
			helper.CreateNewOrGetExistingCusCodeList("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.ExpectedNextCustomsProcedure, "1", "Mise en libre pratique (avec ou sans MAC)", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.ExpectedNextCustomsProcedure, "2", "Placement sous régime économique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.ExpectedNextCustomsProcedure, "3", "MDT - magasin de dépôt temporaire", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.ExpectedNextCustomsProcedure, "4", "Placement sous transit", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var list = lookups.ExpectedNextCustomsProcedureList;
			AssertContainsExactElementsInExactOrder("List should contain all the codes with DataGrouping = 'FR' and CodeType = 'CL907'.", new[] { "1", "2", "3", "4" }, list.GetAllCodes());
			AssertSame("List should be same when loaded in 2nd attempt from cache with the first attempt.", list, lookups.ExpectedNextCustomsProcedureList);

			CombineAssertions("Description corresponding to each code should be as per expected.", () =>
			{
				AssertEquals("Mise en libre pratique (avec ou sans MAC)", list.GetDescriptionFromCode("1"));
				AssertEquals("Placement sous régime économique", list.GetDescriptionFromCode("2"));
				AssertEquals("MDT - magasin de dépôt temporaire", list.GetDescriptionFromCode("3"));
				AssertEquals("Placement sous transit", list.GetDescriptionFromCode("4"));
			});
		}

		public void TestLocationOfGoodsCodeList()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var auth1 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit);
			auth1.CPH_Number = "FR7758258";

			var auth2 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			auth2.CPH_Number = "FR8886886";

			var auth3 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit);
			auth3.CPH_Number = "FR5438438";
			auth3.CPH_RN_NKCountryCode = Constants.CountryCodes.Germany;

			AssertContainsExactElementsInAnyOrder(new[] { auth1.CPH_Number }, lookups.LocationOfGoodsCodeList.Select(x => x.CPH_Number));

			auth3.CPH_RN_NKCountryCode = Constants.CountryCodes.France;

			AssertContainsExactElementsInAnyOrder(new[] { auth1.CPH_Number, auth3.CPH_Number }, lookups.LocationOfGoodsCodeList.Select(x => x.CPH_Number));
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			lookups = nctsHeader.ArrivalMovementHeader.Lookups;
		}
		NctsHeader nctsHeader;
		NctsArrivalMovementHeaderLookups lookups;
	}
}
