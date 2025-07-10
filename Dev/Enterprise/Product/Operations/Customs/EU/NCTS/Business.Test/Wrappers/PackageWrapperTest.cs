using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class PackageWrapperTest : Customs.Business.Testing.DataProviderTestCase<PackageWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PackageWrapper(null));
		}

		public void TestMarksAndNumbersOfPackages()
		{
			package.B5_MarksAndNumbers = "SOME PACKAGE MARKS AND NUMBERS";
			AssertEquals("SOME PACKAGE MARKS AND NUMBERS", wrapper.MarksAndNumbersOfPackages);
		}

		public void TestMarksAndNumbersOfPackagesLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.MarksAndNumbersOfPackagesLanguage);
		}

		public void TestKindOfPackages()
		{
			package.B5_UnitType = "BG";
			AssertEquals("BG", wrapper.KindOfPackages);
		}

		public void TestNumberOfUnits()
		{
			package.B5_UnitCount = 23;
			package.B5_UnitType = ZString.Empty;
			AssertEquals(23, wrapper.NumberOfUnits);

			package.B5_UnitType = bulkPackageType;
			AssertEquals(23, wrapper.NumberOfUnits);

			package.B5_UnitType = unpackedPackageType;
			AssertEquals(23, wrapper.NumberOfUnits);
		}

		public void TestNumberOfPackages_EmptyUnitType()
		{
			package.B5_UnitCount = 23;
			package.B5_UnitType = ZString.Empty;
			AssertEquals(23, wrapper.NumberOfPackages);
		}

		public void TestNumberOfPackages_BulkUnitType()
		{
			package.B5_UnitCount = 18;
			package.B5_UnitType = bulkPackageType;
			AssertEquals(0, wrapper.NumberOfPackages);
		}

		public void TestNumberOfPackages_UnpackedUnitType()
		{
			package.B5_UnitCount = 35;
			package.B5_UnitType = unpackedPackageType;
			AssertEquals(0, wrapper.NumberOfPackages);
		}

		public void TestNumberOfPieces_EmptyUnitType()
		{
			package.B5_UnitCount = 24;
			package.B5_UnitType = ZString.Empty;
			AssertEquals(0, wrapper.NumberOfPieces);
		}

		public void TestNumberOfPieces_BulkUnitType()
		{
			package.B5_UnitCount = 49;
			package.B5_UnitType = bulkPackageType;
			AssertEquals(0, wrapper.NumberOfPieces);
		}

		public void TestNumberOfPieces_UnpackedUnitType()
		{
			package.B5_UnitCount = 28;
			package.B5_UnitType = unpackedPackageType;
			AssertEquals(28, wrapper.NumberOfPieces);
		}

		public void TestIsBulk()
		{
			CombineAssertions(() =>
			{
				package.B5_UnitType = "BX";
				AssertEquals("Not Bulk", false, wrapper.IsBulk);
				package.B5_UnitType = bulkPackageType;
				AssertEquals("Is Bulk", true, wrapper.IsBulk);
			});
		}

		public void TestIsUnpacked()
		{
			CombineAssertions(() =>
			{
				package.B5_UnitType = "BX";
				AssertEquals("Not Unpacked", false, wrapper.IsUnpacked);
				package.B5_UnitType = unpackedPackageType;
				AssertEquals("Is Unpacked", true, wrapper.IsUnpacked);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			bulkPackageType = Factory.SetupBulkCusCode();
			unpackedPackageType = Factory.SetupUnpackCusCode();
			var dummy = Factory.New<DummyBusinessObject>();
			package = Factory.New<NctsPackage>();
			package.B5_ParentID = dummy.PK;
			package.B5_ParentTableCode = dummy.TablePrefix;
			wrapper = new PackageWrapper(package);
		}
		PackageWrapper wrapper;
		NctsPackage package;

		protected override PackageWrapper GetProvider() => wrapper;

		string bulkPackageType;
		string unpackedPackageType;
	}
}
