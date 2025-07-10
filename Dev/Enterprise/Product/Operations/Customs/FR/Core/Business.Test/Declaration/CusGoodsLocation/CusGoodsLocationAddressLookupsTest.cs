using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusGoodsLocationAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAuthorisationNumberList_AuthorisationNumberFilterBusinessObjectDefaults()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var location = header.GoodsLocation;
			var locationAddress = (CusGoodsLocationAddress)location.Address;
			var lookups = locationAddress.Lookups;
			locationAddress.AuthorisationNumber = "001";

			location.CGL_Type = ZString.Empty;
			var numberList = (CusAuthorisationHeaderCollection)lookups.AuthorisationNumberList;
			var authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
			AssertEquals("CGL_Type is neither B nor C, default value for AuthorisationNumber search box is equal to AuthorisationNumber of locationAddress", "001", authorisationNumberFilterBO.Value);

			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			numberList = (CusAuthorisationHeaderCollection)lookups.AuthorisationNumberList;
			authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
			AssertEquals("CGL_Type is B, default value for AuthorisationNumber search box is equal to FRTST", "FRTST", authorisationNumberFilterBO.Value);

			location.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			numberList = (CusAuthorisationHeaderCollection)lookups.AuthorisationNumberList;
			authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
			AssertEquals("CGL_Type is C, default value for AuthorisationNumber search box is equal to LADT", "LADT", authorisationNumberFilterBO.Value);

			var location2 = Factory.New<CusGoodsLocation>();
			var locationAddress2 = (CusGoodsLocationAddress)location2.Address;
			var lookups2 = locationAddress2.Lookups;
			locationAddress2.AuthorisationNumber = "002";
			location2.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			numberList = (CusAuthorisationHeaderCollection)lookups2.AuthorisationNumberList;
			authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
			AssertEquals("Parent of location2 is not TemporaryHeader, default value for AuthorisationNumber search box is equal to AuthorisationNumber of locationAddress2", "002", authorisationNumberFilterBO.Value);
		}
	}
}
