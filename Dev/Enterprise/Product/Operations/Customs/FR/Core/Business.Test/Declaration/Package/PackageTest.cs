using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(Package))]
	class PackageTest : BasePackageTest
	{
		public void TestValidation_Type()
		{
			var package = Factory.New<Package>();
			AssertType<PackageValidation>(package.Validation);
		}
	}
}
