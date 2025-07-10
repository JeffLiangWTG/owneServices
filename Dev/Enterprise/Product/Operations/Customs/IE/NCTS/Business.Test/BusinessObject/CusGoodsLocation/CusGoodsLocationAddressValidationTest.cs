using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRule065()
		{
			var goodslocation = Factory.New<CusGoodsLocation>();
			var propertyInfoAuthorisation = goodslocation.Address.E2_GovRegNumInfo;

			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			goodslocation.Address.AuthorisationNumber = "auth";
			AssertNoMessageErrorContaining(propertyInfoAuthorisation, "required");

			var euGoods = Factory.NewWithValidTestData<EU.NCTS.Business.CusGoodsLocation>();
			var nctsHeader = Factory.NewWithValidTestData<EU.NCTS.Business.NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			euGoods.Parent = movementHeader;
			euGoods.CGL_LocationUse = "ARR";
			euGoods.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			euGoods.Address.E2_GovRegNum = ZString.Empty;
			AssertHasMessageErrorContaining(euGoods.Address.AuthorisationNumberInfo, "required");
		}
	}
}
