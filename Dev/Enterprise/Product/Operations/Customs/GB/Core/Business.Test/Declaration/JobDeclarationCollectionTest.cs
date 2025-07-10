using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	[TestedType(typeof(JobDeclarationCollection))]
	public class JobDeclarationCollectionTest : Customs.Business.Testing.BaseJobDeclarationBizoCollectionTest
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
