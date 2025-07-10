using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class AsycudaPackValidationTest : BusinessObjectValidationTestCase
{
	public void TestPackQty() => CombineAssertions(() =>
	{
		var pack = Bill.Packs.AddNew();
		pack.Validation.ValidateAll();
		AssertHasMessageErrorContaining(pack.APA_PackQtyInfo, ZeroMessage);

		pack.APA_PackQty = 20;
		AssertNoMessageErrorContaining(pack.APA_PackQtyInfo, ZeroMessage);
	});

	public void TestPackUQ() => CombineAssertions(() =>
	{
		var pack = Bill.Packs.AddNew();
		pack.Validation.ValidateAll();
		AssertHasMessageErrorContaining(pack.APA_PackUQInfo, NotEnteredMessage);

		pack.APA_PackUQ = "123";
		AssertNoMessageErrorContaining(pack.APA_PackUQInfo, NotEnteredMessage);
	});

	public void TestMarksAndNumbers() => CombineAssertions(() =>
	{
		var pack = Bill.Packs.AddNew();
		pack.Validation.ValidateAll();
		AssertHasMessageErrorContaining(pack.APA_MarksAndNumbersInfo, NotEnteredMessage);

		pack.APA_MarksAndNumbers = "123";
		AssertNoMessageErrorContaining(pack.APA_MarksAndNumbersInfo, NotEnteredMessage);
	});

	public void TestGoodsDescription() => CombineAssertions(() =>
	{
		var pack = Bill.Packs.AddNew();
		pack.Validation.ValidateAll();
		AssertHasMessageErrorContaining(pack.APA_GoodsDescriptionInfo, NotEnteredMessage);
		pack.APA_GoodsDescription = "123";
		AssertNoMessageErrorContaining(pack.APA_GoodsDescriptionInfo, NotEnteredMessage);
	});

	public void TestCheckAPA_WeightUQ() => CombineAssertions(() =>
	{
		AssertCheckUQ(Bill.Packs.AddNew().APA_WeightUQInfo, "KG");
	});

	public void TestCheckAPA_VolumeUQ() => CombineAssertions(() =>
	{
		AssertCheckUQ(Bill.Packs.AddNew().APA_VolumeUQInfo, "L");
	});

	void AssertCheckUQ(ZPropertyInfo targetInfo, ZString validUQ)
	{
		const string errorMessage = "Unable to convert UOM to a corresponding UN Code List 20 Code.";
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupUnitCodeMappings();

		targetInfo.Value = ZString.Empty;
		AssertNoMessageError("UQ is empty", targetInfo, errorMessage);

		targetInfo.Value = (ZString)"XX";
		AssertHasMessageError("UQ without mapping", targetInfo, errorMessage);

		targetInfo.Value = validUQ;
		AssertNoMessageError("UQ with mapping", targetInfo, errorMessage);
	}

	public void TestCheckAPA_PackUQ() => CombineAssertions(() =>
	{
		const string errorMessage = "does not map";
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("PKG", "PKG");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedArabEmirates, "PKG", "43", "Bag, super bulk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedArabEmirates);
		Factory.Save();
		var refPack = Factory.New<CusRefPacks>();
		refPack.RP_CustomsCountry = "AE";
		refPack.RP_CustomsPack = "43";
		refPack.RP_CommercialPack = "BAG";
		refPack.RP_Type = "";

		var pack = Bill.Packs.AddNew();
		pack.APA_PackUQ = "";
		AssertNoMessageErrorContaining("UQ without mapping", pack.APA_PackUQInfo, errorMessage);

		pack.APA_PackUQ = "BOX";
		AssertHasMessageErrorContaining("UQ without mapping", pack.APA_PackUQInfo, errorMessage);

		pack.APA_PackUQ = "BAG";
		AssertNoMessageErrorContaining("UQ without mapping", pack.APA_PackUQInfo, errorMessage);
	});

	AsycudaManifestHeader Header => header ??= Factory.New<AsycudaManifestHeader>();
	AsycudaManifestHeader header;

	AsycudaBill Bill => bill ??= Header.Bills.AddNew();
	AsycudaBill bill;

	const string ZeroMessage = "cannot be zero";
	const string NotEnteredMessage = "not entered";
}
