using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobDeclarationCollection))]
	sealed class JobDeclarationCollectionTest : Customs.Business.Testing.BaseJobDeclarationBizoCollectionTest
	{
		public void TestTypedIndexer()
		{
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var declaration = collection.AddNew();
			AssertEquals(declaration, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}
