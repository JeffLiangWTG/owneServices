using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAPI_Tariff()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var pack = bill.Packs.AddNew();
		var packedItem = pack.PackedItem;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_TariffInfo);
	}
}
