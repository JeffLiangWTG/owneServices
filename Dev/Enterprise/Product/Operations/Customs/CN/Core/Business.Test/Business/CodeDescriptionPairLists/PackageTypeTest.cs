using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class PackageTypeTest : TestCaseWithFactory
	{
		public void TestGetMappedPackageType()
		{
			AssertMappedPackageType(Core.Constants.PkgUnit.Bag, PackageType.Codes.Bag);
			AssertMappedPackageType(Core.Constants.PkgUnit.Keg, PackageType.Codes.PaperBarrel);
			AssertMappedPackageType(Core.Constants.PkgUnit.Pail, PackageType.Codes.PaperBarrel);
			AssertMappedPackageType(Core.Constants.PkgUnit.Pallet, PackageType.Codes.RecycledWood);
			AssertMappedPackageType(Core.Constants.PkgUnit.Gross, PackageType.Codes.Naked);
			AssertMappedPackageType(Core.Constants.PkgUnit.Reel, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Sheet, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Skid, PackageType.Codes.RecycledWood);
			AssertMappedPackageType(Core.Constants.PkgUnit.Spool, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Tube, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Unit, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Roll, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Drum, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Envelope, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Box, PackageType.Codes.PaperBox);
			AssertMappedPackageType(Core.Constants.PkgUnit.BulkBag, PackageType.Codes.Bag);
			AssertMappedPackageType(Core.Constants.PkgUnit.BreakBulk, PackageType.Codes.Bulk);
			AssertMappedPackageType(Core.Constants.PkgUnit.BaleCompressed, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.BaleUncompressed, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Bundle, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Bottle, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Dozen, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Basket, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Case, PackageType.Codes.PaperBox);
			AssertMappedPackageType(Core.Constants.PkgUnit.Coil, PackageType.Codes.Naked);
			AssertMappedPackageType(Core.Constants.PkgUnit.Cradle, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Crate, PackageType.Codes.WoodBox);
			AssertMappedPackageType(Core.Constants.PkgUnit.Carton, PackageType.Codes.PaperBox);
			AssertMappedPackageType(Core.Constants.PkgUnit.Cylinder, PackageType.Codes.Other);
			AssertMappedPackageType(Core.Constants.PkgUnit.Mix, PackageType.Codes.Other);
		}

		static void AssertMappedPackageType(string systemPackType, string cnPackType)
		{
			AssertEquals(PackageType.GetMappedPackageType(systemPackType), cnPackType);
		}
	}
}
