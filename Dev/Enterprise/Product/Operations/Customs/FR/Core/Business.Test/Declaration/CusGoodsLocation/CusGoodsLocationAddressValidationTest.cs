using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAuthorisationNumber_BR_PN_TS_FR06()
		{
			var message = "Authorization No. must start with 'FRTST'.";
			var header = Factory.New<TemporaryStorageHeader>();
			var location = header.GoodsLocation;
			var locationAddress = (CusGoodsLocationAddress)location.Address;

			location.CGL_Type = ZString.Empty;
			locationAddress.AuthorisationNumber = "ABC";
			AssertNoMessageError("CGL_Type is not B", locationAddress.AuthorisationNumberInfo, message);

			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			locationAddress.AuthorisationNumber = "ABC";
			AssertHasMessageError("Authorization No. must start with 'FRTST' when CGL_Type is B", locationAddress.AuthorisationNumberInfo, message);
			locationAddress.AuthorisationNumber = "FRTS";
			AssertHasMessageError("Authorization No. must start with 'FRTST' when CGL_Type is B", locationAddress.AuthorisationNumberInfo, message);
			locationAddress.AuthorisationNumber = "FRTST100";
			AssertNoMessageError("Authorization No. must start with 'FRTST' when CGL_Type is B", locationAddress.AuthorisationNumberInfo, message);

			var location2 = Factory.New<CusGoodsLocation>();
			var locationAddress2 = (CusGoodsLocationAddress)location2.Address;
			location2.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			locationAddress2.AuthorisationNumber = "ABC";
			AssertNoMessageError("Parent of location2 is not TemporaryHeader", locationAddress2.AuthorisationNumberInfo, "Authorization No. must start with 'FRTST'.");
		}

		public void TestCheckAuthorisationNumber_BR_PN_TS_FR08()
		{
			var message = "Authorization No. must start with 'LADT'.";
			var header = Factory.New<TemporaryStorageHeader>();
			var location = header.GoodsLocation;
			var locationAddress = (CusGoodsLocationAddress)location.Address;

			location.CGL_Type = ZString.Empty;
			locationAddress.AuthorisationNumber = "ABC";
			AssertNoMessageError("CGL_Type is not C", locationAddress.AuthorisationNumberInfo, message);

			location.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			locationAddress.AuthorisationNumber = "ABC";
			AssertHasMessageError("Authorization No. must start with 'LADT' when CGL_Type is C", locationAddress.AuthorisationNumberInfo, message);
			locationAddress.AuthorisationNumber = "LAD";
			AssertHasMessageError("Authorization No. must start with 'LADT' when CGL_Type is C", locationAddress.AuthorisationNumberInfo, message);
			locationAddress.AuthorisationNumber = "LADT100";
			AssertNoMessageError("Authorization No. must start with 'LADT' when CGL_Type is C", locationAddress.AuthorisationNumberInfo, message);

			var location2 = Factory.New<CusGoodsLocation>();
			var locationAddress2 = (CusGoodsLocationAddress)location2.Address;
			location2.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			locationAddress2.AuthorisationNumber = "ABC";
			AssertNoMessageError("Parent of location2 is not TemporaryHeader", locationAddress2.AuthorisationNumberInfo, "Authorization No. must start with 'LADT'.");
		}
	}
}
