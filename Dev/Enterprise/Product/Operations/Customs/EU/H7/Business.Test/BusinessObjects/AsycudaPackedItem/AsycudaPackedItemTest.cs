using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCaptionResourceString()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("Gross Weight", packedItem.API_GrossWeightInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
				AssertEquals("Gross Wgt.", packedItem.API_GrossWeightInfo.GetAttribute<ResourceStringDataAttribute>().MediumCaption);
				AssertEquals("Gr. Wgt.", packedItem.API_GrossWeightInfo.GetAttribute<ResourceStringDataAttribute>().ShortCaption);
				AssertEquals("Gross Weight of the Item.", packedItem.API_GrossWeightInfo.GetAttribute<ResourceStringDataAttribute>().FullDescription);

				AssertEquals("Gross Weight Unit", packedItem.API_GrossWeightUQInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
				AssertEquals("Wgt. UQ", packedItem.API_GrossWeightUQInfo.GetAttribute<ResourceStringDataAttribute>().MediumCaption);
				AssertEquals("UQ", packedItem.API_GrossWeightUQInfo.GetAttribute<ResourceStringDataAttribute>().ShortCaption);
				AssertEquals("Measurement unit of the Gross Weight of the Item.", packedItem.API_GrossWeightUQInfo.GetAttribute<ResourceStringDataAttribute>().FullDescription);

				AssertEquals("Net Weight", packedItem.API_NetWeightInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
				AssertEquals("Net Wgt.", packedItem.API_NetWeightInfo.GetAttribute<ResourceStringDataAttribute>().MediumCaption);
				AssertEquals("Net Wgt.", packedItem.API_NetWeightInfo.GetAttribute<ResourceStringDataAttribute>().ShortCaption);
				AssertEquals("Net Weight of the Item.", packedItem.API_NetWeightInfo.GetAttribute<ResourceStringDataAttribute>().FullDescription);

				AssertEquals("Net Weight Unit", packedItem.API_NetWeightUQInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
				AssertEquals("Wgt. UQ", packedItem.API_NetWeightUQInfo.GetAttribute<ResourceStringDataAttribute>().MediumCaption);
				AssertEquals("UQ", packedItem.API_NetWeightUQInfo.GetAttribute<ResourceStringDataAttribute>().ShortCaption);
				AssertEquals("Measurement unit of the Net Weight of the Item.", packedItem.API_NetWeightUQInfo.GetAttribute<ResourceStringDataAttribute>().FullDescription);
			});
		}

		public void TestAsycudaPackPackedItemLink()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();

			AssertType<AsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink>>(packedItem.AsycudaPackPackedItemLinks);
		}

		public void TestPackagesPivot()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();

			AssertType<AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>>(packedItem.PackagesPivot);
		}

		public void TestToggleLinkageWithPackage()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();

			AssertEquals("0 pivot should have been created.", 0, packedItem.PackagesPivot.Count);

			var pack1 = Factory.New<AsycudaPack>();
			var pack2 = Factory.New<AsycudaPack>();

			var pivot = packedItem.ToggleLinkageWithPackage(pack1, true);
			var pivot2 = packedItem.ToggleLinkageWithPackage(pack2, true);

			CombineAssertions("pivots should have been created.", () =>
			{
				AssertEquals(2, packedItem.PackagesPivot.Count);
				AssertEquals("APP_APA_Pack should equals pivot pack1.PK", pack1.PK, pivot.APP_APA_Pack);
				AssertEquals("APP_APA_Pack should equals pivot pack2.PK", pack2.PK, pivot2.APP_APA_Pack);
			});

			pivot = packedItem.ToggleLinkageWithPackage(pack1, false);
			CombineAssertions("A pivot should have been deleted.", () =>
			{
				packedItem.PackagesPivot.Reload(true);
				AssertEquals(1, packedItem.PackagesPivot.Count);
				AssertNull("pivot should be deleted", pivot);
				AssertEquals("APP_APA_Pack should equals pivot pack2.PK", pack2.PK, pivot2.APP_APA_Pack);
				AssertEquals("only pivot2 should have been in the PackagesPivot", pack2.PK, packedItem.PackagesPivot.Cast<AsycudaPackPackedItemPivot>().FirstOrDefault().APP_APA_Pack);
			});
		}

		public void TestIDataGroupingProviderMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.FrenchGuyana))
			{
				var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
				IDataGroupingProvider provider = packedItem;
				AssertEquals("packedItem.DataGrouping", Constants.CountryCodes.France, packedItem.DataGrouping);
				AssertEquals("provider.DataGrouping", Constants.CountryCodes.France, provider.DataGrouping);
			}
		}

		public void TestSetDefaultValues()
		{
			var packedItem = Factory.New<AsycudaPackedItem>();
			AssertEquals("KG", packedItem.API_GrossWeightUQ);
			AssertEquals("KG", packedItem.API_NetWeightUQ);
		}

		public void TestCaptions()
		{
			var packedItem = Factory.New<AsycudaPackedItem>();
			CombineAssertions("Test all captions to be correct", () =>
				{
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(packedItem.API_FormattedTariffInfo,
						multipleResourceKeys: null, "Tariff", "Tariff", "Tariff", "Integrated Tariff of the European Communities (TARIC) code associated with the item.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(packedItem.API_GoodsValueInfo,
						multipleResourceKeys: null, "Intrinsic Value", "Int. Val.", "Int. Val.", "Intrinsic value of the goods.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(packedItem.API_RX_NKGoodsValueCurrencyInfo,
						multipleResourceKeys: null, "Intrinsic Value Currency", "Curr.", "Currency", "Currency code associated with the Intrinsic Value.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(packedItem.API_CustomsQty2Info,
						multipleResourceKeys: null, "Supplementary Units", "Suppl.", "Suppl. Units", "The quantity of the item in question, expressed in the unit laid down in European Union Legislation, as published in TARIC. To be provided if the declaration concerns goods referred to in Article 27 of Regulation (EC) No 1186/2009.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(packedItem.API_CustomsUQ2Info,
						multipleResourceKeys: null, "Supplementary UQ", "UQ", "Suppl. UQ.", "The measurement unit associated with the Supplementary Unit quantity.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(packedItem.API_GoodsDescriptionInfo,
						multipleResourceKeys: null, "Goods Description", "Desc.", "Description", "Free-form description of the goods.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(packedItem.API_MessageStatusInfo,
						multipleResourceKeys: null, "Message Status", "Msg. Status", "Msg. Status", "Code identifying the status of the last customs declaration message sent to the relevant Customs authority.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(packedItem.API_PackStatusInfo,
						multipleResourceKeys: null, "Customs Status", "Cus. Status", "Cus. Status", "Code identifying the customs status of the declaration.");
				}
			);
		}

		public void TestClonePackedItem()
		{
			var item = GetNewBusinessObjectWithParent(Factory);
			item.API_Tariff = "1234567";
			item.API_GoodsValue = 1;
			item.API_RX_NKGoodsValueCurrency = "AUD";
			item.API_CustomsUQ2 = "KG";
			item.API_CustomsQty2 = 10;
			item.API_GrossWeight = 11;
			item.API_GrossWeightUQ = "KG";
			item.API_RN_NKGoodsOrigin = "CN";
			item.API_GoodsDescription = "description";
			item.API_MessageStatus = "ACC";
			item.API_PackStatus = "ACC";
			var additionalDoc = item.AdditionalDocuments.AddNew();
			additionalDoc.CSI_Code = "A";
			var supportingDoc = item.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "S";
			var previousDoc = item.PreviousDocuments.AddNew();
			previousDoc.CSI_Code = "P";
			Factory.Save();

			var clonedItem = (AsycudaPackedItem)item.Clone();

			CombineAssertions(() =>
			{
				AssertEquals("1234567", clonedItem.API_Tariff);
				AssertEquals(1m, clonedItem.API_GoodsValue);
				AssertEquals("AUD", clonedItem.API_RX_NKGoodsValueCurrency);
				AssertEquals("KG", clonedItem.API_CustomsUQ2);
				AssertEquals(10m, clonedItem.API_CustomsQty2);
				AssertEquals(11m, clonedItem.API_GrossWeight);
				AssertEquals("KG", clonedItem.API_GrossWeightUQ);
				AssertEquals("CN", clonedItem.API_RN_NKGoodsOrigin);
				AssertEquals("description", clonedItem.API_GoodsDescription);
				AssertEquals(string.Empty, clonedItem.API_MessageStatus);
				AssertEquals(string.Empty, clonedItem.API_PackStatus);
				AssertEquals("A", clonedItem.AdditionalDocuments.Single().CSI_Code);
				AssertEquals("S", clonedItem.SupportingDocuments.Single().CSI_Code);
				AssertEquals("P", clonedItem.PreviousDocuments.Single().CSI_Code);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.PackedItems.AddNew(); 
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObjectWithParent(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectWithParent(Factory);
		}

		AsycudaPackedItem GetNewBusinessObjectWithParent(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.PackedItems.AddNew();
		}

		public void TestAPI_CustomsValue()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			var customsValue = DataBoundResourceStrings.GetDataForProperty(packedItem.API_CustomsValueInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Customs Value", customsValue.Caption);
				AssertEquals("Customs Val.", customsValue.MediumCaption);
				AssertEquals("Cus. Val.", customsValue.ShortCaption);
				AssertEquals("Customs Value of the goods.", customsValue.FullDescription);
			});
		}

		public void TestAPI_CustomsQty()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			var customsQty = DataBoundResourceStrings.GetDataForProperty(packedItem.API_CustomsQtyInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Customs Quantity", customsQty.Caption);
				AssertEquals("Customs Qty.", customsQty.MediumCaption);
				AssertEquals("Cus. Qty.", customsQty.ShortCaption);
				AssertEquals("Quantity as indicated on the commercial invoice.", customsQty.FullDescription);
			});
		}

		public void TestAPI_CustomsUQ()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			var customsUQ = DataBoundResourceStrings.GetDataForProperty(packedItem.API_CustomsUQInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Customs Qty. UQ", customsUQ.Caption);
				AssertEquals("Qty. UQ", customsUQ.MediumCaption);
				AssertEquals("Q. UQ", customsUQ.ShortCaption);
				AssertEquals("Customs Quantity measurement unit.", customsUQ.FullDescription);
			});
		}

		public void TestDefaultUQ()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals(Weight.Kilograms, packedItem.API_GrossWeightUQ);
				AssertEquals(Weight.Kilograms, packedItem.API_NetWeightUQ);
			});
		}

		public void TestDefaultCurrency()
		{
			AssertDefaultCurrency(Constants.CountryCodes.UnitedStates, "USD");
			AssertDefaultCurrency(Constants.CountryCodes.Australia, "AUD");
		}

		void AssertDefaultCurrency(string country, string currency)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
				AssertEquals(currency, packedItem.API_RX_NKGoodsValueCurrency);
			}
		}
	}
}
