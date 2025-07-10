using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	public class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAsycudaLinkPackages()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertType<AsycudaLinkPackageCollection<AsycudaLinkPackage>>(packedItem.AsycudaLinkPackages);
		}

		public void TestPackagesPivot()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertType<AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>>(packedItem.PackagesPivot);
		}

		public void TestToggleLinkageWithPackage()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;

			AssertEquals("0 pivot should have been created.", 0, packedItem.PackagesPivot.Count);

			var asycudaPack = Factory.New<AsycudaPack>();
			var asycudaPack2 = Factory.New<AsycudaPack>();

			var pivot = packedItem.ToggleLinkageWithPackage(asycudaPack, true);
			var pivot2 = packedItem.ToggleLinkageWithPackage(asycudaPack2, true);

			CombineAssertions("pivots should have been created.", () =>
			{
				AssertEquals(2, packedItem.PackagesPivot.Count);
				AssertEquals("APP_APA_Pack should equals pivot asycudaPack.PK", asycudaPack.PK, pivot.APP_APA_Pack);
				AssertEquals("APP_APA_Pack should equals pivot asycudaPack2.PK", asycudaPack2.PK, pivot2.APP_APA_Pack);
			});

			pivot = packedItem.ToggleLinkageWithPackage(asycudaPack, false);
			CombineAssertions("A pivot should have been deleted.", () =>
			{
				packedItem.PackagesPivot.Reload(true);
				AssertEquals(1, packedItem.PackagesPivot.Count);
				AssertNull("APP_APA_Pack should equals pivot asycudaPack.PK", pivot);
				AssertEquals("APP_APA_Pack should equals pivot asycudaPack2.PK", asycudaPack2.PK, pivot2.APP_APA_Pack);
				AssertEquals("only pivot2 should have been in the PackagesPivot.", asycudaPack2.PK, packedItem.PackagesPivot.Cast<AsycudaPackPackedItemPivot>().FirstOrDefault().APP_APA_Pack);
			});
		}

		public void TestAdditionalInfos()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertType<AsycudaAdditionalInfoCollection>(packedItem.AdditionalInfos);
		}

		public void TestValidation()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertNotNull(packedItem.Validation);
			AssertType<AsycudaPackedItemValidation>(packedItem.Validation);
		}

		public void TestLookups()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertNotNull(packedItem.Lookups);
			AssertType<AsycudaPackedItemLookups>(packedItem.Lookups);
		}

		public void TestSetDefaultValues()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertEquals("Default Gross Weight Unit should be KG", "KG", packedItem.API_GrossWeightUQ);
		}

		public void TestCaptions()
		{
			AssertCaptions("API_LineNo", "Sequence Number");
			AssertCaptions("API_GoodsDescription", "Goods Description");
			AssertCaptions("API_GrossWeight", "Gross Weight");
			AssertCaptions("API_GrossWeightUQ", "Gross Weight UQ");
			AssertCaptions("API_PackStatus", "Cargo Status");
		}

		public void TestAPI_GoodsDescriptionMaxLength()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertEquals("API_GoodsDescriptionMaxLength", 256, packedItem.API_GoodsDescriptionInfo.MaxLength);
		}

		public void TestAPI_LineNo()
		{
			var packedItem1 = bill.PackedItems.AddNew();
			var packedItem2 = bill.PackedItems.AddNew();
			var packedItem3 = bill.PackedItems.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Line number 1", (ZShort)1, packedItem1.API_LineNo);
				AssertEquals("Line number 2", (ZShort)2, packedItem2.API_LineNo);
				AssertEquals("Line number 3", (ZShort)3, packedItem3.API_LineNo);

				bill.PackedItems.RemoveAndDelete(packedItem2);
				AssertEquals("Line number 1 stay same", (ZShort)1, packedItem1.API_LineNo);
				AssertEquals("Line number 3 change to 2", (ZShort)2, packedItem3.API_LineNo);
			});
		}

		public void TestUniversalTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Israel, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Israel, tariffType.PK, "123456789", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_Tariff = "123456789";
			AssertNotNull("UniversalTariff", packedItem.UniversalTariff);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return bill.PackedItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			bill = header.Bills.AddNew();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		void AssertCaptions(string propertyName, string caption)
		{
			AssertEquals($"{propertyName} Caption", caption, DataBoundResourceStrings.GetDataForProperty(typeof(AsycudaPackedItem), propertyName).Caption);
		}

		IDisposable disposableAction;
		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
