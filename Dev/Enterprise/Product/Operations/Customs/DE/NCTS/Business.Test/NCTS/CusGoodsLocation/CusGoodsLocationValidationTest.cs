using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCheckCGL_Type()
		{
			var cusGoodsLocation = CreateCusGoodsLocationForArrivalMovementHeader();
			CombineAssertions(() =>
			{
				AssertNoErrors("No mandatory validation", cusGoodsLocation.CGL_TypeInfo);
				cusGoodsLocation.CGL_Type = "?";
				AssertNoErrors("No list validation", cusGoodsLocation.CGL_TypeInfo);
			});
		}

		public void TestCheckCGL_AdditionalIdentifier_ListValidation()
		{
			var cusGoodsLocation = CreateCusGoodsLocationForArrivalMovementHeader();
			cusGoodsLocation.CGL_AdditionalIdentifier = "0000";
			AssertHasWarning(cusGoodsLocation.CGL_AdditionalIdentifierInfo, "The captured Additional Identifier is not a valid Location Code of the selected Authorization.");
		}

		CusGoodsLocation CreateCusGoodsLocationForArrivalMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader.ArrivalMovementHeader.GoodsLocation;
		}
	}
}
