using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISPackage))]
	sealed class AQISPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNumber()
		{
			var package = new AQISPackage(Factory);
			AssertEquals("Package Number is empty", true, package.Number.IsEmpty);

			package.Number = 123;
			AssertEquals("Package Number is not empty", false, package.Number.IsEmpty);
		}

		public void TestType()
		{
			var package = new AQISPackage(Factory);
			AssertEquals("Package Type is empty", true, package.Type.IsEmpty);

			package.Type = "CCCC";
			AssertEquals("Package Type is not empty", false, package.Type.IsEmpty);
		}

		public void TestCodeMaxLength()
		{
			var package = new AQISPackage(Factory);
			AssertEquals("Package Type", 4, package.TypeInfo.MaxLength);
		}

		public void TestLookups()
		{
			var package = new AQISPackage(Factory);
			AssertNotNull("Lookups", package.Lookups);
		}

		public void TestValidation()
		{
			var package = new AQISPackage(Factory);
			AssertNotNull("Validation", package.Validation);
		}

		public void TestIAQISUniqueCodeForSort()
		{
			var package = new AQISPackage(Factory);
			package.Type = "T";
			package.Number = 10;
			AssertEquals("IAQISUniqueCodeForSort", 1, ((IAQISUniqueCodeForSort)package).CodesToSortBy.Length);
			AssertEquals("First field to sort by", "T", ((IAQISUniqueCodeForSort)package).CodesToSortBy[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => new AQISPackage(Factory);
	}
}
