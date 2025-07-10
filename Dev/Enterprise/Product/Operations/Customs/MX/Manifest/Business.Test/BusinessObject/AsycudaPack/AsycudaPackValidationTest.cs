using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPA_CommodityCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.APA_CommodityCode = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_CommodityCodeInfo, MandatoryValidation.YouHaveNotEntered);

			bill.Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			pack.APA_CommodityCode = "TEST";
			AssertNoNotifications(pack.APA_CommodityCodeInfo);

			bill.Header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			pack.APA_CommodityCode = "TEST";
			AssertHasMessageErrorContaining(pack.APA_CommodityCodeInfo, "The length of the Commodity Code must be 6.");

			pack.APA_CommodityCode = "092345";
			AssertNoNotifications(pack.APA_CommodityCodeInfo);
		}

		public void TestCheckAPA_GoodsDescription()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_GoodsDescription = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			pack.APA_GoodsDescription = "TEST";
			AssertNoNotifications(pack.APA_GoodsDescriptionInfo);
		}

		public void TestCheckAPA_MarksAndNumbers()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_MarksAndNumbers = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			pack.APA_MarksAndNumbers = "TEST";
			AssertNoNotifications(pack.APA_MarksAndNumbersInfo);
		}

		public void TestCheckAPA_PackQty()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_PackQty = 0;
			AssertHasMessageErrorContaining(pack.APA_PackQtyInfo, MandatoryValidation.ValueCannotBeZero);
			pack.APA_PackQty = 1;
			AssertNoNotifications(pack.APA_PackQtyInfo);
		}

		public void TestCheckAPA_PackUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_PackUQ = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);
			pack.APA_PackUQ = "BAG";
			AssertNoNotifications(pack.APA_PackUQInfo);
		}

		public void TestCheckAPA_WeightUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_Weight = 1;
			pack.APA_WeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			pack.APA_WeightUQ = "DT";
			AssertNoNotifications(pack.APA_WeightUQInfo);

			pack.APA_Weight = 0;
			pack.APA_WeightUQ = ZString.Empty;
			AssertNoNotifications(pack.APA_WeightUQInfo);
			pack.APA_WeightUQ = "DT";
			AssertNoNotifications(pack.APA_WeightUQInfo);
		}
	}
}
