using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclarationCollection))]
	class EMCSJobDeclarationCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestTypedIndexer()
		{
			var collection = new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var declaration = collection.AddNew();
			AssertEquals(declaration, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}
