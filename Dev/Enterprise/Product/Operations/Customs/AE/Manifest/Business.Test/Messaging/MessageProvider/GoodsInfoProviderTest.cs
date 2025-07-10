using System;
using System.Linq;
using Enterprise.Customs.AE.Business.Testing;
using Enterprise.Edifact.D23A.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class GoodsInfoProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsInfoProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new GoodsInfoProvider(null), "Pack is null");
			AssertNoExceptionThrown("Valid pack", () => new GoodsInfoProvider(pack));
		});
	}

	[ExpectNoExceptions]
	public void TestGoods() => NUnit.Framework.Assert.That(GetProvider().Goods, Is.TypeOf<GoodsDetailsProvider>());

	[ExpectNoExceptions]
	public void TestGoodsDescription()
	{
		pack.APA_GoodsDescription = "XYZ";
		var description = GetProvider().GoodsDescription;
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(description.SubjectCode, Is.EqualTo(TextSubjectCodeQualifierList.GoodsItemDescription.ToString()), "Description type");
			NUnit.Framework.Assert.That(description.Text, Is.EqualTo("XYZ"), "Description text");
		});
	}

	[ExpectNoExceptions]
	public void TestMeasurements()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupUnitCodeMappings();

		var measurements = GetProvider().Measurements;
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(measurements.Count, Is.EqualTo(1), "Gross weight measurement mandatory");

			pack.APA_Weight = 1000m;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 1m;
			pack.APA_VolumeUQ = "L";

			measurements = GetProvider().Measurements;
			NUnit.Framework.Assert.That(measurements.Count, Is.EqualTo(2), "Measurements count");
			AssertMeasurement(MeasuredAttributeCodeList.GoodsItemGrossWeight, 1000m, "KGM");
			AssertMeasurement(MeasuredAttributeCodeList.Volume, 1m, "LTR");
		});

		void AssertMeasurement(string purpose, decimal expectedValue, string expectedUnit)
		{
			var measurement = measurements.Single(x => x.MeasurementPurpose == purpose);
			NUnit.Framework.Assert.That(measurement.MeasurementValue, Is.EqualTo(expectedValue), $"{purpose} value");
			NUnit.Framework.Assert.That(measurement.MeasurementUnit, Is.EqualTo(expectedUnit), $"{purpose} unit");
		}
	}

	[ExpectNoExceptions]
	public void TestGoodsContainer()
	{
		var container = pack.Bill.Header.Containers.AddNew();
		pack.ContainerPK = container.PK;
		container.ACN_ContainerNumber = "123456";
		container.ACN_NumberOfPackages = 12;

		var goodsContainer = GetProvider().GoodsContainer;
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(goodsContainer.ContainerIdentifier, Is.EqualTo("123456"), "Container number");
			NUnit.Framework.Assert.That(goodsContainer.PackageQuantity, Is.EqualTo(12), "No. of packages");
		});
	}

	[ExpectNoExceptions]
	public void TestGoodsContainerWhenContainerIsNull()
	{
		NUnit.Framework.Assert.That(GetProvider().GoodsContainer, Is.Null, "Container is null");
	}

	[ExpectNoExceptions]
	public void TestGoodsMarksDescription()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().GoodsMarksDescription, Is.Null.Or.Empty, "Description not set - should be [null] or [empty]");

			pack.APA_MarksAndNumbers = "ABC";
			NUnit.Framework.Assert.That(GetProvider().GoodsMarksDescription, Is.EqualTo("ABC"), "Description set");
		});
	}

	[ExpectNoExceptions]
	public void TestCustomsGoodsIdentifier()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().CustomsGoodsIdentifier, Is.Null.Or.Empty, "Commodity code not set - should be [null] or [empty]");

			pack.PackedItem.API_Tariff = "XYZ";
			NUnit.Framework.Assert.That(GetProvider().CustomsGoodsIdentifier, Is.EqualTo("XYZ"), "Commodity code set");
		});
	}

	[ExpectNoExceptions]
	public void TestOriginCountry()
	{
		pack.Bill.ABL_RL_NKOrigin = "AEABC";
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().OriginCountry.LocationFunctionCode, Is.EqualTo(LocationFunctionCodeQualifierList.CountryOfOrigin.ToString()), "OriginCountry type");
			NUnit.Framework.Assert.That(GetProvider().OriginCountry.LocationIdentifier, Is.EqualTo("AE"), "OriginCountry text");
		});
	}

	protected override GoodsInfoProvider GetProvider() => new GoodsInfoProvider(pack);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		pack = bill.Packs.AddNew();
	}
	AsycudaPack pack;
}
