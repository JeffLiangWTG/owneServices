using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.US.Testing
{
	[TestedType(typeof(USPackageTypePairCollection))]
	sealed class USPackageTypePairCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<USPackageTypePairCollection>
	{
		public void TestAddDefaultValues()
		{
			USPackageTypePairCollection collection = new USPackageTypePairCollection();
			AssertEquals(0, collection.Count);
			collection.AddDefaultValues();
			AssertEquals(22, collection.Count);
			AssertEquals("1st record.CustomsPackageType", PackType.Customs.BG, collection[0].CustomsPackageType);
			AssertEquals("1st record.FreightPackageType", PackType.Freight.BAG, collection[0].FreightPackageType);
			AssertEquals("11th record.CustomsPackageType", PackType.Customs.CT, collection[10].CustomsPackageType);
			AssertEquals("11th record.FreightPackageType", PackType.Freight.CTN, collection[10].FreightPackageType);
			AssertEquals("last record.CustomsPackageType", PackType.Customs.TU, collection[21].CustomsPackageType);
			AssertEquals("last record.FreightPackageType", PackType.Freight.TUB, collection[21].FreightPackageType);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override USPackageTypePairCollection GetCollectionToTest()
		{
			return new USPackageTypePairCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new USPackageTypePair();
		}
	}
}
