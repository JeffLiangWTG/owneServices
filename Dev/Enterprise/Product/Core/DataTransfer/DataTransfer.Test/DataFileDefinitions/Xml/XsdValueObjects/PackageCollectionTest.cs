using System.Collections;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.PackageCollection))]
	sealed class PackageCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestGetTotalWeightInKG()
		{
			Xsd.PackageCollection collection = new Xsd.PackageCollection();
			Xsd.Package package1 = collection.AddNew();
			package1.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(3.4), Constants.Weight.Pounds); // 1.54221406 in kilos
			Xsd.Package package2 = collection.AddNew();
			package2.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(12.342), Constants.Weight.Kilograms);

			AssertEquals("Total added and rounded to 4 decimal places", 13.8842, (double)collection.GetTotalWeightInKG(4));
			AssertEquals("Total added and rounded to 1 decimal place", 13.9, (double)collection.GetTotalWeightInKG(1));

			Xsd.Package package3 = collection.AddNew();
			package3.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(0.8), Constants.Weight.Kilograms);

			AssertEquals("Total changed now that new element added - rounded to 4 dec pl", 14.6842, (double)collection.GetTotalWeightInKG(4));
			AssertEquals("Total changed now that new element added - rounded to 2 dec pl", 14.68, (double)collection.GetTotalWeightInKG(2));

			Xsd.Package package4 = collection.AddNew();
			AssertEquals("Total should not change with the addition of an empty package", 14.6842, (double)collection.GetTotalWeightInKG(4));
			AssertEquals("Total should not change with the addition of an empty package", 14.68, (double)collection.GetTotalWeightInKG(2));
		}

		public void TestGetTotalVolumeInM3()
		{
			Xsd.PackageCollection collection = new Xsd.PackageCollection();
			Xsd.Package package1 = collection.AddNew();
			package1.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(3.4), Constants.Volume.CubicMetres);
			Xsd.Package package2 = collection.AddNew();
			package2.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(12.342), Constants.Volume.CubicFeet); // 0.349486521 cubic metres

			AssertEquals("Total added and rounded to 4 decimal places", 3.7495, (double)collection.GetTotalVolumeInM3(4));
			AssertEquals("Total added and rounded to 2 decimal places", 3.75, (double)collection.GetTotalVolumeInM3(2));

			Xsd.Package package3 = collection.AddNew();
			package3.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(0.8), Constants.Volume.CubicMetres);

			AssertEquals("Total changed now that new element added - rounded to 4 dec pl", 4.5495, (double)collection.GetTotalVolumeInM3(4));
			AssertEquals("Total changed now that new element added - rounded to 2 dec pl", 4.55, (double)collection.GetTotalVolumeInM3(2));

			Xsd.Package package4 = collection.AddNew();
			AssertEquals("Total should not change with addition of an empty package", 4.5495, (double)collection.GetTotalVolumeInM3(4));
			AssertEquals("Total should not change with addition of an empty package", 4.55, (double)collection.GetTotalVolumeInM3(2));
		}

		public void TestCompileTimeCheck()
		{
			Xsd.PackageCollection value = null;
			value = new Xsd.ShipmentShipmentDetails().Packages;
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestAddPackageCollection()
		{
			Xsd.PackageCollection collection = new Xsd.PackageCollection();
			Xsd.Package originalPackage = collection.AddNew();

			Xsd.PackageCollection collectionToAdd = new Xsd.PackageCollection();
			Xsd.Package package1 = collectionToAdd.AddNew();
			Xsd.Package package2 = collectionToAdd.AddNew();
			Xsd.Package package3 = collectionToAdd.AddNew();

			AssertEquals("Original Collection count should be 1", 1, collection.Count);
			AssertEquals("CollectionToAdd count should be 3", 3, collectionToAdd.Count);

			collection.Add(collectionToAdd);
			AssertEquals("Original Collection's count should now be 4", 4, collection.Count);
			Assert("Should have all packages in the collection", ((IList)collection).Contains(originalPackage));
			Assert("Should have all packages in the collection", ((IList)collection).Contains(package1));
			Assert("Should have all packages in the collection", ((IList)collection).Contains(package2));
			Assert("Should have all packages in the collection", ((IList)collection).Contains(package3));

			AssertEquals("Collection to Add's count should remain the same at 3", 3, collectionToAdd.Count);
			Assert("Should have original packages in the collection", ((IList)collection).Contains(package1));
			Assert("Should have original packages in the collection", ((IList)collection).Contains(package2));
			Assert("Should have original packages in the collection", ((IList)collection).Contains(package3));
		}

		public void TestTotalNumberOfPacks()
		{
			Xsd.PackageCollection collection = new Xsd.PackageCollection();
			Xsd.Package pack1 = collection.AddNew();

			AssertEquals("Should be no packs in collection", 0, collection.TotalNumberOfPacks);

			pack1.NumberOfPacks = 2;
			AssertEquals("Should be 2 packs in collection", 2, collection.TotalNumberOfPacks);

			Xsd.Package pack2 = collection.AddNew();
			pack2.NumberOfPacks = 3;
			AssertEquals("Shoudl be 5 packs in collection", 5, collection.TotalNumberOfPacks);
		}
	}
}
