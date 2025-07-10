using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsCusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGL_AdditionalIdentifier()
		{
			goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
			departureMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;

			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("When GoodsLocation is not filled and Ncts is TNN and is not Phase5, no error expected", goodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				departureMovementHeader.BM_Phase = ZString.Empty;
				goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
				AssertNoMessageErrorContaining("When GoodsLocation is not filled and Ncts is not TNN and is Phase5, no error expected", goodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

				departureMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
				AssertHasMessageErrorContaining("When GoodsLocation is not filled and Ncts is TNN and is Phase5, error expected", goodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

				goodsLocation.CGL_AdditionalIdentifier = "COD";
				AssertNoMessageErrorContaining("When GoodsLocation is filled and Ncts is TNN and is Phase5, no error expected", goodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovementHeader = nctsHeader.MovementHeader;
			goodsLocation = departureMovementHeader.GoodsLocation;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovementHeader;
		CusGoodsLocation goodsLocation;
	}
}
