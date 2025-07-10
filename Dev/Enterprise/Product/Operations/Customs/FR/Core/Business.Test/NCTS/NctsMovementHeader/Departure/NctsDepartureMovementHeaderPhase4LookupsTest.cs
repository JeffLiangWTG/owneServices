using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsDepartureMovementHeaderPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNctsTransitStatusList()
		{
			AssertContainsExactElementsInAnyOrder(new NctsTransitStatusList().GetAllCodes(), lookups.NctsTransitStatusList.GetAllCodes());
		}

		public void TestNctsTransitStatusList_Cached()
		{
			AssertSame(lookups.NctsTransitStatusList, lookups.NctsTransitStatusList);
		}

		public void TestAuthorizedLocationOfGoodsCodeList()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var auth1 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
			auth1.CPH_Number = "FR7758258";
			auth1.CPH_OH_PermitHolder = consignor.PK;

			var auth2 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			auth2.CPH_Number = "FR8886886";
			auth2.CPH_OH_PermitHolder = consignor.PK;

			var auth3 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
			auth3.CPH_Number = "FR5438438";
			auth3.CPH_OH_PermitHolder = consignor.PK;
			auth3.CPH_RN_NKCountryCode = Constants.CountryCodes.Germany;

			var auth4 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
			auth4.CPH_OH_PermitHolder = ZGuid.BrettsGuid;
			auth4.CPH_Number = "FR7654321";
			auth4.CPH_RN_NKCountryCode = Constants.CountryCodes.France;

			AssertContainsExactElementsInAnyOrder(new[] { auth1.CPH_Number }, lookups.AuthorizedLocationOfGoodsCodeList.Select(x => x.CPH_Number));

			auth3.CPH_RN_NKCountryCode = Constants.CountryCodes.France;

			AssertContainsExactElementsInAnyOrder(new[] { auth1.CPH_Number, auth3.CPH_Number }, lookups.AuthorizedLocationOfGoodsCodeList.Select(x => x.CPH_Number));

			auth4.CPH_OH_PermitHolder = consignor.PK;

			AssertContainsExactElementsInAnyOrder("List of authorised locations should be filtered on Country = Declaration Country, Type = 'ACR' and Holder = Declaration Consignor", new[] { auth1.CPH_Number, auth3.CPH_Number, auth4.CPH_Number }, lookups.AuthorizedLocationOfGoodsCodeList.Select(x => x.CPH_Number));
		}

		public void TestBarrierPort()
		{
			AssertEquals("BarrierPort should be equal to Port of Dispatch.", "FRCDG", lookups.BarrierPort);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			consignor = Factory.New<OrgHeader>();
			nctsHeader.Consignor.E2_OA_Address = consignor.MainAddress.PK;
			nctsHeader.PortOfDispatch = "FRCDG";
			lookups = nctsHeader.MovementHeader.FRLookups;
		}
		IFRNctsDepartureMovementHeaderLookups lookups;
		OrgHeader consignor;
	}
}
