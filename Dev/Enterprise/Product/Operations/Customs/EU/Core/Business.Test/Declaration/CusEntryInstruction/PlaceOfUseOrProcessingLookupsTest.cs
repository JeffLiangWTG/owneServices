using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Testing
{
	class PlaceOfUseOrProcessingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQualifierList()
		{
			var placeOfUseOrProcessing = Factory.New<PlaceOfUseOrProcessing>();
			var expectedCodes = new ZString[]
			{
				CusGoodsLocationQualifierList.Codes.Address,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier,
				CusGoodsLocationQualifierList.Codes.UnLocode,
			};
			AssertContainsExactElementsInAnyOrder(expectedCodes, placeOfUseOrProcessing.Lookups.QualifierList.GetAllCodes());
		}
	}
}
