using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	public class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
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

			pack.APA_PackQty = 1;
			pack.APA_PackUQ = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

			pack.APA_PackUQ = "YWX";
			AssertHasMessageErrorContaining(pack.APA_PackUQInfo, ListValidation.InvalidCodeMessageError);

			pack.APA_PackUQ = "BAG";
			pack.APA_VolumeUQ = Core.Constants.PkgUnit.Bag;
			AssertNoNotifications(pack.APA_PackUQInfo);

			pack.APA_PackQty = 0;
			pack.APA_PackUQ = ZString.Empty;
			AssertNoNotifications(pack.APA_PackUQInfo);
		}

		public void TestCheckAPA_Weight()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_Weight = 0;
			AssertHasMessageErrorContaining(pack.APA_WeightInfo, MandatoryValidation.ValueCannotBeZero);

			pack.APA_Weight = 1;
			AssertNoNotifications(pack.APA_WeightInfo);
		}

		public void TestCheckAPA_Volume()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_Volume = 0;
			AssertHasMessageErrorContaining(pack.APA_VolumeInfo, MandatoryValidation.ValueCannotBeZero);

			pack.APA_Volume = 1;
			AssertNoNotifications(pack.APA_VolumeInfo);
		}

		public void TestVolumeUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_Volume = 1;
			pack.APA_VolumeUQ = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);

			pack.APA_VolumeUQ = "XQ";
			AssertHasMessageErrorContaining(pack.APA_VolumeUQInfo, ListValidation.InvalidCodeMessageError);

			pack.APA_VolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertNoNotifications(pack.APA_VolumeUQInfo);

			pack.APA_Volume = 0;
			pack.APA_VolumeUQ = ZString.Empty;
			AssertNoNotifications(pack.APA_VolumeUQInfo);
		}

		public void TestCheckAPA_CommodityCode()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_CommodityCode = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_CommodityCodeInfo, MandatoryValidation.YouHaveNotEntered);

			pack.APA_CommodityCode = "TEST";
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
	}
}
