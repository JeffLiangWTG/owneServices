using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaPackedItem))]
sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
{
	[ExpectNoExceptions]
	public void TestValidation()
	{
		var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
		NUnit.Framework.Assert.That(packedItem.Validation, Is.TypeOf<AsycudaPackedItemValidation>());
	}

	[ExpectNoExceptions]
	public void TestGoodDescription()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.GulfCooperationCouncil, Universal.Constants.TariffTypes.HarmonizedSystem);
		var description = new ZString('A', ManifestBase.AutoAsycudaPack.Schema.APA_GoodsDescriptionMaxLength + 10);
		Factory.Save();
		var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.GulfCooperationCouncil, tariffType.PK, "123456789", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), description: description);
		Factory.Save();

		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		header.AMA_ManifestType = AEManifestTypes.Codes.ACI;
		var bill = header.Bills.AddNew();
		var pack = bill.Packs.AddNew();
		var packedItem = pack.PackedItem;
		packedItem.API_Tariff = "123456789";

		NUnit.Framework.Assert.That(packedItem.Pack.APA_GoodsDescription, Is.EqualTo(description.Left(ManifestBase.AutoAsycudaPack.Schema.APA_GoodsDescriptionMaxLength)), "Goods Description");
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var pack = bill.Packs.AddNew();
		return pack.PackedItem;
	}
}
