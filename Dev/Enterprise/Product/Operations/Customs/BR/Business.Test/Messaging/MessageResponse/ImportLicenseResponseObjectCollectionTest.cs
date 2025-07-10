using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseResponseObjectCollection))]
	class ImportLicenseResponseObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportLicenseResponseObjectCollection>
	{
		protected override ImportLicenseResponseObjectCollection GetCollectionToTest()
		{
			return new ImportLicenseResponseObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ImportLicenseResponseObject(Factory);
		}

		public void TestCollectionPermissions()
		{
			var collection = new ImportLicenseResponseObjectCollection(Factory);
			Assert("Not allow new", !collection.AllowNew);
			Assert("Note allow remove", !collection.AllowRemove);
		}
	}
}
