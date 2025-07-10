using System;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class GoodsDetailsProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsDetailsProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new GoodsDetailsProvider(null), "Pack is null");
			AssertNoExceptionThrown("Valid pack", () => new GoodsDetailsProvider(pack));
		});
	}

	[ExpectNoExceptions]
	public void TestPackageQuantity()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().PackageQuantity, Is.EqualTo(0), "Quantity not set");

			pack.APA_PackQty = 123;
			NUnit.Framework.Assert.That(GetProvider().PackageQuantity, Is.EqualTo(123), "Quantity is set");
		});
	}

	[ExpectNoExceptions]
	public void TestPackageType()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("PKG", "PKG");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedArabEmirates, "PKG", "T", "Bag, super bulk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedArabEmirates);
		Factory.Save();

		CombineAssertions(() =>
		{
			pack.APA_PackUQ = "";
			NUnit.Framework.Assert.That(GetProvider().PackageType, Is.Null.Or.Empty, "Package type not set - should be [null] or [empty]");

			pack.APA_PackUQ = "XYZ";
			NUnit.Framework.Assert.That(GetProvider().PackageType, Is.Null.Or.Empty, "Package type has no description - should be [null] or [empty]");

			pack.APA_PackUQ = "T";
			NUnit.Framework.Assert.That(GetProvider().PackageType, Is.EqualTo("Bag, super bulk"), "Package type description");
		});
	}

	[ExpectNoExceptions]
	public void TestPackageTypeCode()
	{
		CombineAssertions(() =>
		{
			pack.APA_PackUQ = "";
			NUnit.Framework.Assert.That(GetProvider().PackageTypeCode, Is.Null.Or.Empty, "Package type not set - should be [null] or [empty]");

			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			NUnit.Framework.Assert.That(GetProvider().PackageTypeCode, Is.EqualTo(Core.Constants.PkgUnit.Bag), "Package type set");
		});
	}

	[ExpectNoExceptions]
	public void TestPackageLineNo()
	{
		NUnit.Framework.Assert.That(GetProvider().PackageLineNo, Is.EqualTo(1), "The default value of Package LineNo is 1 due to Short Sequence Number Generator");
	}

	protected override GoodsDetailsProvider GetProvider() => new GoodsDetailsProvider(pack);

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		pack = bill.Packs.AddNew();
	}
	AsycudaPack pack;
}
