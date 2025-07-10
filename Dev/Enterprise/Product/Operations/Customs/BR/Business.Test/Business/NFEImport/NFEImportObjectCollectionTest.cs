using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFEImportObjectCollection))]
	class NFEImportObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NFEImportObjectCollection>
	{
		protected override NFEImportObjectCollection GetCollectionToTest()
		{
			return new NFEImportObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NFEImportObject(Factory);
		}

		public void TestAllowNew()
		{
			var nfeImportObjectCollection = GetCollectionToTest();
			AssertEquals("Should not allow new", false, nfeImportObjectCollection.AllowNew);
		}
	}
}
