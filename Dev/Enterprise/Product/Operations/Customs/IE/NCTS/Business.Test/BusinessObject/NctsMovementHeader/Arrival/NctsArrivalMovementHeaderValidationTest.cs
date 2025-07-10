using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	public class NctsArrivalMovementHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckGoodsLocationDescription()
		{
			var cusGoodsLocation = arrivalMovement.GoodsLocation;
			var typeMessage = MandatoryValidation.MustBeEnteredMessage(cusGoodsLocation.CGL_TypeInfo.Description);
			var qualifierMessage = MandatoryValidation.MustBeEnteredMessage(cusGoodsLocation.CGL_QualifierInfo.Description);

			cusGoodsLocation.CGL_Qualifier = "";
			cusGoodsLocation.CGL_Type = "ab";
			arrivalMovement.Validation.ValidateGoodsLocationDescription();
			AssertHasMessageError(arrivalMovement.GoodsLocationDescriptionInfo, qualifierMessage);
			AssertNoMessageError(arrivalMovement.GoodsLocationDescriptionInfo, typeMessage);

			cusGoodsLocation.CGL_Qualifier = "c";
			cusGoodsLocation.CGL_Type = "";
			arrivalMovement.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError(arrivalMovement.GoodsLocationDescriptionInfo, qualifierMessage);
			AssertHasMessageError(arrivalMovement.GoodsLocationDescriptionInfo, typeMessage);

			cusGoodsLocation.CGL_Qualifier = "";
			cusGoodsLocation.CGL_Type = "";
			arrivalMovement.Validation.ValidateGoodsLocationDescription();
			AssertHasMessageError(arrivalMovement.GoodsLocationDescriptionInfo, qualifierMessage);
			AssertHasMessageError(arrivalMovement.GoodsLocationDescriptionInfo, typeMessage);

			cusGoodsLocation.CGL_Qualifier = "c";
			cusGoodsLocation.CGL_Type = "ab";
			arrivalMovement.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError(arrivalMovement.GoodsLocationDescriptionInfo, qualifierMessage);
			AssertNoMessageError(arrivalMovement.GoodsLocationDescriptionInfo, typeMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalMovement = nctsHeader.ArrivalMovementHeader;
		}
		NctsHeader nctsHeader;
		NctsArrivalMovementHeader arrivalMovement;
	}
}
