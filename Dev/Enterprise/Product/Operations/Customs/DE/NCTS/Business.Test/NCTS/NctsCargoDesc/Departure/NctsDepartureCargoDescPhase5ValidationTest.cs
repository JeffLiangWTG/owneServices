using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsDepartureCargoDescPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_CusC4Number()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Virtual");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "CUS Codes");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "01000001", "CUSCode 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "11000001");
			Factory.Save();

			goodsItem.BY_HarmonisedTariff = "11000001";
			ValidationTestHelper.AssertInvalidCodeMessageError(goodsItem.BY_CusC4NumberInfo, "INVALID", "01000001");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		}
		EU.NCTS.Business.NctsDepartureCargoDesc goodsItem;
	}
}
