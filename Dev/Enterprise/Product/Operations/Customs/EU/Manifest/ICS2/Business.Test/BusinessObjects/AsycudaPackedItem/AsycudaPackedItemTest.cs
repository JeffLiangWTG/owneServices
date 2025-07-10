using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAPI_ChemicalSubstanceCodeCaption()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			var captionData = DataBoundResourceStrings.GetDataForProperty(packedItem.API_ChemicalSubstanceCodeInfo);

			AssertEquals("Caption", "CUS Code", captionData.Caption);
		}

		public void TestAPI_GoodsValue()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(AsycudaPackedItem), nameof(AsycudaPackedItem.API_GoodsValue), includesInherit: false, x => x.Caption == "Postal Value");
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(AsycudaPackedItem), nameof(AsycudaPackedItem.API_GoodsValue), includesInherit: false, x => x.DecimalPlaces == 2);
		}

		public void TestAPI_RX_NKGoodsValueCurrency()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(AsycudaPackedItem), nameof(AsycudaPackedItem.API_RX_NKGoodsValueCurrency), includesInherit: false, x => x.Caption == "Postal Value Currency" && x.MediumCaption == "Currency" && x.ShortCaption == "Curr.");
		}

		public void TestAPI_TypeOfGoods()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(AsycudaPackedItem), nameof(AsycudaPackedItem.API_TypeOfGoods), includesInherit: false, x => x.Caption == "Type of Goods");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaPackedItem), nameof(AsycudaPackedItem.API_TypeOfGoods), includesInherit: false, x => x.ListDataSourceMember == "Lookups.TypeOfGoodsList");
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			return packedItem;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
