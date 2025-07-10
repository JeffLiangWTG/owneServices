using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(SupernumeraryGoods))]
sealed class SupernumeraryGoodsTest : CusSupportingInfoTest<SupernumeraryGoods>
{
	public void TestLookups()
	{
		AssertType<SupernumeraryGoodsLookups>(SupernumeraryGoods.Lookups);
	}

	public void TestValidation()
	{
		AssertType<SupernumeraryGoodsValidation>(SupernumeraryGoods.Validation);
	}

	public void TestCaptions() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(SupernumeraryGoods.CSI_LineNoInfo, caption: "Sequence Number", shortCaption: "Seq. No.");
		CaptionTestHelper.AssertCaptions(SupernumeraryGoods.CSI_DescriptionInfo, caption: "Description");
		CaptionTestHelper.AssertCaptions(SupernumeraryGoods.CSI_QuantityInfo, caption: "Gross Mass");
		CaptionTestHelper.AssertCaptions(SupernumeraryGoods.CSI_TariffInfo, caption: "Tariff Code", shortCaption: "HS-Code");
		CaptionTestHelper.AssertCaptions(SupernumeraryGoods.CSI_PackQtyInfo, caption: "Number of Packages", shortCaption: "No. Packages");
		CaptionTestHelper.AssertCaptions(SupernumeraryGoods.CSI_PackTypeInfo, caption: "Type of Package");
	});

	public void TestCSI_LineNo() => CombineAssertions(() =>
	{
		var collection = SupernumeraryGoods.Parent.SupernumeraryGoods;

		AssertEquals("1st", 1, SupernumeraryGoods.CSI_LineNo);

		var supernumeraryGoods2 = collection.AddNew();
		AssertEquals("2nd", 2, supernumeraryGoods2.CSI_LineNo);

		SupernumeraryGoods.CSI_LineNo = 2;
		AssertEquals("2nd renumbered", 1, supernumeraryGoods2.CSI_LineNo);
	});

	public void TestCSI_Description()
	{
		AssertEquals("MaxLength", 512, SupernumeraryGoods.CSI_DescriptionInfo.MaxLength);
	}

	public void TestCSI_UnitOfQuantity() => CombineAssertions(() =>
	{
		AssertEquals("Default", Core.Constants.Weight.Kilograms, SupernumeraryGoods.CSI_UnitOfQuantity);
		AssertEquals("ReadOnly", true, SupernumeraryGoods.CSI_UnitOfQuantityInfo.ReadOnly);
		AssertEquals("MaxLength", 2, SupernumeraryGoods.CSI_UnitOfQuantityInfo.MaxLength);
	});

	public void TestCSI_Tariff()
	{
		AssertEquals("MaxLength", 6, SupernumeraryGoods.CSI_TariffInfo.MaxLength);
	}

	public void TestCSI_PackQty()
	{
		AssertEquals("MaxLength", 8, SupernumeraryGoods.CSI_PackQtyInfo.MaxLength);
	}

	public void TestReadOnly() => CombineAssertions(() =>
	{
		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.NctsArrivalAdditionalGoodsInformation))
		{
			AssertEquals("Unlocked", false, SupernumeraryGoods.ReadOnly);
			SupernumeraryGoods.Parent.Header.LockFile("test");
			AssertEquals("Locked", true, SupernumeraryGoods.ReadOnly);
		}
	});

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateSupernumeraryGoods(factory);

	SupernumeraryGoods SupernumeraryGoods => supernumeraryGoods ?? (supernumeraryGoods = CreateSupernumeraryGoods(Factory));
	SupernumeraryGoods supernumeraryGoods;

	static SupernumeraryGoods CreateSupernumeraryGoods(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();
	}
}
