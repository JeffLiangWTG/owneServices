using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(EdwTableCollectionForTest))]
	class EdwTableCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EdwTableCollectionForTest>
	{
		protected override EdwTableCollectionForTest GetCollectionToTest()
		{
			return new EdwTableCollectionForTest();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EdwTestTable("", "");
		}
	}
}
