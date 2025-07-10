using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class SupernumeraryGoodsValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Description()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupernumeraryGoods.CSI_DescriptionInfo);
	}

	public void TestCheckCSI_Quantity() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupernumeraryGoods.CSI_QuantityInfo);
		ValidationTestHelper.AssertErrorIfValueIsNegative(SupernumeraryGoods.CSI_QuantityInfo);
	});

	public void TestCheckCSI_Tariff()
	{
		var refDataHelper = new RefDataTestHelper(Factory);
		refDataHelper.CreateTariffs(Universal.Constants.TariffTypes.HarmonizedSystem, RefDataGrouping.Codes.WorldCustomsOrganisationWCO).CreateTariff("123456").CreateTariff("12345");
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeMessageError(SupernumeraryGoods.CSI_TariffInfo, new[] { new ZString("923456"), new ZString("12345") }, new[] { new ZString("123456") });

		SupernumeraryGoods.CSI_Tariff = ZString.Empty;
		AssertNoNotifications("Tariff is optional", supernumeraryGoods.CSI_TariffInfo);
	}

	public void TestCheckCSI_PackQty() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupernumeraryGoods.CSI_PackQtyInfo);
		ValidationTestHelper.AssertErrorIfValueIsNegative(SupernumeraryGoods.CSI_PackQtyInfo);
	});

	public void TestCheckCSI_PackType()
	{
		var refDataHelper = new RefDataTestHelper(Factory);
		refDataHelper.CreateCodeList(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, RefDataGrouping.Codes.UnitedNationsRecommendations).CreateCode("01");
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(SupernumeraryGoods.CSI_PackTypeInfo, "02", "01");
	}

	SupernumeraryGoods SupernumeraryGoods => supernumeraryGoods ?? (supernumeraryGoods = CreateSupernumeraryGoods());
	SupernumeraryGoods supernumeraryGoods;

	SupernumeraryGoods CreateSupernumeraryGoods()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();
	}
}
