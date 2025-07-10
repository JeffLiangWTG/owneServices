using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsDepartureMovementHeaderPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNctsTransitStatusList()
		{
			AssertContainsExactElementsInAnyOrder(new NCTS5DepartureCustomsStatusList().GetAllCodes(), lookups.NctsTransitStatusList.GetAllCodes());
		}

		public void TestNctsTransitStatusList_Cached()
		{
			AssertSame(lookups.NctsTransitStatusList, lookups.NctsTransitStatusList);
		}

		public void TestBarrierPort()
		{
			AssertEquals("BarrierPort should be equal to BM_RL_NKPortOfPresentation.", "FRCDG", lookups.BarrierPort);
		}

		public void TestAuthorizedLocationOfGoodsCodeList()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var auth1 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
			auth1.CPH_Number = "FR7758258";

			var auth2 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			auth2.CPH_Number = "FR8886886";

			var auth3 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
			auth3.CPH_Number = "FR5438438";
			auth3.CPH_RN_NKCountryCode = Constants.CountryCodes.Germany;

			AssertContainsExactElementsInAnyOrder(new[] { auth1.CPH_Number, auth2.CPH_Number }, lookups.AuthorizedLocationOfGoodsCodeList.Select(x => x.CPH_Number));

			auth3.CPH_RN_NKCountryCode = Constants.CountryCodes.France;

			AssertContainsExactElementsInAnyOrder(new[] { auth1.CPH_Number, auth2.CPH_Number, auth3.CPH_Number }, lookups.AuthorizedLocationOfGoodsCodeList.Select(x => x.CPH_Number));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_RL_NKPortOfPresentation = "FRCDG";
			lookups = nctsHeader.MovementHeader.FRLookups;
		}
		IFRNctsDepartureMovementHeaderLookups lookups;
	}
}
