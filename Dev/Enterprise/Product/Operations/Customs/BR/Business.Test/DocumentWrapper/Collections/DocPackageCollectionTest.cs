using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocPackageCollection))]
	sealed class DocPackageCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocPackageCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var package = Factory.New<BasePackage>();
			return DocPackage.New(package, Factory);
		}

		protected override DocPackageCollection GetCollectionToTest()
		{
			return new DocPackageCollection(Factory);
		}
	}
}
