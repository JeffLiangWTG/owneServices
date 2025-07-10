using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(PackageCollection))]
	public class PackageCollectionTest : Customs.Business.Testing.BasePackageCollectionTest<PackageCollection>
	{
		public override void TestGetPackageWithPackTypeAndCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Box;
			var packGroup = declaration.PackingGroups.AddNew();
			var package1 = packGroup.Packages.AddNew();

			var package2 = packGroup.Packages.AddNew();
			package2.CW_PackQty = 5;
			package2.CW_PackType = "";

			AssertEquals(package2, packGroup.Packages.GetPackageWithPackTypeAndCount(Core.Constants.PkgUnit.Box, 5, true));
			AssertEquals(package1, packGroup.Packages.GetPackageWithPackTypeAndCount("YY", 5, true));

			package2.CW_PackType = "YY";
			AssertEquals(package2, packGroup.Packages.GetPackageWithPackTypeAndCount("YY", 5, true));
		}

		public void TestHasAtLeastOneOtherElementBesidesThisOne()
		{
			Package package = collection.AddNew();
			AssertEquals(false, collection.HasAtLeastOneOtherElementBesidesThisOne(package));

			collection.AddNew();
			AssertEquals(true, collection.HasAtLeastOneOtherElementBesidesThisOne(package));
		}

		public void TestAddNewWithPackType()
		{
			Package package = collection.AddNew("AA");
			AssertEquals("AA", package.CW_PackType);
		}

		public void TestGetElementWithPackType()
		{
			Package package = collection.AddNew();
			package.CW_PackType = "AA";

			Package package2 = collection.AddNew();
			package2.CW_PackType = "BB";

			AssertEquals("GetElementWithPackType AA", package, collection.GetElementWithPackType("AA"));
			AssertEquals("GetElementWithPackType BB", package2, collection.GetElementWithPackType("BB"));
			AssertNull("GetElementWithPackType CC", collection.GetElementWithPackType("CC"));
		}

		public void TestElementType()
		{
			AssertEquals(typeof(Package), collection.GetTypeOfElementsFromPK(ZGuid.Empty));
		}

		JobDeclaration declaration;
		PackingGroup multiPack;
		PackageCollection collection;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			multiPack = declaration.PackingGroups.AddNew();
			collection = new PackageCollection(multiPack);
		}

		protected override PackageCollection GetCollectionToTest()
		{
			return new PackageCollection((PackingGroup)Packages.PackingGroup);
		}
	}
}
