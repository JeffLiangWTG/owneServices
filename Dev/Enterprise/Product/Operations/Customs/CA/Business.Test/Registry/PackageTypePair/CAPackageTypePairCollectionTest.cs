using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(CAPackageTypePairCollection))]
	sealed class CAPackageTypePairCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CAPackageTypePairCollection>
	{
		public void TestAddDefaultValues()
		{
			var collection = new CAPackageTypePairCollection();
			AssertEquals(0, collection.Count);
			collection.AddDefaultValues();
			AssertEquals(28, collection.Count);
			AssertEquals("1st record.CustomsPackageType", CAPackageTypePairCollection.PackType.Customs.BG, collection[0].CustomsPackageType);
			AssertEquals("1st record.FreightPackageType", CAPackageTypePairCollection.PackType.Freight.BAG, collection[0].FreightPackageType);
			AssertEquals("11th record.CustomsPackageType", CAPackageTypePairCollection.PackType.Customs.CT, collection[10].CustomsPackageType);
			AssertEquals("11th record.FreightPackageType", CAPackageTypePairCollection.PackType.Freight.CTN, collection[10].FreightPackageType);
			AssertEquals("last record.CustomsPackageType", CAPackageTypePairCollection.PackType.Customs.EA, collection[27].CustomsPackageType);
			AssertEquals("last record.FreightPackageType", CAPackageTypePairCollection.PackType.Freight.UNT, collection[27].FreightPackageType);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CAPackageTypePairCollection GetCollectionToTest()
		{
			return new CAPackageTypePairCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CAPackageTypePair();
		}
	}
}
