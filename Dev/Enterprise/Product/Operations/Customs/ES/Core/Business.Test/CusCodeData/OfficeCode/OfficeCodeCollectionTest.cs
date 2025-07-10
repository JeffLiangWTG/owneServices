using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(OfficeCodeCollection))]
	class OfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOfficeCodeCollectionType()
		{
			var officeCollection = GetCollectionToTest();
			AssertType<OfficeCode>("Expected ES OfficeCode", officeCollection.AddNew());
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OfficeCodeCollection(Factory.New<JobDeclaration>());
	}
}
