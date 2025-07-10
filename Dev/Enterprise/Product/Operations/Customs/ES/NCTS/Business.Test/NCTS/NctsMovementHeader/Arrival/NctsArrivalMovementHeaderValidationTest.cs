using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsArrivalMovementHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBM_InBondEntryType_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(arrivalMovement.BM_InBondEntryTypeInfo, "XXX", NctsArrivalDocTypeList.Codes.Dat);
		}

		public void TestCheckGoodsLocationDescription()
		{
			var cusGoodsLocation = arrivalMovement.GoodsLocation;
			cusGoodsLocation.CGL_Qualifier = "Y";
			arrivalMovement.Validation.ValidateGoodsLocationDescription();

			AssertNoMessageErrors("Validation not apply in ES", arrivalMovement.GoodsLocationDescriptionInfo);
		}

		public void TestCheckBM_ArrivalDate()
		{
			arrivalMovement.BM_ArrivalDate = ZDateTime.Empty;
			arrivalMovement.Validation.ValidateBM_ArrivalDate();

			AssertNoMessageErrors("Validation not apply in ES", arrivalMovement.BM_ArrivalDateInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalMovement = header.ArrivalMovementHeader;
		}
		NctsHeader header;
		NctsArrivalMovementHeader arrivalMovement;
	}
}
