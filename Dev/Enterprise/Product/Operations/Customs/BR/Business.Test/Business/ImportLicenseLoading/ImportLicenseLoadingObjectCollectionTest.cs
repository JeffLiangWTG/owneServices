using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseLoadingObjectCollection))]
	class ImportLicenseLoadingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportLicenseLoadingObjectCollection>
	{
		public void TestAllowNew()
		{
			var objectCollection = GetCollectionToTest();
			AssertEquals("Should not allow new", false, objectCollection.AllowNew);
			AssertEquals("Should not allow remove", false, objectCollection.AllowRemove);
		}

		protected override ImportLicenseLoadingObjectCollection GetCollectionToTest()
		{
			return Parent.Collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ImportLicenseLoadingObject(Parent);
		}

		ImportLicenseLoadingObjectParent Parent => fParent ?? (fParent = new ImportLicenseLoadingObjectParent(Factory.New<JobDeclaration>()));
		ImportLicenseLoadingObjectParent fParent;
	}
}
