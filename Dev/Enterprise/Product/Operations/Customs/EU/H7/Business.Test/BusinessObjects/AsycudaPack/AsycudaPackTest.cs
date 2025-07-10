using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAPA_PackQtyMaxLength()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(AsycudaPack), nameof(AsycudaPack.APA_PackQty), false, r => r.MaxLength == 8);
		}

		public void TestValidationType()
		{
			var bizObj = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaPackValidation>(bizObj.Validation);
		}

		public void TestPackWeightInKG()
		{
			var pack = Factory.New<AsycudaPack>();

			AssertEquals("Empty Data", 0m, pack.PackWeightInKG);
			pack.APA_Weight = 1000;
			pack.APA_WeightUQ = "KG";
			AssertEquals("No Convert", 1000m, pack.PackWeightInKG);

			pack.APA_WeightUQ = "G";
			AssertEquals("Convert", 1m, pack.PackWeightInKG);

			pack.APA_WeightUQ = "";
			AssertEquals("Invalid unit", 0m, pack.PackWeightInKG);
		}

		public void TestClonePack()
		{
			var pack = GetNewBusinessObjectWithParent(Factory);
			pack.LinePrice = 1;
			pack.LinePriceCurrency = "AUD";
			pack.APA_CommodityCode = "code";
			pack.APA_GoodsDescription = "description";
			pack.APA_LineNo = 1;
			pack.APA_MarksAndNumbers = "numbers";
			pack.APA_PackQty = 1;
			pack.APA_PackUQ = "KG";
			pack.APA_Volume = 1;
			pack.APA_VolumeUQ = "L";
			pack.APA_Weight = 10;
			pack.APA_WeightUQ = "KG";
			Factory.Save();

			var clonedPack = (AsycudaPack)pack.Clone();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(1m, clonedPack.LinePrice);
				AssertEquals("AUD", clonedPack.LinePriceCurrency);
				AssertEquals("code", clonedPack.APA_CommodityCode);
				AssertEquals("description", clonedPack.APA_GoodsDescription);
				AssertEquals(1, (int)clonedPack.APA_LineNo);
				AssertEquals("numbers", clonedPack.APA_MarksAndNumbers);
				AssertEquals(1, clonedPack.APA_PackQty);
				AssertEquals("KG", clonedPack.APA_PackUQ);
				AssertEquals(1m, clonedPack.APA_Volume);
				AssertEquals("L", clonedPack.APA_VolumeUQ);
				AssertEquals(10m, clonedPack.APA_Weight);
				AssertEquals("KG", clonedPack.APA_WeightUQ);
			});
		}

		public void TestCaptions()
		{
			var headers = GetNewBusinessObject() as AsycudaPack;
			CombineAssertions(() =>
			{
				AssertDataBoundCaptions(headers.APA_PackQtyInfo, "Quantity (on Pack)", "Quantity", "Qty.", "Number of packs.");
				AssertDataBoundCaptions(headers.APA_PackUQInfo, "Pack Unit", "Pack UQ", "UQ", "Measurement unit of the pack quantity.");
				AssertDataBoundCaptions(headers.APA_CommodityCodeInfo, "Commodity Code", "Commodity", "Comm.", "");
				AssertDataBoundCaptions(headers.APA_GoodsDescriptionInfo, "Goods Description (on Pack)", "Description", "Desc.", "Description of goods, as stipulated on the pack.");
				AssertDataBoundCaptions(headers.APA_MarksAndNumbersInfo, "Marks and Numbers (on Pack)", "Marks & Nums.", "Marks", "Free form description of the marks and numbers stipulated on the pack.");
				AssertDataBoundCaptions(headers.APA_WeightInfo, "Weight (on Pack)", "Weight", "Wt.", "Weight, as stipulated on the pack.");
				AssertDataBoundCaptions(headers.APA_WeightUQInfo, "Weight Unit", "Wt. UQ", "UQ", "Measurement unit of the pack weight.");
				AssertDataBoundCaptions(headers.APA_VolumeInfo, "Volume (on Pack)", "Volume", "Vol.", "Pack volume.");
				AssertDataBoundCaptions(headers.APA_VolumeUQInfo, "Volume Unit", "Volume UQ", "Vol. UQ", "Measurement unit of the pack volume.");
				AssertDataBoundCaptions(headers.APA_VINNumberInfo, "VIN Number", "VIN No.", "VIN No.", "Vehicle Identification Number.");
				AssertDataBoundCaptions(headers.LinePriceInfo, "Line Price", "Line Price", "Price", "Total customs value of the pack.");
				AssertDataBoundCaptions(headers.LinePriceCurrencyInfo, "Line Price Currency", "Currency", "Curr.", "Currency code associated with the Line Price.");
				AssertDataBoundCaptions(headers.APA_LineNoInfo, "Sequence Number", "Seq No.", "Seq No.", "Pack sequence number.");
			});
		}

		public void TestFetchStrategyType()
		{
			var bizObj = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaPackFetchStrategy>(bizObj.FetchStrategy);
		}

		void AssertDataBoundCaptions(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption, string expectedFullDescription)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			AssertEquals($"{propertyInfo.Name} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} MediumCaption", expectedMediumCaption, resourceStringData.MediumCaption);
			AssertEquals($"{propertyInfo.Name} ShortCaption", expectedShortCaption, resourceStringData.ShortCaption);
			AssertEquals($"{propertyInfo.Name} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectWithParent(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObjectWithParent(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectWithParent(Factory);
		}

		AsycudaPack GetNewBusinessObjectWithParent(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs.AddNew();
		}
	}
}
