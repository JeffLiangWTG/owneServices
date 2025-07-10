using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(PackageTypePairCollectionForTest))]
	sealed class PackageTypePairCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PackageTypePairCollectionForTest>
	{
		public void TestGetMappedPackageType()
		{
			PackageTypePairCollectionForTest collection = new PackageTypePairCollectionForTest();
			collection.AddNew(CustomsPackType_1_ForTest, FreightPackType_1_ForTest);
			collection.AddNew(CustomsPackType_2_ForTest, FreightPackType_2_ForTest);
			AssertEquals(CustomsPackType_1_ForTest, collection.GetMappedPackageType(FreightPackType_1_ForTest));
			AssertEquals(CustomsPackType_2_ForTest, collection.GetMappedPackageType(FreightPackType_2_ForTest));
			AssertEquals("GetMappedPackageType should returns the same value if mapping not exist for this FreightPackType", FreightPackType_3_ForTest, collection.GetMappedPackageType(FreightPackType_3_ForTest));
		}

		public void TestGetMappedFreightPackageType()
		{
			var collection = new PackageTypePairCollectionForTest();
			collection.AddNew("X3", "XXX");

			AssertEquals("XXX", collection.GetMappedFreightPackageType("X3"));
			AssertEquals("XYZ", collection.GetMappedFreightPackageType("XYZ"));
			AssertEquals(string.Empty, collection.GetMappedFreightPackageType(null));
		}

		public void TestGetMappedFreightPackagePair()
		{
			var collection = new PackageTypePairCollectionForTest();
			collection.AddNew("X3", "XXX");

			var pair = collection.GetMappedFreightPackagePair("X3");
			CombineAssertions(() =>
			{
				AssertEquals("X3 exists", "XXX", pair.FreightPackageType);
				AssertNull("XYZ doesn't exist", collection.GetMappedFreightPackagePair("XYZ"));
				AssertNull("null", collection.GetMappedFreightPackagePair(null));
			});
		}

		public void TestAddNewWithParameters()
		{
			PackageTypePairCollectionForTest collection = new PackageTypePairCollectionForTest();
			PackageTypePairForTest packageType = collection.AddNew(CustomsPackType_1_ForTest, FreightPackType_1_ForTest);
			AssertEquals("FreightPackageType", FreightPackType_1_ForTest, packageType.FreightPackageType);
			AssertEquals("CustomsPackageType", CustomsPackType_1_ForTest, packageType.CustomsPackageType);
		}
		const string CustomsPackType_1_ForTest = "ABC";
		const string CustomsPackType_2_ForTest = "A2";
		const string FreightPackType_1_ForTest = "HIJ";
		const string FreightPackType_2_ForTest = "H2";
		const string FreightPackType_3_ForTest = "H3";

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override PackageTypePairCollectionForTest GetCollectionToTest()
		{
			return new PackageTypePairCollectionForTest();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PackageTypePairForTest();
		}
	}
}
