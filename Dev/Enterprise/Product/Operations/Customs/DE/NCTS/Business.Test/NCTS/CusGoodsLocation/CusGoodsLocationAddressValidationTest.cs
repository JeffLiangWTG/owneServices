using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPhoneNumberMandatoryIfNameEntered()
		{
			CombineAssertions(() =>
			{
				cusGoodsLocationAddress.Validation.ValidateE2_Phone();
				AssertNoMessageErrorContaining(cusGoodsLocationAddress.E2_PhoneInfo, MandatoryValidation.YouHaveNotEntered);

				cusGoodsLocation.Address.E2_Contact = "Test Name";
				cusGoodsLocationAddress.Validation.ValidateE2_Phone();
				AssertHasMessageErrorContaining(cusGoodsLocationAddress.E2_PhoneInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckE2_GovRegNum_ParentIsDepartureMovementHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsLocation = header.MovementHeader.GoodsLocation;
			var goodsLocationAddress = goodsLocation.Address;
			var goodsLocationAddressInfo = goodsLocationAddress.E2_GovRegNumInfo;

			AssertNoNotifications("No need to checkE2_GovRegNum", goodsLocationAddressInfo);
		}

		public void TestCheckE2_GovRegNum_ParentIsArrivalMovementHeader()
		{
			var messageAuthorisation = "[C0065] Location: Authorization No. required when qualifier is 'Y'.";
			var goodsLocationAddressInfo = cusGoodsLocationAddress.E2_GovRegNumInfo;

			CombineAssertions(() =>
			{
				AssertHasMessageError("Authorization No. required when qualifier is 'Y'", goodsLocationAddressInfo, messageAuthorisation);

				cusGoodsLocationAddress.AuthorisationNumber = "auth";
				AssertNoMessageError("Authorization No. is not empty", goodsLocationAddressInfo, messageAuthorisation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			cusGoodsLocation = nctsHeader.ArrivalMovementHeader.GoodsLocation;
			cusGoodsLocationAddress = cusGoodsLocation.Address;
		}

		CusGoodsLocation cusGoodsLocation;
		CusGoodsLocationAddress cusGoodsLocationAddress;
	}
}
